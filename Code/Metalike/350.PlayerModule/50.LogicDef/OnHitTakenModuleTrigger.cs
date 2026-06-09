using System;
using UnityEngine;

[Serializable]
public class OnHitTakenModuleTriggerDef : ModuleTriggerDef
{
    [Range(0f, 1f)]
    public float chance = 1f;

    public bool onlyOnCritical = false;

    public override IModuleLogic CreateLogic()
    {
        return new OnHitTakenModuleTrigger(this);
    }
}

public class OnHitTakenModuleTrigger :
    ModuleTriggerBase<OnHitTakenModuleTriggerDef>,
    IBeforeDamageModifier
{
    public OnHitTakenModuleTrigger(OnHitTakenModuleTriggerDef def) : base(def)
    {
    }

    public void OnBeforeDamage(ref DamageData data, Entity dealer)
    {
        if (_def.onlyOnCritical && !data.isCritical)
            return;

        if (UnityEngine.Random.value > _def.chance)
            return;

        ExecuteAll(
            EffectContext.OnPlayerHit(
                _owner,
                dealer,
                data
            )
        );
    }
}
