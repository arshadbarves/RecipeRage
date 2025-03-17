using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using RecipeRage.Modules.Auth;
using RecipeRage.Modules.Friends.Interfaces;
using RecipeRage.Modules.Friends.Utils;
using UnityEngine;

namespace RecipeRage.Modules.Friends.Core
{
    /// <summary>
    /// Implementation of the identity service
    /// Complexity Rating: 3
    /// </summary>
    public class IdentityService : IIdentityService
    {
        private const string SAVE_PATH = "FriendsData";
        private const string FRIEND_CODE_SALT = "RecipeRage2024";
        private string _currentDisplayName;

        private string _currentUserId;
        private Dictionary<string, string> _displayNames;
        private string _friendCode;
        private Dictionary<string, string> _friendCodes;
        private bool _isInitialized;

        /// <summary>
        /// Constructor
        /// </summary>
        public IdentityService()
        {
            _displayNames = new Dictionary<string, string>();
            _friendCodes = new Dictionary<string, string>();
        }

        /// <summary>
        /// Event triggered when a user's display name changes
        /// </summary>
        public event Action<string> OnDisplayNameChanged;

        /// <summary>
        /// Initialize the identity service
        /// </summary>
        /// <param name="onComplete"> Callback when initialization is complete </param>
        public void Initialize(Action<bool> onComplete = null)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("IdentityService: Already initialized");
                onComplete?.Invoke(true);
                return;
            }

            Debug.Log("IdentityService: Initializing...");

            if (!AuthHelper.IsSignedIn())
            {
                Debug.LogWarning("IdentityService: User is not signed in");
                onComplete?.Invoke(false);
                return;
            }

            // Get current user ID and display name from auth system
            _currentUserId = AuthHelper.CurrentUser?.UserId;
            _currentDisplayName = AuthHelper.CurrentUser?.DisplayName;

            if (string.IsNullOrEmpty(_currentUserId))
            {
                Debug.LogError("IdentityService: Failed to get current user ID");
                onComplete?.Invoke(false);
                return;
            }

            // Use default display name if needed
            if (string.IsNullOrEmpty(_currentDisplayName))
            {
                _currentDisplayName = "Player_" + _currentUserId.Substring(0, 6);
            }

            // Load saved data
            LoadDisplayNames();
            LoadFriendCodes();

            // Generate friend code if needed
            if (string.IsNullOrEmpty(_friendCode))
            {
                GenerateFriendCode(code => { _friendCode = code; });
            }

