using System.Collections;
using UnityEngine;

public class Player : Entity, IKnockBackable
{
    [field: SerializeField] public GameEventChannelSO PlayerChannel { get; private set; }
    [field: SerializeField] public InputReader PlayerInput { get; private set; }

    [SerializeField] private StateListSO playerFSM;

    private StateMachine _stateMachine;
    private EntityMover _mover;


    protected override void Awake()
    {
        base.Awake();
        _stateMachine = new StateMachine(this, playerFSM);

        _mover = GetCompo<EntityMover>();
    }

    private void Start()
    {
        _stateMachine.ChangeState("IDLE");
    }

    private void Update()
    {
        _stateMachine.UpdateStateMachine();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (PlayerInput != null)
            PlayerInput.ClearSubscription();
    }

    public void ChangeState(string newState) => _stateMachine.ChangeState(newState);

    protected override void HandleHit() { }

    protected override void HandleDead()
    {
        if (IsDead) return;

        IsDead = true;
        _stateMachine.ChangeState("DEAD");
    }

    public void KnockBack(Vector3 direction, MovementDataSO knockBackMovement)
    {
        _mover.KnockBack(direction, knockBackMovement);
    }
}