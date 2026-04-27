using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Linq;
using Random = UnityEngine.Random;
using DG.Tweening;

public enum CombatState
{
    CombatStart,        // 전투 시작 (몬스터 로드, UI 초기화)
    PlayerAttackTurn,   // 플레이어 공격턴 (스크롤 선택, 패턴 입력)
    EnemyAttackTurn,    // 적 공격 턴
    Processing,         // 데미지 계산, 이펙트 재생 중
    PatternInput,       // 패턴 입력 중
    TurnTransition,     // 턴 전환 대기(hp체크후 combatendState로 넘어갈지 결정)
    CombatEnd,          // 전투 종료 (승리/패배)
    Paused              // 일시정지
}

public class InGameManager : SingletonManager<InGameManager>
{
    [Header("Current State")]
    public CombatState currentState = CombatState.CombatStart;

    [Header("Auto Battle")]
    public bool isAutoPatternMode = false;  // 패턴만 자동
    public bool isFullAutoMode = false;      // 완전 자동

    [Header("System References")]
    private TurnStartText startText;
    public PatternSystem patternSystem;
    public UIManager uiManager;

    [Header("Event Actions")]
    public Action OnPlayerAttackTurnStart;
    public Action OnPatternInputStart;
    public Action OnEnemyAttackTurnStart;
    

    // 상태 변경 이벤트
    public static event Action<CombatState> OnCombatStateChanged;

    // 전투 결과 이벤트
    public static event Action OnCombatVictory;
    public static event Action OnCombatDefeat;


    //현재 생성된 플레이어와 적
    [SerializeField, Header("플로팅 텍스트")] private FloatingTextSpawner floatingTextSpawner;

    [Header("Timing Settings")]
    public float processingTime = 2f;
    // public float walkDuration = 3f; // 걷기 애니메이션 시간

    public int CurrentTurnCnt { get; private set; }

    [Header("Background")]
    [SerializeField] private InfiniteBackground2D _infiniteBackground2D;

    [Header("Boss Round Effects")]
    [SerializeField] private UnityEngine.UI.Image screenFlashImage; // 화면 번쩍임용 이미지 (풀스크린 UI)
    [SerializeField] private Color bossFlashColor = new Color(1f, 0.2f, 0.2f, 0.6f); // 붉은색

    public List<Monster> selectedTargets;
    private Monster[] _pendingTargets;
    public Monster[] PendingTargets => _pendingTargets;
    [SerializeField] private GameObject monsterAttackEffectPrefab; //모든 몬스터 공격 이펙트릴 이걸로 퉁침 (나중에 달라진다면 각 몬스터별로 할당할 것)


    protected override void Awake()
    {
        base.Awake();
        if (startText == null) startText = FindAnyObjectByType<TurnStartText>();
        
        //게임 시작 시 데이터매니저가 없을 경우(CombatScene에서 시작했을 경우)를 대비해 Instance를 한 번 호출함.
        if (!DataManager.Instance) Debug.Log("데이터매니저 없음");


    }

    void Start()
    {
        InitializeCombat().Forget();
    }

