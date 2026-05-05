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
    private readonly Dictionary<string, List<string>> _labelToKeys = new();
    private readonly Dictionary<string, Object> _cachedResources = new();
    private readonly Dictionary<string, AsyncOperationHandle> _handles = new();

    public const string InGame = "InGame";
    public const string Stage = "Stage";

    public bool IsLoaded { get; private set; }

    #region Init & Release

    protected override async UniTask OnInit()
    {
        await Addressables.InitializeAsync().ToUniTask();
        BuildAddressMap();
        await LoadLabelAsync<Object>(InGame);
        IsLoaded = true;
    }

    protected override void OnRelease()
    {
        base.OnRelease();

        foreach (var handle in _handles.Values)
            if (handle.IsValid()) Addressables.Release(handle);

        _handles.Clear();
        _cachedResources.Clear();
        _labelToKeys.Clear();
        _addressMap.Clear();
    }

    private void BuildAddressMap()
    {
        _addressMap.Clear();

        foreach (var locator in Addressables.ResourceLocators)
        {
            foreach (var key in locator.Keys)
            {
                if (key is not string address) continue;

                // GUID, 라벨 등 경로가 아닌 키 필터링
                if (!address.Contains('/')) continue;

                string fileName = Path.GetFileNameWithoutExtension(address);
                if (string.IsNullOrEmpty(fileName)) continue;

                if (_addressMap.TryGetValue(fileName, out string existing))
                {
                    Debug.LogWarning(
                        $"[ResourceManager] 파일명 충돌: '{fileName}'\n" +
                        $"  등록됨: {existing}\n" +
                        $"  무시됨: {address}\n" +
                        $"  → 파일명을 고유하게 변경하세요.");
                    continue;
                }

                _addressMap.Add(fileName, address);
            }
        }

        Debug.Log($"[ResourceManager] 주소 맵 빌드 완료: {_addressMap.Count}개");
    }

    #endregion

    #region Get & Load

    public T Get<T>(string fileName) where T : Object
    {
        if (!_addressMap.TryGetValue(fileName, out string address))
        {
            Debug.LogError($"[ResourceManager] 주소 없음: '{fileName}'");
            return null;
        }

        if (_cachedResources.TryGetValue(address, out Object resource))
            return resource as T;

        Debug.LogWarning($"[ResourceManager] 캐시 미스: '{fileName}'. LoadAsync 먼저 호출하세요.");
        return null;
    }

    public async UniTask<T> LoadAsync<T>(string fileName) where T : Object
    {
        if (!_addressMap.TryGetValue(fileName, out string address))
        {
            Debug.LogError($"[ResourceManager] 주소 없음: '{fileName}'");
            return null;
        }

        if (_cachedResources.TryGetValue(address, out Object cached))
            return cached as T;

        var handle = Addressables.LoadAssetAsync<T>(address);
        await handle.ToUniTask();

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Addressables.Release(handle);
            Debug.LogError($"[ResourceManager] 로드 실패: '{fileName}'");
            return null;
        }

        _cachedResources[address] = handle.Result;
        _handles[address] = handle;
        return handle.Result;
    }

    public async UniTask LoadLabelAsync<T>(string label, Action<string, int, int> onProgress = null)
        where T : Object
    {
        var locations = await Addressables
            .LoadResourceLocationsAsync(label, typeof(T))
            .ToUniTask();

        if (locations == null || locations.Count == 0)
        {
            Debug.LogWarning($"[ResourceManager] '{label}' 라벨에 에셋 없음.");
            return;
        }

        if (!_labelToKeys.ContainsKey(label))
            _labelToKeys[label] = new List<string>();

        int total = locations.Count;
        int completed = 0;
        var tasks = new List<UniTask>();

        foreach (var location in locations)
        {
            string address = location.PrimaryKey;

            if (_cachedResources.ContainsKey(address))
            {
                completed++;
                onProgress?.Invoke(address, completed, total);
                continue;
            }

            tasks.Add(LoadSingleAsync<T>(address, label, () =>
            {
                completed++;
                onProgress?.Invoke(address, completed, total);
            }));
        }

        await UniTask.WhenAll(tasks);
    }

    private async UniTask LoadSingleAsync<T>(string address, string label, Action onComplete)
        where T : Object
    {
        var handle = Addressables.LoadAssetAsync<T>(address);
        await handle.ToUniTask();

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _cachedResources[address] = handle.Result;
            _handles[address] = handle;
            _labelToKeys[label].Add(address);
        }
        else
        {
            Addressables.Release(handle);
            Debug.LogError($"[ResourceManager] 단일 로드 실패: '{address}'");
        }

        onComplete?.Invoke();
    }

    public void ReleaseLabel(string label)
    {
        if (!_labelToKeys.TryGetValue(label, out var keys)) return;

        foreach (var key in keys)
        {
            _cachedResources.Remove(key);

            if (_handles.TryGetValue(key, out var handle) && handle.IsValid())
                Addressables.Release(handle);

            _handles.Remove(key);
        }

        _labelToKeys.Remove(label);
        Debug.Log($"[ResourceManager] '{label}' 해제 완료.");
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

    public void Destroy(GameObject go)
    {
        if (go == null) return;
        Object.Destroy(go);
    }

    #endregion
}