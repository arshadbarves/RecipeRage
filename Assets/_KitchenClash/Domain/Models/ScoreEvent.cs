namespace KitchenClash.Domain
{
    public sealed class ScoreEvent
    {
        public ScoreEventType Type { get; }
        public int RecipeTier { get; }
        public float SpeedRatio { get; }
        public bool RhythmBonus { get; }
        public int ComboCount { get; }

        public ScoreEvent(ScoreEventType type, int recipeTier = 1, float speedRatio = 0f,
            bool rhythmBonus = false, int comboCount = 0)
        {
            Type = type;
            RecipeTier = recipeTier;
            SpeedRatio = speedRatio;
            RhythmBonus = rhythmBonus;
            ComboCount = comboCount;
        }
    }
}
