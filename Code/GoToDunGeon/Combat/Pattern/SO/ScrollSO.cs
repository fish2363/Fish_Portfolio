using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Scroll_", menuName = "Game/Scroll")]
public class ScrollSO : ScriptableObject
{
    [Header("기본 정보")]
    public string scrollName;               // 스크롤 이름 (고유값)
    public Sprite scrollIcon;                     // 스크롤 아이콘
    public bool unlocked;                           //스크롤 해금여부
    [TextArea(3, 5)]
    public string scrollDescription;              // 스크롤 설명

    [Header("속성 및 가치")]
    public ElementSystem.ElementType elementType = ElementSystem.ElementType.None; // 속성 (9가지)
    [Range(1, 6)]
    public int scrollCost = 2;                    // 코스트 (가치, 1-6)
    [Range(1, 3)]
    public int scrollLevel = 1;                   // 레벨 (성장치, 1-3)

    [Header("스크롤 타입")]
    [Tooltip("Attack, Defense, Support, Debuff, Recall 중 선택")]
    public ScrollType scrollType = ScrollType.Attack;  // 스크롤 타입 (5가지)

    [Header("사용 제한")]
    public int scrollManaCost = 5;                // 마나 소모량 (사용 시 소모)
    //public float scrollCooldown = 3f;             // 쿨다운 시간 (초)
    [Range(1, 10)]
    public int scrollMaxUsageCount = 3;           // 최대 사용 횟수 (전투당)
    [System.NonSerialized]
    public int scrollCurrentUsageCount = 0;       // 현재 사용 횟수 (런타임)

    [Header("효과 - 데미지")]
    public int baseDamage = 10;             // 기본 데미지
    [Min(1)] public int hitCount = 1;       // 몇 번 데미지 줄건지
    [Range(0f, 3f)]
    public float attackCoefficient = 1.0f;  // 스킬 계수 (공격력에 곱해지는 값)
    public bool isMultiAttack = false;      // 다중공격 여부
    public int multiAttackTargets = 1;      // 다중공격 대상 수

    [Header("효과 - 디버프")]
    //public StatusEffectType statusEffect = StatusEffectType.None; // 부여할 상태이상
    public float statusChance = 0f;         // 상태이상 확률 (0~1)
    public int statusDuration = 3;          // 상태이상 지속시간

    [Header("자속보정 특수효과")]
    public bool hasSelfElementBonus = true; // 자속보정 효과 있는지
    public float selfElementDamageBonus = 0.2f;     // 자속보정 시 추가 데미지 (20%)
    //public StatusEffectType selfElementStatus = StatusEffectType.None; // 자속보정 시 추가 상태이상

    [Header("공격 범위")]
    public PatternAttackTargetType patternAttackTarget = PatternAttackTargetType.Targeted;
    [Header("시각적 설정")]
    public PatternDifficulty patternDifficulty = PatternDifficulty.Easy; // 패턴 난이도
    public int patternComplexity = 3;       // 패턴 복잡도 (터치 포인트 수)
    public ScrollRarity rarity = ScrollRarity.Normal;   // 희귀도 (UI용)

    [Header("상점 정보")]
    public int shopPrice = 100;             // 상점 구매 가격
    public ScrollCategory category = ScrollCategory.Attack; // 스크롤 카테고리

    [Header("패턴 구성")]
    [SerializeField, Range(3, 12)]
    public int patternLength = 4;                    // 패턴 길이 (3~12개)

    private ElementSystem.ElementType[] patternSequence;         // 패턴 시퀀스 (실제 입력해야 할 순서)

    [Header("난이도 설정")]
    public float timeLimit = 5f;                     // 제한 시간
    public float accuracyThreshold = 0.7f;           // 성공 판정 기준 (70% 이상)

    [Header("UI 자동 연동")]
    [Tooltip("ScrollUIDatabase는 프로젝트에서 자동으로 찾아서 사용됩니다.")]
    [SerializeField] private bool _uiAutoFind = true; // Inspector 표시용

    // UI 리소스 데이터베이스 (자동 찾기)
    private static ScrollUIDatabase _cachedUIDatabase;
    public ScrollUIDatabase scrollUIDatabase
    {
        get
        {
            if (_cachedUIDatabase == null)
            {
                // 프로젝트에서 ScrollUIDatabase 타입의 SO 찾기
                var databases = Resources.LoadAll<ScrollUIDatabase>("ScrollUIDatabase");
                if (databases.Length > 0)
                {
                    _cachedUIDatabase = databases[0];
                }
            }
            
            return _cachedUIDatabase;
        }
    }

