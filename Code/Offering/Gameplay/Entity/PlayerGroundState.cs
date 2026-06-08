using System;
using UnityEngine;

public class PlayerGroundState : PlayerState
{
    private ItemCollector _collector;
    private PlayerHeadHolder _headHolder;
    protected CharacterMovement _movement;

    public PlayerGroundState(Entity entity, int animationHash) : base(entity, animationHash)
    {
        _movement = entity.GetCompo<CharacterMovement>();
        _collector = entity.GetCompo<ItemCollector>();
        _headHolder = entity.GetCompo<PlayerHeadHolder>();
    }

    public override void Enter()
    {
        base.Enter();
        _player.PlayerInput.OnInteractPressed += HandleInteraction;
        _player.PlayerInput.OnJumpPressed += HandleJump;
    }

    private void HandleJump()
    {
        if (!_player.IsLocalPlayer) return;
        if (_movement.IsGround)
            _movement.Jump();
    }

    private void HandleInteraction()
    {
        if (!_player.IsLocalPlayer) return;

        if (_collector.TryInteract() && !_headHolder.HasHead)
            _player.ChangeState("PICKUP");
    }


    public override void Exit()
    {
        base.Exit();
        _player.PlayerInput.OnJumpPressed -= HandleJump;
        _player.PlayerInput.OnInteractPressed -= HandleInteraction;
    }
}
