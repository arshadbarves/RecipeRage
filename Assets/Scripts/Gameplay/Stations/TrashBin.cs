using Core.Characters;
using Modules.Logging;
using Gameplay.Cooking;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Stations
{
    /// <summary>
    /// A station for disposing of unwanted ingredients.
    /// </summary>
    public class TrashBin : StationBase
    {
        [Header("Trash Bin Settings")]
        [SerializeField] private ParticleSystem _trashParticles;
        [SerializeField] private AudioClip _trashSound;

        /// <summary>
        /// Initialize the trash bin.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Set station name
            _stationName = "Trash Bin";
        }

        protected override void HandleInteraction(PlayerController player)
        {
            // If the player is holding an ingredient
            if (player.IsHoldingObject())
            {
                GameObject heldObject = player.GetHeldObject();
                IngredientItem ingredientItem = heldObject.GetComponent<IngredientItem>();

                if (ingredientItem != null)
                {
                    // Take the ingredient (to destroy it)
                    player.DropObject();

                    // Play trash effects
                    PlayTrashEffectsClientRpc();

                    // Destroy the ingredient
                    ingredientItem.NetworkObject.Despawn();
                }
            }
        }

        /// <summary>
        /// Play trash effects on all clients.
        /// </summary>
        [ClientRpc]
        private void PlayTrashEffectsClientRpc()
        {
            // Play particle effects
            if (_trashParticles != null)
            {
                _trashParticles.Play();
            }

            // Play sound
            if (_audioSource != null && _trashSound != null)
            {
                _audioSource.clip = _trashSound;
                _audioSource.Play();
            }
        }
    }
}
