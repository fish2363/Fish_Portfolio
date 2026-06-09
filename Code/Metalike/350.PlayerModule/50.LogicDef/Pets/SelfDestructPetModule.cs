using DG.Tweening;
using GondrLib.ObjectPool.RunTime;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SelfDestructPetDef : PetModuleDef
{
    [Header("Search")]
    public float detectionRange = 12f;
    public LayerMask whatIsTarget;
    public bool requireLineOfSight = true;
    public LayerMask lineOfSightBlockers;

    [Header("Ready")]
    public float attackDelay = 0.6f;
    public bool cancelAttackWhenLineOfSightLost = true;
    public float requiredSlotDistanceBeforeAttack = 0.35f;

    [Header("Dash")]
    public float dashSpeed = 16f;
    public float impactDistance = 0.8f;
    public float maxDashTime = 1.2f;
    public float respawnDelay = 2f;
    public float obstacleProbeDistance = 2f;
    [Range(0f, 2f)] public float obstacleAvoidSideWeight = 0.9f;
    [Range(0f, 1.5f)] public float attackFormationOffsetScale = 0.8f;

    [Header("Explosion")]
    public float explosionRange = 2.5f;
    [Min(0f)] public float explosionVfxRangeScaleMultiplier = 0.8f;
    public StatSO damageStat;
    public AttackDataSO attackData;
    public PoolItemSO explosionFxPrefab;

    public override IModuleLogic CreateLogic()
    {
        return new SelfDestructPetModule(this);
    }
}

public class SelfDestructPetModule : PetModule<SelfDestructPetDef>
{
    private const int MaxTargets = 32;

    private readonly Collider[] _overlapBuffer = new Collider[MaxTargets];
    private readonly HashSet<Entity> _damagedEntities = new();

    private int _fallbackLineOfSightBlockers;
    private bool _hasExploded;
    private float _avoidanceSideSign = 1f;
    private float explosionVisualSize = 1.0f;

    public override bool IsIndependent => true;

    public SelfDestructPetModule(SelfDestructPetDef def) : base(def)
    {
    }

    public override void OnEquip(Entity owner)
    {
        base.OnEquip(owner);
        _fallbackLineOfSightBlockers = LayerMask.GetMask(
            "Default",
            "Ground",
            "Door",
            "Wreck",
            "Room"
        );
    }

    public override void ModuleUpdate(float deltaTime)
    {
        base.ModuleUpdate(deltaTime);

        if (!CanAttack())
            return;

        if (!IsNearSlot(_def.requiredSlotDistanceBeforeAttack))
            return;

        Entity target = FindTarget();
        if (target != null)
            TryAttack(target);
    }

    public override bool TryAttack(Entity target)
    {
        if (!CanAttack() || target == null || target.IsDead)
            return false;

        Vector3 targetPos = GetTargetAimPosition(target);
        if (!HasLineOfSight(_petTrm.position, targetPos))
            return false;

        _currentTarget = target;
        _isBusy = true;
        _hasExploded = false;
        _petCompo.NotifyActionStarted(this);
        KillTweens();

        _actionSequence = DOTween.Sequence();

        float attackDelay = Mathf.Max(0f, _def.attackDelay);
        if (attackDelay > 0f)
        {
            _actionSequence.Append(DOTween.To(
                () => 0f,
                _ => UpdateAttackDelay(),
                1f,
                attackDelay
            ).SetEase(Ease.Linear));
        }

        _actionSequence.Append(DOTween.To(
            () => 0f,
            _ => UpdateDash(),
            1f,
            Mathf.Max(0.05f, _def.maxDashTime)
        ).SetEase(Ease.Linear));
        _actionSequence.AppendCallback(ExplodeAndWaitRespawn);

        return true;
    }

    private void UpdateAttackDelay()
    {
        if (_petTrm == null || _currentTarget == null || _currentTarget.IsDead)
        {
            CancelAttack();
            return;
        }

        FollowSlot(Time.deltaTime);

        Vector3 targetPos = GetTargetAimPosition(_currentTarget);
        if (_def.cancelAttackWhenLineOfSightLost && !HasLineOfSight(_petTrm.position, targetPos))
        {
            CancelAttack();
            return;
        }

        RotateTo(targetPos - _petTrm.position);
    }

