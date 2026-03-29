using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class Menubutton : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    public int index; // 0: Top, 1: Middle, 2: Bottom
    public ButtonAnimatorController controller;

    public void OnPointerEnter(PointerEventData eventData)
    {
        controller.OnButtonHover(index);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        controller.OnButtonExit();
    }
}
