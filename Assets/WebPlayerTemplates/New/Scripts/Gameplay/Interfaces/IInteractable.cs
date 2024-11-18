using Gameplay.Character.Controller;
using UnityEngine;

namespace Gameplay.Interfaces
{
    public interface IInteractable
    {
        bool CanInteract(BaseCharacter character);
        void OnInteractionStarted(BaseCharacter character);
        void OnInteractionContinue(BaseCharacter character);
        void OnInteractionEnded(BaseCharacter character);
        Transform GetInteractionPoint();
        float GetInteractionRange();
        string GetInteractionPrompt();
        ulong GetNetworkObjectId();
    }
}