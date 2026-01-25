using Core.Shared.Events;
using Gameplay.Shared.Events;

namespace Gameplay.Camera
{
    public class CameraShakeService
    {
        private readonly IEventBus _eventBus;
        private ICameraController _cameraController;

        public CameraShakeService(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus?.Subscribe<CameraShakeEvent>(OnCameraShake);
        }

        public void SetCameraController(ICameraController cameraController)
        {
            _cameraController = cameraController;
        }

        private void OnCameraShake(CameraShakeEvent evt)
        {
            _cameraController?.Shake(evt.Intensity, evt.Duration);
        }

        public void Dispose()
        {
            _eventBus?.Unsubscribe<CameraShakeEvent>(OnCameraShake);
        }
    }
}
