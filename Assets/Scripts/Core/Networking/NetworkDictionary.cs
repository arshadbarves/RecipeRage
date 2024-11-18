using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace RecipeRage.Core.Networking
{
    public struct NetworkDictionaryEvent<TKey, TValue>
    {
        public enum EventType
        {
            Add,
            Remove,
            Value
        }

        public EventType Type;
        public TKey Key;
        public TValue Value;
    }

    public class NetworkDictionary<TKey, TValue> : NetworkVariableBase
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        private readonly Dictionary<TKey, TValue> _dictionary = new();
        public event Action<NetworkDictionaryEvent<TKey, TValue>> OnDictionaryChanged;

        public NetworkDictionary(NetworkVariableReadPermission readPerm = DefaultReadPerm,
            NetworkVariableWritePermission writePerm = DefaultWritePerm)
            : base(readPerm, writePerm) { }

        public void Clear()
        {
            if (_dictionary.Count == 0) return;

            _dictionary.Clear();
            MarkDirty();
        }

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public bool ContainsValue(TValue value) => _dictionary.ContainsValue(value);

        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set
            {
                if (_dictionary.TryGetValue(key, out var existingValue) && existingValue.Equals(value))
                    return;

                _dictionary[key] = value;
                MarkDirty();

                OnDictionaryChanged?.Invoke(new NetworkDictionaryEvent<TKey, TValue>
                {
                    Type = NetworkDictionaryEvent<TKey, TValue>.EventType.Value,
                    Key = key,
                    Value = value
                });
            }
        }

        public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            MarkDirty();

            OnDictionaryChanged?.Invoke(new NetworkDictionaryEvent<TKey, TValue>
            {
                Type = NetworkDictionaryEvent<TKey, TValue>.EventType.Add,
                Key = key,
                Value = value
            });
        }

        public bool Remove(TKey key)
        {
            if (!_dictionary.TryGetValue(key, out var value)) return false;

            _dictionary.Remove(key);
            MarkDirty();

            OnDictionaryChanged?.Invoke(new NetworkDictionaryEvent<TKey, TValue>
            {
                Type = NetworkDictionaryEvent<TKey, TValue>.EventType.Remove,
                Key = key,
                Value = value
            });

            return true;
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> Where(Func<KeyValuePair<TKey, TValue>, bool> predicate)
        {
            return System.Linq.Enumerable.Where(_dictionary, predicate);
        }

        public override void WriteField(FastBufferWriter writer)
        {
            writer.WriteValueSafe(_dictionary.Count);
            foreach (var kvp in _dictionary)
            {
                writer.WriteValueSafe(kvp.Key);
                writer.WriteValueSafe(kvp.Value);
            }
        }

        public override void ReadField(FastBufferReader reader)
        {
            reader.ReadValueSafe(out int count);
            _dictionary.Clear();

            for (int i = 0; i < count; i++)
            {
                reader.ReadValueSafe(out TKey key);
                reader.ReadValueSafe(out TValue value);
                _dictionary.Add(key, value);
            }
        }

        public override void ReadDelta(FastBufferReader reader, bool keepDirtyDelta)
        {
            ReadField(reader);
        }

        public override void WriteDelta(FastBufferWriter writer)
        {
            WriteField(writer);
        }
    }
}
