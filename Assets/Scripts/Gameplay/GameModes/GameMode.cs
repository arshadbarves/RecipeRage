using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GameModes
{
    /// <summary>
    /// Scriptable object that defines a game mode in RecipeRage.
    /// </summary>
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
        
        [Tooltip("Whether players can join in progress")]
        [SerializeField] private bool _allowJoinInProgress = false;
        
        [Header("Map Settings")]
        [Tooltip("Available maps for this game mode")]
        [SerializeField] private List<string> _availableMaps = new List<string>();
        
        [Tooltip("Default map for this game mode")]
        [SerializeField] private string _defaultMap;
        
        [Header("Recipe Settings")]
        [Tooltip("Recipe difficulty multiplier")]
        [Range(0.5f, 2f)]
        [SerializeField] private float _recipeDifficultyMultiplier = 1f;
        
        [Tooltip("Order frequency in seconds")]
        [SerializeField] private float _orderFrequency = 30f;
        
        [Tooltip("Maximum active orders")]
        [SerializeField] private int _maxActiveOrders = 5;
        
        [Header("Reward Settings")]
        [Tooltip("Base coins reward")]
        [SerializeField] private int _baseCoinsReward = 50;
        
        [Tooltip("Win bonus coins")]
        [SerializeField] private int _winBonusCoins = 25;
        
        [Tooltip("Coins per completed order")]
        [SerializeField] private int _coinsPerOrder = 10;
        
        [Tooltip("Experience points reward")]
        [SerializeField] private int _experienceReward = 100;
        
        [Header("Special Features")]
        [Tooltip("Whether power-ups are enabled")]
        [SerializeField] private bool _powerUpsEnabled = true;
        
        [Tooltip("Whether special abilities are enabled")]
        [SerializeField] private bool _specialAbilitiesEnabled = true;
        
        [Tooltip("Whether character classes are restricted")]
        [SerializeField] private bool _restrictCharacterClasses = false;
        
        [Tooltip("Allowed character classes (empty means all allowed)")]
        [SerializeField] private List<int> _allowedCharacterClasses = new List<int>();
        
        [Tooltip("Custom game mode parameters")]
        [SerializeField] private string _customParameters;
        
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
        /// Whether players can join in progress.
        /// </summary>
        public bool AllowJoinInProgress => _allowJoinInProgress;
        
        /// <summary>
        /// Available maps for this game mode.
        /// </summary>
        public List<string> AvailableMaps => _availableMaps;
        
        /// <summary>
        /// Default map for this game mode.
        /// </summary>
        public string DefaultMap => _defaultMap;
        
        /// <summary>
        /// Recipe difficulty multiplier.
        /// </summary>
        public float RecipeDifficultyMultiplier => _recipeDifficultyMultiplier;
        
        /// <summary>
        /// Order frequency in seconds.
        /// </summary>
        public float OrderFrequency => _orderFrequency;
        
        /// <summary>
        /// Maximum active orders.
        /// </summary>
        public int MaxActiveOrders => _maxActiveOrders;
        
        /// <summary>
        /// Base coins reward.
        /// </summary>
        public int BaseCoinsReward => _baseCoinsReward;
        
        /// <summary>
        /// Win bonus coins.
        /// </summary>
        public int WinBonusCoins => _winBonusCoins;
        
        /// <summary>
        /// Coins per completed order.
        /// </summary>
        public int CoinsPerOrder => _coinsPerOrder;
        
        /// <summary>
        /// Experience points reward.
        /// </summary>
        public int ExperienceReward => _experienceReward;
        
        /// <summary>
        /// Whether power-ups are enabled.
        /// </summary>
        public bool PowerUpsEnabled => _powerUpsEnabled;
        
        /// <summary>
        /// Whether special abilities are enabled.
        /// </summary>
        public bool SpecialAbilitiesEnabled => _specialAbilitiesEnabled;
        
        /// <summary>
        /// Whether character classes are restricted.
        /// </summary>
        public bool RestrictCharacterClasses => _restrictCharacterClasses;
        
        /// <summary>
        /// Allowed character classes (empty means all allowed).
        /// </summary>
        public List<int> AllowedCharacterClasses => _allowedCharacterClasses;
        
        /// <summary>
        /// Custom game mode parameters.
        /// </summary>
        public string CustomParameters => _customParameters;
        
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
            
            // Ensure we have at least one map
            if (_availableMaps.Count == 0)
            {
                _availableMaps.Add("Kitchen");
            }
            
            // Ensure default map is in available maps
            if (string.IsNullOrEmpty(_defaultMap) || !_availableMaps.Contains(_defaultMap))
            {
                _defaultMap = _availableMaps[0];
            }
            
            // Ensure game time is positive
            _gameTime = Mathf.Max(60f, _gameTime);
            
            // Ensure target score is positive
            _targetScore = Mathf.Max(0, _targetScore);
            
            // Ensure order frequency is positive
            _orderFrequency = Mathf.Max(5f, _orderFrequency);
            
            // Ensure max active orders is positive
            _maxActiveOrders = Mathf.Max(1, _maxActiveOrders);
        }
    }
}
