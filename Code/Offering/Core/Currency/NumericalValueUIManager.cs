using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VContainer;

public class NumericalValueUIManager : MonoBehaviour
{
    [SerializeField, Header("Money")] private TextMeshProUGUI _moneyText;

    private readonly Dictionary<NumericalValueType, Action<float>> _valueUiHandlers = new();
    private ICurrencyService _currencyService;
    private bool _subscribed;

    [Inject]
    public void Construct(ICurrencyService currencyService) => Bind(currencyService);

    private void Awake()
    {
        _valueUiHandlers[NumericalValueType.Money] = OnMoneyUI;
    }

    public void Bind(ICurrencyService currencyService)
    {
        if (_currencyService == currencyService)
            return;

        Unsubscribe();                      
        _currencyService = currencyService;

        if (isActiveAndEnabled)
        {
            Subscribe();                 
            RefreshAll();                
        }
    }

    private void OnEnable() => Subscribe();
    private void OnDisable() => Unsubscribe();

    private void Subscribe()
    {
        if (_subscribed || _currencyService == null)
            return;

        _currencyService.OnValueChanged += HandleValueChanged;
        _subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_subscribed || _currencyService == null)
            return;

        _currencyService.OnValueChanged -= HandleValueChanged;
        _subscribed = false;
    }

    private void HandleValueChanged(NumericalValueType type, float value)
    {
        if (_valueUiHandlers.TryGetValue(type, out Action<float> handler))
            handler.Invoke(value);
    }

    private void RefreshAll()
    {
        
    }

    private void OnMoneyUI(float value)
    {
        if (_moneyText != null)
            _moneyText.text = $"{value}";
    }
}