using System.Collections.Generic;
using Core.Logging;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Cooking
{
    /// <summary>
    /// Represents a plate that holds ingredients for dish assembly.
    /// Follows Single Responsibility Principle - handles only plate state.
    /// </summary>
    public class PlateItem : ItemBase
    {
        [Header("Plate Settings")]
        [SerializeField] private int _maxIngredients = 5;
        [SerializeField] private Transform[] _ingredientSlots;
        [SerializeField] private GameObject _plateVisual;

        /// <summary>
        /// The list of ingredient NetworkObject IDs on this plate.
        /// </summary>
        private NetworkList<ulong> _ingredientIds;

        /// <summary>
        /// The target recipe ID for this plate.
        /// </summary>
        private NetworkVariable<int> _recipeId = new NetworkVariable<int>(-1);

        /// <summary>
        /// Whether the dish is complete.
        /// </summary>
        private NetworkVariable<bool> _isComplete = new NetworkVariable<bool>(false);

        /// <summary>
        /// Get the recipe ID.
        /// </summary>
        public int RecipeId => _recipeId.Value;

        /// <summary>
        /// Get whether the dish is complete.
        /// </summary>
        public bool IsComplete => _isComplete.Value;

        /// <summary>
        /// Get the number of ingredients on the plate.
        /// </summary>
        public int IngredientCount => _ingredientIds.Count;

        /// <summary>
        /// Initialize the plate.
        /// </summary>
        private void Awake()
        {
            _ingredientIds = new NetworkList<ulong>();
        }

        /// <summary>
        /// Set up network variable callbacks.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _ingredientIds.OnListChanged += OnIngredientListChanged;
            _isComplete.OnValueChanged += OnIsCompleteChanged;
        }

        /// <summary>
        /// Clean up network variable callbacks.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            _ingredientIds.OnListChanged -= OnIngredientListChanged;
            _isComplete.OnValueChanged -= OnIsCompleteChanged;
        }

        /// <summary>
        /// Set the target recipe for this plate.
        /// </summary>
        public void SetRecipe(int recipeId)
        {
            if (!IsServer)
            {
                GameLogger.LogWarning("Only the server can set the recipe");
                return;
            }

            _recipeId.Value = recipeId;
        }

        /// <summary>
        /// Add an ingredient to the plate.
        /// </summary>
        public bool AddIngredient(ulong ingredientNetworkId)
        {
            if (!IsServer)
            {
                GameLogger.LogWarning("Only the server can add ingredients");
                return false;
            }

            // Check if plate is full
            if (_ingredientIds.Count >= _maxIngredients)
            {
                GameLogger.LogWarning("Plate is full");
                return false;
            }

            // Check if ingredient already on plate
            if (_ingredientIds.Contains(ingredientNetworkId))
            {
                GameLogger.LogWarning("Ingredient already on plate");
                return false;
            }

            // Add ingredient
            _ingredientIds.Add(ingredientNetworkId);

            // Get the ingredient NetworkObject
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ingredientNetworkId, out NetworkObject ingredientObject))
            {
                // Parent to plate
                if (_ingredientSlots != null && _ingredientIds.Count <= _ingredientSlots.Length)
                {
                    Transform slot = _ingredientSlots[_ingredientIds.Count - 1];
                    ingredientObject.transform.SetParent(slot);
                    ingredientObject.transform.localPosition = Vector3.zero;
                    ingredientObject.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    // Stack on plate
                    ingredientObject.transform.SetParent(transform);
                    ingredientObject.transform.localPosition = Vector3.up * 0.1f * _ingredientIds.Count;
                    ingredientObject.transform.localRotation = Quaternion.identity;
                }
            }

            GameLogger.Log($"Added ingredient {ingredientNetworkId} to plate");
            return true;
        }

        /// <summary>
        /// Remove an ingredient from the plate.
        /// </summary>
        public bool RemoveIngredient(ulong ingredientNetworkId)
        {
            if (!IsServer)
            {
                GameLogger.LogWarning("Only the server can remove ingredients");
                return false;
            }

            // Check if ingredient is on plate
            if (!_ingredientIds.Contains(ingredientNetworkId))
            {
                GameLogger.LogWarning("Ingredient not on plate");
                return false;
            }

            // Remove ingredient
            _ingredientIds.Remove(ingredientNetworkId);

            // Unparent ingredient
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ingredientNetworkId, out NetworkObject ingredientObject))
            {
                ingredientObject.transform.SetParent(null);
            }

            GameLogger.Log($"Removed ingredient {ingredientNetworkId} from plate");
            return true;
        }

        /// <summary>
        /// Get all ingredients on the plate.
        /// </summary>
        public List<IngredientItem> GetIngredients()
        {
            List<IngredientItem> ingredients = new List<IngredientItem>();

            foreach (ulong ingredientId in _ingredientIds)
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ingredientId, out NetworkObject ingredientObject))
                {
                    IngredientItem ingredient = ingredientObject.GetComponent<IngredientItem>();
                    if (ingredient != null)
                    {
                        ingredients.Add(ingredient);
                    }
                }
            }

            return ingredients;
        }

        /// <summary>
        /// Mark the dish as complete.
        /// </summary>
        public void CompleteDish()
        {
            if (!IsServer)
            {
                GameLogger.LogWarning("Only the server can complete the dish");
                return;
            }

            _isComplete.Value = true;
        }

        /// <summary>
        /// Clear all ingredients from the plate.
        /// </summary>
        public void Clear()
        {
            if (!IsServer)
            {
                GameLogger.LogWarning("Only the server can clear the plate");
                return;
            }

            // Unparent all ingredients
            foreach (ulong ingredientId in _ingredientIds)
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ingredientId, out NetworkObject ingredientObject))
                {
                    ingredientObject.transform.SetParent(null);
                }
            }

            _ingredientIds.Clear();
            _isComplete.Value = false;
            _recipeId.Value = -1;
        }

        /// <summary>
        /// Get the interaction prompt.
        /// </summary>
        public override string GetInteractionPrompt()
        {
            if (_isHeld.Value)
            {
                return "Drop Plate";
            }
            else
            {
                return $"Pick Up Plate ({_ingredientIds.Count} ingredients)";
            }
        }

        protected override void OnIsHeldChanged(bool previousValue, bool newValue)
        {
            base.OnIsHeldChanged(previousValue, newValue);
            // Update collider or visuals specific to Plate
            if (_plateVisual != null)
            {
                // Could add visual feedback here
            }
        }

        /// <summary>
        /// Handle ingredient list changes.
        /// </summary>
        private void OnIngredientListChanged(NetworkListEvent<ulong> changeEvent)
        {
            // Update visuals or trigger events
            GameLogger.Log($"Ingredient list changed, now has {_ingredientIds.Count} ingredients");
        }

        /// <summary>
        /// Handle completion state changes.
        /// </summary>
        private void OnIsCompleteChanged(bool previousValue, bool newValue)
        {
            if (newValue)
            {
                GameLogger.Log("Dish completed!");
                // Update visuals or trigger effects
            }
        }
    }
}
