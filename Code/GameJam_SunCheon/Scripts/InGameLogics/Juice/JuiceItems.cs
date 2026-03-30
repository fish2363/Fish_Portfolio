using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JuiceItems : AdvancedDragObj
{
    [SerializeField] JuiceState ingState = JuiceState.None;

    //[Header("Juice Parts")]
    //[SerializeField] Image selfImg;

    public override void OnBeginDrag(PointerEventData e)
    {
        base.OnBeginDrag(e);
    }

    public override void OnEndDrag(PointerEventData e)
    {
        base.OnEndDrag(e);
    }

    public override void EndDragEvent(DragObj target, ref bool executed)
    {
        base.EndDragEvent(target, ref executed);
        if (target is JuiceController ctrl)
        {
            executed = ctrl.CheckOrder(ingState);
        }
        executed = false;
    }

    public override void Execute(DragObj obj)
    {
        base.Execute(obj);
        if (obj is JuiceController ctrl)
        {
            if (ingState == JuiceState.Submit)
            {
                ctrl.CurState = JuiceState.None;
                StageTrigger.Instance.CupPut(JuiceState.None);
                ctrl.juiceMng.AddJuiceCount(1);  // ✅ 노티파이로 값 변경
                UIManager.Get<UIPopupJuice>().ActiveWaitingCup(ctrl.juiceMng.JuiceCount);
            }
        }
    }
}