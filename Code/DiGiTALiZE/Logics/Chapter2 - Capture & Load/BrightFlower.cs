using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrightFlower : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject _collider;

    public void FoldFlower()
    {
        _collider.SetActive(false);
        anim.SetBool("Fold", true);
        print("폴드");
    }

    public void ExpandFlower()
    {
        _collider.SetActive(true);
        anim.SetBool("Fold", false);
        print("액스펜드");
    }
}
