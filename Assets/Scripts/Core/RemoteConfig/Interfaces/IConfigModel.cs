namespace Core.Core.RemoteConfig.Interfaces
{
    /// <summary>
    /// Base interface for all configuration models
    /// </summary>
    public interface IConfigModel
    {
        /// <summary>
        /// Validates the configuration data
        /// </summary>
        /// <returns>True if configuration is valid, false otherwise</returns>
        bool Validate();
    }
}
