using System.Collections.Generic;
using UnityEngine;

public class WeaponHolder : MonoBehaviour, IEntityComponent
{
    [SerializeField] private Transform holderTrm;
    [SerializeField] private Transform headHolder;
    private List<AbstractWeapon> weapons = new();
    public WeaponDataSO CurWeapon { get; private set; }

    public void Initialize(Entity entity)
    {
    }

    public void EquipWeapon(WeaponDataSO weaponData,bool isHead = false)
    {
        if (weaponData == null || weaponData.weapon == null) return;
        CurWeapon = weaponData;
        Transform target = isHead ? headHolder : holderTrm;
        AbstractWeapon newWeapon = Instantiate(weaponData.weapon, target);

        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;

        weapons.Add(newWeapon);

        Debug.Log($"[{weaponData.weaponName}] 장착 완료!");
    }

    public void DropWeapons()
    {
        foreach (AbstractWeapon weapon in weapons)
        {
            weapon.Drop();
        }
        weapons.Clear(); // 버린 후에는 리스트도 비워줘야 합니다!
    }
}