using GondrLib.ObjectPool.RunTime;
using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class ProcBonusDamageDef : IModuleEffectDef
{
    [Header("FX Setting")]
    public PoolItemSO fxPrefab;
    public Vector3 fxOffset = new Vector3(0f, 1.0f, 0f);
    public float fxRandomRadius = 0.3f;

    [Header("Attack Setting")]
    [Range(0.1f, 1f)] public float procChance = 0.5f;
    [Min(1)] public int pulseCount = 2;
    [Min(0f)] public float pulseInterval = 1f;
    public bool lockTarget = true;

    [Header("Types Setting")]
    public float stunDuration = 0.4f;
    public float explosionRange = 0f;

    [Header("damageData")]
    public StatSO damageStat;
    public AttackDataSO attackData;

    public IModuleEffect CreateEffect() => new ProcBonusDamageModule(this);
}

public class ProcBonusDamageModule : IExecutableEffect
{
    private const int MAX_TARGETS = 32;
    private static readonly Collider[] _overlapBuffer = new Collider[MAX_TARGETS];

    private readonly ProcBonusDamageDef _def;
    private Entity _owner;
    private ModuleController _moduleController;

    private bool _isRunning;
    private Transform _targetTrm;
    private Vector3 _targetPosCache;

    public ProcBonusDamageModule(ProcBonusDamageDef def) => _def = def;

    private IEnumerator ElectricRoutine()
    {
        for (int i = 0; i < _def.pulseCount; i++)
        {
            if (_def.lockTarget && (_targetTrm == null ||
            _targetTrm.TryGetComponent(out Entity e) && e.IsDead)) break;

            Vector3 basePos = _def.lockTarget ? _targetTrm.position : _targetPosCache;
            Vector2 r = UnityEngine.Random.insideUnitCircle * _def.fxRandomRadius;
            Vector3 spawnPos = basePos + _def.fxOffset + new Vector3(r.x, 0f, r.y);

            if (_def.fxPrefab != null)
            {
                PoolingEffect fx = _moduleController.PoolManager.Pop<PoolingEffect>(_def.fxPrefab);
                fx.PlayVFX(spawnPos, Quaternion.identity);
            }

            if (_def.explosionRange <= 0)
            {
                ApplyDamage(_targetTrm);
            }
            else
            {
                int hitCount = Physics.OverlapSphereNonAlloc(
                    basePos, _def.explosionRange, _overlapBuffer, _moduleController.WhatIsTarget);

                for (int j = 0; j < hitCount; j++)
                    ApplyDamage(_overlapBuffer[j].transform);
            }

            if (i < _def.pulseCount - 1 && _def.pulseInterval > 0f)
                yield return new WaitForSeconds(_def.pulseInterval);
        }
        _isRunning = false;
    }

    private void ApplyDamage(Transform target)
    {
        if (!target.TryGetComponent(out Entity enemy)) return;
        if (!enemy.GetCompo<EntityHealth>().TryGetComponent(out IDamageable damageable)) return;

        Vector3 hitDir = (target.position - _owner.transform.position).normalized;

        DamageData damageData = _moduleController.DamageCompo.CalculateDamage(_def.damageStat, _def.attackData, 1f);
        damageable.ApplyDamage(damageData, _owner.transform.position, hitDir, _def.attackData, _owner);
        //if (enemy is Enemy noBoss) noBoss.EnemyStunned(_def.stunDuration);
    }

    public void OnEquip(Entity owner)
    {
        _owner = owner;
        _moduleController = owner.GetCompo<ModuleController>();
    }

    public void OnUnequip()
    {
        _owner.StopSafeCoroutine(this);
        _isRunning = false;
    }

    public void Execute(EffectContext ctx)
    {
        if (_isRunning || UnityEngine.Random.value > _def.procChance) return;
        if (ctx.Target == null || ctx.Target.IsDead) return;

        _targetTrm = ctx.Target.transform;
        _targetPosCache = _targetTrm.position;

        _isRunning = true;
        _owner.StartSafeCoroutine(this, ElectricRoutine());
    }
}