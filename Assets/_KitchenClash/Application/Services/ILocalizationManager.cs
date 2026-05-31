using System;
using System.Collections.Generic;

namespace KitchenClash.Application.Services
{
    public interface ILocalizationManager
    {
        string CurrentLanguage { get; }
        IReadOnlyCollection<string> AvailableLanguages { get; }
        void Initialize();
        void SetLanguage(string languageCode);
        string GetText(string key);
        string GetText(string key, params object[] args);
        bool HasKey(string key);
        void Reload();
        void RegisterBinding(object owner, string key, Action<string> onUpdate);
        void UnregisterAll(object owner);
    }
}
