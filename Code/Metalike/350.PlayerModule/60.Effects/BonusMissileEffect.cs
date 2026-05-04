using GondrLib.ObjectPool.RunTime;
using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class BonusMissileEffectDef : IModuleEffectDef
{
    [Header("발사체")]
    public PoolItemSO bulletPrefab;
    public float speed = 15f;

    [Header("온히트 추가타")]
    public float spawnRadius = 4f;    
    public float homingStrength = 10f;

    public IModuleEffect CreateEffect() => new BonusMissileEffect(this);
}

public class BonusMissileEffect : IExecutableEffect
{
    private readonly BonusMissileEffectDef _def;
    private ModuleController _moduleController;

    public BonusMissileEffect(BonusMissileEffectDef def) => _def = def;

    public void OnEquip(Entity owner)
    {
        _moduleController = owner.GetCompo<ModuleController>();
    }

    public void OnUnequip() { }

    private IEnumerator HomingRoutine(Projectile bullet, Entity target)
    {
        Vector3 currentDir = bullet.transform.forward;

        while (bullet != null && bullet.gameObject.activeSelf)
        {
            Vector3 targetPos = (target != null && !target.IsDead)
                ? target.transform.position + Vector3.up * 0.5f
                : bullet.transform.position + bullet.transform.forward * 10f;

            Vector3 toTarget = (targetPos - bullet.transform.position).normalized;

            currentDir = Vector3.Slerp(currentDir, toTarget, Time.deltaTime * _def.homingStrength);
            bullet.transform.rotation = Quaternion.LookRotation(currentDir);
            bullet.transform.position += currentDir * _def.speed * Time.deltaTime;

            yield return null;
        }
    }

    public void Execute(EffectContext ctx)
    {
        if (ctx.Target == null || ctx.Target.IsDead) return;
        if (_def.bulletPrefab == null) return;

        Vector3 randomOffset = new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-0.2f, 0.2f),
            UnityEngine.Random.Range(-1f, 1f)
        ).normalized * _def.spawnRadius;

        Vector3 spawnPos = ctx.Target.transform.position + randomOffset;
        Vector3 toTarget = (ctx.Target.transform.position + Vector3.up * 0.5f - spawnPos).normalized;
        Quaternion spawnRot = Quaternion.LookRotation(toTarget);

        Projectile bullet = _moduleController.PoolManager.Pop<Projectile>(_def.bulletPrefab);
        bullet.transform.SetPositionAndRotation(spawnPos, spawnRot);
        bullet.StartCoroutine(HomingRoutine(bullet, ctx.Target));
    }
}