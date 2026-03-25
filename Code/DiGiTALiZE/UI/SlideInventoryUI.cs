using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SlideInventoryUI : MonoBehaviour
{
    private HorizontalLayoutGroup layoutGroup;
    private bool isOpenedInventory = false;
    private bool ismoveInGallury;

    [SerializeField]
    private float maxArea;

    [Header("돌게 할 오브젝트")]
    [SerializeField]
    private Image rotateObject;
    public Ease spinEase;

    private void Awake()
    {
        layoutGroup = GetComponent<HorizontalLayoutGroup>();
    }

    private void Start()
    {
        isOpenedInventory = true;
        OnClickButton();
    }

    public void OnClickButton()
    {
        RotateAnimation();
        if (!isOpenedInventory)
        {
            isOpenedInventory = true;
            StartCoroutine(SpacingChangeUI(maxArea, 0));
        }
        else
        {
            isOpenedInventory = false;
            StartCoroutine(SpacingChangeUI(-100f , 135));
        }
    }

    private IEnumerator SpacingChangeUI(float max,int move)
    {

        for (int i = 0; i < gameObject.GetComponentsInChildren<Image>().Length; i++)
            gameObject.GetComponentsInChildren<Image>()[i].DOKill();

        yield return DOTween.Sequence().Append(DOTween.To(() => layoutGroup.spacing, x => layoutGroup.spacing = x, max, 1));
            //.Append(DOTween.To(() => layoutGroup.padding.left, x => layoutGroup.padding.left = x, move, 1));
        
        if (max > 0)
        {
            //gameObject.transform.DOMoveX(49, 1f);
            for (int i=0; i<gameObject.GetComponentsInChildren<Image>().Length; i++)
            {
                gameObject.GetComponentsInChildren<Image>()[i].DOFade(1, 0.3f);
            }
        }
        else
        {
            //gameObject.transform.DOMoveX(-49,1f);
            ismoveInGallury = true;
            for (int i = 0; i < gameObject.GetComponentsInChildren<Image>().Length; i++)
            {
                gameObject.GetComponentsInChildren<Image>()[i].DOFade(0, 2f);
                gameObject.GetComponentsInChildren<Image>()[i].rectTransform.DOMoveX(-50, 0.3f);
            }
        }
    }

    private void RotateAnimation()
    {
        rotateObject.rectTransform.DORotate(new Vector3(0, 0, 360), 0.2f, RotateMode.FastBeyond360).SetEase(spinEase);
    }
}
