using System;
using UnityEngine;

[Serializable]
[ModuleDisplayName("체력 회복", "발동 시, 플레이어의 체력을 회복시킵니다.")]
public class PlayerHealModuleEffectDef : IModuleEffectDef
{
    [Header("회복")]
    [Min(0f)]
    public float amount = 10f;

    public bool isPercentage = false;

    public IModuleEffect CreateEffect()
    {
        return new PlayerHealModuleEffect(this);
    }
}

public class PlayerHealModuleEffect : IExecutableEffect
{
    private readonly PlayerHealModuleEffectDef _def;
    private PlayerHealthCompo _healthCompo;

    public PlayerHealModuleEffect(PlayerHealModuleEffectDef def)
    {
        _def = def;
    }

    public void OnInitialize(Entity owner)
    {
        _healthCompo = owner.GetCompo<PlayerHealthCompo>();
    }

    public void OnUnequip()
    {
        _healthCompo = null;
    }

    public void Execute(EffectContext ctx)
    {
        if (_healthCompo == null)
            return;

        _healthCompo.ApplyHeal(new InstantHealData
        {
            amount = _def.amount,
            isPercentage = _def.isPercentage
        });
    }
}