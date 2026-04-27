using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 전투 시스템 - 데미지 계산과 전투 로직만 담당 (정적 유틸리티 클래스)
/// </summary>
public static class CombatSystem
{
    #region 데미지 계산 상수
    // 크리티컬 데미지 시간 기준
    public static float criticalTime20Percent = 0.2f;  // 20% 이내 시간
    public static float criticalTime50Percent = 0.5f;  // 50% 이내 시간
    public static float criticalTime80Percent = 0.8f;  // 80% 이내 시간

    public static float criticalDamage20Percent = 2.0f; // 200% 데미지
    public static float criticalDamage50Percent = 1.5f; // 150% 데미지
    public static float criticalDamage80Percent = 1.2f; // 120% 데미지
    #endregion

    #region 메인 데미지 계산

    // 계산식: (((((기본 데미지 + 스킬 계수*공격력) * 자속성 계수) * 몬스터 속성 상성 계수) * 재능에서 부여된 % 추가량) * 아티팩트에서 부여된 % 추가량) * 패턴 정확도 계수) = 최종 데미지
    // 최종 데미지 - 방어력 = 몬스터가 받는 데미지
    public static float CalculateFinalDamage(ScrollSO scroll, Entity attacker, PatternResult patternResult)
    {
        // FinalDamageResult result = new FinalDamageResult();
    
        // 1. 기본 데미지 + 스킬 계수 * 공격력
        float baseDamage = scroll.GetLevelAdjustedDamage();
        bool isPlayer = attacker is Player;

        float attack = isPlayer ? ((Player)attacker).GetModifiedAttack() : ((Monster)attacker).GetAttackDamage();
        
        
        float skillCoefficient = scroll.GetAttackCoefficient();
        float damage = baseDamage + (skillCoefficient * attack);

        if (isPlayer)
        {
            Player player = (Player)attacker;
            
            // 2. 자속성 보정(스크롤과 플레이어 속성이 같으면 강화)
            if (scroll.elementType == player.GetPlayerElement()) 
                damage *= ElementSystem.Instance.selfElementBonus;

            // 4. 재능에서 부여된 % 추가량
            float talentDamageModifier = GetTalentDamageModifier(scroll, player);
            damage *= talentDamageModifier;

            // 5. 아티팩트에서 부여된 % 추가량
            float artifactDamageModifier = GetArtifactDamageModifier(scroll, player);
            damage *= artifactDamageModifier;
        }

        // 6. 패턴 정확도 계수
        float accuracyModifier = Mathf.Clamp(patternResult.accuracy, 0.1f, 1.0f);
        damage *= accuracyModifier;

        // 7. 크리티컬(완벽 패턴일 때만)
        float criticalMultiplier = 1.0f;
        bool isCritical = false;
        if (patternResult.isPerfect)
        {
            // 크리티컬판정시간증가: totalTime을 줄여서 같은 완료 시간이어도 더 높은 크리티컬 등급
            float adjustedTotalTime = patternResult.totalTime;
            float critTimeBonus = GetTalentCriticalTimeBonus();
            if (critTimeBonus > 0f)
                adjustedTotalTime = Mathf.Max(adjustedTotalTime - critTimeBonus, 0.1f);

            criticalMultiplier = CalculateCriticalMultiplier(patternResult.completionTime, adjustedTotalTime);

            // 크리티컬데미지증가: 크리티컬 배율에 재능 보너스 가산
            if (criticalMultiplier > 1.0f)
                criticalMultiplier += GetTalentCriticalDamageBonus();

            isCritical = criticalMultiplier > 1.0f;
            damage *= criticalMultiplier;
        }

        // result.patternResult = patternResult;
        // result.criticalMultiplier = criticalMultiplier;
        // result.isCritical = isCritical;
        // result.damage = damage;

        // 8. 자동 전투 모드 페널티 적용
        if (InGameManager.Instance != null)
        {
            if (InGameManager.Instance.isFullAutoMode)
            {
                damage *= 0.7f; // 70% 데미지만 적용
                Debug.Log($"[완전 자동] 데미지 페널티 적용: {damage}");
            }
            else if (InGameManager.Instance.isAutoPatternMode)
            {
                damage *= 0.8f; // 80% 데미지만 적용
                Debug.Log($"[패턴 자동] 데미지 페널티 적용: {damage}");
            }
        }

        return damage;
    }

