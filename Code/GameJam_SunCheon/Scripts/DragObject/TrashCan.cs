using UnityEngine;
using UnityEngine.EventSystems;

public class TrashCan : DragObj
{
    // ✅ 원래 있던 빈 OnDrag 유지 (쓰레기통은 드래그 대상 아님)
    public override void OnDrag(PointerEventData eventData)
    {
    }

    public override void Execute(DragObj obj)
    {
        base.Execute(obj);
        if(obj.TryGetComponent(out Food food))
        {
            if (food.foodEnum == WantFoodEnum.Stew) StageTrigger.Instance.OnRemoveStew();
        }

        // 빵이면 TrayManager에서 제거
        var meta = obj.GetComponent<BreadMeta>();
        if (meta != null)
        {
            TrayManager.Instance
                      .GetTray(meta.originTray)
                      .Remove(meta.foodEnum);

            // (옵션) 주방 미니빵도 함께 지우려면 BreadMeta에 kitchenIndex 추가한 뒤:
            // StageTrigger.Instance.Remove(meta.kitchenIndex);
        }

        // 실제 오브젝트 삭제
        Destroy(obj.gameObject);
    }
}
