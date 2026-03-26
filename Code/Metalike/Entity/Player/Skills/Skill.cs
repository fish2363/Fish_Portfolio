using System;
using UnityEngine;

public delegate void CooldownInfo(float current, float duration);

public abstract class Skill : ExtendedMono
{
    [field: SerializeField] public SkillDataSO Data { get; private set; }

    protected float _cooldownTimer = 0f;
    protected Entity _owner;
    protected SkillComponent _skillComponent;

    public bool IsCooldown => _cooldownTimer > 0f;
    public bool IsWait { get; set; }
    public event CooldownInfo OnCooldownEvent;

    public float CooldownCurrent => _cooldownTimer;

    public float CooldownDuration
    {
        get
        {
            if (Data == null) return 0f;
            if (_skillComponent == null) return Data.cooldown;
            return _skillComponent.GetCooldownDuration(Data.cooldown);
        }
    }

    public virtual void InitializeSkill(Entity owner, SkillComponent skillComponent)
    {
        _owner = owner;
        _skillComponent = skillComponent;
    }

    protected virtual void Update()
    {
        if (_cooldownTimer > 0f)
        {
            _cooldownTimer -= Time.deltaTime;
            if (_cooldownTimer < 0f)
            {
                _cooldownTimer = 0f;
            }
            OnCooldownEvent?.Invoke(_cooldownTimer, CooldownDuration);
        }
    }

    public virtual void UseSkill()
    {
        if (_cooldownTimer > 0) return;
        _cooldownTimer = CooldownDuration;
        _skillComponent.OnSkillEvent?.Invoke();
    }

    protected virtual void OnDisable()
    {
        
    }
}