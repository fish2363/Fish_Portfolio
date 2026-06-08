using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

using Object = UnityEngine.Object;

public sealed class ResourceService : IResourceService
{
    private readonly Dictionary<AddressableType, Dictionary<string, string>> addressMap = new();
    private readonly Dictionary<string, Object> cachedResources = new();
    private readonly Dictionary<string, AsyncOperationHandle> handles = new();

    public bool IsInitialized { get; private set; }

    public async UniTask InitAsync()
    {
        if (IsInitialized)
        {
            return;
        }

        AsyncOperationHandle<IResourceLocator> handle = Addressables.InitializeAsync(false);
        await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("Addressables initialization failed");
            return;
        }

        BuildAddressMap();
        IsInitialized = true;
    }

    public void Release()
    {
        if (!IsInitialized)
        {
            return;
        }

        foreach (AsyncOperationHandle handle in handles.Values)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }

        handles.Clear();
        cachedResources.Clear();
        addressMap.Clear();
        IsInitialized = false;
    }

    public T Get<T>(AddressableType type, string fileName) where T : Object
    {
        if (!TryGetAddress(type, fileName, out string address))
        {
            Debug.LogWarning($"Address not found: {type}/{fileName}");
            return null;
        }

        if (cachedResources.TryGetValue(address, out Object resource))
        {
            return resource as T;
        }

        Debug.LogWarning($"Cache miss: {address}");
        return null;
    }

    public async UniTask<T> Load<T>(AddressableType type, string fileName, bool isCache = true) where T : Object
    {
        if (!TryGetAddress(type, fileName, out string address))
        {
            Debug.LogWarning($"Address not found: {type}/{fileName}");
            return null;
        }

        if (cachedResources.TryGetValue(address, out Object cached))
        {
            return cached as T;
        }

        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
        await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Failed to load asset {type}/{fileName}");
            return null;
        }

        if (isCache)
        {
            cachedResources[address] = handle.Result;
        }

        handles[address] = handle;
        return handle.Result;
    }

    public async UniTask LoadLabelAsync<T>(string label, Action<string, int, int> callback = null) where T : Object
    {
        AsyncOperationHandle<IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation>> locationsHandle =
            Addressables.LoadResourceLocationsAsync(label);
        await locationsHandle.Task;

        if (locationsHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Failed to load label: {label}");
            return;
        }

        int totalCount = locationsHandle.Result.Count;
        int loadCount = 0;

        foreach (var location in locationsHandle.Result)
        {
            string address = location.PrimaryKey;

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                cachedResources[address] = handle.Result;
                handles[address] = handle;
            }
            else
            {
                Debug.LogError($"Failed to load asset from label: {address}");
            }

            loadCount++;
            callback?.Invoke(address, loadCount, totalCount);
        }

        Addressables.Release(locationsHandle);
    }

    public async UniTask<GameObject> LoadUIAsync(string fileName)
    {
        return await Load<GameObject>(AddressableType.UI, fileName, false);
    }

    public GameObject Instantiate(AddressableType type, string fileName, Transform parent = null)
    {
        GameObject prefab = Get<GameObject>(type, fileName);
        if (prefab == null)
        {
            Debug.LogError($"Cached prefab not found: {type}/{fileName}");
            return null;
        }

        GameObject instance = Object.Instantiate(prefab, parent);
        instance.name = prefab.name;
        return instance;
    }

    public void ReleaseLabel(string label)
    {
        AsyncOperationHandle<IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation>> locationsHandle =
            Addressables.LoadResourceLocationsAsync(label);
        locationsHandle.WaitForCompletion();

        if (locationsHandle.Status != AsyncOperationStatus.Succeeded)
        {
            return;
        }

        foreach (var location in locationsHandle.Result)
        {
            string address = location.PrimaryKey;
            cachedResources.Remove(address);

            if (handles.TryGetValue(address, out AsyncOperationHandle handle))
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }

                handles.Remove(address);
            }
        }

        Addressables.Release(locationsHandle);
    }

    private void BuildAddressMap()
    {
        foreach (IResourceLocator locator in Addressables.ResourceLocators)
        {
            foreach (object rawKey in locator.Keys)
            {
                if (rawKey is not string address)
                {
                    continue;
                }

                if (!TryParseAddress(address, out AddressableType type, out string fileName))
                {
                    continue;
                }

                if (!addressMap.TryGetValue(type, out Dictionary<string, string> fileMap))
                {
                    fileMap = new Dictionary<string, string>();
                    addressMap.Add(type, fileMap);
                }

                if (fileMap.TryGetValue(fileName, out string existingAddress))
                {
                    Debug.LogWarning($"Duplicate file name mapping: {type}/{fileName} -> {existingAddress} / {address}");
                }

                fileMap[fileName] = address;
            }
        }
    }

    private bool TryParseAddress(string address, out AddressableType type, out string fileName)
    {
        type = default;
        fileName = null;

        string[] split = address.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (split.Length < 4)
        {
            return false;
        }

        string folderName = split[2];
        string lastToken = split[^1];

        if (!Enum.TryParse(folderName, true, out type))
        {
            Debug.LogWarning($"Invalid addressable type folder: {folderName} | address: {address}");
            return false;
        }

        fileName = Path.GetFileNameWithoutExtension(lastToken);
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogWarning($"Invalid file name token: {lastToken} | address: {address}");
            return false;
        }

        return true;
    }

    private bool TryGetAddress(AddressableType type, string fileName, out string address)
    {
        address = null;

        if (!addressMap.TryGetValue(type, out Dictionary<string, string> fileMap))
        {
            return false;
        }

        return fileMap.TryGetValue(fileName, out address);
    }
}

