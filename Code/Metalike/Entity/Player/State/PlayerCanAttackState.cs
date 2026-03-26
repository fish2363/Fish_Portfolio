using UnityEngine;

public abstract class PlayerCanAttackState : PlayerState
{
    private SkillComponent _skillComponent;

    public PlayerCanAttackState(Entity entity, int animationHash) : base(entity, animationHash)
    {
        _skillComponent = entity.GetCompo<SkillComponent>();
    }


    public override void Enter()
    {
        base.Enter();
        _player.PlayerInput.OnSkillPressed += HandleSkillPressed;
    }

    private void HandleSkillPressed()
    {

        Skill skill = _skillComponent.GetCurrentSkill();
        if (skill == null) return;
        if (skill.IsCooldown) return;
        if (skill.IsWait) return;

        _player.ChangeState("SKILL");
    }

    public override void Exit()
    {
        _player.PlayerInput.OnSkillPressed -= HandleSkillPressed;
        base.Exit();
    }

    private void HandleSkillKeyPressed(bool isPressed)
    {
    }


    private void HandleAttackPressed()
    {
        _player.ChangeState("ATTACK");
    }
}
