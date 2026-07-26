namespace Playcenter.Services
{
    /// <summary>
    /// Key/value persistence behind <see cref="ISettingsService"/>.
    /// Keeps PlayerPrefs (or any backend) out of the service core for testability.
    /// </summary>
    public interface ISettingsStore
    {
        float GetFloat(string key, float defaultValue);

        void SetFloat(string key, float value);

        int GetInt(string key, int defaultValue);

        void SetInt(string key, int value);

        string GetString(string key, string defaultValue);

        void SetString(string key, string value);

        void Save();
    }
}
