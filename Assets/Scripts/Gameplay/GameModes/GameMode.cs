using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GameModes
{
    [CreateAssetMenu(fileName = "NewGameMode", menuName = "RecipeRage/Game Mode")]
    public class GameMode : ScriptableObject
    {
        [Header("Basic Settings")]
        [Tooltip("Unique identifier for the game mode")]
        [SerializeField] private string _id;

        [Tooltip("Display name of the game mode")]
        [SerializeField] private string _displayName;

        [Tooltip("Description of the game mode")]
        [TextArea(3, 5)]
        [SerializeField] private string _description;

        [Tooltip("Icon for the game mode")]
        [SerializeField] private Sprite _icon;

        [Tooltip("Subtitle for the game mode (e.g. 'Classic Battle')")]
        [SerializeField] private string _subtitle;

        [Tooltip("Category for grouping in Map Selection")]
        [SerializeField] private GameModeCategory _category;

        [Header("Team Settings")]
        [Tooltip("Number of teams in this game mode")]
        [SerializeField] private int _teamCount = 2;

        [Tooltip("Maximum players per team")]
        [SerializeField] private int _playersPerTeam = 2;

        [Tooltip("Whether friendly fire is enabled")]
        [SerializeField] private bool _friendlyFire = false;

        [Header("Game Settings")]
        [Tooltip("Game time in seconds")]
        [SerializeField] private float _gameTime = 300f;

        [Tooltip("Target score to win (0 for time-based only)")]
        [SerializeField] private int _targetScore = 1000;

        [Tooltip("Whether the game has a time limit")]
        [SerializeField] private bool _hasTimeLimit = true;

        [Tooltip("Whether the game has a score limit")]
        [SerializeField] private bool _hasScoreLimit = true;

        [Header("Map Settings")]
        [Tooltip("Scene name for the map to load additively")]
        [SerializeField] private string _mapSceneName;

        [Header("Reward Settings")]
        [Tooltip("Experience points reward")]
        [SerializeField] private int _experienceReward = 100;

        [Tooltip("Win bonus coins")]
        [SerializeField] private int _winBonusCoins = 25;

        [Header("Special Features")]
        [Tooltip("Whether power-ups are enabled")]
        [SerializeField] private bool _powerUpsEnabled = true;

        [Tooltip("Whether special abilities are enabled")]
        [SerializeField] private bool _specialAbilitiesEnabled = true;

        [Tooltip("Custom game mode parameters")]
        [SerializeField] private string _customParameters;

        [Header("Unlock Settings")]
        [Tooltip("Whether unlocked by default")]
        [SerializeField] private bool _unlockedByDefault = false;

        [Tooltip("Trophy cost to unlock")]
        [SerializeField] private int _unlockTrophyCost = 0;

        [Tooltip("Player level required to unlock")]
        [SerializeField] private int _unlockLevel = 1;

        /// <summary>
        /// Unique identifier for the game mode.
        /// </summary>
        public string Id => _id;

        /// <summary>
        /// Display name of the game mode.
        /// </summary>
        public string DisplayName => _displayName;

        /// <summary>
        /// Description of the game mode.
        /// </summary>
        public string Description => _description;

        /// <summary>
        /// Icon for the game mode.
        /// </summary>
        public Sprite Icon => _icon;

        /// <summary>
        /// Subtitle for UI display.
        /// </summary>
        public string Subtitle => _subtitle;

        /// <summary>
        /// Category for UI grouping.
        /// </summary>
        public GameModeCategory Category => _category;

        /// <summary>
        /// Number of teams in this game mode.
        /// </summary>
        public int TeamCount => _teamCount;

        /// <summary>
        /// Maximum players per team.
        /// </summary>
        public int PlayersPerTeam => _playersPerTeam;

        /// <summary>
        /// Whether friendly fire is enabled.
        /// </summary>
        public bool FriendlyFire => _friendlyFire;

        /// <summary>
        /// Game time in seconds.
        /// </summary>
        public float GameTime => _gameTime;

        /// <summary>
        /// Target score to win (0 for time-based only).
        /// </summary>
        public int TargetScore => _targetScore;

        /// <summary>
        /// Whether the game has a time limit.
        /// </summary>
        public bool HasTimeLimit => _hasTimeLimit;

        /// <summary>
        /// Whether the game has a score limit.
        /// </summary>
        public bool HasScoreLimit => _hasScoreLimit;

        /// <summary>
        /// Scene name for the map to load additively.
        /// </summary>
        public string MapSceneName => _mapSceneName;

        /// <summary>
        /// Experience points reward.
        /// </summary>
        public int ExperienceReward => _experienceReward;

        /// <summary>
        /// Win bonus coins.
        /// </summary>
        public int WinBonusCoins => _winBonusCoins;

        /// <summary>
        /// Whether power-ups are enabled.
        /// </summary>
        public bool PowerUpsEnabled => _powerUpsEnabled;

        /// <summary>
        /// Whether special abilities are enabled.
        /// </summary>
        public bool SpecialAbilitiesEnabled => _specialAbilitiesEnabled;

        /// <summary>
        /// Custom game mode parameters.
        /// </summary>
        public string CustomParameters => _customParameters;

        /// <summary>
        /// Whether the game mode is unlocked by default.
        /// </summary>
        public bool UnlockedByDefault => _unlockedByDefault;

        /// <summary>
        /// Trophy cost required to unlock the game mode.
        /// </summary>
        public int UnlockTrophyCost => _unlockTrophyCost;

        /// <summary>
        /// Player level required to unlock the game mode.
        /// </summary>
        public int UnlockLevel => _unlockLevel;

        /// <summary>
        /// Get the total maximum players for this game mode.
        /// </summary>
        public int MaxPlayers => _teamCount * _playersPerTeam;

        /// <summary>
        /// Validate the game mode settings.
        /// </summary>
        private void OnValidate()
        {
            // Ensure we have at least one team
            _teamCount = Mathf.Max(1, _teamCount);

            // Ensure we have at least one player per team
            _playersPerTeam = Mathf.Max(1, _playersPerTeam);

            // Ensure game time is positive
            _gameTime = Mathf.Max(60f, _gameTime);

            // Ensure target score is positive
            _targetScore = Mathf.Max(0, _targetScore);
        }
    }
}
