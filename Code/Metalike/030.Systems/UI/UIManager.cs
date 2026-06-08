using Cysharp.Threading.Tasks;
using InputControl;
using ManagingSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class UIManager : BaseManager<UIManager>
{
    [SerializeField] private UIInputReader inputSO;
    [SerializeField] private Transform[] uiParents;

    private readonly Dictionary<Type, UIBase> _uiMap = new();
    private readonly Dictionary<Type, UniTask<UIBase>> _loadingTasks = new();

    private readonly List<UIBase> _popupStack = new();
    private readonly List<UIBase> _fixedStack = new();

    #region Init & Release

    protected override UniTask OnInit()
    {
        _uiMap.Clear();
        _loadingTasks.Clear();
        _popupStack.Clear();
        _fixedStack.Clear();

        if (inputSO != null)
        {
            inputSO.OnPauseClickEvent -= TryCloseByEsc;
            inputSO.OnPauseClickEvent += TryCloseByEsc;
        }

        return UniTask.CompletedTask;
    }

    protected override void OnRelease()
    {
        if (inputSO != null)
            inputSO.OnPauseClickEvent -= TryCloseByEsc;

        foreach (UIBase ui in _uiMap.Values)
        {
            if (ui != null)
                Object.Destroy(ui.gameObject);
        }

        _uiMap.Clear();
        _loadingTasks.Clear();
        _popupStack.Clear();
        _fixedStack.Clear();
    }

    #endregion

    #region Show / Hide API

    public async UniTask<T> Show<T>(params object[] param) where T : UIBase
    {
        UIBase ui = await GetOrCreateAsync(typeof(T));
        if (ui == null) return null;

        T typedUI = ui as T;
        if (typedUI == null || !typedUI.CanOpen(param)) return null;

        PushToStack(typedUI);
        typedUI.Show(param);
        return typedUI;
    }

    public void Hide<T>(params object[] param) where T : UIBase
    {
        if (!_uiMap.TryGetValue(typeof(T), out UIBase ui) || ui == null)
            return;

        ui.Hide(param);
    }

    public void HideImmediate<T>() where T : UIBase
    {
        if (!_uiMap.TryGetValue(typeof(T), out UIBase ui) || ui == null)
            return;

        RemoveFromStack(ui);
        ui.HideImmediate();
    }

    public void Hide(params object[] param)
    {
        if (TryCloseTop(_fixedStack, param)) return;
        TryCloseTop(_popupStack, param);
    }

    public void Destroy<T>() where T : UIBase
    {
        Type type = typeof(T);
        if (!_uiMap.TryGetValue(type, out UIBase ui) || ui == null)
            return;

        RemoveFromStack(ui);
        _uiMap.Remove(type);
        Object.Destroy(ui.gameObject); 
    }

    public bool IsOpened<T>() where T : UIBase
        => _uiMap.TryGetValue(typeof(T), out UIBase ui) && ui != null && ui.gameObject.activeSelf;

    public T Get<T>() where T : UIBase
    {
        _uiMap.TryGetValue(typeof(T), out UIBase ui);
        return ui as T;
    }

    #endregion

    #region Internal Callbacks (UIBase → UIManager)

    public void NotifyHidden(UIBase ui) => RemoveFromStack(ui);

    public void RemoveCache(Type type) => _uiMap.Remove(type);

    #endregion

    #region Create

    private async UniTask<UIBase> GetOrCreateAsync(Type type)
    {
        if (_uiMap.TryGetValue(type, out UIBase existing) && existing != null)
            return existing;

        if (_loadingTasks.TryGetValue(type, out UniTask<UIBase> ongoing))
            return await ongoing;

        UniTask<UIBase> task = CreateAsync(type).Preserve();
        _loadingTasks[type] = task;

        try { return await task; }
        finally { _loadingTasks.Remove(type); }
    }

    private async UniTask<UIBase> CreateAsync(Type type)
    {
        string fileName = type.Name;

        // 캐시 히트 시 즉시, 아니면 Addressables 비동기 로드
        GameObject prefab = ResourceManager.Instance.Get<GameObject>(fileName);
        if (prefab == null)
            prefab = await ResourceManager.Instance.LoadAsync<GameObject>(fileName);

        if (prefab == null)
        {
            Debug.LogError($"프리팹 로드 실패:{fileName}");
            return null;
        }

        UIBase prefabUI = prefab.GetComponent<UIBase>();
        if (prefabUI == null)
        {
            Debug.LogError($"프리팹에 UIBase 없음:{fileName}");
            return null;
        }

        int layerIndex = (int)prefabUI.uiLayer;
        GameObject go = Object.Instantiate(prefab, uiParents[layerIndex]);
        go.name = fileName;

        UIBase ui = go.GetComponent<UIBase>();
        _uiMap[type] = ui;
        return ui;
    }

    #endregion

    #region Stack

    private void PushToStack(UIBase ui)
    {
        List<UIBase> stack = GetStack(ui.uiLayer);
        if (stack == null) return;

        if (!stack.Contains(ui)) // 중복 push 방지
            stack.Add(ui);

        ui.transform.SetAsLastSibling();
    }

    private void RemoveFromStack(UIBase ui)
    {
        _popupStack.Remove(ui);
        _fixedStack.Remove(ui);
    }

    private bool TryCloseTop(List<UIBase> stack, object[] param)
    {
        if (stack.Count == 0) return false;

        UIBase top = stack[^1];
        stack.RemoveAt(stack.Count - 1);
        top.Hide(param);
        return true;
    }

    private void TryCloseByEsc()
    {
        UIBase top = null;
        if (_fixedStack.Count > 0) top = _fixedStack[^1];
        else if (_popupStack.Count > 0) top = _popupStack[^1];

        if (top == null || !top.canCloseByEsc) return;

        top.Hide();
    }

    private List<UIBase> GetStack(EUILayer layer) => layer switch
    {
        EUILayer.Popup => _popupStack,
        EUILayer.Fixed => _fixedStack,
        _ => null
    };

    #endregion
}
