public struct EffectContext
{
    public Entity Owner;
    public Entity Target;
    public bool IsCritical;
    public float Damage;
    public float DeltaTime;

    public Projectile Projectile;
    public ProjectileHitInfo HitInfo;
    public ProjectileReflectSettings ReflectSettings;

    public static EffectContext OnEquip(Entity owner) => new()
    {
        Owner = owner
    };

    public static EffectContext OnHit(Entity owner, Entity target, bool crit) => new()
    {
        Owner = owner,
        Target = target,
        IsCritical = crit
    };

    public static EffectContext OnPlayerHit(Entity owner, Entity dealer, bool crit, float dmg) => new()
    {
        Owner = owner,
        Target = dealer,
        IsCritical = crit,
        Damage = dmg
    };

    public static EffectContext OnTimerTick(Entity owner, float dt) => new()
    {
        Owner = owner,
        DeltaTime = dt
    };

    public static EffectContext OnSkillCast(Entity owner) => new()
    {
        Owner = owner
    };

    public static EffectContext OnProjectileHit(
        Projectile projectile,
        ProjectileHitInfo hitInfo,
        ProjectileReflectSettings reflectSettings = default) => new()
        {
            Owner = projectile != null ? projectile.Owner : null,
            Projectile = projectile,
            HitInfo = hitInfo,
            ReflectSettings = reflectSettings
        };

    public static EffectContext OnProjectileUpdate(Projectile projectile, float deltaTime) => new()
    {
        Owner = projectile != null ? projectile.Owner : null,
        DeltaTime = deltaTime,
        Projectile = projectile
    };

    public static EffectContext OnProjectileFire(Projectile projectile) => new()
    {
        Owner = projectile != null ? projectile.Owner : null,
        Projectile = projectile
    };

    public static EffectContext OnProjectileReset(Projectile projectile) => new()
    {
        Owner = projectile != null ? projectile.Owner : null,
        Projectile = projectile
    };
}