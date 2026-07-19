using Playcenter.Services;
using UnityEngine;

namespace KitchenClash.Infrastructure.Settings
{
    /// <summary>
    /// PlayerPrefs-backed settings store. Never calls DeleteAll.
    /// </summary>
    public sealed class PlayerPrefsSettingsStore : ISettingsStore
    {
        public float GetFloat(string key, float defaultValue) => PlayerPrefs.GetFloat(key, defaultValue);

        public void SetFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);

        public int GetInt(string key, int defaultValue) => PlayerPrefs.GetInt(key, defaultValue);

        public void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);

        public string GetString(string key, string defaultValue) => PlayerPrefs.GetString(key, defaultValue);

        public void SetString(string key, string value) => PlayerPrefs.SetString(key, value ?? string.Empty);

        public void Save() => PlayerPrefs.Save();
    }
}
