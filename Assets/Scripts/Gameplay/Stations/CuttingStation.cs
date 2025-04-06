using RecipeRage.Gameplay.Cooking;
using UnityEngine;

namespace RecipeRage.Gameplay.Stations
{
    /// <summary>
    /// A station for cutting ingredients.
    /// </summary>
    public class CuttingStation : CookingStation
    {
        [Header("Cutting Station Settings")]
        [SerializeField] private GameObject _knifeObject;
        [SerializeField] private AudioClip _cuttingSound;
        [SerializeField] private ParticleSystem _cuttingParticles;
        
        /// <summary>
        /// Initialize the cutting station.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            
            // Set station name
            _stationName = "Cutting Board";
        }
        
        /// <summary>
        /// Check if the ingredient can be cut.
        /// </summary>
        /// <param name="ingredientItem">The ingredient to check</param>
        /// <returns>True if the ingredient can be cut</returns>
        protected override bool CanProcessIngredient(IngredientItem ingredientItem)
        {
            // Check if the ingredient requires cutting
            return ingredientItem != null && 
                   ingredientItem.Ingredient != null && 
                   ingredientItem.Ingredient.RequiresCutting && 
                   !ingredientItem.IsCut;
        }
        
        /// <summary>
        /// Cut the ingredient.
        /// </summary>
        /// <param name="ingredientItem">The ingredient to cut</param>
        /// <returns>True if the ingredient was cut</returns>
        protected override bool ProcessIngredient(IngredientItem ingredientItem)
        {
            if (!CanProcessIngredient(ingredientItem))
            {
                return false;
            }
            
            // Cut the ingredient
            ingredientItem.Cut();
            
            // Play cutting particles
            if (_cuttingParticles != null)
            {
                _cuttingParticles.Play();
            }
            
            return true;
        }
        
        /// <summary>
        /// Start processing an ingredient.
        /// </summary>
        /// <param name="ingredientItem">The ingredient to process</param>
        protected override void StartProcessing(IngredientItem ingredientItem)
        {
            base.StartProcessing(ingredientItem);
            
            // Show knife
            if (_knifeObject != null)
            {
                _knifeObject.SetActive(true);
            }
            
            // Play cutting sound
            if (_audioSource != null && _cuttingSound != null)
            {
                _audioSource.clip = _cuttingSound;
                _audioSource.loop = true;
                _audioSource.Play();
            }
        }
        
        /// <summary>
        /// Complete processing an ingredient.
        /// </summary>
        protected override void CompleteProcessing()
        {
            // Hide knife
            if (_knifeObject != null)
            {
                _knifeObject.SetActive(false);
            }
            
            // Stop cutting sound
            if (_audioSource != null)
            {
                _audioSource.Stop();
                _audioSource.loop = false;
            }
            
            base.CompleteProcessing();
        }
        
        /// <summary>
        /// Cancel processing an ingredient.
        /// </summary>
        protected override void CancelProcessing()
        {
            // Hide knife
            if (_knifeObject != null)
            {
                _knifeObject.SetActive(false);
            }
            
            // Stop cutting sound
            if (_audioSource != null)
            {
                _audioSource.Stop();
                _audioSource.loop = false;
            }
            
            base.CancelProcessing();
        }
    }
}
