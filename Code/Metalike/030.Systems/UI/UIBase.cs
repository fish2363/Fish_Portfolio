using UnityEngine;
using DG.Tweening;
using System;

[RequireComponent(typeof(CanvasGroup))]
public class UIBase : MonoBehaviour
{
    public EUILayer uiLayer = EUILayer.Popup;
    public bool isDestroyAtClosed = false;

    public bool canCloseByEsc = true;

    protected CanvasGroup canvasGroup;

    public Action<object[]> onOpened;
    public Action<object[]> onClosed;

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }
    protected virtual void RegisterCursor()
    {
        if (CursorManager.Instance != null)
            CursorManager.Instance.RegisterUI(this);
    }

    protected virtual void UnregisterCursor()
    {
        if (CursorManager.Instance != null)
            CursorManager.Instance.UnRegisterUI(this);
    }
    public virtual void Show(params object[] param)
    {
        if(uiLayer != EUILayer.Default)
        RegisterCursor();

        canvasGroup.DOKill();
        gameObject.SetActive(true);
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        InitialSetup();

        PlayShowAnimation()
            .SetUpdate(true)
            .OnComplete(() => onOpened?.Invoke(param));
    }
    protected virtual void InitialSetup()
    {
        canvasGroup.alpha = 0f;
    }

    public virtual void Hide(params object[] param)
    {
        UnregisterCursor();

        canvasGroup.DOKill();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        PlayHideAnimation()
            .SetUpdate(true)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                onClosed?.Invoke(param);

                UIManager.Instance.NotifyHidden(this);

                if (isDestroyAtClosed)
                {
                    UIManager.Instance.RemoveCache(GetType());
                    Destroy(gameObject);
                }
            });
    }

    protected virtual Tween PlayShowAnimation()
    {
        return canvasGroup.DOFade(1f, 0.2f);
    }

    protected virtual Tween PlayHideAnimation()
    {
        return canvasGroup.DOFade(0f, 0.2f);
    }

    public virtual bool CanOpen(params object[] param) => true;
    public virtual void OnClickClose()
    {
        UIManager.Instance.CloseUI(this);
    }
}