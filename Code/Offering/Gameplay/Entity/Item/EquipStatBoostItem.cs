using System;
using UnityEngine;

[Serializable]
public class EquipStatBoostItemDef : IEquipItemDef
{
    [Range(0f, 1f)] public float cooldownReducePercent = 0.2f;
    public StatSO statSO;

    public IEquipItem CreateLogic() => new EquipStatBoostItem(this);
}

public class EquipStatBoostItem : IEquipItem
{
    private readonly EquipStatBoostItemDef _def;

    private Entity _owner;
    private EntityStatCompo _statCompo;


    public EquipStatBoostItem(EquipStatBoostItemDef def) => _def = def;

    public void OnEquip(Entity owner)
    {
        _owner = owner;
        _statCompo = owner.GetCompo<EntityStatCompo>();

        float delta = _statCompo.GetStat(_def.statSO).Value * _def.cooldownReducePercent;

        _statCompo.AddModifier(_def.statSO,this, delta);
    }

    public void OnUnequip()
    {
        _statCompo.RemoveModifier(_def.statSO, this);
    }
}
