using System;

namespace Core.RemoteConfig
{
    /// <summary>
    /// Base interface for all configuration models
    /// Provides common properties for versioning, validation, and tracking
    /// </summary>
    public interface IConfigModel
    {
        /// <summary>
        /// Unique identifier for this configuration domain
        /// </summary>
        string ConfigKey { get; }
        
        /// <summary>
        /// Version string for this configuration
        /// </summary>
        string Version { get; }
        
        /// <summary>
        /// Validates the configuration data
        /// </summary>
        /// <returns>True if configuration is valid, false otherwise</returns>
        bool Validate();
        
        /// <summary>
        /// Timestamp of when this configuration was last modified
        /// </summary>
        DateTime LastModified { get; }
    }
}
