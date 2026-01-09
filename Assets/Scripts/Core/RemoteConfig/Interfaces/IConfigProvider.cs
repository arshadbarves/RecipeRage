using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Core.Core.RemoteConfig.Interfaces
{
    /// <summary>
    /// Interface for configuration data providers
    /// Abstracts the source of configuration data (Firebase, Local, etc.)
    /// </summary>
    public interface IConfigProvider
    {
        /// <summary>
        /// Name of this provider for logging and debugging
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Checks if this provider is currently available
        /// </summary>
        /// <returns>True if provider can be used, false otherwise</returns>
        bool IsAvailable();

        /// <summary>
        /// Initializes the configuration provider
        /// </summary>
        /// <returns>True if initialization succeeded, false otherwise</returns>
        UniTask<bool> Initialize();

        /// <summary>
        /// Fetches a specific configuration by key
        /// </summary>
        /// <typeparam name="T">Type of configuration model to fetch</typeparam>
        /// <param name="key">Configuration key identifier</param>
        /// <returns>Configuration model instance</returns>
        UniTask<T> FetchConfig<T>(string key) where T : IConfigModel;

        /// <summary>
        /// Fetches all available configurations
        /// </summary>
        /// <returns>Dictionary of configuration key to model instance</returns>
        UniTask<Dictionary<string, IConfigModel>> FetchAllConfigs();
    }
}
