using System.Collections.Generic;
using RecipeRage.Gameplay.Cooking;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Gameplay.Stations
{
    /// <summary>
    /// A station for assembling ingredients into a dish.
    /// </summary>
    public class AssemblyStation : CookingStation
    {
        [Header("Assembly Station Settings")]
        [SerializeField] private int _maxIngredients = 5;
        [SerializeField] private Transform[] _ingredientSlots;
        [SerializeField] private GameObject _platePrefab;
        [SerializeField] private AudioClip _assembleSound;
        
        /// <summary>
        /// The list of ingredients on the assembly station.
        /// </summary>
        private List<IngredientItem> _ingredients = new List<IngredientItem>();
        
        /// <summary>
        /// The plate object.
        /// </summary>
        private GameObject _plate;
        
        /// <summary>
        /// Whether the station has a plate.
        /// </summary>
        private bool _hasPlate;
        
        /// <summary>
        /// Initialize the assembly station.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            
            // Set station name
            _stationName = "Assembly Station";
            
            // Create plate if prefab is set
            if (_platePrefab != null && _ingredientPlacementPoint != null)
            {
                _plate = Instantiate(_platePrefab, _ingredientPlacementPoint.position, Quaternion.identity);
                _plate.transform.SetParent(_ingredientPlacementPoint);
                _plate.SetActive(false);
            }
        }
        
        /// <summary>
        /// Handle interaction from a player.
        /// </summary>
        /// <param name="player">The player that is interacting</param>
        public override void Interact(Core.Characters.PlayerController player)
        {
            if (!IsServer)
            {
                // Request interaction from the server
                InteractServerRpc(player.NetworkObject);
                return;
            }
            
            // If the player is holding an ingredient
            if (player.IsHoldingObject())
            {
                GameObject heldObject = player.GetHeldObject();
                IngredientItem ingredientItem = heldObject.GetComponent<IngredientItem>();
                
                if (ingredientItem != null)
                {
                    // If the player is holding a plate
                    if (ingredientItem.IsPlate)
                    {
                        // If the station doesn't have a plate
                        if (!_hasPlate)
                        {
                            // Take the plate from the player
                            player.DropObject();
                            
                            // Place the plate on the station
                            PlacePlate(ingredientItem);
                        }
                    }
                    // If the player is holding an ingredient
                    else if (_hasPlate && _ingredients.Count < _maxIngredients)
                    {
                        // Take the ingredient from the player
                        player.DropObject();
                        
                        // Add the ingredient to the plate
                        AddIngredientToPlate(ingredientItem);
                    }
                }
            }
            // If the station has a plate with ingredients
            else if (_hasPlate)
            {
                // Give the plate to the player
                if (player.PickUpObject(_plate))
                {
                    // Clear the station
                    _hasPlate = false;
                    _ingredients.Clear();
                    
                    // Hide the plate
                    if (_plate != null)
                    {
                        _plate.SetActive(false);
                    }
                }
            }
        }
        
        /// <summary>
        /// Get the interaction prompt text.
        /// </summary>
        /// <returns>The interaction prompt text</returns>
        public override string GetInteractionPrompt()
        {
            if (_hasPlate)
            {
                if (_ingredients.Count > 0)
                {
                    return $"Take Plate ({_ingredients.Count} ingredients)";
                }
                else
                {
                    return "Add Ingredients to Plate";
                }
            }
            else
            {
                return "Place Plate";
            }
        }
        
        /// <summary>
        /// Check if the station can be interacted with.
        /// </summary>
        /// <param name="player">The player that wants to interact</param>
        /// <returns>True if the station can be interacted with</returns>
        public override bool CanInteract(Core.Characters.PlayerController player)
        {
            // If the player is holding an object
            if (player.IsHoldingObject())
            {
                GameObject heldObject = player.GetHeldObject();
                IngredientItem ingredientItem = heldObject.GetComponent<IngredientItem>();
                
                if (ingredientItem != null)
                {
                    // If the player is holding a plate
                    if (ingredientItem.IsPlate)
                    {
                        // Can interact if the station doesn't have a plate
                        return !_hasPlate;
                    }
                    // If the player is holding an ingredient
                    else
                    {
                        // Can interact if the station has a plate and isn't full
                        return _hasPlate && _ingredients.Count < _maxIngredients;
                    }
                }
                
                return false;
            }
            
            // Can interact if the station has a plate
            return _hasPlate;
        }
        
        /// <summary>
        /// Check if the ingredient can be processed.
        /// </summary>
        /// <param name="ingredientItem">The ingredient to check</param>
        /// <returns>True if the ingredient can be processed</returns>
        protected override bool CanProcessIngredient(IngredientItem ingredientItem)
        {
            // Assembly station doesn't process ingredients
            return false;
        }
        
        /// <summary>
        /// Process the ingredient.
        /// </summary>
        /// <param name="ingredientItem">The ingredient to process</param>
        /// <returns>True if the ingredient was processed</returns>
        protected override bool ProcessIngredient(IngredientItem ingredientItem)
        {
            // Assembly station doesn't process ingredients
            return false;
        }
        
        /// <summary>
        /// Place a plate on the station.
        /// </summary>
        /// <param name="plateItem">The plate to place</param>
        private void PlacePlate(IngredientItem plateItem)
        {
            if (_plate == null)
            {
                return;
            }
            
            // Set plate state
            _hasPlate = true;
            
            // Show the plate
            _plate.SetActive(true);
            
            // Destroy the player's plate
            Destroy(plateItem.gameObject);
            
            // Play sound
            if (_audioSource != null && _assembleSound != null)
            {
                _audioSource.PlayOneShot(_assembleSound);
            }
            
            // Notify clients
            PlacePlateClientRpc();
        }
        
        /// <summary>
        /// Add an ingredient to the plate.
        /// </summary>
        /// <param name="ingredientItem">The ingredient to add</param>
        private void AddIngredientToPlate(IngredientItem ingredientItem)
        {
            if (!_hasPlate || _ingredients.Count >= _maxIngredients)
            {
                return;
            }
            
            // Add the ingredient to the list
            _ingredients.Add(ingredientItem);
            
            // Place the ingredient in a slot
            if (_ingredientSlots != null && _ingredientSlots.Length > 0 && _ingredients.Count <= _ingredientSlots.Length)
            {
                Transform slot = _ingredientSlots[_ingredients.Count - 1];
                ingredientItem.transform.SetParent(slot);
                ingredientItem.transform.localPosition = Vector3.zero;
                ingredientItem.transform.localRotation = Quaternion.identity;
            }
            else
            {
                // If no slots are available, just place it on the plate
                ingredientItem.transform.SetParent(_plate.transform);
                ingredientItem.transform.localPosition = new Vector3(0, 0.1f * _ingredients.Count, 0);
                ingredientItem.transform.localRotation = Quaternion.identity;
            }
            
            // Play sound
            if (_audioSource != null && _assembleSound != null)
            {
                _audioSource.PlayOneShot(_assembleSound);
            }
            
            // Notify clients
            AddIngredientClientRpc();
        }
        
        /// <summary>
        /// Notify clients that a plate was placed.
        /// </summary>
        [ClientRpc]
        private void PlacePlateClientRpc()
        {
            if (!IsServer && _plate != null)
            {
                _plate.SetActive(true);
                _hasPlate = true;
            }
        }
        
        /// <summary>
        /// Notify clients that an ingredient was added.
        /// </summary>
        [ClientRpc]
        private void AddIngredientClientRpc()
        {
            if (!IsServer)
            {
                // Play sound
                if (_audioSource != null && _assembleSound != null)
                {
                    _audioSource.PlayOneShot(_assembleSound);
                }
            }
        }
    }
}