    // 몬스터 공격 데미지 계산 (통합)
    public static int CalculateMonsterAttackDamage(Monster attacker, Entity target)
    {
        float damage = attacker.GetAttackDamage();
        
        // 보스 분노 상태 적용
        if (attacker is BossMonster boss && boss.IsEnraged())
        {
            damage *= boss.GetEnrageDamageMultiplier();
        }
        
        // 방어력 적용
        int defense = target.GetModifiedDefense();
        return Mathf.Max(1, Mathf.RoundToInt(damage) - defense);
    }

    #endregion

    #region 크리티컬 데미지 계산

    // 패턴 완료 시간에 따른 크리티컬 데미지 배율 계산
    private static float CalculateCriticalMultiplier(float completionTime, float totalTime)
    {
        float timeRatio = completionTime / totalTime;

        if (timeRatio <= criticalTime20Percent)
        {
            return criticalDamage20Percent; // 200% 데미지
        }
        else if (timeRatio <= criticalTime50Percent)
        {
            return criticalDamage50Percent; // 150% 데미지
        }
        else if (timeRatio <= criticalTime80Percent)
        {
            return criticalDamage80Percent; // 120% 데미지
        }

        return 1.0f; // 기본 데미지
    }

    #endregion

    #region 방어력 계산

    // 방어력 적용된 데미지 계산
    private static float ApplyDefense(float rawDamage, int defense)
    {
        // 방어력 공식: 데미지 * (100 / (100 + 방어력))
        float damageReduction = 100f / (100f + defense);
        return rawDamage * damageReduction;
    }

    #endregion

    #region 다중 공격 처리

    // 다중 공격 데미지 분배 계산
    public static List<int> CalculateMultiAttackDamage(float totalDamage, int targetCount)
    {
        List<int> damages = new List<int>();

        // 각 대상별로 약간의 랜덤 변동 적용 (90%~110%)
        for (int i = 0; i < targetCount; i++)
        {
            float variation = Random.Range(0.9f, 1.1f);
            int damage = Mathf.RoundToInt(totalDamage * variation);
            damages.Add(damage);
        }

        return damages;
    }

    // 총 데미지 계산
    private static int GetTotalDamage(List<int> damageList)
    {
        int total = 0;
        foreach (int damage in damageList)
        {
            total += damage;
        }
        return total;
    }

    #endregion

    #region 재능/아티팩트 데미지 보정

    // 재능(Relic) 데미지 보정 계산
    private static float GetTalentDamageModifier(ScrollSO scroll, Player player)
    {
        if (RelicManager.Instance == null) return 1.0f;

        float modifier = 1.0f;

        // 재능에서 데미지 보정값 가져오기
        foreach (var relicState in RelicManager.Instance.ownedRelics)
        {
            if (relicState.level <= 0) continue; // 잠긴 재능은 무시

            foreach (var effect in relicState.currentEffects)
            {
                switch ((EffectType)effect.effectType)
                {
                    // EffectType.공격력증가는 Player.ApplyTalentStats()에서 이미 baseAttack에 고정값으로 적용됨
                    // 여기서 % 보정으로 중복 적용하지 않음
                    case EffectType.데미지증가:
                        modifier += effect.currentValue / 100f;
                        break;
                    case EffectType.크리티컬데미지증가:
                        // 크리티컬 보너스는 GetTalentCriticalDamageBonus()에서 별도 처리
                        break;
                    case EffectType.전체원소데미지:
                        modifier += effect.currentValue / 100f;
                        break;
                    // case EffectType.불원소데미지:
                    //     if (scroll.elementType == ElementSystem.ElementType.Fire)
                    //         modifier += effect.currentValue / 100f;
                    //     break;
                    // case EffectType.물원소데미지:
                    //     if (scroll.elementType == ElementSystem.ElementType.Water)
                    //         modifier += effect.currentValue / 100f;
                    //     break;
                    // case EffectType.바람원소데미지:
                    //     if (scroll.elementType == ElementSystem.ElementType.Wind)
                    //         modifier += effect.currentValue / 100f;
                    //     break;
                    // case EffectType.땅원소데미지:
                    //     if (scroll.elementType == ElementSystem.ElementType.Earth)
                    //         modifier += effect.currentValue / 100f;
                    //     break;
                    // case EffectType.번개원소데미지:
                    //     if (scroll.elementType == ElementSystem.ElementType.Lightning)
                    //         modifier += effect.currentValue / 100f;
                    //     break;
                    // case EffectType.풀원소데미지:
                    //     if (scroll.elementType == ElementSystem.ElementType.Nature)
                    //         modifier += effect.currentValue / 100f;
                    //     break;
                    // case EffectType.어둠원소데미지:
                    //     if (scroll.elementType == ElementSystem.ElementType.Dark)
                    //         modifier += effect.currentValue / 100f;
                    //     break;
                }
            }
        }

        return modifier;
    }

