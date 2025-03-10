using System;
using System.Collections.Generic;

namespace RecipeRage.Modules.Cloud.Interfaces
{
    /// <summary>
    /// Status information for cloud storage service
    /// 
    /// Complexity Rating: 1
    /// </summary>
    [Serializable]
    public class CloudStorageStatus
    {
        /// <summary>
        /// Whether the service is initialized
        /// </summary>
        public bool IsInitialized { get; set; }
        
        /// <summary>
        /// Number of providers
        /// </summary>
        public int ProviderCount { get; set; }
        
        /// <summary>
        /// Available providers
        /// </summary>
        public List<string> AvailableProviders { get; set; }
        
        /// <summary>
        /// Total files in cloud storage
        /// </summary>
        public int TotalFiles { get; set; }
        
        /// <summary>
        /// Total local cached files
        /// </summary>
        public int LocalCachedFiles { get; set; }
        
        /// <summary>
        /// Last synchronization time
        /// </summary>
        public DateTime LastSyncTime { get; set; }
        
        /// <summary>
        /// Used storage (bytes)
        /// </summary>
        public long UsedStorage { get; set; }
        
        /// <summary>
        /// Total available storage (bytes)
        /// </summary>
        public long TotalStorage { get; set; }
        
        /// <summary>
        /// Last error
        /// </summary>
        public string LastError { get; set; }
        
        /// <summary>
        /// Create a new cloud storage status
        /// </summary>
        public CloudStorageStatus()
        {
            IsInitialized = false;
            ProviderCount = 0;
            AvailableProviders = new List<string>();
            TotalFiles = 0;
            LocalCachedFiles = 0;
            LastSyncTime = DateTime.MinValue;
            UsedStorage = 0;
            TotalStorage = 0;
            LastError = string.Empty;
        }
        
        /// <summary>
        /// Get percentage of used storage
        /// </summary>
        /// <returns>Used storage percentage</returns>
        public float GetUsedPercentage()
        {
            if (TotalStorage <= 0)
            {
                return 0;
            }
            
            return (float)UsedStorage / TotalStorage * 100;
        }
        
        /// <summary>
        /// Get human-readable used storage
        /// </summary>
        /// <returns>Human-readable used storage</returns>
        public string GetReadableUsedStorage()
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = UsedStorage;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            
            return $"{len:0.##} {sizes[order]}";
        }
        
        /// <summary>
        /// Get human-readable total storage
        /// </summary>
        /// <returns>Human-readable total storage</returns>
        public string GetReadableTotalStorage()
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = TotalStorage;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            
            return $"{len:0.##} {sizes[order]}";
        }
    }
} 