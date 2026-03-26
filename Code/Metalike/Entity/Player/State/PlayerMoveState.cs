using UnityEngine;

public class PlayerMoveState : PlayerCanAttackState
{
    private CharacterMovement _movement;
    private readonly string _footStepEffectName = "FootStep";
    public PlayerMoveState(Entity entity, int animationHash) : base(entity, animationHash)
    {
        _movement = entity.GetCompo<CharacterMovement>();
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        Vector2 movementKey = _player.PlayerInput.MovementKey;
        _movement.SetMovementDirection(movementKey);
        if (movementKey.magnitude < _inputThreshold)
            _player.ChangeState("IDLE");
    }
}