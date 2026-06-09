using Core.EventBus;
using Public.Core.Events;
using System;
using UnityEngine;

[Serializable]
public class OnHitDealtModuleTriggerDef : ModuleTriggerDef
{
    [Range(0f, 1f)]
    public float chance = 1f;

    public bool onlyOnCritical = false;

    public override IModuleLogic CreateLogic()
    {
        return new OnHitDealtModuleTrigger(this);
    }
}

public class OnHitDealtModuleTrigger : ModuleTriggerBase<OnHitDealtModuleTriggerDef>, IHitModifier
{
    public OnHitDealtModuleTrigger(OnHitDealtModuleTriggerDef def) : base(def)
    {
    }

    public override void OnEquip(Entity owner)
    {
        base.OnEquip(owner);
    }

    public void OnHit(Entity target, DamageData data)
    {
        if (_def.onlyOnCritical && !data.isCritical)
            return;

        if (UnityEngine.Random.value > _def.chance)
            return;

        if (target == null || target.IsDead)
            return;

        ExecuteAll(EffectContext.OnHit(_owner, target, data));
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
    }
}
