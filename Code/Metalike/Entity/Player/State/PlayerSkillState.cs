using Core.EventBus;
using UnityEngine;

public class PlayerSkillState : PlayerState
{
    private SkillComponent _skillComponent;
    private CharacterMovement _movement;

    public PlayerSkillState(Entity entity, int animationHash) : base(entity, animationHash)
    {
        _skillComponent = entity.GetCompo<SkillComponent>();
        _movement = entity.GetCompo<CharacterMovement>();
    }
    
    public override void Enter()
    {
        base.Enter();
        if (!_movement.CanManualMovement) return;

        _skillComponent.CurrentSkill.UseSkill(); //스킬 사용
        _player.ChangeState("IDLE");
    }

    public override void Update()
    {
        base.Update();

        //if(_isSkillCall)
        //{
        //    _skillComponent.CurrentSkill.UseSkill(); //스킬 사용
        //    _isSkillCall = false;
        //}

        //if (_isTriggerCall)
    }

    public override void Exit()
    {
        base.Exit();
    }
}
