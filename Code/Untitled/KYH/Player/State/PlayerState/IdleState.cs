using UnityEngine;

public class IdleState : PlayerGroundState
{
    public IdleState(Entity entity, AnimParamSO animParam) : base(entity, animParam)
    {
    }

    public override void Enter()
    {
        base.Enter();
        _mover.StopImmediately(false);
    }

    public override void Update()
    {
        base.Update();

        float xInput = _player.PlayerInput.InputDirection.x;

        float facingDir = _renderer.FacingDirection;
        if (Mathf.Abs(facingDir + xInput) > 1.5f && _mover.IsWallDetected(facingDir)) return;

        if (Mathf.Abs(xInput) > 0 && _mover.CanManualMove && !_dialogueComponent.isDialogue)
        {
            _player.ChangeState("MOVE");
        }
    }
}
