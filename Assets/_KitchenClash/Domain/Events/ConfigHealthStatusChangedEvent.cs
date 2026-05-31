namespace KitchenClash.Domain
{
    public sealed class ConfigHealthStatusChangedEvent
    {
        public ConfigHealthStatus Status { get; set; }
    }
}
