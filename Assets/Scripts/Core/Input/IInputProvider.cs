using System;
using UnityEngine;

namespace Core.Input
{
    public interface IInputProvider
    {
        event Action<Vector2> OnMovementInput;

        event Action OnInteractionInput;

        event Action OnSpecialAbilityInput;

        event Action OnPauseInput;

        void Initialize();

        void Update();

        void Enable();

        void Disable();

        Vector2 GetMovementInput();

        bool IsInteractionActive();

        bool IsSpecialAbilityActive();
    }
}