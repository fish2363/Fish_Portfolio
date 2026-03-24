using System;
using UnityEngine;

[Serializable]
public class HealthConditionResistModuleDef : IModuleLogicDef
{
    [Header("피격 데미지 감소율"),Range(1, 100)] public float reducePercent = 50f;
    [Header("이 값보다 이상일때만 보호"),Range(0.1f, 10.0f)] public float canReduce = 0.8f;
    public IModuleLogic CreateLogic() => new HealthConditionResistModule(this);
}

public class HealthConditionResistModule : IModuleLogic, IBeforeDamageModifier
{
    private readonly HealthConditionResistModuleDef _def;
    private PlayerHealthCompo _entityHealth;
    private Player _player;

    public HealthConditionResistModule(HealthConditionResistModuleDef def) => _def = def;

    public void OnEquip(Entity owner)
    {
        _player = owner as Player;
        _entityHealth = owner.GetCompo<PlayerHealthCompo>();
    }
    public void ModuleUpdate(float deltaTime) { }
    public void OnUnequip() { }

    public void OnBeforeDamage(ref DamageData data, Entity dealer)
    {
        if (_entityHealth == null || _player == null) return;
        float cur = _entityHealth.GetCurHealth(_player.CurrentCharacter);
        float max = _entityHealth.GetMaxValue(_player.CurrentCharacter);

        if (cur >= max * _def.canReduce) data.damage *= 1f - (_def.reducePercent / 100f);
    }
}
