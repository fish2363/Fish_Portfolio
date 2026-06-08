using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface ICurrencyService : IGameService
{
    event ValueChangedHanlder OnValueChanged;
    float GetNumericalValue(NumericalValueType currencyType);
    void ModifyNumericalValue(NumericalValueType valueType, ModifyType modifyType, float amount);
}

public class NumericalValueManager : MonoBehaviour, ICurrencyService
{
    [Header("Game Start Values")]
    [SerializeField] private float defaultMoney = 1000f;

    private readonly Dictionary<NumericalValueType, float> _numericalValues = new();

    public event ValueChangedHanlder OnValueChanged;
    public bool IsInitialized { get; private set; }

    public UniTask InitAsync()
    {
        if (IsInitialized)
            return UniTask.CompletedTask;

        InitializeValues();
        ModifyNumericalValue(NumericalValueType.Money, ModifyType.Set, defaultMoney);
        IsInitialized = true;
        return UniTask.CompletedTask;
    }

    public void Release()
    {
        _numericalValues.Clear();
        OnValueChanged = null;
        IsInitialized = false;
    }

    public float GetNumericalValue(NumericalValueType currencyType)
    {
        return _numericalValues.GetValueOrDefault(currencyType);
    }

    public void ModifyNumericalValue(NumericalValueType valueType, ModifyType modifyType, float amount)
    {
        if (!IsInitialized)
            InitializeValues();

        float currentValue = _numericalValues.GetValueOrDefault(valueType);
        float nextValue = modifyType switch
        {
            ModifyType.Set => amount,
            ModifyType.Add => Mathf.Clamp(currentValue + amount, 0, 9999999),
            ModifyType.Multiply => Mathf.Clamp(currentValue * amount, 0, 9999999),
            ModifyType.Divine => amount == 0 ? currentValue : currentValue / amount,
            _ => currentValue
        };

        _numericalValues[valueType] = nextValue;
        OnValueChanged?.Invoke(valueType, nextValue);
    }

    private void InitializeValues()
    {
        _numericalValues.Clear();
        foreach (NumericalValueType valueType in Enum.GetValues(typeof(NumericalValueType)))
        {
            _numericalValues[valueType] = 0;
        }
    }
}
