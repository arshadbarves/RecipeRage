using System;
using System.Collections.Generic;

namespace Core.Shared
{
    /// <summary>
    /// Lightweight reactive property for MVVM binding.
    /// Notifies subscribers when the value changes.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public class BindableProperty<T>
    {
        private T _value;
        private readonly EqualityComparer<T> _comparer;

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

        public BindableProperty(T initialValue = default)
        {
            _value = initialValue;
            _comparer = EqualityComparer<T>.Default;
        }

        /// <summary>
        /// Sets the value without triggering the event.
        /// </summary>
        public void SetValueData(T value)
        {
            _value = value;
        }

        public void Bind(Action<T> callback)
        {
            OnValueChanged += callback;
            callback(_value); // Trigger immediately
        }

        public void Unbind(Action<T> callback)
        {
            OnValueChanged -= callback;
        }

        public static implicit operator T(BindableProperty<T> property) => property.Value;
    }
}