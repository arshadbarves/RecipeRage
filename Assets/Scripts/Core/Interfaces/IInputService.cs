using System;
using UnityEngine;

namespace Core.Interfaces
{
    public interface IInputService : IGameService
    {
        Vector2 GetMovementInput();
        event Action OnPrimaryInteractAction;
        event Action OnSecondaryInteractAction;
        event Action OnPrimaryAttackAction;
        event Action OnSecondaryAttackAction;
        Task InitializeAsync();
    }
}