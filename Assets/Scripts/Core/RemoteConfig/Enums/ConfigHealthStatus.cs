namespace Core.RemoteConfig
{
    /// <summary>
    /// Defines the health status of the configuration system
    /// </summary>
    public enum ConfigHealthStatus
    {
        /// <summary>
        /// Configuration system is operating normally
        /// All configurations loaded and validated successfully
        /// </summary>
        Healthy,
        
        /// <summary>
        /// Configuration system is partially operational
        /// Some configurations may be using fallback values
        /// </summary>
        Degraded,
        
        /// <summary>
        /// Configuration system has failed
        /// Unable to load configurations from any source
        /// </summary>
        Failed
    }
}
