using System;
using UnityEngine;

[Serializable]
public class NotifyValue<T>
{
    private T _value;

    /// <summary>
    /// 값이 변경될 때마다 호출되는 이벤트
    /// </summary>
    public event Action<T> OnValueChanged;

    public NotifyValue(T defaultValue = default)
    {
        _value = defaultValue;
    }

    public T Value
    {
        get => _value;
        set
        {
            // 값이 실제로 변경될 때만 이벤트 발행
            if (!Equals(_value, value))
            {
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }
    }
}
