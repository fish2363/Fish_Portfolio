using UnityEngine;

public class MoveState : PlayerGroundState
{
    [SerializeField] private float dashAttackInputDelay = 0.3f;

    private PlayerDashComponent _dashComponent;
    private float _stateEnterTime;

    public MoveState(Entity entity, AnimParamSO animParam) : base(entity, animParam)
    {
        _dashComponent = entity.GetCompo<PlayerDashComponent>();
    }

    public override void Enter()
    {
        base.Enter();
        _stateEnterTime = Time.time;
    }

    public override void Update()
    {
        base.Update();

        float xInput = _player.PlayerInput.InputDirection.x;

        if (_mover.CanManualMove && !_dialogueComponent.isDialogue)
            _mover.SetMovementX(xInput);

        if (Mathf.Approximately(xInput, 0f) ||
            _mover.IsWallDetected(_renderer.FacingDirection) ||
            !_mover.CanManualMove)
        {
            _player.ChangeState("IDLE");
        }
    }

    protected override void HandleAttackKeyPress()
    {
        bool canDashAttack =
            _stateEnterTime + dashAttackInputDelay < Time.time &&
            _attackCompo.CanAttack &&
            !_dialogueComponent.isDialogue &&
            !_dashComponent.IsDashCoolingDown;

        if (canDashAttack)
        {
            _player.ChangeState("DASH_ATTACK");
            return;
        }

        base.HandleAttackKeyPress();
    }
}