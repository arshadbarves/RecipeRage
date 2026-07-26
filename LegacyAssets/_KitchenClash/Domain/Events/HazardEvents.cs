namespace KitchenClash.Domain
{
    public sealed class FireStartedEvent
    {
        public string StationId { get; set; }
    }

    public sealed class FireExtinguishedEvent
    {
        public string StationId { get; set; }
    }

    public sealed class FirePenaltyEvent
    {
        public string StationId { get; set; }
    }
}
