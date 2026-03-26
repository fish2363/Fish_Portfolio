using System;
using UnityEngine;

public class EntityAnimatorTrigger : MonoBehaviour, IEntityComponent
{
    public Action OnAnimationEndTrigger;
    public Action OnAttackVFXTrigger;
    public Action<bool> OnDamageToggleTrigger;
    public Action OnDeadTrigger;
    public Action AttackStartTrigger;

    protected Entity _entity;

    public virtual void Initialize(Entity _entity)
    {
        this._entity = _entity;
    }

    protected void Dead()
    {
        OnDeadTrigger?.Invoke();
    }
    protected void AnimationEnd()
    {
        OnAnimationEndTrigger?.Invoke();
    }

    protected void AttackStart()
    {
        AttackStartTrigger?.Invoke();
    }
    protected void PlayAttackVFX()
    {
        OnAttackVFXTrigger?.Invoke();
    }

    protected void StartDamageCast()
    {
        OnDamageToggleTrigger?.Invoke(true);
    }

    protected void StopDamageCast()
    {
        OnDamageToggleTrigger?.Invoke(false);
    }
}