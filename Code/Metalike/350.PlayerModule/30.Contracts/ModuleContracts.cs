using System.Collections.Generic;

public interface IModuleLogic
{
    void OnEquip(Entity owner);
    void OnUnequip();
}

public interface IModuleLogicDef
{
    IModuleLogic CreateLogic();
}

public interface IModuleEffect
{
    void OnEquip(Entity owner);
    void OnUnequip();
}

public interface IExecutableEffect : IModuleEffect
{
    void Execute(EffectContext ctx);
}

public interface IModuleEffectDef
{
    IModuleEffect CreateEffect();
}

public interface IModuleEffectContainer
{
    void CollectEffects(List<IModuleEffect> results);
}
