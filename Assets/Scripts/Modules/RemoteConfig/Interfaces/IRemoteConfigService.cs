using System;
using Modules.Shared.Interfaces;
using Cysharp.Threading.Tasks;

namespace Modules.RemoteConfig
{
    /// <summary>
    /// Core service interface for managing remote configuration
    /// Provides centralized access to all configuration domains
    /// </summary>
    public interface IRemoteConfigService : IInitializable
    {
        // Initialization
        
        /// <summary>
        /// Initializes the remote config service and fetches initial configuration
        /// </summary>
        /// <returns>True if initialization succeeded, false otherwise</returns>
        UniTask<bool> Initialize();
        
        // Configuration Access
        
        /// <summary>
        /// Gets a configuration model by type
        /// </summary>
        /// <typeparam name="T">Type of configuration model to retrieve</typeparam>
        /// <returns>Configuration model instance, or null if not found</returns>
        T GetConfig<T>() where T : class, IConfigModel;
        
        /// <summary>
        /// Attempts to get a configuration model by type
        /// </summary>
        /// <typeparam name="T">Type of configuration model to retrieve</typeparam>
        /// <param name="config">Output parameter for configuration model</param>
        /// <returns>True if configuration was found, false otherwise</returns>
        bool TryGetConfig<T>(out T config) where T : class, IConfigModel;
        
        // Configuration Refresh
        
        /// <summary>
        /// Refreshes all configurations from the active provider
        /// </summary>
        /// <returns>True if refresh succeeded, false otherwise</returns>
        UniTask<bool> RefreshConfig();
        
        /// <summary>
        /// Refreshes a specific configuration by type
        /// </summary>
        /// <typeparam name="T">Type of configuration model to refresh</typeparam>
        /// <returns>True if refresh succeeded, false otherwise</returns>
        UniTask<bool> RefreshConfig<T>() where T : class, IConfigModel;
        

        
        // Status
        
        /// <summary>
        /// Gets the current health status of the configuration system
        /// </summary>
        ConfigHealthStatus HealthStatus { get; }
        
        /// <summary>
        /// Gets the timestamp of the last successful configuration update
        /// </summary>
        DateTime LastUpdateTime { get; }
        
        // Events
        
        /// <summary>
        /// Event fired when any configuration is updated
        /// </summary>
        event Action<IConfigModel> OnConfigUpdated;
        
        /// <summary>
        /// Event fired when a specific configuration type is updated
        /// </summary>
        event Action<Type, IConfigModel> OnSpecificConfigUpdated;
        
        /// <summary>
        /// Event fired when the health status changes
        /// </summary>
        event Action<ConfigHealthStatus> OnHealthStatusChanged;
    }
}
