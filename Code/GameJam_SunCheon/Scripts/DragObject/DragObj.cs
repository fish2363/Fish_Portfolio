using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public abstract class DragObj : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler,IPointerEnterHandler,IPointerExitHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;

    protected Vector3 _startLocalPos;
    private Image _dragImage;

    public bool isFood    { get; set; }
    public bool isJangBan { get; set; }
    public bool isOhterObj { get; set; }
    public bool isPutNotOhterObj { get; set; }
    public bool AcceptDrop = false;

    private bool isDrag;

    [SerializeField] private float returnDuration = 0.25f;

    private Coroutine _coroutine;

    protected virtual void Awake()
    {
        
    }

    protected virtual void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        Debug.Log(canvas == null);
        Debug.Log(rectTransform == null);
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        canvas = GetComponentInParent<Canvas>();

        _startLocalPos = rectTransform.localPosition;

        if (!isFood && !isJangBan)
            isOhterObj = true;

        isDrag = true;
        InGameUIManager.Instance.SetCursor(CursorState.Hold);
        _dragImage = GetComponent<Image>();
        if (_dragImage != null)
            _dragImage.raycastTarget = false;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (rectTransform == null || canvas == null) return;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out localPoint))
        {
            rectTransform.localPosition = localPoint;
        }
    }
    public virtual void Execute(DragObj obj)
    {
        Debug.Log($"{name} Execute called by {obj.name}");
    }
    
    public virtual void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("EndDrag 호출");
        InGameUIManager.Instance.SetCursor(CursorState.Default);
        isDrag = false;

        // 드래그한 객체 Raycast Target 복원 (항상)
        if (_dragImage != null)
            _dragImage.raycastTarget = true;

        bool executed = false;
        bool isContain = false;

        // 마우스 위치 UI Raycast
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            GameObject go = result.gameObject;

            // 자기 자신 제외
            if (go == eventData.pointerDrag) continue;

            Debug.Log(go.name);
            if (!go.TryGetComponent<DragObj>(out var targetDrag))
                break;

            var draggedObj = eventData.pointerDrag.GetComponent<DragObj>();

            // 음식일 경우
            if (isFood)
            {
                if (go.TryGetComponent<JangBan>(out var jang))
                {
                    if (!jang.IsContain())
                    {
                        targetDrag.Execute(draggedObj);
                        executed = true;
                    }
                    else
                    {
                        isContain = true;
                    }
                }
                else if (go.TryGetComponent<TrashCan>(out _))
                {
                    targetDrag.Execute(draggedObj);
                    executed = true;
                }
                else if (go.TryGetComponent<BreadPlate>(out _))
                {
                    targetDrag.Execute(draggedObj);
                    executed = true;
                }
                else
                {
                    InGameUIManager.Instance.ShowFloatingText("'쟁반'에만 배치할 수 있습니다.", Color.red);
                }
            }
            else if(isJangBan)
            {
                Debug.Log(go.TryGetComponent<Human>(out var _));
                if (go.TryGetComponent<Human>(out var _))
                {
                    targetDrag.Execute(draggedObj);
                    executed = true;
                }
                else if (go.TryGetComponent<TrashCan>(out _))
                {
                    targetDrag.Execute(draggedObj);
                    executed = true;
                }
            }
            // 음식이 아닌 경우
            else if((!targetDrag.isPutNotOhterObj || !isOhterObj) && targetDrag.AcceptDrop)
            {
                targetDrag.Execute(draggedObj);
                executed = true;
            }
            break; // 첫 번째만 처리
        }

        // 실행되지 않았으면 원위치로 이동 + 경고 메시지
        if (!executed)
        {
            if (isContain)
            {
                InGameUIManager.Instance.ShowFloatingText("이미 음식이 존재합니다.", Color.red);
            }
            else if (isFood)
            {
                InGameUIManager.Instance.ShowFloatingText("'쟁반'에만 배치할 수 있습니다.", Color.red);
            }
            else if (isJangBan)
            {
                InGameUIManager.Instance.ShowFloatingText("손님에게 직접 전달하세요.", Color.red);
            }

            rectTransform
                .DOLocalMove(_startLocalPos, returnDuration)
                .SetEase(Ease.OutCubic);
        }

        _dragImage = null;
    }
    public void ResetPosition()
    {
        rectTransform.DOLocalMove(_startLocalPos, returnDuration).SetEase(Ease.OutCubic);
    }
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        InGameUIManager.Instance.SetCursor(CursorState.Hover);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (isDrag) return;
        InGameUIManager.Instance.SetCursor(CursorState.Default);
    }
}
