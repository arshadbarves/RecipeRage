using UnityEngine;

namespace Core.GameModes
{
    /// <summary>
    /// Defines a map that can be loaded additively for gameplay.
    /// </summary>
    [CreateAssetMenu(fileName = "NewMap", menuName = "RecipeRage/Map Data")]
    public class MapData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Unique identifier for this map (must match scene name)")]
        [SerializeField] private string _id;

        [Tooltip("Display name shown in UI")]
        [SerializeField] private string _displayName;

        [Tooltip("Description of the map")]
        [TextArea(3, 5)]
        [SerializeField] private string _description;

        [Tooltip("Preview image for map selection")]
        [SerializeField] private Sprite _previewImage;

        [Header("Scene Settings")]
        [Tooltip("Scene name to load (must be in Build Settings)")]
        [SerializeField] private string _sceneName;

        [Header("Map Properties")]
        [Tooltip("Recommended player count")]
        [SerializeField] private int _recommendedPlayerCount = 4;

        [Tooltip("Map size category")]
        [SerializeField] private MapSize _mapSize = MapSize.Medium;

        [Tooltip("Map theme/style")]
        [SerializeField] private string _theme = "Kitchen";

        /// <summary>
        /// Unique identifier for this map.
        /// </summary>
        public string Id => _id;

        /// <summary>
        /// Display name shown in UI.
        /// </summary>
        public string DisplayName => _displayName;

        /// <summary>
        /// Description of the map.
        /// </summary>
        public string Description => _description;

        /// <summary>
        /// Preview image for map selection.
        /// </summary>
        public Sprite PreviewImage => _previewImage;

        /// <summary>
        /// Scene name to load.
        /// </summary>
        public string SceneName => _sceneName;

        /// <summary>
        /// Recommended player count.
        /// </summary>
        public int RecommendedPlayerCount => _recommendedPlayerCount;

        /// <summary>
        /// Map size category.
        /// </summary>
        public MapSize MapSize => _mapSize;

        /// <summary>
        /// Map theme/style.
        /// </summary>
        public string Theme => _theme;

        private void OnValidate()
        {
            // Ensure ID matches scene name for consistency
            if (!string.IsNullOrEmpty(_sceneName) && string.IsNullOrEmpty(_id))
            {
                _id = _sceneName;
            }
        }
    }

    public enum MapSize
    {
        Small,
        Medium,
        Large
    }
}
