using UnityEngine;

public class WallSlideState : EntityState
{
    private Player _player;
    private EntityMover _mover;

    private const float WALL_SLIDE_GRAVITY_SCALE = 0.6f;
    private const float WALL_SLIDE_LIMIT_SPEED = 5f;
    private const float NORMAL_LIMIT_SPEED = 40f;

    public WallSlideState(Entity entity, AnimParamSO animParam) : base(entity, animParam)
    {
        _player = entity as Player;
        _mover = entity.GetCompo<EntityMover>();
    }
    public override void Enter()
    {
        base.Enter();
        _mover.StopImmediately(true);
        _mover.SetLimitYSpeed(WALL_SLIDE_LIMIT_SPEED);
        _mover.SetGravityScale(WALL_SLIDE_GRAVITY_SCALE);
        _mover.EffectorPlayer.PlayEffect("WallRideEffect");
        AudioManager.Instance.PlaySound2D("WallSlide",0,true,SoundType.SfX);
    }

    public override void Update()
    {
        base.Update();
        float xInput = _player.PlayerInput.InputDirection.x;
        if (Mathf.Abs(xInput + _renderer.FacingDirection) < 0.5f)
        {
            Debug.Log("°¹¹ß");
            _player.ChangeState("FALL");
            _mover.ResetJumpCount();
            return;
        }

        //Âß ³»·Á°¡´Ù°¡ ¶¥¿¡ ´ê¾Ò´Ù¸é IDLE·Î º¯°æÇØ¾ß ÇÑ´Ù.
        if (_mover.IsGroundDetected() || _mover.IsWallDetected(_renderer.FacingDirection) == false)
        {
            _player.ChangeState("IDLE");
        }
    }

    public override void Exit()
    {
        AudioManager.Instance.StopLoopSound("WallSlide");
        _mover.EffectorPlayer.StopEffect("WallRideEffect");
        _mover.SetGravityScale(1f);
        _mover.SetLimitYSpeed(NORMAL_LIMIT_SPEED);
        base.Exit();
    }
}
