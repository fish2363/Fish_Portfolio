using UnityEngine;
using UnityEngine.EventSystems;

public class Customer : DragObj
{
    public override void OnBeginDrag(PointerEventData eventData)
    {
    }
    public override void OnDrag(PointerEventData eventData)
    {
    }

    public override void Execute(DragObj obj)
    {
        base.Execute(obj);
        GetComponentInParent<Human>().OnServing(WantFoodEnum.Juice);
    }
}
