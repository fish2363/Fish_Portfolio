using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// 원소 상호작용 시스템 - 원소 간의 관계와 상태이상 정보만 담당
/// </summary>
public class ElementSystem : SingletonManager<ElementSystem>
{
    // 9가지 원소 정의 (불, 물, 바람, 땅, 번개, 얼음, 풀, 빛, 어둠)
    public enum ElementType
    {
        Water,      // 물 0
        Fire,       // 불 1
        Nature,     // 풀 2
        Wind,       // 바람 3
        Earth,      // 땅 4
        Lightning,  // 전기 5
        Light,      // 빛 6
        Dark,       // 어둠 7
        None        // 무속성
    }

    // 원소 상호작용 결과 타입
    public enum InteractionType
    {
        Normal,     // 보통 효과 (0.9x)
        Weak,       // 저항 효과 (0.77x)
        Strong,     // 약점 효과 (1.3x)
        Special     // 특수 효과 (상태이상 발생)
    }

    // 원소 상호작용 결과 데이터
    [System.Serializable]
    public class ElementInteraction
    {
        public ElementType attackElement;   // 공격 원소
        public ElementType defenseElement;  // 방어 원소
        public InteractionType result;      // 상호작용 결과
        public float damageMultiplier;      // 데미지 배율
        public string statusEffectName;     // 발생할 상태이상 이름
        public float statusChance = 1.0f;   // 상태이상 발생 확률
    }

    // 자속보정 보너스 설정
    [Header("자속보정 설정")]
    public float selfElementBonus = 1.2f;  // 자속보정 시 데미지 보너스

    // 데미지 배율 상수들
    public const float NeutralDamageMultiplier = 0.9f;      // 보통 효과
    public const float ResistDamageMultiplier = 0.77f;      // 저항 효과
    public const float WeaknessDamageMultiplier = 1.3f;     // 약점 효과

    private Dictionary<string, ElementInteraction> interactionMap;

    async void Start()
    {
        await InitializeInteractionMapAsync(); // 원소 상호작용 맵 초기화
    }

    // 원소 상호작용 맵 초기화 (수정: 조합 상호작용 제거, 기본 약점/저항만)
    private async UniTask InitializeInteractionMapAsync()
    {
        interactionMap = new Dictionary<string, ElementInteraction>();

        // 기본적인 약점 관계만 설정 (예시)
        // 실제 약점/저항 관계는 기획에 따라 조정 가능
        AddInteraction(ElementType.Water, ElementType.Fire, InteractionType.Strong, WeaknessDamageMultiplier, "");
        AddInteraction(ElementType.Lightning, ElementType.Water, InteractionType.Strong, WeaknessDamageMultiplier, "");
        AddInteraction(ElementType.Earth, ElementType.Lightning, InteractionType.Strong, WeaknessDamageMultiplier, "");
        AddInteraction(ElementType.Wind, ElementType.Earth, InteractionType.Strong, WeaknessDamageMultiplier, "");
        AddInteraction(ElementType.Nature, ElementType.Water, InteractionType.Strong, WeaknessDamageMultiplier, "");
        AddInteraction(ElementType.Light, ElementType.Dark, InteractionType.Strong, WeaknessDamageMultiplier, "");
        AddInteraction(ElementType.Dark, ElementType.Light, InteractionType.Strong, WeaknessDamageMultiplier, "");

        // 저항 관계 (역방향)
        AddInteraction(ElementType.Fire, ElementType.Water, InteractionType.Weak, ResistDamageMultiplier, "");
        AddInteraction(ElementType.Water, ElementType.Lightning, InteractionType.Weak, ResistDamageMultiplier, "");
        AddInteraction(ElementType.Lightning, ElementType.Earth, InteractionType.Weak, ResistDamageMultiplier, "");
        AddInteraction(ElementType.Earth, ElementType.Wind, InteractionType.Weak, ResistDamageMultiplier, "");
        AddInteraction(ElementType.Water, ElementType.Nature, InteractionType.Weak, ResistDamageMultiplier, "");
        AddInteraction(ElementType.Dark, ElementType.Light, InteractionType.Weak, ResistDamageMultiplier, "");
        AddInteraction(ElementType.Light, ElementType.Dark, InteractionType.Weak, ResistDamageMultiplier, "");

        // 비동기 초기화 완료 대기
        await UniTask.Yield();
    }

