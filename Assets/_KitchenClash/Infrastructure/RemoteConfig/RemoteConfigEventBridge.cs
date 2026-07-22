using KitchenClash.Domain;
using KitchenClash.Domain.Events;
using Playcenter.Services;
using Playcenter.Shell;

namespace KitchenClash.Infrastructure.RemoteConfig
{
    /// <summary>Bridges the SDK RemoteConfigService C# events onto the game IEventBus.</summary>
    public sealed class RemoteConfigEventBridge
    {
        private readonly RemoteConfigService _service;
        private readonly IEventBus _eventBus;

        public RemoteConfigEventBridge(RemoteConfigService service, IEventBus eventBus)
        {
            _service = service;
            _eventBus = eventBus;
        }

        public void Attach()
        {
            if (_service == null || _eventBus == null)
            {
                return;
            }
            _service.OnConfigUpdated += HandleConfigUpdated;
            _service.OnHealthChanged += HandleHealthChanged;
        }

        public void Detach()
        {
            if (_service == null)
            {
                return;
            }
            _service.OnConfigUpdated -= HandleConfigUpdated;
            _service.OnHealthChanged -= HandleHealthChanged;
        }

        private void HandleConfigUpdated(IConfigModel config)
        {
            _eventBus?.Publish(new ConfigUpdatedEvent(config));
        }

        private void HandleHealthChanged(ConfigHealthStatus status)
        {
            _eventBus?.Publish(new ConfigHealthStatusChangedEvent { Status = status });
        }
    }
}
