using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using RecipeRage.Gameplay.Core;
using RecipeRage.Gameplay.Core.States;
using RecipeRage.Gameplay.Player;
using Unity.Collections;

namespace RecipeRage.Gameplay.Kitchen.Stations
{
    public class MixingStation : BaseStation
    {
        [Header("Mixing Settings")]
        [SerializeField] private float mixTime = 5f;
        [SerializeField] private int maxIngredients = 4;
        [SerializeField] private float qualityMultiplier = 1.2f;
        [SerializeField] private ParticleSystem mixingEffect;
        [SerializeField] private AudioClip[] mixingSounds;

        // Network state
        private readonly NetworkVariable<float> _mixProgress = new();
        private readonly NetworkList<InventoryItem> _currentIngredients;
        private readonly NetworkVariable<bool> _isMixing = new();

        public MixingStation()
        {
            _currentIngredients = new NetworkList<InventoryItem>();
        }

        private void Update()
        {
            if (!IsServer) return;
            if (!_isMixing.Value) return;

            UpdateMixing();
        }

        private void UpdateMixing()
        {
            if (_mixProgress.Value < 1f)
            {
                _mixProgress.Value = Mathf.Min(1f, _mixProgress.Value + Time.deltaTime / mixTime);
                
                if (_mixProgress.Value >= 1f)
                {
                    CompleteMixing();
                }
            }
        }

        protected override bool OnStationUsageStart(BaseNetworkCharacter character)
        {
            var inventory = character.GetComponent<PlayerInventory>();
            if (inventory == null) return false;

            // Allow adding ingredients if not mixing
            if (!_isMixing.Value)
            {
                return true;
            }

            return false;
        }

        protected override bool OnStationUsageComplete(BaseNetworkCharacter character)
        {
            return true; // Always allow completion to check ingredients
        }

        protected override bool OnStationUsageCancel(BaseNetworkCharacter character)
        {
            if (_isMixing.Value)
            {
                CancelMixingServerRpc();
            }
            return true;
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddIngredientServerRpc(ulong characterId, InventoryItem ingredient)
        {
            if (_isMixing.Value) return;
            if (_currentIngredients.Count >= maxIngredients) return;

            var character = FindCharacter(characterId);
            if (character == null) return;

            var inventory = character.GetComponent<PlayerInventory>();
            if (inventory == null) return;

            if (inventory.TryRemoveItem(ingredient))
            {
                _currentIngredients.Add(ingredient);
                PlayMixEffectClientRpc(0); // Add sound
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void StartMixingServerRpc(ulong characterId)
        {
            if (_isMixing.Value) return;
            if (_currentIngredients.Count < 2) return;

            _isMixing.Value = true;
            _mixProgress.Value = 0f;
            PlayMixEffectClientRpc(1); // Mix sound
        }

        [ServerRpc(RequireOwnership = false)]
        public void CancelMixingServerRpc()
        {
            if (!_isMixing.Value) return;

            _isMixing.Value = false;
            _mixProgress.Value = 0f;
            PlayMixEffectClientRpc(2); // Cancel sound
        }

        private void CompleteMixing()
        {
            if (_currentIngredients.Count < 2) return;

            // Calculate average quality
            float totalQuality = 0f;
            foreach (var ingredient in _currentIngredients)
            {
                totalQuality += ingredient.Quality;
            }
            float averageQuality = (totalQuality / _currentIngredients.Count) * qualityMultiplier;

            // Create mixed ingredient
            var mixedItem = new InventoryItem
            {
                ItemId = (int)IngredientType.Mixed,
                Type = ItemType.Ingredient,
                Quality = Mathf.Clamp01(averageQuality)
            };

            // Add to station inventory for collection
            _currentIngredients.Clear();
            _currentIngredients.Add(mixedItem);
            _isMixing.Value = false;
            
            PlayMixEffectClientRpc(3); // Complete sound
        }

        [ServerRpc(RequireOwnership = false)]
        public void CollectMixedItemServerRpc(ulong characterId)
        {
            if (_isMixing.Value) return;
            if (_currentIngredients.Count != 1) return;

            var character = FindCharacter(characterId);
            if (character == null) return;

            var inventory = character.GetComponent<PlayerInventory>();
            if (inventory == null) return;

            var mixedItem = _currentIngredients[0];
            if (inventory.TryAddItem(mixedItem))
            {
                _currentIngredients.Clear();
                PlayMixEffectClientRpc(4); // Collect sound
            }
        }

        [ClientRpc]
        private void PlayMixEffectClientRpc(int soundIndex)
        {
            if (mixingEffect != null)
            {
                if (soundIndex == 1) // Start mixing
                {
                    mixingEffect.Play();
                }
                else if (soundIndex == 2) // Cancel
                {
                    mixingEffect.Stop();
                }
            }

            if (audioSource != null && mixingSounds != null && soundIndex < mixingSounds.Length)
            {
                var sound = mixingSounds[soundIndex];
                if (sound != null)
                {
                    audioSource.PlayOneShot(sound);
                }
            }
        }
    }
}