    // 원소별 기본 상태이상 반환
    public string GetElementStatusEffect(ElementType elementType)
    {
        switch (elementType)
        {
            case ElementType.Water:
                return "젖음";        // 피해량 25% 증가
            case ElementType.Fire:
                return "화상";        // 매 턴 최대체력 2% DoT
            case ElementType.Lightning:
                return "마비";        // 받는 피해 증가
            case ElementType.Nature:
                return "속박";        // 1턴 행동 불가
            case ElementType.Wind:
                return "어지러움";     // 공격 명중률 감소
            case ElementType.Light:
                return "실명";        // 명중률 대폭 감소
            case ElementType.Dark:
                return "혼란";        // 매 턴 정신 피해
            case ElementType.Earth:
                return "기절";        // 1턴 행동 불가
            default:
                return "";
        }
    }

    // 상태이상 적용 확률 계산
    public float CalculateStatusEffectChance(ElementType scrollElement, ElementType playerElement, ElementType monsterElement)
    {
        // 우선순위 1: 스크롤 속성이 몬스터 속성과 일치할 때 0%
        if (scrollElement == monsterElement)
        {
            return 0.0f;
        }

        // 우선순위 2: 스크롤 속성이 캐릭터 속성과 일치할 때 100%
        if (scrollElement == playerElement)
        {
            return 1.0f;
        }

        // 우선순위 3: 스크롤 속성이 몬스터 속성과 다를 때 50%
        return 0.5f;
    }

    // 원소 상호작용 추가 메소드
    private void AddInteraction(ElementType attack, ElementType defense, InteractionType type,
                               float multiplier, string statusEffect)
    {
        string key = GetInteractionKey(attack, defense);
        ElementInteraction interaction = new ElementInteraction
        {
            attackElement = attack,
            defenseElement = defense,
            result = type,
            damageMultiplier = multiplier,
            statusEffectName = statusEffect,
        };
        interactionMap[key] = interaction;
    }

    // 원소 상호작용 키 생성
    private string GetInteractionKey(ElementType attack, ElementType defense)
    {
        return $"{attack}_{defense}";
    }

    // 원소 상호작용 정보 가져오기
    public ElementInteraction GetElementInteraction(ElementType attackElement, ElementType defenseElement)
    {
        string key = GetInteractionKey(attackElement, defenseElement);
        return interactionMap.ContainsKey(key) ? interactionMap[key] : null;
    }

    // 자속보정 적용 여부 확인
    public bool IsSelfElementBonus(ElementType attackElement, ElementType playerElement)
    {
        return attackElement == playerElement && attackElement != ElementType.None;
    }

    // 원소 상호작용 결과만 반환 (데미지 계산은 제거)
    public ElementInteractionResult GetInteractionResult(ElementType attackElement, ElementType defenseElement, ElementType playerElement)
    {
        ElementInteractionResult result = new ElementInteractionResult();
        
        // 자속보정 적용 여부
        result.isSelfElementBonus = IsSelfElementBonus(attackElement, playerElement);
        
        // 원소 상호작용 적용
        ElementInteraction interaction = GetElementInteraction(attackElement, defenseElement);
        if (interaction != null)
        {
            result.interaction = interaction;
            result.interactionType = interaction.result;
            result.damageMultiplier = interaction.damageMultiplier;
            
            // 상태이상 발생 확률 = 100%
            if (!string.IsNullOrEmpty(interaction.statusEffectName))
            {
                if (Random.Range(0f, 1f) <= interaction.statusChance)
                {
                    result.statusEffectName = interaction.statusEffectName;
                    result.shouldApplyStatus = true;
                }
            }
        }
        else
        {
            // 기본 상호작용 (같은 원소끼리는 보통 효과)
            result.damageMultiplier = NeutralDamageMultiplier;
            result.interactionType = InteractionType.Normal;
        }

        return result;
    }
}

// 원소 상호작용 결과 클래스 (데미지 계산 결과에서 분리)
[System.Serializable]
public class ElementInteractionResult
{
    public ElementSystem.ElementType attackElement;   // 공격 원소
    public ElementSystem.ElementType defenseElement;  // 방어 원소
    public ElementSystem.InteractionType interactionType; // 상호작용 타입
    public ElementSystem.ElementInteraction interaction;  // 상호작용 상세 정보
    public bool isSelfElementBonus;     // 자속보정 적용 여부
    public bool shouldApplyStatus;      // 상태이상 적용 여부
    public string statusEffectName;     // 적용할 상태이상 이름
    public float damageMultiplier;      // 데미지 배율
}