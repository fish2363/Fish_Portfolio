using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ScrollCardUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
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
    
    [Header("===== 드래그 확대 설정 =====")]
    [SerializeField, Range(1f, 2f)] private float dragEnlargeMultiplier = 1.5f;

    [Header("===== 카드 전용 (기존 유지) =====")]
    [SerializeField] private RectTransform childTrans;

    [SerializeField] private ScrollSO currentScroll;
    private ScrollUI detailScrollUI;
    private UIManager uiManager;

    private RectTransform rectTransform;
    private Vector3 originalScale; // 원래 스케일 저장

    private bool longPressed = false;
    private bool isCanUseSkill = false;
    private float pressTime = 0f;
    public float longPressThreshold = 0.3f;

    private Monster lastTargetMonster;
    private GameObject prevParent;
    private Quaternion originRotate;
    private bool isMovingAnim = false;

    private bool isRewardMode = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // childTrans의 원래 스케일 저장
        if (childTrans != null)
        {
            originalScale = childTrans.localScale;
        }
    }

    private void Update()
    {
        if (isRewardMode) return; // 보상 모드에서는 드래그 업데이트 비활성화

        if (pressTime > 0f && !longPressed)
        {
            pressTime += Time.deltaTime;
            if (pressTime >= longPressThreshold)
            {
                longPressed = true;
                transform.SetAsLastSibling();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isRewardMode) return; // 보상 모드에서는 드래그 비활성화

        pressTime = 0.0001f;
        longPressed = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isRewardMode) 
        {
            OnCardClick(); // 보상 모드에서는 단순 클릭만
            return;
        }

        pressTime = 0f;
        if (longPressed)
        {
            CancelDragScroll();
            if (isCanUseSkill) OnScrollSelected(currentScroll);
        }
        else
            OnCardClick();

        isCanUseSkill = false;
    }

    private void CancelDragScroll()
    {
        // if (InGameManager.Instance.selectedTarget.Count > 0) OnScrollSelected(currentScroll);

        // InGameManager.Instance.selectedTarget.Clear();
        uiManager.bezierArrow.StopBazier();
        longPressed = false;
        isMovingAnim = false;

        if (prevParent != null)
            childTrans.SetParent(prevParent.transform);
        childTrans.DORotateQuaternion(originRotate, 0.1f);
        originRotate = Quaternion.identity;

        childTrans.localPosition = Vector2.zero;
        childTrans.DOScale(originalScale, 0.2f);
        
        ToggleGrid(true);

        //var fan = transform.parent.GetComponent<ScrollFanSystem>();
        //if (fan != null)
            //fan.ArrangeChildrenInFan();
    }

    private void ToggleGrid(bool b)
    {
        RectTransform rt = (RectTransform)uiManager.spellGrid.transform;
        
        rt.DOKill();
        rt.DOAnchorPosY(b ? 0 : -rt.rect.height - 50f, 0.2f).SetEase(Ease.Linear);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isRewardMode) return; // 보상 모드에서는 드래그 비활성화
        if (!longPressed) return;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform.parent.parent as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint))
        {
            uiManager.mousePos.position = eventData.position;

            //if (childTrans.position.y > Screen.height / 2)
            //{

                if (currentScroll.patternAttackTarget == ScrollSO.PatternAttackTargetType.Targeted)
                {
                    FindDistanceMonster();

                    if (isMovingAnim) return;
                    isMovingAnim = true;
                    prevParent = childTrans.parent.gameObject;
                    childTrans.SetParent(childTrans.parent.parent);
                    childTrans.DOAnchorPos(new Vector2(0, 50f), 0.2f);
                    originRotate = childTrans.transform.rotation;
                    childTrans.DORotate(Vector3.zero,0.1f);
                    uiManager.bezierArrow.StartBazier(childTrans, uiManager.bezierArrow.baizerHead);
                    childTrans.DOScale(originalScale * 0.12f * dragEnlargeMultiplier, 0.2f);
                    ToggleGrid(false);
                }
                else if(currentScroll.patternAttackTarget == ScrollSO.PatternAttackTargetType.All)
                {
                    uiManager.bezierArrow.baizerHead.position = uiManager.mousePos.position;
                    if (uiManager.bezierArrow.baizerHead.position.y > Screen.height / 2)
                        FindAllMonster();
                    else
                        ClearAllMonster();

                    if (isMovingAnim) return;


                    isMovingAnim = true;
                    prevParent = childTrans.parent.gameObject;
                    childTrans.SetParent(childTrans.parent.parent);
                    childTrans.DOAnchorPos(new Vector2(0, 50f), 0.2f);
                    originRotate = childTrans.transform.rotation;
                    childTrans.DORotate(Vector3.zero, 0.1f);
                    uiManager.bezierArrow.StartBazier(childTrans, uiManager.bezierArrow.baizerHead);
                    childTrans.DOScale(originalScale * 0.12f * dragEnlargeMultiplier, 0.2f);
                    ToggleGrid(false);
                }
            //}
            //else
            //{
                //isCanUseSkill = false;
                //childTrans.position = uiManager.mousePos.position;
                //ChangeScaleToUseAllTargetingScroll(false);

                //int newIndex = 0;
                //for (int i = 0; i < transform.parent.childCount; i++)
                //{
                //    if (transform.parent.GetChild(i) == transform) continue;
                //    if (localPoint.x > transform.parent.GetChild(i).localPosition.x) newIndex = i + 1;
                //}

                //transform.SetSiblingIndex(newIndex);

                //var fan = transform.parent.GetComponent<ScrollFanSystem>();
                //if (fan != null)
                //{
                //    fan.ArrangeChildrenInFan(this.transform);
                //    float rotationAngle = 0f; // 0dmfh ghlwjs
                //    transform.DOLocalRotate(new Vector3(0, 0, rotationAngle), 0.2f).SetEase(Ease.OutQuad);
                //}
            //}
        }
    }

    //private void ChangeScaleToUseAllTargetingScroll(bool isScaleType)
    //{
    //    if(isScaleType)
    //        childTrans.DOScale(Vector3.one * 1.2f, 0.2f);  // 살짝 커지게
    //    else
    //        childTrans.DOScale(Vector3.one, 0.2f);   // 정상 크기
    //}

    private void FindAllMonster()
    {
        InGameManager.Instance.selectedTarget = new List<Monster>(SpawnManager.Instance.CurrentMonsters);
        uiManager.bezierArrow.CanFindTarget(true);
        isCanUseSkill = InGameManager.Instance.selectedTarget.Count > 0;
    }

    private void ClearAllMonster()
    {
        InGameManager.Instance.selectedTarget.Clear();
        uiManager.bezierArrow.CanFindTarget(false);
        isCanUseSkill = false;
    }

    private void FindDistanceMonster()
    {
        // SafeChoice 스크롤 시스템 제거됨 - 일반 타겟팅 로직 진행

        float closestDistance = Screen.height * 0.06f;
        Monster closestMonster = null;

        foreach (Monster monster in SpawnManager.Instance.CurrentMonsters)
        {
            if (monster == null || !monster.IsAlive()) continue;
            Vector2 screenPos = Camera.main.WorldToScreenPoint(monster.GetCenterPosition());
            float distance = Vector2.Distance(uiManager.mousePos.position, screenPos);

            if (distance < closestDistance)
            {
                closestMonster = monster;
                closestDistance = distance;
            }
        }

        if (closestMonster == null)
        {
            InGameManager.Instance.selectedTarget.Clear();
            uiManager.bezierArrow.baizerHead.position = uiManager.mousePos.position;
            uiManager.bezierArrow.CanFindTarget(false);
            lastTargetMonster = null;
            isCanUseSkill = false;
            return;
        }

        if (InGameManager.Instance.selectedTarget.Count == 0)
            InGameManager.Instance.selectedTarget.Add(closestMonster);
        else
            InGameManager.Instance.selectedTarget[0] = closestMonster;

        uiManager.bezierArrow.baizerHead.position = Camera.main.WorldToScreenPoint(closestMonster.GetCenterPosition());
        uiManager.bezierArrow.CanFindTarget(true);
        lastTargetMonster = closestMonster;
        isCanUseSkill = true;
    }


    private void OnMonsterOutOfRange()
    {
        uiManager.bezierArrow.baizerHead.position = uiManager.mousePos.position;
    }

    /// <summary>
    /// 스크롤 카드를 초기화하고 데이터를 로드
    /// CSV 인덱스를 기반으로 스크롤 데이터를 표시합니다.
    /// </summary>
    public void InitializeCard(ScrollSO scroll, ScrollUI detailUI, UIManager uIManager)
    {
        detailScrollUI = detailUI;
        uiManager = uIManager;

        currentScroll = scroll;

        if (currentScroll != null)
        {
            UpdateScrollDisplay();
        }
    }

    /// <summary>
    /// 보상용 간단 초기화 - 드래그/전투 기능 없이 표시용으로만 사용
    /// RewardSelectionUI에서 사용
    /// </summary>
    public void InitializeForReward(ScrollSO scroll)
    {
        currentScroll = scroll;
        
        // 드래그/전투 관련 변수들 비활성화
        longPressed = false;
        isCanUseSkill = false;
        pressTime = 0f;
        
        if (currentScroll != null)
        {
            UpdateScrollDisplay();
        }
    }

    /// <summary>
    /// 보상용 모드 설정
    /// </summary>
    public void SetRewardMode(bool rewardMode)
    {
        isRewardMode = rewardMode;
    }

    /// <summary>
    /// 현재 카드가 표시하고 있는 ScrollSO 데이터 반환
    /// </summary>
    public ScrollSO GetScrollData()
    {
        return currentScroll;
    }

    /// <summary>
    /// 카드 클릭 시 상세보기 ScrollUI를 팝업으로 표시
    /// 상세보기에서 선택/취소가 가능하며, 선택시 InGameManager로 전달
    /// </summary>
    private void OnCardClick()
    {
        if (longPressed) return; // 드래그 중이면 클릭 무효화
        if (detailScrollUI == null || currentScroll == null) return;
        
        // 이미 다른 스크롤이 확대되어 있으면 클릭 무시
        if (detailScrollUI.gameObject.activeInHierarchy) return;
        
        detailScrollUI.ShowScroll(currentScroll, OnScrollSelected);
    }

    private void OnScrollSelected(ScrollSO selectedScroll)
    {
        // 모든 스크롤을 일반 전투 스크롤로 처리 (SafeChoice 시스템 제거됨)
        uiManager.InGameManager.OnScrollSelected(selectedScroll);
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
                // Debug.Log($"✅ MainImage: {currentScroll.scrollIcon.name}");
            }
            else
            {
                mainImage.gameObject.SetActive(false);
                Debug.Log("⚠️ MainImage: scrollIcon 없음");
            }
        }

    }

    /// <summary>
    /// 현재 카드에 표시된 스크롤 데이터를 반환
    /// </summary>
    public ScrollSO GetCurrentScroll() => currentScroll;
}
