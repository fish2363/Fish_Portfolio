using System;
using UnityEngine;

public class EntityAnimatorTrigger : MonoBehaviour, IEntityComponent
{
    public Action OnAnimationEndTrigger;
    public Action OnAttackVFXTrigger;
    public Action<bool> OnDamageToggleTrigger;
    public Action OnDeadTrigger;
    public Action AttackStartTrigger;
    public event Action OnDamageCastTrigger;

    protected Entity _entity;

    public virtual void Initialize(Entity _entity)
    {
        this._entity = _entity;
    }

    private void Dead()
    {
        OnDeadTrigger?.Invoke();
    }
    private void AnimationEnd()
    {
        OnAnimationEndTrigger?.Invoke();
    }

    private void AttackStart()
    {
        AttackStartTrigger?.Invoke();
    }
    private void PlayAttackVFX()
    {
        OnAttackVFXTrigger?.Invoke();
    }

    private void StartDamageCast()
    {
        OnDamageToggleTrigger?.Invoke(true);
    }
    private void DamageCast()
    {
        OnDamageCastTrigger?.Invoke();
    }

    private void StopDamageCast()
    {
        OnDamageToggleTrigger?.Invoke(false);
    }
}