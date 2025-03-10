using System;
using System.Collections.Generic;

namespace RecipeRage.Modules.Cloud.Interfaces
{
    /// <summary>
    /// Interface for cloud storage services
    /// Provides unified cloud storage capabilities throughout the application
    /// 
    /// Complexity Rating: 3
    /// </summary>
    public interface ICloudStorageService
    {
        /// <summary>
        /// Event triggered when a file is saved
        /// </summary>
        event Action<string, bool> OnFileSaved;
        
        /// <summary>
        /// Event triggered when a file is loaded
        /// </summary>
        event Action<string, bool> OnFileLoaded;
        
        /// <summary>
        /// Event triggered when a file is deleted
        /// </summary>
        event Action<string, bool> OnFileDeleted;
        
        /// <summary>
        /// Event triggered when synchronization occurs
        /// </summary>
        event Action<bool, int, string> OnSyncCompleted;
        
        /// <summary>
        /// Initialize the cloud storage service
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
        /// Synchronize local cache with cloud
        /// </summary>
        /// <param name="onComplete">Callback when sync is complete</param>
        void Sync(Action<bool, int, string> onComplete = null);
        
        /// <summary>
        /// Get the total storage quota
        /// </summary>
        /// <param name="onComplete">Callback with quota information</param>
        void GetStorageQuota(Action<long, long, string> onComplete);
        
        /// <summary>
        /// Check if the service is initialized
        /// </summary>
        /// <returns>True if initialized, false otherwise</returns>
        bool IsInitialized();
        
        /// <summary>
        /// Add a storage provider
        /// </summary>
        /// <param name="provider">The provider to add</param>
        void AddProvider(ICloudStorageProvider provider);
        
        /// <summary>
        /// Get provider by name
        /// </summary>
        /// <param name="providerName">Name of the provider</param>
        /// <returns>The provider if found, null otherwise</returns>
        ICloudStorageProvider GetProvider(string providerName);
        
        /// <summary>
        /// Get status of the cloud storage service
        /// </summary>
        /// <returns>Service status</returns>
        CloudStorageStatus GetStatus();
    }
} 