    [Header("시각적 설정")]
    public GameObject[] patternPrefab;               // 패턴 UI 프리팹들
    public ParticleSystem successEffect;            // 성공 이펙트
    public ParticleSystem failEffect;               // 실패 이펙트

    [Header("스킬 로직 데이터")]
    public LogicData logicData; //스킬 발동 로직이 담긴 SO

    public ElementSystem.ElementType[] GetPattern()
    {
        if (patternSequence == null || patternSequence.Length == 0)
            return new ElementSystem.ElementType[0];

        HashSet<ElementSystem.ElementType> uniqueElements = new();

        foreach (ElementSystem.ElementType element in patternSequence)
        {
            uniqueElements.Add(element);
        }

        ElementSystem.ElementType[] result = new ElementSystem.ElementType[uniqueElements.Count];
        uniqueElements.CopyTo(result);
        return result;
    }

    public ElementSystem.ElementType[] GetPatternSequence()
    {
        return patternSequence ?? Array.Empty<ElementSystem.ElementType>();
    }

    public void SetPatternSequence(ElementSystem.ElementType[] sequence)
    {
        if (sequence == null || sequence.Length == 0)
        {
            patternSequence = System.Array.Empty<ElementSystem.ElementType>();
            patternLength = 0;
            return;
        }

        patternSequence = (ElementSystem.ElementType[])sequence.Clone();
        patternLength = patternSequence.Length;
    }

    /// <summary>
    /// 패턴 자동 생성
    /// </summary>
    public ElementSystem.ElementType[] GenerateRandomPattern()
    {
        patternSequence = new ElementSystem.ElementType[patternLength];

        ElementSystem.ElementType[] availableElements = (ElementSystem.ElementType[])Enum.GetValues(typeof(ElementSystem.ElementType));
        int rand;
        patternSequence[0] = elementType;
        for (int i = 1; i < patternLength; i++)
        {
            do rand = UnityEngine.Random.Range(0, availableElements.Length);
            while (availableElements[rand] == ElementSystem.ElementType.None);

            patternSequence[i] = availableElements[rand];
        }
        return patternSequence;
    }

    // 스킬 계수 반환 (공격력에 곱해지는 값)
    public float GetAttackCoefficient()
    {
        return attackCoefficient;
    }

    // 스크롤 희귀도
    public enum ScrollRarity
    {
        Normal,     // 일반 (흰색)
        Rare,       // 희귀 (파란색)
        Epic,       // 영웅 (보라색)
        Legendary   // 전설 (주황색)
    }

    // 패턴 난이도
    public enum PatternDifficulty
    {
        Easy,       // 쉬움 (넉넉한 타이밍)
        Normal,     // 보통
        Hard,       // 어려움 (정확한 타이밍 필요)
        Expert      // 전문가 (매우 정확해야 함)
    }

    // 패턴 스킬 공격 방식
    public enum PatternAttackTargetType
    {
        All,       // 전체 공격
        Targeted   // 특정 대상 공격
    }

    // 스크롤 카테고리
    public enum ScrollCategory
    {
        Attack,     // 공격
        Defense,    // 방어
        Support,    // 지원
        Debuff,     // 디버프
        Recall     // 소환
    }

    #region 에디터 검증
    void OnValidate()
    {
        // 패턴 길이가 변경되면 배열 크기 조정
        if (patternSequence != null && patternSequence.Length != patternLength)
        {
            System.Array.Resize(ref patternSequence, patternLength);
        }
    }
    #endregion

    #region 사용 횟수 관리

    // 스크롤 사용 가능 여부 체크 (사용 횟수 포함)
    public bool CanBeUsed(int currentMana, bool isOnCooldown)
    {
        return currentMana >= scrollManaCost &&
               !isOnCooldown &&
               scrollCurrentUsageCount < scrollMaxUsageCount;
    }

    // 스크롤 사용 (사용 횟수 증가)
    public bool UseScroll()
    {
        if (scrollCurrentUsageCount >= scrollMaxUsageCount) return false;

        scrollCurrentUsageCount++;
        return true;
    }

