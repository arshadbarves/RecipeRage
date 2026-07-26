using System.Collections.Generic;
using Playcenter.Services;

namespace KitchenClash.Infrastructure.Settings
{
    /// <summary>
    /// In-memory <see cref="ISettingsStore"/> for EditMode tests and offline fakes.
    /// </summary>
    public sealed class DictionarySettingsStore : ISettingsStore
    {
        private readonly Dictionary<string, float> _floats = new Dictionary<string, float>();
        private readonly Dictionary<string, int> _ints = new Dictionary<string, int>();
        private readonly Dictionary<string, string> _strings = new Dictionary<string, string>();

        public int SaveCallCount { get; private set; }

        public float GetFloat(string key, float defaultValue)
            => _floats.TryGetValue(key, out float value) ? value : defaultValue;

        public void SetFloat(string key, float value) => _floats[key] = value;

        public int GetInt(string key, int defaultValue)
            => _ints.TryGetValue(key, out int value) ? value : defaultValue;

        public void SetInt(string key, int value) => _ints[key] = value;

        public string GetString(string key, string defaultValue)
            => _strings.TryGetValue(key, out string value) ? value : defaultValue;

        public void SetString(string key, string value) => _strings[key] = value ?? string.Empty;

        public void Save() => SaveCallCount++;
    }
}
