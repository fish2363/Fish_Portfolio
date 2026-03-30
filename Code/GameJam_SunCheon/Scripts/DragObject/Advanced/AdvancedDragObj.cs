using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class AdvancedDragObj : DragObj, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("생성/이동 대상")]
    [SerializeField] protected GameObject movePrefab;
    [SerializeField] protected Canvas canvas;
    [SerializeField] protected Camera uiCamera;

    [Header("이동 제한(옵션)")]
    [SerializeField] protected RectTransform movementBounds;

    [Header("설정")]
    [SerializeField] protected float advancedReturnDuration = 0.25f;

    [Header("드래그 제어")]
    [SerializeField] public bool dragEnabled = true;
    bool blockedDragActive = false;
    public void SetDragEnabled(bool on) 
    { 
        dragEnabled = on; 
        if (on) blockedDragActive = false;
    }

    protected RectTransform current, spawned, selfRt;
    protected Vector3 startLocalPos;
    protected Transform startParent;
    protected int startSiblingIndex;
    protected Image currentImg;

    protected enum CompletionMode { Consume, ReturnOnly, ReturnThenDestroySpawned }
    protected CompletionMode completion = CompletionMode.Consume;

    protected override void Awake()
    {
        selfRt = transform as RectTransform;
        if (!canvas) canvas = GetComponentInParent<Canvas>();
        if (!uiCamera && canvas) uiCamera = canvas.worldCamera;
    }

    public void RequireConsume() => completion = CompletionMode.Consume;
    public void RequireReturn(bool destroySpawned = true)
        => completion = destroySpawned ? CompletionMode.ReturnThenDestroySpawned : CompletionMode.ReturnOnly;

    public override void OnBeginDrag(PointerEventData e)
    {
        if (!dragEnabled) { blockedDragActive = true; return; }

        if (movePrefab)
        {
            var go = Instantiate(movePrefab, canvas ? canvas.transform : transform.parent);
            spawned = go.GetComponent<RectTransform>();
            current = spawned;
            current.localScale = Vector3.one;
        }
        else current = selfRt;

        startParent = current.parent;
        startSiblingIndex = current.GetSiblingIndex();
        startLocalPos = current.localPosition;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(startParent as RectTransform, e.position, uiCamera, out var lp))
            current.localPosition = lp;

        currentImg = current.GetComponent<Image>();
        if (currentImg) currentImg.raycastTarget = false;

        completion = CompletionMode.Consume; // 초기화
        StartDragEvent(current.gameObject);
    }

    public override void OnDrag(PointerEventData e)
    {
        if (blockedDragActive || !current || !startParent) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(startParent as RectTransform, e.position, uiCamera, out var lp))
        {
            var target = lp;

            if (movementBounds)
            {
                var parentRT = startParent as RectTransform;
                if (parentRT)
                {
                    Vector3[] world = new Vector3[4];
                    movementBounds.GetWorldCorners(world);
                    Vector2 bl = (Vector2)parentRT.InverseTransformPoint(world[0]);
                    Vector2 tl = (Vector2)parentRT.InverseTransformPoint(world[1]);
                    Vector2 tr = (Vector2)parentRT.InverseTransformPoint(world[2]);
                    Vector2 br = (Vector2)parentRT.InverseTransformPoint(world[3]);
                    float minX = Mathf.Min(bl.x, tl.x, tr.x, br.x);
                    float maxX = Mathf.Max(bl.x, tl.x, tr.x, br.x);
                    float minY = Mathf.Min(bl.y, tl.y, tr.y, br.y);
                    float maxY = Mathf.Max(bl.y, tl.y, tr.y, br.y);
                    target = new Vector2(Mathf.Clamp(target.x, minX, maxX), Mathf.Clamp(target.y, minY, maxY));
                }
            }

            current.localPosition = target;
        }
    }

    public override void OnEndDrag(PointerEventData e)
    {
        if (blockedDragActive)
        {
            blockedDragActive = false;
            CancelDragEvent();
            return;
        }

        if (currentImg) currentImg.raycastTarget = true;

        bool hitTarget = false;
        var ped = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        var hits = new List<RaycastResult>(8);
        EventSystem.current.RaycastAll(ped, hits);

        foreach (var hit in hits)
        {
            var go = hit.gameObject;
            if (!go || go == e.pointerDrag || go == (spawned ? spawned.gameObject : null)) continue;

            if (go.TryGetComponent<DragObj>(out var target))
            {
                hitTarget = true;
                EndDragEvent(target, ref hitTarget);
                break;
            }
        }

        if (!hitTarget)
        {
            ReturnToOrigin(() =>
            {
                if (spawned) Destroy(spawned.gameObject);
                CleanupAfterEnd();
                CancelDragEvent();
            });
        }
        else
        {
            switch (completion)
            {
                case CompletionMode.Consume:
                    if (spawned) Destroy(spawned.gameObject);
                    CleanupAfterEnd();
                    break;
                case CompletionMode.ReturnOnly:
                    ReturnToOrigin(CleanupAfterEnd);
                    break;
                case CompletionMode.ReturnThenDestroySpawned:
                    ReturnToOrigin(() =>
                    {
                        if (spawned) Destroy(spawned.gameObject);
                        CleanupAfterEnd();
                    });
                    break;
            }
        }
    }

    protected void ReturnToOrigin(TweenCallback onComplete)
    {
        if (!current) { onComplete?.Invoke(); return; }
        current.DOKill(false);
        current.DOLocalMove(startLocalPos, advancedReturnDuration)
               .SetEase(Ease.OutCubic)
               .OnComplete(onComplete);
    }

    protected void CleanupAfterEnd()
    {
        spawned = null;
        current = null;
    }

    public virtual void StartDragEvent(GameObject moveTarget = null) { }

    public virtual void EndDragEvent(DragObj target, ref bool executed)
    {
        executed = true;
        // 기본은 '복귀 후 파괴'로. 타깃이 성공 소비 시 RequireConsume()으로 덮어씀.
        completion = CompletionMode.ReturnThenDestroySpawned;
        target.Execute(this);
    }

    public virtual void CancelDragEvent() { }
}
