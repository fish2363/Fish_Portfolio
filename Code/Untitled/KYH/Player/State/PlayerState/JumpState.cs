using UnityEngine;

public class JumpState : PlayerAirState
{
    public JumpState(Entity entity, AnimParamSO animParam) : base(entity, animParam)
    {
    }

    public override void Enter()
    {
        base.Enter();
        AudioManager.Instance.PlaySound2D("PlayerJump",0,false,SoundType.SfX);
        _mover.DecreaseJumpCount();
        _mover.StopImmediately(true);
        _mover.Jump();
        _mover.EffectorPlayer.PlayEffect("JumpEffect",false);
        _mover.OnVelocity.AddListener(HandleVelocityChange);
    }

    public override void Exit()
    {
        _mover.OnVelocity.RemoveListener(HandleVelocityChange);
        base.Exit();
    }

    private void HandleVelocityChange(Vector2 velocity)
    {
        if (velocity.y < 0)
            _player.ChangeState("FALL");
    }
}
