namespace KitchenClash.Domain
{
    public enum MapDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    public sealed class MapDefinition
    {
        public string MapId { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string SceneName { get; set; }
        public string KitchenTheme { get; set; }
        public string GameMode { get; set; }
        public MapDifficulty Difficulty { get; set; }
        public string[] AvailableRecipeIds { get; set; }
        public int StationCount { get; set; }
        public StationLayout[] Stations { get; set; }
        public MapHazardConfig Hazards { get; set; }
    }
}
