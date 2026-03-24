public interface IModuleLogic
{
    void OnEquip(Entity owner);
    void ModuleUpdate(float deltaTime);
    void OnUnequip();
}

public interface IModuleLogicDef
{
    IModuleLogic CreateLogic();
}