using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using VContainer;

public interface IPoolService
{
    T Pop<T>(PoolItemSO item) where T : IPoolable;
    void Push(IPoolable item);
}

public class PoolManagerMono : MonoBehaviour, IPoolService
{
    [SerializeField] private PoolManagerSO poolManager;


    [Inject]
    public void Construct(IObjectResolver resolver)
    {
        poolManager.Initialize(transform, resolver);
    }
        
    public T Pop<T>(PoolItemSO item) where T : IPoolable
    {
        return (T)poolManager.Pop(item);
    }
        
    public void Push(IPoolable item)
    {
        poolManager.Push(item);
    }
}
