using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using RecipeRage.Modules.Cloud;
using RecipeRage.Modules.Cloud.Interfaces;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Examples
{
    /// <summary>
    /// Example script demonstrating how to use the Cloud Storage module
    /// Shows saving, loading, and managing files in cloud storage using Epic Online Services
    /// 
    /// Complexity Rating: 2
    /// </summary>
    public class CloudStorageExample : MonoBehaviour
    {
        [Header("Cloud Storage Settings")]
        [SerializeField] private bool _autoSyncOnStart = true;
        [SerializeField] private string _defaultFileName = "example_data.json";
        
        [Header("UI Elements")]
        [SerializeField] private InputField _fileNameInput;
        [SerializeField] private InputField _contentInput;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _loadButton;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private Button _listFilesButton;
        [SerializeField] private Button _syncButton;
        [SerializeField] private Text _statusText;
        [SerializeField] private Text _fileListText;
        [SerializeField] private Text _quotaText;
        [SerializeField] private Toggle _jsonFormatToggle;
        
        [Header("Example Data")]
        [SerializeField] private string _playerName = "ExamplePlayer";
        [SerializeField] private int _playerScore = 1000;
        [SerializeField] private bool _tutorialCompleted = true;
        
        // Track initialization state
        private bool _isInitialized = false;
        
        /// <summary>
        /// Initialize on enable
        /// </summary>
        private void OnEnable()
        {
            InitializeCloudStorage();
            SetupUI();
            RegisterEventHandlers();
        }
        
        /// <summary>
        /// Clean up on disable
        /// </summary>
        private void OnDisable()
        {
            UnregisterEventHandlers();
        }
        
        /// <summary>
        /// Initialize cloud storage
        /// </summary>
        private void InitializeCloudStorage()
        {
            UpdateStatusText("Initializing cloud storage...");
            
            CloudStorageHelper.Initialize(success =>
            {
                _isInitialized = success;
                
                if (success)
                {
                    UpdateStatusText("Cloud storage initialized successfully");
                    UpdateQuotaText();
                    
                    // Auto sync if enabled
                    if (_autoSyncOnStart)
                    {
                        SyncFiles();
                    }
                }
                else
                {
                    UpdateStatusText("Failed to initialize cloud storage");
                }
                
                // Update UI button states
                UpdateUIState();
            });
        }
        
        /// <summary>
        /// Set up UI elements
        /// </summary>
        private void SetupUI()
        {
            // Set default file name
            if (_fileNameInput != null)
            {
                _fileNameInput.text = _defaultFileName;
            }
            
            // Setup example content
            if (_contentInput != null && string.IsNullOrEmpty(_contentInput.text))
            {
                UpdateContentInput();
            }
            
            // Set up button callbacks
            if (_saveButton != null)
            {
                _saveButton.onClick.AddListener(OnSaveButtonClicked);
            }
            
            if (_loadButton != null)
            {
                _loadButton.onClick.AddListener(OnLoadButtonClicked);
            }
            
            if (_deleteButton != null)
            {
                _deleteButton.onClick.AddListener(OnDeleteButtonClicked);
            }
            
            if (_listFilesButton != null)
            {
                _listFilesButton.onClick.AddListener(OnListFilesButtonClicked);
            }
            
            if (_syncButton != null)
            {
                _syncButton.onClick.AddListener(OnSyncButtonClicked);
            }
            
            if (_jsonFormatToggle != null)
            {
                _jsonFormatToggle.onValueChanged.AddListener(OnJsonFormatToggleChanged);
            }
            
            // Update UI button states
            UpdateUIState();
        }
        
        /// <summary>
        /// Register event handlers
        /// </summary>
        private void RegisterEventHandlers()
        {
            CloudStorageHelper.RegisterFileSavedCallback(OnFileSaved);
            CloudStorageHelper.RegisterFileLoadedCallback(OnFileLoaded);
            CloudStorageHelper.RegisterFileDeletedCallback(OnFileDeleted);
            CloudStorageHelper.RegisterSyncCompletedCallback(OnSyncCompleted);
        }
        
        /// <summary>
        /// Unregister event handlers
        /// </summary>
        private void UnregisterEventHandlers()
        {
            CloudStorageHelper.UnregisterFileSavedCallback(OnFileSaved);
            CloudStorageHelper.UnregisterFileLoadedCallback(OnFileLoaded);
            CloudStorageHelper.UnregisterFileDeletedCallback(OnFileDeleted);
            CloudStorageHelper.UnregisterSyncCompletedCallback(OnSyncCompleted);
        }
        
        /// <summary>
        /// Handle save button click
        /// </summary>
        private void OnSaveButtonClicked()
        {
            if (!CheckInitialized())
            {
                return;
            }
            
            string fileName = GetFileName();
            if (string.IsNullOrEmpty(fileName))
            {
                UpdateStatusText("File name cannot be empty");
                return;
            }
            
            string content = _contentInput != null ? _contentInput.text : string.Empty;
            if (string.IsNullOrEmpty(content))
            {
                UpdateStatusText("Content cannot be empty");
                return;
            }
            
            UpdateStatusText($"Saving file {fileName}...");
            
            // Save as JSON or plain text
            if (_jsonFormatToggle != null && _jsonFormatToggle.isOn)
            {
                // Save as JSON object
                var exampleData = new ExampleData
                {
                    PlayerName = _playerName,
                    PlayerScore = _playerScore,
                    TutorialCompleted = _tutorialCompleted,
                    LastSaved = DateTime.UtcNow,
                    CustomData = content
                };
                
                CloudStorageHelper.SaveJson(fileName, exampleData, (success, error) =>
                {
                    if (success)
                    {
                        UpdateStatusText($"File {fileName} saved successfully as JSON");
                    }
                    else
                    {
                        UpdateStatusText($"Failed to save file: {error}");
                    }
                });
            }
            else
            {
                // Save as plain text
                CloudStorageHelper.SaveText(fileName, content, (success, error) =>
                {
                    if (success)
                    {
                        UpdateStatusText($"File {fileName} saved successfully as text");
                    }
                    else
                    {
                        UpdateStatusText($"Failed to save file: {error}");
                    }
                });
            }
        }
        
        /// <summary>
        /// Handle load button click
        /// </summary>
        private void OnLoadButtonClicked()
        {
            if (!CheckInitialized())
            {
                return;
            }
            
            string fileName = GetFileName();
            if (string.IsNullOrEmpty(fileName))
            {
                UpdateStatusText("File name cannot be empty");
                return;
            }
            
            UpdateStatusText($"Loading file {fileName}...");
            
            // Load as JSON or plain text
            if (_jsonFormatToggle != null && _jsonFormatToggle.isOn)
            {
                // Load as JSON object
                CloudStorageHelper.LoadJson<ExampleData>(fileName, (data, error) =>
                {
                    if (data != null)
                    {
                        _playerName = data.PlayerName;
                        _playerScore = data.PlayerScore;
                        _tutorialCompleted = data.TutorialCompleted;
                        
                        // Update content input
                        if (_contentInput != null)
                        {
                            string jsonData = JsonUtility.ToJson(data, true);
                            _contentInput.text = jsonData;
                        }
                        
                        UpdateStatusText($"File {fileName} loaded successfully as JSON");
                    }
                    else
                    {
                        UpdateStatusText($"Failed to load file: {error}");
                    }
                });
            }
            else
            {
                // Load as plain text
                CloudStorageHelper.LoadText(fileName, (text, error) =>
                {
                    if (text != null)
                    {
                        // Update content input
                        if (_contentInput != null)
                        {
                            _contentInput.text = text;
                        }
                        
                        UpdateStatusText($"File {fileName} loaded successfully as text");
                    }
                    else
                    {
                        UpdateStatusText($"Failed to load file: {error}");
                    }
                });
            }
        }
        
        /// <summary>
        /// Handle delete button click
        /// </summary>
        private void OnDeleteButtonClicked()
        {
            if (!CheckInitialized())
            {
                return;
            }
            
            string fileName = GetFileName();
            if (string.IsNullOrEmpty(fileName))
            {
                UpdateStatusText("File name cannot be empty");
                return;
            }
            
            UpdateStatusText($"Deleting file {fileName}...");
            
            CloudStorageHelper.DeleteFile(fileName, (success, error) =>
            {
                if (success)
                {
                    UpdateStatusText($"File {fileName} deleted successfully");
                    UpdateQuotaText();
                }
                else
                {
                    UpdateStatusText($"Failed to delete file: {error}");
                }
            });
        }
        
        /// <summary>
        /// Handle list files button click
        /// </summary>
        private void OnListFilesButtonClicked()
        {
            if (!CheckInitialized())
            {
                return;
            }
            
            UpdateStatusText("Listing files...");
            
            CloudStorageHelper.ListFiles((files, error) =>
            {
                if (files != null)
                {
                    UpdateFileListText(files);
                    UpdateStatusText($"Listed {files.Count} files");
                }
                else
                {
                    UpdateStatusText($"Failed to list files: {error}");
                }
            });
        }
        
        /// <summary>
        /// Handle sync button click
        /// </summary>
        private void OnSyncButtonClicked()
        {
            SyncFiles();
        }
        
        /// <summary>
        /// Handle JSON format toggle change
        /// </summary>
        /// <param name="isOn">Toggle state</param>
        private void OnJsonFormatToggleChanged(bool isOn)
        {
            UpdateContentInput();
        }
        
        /// <summary>
        /// Sync files with cloud storage
        /// </summary>
        private void SyncFiles()
        {
            if (!CheckInitialized())
            {
                return;
            }
            
            UpdateStatusText("Syncing files...");
            
            CloudStorageHelper.Sync((success, syncCount, error) =>
            {
                // Sync complete callback is handled by event handler
            });
        }
        
        /// <summary>
        /// Update the content input based on the format toggle
        /// </summary>
        private void UpdateContentInput()
        {
            if (_contentInput == null)
            {
                return;
            }
            
            if (_jsonFormatToggle != null && _jsonFormatToggle.isOn)
            {
                // Show JSON example
                var exampleData = new ExampleData
                {
                    PlayerName = _playerName,
                    PlayerScore = _playerScore,
                    TutorialCompleted = _tutorialCompleted,
                    LastSaved = DateTime.UtcNow,
                    CustomData = "This is an example of JSON data"
                };
                
                string jsonData = JsonUtility.ToJson(exampleData, true);
                _contentInput.text = jsonData;
            }
            else
            {
                // Show plain text example
                _contentInput.text = "This is an example of plain text data";
            }
        }
        
        /// <summary>
        /// Update the UI state based on initialization
        /// </summary>
        private void UpdateUIState()
        {
            bool enabled = _isInitialized;
            
            if (_saveButton != null) _saveButton.interactable = enabled;
            if (_loadButton != null) _loadButton.interactable = enabled;
            if (_deleteButton != null) _deleteButton.interactable = enabled;
            if (_listFilesButton != null) _listFilesButton.interactable = enabled;
            if (_syncButton != null) _syncButton.interactable = enabled;
            if (_fileNameInput != null) _fileNameInput.interactable = enabled;
            if (_contentInput != null) _contentInput.interactable = enabled;
            if (_jsonFormatToggle != null) _jsonFormatToggle.interactable = enabled;
        }
        
        /// <summary>
        /// Get the current file name from the input field
        /// </summary>
        /// <returns>File name</returns>
        private string GetFileName()
        {
            return _fileNameInput != null ? _fileNameInput.text : _defaultFileName;
        }
        
        /// <summary>
        /// Update the status text
        /// </summary>
        /// <param name="status">Status message</param>
        private void UpdateStatusText(string status)
        {
            if (_statusText != null)
            {
                _statusText.text = status;
                LogHelper.Info("CloudStorageExample", status);
            }
        }
        
        /// <summary>
        /// Update the file list text
        /// </summary>
        /// <param name="files">List of files</param>
        private void UpdateFileListText(List<CloudFileMetadata> files)
        {
            if (_fileListText == null)
            {
                return;
            }
            
            if (files == null || files.Count == 0)
            {
                _fileListText.text = "No files found";
                return;
            }
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Files ({files.Count}):");
            
            foreach (var file in files)
            {
                sb.AppendLine($"- {file.FileName} ({file.GetReadableSize()}, {file.LastModified})");
            }
            
            _fileListText.text = sb.ToString();
        }
        
        /// <summary>
        /// Update quota text
        /// </summary>
        private void UpdateQuotaText()
        {
            if (_quotaText == null || !_isInitialized)
            {
                return;
            }
            
            CloudStorageHelper.GetStorageQuota((used, total, error) =>
            {
                if (string.IsNullOrEmpty(error))
                {
                    float percentage = total > 0 ? (float)used / total * 100 : 0;
                    
                    string usedStr = FormatSize(used);
                    string totalStr = FormatSize(total);
                    
                    _quotaText.text = $"Storage: {usedStr} / {totalStr} ({percentage:F1}%)";
                }
                else
                {
                    _quotaText.text = "Storage: Unknown";
                }
            });
        }
        
        /// <summary>
        /// Format a size in bytes to a human-readable string
        /// </summary>
        /// <param name="size">Size in bytes</param>
        /// <returns>Formatted size</returns>
        private string FormatSize(long size)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = size;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            
            return $"{len:0.##} {sizes[order]}";
        }
        
        /// <summary>
        /// Check if cloud storage is initialized
        /// </summary>
        /// <returns>True if initialized, false otherwise</returns>
        private bool CheckInitialized()
        {
            if (!_isInitialized)
            {
                UpdateStatusText("Cloud storage is not initialized");
                return false;
            }
            
            return true;
        }
        
        #region Event Handlers
        
        /// <summary>
        /// Handle file saved event
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="success">Whether the operation was successful</param>
        private void OnFileSaved(string fileName, bool success)
        {
            UpdateQuotaText();
        }
        
        /// <summary>
        /// Handle file loaded event
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="success">Whether the operation was successful</param>
        private void OnFileLoaded(string fileName, bool success)
        {
            // Nothing to do here for now
        }
        
        /// <summary>
        /// Handle file deleted event
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="success">Whether the operation was successful</param>
        private void OnFileDeleted(string fileName, bool success)
        {
            UpdateQuotaText();
        }
        
        /// <summary>
        /// Handle sync completed event
        /// </summary>
        /// <param name="success">Whether the operation was successful</param>
        /// <param name="syncCount">Number of files synced</param>
        /// <param name="error">Error message if any</param>
        private void OnSyncCompleted(bool success, int syncCount, string error)
        {
            if (success)
            {
                UpdateStatusText($"Sync completed successfully with {syncCount} files synced");
                CloudStorageHelper.ListFiles((files, listError) =>
                {
                    if (files != null)
                    {
                        UpdateFileListText(files);
                    }
                });
            }
            else
            {
                UpdateStatusText($"Sync failed: {error}");
            }
            
            UpdateQuotaText();
        }
        
        #endregion
        
        /// <summary>
        /// Show cloud storage status
        /// </summary>
        public void ShowStatus()
        {
            var status = CloudStorageHelper.GetStatus();
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Cloud Storage Status:");
            sb.AppendLine($"Initialized: {status.IsInitialized}");
            sb.AppendLine($"Providers: {status.ProviderCount}");
            
            if (status.AvailableProviders != null && status.AvailableProviders.Count > 0)
            {
                sb.AppendLine($"Available Providers: {string.Join(", ", status.AvailableProviders)}");
            }
            
            sb.AppendLine($"Total Files: {status.TotalFiles}");
            sb.AppendLine($"Local Files: {status.LocalCachedFiles}");
            sb.AppendLine($"Last Sync: {status.LastSyncTime}");
            sb.AppendLine($"Used Storage: {status.GetReadableUsedStorage()}");
            sb.AppendLine($"Total Storage: {status.GetReadableTotalStorage()}");
            
            if (!string.IsNullOrEmpty(status.LastError))
            {
                sb.AppendLine($"Last Error: {status.LastError}");
            }
            
            LogHelper.Info("CloudStorageExample", sb.ToString());
            
            if (_statusText != null)
            {
                _statusText.text = sb.ToString();
            }
        }
        
        /// <summary>
        /// Example data class for JSON serialization
        /// </summary>
        [Serializable]
        private class ExampleData
        {
            public string PlayerName;
            public int PlayerScore;
            public bool TutorialCompleted;
            public DateTime LastSaved;
            public string CustomData;
        }
    }
} 