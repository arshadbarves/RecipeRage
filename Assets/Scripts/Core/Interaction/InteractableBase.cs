using UnityEngine;

namespace RecipeRage.Core.Interaction
{
    /// <summary>
    /// Base class for objects that can be interacted with by the player.
    /// </summary>
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField] protected string _interactionPrompt = "Interact";
        [SerializeField] protected bool _isInteractable = true;
        [SerializeField] protected float _interactionCooldown = 0.5f;
        
        /// <summary>
        /// Timer for interaction cooldown.
        /// </summary>
        protected float _interactionCooldownTimer = 0f;
        
        /// <summary>
        /// Flag to track if the object is currently being interacted with.
        /// </summary>
        protected bool _isBeingInteractedWith = false;
        
        /// <summary>
        /// Update the interaction cooldown timer.
        /// </summary>
        protected virtual void Update()
        {
            // Update interaction cooldown timer
            if (_interactionCooldownTimer > 0f)
            {
                _interactionCooldownTimer -= Time.deltaTime;
            }
        }
        
        /// <summary>
        /// Called when the player interacts with this object.
        /// </summary>
        /// <param name="interactor">The GameObject that is interacting with this object</param>
        public virtual void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                return;
            }
            
            // Set interaction flags
            _isBeingInteractedWith = true;
            _interactionCooldownTimer = _interactionCooldown;
            
            // Perform interaction
            OnInteract(interactor);
            
            // Reset interaction flag
            _isBeingInteractedWith = false;
        }
        
        /// <summary>
        /// Get the interaction prompt text for this object.
        /// </summary>
        /// <returns>The interaction prompt text</returns>
        public virtual string GetInteractionPrompt()
        {
            return _interactionPrompt;
        }
        
        /// <summary>
        /// Check if this object can be interacted with.
        /// </summary>
        /// <param name="interactor">The GameObject that is trying to interact with this object</param>
        /// <returns>True if the object can be interacted with</returns>
        public virtual bool CanInteract(GameObject interactor)
        {
            return _isInteractable && !_isBeingInteractedWith && _interactionCooldownTimer <= 0f;
        }
        
        /// <summary>
        /// Called when the object is interacted with.
        /// Override this method to implement specific interaction behavior.
        /// </summary>
        /// <param name="interactor">The GameObject that is interacting with this object</param>
        protected abstract void OnInteract(GameObject interactor);
        
        /// <summary>
        /// Set whether this object is interactable.
        /// </summary>
        /// <param name="isInteractable">Whether the object is interactable</param>
        public virtual void SetInteractable(bool isInteractable)
        {
            _isInteractable = isInteractable;
        }
    }
}
