namespace KitchenClash.Domain
{
    public sealed class SFXEvent
    {
        public SFXType Type { get; set; }

        public SFXEvent() { }

        public SFXEvent(SFXType type)
        {
            Type = type;
        }
    }
}
