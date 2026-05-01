using UnityEngine;

namespace KitchenClash.Infrastructure.Network
{
    public abstract class InteractableBase : MonoBehaviour, Application.Services.IInteractable
    {
        [SerializeField] protected string _interactionPrompt = "Interact";
        [SerializeField] protected bool _isInteractable = true;
        [SerializeField] protected float _interactionCooldown = 0.5f;
        protected float _interactionCooldownTimer = 0f;

        protected virtual void Update()
        {
            if (_interactionCooldownTimer > 0f) _interactionCooldownTimer -= Time.deltaTime;
        }

        public virtual void Interact(object player)
        {
            if (!CanInteract(player)) return;
            _interactionCooldownTimer = _interactionCooldown;
            OnInteract(player);
        }

        public virtual string GetInteractionPrompt() => _interactionPrompt;

        public virtual bool CanInteract(object player)
        {
            return _isInteractable && _interactionCooldownTimer <= 0f;
        }

        protected abstract void OnInteract(object player);

        public virtual void SetInteractable(bool isInteractable) => _isInteractable = isInteractable;
    }
}
