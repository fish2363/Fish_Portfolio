using Core.EventBus;
using GondrLib.ObjectPool.RunTime;
using Public.Core.Events;
using System;
using System.Collections;
using UnityEngine;
using Work.SB._01.Scripts.Enemy.Script;

[Serializable]
public class ProcBonusDamageDef : IModuleLogicDef
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

    [Header("Damage")]
    public StatSO damageStat;
    public AttackDataSO attackData;

    public IModuleLogic CreateLogic() => new ProcBonusDamageModule(this);
}

public class ProcBonusDamageModule : IModuleLogic
{
    private readonly ProcBonusDamageDef _def;
    private Entity _owner;
    private ModuleController _moduleController;

    private bool _isRunning;
    private Transform _targetTrm;
    private Vector3 _targetPosCache;

    public ProcBonusDamageModule(ProcBonusDamageDef def) => _def = def;

    public void OnEquip(Entity owner)
    {
        _owner = owner;
        _moduleController = owner.GetCompo<ModuleController>();
        Bus<AttackSuccessEvent>.OnEvent += HandleSuccess;
    }

    public void ModuleUpdate(float deltaTime) { }

    public void OnUnequip()
    {
        Bus<AttackSuccessEvent>.OnEvent -= HandleSuccess;
        _owner.StopSafeCoroutine(this);
        _isRunning = false;
    }

    private void HandleSuccess(AttackSuccessEvent evt)
    {
        if (_isRunning || UnityEngine.Random.value > _def.procChance) return;
        if (_targetTrm == null) return;

        _targetTrm = evt._enemy.transform;
        _targetPosCache = evt._enemy.transform.position;

        _isRunning = true;
        _owner.StartSafeCoroutine(this,ElectricRoutine());
    }

    private IEnumerator ElectricRoutine()
    {
        for (int i = 0; i < _def.pulseCount; i++)
        {
            Vector3 basePos = _def.lockTarget ? _targetTrm.position : _targetPosCache;
            Vector2 r = UnityEngine.Random.insideUnitCircle * _def.fxRandomRadius;
            Vector3 spawnPos = basePos + _def.fxOffset + new Vector3(r.x, 0f, r.y);

            if (_def.fxPrefab != null)
            {
                PoolingEffect fx = _moduleController.poolManager.Pop<PoolingEffect>(_def.fxPrefab);
                fx.PlayVFX(spawnPos, Quaternion.identity);
            }

            if(_def.explosionRange <= 0)
            {
                ApplyDamage(_targetTrm);
            }
            else
            {
                Collider[] enemies = Physics.OverlapSphere(basePos, _def.explosionRange, _moduleController.whatIsTarget);
                foreach (Collider col in enemies)
                    ApplyDamage(col.transform);
            }

            if (i < _def.pulseCount - 1 && _def.pulseInterval > 0f)
                yield return new WaitForSeconds(_def.pulseInterval);
        }
        _isRunning = false;
    }

    private void ApplyDamage(Transform target)
    {
        if (target.TryGetComponent(out Entity enemy) && enemy.GetCompo<EntityHealth>().TryGetComponent(out IDamageable damageable))
        {
            Vector3 hitDir = (_targetTrm.transform.position - _owner.transform.position).normalized;

            DamageData damageData = _moduleController.DamageCompo.CalculateDamage(_def.damageStat, _def.attackData, 1f);
            damageable.ApplyDamage(damageData, _owner.transform.position, hitDir, _def.attackData, _owner);
            //if (enemy is Enemy noBoss) noBoss.EnemyStunned(_def.stunDuration);
        }
    }
}