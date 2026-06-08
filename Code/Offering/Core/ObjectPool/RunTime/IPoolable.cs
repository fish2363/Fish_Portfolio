using UnityEngine;

public interface IPoolable
{
    public PoolItemSO PoolItem { get;}
    public GameObject GameObject { get; }
    public void SetUpPool(Pool pool); //꼭 해줄 필요는 없지만 편의상 해주면 좋다.
    public void ResetItem();
}
