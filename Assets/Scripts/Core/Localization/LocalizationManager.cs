using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Logging;
using UnityEngine;
using VContainer.Unity;

namespace Core.Localization
{
    public class LocalizationManager : ILocalizationManager, IInitializable
    {
        private const string DefaultLanguage = "English";
        private const string LocalizationResourcePath = "Data/Localization"; // Resources/Data/Localization.csv

        private string _currentLanguage = DefaultLanguage;
        private readonly Dictionary<string, int> _languageColumnIndices = new Dictionary<string, int>();
        private readonly Dictionary<string, string[]> _localizationTable = new Dictionary<string, string[]>();

        // Professional features
        private readonly Dictionary<string, Dictionary<string, string>> _optimizedCache = new Dictionary<string, Dictionary<string, string>>();
        private readonly HashSet<string> _missingKeys = new HashSet<string>();
        private readonly HashSet<string> _reportedMissingKeys = new HashSet<string>();
        private readonly List<string> _fallbackLanguages = new List<string> { "English" };

        // Binding System
        // Maps Owner -> List of (Key, UpdateAction)
        private readonly Dictionary<object, List<(string Key, Action<string> Action)>> _bindings = new Dictionary<object, List<(string, Action<string>)>>();

        public string CurrentLanguage => _currentLanguage;
        public IReadOnlyCollection<string> AvailableLanguages => _languageColumnIndices.Keys;
        public IReadOnlyCollection<string> MissingKeys => _missingKeys;

        public event Action OnLanguageChanged;

        public void Initialize()
        {
            LoadLocalizationData();
            AutoDetectLanguage();
        }

        private void AutoDetectLanguage()
        {
            // Try to auto-detect system language
            string systemLanguage = Application.systemLanguage.ToString();

            // Map Unity's SystemLanguage to our language names
            string detectedLanguage = systemLanguage switch
            {
                "Spanish" => "Spanish",
                "English" => "English",
                // Add more mappings as needed
                _ => null
            };

            if (!string.IsNullOrEmpty(detectedLanguage) && _languageColumnIndices.ContainsKey(detectedLanguage))
            {
                SetLanguage(detectedLanguage);
                GameLogger.Log($"[Localization] Auto-detected language: {detectedLanguage}");
            }
            else
            {
                GameLogger.Log($"[Localization] Using default language: {DefaultLanguage}");
            }
        }

        private void LoadLocalizationData()
        {
            TextAsset csvFile = Resources.Load<TextAsset>(LocalizationResourcePath);
            if (csvFile == null)
            {
                GameLogger.LogError($"Localization file not found at Resources/{LocalizationResourcePath}");
                return;
            }

            ParseCSV(csvFile.text);
        }

        private void ParseCSV(string csvText)
        {
            string[] lines = csvText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) return;

            // Header Row: Key, English, Spanish, ...
            string[] headers = ParseCSVLine(lines[0]);

            // Map Language Codes to Column Indices
            for (int i = 1; i < headers.Length; i++)
            {
                string lang = headers[i].Trim();
                _languageColumnIndices[lang] = i;
                GameLogger.Log($"[Localization] Loaded language: {lang} at index {i}");
            }

            // Data Rows
            for (int i = 1; i < lines.Length; i++)
            {
                string[] row = ParseCSVLine(lines[i]);
                if (row.Length > 0)
                {
                    string key = row[0].Trim();
                    if (!string.IsNullOrEmpty(key))
                    {
                        _localizationTable[key] = row;
                    }
                }
            }
        }

