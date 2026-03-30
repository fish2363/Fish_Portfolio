// JuiceTrashCan.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class JuiceTrashCan : DragObj
{
    public override void OnDrag(PointerEventData eventData) { }

    public override void Execute(DragObj obj)
    {
        base.Execute(obj);

        // 드롭 소스에 복귀 지시: 원위치로 이동 후 스폰본이면 파괴
        if (obj is AdvancedDragObj src)
            src.RequireReturn(destroySpawned: true);

        // 주스 상태 리셋
        if (obj.TryGetComponent<JuiceController>(out var jc))
            jc.CurState = JuiceState.None;
    }
}