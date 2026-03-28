using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Tape
{
    runaway,
    news,
    Jinglebels
}

public class TapeItem : MonoBehaviour, IPuzzleItem
{
    [SerializeField] private Tape playTape;

    public void Use(ItemContainer handController, RaycastHit hit)
    {
        if (hit.collider.TryGetComponent(out TelevisionObj tv))
        {
            AudioManager.Instance.PlaySound2D("TapeS", 0, false, SoundType.VfX);
            tv.ChangeVideo(playTape);

            handController.ChangeItem(null);
            Destroy(gameObject);
        }
    }
}