    // 재능 크리티컬 데미지 보너스 (크리티컬 배율에 가산)
    private static float GetTalentCriticalDamageBonus()
    {
        if (RelicManager.Instance == null) return 0f;
        return RelicManager.Instance.GetTotalValue(EffectType.크리티컬데미지증가) / 100f;
    }

    // 재능 크리티컬 판정 시간 보너스 (totalTime에서 차감하여 판정 구간 확대)
    private static float GetTalentCriticalTimeBonus()
    {
        if (RelicManager.Instance == null) return 0f;
        return RelicManager.Instance.GetTotalValue(EffectType.크리티컬판정시간증가);
    }

    // 아티팩트 데미지 보정 계산
    private static float GetArtifactDamageModifier(ScrollSO scroll, Player player)
    {
        if (ArtifactManager.Instance == null) return 1.0f;

        float modifier = 1.0f;

        // 아티팩트에서 데미지 보정값 가져오기
        foreach (var artifact in ArtifactManager.Instance.GetPlayerArtifacts())
        {
            // 주 효과 타입에 따른 데미지 보정
            switch (artifact.EffectType)
            {
                case ArtifactEffectType.HP:
                    // HP는 데미지 계산에 직접 영향 없음
                    break;
                case ArtifactEffectType.Damage:
                    modifier += artifact.Value1 / 100f;
                    break;
                case ArtifactEffectType.Offense:
                    modifier += artifact.Value1 / 100f;
                    break;
                case ArtifactEffectType.Defense:
                    // 일반 방어력은 데미지 계산에 직접 영향 없음 (방어 계산에서 처리)
                    // "기생의 씨앗" 같은 원소별 아티팩트는 별도 처리
                    break;
                case ArtifactEffectType.ResourceGain:
                    // 자원 획득은 데미지 계산에 직접 영향 없음
                    break;
                case ArtifactEffectType.StatusInflict:
                    // 상태이상 부여는 데미지 계산에 직접 영향 없음
                    break;
                case ArtifactEffectType.StatusCleanse:
                    // 상태이상 제거는 데미지 계산에 직접 영향 없음
                    break;
                case ArtifactEffectType.CardUpgrade:
                    // 카드 강화는 데미지 계산에 직접 영향 없음
                    break;
                case ArtifactEffectType.Economy:
                    // 골드/상점은 데미지 계산에 직접 영향 없음
                    break;
                case ArtifactEffectType.Scroll:
                    // 스크롤 관련은 데미지 계산에 직접 영향 없음
                    break;
            }

            // 부 효과 타입에 따른 데미지 보정
            switch (artifact.EffectType2)
            {
                case ArtifactEffectType2.None:
                    break;
                case ArtifactEffectType2.HP:
                    // HP는 데미지 계산에 직접 영향 없음
                    break;
                case ArtifactEffectType2.Damage:
                    modifier += artifact.Value2 / 100f;
                    break;
                case ArtifactEffectType2.Offense:
                    modifier += artifact.Value2 / 100f;
                    break;
                case ArtifactEffectType2.Defense:
                    // 방어력은 데미지 계산에 직접 영향 없음 (방어 계산에서 처리)
                    break;
                case ArtifactEffectType2.ResourceGain:
                    // 자원 획득은 데미지 계산에 직접 영향 없음
                    break;
                case ArtifactEffectType2.StatusInflict:
                    // 상태이상 부여는 데미지 계산에 직접 영향 없음
                    break;
                case ArtifactEffectType2.StatusCleanse:
                    // 상태이상 제거는 데미지 계산에 직접 영향 없음
                    break;
                case ArtifactEffectType2.CardUpgrade:
                    // 카드 강화는 데미지 계산에 직접 영향 없음
                    break;
                case ArtifactEffectType2.Economy:
                    // 골드/상점은 데미지 계산에 직접 영향 없음
                    break;
                case ArtifactEffectType2.Scroll:
                    // 스크롤 관련은 데미지 계산에 직접 영향 없음
                    break;
            }
        }

        return modifier;
    }

