using System.Collections.Generic;

namespace Core.Localization
{
    public interface ILocalizationManager
    {
        string CurrentLanguage { get; }
        void SetLanguage(string languageCode);
        string GetText(string key);
        string GetText(string key, params object[] args);
        bool HasKey(string key);
    }
}