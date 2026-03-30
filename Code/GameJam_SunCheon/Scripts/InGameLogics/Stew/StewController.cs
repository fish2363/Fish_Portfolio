using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StewController : AdvancedDragObj
{
    StewManager stewManager;

    [SerializeField] Image selfImg;
    [SerializeField] Sprite[] stateSprites; //none ~ 
    [SerializeField] public GameObject smokeObj;


    private StewState _curState;
    public StewState CurState 
    { 
        get { return _curState; } 
        set 
        { 
            _curState = value; 
            SetSpritesByStates(); 
            
        } 
    }

    public void SetManager(StewManager mng)
    {
        stewManager = mng;
    }

    public override void Execute(DragObj obj)
    {
        base.Execute(obj);
    }

    public bool CheckOrder(StewState state)
    {
        if (state == CurState + 1)
        {
            CurState++;
            return true;
        }
        return false;
    }

    private void SetSpritesByStates()
    {
        switch (CurState)
        {
            case StewState.None:
                selfImg.sprite = stateSprites[0];
                break;
            case StewState.Set:
                selfImg.sprite = stateSprites[1];
                break;
            case StewState.Stock:
                selfImg.sprite = stateSprites[2];
                break;
            case StewState.Fish:
                selfImg.sprite = stateSprites[3];
                break;
            case StewState.Radish:
                selfImg.sprite = stateSprites[4];
                break;
            case StewState.Spices:
                selfImg.sprite = stateSprites[5];
                break;
            case StewState.Welsh:
                selfImg.sprite = stateSprites[6];
                if (stewManager.CoolCoroutine != null)
                {
                    StopCoroutine(stewManager.CoolCoroutine);
                    stewManager.CoolCoroutine = null;
                }
                stewManager.CoolCoroutine = StartCoroutine(CoolingCoroutine());
                break;
            case StewState.Ready:
                smokeObj.SetActive(true);
                FindAnyObjectByType<JJangDDungPlate>().CreateStew();
                break;
            case StewState.Cold:
                smokeObj.SetActive(false);
                break;
        }
    }

    

    private IEnumerator CoolingCoroutine()
    {
        yield return new WaitForSeconds(1);
        CurState++;
        yield return new WaitForSeconds(10f);
        CurState++;
    }
}