using UnityEngine;

public class PlayerPickupState : PlayerState
{
    private CharacterMovement _movement;
    private float _enterTime;

    private const float MaxPickupDuration = 1f;

    public PlayerPickupState(Entity entity, int animationHash) : base(entity, animationHash)
    {
        _player = entity as Player;
        _movement = entity.GetCompo<CharacterMovement>();
    }

    public override void Enter()
    {
        base.Enter();
        _enterTime = Time.time;
        _movement.SetManualMovement(false);
    }

    public override void Update()
    {
        base.Update();

        if (_isTriggerCall || Time.time >= _enterTime + MaxPickupDuration)
        {
            Debug.Log("Pickup finished");
            _player.ChangeState("IDLE");
        }
    }

    public override void Exit()
    {
        _movement.SetManualMovement(true);
        base.Exit();
    }
}
