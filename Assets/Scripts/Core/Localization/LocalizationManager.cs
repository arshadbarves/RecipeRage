using System;
using System.Collections.Generic;
using System.Linq;
using Core.Logging;
using UnityEngine;

namespace Core.Localization
{
    public class LocalizationManager : ILocalizationManager
    {
        private const string DefaultLanguage = "English";
        private const string LocalizationResourcePath = "Data/Localization"; // Resources/Data/Localization.csv

        private string _currentLanguage = DefaultLanguage;
        private readonly Dictionary<string, int> _languageColumnIndices = new Dictionary<string, int>();
        private readonly Dictionary<string, string[]> _localizationTable = new Dictionary<string, string[]>();

        public string CurrentLanguage => _currentLanguage;

        public event Action OnLanguageChanged;

        public void Initialize()
        {
            LoadLocalizationData();
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

        // Simple CSV parser handling quotes
        private string[] ParseCSVLine(string line)
        {
            // Basic split by comma, ignoring complexity for now. 
            // Ideally, use a robust CSV parser if keys contain commas. 
            // For this implementation, we assume keys/values don't contain commas or are simple.
            return line.Split(',');
        }

        public void SetLanguage(string languageCode)
        {
            if (_languageColumnIndices.ContainsKey(languageCode))
            {
                _currentLanguage = languageCode;
                OnLanguageChanged?.Invoke();
                GameLogger.Log($"[Localization] Language set to: {_currentLanguage}");
            }
            else
            {
                GameLogger.LogWarning($"[Localization] Language '{languageCode}' not found.");
            }
        }

        public string GetText(string key)
        {
            if (!_localizationTable.TryGetValue(key, out string[] row))
            {
                return $"[{key}]"; // Missing key indicator
            }

            if (_languageColumnIndices.TryGetValue(_currentLanguage, out int index))
            {
                if (index < row.Length)
                {
                    string value = row[index];
                    return string.IsNullOrEmpty(value) ? row[_languageColumnIndices[DefaultLanguage]] : value; // Fallback to Default
                }
            }

            return $"[{key}]";
        }

        public string GetText(string key, params object[] args)
        {
            string text = GetText(key);
            try
            {
                return string.Format(text, args);
            }
            catch (FormatException)
            {
                return text;
            }
        }

        public bool HasKey(string key)
        {
            return _localizationTable.ContainsKey(key);
        }
    }
}
