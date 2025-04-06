using RecipeRage.Gameplay.Cooking;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Gameplay.Stations
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
        protected override void ProcessIngredient(IngredientItem ingredientItem)
        {
            if (!IsServer)
            {
                return;
            }
            
            // Play trash effects
            PlayTrashEffectsClientRpc();
            
            // Destroy the ingredient
            ingredientItem.NetworkObject.Despawn();
        }
        
        /// <summary>
        /// Check if the ingredient can be accepted.
        /// </summary>
        /// <param name="ingredientItem">The ingredient to check</param>
        /// <returns>True, as trash bins accept all ingredients</returns>
        public override bool CanAcceptIngredient(IngredientItem ingredientItem)
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
