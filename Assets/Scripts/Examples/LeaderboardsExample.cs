using System.Collections.Generic;
using RecipeRage.Leaderboards;
using RecipeRage.Modules.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace RecipeRage.Examples
{
    /// <summary>
    /// Example class demonstrating usage of the Leaderboards module
    /// </summary>
    public class LeaderboardsExample : MonoBehaviour
    {
        [Header("Leaderboard Settings")] [SerializeField]
        private string _leaderboardId = "reciperage_highscores";

        [SerializeField] private int _pageSize = 10;
        [SerializeField] private bool _includeFriends;

        [Header("Score Settings")] [SerializeField]
        private int _scoreIncrement = 100;

        [SerializeField] private int _startRank = 1;

        [Header("UI Elements")] [SerializeField]
        private InputField _leaderboardIdInput;

        [SerializeField] private InputField _scoreInput;
        [SerializeField] private Button _initializeButton;
        [SerializeField] private Button _queryLeaderboardButton;
        [SerializeField] private Button _queryFriendsButton;
        [SerializeField] private Button _getCurrentUserEntryButton;
        [SerializeField] private Button _submitScoreButton;
        [SerializeField] private Toggle _friendsToggle;
        [SerializeField] private Dropdown _pageSizeDropdown;
        [SerializeField] private Text _statusText;
        [SerializeField] private ScrollRect _leaderboardScrollRect;
        [SerializeField] private Transform _leaderboardContentTransform;
        [SerializeField] private GameObject _leaderboardEntryPrefab;

        // Reference to the cached leaderboard definition (if any)
        private LeaderboardDefinition _currentDefinition;

        // Current leaderboard entries
        private List<LeaderboardEntry> _currentEntries = new List<LeaderboardEntry>();

        // Track if the system is initialized
        private bool _isInitialized;

        private void OnEnable()
        {
            // Initialize UI
            InitializeUI();

            // Register for events
            RegisterEvents();
        }

        private void OnDisable()
        {
            // Unregister from events
            UnregisterEvents();
        }

        /// <summary>
        /// Initializes the UI elements
        /// </summary>
        private void InitializeUI()
        {
            // Initialize UI elements
            if (_leaderboardIdInput != null)
                _leaderboardIdInput.text = _leaderboardId;

            if (_scoreInput != null)
                _scoreInput.text = _scoreIncrement.ToString();

            if (_initializeButton != null)
                _initializeButton.onClick.AddListener(OnInitializeButtonClicked);

            if (_queryLeaderboardButton != null)
                _queryLeaderboardButton.onClick.AddListener(OnQueryLeaderboardButtonClicked);

            if (_queryFriendsButton != null)
                _queryFriendsButton.onClick.AddListener(OnQueryFriendsButtonClicked);

            if (_getCurrentUserEntryButton != null)
                _getCurrentUserEntryButton.onClick.AddListener(OnGetCurrentUserEntryButtonClicked);

            if (_submitScoreButton != null)
                _submitScoreButton.onClick.AddListener(OnSubmitScoreButtonClicked);

            if (_friendsToggle != null)
                _friendsToggle.onValueChanged.AddListener(value => _includeFriends = value);

            if (_pageSizeDropdown != null)
            {
                _pageSizeDropdown.ClearOptions();
                _pageSizeDropdown.AddOptions(new List<string> { "5", "10", "25", "50", "100" });
                _pageSizeDropdown.value = 1; // Default to 10
                _pageSizeDropdown.onValueChanged.AddListener(OnPageSizeChanged);
            }

            UpdateUIState();
        }

        /// <summary>
        /// Registers for leaderboard events
        /// </summary>
        private void RegisterEvents()
        {
            LeaderboardsHelper.RegisterOnLeaderboardQueried(OnLeaderboardQueried);
            LeaderboardsHelper.RegisterOnScoreSubmitted(OnScoreSubmitted);
            LeaderboardsHelper.RegisterOnScoreSubmissionFailed(OnScoreSubmissionFailed);
        }

        /// <summary>
        /// Unregisters from leaderboard events
        /// </summary>
        private void UnregisterEvents()
        {
            LeaderboardsHelper.UnregisterOnLeaderboardQueried(OnLeaderboardQueried);
            LeaderboardsHelper.UnregisterOnScoreSubmitted(OnScoreSubmitted);
            LeaderboardsHelper.UnregisterOnScoreSubmissionFailed(OnScoreSubmissionFailed);
        }

        /// <summary>
        /// Handles page size dropdown change
        /// </summary>
        /// <param name="index"> Selected index </param>
        private void OnPageSizeChanged(int index)
        {
            int[] sizes = { 5, 10, 25, 50, 100 };
            if (index >= 0 && index < sizes.Length) _pageSize = sizes[index];
        }

        /// <summary>
        /// Handles Initialize button click
        /// </summary>
        private void OnInitializeButtonClicked()
        {
            SetStatus("Initializing leaderboards...");

            LeaderboardsHelper.Initialize(success =>
            {
                _isInitialized = success;
                if (success)
                {
                    SetStatus("Leaderboards initialized successfully!");
                    GetLeaderboardDefinitions();
                }
                else
                {
                    SetStatus("Failed to initialize leaderboards!");
                }

                UpdateUIState();
            });
        }

        /// <summary>
        /// Gets all available leaderboard definitions
        /// </summary>
        private void GetLeaderboardDefinitions()
        {
            LeaderboardsHelper.GetLeaderboardDefinitions(definitions =>
            {
                if (definitions != null && definitions.Count > 0)
                {
                    SetStatus($"Found {definitions.Count} leaderboard definitions");
                    LogHelper.Debug("LeaderboardsExample", $"Found {definitions.Count} leaderboard definitions");

                    foreach (var def in definitions)
                        LogHelper.Debug("LeaderboardsExample",
                            $"Leaderboard: {def.LeaderboardId}, Stat: {def.StatName}");

                    // Find the definition for our leaderboard
                    _currentDefinition = definitions.Find(d => d.LeaderboardId == _leaderboardId);
                }
                else
                {
                    SetStatus("No leaderboard definitions found");
                }
            });
        }

        /// <summary>
        /// Handles Query Leaderboard button click
        /// </summary>
        private void OnQueryLeaderboardButtonClicked()
        {
            if (!_isInitialized)
            {
                SetStatus("Leaderboards not initialized!");
                return;
            }

            // Get the leaderboard ID from the input field
            if (_leaderboardIdInput != null)
                _leaderboardId = _leaderboardIdInput.text;

            if (string.IsNullOrEmpty(_leaderboardId))
            {
                SetStatus("Leaderboard ID cannot be empty!");
                return;
            }

            SetStatus($"Querying leaderboard {_leaderboardId}...");

            LeaderboardsHelper.GetLeaderboardEntries(_leaderboardId, _startRank, _pageSize, entries =>
            {
                if (entries != null)
                {
                    SetStatus($"Retrieved {entries.Count} entries for leaderboard {_leaderboardId}");
                    _currentEntries = entries;
                    DisplayLeaderboardEntries(entries);
                }
                else
                {
                    SetStatus($"Failed to retrieve entries for leaderboard {_leaderboardId}");
                }
            });
        }

        /// <summary>
        /// Handles Query Friends button click
        /// </summary>
        private void OnQueryFriendsButtonClicked()
        {
            if (!_isInitialized)
            {
                SetStatus("Leaderboards not initialized!");
                return;
            }

            // Get the leaderboard ID from the input field
            if (_leaderboardIdInput != null)
                _leaderboardId = _leaderboardIdInput.text;

            if (string.IsNullOrEmpty(_leaderboardId))
            {
                SetStatus("Leaderboard ID cannot be empty!");
                return;
            }

            SetStatus($"Querying friends leaderboard {_leaderboardId}...");

            LeaderboardsHelper.GetLeaderboardEntriesForFriends(_leaderboardId, entries =>
            {
                if (entries != null)
                {
                    SetStatus($"Retrieved {entries.Count} friend entries for leaderboard {_leaderboardId}");
                    _currentEntries = entries;
                    DisplayLeaderboardEntries(entries);
                }
                else
                {
                    SetStatus($"Failed to retrieve friend entries for leaderboard {_leaderboardId}");
                }
            });
        }

        /// <summary>
        /// Handles Get Current User Entry button click
        /// </summary>
        private void OnGetCurrentUserEntryButtonClicked()
        {
            if (!_isInitialized)
            {
                SetStatus("Leaderboards not initialized!");
                return;
            }

            // Get the leaderboard ID from the input field
            if (_leaderboardIdInput != null)
                _leaderboardId = _leaderboardIdInput.text;

            if (string.IsNullOrEmpty(_leaderboardId))
            {
                SetStatus("Leaderboard ID cannot be empty!");
                return;
            }

            SetStatus($"Querying current user entry for leaderboard {_leaderboardId}...");

            LeaderboardsHelper.GetCurrentUserLeaderboardEntry(_leaderboardId, entry =>
            {
                if (entry != null)
                {
                    SetStatus(
                        $"Current user is ranked {entry.Rank} with score {entry.Score} on leaderboard {_leaderboardId}");

                    // Display the entry
                    _currentEntries = new List<LeaderboardEntry> { entry };
                    DisplayLeaderboardEntries(_currentEntries);
                }
                else
                {
                    SetStatus($"Current user not found on leaderboard {_leaderboardId}");
                }
            });
        }

        /// <summary>
        /// Handles Submit Score button click
        /// </summary>
        private void OnSubmitScoreButtonClicked()
        {
            if (!_isInitialized)
            {
                SetStatus("Leaderboards not initialized!");
                return;
            }

            // Get the leaderboard ID from the input field
            if (_leaderboardIdInput != null)
                _leaderboardId = _leaderboardIdInput.text;

            if (string.IsNullOrEmpty(_leaderboardId))
            {
                SetStatus("Leaderboard ID cannot be empty!");
                return;
            }

            // Get the score from the input field
            long score = _scoreIncrement;
            if (_scoreInput != null)
                if (!long.TryParse(_scoreInput.text, out score))
                {
                    SetStatus("Invalid score value!");
                    return;
                }

            SetStatus($"Submitting score {score} to leaderboard {_leaderboardId}...");

            LeaderboardsHelper.SubmitScore(_leaderboardId, score, success =>
            {
                if (success)
                {
                    SetStatus($"Successfully submitted score {score} to leaderboard {_leaderboardId}");

                    // Refresh the leaderboard
                    OnQueryLeaderboardButtonClicked();
                }
                else
                {
                    SetStatus($"Failed to submit score {score} to leaderboard {_leaderboardId}");
                }
            });
        }

        /// <summary>
        /// Displays leaderboard entries in the UI
        /// </summary>
        /// <param name="entries"> Entries to display </param>
        private void DisplayLeaderboardEntries(List<LeaderboardEntry> entries)
        {
            if (_leaderboardContentTransform == null || _leaderboardEntryPrefab == null)
                return;

            // Clear existing entries
            foreach (Transform child in _leaderboardContentTransform) Destroy(child.gameObject);

            // Display new entries
            foreach (var entry in entries)
            {
                var entryObj = Instantiate(_leaderboardEntryPrefab, _leaderboardContentTransform);
                var entryUI = entryObj.GetComponent<LeaderboardEntryUI>();

                if (entryUI != null)
                {
                    entryUI.SetEntry(entry, _currentDefinition);
                }
                else
                {
                    // If no custom component, just try to set text components
                    Text[] texts = entryObj.GetComponentsInChildren<Text>();
                    if (texts.Length >= 3)
                    {
                        texts[0].text = LeaderboardsFormatter.FormatRank(entry.Rank);
                        texts[1].text = entry.DisplayName;
                        texts[2].text = entry.GetFormattedScore(_currentDefinition);
                    }

                    // Highlight current user
                    if (entry.IsCurrentUser)
                    {
                        var background = entryObj.GetComponent<Image>();
                        if (background != null) background.color = new Color(0.8f, 0.9f, 1f);
                    }
                }
            }
        }

        /// <summary>
        /// Handles leaderboard queried event
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard </param>
        /// <param name="entries"> Entries that were retrieved </param>
        private void OnLeaderboardQueried(string leaderboardId, List<LeaderboardEntry> entries)
        {
            LogHelper.Debug("LeaderboardsExample",
                $"Leaderboard queried event: {leaderboardId}, {entries.Count} entries");
        }

        /// <summary>
        /// Handles score submitted event
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard </param>
        /// <param name="score"> Score that was submitted </param>
        /// <param name="success"> Whether the submission was successful </param>
        private void OnScoreSubmitted(string leaderboardId, long score, bool success)
        {
            LogHelper.Debug("LeaderboardsExample",
                $"Score submitted event: {leaderboardId}, Score: {score}, Success: {success}");
        }

        /// <summary>
        /// Handles score submission failed event
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard </param>
        /// <param name="score"> Score that failed to submit </param>
        /// <param name="error"> Error message </param>
        private void OnScoreSubmissionFailed(string leaderboardId, long score, string error)
        {
            LogHelper.Warning("LeaderboardsExample",
                $"Score submission failed event: {leaderboardId}, Score: {score}, Error: {error}");
        }

        /// <summary>
        /// Sets the status text
        /// </summary>
        /// <param name="message"> Message to display </param>
        private void SetStatus(string message)
        {
            if (_statusText != null) _statusText.text = message;

            LogHelper.Info("LeaderboardsExample", message);
        }

        /// <summary>
        /// Updates the UI state based on initialization status
        /// </summary>
        private void UpdateUIState()
        {
            if (_initializeButton != null)
                _initializeButton.interactable = !_isInitialized;

            if (_queryLeaderboardButton != null)
                _queryLeaderboardButton.interactable = _isInitialized;

            if (_queryFriendsButton != null)
                _queryFriendsButton.interactable = _isInitialized;

            if (_getCurrentUserEntryButton != null)
                _getCurrentUserEntryButton.interactable = _isInitialized;

            if (_submitScoreButton != null)
                _submitScoreButton.interactable = _isInitialized;

            if (_friendsToggle != null)
                _friendsToggle.interactable = _isInitialized;

            if (_pageSizeDropdown != null)
                _pageSizeDropdown.interactable = _isInitialized;

            if (_leaderboardIdInput != null)
                _leaderboardIdInput.interactable = _isInitialized;

            if (_scoreInput != null)
                _scoreInput.interactable = _isInitialized;
        }
    }

    /// <summary>
    /// UI component for displaying a leaderboard entry
    /// </summary>
    [AddComponentMenu("RecipeRage/Examples/LeaderboardEntryUI")]
    public class LeaderboardEntryUI : MonoBehaviour
    {
        [SerializeField] private Text _rankText;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _scoreText;
        [SerializeField] private Text _extraInfoText;
        [SerializeField] private Image _background;
        [SerializeField] private Image _avatar;
        [SerializeField] private Color _currentUserColor = new Color(0.8f, 0.9f, 1f);
        [SerializeField] private Color _friendColor = new Color(0.9f, 1f, 0.8f);
        [SerializeField] private Color _defaultColor = Color.white;

        /// <summary>
        /// Sets the entry data and updates the UI
        /// </summary>
        /// <param name="entry"> Leaderboard entry to display </param>
        /// <param name="definition"> Optional leaderboard definition for formatting </param>
        public void SetEntry(LeaderboardEntry entry, LeaderboardDefinition definition = null)
        {
            if (entry == null)
                return;

            if (_rankText != null)
                _rankText.text = LeaderboardsFormatter.FormatRank(entry.Rank);

            if (_nameText != null)
                _nameText.text = entry.DisplayName;

            if (_scoreText != null)
                _scoreText.text = entry.GetFormattedScore(definition);

            if (_extraInfoText != null)
                _extraInfoText.text = LeaderboardsFormatter.FormatRelativeTime(entry.Timestamp);

            if (_avatar != null && entry.Avatar != null)
                _avatar.sprite = entry.Avatar;

            if (_background != null)
            {
                if (entry.IsCurrentUser)
                    _background.color = _currentUserColor;
                else if (entry.IsFriend)
                    _background.color = _friendColor;
                else
                    _background.color = _defaultColor;
            }
        }
    }
}