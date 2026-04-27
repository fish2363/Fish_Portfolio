using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using UnityEngine.InputSystem;

public class ScrollUI : MonoBehaviour
{
    [Header("===== 메인 배경 =====")]
    [SerializeField] private Image ScrollBackground;          // 두루마리 배경 이미지

    [Header("===== 프레임 이미지들 (등급별) =====")]
    [SerializeField] private Image elementalBackground;       // ElementalScroll/Background
    [SerializeField] private Image manaBackground;           // ManaScroll/Background  

    [Header("===== 원소 & 타입 이미지 =====")]
    [SerializeField] private Image backgroundColorImage;      // ElementalScroll/BackgroundColor (원소별 배경)
    [SerializeField] private Image scrollTypeIcon;           // ElementalScroll/backgroundColor/Icon (타입 아이콘)

    [Header("===== 텍스트 UI =====")]
    [SerializeField] private TextMeshProUGUI titleText;      // TitleText - 스크롤 이름
    [SerializeField] private TextMeshProUGUI manaText;       // ManaText - 마나 코스트
    [SerializeField] private TextMeshProUGUI descriptionText; // ScrollDescriptionText - 스크롤 설명

    [Header("===== 메인 이미지 =====")]
    [SerializeField] private Image mainImage;                // ImagePanel/Image (미정)

    [Header("Buttons")]
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI cancelText;
    [SerializeField] private Button selectButton;
    [SerializeField] public TextMeshProUGUI changingText;

    [Header("Animation")]
    [SerializeField] private CanvasGroup canvasGroup; // 페이드 인/아웃 캔버스 그룹
    [SerializeField] private float animationDuration = 0.3f; // 지속 시간

    [Header("UI 모드 설정")]
    [SerializeField] private bool showButtons = true; // 버튼 표시 여부 (false면 카드 모드)

    [Header("배경 오버레이")]
    private GameObject backgroundOverlay; // 배경 어둡게 하는 오버레이
    private bool useBackgroundOverlay = false; // 배경 오버레이 사용 여부

    private ScrollSO currentScroll; // 현재 표시중인 스크롤 데이터
    private Action<ScrollSO> onSelectCallback; // 선택 버튼을 눌렀을 때 실행할 함수를 저장하는 변수
    private Action onCancelCallback;         // 취소 버튼을 눌렀을 때 실행할 함수를 저장하는 변수
    public event Action OnHidden; //확대된거 없앨때 쓰려고 만든 이벤트 액션함수
    private bool shouldTriggerOnHidden = true; // OnHidden 이벤트 발생 여부 (선택 버튼 클릭 시에는 false)

    //[Header("클릭 처리")]
    //[SerializeField] private bool clickInsideSelectWhenSelectable = true; // onSelect가 있을 때 내부 클릭 -> 선택
    //private Button cardClickCatcher; // 카드 전체를 덮는 클릭 캐처
    //[SerializeField] private RectTransform scrollBackground;

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        CreateBackgroundOverlay();

        SetButtonsVisible(showButtons);

