using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using Sound;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [field: SerializeField] public InGameManager InGameManager { get; private set; }

    [Header("스크롤 버튼 프리펩")]
    [SerializeField] public GameObject scrollCardPrefab;  // ScrollCard 프리팹 (ScrollCardUI 포함)
    [SerializeField] private ScrollUI scrollDetailUI;     // Scroll 상세보기 UI (팝업용)

    [SerializeField] public ScrollFanSystem fanSystem;
    public SpellGrid spellGrid;
    public BezierArrow bezierArrow; 
    public RectTransform mousePos; 

    [Header("테스트용 선택한 스크롤 리스트")]
    public List<ScrollSO> playerScrolls = new();
    
    [Header("UI를 뺀 연출 영역")]
    [SerializeField] public GameObject playArea;

    [Header("인풋 막는 패널")]
    [SerializeField] private Image blackBlockImage;
    
    [Header("게임오버 패널")]
    [SerializeField] private GameOverPanel gameOverPanel;
    
    [Header("메뉴패널")]
    [SerializeField] private GameObject menuButton;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject leaveConfirmPanel;
    
    [Header("루트 캔버스")]
    [SerializeField] public Canvas canvas;

    [Header("Auto Battle UI")]
    [SerializeField] private Toggle autoPatternToggle;  // 패턴만 자동
    [SerializeField] private Toggle fullAutoToggle;     // 완전 자동
    [SerializeField] private GameObject autoIcon;       // "AUTO" 표시 아이콘

    public Action OnCancelUIClick;
    public Action OnUseUIClick;

    private void Awake()
    {
        InGameManager.OnPlayerAttackTurnStart += UnBlockPlayerInput;
        InGameManager.OnPatternInputStart += BlockPlayerInput;
        
        // 전투 결과 이벤트 구독
        InGameManager.OnCombatDefeat += OnCombatDefeat;
        
        // 휴식 라운드 시작 시에도 블록 해제
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.OnRoundStarted += OnRoundStarted;
            RoundManager.Instance.OnStageCompleted += OnStageCompleted;
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            OnBack();
        }
    }

    private void OnDestroy()
    {
        InGameManager.OnPlayerAttackTurnStart -= UnBlockPlayerInput;
        InGameManager.OnPatternInputStart -= BlockPlayerInput;
        
        // 전투 결과 이벤트 구독 해제
        InGameManager.OnCombatDefeat -= OnCombatDefeat;
        
        // RoundManager 인스턴스가 이미 존재할 때만 이벤트 해제 (자동 생성 방지)
        var roundManager = RoundManager.TryGetInstance();
        if (roundManager != null)
        {
            roundManager.OnRoundStarted -= OnRoundStarted;
            roundManager.OnStageCompleted -= OnStageCompleted;
        }
    }
    public void SetSpellGridVisible(bool visible)
    {
        RectTransform rt = (RectTransform)spellGrid.transform;
        rt.DOKill();
        rt.DOAnchorPosY(visible ? 0 : -rt.rect.height - 50f, 0.2f).SetEase(Ease.Linear);
    }
    public void ShowSpellGrid() => SetSpellGridVisible(true);
    public void HideSpellGrid() => SetSpellGridVisible(false);

    private void UnBlockPlayerInput() 
    {
        Debug.Log("[UIManager] 블랙 패널 해제");
        blackBlockImage.gameObject.SetActive(false);
    }
    
    private void BlockPlayerInput() 
    {
        // 휴식 라운드에서는 블록하지 않음
        var currentRoundData = RoundManager.Instance?.GetCurrentRoundData();
        if (currentRoundData != null && currentRoundData.roundType == RoundDataSO.RoundType.Rest)
        {
            Debug.Log("[UIManager] 휴식 라운드에서는 블록패널 사용하지 않음");
            return;
        }
        
        Debug.Log("[UIManager] 블랙 패널 활성화");
        blackBlockImage.gameObject.SetActive(true);
    }
    
    private void OnRoundStarted(int stage, int round, bool isBoss)
    {
        var currentRoundData = RoundManager.Instance.GetCurrentRoundData();
        if (currentRoundData != null)
        {
            if (currentRoundData.roundType == RoundDataSO.RoundType.Rest)
            {
                Debug.Log("[UIManager] 휴식 라운드 시작 - 블록패널 해제");
                UnBlockPlayerInput();
            }
            else
            {
                // 일반 라운드에서는 플레이어 턴이 시작될 때까지 블록패널 유지
                if (blackBlockImage != null)
                    blackBlockImage.gameObject.SetActive(true);

                // 스크롤 데이터가 변경되었을 수 있으므로 UI 갱신
                StartCoroutine(RefreshScrollUIAfterDelay());
            }
        }
    }

    // 잠시 대기 후 스크롤 UI 갱신 (DataManager 동기화 완료 대기)
    private IEnumerator RefreshScrollUIAfterDelay()
    {
        yield return new WaitForSeconds(0.5f); // DataManager 동기화 대기
        StartSetting();
    }

    private void Start()
    {
        // 블랙 패널 초기 상태는 OnRoundStarted에서 관리됨
        // Debug.Log("[UIManager] UIManager 시작 - 블랙 패널 상태는 라운드 시작 이벤트에서 설정");

        // // Player 생성을 기다린 후 ScrollCard 생성
        // StartCoroutine(WaitForPlayerAndStartSetting());

        // 자동 전투 UI 초기화
        InitializeAutoBattleUI();
    }

    // Player가 생성될 때까지 기다린 후 ScrollCard 생성
    public IEnumerator WaitForPlayerAndStartSetting()
    {
        float timeout = 2f;
        float elapsed = 0f;

        // Player 객체가 생성될 때까지 대기
        while (elapsed < timeout)
        {
            if (SpawnManager.Instance.CurrentPlayer != null)
            {
                yield return new WaitForSeconds(0.1f);
                StartSetting();
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 타임아웃 발생 시에도 StartSetting 실행
        StartSetting();
    }
    
    public void StartSetting()
    {
        spellGrid.gameObject.SetActive(true);

        if (fanSystem == null || scrollCardPrefab == null || scrollDetailUI == null) return;

        ClearExistingCards();

        LoadPlayerHasScrolls();

        CreateScrollCards();

        fanSystem.ArrangeChildrenInFan();
    }

    private void LoadPlayerHasScrolls()
    {
        playerScrolls.Clear();

        // DataManager에서 스크롤 ID 목록 가져오기 (GameSessionData와 동기화됨)
        var selectedScrollIds = DataManager.Instance?.SelectedScrollIds;

        if (selectedScrollIds != null && selectedScrollIds.Count > 0)
        {
            foreach (string scrollId in selectedScrollIds)
            {
                if (string.IsNullOrEmpty(scrollId)) continue;

                var scroll = DataManager.Instance.LoadedScroll.GetValueOrDefault(scrollId);
                if (scroll != null)
                {
                    playerScrolls.Add(scroll);
                }
            }

            // Player.Hand도 동기화
            Player currentPlayer = SpawnManager.Instance.CurrentPlayer;
            if (currentPlayer != null)
            {
                var playerHand = currentPlayer.GetCurrentHand();
                playerHand.Clear();
                foreach (var scroll in playerScrolls)
                {
                    currentPlayer.AddScrollToHand(scroll);
                }
            }
        }
    }

    /// <summary>
    /// 기존 카드들 제거
    /// </summary>
    private void ClearExistingCards()
    {
        for (int i = fanSystem.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(fanSystem.transform.GetChild(i).gameObject);
        }
    }
    
    /// <summary>
    /// ScrollCard 프리팹들 생성 및 초기화
    /// </summary>
    private void CreateScrollCards()
    {
        for (int i = 0; i < playerScrolls.Count; i++)
        {
            // ScrollCard 프리팹 생성
            GameObject scrollCardObj = Instantiate(scrollCardPrefab, fanSystem.transform);

            // ScrollCardUI 컴포넌트 가져오기
            ScrollCardUI scrollCardUI = scrollCardObj.GetComponent<ScrollCardUI>();
            if (scrollCardUI != null)
            {
                // 카드 초기화: (CSV인덱스, 상세보기UI, CSV파일)
                scrollCardUI.InitializeCard(playerScrolls[i], scrollDetailUI, this);
            }
            else
            {
                Debug.LogError($"ScrollCard 프리팹에 ScrollCardUI 컴포넌트가 없습니다!");
            }
        }
    }

    // 게임오버 패널 관리
    
    // 전투 패배 시 호출
    private void OnCombatDefeat()
    {
        ShowGameOverPanel(false);
    }
    
    // 스테이지 완료 시 호출 (30라운드 클리어)
    private void OnStageCompleted(int stageNumber)
    {
        ShowGameOverPanel(true);
    }
    
    // 게임오버 패널 표시
    private void ShowGameOverPanel(bool isVictory)
    {
        if (gameOverPanel == null)
        {
            Debug.LogError("GameOverPanel이 설정되지 않았습니다!");
            return;
        }
        
        // 현재 라운드 정보 가져오기
        int currentRound = RoundManager.Instance != null ? RoundManager.Instance.GetCurrentRound() : 1;
        int currentStage = RoundManager.Instance != null ? RoundManager.Instance.GetCurrentStage() : 1;
        
        // 최고 달성 라운드 업데이트 및 가져오기
        UpdateBestRound(currentStage, currentRound);
        int bestRound = GetBestRound(currentStage);
        
        if (isVictory)
        {
            // 30라운드 클리어 (완주)
            Debug.Log("🎉 스테이지 완료! 게임오버 패널 표시");
        }
        else
        {
            // 패배
            Debug.Log("💀 게임 패배! 게임오버 패널 표시");
        }

        // 골드 70개, 다이아 70개 고정 보상 + 재능 재화량증가 보너스
        int baseGold = 70;
        int bonusGold = 0;
        if (RelicManager.Instance != null)
        {
            float bonusPercent = RelicManager.Instance.GetTotalValue(EffectType.재화량증가);
            bonusGold = Mathf.RoundToInt(baseGold * bonusPercent / 100f);
        }
        gameOverPanel.ShowGameOverPanel(currentRound, bestRound, baseGold + bonusGold, 70, isVictory);
    }
    
    // 최고 달성 라운드 업데이트 (Easy Save 3 사용)
    private void UpdateBestRound(int stageNumber, int currentRound)
    {
        string saveKey = $"BestRound_Stage{stageNumber}";
        int previousBest = ES3.Load<int>(saveKey, 0);
        
        if (currentRound > previousBest)
        {
            ES3.Save(saveKey, currentRound);
            Debug.Log($"새로운 최고 기록! Stage{stageNumber}: {currentRound}라운드");
        }
    }
    
    // 최고 달성 라운드 가져오기 (Easy Save 3 사용)
    private int GetBestRound(int stageNumber)
    {
        string saveKey = $"BestRound_Stage{stageNumber}";
        return ES3.Load<int>(saveKey, 0);
    }

    public Rect GetPlayAreaRect()
    {
        RectTransform area = playArea.GetComponent<RectTransform>();

        Vector3[] corners = new Vector3[4];
        area.GetWorldCorners(corners);

        Canvas canvas = area.GetComponentInParent<Canvas>();
        Camera cam = (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? (canvas.worldCamera ? canvas.worldCamera : Camera.main)
            : null; // Overlay면 null!

        Vector2 p0 = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
        Vector2 p1 = RectTransformUtility.WorldToScreenPoint(cam, corners[1]);
        Vector2 p2 = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);
        Vector2 p3 = RectTransformUtility.WorldToScreenPoint(cam, corners[3]);

        float minX = Mathf.Min(p0.x, Mathf.Min(p1.x, Mathf.Min(p2.x, p3.x)));
        float maxX = Mathf.Max(p0.x, Mathf.Max(p1.x, Mathf.Max(p2.x, p3.x)));
        float minY = Mathf.Min(p0.y, Mathf.Min(p1.y, Mathf.Min(p2.y, p3.y)));
        float maxY = Mathf.Max(p0.y, Mathf.Max(p1.y, Mathf.Max(p2.y, p3.y)));

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    public void OnBack()
        => ToggleMenu(true);
    
    public void OnMenuButtonClick()
        => ToggleMenu(true);
    
    public void OnMenuBackgroundClick()
        => ToggleMenu(false);

    public void OnResumeButtonClick()
        => ToggleMenu(false);


    private void ToggleMenu(bool open)
    {
        menuPanel.SetActive(open);
        leaveConfirmPanel.SetActive(false);
        Time.timeScale = open ? 0f : 1f;
    }
    
    public void OnSettingButtonClick()
    {
        //셋팅버튼은 미구현
    }
    
    public void OnLeaveButtonClick()
    {
        //메인메뉴로 나가기 버튼
        leaveConfirmPanel.SetActive(true);
    }

    public void OnLeaveConfirmButtonClick()
    {
        Time.timeScale = 1f;
        SoundManager.Instance.StopBGM();
        SceneManager.LoadScene("0_LobbyScene");
    }

    public void OnLeaveCancelButtonClick()
    {
        leaveConfirmPanel.SetActive(false);
    }

    #region 자동 전투 UI

    private void InitializeAutoBattleUI()
    {
        if (autoPatternToggle == null || fullAutoToggle == null) return;

        // 해금 여부 확인
        bool isUnlocked = DataManager.Instance.userModel.IsAutoBattleUnlocked;

        // 토글 상태 설정
        autoPatternToggle.interactable = isUnlocked;
        fullAutoToggle.interactable = isUnlocked;
        autoPatternToggle.isOn = false;
        fullAutoToggle.isOn = false;

        // 이벤트 연결
        autoPatternToggle.onValueChanged.AddListener(OnAutoPatternToggleChanged);
        fullAutoToggle.onValueChanged.AddListener(OnFullAutoToggleChanged);

        // 아이콘 초기 비활성화
        if (autoIcon != null)
        {
            autoIcon.SetActive(false);
        }
    }

    private void OnAutoPatternToggleChanged(bool isOn)
    {
        // 패턴 자동 모드 설정
        InGameManager.isAutoPatternMode = isOn;

        // 완전 자동이 켜져있으면 패턴 자동은 자동으로 활성화
        if (InGameManager.isFullAutoMode && !isOn)
        {
            // 완전 자동 끄기
            fullAutoToggle.isOn = false;
        }

        UpdateAutoIcon();
        Debug.Log($"패턴 자동: {(isOn ? "ON" : "OFF")}");
    }

    private void OnFullAutoToggleChanged(bool isOn)
    {
        // 완전 자동 모드 설정
        InGameManager.isFullAutoMode = isOn;

        // 완전 자동이 켜지면 패턴 자동도 자동으로 활성화
        if (isOn && !InGameManager.isAutoPatternMode)
        {
            autoPatternToggle.isOn = true;
        }

        UpdateAutoIcon();
        Debug.Log($"완전 자동: {(isOn ? "ON" : "OFF")}");
    }

    private void UpdateAutoIcon()
    {
        if (autoIcon == null) return;

        // 완전 자동이거나 패턴 자동일 때 아이콘 표시
        bool showIcon = InGameManager.isAutoPatternMode || InGameManager.isFullAutoMode;

        autoIcon.SetActive(showIcon);
    }

    #endregion
}
