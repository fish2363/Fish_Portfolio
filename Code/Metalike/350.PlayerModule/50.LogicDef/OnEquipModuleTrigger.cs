using System;

[Serializable]
[ModuleDisplayName("장착 시", "모듈을 장착할 때 연결된 효과를 실행합니다.")]
public class OnEquipModuleTriggerDef : ModuleTriggerDef
{
    public override IModuleLogic CreateLogic()
    {
        return new OnEquipModuleTrigger(this);
    }
}

public class OnEquipModuleTrigger : ModuleTriggerBase<OnEquipModuleTriggerDef>
{
    public OnEquipModuleTrigger(OnEquipModuleTriggerDef def) : base(def)
    {
    }

    public override void OnEquip(Entity owner)
    {
        base.OnEquip(owner);
        ExecuteAll(EffectContext.OnEquip(owner));
    }
}