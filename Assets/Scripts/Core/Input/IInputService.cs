using System;
using UnityEngine;

namespace Core.Core.Input
{
    public interface IInputService
    {
        Vector2 GetMovementInput();
        bool IsInteractionActive();
        bool IsSpecialAbilityActive();
        
        void Enable();
        void Disable();
        void Update(float deltaTime);
        
        event Action<Vector2> OnMovementInput;
        event Action OnInteractionInput;
        event Action OnSpecialAbilityInput;
        event Action OnPauseInput;
    }
}
