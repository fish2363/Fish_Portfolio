using Cysharp.Threading.Tasks;
using ManagingSystem;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public class ResourceManager : BaseManager<ResourceManager>
{
    private readonly Dictionary<string, string> _addressMap = new();        
    private readonly Dictionary<string, Object> _cachedResources = new();   
    private readonly Dictionary<string, AsyncOperationHandle> _handles = new();      
    private readonly Dictionary<string, AsyncOperationHandle> _labelHandles = new(); 
    private readonly Dictionary<string, List<string>> _labelToKeys = new();

    public const string InGame = "InGame";
    public const string Stage = "Stage";

    public bool IsLoaded { get; private set; }

    #region Init & Release

    protected override async UniTask OnInit()
    {
        await Addressables.InitializeAsync().ToUniTask();
        BuildAddressMap();
        IsLoaded = true;
    }

    protected override void OnRelease()
    {
        base.OnRelease();

        foreach (var handle in _handles.Values)
            if (handle.IsValid()) Addressables.Release(handle);

        foreach (var handle in _labelHandles.Values)
            if (handle.IsValid()) Addressables.Release(handle);

        _handles.Clear();
        _labelHandles.Clear();
        _cachedResources.Clear();
        _labelToKeys.Clear();
        _addressMap.Clear();
        IsLoaded = false;
    }

    private void BuildAddressMap()
    {
        _addressMap.Clear();

        foreach (var locator in Addressables.ResourceLocators)
        {
            foreach (var key in locator.Keys)
            {
                if (key is not string address) continue;
                if (!address.Contains('/')) continue;  

                string fileName = Path.GetFileNameWithoutExtension(address);
                if (string.IsNullOrEmpty(fileName)) continue;

                if (_addressMap.TryGetValue(fileName, out string existing))
                {
                    Debug.LogWarning(
                        $"파일명 충돌:{fileName}\n등록됨: {existing}\n  무시됨: {address}\n  파일명을 고유하게 변경하세요");
                    continue;
                }

                _addressMap.Add(fileName, address);
            }
        }

        Debug.Log($"주소 맵 빌드 완료: {_addressMap.Count}개");
    }

    #endregion

    #region Get & Load (단일)

    public T Get<T>(string fileName) where T : Object
    {
        if (!_addressMap.TryGetValue(fileName, out string address))
        {
            Debug.LogError($"주소 없음: '{fileName}'");
            return null;
        }

        if (_cachedResources.TryGetValue(address, out Object resource))
            return resource as T;

        Debug.LogWarning($"캐시 미스:{fileName} LoadAsync 먼저 호출하세요.");
        return null;
    }

    public async UniTask<T> LoadAsync<T>(string fileName) where T : Object
    {
        if (!_addressMap.TryGetValue(fileName, out string address))
        {
            Debug.LogError($"주소 없음: {fileName}");
            return null;
        }

        if (_cachedResources.TryGetValue(address, out Object cached))
            return cached as T;

        var handle = Addressables.LoadAssetAsync<T>(address);
        await handle.ToUniTask();

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Addressables.Release(handle);
            Debug.LogError($"로드 실패:{fileName}");
            return null;
        }

        _cachedResources[address] = handle.Result;
        _handles[address] = handle;
        return handle.Result;
    }

    public void Release(string fileName)
    {
        if (!_addressMap.TryGetValue(fileName, out string address)) return;
        if (!_handles.TryGetValue(address, out var handle)) return;

        if (handle.IsValid()) Addressables.Release(handle);
        _handles.Remove(address);
        _cachedResources.Remove(address);
    }

    #endregion

    #region Load (라벨 배치)

    public async UniTask LoadLabelAsync<T>(string label, IProgress<float> onProgress = null)
        where T : Object
    {
        if (_labelToKeys.ContainsKey(label)) return; 

        var locations = await Addressables
            .LoadResourceLocationsAsync(label, typeof(T))
            .ToUniTask();

        if (locations == null || locations.Count == 0)
        {
            Debug.LogWarning($"{label} 라벨에 에셋 없음.");
            return;
        }

        var handle = Addressables.LoadAssetsAsync<T>(locations, null);
        await handle.ToUniTask(onProgress);

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Addressables.Release(handle);
            Debug.LogError($"{label} 라벨 로드 실패.");
            return;
        }

        var keys = new List<string>(locations.Count);
        for (int i = 0; i < locations.Count; i++)
        {
            string address = locations[i].PrimaryKey;
            _cachedResources[address] = handle.Result[i];
            keys.Add(address);
        }

        _labelHandles[label] = handle;  
        _labelToKeys[label] = keys;
    }

    public void ReleaseLabel(string label)
    {
        if (_labelToKeys.TryGetValue(label, out var keys))
        {
            foreach (var key in keys)
                _cachedResources.Remove(key);
            _labelToKeys.Remove(label);
        }

        if (_labelHandles.TryGetValue(label, out var handle))
        {
            if (handle.IsValid()) Addressables.Release(handle);
            _labelHandles.Remove(label);
        }

        Debug.Log($"{label} 해제 완료.");
    }

    #endregion

    #region Instantiate & Destroy

    public GameObject Instantiate(string fileName, Transform parent = null)
    {
        GameObject prefab = Get<GameObject>(fileName);
        if (prefab == null) return null;

        GameObject go = Object.Instantiate(prefab, parent);
        go.name = prefab.name;
        return go;
    }

    public GameObject Instantiate(string fileName, Vector3 position)
    {
        GameObject prefab = Get<GameObject>(fileName);
        if (prefab == null) return null;

        GameObject go = Object.Instantiate(prefab, position, Quaternion.identity);
        go.name = prefab.name;
        return go;
    }

    public async UniTask<GameObject> InstantiateAsync(string fileName, Transform parent = null)
    {
        GameObject prefab = await LoadAsync<GameObject>(fileName);
        if (prefab == null) return null;

        GameObject go = Object.Instantiate(prefab, parent);
        go.name = prefab.name;
        return go;
    }

    public async UniTask<GameObject> InstantiateAsync(string fileName, Vector3 position)
    {
        GameObject prefab = await LoadAsync<GameObject>(fileName);
        if (prefab == null) return null;

        GameObject go = Object.Instantiate(prefab, position, Quaternion.identity);
        go.name = prefab.name;
        return go;
    }

    public void Destroy(GameObject go)
    {
        if (go == null) return;
        Object.Destroy(go);
    }

    #endregion
}