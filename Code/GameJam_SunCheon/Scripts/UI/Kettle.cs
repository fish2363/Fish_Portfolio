using UnityEngine;
using UnityEngine.EventSystems;

public class Kettle : DragObj
{
    // 드래그 전에 원래 회전값 저장
    private Quaternion _originalRot;

    protected override void Awake()
    {
        base.Awake();
        _originalRot = transform.rotation;
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        // 오른쪽으로 살짝 기울이기 (Z축 +10도)
        transform.rotation = _originalRot * Quaternion.Euler(0f, 0f, -10f);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        // 필요하면 드래그 중 추가 로직
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        // 드래그 끝나면 원래 회전으로 복원
        transform.rotation = _originalRot;
    }
}