            _isInitialized = true;
            Debug.Log("IdentityService: Initialized successfully");
            onComplete?.Invoke(true);
        }

        /// <summary>
        /// Get the current user's unique identifier
        /// </summary>
        /// <returns> Current user's ID </returns>
        public string GetCurrentUserId()
        {
            return _currentUserId;
        }

        /// <summary>
        /// Get the current user's display name
        /// </summary>
        /// <returns> Current user's display name </returns>
        public string GetCurrentDisplayName()
        {
            return _currentDisplayName;
        }

        /// <summary>
        /// Set the current user's display name
        /// </summary>
        /// <param name="displayName"> New display name </param>
        /// <param name="onComplete"> Callback when the operation is complete </param>
        public void SetDisplayName(string displayName, Action<bool> onComplete = null)
        {
            if (!_isInitialized)
            {
                Debug.LogError("IdentityService: Not initialized");
                onComplete?.Invoke(false);
                return;
            }

            if (string.IsNullOrEmpty(displayName))
            {
                Debug.LogError("IdentityService: Display name cannot be empty");
                onComplete?.Invoke(false);
                return;
            }

            // Update the display name
            string oldDisplayName = _currentDisplayName;
            _currentDisplayName = displayName;

            // Update the local cache
            _displayNames[_currentUserId] = displayName;

            // Save to disk
            SaveDisplayNames();

            // Notify of change
            if (oldDisplayName != displayName)
            {
                OnDisplayNameChanged?.Invoke(displayName);
            }

            Debug.Log($"IdentityService: Display name changed from '{oldDisplayName}' to '{displayName}'");
            onComplete?.Invoke(true);
        }

        /// <summary>
        /// Get a user's display name by their ID
        /// </summary>
        /// <param name="userId"> User ID </param>
        /// <returns> Display name or null if not found </returns>
        public string GetDisplayName(string userId)
        {
            if (!_isInitialized)
            {
                Debug.LogError("IdentityService: Not initialized");
                return null;
            }

            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            if (userId == _currentUserId)
            {
                return _currentDisplayName;
            }

            if (_displayNames.TryGetValue(userId, out string displayName))
            {
                return displayName;
            }

            return null;
        }

        /// <summary>
        /// Get the current user's friend code
        /// </summary>
        /// <returns> Current user's friend code </returns>
        public string GetMyFriendCode()
        {
            if (!_isInitialized)
            {
                Debug.LogError("IdentityService: Not initialized");
                return null;
            }

            return _friendCode;
        }

        /// <summary>
        /// Generate a friend code for the current user if one doesn't exist
        /// </summary>
        /// <param name="onComplete"> Callback with the generated code </param>
        public void GenerateFriendCode(Action<string> onComplete)
        {
            if (!_isInitialized && string.IsNullOrEmpty(_currentUserId))
            {
                Debug.LogError("IdentityService: Cannot generate friend code - not initialized or no user ID");
                onComplete?.Invoke(null);
                return;
            }

            // Generate a code if we don't have one yet
            if (string.IsNullOrEmpty(_friendCode))
            {
                _friendCode = FriendCodeGenerator.GenerateFriendCode(_currentUserId, FRIEND_CODE_SALT);
                _friendCodes[_currentUserId] = _friendCode;

                // Save to disk
                SaveFriendCodes();

                Debug.Log($"IdentityService: Generated friend code: {_friendCode}");
            }

            onComplete?.Invoke(_friendCode);
        }

        /// <summary>
        /// Look up a user by their friend code
        /// </summary>
        /// <param name="friendCode"> Friend code to look up </param>
        /// <param name="onComplete"> Callback with the user ID and display name if found </param>
        public void LookupUserByFriendCode(string friendCode, Action<string, string> onComplete)
        {
            if (!_isInitialized)
            {
                Debug.LogError("IdentityService: Not initialized");
                onComplete?.Invoke(null, null);
                return;
            }

            if (!IsValidFriendCode(friendCode))
            {
                Debug.LogError($"IdentityService: Invalid friend code format: {friendCode}");
                onComplete?.Invoke(null, null);
                return;
            }

            // Check if the code is our own
            if (friendCode == _friendCode)
            {
                Debug.LogWarning("IdentityService: Cannot look up your own friend code");
                onComplete?.Invoke(null, null);
                return;
            }

            // Reverse lookup from code to user ID
            string userId = null;

            foreach (KeyValuePair<string, string> pair in _friendCodes)
            {
                if (pair.Value == friendCode)
                {
                    userId = pair.Key;
                    break;
                }
            }

            if (string.IsNullOrEmpty(userId))
            {
                // Store the code for future use
                // This would typically come from server, but for demo we'll generate a random ID
                userId = Guid.NewGuid().ToString();
                _friendCodes[userId] = friendCode;
                SaveFriendCodes();
            }

            // Get display name if available
            string displayName = GetDisplayName(userId);
            if (string.IsNullOrEmpty(displayName))
            {
                // Use a default name based on the friend code
                displayName = "User_" + friendCode.Substring(0, 4);
                _displayNames[userId] = displayName;
                SaveDisplayNames();
            }

            Debug.Log($"IdentityService: Looked up friend code {friendCode} -> User {displayName} ({userId})");
            onComplete?.Invoke(userId, displayName);
        }

        /// <summary>
        /// Check if a friend code is valid
        /// </summary>
        /// <param name="friendCode"> Friend code to check </param>
        /// <returns> True if the code is valid </returns>
        public bool IsValidFriendCode(string friendCode)
        {
            return FriendCodeGenerator.IsValidFriendCode(friendCode);
        }

        /// <summary>
        /// Add a known display name
        /// </summary>
        /// <param name="userId"> User ID </param>
        /// <param name="displayName"> Display name </param>
        public void AddKnownDisplayName(string userId, string displayName)
        {
            if (!_isInitialized || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(displayName))
            {
                return;
            }

            _displayNames[userId] = displayName;
            SaveDisplayNames();
        }

        /// <summary>
        /// Load display names from persistent storage
        /// </summary>
        private void LoadDisplayNames()
        {
            try
            {
                string filePath = Path.Combine(Application.persistentDataPath, SAVE_PATH, "displayNames.json");
                if (File.Exists(filePath))
                {
                    string jsonData = File.ReadAllText(filePath);
                    _displayNames = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);

                    Debug.Log($"IdentityService: Loaded {_displayNames.Count} display names");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"IdentityService: Error loading display names: {ex.Message}");
            }
        }

        /// <summary>
        /// Save display names to persistent storage
        /// </summary>
        private void SaveDisplayNames()
        {
            try
            {
                string dirPath = Path.Combine(Application.persistentDataPath, SAVE_PATH);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                string filePath = Path.Combine(dirPath, "displayNames.json");
                string jsonData = JsonConvert.SerializeObject(_displayNames);

                File.WriteAllText(filePath, jsonData);

                Debug.Log($"IdentityService: Saved {_displayNames.Count} display names");
            }
            catch (Exception ex)
            {
                Debug.LogError($"IdentityService: Error saving display names: {ex.Message}");
            }
        }

        /// <summary>
        /// Load friend codes from persistent storage
        /// </summary>
        private void LoadFriendCodes()
        {
            try
            {
                string filePath = Path.Combine(Application.persistentDataPath, SAVE_PATH, "friendCodes.json");
                if (File.Exists(filePath))
                {
                    string jsonData = File.ReadAllText(filePath);
                    _friendCodes = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);

                    // Get current user's friend code if it exists
                    if (_friendCodes.TryGetValue(_currentUserId, out string friendCode))
                    {
                        _friendCode = friendCode;
                    }

                    Debug.Log($"IdentityService: Loaded {_friendCodes.Count} friend codes");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"IdentityService: Error loading friend codes: {ex.Message}");
            }
        }

        /// <summary>
        /// Save friend codes to persistent storage
        /// </summary>
        private void SaveFriendCodes()
        {
            try
            {
                string dirPath = Path.Combine(Application.persistentDataPath, SAVE_PATH);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                string filePath = Path.Combine(dirPath, "friendCodes.json");
                string jsonData = JsonConvert.SerializeObject(_friendCodes);

                File.WriteAllText(filePath, jsonData);

                Debug.Log($"IdentityService: Saved {_friendCodes.Count} friend codes");
            }
            catch (Exception ex)
            {
                Debug.LogError($"IdentityService: Error saving friend codes: {ex.Message}");
            }
        }
    }
}