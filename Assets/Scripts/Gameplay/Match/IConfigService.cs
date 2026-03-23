namespace Gameplay.Match
{
    /// <summary>
    /// Small config adapter for GDD-owned gameplay values that may come from remote config.
    /// </summary>
    public interface IConfigService
    {
        int GetInt(string key, int defaultValue);
        float GetFloat(string key, float defaultValue);
        bool GetBool(string key, bool defaultValue);
        string GetString(string key, string defaultValue);
    }
}
