using System.Collections.Generic;
using UnityEngine;

public class WeaponHolder : MonoBehaviour, IEntityComponent
{
    [Header("Mount Points")]
    [SerializeField] private Transform headMount;
    [SerializeField] private Transform rightHandMount;

    private Entity _owner;

    // 현재 장착중인 장비들의 시각적 모델과 논리적 효과를 추적합니다.
    private Dictionary<EquipSlot, ItemVisual> _equippedVisuals = new();
    private Dictionary<EquipSlot, List<IEquipItem>> _equippedEffects = new();

    public void Initialize(Entity entity)
    {
        _owner = entity;
    }
    
    public bool GetHoldItem()=> _equippedVisuals.Count >= 0;

    public void Equip(ItemSO equipmentData)
    {
        if (equipmentData == null || equipmentData.visualPrefab == null) return;

        Unequip(equipmentData.slotType);

        Transform mountPoint = GetMountPoint(equipmentData.slotType);
        ItemVisual newVisual = Instantiate(equipmentData.visualPrefab, mountPoint);

        newVisual.transform.localPosition = equipmentData.equipPosition;
        newVisual.transform.localRotation = Quaternion.Euler(equipmentData.equipRotation);
        newVisual.transform.localScale = equipmentData.equipScale; // 여기서 0.004 사이즈가 됨!

        newVisual.SetupAsEquipped();

        _equippedVisuals[equipmentData.slotType] = newVisual;

        List<IEquipItem> newEffects = equipmentData.CreateAllEffects();
        foreach (var effect in newEffects)
        {
            effect.OnEquip(_owner);
        }
        _equippedEffects[equipmentData.slotType] = newEffects;

        Debug.Log($"[{equipmentData.weaponName}] 장착");
    }

    public void Unequip(EquipSlot slot)
    {
        if (_equippedEffects.TryGetValue(slot, out var effects))
        {
            foreach (var effect in effects) effect.OnUnequip();
            _equippedEffects.Remove(slot);
        }

        if (_equippedVisuals.TryGetValue(slot, out var visual))
        {
            visual.Drop(visual.GetWeaponData().dropScale);
            _equippedVisuals.Remove(slot);
        }
    }
    public void DropAllItems()
    {
        List<EquipSlot> activeSlots = new List<EquipSlot>(_equippedVisuals.Keys);

        foreach (EquipSlot slot in activeSlots)
        {
            Unequip(slot); 
        }

        _equippedVisuals.Clear();
        _equippedEffects.Clear();
    }
    private Transform GetMountPoint(EquipSlot slot)
    {
        return slot switch
        {
            EquipSlot.Head => headMount,
            EquipSlot.RightHand => rightHandMount,
            _ => rightHandMount
        };
    }
}