    private void UpdateDash()
    {
        if (_petTrm == null || _currentTarget == null || _currentTarget.IsDead)
        {
            ExplodeAndWaitRespawn();
            return;
        }

        Vector3 targetPos = GetTargetAimPosition(_currentTarget);
        Vector3 attackTargetPos = GetAttackTargetPosition(_currentTarget);
        Vector3 toTarget = targetPos - _petTrm.position;
        Vector3 toAttackTarget = attackTargetPos - _petTrm.position;

        float impactSqr = _def.impactDistance * _def.impactDistance;
        if (toTarget.sqrMagnitude <= impactSqr || toAttackTarget.sqrMagnitude <= impactSqr)
        {
            ExplodeAndWaitRespawn();
            return;
        }

        Vector3 direction = GetObstacleAwareDashDirection(targetPos, attackTargetPos);
        _petTrm.position = Vector3.MoveTowards(
            _petTrm.position,
            _petTrm.position + direction,
            _def.dashSpeed * Time.deltaTime
        );

        RotateTo(direction);
    }

    private Vector3 GetObstacleAwareDashDirection(Vector3 targetPos, Vector3 attackTargetPos)
    {
        Vector3 toMoveTarget = attackTargetPos - _petTrm.position;
        if (toMoveTarget.sqrMagnitude <= 0.0001f)
            return _petTrm.forward;

        Vector3 direct = toMoveTarget.normalized;
        Vector3 toRealTarget = targetPos - _petTrm.position;
        if (toRealTarget.sqrMagnitude <= 0.0001f)
            return direct;

        float targetDistance = toRealTarget.magnitude;
        int blockerMask = GetLineOfSightBlockerMask();

        if (blockerMask == 0 ||
            !Physics.Raycast(
                _petTrm.position,
                toRealTarget.normalized,
                out RaycastHit hit,
                targetDistance,
                blockerMask,
                QueryTriggerInteraction.Ignore))
        {
            return direct;
        }

        Vector3 flatDirect = direct;
        flatDirect.y = 0f;
        if (flatDirect.sqrMagnitude <= 0.0001f)
            flatDirect = _petTrm.forward;

        flatDirect.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, flatDirect).normalized;

        Vector3 preferred = GetAvoidanceDirection(flatDirect, right, _avoidanceSideSign);
        if (!IsDirectionBlocked(preferred, blockerMask))
            return preferred;

        _avoidanceSideSign *= -1f;
        Vector3 alternate = GetAvoidanceDirection(flatDirect, right, _avoidanceSideSign);
        if (!IsDirectionBlocked(alternate, blockerMask))
            return alternate;

