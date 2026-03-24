using System;
using UnityEngine;

[Serializable]
public class SturdyDef : IModuleLogicDef
{
    [Header("버티기 설정")]
    [Tooltip("치명상을 입었을 때 무적(버티기)이 유지되는 시간")]
    public float activeDuration = 2f;

    [Tooltip("버티기 효과 발동 후 다시 발동되기까지의 쿨타임")]
    public float cooldownTime = 60f;

    public IModuleLogic CreateLogic() => new SturdyModule(this);
}

public class SturdyModule : IModuleLogic, IBeforeDamageModifier
{
    private readonly SturdyDef _def;
    private Player _player;
    private PlayerHealthCompo _health;

    private float _activeTimer;
    private float _cooldownTimer;

    private bool IsActive => _activeTimer > 0f;
    private bool IsCooldown => _cooldownTimer > 0f;

    public SturdyModule(SturdyDef def) => _def = def;

    public void OnEquip(Entity owner)
    {
        _player = owner as Player;
        _health = owner.GetCompo<PlayerHealthCompo>();

        _activeTimer = 0f;
        _cooldownTimer = 0f;
    }

    public void ModuleUpdate(float deltaTime)
    {
        if (_activeTimer > 0f)
        {
            _activeTimer -= deltaTime;
            if (_activeTimer <= 0f)
            {
                _activeTimer = 0f;
                _cooldownTimer = _def.cooldownTime;
            }
        }
        else if (_cooldownTimer > 0f)
        {
            _cooldownTimer -= deltaTime;
        }
    }

    public void OnUnequip()
    {
        _activeTimer = 0f;
        _cooldownTimer = 0f;
    }

    public void OnBeforeDamage(ref DamageData data, Entity dealer)
    {
        if (data.damage <= 0f || data.damageType == DamageType.DOT) return;

        float curHealth = _health.GetCurHealth(_player.CurrentCharacter);

        if (data.damage < curHealth) return;
        if (!IsActive && IsCooldown) return;


        data.damage = Mathf.Max(curHealth - 1f, 0f);

        if (!IsActive)
            _activeTimer = _def.activeDuration;
    }
}