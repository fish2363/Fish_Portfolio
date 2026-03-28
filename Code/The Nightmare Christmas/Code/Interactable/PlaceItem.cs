using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceItem : MonoBehaviour, IInteractable
{
    public ItemSO thisItem;

    public void Interact()
    {
        AudioManager.Instance.PlaySound2D("Pick", 0, false, SoundType.VfX);
        print("Å‰µæ");
        Item item = FindAnyObjectByType<Item>();
        item.currentItem = thisItem;
        item.ChangeItem(false);
        Destroy(gameObject);
    }
}
