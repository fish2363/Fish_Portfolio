using System;
using UnityEngine;

[Serializable]
public class EquipAttackChangeColorModifiereDef : IEquipItemDef
{
    [Header("지속시간"),Range(1,20)] public float duration = 1f;

    public IEquipItem CreateLogic() => new EquipAttackChangeColorModifier(this);
}

public class EquipAttackChangeColorModifier : IEquipItem,IEquipAttackModifierItem
{
    private readonly EquipAttackChangeColorModifiereDef _def;
    public EquipAttackChangeColorModifier(EquipAttackChangeColorModifiereDef def) => _def = def;

    public void OnEquip(Entity owner)
    {
    }

    public void OnAttack(Player target)
    {

    }

    public void OnUnequip()
    {
    }
}
