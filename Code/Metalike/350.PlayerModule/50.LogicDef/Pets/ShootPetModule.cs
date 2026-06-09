using GondrLib.ObjectPool.RunTime;
using System;
using UnityEngine;

public enum ETargetSelectMode
{
    Closest,
    LowestHp,
    Random
}

[Serializable]
public class ShootPetDef : PetModuleDef
{
    [Header("탐색")]
    public float detectionRange = 12f;
    public LayerMask whatIsTarget;
    public ETargetSelectMode targetSelectMode = ETargetSelectMode.Closest;

    public PoolItemSO bulletPrefab;
    public StatSO damageStat;
    public AttackDataSO attackData;

    [Header("공격")]
    public float attackCooldown = 1.5f;
    public float bulletSpeed = 20f;
    public bool rotateToTargetOnAttack = true;
    public bool requireLineOfSight = true;
    public LayerMask lineOfSightBlockers;

    [Header("총구 위치")]
    public Vector3 muzzleOffset = new Vector3(0f, 0f, 0.5f);

    public override IModuleLogic CreateLogic()
    {
        return new ShootPetModule(this);
    }
}

public class ShootPetModule : PetModule<ShootPetDef>
{
    private float _cooldownTimer;
    private readonly Collider[] _overlapBuffer = new Collider[16];
    private int _fallbackLineOfSightBlockers;

    public override bool IsIndependent => true;

    public ShootPetModule(ShootPetDef def) : base(def)
    {
    }

    public override void OnEquip(Entity owner)
    {
        base.OnEquip(owner);
        _cooldownTimer = _def.attackCooldown;
        _fallbackLineOfSightBlockers = LayerMask.GetMask(
            "Default",
            "Ground",
            "Door",
            "Wreck",
            "Room",
            "Shutter"
        );
    }

    public override void ModuleUpdate(float deltaTime)
    {
        base.ModuleUpdate(deltaTime);

        _cooldownTimer -= deltaTime;
        if (_cooldownTimer > 0f)
            return;

        Entity target = FindTarget();
        if (target == null)
            return;

        if (TryAttack(target))
            _cooldownTimer = _def.attackCooldown;
    }

    public override bool TryAttack(Entity target)
    {
        if (!CanAttack() || target == null || target.IsDead)
            return false;

        Vector3 muzzlePos = GetMuzzleWorldPosition();
        Vector3 targetPos = GetTargetAimPosition(target);

        if (!HasLineOfSight(muzzlePos, targetPos))
            return false;

        Vector3 direction = targetPos - muzzlePos;

        if (direction.sqrMagnitude <= 0.0001f)
            return false;

        direction.Normalize();

        Vector3 lookDir = direction;
        lookDir.y = 0f;

        if (_def.rotateToTargetOnAttack && lookDir.sqrMagnitude > 0.0001f)
            _petTrm.rotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);

        FireBullet(muzzlePos, direction);
        return true;
    }

    private void FireBullet(Vector3 muzzlePos, Vector3 direction)
    {
        if (_moduleController == null ||
            _moduleController.PoolManager == null ||
            _def.bulletPrefab == null ||
            _def.damageStat == null ||
            _def.attackData == null ||
            _entityStatCompo == null)
        {
            return;
        }

        Projectile bullet = _moduleController.PoolManager.Pop<Projectile>(_def.bulletPrefab);
        if (bullet == null)
            return;

        bullet.transform.SetPositionAndRotation(
            muzzlePos,
            Quaternion.LookRotation(direction, Vector3.up)
        );

        var stat = _entityStatCompo.GetStat(_def.damageStat);
        if (stat == null)
            return;

        DamageData damageData = _moduleController.DamageCompo.CalculateDamage(
            stat,
            _def.attackData
        );

        bullet.Fire(damageData, direction, _def.bulletSpeed, _def.attackData, _owner);
    }

    private Vector3 GetMuzzleWorldPosition()
    {
        if (_petTrm == null)
            return _owner != null ? _owner.transform.position : Vector3.zero;

        return _petTrm.TransformPoint(_def.muzzleOffset);
    }

    private Entity FindTarget()
    {
        if (_petTrm == null)
            return null;

        int count = Physics.OverlapSphereNonAlloc(
            _petTrm.position,
            _def.detectionRange,
            _overlapBuffer,
            _def.whatIsTarget
        );

        if (count == 0)
            return null;

        return _def.targetSelectMode switch
        {
            ETargetSelectMode.Closest => FindClosest(count),
            ETargetSelectMode.LowestHp => FindLowestHp(count),
            ETargetSelectMode.Random => FindRandom(count),
            _ => FindClosest(count)
        };
    }

    private Entity FindClosest(int count)
    {
        Entity best = null;
        float bestDist = float.MaxValue;
        Vector3 muzzlePos = GetMuzzleWorldPosition();

        for (int i = 0; i < count; i++)
        {
            if (!TryGetTargetEntity(_overlapBuffer[i], out Entity entity))
                continue;

            if (!HasLineOfSight(muzzlePos, GetTargetAimPosition(entity)))
                continue;

            float dist = (_petTrm.position - entity.transform.position).sqrMagnitude;
            if (dist >= bestDist)
                continue;

            bestDist = dist;
            best = entity;
        }

        return best;
    }

    private Entity FindLowestHp(int count)
    {
        Entity best = null;
        float bestHp = float.MaxValue;
        Vector3 muzzlePos = GetMuzzleWorldPosition();

        for (int i = 0; i < count; i++)
        {
            if (!TryGetTargetEntity(_overlapBuffer[i], out Entity entity))
                continue;

            if (!HasLineOfSight(muzzlePos, GetTargetAimPosition(entity)))
                continue;

            EntityHealth hp = entity.GetCompo<EntityHealth>();
            if (hp == null)
                continue;

            if (hp.CurrentHealth >= bestHp)
                continue;

            bestHp = hp.CurrentHealth;
            best = entity;
        }

        return best;
    }

    private Entity FindRandom(int count)
    {
        Entity selected = null;
        int validCount = 0;
        Vector3 muzzlePos = GetMuzzleWorldPosition();

        for (int i = 0; i < count; i++)
        {
            if (!TryGetTargetEntity(_overlapBuffer[i], out Entity entity))
                continue;

            if (!HasLineOfSight(muzzlePos, GetTargetAimPosition(entity)))
                continue;

            validCount++;

            if (UnityEngine.Random.Range(0, validCount) == 0)
                selected = entity;
        }

        return selected;
    }

    private bool HasLineOfSight(Vector3 origin, Vector3 targetPos)
    {
        if (!_def.requireLineOfSight)
            return true;

        int blockerMask = _def.lineOfSightBlockers.value != 0
            ? _def.lineOfSightBlockers.value
            : _fallbackLineOfSightBlockers;

        if (blockerMask == 0)
            return true;

        return !Physics.Linecast(origin, targetPos, blockerMask, QueryTriggerInteraction.Ignore);
    }

    private static Vector3 GetTargetAimPosition(Entity target)
    {
        return target.transform.position + Vector3.up * 0.5f;
    }

    private static bool TryGetTargetEntity(Collider collider, out Entity entity)
    {
        entity = null;

        if (collider == null)
            return false;

        if (!collider.TryGetComponent(out entity))
            entity = collider.GetComponentInParent<Entity>();

        return entity != null && !entity.IsDead;
    }
}
