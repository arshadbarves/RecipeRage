using KitchenClash.Domain;
using Playcenter.Shell;

namespace KitchenClash.Application.Services
{
    public class CameraShakeService
    {
        private readonly IEventBus _eventBus;

        public CameraShakeService(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void Dispose()
        {
        }
    }
}