    // 남은 사용 횟수 가져오기
    public int GetRemainingUsage()
    {
        return scrollMaxUsageCount - scrollCurrentUsageCount;
    }

    // 사용 횟수 초기화
    public void ResetUsageCount()
    {
        scrollCurrentUsageCount = 0;
    }

    // 사용 횟수 진행률 (UI용)
    public float GetUsageProgress()
    {
        return (float)scrollCurrentUsageCount / scrollMaxUsageCount;
    }

    #endregion

    #region 레벨 시스템

    // 레벨별 데미지 계산
    public int GetLevelAdjustedDamage()
    {
        float levelMultiplier = 1f + (scrollLevel - 1) * 0.3f; // 레벨당 30% 증가
        return Mathf.RoundToInt(baseDamage * levelMultiplier);
    }

    // 레벨별 상태이상 확률 계산
    public float GetLevelAdjustedStatusChance()
    {
        float levelBonus = (scrollLevel - 1) * 0.1f; // 레벨당 10% 증가
        return Mathf.Clamp01(statusChance + levelBonus);
    }

    // 레벨별 마나 코스트 계산 (레벨이 높을수록 효율적)
    public int GetLevelAdjustedManaCost()
    {
        float levelReduction = (scrollLevel - 1) * 0.1f; // 레벨당 10% 감소
        return Mathf.Max(1, Mathf.RoundToInt(scrollManaCost * (1f - levelReduction)));
    }

    // 레벨업 가능 여부
    public bool CanLevelUp()
    {
        return scrollLevel < 3;
    }

    // 레벨업 실행
    public void LevelUp()
    {
        if (CanLevelUp())
        {
            scrollLevel++;
            Debug.Log($"{scrollName}이(가) 레벨 {scrollLevel}로 상승했습니다!");
        }
    }

    #endregion

    #region 데미지 및 효과 계산

    // 실제 데미지 계산 (레벨 + 자속보정 포함)
    //public int CalculateActualDamage(ElementSystem.ElementType playerElement)
    //{
    //    float actualDamage = GetLevelAdjustedDamage();

    //    자속보정 적용
    //    if (hasSelfElementBonus && elementType == playerElement && elementType != ElementSystem.ElementType.None)
    //    {
    //        actualDamage *= (1f + selfElementDamageBonus);
    //    }

    //    return Mathf.RoundToInt(actualDamage);
    //}

    // 상태이상 적용 확률 계산 (레벨 + 자속보정 포함)
    //public float GetStatusEffectChance(ElementSystem.ElementType playerElement)
    //{
    //    float chance = GetLevelAdjustedStatusChance();

    //    자속보정 시 상태이상 확률 증가
    //    if (hasSelfElementBonus && elementType == playerElement && elementType != ElementSystem.ElementType.None)
    //    {
    //        chance += 0.15f; // 15% 추가 확률
    //    }

    //    return Mathf.Clamp01(chance);
    //}

    // 실제 마나 코스트 계산 (레벨 적용)
    public int GetActualManaCost()
    {
        return GetLevelAdjustedManaCost();
    }

    // 자속보정 시 추가 상태이상 적용 가능한지
    //public bool ShouldApplySelfElementStatus(ElementSystem.ElementType playerElement)
    //{
    //    return hasSelfElementBonus &&
    //           elementType == playerElement &&
    //           elementType != ElementSystem.ElementType.None &&
    //           selfElementStatus != StatusEffectType.None &&
    //           UnityEngine.Random.Range(0f, 1f) <= 0.3f; // 30% 확률
    //}

    #endregion

    #region UI 및 정보 표시

