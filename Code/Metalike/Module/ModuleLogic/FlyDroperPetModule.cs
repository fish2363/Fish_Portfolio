using DG.Tweening;
using GondrLib.ObjectPool.RunTime;
using System;
using UnityEngine;

[Serializable]
public class FlyDroperPetDef : PetModuleDef
{
    [Header("설치할 프리팹")]
    public PoolItemSO objPrefab;
    public float mineLifeTime = 6f;
    public LayerMask whatIsEnemy;

    [Header("투하 연출")]
    public float approachHeight = 2.5f;
    public float approachSide = 1.4f;
    public float approachSpeed = 7f;
    public float dropPause = 0.05f;
    public float exitDistance = 1.1f;
    public float exitTime = 0.16f;

    [Header("VFX")]
    public PoolItemSO hitFxPrefab;

    public override IModuleLogic CreateLogic() => new FlyDroperPetModule(this);
}

public class FlyDroperPetModule : PetModule<FlyDroperPetDef>
{
    public FlyDroperPetModule(FlyDroperPetDef def) : base(def) { }

    public override bool TryAttack(Entity target)
    {
        if (!CanAttack() || target.IsDead || _def.objPrefab == null) return false;

        Transform targetTrm = target.transform;
        _currentTarget = target;
        if (_currentTarget == null) return false;

        _isBusy = true;
        _petCompo.NotifyActionStarted(this);

        KillTweens();


        Vector3 targetPos = targetTrm.position;
        Vector3 toOwner = _ownerTrm.position - targetPos;
        toOwner.y = 0f;

        if (toOwner.sqrMagnitude < 0.0001f) toOwner = Vector3.back;
        toOwner.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, toOwner).normalized;
        float sideSign = UnityEngine.Random.value < 0.5f ? -1f : 1f;

        Vector3 approachPos = targetPos + Vector3.up * _def.approachHeight + right * _def.approachSide * sideSign;
        Vector3 exitPos = targetPos + right * (_def.exitDistance * -sideSign) + Vector3.up * (_def.approachHeight * 0.35f);

        float approachDuration = Vector3.Distance(_petTrm.position, approachPos) / Mathf.Max(0.01f, _def.approachSpeed);

        _actionSequence = DOTween.Sequence();
        _actionSequence.SetLink(_petInstance, LinkBehaviour.KillOnDestroy);

        _actionSequence.AppendCallback(() =>
        {
            Vector3 lookDir = approachPos - _petTrm.position; lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.0001f)
                _petTrm.rotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
        });

        _actionSequence.Append(_petTrm.DOMove(approachPos, approachDuration).SetEase(Ease.Linear));

        _actionSequence.AppendCallback(() =>
        {
            Vector3 descendLook = _currentTarget.transform.position - _petTrm.position; descendLook.y = 0f;
            if (descendLook.sqrMagnitude > 0.0001f)
                _petTrm.rotation = Quaternion.LookRotation(descendLook.normalized, Vector3.up);
        });

        _actionSequence.Append(_petTrm.DOMove(targetPos, 0.14f).SetEase(Ease.InCubic));

        _actionSequence.AppendCallback(() =>
        {
            Vector3 dropPos = _currentTarget != null ? _currentTarget.transform.position : targetPos;

            if (_def.hitFxPrefab != null)
            {
                PoolingEffect effect = _moduleController.poolManager.Pop<PoolingEffect>(_def.hitFxPrefab);
                effect.transform.position = dropPos;
            }

            AssistMine mine = _moduleController.poolManager.Pop<AssistMine>(_def.objPrefab);
            mine.transform.position = dropPos;
            mine.Setup(_def.whatIsEnemy, _def.mineLifeTime, _owner);
        });

        _actionSequence.AppendInterval(_def.dropPause);
        _actionSequence.Append(_petTrm.DOMove(exitPos, _def.exitTime).SetEase(Ease.OutQuad));
        _actionSequence.AppendInterval(0.06f);
        _actionSequence.AppendCallback(FinishAction);

        return true;
    }
}