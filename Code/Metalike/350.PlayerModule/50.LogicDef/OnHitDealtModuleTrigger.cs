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

public class OnHitDealtModuleTrigger : ModuleTriggerBase<OnHitDealtModuleTriggerDef>
{
    public OnHitDealtModuleTrigger(OnHitDealtModuleTriggerDef def) : base(def)
    {
    }

    public override void OnEquip(Entity owner)
    {
        base.OnEquip(owner);
        Bus<HitDealtEvent>.OnEvent += HandleSuccess;
    }

    public override void OnUnequip()
    {
        Bus<HitDealtEvent>.OnEvent -= HandleSuccess;
        base.OnUnequip();
    }

    private void HandleSuccess(HitDealtEvent evt)
    {
        if (evt.owner != _owner)
            return;

        if (_def.onlyOnCritical && !evt.isCritical)
            return;

        if (UnityEngine.Random.value > _def.chance)
            return;

        if (evt.target == null || evt.target.IsDead)
            return;

        ExecuteAll(EffectContext.OnHit(_owner, evt.target, evt.isCritical));
    }
}
