using System;
using System.Threading;
using UnityEngine;

public abstract class PlayerCanAttackState : PlayerGroundState
{

    public PlayerCanAttackState(Entity entity, int animationHash) : base(entity, animationHash)
    {
    }


    public override void Enter()
    {
        base.Enter();
        _player.PlayerInput.OnAttackPressed += HandleAttackPressed;
    }

    public override void Update()
    {
        base.Update();
    }
    public override void Exit()
    {
        _player.PlayerInput.OnAttackPressed -= HandleAttackPressed;
        base.Exit();
    }

    private void HandleAttackPressed()
    {
        if (!_player.IsLocalPlayer) return;

        _player.ChangeState("ATTACK");
    }
}