        //if (showButtons)
        //{
        //    // 상세보기 모드: 버튼 이벤트 연결, 시작 시 비활성화
        //    if (cancelButton != null) cancelButton.onClick.AddListener(OnCancelButtonClick);
        //    if (selectButton != null) selectButton.onClick.AddListener(OnSelectButtonClick);
        //    gameObject.SetActive(false);
        //}
        // 리스너는 항상 연결해 둡니다. 버튼은 필요 시 ShowScroll/InfoOnly에서 on/off.
         if (cancelButton != null)
         {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(OnCancelButtonClick);
         }
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnSelectButtonClick);
        }
        gameObject.SetActive(false); // 시작은 비활성
    }

    /// <summary>
    /// 카드 영역 밖 클릭 감지용
    /// </summary>
    private void Update()
    {
        if (!gameObject.activeInHierarchy) return;
        if (cancelButton != null && cancelButton.gameObject.activeSelf) return;

        Vector2 inputPosition = Vector2.zero;
        bool inputDetected = false;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            inputPosition = Mouse.current.position.ReadValue();
            inputDetected = true;
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            inputPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            inputDetected = true;
        }

        if (inputDetected)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                GetComponent<RectTransform>(),
                inputPosition,
                Camera.main))
            {
                Hide();
            }
        }
    }

    //private void EnsureCardClickCatcher()
    //{
    //    if (cardClickCatcher != null) return;

    //    RectTransform targetParent = scrollBackground;
    //    if (targetParent == null)
    //    {
    //        Transform found = transform.Find("ScrollBackground");
    //        targetParent = (found as RectTransform) ?? (RectTransform)transform;
    //    }

    //    GameObject go = new GameObject("CardClickCatcher",typeof(RectTransform), typeof(Image), typeof(Button));

    //    RectTransform rt = go.GetComponent<RectTransform>();

    //    rt.SetParent(targetParent, false); 

    //    rt.pivot = targetParent.pivot;
    //    rt.anchorMin = Vector2.zero;
    //    rt.anchorMax = Vector2.one;
    //    rt.offsetMin = Vector2.zero;
    //    rt.offsetMax = Vector2.zero;
    //    rt.anchoredPosition = Vector2.zero;

    //    Image img = go.GetComponent<Image>();
    //    img.color = new Color(1f, 1f, 1f, 0f);
    //    img.raycastTarget = true;

    //    cardClickCatcher = go.GetComponent<Button>();
    //    cardClickCatcher.transition = Selectable.Transition.None;
    //    cardClickCatcher.onClick.AddListener(OnCardAreaClicked);

    //    rt.SetAsLastSibling();
    //}

    /// <summary>
    /// 버튼 표시/숨기기
    /// </summary>
    private void SetButtonsVisible(bool visible)
    {
        if (cancelButton != null) cancelButton.gameObject.SetActive(visible);
        if (selectButton != null) selectButton.gameObject.SetActive(visible);
    }

    /// <summary>
    /// 선택 버튼 텍스트를 제외/선택 모드로 변경
    /// </summary>
    private void SelectExcludeButton(bool isExcludeMode)
    {
        if (changingText == null) return;

        changingText.text = isExcludeMode ? "제외" : "선택";
        //changingText.color = isExcludeMode ? Color.red : Color.black;
        //현재 선택 색을 흰색으로 하고 아웃라인을 그리자 라는 내용이 나와 수정했습니다. 원래상태로 돌아간다면 위의 코드 다시 살려주세요.
        changingText.color = isExcludeMode ? Color.red : Color.white;
    }

    /// <summary>
    /// 스크롤 객체를 받아 상세보기를 표시
    /// ScrollCardUI에서 호출되며, 팝업 형태로 표시
    /// </summary>
    public void ShowScroll(ScrollSO scroll, Action<ScrollSO> onSelect = null, Action onCancel = null)
    {
        if (scroll == null) return;

        currentScroll = scroll;
        onSelectCallback = onSelect;
        onCancelCallback = onCancel;

        bool showButtons = (onSelect != null || onCancel != null);
        SetButtonsVisible(showButtons);

        if (showButtons) SelectExcludeButton(false);

        UpdateScrollDisplay();
        Show();
    }

    /// <summary>
    /// 스크롤 정보만 표시 (버튼 없이)
    /// 하단 인벤토리에서 사용
    /// </summary>
    public void ShowScrollInfoOnly(ScrollSO scroll)
    {
        if (scroll == null) return;

        currentScroll = scroll;
        onSelectCallback = null;
        onCancelCallback = null;

        //버튼 숨기기
        SetButtonsVisible(false);

        UpdateScrollDisplay();
        Show();

    }

    public void ShowScrollWithExcludeOption(ScrollSO scroll, Action<ScrollSO> onExclude = null)
    {
        if (scroll == null) return;

        currentScroll = scroll;
        onSelectCallback = onExclude;
        onCancelCallback = null;

        //버튼 표시하되 텍스트를 '제외'로 변경
        SetButtonsVisible(true);
        SelectExcludeButton(true);

        UpdateScrollDisplay();
        Show();
    }

    /// <summary>
    /// 스크롤 데이터를 UI 요소들에 실제로 표시하는 메소드
    /// </summary>
    private void UpdateScrollDisplay()
    {
        if (currentScroll == null) return;

        // ===== 1. ScrollBackground - 카드 등급에 맞는 배경 =====
        if (ScrollBackground != null)
        {
            var cardBg = currentScroll.GetCardBackground();
            if (cardBg != null)
            {
                ScrollBackground.sprite = cardBg;
            }
        }

        // ===== 2. Frame 이미지들 - 등급별 테두리 =====
        if (elementalBackground != null)
        {
            var frame01 = currentScroll.GetFrame01();
            if (frame01 != null)
            {
                elementalBackground.sprite = frame01;
            }
        }

        if (manaBackground != null)
        {
            var frame02 = currentScroll.GetFrame02();
            if (frame02 != null)
            {
                manaBackground.sprite = frame02;
            }
        }

        // ===== 3. ElementGems - 원소에 맞는 배경 젬 =====
        if (backgroundColorImage != null)
        {
            var elementGem = currentScroll.GetElementGem();
            if (elementGem != null)
            {
                backgroundColorImage.sprite = elementGem;
            }
        }

        // ===== 4. ScrollTypeImages - 스크롤 타입 아이콘 =====
        if (scrollTypeIcon != null)
        {
            var typeIcon = currentScroll.GetScrollTypeIcon();
            if (typeIcon != null)
            {
                scrollTypeIcon.sprite = typeIcon;
                scrollTypeIcon.gameObject.SetActive(true);
            }
            else
            {
                scrollTypeIcon.gameObject.SetActive(false);
            }
        }

        // ===== 5. 텍스트 UI 요소들 =====
        if (manaText != null)
        {
            manaText.text = currentScroll.scrollManaCost.ToString();
        }

        if (titleText != null)
        {
            titleText.text = currentScroll.scrollName;
        }

        if (descriptionText != null)
        {
            // 플레이어 공격력을 가져와서 계산된 설명 표시
            int playerAttack = GetPlayerAttack();
            string calculatedDesc = currentScroll.GetCalculatedDescription(playerAttack);

            descriptionText.text = calculatedDesc;
        }

        // ===== 6. 메인 이미지 =====
        if (mainImage != null)
        {
            if (currentScroll.scrollIcon != null)
            {
                mainImage.sprite = currentScroll.scrollIcon;
                mainImage.gameObject.SetActive(true);
                Debug.Log($"✅ MainImage: {currentScroll.scrollIcon.name}");
            }
            else
            {
                mainImage.gameObject.SetActive(false);
                Debug.Log("⚠️ MainImage: scrollIcon 없음");
            }
        }

    }

    /// <summary>
    /// 스크롤 상세보기 UI를 팝업으로 표시
    /// 작은 크기에서 원본 크기로 확대되는 애니메이션을 포함
    /// </summary>
    private void Show()
    {
        Debug.Log("[ScrollUI] Show() 함수 실행!");
        gameObject.SetActive(true);

        // UI 최상위로 보내기
        transform.SetAsLastSibling();

        if (useBackgroundOverlay)
        {
            ShowBackgroundOverlay();
        }
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        transform.localScale = Vector3.one * 0.3f;

        Debug.Log($"[ScrollUI] DOTween 애니메이션 시작 - alpha: {canvasGroup.alpha}, scale: {transform.localScale}");

        Sequence showSequence = DOTween.Sequence();
        showSequence.Append(canvasGroup.DOFade(1f, animationDuration));
        showSequence.Join(transform.DOScale(1f, animationDuration).SetEase(Ease.OutBack));

        showSequence.OnComplete(() => {
            Debug.Log($"[ScrollUI] DOTween 완료! - alpha: {canvasGroup.alpha}, scale: {transform.localScale}");
        });
    }

    /// <summary>
    /// 스크롤 상세보기 UI를 숨김
    /// 축소 애니메이션 후 비활성화
    /// </summary>
    private void Hide()
    {
        if (useBackgroundOverlay)
        {
            HideBackgroundOverlay();
        }

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        Sequence hideSequence = DOTween.Sequence();
        hideSequence.Append(canvasGroup.DOFade(0f, animationDuration));
        hideSequence.Join(transform.DOScale(0.3f, animationDuration).SetEase(Ease.InBack));
        hideSequence.OnComplete(() =>
        {
            gameObject.SetActive(false);

            // shouldTriggerOnHidden 플래그가 true일 때만 OnHidden 이벤트 발생
            if (shouldTriggerOnHidden)
            {
                OnHidden?.Invoke();
            }

            // 플래그 초기화
            shouldTriggerOnHidden = true;
        });
    }

    #region 배경 오버레이 시스템

    /// <summary>
    /// 배경 오버레이 사용 여부 설정 (RewardSelectionUI에서만 true로 설정)
    /// </summary>
    public void SetUseBackgroundOverlay(bool use)
    {
        useBackgroundOverlay = use;
    }

    private void CreateBackgroundOverlay()
    {
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null) return;

        backgroundOverlay = new GameObject("ScrollUI_BackgroundOverlay");
        backgroundOverlay.transform.SetParent(parentCanvas.transform, false);
        backgroundOverlay.transform.SetSiblingIndex(transform.GetSiblingIndex());

        RectTransform rect = backgroundOverlay.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        UnityEngine.UI.Image image = backgroundOverlay.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0f, 0f, 0f, 0.95f);
        image.raycastTarget = true;

        CanvasGroup canvasGroup = backgroundOverlay.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        backgroundOverlay.SetActive(false);
    }

    private void ShowBackgroundOverlay()
    {
        if (backgroundOverlay == null) return;

        backgroundOverlay.SetActive(true);
        backgroundOverlay.GetComponent<CanvasGroup>().DOFade(1f, animationDuration);
    }

    private void HideBackgroundOverlay()
    {
        if (backgroundOverlay == null) return;

        backgroundOverlay.GetComponent<CanvasGroup>().DOFade(0f, animationDuration).OnComplete(() =>
        {
            backgroundOverlay.SetActive(false);
        });
    }

    #endregion

    /// <summary>
    /// 취소 버튼 클릭시 숨기기
    /// </summary>
    private void OnCancelButtonClick()
    {
        onCancelCallback?.Invoke();
        Hide();
    }

    /// <summary>
    /// 선택 버튼 클릭시 호출
    /// </summary>
    private void OnSelectButtonClick()
    {
        // 선택 버튼으로 스크롤 선택 시에는 OnHidden 이벤트를 발생시키지 않음
        // (스펠 그리드가 꺼지는 것을 방지)
        shouldTriggerOnHidden = false;

        onSelectCallback?.Invoke(currentScroll);
        Hide();
    }

    /// <summary>
    /// 현재 스크롤 데이터 가져오기
    /// </summary>
    public ScrollSO GetCurrentScroll()
    {
        return currentScroll;
    }
    
    /// <summary>
    /// 저장된 데이터에서 플레이어 공격력 계산
    /// </summary>
    private int GetPlayerAttack()
    {
        // 1. 기본 캐릭터 공격력 (DataManager에서)
        int baseAttack = GetCharacterBaseAttack();

        // 2. 재능 보너스 (RelicManager에서)
        int relicBonus = GetRelicAttackBonus();

        // 3. 아티팩트 보너스 (ArtifactManager에서)
        int artifactBonus = GetArtifactAttackBonus();

        return baseAttack + relicBonus + artifactBonus;
    }

    private int GetCharacterBaseAttack()
    {
        // 선택된 캐릭터의 기본 공격력
        if (DataManager.Instance?.SelectedCharacterData != null)
        {
            return DataManager.Instance.SelectedCharacterData.stats.attack;
        }
        return 20; // 기본값
    }

    private int GetRelicAttackBonus()
    {
        if (RelicManager.Instance == null) return 0;

        int bonus = 0;
        foreach (var relicState in RelicManager.Instance.ownedRelics)
        {
            if (relicState.level <= 0) continue;

            foreach (var effect in relicState.currentEffects)
            {
                if ((EffectType)effect.effectType == EffectType.공격력증가)
                {
                    bonus += Mathf.RoundToInt(effect.currentValue);
                }
            }
        }
        return bonus;
    }

    private int GetArtifactAttackBonus()
    {
        if (ArtifactManager.Instance == null) return 0;

        int bonus = 0;
        foreach (var artifact in ArtifactManager.Instance.GetPlayerArtifacts())
        {
            if (artifact.EffectType == ArtifactEffectType.Offense)
                bonus += Mathf.RoundToInt(artifact.Value1);
            if (artifact.EffectType2 == ArtifactEffectType2.Offense)
                bonus += Mathf.RoundToInt(artifact.Value2);
        }
        return bonus;
    }

    private void OnDestroy()
    {
        if (showButtons)
        {
            //상세보기 모드일 때 버튼 이벤트 정리
            if (cancelButton != null) cancelButton.onClick.RemoveListener(OnCancelButtonClick);
            if (selectButton != null) selectButton.onClick.RemoveListener(OnSelectButtonClick);
        }

        // 배경 오버레이 정리
        if (backgroundOverlay != null)
        {
            Destroy(backgroundOverlay);
        }
    }
}