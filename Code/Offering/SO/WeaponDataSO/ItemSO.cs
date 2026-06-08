using System.Collections.Generic;
using UnityEngine;

public interface IEquipItemDef
{
    IEquipItem CreateLogic();
}

public interface IEquipItem
{
    void OnEquip(Entity owner);
    void OnUnequip();
}

public interface IEquipUpdateItem
{
    void OnUpdate(float deltaTime);
}
public interface IEquipAttackModifierItem
{
    void OnAttack(Player target);
}

public enum EquipSlot { Head, RightHand, LeftHand }

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "ScriptableObjects/ItemSO")]
public class ItemSO : ScriptableObject
{
    public string weaponName;
    [TextArea] public string weaponDescription;

    public ItemVisual visualPrefab;
    public EquipSlot slotType;

    [Header("착용 시 오프셋")]
    public Vector3 equipPosition = Vector3.zero;
    public Vector3 equipRotation = Vector3.zero;
    [Tooltip("착용했을 때의 크기")]
    public Vector3 equipScale = Vector3.one;

    [Header("바닥에 떨어졌을 때 크기")]
    public Vector3 dropScale = Vector3.one;

    [SerializeReference]
    public List<IEquipItemDef> effectDefinitions = new();

    public List<IEquipItem> CreateAllEffects()
    {
        List<IEquipItem> effects = new();
        foreach (var def in effectDefinitions)
        {
            if (def != null) effects.Add(def.CreateLogic());
        }
        return effects;
    }
}