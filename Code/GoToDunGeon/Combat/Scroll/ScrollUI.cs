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

    //[Header("Buttons")]
    //[SerializeField] private Button cancelButton;
    //[SerializeField] private TextMeshProUGUI cancelText;
    //[SerializeField] private Button selectButton;
    //[SerializeField] private TextMeshProUGUI changingText;

    [Header("Animation")]
    [SerializeField] private CanvasGroup canvasGroup; // 페이드 인/아웃 캔버스 그룹
    [SerializeField] private float animationDuration = 0.3f; // 지속 시간

    [Header("UI 모드 설정")]
    [SerializeField] private bool showButtons = true; // 버튼 표시 여부 (false면 카드 모드)

    private ScrollSO currentScroll; // 현재 표시중인 스크롤 데이터
    private Action<ScrollSO> onSelectCallback; // 선택 버튼을 눌렀을 때 실행할 함수를 저장하는 변수
    private Action onCancelCallback;         // 취소 버튼을 눌렀을 때 실행할 함수를 저장하는 변수

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        //SetButtonsVisible(showButtons);

        //if (showButtons)
        //{
        //    // 상세보기 모드: 버튼 이벤트 연결, 시작 시 비활성화
        //    if (cancelButton != null) cancelButton.onClick.AddListener(OnCancelButtonClick);
        //    if (selectButton != null) selectButton.onClick.AddListener(OnSelectButtonClick);
        //    gameObject.SetActive(false);
        //}
    }

    /// <summary>
    /// 카드 영역 밖 클릭 감지용
    /// </summary>
    private void Update()
    {
        if (!gameObject.activeInHierarchy) return;
        //if (cancelButton != null && cancelButton.gameObject.activeSelf) return;

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

    /// <summary>
    /// 버튼 표시/숨기기
    /// </summary>
    //private void SetButtonsVisible(bool visible)
    //{
    //    if (cancelButton != null) cancelButton.gameObject.SetActive(visible);
    //    if (selectButton != null) selectButton.gameObject.SetActive(visible);
    //}

    /// <summary>
    /// 선택 버튼 텍스트를 제외/선택 모드로 변경
    /// </summary>
    //private void SelectExcludeButton(bool isExcludeMode)
    //{
    //    if (changingText == null) return;

    //    changingText.text = isExcludeMode ? "제외" : "선택";
    //    changingText.color = isExcludeMode ? Color.red : Color.black;
    //}

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
        //SetButtonsVisible(showButtons);

        //if (showButtons) SelectExcludeButton(false);

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

        // 버튼 숨기기
        //SetButtonsVisible(false);

        UpdateScrollDisplay();
        Show();

    }

    public void ShowScrollWithExcludeOption(ScrollSO scroll, Action<ScrollSO> onExclude = null)
    {
        if (scroll == null) return;

        currentScroll = scroll;
        onSelectCallback = onExclude;
        onCancelCallback = null;

        // 버튼 표시하되 텍스트를 '제외'로 변경
        //SetButtonsVisible(true);
        //SelectExcludeButton(true);

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
            descriptionText.text = currentScroll.scrollDescription;
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
        gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        transform.localScale = Vector3.one * 0.3f;

        Sequence showSequence = DOTween.Sequence();
        showSequence.Append(canvasGroup.DOFade(1f, animationDuration));
        showSequence.Join(transform.DOScale(1f, animationDuration).SetEase(Ease.OutBack));
    }

    /// <summary>
    /// 스크롤 상세보기 UI를 숨김
    /// 축소 애니메이션 후 비활성화
    /// </summary>
    private void Hide()
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        Sequence hideSequence = DOTween.Sequence();
        hideSequence.Append(canvasGroup.DOFade(0f, animationDuration));
        hideSequence.Join(transform.DOScale(0.3f, animationDuration).SetEase(Ease.InBack));
        hideSequence.OnComplete(() => gameObject.SetActive(false));
    }

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

    private void OnDestroy()
    {
        if (showButtons)
        {
            // 상세보기 모드일 때 버튼 이벤트 정리
            //if (cancelButton != null) cancelButton.onClick.RemoveListener(OnCancelButtonClick);
            //if (selectButton != null) selectButton.onClick.RemoveListener(OnSelectButtonClick);
        }
    }
}