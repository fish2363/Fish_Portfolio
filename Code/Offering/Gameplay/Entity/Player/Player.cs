using System;
using UnityEngine;
using UnityEngine.Events;

public class Player : Entity, IKnockBackable, IPoolable
{
    [field: SerializeField] public PlayerInputSO PlayerInput { get; private set; }
    [SerializeField] private StateDataSO[] stateDataList;

    [SerializeField] private UnityEvent onFullHealRequested;

    private EntityStateMachine _stateMachine;
    private CharacterMovement _movement;
    private PlayerHeadController _headController;

    public bool IsLocalPlayer { get; private set; }
    public string NetworkPlayerID { get; set; }

    public bool IsHeadDetached => _headController != null && _headController.IsDetached;
    public Player HeadCarrier => _headController != null ? _headController.Carrier : null;
    public bool IsHeadCarried => _headController != null && _headController.IsCarried;

    public GameObject GameObject => gameObject;
    [field: SerializeField] public PoolItemSO PoolItem { get; private set; }
    private Pool _myPool;

    public event Action OnHeadDropEvent;
    public event Action OnSelfAttachedEvent;
    public event Action<Player> OnHeadPickedUpEvent;
    public event Action OnHeadDroppedByCarrierEvent;
    public event Action<Player> OnSacrificedEvent;

    protected override void Awake()
    {
        base.Awake();

        _stateMachine = new EntityStateMachine(this, stateDataList);
        _movement = GetCompo<CharacterMovement>();
        _headController = GetCompo<PlayerHeadController>();

        OnDeathEvent.AddListener(HandleDeadEvent);
        Bus<DropHeadEvent>.OnEvent += HandleDropHead;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        OnDeathEvent.RemoveListener(HandleDeadEvent);
        Bus<DropHeadEvent>.OnEvent -= HandleDropHead;
    }

    public void StartSetting(string id, bool isLocalPlayer)
    {
        NetworkPlayerID = id;
        SetLocalPlayer(isLocalPlayer);
        ChangeState("IDLE");
    }

    public void SetLocalPlayer(bool isLocalPlayer)
    {
        IsLocalPlayer = isLocalPlayer;

        if (IsLocalPlayer)
            EnableLocalInput();
        else
            DisableLocalInput();
    }

    private void Update()
    {
        if (!IsLocalPlayer) return;

        _stateMachine.UpdateStateMachine();
    }

    private void HandleDropHead(DropHeadEvent evt)
    {
        if (NetworkPlayerID != evt.playerID) return;

        DropHead();
    }

    private void HandleDeadEvent()
    {
        if (IsDead) return;

        IsDead = true;
        DropHead();
    }

    public void DropHead()
        => _headController.Drop();

    public bool CanSelfAttach()
        => _headController.CanSelfAttach();

    public void SelfAttachHead()
        => _headController.SelfAttach();

    public void AttachHeadAndRevive()
        => _headController.AttachAndRevive();

    public bool TryPickUpHead(Player carrier)
        => _headController.TryPickUp(carrier);

    public void DropFromCarrier()
        => _headController.DropFromCarrier();

    public void SacrificeBy(Player scorer)
        => _headController.SacrificeBy(scorer);

    public void EnableLocalInput()
    {
        IsLocalPlayer = true;

        EntityHealthCompo health = GetCompo<EntityHealthCompo>();
        if (health != null)
            health.NotifyHealthChanged();
    }

    public void DisableLocalInput()
    {
        IsLocalPlayer = false;

        if (_movement != null)
            _movement.SetMovementDirection(Vector2.zero);
    }

    public void RestoreFullHealth()
    {
        EntityHealthCompo health = GetCompo<EntityHealthCompo>();
        if (health != null)
            health.RestoreFullHealth(true);
    }

    public void RaiseHeadDropped() => OnHeadDropEvent?.Invoke();

    public void RaiseSelfAttached() => OnSelfAttachedEvent?.Invoke();

    public void RaiseHeadPickedUp(Player carrier) => OnHeadPickedUpEvent?.Invoke(carrier);

    public void RaiseHeadDroppedByCarrier() => OnHeadDroppedByCarrierEvent?.Invoke();

    public void RaiseSacrificed(Player scorer) => OnSacrificedEvent?.Invoke(scorer);

    public void ChangeState(string newStateName, bool force = false)
        => _stateMachine.ChangeState(newStateName, force);

    public void KnockBack(Vector3 direction, MovementDataSO knockbackMovement)
        => _movement.KnockBack(direction, knockbackMovement);

    public void SetUpPool(Pool pool) => _myPool = pool;

    public void ResetItem()
    {
    }
}
