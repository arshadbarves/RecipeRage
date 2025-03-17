using System;
using System.Collections.Generic;
using RecipeRage.Modules.Auth;
using RecipeRage.Modules.Friends.Data;
using RecipeRage.Modules.Friends.Interfaces;
using UnityEngine;

namespace RecipeRage.Modules.Friends.Core
{
    /// <summary>
    /// Implementation of the presence service
    /// Complexity Rating: 3
    /// </summary>
    public class PresenceService : IPresenceService
    {
        private const float PRESENCE_UPDATE_INTERVAL = 30f; // seconds
        private readonly Dictionary<string, PresenceData> _friendsPresence;
        private bool _isInitialized;
        private DateTime _lastPresenceUpdate;
        private PresenceData _myPresence;

        /// <summary>
        /// Constructor
        /// </summary>
        public PresenceService()
        {
            _friendsPresence = new Dictionary<string, PresenceData>();
            _lastPresenceUpdate = DateTime.UtcNow;
        }

        /// <summary>
        /// Event triggered when a friend's presence changes
        /// </summary>
        public event Action<string, PresenceData> OnFriendPresenceChanged;

        /// <summary>
        /// Initialize the presence service
        /// </summary>
        /// <param name="onComplete"> Callback when initialization is complete </param>
        public void Initialize(Action<bool> onComplete = null)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("PresenceService: Already initialized");
                onComplete?.Invoke(true);
                return;
            }

            Debug.Log("PresenceService: Initializing...");

            if (!AuthHelper.IsSignedIn())
            {
                Debug.LogWarning("PresenceService: User is not signed in");
                onComplete?.Invoke(false);
                return;
            }

            // Initialize my presence
            _myPresence = new PresenceData
            {
                UserId = AuthHelper.CurrentUser.UserId,
                DisplayName = AuthHelper.CurrentUser.DisplayName,
                Status = UserStatus.Online,
                LastOnline = DateTime.UtcNow,
                Activity = "",
                IsJoinable = false,
                JoinData = null
            };

            // Register for app lifecycle events
            FriendsServiceUpdater.Instance.OnApplicationPauseEvent += OnApplicationPause;
            FriendsServiceUpdater.Instance.OnApplicationQuitEvent += OnApplicationQuit;
            FriendsServiceUpdater.Instance.OnUpdate += Update;

