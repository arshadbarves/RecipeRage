using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RecipeRage.Modules.Lobbies.Core;
using RecipeRage.Modules.Lobbies.Data;
using RecipeRage.Modules.Lobbies.Interfaces;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Modules.Lobbies.UI
{
    /// <summary>
    /// UI controller for matchmaking functionality
    /// </summary>
    public class MatchmakingUIController : MonoBehaviour
    {
        #region UI Elements

        [Header("Matchmaking Panel")]
        [SerializeField] private GameObject matchmakingPanel;
        [SerializeField] private TMP_Dropdown gameModeDropdown;
        [SerializeField] private TMP_Dropdown regionDropdown;
        [SerializeField] private TMP_InputField minPlayersInput;
        [SerializeField] private TMP_InputField maxPlayersInput;
        [SerializeField] private Toggle useSkillToggle;
        [SerializeField] private Toggle allowJoinInProgressToggle;
        [SerializeField] private Button startMatchmakingButton;
        
        [Header("Status Panel")]
        [SerializeField] private GameObject statusPanel;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI playersFoundText;
        [SerializeField] private TextMeshProUGUI estimatedTimeText;
        [SerializeField] private TextMeshProUGUI elapsedTimeText;
        [SerializeField] private TextMeshProUGUI regionText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Button cancelButton;

        [Header("Settings")]
        [SerializeField] private float updateInterval = 0.5f;

        #endregion

        #region Private Fields

        private IMatchmakingService _matchmakingService;
        private ILobbyService _lobbyService;
        private bool _isInitialized = false;
        private List<string> _availableGameModes = new List<string>();
        private List<string> _availableRegions = new List<string>();
        private Coroutine _updateCoroutine;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Initialize UI
            if (matchmakingPanel != null) matchmakingPanel.SetActive(false);
            if (statusPanel != null) statusPanel.SetActive(false);
        }

        private void Start()
        {
            // Get references to services
            if (LobbyModule.Instance != null)
            {
                _matchmakingService = LobbyModule.Instance.MatchmakingService;
                _lobbyService = LobbyModule.Instance.LobbyService;
                
                if (_matchmakingService != null && _lobbyService != null)
                {
                    InitializeUI();
                    SubscribeToEvents();
                    _isInitialized = true;
                }
                else
                {
                    LogHelper.Error("MatchmakingUIController", "Matchmaking or Lobby service is null. Make sure LobbyModule is initialized.");
                }
            }
            else
            {
                LogHelper.Error("MatchmakingUIController", "LobbyModule instance is null. Make sure it exists in the scene.");
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            
            if (_updateCoroutine != null)
            {
                StopCoroutine(_updateCoroutine);
                _updateCoroutine = null;
            }
        }

        #endregion

        #region Initialization

        private void InitializeUI()
        {
            // Setup game mode dropdown
            SetupGameModes();
            
            // Setup region dropdown
            SetupRegions();
            
            // Setup inputs with default values
            if (minPlayersInput != null) minPlayersInput.text = "2";
            if (maxPlayersInput != null) maxPlayersInput.text = "4";
            
            // Setup toggles
            if (useSkillToggle != null) useSkillToggle.isOn = true;
            if (allowJoinInProgressToggle != null) allowJoinInProgressToggle.isOn = true;
            
            // Setup buttons
            if (startMatchmakingButton != null)
            {
                startMatchmakingButton.onClick.AddListener(OnStartMatchmakingClicked);
            }
            
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancelMatchmakingClicked);
            }
            
            // Show matchmaking panel, hide status panel
            if (matchmakingPanel != null) matchmakingPanel.SetActive(true);
            if (statusPanel != null) statusPanel.SetActive(false);
        }

        private void SetupGameModes()
        {
            if (gameModeDropdown == null) return;
            
            // Clear existing options
            gameModeDropdown.ClearOptions();
            
            // Add available game modes
            _availableGameModes = new List<string>
            {
                "casual",
                "competitive",
                "ranked",
                "custom"
            };
            
            gameModeDropdown.AddOptions(_availableGameModes);
        }

        private void SetupRegions()
        {
            if (regionDropdown == null) return;
            
            // Clear existing options
            regionDropdown.ClearOptions();
            
            // Add available regions
            _availableRegions = new List<string>
            {
                "us-east",
                "us-west",
                "eu-central",
                "eu-west",
                "asia-east",
                "asia-south",
                "oceania"
            };
            
            regionDropdown.AddOptions(_availableRegions);
        }

        private void SubscribeToEvents()
        {
            if (_matchmakingService == null) return;
            
            _matchmakingService.OnMatchmakingStarted += HandleMatchmakingStarted;
            _matchmakingService.OnMatchmakingCanceled += HandleMatchmakingCanceled;
            _matchmakingService.OnMatchmakingComplete += HandleMatchmakingComplete;
            _matchmakingService.OnMatchmakingFailed += HandleMatchmakingFailed;
            _matchmakingService.OnMatchmakingStatusUpdated += HandleMatchmakingStatusUpdated;
        }

        private void UnsubscribeFromEvents()
        {
            if (_matchmakingService == null) return;
            
            _matchmakingService.OnMatchmakingStarted -= HandleMatchmakingStarted;
            _matchmakingService.OnMatchmakingCanceled -= HandleMatchmakingCanceled;
            _matchmakingService.OnMatchmakingComplete -= HandleMatchmakingComplete;
            _matchmakingService.OnMatchmakingFailed -= HandleMatchmakingFailed;
            _matchmakingService.OnMatchmakingStatusUpdated -= HandleMatchmakingStatusUpdated;
        }

        #endregion

        #region UI Button Handlers

        private void OnStartMatchmakingClicked()
        {
            if (!_isInitialized || _matchmakingService == null)
            {
                LogHelper.Error("MatchmakingUIController", "Cannot start matchmaking - not initialized or service is null");
                return;
            }
            
            // Create matchmaking options from UI inputs
            MatchmakingOptions options = new MatchmakingOptions();
            
            // Game mode
            if (gameModeDropdown != null)
            {
                options.GameMode = _availableGameModes[gameModeDropdown.value];
            }
            
            // Region preferences
            if (regionDropdown != null)
            {
                options.PreferredRegions.Add(_availableRegions[regionDropdown.value]);
            }
            
            // Player counts
            if (minPlayersInput != null && int.TryParse(minPlayersInput.text, out int minPlayers))
            {
                options.MinPlayers = Mathf.Clamp(minPlayers, 2, 32);
            }
            
            if (maxPlayersInput != null && int.TryParse(maxPlayersInput.text, out int maxPlayers))
            {
                options.MaxPlayers = Mathf.Clamp(maxPlayers, options.MinPlayers, 32);
            }
            
            // Toggles
            if (useSkillToggle != null)
            {
                options.UseSkillBasedMatching = useSkillToggle.isOn;
            }
            
            if (allowJoinInProgressToggle != null)
            {
                options.AllowJoinInProgress = allowJoinInProgressToggle.isOn;
            }
            
            // Set a default timeout
            options.TimeoutSeconds = 120;
            
            // Start matchmaking
            _matchmakingService.StartMatchmaking(options, success =>
            {
                if (!success)
                {
                    LogHelper.Error("MatchmakingUIController", $"Failed to start matchmaking: {_matchmakingService.LastError}");
                    ShowError($"Failed to start matchmaking: {_matchmakingService.LastError}");
                }
            });
        }

        private void OnCancelMatchmakingClicked()
        {
            if (!_isInitialized || _matchmakingService == null)
            {
                LogHelper.Error("MatchmakingUIController", "Cannot cancel matchmaking - not initialized or service is null");
                return;
            }
            
            _matchmakingService.CancelMatchmaking();
        }

        #endregion

        #region Event Handlers

        private void HandleMatchmakingStarted(MatchmakingOptions options)
        {
            // Update UI
            if (matchmakingPanel != null) matchmakingPanel.SetActive(false);
            if (statusPanel != null) statusPanel.SetActive(true);
            
            // Start update coroutine
            if (_updateCoroutine != null)
            {
                StopCoroutine(_updateCoroutine);
            }
            _updateCoroutine = StartCoroutine(UpdateStatusUI());
            
            LogHelper.Info("MatchmakingUIController", $"Matchmaking started with options: {options}");
        }

        private void HandleMatchmakingCanceled()
        {
            // Update UI
            if (matchmakingPanel != null) matchmakingPanel.SetActive(true);
            if (statusPanel != null) statusPanel.SetActive(false);
            
            // Stop update coroutine
            if (_updateCoroutine != null)
            {
                StopCoroutine(_updateCoroutine);
                _updateCoroutine = null;
            }
            
            LogHelper.Info("MatchmakingUIController", "Matchmaking canceled");
        }

        private void HandleMatchmakingComplete(LobbyInfo lobby)
        {
            // Update UI
            if (matchmakingPanel != null) matchmakingPanel.SetActive(false);
            if (statusPanel != null) statusPanel.SetActive(false);
            
            // Stop update coroutine
            if (_updateCoroutine != null)
            {
                StopCoroutine(_updateCoroutine);
                _updateCoroutine = null;
            }
            
            LogHelper.Info("MatchmakingUIController", $"Matchmaking complete. Joined lobby: {lobby.Name} ({lobby.LobbyId})");
            
            // Here you would transition to the lobby UI or game setup UI
        }

        private void HandleMatchmakingFailed(string errorMessage)
        {
            // Update UI
            if (matchmakingPanel != null) matchmakingPanel.SetActive(true);
            if (statusPanel != null) statusPanel.SetActive(false);
            
            // Stop update coroutine
            if (_updateCoroutine != null)
            {
                StopCoroutine(_updateCoroutine);
                _updateCoroutine = null;
            }
            
            LogHelper.Error("MatchmakingUIController", $"Matchmaking failed: {errorMessage}");
            ShowError($"Matchmaking failed: {errorMessage}");
        }

        private void HandleMatchmakingStatusUpdated(MatchmakingStatus status)
        {
            // Update is handled by the coroutine, but we'll handle state changes here
            switch (status.State)
            {
                case MatchmakingState.MatchFound:
                    if (statusText != null)
                    {
                        statusText.text = "Match Found! Joining...";
                    }
                    break;
                
                case MatchmakingState.TimedOut:
                    if (statusText != null)
                    {
                        statusText.text = "Matchmaking Timed Out";
                    }
                    break;
            }
        }

        #endregion

        #region UI Update

        private IEnumerator UpdateStatusUI()
        {
            while (true)
            {
                if (_matchmakingService != null && _matchmakingService.IsMatchmaking)
                {
                    MatchmakingStatus status = _matchmakingService.CurrentStatus;
                    
                    // Update status text
                    if (statusText != null)
                    {
                        statusText.text = GetStatusText(status.State);
                    }
                    
                    // Update players found text
                    if (playersFoundText != null)
                    {
                        playersFoundText.text = $"Players: {status.PlayersFound}/{status.PlayersNeeded}";
                    }
                    
                    // Update estimated time text
                    if (estimatedTimeText != null)
                    {
                        if (status.EstimatedTimeRemainingSeconds >= 0)
                        {
                            TimeSpan timeSpan = TimeSpan.FromSeconds(status.EstimatedTimeRemainingSeconds);
                            estimatedTimeText.text = $"Est. Time: {timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
                        }
                        else
                        {
                            estimatedTimeText.text = "Est. Time: --:--";
                        }
                    }
                    
                    // Update elapsed time text
                    if (elapsedTimeText != null)
                    {
                        TimeSpan elapsed = status.ElapsedTime;
                        elapsedTimeText.text = $"Elapsed: {elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                    }
                    
                    // Update region text
                    if (regionText != null)
                    {
                        regionText.text = $"Region: {status.CurrentRegion}";
                    }
                    
                    // Update progress slider
                    if (progressSlider != null)
                    {
                        progressSlider.value = status.ProgressPercentage / 100f;
                    }
                }
                
                yield return new WaitForSeconds(updateInterval);
            }
        }

        private string GetStatusText(MatchmakingState state)
        {
            switch (state)
            {
                case MatchmakingState.Initializing:
                    return "Initializing Matchmaking...";
                
                case MatchmakingState.Searching:
                    return "Searching for Players...";
                
                case MatchmakingState.MatchFound:
                    return "Match Found! Joining...";
                
                case MatchmakingState.Completed:
                    return "Matchmaking Complete";
                
                case MatchmakingState.Canceled:
                    return "Matchmaking Canceled";
                
                case MatchmakingState.TimedOut:
                    return "Matchmaking Timed Out";
                
                case MatchmakingState.Failed:
                    return "Matchmaking Failed";
                
                default:
                    return "Waiting...";
            }
        }

        private void ShowError(string message)
        {
            // In a real implementation, you would show a modal dialog
            Debug.LogError($"MatchmakingUI Error: {message}");
        }

        #endregion
    }
} 