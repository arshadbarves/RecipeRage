using Gameplay.Cooking;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Stations
{
    /// <summary>
    /// A station for disposing of unwanted ingredients.
    /// </summary>
    public class TrashBin : CookingStation
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

        /// <summary>
        /// Process the ingredient.
        /// </summary>
        /// <param name="ingredientItem">The ingredient to process</param>
        /// <returns>True if the ingredient was processed successfully</returns>
        protected override bool ProcessIngredient(IngredientItem ingredientItem)
        {
            if (!IsServer)
            {
                return false;
            }

            // Play trash effects
            PlayTrashEffectsClientRpc();

            // Destroy the ingredient
            ingredientItem.NetworkObject.Despawn();

            return true;
        }

        /// <summary>
        /// Check if the ingredient can be processed by this station.
        /// </summary>
        /// <param name="ingredientItem">The ingredient to check</param>
        /// <returns>True if the ingredient can be processed</returns>
        protected override bool CanProcessIngredient(IngredientItem ingredientItem)
        {
            // Trash bins accept all ingredients
            return ingredientItem != null;
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
