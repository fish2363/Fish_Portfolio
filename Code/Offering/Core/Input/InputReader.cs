using UnityEngine;

public abstract class InputReader : ScriptableObject
{
    private Controls _inputControls;
    public Controls InputControls => _inputControls;

    protected virtual void OnEnable()
    {
        if (_inputControls == null)
        {
            _inputControls = new Controls();
        }
    }

    public abstract void ClearInputEvent();
}
