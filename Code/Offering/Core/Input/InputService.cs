using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;

[Flags]
public enum EInputCategory
{
    None = 0,
    Player = 1 << 0,
    UI = 1 << 1,
    Timeline = 1 << 2,
    All = ~0
}

public interface IInputService : IGameService
{
    PlayerInputSO PlayerInputReader { get; }
    UIInputReader UIInputReader { get; }
    TimelineInputReader TimelineInputReader { get; }

    void SetEnableInputOnly(EInputCategory categories, bool enable);
    void SetEnableInput(EInputCategory category, bool enable);
    void SetEnableInputAll(bool enable);

    void ChangeKeyBinding(EInputCategory category, string actionName, int bindingIndex,
        Action<InputActionRebindingExtensions.RebindingOperation> onCancel,
        Action<InputActionRebindingExtensions.RebindingOperation> onComplete);
}

public class InputService : MonoBehaviour, IInputService
{
    [field: SerializeField] public PlayerInputSO PlayerInputReader { get; private set; }
    [field: SerializeField] public UIInputReader UIInputReader { get; private set; }
    [field: SerializeField] public TimelineInputReader TimelineInputReader { get; private set; }

    private Dictionary<EInputCategory, InputActionMap> _inputDictionary;
    private static readonly Regex BindingRegex = new Regex(@"^(?:[a-zA-Z]|Space|Tab)$");

    public bool IsInitialized { get; private set; }

    public UniTask InitAsync()
    {
        if (IsInitialized) return UniTask.CompletedTask;
        SettingDictionary();
        IsInitialized = true;
        return UniTask.CompletedTask;
    }

    public void Release()
    {
        if (_inputDictionary != null)
        {
            foreach (var map in _inputDictionary.Values)
                map?.Disable();        
            _inputDictionary = null;
        }
        IsInitialized = false;
    }


    private void SettingDictionary()
    {
        _inputDictionary = new Dictionary<EInputCategory, InputActionMap>
            {
                { EInputCategory.Player, PlayerInputReader.Actions },
                { EInputCategory.UI, UIInputReader.Actions },
                { EInputCategory.Timeline, TimelineInputReader.Actions }
            };
    }

    private bool EnsureInputDictionary()
    {
        if (_inputDictionary != null)
            return true;

        if (PlayerInputReader == null || UIInputReader == null || TimelineInputReader == null)
        {
            Debug.LogError($"{gameObject.name} InputReader reference is missing.");
            return false;
        }

        SettingDictionary();
        return true;
    }

    public void SetEnableInputOnly(EInputCategory categories, bool enable)
    {
        if (!EnsureInputDictionary())
            return;

        foreach (var kvp in _inputDictionary)
        {
            bool isInCategories = categories.HasFlag(kvp.Key);
            bool isEnable = isInCategories ? enable : !enable;

            if (isEnable)
                kvp.Value.Enable();
            else
                kvp.Value.Disable();
        }
    }

    public void SetEnableInput(EInputCategory category, bool enable)
    {
        if (!EnsureInputDictionary())
            return;

        if (!_inputDictionary.TryGetValue(category, out var actionMap) || actionMap == null)
        {
            Debug.LogError($"{gameObject.name} Input category not found: {category}");
            return;
        }

        if (enable)
            actionMap.Enable();
        else
            actionMap.Disable();
    }

    public void SetEnableInputAll(bool enable) => SetEnableInputOnly(EInputCategory.All, enable);

    #region Rebinding

    public void ChangeKeyBinding(EInputCategory category, string actionName, int bindingIndex,
        Action<InputActionRebindingExtensions.RebindingOperation> onCancel,
        Action<InputActionRebindingExtensions.RebindingOperation> onComplete)
    {
        if (!EnsureInputDictionary())
            return;

        var map = _inputDictionary[category];
        var action = map.FindAction(actionName);

        if (action == null)
        {
            Debug.LogError($"{category} map does not contain action '{actionName}'.");
            return;
        }

        map.Disable();

        action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse")
            .WithCancelingThrough("<keyboard>/Backspace")
            .OnComplete(operation =>
            {
                var keyString = operation.action.bindings[bindingIndex].ToDisplayString();

                if (!BindingRegex.IsMatch(keyString))
                {
                    operation.action.RemoveBindingOverride(bindingIndex);
                    FinishRebinding(onCancel, operation, map);
                    return;
                }

                FinishRebinding(onComplete, operation, map);
            })
            .OnCancel(operation => FinishRebinding(onCancel, operation, map))
            .Start();
    }

    private void FinishRebinding(Action<InputActionRebindingExtensions.RebindingOperation> callback,
        InputActionRebindingExtensions.RebindingOperation operation, InputActionMap map)
    {
        callback?.Invoke(operation);
        operation.Dispose();
        map.Enable();
    }

    #endregion
}
