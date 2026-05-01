using System;
using UnityEngine;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// Handles player interactions and abilities.
    /// </summary>
    public class PlayerInteractionController
    {
        private readonly Transform _transform;
        private readonly float _interactionRadius;
        private readonly LayerMask _interactionLayer;
        
        public event Action<IInteractable> OnInteraction;
        public event Action<CharacterAbility> OnAbilityUsed;
        
        public PlayerInteractionController(
            Transform transform,
            float interactionRadius,
            LayerMask interactionLayer)
        {
            _transform = transform;
            _interactionRadius = interactionRadius;
            _interactionLayer = interactionLayer;
        }
        
        public bool TryInteract(PlayerStateController stateController, PlayerController playerController)
        {
            if (!CanInteract(stateController))
            {
                return false;
            }
            
            IInteractable interactable = FindClosestInteractable();
            
            if (interactable != null)
            {
                stateController.SetState(PlayerMovementState.Interacting);
                
                interactable.Interact(playerController);
                OnInteraction?.Invoke(interactable);
                
                stateController.SetState(PlayerMovementState.Idle);
                
                return true;
            }
            
            return false;
        }
        
        public bool TryUseAbility(
            CharacterAbility ability,
            PlayerStateController stateController)
        {
            if (ability == null) return false;
            
            if (stateController.CurrentState == PlayerMovementState.Stunned)
            {
                return false;
            }
            
            if (ability.Activate())
            {
                stateController.SetState(PlayerMovementState.UsingAbility);
                
                OnAbilityUsed?.Invoke(ability);
                
                stateController.SetState(PlayerMovementState.Idle);
                
                return true;
            }
            
            return false;
        }
        
        private bool CanInteract(PlayerStateController stateController)
        {
            return stateController.CurrentState != PlayerMovementState.Stunned &&
                   stateController.CurrentState != PlayerMovementState.Interacting;
        }
        
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
