using UnityEngine;

public class PlayerAirState : EntityState
{
    protected Player _player;
    protected EntityMover _mover;
    protected PlayerAttackCompo _attackCompo;

    public PlayerAirState(Entity entity, AnimParamSO animParam) : base(entity, animParam)
    {
        _player = entity as Player;
        _mover = entity.GetCompo<EntityMover>();
        _attackCompo = entity.GetCompo<PlayerAttackCompo>();
    }

    public override void Enter()
    {
        base.Enter();
        _mover.SetMoveSpeedMultiplier(0.7f);
        _player.PlayerInput.OnJumpKeyPressed += HandleAirJump;
        _player.PlayerInput.OnAttackKeyPressed += HandleAirAttack;
    }

    public override void Update()
    {
        base.Update();
        Debug.Log("因掻");
        float xInput = _player.PlayerInput.InputDirection.x;
        if (Mathf.Abs(xInput) > 0)
            _mover.SetMovementX(xInput);

        bool isFrontMove = Mathf.Abs(xInput + _renderer.FacingDirection) > 1;

        if(_mover.IsWallDetected(_renderer.FacingDirection) && !_mover.IsGroundDetected())
        {
            Debug.Log("ししししししししししししししししししししししししししししししししししししし");
            if (isFrontMove)
            {
                _player.ChangeState("WALL_SLIDE");
            }
            else
            {
                _player.ChangeState("IDLE");
            }
        }
        
    }

    public override void Exit()
    {
        _player.PlayerInput.OnAttackKeyPressed -= HandleAirAttack;
        _player.PlayerInput.OnJumpKeyPressed -= HandleAirJump;
        _mover.SetMoveSpeedMultiplier(1f);
        base.Exit();
    }
    private void HandleAirJump()
    {
        if (_mover.CanJump)
            _player.ChangeState("JUMP");
    }

    private void HandleAirAttack()
    {
        if(_attackCompo.CanAttack && !_dialogueComponent.isDialogue)
        _player.ChangeState("JUMP_ATTACK");
    }
}
