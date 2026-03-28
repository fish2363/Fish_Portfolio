using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Item")]
public class ItemSO : ScriptableObject
{
    public AudioClip useSound;
    public GameObject itemPrefab;
    public GameObject itemPlacePrefab;
    public string itemName;
    public bool isPuzzleItem;
    public bool isProp;
}
