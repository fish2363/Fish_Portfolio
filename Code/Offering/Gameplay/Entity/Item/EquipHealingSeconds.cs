using System;
using UnityEngine;

[Serializable]
public class EquipHealingSecondsDef : IEquipItemDef
{
    [Header("n초마다 실행")]
    public float tick;
    [Header("n초당 몇 회복")]
    public float amount;
    [Header("%인가요?")]
    public bool isPercentage;

    public IEquipItem CreateLogic() => new EquipHealingSeconds(this);
}


public class EquipHealingSeconds : IEquipItem, IEquipUpdateItem
{
    private readonly EquipHealingSecondsDef _def;
    private EntityHealthCompo _entityHealth;

    private float _timer;
    
    public EquipHealingSeconds(EquipHealingSecondsDef def) => _def = def;


    public void OnEquip(Entity owner)
    {
        _entityHealth = owner.GetCompo<EntityHealthCompo>();
        _timer = 0f;
    }

    public void OnUnequip()
    {
        _entityHealth = null;
    }

    public void OnUpdate(float deltaTime)
    {
        if (_entityHealth == null) return;

        _timer += deltaTime;

        if (_timer >= _def.tick)
        {
            InstantHealData healData = new InstantHealData()
            {
                amount = _def.amount,
                isPercentage = _def.isPercentage
            };
            _entityHealth.ApplyHeal(healData);

            _timer -= 0f;
        }
    }
}
