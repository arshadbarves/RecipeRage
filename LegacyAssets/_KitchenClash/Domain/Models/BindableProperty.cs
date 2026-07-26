using System;
using System.Collections.Generic;

namespace KitchenClash.Domain
{
    public sealed class BindableProperty<T>
    {
        private T _value;
        private readonly EqualityComparer<T> _comparer = EqualityComparer<T>.Default;

        public event Action<T> OnValueChanged;

        public T Value
        {
            get => _value;
            set
            {
                if (!_comparer.Equals(_value, value))
                {
                    _value = value;
                    OnValueChanged?.Invoke(_value);
                }
            }
        }

        public BindableProperty(T initialValue = default) => _value = initialValue;

        public void SetValueWithoutNotify(T value) => _value = value;

        public void Bind(Action<T> callback)
        {
            OnValueChanged += callback;
            callback(_value);
        }

        public void Unbind(Action<T> callback) => OnValueChanged -= callback;

        public static implicit operator T(BindableProperty<T> property) => property.Value;
    }
}
