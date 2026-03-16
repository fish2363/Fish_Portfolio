using UnityEngine;

public class PlayerGroundState : EntityState
{
    protected Player _player;
    protected EntityMover _mover;
    protected PlayerAttackCompo _attackCompo;
    public PlayerGroundState(Entity entity, AnimParamSO animParam) : base(entity, animParam)
    {
        _player = entity as Player;
        _mover = entity.GetCompo<EntityMover>();
        _attackCompo = entity.GetCompo<PlayerAttackCompo>();
    }
    public override void Enter()
    {
        base.Enter();
        _player.PlayerInput.OnJumpKeyPressed += HandleJumpKeyPress;
        _player.PlayerInput.OnAttackKeyPressed += HandleAttackKeyPress;
        _player.PlayerInput.OnCounterKeyPressed += HandleCounterKeyPress;
        _player.PlayerInput.OnSkillKeyPressed += HandleSkillKeyPress;
    }

    public override void Update()
    {
        base.Update();
        if (_mover.IsGroundDetected() == false && _mover.CanManualMove && !_mover.IsWallDetected(_renderer.FacingDirection))
        {
            _player.ChangeState("FALL");
        }
    }
    public override void Exit()
    {
        _player.PlayerInput.OnJumpKeyPressed -= HandleJumpKeyPress;
        _player.PlayerInput.OnAttackKeyPressed -= HandleAttackKeyPress;
        _player.PlayerInput.OnCounterKeyPressed -= HandleCounterKeyPress;
        _player.PlayerInput.OnSkillKeyPressed -= HandleSkillKeyPress;
        base.Exit();
    }
    

    private void HandleSkillKeyPress(bool isPressed)
    {
        //Skill activeSkill = _player.GetCompo<SkillCompo>().activeSkill;
        //if (activeSkill == null) return;

        //if (isPressed && activeSkill.AttemptUseSkill())
        //{
        //    if (activeSkill is IReleasable)
        //        _player.ChangeState("SKILL_CHARGE");
        //    else
        //        _player.ChangeState("SKILL_USE");
        //}
    }

    private void HandleCounterKeyPress()
    {
        //나중에 쿨타임도 체크해야한다.
        if (_attackCompo.CanAttack && !_dialogueComponent.isDialogue && _mover.CanManualMove)
            _player.ChangeState("COUNTER_ATTACK");
    }

    protected virtual void HandleAttackKeyPress()
    {
        if (_mover.IsGroundDetected() && _attackCompo.CanAttack && !_dialogueComponent.isDialogue && _mover.CanManualMove)
            _player.ChangeState("ATTACK");
    }

    private void HandleJumpKeyPress()
    {
        if (_mover.IsGroundDetected() && !_dialogueComponent.isDialogue && _mover.CanManualMove)
            _player.ChangeState("JUMP");
    }
}
