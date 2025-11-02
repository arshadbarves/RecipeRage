using UnityEngine;
using Core.Logging;

namespace Core.Input
{
    /// <summary>
    /// Debug helper to visualize input values in the console.
    /// Attach to any GameObject to see input in real-time.
    /// </summary>
    public class InputDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool _logMovementInput = true;
        [SerializeField] private bool _logInteractionInput = true;
        [SerializeField] private bool _logAbilityInput = true;
        [SerializeField] private float _logInterval = 0.5f; // Log every 0.5 seconds

        private IInputProvider _inputProvider;
        private float _lastLogTime;

        private void Start()
        {
            // Create input provider
            _inputProvider = InputProviderFactory.CreateForPlatform();
            
            if (_inputProvider == null)
            {
                GameLogger.LogError("Failed to create input provider!");
                enabled = false;
                return;
            }

            // Subscribe to events
            if (_logMovementInput)
            {
                _inputProvider.OnMovementInput += OnMovementInput;
            }

            if (_logInteractionInput)
            {
                _inputProvider.OnInteractionInput += OnInteractionInput;
            }

            if (_logAbilityInput)
            {
                _inputProvider.OnSpecialAbilityInput += OnAbilityInput;
            }

            GameLogger.Log("Started - watching for input events");
        }

        private void Update()
        {
            if (_inputProvider == null) return;

            // Update the provider
            _inputProvider.Update();

            // Log current state periodically
            if (Time.time - _lastLogTime >= _logInterval)
            {
                _lastLogTime = Time.time;

                Vector2 movement = _inputProvider.GetMovementInput();
                if (movement.sqrMagnitude > 0.01f)
                {
                    GameLogger.Log($"Current Movement: {movement} (magnitude: {movement.magnitude:F2})");
                }
            }
        }

        private void OnMovementInput(Vector2 input)
        {
            GameLogger.Log($"Movement Event: {input} (magnitude: {input.magnitude:F2})");
        }

        private void OnInteractionInput()
        {
            GameLogger.Log("Interaction Event!");
        }

        private void OnAbilityInput()
        {
            GameLogger.Log("Ability Event!");
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (_inputProvider != null)
            {
                _inputProvider.OnMovementInput -= OnMovementInput;
                _inputProvider.OnInteractionInput -= OnInteractionInput;
                _inputProvider.OnSpecialAbilityInput -= OnAbilityInput;
            }
        }
    }
}
