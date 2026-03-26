using System;
using System.Threading;
using UnityEngine;

public abstract class PlayerCanAttackState : PlayerState
{
    protected CharacterMovement _movement;

    public PlayerCanAttackState(Entity entity, int animationHash) : base(entity, animationHash)
    {
        _movement = entity.GetCompo<CharacterMovement>();
    }


    public override void Enter()
    {
        base.Enter();
        _player.PlayerInput.OnJumpPressed += HandleJump;
    }

    private void HandleJump()
    {
        if (_movement.IsGround)
            _movement.Jump();
    }

    public override void Update()
    {
        base.Update();
    }
    public override void Exit()
    {
        _player.PlayerInput.OnJumpPressed -= HandleJump;
        base.Exit();
    }

    private void HandleAttackPressed()
    {
        _player.ChangeState("ATTACK");
    }
}