        return preferred;
    }

    private Vector3 GetAvoidanceDirection(Vector3 direct, Vector3 right, float sideSign)
    {
        Vector3 direction = direct + right * (sideSign * _def.obstacleAvoidSideWeight);
        if (direction.sqrMagnitude <= 0.0001f)
            return direct;

        return direction.normalized;
    }

    private Vector3 GetAttackTargetPosition(Entity target)
    {
        Vector3 targetPos = GetTargetAimPosition(target);
        if (_petCompo == null || _ownerTrm == null)
            return targetPos;

        Vector3 slotOffset = _petCompo.GetSlotWorldPosition(this) - _ownerTrm.position;
        slotOffset.y = 0f;

        if (slotOffset.sqrMagnitude <= 0.0001f)
            return targetPos;

        return targetPos + slotOffset * _def.attackFormationOffsetScale;
    }

    private bool IsDirectionBlocked(Vector3 direction, int blockerMask)
    {
        float distance = Mathf.Max(0.1f, _def.obstacleProbeDistance);
        return Physics.Raycast(
            _petTrm.position,
            direction,
            distance,
            blockerMask,
            QueryTriggerInteraction.Ignore
        );
    }

    private void RotateTo(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.0001f)
            _petTrm.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    private void CancelAttack()
    {
        KillTweens();
        FinishAction();
    }

    private void ExplodeAndWaitRespawn()
    {
        if (_hasExploded)
            return;

        _hasExploded = true;
        KillTweens();
        Explode();

        _actionSequence = DOTween.Sequence();
        _actionSequence.AppendInterval(Mathf.Max(0f, _def.respawnDelay));
        _actionSequence.AppendCallback(() =>
        {
            SpawnPet();
            _hasExploded = false;
            FinishAction();
        });
    }

    private void Explode()
    {
        Vector3 explosionPos = _petTrm != null
            ? _petTrm.position
            : _currentTarget != null
                ? _currentTarget.transform.position
                : _ownerTrm.position;

        PlayExplosionFx(explosionPos);
        ApplyExplosionDamage(explosionPos);
        DestroyPet();
    }

    private Entity FindTarget()
    {
        int count = Physics.OverlapSphereNonAlloc(
            _petTrm.position,
            _def.detectionRange,
            _overlapBuffer,
            _def.whatIsTarget,
            QueryTriggerInteraction.Ignore
        );

        Entity best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            if (!TryGetTargetEntity(_overlapBuffer[i], out Entity entity))
                continue;

            if (!HasLineOfSight(_petTrm.position, GetTargetAimPosition(entity)))
                continue;

            float dist = (_petTrm.position - entity.transform.position).sqrMagnitude;
            if (dist >= bestDist)
                continue;

            bestDist = dist;
            best = entity;
        }

        return best;
    }

    private void ApplyExplosionDamage(Vector3 explosionPos)
    {
        if (_moduleController == null ||
            _moduleController.DamageCompo == null ||
            _def.damageStat == null ||
            _def.attackData == null)
        {
            return;
        }

        _damagedEntities.Clear();

        int count = Physics.OverlapSphereNonAlloc(
            explosionPos,
            _def.explosionRange,
            _overlapBuffer,
            _def.whatIsTarget,
            QueryTriggerInteraction.Ignore
        );

        DamageData damageData = _moduleController.DamageCompo.CalculateDamage(
            _def.damageStat,
            _def.attackData
        );

        for (int i = 0; i < count; i++)
        {
            if (!TryGetTargetEntity(_overlapBuffer[i], out Entity entity))
                continue;

            if (!_damagedEntities.Add(entity))
                continue;

            IDamageable damageable = _overlapBuffer[i].GetComponentInChildren<IDamageable>();
            if (damageable == null)
                continue;

            Vector3 hitDir = entity.transform.position - explosionPos;
            hitDir.y = 0f;
            if (hitDir.sqrMagnitude <= 0.0001f)
                hitDir = Vector3.up;
            else
                hitDir.Normalize();

            damageable.ApplyDamage(
                damageData,
                entity.transform.position,
                hitDir,
                _def.attackData,
                _owner
            );
        }
    }

    private void PlayExplosionFx(Vector3 position)
    {
        if (_def.explosionFxPrefab != null &&
            _moduleController != null &&
            _moduleController.PoolManager != null)
        {
            PoolingEffect effect = _moduleController.PoolManager.Pop<PoolingEffect>(_def.explosionFxPrefab);
            if (effect != null)
                effect.PlayVFX(position, Quaternion.identity, GetExplosionVfxScale());
        }
    }
    private float GetExplosionRange()
    {
        if (_def.explosionRange > 0f)
            return Mathf.Max(0.01f, _def.explosionRange);

        return Mathf.Max(0.01f, _def.explosionRange);
    }

    private Vector3 GetExplosionVfxScale()
    {
        float k = GetExplosionRange();
        float visualMultiplier = Mathf.Max(0f, explosionVisualSize);
        float correction = Mathf.Max(0f, _def.explosionVfxRangeScaleMultiplier);
        return Vector3.one * (k * visualMultiplier * correction);
    }

    private bool HasLineOfSight(Vector3 origin, Vector3 targetPos)
    {
        if (!_def.requireLineOfSight)
            return true;

        int blockerMask = GetLineOfSightBlockerMask();

        if (blockerMask == 0)
            return true;

        return !Physics.Linecast(origin, targetPos, blockerMask, QueryTriggerInteraction.Ignore);
    }

    private int GetLineOfSightBlockerMask()
    {
        return _def.lineOfSightBlockers.value != 0
            ? _def.lineOfSightBlockers.value
            : _fallbackLineOfSightBlockers;
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
