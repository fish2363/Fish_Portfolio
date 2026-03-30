using System;
using UnityEngine.EventSystems;

public class StewTrashCan : DragObj
{
    public override void OnDrag(PointerEventData eventData)
    {
    }
    public override void Execute(DragObj obj)
    {
        base.Execute(obj);

        // 드롭 소스에 복귀 지시: 원위치로 이동 후 스폰본이면 파괴
        if (obj is AdvancedDragObj src)
            src.RequireReturn(destroySpawned: true);


        try
        {
            obj.GetComponent<StewController>().CurState = StewState.None;
        }
        catch (Exception) { }
    }
}
