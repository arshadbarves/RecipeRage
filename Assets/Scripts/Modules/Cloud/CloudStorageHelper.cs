using System;
using System.Collections.Generic;
using UnityEngine;
using RecipeRage.Modules.Cloud.Core;
using RecipeRage.Modules.Cloud.Interfaces;
using RecipeRage.Modules.Cloud.Providers.EOS;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Modules.Cloud
{
    /// <summary>
    /// Static helper class for easy access to cloud storage functionality
    /// 
    /// Complexity Rating: 2
    /// </summary>
    public static class CloudStorageHelper
    {
        private static ICloudStorageService _cloudStorageService;
        private static bool _isInitialized = false;
        
        /// <summary>
        /// Static constructor
        /// </summary>
        static CloudStorageHelper()
        {
            // Create service instance if it doesn't exist
            if (_cloudStorageService == null)
            {
                _cloudStorageService = new CloudStorageService();
                LogHelper.Info("CloudStorageHelper", "Created cloud storage service instance");
            }
        }
        
        /// <summary>
        /// Initialize the cloud storage service
        /// </summary>
        /// <param name="onComplete">Callback when initialization is complete</param>
        public static void Initialize(Action<bool> onComplete = null)
        {
            if (_isInitialized)
            {
                LogHelper.Warning("CloudStorageHelper", "Cloud storage service is already initialized");
                onComplete?.Invoke(true);
                return;
            }
            
            EnsureServiceCreated();
            
            // Add EOS provider
            AddEOSProvider();
            
            // Initialize
            _cloudStorageService.Initialize(success =>
            {
                if (success)
                {
                    _isInitialized = true;
                    LogHelper.Info("CloudStorageHelper", "Cloud storage service initialized successfully");
                }
                else
                {
                    LogHelper.Error("CloudStorageHelper", "Failed to initialize cloud storage service");
                }
                
                onComplete?.Invoke(success);
            });
        }
        
        /// <summary>
        /// Save a file to cloud storage
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="data">Data to save</param>
        /// <param name="onComplete">Callback when save is complete</param>
        public static void SaveFile(string fileName, byte[] data, Action<bool, string> onComplete = null)
        {
            if (!CheckInitialized("SaveFile"))
            {
                onComplete?.Invoke(false, "Cloud storage service is not initialized");
                return;
            }
            
            _cloudStorageService.SaveFile(fileName, data, onComplete);
        }
        
        /// <summary>
        /// Load a file from cloud storage
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="onComplete">Callback with loaded data</param>
        public static void LoadFile(string fileName, Action<byte[], string> onComplete)
        {
            if (!CheckInitialized("LoadFile"))
            {
                onComplete?.Invoke(null, "Cloud storage service is not initialized");
                return;
            }
            
            _cloudStorageService.LoadFile(fileName, onComplete);
        }
        
        /// <summary>
        /// Delete a file from cloud storage
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="onComplete">Callback when deletion is complete</param>
        public static void DeleteFile(string fileName, Action<bool, string> onComplete = null)
        {
            if (!CheckInitialized("DeleteFile"))
            {
                onComplete?.Invoke(false, "Cloud storage service is not initialized");
                return;
            }
            
            _cloudStorageService.DeleteFile(fileName, onComplete);
        }
        
        /// <summary>
        /// List files in cloud storage
        /// </summary>
        /// <param name="onComplete">Callback with file list</param>
        public static void ListFiles(Action<List<CloudFileMetadata>, string> onComplete)
        {
            if (!CheckInitialized("ListFiles"))
            {
                onComplete?.Invoke(null, "Cloud storage service is not initialized");
                return;
            }
            
            _cloudStorageService.ListFiles(onComplete);
        }
        
        /// <summary>
        /// Get file metadata
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="onComplete">Callback with file metadata</param>
        public static void GetFileMetadata(string fileName, Action<CloudFileMetadata, string> onComplete)
        {
            if (!CheckInitialized("GetFileMetadata"))
            {
                onComplete?.Invoke(null, "Cloud storage service is not initialized");
                return;
            }
            
            _cloudStorageService.GetFileMetadata(fileName, onComplete);
        }
        
        /// <summary>
        /// Check if a file exists in cloud storage
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="onComplete">Callback with existence check result</param>
        public static void FileExists(string fileName, Action<bool> onComplete)
        {
            if (!CheckInitialized("FileExists"))
            {
                onComplete?.Invoke(false);
                return;
            }
            
            _cloudStorageService.FileExists(fileName, onComplete);
        }
        
        /// <summary>
        /// Synchronize local cache with cloud
        /// </summary>
        /// <param name="onComplete">Callback when sync is complete</param>
        public static void Sync(Action<bool, int, string> onComplete = null)
        {
            if (!CheckInitialized("Sync"))
            {
                onComplete?.Invoke(false, 0, "Cloud storage service is not initialized");
                return;
            }
            
            _cloudStorageService.Sync(onComplete);
        }
        
        /// <summary>
        /// Get the total storage quota
        /// </summary>
        /// <param name="onComplete">Callback with quota information</param>
        public static void GetStorageQuota(Action<long, long, string> onComplete)
        {
            if (!CheckInitialized("GetStorageQuota"))
            {
                onComplete?.Invoke(0, 0, "Cloud storage service is not initialized");
                return;
            }
            
            _cloudStorageService.GetStorageQuota(onComplete);
        }
        
        /// <summary>
        /// Check if the service is initialized
        /// </summary>
        /// <returns>True if initialized, false otherwise</returns>
        public static bool IsInitialized()
        {
            return _isInitialized && _cloudStorageService != null && _cloudStorageService.IsInitialized();
        }
        
        /// <summary>
        /// Get cloud storage service status
        /// </summary>
        /// <returns>Service status</returns>
        public static CloudStorageStatus GetStatus()
        {
            if (!_isInitialized || _cloudStorageService == null)
            {
                return new CloudStorageStatus
                {
                    IsInitialized = false,
                    LastError = "Cloud storage service is not initialized"
                };
            }
            
            return _cloudStorageService.GetStatus();
        }
        
        /// <summary>
        /// Register for file saved events
        /// </summary>
        /// <param name="callback">Callback for file saved events</param>
        public static void RegisterFileSavedCallback(Action<string, bool> callback)
        {
            if (!CheckInitialized("RegisterFileSavedCallback"))
            {
                return;
            }
            
            _cloudStorageService.OnFileSaved += callback;
        }
        
        /// <summary>
        /// Unregister from file saved events
        /// </summary>
        /// <param name="callback">Callback to unregister</param>
        public static void UnregisterFileSavedCallback(Action<string, bool> callback)
        {
            if (_cloudStorageService != null)
            {
                _cloudStorageService.OnFileSaved -= callback;
            }
        }
        
        /// <summary>
        /// Register for file loaded events
        /// </summary>
        /// <param name="callback">Callback for file loaded events</param>
        public static void RegisterFileLoadedCallback(Action<string, bool> callback)
        {
            if (!CheckInitialized("RegisterFileLoadedCallback"))
            {
                return;
            }
            
            _cloudStorageService.OnFileLoaded += callback;
        }
        
        /// <summary>
        /// Unregister from file loaded events
        /// </summary>
        /// <param name="callback">Callback to unregister</param>
        public static void UnregisterFileLoadedCallback(Action<string, bool> callback)
        {
            if (_cloudStorageService != null)
            {
                _cloudStorageService.OnFileLoaded -= callback;
            }
        }
        
        /// <summary>
        /// Register for file deleted events
        /// </summary>
        /// <param name="callback">Callback for file deleted events</param>
        public static void RegisterFileDeletedCallback(Action<string, bool> callback)
        {
            if (!CheckInitialized("RegisterFileDeletedCallback"))
            {
                return;
            }
            
            _cloudStorageService.OnFileDeleted += callback;
        }
        
        /// <summary>
        /// Unregister from file deleted events
        /// </summary>
        /// <param name="callback">Callback to unregister</param>
        public static void UnregisterFileDeletedCallback(Action<string, bool> callback)
        {
            if (_cloudStorageService != null)
            {
                _cloudStorageService.OnFileDeleted -= callback;
            }
        }
        
        /// <summary>
        /// Register for sync completed events
        /// </summary>
        /// <param name="callback">Callback for sync completed events</param>
        public static void RegisterSyncCompletedCallback(Action<bool, int, string> callback)
        {
            if (!CheckInitialized("RegisterSyncCompletedCallback"))
            {
                return;
            }
            
            _cloudStorageService.OnSyncCompleted += callback;
        }
        
        /// <summary>
        /// Unregister from sync completed events
        /// </summary>
        /// <param name="callback">Callback to unregister</param>
        public static void UnregisterSyncCompletedCallback(Action<bool, int, string> callback)
        {
            if (_cloudStorageService != null)
            {
                _cloudStorageService.OnSyncCompleted -= callback;
            }
        }
        
        /// <summary>
        /// Set the cloud storage service instance
        /// </summary>
        /// <param name="service">Service instance</param>
        public static void SetCloudStorageService(ICloudStorageService service)
        {
            if (service == null)
            {
                LogHelper.Error("CloudStorageHelper", "Cannot set null cloud storage service");
                return;
            }
            
            _cloudStorageService = service;
            _isInitialized = service.IsInitialized();
            LogHelper.Info("CloudStorageHelper", "Cloud storage service instance set externally");
        }
        
        /// <summary>
        /// Add a provider to the service
        /// </summary>
        /// <param name="provider">Provider to add</param>
        public static void AddProvider(ICloudStorageProvider provider)
        {
            if (provider == null)
            {
                LogHelper.Error("CloudStorageHelper", "Cannot add null provider");
                return;
            }
            
            EnsureServiceCreated();
            _cloudStorageService.AddProvider(provider);
            LogHelper.Info("CloudStorageHelper", $"Added provider: {provider.GetProviderName()}");
        }
        
        /// <summary>
        /// Add EOS provider
        /// </summary>
        private static void AddEOSProvider()
        {
            try
            {
                var eosProvider = new EOSPlayerDataStorageProvider();
                AddProvider(eosProvider);
            }
            catch (Exception ex)
            {
                LogHelper.Error("CloudStorageHelper", $"Failed to create EOS provider: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Ensure service instance is created
        /// </summary>
        private static void EnsureServiceCreated()
        {
            if (_cloudStorageService == null)
            {
                _cloudStorageService = new CloudStorageService();
                LogHelper.Info("CloudStorageHelper", "Created cloud storage service instance");
            }
        }
        
        /// <summary>
        /// Check if the cloud storage service is initialized
        /// </summary>
        /// <param name="methodName">Method name for logging</param>
        /// <returns>True if initialized, false otherwise</returns>
        private static bool CheckInitialized(string methodName = null)
        {
            if (!_isInitialized || _cloudStorageService == null || !_cloudStorageService.IsInitialized())
            {
                if (!string.IsNullOrEmpty(methodName))
                {
                    LogHelper.Warning("CloudStorageHelper", $"{methodName}: Cloud storage service is not initialized");
                }
                
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Convenient method to save a string to a file
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="text">Text to save</param>
        /// <param name="onComplete">Callback when save is complete</param>
        public static void SaveText(string fileName, string text, Action<bool, string> onComplete = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                onComplete?.Invoke(false, "Text cannot be empty");
                return;
            }
            
            byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
            SaveFile(fileName, data, onComplete);
        }
        
        /// <summary>
        /// Convenient method to load a string from a file
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="onComplete">Callback with loaded text</param>
        public static void LoadText(string fileName, Action<string, string> onComplete)
        {
            LoadFile(fileName, (data, error) =>
            {
                if (data != null)
                {
                    try
                    {
                        string text = System.Text.Encoding.UTF8.GetString(data);
                        onComplete?.Invoke(text, null);
                    }
                    catch (Exception ex)
                    {
                        onComplete?.Invoke(null, $"Failed to convert data to string: {ex.Message}");
                    }
                }
                else
                {
                    onComplete?.Invoke(null, error);
                }
            });
        }
        
        /// <summary>
        /// Convenient method to save a JSON object to a file
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="fileName">Name of the file</param>
        /// <param name="obj">Object to save</param>
        /// <param name="onComplete">Callback when save is complete</param>
        public static void SaveJson<T>(string fileName, T obj, Action<bool, string> onComplete = null)
        {
            if (obj == null)
            {
                onComplete?.Invoke(false, "Object cannot be null");
                return;
            }
            
            try
            {
                string json = JsonUtility.ToJson(obj);
                SaveText(fileName, json, onComplete);
            }
            catch (Exception ex)
            {
                onComplete?.Invoke(false, $"Failed to serialize object to JSON: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Convenient method to load a JSON object from a file
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="fileName">Name of the file</param>
        /// <param name="onComplete">Callback with loaded object</param>
        public static void LoadJson<T>(string fileName, Action<T, string> onComplete)
        {
            LoadText(fileName, (json, error) =>
            {
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        T obj = JsonUtility.FromJson<T>(json);
                        onComplete?.Invoke(obj, null);
                    }
                    catch (Exception ex)
                    {
                        onComplete?.Invoke(default, $"Failed to deserialize JSON: {ex.Message}");
                    }
                }
                else
                {
                    onComplete?.Invoke(default, error);
                }
            });
        }
    }
} 