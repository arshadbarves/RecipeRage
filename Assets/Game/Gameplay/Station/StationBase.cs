using UnityEngine;

namespace RecipeRage
{
    public abstract class StationBase : MonoBehaviour, IInteractable
    {
        [SerializeField] protected string _stationName = "Station";

        public virtual bool CanInteract(PlayerController player) => true;

        public abstract void Interact(PlayerController player);

        public virtual string GetPrompt() => $"Use {_stationName}";
    }
}
