namespace KitchenClash.Domain
{
    public sealed class StationLayout
    {
        public string StationId { get; set; }
        public StationType Type { get; set; }
        public float GridX { get; set; }
        public float GridY { get; set; }
    }
}
