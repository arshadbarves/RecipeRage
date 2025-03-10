using System;
using System.Collections.Generic;

namespace RecipeRage.Modules.Cloud.Interfaces
{
    /// <summary>
    /// Interface for cloud storage providers
    /// Implemented by specific cloud storage services
    /// 
    /// Complexity Rating: 3
    /// </summary>
    public interface ICloudStorageProvider
    {
        /// <summary>
        /// Initialize the provider
        /// </summary>
        /// <param name="onComplete">Callback when initialization is complete</param>
        void Initialize(Action<bool> onComplete = null);
        
        /// <summary>
        /// Save data to cloud storage
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="data">Data to save</param>
        /// <param name="onComplete">Callback when save is complete</param>
        void SaveFile(string fileName, byte[] data, Action<bool, string> onComplete = null);
        
        /// <summary>
        /// Load data from cloud storage
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="onComplete">Callback with loaded data</param>
        void LoadFile(string fileName, Action<byte[], string> onComplete);
        
        /// <summary>
        /// Delete a file from cloud storage
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="onComplete">Callback when deletion is complete</param>
        void DeleteFile(string fileName, Action<bool, string> onComplete = null);
        
        /// <summary>
        /// List files in cloud storage
        /// </summary>
        /// <param name="onComplete">Callback with file list</param>
        void ListFiles(Action<List<CloudFileMetadata>, string> onComplete);
        
        /// <summary>
        /// Get file metadata
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="onComplete">Callback with file metadata</param>
        void GetFileMetadata(string fileName, Action<CloudFileMetadata, string> onComplete);
        
        /// <summary>
        /// Check if a file exists in cloud storage
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="onComplete">Callback with existence check result</param>
        void FileExists(string fileName, Action<bool> onComplete);
        
        /// <summary>
        /// Get the total storage quota
        /// </summary>
        /// <param name="onComplete">Callback with quota information</param>
        void GetStorageQuota(Action<long, long, string> onComplete);
        
        /// <summary>
        /// Get the provider name
        /// </summary>
        /// <returns>Provider name</returns>
        string GetProviderName();
        
        /// <summary>
        /// Check if the provider is available
        /// </summary>
        /// <returns>True if available, false otherwise</returns>
        bool IsAvailable();
    }
} 