using UnityEngine;

namespace Core.Characters
{
    /// <summary>
    /// Handles Rigidbody-based physics movement for the player.
    /// Single Responsibility: Movement physics and rotation.
    /// </summary>
    public class PlayerMovementController
    {
        private readonly Rigidbody _rigidbody;
        private readonly Transform _transform;
        
        // Movement settings
        private readonly float _baseMovementSpeed;
        private readonly float _rotationSpeed;
        private readonly float _carryingSpeedMultiplier;
        
        /// <summary>
        /// Current movement speed (can be modified by character class).
        /// </summary>
        public float MovementSpeed { get; set; }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public PlayerMovementController(
            Rigidbody rigidbody,
            Transform transform,
            float baseMovementSpeed,
            float rotationSpeed,
            float carryingSpeedMultiplier)
        {
            _rigidbody = rigidbody;
            _transform = transform;
            _baseMovementSpeed = baseMovementSpeed;
            _rotationSpeed = rotationSpeed;
            _carryingSpeedMultiplier = carryingSpeedMultiplier;
            
            MovementSpeed = baseMovementSpeed;
            
            ConfigureRigidbody();
        }
        
        /// <summary>
        /// Configure Rigidbody for AAA-level physics.
        /// </summary>
        private void ConfigureRigidbody()
        {
            if (_rigidbody == null) return;
            
            _rigidbody.freezeRotation = true;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        
        /// <summary>
        /// Apply movement based on input and state.
        /// </summary>
        public void ApplyMovement(Vector2 input, PlayerMovementState state, float deltaTime)
        {
            if (_rigidbody == null) return;
            
            // Check if movement is allowed
            if (!CanMove(state))
            {
                StopMovement();
                return;
            }
            
            // No movement if input is too small
            if (input.sqrMagnitude < 0.01f)
            {
                StopMovement();
                return;
            }
            
            // Calculate target velocity
            float currentSpeed = GetSpeedForState(state);
            Vector3 targetVelocity = new Vector3(
                input.x * currentSpeed,
                _rigidbody.linearVelocity.y, // Preserve vertical velocity (gravity)
                input.y * currentSpeed
            );
            
            // Apply velocity
            _rigidbody.linearVelocity = targetVelocity;
            
            // Rotate to face movement direction
            RotateTowardsMovement(targetVelocity, deltaTime);
        }
        
        /// <summary>
        /// Stop movement smoothly.
        /// </summary>
        public void StopMovement()
        {
            if (_rigidbody == null) return;
            _rigidbody.linearVelocity = new Vector3(0f, _rigidbody.linearVelocity.y, 0f);
        }
        
        /// <summary>
        /// Rotate player to face movement direction.
        /// </summary>
        private void RotateTowardsMovement(Vector3 velocity, float deltaTime)
        {
            if (velocity.sqrMagnitude < 0.01f) return;
            
            Vector3 lookDirection = new Vector3(velocity.x, 0f, velocity.z);
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            
            _transform.rotation = Quaternion.Slerp(
                _transform.rotation,
                targetRotation,
                _rotationSpeed * deltaTime
            );
        }
        
        /// <summary>
        /// Check if movement is allowed in current state.
        /// </summary>
        private bool CanMove(PlayerMovementState state)
        {
            return state != PlayerMovementState.Interacting &&
                   state != PlayerMovementState.Stunned;
        }
        
        /// <summary>
        /// Get movement speed based on state.
        /// </summary>
        private float GetSpeedForState(PlayerMovementState state)
        {
            switch (state)
            {
                case PlayerMovementState.Carrying:
                    return MovementSpeed * _carryingSpeedMultiplier;
                    
                case PlayerMovementState.Interacting:
                case PlayerMovementState.Stunned:
                    return 0f;
                    
                default:
                    return MovementSpeed;
            }
        }
        
        /// <summary>
        /// Get current velocity.
        /// </summary>
        public Vector3 GetVelocity()
        {
            return _rigidbody != null ? _rigidbody.linearVelocity : Vector3.zero;
        }
        
        /// <summary>
        /// Get current horizontal speed.
        /// </summary>
        public float GetCurrentSpeed()
        {
            if (_rigidbody == null) return 0f;
            Vector3 horizontalVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);
            return horizontalVelocity.magnitude;
        }
    }
}
