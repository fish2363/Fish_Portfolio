using GondrLib.Dependencies;
using Unity.Cinemachine;
using UnityEngine;

[Provide]
public class Player : Entity, IDependencyProvider, IKnockBackable
{
    [field: SerializeField] public PlayerInputSO PlayerInput { get; private set; }
    [SerializeField] private StateDataSO[] stateDataList;

    [Header("Visual Models")]

    public CameraTargetFollow curFollow; // 굴러다닐 머리 모델
    public Transform _headPoint; // 굴러다닐 머리 모델
    // 외부(State)에서 접근할 수 있도록 프로퍼티 열기

    private EntityStateMachine _stateMachine;
    private CharacterMovement _movement;

    [Provide] public Player ProvidePlayer() => this;

    protected override void Awake()
    {
        base.Awake();
        _stateMachine = new EntityStateMachine(this, stateDataList);
        _movement = GetCompo<CharacterMovement>();
        OnDeathEvent.AddListener(HandleDeadEvent);
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
    {
        _movement.KnockBack(direction, knockbackMovement);
    }
}