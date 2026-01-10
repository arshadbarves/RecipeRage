using System;
using Gameplay.UI.Data;
using Core.Logging;
using Core.RemoteConfig.Interfaces;
using Core.RemoteConfig.Models;
using Core.UI;
using Core.UI.Core;
using Core.UI.Interfaces;
using Gameplay.UI.Features.MainMenu;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Gameplay.UI.Features.Maps
{
    /// <summary>
    /// Map selection screen - shows all maps grouped by category
    /// </summary>
    [UIScreen(UIScreenCategory.Screen, "Screens/MapSelectionTemplate")]
    public class MapSelectionScreen : BaseUIScreen
    {
        [Inject]
        private IRemoteConfigService _remoteConfigService;

        [Inject]
        private IUIService _uiService;

        private MapDatabase _mapDatabase;
        private Action<MapInfo> _onMapSelected;

        // UI Elements
        private Button _backButton;
        private ScrollView _categoriesScroll;
        private Label _screenTitle;

        // Template
        private VisualTreeAsset _mapCardTemplate;

        protected override void OnInitialize()
        {
            LoadTemplate();
            CacheUIElements();
            SetupButtons();
            LoadMapDatabase();
            SubscribeToRemoteConfig();
        }

        private void SubscribeToRemoteConfig()
        {
            if (_remoteConfigService != null)
            {
                // Subscribe to MapConfig updates
                _remoteConfigService.OnSpecificConfigUpdated += OnMapConfigUpdated;
            }
        }

        private void OnMapConfigUpdated(Type configType, IConfigModel config)
        {
            // Check if this is a MapConfig update
            if (configType == typeof(MapConfig))
            {
                // Refresh the UI with new map data
                if (IsVisible)
                {
                    PopulateMaps();
                }
            }
        }

        private void LoadTemplate()
        {
            _mapCardTemplate = Resources.Load<VisualTreeAsset>("UI/Templates/Components/MapCard");
            if (_mapCardTemplate == null)
            {
                GameLogger.LogError("Failed to load MapCard template!");
            }
            else
            {
            }
        }

        private void CacheUIElements()
        {
            _backButton = GetElement<Button>("back-button");
            _categoriesScroll = GetElement<ScrollView>("categories-scroll");
            _screenTitle = GetElement<Label>("screen-title");

            if (_screenTitle != null)
            {
                _screenTitle.text = "CHOOSE EVENT";
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
                int totalMaps = 0;
                if (_mapDatabase.categories != null)
                {
                    foreach (var category in _mapDatabase.categories)
                    {
                        totalMaps += category.maps?.Count ?? 0;
                    }
                }
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
            Show(false, true);
        }

        private void PopulateMaps()
        {
            if (_categoriesScroll == null || _mapDatabase == null) return;

            _categoriesScroll.Clear();

            if (_mapDatabase.categories == null || _mapDatabase.categories.Count == 0)
            {
                GameLogger.LogWarning("No categories found in map database");
                return;
            }

            // Create a container for each category
            foreach (var category in _mapDatabase.categories)
            {
                if (category.maps == null || category.maps.Count == 0)
                    continue;

                VisualElement categoryContainer = CreateCategoryContainer(category);
                _categoriesScroll.Add(categoryContainer);
            }
        }

        private VisualElement CreateCategoryContainer(MapCategory category)
        {
            // Main category container
            VisualElement container = new VisualElement();
            container.AddToClassList("category-container");

            // Add category-specific class for background color
            container.AddToClassList(GetCategoryClass(category.id));

            // Category header
            VisualElement header = new VisualElement();
            header.AddToClassList("category-header");

            // Category title (centered, no icon)
            Label title = new Label(category.name.ToUpper());
            title.AddToClassList("category-title");
            header.Add(title);

            container.Add(header);

            // Maps grid for this category
            VisualElement mapsGrid = new VisualElement();
            mapsGrid.AddToClassList("category-maps-grid");

            // Get all maps (including coming soon ones)
            var allMaps = category.maps;
            int mapCount = allMaps.Count;

            // Add maps to grid with spacing
            for (int i = 0; i < allMaps.Count; i++)
            {
                VisualElement mapCard = CreateMapCard(allMaps[i]);

                // Add spacing class if there are multiple cards and this is not the last one
                if (mapCount > 1 && i < mapCount - 1)
                {
                    mapCard.AddToClassList("has-spacing");
                }

                mapsGrid.Add(mapCard);
            }

            container.Add(mapsGrid);

            return container;
        }

        private string GetCategoryClass(string categoryId)
        {
            return categoryId?.ToLower() switch
            {
                "special" => "special-events",
                "trophies" => "trophy-events",
                "ranked" => "ranked",
                "community" => "community",
                _ => "trophy-events"
            };
        }

        /// <summary>
        /// Create map card from template and populate with data
        /// </summary>
        private VisualElement CreateMapCard(MapInfo map)
        {
            if (_mapCardTemplate == null)
            {
                GameLogger.LogError("MapCard template is null!");
                return new VisualElement();
            }

            bool isCurrentMap = map.id == _mapDatabase.currentMapId;

            // Clone template
            TemplateContainer cardContainer = _mapCardTemplate.CloneTree();
            Button card = cardContainer.Q<Button>("map-card");

            if (card == null)
            {
                GameLogger.LogError("Failed to find map-card button in template!");
                return cardContainer;
            }

            // Assign color based on map ID (Brawl Stars style)
            string colorClass = GetMapColorClass(map.id);
            card.AddToClassList(colorClass);

            // Setup click handler
            card.clicked += () => OnMapCardClicked(map);

            // Mark as current map
            if (isCurrentMap)
            {
                card.AddToClassList("current-map");
                card.SetEnabled(false);
            }

            // Check if map is coming soon (not yet available)
            bool isComingSoon = !map.isAvailable;

            // Add coming soon styling
            if (isComingSoon)
            {
                card.AddToClassList("coming-soon");
                card.SetEnabled(false);

                // Add "COMING SOON" label overlay
                Label comingSoonLabel = new Label("COMING SOON");
                comingSoonLabel.AddToClassList("coming-soon-label");
                cardContainer.Q<VisualElement>("map-thumbnail")?.Add(comingSoonLabel);
            }

            // Populate data from template elements
            PopulateCardData(cardContainer, map, isCurrentMap, isComingSoon);

            return cardContainer;
        }

        /// <summary>
        /// Populate card template with map data
        /// </summary>
        private void PopulateCardData(VisualElement cardContainer, MapInfo map, bool isCurrentMap, bool isComingSoon = false)
        {
            // Mode text
            Label modeText = cardContainer.Q<Label>("mode-text");
            if (modeText != null)
            {
                modeText.text = !string.IsNullOrEmpty(map.gameMode) ? map.gameMode : $"{map.maxPlayers}v{map.maxPlayers}";
            }

            // Selected label visibility
            Label currentLabel = cardContainer.Q<Label>("current-label");
            if (currentLabel != null)
            {
                if (isCurrentMap)
                {
                    currentLabel.RemoveFromClassList("hidden");
                }
                else
                {
                    currentLabel.AddToClassList("hidden");
                }
            }

            // Map name
            Label mapName = cardContainer.Q<Label>("map-name");
            if (mapName != null)
            {
                mapName.text = map.name.ToUpper();
            }

            // Map subtitle
            Label mapSubtitle = cardContainer.Q<Label>("map-subtitle");
            if (mapSubtitle != null)
            {
                mapSubtitle.text = map.subtitle;
            }

            // Map description
            Label mapDescription = cardContainer.Q<Label>("map-description");
            if (mapDescription != null)
            {
                if (!string.IsNullOrEmpty(map.description))
                {
                    mapDescription.text = map.description;
                }
                else
                {
                    mapDescription.AddToClassList("hidden");
                }
            }

            // Player count
            Label mapPlayers = cardContainer.Q<Label>("map-players");
            if (mapPlayers != null)
            {
                mapPlayers.text = $"👥 {map.maxPlayers} PLAYERS";
            }
        }

        /// <summary>
        /// Get color class for map based on ID (Brawl Stars style)
        /// </summary>
        private string GetMapColorClass(string mapId)
        {
            // Assign colors based on map ID hash for consistency
            int hash = mapId.GetHashCode();
            string[] colors = { "color-orange", "color-green", "color-purple", "color-pink", "color-blue", "color-red" };
            return colors[Math.Abs(hash) % colors.Length];
        }

        private void OnMapCardClicked(MapInfo map)
        {

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
            _uiService?.ShowNotification($"Map changed to {map.name}", NotificationType.Success, 2f);

            // Go back
            OnBackClicked();
        }

        private void OnBackClicked()
        {
            if (_uiService != null)
            {
                bool wentBack = _uiService.GoBack(false);

                if (!wentBack)
                    _uiService.Show<MainMenuScreen>(true, false);
            }
        }

        protected override void OnDispose()
        {
            if (_backButton != null)
            {
                _backButton.clicked -= OnBackClicked;
            }

            // Unsubscribe from RemoteConfig
            if (_remoteConfigService != null)
            {
                _remoteConfigService.OnSpecificConfigUpdated -= OnMapConfigUpdated;
            }
        }
    }
}
