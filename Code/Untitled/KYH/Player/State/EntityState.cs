using UnityEngine;

public abstract class EntityState
{
    protected Entity _entity;
    protected AnimParamSO _animParam;
    protected EntityAnimationTrigger _animatorTrigger; //1
    protected EntityRenderer _renderer;
    protected DialogueComponent _dialogueComponent;

    protected bool _isTriggerCall;

    public EntityState(Entity entity, AnimParamSO animParam)
    {
        _entity = entity;
        _animParam = animParam;
        _renderer = _entity.GetCompo<EntityRenderer>();
        _animatorTrigger = entity.GetCompo<EntityAnimationTrigger>(); //2
        _dialogueComponent = entity.GetCompo<DialogueComponent>();
    }

    public virtual void Enter()
    {
        _animatorTrigger.OnAnimationEnd += AnimationEndTrigger; //3
        _renderer.SetParam(_animParam, true);
        _isTriggerCall = false;
    }

    public virtual void Update() { }

    public virtual void Exit()
    {
        _renderer.SetParam(_animParam, false);
        _animatorTrigger.OnAnimationEnd -= AnimationEndTrigger; //4
    }

    public virtual void AnimationEndTrigger() =>
        _isTriggerCall = true;
}
