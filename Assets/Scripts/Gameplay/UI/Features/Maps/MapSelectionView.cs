using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.GameModes;
using Core.Logging;
using Core.RemoteConfig.Interfaces;
using Core.RemoteConfig.Models;
using Core.UI;
using Core.UI.Core;
using Core.UI.Interfaces;
using Gameplay.UI.Data;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Gameplay.UI.Features.Maps
{
    /// <summary>
    /// Map selection view - shows all maps grouped by category
    /// </summary>
    [UIScreen(UIScreenCategory.Screen, "Screens/MapSelectionViewTemplate")]
    public class MapSelectionView : BaseUIScreen
    {
        [Inject]
        private IGameModeService _gameModeService;

        [Inject]
        private IRemoteConfigService _remoteConfigService;

        private Action<GameMode> _onGameModeSelected;

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
        }



        private void LoadTemplate()
        {
            _mapCardTemplate = Resources.Load<VisualTreeAsset>("UI/Templates/Components/MapCard");
            if (_mapCardTemplate == null)
            {
                GameLogger.LogError("Failed to load MapCard template!");
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



        protected override void OnShow()
        {
            PopulateMaps();
        }

        /// <summary>
        /// Show the map selection screen with callback
        /// </summary>
        public void ShowWithCallback(Action<GameMode> onGameModeSelected)
        {
            _onGameModeSelected = onGameModeSelected;
            Show(false, true);
        }

        private void PopulateMaps()
        {
            if (_categoriesScroll == null || _gameModeService == null) return;

            _categoriesScroll.Clear();

            var gameModes = _gameModeService.GetAvailableGameModes();
            if (gameModes == null || !Enumerable.Any(gameModes))
            {
                GameLogger.LogWarning("No available game modes found");
                return;
            }

            // Group by category
            var categorizedModes = new Dictionary<GameModeCategory, List<GameMode>>();
            foreach (var mode in gameModes)
            {
                if (!categorizedModes.ContainsKey(mode.Category))
                {
                    categorizedModes[mode.Category] = new List<GameMode>();
                }
                categorizedModes[mode.Category].Add(mode);
            }

            // Create a container for each category
            // Define sort order if needed, otherwise rely on Enum order or Keys
            foreach (var category in categorizedModes.Keys)
            {
                var modes = categorizedModes[category];
                if (modes == null || modes.Count == 0)
                    continue;

                VisualElement categoryContainer = CreateCategoryContainer(category, modes);
                _categoriesScroll.Add(categoryContainer);
            }
        }

        private VisualElement CreateCategoryContainer(GameModeCategory category, List<GameMode> modes)
        {
            // Main category container
            VisualElement container = new VisualElement();
            container.AddToClassList("category-container");

            // Add category-specific class for background color
            container.AddToClassList(GetCategoryClass(category));

            // Category header
            VisualElement header = new VisualElement();
            header.AddToClassList("category-header");

            // Category title (centered, no icon)
            Label title = new Label(category.ToString().ToUpper());
            title.AddToClassList("category-title");
            header.Add(title);

            container.Add(header);

            // Maps grid for this category
            VisualElement mapsGrid = new VisualElement();
            mapsGrid.AddToClassList("category-maps-grid");

            int modeCount = modes.Count;

            // Add maps to grid with spacing
            for (int i = 0; i < modes.Count; i++)
            {
                VisualElement mapCard = CreateMapCard(modes[i]);

                // Add spacing class if there are multiple cards and this is not the last one
                if (modeCount > 1 && i < modeCount - 1)
                {
                    mapCard.AddToClassList("has-spacing");
                }

                mapsGrid.Add(mapCard);
            }

            container.Add(mapsGrid);

            return container;
        }

        private string GetCategoryClass(GameModeCategory category)
        {
            return category switch
            {
                GameModeCategory.Special => "special-events",
                GameModeCategory.Trophies => "trophy-events",
                GameModeCategory.Ranked => "ranked",
                GameModeCategory.Community => "community",
                _ => "trophy-events"
            };
        }

        /// <summary>
        /// Create map card from template and populate with data
        /// </summary>
        /// <summary>
        /// Create map card from template and populate with data
        /// </summary>
        private VisualElement CreateMapCard(GameMode mode)
        {
            if (_mapCardTemplate == null)
            {
                GameLogger.LogError("MapCard template is null!");
                return new VisualElement();
            }

            bool isSelected = _gameModeService.SelectedGameMode == mode;

            // Clone template
            TemplateContainer cardContainer = _mapCardTemplate.CloneTree();
            Button card = cardContainer.Q<Button>("map-card");

            if (card == null)
            {
                GameLogger.LogError("Failed to find map-card button in template!");
                return cardContainer;
            }

            // Assign color based on map ID (Brawl Stars style)
            string colorClass = GetMapColorClass(mode.Id);
            card.AddToClassList(colorClass);

            // Setup click handler
            card.clicked += () => OnGameModeClicked(mode);

            // Mark as current map
            if (isSelected)
            {
                card.AddToClassList("current-map");
                card.SetEnabled(false);
            }

            // Populate data from template elements
            PopulateCardData(cardContainer, mode, isSelected);

            return cardContainer;
        }

        /// <summary>
        /// Populate card template with map data
        /// </summary>
        /// <summary>
        /// Populate card template with map data
        /// </summary>
        private void PopulateCardData(VisualElement cardContainer, GameMode mode, bool isSelected)
        {
            // Mode text
            Label modeText = cardContainer.Q<Label>("mode-text");
            if (modeText != null)
            {
                // Use Subtitle or GameMode type if available
                modeText.text = !string.IsNullOrEmpty(mode.Subtitle) ? mode.Subtitle : $"{mode.MaxPlayers} PLAYERS";
            }

            // Selected label visibility
            Label currentLabel = cardContainer.Q<Label>("current-label");
            if (currentLabel != null)
            {
                if (isSelected)
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
                mapName.text = mode.DisplayName.ToUpper();
            }

            // Map subtitle - reuse description or subtitle?
            Label mapSubtitle = cardContainer.Q<Label>("map-subtitle");
            if (mapSubtitle != null)
            {
                // Just use max players as subtitle or empty
                mapSubtitle.text = $"{mode.TeamCount} Teams";
            }

            // Map description
            Label mapDescription = cardContainer.Q<Label>("map-description");
            if (mapDescription != null)
            {
                if (!string.IsNullOrEmpty(mode.Description))
                {
                    mapDescription.text = mode.Description;
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
                mapPlayers.text = $"👥 {mode.MaxPlayers} PLAYERS";
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

        private void OnGameModeClicked(GameMode mode)
        {
            if (_gameModeService.SelectGameMode(mode))
            {
                // Callback
                _onGameModeSelected?.Invoke(mode);

                // Show toast
                UIService?.ShowNotification($"Mode changed to {mode.DisplayName}", NotificationType.Success, 2f);

                // Go back
                OnBackClicked();
            }
        }

        private void OnBackClicked()
        {
            UIService?.GoBack(false);
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
