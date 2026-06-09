using GondrLib.ObjectPool.RunTime;
using System;
using System.Collections.Generic;
using UnityEngine;
using Work.SB._01.Scripts.Enemy.Script;

[Serializable]
[ModuleDisplayName("탄막 제거", "발동 시 플레이어 주변의 탄막을 제거합니다.")]
public class ClearProjectilesOnFistsEffectDef : IModuleEffectDef
{
    [Header("Range")]
    [Min(0f)]
    public float radius = 5f;

    public LayerMask projectileMask = ~0;

    [Header("Effect")]
    public string clearEffect;
    public Vector3 effectOffset = Vector3.zero;

    [Header("Filter")]
    public bool includeOwnerProjectiles = false;
    public bool includeOwnerlessProjectiles = true;

    public IModuleEffect CreateEffect()
    {
        return new ClearProjectilesOnFistsEffect(this);
    }
}

public class ClearProjectilesOnFistsEffect : IExecutableEffect
{
    private const int MaxProjectiles = 128;
    private static readonly Collider[] _hits = new Collider[MaxProjectiles];
    private static readonly HashSet<EnemyProjectileBase> _clearedProjectiles = new();

    private readonly ClearProjectilesOnFistsEffectDef _def;
    private Entity _owner;
    private EntityVFX _entityVFX;

    public ClearProjectilesOnFistsEffect(ClearProjectilesOnFistsEffectDef def)
    {
        _def = def;
    }

    public void OnInitialize(Entity owner)
    {
        _owner = owner;
        _entityVFX = owner.GetCompo<EntityVFX>();
    }

    public void OnUnequip()
    {
        _owner = null;
        _entityVFX = null;
    }

    public void Execute(EffectContext ctx)
    {
        if (_owner == null || _def.radius <= 0f)
            return;

        Vector3 center = _owner.transform.position;
        PlayClearEffect(center);
        ClearProjectiles(center);
    }

    private void PlayClearEffect(Vector3 center)
    {
        if (_def.clearEffect == null)
            return;

        _entityVFX.PlayVfx(_def.clearEffect,_owner.transform.position,Quaternion.identity);
    }

    private void ClearProjectiles(Vector3 center)
    {
        _clearedProjectiles.Clear();

        int hitCount = Physics.OverlapSphereNonAlloc(
            center,
            _def.radius,
            _hits,
            _def.projectileMask
        );

        for (int i = 0; i < hitCount; i++)
        {
            if (_hits[i] == null)
                continue;

            EnemyProjectileBase projectile = _hits[i].GetComponent<EnemyProjectileBase>();
            if (projectile == null || !projectile.gameObject.activeInHierarchy)
            {
                Debug.LogError(projectile.name);
                continue;
            }

            if (!_clearedProjectiles.Add(projectile))
                continue;

            if (!CanClear(projectile))
                continue;

            projectile.DeleteProjectile();
        }
    }

    private bool CanClear(EnemyProjectileBase projectile)
    {
        if (projectile.Owner == null)
            return _def.includeOwnerlessProjectiles;

        if (projectile.Owner == _owner)
            return _def.includeOwnerProjectiles;

        return true;
    }
}
