using System;
using UnityEngine;

[Serializable]
public class OnTimerModuleTriggerDef : ModuleTriggerDef
{
    public float interval = 1f;

    public override IModuleLogic CreateLogic()
    {
        return new OnTimerModuleTrigger(this);
    }
}

public class OnTimerModuleTrigger :
    ModuleTriggerBase<OnTimerModuleTriggerDef>,
    IUpdateModuleLogic
{
    private float _timer;

    public OnTimerModuleTrigger(OnTimerModuleTriggerDef def) : base(def)
    {
    }

    public override void OnEquip(Entity owner)
    {
        base.OnEquip(owner);
        _timer = Mathf.Max(0.01f, _def.interval);
    }

    public void ModuleUpdate(float deltaTime)
    {
        _timer -= deltaTime;

        if (_timer > 0f)
            return;

        _timer = Mathf.Max(0.01f, _def.interval);
        ExecuteAll(EffectContext.OnTimerTick(_owner, deltaTime));
    }
}