    // 원소별 아티팩트 고정 데미지 보너스 계산
    private static int GetElementalArtifactBonus(ScrollSO scroll, Player player)
    {
        if (ArtifactManager.Instance == null || scroll == null) return 0;

        int bonus = 0;

        foreach (var artifact in ArtifactManager.Instance.GetPlayerArtifacts())
        {
            // "기생의 씨앗": Nature 원소 스크롤에 고정 데미지 +5
            if (artifact.ArtifactName == "기생의 씨앗" &&
                scroll.elementType == ElementSystem.ElementType.Nature)
            {
                bonus += Mathf.RoundToInt(artifact.Value1); // +5 고정 데미지
                Debug.Log($"[기생의 씨앗] Nature 원소 스크롤 고정 데미지 +{artifact.Value1}");
            }

            // 향후 다른 원소별 아티팩트들 추가 가능
            // 예: "고목반지" - Nature 원소 데미지 +15% (% 보정은 위의 GetArtifactDamageModifier에서)
        }

        return bonus;
    }

    // // 모든 타겟 데미지 계산
    // public static Dictionary<Monster, int> CalculateAllTargetDamage(ScrollSO scroll, Player player, Monster[] targets, PatternResult patternResult)
    // {
    //     var results = new Dictionary<Monster, int>();
    //
    //     foreach (Monster target in targets)
    //     {
    //         if (target != null && target.IsAlive())
    //         {
    //             FinalDamageResult damageResult = CalculateFinalDamage(scroll, player, target, patternResult);
    //             results[target] = damageResult.totalDamage;
    //         }
    //     }
    //
    //     return results;
    // }


    #endregion

    #region 전투 결과 검증

    // 전투 종료 조건 체크
    public static bool CheckCombatEnd(Player player, Monster monster)
    {
        return player.GetCurrentHP() <= 0 || monster.GetCurrentHP() <= 0;
    }

    // 승리/패배 판정
    public static CombatResult GetCombatResult(Player player, Monster monster)
    {
        if (monster.GetCurrentHP() <= 0)
        {
            return CombatResult.Victory;
        }
        else if (player.GetCurrentHP() <= 0)
        {
            return CombatResult.Defeat;
        }
        else
        {
            return CombatResult.Ongoing;
        }
    }

    #endregion

    #region 유틸리티 메소드

    // 데미지 로그 출력
    // public static void LogDamageResult(FinalDamageResult result, string attackerName, string targetName)
    // {
    //     string log = $"{attackerName} → {targetName}: ";
    //     log += $"총 데미지 {result.totalDamage}";
    //
    //     if (result.isCritical) log += " (크리티컬!)";
    //     if (result.elementResult.isSelfElementBonus) log += " (자속보정!)";
    //     if (result.isMultiAttack) log += $" (다중공격 {result.damagePerTarget.Count}대상)";
    //
    //     // 원소 상호작용 표시
    //     switch (result.elementResult.interactionType)
    //     {
    //         case ElementSystem.InteractionType.Strong:
    //             log += " (약점!)";
    //             break;
    //         case ElementSystem.InteractionType.Weak:
    //             log += " (저항)";
    //             break;
    //     }
    //
    //     Debug.Log(log);
    // }

    #endregion
}

#region 전투 결과 데이터 클래스

// 패턴 입력 결과 (다른 파트에서 제공받을 데이터)
[System.Serializable]
public class PatternResult
{
    public float accuracy;          // 정확도 (0~1)
    public bool isPerfect;          // 완벽한 입력 여부
    public float completionTime;    // 완료 시간
    public float totalTime;         // 전체 제한 시간
}

// 최종 데미지 계산 결과
[System.Serializable]
public class FinalDamageResult
{
    public PatternResult patternResult;         // 패턴 입력 결과
    public ElementInteractionResult elementResult; // 원소 시스템 계산 결과
    public float criticalMultiplier;            // 크리티컬 데미지 배율
    public bool isCritical;                     // 크리티컬 발생 여부
    public float damage;           // 몬스터 상성/방어력 적용 전 데미지(부동소수)
}


// 전투 결과 열거형
public enum CombatResult
{
    Ongoing,    // 진행 중
    Victory,    // 승리
    Defeat      // 패배
}

#endregion