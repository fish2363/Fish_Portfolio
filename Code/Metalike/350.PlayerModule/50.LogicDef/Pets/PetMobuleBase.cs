using DG.Tweening;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public abstract class PetModuleDef : IModuleLogicDef
{
    [Header("Base: 펫 프리팹")]
    public GameObject petPrefab;

    [Header("Base: 대기 이동")]
    public float followMoveSpeed = 4.2f;
    public float catchUpMultiplier = 1.8f;
    public float rotateSpeed = 10f;
    public float defaultHeight = 10f;

    [Header("Base: 텔레포트 안전장치")]
    public float teleportToOwnerDistance = 15f;
    public float teleportGraceTimeAfterAction = 0.8f;

    public abstract IModuleLogic CreateLogic();
}

public abstract class PetModule<T> : IModuleLogic, IPetModule, IUpdateModuleLogic
    where T : PetModuleDef

{
    protected readonly T _def;
    protected Entity _owner;
    protected Transform _ownerTrm;
    protected PetComponent _petCompo;
    protected ModuleController _moduleController;
    protected EntityStatCompo _entityStatCompo;

    protected GameObject _petInstance;
    protected Transform _petTrm;

    protected bool _isBusy;
    protected Sequence _actionSequence;
    protected Entity _currentTarget;

    protected float _lastActionFinishedTime = -999f;

    public bool IsBusy => _isBusy;
    public bool HasPetInstance => _petInstance != null;

    public virtual bool IsIndependent => false;

    protected PetModule(T def)
    {
        _def = def;
    }

    public virtual void OnEquip(Entity owner)
    {
        _owner = owner;
        _ownerTrm = owner.transform;
        _petCompo = owner.GetCompo<PetComponent>();
        _moduleController = owner.GetCompo<ModuleController>();
        _entityStatCompo = owner.GetCompo<EntityStatCompo>();

        SpawnPet();
    }

    public virtual void ModuleUpdate(float deltaTime)
    {
        if (_petCompo == null || _petTrm == null || _isBusy)
            return;

        FollowSlot(deltaTime);
    }

    public virtual void OnUnequip()
    {
        KillTweens();
        DestroyPet();

        _owner = null;
        _ownerTrm = null;
        _petCompo = null;
        _moduleController = null;
        _entityStatCompo = null;
        _currentTarget = null;
        _isBusy = false;
    }

    protected void FollowSlot(float deltaTime)
    {
        Vector3 slotPos = _petCompo.GetSlotWorldPosition(this);
        Vector3 targetPos = new Vector3(slotPos.x, slotPos.y + _def.defaultHeight, slotPos.z);
        Vector3 currentPos = _petTrm.position;

        Vector3 toTarget = targetPos - currentPos;
        toTarget.y = 0f;

        float slotDistance = toTarget.magnitude;
        float ownerDistance = _ownerTrm != null
            ? Vector3.Distance(_petTrm.position, _ownerTrm.position)
            : slotDistance;

        bool canTeleport = Time.time >= _lastActionFinishedTime + _def.teleportGraceTimeAfterAction;

        if (canTeleport && ownerDistance >= _def.teleportToOwnerDistance)
        {
            _petTrm.position = targetPos;

            if (_ownerTrm != null)
            {
                Vector3 ownerForward = _ownerTrm.forward;
                ownerForward.y = 0f;

                if (ownerForward.sqrMagnitude > 0.0001f)
                    _petTrm.rotation = Quaternion.LookRotation(ownerForward.normalized, Vector3.up);
            }

            OnTeleported();
            return;
        }

        float followSpeed = _def.followMoveSpeed;

        if (slotDistance > 3f)
            followSpeed *= _def.catchUpMultiplier;
        else if (slotDistance > 1.5f)
            followSpeed *= 1.3f;

        _petTrm.position = Vector3.MoveTowards(
            currentPos,
            targetPos,
            followSpeed * deltaTime
        );

        if (toTarget.sqrMagnitude > 0.0001f)
        {
            _petTrm.rotation = Quaternion.Slerp(
                _petTrm.rotation,
                Quaternion.LookRotation(toTarget.normalized, Vector3.up),
                deltaTime * _def.rotateSpeed
            );
        }
        else if (_ownerTrm != null)
        {
            Vector3 ownerForward = _ownerTrm.forward;
            ownerForward.y = 0f;

            if (ownerForward.sqrMagnitude > 0.0001f)
            {
                _petTrm.rotation = Quaternion.Slerp(
                    _petTrm.rotation,
                    Quaternion.LookRotation(ownerForward.normalized, Vector3.up),
                    deltaTime * _def.rotateSpeed
                );
            }
        }
    }

    protected void SpawnPet()
    {
        if (_def.petPrefab == null)
            return;

        Vector3 spawnPos = _ownerTrm != null ? _ownerTrm.position : Vector3.zero;

        Quaternion spawnRot = _ownerTrm != null && _ownerTrm.forward.sqrMagnitude > 0.0001f
            ? Quaternion.LookRotation(_ownerTrm.forward, Vector3.up)
            : Quaternion.identity;

        _petInstance = Object.Instantiate(_def.petPrefab, spawnPos, spawnRot);
        _petTrm = _petInstance.transform;

        OnPetSpawned();
    }

    protected void DestroyPet()
    {
        if (_petInstance != null)
            Object.Destroy(_petInstance);

        _petInstance = null;
        _petTrm = null;
    }

    protected void KillTweens()
    {
        _actionSequence?.Kill();
        _actionSequence = null;
    }

    public bool CanAttack()
    {
        return _petCompo != null &&
               _petTrm != null &&
               !_isBusy &&
               _ownerTrm != null;
    }

    protected void FinishAction()
    {
        _currentTarget = null;
        _isBusy = false;
        _lastActionFinishedTime = Time.time;
        _petCompo?.NotifyActionFinished(this);
    }

    protected virtual void OnPetSpawned()
    {
    }

    protected virtual void OnTeleported()
    {
    }

    public abstract bool TryAttack(Entity target);
}
