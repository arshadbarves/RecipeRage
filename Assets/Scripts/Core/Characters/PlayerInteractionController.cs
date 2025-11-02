using System;
using UnityEngine;

namespace Core.Characters
{
    /// <summary>
    /// Handles player interactions and abilities.
    /// Single Responsibility: Interaction logic and ability activation.
    /// </summary>
    public class PlayerInteractionController
    {
        private readonly Transform _transform;
        private readonly float _interactionRadius;
        private readonly LayerMask _interactionLayer;
        
        /// <summary>
        /// Event triggered when player interacts.
        /// </summary>
        public event Action<IInteractable> OnInteraction;
        
        /// <summary>
        /// Event triggered when ability is used.
        /// </summary>
        public event Action<CharacterAbility> OnAbilityUsed;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public PlayerInteractionController(
            Transform transform,
            float interactionRadius,
            LayerMask interactionLayer)
        {
            _transform = transform;
            _interactionRadius = interactionRadius;
            _interactionLayer = interactionLayer;
        }
        
        /// <summary>
        /// Try to interact with nearby objects.
        /// </summary>
        public bool TryInteract(PlayerStateController stateController, PlayerController playerController)
        {
            // Can't interact while stunned or already interacting
            if (!CanInteract(stateController))
            {
                return false;
            }
            
            // Find closest interactable
            IInteractable interactable = FindClosestInteractable();
            
            if (interactable != null)
            {
                // Set interacting state
                stateController.SetState(PlayerMovementState.Interacting);
                
                // Interact
                interactable.Interact(playerController);
                OnInteraction?.Invoke(interactable);
                
                // Return to idle
                stateController.SetState(PlayerMovementState.Idle);
                
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Try to use ability.
        /// </summary>
        public bool TryUseAbility(
            CharacterAbility ability,
            PlayerStateController stateController)
        {
            if (ability == null) return false;
            
            // Can't use ability while stunned
            if (stateController.CurrentState == PlayerMovementState.Stunned)
            {
                return false;
            }
            
            // Try to activate
            if (ability.Activate())
            {
                // Set ability state
                stateController.SetState(PlayerMovementState.UsingAbility);
                
                OnAbilityUsed?.Invoke(ability);
                
                // Return to idle
                stateController.SetState(PlayerMovementState.Idle);
                
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if interaction is allowed.
        /// </summary>
        private bool CanInteract(PlayerStateController stateController)
        {
            return stateController.CurrentState != PlayerMovementState.Stunned &&
                   stateController.CurrentState != PlayerMovementState.Interacting;
        }
        
        /// <summary>
        /// Find closest interactable object.
        /// </summary>
        private IInteractable FindClosestInteractable()
        {
            Collider[] colliders = Physics.OverlapSphere(
                _transform.position,
                _interactionRadius,
                _interactionLayer
            );
            
            float closestDistance = float.MaxValue;
            IInteractable closestInteractable = null;
            
            foreach (Collider collider in colliders)
            {
                IInteractable interactable = collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    float distance = Vector3.Distance(_transform.position, collider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestInteractable = interactable;
                    }
                }
            }
            
            return closestInteractable;
        }
    }
}