    void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.iKey.wasPressedThisFrame)
        {
            TestShowRewardUI();
        }
    }
    
    #region 전투 시작 및 초기화

    private async UniTask InitializeCombat()
    {
        Debug.Log("전투 초기화 시작");
        
        ChangeState(CombatState.CombatStart);

        // 게임 세션 데이터 확인
        var sessionData = DataManager.Instance.LoadGameSession();
        if (sessionData != null)
        {
            Debug.Log($"게임 세션 로드: {sessionData.stageName} - 캐릭터: {sessionData.selectedCharacterId}");
        }
        else
        {
            Debug.LogWarning("게임 세션 데이터가 없습니다. 기본값으로 진행합니다.");
        }

        await DataManager.Instance.EnsureCombatAssetsReadyAsync();
        
        SpawnManager.Instance.SpawnPlayer();
        // fieldPlayer = SpawnManager.Instance.SpawnPlayerFromSessionData();
        
        StartCoroutine(uiManager.WaitForPlayerAndStartSetting());

        // RoundManager를 통해 라운드 시작
        if (RoundManager.Instance != null)
        {
            bool roundStarted = RoundManager.Instance.StartFromGameSession();
            if (!roundStarted)
            {
                Debug.LogError("라운드 시작 실패!");
                return;
            }
        }
        else
        {
            Debug.LogError("RoundManager.Instance가 null입니다!");
            return;
        }

        await UniTask.Delay(1000); // 1초 대기

        // 아티팩트: 전투 시작 시 트리거
        if (ArtifactManager.Instance != null)
        {
            await ArtifactManager.Instance.TriggerBattleStartAsync();
        }

        // 첫 라운드는 PlayRoundTransitionAnimation()에서 모든 처리를 담당하므로
        // InitializeCombat()에서는 추가 처리 불필요
    }

    #endregion

    #region 맵 이동 애니메이션

    public async UniTask MoveMap(bool isRestRound)
    {
        // 보스 라운드 체크
        bool isBossRound = RoundManager.Instance.IsCurrentRoundBoss();

        if (isBossRound && !isRestRound)
        {
            // 보스 라운드 특별 연출
            await PlayBossRoundTransition();
        }
        else
        {
            // 일반 라운드 연출
            SpawnManager.Instance.CurrentPlayer?.PlayWalkAnimation();
            SetBackgroundScrolling(true, resetOffset:false);

            await SpawnManager.Instance.MoveMapToTarget(isRestRound);

            SetBackgroundScrolling(false);
            SpawnManager.Instance.CurrentPlayer?.PlayIdleAnimation();
        }
    }

    /// <summary>
    /// 보스 라운드 특별 전환 연출
    /// </summary>
    private async UniTask PlayBossRoundTransition()
    {
        // 1. 플레이어 걷기 시작
        SpawnManager.Instance.CurrentPlayer?.PlayWalkAnimation();

        // 2. 배경 느리게 시작 (일반 속도의 50%)
        float normalSpeed = SpawnManager.Instance ? SpawnManager.Instance.MonsterMoveSpeed : 2f;
        _infiniteBackground2D.SetWorldSpeed(normalSpeed * 0.5f);
        _infiniteBackground2D.paused = false;

        await UniTask.Delay(1000); // 1초 느리게 이동

        // 3. 화면 번쩍임 효과
        await PlayScreenFlash();

        // 4. 카메라 흔들림
        Camera mainCam = Camera.main;
        Vector3 originalCamPos = Vector3.zero;

        if (mainCam != null)
        {
            // 원래 카메라 위치 저장
            originalCamPos = mainCam.transform.localPosition;

            mainCam.transform.DOShakePosition(0.6f, strength: 0.6f, vibrato: 20, randomness: 90)
                .SetEase(Ease.OutQuad);
        }

        // 5. 배경 급가속 (일반 속도의 150%)
        _infiniteBackground2D.SetWorldSpeed(normalSpeed * 1.5f);

        await UniTask.Delay(300); // 카메라 흔들림과 함께 진행

        // 6. 몬스터 위치로 이동
        await SpawnManager.Instance.MoveMapToTarget(false);

        // 7. 정상 속도로 복귀 후 정지
        SetBackgroundScrolling(false);
        SpawnManager.Instance.CurrentPlayer?.PlayIdleAnimation();

        // 8. 카메라 원위치 (원래 위치로 복귀)
        if (mainCam != null)
        {
            mainCam.transform.DOLocalMove(originalCamPos, 0.3f).SetEase(Ease.OutQuad);
        }
    }

    /// <summary>
    /// 화면 번쩍임 효과 (보스 등장)
    /// </summary>
    private async UniTask PlayScreenFlash()
    {
        if (screenFlashImage == null) return;

        screenFlashImage.gameObject.SetActive(true);
        screenFlashImage.color = new Color(bossFlashColor.r, bossFlashColor.g, bossFlashColor.b, 0f);

        // 페이드 인 (빠르게)
        await screenFlashImage.DOFade(bossFlashColor.a, 0.1f).AsyncWaitForCompletion();

        // 잠깐 유지
        await UniTask.Delay(100);

        // 페이드 아웃
        await screenFlashImage.DOFade(0f, 0.3f).AsyncWaitForCompletion();

        screenFlashImage.gameObject.SetActive(false);
    }

    /// 슬로우 모션 → 프리즈 프레임 → 강렬한 임팩트
    private async UniTask PlayBossDefeatCinematic()
    {
        float originalTimeScale = Time.timeScale;
        Camera mainCam = Camera.main;

        // 1단계: 슬로우 모션 시작 (느리게 죽어가는 연출)
        Time.timeScale = 0.2f;
        await UniTask.Delay(500, ignoreTimeScale: true); // 실제 시간 0.5초 대기

        // 2단계: 프리즈 프레임 (완전 정지 - 임팩트 순간)
        Time.timeScale = 0f;
        await UniTask.Delay(150, ignoreTimeScale: true); // 0.15초 프리즈

        // 3단계: 강렬한 임팩트 효과 (시간 정지 상태에서 실행)
        // 화이트 플래시 (밝은 섬광)
        if (screenFlashImage != null)
        {
            screenFlashImage.gameObject.SetActive(true);
            Color whiteFlash = new Color(1f, 1f, 1f, 0.8f); // 화이트 플래시
            screenFlashImage.color = whiteFlash;

            // unscaled time으로 페이드 아웃 (시간 정지 영향 안받음)
            screenFlashImage.DOFade(0f, 0.4f)
                .SetUpdate(true) // Time.timeScale 무시
                .SetEase(Ease.OutQuad);
        }

        // 강한 카메라 쉐이크 (시간 정지 영향 안받음)
        if (mainCam != null)
        {
            Vector3 originalCamPos = mainCam.transform.localPosition;

            mainCam.transform.DOShakePosition(0.6f, strength: 0.8f, vibrato: 30, randomness: 90)
                .SetUpdate(true) // Time.timeScale 무시
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    // 카메라 위치 복원
                    mainCam.transform.DOLocalMove(originalCamPos, 0.2f).SetUpdate(true);
                });
        }

        // 4단계: 시간 정상화
        await UniTask.Delay(100, ignoreTimeScale: true);
        Time.timeScale = originalTimeScale;

        // 5단계: 여운 (약간 대기)
        await UniTask.Delay(300, ignoreTimeScale: true);

        // 플래시 이미지 비활성화
        if (screenFlashImage != null)
        {
            screenFlashImage.gameObject.SetActive(false);
        }
    }
    
    private void SetBackgroundScrolling(bool isScrolling, bool resetOffset = true)
    {
        if (isScrolling)
        {
            float uPerSec = SpawnManager.Instance ? SpawnManager.Instance.MonsterMoveSpeed : 2f;
            _infiniteBackground2D.SetWorldSpeed(uPerSec);
            if (resetOffset) _infiniteBackground2D.ResetOffset(); // <- 선택적
            _infiniteBackground2D.paused = false;
        }
        else
        {
            _infiniteBackground2D.paused = true;
        }

        // Debug.Log($"배경 스크롤 상태: {(isScrolling ? "활성화" : "비활성화")}");
    }

    // private async UniTask PlayCombatStartAnimations()
    // {
    //     Debug.Log("전투 시작 애니메이션 실행");
    //     
    //     var sessionData = DataManager.Instance.LoadGameSession();
    //     if (sessionData == null)
    //     {
    //         Debug.LogError("세션 데이터가 없습니다.");
    //         return;
    //     }
    //
    //     RoundDataSO roundData = RoundManager.Instance.GetRoundData(sessionData.currentStageNumber, sessionData.currentRoundNumber);
    //     if (!roundData)
    //     {
    //         Debug.LogError("RoundData를 찾을 수 없습니다.");
    //         return;
    //     }
    //
    //     // 첫 라운드 몬스터 스폰
    //     List<Monster> spawned = SpawnManager.Instance.SpawnMonstersForRound(roundData, autoMove: false);
    //     fieldEnemies = spawned;
    //     
    //     SetPlayersRun(true);
    //     SetBackgroundScrolling(true, resetOffset:false);
    //
    //     await SpawnManager.Instance.MoveMonstersToTargetsAsync(spawned);
    //     
    //     SetBackgroundScrolling(false);
    //     SpawnManager.Instance.CurrentPlayer?.PlayIdleAnimation();
    // }

    /// <summary>
    /// 외부에서 라운드 전환 애니메이션 실행 (매 라운드마다 호출용)
    /// </summary>
    public async void PlayRoundTransitionAnimation()
    {
        // Debug.Log("라운드 전환 애니메이션 시작");
        
        // 현재 라운드가 휴식 라운드인지 확인
        bool isRestRound = RoundManager.Instance.GetCurrentRoundData()?.roundType == RoundDataSO.RoundType.Rest;
        
        SpawnManager.Instance.ClearAllMonsters();
        await UniTask.Delay(200); // 잠시 대기

        if (isRestRound)
        {
            // 휴식 라운드: 캠프파이어 스폰 (실패해도 계속 진행)
            var spawnedCampfire = SpawnManager.Instance.SpawnCampfire();
        }
        else
        {
            // 일반 라운드: 몬스터 스폰
            var spawnedMonsters = SpawnManager.Instance.SpawnMonsters();
            if (spawnedMonsters == null || spawnedMonsters.Count == 0)
            {
                Debug.LogError("[InGameManager] 몬스터 스폰 실패! MoveMap 실행하지 않음");
                return;
            }
        }
        
        // 카피바라고 스타일 애니메이션 실행
        await MoveMap(isRestRound);
        
        // 일반 라운드면 플레이어 턴 시작
        if (!isRestRound)
        {
            // Debug.Log("[InGameManager] 일반 라운드 전환 완료 - 플레이어 턴 시작");
            StartPlayerAttackTurn();
        }
        else
        {
            // 휴식라운드 이동 완료 후 캠프파이어 불꽃 켜기
            StartRestRoundCampfire().Forget();
            // 휴식라운드 UI 시퀀스 시작
            RoundManager.Instance.StartRestRoundSequence();
        }
        
        // Debug.Log("라운드 전환 애니메이션 완료");
    }

    // 휴식라운드 캠프파이어 시작
    private async UniTask StartRestRoundCampfire()
    {
        if (SpawnManager.Instance.CurrentCampfire != null)
        {
            CampfireController campfireController = SpawnManager.Instance.CurrentCampfire.GetComponent<CampfireController>();
            if (campfireController != null)
            {
                // 부드럽게 불꽃 켜기
                await campfireController.TurnOnFireAsync(1.5f);
            }
            else
            {
                Debug.LogWarning("[InGameManager] CampfireController 컴포넌트를 찾을 수 없습니다!");
            }
        }
    }

    #endregion

    #region 상태 관리

    private void ChangeState(CombatState newState)
    {
        if (currentState != newState)
        {
            CombatState previousState = currentState;
            currentState = newState;

            Debug.Log($"State Changed: {previousState} → {newState}");
            OnCombatStateChanged?.Invoke(newState);
        }
    }

    public void PauseCombat() // 전투 일시정지
    {
        if (currentState != CombatState.Paused)
        {
            ChangeState(CombatState.Paused);
        }
    }

    public void ResumeCombat() // 전투 재개
    {
        if (currentState == CombatState.Paused)
        {
            // 이전 상태로 복귀 (구현 필요)
            ChangeState(CombatState.PlayerAttackTurn);
        }
    }

    #endregion

    #region 헬퍼 메소드

    public void SpawnFloatingText(Vector3 position, int damage)
    {
        floatingTextSpawner.Spawn(position, damage);
    }

    #endregion

    #region 플레이어 공격턴

    public async void StartPlayerAttackTurn()
    {
        CurrentTurnCnt++;

        // 플레이어 턴 시작 시 마나/체력 회복 (재능 보너스 포함)
        Player player = SpawnManager.Instance?.CurrentPlayer;
        if (player != null)
        {
            player.RecoverManaPerTurn();
            player.RecoverHealthPerTurn();
        }

        // 아티팩트: 턴 시작 시 트리거
        if (ArtifactManager.Instance != null)
        {
            await ArtifactManager.Instance.TriggerTurnStartAsync();
        }

        if (startText != null) await startText.StartTurnUI(StartTextType.MYTURN,CurrentTurnCnt);
        ChangeState(CombatState.PlayerAttackTurn);
        OnPlayerAttackTurnStart?.Invoke();

        // 완전 자동 모드 체크
        if (isFullAutoMode)
        {
            // 짧은 딜레이
            await UniTask.Delay(500);

            // 최적 스크롤 자동 선택
            ScrollSO selectedScroll = AutoSelectBestScroll();

            if (selectedScroll != null)
            {
                // 타겟 자동 선택 (첫 번째 살아있는 적)
                var aliveEnemies = SpawnManager.Instance.CurrentMonsters.Where(m => m.IsAlive()).ToArray();
                if (aliveEnemies.Length > 0)
                {
                    selectedTargets = new List<Monster> { aliveEnemies[0] };
                }

                // 스크롤 선택 실행
                OnScrollSelected(selectedScroll);
            }
            return;
        }

        // 패턴 자동 모드 또는 수동 모드: 플레이어 입력 대기
        // (기존 로직은 OnScrollSelected에서 처리)
    }

    //public void OnScrollSelected(ScrollSO selectedScroll) // 스크롤 선택 시 패턴 시스템으로 전달
    //{
    //    if (currentState != CombatState.PlayerAttackTurn) return;

    //    // 패턴 입력 시작
    //    OnPatternInputStart?.Invoke();
    //    ChangeState(CombatState.PatternInput);
    //    _pendingTargets = selectedTarget.ToArray();
    //    patternSystem.StartAttackPattern(selectedScroll);
    //    Debug.Log($"스크롤 선택: {selectedScroll.scrollName}");
    //}

    public void OnScrollSelected(ScrollSO selectedScroll) // 스크롤 선택 시 패턴 시스템으로 전달
    {
        if (currentState != CombatState.PlayerAttackTurn) return;

        // 플레이어 턴 시작 시 상태이상 처리
        Player player = SpawnManager.Instance.CurrentPlayer;
        if (player != null)
        {
            player.OnTurnStart();
            if (!player.CanAct())
            {
                Debug.Log($"{player.name}이(가) 상태이상으로 행동 불가!");
                StartEnemyTurn();
                return;
            }
        }

        // 마나 확인 및 소비
        if (player != null)
        {
            Debug.Log($"[마나 소비 전] {player.name}: {player.GetCurrentMana()}/{player.GetMaxMana()}");
            if (!player.ConsumeMana(selectedScroll.scrollManaCost))
            {
                Debug.Log($"마나가 부족합니다. 필요: {selectedScroll.scrollManaCost}, 현재: {player.GetCurrentMana()}");
                return;
            }
            Debug.Log($"[마나 소비 후] {player.name}: {player.GetCurrentMana()}/{player.GetMaxMana()}");
        }
        else
        {
            Debug.LogError("CurrentPlayer가 null입니다!");
            return;
        }

        uiManager.ShowSpellGrid();
        // 패턴 입력 시작
        OnPatternInputStart?.Invoke();
        ChangeState(CombatState.PatternInput);
        _pendingTargets = selectedTargets.ToArray();
        patternSystem.StartAttackPattern(selectedScroll);
        Debug.Log($"스크롤 선택: {selectedScroll.scrollName}, 마나 소비: {selectedScroll.scrollManaCost}");
    }

    public void OnAttackPatternComplete(ScrollSO scroll, PatternResult patternResult) // 공격 패턴 완료 시 데미지 처리
    {
        if (currentState != CombatState.PatternInput) return;

        // 패턴 완료 시 그리드 OFF
        uiManager.HideSpellGrid();

        // CombatSystem에서 모든 타겟 데미지 계산
        Player player = SpawnManager.Instance.CurrentPlayer;
        float damage = CombatSystem.CalculateFinalDamage(scroll, player, patternResult);

        // 플레이어 공격 처리 시작
        ProcessPlayerAttackAsync(scroll, damage).Forget();
    }

    private async UniTask ProcessPlayerAttack(ScrollSO scroll, float damage) // 플레이어 공격 데미지 적용
    {
        ChangeState(CombatState.Processing);
        // Debug.Log($"플레이어 공격 처리 시작 - 타겟 수: {damage.Count}");

        // 아티팩트: 카드 사용 시 트리거
        if (ArtifactManager.Instance != null)
        {
            await ArtifactManager.Instance.TriggerCardUsedAsync(scroll);
        }

        await RunScrollAsync(scroll, damage);

        // await UniTask.Delay((int)(processingTime * 1000));

        // HP 체크 - 모든 몬스터가 죽었는지 확인
        bool allMonstersDead = true;
        foreach (Monster monster in SpawnManager.Instance.CurrentMonsters)
        {
            if (monster && monster.IsAlive())
            {
                allMonstersDead = false;
                break;
            }
        }

        if (allMonstersDead)
        {
            // 보스 라운드 체크 - 보스 처치 시 특별 연출
            bool isBossRound = RoundManager.Instance.IsCurrentRoundBoss();
            if (isBossRound)
            {
                await PlayBossDefeatCinematic(); // 보스 처치 피니시 연출
            }

            EndCombat(true); // 승리
        }
        else
        {
            StartEnemyTurn(); // 방어턴으로
        }
    }

    private async UniTaskVoid ProcessPlayerAttackAsync(ScrollSO scroll, float damage)
    {
        await ProcessPlayerAttack(scroll, damage);
    }
    
    private async UniTask RunScrollAsync(ScrollSO scroll, float damage)
    {
        if (!scroll) throw new InvalidOperationException("[RunScroll] scroll이 null입니다.");
        if (!scroll.logicData)
        {
            Debug.LogWarning($"[RunScroll] scroll({scroll.name})에 Logic이 비어있습니다.");
            return;
        }

        CancellationToken token = this.GetCancellationTokenOnDestroy();

        // 단일 타겟: 선택된 대상 1개만 넣어서 실행
        ScrollContext ctx = new ScrollContext(scroll, SpawnManager.Instance.CurrentPlayer, selectedTargets.ToArray() as Entity[], token, damage);
        await scroll.logicData.ExecuteAsync(ctx);
    }

    #endregion

    #region 적 공격턴

    public async void StartEnemyTurn() // 적 공격턴 시작
    {
        ChangeState(CombatState.EnemyAttackTurn);
        OnEnemyAttackTurnStart?.Invoke();

        // 아티팩트: 플레이어 턴 끝 트리거
        if (ArtifactManager.Instance != null)
        {
            await ArtifactManager.Instance.TriggerTurnEndAsync();
        }

        // Debug.Log("적 공격턴 시작");
        if (startText != null) await startText.StartTurnUI(StartTextType.ENEMYTURN, CurrentTurnCnt);
        selectedTargets.Clear();
        // 몬스터들 순서대로 공격 처리 (기존 단일 공격에서 변경)
        ProcessMonstersAttackSequentiallyAsync().Forget();
    }

    private async UniTask ProcessMonstersAttackSequentially()
    {
        ChangeState(CombatState.Processing);
        Debug.Log("몬스터 순차 공격 시작");

        // 살아있는 몬스터들을 순서대로 정렬 (몬스터 1번, 2번, 3번 순)
        Monster[] allMonsters = SpawnManager.Instance.CurrentMonsters.ToArray();
        List<Monster> aliveMonsters = new List<Monster>();

        foreach (Monster monster in allMonsters)
        {
            if (monster && monster.IsAlive())
            {
                aliveMonsters.Add(monster);
            }
        }

        // 몬스터 이름이나 인덱스로 정렬 (예: Monster_1, Monster_2, Monster_3)
        aliveMonsters.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));

        // Debug.Log($"공격할 몬스터 수: {aliveMonsters.Count}");

        Player player = SpawnManager.Instance.CurrentPlayer;
        bool isAlive = player.IsAlive();
        // 각 몬스터가 순서대로 공격
        foreach (Monster monster in aliveMonsters)
        {
            if (monster.IsAlive()) // 다시 한 번 확인 (이전 공격으로 죽었을 수도 있음)
            {
                await ProcessSingleMonsterAttack(monster);

                // 플레이어가 죽었는지 중간에 체크
                isAlive = player.IsAlive();
                if (!isAlive)
                {
                    Debug.Log("플레이어가 사망하여 몬스터 공격 중단");
                    break;
                }
            }
        }

        await UniTask.Delay(500); // 잠시 대기

        if (!isAlive)
        {
            EndCombat(false); // 패배
        }
        else
        {
            StartPlayerAttackTurn(); // 다음 공격턴으로
        }
    }

    // 추가: async wrapper 메소드
    private async UniTaskVoid ProcessMonstersAttackSequentiallyAsync()
    {
        await ProcessMonstersAttackSequentially();
    } 
    
    private async UniTask ProcessSingleMonsterAttack(Monster attackingMonster)
    {
        // 몬스터가 이미 파괴되었는지 체크
        if (attackingMonster == null || !attackingMonster)
        {
            Debug.LogWarning("몬스터가 이미 파괴되어 공격을 건너뜁니다.");
            return;
        }

        // Debug.Log($" {attackingMonster.name} 공격 시작");

        // 몬스터 턴 시작 시 상태이상 처리
        attackingMonster.OnTurnStart();
        if (!attackingMonster.CanAct())
        {
            Debug.Log($"{attackingMonster.name}이(가) 상태이상으로 행동 불가!");
            await UniTask.Delay(1000); // 1초 대기
            return; // 이 몬스터는 턴 스킵
        }

        if (attackingMonster.IsBoss())
        {
            Debug.Log($"보스 몬스터 {attackingMonster.name}의 공격");
            CancellationToken token = this.GetCancellationTokenOnDestroy();
            Player player = SpawnManager.Instance.CurrentPlayer;

            // BossMonster로 캐스팅해서 보스 스크롤 가져오기
            BossMonster boss = attackingMonster as BossMonster;
            ScrollSO[] bossScrolls = boss.GetBossScrolls();

            if (bossScrolls == null || bossScrolls.Length == 0)
            {
                Debug.LogWarning($"보스 {boss.name}에게 스크롤이 없습니다! 일반 공격으로 전환합니다.");
                // 일반 몬스터 공격으로 폴백
                attackingMonster.PlayAttackAnimation();
                await UniTask.Delay(500);

                int finalDamage = CombatSystem.CalculateMonsterAttackDamage(attackingMonster, player);

                float deg = Random.Range(-10, 10);
                EffectController.Instance.Create(
                    monsterAttackEffectPrefab, player.transform.position + new Vector3(0.15f, 0.15f, 0f),
                    deg, 2f, 2f, 100, false);

                if (player != null && player.IsAlive())
                {
                    player.PlayDamagedAnimation();
                    player.TakeDamage(finalDamage);

                    if (!player.IsAlive())
                    {
                        player.PlayDeathAnimation();
                    }
                }

                await UniTask.Delay(1000);
                return;
            }

            ScrollSO selectedScroll = bossScrolls[Random.Range(0, bossScrolls.Length)];
            Debug.Log($"보스가 스크롤 사용: {selectedScroll.scrollName}");

            // CombatSystem을 통한 보스 데미지 계산
            PatternResult patternResult = new PatternResult
            {
                accuracy = 1.0f,
                isPerfect = false,
                completionTime = 0f,
                totalTime = 1f
            };
            float damage = CombatSystem.CalculateFinalDamage(selectedScroll, player, patternResult);

            ScrollContext ctx = new ScrollContext(
                selectedScroll,
                attackingMonster,
                new Entity[] { player },
                token,
                damage);

            await selectedScroll.logicData.ExecuteAsync(ctx);

            // 몬스터 스크롤 실행 완료 후 플레이어에게 상태이상 적용
            if (StatusEffectSystem.Instance != null && selectedScroll.statusEffect != StatusEffectSystem.StatusEffectType.None)
            {
                StatusEffectSystem.Instance.ApplyScrollStatusEffect(selectedScroll, player.gameObject);
            }
        }
        else
        {
            // 1. 몬스터 공격 애니메이션
            attackingMonster.PlayAttackAnimation();

            await UniTask.Delay(500); // 공격 애니메이션 시간

            // 2. CombatSystem을 통한 데미지 계산
            Player player = SpawnManager.Instance.CurrentPlayer;
            int finalDamage = CombatSystem.CalculateMonsterAttackDamage(attackingMonster, player);

            // 이펙트 생성
            float deg = Random.Range(-10, 10);
            EffectController.Instance.Create(
                monsterAttackEffectPrefab, player.transform.position + new Vector3(0.15f, 0.15f, 0f), 
                deg, 2f, 2f, 100, false);

            // 3. 플레이어에게 데미지 적용
            if (player != null && player.IsAlive())
            {
                player.PlayDamagedAnimation();
                player.TakeDamage(finalDamage);

                // 플레이어가 죽었는지 확인하고 사망 애니메이션 재생
                if (!player.IsAlive())
                {
                    player.PlayDeathAnimation();
                }
            }

            await UniTask.Delay(1000); // 피격 애니메이션 시간
        }
        // Debug.Log($"{attackingMonster.name} 공격 완료");
    }
    #endregion

    #region 전투 종료

    public async void EndCombat(bool victory) // 전투 종료 및 승패 결과 처리
    {
        ChangeState(CombatState.CombatEnd);

        if (victory)
        {
            Debug.Log("🎉 전투 승리!");
            if (startText != null) await startText.StartTurnUI(StartTextType.ROUNDEND, CurrentTurnCnt);
            CurrentTurnCnt = 0;
            OnCombatVictory?.Invoke();
        }
        else
        {
            Debug.Log("💀 전투 패배!");
            OnCombatDefeat?.Invoke();
        }
    }

    #endregion

    #region 수동 애니메이션 트리거 메서드 (디버그용)

    /// <summary>
    /// 외부에서 플레이어 공격 애니메이션 수동 실행 (디버그용)
    /// </summary>
    public void TriggerPlayerAttackAnimation()
        => SpawnManager.Instance.CurrentPlayer.PlayAttackAnimation();
    

    /// <summary>
    /// 외부에서 몬스터 공격 애니메이션 수동 실행 (디버그용)
    /// </summary>
    public void TriggerMonsterAttackAnimation()
    {
        foreach (Monster monster in SpawnManager.Instance.CurrentMonsters)
            monster.PlayAttackAnimation();
    }

    #endregion

    #region 디버그 메서드

    [ContextMenu("전투 상태 출력")]
    public void LogCombatStatus()
    {
        Debug.Log($"=== 전투 상태 ===");
        Debug.Log($"현재 상태: {currentState}");
        Debug.Log($"PatternSystem: {(patternSystem != null ? "설정됨" : "없음")}");
        Debug.Log($"UIManager: {(uiManager != null ? "설정됨" : "없음")}");
        Debug.Log($"처리 시간: {processingTime}초");
        // Debug.Log($"걷기 시간: {walkDuration}초");
    }

    /// <summary>
    /// 테스트 전용 - 키보드 'I'로 보상 UI 바로 표시 (나중에 삭제 예정)
    /// </summary>
    [ContextMenu("🧪 테스트 - 보상 UI")]
    public void TestShowRewardUI()
    {
        Debug.Log("🎁 [테스트] 'I'키 - 보상 UI 강제 표시");
        StartCoroutine(SimpleRewardUITest());
    }

    // 테스트 전용 - 간단하게 보상 UI만 바로 표시
    private IEnumerator SimpleRewardUITest()
    {
        // RewardSelectionUI 찾기
        RewardSelectionUI rewardUI = FindObjectOfType<RewardSelectionUI>(true);
        if (!rewardUI) 
        {
            Debug.LogError("❌ RewardSelectionUI 못 찾음!");
            yield break;
        }

        // // 스크롤 데이터 로드 (간단하게)
        // if (!AddressableScrollRepository.IsLoaded)
        // {
        //     yield return AddressableScrollRepository.LoadAllFromLabel("ScrollData", null);
        // }

        // var scrolls = new List<ScrollSO>(AddressableScrollRepository.All);

        List<ScrollSO> scrolls = DataManager.Instance.LoadedScroll.Values.ToList();
        
        // Resources 폴백
        if (scrolls.Count == 0)
        {
            var resources = Resources.LoadAll<ScrollSO>("ScrollData");
            if (resources?.Length > 0) scrolls.AddRange(resources);
        }

        if (scrolls.Count == 0)
        {
            Debug.LogError("❌ 스크롤 데이터 없음!");
            yield break;
        }

        Debug.Log($"✅ 스크롤 {scrolls.Count}개 로드됨");
        
        // 보상 UI 표시
        rewardUI.Show(scrolls, (scroll) => Debug.Log($"🎯 선택: {scroll.scrollName}"));
    }

    [ContextMenu("강제 플레이어 공격턴 시작")]
    public void ForceStartPlayerTurn()
    {
        if (Application.isPlaying)
        {
            StartPlayerAttackTurn();
        }
    }

    [ContextMenu("강제 적 공격턴 시작")]
    public void ForceStartEnemyTurn()
    {
        if (Application.isPlaying)
        {
            StartEnemyTurn();
        }
    }

    [ContextMenu("테스트 - 플레이어 공격 애니메이션")]
    public void TestPlayerAttackAnimation()
    {
        if (Application.isPlaying)
        {
            TriggerPlayerAttackAnimation();
        }
    }

    [ContextMenu("테스트 - 몬스터 공격 애니메이션")]
    public void TestMonsterAttackAnimation()
    {
        if (Application.isPlaying)
        {
            TriggerMonsterAttackAnimation();
        }
    }

    #endregion

    #region 자동 전투

    private ScrollSO AutoSelectBestScroll()
    {
        var player = SpawnManager.Instance.CurrentPlayer;
        if (player == null) return null;

        // 사용 가능한 스크롤 목록
        var availableScrolls = uiManager.playerScrolls;

        // 마나가 충분하고 사용 가능한 스크롤만 필터링
        var usableScrolls = availableScrolls
            .Where(s => s.scrollManaCost <= player.GetCurrentMana())
            .Where(s => s.scrollCurrentUsageCount < s.scrollMaxUsageCount)
            .ToList();

        if (usableScrolls.Count == 0) return null;

        // 데미지 효율이 가장 높은 스크롤 선택 (데미지 / 마나)
        return usableScrolls
            .OrderByDescending(s => {
                float damage = s.baseDamage + (s.attackCoefficient * player.GetModifiedAttack());
                return damage / Mathf.Max(1, s.scrollManaCost);
            })
            .First();
    }

    #endregion
}