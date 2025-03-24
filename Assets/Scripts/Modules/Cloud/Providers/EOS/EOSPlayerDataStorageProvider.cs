using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.PlayerDataStorage;
using PlayEveryWare.EpicOnlineServices;
using RecipeRage.Modules.Auth;
using RecipeRage.Modules.Cloud.Interfaces;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Modules.Cloud.Providers.EOS
{
    /// <summary>
    /// Epic Online Services implementation of the cloud storage provider.
    /// Uses EOS Player Data Storage for cloud save functionality.
    /// Complexity Rating: 4
    /// </summary>
    public class EOSPlayerDataStorageProvider : ICloudStorageProvider
    {
        /// <summary>
        /// Provider name
        /// </summary>
        private const string PROVIDER_NAME = "EOSPlayerDataStorage";

        /// <summary>
        /// Maximum chunk size for file operations (100KB - EOS limit is 4MB but we use smaller chunks)
        /// </summary>
        private const int MAX_CHUNK_SIZE = 102400;

        /// <summary>
        /// Maximum file size (10MB - actual EOS limit is much higher but we use a reasonable limit)
        /// </summary>
        private const long MAX_FILE_SIZE = 10485760;

        /// <summary>
        /// Active file operations
        /// </summary>
        private readonly Dictionary<string, FileTransferOperation> _activeOperations = new Dictionary<string, FileTransferOperation>();

        /// <summary>
        /// Cache of file metadata
        /// </summary>
        private readonly Dictionary<string, CloudFileMetadata> _fileMetadataCache = new Dictionary<string, CloudFileMetadata>();

        /// <summary>
        /// Whether the provider is initialized
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        /// Last error message
        /// </summary>
        private string _lastError = string.Empty;

        /// <summary>
        /// Initialize the provider
        /// </summary>
        /// <param name="onComplete"> Callback when initialization is complete </param>
        public void Initialize(Action<bool> onComplete = null)
        {
            if (_isInitialized)
            {
                LogHelper.Warning("EOSPlayerDataStorageProvider", "Provider is already initialized");
                onComplete?.Invoke(true);
                return;
            }

            LogHelper.Info("EOSPlayerDataStorageProvider", "Initializing EOS Player Data Storage provider");

            // Check if EOSManager is available
            if (EOSManager.Instance == null || EOSManager.Instance.GetPlayerDataStorageInterface() == null)
            {
                _lastError = "EOS Manager or Player Data Storage interface is not available";
                LogHelper.Error("EOSPlayerDataStorageProvider", _lastError);
                onComplete?.Invoke(false);
                return;
            }

            // Check if user is logged in to EOS
            if (!AuthHelper.IsSignedIn() || string.IsNullOrEmpty(AuthHelper.CurrentUser?.UserId))
            {
                _lastError = "User is not signed in to EOS";
                LogHelper.Error("EOSPlayerDataStorageProvider", _lastError);
                onComplete?.Invoke(false);
                return;
            }

            _isInitialized = true;
            LogHelper.Info("EOSPlayerDataStorageProvider", "EOS Player Data Storage provider initialized successfully");
            onComplete?.Invoke(true);

            // Query and cache file list
            QueryFileList();
        }

        /// <summary>
        /// Save a file to EOS Player Data Storage
        /// </summary>
        /// <param name="fileName"> Name of the file </param>
        /// <param name="data"> Data to save </param>
        /// <param name="onComplete"> Callback when save is complete </param>
        public void SaveFile(string fileName, byte[] data, Action<bool, string> onComplete = null)
        {
            if (!CheckInitialized("SaveFile", onComplete)) return;

            if (string.IsNullOrEmpty(fileName))
            {
                string error = "File name cannot be empty";
                LogHelper.Error("EOSPlayerDataStorageProvider", error);
                onComplete?.Invoke(false, error);
                return;
            }

            if (data == null || data.Length == 0)
            {
                string error = "Data cannot be empty";
                LogHelper.Error("EOSPlayerDataStorageProvider", error);
                onComplete?.Invoke(false, error);
                return;
            }

            if (data.Length > MAX_FILE_SIZE)
            {
                string error = $"File size exceeds maximum allowed size of {MAX_FILE_SIZE} bytes";
                LogHelper.Error("EOSPlayerDataStorageProvider", error);
                onComplete?.Invoke(false, error);
                return;
            }

            // Check if an operation is already active for this file
            if (_activeOperations.ContainsKey(fileName))
            {
                string error = $"A file operation is already in progress for {fileName}";
                LogHelper.Error("EOSPlayerDataStorageProvider", error);
                onComplete?.Invoke(false, error);
                return;
            }

            LogHelper.Info("EOSPlayerDataStorageProvider", $"Saving file {fileName} to EOS Player Data Storage");

            // Get the product user ID
            var productUserId = GetCurrentProductUserId();
            if (productUserId == null)
            {
                string error = "Could not get valid product user ID";
                LogHelper.Error("EOSPlayerDataStorageProvider", error);
                onComplete?.Invoke(false, error);
                return;
            }

            // Create file options
            var fileTransferRequest = new FileTransferRequestData();
            var options = new WriteFileOptions
            {
                LocalUserId = productUserId,
                Filename = fileName,
                ChunkLengthBytes = MAX_CHUNK_SIZE,
                WriteFileDataCallback = (ref WriteFileDataCallbackInfo data, out ArraySegment<byte> outDataBuffer) =>
                {
                    // Get current operation
                    if (!_activeOperations.TryGetValue(fileName, out var operation))
                    {
                        // Operation was cancelled or doesn't exist
                        outDataBuffer = new ArraySegment<byte>();
                        return WriteResult.FailRequest;
                    }

                    // Calculate offset and length to copy
                    int dataLength = operation.Data.Length;
                    // Access the properties according to the SDK
                    ulong offset = 0;
                    if (data.GetType().GetProperty("DataOffset") != null)
                    {
                        offset = (ulong)data.GetType().GetProperty("DataOffset").GetValue(data);
                    }
                    uint length = data.DataBufferLengthBytes;

                    // Check if we've reached end of data
                    if (offset >= (ulong)dataLength)
                    {
                        outDataBuffer = new ArraySegment<byte>();
                        return WriteResult.CompleteRequest;
                    }

                    // Adjust length if we're near the end
                    if (offset + length > (ulong)dataLength) length = (uint)(dataLength - (int)offset);

                    // Create data segment to return
                    byte[] chunk = new byte[length];
                    Array.Copy(operation.Data, (int)offset, chunk, 0, length);
                    outDataBuffer = new ArraySegment<byte>(chunk);
                    operation.Progress = (float)offset / dataLength;

                    // Return result
                    return offset + length >= (ulong)dataLength ? WriteResult.CompleteRequest : WriteResult.ContinueWriting;
                },
                FileTransferProgressCallback = (ref FileTransferProgressCallbackInfo data) =>
                {
                    // Report progress
                    if (_activeOperations.TryGetValue(fileName, out var operation))
                    {
                        operation.Progress = data.BytesTransferred / (float)data.TotalFileSizeBytes;
                        LogHelper.Debug("EOSPlayerDataStorageProvider",
                            $"File {fileName} upload progress: {operation.Progress:P0}");
                    }
                }
            };

            // Create operation
            var operation = new FileTransferOperation
            {
                FileName = fileName,
                Data = data,
                Progress = 0f,
                Request = fileTransferRequest
            };

            // Add to active operations
            _activeOperations[fileName] = operation;

            // Start file write
            var playerDataStorageInterface = EOSManager.Instance.GetPlayerDataStorageInterface();
            fileTransferRequest.TransferHandle = playerDataStorageInterface.WriteFile(ref options, null,
                (ref WriteFileCallbackInfo info) =>
                {
                    // Remove from active operations
                    _activeOperations.Remove(fileName);

                    // Handle result
                    if (info.ResultCode == Result.Success)
                    {
                        LogHelper.Info("EOSPlayerDataStorageProvider",
                            $"File {fileName} saved successfully to EOS Player Data Storage");

                        // Create metadata
                        var metadata = new CloudFileMetadata(
                            fileName,
                            data.Length,
                            DateTime.UtcNow,
                            PROVIDER_NAME
                        );
                        metadata.IsSynced = true;
                        metadata.IsLocal = true;

                        // Update metadata cache
                        _fileMetadataCache[fileName] = metadata;

                        onComplete?.Invoke(true, null);
                    }
                    else
                    {
                        string error = $"Failed to save file: {info.ResultCode}";
                        LogHelper.Error("EOSPlayerDataStorageProvider", error);
                        onComplete?.Invoke(false, error);
                    }
                });

            // Update request in the operation
            operation.Request = fileTransferRequest;
        }

        /// <summary>
        /// Load a file from EOS Player Data Storage
        /// </summary>
        /// <param name="fileName"> Name of the file </param>
        /// <param name="onComplete"> Callback with loaded data </param>
        public void LoadFile(string fileName, Action<byte[], string> onComplete)
        {
            if (!CheckInitialized("LoadFile", (success, error) => onComplete?.Invoke(null, error))) return;

            if (string.IsNullOrEmpty(fileName))
            {
                string error = "File name cannot be empty";
                LogHelper.Error("EOSPlayerDataStorageProvider", error);
                onComplete?.Invoke(null, error);
                return;
            }

            // Check if an operation is already active for this file
            if (_activeOperations.ContainsKey(fileName))
            {
                string error = $"A file operation is already in progress for {fileName}";
                LogHelper.Error("EOSPlayerDataStorageProvider", error);
                onComplete?.Invoke(null, error);
                return;
            }

            LogHelper.Info("EOSPlayerDataStorageProvider", $"Loading file {fileName} from EOS Player Data Storage");

            // Get the product user ID
            var productUserId = GetCurrentProductUserId();
            if (productUserId == null)
            {
                string error = "Could not get valid product user ID";
                LogHelper.Error("EOSPlayerDataStorageProvider", error);
                onComplete?.Invoke(null, error);
                return;
            }

            // First get file metadata
            try
            {
                // Use QueryFileOptions instead of GetFileMetadataOptions 
                var queryFileOptions = new QueryFileOptions
                {
                    LocalUserId = productUserId,
                    Filename = fileName
                };

                var playerDataStorageInterface = EOSManager.Instance.GetPlayerDataStorageInterface();
                playerDataStorageInterface.QueryFile(ref queryFileOptions, null,
                    (ref QueryFileCallbackInfo queryInfo) =>
                    {
                        if (queryInfo.ResultCode != Result.Success)
                        {
                            string error = $"Failed to query file: {queryInfo.ResultCode}";
                            LogHelper.Error("EOSPlayerDataStorageProvider", error);
                            onComplete?.Invoke(null, error);
                            return;
                        }

                        // Get file size
                        var fileSizeOptions = new CopyFileMetadataByFilenameOptions
                        {
                            LocalUserId = productUserId,
                            Filename = fileName
                        };

                        FileMetadata? fileMetadata = null;
                        var result = playerDataStorageInterface.CopyFileMetadataByFilename(ref fileSizeOptions, out fileMetadata);

                        if (result != Result.Success || fileMetadata == null)
                        {
                            string error = $"Failed to get file size: {result}";
                            LogHelper.Error("EOSPlayerDataStorageProvider", error);
                            onComplete?.Invoke(null, error);
                            return;
                        }

                        // Prepare buffer for file data
                        byte[] fileData = new byte[fileMetadata.Value.FileSizeBytes];

                        // Create file read options
                        var fileTransferRequest = new FileTransferRequestData();
                        var readOptions = new ReadFileOptions
                        {
                            LocalUserId = productUserId,
                            Filename = fileName,
                            ReadChunkLengthBytes = MAX_CHUNK_SIZE,
                            ReadFileDataCallback = (ref ReadFileDataCallbackInfo data) =>
                            {
                                // Get current operation
                                if (!_activeOperations.TryGetValue(fileName, out var operation))
                                    // Operation was cancelled or doesn't exist
                                    return ReadResult.FailRequest;

                                // Calculate offset and length based on SDK
                                ulong offset = 0;
                                uint length = 0;
                                ArraySegment<byte> dataChunk = new ArraySegment<byte>();

                                // Use reflection to get properties based on SDK version
                                try
                                {
                                    // Try to get offset
                                    if (data.GetType().GetProperty("ReadOffset") != null)
                                    {
                                        offset = (ulong)data.GetType().GetProperty("ReadOffset").GetValue(data);
                                    }
                                    else if (data.GetType().GetProperty("DataOffset") != null)
                                    {
                                        offset = (ulong)data.GetType().GetProperty("DataOffset").GetValue(data);
                                    }

                                    // Try to get data chunk
                                    if (data.GetType().GetProperty("DataChunk") != null)
                                    {
                                        var chunk = data.GetType().GetProperty("DataChunk").GetValue(data);
                                        if (chunk != null)
                                        {
                                            // Try to get length
                                            if (data.GetType().GetProperty("DataChunkLength") != null)
                                            {
                                                length = (uint)data.GetType().GetProperty("DataChunkLength").GetValue(data);
                                            }
                                            else if (data.GetType().GetProperty("ChunkLength") != null)
                                            {
                                                length = (uint)data.GetType().GetProperty("ChunkLength").GetValue(data);
                                            }

                                            // Copy data
                                            if (length > 0)
                                            {
                                                if (chunk is byte[] byteArray)
                                                {
                                                    Array.Copy(byteArray, 0, operation.Data, (int)offset, (int)length);
                                                }
                                            }
                                        }
                                    }

                                    operation.Progress = (float)offset / operation.Data.Length;
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.Warning("EOSPlayerDataStorageProvider", $"Error processing data chunk: {ex.Message}");
                                }

                                return ReadResult.ContinueReading;
                            },
                            FileTransferProgressCallback = (ref FileTransferProgressCallbackInfo data) =>
                            {
                                // Report progress
                                if (_activeOperations.TryGetValue(fileName, out var operation))
                                {
                                    operation.Progress = data.BytesTransferred / (float)data.TotalFileSizeBytes;
                                    LogHelper.Debug("EOSPlayerDataStorageProvider",
                                        $"File {fileName} download progress: {operation.Progress:P0}");
                                }
                            }
                        };

                        // Create operation
                        var operation = new FileTransferOperation
                        {
                            FileName = fileName,
                            Data = fileData,
                            Progress = 0f,
                            Request = fileTransferRequest
                        };

                        // Add to active operations
                        _activeOperations[fileName] = operation;

                        // Start file read
                        fileTransferRequest.TransferHandle = playerDataStorageInterface.ReadFile(ref readOptions, null,
                            (ref ReadFileCallbackInfo info) =>
                            {
                                // Remove from active operations
                                _activeOperations.Remove(fileName);

                                // Handle result
                                if (info.ResultCode == Result.Success)
                                {
                                    LogHelper.Info("EOSPlayerDataStorageProvider",
                                        $"File {fileName} loaded successfully from EOS Player Data Storage");

                                    // Create metadata based on the actual SDK properties
                                    var metadata = new CloudFileMetadata(
                                        fileName,
                                        fileData.Length,
                                        DateTime.UtcNow, // Use current time if we can't get the actual time
                                        PROVIDER_NAME
                                    );
                                    metadata.IsSynced = true;

                                    // Update metadata cache
                                    _fileMetadataCache[fileName] = metadata;

                                    onComplete?.Invoke(fileData, null);
                                }
                                else
                                {
                                    string error = $"Failed to load file: {info.ResultCode}";
                                    LogHelper.Error("EOSPlayerDataStorageProvider", error);
                                    onComplete?.Invoke(null, error);
                                }
                            });

                        // Update request in the operation
                        operation.Request = fileTransferRequest;
                    });
            }
            catch (Exception ex)
            {
                LogHelper.Error("EOSPlayerDataStorageProvider", $"Exception occurred: {ex.Message}");
                onComplete?.Invoke(null, ex.Message);
            }
        }

        /// <summary>
        /// Delete a file from EOS Player Data Storage
        /// </summary>
        /// <param name="fileName"> Name of the file </param>
        /// <param name="onComplete"> Callback when deletion is complete </param>
        public void DeleteFile(string fileName, Action<bool, string> onComplete = null)
        {
            if (!CheckInitialized("DeleteFile", onComplete)) return;

            if (string.IsNullOrEmpty(fileName))
            {
                string error = "File name cannot be empty";
                LogHelper.Error("EOSPlayerDataStorageProvider", error);
                onComplete?.Invoke(false, error);
                return;
            }

            // Check if an operation is already active for this file
            if (_activeOperations.ContainsKey(fileName))
            {
                string error = $"A file operation is already in progress for {fileName}";
                LogHelper.Error("EOSPlayerDataStorageProvider", error);
                onComplete?.Invoke(false, error);
                return;
            }

            LogHelper.Info("EOSPlayerDataStorageProvider", $"Deleting file {fileName} from EOS Player Data Storage");

            // Get the product user ID
            var productUserId = GetCurrentProductUserId();
            if (productUserId == null)
            {
                string error = "Could not get valid product user ID";
                LogHelper.Error("EOSPlayerDataStorageProvider", error);
                onComplete?.Invoke(false, error);
                return;
            }

            // Delete file
            var options = new DeleteFileOptions
            {
                LocalUserId = productUserId,
                Filename = fileName
            };

            var playerDataStorageInterface = EOSManager.Instance.GetPlayerDataStorageInterface();
            playerDataStorageInterface.DeleteFile(ref options, null,
                (ref DeleteFileCallbackInfo info) =>
                {
                    // Handle result
                    if (info.ResultCode == Result.Success)
                    {
                        LogHelper.Info("EOSPlayerDataStorageProvider",
                            $"File {fileName} deleted successfully from EOS Player Data Storage");

                        // Remove from metadata cache
                        _fileMetadataCache.Remove(fileName);

                        onComplete?.Invoke(true, null);
                    }
                    else
                    {
                        string error = $"Failed to delete file: {info.ResultCode}";
                        LogHelper.Error("EOSPlayerDataStorageProvider", error);
                        onComplete?.Invoke(false, error);
                    }
                });
        }

        /// <summary>
        /// List files in EOS Player Data Storage
        /// </summary>
        /// <param name="onComplete"> Callback with file list </param>
        public void ListFiles(Action<List<CloudFileMetadata>, string> onComplete)
        {
            if (!CheckInitialized("ListFiles", (success, error) => onComplete?.Invoke(null, error))) return;

            LogHelper.Info("EOSPlayerDataStorageProvider", "Listing files from EOS Player Data Storage");

            // Get the product user ID
            var productUserId = GetCurrentProductUserId();
            if (productUserId == null)
            {
                string error = "Could not get valid product user ID";
                LogHelper.Error("EOSPlayerDataStorageProvider", error);
                onComplete?.Invoke(null, error);
                return;
            }

            // Query file list
            var options = new QueryFileListOptions
            {
                LocalUserId = productUserId
            };

            var playerDataStorageInterface = EOSManager.Instance.GetPlayerDataStorageInterface();
            playerDataStorageInterface.QueryFileList(ref options, null, OnQueryFileListCompleted);
        }

        /// <summary>
        /// Get file metadata
        /// </summary>
        /// <param name="fileName"> Name of the file </param>
        /// <param name="onComplete"> Callback with file metadata </param>
        public void GetFileMetadata(string fileName, Action<CloudFileMetadata, string> onComplete)
        {
            if (!CheckInitialized("GetFileMetadata", (success, error) => onComplete?.Invoke(null, error))) return;

            if (string.IsNullOrEmpty(fileName))
            {
                string error = "File name cannot be empty";
                LogHelper.Error("EOSPlayerDataStorageProvider", error);
                onComplete?.Invoke(null, error);
                return;
            }

            // Check if metadata is cached
            if (_fileMetadataCache.TryGetValue(fileName, out var cachedMetadata))
            {
                LogHelper.Info("EOSPlayerDataStorageProvider", $"Using cached metadata for {fileName}");
                onComplete?.Invoke(cachedMetadata, null);
                return;
            }

            LogHelper.Info("EOSPlayerDataStorageProvider",
                $"Getting metadata for file {fileName} from EOS Player Data Storage");

            // Get the product user ID
            var productUserId = GetCurrentProductUserId();
            if (productUserId == null)
            {
                string error = "Could not get valid product user ID";
                LogHelper.Error("EOSPlayerDataStorageProvider", error);
                onComplete?.Invoke(null, error);
                return;
            }

            // Get file metadata
            var options = new CopyFileMetadataByFilenameOptions
            {
                LocalUserId = productUserId,
                Filename = fileName
            };

            var playerDataStorageInterface = EOSManager.Instance.GetPlayerDataStorageInterface();
            FileMetadata? fileMetadata = null;
            var result = playerDataStorageInterface.CopyFileMetadataByFilename(ref options, out fileMetadata);

            if (result == Result.Success && fileMetadata.HasValue)
            {
                // Create timestamp - handle different SDK versions
                DateTime modifiedTime = DateTime.UtcNow;
                try
                {
                    // Try different ways to get last modified time based on SDK version
                    if (fileMetadata.Value.GetType().GetProperty("LastModified") != null)
                    {
                        // Some versions use LastModified (Unix time)
                        var lastModified = fileMetadata.Value.GetType().GetProperty("LastModified").GetValue(fileMetadata.Value);
                        if (lastModified is long unixTime)
                        {
                            modifiedTime = DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
                        }
                    }
                    else if (fileMetadata.Value.GetType().GetProperty("LastModifiedTime") != null)
                    {
                        // Some versions use LastModifiedTime (Windows file time)
                        var lastModified = fileMetadata.Value.GetType().GetProperty("LastModifiedTime").GetValue(fileMetadata.Value);
                        if (lastModified is long fileTime)
                        {
                            modifiedTime = DateTime.FromFileTimeUtc(fileTime);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Warning("EOSPlayerDataStorageProvider", $"Error getting last modified time: {ex.Message}");
                    // Use current time as fallback
                }

                var metadata = new CloudFileMetadata(
                    fileMetadata.Value.Filename,
                    fileMetadata.Value.FileSizeBytes,
                    modifiedTime,
                    PROVIDER_NAME
                );

                // Update cache
                _fileMetadataCache[fileMetadata.Value.Filename] = metadata;

                LogHelper.Info("EOSPlayerDataStorageProvider",
                    $"Retrieved metadata for file {fileName}: {metadata.Size} bytes, last modified {metadata.LastModified}");
                onComplete?.Invoke(metadata, null);
            }
            else
            {
                string error = $"Failed to get file metadata: {result}";
                LogHelper.Error("EOSPlayerDataStorageProvider", error);
                onComplete?.Invoke(null, error);
            }
        }

        /// <summary>
        /// Check if a file exists
        /// </summary>
        /// <param name="fileName"> Name of the file </param>
        /// <param name="onComplete"> Callback with existence check result </param>
        public void FileExists(string fileName, Action<bool> onComplete)
        {
            if (!_isInitialized)
            {
                LogHelper.Error("EOSPlayerDataStorageProvider", "Provider is not initialized");
                onComplete?.Invoke(false);
                return;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                LogHelper.Error("EOSPlayerDataStorageProvider", "File name cannot be empty");
                onComplete?.Invoke(false);
                return;
            }

            // Check cache first
            if (_fileMetadataCache.ContainsKey(fileName))
            {
                LogHelper.Info("EOSPlayerDataStorageProvider", $"File {fileName} exists according to cache");
                onComplete?.Invoke(true);
                return;
            }

            // Get file metadata (will return error if file doesn't exist)
            GetFileMetadata(fileName, (metadata, error) =>
            {
                bool exists = metadata != null;
                LogHelper.Info("EOSPlayerDataStorageProvider",
                    $"File {fileName} {(exists ? "exists" : "does not exist")}");
                onComplete?.Invoke(exists);
            });
        }

        /// <summary>
        /// Get storage quota
        /// </summary>
        /// <param name="onComplete"> Callback with quota information </param>
        public void GetStorageQuota(Action<long, long, string> onComplete)
        {
            if (!CheckInitialized("GetStorageQuota", (success, error) => onComplete?.Invoke(0, 0, error))) return;

            LogHelper.Info("EOSPlayerDataStorageProvider", "Getting storage quota from EOS Player Data Storage");

            // EOS doesn't provide a direct way to get quota, so we'll estimate it

            // Get the product user ID
            var productUserId = GetCurrentProductUserId();
            if (productUserId == null)
            {
                string error = "Could not get valid product user ID";
                LogHelper.Error("EOSPlayerDataStorageProvider", error);
                onComplete?.Invoke(0, 0, error);
                return;
            }

            // Query file list to get total size
            var options = new QueryFileListOptions
            {
                LocalUserId = productUserId
            };

            var playerDataStorageInterface = EOSManager.Instance.GetPlayerDataStorageInterface();
            playerDataStorageInterface.QueryFileList(ref options, null,
                (ref QueryFileListCallbackInfo info) =>
                {
                    if (info.ResultCode != Result.Success)
                    {
                        string error = $"Failed to query file list: {info.ResultCode}";
                        LogHelper.Error("EOSPlayerDataStorageProvider", error);
                        onComplete?.Invoke(0, 0, error);
                        return;
                    }

                    // Get file count
                    var getFileCountOptions = new GetFileMetadataCountOptions
                    {
                        LocalUserId = productUserId
                    };

                    int fileCount = 0;
                    var countResult = playerDataStorageInterface.GetFileMetadataCount(ref getFileCountOptions, out fileCount);
                    if (countResult != Result.Success)
                    {
                        string countError = $"Failed to get file count: {countResult}";
                        LogHelper.Warning("EOSPlayerDataStorageProvider", countError);
                        onComplete?.Invoke(0, 0, countError);
                        return;
                    }

                    // Get total size
                    long totalSize = 0;
                    for (uint i = 0; i < fileCount; i++)
                    {
                        var copyOptions = new CopyFileMetadataAtIndexOptions
                        {
                            LocalUserId = productUserId,
                            Index = i
                        };

                        FileMetadata? fileMetadata = null;
                        var result = playerDataStorageInterface.CopyFileMetadataAtIndex(ref copyOptions, out fileMetadata);

                        if (result == Result.Success && fileMetadata.HasValue)
                        {
                            totalSize += fileMetadata.Value.FileSizeBytes;
                        }
                    }

                    // EOS doesn't provide a way to get the quota, so we use a reasonable estimate
                    // EOS provides 200 MB per game per user as documented
                    long totalQuota = 200 * 1024 * 1024; // 200 MB

                    LogHelper.Info("EOSPlayerDataStorageProvider",
                        $"Storage used: {totalSize} bytes out of approximately {totalQuota} bytes");
                    onComplete?.Invoke(totalSize, totalQuota, null);
                });
        }

        /// <summary>
        /// Get the provider name
        /// </summary>
        /// <returns> Provider name </returns>
        public string GetProviderName()
        {
            return PROVIDER_NAME;
        }

        /// <summary>
        /// Check if the provider is available
        /// </summary>
        /// <returns> True if available, false otherwise </returns>
        public bool IsAvailable()
        {
            // Check if everything is set up correctly
            bool eosManagerAvailable = EOSManager.Instance != null &&
                                       EOSManager.Instance.GetPlayerDataStorageInterface() != null;
            bool userLoggedIn = AuthHelper.IsSignedIn() && !string.IsNullOrEmpty(AuthHelper.CurrentUser?.UserId);

            return _isInitialized && eosManagerAvailable && userLoggedIn;
        }

        /// <summary>
        /// Query file list and cache metadata
        /// </summary>
        private void QueryFileList()
        {
            // Get the product user ID
            var productUserId = GetCurrentProductUserId();
            if (productUserId == null)
            {
                LogHelper.Error("EOSPlayerDataStorageProvider", "Could not get valid product user ID");
                return;
            }

            // Query file list
            var options = new QueryFileListOptions
            {
                LocalUserId = productUserId
            };

            var playerDataStorageInterface = EOSManager.Instance.GetPlayerDataStorageInterface();
            playerDataStorageInterface.QueryFileList(ref options, null, OnQueryFileListCompleted);
        }

        /// <summary>
        /// Get the current EOS product user ID
        /// </summary>
        /// <returns> Product user ID or null if not available </returns>
        private ProductUserId GetCurrentProductUserId()
        {
            // Get the product user ID from the auth service
            string userId = AuthHelper.CurrentUser?.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                LogHelper.Error("EOSPlayerDataStorageProvider", "No user ID available");
                return null;
            }

            // Try to convert to product user ID
            var productUserId = ProductUserId.FromString(userId);
            if (productUserId == null || !productUserId.IsValid())
            {
                LogHelper.Error("EOSPlayerDataStorageProvider", $"Invalid product user ID: {userId}");
                return null;
            }

            return productUserId;
        }

        /// <summary>
        /// Check if the provider is initialized
        /// </summary>
        /// <param name="methodName"> Name of the calling method </param>
        /// <param name="onComplete"> Callback for reporting error </param>
        /// <returns> True if initialized, false otherwise </returns>
        private bool CheckInitialized(string methodName, Action<bool, string> onComplete = null)
        {
            if (!_isInitialized)
            {
                string error = "Provider is not initialized";
                LogHelper.Error("EOSPlayerDataStorageProvider", $"{methodName}: {error}");
                onComplete?.Invoke(false, error);
                return false;
            }

            if (EOSManager.Instance == null || EOSManager.Instance.GetPlayerDataStorageInterface() == null)
            {
                string error = "EOS Manager or Player Data Storage interface is not available";
                LogHelper.Error("EOSPlayerDataStorageProvider", $"{methodName}: {error}");
                onComplete?.Invoke(false, error);
                return false;
            }

            if (!AuthHelper.IsSignedIn() || string.IsNullOrEmpty(AuthHelper.CurrentUser?.UserId))
            {
                string error = "User is not signed in to EOS";
                LogHelper.Error("EOSPlayerDataStorageProvider", $"{methodName}: {error}");
                onComplete?.Invoke(false, error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Data model for file transfer requests
        /// </summary>
        private class FileTransferRequestData
        {
            /// <summary>
            /// Handle to the EOS file transfer request
            /// </summary>
            public PlayerDataStorageFileTransferRequest TransferHandle { get; set; }

            /// <summary>
            /// Whether the transfer is active
            /// </summary>
            public bool IsActive { get; set; }

            /// <summary>
            /// Current transfer progress (0-1)
            /// </summary>
            public float Progress { get; set; }

            /// <summary>
            /// Total size of the file being transferred
            /// </summary>
            public ulong TotalSize { get; set; }

            /// <summary>
            /// Bytes transferred so far
            /// </summary>
            public ulong BytesTransferred { get; set; }

            /// <summary>
            /// Callback for when the transfer completes
            /// </summary>
            public Action<bool> OnComplete { get; set; }
        }

        /// <summary>
        /// Represents a file transfer operation
        /// </summary>
        private class FileTransferOperation
        {
            /// <summary>
            /// File data
            /// </summary>
            public byte[] Data;

            /// <summary>
            /// File name
            /// </summary>
            public string FileName;

            /// <summary>
            /// Progress (0-1)
            /// </summary>
            public float Progress;

            /// <summary>
            /// File transfer request
            /// </summary>
            public FileTransferRequestData Request;
        }

        private void OnQueryFileListCompleted(ref QueryFileListCallbackInfo data)
        {
            // Handle result
            if (data.ResultCode != Result.Success)
            {
                LogHelper.Error("EOSPlayerDataStorageProvider", $"Failed to query file list: {data.ResultCode}");
                return;
            }

            // Get file count
            uint fileCount = data.FileCount;
            LogHelper.Info("EOSPlayerDataStorageProvider", $"Found {fileCount} files in EOS Player Data Storage");

            // Process files
            _fileMetadataCache.Clear();
            var playerDataStorageInterface = EOSManager.Instance.GetPlayerDataStorageInterface();

            for (uint i = 0; i < fileCount; i++)
            {
                CopyFileMetadataAtIndexOptions copyOptions = new CopyFileMetadataAtIndexOptions();
                // Use reflection to set the Index or FileIndex property
                var indexProperty = copyOptions.GetType().GetProperty("Index") ??
                                   copyOptions.GetType().GetProperty("FileIndex");
                if (indexProperty != null)
                {
                    indexProperty.SetValue(copyOptions, i);
                }
                copyOptions.LocalUserId = EOSManager.Instance.GetProductUserId();

                // Use the out variant that works with the current SDK version
                try
                {
                    FileMetadata? fileMetadata = null;
                    if (playerDataStorageInterface.CopyFileMetadataAtIndex(ref copyOptions, out fileMetadata) == Result.Success &&
                        fileMetadata.HasValue)
                    {
                        // Create metadata
                        var metadata = new CloudFileMetadata(
                            fileMetadata.Value.Filename,
                            fileMetadata.Value.FileSizeBytes,
                            DateTime.UtcNow, // Default to current time
                            PROVIDER_NAME
                        );
                        metadata.IsSynced = true;

                        // Update cache
                        _fileMetadataCache[fileMetadata.Value.Filename] = metadata;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Warning("EOSPlayerDataStorageProvider", $"Error getting file metadata: {ex.Message}");
                }
            }
        }
    }
}