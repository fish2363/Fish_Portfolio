using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [field: SerializeField] public InGameManager InGameManager { get; private set; }

    [Header("스크롤 버튼 프리펩")]
    [SerializeField] public GameObject scrollCardPrefab;  // ScrollCard 프리팹 (ScrollCardUI 포함)
    [SerializeField] private ScrollUI scrollDetailUI;     // Scroll 상세보기 UI (팝업용)

    [SerializeField] private ScrollFanSystem fanSystem;
    public SpellGrid spellGrid;
    public BezierArrow bezierArrow; 
    public RectTransform mousePos; 

    [Header("테스트용 선택한 스크롤 리스트")]
    public List<ScrollSO> playerScrolls = new();

    [Header("인풋 막는 패널")]
    [SerializeField] private Image blackBlockImage;
    
    [Header("게임오버 패널")]
    [SerializeField] private GameOverPanel gameOverPanel;

    public Action OnCancelUIClick;
    public Action OnUseUIClick;

    private void Awake()
    {
        InGameManager.OnPlayerAttackTurnStart += UnBlockPlayerInput;
        InGameManager.OnPatternInputStart += BlockPlayerInput;
        
        InGameManager.OnCombatDefeat += OnCombatDefeat;
        
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.OnRoundStarted += OnRoundStarted;
            RoundManager.Instance.OnStageCompleted += OnStageCompleted;
        }
    }

    private void OnDestroy()
    {
        InGameManager.OnPlayerAttackTurnStart -= UnBlockPlayerInput;
        InGameManager.OnPatternInputStart -= BlockPlayerInput;
        
        InGameManager.OnCombatDefeat -= OnCombatDefeat;
        
        var roundManager = RoundManager.TryGetInstance();
        if (roundManager != null)
        {
            roundManager.OnRoundStarted -= OnRoundStarted;
            roundManager.OnStageCompleted -= OnStageCompleted;
        }
    }

    private void UnBlockPlayerInput() 
    {
        Debug.Log("[UIManager] 블랙 패널 해제");
        blackBlockImage.gameObject.SetActive(false);
    }
    
    private void BlockPlayerInput() 
    {
        var currentRoundData = RoundManager.Instance?.GetCurrentRoundData();
        if (currentRoundData != null && currentRoundData.roundType == RoundDataSO.RoundType.Rest)
        {
            return;
        }
        
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
                Debug.Log("[UIManager] 일반 라운드 시작 - 블록패널 활성화 (플레이어 턴까지 대기)");
                if (blackBlockImage != null)
                    blackBlockImage.gameObject.SetActive(true);
            }
        }
    }

    private void Start()
    {
        StartCoroutine(WaitForPlayerAndStartSetting());
    }

    /// <summary>
    /// Player가 생성될 때까지 기다린 후 ScrollCard 생성
    /// </summary>
    private IEnumerator WaitForPlayerAndStartSetting()
    {

        float timeout = 2f;
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            if (Player.Instance != null)
            {
                var playerHand = Player.Instance.GetCurrentHand();
                if (playerHand != null && playerHand.Count > 0)
                {
                    yield return new WaitForSeconds(0.1f);
                    StartSetting();
                    yield break;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
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
        Player currentPlayer = Player.Instance;

        if (currentPlayer != null)
        {
            var playerHand = currentPlayer.GetCurrentHand();

            if (playerHand != null && playerHand.Count > 0)
            {
                playerScrolls = new List<ScrollSO>(playerHand);
                // Debug.Log($"[UIManager] Player의 스크롤 {playerScrolls.Count}개 로드 완료");
                //
                // foreach (var scroll in playerScrolls)
                // {
                //     Debug.Log($"  - {scroll.scrollName}");
                // }
                return;
            }
        }

        var selectedScrollIds = DataManager.Instance?.SelectedScrollIds;

        if (selectedScrollIds != null && selectedScrollIds.Count > 0)
        {
            foreach (string scrollIds in selectedScrollIds)
            {
                if (string.IsNullOrEmpty(scrollIds)) continue;

                var scroll = DataManager.Instance.LoadedScroll.GetValueOrDefault(scrollIds);
                if (scroll != null)
                {
                    playerScrolls.Add(scroll);
                    Debug.Log($"[UIManager] DataManager에서 스크롤 로드: {scroll.scrollName}");
                }
            }
        }
        else
        {
            Debug.LogError("[UIManager] DataManager에 선택된 스크롤이 없음!!!");
            // Debug.LogWarning("[UIManager] Player나 DataManager에 선택된 스크롤이 없어 랜덤 스크롤 사용");
            // var allScrolls = AddressableScrollRepository.All.ToList();
            // if (allScrolls.Count > 0)
            // {
            //     playerScrolls = allScrolls
            //         .OrderBy(x => UnityEngine.Random.value)
            //         .Take(6)
            //         .ToList();
            // }
        }
        // else
        // {
        //     playerScrolls = new List<ScrollSO>();
        // }
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
            GameObject scrollCardObj = Instantiate(scrollCardPrefab, fanSystem.transform);
            
            ScrollCardUI scrollCardUI = scrollCardObj.GetComponent<ScrollCardUI>();
            if (scrollCardUI != null)
            {
                scrollCardUI.InitializeCard(playerScrolls[i], scrollDetailUI, this);
            }
            else
            {
                Debug.LogError($"ScrollCard 프리팹에 ScrollCardUI 컴포넌트가 없습니다!");
            }
        }
    }

    private void OnCombatDefeat()
    {
        ShowGameOverPanel(false);
    }
    
    private void OnStageCompleted(int stageNumber)
    {
        ShowGameOverPanel(true);
    }
    
    private void ShowGameOverPanel(bool isVictory)
    {
        if (gameOverPanel == null)
        {
            return;
        }
        
        int currentRound = RoundManager.Instance != null ? RoundManager.Instance.GetCurrentRound() : 1;
        int currentStage = RoundManager.Instance != null ? RoundManager.Instance.GetCurrentStage() : 1;
        
        UpdateBestRound(currentStage, currentRound);
        int bestRound = GetBestRound(currentStage);
        
        if (isVictory)
        {
        }
        else
        {
            // 패배
        }
        
        gameOverPanel.ShowGameOverPanel(currentRound, bestRound, 0, 0);
    }
    
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
    
    private int GetBestRound(int stageNumber)
    {
        string saveKey = $"BestRound_Stage{stageNumber}";
        return ES3.Load<int>(saveKey, 0);
    }
}
