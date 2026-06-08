using System;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class UIBase : MonoBehaviour
{
    public EUILayer uiLayer = EUILayer.Popup;
    public bool isDestroyAtClosed = false;
    public bool canCloseByEsc = true;

    public Action<object[]> onOpened;
    public Action<object[]> onClosed;

    protected CanvasGroup canvasGroup;

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // 어떤 경로로 파괴되더라도 커서 등록이 남지 않도록 보장
    protected virtual void OnDestroy()
    {
        UnregisterCursor();
    }

    #region Show / Hide
    public virtual void Show(params object[] param)
    {
        if (uiLayer != EUILayer.Default)
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

    public virtual void Hide(params object[] param)
    {
        if (!gameObject.activeSelf) return; // 이미 닫혀 있으면 무시

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

                if (UIManager.Instance != null)
                    UIManager.Instance.NotifyHidden(this);

                if (isDestroyAtClosed)
                {
                    if (UIManager.Instance != null)
                        UIManager.Instance.RemoveCache(GetType());

                    Destroy(gameObject);
                }
            });
    }

    public virtual void HideImmediate()
    {
        UnregisterCursor();

        canvasGroup.DOKill();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        gameObject.SetActive(false);

        onClosed?.Invoke(Array.Empty<object>());

        if (UIManager.Instance != null)
            UIManager.Instance.NotifyHidden(this);

        if (isDestroyAtClosed)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.RemoveCache(GetType());

            Destroy(gameObject);
        }
    }

    #endregion

    #region Overridable Hooks

    protected virtual void InitialSetup() => canvasGroup.alpha = 0f;
    protected virtual Tween PlayShowAnimation() => canvasGroup.DOFade(1f, 0.2f);
    protected virtual Tween PlayHideAnimation() => canvasGroup.DOFade(0f, 0.2f);
    public virtual bool CanOpen(params object[] param) => true;
    public virtual void OnClickClose() => Hide();

    #endregion

    #region Cursor

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

    #endregion
}