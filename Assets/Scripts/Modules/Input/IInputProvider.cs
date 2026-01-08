using System;
using UnityEngine;

namespace Modules.Input
{
    /// <summary>
    /// Interface for input providers that handle different input methods.
    /// </summary>
    public interface IInputProvider
    {
        /// <summary>
        /// Event triggered when movement input changes.
        /// </summary>
        event Action<Vector2> OnMovementInput;
        
        /// <summary>
        /// Event triggered when interaction input is detected.
        /// </summary>
        event Action OnInteractionInput;
        
        /// <summary>
        /// Event triggered when special ability input is detected.
        /// </summary>
        event Action OnSpecialAbilityInput;
        
        /// <summary>
        /// Event triggered when pause input is detected.
        /// </summary>
        event Action OnPauseInput;
        
        /// <summary>
        /// Initialize the input provider.
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Update the input provider.
        /// </summary>
        void Update();
        
        /// <summary>
        /// Enable the input provider.
        /// </summary>
        void Enable();
        
        /// <summary>
        /// Disable the input provider.
        /// </summary>
        void Disable();
        
        /// <summary>
        /// Get the current movement input.
        /// </summary>
        /// <returns>Movement input vector</returns>
        Vector2 GetMovementInput();
        
        /// <summary>
        /// Check if interaction input is active.
        /// </summary>
        /// <returns>True if interaction input is active</returns>
        bool IsInteractionActive();
        
        /// <summary>
        /// Check if special ability input is active.
        /// </summary>
        /// <returns>True if special ability input is active</returns>
        bool IsSpecialAbilityActive();
    }
}
