using KitchenClash.Domain.Interfaces;
using KitchenClash.Application.Services;
using Unity.Netcode;

namespace KitchenClash.Infrastructure.Network
{
    public class MatchContext : IMatchContext, IMatchRuntimeRegistry, System.IDisposable
    {
        private readonly IEventBus _eventBus;

        public bool IsHost => false;
        public bool IsServer => false;
        public bool IsClient => false;

        public MatchContext(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void Refresh() { }
        public void ShutdownNetworkSession() { }
        public void ClearSceneRuntime() { }
        public void Dispose() { }
    }
}