        // Robust CSV parser handling quotes, commas, and multi-line values
        private string[] ParseCSVLine(string line)
        {
            List<string> result = new List<string>();
            StringBuilder currentField = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        // Check if it's an escaped quote (double quote)
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            currentField.Append('"');
                            i++; // Skip next quote
                        }
                        else
                        {
                            inQuotes = false; // End of quoted field
                        }
                    }
                    else
                    {
                        currentField.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else if (c == ',')
                    {
                        result.Add(currentField.ToString());
                        currentField.Clear();
                    }
                    else
                    {
                        currentField.Append(c);
                    }
                }
            }

            // Add the last field
            result.Add(currentField.ToString());

            return result.ToArray();
        }

        public void SetLanguage(string languageCode)
        {
            if (_languageColumnIndices.ContainsKey(languageCode))
            {
                _currentLanguage = languageCode;
                BuildOptimizedCache(); // Rebuild cache for new language
                BuildOptimizedCache(); // Rebuild cache for new language
                OnLanguageChanged?.Invoke();
                UpdateBindings();
                GameLogger.Log($"[Localization] Language set to: {_currentLanguage}");
            }
            else
            {
                GameLogger.LogWarning($"[Localization] Language '{languageCode}' not found.");
            }
        }

        private void BuildOptimizedCache()
        {
            // Build optimized dictionary for current language
            if (!_optimizedCache.TryGetValue(_currentLanguage, out var cache))
            {
                cache = new Dictionary<string, string>();
                _optimizedCache[_currentLanguage] = cache;

                foreach (var kvp in _localizationTable)
                {
                    string key = kvp.Key;
                    string[] row = kvp.Value;

                    if (_languageColumnIndices.TryGetValue(_currentLanguage, out int index) && index < row.Length)
                    {
                        string value = row[index];
                        if (!string.IsNullOrEmpty(value))
                        {
                            cache[key] = value;
                        }
                        else
                        {
                            // Fallback to default language
                            if (_languageColumnIndices.TryGetValue(DefaultLanguage, out int defaultIndex) && defaultIndex < row.Length)
                            {
                                cache[key] = row[defaultIndex];
                            }
                        }
                    }
                }

                GameLogger.Log($"[Localization] Built cache for {_currentLanguage}: {cache.Count} entries");
            }
        }

        public string GetText(string key)
        {
            // Use optimized cache if available
            if (_optimizedCache.TryGetValue(_currentLanguage, out var cache))
            {
                if (cache.TryGetValue(key, out string cachedValue))
                {
                    return cachedValue;
                }
            }

            // Fallback to direct lookup
            if (!_localizationTable.TryGetValue(key, out string[] row))
            {
                TrackMissingKey(key);
                return $"[{key}]"; // Missing key indicator
            }

            if (_languageColumnIndices.TryGetValue(_currentLanguage, out int index))
            {
                if (index < row.Length)
                {
                    string value = row[index];
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value;
                    }

                    // Try fallback languages
                    foreach (var fallbackLang in _fallbackLanguages)
                    {
                        if (_languageColumnIndices.TryGetValue(fallbackLang, out int fallbackIndex) && fallbackIndex < row.Length)
                        {
                            string fallbackValue = row[fallbackIndex];
                            if (!string.IsNullOrEmpty(fallbackValue))
                            {
                                return fallbackValue;
                            }
                        }
                    }
                }
            }

            TrackMissingKey(key);
            return $"[{key}]";
        }

        private void TrackMissingKey(string key)
        {
            _missingKeys.Add(key);

            // Only report each missing key once to avoid spam
            if (!_reportedMissingKeys.Contains(key))
            {
                _reportedMissingKeys.Add(key);
                GameLogger.LogWarning($"[Localization] Missing key: '{key}' in language '{_currentLanguage}'");
            }
        }

        public string GetText(string key, params object[] args)
        {
            string text = GetText(key);

            // Don't try to format if it's a missing key indicator
            if (text.StartsWith("[") && text.EndsWith("]"))
            {
                return text;
            }

            try
            {
                return string.Format(text, args);
            }
            catch (FormatException ex)
            {
                GameLogger.LogWarning($"[Localization] Format error for key '{key}': {ex.Message}");
                return text;
            }
        }

        public bool HasKey(string key)
        {
            return _localizationTable.ContainsKey(key);
        }

        // Professional feature: Pluralization support
        public string GetTextPlural(string keyPrefix, int count)
        {
            // Simple pluralization: keyPrefix_zero, keyPrefix_one, keyPrefix_other
            string key = count switch
            {
                0 => $"{keyPrefix}_zero",
                1 => $"{keyPrefix}_one",
                _ => $"{keyPrefix}_other"
            };

            // Try specific plural key first
            if (HasKey(key))
            {
                return GetText(key, count);
            }

            // Fallback to base key
            return GetText(keyPrefix, count);
        }

        // Professional feature: Get missing keys report
        public string GetMissingKeysReport()
        {
            if (_missingKeys.Count == 0)
            {
                return "No missing keys found.";
            }

            var report = new StringBuilder();
            report.AppendLine($"Missing Keys Report ({_missingKeys.Count} keys):");
            report.AppendLine("------------------------------------");

            foreach (var key in _missingKeys.OrderBy(k => k))
            {
                report.AppendLine($"- {key}");
            }

            return report.ToString();
        }

        // Professional feature: Clear missing keys tracker (useful for testing)
        public void ClearMissingKeys()
        {
            _missingKeys.Clear();
            _reportedMissingKeys.Clear();
            GameLogger.Log("[Localization] Missing keys tracker cleared.");
        }

        // Professional feature: Reload localization data (useful for hot-reload during development)
        public void Reload()
        {
            _localizationTable.Clear();
            _languageColumnIndices.Clear();
            _optimizedCache.Clear();
            _missingKeys.Clear();
            _reportedMissingKeys.Clear();

            LoadLocalizationData();
            BuildOptimizedCache();
            OnLanguageChanged?.Invoke();

            GameLogger.Log("[Localization] Localization data reloaded.");
        }

        // Professional feature: Validate all keys in localization table
        public Dictionary<string, List<string>> ValidateAllKeys()
        {
            var missingTranslations = new Dictionary<string, List<string>>();

            foreach (var kvp in _localizationTable)
            {
                string key = kvp.Key;
                string[] row = kvp.Value;

                foreach (var langKvp in _languageColumnIndices)
                {
                    string language = langKvp.Key;
                    int index = langKvp.Value;

                    if (index >= row.Length || string.IsNullOrEmpty(row[index]))
                    {
                        if (!missingTranslations.ContainsKey(key))
                        {
                            missingTranslations[key] = new List<string>();
                        }
                        missingTranslations[key].Add(language);
                    }
                }
            }

            if (missingTranslations.Count > 0)
            {
                GameLogger.LogWarning($"[Localization] Validation found {missingTranslations.Count} keys with missing translations.");
            }
            else
            {
                GameLogger.Log("[Localization] All keys have complete translations.");
            }

            return missingTranslations;
        }

        // Professional feature: Get localization statistics
        public string GetStatistics()
        {
            var stats = new StringBuilder();
            stats.AppendLine("Localization Statistics:");
            stats.AppendLine($"- Total Keys: {_localizationTable.Count}");
            stats.AppendLine($"- Available Languages: {_languageColumnIndices.Count}");
            stats.AppendLine($"- Current Language: {_currentLanguage}");
            stats.AppendLine($"- Cached Languages: {_optimizedCache.Count}");
            stats.AppendLine($"- Missing Keys: {_missingKeys.Count}");
            stats.AppendLine($"- Active Bindings (Owners): {_bindings.Count}");

            return stats.ToString();
        }

        public void RegisterBinding(object owner, string key, Action<string> onUpdate)
        {
            if (owner == null) return;

            if (!_bindings.ContainsKey(owner))
            {
                _bindings[owner] = new List<(string, Action<string>)>();
            }

            // Add binding
            _bindings[owner].Add((key, onUpdate));

            // Execute immediately
            onUpdate?.Invoke(GetText(key));
        }

        public void UnregisterAll(object owner)
        {
            if (owner != null && _bindings.ContainsKey(owner))
            {
                _bindings.Remove(owner);
            }
        }

        private void UpdateBindings()
        {
            foreach (var group in _bindings.Values)
            {
                foreach (var binding in group)
                {
                    binding.Action?.Invoke(GetText(binding.Key));
                }
            }
        }
    }
}
