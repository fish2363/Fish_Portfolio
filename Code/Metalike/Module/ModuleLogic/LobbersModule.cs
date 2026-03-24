using GondrLib.ObjectPool.RunTime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LobbersTargetType
{
    FarthestEnemy,
    ClosestEnemy,
    FarthestEnemies,
    ClosestEnemies
}

[Serializable]
public class LobbersDef : IModuleLogicDef
{
    [Header("Prefab / VFX")]
    public PoolItemSO decalPrefab;
    public PoolItemSO projectilePrefab;
    public PoolItemSO explosionParticlePrefab;

    [Header("Damage")]
    public StatSO damageStat;
    public AttackDataSO attackData;

    [Header("Projectile Settings")]
    public float projectileHeight = 5f;
    public float projectileSpeed = 1.5f;
    public int shootCount;
    public float warningTime = 2f;
    public float range = 3f;
    public float cooldownTime = 5f;
    public LobbersTargetType targetType;

    public IModuleLogic CreateLogic() => new LobbersModule(this);
}
public class LobbersModule : IModuleLogic
{
    private readonly LobbersDef _def;
    private Entity _owner;
    private ModuleController _moduleController;

    private float _cooldownTimer;
    private const float DecalYOffset = 0.5f;

    public LobbersModule(LobbersDef def) => _def = def;

    public void OnEquip(Entity owner)
    {
        _owner = owner;
        _moduleController = owner.GetCompo<ModuleController>();
    }

    public void ModuleUpdate(float deltaTime)
    {
        if (_cooldownTimer > 0)
        {
            _cooldownTimer -= deltaTime;
            return;
        }

        Fire();
    }

    public void OnUnequip() { _cooldownTimer = 0f; }

    private void Fire()
    {
        Vector3 pos = _owner.transform.position;
        int count = _def.shootCount;

        List<Entity> targets = _def.targetType switch
        {
            LobbersTargetType.FarthestEnemy => _moduleController.FindFarthestEnemies(pos, 40f, 1),
            LobbersTargetType.ClosestEnemy => _moduleController.FindClosestEnemies(pos, 40f, 1),
            LobbersTargetType.FarthestEnemies => _moduleController.FindFarthestEnemies(pos, 40f, count),
            LobbersTargetType.ClosestEnemies => _moduleController.FindClosestEnemies(pos, 40f, count),
            _ => new List<Entity>()
        };

        if (targets == null || targets.Count == 0) return;

        _cooldownTimer = _def.cooldownTime;

        foreach (Entity target in targets) 
        {
            _owner.StartCoroutine(SiegeRoutine(target.transform));
        }
    }

    private IEnumerator SiegeRoutine(Transform target)
    {
        RoundDecal decal = _moduleController.poolManager.Pop<RoundDecal>(_def.decalPrefab);

        float elapsed = 0f;
        while (elapsed < _def.warningTime)
        {
            elapsed += Time.deltaTime;
            if (target == null) break;
            decal.transform.position = GetTargetPoint(target);
            yield return null;
        }

        Vector3 firePoint = GetTargetPoint(target);
        decal.Push();
        FireProjectile(firePoint,target);
    }

    private void FireProjectile(Vector3 startTargetPos,Transform target)
    {
        Vector3 startPos = _owner.transform.position;
        LobberProjectile projectile = _moduleController.poolManager.Pop<LobberProjectile>(_def.projectilePrefab);
        projectile.transform.position = startPos;

        _owner.StartCoroutine(ParabolaMove(projectile, startPos,target));
    }

    private IEnumerator ParabolaMove(LobberProjectile projectile, Vector3 start, Transform target)
    {
        float t = 0f;
        while (t < 1f)
        {
            if (target == null) break;
            t += Time.deltaTime * _def.projectileSpeed;
            Vector3 end = GetTargetPoint(target);
            Vector3 pos = Vector3.Lerp(start, end, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * _def.projectileHeight;
            projectile.transform.position = pos;
            yield return null;
        }

        Explode(GetTargetPoint(target));
        projectile.ReturnToPool();
    }

    private void Explode(Vector3 pos)
    {
        if (_def.explosionParticlePrefab != null)
        {
            PoolingEffect projectile = _moduleController.poolManager.Pop<PoolingEffect>(_def.explosionParticlePrefab);
            projectile.transform.position = pos;
            projectile.PlayVFX(pos,Quaternion.identity);
        }

        Collider[] enemies = Physics.OverlapSphere(pos, _def.range, _moduleController.whatIsTarget);
        foreach (Collider col in enemies)
        {
            if (col.TryGetComponent(out Entity enemy) && enemy.GetCompo<EntityHealth>().TryGetComponent(out IDamageable damageable))
            {
                DamageData damageData = _moduleController.DamageCompo.CalculateDamage(_def.damageStat, _def.attackData, 1.2f);
                damageable.ApplyDamage(damageData, enemy.transform.position, Vector3.up, _def.attackData, _owner);
            }
        }
    }

    private Vector3 GetTargetPoint(Transform target) => target != null ? target.position + new Vector3(0, DecalYOffset, 0) : Vector3.zero;
}