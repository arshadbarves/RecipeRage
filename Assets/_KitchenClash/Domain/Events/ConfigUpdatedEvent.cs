using KitchenClash.Domain;

namespace KitchenClash.Domain.Events
{
    public class ConfigUpdatedEvent
    {
        public IConfigModel Config { get; }

        public ConfigUpdatedEvent(IConfigModel config)
        {
            Config = config;
        }
    }
}
