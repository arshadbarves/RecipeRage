using UnityEngine;

namespace Gameplay.Characters
{
    /// <summary>
    /// Handles input smoothing and processing.
    /// Single Responsibility: Input transformation and smoothing.
    /// </summary>
    public class PlayerInputHandler
    {
        private Vector2 _rawInput = Vector2.zero;
        private Vector2 _smoothedInput = Vector2.zero;
        private Vector2 _inputVelocity = Vector2.zero;
        
        private readonly bool _enableSmoothing;
        private readonly float _smoothTime;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public PlayerInputHandler(bool enableSmoothing, float smoothTime)
        {
            _enableSmoothing = enableSmoothing;
            _smoothTime = smoothTime;
        }
        
        /// <summary>
        /// Set raw input from input system.
        /// </summary>
        public void SetRawInput(Vector2 input)
        {
            _rawInput = input;
        }
        
        /// <summary>
        /// Update smoothed input (call in Update).
        /// </summary>
        public void UpdateSmoothing()
        {
            if (!_enableSmoothing)
            {
                _smoothedInput = _rawInput;
                return;
            }
            
            _smoothedInput = Vector2.SmoothDamp(
                _smoothedInput,
                _rawInput,
                ref _inputVelocity,
                _smoothTime
            );
        }
        
        /// <summary>
        /// Get smoothed input for movement.
        /// </summary>
        public Vector2 GetSmoothedInput()
        {
            return _smoothedInput;
        }
        
        /// <summary>
        /// Get raw input (unsmoothed).
        /// </summary>
        public Vector2 GetRawInput()
        {
            return _rawInput;
        }
    }
}
