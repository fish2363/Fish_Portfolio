public interface IBeforeDamageModifier
{
    void OnBeforeDamage(ref DamageData data, Entity dealer);
}
public interface ISkillCastModifier
{
    void OnSkillCast();
}

public interface IHitModifier
{
    void OnHit(Entity target, DamageData data);
}
