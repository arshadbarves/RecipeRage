using UnityEngine;
using System;
using System.Linq;
using RecipeRage.Core.Player;
using RecipeRage.Core.Interaction;
using RecipeRage.Gameplay.Cooking;
using Unity.Netcode;

namespace RecipeRage.Gameplay.Interactables
{
    /// <summary>
    /// A station where players can serve completed dishes
    /// </summary>
    public class ServingStation : BaseStation, IInteractable
    {
        #region Properties
        public bool CanInteract => !_isBeingUsed.Value;
        public InteractionType InteractionType => InteractionType.Serve;
        public InteractionState CurrentState { get; private set; } = InteractionState.Idle;
        #endregion

        #region Events
        public event Action<RecipeData, float> OnRecipeServed;
        #endregion

        #region Serialized Fields
        [Header("Serving Settings")]
        [SerializeField] private float _scoreMultiplier = 1f;

        [Header("Audio")]
        [SerializeField] private AudioClip _serveSuccessSound;
        [SerializeField] private AudioClip _serveFailSound;
        #endregion

        #region Private Fields
        private NetworkVariable<bool> _isBeingUsed = new NetworkVariable<bool>();
        #endregion

        #region Unity Lifecycle
        protected override void Awake()
        {
            base.Awake();
        }

        public override void OnNetworkSpawn()
        {
            _isBeingUsed.OnValueChanged += OnBeingUsedChanged;
            UpdateVisuals();
        }

        public override void OnNetworkDespawn()
        {
            _isBeingUsed.OnValueChanged -= OnBeingUsedChanged;
        }
        #endregion

        #region IInteractable Implementation
        public bool StartInteraction(PlayerController player, Action onComplete)
        {
            if (!CanInteract || !IsServer)
                return false;

            if (player.HeldItem == null)
                return false;

            var plate = player.HeldItem.GetComponent<Plate>();
            if (plate == null || plate.IngredientCount == 0)
            {
                PlayFailEffect();
                return false;
            }

            ServeItemServerRpc(player.NetworkObjectId);
            return true;
        }

        public void CancelInteraction(PlayerController player)
        {
            // Serving interactions are instant, no need to cancel
        }

        public bool ContinueInteraction(PlayerController player)
        {
            // Serving interactions are instant, no need to continue
            return false;
        }
        #endregion

        #region Server RPCs
        [ServerRpc(RequireOwnership = false)]
        private void ServeItemServerRpc(ulong playerId)
        {
            if (_isBeingUsed.Value)
                return;

            _isBeingUsed.Value = true;

            var playerObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerId];
            var player = playerObject.GetComponent<PlayerController>();
            if (player != null && player.HeldItem != null)
            {
                var plate = player.HeldItem.GetComponent<Plate>();
                if (ValidateRecipe(plate, out RecipeData recipe, out float score))
                {
                    // Destroy the plate and its contents
                    var item = player.HeldItem;
                    player.HeldItem = null;
                    
                    if (item.TryGetComponent<NetworkObject>(out var networkObj))
                    {
                        networkObj.Despawn();
                    }
                    Destroy(item);

                    // Notify success
                    ServeSuccessClientRpc(recipe.recipeId, score);
                    OnRecipeServed?.Invoke(recipe, score);
                }
                else
                {
                    // Return the plate to the player
                    ServeFailClientRpc();
                }
            }

            _isBeingUsed.Value = false;
        }
        #endregion

        #region Client RPCs
        [ClientRpc]
        private void ServeSuccessClientRpc(string recipeId, float score)
        {
            PlaySuccessEffect();
        }

        [ClientRpc]
        private void ServeFailClientRpc()
        {
            PlayFailEffect();
        }
        #endregion

        #region Private Methods
        private bool ValidateRecipe(Plate plate, out RecipeData recipe, out float score)
        {
            recipe = null;
            score = 0f;

            if (plate == null)
                return false;

            // TODO: Implement recipe validation logic
            // This should check the ingredients on the plate against available recipes
            // and calculate a score based on correctness and timing
            return false;
        }

        private void OnBeingUsedChanged(bool previousValue, bool newValue)
        {
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            SetHighlight(CanInteract);
        }

        private void PlaySuccessEffect()
        {
            PlayParticles();
            PlaySound(_serveSuccessSound);
            SetStationMaterial(StationMaterialState.Success);
        }

        private void PlayFailEffect()
        {
            PlayParticles();
            PlaySound(_serveFailSound);
            SetStationMaterial(StationMaterialState.Error);
        }
        #endregion
    }
} 