using UnityEngine;

public class PlayerAttackState : PlayerState
{
    private PlayerAttackCompo _attackCompo;
    private CharacterMovement _movement;

    public PlayerAttackState(Entity entity, int animationHash) : base(entity, animationHash)
    {
        _movement = entity.GetCompo<CharacterMovement>();
        _attackCompo = entity.GetCompo<PlayerAttackCompo>();
    }

    public override void Enter()
    {
        base.Enter();

        _movement.StopImmediately();
        _movement.CanManualMovement = false;

        Vector3 attackDirection = _attackCompo.GetCameraAttackDirection();
        _attackCompo.RotateToAttackDirection(attackDirection);

        _attackCompo.Attack();

        ApplyAttackData(attackDirection);
    }

    private void ApplyAttackData(Vector3 attackDirection)
    {
        AttackDataSO currentAtkData = _attackCompo.GetCurrentAttackData();

        if (attackDirection.sqrMagnitude < 0.0001f)
            attackDirection = _player.transform.forward;

        _player.transform.rotation = Quaternion.LookRotation(attackDirection);

        if (currentAtkData.movementData != null)
        {
            _movement.ApplyMovementData(attackDirection, currentAtkData.movementData);
        }
    }

    public override void Exit()
    {
        _attackCompo.EndAttack();

        _movement.StopImmediately();
        _movement.CanManualMovement = true;

        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (_isTriggerCall)
            _player.ChangeState("IDLE");
    }
}
