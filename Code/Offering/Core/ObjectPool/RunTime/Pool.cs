using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class Pool
{
    private readonly Stack<IPoolable> _pool;
    private readonly Transform _parentTrm;
    private readonly GameObject _prefab;
    private readonly IObjectResolver _resolver;

    public Pool(IPoolable poolable, Transform parentTrm, int count, IObjectResolver resolver)
    {
        _pool = new Stack<IPoolable>(count);
        _parentTrm = parentTrm;
        _prefab = poolable.GameObject;
        _resolver = resolver;

        for (int i = 0; i < count; i++)
        {
            IPoolable item = CreateItem();
            item.GameObject.SetActive(false);
            _pool.Push(item);
        }
    }
    private IPoolable CreateItem()
    {
        GameObject gameObj = _resolver.Instantiate(_prefab, _parentTrm);
        IPoolable item = gameObj.GetComponent<IPoolable>();
        item.SetUpPool(this);
        return item;
    }
    public IPoolable Pop()
    {
        IPoolable item;
        if (_pool.Count == 0)
        {
            item = CreateItem();
        }
        else
        {
            item = _pool.Pop();
            item.GameObject.SetActive(true);
        }
        item.ResetItem();
        return item;
    }

    public void Push(IPoolable item)
    {
        item.GameObject.SetActive(false);
        _pool.Push(item);
    }
}
