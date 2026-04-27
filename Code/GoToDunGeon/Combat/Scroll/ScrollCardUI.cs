using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

public class ScrollCardUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
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
    [SerializeField] private Vector2 cardPos = new Vector2(0, 50f);

    [Header("===== 카드 전용 (기존 유지) =====")]
    [SerializeField] private RectTransform childTrans;

    [SerializeField] private ScrollSO currentScroll;
    private ScrollUI detailScrollUI;
    private UIManager uiManager;

    private RectTransform rectTransform;
    private Vector3 originalScale; // 원래 스케일 저장

    private bool isDragging = false;
    private bool isCanUseSkill = false;

    private Monster lastTargetMonster;
    private GameObject prevParent;
    private bool isMovingAnim = false;

    private bool isRewardMode = false;
    private bool bezierActive = false;
    
    private Tweener followTween;
    private const string TweenId_Follow = "CardFollow";
    private const string TweenId_Scale  = "CardScale";
    private float followEaseTime = 0.12f;
    private Ease followEase = Ease.OutQuad;
    private float[] _slotCentersCached; // 길이 = 자식 수(N)  | 드래그 시작 시 캡처
    private float[] _slotMidsCached; // 길이 = N-1        | 드래그 시작 시 계산
    private int _slotCountCached = 0; // N
    private bool _hasSlotCache = false;
    private float midBiasWidthFrac = 0.12f; // mid를 왼쪽으로 이만큼 이동
    private Camera _camera;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // childTrans의 원래 스케일 저장
        if (childTrans != null)
        {
            originalScale = childTrans.localScale;
        }
        _camera = Camera.main;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isRewardMode)
        {
            OnCardClick(); // 보상 모드에서는 단순 클릭만
            return;
        }
        
        if (!isDragging)
            OnCardClick();

        if (!bezierActive)
            isCanUseSkill = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isRewardMode) return;
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isRewardMode) return;
        if (!isDragging) return;

        uiManager.mousePos.position = eventData.position;

        bool wasBezier = bezierActive;
        bool useBezier = ShouldUseBezier(eventData.position);

        if (useBezier)
        {
            if (currentScroll.patternAttackTarget == ScrollSO.PatternAttackTargetType.Targeted)
            {
                FindDistanceMonster();
            }
            else if (currentScroll.patternAttackTarget == ScrollSO.PatternAttackTargetType.All)
            {
                uiManager.bezierArrow.baizerHead.position = uiManager.mousePos.position;
                FindAllMonster();
            }

            if (!isMovingAnim) EnterBezierMode();
            return;
        }

        if (wasBezier)
        {
            ExitBezierMode();
        }

        UpdateFollow(uiManager.mousePos.position);
        
        if (!_hasSlotCache) BuildSlotCache();
        TryReorderInHand(eventData.position);

        if (currentScroll.patternAttackTarget == ScrollSO.PatternAttackTargetType.All)
            ClearAllMonster();
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (isRewardMode) return;
        
        bool isAll = currentScroll.patternAttackTarget == ScrollSO.PatternAttackTargetType.All;
        bool hitTargetTargeted = bezierActive && lastTargetMonster;
        bool hitTargetAll = isAll && isCanUseSkill && (bezierActive || isMovingAnim);
        bool hitTarget = isAll ? hitTargetAll : hitTargetTargeted;
        
        Debug.Log($"[ScrollCardUI] OnEndDrag hitTarget={hitTarget}, bezier={bezierActive}, last={lastTargetMonster}, canUse={isCanUseSkill}, isAll={isAll}");
        
        if (hitTarget)
        {
            OnScrollSelected(currentScroll);
            CancelDragScroll(true);   // UI 유지
        }
        else
        {
            CancelDragScroll(false);  // 원래대로 닫음
        }

        isDragging = false;
    }


    private void CancelDragScroll(bool keepUI = false)   
    {
        if (uiManager && uiManager.bezierArrow)
            uiManager.bezierArrow.StopBazier();

        isMovingAnim = false;

        if (bezierActive)
        {
            ExitBezierMode(); 
        }
        else
        {
            if (prevParent) childTrans.SetParent(prevParent.transform);
            childTrans.DOScale(originalScale, 0.2f).SetId(TweenId_Scale);
        }

        DOTween.Kill(childTrans, TweenId_Follow);
        childTrans.DOLocalMove(Vector3.zero, 0.15f).SetEase(Ease.OutQuad);
        childTrans.DOScale(originalScale, 0.2f).SetId(TweenId_Scale);
        uiManager.fanSystem.ArrangeChildrenInFan();

        if (!keepUI)
            ToggleGrid(false);

        uiManager.fanSystem.ArrangeChildrenInFan();
        _hasSlotCache = false;
        _slotCountCached = 0;
    }

    private void ToggleGrid(bool b)
    {
        //RectTransform rt = (RectTransform)uiManager.spellGrid.transform;

        //rt.DOKill();
        //rt.DOAnchorPosY(b ? 0 : -rt.rect.height - 50f, 0.2f).SetEase(Ease.Linear);
        uiManager?.SetSpellGridVisible(b);
    }

    private void FindAllMonster()
    {
        // 살아있는 몬스터만 선택
        InGameManager.Instance.selectedTargets = SpawnManager.Instance.CurrentMonsters
            .Where(monster => monster != null && monster.IsAlive())
            .ToList();
        uiManager.bezierArrow.CanFindTarget(true);
        isCanUseSkill = InGameManager.Instance.selectedTargets.Count > 0;
    }

    private void ClearAllMonster()
    {
        InGameManager.Instance.selectedTargets.Clear();
        uiManager.bezierArrow.CanFindTarget(false);
        isCanUseSkill = false;
    }

    private void TryReorderInHand(Vector2 screenPos)
    {
        RectTransform parentRt = (RectTransform)uiManager.fanSystem.transform;

        // 드래그 첫 프레임에서 1회 캐시(또는 childCount 변동 시 재빌드)
        if (!_hasSlotCache || _slotCountCached != parentRt.childCount)
            BuildSlotCache();

        // 부모 로컬좌표로 변환
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRt, screenPos, null, out localPoint);
        float mouseX = localPoint.x;

        int n = _slotCountCached;              // 자식 수
        int midCount = Mathf.Max(0, n - 1);    // mid 수
        int slot = (midCount == 0) ? 0 : LowerBound(_slotMidsCached, midCount, mouseX); // 0..n-1

        // 필요할 때만 순서 변경
        int selfIndex = transform.GetSiblingIndex();
        if (slot != selfIndex)
        {
            transform.SetSiblingIndex(slot);
            // 드래그 중인 이 카드만 제외하고 나머지만 재배치(연출 깨지지 않게)
            uiManager.fanSystem.ArrangeChildrenInFan(transform);
        }
    }
    
    private void BuildSlotCache()
    {
        RectTransform parentRt = (RectTransform)uiManager.fanSystem.transform;
        int n = parentRt.childCount;
        _slotCountCached = n;

        if (_slotCentersCached == null || _slotCentersCached.Length != n)
            _slotCentersCached = new float[n];

        // 1) 각 슬롯의 "드래그 시작 시점" 중심 X를 고정 저장
        for (int i = 0; i < n; i++)
        {
            RectTransform rt = (RectTransform)parentRt.GetChild(i);
            _slotCentersCached[i] = rt.localPosition.x;
        }

        int midCount = Mathf.Max(0, n - 1);
        if (_slotMidsCached == null || _slotMidsCached.Length != midCount)
            _slotMidsCached = new float[midCount];

        // 2) 카드 너비(시각상의 겹침 보정을 위해) — 한 번만 읽음
        //    카드들이 같은 프리팹이라면 내 카드의 width로 충분
        RectTransform selfRt = (RectTransform)transform;
        float widthSelf = selfRt.rect.width;

        // 3) 인접 슬롯들의 mid를 "왼쪽으로" 바이어스
        for (int i = 0; i < midCount; i++)
        {
            float xL = _slotCentersCached[i];
            float xR = _slotCentersCached[i + 1];

            float mid = (xL + xR) * 0.5f;

            // 픽셀 오프셋을 0으로 두고, 카드 폭 비율만 적용
            float bias = widthSelf * midBiasWidthFrac;

            _slotMidsCached[i] = mid - bias; // 왼쪽으로 치우침
        }

        _hasSlotCache = true;
    }
    
    private static int LowerBound(float[] mids, int midCount, float value)
    {
        int lo = 0;
        int hi = midCount;
        while (lo < hi)
        {
            int m = (lo + hi) >> 1; // 2로 나눔
            if (value > mids[m]) lo = m + 1;
            else hi = m;
        }
        return lo;
    }

    private void FindDistanceMonster()
    {
        // SafeChoice 스크롤 시스템 제거됨 - 일반 타겟팅 로직 진행

        float closestDistance = Screen.height * 0.06f;
        Monster closestMonster = null;

        foreach (Monster monster in SpawnManager.Instance.CurrentMonsters)
        {
            if (monster == null || !monster.IsAlive()) continue;
            Vector2 screenPos = _camera.WorldToScreenPoint(monster.GetCenterPosition());
            float distance = Vector2.Distance(uiManager.mousePos.position, screenPos);

            if (distance < closestDistance)
            {
                closestMonster = monster;
                closestDistance = distance;
            }
        }

        if (closestMonster == null)
        {
            InGameManager.Instance.selectedTargets.Clear();
            uiManager.bezierArrow.baizerHead.position = uiManager.mousePos.position;
            uiManager.bezierArrow.CanFindTarget(false);
            lastTargetMonster = null;
            isCanUseSkill = false;
            return;
        }

        if (InGameManager.Instance.selectedTargets.Count == 0)
            InGameManager.Instance.selectedTargets.Add(closestMonster);
        else
            InGameManager.Instance.selectedTargets[0] = closestMonster;

        uiManager.bezierArrow.baizerHead.position = _camera.WorldToScreenPoint(closestMonster.GetCenterPosition());
        uiManager.bezierArrow.CanFindTarget(true);
        lastTargetMonster = closestMonster;
        isCanUseSkill = true;
    }

    /// <summary>
    /// 스크롤 카드를 초기화하고 데이터를 로드
    /// CSV 인덱스를 기반으로 스크롤 데이터를 표시합니다.
    /// </summary>
    public void InitializeCard(ScrollSO scroll, ScrollUI detailUI, UIManager uIManager)
    {
        detailScrollUI = detailUI;
        uiManager = uIManager;

        detailScrollUI.OnHidden += () => ToggleGrid(false);

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

        isDragging = false;
        isCanUseSkill = false;

        if (currentScroll)
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
        if (isDragging) return; // 드래그 중이면 클릭 무효화
        if (detailScrollUI == null || currentScroll == null) return;

        // 이미 다른 스크롤이 확대되어 있으면 클릭 무시
        //if (detailScrollUI.gameObject.activeInHierarchy) return;
        if (detailScrollUI.gameObject.activeInHierarchy) return;

        detailScrollUI.ShowScroll(currentScroll, OnScrollSelected);

        ToggleGrid(true);
    }

    private void OnScrollSelected(ScrollSO selectedScroll)
    {
        // 드래그로 이미 타겟이 선택되어 있으면 그대로 사용
        // 클릭 선택 시에만 자동 타겟 설정 (살아있는 몬스터만)
        if (InGameManager.Instance.selectedTargets.Count == 0)
        {
            // 살아있는 몬스터만 필터링
            List<Monster> aliveMonsters = SpawnManager.Instance.CurrentMonsters
                .Where(monster => monster != null && monster.IsAlive())
                .ToList();

            if (selectedScroll.patternAttackTarget == ScrollSO.PatternAttackTargetType.All)
            {
                InGameManager.Instance.selectedTargets = aliveMonsters;
            }
            else if (selectedScroll.patternAttackTarget == ScrollSO.PatternAttackTargetType.Targeted)
            {
                if (aliveMonsters.Count > 0)
                {
                    InGameManager.Instance.selectedTargets.Add(aliveMonsters[0]);
                }
            }
        }

        if (InGameManager.Instance.selectedTargets.Count == 0)
        {
            return;
        }

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
    
    private void StopFollow()
    {
        if (followTween != null && followTween.IsActive())
        {
            followTween.Kill();
            followTween = null;
        }
    }

    private void EnsureFollowTween(Vector3 target)
    {
        if (followTween == null || !followTween.IsActive())
        {
            DOTween.Kill(childTrans, TweenId_Follow);

            followTween = childTrans.DOMove(target, followEaseTime)
                .SetEase(followEase).SetUpdate(true).SetAutoKill(false).SetId(TweenId_Follow);
        }
    }

    private void UpdateFollow(Vector3 target)
    {
        EnsureFollowTween(target);
        followTween.ChangeEndValue(target, followEaseTime, true);
        if (!followTween.IsPlaying()) followTween.Play();
    }

    private bool ShouldUseBezier(Vector2 screenPos)
    {
        Rect playRect = uiManager.GetPlayAreaRect(); // UIManager에 이미 구현됨
        
        if (!bezierActive)
            return playRect.Contains(screenPos);

        return screenPos.y >= playRect.yMin;
    }

    private void EnterBezierMode()
    {
        if (isMovingAnim) return;
        StopFollow();

        isMovingAnim = true;
        bezierActive = true;
        
        childTrans.DOKill();

        prevParent = childTrans.parent.gameObject;
        childTrans.SetParent(childTrans.parent.parent);
        childTrans.DOAnchorPos(cardPos, 0.2f);
        childTrans.DOLocalRotate(Vector3.zero, 0.12f).SetUpdate(true);
        childTrans.DOScale(originalScale * 0.12f * dragEnlargeMultiplier, 0.2f).SetId(TweenId_Scale);
        ToggleGrid(true);

        if (currentScroll.patternAttackTarget == ScrollSO.PatternAttackTargetType.All) return;
        
        uiManager.bezierArrow.StartBazier(childTrans, uiManager.bezierArrow.baizerHead);
    }

    private void ExitBezierMode()
    {
        if (!bezierActive) return;

        uiManager.bezierArrow.StopBazier();
        uiManager.bezierArrow.CanFindTarget(false);
        bezierActive = false;
        isMovingAnim = false;

        childTrans.DOKill();

        childTrans.SetParent(prevParent.transform);
        childTrans.DOLocalRotate(Vector3.zero, 0.12f).SetUpdate(true);

        childTrans.DOScale(originalScale, 0.15f).SetId(TweenId_Scale);
    }
}
