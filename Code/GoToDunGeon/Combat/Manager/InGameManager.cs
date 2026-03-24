using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Linq;

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

    [Header("System References")]
    public TurnStartText startText;
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
    
    public List<Monster> selectedTarget;
    private Monster[] _pendingTargets;
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

        // 전투 시작 애니메이션 시퀀스
        await PlayCombatStartAnimations();
        
        // 휴식라운드가 아닌 경우에만 턴 시작
        bool isRestRound = false;
        if (RoundManager.Instance != null)
        {
            var currentRoundData = RoundManager.Instance.GetCurrentRoundData();
            isRestRound = currentRoundData != null && currentRoundData.roundType == RoundDataSO.RoundType.Rest;
        }
        
        if (!isRestRound)
        {
            StartPlayerAttackTurn();
        }
    }

    #endregion

    #region 전투 시작 애니메이션
    
    // 전투 시작 애니메이션 시퀀스
    private async UniTask PlayCombatStartAnimations()
    {
        // RoundManager에서 이미 라운드가 시작되었으므로 라운드 타입 확인
        bool isRestRound = false;
        if (RoundManager.Instance != null)
        {
            var currentRoundData = RoundManager.Instance.GetCurrentRoundData();
            isRestRound = currentRoundData != null && currentRoundData.roundType == RoundDataSO.RoundType.Rest;
        }

        // 전투라운드에서만 몬스터 스폰 (휴식라운드는 RoundManager에서 처리됨)
        if (!isRestRound)
        {
            SpawnManager.Instance.SpawnMonsters();
        }
        
        await MoveMap(isRestRound);
    }

    public async UniTask MoveMap(bool isRestRound)
    {
        SpawnManager.Instance.CurrentPlayer?.PlayWalkAnimation();
        SetBackgroundScrolling(true, resetOffset:false);

        await SpawnManager.Instance.MoveMapToTarget(isRestRound);

        SetBackgroundScrolling(false);
        SpawnManager.Instance.CurrentPlayer?.PlayIdleAnimation();
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

    /// <summary>
    /// 외부에서 라운드 전환 애니메이션 실행 (매 라운드마다 호출용)
    /// </summary>
    public async void PlayRoundTransitionAnimation()
    {
        Debug.Log("라운드 전환 애니메이션 시작");
        
        // 현재 라운드가 휴식 라운드인지 확인
        bool isRestRound = RoundManager.Instance.GetCurrentRoundData()?.roundType == RoundDataSO.RoundType.Rest;
        Debug.Log($"[InGameManager] 현재 라운드 타입 확인: {(isRestRound ? "휴식" : "전투")}");
        
        SpawnManager.Instance.ClearAllMonsters();
        await UniTask.Delay(200); // 잠시 대기
        
        // 휴식 라운드가 아닐 때만 몬스터 스폰
        if (!isRestRound)
        {
            var spawnedMonsters = SpawnManager.Instance.SpawnMonsters();
            Debug.Log($"[InGameManager] 몬스터 스폰 결과: {spawnedMonsters?.Count ?? 0}마리");
            
            // 몬스터 스폰 실패 시 에러 로그
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
            Debug.Log("[InGameManager] 일반 라운드 전환 완료 - 플레이어 턴 시작");
            StartPlayerAttackTurn();
        }
        else
        {
            Debug.Log("[InGameManager] 휴식 라운드 전환 완료");
        }
        
        Debug.Log("라운드 전환 애니메이션 완료");
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
        if (startText != null) await startText.StartTurnUI(StartTextType.MYTURN,CurrentTurnCnt);
        ChangeState(CombatState.PlayerAttackTurn);
        OnPlayerAttackTurnStart?.Invoke();
        // Debug.Log("플레이어 공격턴 시작");
    }

    public void OnScrollSelected(ScrollSO selectedScroll) // 스크롤 선택 시 패턴 시스템으로 전달
    {
        if (currentState != CombatState.PlayerAttackTurn) return;

        // 패턴 입력 시작
        OnPatternInputStart?.Invoke();
        ChangeState(CombatState.PatternInput);
        _pendingTargets = selectedTarget.ToArray();
        patternSystem.StartAttackPattern(selectedScroll);
        Debug.Log($"스크롤 선택: {selectedScroll.scrollName}");
    }

    public void OnAttackPatternComplete(ScrollSO scroll, float accuracy, int damage) // 공격 패턴 완료 시 데미지 처리
    {
        if (currentState != CombatState.PatternInput) return;

        // 플레이어 공격 처리 시작
        ProcessPlayerAttackAsync(scroll, accuracy, damage).Forget();
    }

    private async UniTask ProcessPlayerAttack(ScrollSO scroll, float accuracy, int damage) // 플레이어 공격 데미지 계산 및 적용
    {
        ChangeState(CombatState.Processing);
        Debug.Log($"플레이어 공격 처리 시작 - 정확도: {accuracy}, 데미지: {damage}");

        // await PlayPlayerAttackAnimationSequence();
        
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
            EndCombat(true); // 승리
        }
        else
        {
            StartEnemyTurn(); // 방어턴으로
        }
    }

    private async UniTaskVoid ProcessPlayerAttackAsync(ScrollSO scroll, float accuracy, int damage)
    {
        await ProcessPlayerAttack(scroll, accuracy, damage);
    }
    
    private async UniTask RunScrollAsync(ScrollSO scroll, int damage)
    {
        if (!scroll) throw new InvalidOperationException("[RunScroll] scroll이 null입니다.");
        if (!scroll.logicData)
        {
            Debug.LogWarning($"[RunScroll] scroll({scroll.name})에 Logic이 비어있습니다.");
            return;
        }
        
        Debug.Log($"[RunScrollAsync] {scroll.name} 발동. LogicData: {scroll.logicData.name}");

        Monster[] targets = _pendingTargets;

        CancellationToken token = this.GetCancellationTokenOnDestroy();
        // ScrollContext ctx = new ScrollContext
        // {
        //     Caster = SpawnManager.Instance.GetCurrentPlayer(),
        //     Targets = targets,
        //     Token = token,
        //     Value = damage,
        //     Count = scroll.hitCount,
        // };
        ScrollContext ctx =
            new ScrollContext(scroll.scrollName, SpawnManager.Instance.CurrentPlayer, targets, token, damage, scroll.hitCount);
        Debug.Log($"[test] SpawnManager.Instance.CurrentPlayer: {SpawnManager.Instance.CurrentPlayer}");

        await scroll.logicData.ExecuteAsync(ctx);
    }
    
    
    // 공격은 SpawnManager.Instance.GetCurrentPlayer().PlayAttackAnimation() 쓰시고
    // 피격은 SpawnManager.Instance.GetCurrentMonsters()[n].PlayDamagedAnimation(); 쓰세요, -발렌
    // /// <summary>
    // /// 플레이어 공격 애니메이션 시퀀스
    // /// </summary>
    // private async UniTask PlayPlayerAttackAnimationSequence()
    // {
    //     Debug.Log("플레이어 공격 애니메이션 시퀀스 시작");
    //
    //     // 1. 플레이어 공격 애니메이션
    //     GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
    //     foreach (GameObject playerObj in players)
    //     {
    //         AnimationManager animMgr = playerObj.GetComponent<AnimationManager>();
    //         if (animMgr != null)
    //         {
    //             animMgr.PlayAttack();
    //             Debug.Log($"⚔️ {playerObj.name} 공격 애니메이션 재생");
    //         }
    //     }
    //
    //     await UniTask.Delay(1500); // 공격 애니메이션 시간
    //
    //     // 2. 몬스터 피격 애니메이션
    //     GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
    //     foreach (GameObject monsterObj in monsters)
    //     {
    //         Monster monster = monsterObj.GetComponent<Monster>();
    //         if (monster != null && monster.IsAlive())
    //         {
    //             AnimationManager monsterAnimMgr = monsterObj.GetComponent<AnimationManager>();
    //             if (monsterAnimMgr != null)
    //             {
    //                 monsterAnimMgr.PlayDamaged();
    //                 Debug.Log($"{monsterObj.name} 피격 애니메이션 재생");
    //             }
    //         }
    //     }
    //
    //     await UniTask.Delay(1000); // 피격 애니메이션 시간
    // }

    #endregion

    #region 적 공격턴

    public async void StartEnemyTurn() // 적 공격턴 시작
    {
        ChangeState(CombatState.EnemyAttackTurn);
        OnEnemyAttackTurnStart?.Invoke();
        // Debug.Log("적 공격턴 시작");
        if (startText != null) await startText.StartTurnUI(StartTextType.ENEMYTURN, CurrentTurnCnt);
        selectedTarget.Clear();
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
        // Debug.Log($" {attackingMonster.name} 공격 시작");

        // 1. 몬스터 공격 애니메이션
        attackingMonster.PlayAttackAnimation();

        await UniTask.Delay(500); // 공격 애니메이션 시간

        // 2. 데미지 계산
        int monsterDamage = attackingMonster.GetAttackDamage();
        // Debug.Log($"{attackingMonster.name}의 공격력: {monsterDamage}");

        Player player = SpawnManager.Instance.CurrentPlayer;

        Debug.Log("공격");
        EffectController.Instance.Create(
            monsterAttackEffectPrefab, player.transform.position + ScrollContext.OffsetCenter, 
            0, 10f, 1f, 100, false);

        // 3. 플레이어들에게 데미지 적용
        if (player != null && player.IsAlive())
        {
            // 플레이어 피격 애니메이션
            player.PlayDamagedAnimation();

            // 데미지 적용
            player.TakeDamage(player.CalculateDamage(monsterDamage));
            // Debug.Log($"{playerObj.name}이 {monsterDamage} 데미지를 받았습니다.");

            // 플레이어가 죽었는지 확인하고 사망 애니메이션 재생
            if (!player.IsAlive())
            {
                player.PlayDeathAnimation();
            }
        }

        await UniTask.Delay(1000); // 피격 애니메이션 시간
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
}