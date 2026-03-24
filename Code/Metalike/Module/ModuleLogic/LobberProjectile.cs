using GondrLib.ObjectPool.RunTime;
using UnityEngine;

public class LobberProjectile : MonoBehaviour, IPoolable
{
    public PoolItemSO PoolItem { get; private set; }
    public GameObject GameObject => gameObject;
    private Pool _pool;

    public void SetUpPool(Pool pool)
    {
        _pool = pool;
    }

    public virtual void ResetItem()
    {
    }
    public void ReturnToPool()
    {
        if (_pool != null)
            _pool.Push(this);
        else
            gameObject.SetActive(false);
    }
}
