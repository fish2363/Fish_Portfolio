using UnityEngine;
using UnityEngine.EventSystems;

public class Food : DragObj
{
    public WantFoodEnum foodEnum;

    private void Start()
    {
        isFood = true;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
    }
}
