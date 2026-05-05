using Cysharp.Threading.Tasks;
using ManagingSystem;
using UnityEngine;

namespace ManagingSystem
{
    public abstract class BaseManager<T> : MonoSingleton<T> where T : BaseManager<T>
    {
        public bool IsInitialized { get; private set; }

        public async UniTask InitAsync()
        {
            if (IsInitialized) return;

            await OnInit();
            IsInitialized = true;

            AfterInit();
        }

        protected virtual UniTask OnInit()
        {
            return UniTask.CompletedTask;
        }

        protected virtual void AfterInit() { }

        public void Release()
        {
            if (!IsInitialized) return;

            OnRelease();
            IsInitialized = false;
        }

        protected virtual void OnRelease() { }
    }
}

