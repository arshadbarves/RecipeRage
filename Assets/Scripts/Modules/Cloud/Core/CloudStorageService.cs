using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using RecipeRage.Modules.Cloud.Interfaces;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Modules.Cloud.Core
{
    /// <summary>
    /// Main implementation of the cloud storage service
    /// Manages providers and handles file operations
    /// 
    /// Complexity Rating: 4
    /// </summary>
    public class CloudStorageService : ICloudStorageService
    {
        /// <summary>
        /// List of available providers
        /// </summary>
        private readonly List<ICloudStorageProvider> _providers = new List<ICloudStorageProvider>();
        
        /// <summary>
        /// Cache of file metadata
        /// </summary>
        private Dictionary<string, CloudFileMetadata> _fileMetadataCache = new Dictionary<string, CloudFileMetadata>();
        
        /// <summary>
        /// Whether the service is initialized
        /// </summary>
        private bool _isInitialized = false;
        
        /// <summary>
        /// Local cache directory
        /// </summary>
        private readonly string _localCacheDirectory;
        
        /// <summary>
        /// Last synchronization time
        /// </summary>
        private DateTime _lastSyncTime = DateTime.MinValue;
        
        /// <summary>
        /// Last error message
        /// </summary>
        private string _lastError = string.Empty;
        
        /// <summary>
        /// Event for file saved
        /// </summary>
        public event Action<string, bool> OnFileSaved;
        
        /// <summary>
        /// Event for file loaded
        /// </summary>
        public event Action<string, bool> OnFileLoaded;
        
        /// <summary>
        /// Event for file deleted
        /// </summary>
        public event Action<string, bool> OnFileDeleted;
        
        /// <summary>
        /// Event for sync completed
        /// </summary>
        public event Action<bool, int, string> OnSyncCompleted;
        
        /// <summary>
        /// Create a new cloud storage service
        /// </summary>
        public CloudStorageService()
        {
            // Create local cache directory
            _localCacheDirectory = Path.Combine(Application.persistentDataPath, "CloudStorage");
            if (!Directory.Exists(_localCacheDirectory))
            {
                Directory.CreateDirectory(_localCacheDirectory);
            }
            
            LogHelper.Info("CloudStorageService", $"Created local cache directory: {_localCacheDirectory}");
        }
        
        /// <summary>
        /// Initialize the cloud storage service
        /// </summary>
        /// <param name="onComplete">Callback when initialization is complete</param>
        public void Initialize(Action<bool> onComplete = null)
        {
            if (_isInitialized)
            {
                LogHelper.Warning("CloudStorageService", "Service is already initialized");
                onComplete?.Invoke(true);
                return;
            }
            
            LogHelper.Info("CloudStorageService", "Initializing cloud storage service");
            
            // Check if we have any providers
            if (_providers.Count == 0)
            {
                _lastError = "No providers registered";
                LogHelper.Error("CloudStorageService", _lastError);
                onComplete?.Invoke(false);
                return;
            }
            
            // Initialize all providers
            int successCount = 0;
            int totalCount = _providers.Count;
            
            foreach (var provider in _providers)
            {
                provider.Initialize(success =>
                {
                    if (success)
                    {
                        LogHelper.Info("CloudStorageService", $"Provider {provider.GetProviderName()} initialized successfully");
                        successCount++;
                    }
                    else
                    {
                        LogHelper.Error("CloudStorageService", $"Failed to initialize provider {provider.GetProviderName()}");
                    }
                    
                    // Check if all providers are initialized
                    if (successCount + (totalCount - successCount) == totalCount)
                    {
                        bool allSuccess = successCount == totalCount;
                        _isInitialized = successCount > 0;
                        
                        if (_isInitialized)
                        {
                            LogHelper.Info("CloudStorageService", $"Cloud storage service initialized with {successCount}/{totalCount} providers");
                            // Load metadata cache
                            LoadMetadataCache();
                        }
                        else
                        {
                            _lastError = "Failed to initialize all providers";
                            LogHelper.Error("CloudStorageService", _lastError);
                        }
                        
                        onComplete?.Invoke(_isInitialized);
                    }
                });
            }
        }
        
        /// <summary>
        /// Add a provider to the service
        /// </summary>
        /// <param name="provider">The provider to add</param>
        public void AddProvider(ICloudStorageProvider provider)
        {
            if (provider == null)
            {
                LogHelper.Error("CloudStorageService", "Cannot add null provider");
                return;
            }
            
            // Check if provider already exists
            if (_providers.Any(p => p.GetProviderName() == provider.GetProviderName()))
            {
                LogHelper.Warning("CloudStorageService", $"Provider {provider.GetProviderName()} is already registered");
                return;
            }
            
            _providers.Add(provider);
            LogHelper.Info("CloudStorageService", $"Added provider: {provider.GetProviderName()}");
        }
        
        /// <summary>
        /// Get a provider by name
        /// </summary>
        /// <param name="providerName">Name of the provider</param>
        /// <returns>The provider if found, null otherwise</returns>
        public ICloudStorageProvider GetProvider(string providerName)
        {
            return _providers.FirstOrDefault(p => p.GetProviderName() == providerName);
        }
        
        /// <summary>
        /// Save a file to cloud storage
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="data">Data to save</param>
        /// <param name="onComplete">Callback when save is complete</param>
        public void SaveFile(string fileName, byte[] data, Action<bool, string> onComplete = null)
        {
            if (!_isInitialized)
            {
                string error = "Service is not initialized";
                LogHelper.Error("CloudStorageService", error);
                onComplete?.Invoke(false, error);
                return;
            }
            
            if (string.IsNullOrEmpty(fileName))
            {
                string error = "File name cannot be empty";
                LogHelper.Error("CloudStorageService", error);
                onComplete?.Invoke(false, error);
                return;
            }
            
            if (data == null || data.Length == 0)
            {
                string error = "Data cannot be empty";
                LogHelper.Error("CloudStorageService", error);
                onComplete?.Invoke(false, error);
                return;
            }
            
            // Use the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable());
            if (provider == null)
            {
                string error = "No available providers";
                LogHelper.Error("CloudStorageService", error);
                onComplete?.Invoke(false, error);
                return;
            }
            
            LogHelper.Info("CloudStorageService", $"Saving file {fileName} using provider {provider.GetProviderName()}");
            
            // Save to local cache first
            string localPath = Path.Combine(_localCacheDirectory, fileName);
            try
            {
                File.WriteAllBytes(localPath, data);
                
                // Calculate MD5 hash
                string hash = CalculateMD5(data);
                
                // Create or update metadata
                var metadata = new CloudFileMetadata(
                    fileName: fileName,
                    size: data.Length,
                    lastModified: DateTime.UtcNow,
                    provider: provider.GetProviderName()
                );
                metadata.Hash = hash;
                metadata.IsLocal = true;
                metadata.IsSynced = false;
                
                // Save to cloud
                provider.SaveFile(fileName, data, (success, error) =>
                {
                    if (success)
                    {
                        LogHelper.Info("CloudStorageService", $"File {fileName} saved successfully to cloud");
                        metadata.IsSynced = true;
                    }
                    else
                    {
                        LogHelper.Warning("CloudStorageService", $"File {fileName} saved locally but failed to save to cloud: {error}");
                    }
                    
                    // Update metadata cache
                    UpdateMetadataCache(metadata);
                    
                    // Invoke callbacks
                    OnFileSaved?.Invoke(fileName, success);
                    onComplete?.Invoke(success, error);
                });
            }
            catch (Exception ex)
            {
                string error = $"Failed to save file to local cache: {ex.Message}";
                LogHelper.Error("CloudStorageService", error);
                onComplete?.Invoke(false, error);
            }
        }
        
        /// <summary>
        /// Load a file from cloud storage
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="onComplete">Callback with loaded data</param>
        public void LoadFile(string fileName, Action<byte[], string> onComplete)
        {
            if (!_isInitialized)
            {
                string error = "Service is not initialized";
                LogHelper.Error("CloudStorageService", error);
                onComplete?.Invoke(null, error);
                return;
            }
            
            if (string.IsNullOrEmpty(fileName))
            {
                string error = "File name cannot be empty";
                LogHelper.Error("CloudStorageService", error);
                onComplete?.Invoke(null, error);
                return;
            }
            
            LogHelper.Info("CloudStorageService", $"Loading file {fileName}");
            
            // Check if we have a local copy
            string localPath = Path.Combine(_localCacheDirectory, fileName);
            if (File.Exists(localPath))
            {
                try
                {
                    byte[] data = File.ReadAllBytes(localPath);
                    LogHelper.Info("CloudStorageService", $"File {fileName} loaded from local cache");
                    OnFileLoaded?.Invoke(fileName, true);
                    onComplete?.Invoke(data, null);
                    return;
                }
                catch (Exception ex)
                {
                    LogHelper.Warning("CloudStorageService", $"Failed to load file from local cache: {ex.Message}");
                    // Continue with cloud load
                }
            }
            
            // Use the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable());
            if (provider == null)
            {
                string error = "No available providers";
                LogHelper.Error("CloudStorageService", error);
                onComplete?.Invoke(null, error);
                return;
            }
            
            // Load from cloud
            provider.LoadFile(fileName, (data, error) =>
            {
                bool success = data != null;
                
                if (success)
                {
                    LogHelper.Info("CloudStorageService", $"File {fileName} loaded successfully from cloud");
                    
                    // Save to local cache
                    try
                    {
                        File.WriteAllBytes(localPath, data);
                        
                        // Calculate MD5 hash
                        string hash = CalculateMD5(data);
                        
                        // Update metadata
                        var metadata = new CloudFileMetadata(
                            fileName: fileName,
                            size: data.Length,
                            lastModified: DateTime.UtcNow,
                            provider: provider.GetProviderName()
                        );
                        metadata.Hash = hash;
                        metadata.IsLocal = true;
                        metadata.IsSynced = true;
                        
                        // Update metadata cache
                        UpdateMetadataCache(metadata);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Warning("CloudStorageService", $"Failed to save file to local cache: {ex.Message}");
                    }
                }
                else
                {
                    LogHelper.Error("CloudStorageService", $"Failed to load file from cloud: {error}");
                }
                
                OnFileLoaded?.Invoke(fileName, success);
                onComplete?.Invoke(data, error);
            });
        }
        
        /// <summary>
        /// Delete a file from cloud storage
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="onComplete">Callback when deletion is complete</param>
        public void DeleteFile(string fileName, Action<bool, string> onComplete = null)
        {
            if (!_isInitialized)
            {
                string error = "Service is not initialized";
                LogHelper.Error("CloudStorageService", error);
                onComplete?.Invoke(false, error);
                return;
            }
            
            if (string.IsNullOrEmpty(fileName))
            {
                string error = "File name cannot be empty";
                LogHelper.Error("CloudStorageService", error);
                onComplete?.Invoke(false, error);
                return;
            }
            
            LogHelper.Info("CloudStorageService", $"Deleting file {fileName}");
            
            // Delete from local cache
            string localPath = Path.Combine(_localCacheDirectory, fileName);
            bool localDeleteSuccess = false;
            
            try
            {
                if (File.Exists(localPath))
                {
                    File.Delete(localPath);
                    LogHelper.Info("CloudStorageService", $"File {fileName} deleted from local cache");
                }
                localDeleteSuccess = true;
            }
            catch (Exception ex)
            {
                LogHelper.Warning("CloudStorageService", $"Failed to delete file from local cache: {ex.Message}");
            }
            
            // Use the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable());
            if (provider == null)
            {
                string error = "No available providers";
                LogHelper.Error("CloudStorageService", error);
                
                // If we deleted the local file, return success
                if (localDeleteSuccess)
                {
                    // Remove from metadata cache
                    RemoveFromMetadataCache(fileName);
                    OnFileDeleted?.Invoke(fileName, true);
                    onComplete?.Invoke(true, null);
                    return;
                }
                
                onComplete?.Invoke(false, error);
                return;
            }
            
            // Delete from cloud
            provider.DeleteFile(fileName, (success, error) =>
            {
                if (success)
                {
                    LogHelper.Info("CloudStorageService", $"File {fileName} deleted successfully from cloud");
                    
                    // Remove from metadata cache
                    RemoveFromMetadataCache(fileName);
                }
                else
                {
                    LogHelper.Warning("CloudStorageService", $"Failed to delete file from cloud: {error}");
                }
                
                // If either local or cloud delete succeeded, consider it a success
                bool overallSuccess = localDeleteSuccess || success;
                OnFileDeleted?.Invoke(fileName, overallSuccess);
                onComplete?.Invoke(overallSuccess, error);
            });
        }
        
        /// <summary>
        /// List files in cloud storage
        /// </summary>
        /// <param name="onComplete">Callback with file list</param>
        public void ListFiles(Action<List<CloudFileMetadata>, string> onComplete)
        {
            if (!_isInitialized)
            {
                string error = "Service is not initialized";
                LogHelper.Error("CloudStorageService", error);
                onComplete?.Invoke(null, error);
                return;
            }
            
            // Use the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable());
            if (provider == null)
            {
                string error = "No available providers";
                LogHelper.Error("CloudStorageService", error);
                onComplete?.Invoke(null, error);
                return;
            }
            
            LogHelper.Info("CloudStorageService", "Listing files from cloud storage");
            
            // Get files from cloud
            provider.ListFiles((files, error) =>
            {
                if (files != null)
                {
                    LogHelper.Info("CloudStorageService", $"Listed {files.Count} files from cloud storage");
                    
                    // Update metadata cache with cloud files
                    foreach (var file in files)
                    {
                        // Check if the file exists locally
                        string localPath = Path.Combine(_localCacheDirectory, file.FileName);
                        file.IsLocal = File.Exists(localPath);
                        
                        // Update metadata cache
                        UpdateMetadataCache(file);
                    }
                    
                    // Return combined list of cloud and local files
                    onComplete?.Invoke(_fileMetadataCache.Values.ToList(), null);
                }
                else
                {
                    LogHelper.Error("CloudStorageService", $"Failed to list files from cloud: {error}");
                    
                    // Return local metadata only
                    onComplete?.Invoke(_fileMetadataCache.Values.ToList(), error);
                }
            });
        }
        
        /// <summary>
        /// Get file metadata
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="onComplete">Callback with file metadata</param>
        public void GetFileMetadata(string fileName, Action<CloudFileMetadata, string> onComplete)
        {
            if (!_isInitialized)
            {
                string error = "Service is not initialized";
                LogHelper.Error("CloudStorageService", error);
                onComplete?.Invoke(null, error);
                return;
            }
            
            if (string.IsNullOrEmpty(fileName))
            {
                string error = "File name cannot be empty";
                LogHelper.Error("CloudStorageService", error);
                onComplete?.Invoke(null, error);
                return;
            }
            
            // Check cache first
            if (_fileMetadataCache.TryGetValue(fileName, out CloudFileMetadata cachedMetadata))
            {
                LogHelper.Info("CloudStorageService", $"Found metadata for {fileName} in cache");
                onComplete?.Invoke(cachedMetadata, null);
                return;
            }
            
            // Check if file exists locally
            string localPath = Path.Combine(_localCacheDirectory, fileName);
            if (File.Exists(localPath))
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(localPath);
                    byte[] data = File.ReadAllBytes(localPath);
                    string hash = CalculateMD5(data);
                    
                    // Create metadata
                    var provider = _providers.FirstOrDefault(p => p.IsAvailable());
                    string providerName = provider?.GetProviderName() ?? "Unknown";
                    
                    var metadata = new CloudFileMetadata(
                        fileName: fileName,
                        size: fileInfo.Length,
                        lastModified: fileInfo.LastWriteTimeUtc,
                        provider: providerName
                    );
                    metadata.Hash = hash;
                    metadata.IsLocal = true;
                    metadata.IsSynced = false; // We don't know if it's synced
                    
                    // Update metadata cache
                    UpdateMetadataCache(metadata);
                    
                    LogHelper.Info("CloudStorageService", $"Created metadata for {fileName} from local file");
                    onComplete?.Invoke(metadata, null);
                    return;
                }
                catch (Exception ex)
                {
                    LogHelper.Warning("CloudStorageService", $"Failed to create metadata from local file: {ex.Message}");
                }
            }
            
            // Get metadata from cloud
            var activeProvider = _providers.FirstOrDefault(p => p.IsAvailable());
            if (activeProvider == null)
            {
                string error = "No available providers";
                LogHelper.Error("CloudStorageService", error);
                onComplete?.Invoke(null, error);
                return;
            }
            
            // Get from cloud
            activeProvider.GetFileMetadata(fileName, (metadata, error) =>
            {
                if (metadata != null)
                {
                    LogHelper.Info("CloudStorageService", $"Got metadata for {fileName} from cloud");
                    
                    // Check if the file exists locally
                    metadata.IsLocal = File.Exists(localPath);
                    
                    // Update metadata cache
                    UpdateMetadataCache(metadata);
                    
                    onComplete?.Invoke(metadata, null);
                }
                else
                {
                    LogHelper.Error("CloudStorageService", $"Failed to get metadata from cloud: {error}");
                    onComplete?.Invoke(null, error);
                }
            });
        }
        
        /// <summary>
        /// Check if a file exists
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="onComplete">Callback with existence check result</param>
        public void FileExists(string fileName, Action<bool> onComplete)
        {
            if (!_isInitialized)
            {
                LogHelper.Error("CloudStorageService", "Service is not initialized");
                onComplete?.Invoke(false);
                return;
            }
            
            if (string.IsNullOrEmpty(fileName))
            {
                LogHelper.Error("CloudStorageService", "File name cannot be empty");
                onComplete?.Invoke(false);
                return;
            }
            
            // Check cache first
            if (_fileMetadataCache.ContainsKey(fileName))
            {
                LogHelper.Info("CloudStorageService", $"File {fileName} exists in cache");
                onComplete?.Invoke(true);
                return;
            }
            
            // Check if file exists locally
            string localPath = Path.Combine(_localCacheDirectory, fileName);
            if (File.Exists(localPath))
            {
                LogHelper.Info("CloudStorageService", $"File {fileName} exists locally");
                onComplete?.Invoke(true);
                return;
            }
            
            // Check on cloud
            var provider = _providers.FirstOrDefault(p => p.IsAvailable());
            if (provider == null)
            {
                LogHelper.Error("CloudStorageService", "No available providers");
                onComplete?.Invoke(false);
                return;
            }
            
            // Check on cloud
            provider.FileExists(fileName, exists =>
            {
                LogHelper.Info("CloudStorageService", $"File {fileName} {(exists ? "exists" : "does not exist")} on cloud");
                onComplete?.Invoke(exists);
            });
        }
        
        /// <summary>
        /// Synchronize local cache with cloud
        /// </summary>
        /// <param name="onComplete">Callback when sync is complete</param>
        public void Sync(Action<bool, int, string> onComplete = null)
        {
            if (!_isInitialized)
            {
                string error = "Service is not initialized";
                LogHelper.Error("CloudStorageService", error);
                onComplete?.Invoke(false, 0, error);
                OnSyncCompleted?.Invoke(false, 0, error);
                return;
            }
            
            LogHelper.Info("CloudStorageService", "Starting synchronization");
            
            // Use the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable());
            if (provider == null)
            {
                string error = "No available providers";
                LogHelper.Error("CloudStorageService", error);
                onComplete?.Invoke(false, 0, error);
                OnSyncCompleted?.Invoke(false, 0, error);
                return;
            }
            
            // List files from cloud
            provider.ListFiles((cloudFiles, error) =>
            {
                if (cloudFiles == null)
                {
                    LogHelper.Error("CloudStorageService", $"Failed to list files from cloud: {error}");
                    onComplete?.Invoke(false, 0, error);
                    OnSyncCompleted?.Invoke(false, 0, error);
                    return;
                }
                
                // Get local files
                List<string> localFiles = new List<string>();
                try
                {
                    string[] files = Directory.GetFiles(_localCacheDirectory);
                    localFiles.AddRange(files.Select(f => Path.GetFileName(f)));
                }
                catch (Exception ex)
                {
                    LogHelper.Warning("CloudStorageService", $"Failed to list local files: {ex.Message}");
                }
                
                int syncCount = 0;
                
                // Sync cloud files to local
                foreach (var cloudFile in cloudFiles)
                {
                    string fileName = cloudFile.FileName;
                    string localPath = Path.Combine(_localCacheDirectory, fileName);
                    
                    // If file doesn't exist locally or is older than cloud version, download it
                    if (!File.Exists(localPath) || 
                        (File.GetLastWriteTimeUtc(localPath) < cloudFile.LastModified))
                    {
                        // Download file
                        provider.LoadFile(fileName, (data, loadError) =>
                        {
                            if (data != null)
                            {
                                try
                                {
                                    File.WriteAllBytes(localPath, data);
                                    LogHelper.Info("CloudStorageService", $"Downloaded file {fileName} from cloud");
                                    syncCount++;
                                    
                                    // Update metadata
                                    cloudFile.IsLocal = true;
                                    cloudFile.IsSynced = true;
                                    UpdateMetadataCache(cloudFile);
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.Warning("CloudStorageService", $"Failed to save downloaded file: {ex.Message}");
                                }
                            }
                            else
                            {
                                LogHelper.Warning("CloudStorageService", $"Failed to download file {fileName}: {loadError}");
                            }
                        });
                    }
                    else
                    {
                        // File exists locally and is up-to-date
                        cloudFile.IsLocal = true;
                        cloudFile.IsSynced = true;
                        UpdateMetadataCache(cloudFile);
                    }
                }
                
                // Find local files that don't exist in cloud
                foreach (string localFileName in localFiles)
                {
                    if (!cloudFiles.Any(f => f.FileName == localFileName))
                    {
                        string localPath = Path.Combine(_localCacheDirectory, localFileName);
                        
                        try
                        {
                            // Upload to cloud
                            byte[] data = File.ReadAllBytes(localPath);
                            provider.SaveFile(localFileName, data, (success, saveError) =>
                            {
                                if (success)
                                {
                                    LogHelper.Info("CloudStorageService", $"Uploaded file {localFileName} to cloud");
                                    syncCount++;
                                    
                                    // Create metadata
                                    var metadata = new CloudFileMetadata(
                                        fileName: localFileName,
                                        size: data.Length,
                                        lastModified: File.GetLastWriteTimeUtc(localPath),
                                        provider: provider.GetProviderName()
                                    );
                                    metadata.Hash = CalculateMD5(data);
                                    metadata.IsLocal = true;
                                    metadata.IsSynced = true;
                                    
                                    // Update metadata cache
                                    UpdateMetadataCache(metadata);
                                }
                                else
                                {
                                    LogHelper.Warning("CloudStorageService", $"Failed to upload file {localFileName}: {saveError}");
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Warning("CloudStorageService", $"Failed to read local file for upload: {ex.Message}");
                        }
                    }
                }
                
                _lastSyncTime = DateTime.UtcNow;
                LogHelper.Info("CloudStorageService", $"Synchronization completed with {syncCount} files synced");
                
                // Update metadata cache
                SaveMetadataCache();
                
                onComplete?.Invoke(true, syncCount, null);
                OnSyncCompleted?.Invoke(true, syncCount, null);
            });
        }
        
        /// <summary>
        /// Get storage quota
        /// </summary>
        /// <param name="onComplete">Callback with quota information</param>
        public void GetStorageQuota(Action<long, long, string> onComplete)
        {
            if (!_isInitialized)
            {
                string error = "Service is not initialized";
                LogHelper.Error("CloudStorageService", error);
                onComplete?.Invoke(0, 0, error);
                return;
            }
            
            // Use the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable());
            if (provider == null)
            {
                string error = "No available providers";
                LogHelper.Error("CloudStorageService", error);
                onComplete?.Invoke(0, 0, error);
                return;
            }
            
            // Get quota from provider
            provider.GetStorageQuota((used, total, error) =>
            {
                if (string.IsNullOrEmpty(error))
                {
                    LogHelper.Info("CloudStorageService", $"Storage quota: {used} / {total} bytes");
                }
                else
                {
                    LogHelper.Error("CloudStorageService", $"Failed to get storage quota: {error}");
                }
                
                onComplete?.Invoke(used, total, error);
            });
        }
        
        /// <summary>
        /// Check if the service is initialized
        /// </summary>
        /// <returns>True if initialized, false otherwise</returns>
        public bool IsInitialized()
        {
            return _isInitialized;
        }
        
        /// <summary>
        /// Get status of the cloud storage service
        /// </summary>
        /// <returns>Service status</returns>
        public CloudStorageStatus GetStatus()
        {
            var status = new CloudStorageStatus();
            
            status.IsInitialized = _isInitialized;
            status.ProviderCount = _providers.Count;
            status.AvailableProviders = _providers.Where(p => p.IsAvailable()).Select(p => p.GetProviderName()).ToList();
            status.TotalFiles = _fileMetadataCache.Count;
            status.LocalCachedFiles = _fileMetadataCache.Values.Count(f => f.IsLocal);
            status.LastSyncTime = _lastSyncTime;
            status.LastError = _lastError;
            
            // Get storage info
            long localUsed = 0;
            
            try
            {
                var directory = new DirectoryInfo(_localCacheDirectory);
                localUsed = directory.GetFiles().Sum(f => f.Length);
            }
            catch (Exception ex)
            {
                LogHelper.Warning("CloudStorageService", $"Failed to calculate local storage usage: {ex.Message}");
            }
            
            status.UsedStorage = localUsed;
            
            // Use provider quota if available
            var provider = _providers.FirstOrDefault(p => p.IsAvailable());
            if (provider != null)
            {
                provider.GetStorageQuota((used, total, error) =>
                {
                    if (string.IsNullOrEmpty(error))
                    {
                        status.UsedStorage = used;
                        status.TotalStorage = total;
                    }
                    else
                    {
                        LogHelper.Warning("CloudStorageService", $"Failed to get storage quota from provider: {error}");
                    }
                });
            }
            
            return status;
        }
        
        /// <summary>
        /// Calculate MD5 hash of data
        /// </summary>
        /// <param name="data">Data to hash</param>
        /// <returns>MD5 hash as string</returns>
        private string CalculateMD5(byte[] data)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
        
        /// <summary>
        /// Update metadata cache
        /// </summary>
        /// <param name="metadata">Metadata to update</param>
        private void UpdateMetadataCache(CloudFileMetadata metadata)
        {
            if (metadata == null)
            {
                return;
            }
            
            _fileMetadataCache[metadata.FileName] = metadata;
            
            // Save cache periodically
            if (_fileMetadataCache.Count % 10 == 0)
            {
                SaveMetadataCache();
            }
        }
        
        /// <summary>
        /// Remove file from metadata cache
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        private void RemoveFromMetadataCache(string fileName)
        {
            if (_fileMetadataCache.ContainsKey(fileName))
            {
                _fileMetadataCache.Remove(fileName);
                SaveMetadataCache();
            }
        }
        
        /// <summary>
        /// Save metadata cache to disk
        /// </summary>
        private void SaveMetadataCache()
        {
            try
            {
                string cacheFile = Path.Combine(_localCacheDirectory, "metadata_cache.json");
                string json = JsonUtility.ToJson(new MetadataCache { Files = _fileMetadataCache.Values.ToList() });
                File.WriteAllText(cacheFile, json);
                LogHelper.Info("CloudStorageService", $"Saved metadata cache with {_fileMetadataCache.Count} entries");
            }
            catch (Exception ex)
            {
                LogHelper.Warning("CloudStorageService", $"Failed to save metadata cache: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Load metadata cache from disk
        /// </summary>
        private void LoadMetadataCache()
        {
            try
            {
                string cacheFile = Path.Combine(_localCacheDirectory, "metadata_cache.json");
                if (File.Exists(cacheFile))
                {
                    string json = File.ReadAllText(cacheFile);
                    var cache = JsonUtility.FromJson<MetadataCache>(json);
                    
                    if (cache != null && cache.Files != null)
                    {
                        _fileMetadataCache = cache.Files.ToDictionary(f => f.FileName);
                        LogHelper.Info("CloudStorageService", $"Loaded metadata cache with {_fileMetadataCache.Count} entries");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warning("CloudStorageService", $"Failed to load metadata cache: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Helper class for serializing metadata cache
        /// </summary>
        [Serializable]
        private class MetadataCache
        {
            public List<CloudFileMetadata> Files;
        }
    }
} 