using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/ItemList")]
public class ItemSOList : ScriptableObject
{
    public List<ItemSO> items = new List<ItemSO>();
}
