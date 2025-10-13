using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    /// <summary>
    /// Main Menu tab content - handles lobby/home view
    /// Pure C# class, no MonoBehaviour
    /// </summary>
    public class MainMenuUI
    {
        private VisualElement _root;
        
        // UI Elements
        private Button _playButton;
        private Button _mapButton;
        private Label _mapNameLabel;
        private Label _mapSubtitleLabel;
        private Label _timerLabel;
        
        public void Initialize(VisualElement root)
        {
            Debug.Log("[MainMenuUI] Initialize called");
            
            if (root == null)
            {
                Debug.LogError("[MainMenuUI] Root is null!");
                return;
            }
            
            _root = root;
            Debug.Log($"[MainMenuUI] Root element: {_root.name}");
            
            // Cache UI elements
            _playButton = _root.Q<Button>("play-button");
            _mapButton = _root.Q<Button>("map-button");
            _mapNameLabel = _root.Q<Label>("map-name");
            _mapSubtitleLabel = _root.Q<Label>("map-subtitle");
            _timerLabel = _root.Q<Label>("timer-text");
            
            Debug.Log($"[MainMenuUI] Elements found - Play: {_playButton != null}, Map: {_mapButton != null}");
            
            SetupButtons();
            LoadMapInfo();
        }
        
        private void SetupButtons()
        {
            if (_playButton != null)
            {
                _playButton.clicked += OnPlayClicked;
                Debug.Log("[MainMenuUI] Play button listener added");
            }
            
            if (_mapButton != null)
            {
                _mapButton.clicked += OnMapClicked;
                Debug.Log("[MainMenuUI] Map button listener added");
            }
        }
        
        private void LoadMapInfo()
        {
            // Load current map info from PlayerPrefs or game state
            string mapName = PlayerPrefs.GetString("CurrentMap", "CRUMB HAVEN");
            string mapSubtitle = PlayerPrefs.GetString("CurrentMapSubtitle", "COOPERATIVE CHAOS");
            
            if (_mapNameLabel != null)
            {
                _mapNameLabel.text = mapName;
            }
            
            if (_mapSubtitleLabel != null)
            {
                _mapSubtitleLabel.text = mapSubtitle;
            }
            
            UpdateMapTimer();
        }
        
        private void UpdateMapTimer()
        {
            // Calculate time until next map rotation
            // This is placeholder logic - implement your actual timer system
            if (_timerLabel != null)
            {
                _timerLabel.text = "NEW MAP IN : 30h 12m";
            }
        }
        
        private void OnPlayClicked()
        {
            Debug.Log("[MainMenuUI] Play button clicked - Starting matchmaking");
            
            // Transition to matchmaking/lobby
            Core.State.GameStateManager gameStateManager = Core.State.GameStateManager.Instance;
            if (gameStateManager != null)
            {
                gameStateManager.ChangeState(new Core.State.States.LobbyState());
            }
        }
        
        private void OnMapClicked()
        {
            Debug.Log("[MainMenuUI] Map button clicked - Opening map selection");
            
            // Show map selection UI
            // This could open a popup or navigate to a map selection screen
        }
    }
}
