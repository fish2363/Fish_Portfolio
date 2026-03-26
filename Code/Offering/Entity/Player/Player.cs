using GondrLib.Dependencies;
using System;
using Unity.Cinemachine;
using UnityEngine;

[Provide]
public class Player : Entity, IDependencyProvider, IKnockBackable
{
    [field: SerializeField] public PlayerInputSO PlayerInput { get; private set; }
    [SerializeField] private StateDataSO[] stateDataList;

    private EntityStateMachine _stateMachine;
    private CharacterMovement _movement;

    public event Action OnHeadDropEvent;

    [Provide] public Player ProvidePlayer() => this;

    protected override void Awake()
    {
        base.Awake();
        _stateMachine = new EntityStateMachine(this, stateDataList);
        _movement = GetCompo<CharacterMovement>();
        OnDeathEvent.AddListener(HandleDeadEvent);
        Bus<DropHeadEvent>.OnEvent += HandleDropHead;
    }

    private void HandleDropHead(DropHeadEvent evt)
    {
        if (this != evt.player) return;
        OnHeadDropEvent?.Invoke();
        ChangeState("HEAD_ROLL");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        OnDeathEvent.RemoveListener(HandleDeadEvent);
    }

    private void HandleDeadEvent()
    {
        if (IsDead) return;
        IsDead = true;
        ChangeState("DEAD", true);
    }

    private void Start()
    {
        const string idle = "IDLE";
        _stateMachine.ChangeState(idle);
    }

    private void Update()
    {
        _stateMachine.UpdateStateMachine();
    }

    public void ChangeState(string newStateName, bool force = false)
        => _stateMachine.ChangeState(newStateName, force);


    public void KnockBack(Vector3 direction, MovementDataSO knockbackMovement)
        => _movement.KnockBack(direction, knockbackMovement);
}