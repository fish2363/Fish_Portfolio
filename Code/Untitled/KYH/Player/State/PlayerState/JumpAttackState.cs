using UnityEngine;

public class JumpAttackState : EntityState
{
    private Player _player;
    private EntityMover _mover;
    private PlayerAttackCompo _attackCompo;

    public JumpAttackState(Entity entity, AnimParamSO animParam) : base(entity, animParam)
    {
        _player = entity as Player;
        _mover = entity.GetCompo<EntityMover>();
        _attackCompo = entity.GetCompo<PlayerAttackCompo>();
    }

    public override void Enter()
    {
        base.Enter();
        _mover.StopImmediately(true);
        _mover.SetGravityScale(0.1f); //순간적으로 공중에 멈추도록
        _mover.CanManualMove = false;
        _mover.EffectorPlayer.PlayEffect("JumpAttackEffect");
        AudioManager.Instance.PlaySound2D($"SwordAttack{2}", 0, false, SoundType.SfX);
        AudioManager.Instance.PlaySound2D($"SwordAttack{0}", 0.5f, false, SoundType.SfX);
        SetAttackData();
    }

    private void SetAttackData()
    {
        AttackDataSO attackData = _attackCompo.GetAttackData("PlayerJumpAttack");
        Vector2 movement = attackData.movement;
        movement.x *= _renderer.FacingDirection;
        _mover.AddForceToEntity(movement);

        _attackCompo.SetAttackData(attackData);
    }

    public override void Update()
    {
        base.Update();
        if (_isTriggerCall)
            _player.ChangeState("FALL");
        if(_dialogueComponent.isDialogue)
            _player.ChangeState("IDLE");
    }

    public override void Exit()
    {
        _mover.CanManualMove = true;
        _mover.SetGravityScale(1f);
        base.Exit();
    }
}