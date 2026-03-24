using Core.EventBus;
using System;
using UnityEngine;

[Serializable]
public class BulletModifierModuleDef: IModuleLogicDef
{
    public string modifierEventName;
    public IModuleLogic CreateLogic() => new BulletModifierModule(this);
}

public class BulletModifierModule : IModuleLogic
{
    private readonly BulletModifierModuleDef _def;
    public BulletModifierModule(BulletModifierModuleDef def) => _def = def;

    public void OnEquip(Entity owner) => Bus<ProjectileModuleEvent>.Raise(new ProjectileModuleEvent(_def.modifierEventName));
    public void ModuleUpdate(float deltaTime) { }
    public void OnUnequip() { }
}