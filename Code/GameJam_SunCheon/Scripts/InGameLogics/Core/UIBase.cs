using UnityEngine;
using UnityEngine.Events;

public enum eUIPosition
{
    Default,
    Popup,
    Override
}

[RequireComponent(typeof(CanvasGroup))]
public class UIBase : MonoBehaviour
{
    public eUIPosition uiPosition = eUIPosition.Default;
    public bool isDestroyAtClosed = false;

    public UnityAction<object[]> opened;
    public UnityAction<object[]> closed;

    protected virtual void Awake()
    {
        opened += Opened;
        closed += Closed;
    }

    public virtual void Opened(object[] param) { }
    public virtual void Closed(object[] param) { }
    public virtual void CloseDDD()
    {
    }
    public virtual void OnUICloseBtn()
    {
        UIManager.Hide();
    }
}