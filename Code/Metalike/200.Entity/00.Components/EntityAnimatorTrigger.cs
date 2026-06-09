using System;
using UnityEngine;

public class EntityAnimatorTrigger : MonoBehaviour, IEntityComponent
{
    public Action OnAnimationEndTrigger;
    public Action OnAttackVFXTrigger;
    public Action<bool> OnManualRotationTrigger;
    public Action OnDamageCastTrigger;
    public Action<bool> OnDamageToggleTrigger;
    public Action OnCastSkillTrigger;
    public Action OnDeadTrigger;
    public Action AttackStartTrigger;

    protected Entity _entity;

    public virtual void Initialize(Entity _entity)
    {
        this._entity = _entity;
    }

    // ===== Animation Events =====

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

    protected void StartManualRotation()
    {
        OnManualRotationTrigger?.Invoke(true);
    }

    protected void StopManualRotation()
    {
        OnManualRotationTrigger?.Invoke(false);
    }

    protected void DamageCast()
    {
        OnDamageCastTrigger?.Invoke();
    }

    protected void StartDamageCast()
    {
        OnDamageToggleTrigger?.Invoke(true);
    }

    protected void StopDamageCast()
    {
        OnDamageToggleTrigger?.Invoke(false);
    }

    protected void CastSkill()
    {
        OnCastSkillTrigger?.Invoke();
    }
}