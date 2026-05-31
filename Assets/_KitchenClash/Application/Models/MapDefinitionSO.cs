using UnityEngine;
using KitchenClash.Domain;

namespace KitchenClash.Application.Models
{
    [CreateAssetMenu(fileName = "NewMap", menuName = "KitchenClash/Map Definition")]
    public class MapDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        public string MapId;
        public string DisplayName;
        [TextArea(2, 5)]
        public string Description;
        public string SceneName;

        [Header("Gameplay")]
        public string KitchenTheme;
        public string GameMode;
        public MapDifficulty Difficulty = MapDifficulty.Medium;
        public string[] AvailableRecipeIds;
        public int StationCount = 4;

        [Header("Stations")]
        public StationLayout[] Stations;

        [Header("Hazards")]
        public float FireChanceMultiplier = 1.0f;
        public bool HasSpecialHazards;
        public string SpecialHazardType;

        public MapDefinition ToDomain()
        {
            return new MapDefinition
            {
                MapId = MapId,
                DisplayName = DisplayName,
                Description = Description,
                SceneName = SceneName,
                KitchenTheme = KitchenTheme,
                GameMode = GameMode,
                Difficulty = Difficulty,
                AvailableRecipeIds = AvailableRecipeIds,
                StationCount = StationCount,
                Stations = Stations,
                Hazards = new MapHazardConfig
                {
                    FireChanceMultiplier = FireChanceMultiplier,
                    HasSpecialHazards = HasSpecialHazards,
                    SpecialHazardType = SpecialHazardType
                }
            };
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(DisplayName))
                DisplayName = name;
            if (string.IsNullOrEmpty(MapId))
                MapId = name.ToLower().Replace(" ", "_");
        }
    }
}
