using GondrLib.ObjectPool.RunTime;
using UnityEngine;

public class PlayerVisual : MonoBehaviour,IPoolable
{
    [field: SerializeField] public PoolItemSO PoolItem { get; private set; }
    [field: SerializeField] public MeshContainer CurrentMesh { get; private set; }
    public GameObject GameObject => gameObject;
    public Vector3 scale;
    public Vector3 position;
    public Quaternion rotation;
    private Pool _myPool;
    
    public void InitalSetting()
    {
        transform.localScale = scale;
        transform.localPosition = position;
        transform.rotation = rotation;
    }

    public void ResetItem()
    {
    }

    public void SetUpPool(Pool pool)
    {
        _myPool = pool;
    }

    private void OnValidate()
    {
        scale = transform.localScale;
        position = transform.position;
        rotation = transform.rotation;
    }
}
