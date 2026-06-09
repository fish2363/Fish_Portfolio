using GondrLib.ObjectPool.RunTime;
using PJS.Managers;
using System;
using System.Collections;
using UnityEngine;

[ModuleDisplayName("보너스 미사일", "보너스 미사일을 발사합니다.")]
[Serializable]
public class BonusMissileEffectDef : IModuleEffectDef
{
    [Header("발사체")]
    public PoolItemSO bulletPrefab;
    public float speed = 15f;

    [Header("곡선 탄도")]
    public float launchHeightOffset = 0.8f;
    public float targetHeightOffset = 0.5f;
    public float arcHeight = 2f;
    public float arcSideOffset = 1.5f;

    [Header("스케일 기준")]
    public bool useCurrencyScaling = false;
    public CurrencyTypeSO currencySO;

    [Header("발동 확률")]
    [Range(0f, 1f)] public float baseProbability = 1f;
    public float probabilityPerCurrency = 0.001f;
    public float probabilityPerDamage = 0.01f;
    [Range(0f, 1f)] public float maxProbability = 1f;

    [Header("피해량")]
    public float baseDamageMultiplier = 1f;
    public float damageMultiplierPerCurrency = 0.01f;
    public float damageMultiplierPerDamage = 0.02f;
    public float maxDamageMultiplier = 5f;

    public IModuleEffect CreateEffect() => new BonusMissileEffect(this);
}

public class BonusMissileEffect : IExecutableEffect
{
    private readonly BonusMissileEffectDef _def;
    private ModuleController _moduleController;

    public BonusMissileEffect(BonusMissileEffectDef def) => _def = def;

    public void OnInitialize(Entity owner)
    {
        _moduleController = owner.GetCompo<ModuleController>();
    }

    public void OnUnequip() { }

    private IEnumerator ArcRoutine(Projectile bullet, Entity target, Vector3 startPos, Vector3 targetPos, Vector3 sideDir)
    {
        float distance = Vector3.Distance(startPos, targetPos);
        float duration = _def.speed > 0f ? Mathf.Max(0.05f, distance / _def.speed) : 0.05f;
        float elapsed = 0f;
        Vector3 prevPos = startPos;

        while (bullet != null && bullet.gameObject.activeSelf && elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (target != null && !target.IsDead)
                targetPos = GetTargetPoint(target);

            Vector3 control = GetArcControlPoint(startPos, targetPos, sideDir);
            Vector3 nextPos = EvaluateQuadraticBezier(startPos, control, targetPos, t);
            Vector3 moveDir = nextPos - prevPos;

            if (moveDir.sqrMagnitude > 0.0001f)
                bullet.transform.rotation = Quaternion.LookRotation(moveDir.normalized);

            bullet.transform.position = nextPos;
            prevPos = nextPos;

            yield return null;
        }

        if (bullet == null || !bullet.gameObject.activeSelf)
            yield break;

        if (target != null && !target.IsDead)
            bullet.ApplyDamageDirect(target.transform, bullet.DamageData, targetPos, -bullet.transform.forward);

        bullet.ReturnToPool();
    }

    public void Execute(EffectContext ctx)
    {
        if (ctx.Owner == null) return;
        if (ctx.Target == null || ctx.Target.IsDead) return;
        if (_def.bulletPrefab == null) return;
        if (_moduleController == null || _moduleController.PoolManager == null) return;

        float scaleValue = GetScaleValue(ctx);
        float probability = CalculateProbability(scaleValue);

        if (UnityEngine.Random.value > probability)
            return;

        Projectile bullet = _moduleController.PoolManager.Pop<Projectile>(_def.bulletPrefab);
        if (bullet == null) return;

        Vector3 spawnPos = GetSpawnPosition(ctx.Owner);
        Vector3 targetPos = GetTargetPoint(ctx.Target);
        Vector3 sideDir = GetSideArcDirection(spawnPos, targetPos);
        Vector3 toTarget = GetDirectionToTarget(spawnPos, targetPos);

        DamageData damageData = ctx.DamageData;
        damageData.damage *= CalculateDamageMultiplier(scaleValue);

        bullet.transform.SetPositionAndRotation(spawnPos, Quaternion.LookRotation(toTarget));
        bullet.BeginManualMovement(damageData, toTarget, null, ctx.Owner);
        bullet.StartCoroutine(ArcRoutine(bullet, ctx.Target, spawnPos, targetPos, sideDir));
    }

    private float GetScaleValue(EffectContext ctx)
    {
        if (!_def.useCurrencyScaling)
            return ctx.DamageData.damage;

        if (_def.currencySO == null)
            return 0f;

        return CurrencyManager.Instance.GetAmount(_def.currencySO);
    }

    private float CalculateProbability(float scaleValue)
    {
        float perValue = _def.useCurrencyScaling
            ? _def.probabilityPerCurrency
            : _def.probabilityPerDamage;

        float probability = _def.baseProbability + scaleValue * perValue;
        return Mathf.Clamp(probability, 0f, _def.maxProbability);
    }

    private float CalculateDamageMultiplier(float scaleValue)
    {
        float perValue = _def.useCurrencyScaling
            ? _def.damageMultiplierPerCurrency
            : _def.damageMultiplierPerDamage;

        float multiplier = _def.baseDamageMultiplier + scaleValue * perValue;
        return Mathf.Min(multiplier, _def.maxDamageMultiplier);
    }

    private Vector3 GetSpawnPosition(Entity owner)
    {
        return owner.transform.position + Vector3.up * _def.launchHeightOffset;
    }

    private Vector3 GetTargetPoint(Entity target)
    {
        return target.transform.position + Vector3.up * _def.targetHeightOffset;
    }

    private Vector3 GetDirectionToTarget(Vector3 spawnPos, Vector3 targetPoint)
    {
        Vector3 toTarget = targetPoint - spawnPos;

        if (toTarget.sqrMagnitude < 0.0001f)
            return Vector3.forward;

        return toTarget.normalized;
    }

    private Vector3 GetSideArcDirection(Vector3 spawnPos, Vector3 targetPos)
    {
        Vector3 flatDir = targetPos - spawnPos;
        flatDir.y = 0f;

        if (flatDir.sqrMagnitude < 0.0001f)
            flatDir = Vector3.forward;
        else
            flatDir.Normalize();

        Vector3 sideDir = Vector3.Cross(Vector3.up, flatDir).normalized;
        return UnityEngine.Random.value < 0.5f ? sideDir : -sideDir;
    }

    private Vector3 GetArcControlPoint(Vector3 startPos, Vector3 targetPos, Vector3 sideDir)
    {
        Vector3 middle = Vector3.Lerp(startPos, targetPos, 0.5f);
        return middle + Vector3.up * _def.arcHeight + sideDir * _def.arcSideOffset;
    }

    private Vector3 EvaluateQuadraticBezier(Vector3 startPos, Vector3 controlPos, Vector3 targetPos, float t)
    {
        float inverseT = 1f - t;
        return inverseT * inverseT * startPos
            + 2f * inverseT * t * controlPos
            + t * t * targetPos;
    }
}
