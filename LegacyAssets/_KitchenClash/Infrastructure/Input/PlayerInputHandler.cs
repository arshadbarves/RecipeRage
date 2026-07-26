using UnityEngine;

namespace KitchenClash.Infrastructure.Input
{
    public class PlayerInputHandler
    {
        private Vector2 _rawInput = Vector2.zero;
        private Vector2 _smoothedInput = Vector2.zero;
        private Vector2 _inputVelocity = Vector2.zero;

        private readonly bool _enableSmoothing;
        private readonly float _smoothTime;

        public PlayerInputHandler(bool enableSmoothing, float smoothTime)
        {
            _enableSmoothing = enableSmoothing;
            _smoothTime = smoothTime;
        }

        public void SetRawInput(Vector2 input) => _rawInput = input;

        public void UpdateSmoothing()
        {
            if (!_enableSmoothing)
            {
                _smoothedInput = _rawInput;
                return;
            }
            _smoothedInput = Vector2.SmoothDamp(_smoothedInput, _rawInput, ref _inputVelocity, _smoothTime);
        }

        public Vector2 GetSmoothedInput() => _smoothedInput;
        public Vector2 GetRawInput() => _rawInput;
    }
}
