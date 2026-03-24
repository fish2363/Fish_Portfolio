using DG.Tweening;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class RamPetDef : PetModuleDef
{
    public bool enableTrailIfExists = true;

    [Header("Punch: Attack Movement Settings")]
    public float dashDistance = 4f;
    public float approachRange = 3f;
    public float passThroughDistance = 1f;
    public float driftSideDistance = 2f;
    public float dashTime = 0.2f;
    public float brakeTime = 0.3f;

    [Header("Punch: Damage Settings")]
    public StatSO damageStat;
    public AttackDataSO attackData;
    public GameObject impactFxPrefab;

    public override IModuleLogic CreateLogic() => new RamPetModule(this);
}

public class RamPetModule : PetModule<RamPetDef>
{
    private TrailRenderer _trail;

    public RamPetModule(RamPetDef def) : base(def) { }

    public override void OnEquip(Entity owner)
    {
        base.OnEquip(owner);
    }

    protected override void OnPetSpawned()
    {
        if (_def.enableTrailIfExists && _petInstance != null)
        {
            _trail = _petInstance.GetComponentInChildren<TrailRenderer>(true);
            if (_trail != null) { _trail.enabled = true; _trail.emitting = false; _trail.Clear(); }
        }
    }

    protected override void OnTeleported()
    {
        if (_trail != null) { _trail.emitting = false; _trail.Clear(); }
    }

    public override bool TryAttack(Entity target)
    {
        if (!CanAttack() || target == null) return false;

        _currentTarget = target;
        _isBusy = true;
        _petCompo.NotifyActionStarted(this);
        KillTweens();

        Vector3 startPos = _petTrm.position;
        Vector3 targetPos = target.transform.position;
        Vector3 toTarget = targetPos - startPos;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.0001f) toTarget = _ownerTrm.forward;
        toTarget.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, toTarget).normalized;
        float sideSign = UnityEngine.Random.value < 0.5f ? -1f : 1f;
        Vector3 approachPos = targetPos - toTarget * _def.dashDistance;

        Vector3 toApproach = approachPos - startPos;
        toApproach.y = 0f;
        float approachDist = toApproach.magnitude;
        float approachTime = 0f;
        bool needApproach = approachDist > 0.05f;

        if (approachDist > _def.approachRange) approachTime = approachDist / Mathf.Max(0.01f, _def.followMoveSpeed);
        else if (needApproach) approachTime = Mathf.Min(0.08f, approachDist / Mathf.Max(0.01f, _def.followMoveSpeed * 1.5f));

        Vector3 passPos = targetPos + toTarget * _def.passThroughDistance;
        Vector3 driftPos = passPos + right * (_def.driftSideDistance * sideSign);

        _actionSequence = DOTween.Sequence();
        _actionSequence.SetLink(_petInstance, LinkBehaviour.KillOnDestroy);

        _actionSequence.AppendCallback(() => { if (_trail != null) { _trail.enabled = true; _trail.Clear(); _trail.emitting = false; } });

        if (needApproach && approachTime > 0f)
        {
            _actionSequence.AppendCallback(() => {
                Vector3 lookDir = approachPos - _petTrm.position; lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.0001f) _petTrm.rotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
            });
            _actionSequence.Append(_petTrm.DOMove(approachPos, approachTime).SetEase(Ease.Linear));
        }

        _actionSequence.AppendCallback(() => {
            Vector3 dashDir = (_currentTarget != null ? _currentTarget.transform.position : targetPos) - _petTrm.position;
            dashDir.y = 0f;
            if (dashDir.sqrMagnitude < 0.0001f) dashDir = toTarget;

            _petTrm.rotation = Quaternion.LookRotation(dashDir.normalized, Vector3.up);
            if (_trail != null) _trail.emitting = true;
        });

        _actionSequence.Append(DOTween.To(() => 0f, _ => {
            if (_currentTarget == null || _petTrm == null) return;
            Vector3 dashDir = _currentTarget.transform.position - _petTrm.position; dashDir.y = 0f;
            if (dashDir.sqrMagnitude < 0.0001f) return;

            dashDir.Normalize();
            _petTrm.position += dashDir * ((_def.dashDistance / _def.dashTime) * Time.deltaTime);
            _petTrm.rotation = Quaternion.LookRotation(dashDir, Vector3.up);
        }, 1f, _def.dashTime).SetEase(Ease.OutCubic));

        _actionSequence.AppendCallback(() => {
            Vector3 impactDir = _petTrm != null ? _petTrm.forward : toTarget;
            ApplyDamageOnce();
            if (_def.impactFxPrefab != null) Object.Instantiate(_def.impactFxPrefab, _petTrm.position, Quaternion.LookRotation(impactDir, Vector3.up));
        });

        _actionSequence.Append(_petTrm.DOMove(passPos, Mathf.Max(0.05f, _def.dashTime * 0.55f)).SetEase(Ease.InQuad));
        _actionSequence.Append(_petTrm.DOMove(driftPos, _def.brakeTime).SetEase(Ease.OutExpo));

        // ÈÅ(Hook)À¸·Î Æ®·¹ÀÏ ²ô±â ¿¬µ¿
        _actionSequence.AppendCallback(() => { if (_trail != null) _trail.emitting = false; });

        _actionSequence.AppendInterval(0.08f);
        _actionSequence.AppendCallback(FinishAction);

        return true;
    }

    private void ApplyDamageOnce()
    {
        if (_currentTarget.GetCompo<EntityHealth>()?.TryGetComponent(out IDamageable damageable) == true)
        {
            var damageData = _moduleController.DamageCompo.CalculateDamage(_def.damageStat, _def.attackData, 1f);
            damageable.ApplyDamage(damageData, _petTrm != null ? _petTrm.position : _owner.transform.position, Vector3.up, _def.attackData, _owner);
        }
    }
}