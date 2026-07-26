using KitchenClash.Domain;
using Playcenter.Services;

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
