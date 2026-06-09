using System;
using UnityEngine;

[Serializable]
[ModuleDisplayName("저체력 스탯 증폭", "현재 체력이 낮을수록 지정한 스탯이 선형으로 증가합니다.")]
public class LowHealthStatBoostModuleDef : IModuleLogicDef
{
    [Header("증가시킬 스탯")]
    public StatSO targetStat;

    [Header("체력이 0%일 때 최대 증가량(절대값)")]
    public float maxIncreaseValue = 10f;

    [Header("갱신 주기(초)")]
    [Min(0.02f)]
    public float updateInterval = 0.1f;

    public IModuleLogic CreateLogic()
    {
        return new LowHealthStatBoostModule(this);
    }
}

public class LowHealthStatBoostModule : IModuleLogic, IUpdateModuleLogic
{
    private readonly LowHealthStatBoostModuleDef _def;

    private EntityStatCompo _statCompo;
    private PlayerHealthCompo _healthCompo;

    private float _lastAppliedBonus;
    private float _updateTimer;

    public LowHealthStatBoostModule(LowHealthStatBoostModuleDef def)
    {
        _def = def;
    }

    public void OnEquip(Entity owner)
    {
        _statCompo = owner.GetCompo<EntityStatCompo>();
        _healthCompo = owner.GetCompo<PlayerHealthCompo>();
        _lastAppliedBonus = 0f;
        _updateTimer = 0f;

        ApplyBonusImmediate();
    }

    public void OnUnequip()
    {
        RemoveAppliedBonus();
        _healthCompo = null;
        _statCompo = null;
    }

    public void ModuleUpdate(float deltaTime)
    {
        _updateTimer += deltaTime;
        if (_updateTimer < Mathf.Max(0.02f, _def.updateInterval))
            return;

        _updateTimer = 0f;
        ApplyBonusImmediate();
    }

    private void ApplyBonusImmediate()
    {
        if (_statCompo == null || _healthCompo == null || _def.targetStat == null)
            return;

        HealthInfo health = _healthCompo.CurrentHealthInfo;
        if (health == null || health.maxHp <= 0f)
            return;

        float hpRatio = Mathf.Clamp01(health.currentHp / health.maxHp);
        float missingHpRatio = 1f - hpRatio;
        float nextBonus = Mathf.Max(0f, _def.maxIncreaseValue) * missingHpRatio;

        if (Mathf.Approximately(_lastAppliedBonus, nextBonus))
            return;

        _statCompo.RemoveModifier(_def.targetStat, this);
        _statCompo.AddModifier(_def.targetStat, this, nextBonus);
        _lastAppliedBonus = nextBonus;
    }

    private void RemoveAppliedBonus()
    {
        if (_statCompo == null || _def.targetStat == null)
            return;

        _statCompo.RemoveModifier(_def.targetStat, this);
        _lastAppliedBonus = 0f;
    }
}
