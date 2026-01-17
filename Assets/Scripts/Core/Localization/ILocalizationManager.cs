using System.Collections.Generic;

namespace Core.Localization
{
    public interface ILocalizationManager
    {
        string CurrentLanguage { get; }
        IReadOnlyCollection<string> AvailableLanguages { get; }
        IReadOnlyCollection<string> MissingKeys { get; }

        void Initialize();
        void SetLanguage(string languageCode);
        string GetText(string key);
        string GetText(string key, params object[] args);
        bool HasKey(string key);

        // Professional features
        string GetTextPlural(string keyPrefix, int count);
        string GetMissingKeysReport();
        void ClearMissingKeys();
        void Reload();
        Dictionary<string, List<string>> ValidateAllKeys();
        string GetStatistics();

        event System.Action OnLanguageChanged;

        // Binding System
        void RegisterBinding(object owner, string key, System.Action<string> onUpdate);
        void UnregisterAll(object owner);
    }
}