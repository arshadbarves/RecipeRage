using System;
using UnityEngine;

namespace Core.Core.Input
{
    /// <summary>
    /// Input service - pure C# class, no MonoBehaviour
    /// </summary>
    public class InputService : IInputService
    {
        private readonly IInputProvider _provider;

        public event Action<Vector2> OnMovementInput;
        public event Action OnInteractionInput;
        public event Action OnSpecialAbilityInput;
        public event Action OnPauseInput;

        public InputService(IInputProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            
            // Forward events
            _provider.OnMovementInput += (input) => OnMovementInput?.Invoke(input);
            _provider.OnInteractionInput += () => OnInteractionInput?.Invoke();
            _provider.OnSpecialAbilityInput += () => OnSpecialAbilityInput?.Invoke();
            _provider.OnPauseInput += () => OnPauseInput?.Invoke();
            
            _provider.Enable();
        }

        public Vector2 GetMovementInput() => _provider.GetMovementInput();
        public bool IsInteractionActive() => _provider.IsInteractionActive();
        public bool IsSpecialAbilityActive() => _provider.IsSpecialAbilityActive();

        public void Enable() => _provider.Enable();
        public void Disable() => _provider.Disable();
        
        public void Update(float deltaTime)
        {
            _provider.Update();
        }
    }
}
