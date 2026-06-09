using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NotifyValue<T>
{
    public delegate void ValueChanged(T prev, T next);
    public event ValueChanged OnValueChanged;

    [SerializeField] private T _value;

    public T Value
    {
        get => _value;
        set
        {
            T before = _value;
            _value = value;

            if (!EqualityComparer<T>.Default.Equals(before, value))
                OnValueChanged?.Invoke(before, value);
        }
    }

    public NotifyValue() : this(default) { }

    public NotifyValue(T value)
    {
        _value = value;
    }

    // 이벤트를 발생시키지 않고 값만 설정 (초기화 등에 사용)
    public void SetWithoutNotify(T value)
    {
        _value = value;
    }
}