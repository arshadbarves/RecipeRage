using System;
using Core.Input;
using Core.Utilities.Patterns;
using Unity.Netcode;
using UnityEngine;

namespace Core.Characters
{
    /// <summary>
    /// Controls player movement and actions in RecipeRage.
    /// </summary>
    public class PlayerController : NetworkBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _baseMovementSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 10f;

        [Header("Interaction Settings")]
        [SerializeField] private float _interactionRadius = 1.5f;
        [SerializeField] private LayerMask _interactionLayer;
        [SerializeField] private Transform _holdPoint;

        [Header("Character Settings")]
        [SerializeField] private int _characterClassId;

        /// <summary>
        /// The player's character manager.
        /// </summary>
        private CharacterManager _characterManager;

        /// <summary>
        /// The player's currently held object.
        /// </summary>
        private GameObject _heldObject;

        /// <summary>
        /// The player's input provider.
        /// </summary>
        private IInputProvider _inputProvider;

        /// <summary>
        /// The player's rigidbody.
        /// </summary>
        private Rigidbody _rigidbody;

        /// <summary>
        /// The player's current movement speed.
        /// </summary>
        public float MovementSpeed { get; set; }

        /// <summary>
        /// The player's current interaction speed modifier.
        /// </summary>
        public float InteractionSpeedModifier { get; set; } = 1f;

        /// <summary>
        /// The player's current carrying capacity.
        /// </summary>
        public int CarryingCapacity { get; set; } = 1;

        /// <summary>
        /// The player's character class.
        /// </summary>
        public CharacterClass CharacterClass { get; private set; }

        /// <summary>
        /// The player's primary ability.
        /// </summary>
        public CharacterAbility PrimaryAbility { get; private set; }

        /// <summary>
        /// Initialize the player controller.
        /// </summary>
        private void Awake()
        {
            // Get components
            _rigidbody = GetComponent<Rigidbody>();

            // Set default values
            MovementSpeed = _baseMovementSpeed;
        }

        /// <summary>
        /// Update the player controller.
        /// </summary>
        private void Update()
        {
            // Only update for the local player
            if (!IsLocalPlayer)
            {
                return;
            }

            // Update ability
            if (PrimaryAbility != null)
            {
                PrimaryAbility.Update(Time.deltaTime);
            }
        }

        /// <summary>
        /// Event triggered when the player interacts with an object.
        /// </summary>
        public event Action<IInteractable> OnInteraction;

        /// <summary>
        /// Event triggered when the player uses their ability.
        /// </summary>
        public event Action<CharacterAbility> OnAbilityUsed;

        /// <summary>
        /// Set up the player controller when the network object spawns.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Only set up input for the local player
            if (IsLocalPlayer)
            {
                // Get the input provider
                _inputProvider = ServiceLocator.Instance.Get<IInputProvider>();
                if (_inputProvider == null)
                {
                    Debug.LogError("[PlayerController] Input provider not found");
                    return;
                }

                // Subscribe to input events
                _inputProvider.OnMovementInput += HandleMove;
                _inputProvider.OnInteractionInput += HandleInteract;
                _inputProvider.OnSpecialAbilityInput += HandleAbility;

                Debug.Log("[PlayerController] Local player initialized");
            }

            // Get the character manager
            _characterManager = ServiceLocator.Instance.Get<CharacterManager>();
            if (_characterManager == null)
            {
                Debug.LogError("[PlayerController] Character manager not found");
                return;
            }