            _isInitialized = true;
            Debug.Log("PresenceService: Initialized successfully");
            onComplete?.Invoke(true);
        }

        /// <summary>
        /// Set the current user's status
        /// </summary>
        /// <param name="status"> New status </param>
        public void SetStatus(UserStatus status)
        {
            if (!_isInitialized)
            {
                Debug.LogError("PresenceService: Not initialized");
                return;
            }

            if (_myPresence.Status == status) return;

            _myPresence.Status = status;

            if (status != UserStatus.Offline) _myPresence.LastOnline = DateTime.UtcNow;

            Debug.Log($"PresenceService: Status set to {status}");
        }

        /// <summary>
        /// Set the current user's activity
        /// </summary>
        /// <param name="activity"> Activity description </param>
        /// <param name="joinable"> Whether the activity can be joined </param>
        /// <param name="joinData"> Data needed to join the activity </param>
        public void SetActivity(string activity, bool joinable = false, string joinData = null)
        {
            if (!_isInitialized)
            {
                Debug.LogError("PresenceService: Not initialized");
                return;
            }

            _myPresence.Activity = activity ?? "";
            _myPresence.IsJoinable = joinable;
            _myPresence.JoinData = joinData;

            Debug.Log($"PresenceService: Activity set to '{activity}' (joinable: {joinable})");
        }

        /// <summary>
        /// Get a friend's presence information
        /// </summary>
        /// <param name="friendId"> Friend's ID </param>
        /// <returns> Presence data or null if not available </returns>
        public PresenceData GetFriendPresence(string friendId)
        {
            if (!_isInitialized)
            {
                Debug.LogError("PresenceService: Not initialized");
                return null;
            }

            if (string.IsNullOrEmpty(friendId)) return null;

            if (_friendsPresence.TryGetValue(friendId, out var presence)) return presence;

            return null;
        }

        /// <summary>
        /// Get presence information for all friends
        /// </summary>
        /// <returns> Dictionary mapping friend IDs to presence data </returns>
        public IReadOnlyDictionary<string, PresenceData> GetAllFriendsPresence()
        {
            if (!_isInitialized)
            {
                Debug.LogError("PresenceService: Not initialized");
                return new Dictionary<string, PresenceData>();
            }

            return _friendsPresence;
        }

        /// <summary>
        /// Check if a friend is online
        /// </summary>
        /// <param name="friendId"> Friend's ID </param>
        /// <returns> True if the friend is online </returns>
        public bool IsFriendOnline(string friendId)
        {
            if (!_isInitialized)
            {
                Debug.LogError("PresenceService: Not initialized");
                return false;
            }

            if (string.IsNullOrEmpty(friendId)) return false;

            if (!_friendsPresence.TryGetValue(friendId, out var presence)) return false;

            return presence.Status != UserStatus.Offline;
        }

        /// <summary>
        /// Get the current user's presence data
        /// </summary>
        /// <returns> Current user's presence </returns>
        public PresenceData GetMyPresence()
        {
            if (!_isInitialized)
            {
                Debug.LogError("PresenceService: Not initialized");
                return null;
            }

            return _myPresence;
        }

        /// <summary>
        /// Update method for periodic checks
        /// </summary>
        private void Update()
        {
            if (!_isInitialized) return;

            // Check if we should update presence (e.g., for session duration)
            if ((DateTime.UtcNow - _lastPresenceUpdate).TotalSeconds > PRESENCE_UPDATE_INTERVAL)
            {
                _lastPresenceUpdate = DateTime.UtcNow;

                // Update any timed-out friend presences
                var offlineFriends = new List<string>();

                foreach (KeyValuePair<string, PresenceData> pair in _friendsPresence)
                    if (pair.Value.Status != UserStatus.Offline &&
                        (DateTime.UtcNow - pair.Value.LastOnline).TotalMinutes > 5)
                        // Friend hasn't updated presence in 5 minutes, mark as offline
                        offlineFriends.Add(pair.Key);

                foreach (string friendId in offlineFriends)
                {
                    var presence = _friendsPresence[friendId];
                    presence.Status = UserStatus.Offline;
                    presence.Activity = "";
                    presence.IsJoinable = false;
                    presence.JoinData = null;

                    _friendsPresence[friendId] = presence;

                    // Notify listeners
                    OnFriendPresenceChanged?.Invoke(friendId, presence);
                }
            }
        }

        /// <summary>
        /// Update a friend's presence data
        /// </summary>
        /// <param name="friendId"> Friend ID </param>
        /// <param name="presenceData"> New presence data </param>
        public void UpdateFriendPresence(string friendId, PresenceData presenceData)
        {
            if (!_isInitialized)
            {
                Debug.LogError("PresenceService: Not initialized");
                return;
            }

            if (string.IsNullOrEmpty(friendId) || presenceData == null)
            {
                Debug.LogError("PresenceService: Invalid friend ID or presence data");
                return;
            }

            // Store the old presence for comparison
            PresenceData oldPresence = null;
            if (_friendsPresence.TryGetValue(friendId, out var existing)) oldPresence = existing;

            // Update the presence
            _friendsPresence[friendId] = presenceData;

            // Notify listeners if anything changed
            if (oldPresence == null ||
                oldPresence.Status != presenceData.Status ||
                oldPresence.Activity != presenceData.Activity ||
                oldPresence.IsJoinable != presenceData.IsJoinable)
                OnFriendPresenceChanged?.Invoke(friendId, presenceData);

            Debug.Log(
                $"PresenceService: Updated presence for friend {presenceData.DisplayName} ({friendId}): {presenceData.Status}");
        }

        /// <summary>
        /// Handle application pause
        /// </summary>
        /// <param name="isPaused"> Whether the application is paused </param>
        private void OnApplicationPause(bool isPaused)
        {
            if (!_isInitialized) return;

            if (isPaused)
                SetStatus(UserStatus.Away);
            else
                SetStatus(UserStatus.Online);
        }

        /// <summary>
        /// Handle application quit
        /// </summary>
        private void OnApplicationQuit()
        {
            if (!_isInitialized) return;

            SetStatus(UserStatus.Offline);
        }
    }
}