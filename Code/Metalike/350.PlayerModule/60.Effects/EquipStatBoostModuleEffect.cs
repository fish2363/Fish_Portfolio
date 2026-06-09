using System;
using System.Collections.Generic;
using UnityEngine;

public enum StatBoostDurationMode
{
    UntilUnequip,
    Timed
}

[Serializable]
[ModuleDisplayName("스탯 증가", "트리거 발동 시 특정 스탯을 비율로 증가시킵니다.")]
public class StatBoostEffectDef : IModuleEffectDef
{
    [Range(-1f, 1f)]
    public float upgradePercent = 0.2f;

    public StatSO statSO;

    public StatBoostDurationMode durationMode = StatBoostDurationMode.UntilUnequip;

    [Min(0.1f),Header("Mode가 Timed일때만 발동")]
    public float duration = 3f;

    public bool canStack = false;

    public IModuleEffect CreateEffect()
    {
        return new StatBoostEffect(this);
    }
}

public class StatBoostEffect : IExecutableEffect, IUpdateModuleLogic
{
    private struct ActiveBoost
    {
        public object ModifierKey;
        public float RemainingTime;

        public ActiveBoost(object modifierKey, float remainingTime)
        {
            ModifierKey = modifierKey;
            RemainingTime = remainingTime;
        }
    }

    private readonly StatBoostEffectDef _def;
    private readonly List<ActiveBoost> _activeBoosts = new();

    private EntityStatCompo _statCompo;

    public StatBoostEffect(StatBoostEffectDef def)
    {
        _def = def;
    }

    public void OnInitialize(Entity owner)
    {
        _statCompo = owner.GetCompo<EntityStatCompo>();
    }

    public void OnUnequip()
    {
        RemoveAllBoosts();
        _statCompo = null;
    }

    public void Execute(EffectContext ctx)
    {
        if (_statCompo == null || _def.statSO == null)
            return;

        if (!_def.canStack && _activeBoosts.Count > 0)
        {
            RefreshFirstBoost();
            return;
        }

        ApplyNewBoost();
    }

    public void ModuleUpdate(float deltaTime)
    {
        if (_def.durationMode != StatBoostDurationMode.Timed)
            return;

        for (int i = _activeBoosts.Count - 1; i >= 0; i--)
        {
            ActiveBoost boost = _activeBoosts[i];
            boost.RemainingTime -= deltaTime;

            if (boost.RemainingTime <= 0f)
            {
                RemoveBoostAt(i);
                continue;
            }

            _activeBoosts[i] = boost;
        }
    }

    private void ApplyNewBoost()
    {
        var stat = _statCompo.GetStat(_def.statSO);
        if (stat == null)
            return;

        object modifierKey = _def.canStack ? new object() : this;
        float delta = stat.Value * _def.upgradePercent;

        _statCompo.AddModifier(_def.statSO, modifierKey, delta);

        float remainingTime = _def.durationMode == StatBoostDurationMode.Timed
            ? Mathf.Max(0.1f, _def.duration)
            : 0f;

        _activeBoosts.Add(new ActiveBoost(modifierKey, remainingTime));
    }

    private void RefreshFirstBoost()
    {
        if (_def.durationMode != StatBoostDurationMode.Timed)
            return;

        ActiveBoost boost = _activeBoosts[0];
        boost.RemainingTime = Mathf.Max(0.1f, _def.duration);
        _activeBoosts[0] = boost;
    }

    private void RemoveBoostAt(int index)
    {
        if (_statCompo != null && _def.statSO != null)
            _statCompo.RemoveModifier(_def.statSO, _activeBoosts[index].ModifierKey);

        _activeBoosts.RemoveAt(index);
    }

    private void RemoveAllBoosts()
    {
        for (int i = _activeBoosts.Count - 1; i >= 0; i--)
            RemoveBoostAt(i);

        _activeBoosts.Clear();
    }
}
