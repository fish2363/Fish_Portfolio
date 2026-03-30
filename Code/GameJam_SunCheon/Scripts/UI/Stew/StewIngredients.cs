using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StewIngredients : AdvancedDragObj
{
    [SerializeField] StewState ingState = StewState.None;
    [Header("For Pot Only")]
    [SerializeField] Image potImg;
    [Header("Spawn (UI Only)")]
    [SerializeField] private GameObject spawnPrefab;

    public override void OnBeginDrag(PointerEventData e)
    {
        base.OnBeginDrag(e);
    }

    public override void OnEndDrag(PointerEventData e)
    {
        base.OnEndDrag(e);
        //SpawnAtMouseUI(e);
    }

    private void SpawnAtMouseUI(PointerEventData e)
    {
        Transform targetParent = transform.parent;          // 부모의 부모
        
        Transform go = Instantiate(spawnPrefab, Input.mousePosition,Quaternion.identity).transform;
        go.SetParent(targetParent);
    }

    public override void EndDragEvent(DragObj target, ref bool executed)
    {
        base.EndDragEvent(target, ref executed);
        if (target is StewController sc)
        {
            executed = sc.CheckOrder(ingState);
        }
        executed = false;
    }
}