    // 희귀도별 색상 가져오기
    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case ScrollRarity.Normal: return Color.black;
            case ScrollRarity.Rare: return new Color(0.3f, 0.5f, 0.9f, 1f);          // 진한 파란색
            case ScrollRarity.Epic: return new Color(0.7f, 0.3f, 0.9f, 1f);          // 밝은 보라색
            case ScrollRarity.Legendary: return new Color(1f, 0.65f, 0.15f, 1f);     // 황금색/주황색
            default: return Color.white;
        }
    }

    // 희귀도별 뽑기 확률 가중치 가져오기
    public float GetRarityWeight()
    {
        switch (rarity)
        {
            case ScrollRarity.Normal: return 50f;      // 50%
            case ScrollRarity.Rare: return 15f;        // 15%
            case ScrollRarity.Epic: return 4f;         // 4%
            case ScrollRarity.Legendary: return 1f;    // 1%
            default: return 50f;
        }
    }
    
    /// <summary>
    /// 스크롤 카테고리를 한글 텍스트로 변환
    /// </summary>
    public string GetCategoryText()
    {
        switch (category)
        {
            case ScrollCategory.Attack: return "공격";
            case ScrollCategory.Defense: return "방어";
            case ScrollCategory.Support: return "지원";
            case ScrollCategory.Debuff: return "디버프";
            case ScrollCategory.Recall: return "소환";
            default: return "기타";
        }
    }

    #endregion

    #region UI 리소스 자동 선택 (ScrollUIDatabase 연동)

    /// <summary>
    /// 현재 등급에 맞는 카드 배경 가져오기
    /// </summary>
    public Sprite GetCardBackground()
    {
        if (scrollUIDatabase != null)
            return scrollUIDatabase.GetCardBackground(rarity);
        return null;
    }

    /// <summary>
    /// 현재 원소에 맞는 젬 가져오기
    /// </summary>
    public Sprite GetElementGem()
    {
        if (scrollUIDatabase != null)
            return scrollUIDatabase.GetElementGem(elementType);
        return null;
    }

    /// <summary>
    /// 01번 테두리 가져오기 (가운데 원 - 등급별 테두리)
    /// </summary>
    public Sprite GetFrame01()
    {
        if (scrollUIDatabase != null)
            return scrollUIDatabase.GetFrame01(rarity);
        return null;
    }

    /// <summary>
    /// 02번 테두리 가져오기 (맨 위 작은 원 - 등급별 테두리)
    /// </summary>
    public Sprite GetFrame02()
    {
        if (scrollUIDatabase != null)
            return scrollUIDatabase.GetFrame02(rarity);
        return null;
    }

    /// <summary>
    /// 현재 스크롤 타입에 맞는 아이콘 가져오기
    /// </summary>
    public Sprite GetScrollTypeIcon()
    {
        if (scrollUIDatabase != null)
            return scrollUIDatabase.GetScrollTypeImage(scrollType);
        return null;
    }

    #endregion

    #region ===== 🧪 에디터 테스트 (Inspector 테스트용) =====

    /// <summary>
    /// Inspector에서 우클릭으로 UI 이미지 자동 선택 테스트
    /// </summary>
    [ContextMenu("🧪 UI 이미지 자동 선택 테스트")]
    public void TestUIImageSelection()
    {
        Debug.Log($"=== {scrollName} UI 이미지 테스트 시작 ===");
        Debug.Log($"📊 등급: {rarity}, 원소: {elementType}");
        
        // 카드 배경 테스트
        var cardBg = GetCardBackground();
        Debug.Log($"📱 카드 배경: {(cardBg ? cardBg.name : "❌ 없음")}");
        
        // 원소 젬 테스트  
        var gem = GetElementGem();
        Debug.Log($"💎 원소 젬: {(gem ? gem.name : "❌ 없음")}");
        
        // 테두리들 테스트
        var frame01 = GetFrame01();
        var frame02 = GetFrame02();
        Debug.Log($"🖼️ 01번 테두리: {(frame01 ? frame01.name : "❌ 없음")}");
        Debug.Log($"🖼️ 02번 테두리: {(frame02 ? frame02.name : "❌ 없음")}");
        
        // ScrollUIDatabase 연결 상태 확인
        Debug.Log($"🔗 Database 연결: {(scrollUIDatabase ? $"✅ {scrollUIDatabase.name}" : "❌ 연결 실패")}");
        
        Debug.Log($"=== {scrollName} 테스트 완료 ===");
    }

    /// <summary>
    /// Inspector에서 ScrollUIDatabase 강제 새로고침
    /// </summary>
    [ContextMenu("🔄 ScrollUIDatabase 새로고침")]
    public void RefreshUIDatabase()
    {
        _cachedUIDatabase = null; // 캐시 초기화
        var db = scrollUIDatabase; // 다시 찾기
        Debug.Log($"🔄 ScrollUIDatabase 새로고침: {(db ? $"✅ {db.name}" : "❌ 찾을 수 없음")}");
    }

    #endregion


}
