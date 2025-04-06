using RecipeRage.Gameplay.Cooking;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Gameplay.Stations
{
    /// <summary>
    /// A station for cooking ingredients in a pot.
    /// </summary>
    public class CookingPot : CookingStation
    {
        [Header("Cooking Pot Settings")]
        [SerializeField] private float _burningTime = 10f;
        [SerializeField] private GameObject _steamEffect;
        [SerializeField] private GameObject _fireEffect;
        [SerializeField] private AudioClip _boilingSound;
        [SerializeField] private AudioClip _burningSound;

        /// <summary>
        /// Whether the ingredient is burning.
        /// </summary>
        private bool _isBurning;

        /// <summary>
        /// The burning timer.
        /// </summary>
        private float _burningTimer;

        /// <summary>
        /// Initialize the cooking pot.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Set station name
            _stationName = "Cooking Pot";

            // Hide effects
            if (_steamEffect != null)
            {
                _steamEffect.SetActive(false);
            }

            if (_fireEffect != null)
            {
                _fireEffect.SetActive(false);
            }
        }

        /// <summary>
        /// Update the cooking pot.
        /// </summary>
        protected override void Update()
        {
            base.Update();

            // Only update burning on the server
            if (!IsServer || !_isProcessing || _currentIngredient == null)
            {
                return;
            }

            // Check if the ingredient is cooked
            if (_processingProgress >= 1f)
            {
                // Start burning timer
                _burningTimer += Time.deltaTime;

                // Check if the ingredient is burning
                if (_burningTimer >= _burningTime && !_isBurning)
                {
                    _isBurning = true;

                    // Show fire effect
                    if (_fireEffect != null)
                    {
                        ShowFireEffectClientRpc();
                    }

                    // Play burning sound
                    if (_audioSource != null && _burningSound != null)
                    {
                        PlayBurningSoundClientRpc();
                    }

                    // Burn the ingredient
                    if (_currentIngredient != null)
                    {
                        _currentIngredient.Burn();
                    }
                }
            }
        }

        /// <summary>
        /// Check if the ingredient can be cooked.
        /// </summary>
        /// <param name="ingredientItem">The ingredient to check</param>
        /// <returns>True if the ingredient can be cooked</returns>
        protected override bool CanProcessIngredient(IngredientItem ingredientItem)
        {
            // Check if the ingredient requires cooking
            return ingredientItem != null &&
                   ingredientItem.Ingredient != null &&
                   ingredientItem.Ingredient.RequiresCooking &&
                   !ingredientItem.IsCooked;
        }

        /// <summary>
        /// Cook the ingredient.
        /// </summary>
        /// <param name="ingredientItem">The ingredient to cook</param>
        /// <returns>True if the ingredient was cooked</returns>
        protected override bool ProcessIngredient(IngredientItem ingredientItem)
        {
            if (!CanProcessIngredient(ingredientItem))
            {
                return false;
            }

            // Cook the ingredient
            ingredientItem.Cook();

            return true;
        }

        /// <summary>
        /// Start processing an ingredient.
        /// </summary>
        /// <param name="ingredientItem">The ingredient to process</param>
        protected override void StartProcessing(IngredientItem ingredientItem)
        {
            base.StartProcessing(ingredientItem);

            // Reset burning state
            _isBurning = false;
            _burningTimer = 0f;

            // Show steam effect
            if (_steamEffect != null)
            {
                ShowSteamEffectClientRpc();
            }

            // Play boiling sound
            if (_audioSource != null && _boilingSound != null)
            {
                PlayBoilingSoundClientRpc();
            }
        }

        /// <summary>
        /// Complete processing an ingredient.
        /// </summary>
        protected override void CompleteProcessing()
        {
            // Don't complete processing if the ingredient is still cooking
            // This allows the ingredient to burn if left too long
            if (_isProcessing && _currentIngredient != null && !_isBurning)
            {
                base.CompleteProcessing();
            }
        }

        /// <summary>
        /// Cancel processing an ingredient.
        /// </summary>
        protected override void CancelProcessing()
        {
            // Hide effects
            if (_steamEffect != null)
            {
                HideSteamEffectClientRpc();
            }

            if (_fireEffect != null)
            {
                HideFireEffectClientRpc();
            }

            // Stop sounds
            if (_audioSource != null)
            {
                StopSoundsClientRpc();
            }

            base.CancelProcessing();
        }

        /// <summary>
        /// Show the steam effect on all clients.
        /// </summary>
        [ClientRpc]
        private void ShowSteamEffectClientRpc()
        {
            if (_steamEffect != null)
            {
                _steamEffect.SetActive(true);
            }
        }

        /// <summary>
        /// Hide the steam effect on all clients.
        /// </summary>
        [ClientRpc]
        private void HideSteamEffectClientRpc()
        {
            if (_steamEffect != null)
            {
                _steamEffect.SetActive(false);
            }
        }

        /// <summary>
        /// Show the fire effect on all clients.
        /// </summary>
        [ClientRpc]
        private void ShowFireEffectClientRpc()
        {
            if (_fireEffect != null)
            {
                _fireEffect.SetActive(true);
            }
        }

        /// <summary>
        /// Hide the fire effect on all clients.
        /// </summary>
        [ClientRpc]
        private void HideFireEffectClientRpc()
        {
            if (_fireEffect != null)
            {
                _fireEffect.SetActive(false);
            }
        }

        /// <summary>
        /// Play the boiling sound on all clients.
        /// </summary>
        [ClientRpc]
        private void PlayBoilingSoundClientRpc()
        {
            if (_audioSource != null && _boilingSound != null)
            {
                _audioSource.clip = _boilingSound;
                _audioSource.loop = true;
                _audioSource.Play();
            }
        }

        /// <summary>
        /// Play the burning sound on all clients.
        /// </summary>
        [ClientRpc]
        private void PlayBurningSoundClientRpc()
        {
            if (_audioSource != null && _burningSound != null)
            {
                _audioSource.clip = _burningSound;
                _audioSource.loop = true;
                _audioSource.Play();
            }
        }

        /// <summary>
        /// Stop all sounds on all clients.
        /// </summary>
        [ClientRpc]
        private void StopSoundsClientRpc()
        {
            if (_audioSource != null)
            {
                _audioSource.Stop();
                _audioSource.loop = false;
            }
        }
    }
}
