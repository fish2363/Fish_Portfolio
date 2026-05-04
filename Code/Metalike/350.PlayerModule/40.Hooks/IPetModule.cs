public interface IPetModule : IModuleHook
{
    bool CanAttack();
    bool TryAttack(Entity target);
    bool IsIndependent { get; }
}