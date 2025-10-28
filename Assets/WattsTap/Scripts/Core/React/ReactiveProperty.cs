using System;
using UnityEngine;

namespace WattsTap.Core.React
{
    public class ReactiveProperty<T>
    {
        private T _value;

        public T Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value))
                {
                    return;
                }

                Debug.Log($"Value changed: {_value} -> {value}");
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }

        public event Action<T> OnValueChanged;

        public ReactiveProperty()
        {
        }

        public ReactiveProperty(T initialValue)
        {
            _value = initialValue;
        }

        public void ForceNotify()
        {
            OnValueChanged?.Invoke(_value);
        }

        public void Dispose()
        {
            OnValueChanged = null;
        }
    }
}
