public interface IBeforeDamageModifier : IModuleHook
{
    void OnBeforeDamage(ref DamageData data, Entity dealer);
}
