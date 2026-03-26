using Cysharp.Threading.Tasks;
using UnityEngine;


public abstract class BaseManager<T> : MonoSingleton<T> where T : BaseManager<T>
{
    public bool IsInitialized { get; private set; }

    #region Public Methods
    public async UniTask InitAsync()
    {
        if (IsInitialized) return;

        await OnInit();
        IsInitialized = true;
    }

    public void Release()
    {
        if (!IsInitialized) return;

        OnRelease();
        IsInitialized = false;
    }
    #endregion

    #region Protected Methods
    protected virtual UniTask OnInit()
    {
        return UniTask.CompletedTask;
    }

    protected virtual void OnRelease() { }
    #endregion
}