namespace KitchenClash.Domain
{
    public sealed class MapHazardConfig
    {
        public float FireChanceMultiplier { get; set; } = 1.0f;
        public bool HasSpecialHazards { get; set; }
        public string SpecialHazardType { get; set; }
    }
}
