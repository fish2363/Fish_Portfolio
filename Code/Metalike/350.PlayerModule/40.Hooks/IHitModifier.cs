public interface IHitModifier : IModuleHook
{
    void OnHit(Entity target, DamageData data);
}