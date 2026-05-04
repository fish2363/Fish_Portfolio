using System;
using UnityEngine;

[Serializable]
public class OnSkillCastModuleTriggerDef : ModuleTriggerDef
{
    public float cooldownTime = 0f;

    public override IModuleLogic CreateLogic()
    {
        return new OnSkillCastModuleTrigger(this);
    }
}

public class OnSkillCastModuleTrigger :
    ModuleTriggerBase<OnSkillCastModuleTriggerDef>,
    ISkillCastModifier,
    IUpdateModuleLogic
{
    private float _cooldownTimer;

    public OnSkillCastModuleTrigger(OnSkillCastModuleTriggerDef def) : base(def)
    {
    }

    public void ModuleUpdate(float deltaTime)
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= deltaTime;
    }

    public void OnSkillCast()
    {
        if (_cooldownTimer > 0f)
            return;

        ExecuteAll(EffectContext.OnSkillCast(_owner));

        if (_def.cooldownTime > 0f)
            _cooldownTimer = _def.cooldownTime;
    }
}
