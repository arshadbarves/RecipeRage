using System;
using Core.Bootstrap;
using UI.Core;
using UI.Data;
using UnityEngine;
using UnityEngine.UIElements;
using Core.Logging;

namespace UI.Screens
{
    /// <summary>
    /// Map selection screen - full screen for selecting maps
    /// </summary>
    [UIScreen(UIScreenType.MapSelection, UIScreenCategory.Screen, "Screens/MapSelectionTemplate")]
    public class MapSelectionScreen : BaseUIScreen
    {
        private MapDatabase _mapDatabase;
        private Action<MapInfo> _onMapSelected;

        // UI Elements
        private Button _backButton;
        private ScrollView _mapGrid;
        private Label _screenTitle;

        protected override void OnInitialize()
        {
            CacheUIElements();
            SetupButtons();
            LoadMapDatabase();

            GameLogger.Log("Initialized");
        }

        private void CacheUIElements()
        {
            _backButton = GetElement<Button>("back-button");
            _mapGrid = GetElement<ScrollView>("map-grid");
            _screenTitle = GetElement<Label>("screen-title");

            if (_screenTitle != null)
            {
                _screenTitle.text = "SELECT MAP";
            }
        }

        private void SetupButtons()
        {
            if (_backButton != null)
            {
                _backButton.clicked += OnBackClicked;
            }
        }

        private void LoadMapDatabase()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("UI/Data/Maps");
            if (jsonFile != null)
            {
                _mapDatabase = JsonUtility.FromJson<MapDatabase>(jsonFile.text);
                GameLogger.Log($"Loaded {_mapDatabase.maps.Count} maps");
            }
            else
            {
                GameLogger.LogError("Failed to load Maps.json from Resources/Data");
            }
        }

        protected override void OnShow()
        {
            PopulateMaps();
        }

        /// <summary>
        /// Show the map selection screen with callback
        /// </summary>
        public void ShowWithCallback(Action<MapInfo> onMapSelected)
        {
            _onMapSelected = onMapSelected;
            Show(true, true);
        }

        private void PopulateMaps()
        {
            if (_mapGrid == null || _mapDatabase == null) return;

            _mapGrid.Clear();

            var availableMaps = _mapDatabase.GetAvailableMaps();

            foreach (MapInfo map in availableMaps)
            {
                VisualElement mapCard = CreateMapCard(map);
                _mapGrid.Add(mapCard);
            }

            GameLogger.Log($"Populated {availableMaps.Count} maps");
        }

        private VisualElement CreateMapCard(MapInfo map)
        {
            bool isCurrentMap = map.id == _mapDatabase.currentMapId;

            VisualElement card = new VisualElement();
            card.AddToClassList("map-card");

            if (isCurrentMap)
            {
                card.AddToClassList("current-map");
            }

            // Thumbnail
            VisualElement thumbnail = new VisualElement();
            thumbnail.AddToClassList("map-thumbnail");
            // TODO: Load actual thumbnail image
            // thumbnail.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>(map.thumbnail));
            card.Add(thumbnail);

            // Current map indicator
            if (isCurrentMap)
            {
                Label currentLabel = new Label("CURRENT");
                currentLabel.AddToClassList("current-label");
                thumbnail.Add(currentLabel);
            }

            // Info container
            VisualElement infoContainer = new VisualElement();
            infoContainer.AddToClassList("map-info");

            // Name
            Label nameLabel = new Label(map.name);
            nameLabel.AddToClassList("map-name");
            infoContainer.Add(nameLabel);

            // Subtitle
            Label subtitleLabel = new Label(map.subtitle);
            subtitleLabel.AddToClassList("map-subtitle");
            infoContainer.Add(subtitleLabel);

            // Description
            Label descLabel = new Label(map.description);
            descLabel.AddToClassList("map-description");
            infoContainer.Add(descLabel);

            // Players
            Label playersLabel = new Label($"Max Players: {map.maxPlayers}");
            playersLabel.AddToClassList("map-players");
            infoContainer.Add(playersLabel);

            card.Add(infoContainer);

            // Select button
            Button selectButton = new Button(() => OnMapCardClicked(map));
            selectButton.text = isCurrentMap ? "SELECTED" : "SELECT";
            selectButton.AddToClassList("select-button");
            selectButton.SetEnabled(!isCurrentMap);
            card.Add(selectButton);

            return card;
        }

        private void OnMapCardClicked(MapInfo map)
        {
            GameLogger.Log($"Map selected: {map.name}");

            // Update current map in database
            _mapDatabase.currentMapId = map.id;

            // Save to PlayerPrefs
            PlayerPrefs.SetString("CurrentMap", map.name);
            PlayerPrefs.SetString("CurrentMapSubtitle", map.subtitle);
            PlayerPrefs.SetString("CurrentMapId", map.id);
            PlayerPrefs.Save();

            // Callback
            _onMapSelected?.Invoke(map);

            // Show toast
            var uiService = GameBootstrap.Services?.UIService;
            uiService?.ShowNotification($"Map changed to {map.name}", NotificationType.Success, 2f);

            // Go back
            OnBackClicked();
        }

        private void OnBackClicked()
        {
            var uiService = GameBootstrap.Services?.UIService;
            if (uiService != null)
            {
                bool wentBack = uiService.GoBack(true);

                if (!wentBack)
                {
                    // If no history, go to main menu
                    uiService.ShowScreen(UIScreenType.MainMenu, true, false);
                }
            }
        }

        protected override void OnDispose()
        {
            if (_backButton != null)
            {
                _backButton.clicked -= OnBackClicked;
            }
        }
    }
}