            // Set up character class
            SetupCharacterClass();
        }

        /// <summary>
        /// Clean up when the network object despawns.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            // Unsubscribe from input events
            if (IsLocalPlayer && _inputProvider != null)
            {
                _inputProvider.OnMovementInput -= HandleMove;
                _inputProvider.OnInteractionInput -= HandleInteract;
                _inputProvider.OnSpecialAbilityInput -= HandleAbility;
            }
        }

        /// <summary>
        /// Set up the character class.
        /// </summary>
        private void SetupCharacterClass()
        {
            if (_characterManager == null)
            {
                return;
            }

            // Get the character class
            CharacterClass = _characterManager.GetCharacterClass(_characterClassId);
            if (CharacterClass == null)
            {
                // Use the default character class
                CharacterClass = _characterManager.SelectedCharacterClass;
                _characterClassId = CharacterClass != null ? CharacterClass.Id : 0;
            }

            if (CharacterClass != null)
            {
                // Apply character class modifiers
                MovementSpeed = _baseMovementSpeed * CharacterClass.MovementSpeedModifier;
                InteractionSpeedModifier = CharacterClass.InteractionSpeedModifier;
                CarryingCapacity = Mathf.RoundToInt(CharacterClass.CarryingCapacityModifier);

                // Create ability
                PrimaryAbility = _characterManager.CreateAbilityForPlayer(CharacterClass, this);

                Debug.Log($"[PlayerController] Character class set: {CharacterClass.DisplayName} ({CharacterClass.Id})");
            }
            else
            {
                Debug.LogError("[PlayerController] No character class available");
            }
        }

        /// <summary>
        /// Set the character class.
        /// </summary>
        /// <param name="characterClassId"> The character class ID </param>
        public void SetCharacterClass(int characterClassId)
        {
            if (_characterManager == null)
            {
                return;
            }

            // Check if the character class is unlocked
            if (!_characterManager.IsCharacterClassUnlocked(characterClassId))
            {
                Debug.LogError($"[PlayerController] Cannot set character class: not unlocked: {characterClassId}");
                return;
            }

            // Set the character class ID
            _characterClassId = characterClassId;

            // Set up the character class
            SetupCharacterClass();

            // Sync to server
            if (IsLocalPlayer)
            {
                SetCharacterClassServerRpc(characterClassId);
            }
        }

        /// <summary>
        /// Set the character class on the server.
        /// </summary>
        /// <param name="characterClassId"> The character class ID </param>
        [ServerRpc]
        private void SetCharacterClassServerRpc(int characterClassId)
        {
            // Set the character class ID
            _characterClassId = characterClassId;

            // Notify clients
            SetCharacterClassClientRpc(characterClassId);
        }

        /// <summary>
        /// Set the character class on all clients.
        /// </summary>
        /// <param name="characterClassId"> The character class ID </param>
        [ClientRpc]
        private void SetCharacterClassClientRpc(int characterClassId)
        {
            // Skip the local player (already set)
            if (IsLocalPlayer)
            {
                return;
            }

            // Set the character class ID
            _characterClassId = characterClassId;

            // Set up the character class
            SetupCharacterClass();
        }

        /// <summary>
        /// Handle move input.
        /// </summary>
        /// <param name="moveInput"> The move input vector </param>
        private void HandleMove(Vector2 moveInput)
        {
            if (!IsLocalPlayer || _rigidbody == null)
            {
                return;
            }

            // Convert input to world space movement
            Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y) * MovementSpeed * Time.deltaTime;

            // Move the player
            transform.position += movement;

            // Rotate the player to face the movement direction
            if (movement != Vector3.zero)
            {
                var targetRotation = Quaternion.LookRotation(movement.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Handle interact input.
        /// </summary>
        private void HandleInteract()
        {
            if (!IsLocalPlayer)
            {
                return;
            }

            // Find interactable objects in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, _interactionRadius, _interactionLayer);

            // Find the closest interactable
            float closestDistance = float.MaxValue;
            IInteractable closestInteractable = null;

            foreach (Collider collider in colliders)
            {
                IInteractable interactable = collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestInteractable = interactable;
                    }
                }
            }

            // Interact with the closest interactable
            if (closestInteractable != null)
            {
                closestInteractable.Interact(this);
                OnInteraction?.Invoke(closestInteractable);

                // Sync to server
                InteractServerRpc();
            }
        }

        /// <summary>
        /// Handle ability input.
        /// </summary>
        private void HandleAbility()
        {
            if (!IsLocalPlayer || PrimaryAbility == null)
            {
                return;
            }

            // Activate the ability
            if (PrimaryAbility.Activate())
            {
                OnAbilityUsed?.Invoke(PrimaryAbility);

                // Sync to server
                UseAbilityServerRpc();
            }
        }

        /// <summary>
        /// Notify the server that the player interacted.
        /// </summary>
        [ServerRpc]
        private void InteractServerRpc()
        {
            // Notify clients
            InteractClientRpc();
        }

        /// <summary>
        /// Notify all clients that the player interacted.
        /// </summary>
        [ClientRpc]
        private void InteractClientRpc()
        {
            // Skip the local player (already handled)
            if (IsLocalPlayer)
            {
            }

            // Play interaction animation or effects
            // TODO: Implement interaction animation
        }

        /// <summary>
        /// Notify the server that the player used their ability.
        /// </summary>
        [ServerRpc]
        private void UseAbilityServerRpc()
        {
            // Notify clients
            UseAbilityClientRpc();
        }

        /// <summary>
        /// Notify all clients that the player used their ability.
        /// </summary>
        [ClientRpc]
        private void UseAbilityClientRpc()
        {
            // Skip the local player (already handled)
            if (IsLocalPlayer)
            {
            }

            // Play ability animation or effects
            // TODO: Implement ability animation
        }

        /// <summary>
        /// Pick up an object.
        /// </summary>
        /// <param name="obj"> The object to pick up </param>
        /// <returns> True if the object was picked up </returns>
        public bool PickUpObject(GameObject obj)
        {
            if (_heldObject != null)
            {
                Debug.Log("[PlayerController] Cannot pick up object: already holding an object");
                return false;
            }

            if (_holdPoint == null)
            {
                Debug.LogError("[PlayerController] Cannot pick up object: hold point not set");
                return false;
            }

            // Set the held object
            _heldObject = obj;

            // Parent the object to the hold point
            obj.transform.SetParent(_holdPoint);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;

            // Disable physics
            Rigidbody objRigidbody = obj.GetComponent<Rigidbody>();
            if (objRigidbody != null)
            {
                objRigidbody.isKinematic = true;
            }

            Debug.Log($"[PlayerController] Picked up object: {obj.name}");

            return true;
        }

        /// <summary>
        /// Drop the held object.
        /// </summary>
        /// <returns> The dropped object, or null if no object was held </returns>
        public GameObject DropObject()
        {
            if (_heldObject == null)
            {
                return null;
            }

            GameObject obj = _heldObject;
            _heldObject = null;

            // Unparent the object
            obj.transform.SetParent(null);

            // Enable physics
            Rigidbody objRigidbody = obj.GetComponent<Rigidbody>();
            if (objRigidbody != null)
            {
                objRigidbody.isKinematic = false;
            }

            Debug.Log($"[PlayerController] Dropped object: {obj.name}");

            return obj;
        }

        /// <summary>
        /// Get the held object.
        /// </summary>
        /// <returns> The held object, or null if no object is held </returns>
        public GameObject GetHeldObject()
        {
            return _heldObject;
        }

        /// <summary>
        /// Check if the player is holding an object.
        /// </summary>
        /// <returns> True if the player is holding an object </returns>
        public bool IsHoldingObject()
        {
            return _heldObject != null;
        }
    }
}