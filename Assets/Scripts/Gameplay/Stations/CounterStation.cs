using System.Collections.Generic;
using Core.Characters;
using Modules.Logging;
using Gameplay.Cooking;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Stations
{
    /// <summary>
    /// A standard kitchen counter that can hold one item (Ingredient or Plate).
    /// </summary>
    public class CounterStation : StationBase
    {
        [Header("Counter Settings")]
        [SerializeField] private AudioClip _placeSound;
        [SerializeField] private AudioClip _pickupSound;

        /// <summary>
        /// The item currently on the counter.
        /// </summary>
        private ItemBase _heldItem;

        protected override void Awake()
        {
            base.Awake();
            _stationName = "Counter";
        }

        protected override void HandleInteraction(PlayerController player)
        {
            bool playerHasItem = player.IsHoldingObject();
            bool counterHasItem = _heldItem != null;

            // CASE 1: Place Item on Empty Counter
            if (playerHasItem && !counterHasItem)
            {
                PlaceItem(player);
                return;
            }

            // CASE 2: Pick Up Item from Counter
            if (!playerHasItem && counterHasItem)
            {
                PickUpItem(player);
                return;
            }

            // CASE 3: Combine Items (Plate Logic)
            if (playerHasItem && counterHasItem)
            {
                HandleCombination(player);
            }
        }

        private void PlaceItem(PlayerController player)
        {
            GameObject heldObject = player.DropObject();
            ItemBase item = heldObject.GetComponent<ItemBase>();
            
            if (item != null)
            {
                // Physically move to counter
                item.transform.SetParent(_ingredientPlacementPoint);
                item.transform.localPosition = Vector3.zero;
                item.transform.localRotation = Quaternion.identity;

                _heldItem = item;
                PlaySound(_placeSound);
            }
        }

        private void PickUpItem(PlayerController player)
        {
            if (player.PickUpObject(_heldItem.gameObject))
            {
                _heldItem = null;
                PlaySound(_pickupSound);
            }
        }

        private void HandleCombination(PlayerController player)
        {
            GameObject playerObject = player.GetHeldObject();
            IngredientItem playerIngredient = playerObject.GetComponent<IngredientItem>();
            PlateItem counterPlate = _heldItem as PlateItem;

            // Scenario A: Player holds Ingredient, Counter has Plate -> Add to Plate
            if (playerIngredient != null && counterPlate != null)
            {
                if (counterPlate.AddIngredient(playerIngredient.NetworkObject.NetworkObjectId))
                {
                    player.DropObject(); // Player releases ingredient
                    // Note: PlateItem logic handles parenting the ingredient to itself
                    PlaySound(_placeSound);
                }
            }
            
            // Scenario B: Player holds Plate, Counter has Ingredient -> Add to Plate (Pickup)
            // Note: In Overcooked, you usually pick up the ingredient with the plate.
            // This would require the Counter to release the ingredient and the Plate (on player) to accept it.
            // For now, we'll implement Scenario A primarily as it matches the "Assembly Station" requirement better.
        }

        private void PlaySound(AudioClip clip)
        {
            if (_audioSource != null && clip != null)
            {
                // Play locally for instant feedback, but usually this should be a ClientRpc
                // Since StationBase is NetworkBehaviour, we should use RPCs for sound if valid
                PlaySoundClientRpc();
            }
        }

        [ClientRpc]
        private void PlaySoundClientRpc()
        {
            if (_audioSource != null && _placeSound != null)
            {
                _audioSource.PlayOneShot(_placeSound);
            }
        }

        public override string GetInteractionPrompt()
        {
            if (_heldItem != null)
            {
                if (_heldItem is PlateItem plate)
                {
                    return $"Interact with Plate ({plate.IngredientCount} items)";
                }
                return $"Pick Up {_heldItem.name}";
            }
            return "Place Item";
        }
    }
}
