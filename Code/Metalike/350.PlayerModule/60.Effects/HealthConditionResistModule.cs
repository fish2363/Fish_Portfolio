using System;
using UnityEngine;

[Serializable]
public class HealthConditionResistModuleDef : IModuleLogicDef
{
    [Header("피격 데미지 감소율")]
    [Range(1f, 100f)]
    public float reducePercent = 50f;

    [Header("이 값 이상일 때만 보호")]
    [Range(0.1f, 1.0f)]
    public float canReduce = 0.8f;

    public IModuleLogic CreateLogic()
    {
        return new HealthConditionResistModule(this);
    }
}

public class HealthConditionResistModule : IModuleLogic, IBeforeDamageModifier
{
    private readonly HealthConditionResistModuleDef _def;

    private PlayerHealthCompo _healthCompo;
    private Player _player;

    public HealthConditionResistModule(HealthConditionResistModuleDef def)
    {
        _def = def;
    }

    public void OnEquip(Entity owner)
    {
        _player = owner as Player;
        _healthCompo = owner.GetCompo<PlayerHealthCompo>();
    }

    public void OnUnequip()
    {
        _player = null;
        _healthCompo = null;
    }

    public void OnBeforeDamage(ref DamageData data, Entity dealer)
    {
        if (_player == null || _healthCompo == null)
            return;

        float currentHp = _healthCompo.CurrentHealthInfo.currentHp;
        float maxHp = _healthCompo.CurrentHealthInfo.maxHp;

        if (maxHp <= 0f)
            return;

        bool canReduceDamage = currentHp >= maxHp * _def.canReduce;
        if (!canReduceDamage)
            return;

        float reduceRate = Mathf.Clamp01(_def.reducePercent / 100f);
        data.damage *= 1f - reduceRate;
    }
}
