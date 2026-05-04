using UnityEngine;

public abstract class EntityState
{
    protected Entity _entity;
    protected int _animationHash;
    protected EntityAnimator _entityAnimator;
    protected bool _isTriggerCall;
    protected bool _isSkillCall;

    public EntityState(Entity entity, int animationHash)
    {
        _entity = entity;
        _animationHash = animationHash;
        _entityAnimator = entity.GetCompo<EntityAnimator>();
    }

    public virtual void Enter()
    {
        _entityAnimator.SetParam(_animationHash, true);
        _isTriggerCall = false;
        if(_entityAnimator.Handler != null)
        _entityAnimator.Handler.OnAnimationEndTrigger += AnimationEndTrigger; //3
    }

    public virtual void Update() { }

    public virtual void Exit()
    {
        _entityAnimator.SetParam(_animationHash, false);
        if (_entityAnimator.Handler != null)
            _entityAnimator.Handler.OnAnimationEndTrigger -= AnimationEndTrigger; //4
    }

    public virtual void AnimationEndTrigger() => _isTriggerCall = true;
    public virtual void SkillTrigger() => _isSkillCall = true;
}
