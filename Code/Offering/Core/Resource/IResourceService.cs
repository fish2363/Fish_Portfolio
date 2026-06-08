using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

using Object = UnityEngine.Object;

public interface IResourceService : IGameService
{
    T Get<T>(AddressableType type, string fileName) where T : Object;
    UniTask<T> Load<T>(AddressableType type, string fileName, bool isCache = true) where T : Object;
    UniTask LoadLabelAsync<T>(string label, Action<string, int, int> callback = null) where T : Object;
    UniTask<GameObject> LoadUIAsync(string fileName);
    GameObject Instantiate(AddressableType type, string fileName, Transform parent = null);
    void ReleaseLabel(string label);
}
