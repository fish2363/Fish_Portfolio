using System.Collections.Generic;
using UnityEngine;
using VContainer;

[CreateAssetMenu(fileName = "PoolManager", menuName = "SO/Pool/Manager", order = 0)]
public class PoolManagerSO : ScriptableObject
{
    public List<PoolItemSO> itemList = new List<PoolItemSO>();

    private Dictionary<PoolItemSO, Pool> _pools;
    private Transform _rootTrm;

    public void Initialize(Transform rootTrm, IObjectResolver resolver)
    {
        _rootTrm = rootTrm;
        _pools = new Dictionary<PoolItemSO, Pool>();

        foreach (var item in itemList)
        {
            IPoolable poolable = item.prefab.GetComponent<IPoolable>();
            Debug.Assert(poolable != null, $"풀링하려는 객체가 IPoolable가 없음 {item.prefab.name}");

            Transform poolParent = new GameObject(item.poolingName).transform;
            poolParent.SetParent(_rootTrm);

            Pool pool = new Pool(poolable, poolParent, item.initCount, resolver);
            _pools.Add(item, pool);
        }
    }

    public IPoolable Pop(PoolItemSO item)
    {
        if (_pools.TryGetValue(item, out Pool pool))
        {
            return pool.Pop();
        }

        return null;
    }

    public void Push(IPoolable item)
    {
        if (_pools.TryGetValue(item.PoolItem, out Pool pool))
        {
            pool.Push(item);
        }
    }
}
