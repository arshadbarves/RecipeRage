using UnityEngine;
using Unity.Netcode;
using RecipeRage.Gameplay.Core;
using RecipeRage.Gameplay.Core.States;
using RecipeRage.Gameplay.Player;

namespace RecipeRage.Gameplay.Kitchen.Stations
{
    public class GrillStation : BaseStation
    {
        [Header("Grilling Settings")]
        [SerializeField] private float grillTime = 8f;
        [SerializeField] private float burnTime = 4f;
        [SerializeField] private float qualityBonus = 0.3f;
        [SerializeField] private ParticleSystem smokeEffect;
        [SerializeField] private ParticleSystem fireEffect;
        [SerializeField] private AudioClip sizzleSound;
        [SerializeField] private AudioClip burnSound;

        // Network state
        private readonly NetworkVariable<float> _grillProgress = new();
        private readonly NetworkVariable<float> _burnProgress = new();
        private readonly NetworkVariable<bool> _isAttended = new();
        private readonly NetworkVariable<ulong> _currentUserId = new();

        private void Update()
        {
            if (!IsServer) return;
            if (CurrentState.Value != StateType.InUse) return;

            UpdateGrilling();
        }

        private void UpdateGrilling()
        {
            float deltaTime = Time.deltaTime;

            // Update grilling progress
            if (_grillProgress.Value < 1f)
            {
                _grillProgress.Value = Mathf.Min(1f, _grillProgress.Value + deltaTime / grillTime);
                
                if (_grillProgress.Value >= 1f)
                {
                    // Start burn timer if not attended
                    if (!_isAttended.Value)
                    {
                        PlayEffectClientRpc(StationEffectType.Start);
                    }
                    else
                    {
                        var character = FindCharacter(_currentUserId.Value);
                        if (character != null)
                        {
                            CompleteInteraction(character);
                        }
                    }
                }
            }
            // Update burn progress if not attended
            else if (!_isAttended.Value)
            {
                _burnProgress.Value = Mathf.Min(1f, _burnProgress.Value + deltaTime / burnTime);
                
                if (_burnProgress.Value >= 1f)
                {
                    var character = FindCharacter(_currentUserId.Value);
                    if (character != null)
                    {
                        BurnFoodServerRpc(character.NetworkObjectId);
                    }
                }
            }
        }

        protected override bool OnStationUsageStart(BaseNetworkCharacter character)
        {
            var inventory = character.GetComponent<PlayerInventory>();
            if (inventory == null) return false;

            // Check for grillable ingredients
            var items = inventory.GetItems();
            foreach (var item in items)
            {
                if (CanGrillIngredient(item))
                {
                    _grillProgress.Value = 0f;
                    _burnProgress.Value = 0f;
                    _isAttended.Value = true;
                    _currentUserId.Value = character.NetworkObjectId;
                    return true;
                }
            }

            return false;
        }

        protected override bool OnStationUsageComplete(BaseNetworkCharacter character)
        {
            if (_grillProgress.Value < 1f) return false;

            var inventory = character.GetComponent<PlayerInventory>();
            if (inventory == null) return false;

            // Find and improve grillable ingredient
            var items = inventory.GetItems();
            foreach (var item in items)
            {
                if (CanGrillIngredient(item))
                {
                    var grilledItem = new InventoryItem
                    {
                        ItemId = item.ItemId,
                        Type = item.Type,
                        Quality = Mathf.Min(1f, item.Quality + qualityBonus)
                    };

                    if (inventory.TryRemoveItem(item) && inventory.TryAddItem(grilledItem))
                    {
                        ResetStationServerRpc();
                        return true;
                    }
                }
            }

            return false;
        }

        protected override bool OnStationUsageCancel(BaseNetworkCharacter character)
        {
            _isAttended.Value = false;
            return true;
        }

        private bool CanGrillIngredient(InventoryItem item)
        {
            return item.Type == ItemType.Ingredient && 
                   (item.ItemId == (int)IngredientType.Meat ||
                    item.ItemId == (int)IngredientType.Fish);
        }

        [ServerRpc(RequireOwnership = false)]
        private void BurnFoodServerRpc(ulong characterId)
        {
            var character = FindCharacter(characterId);
            if (character == null) return;

            var inventory = character.GetComponent<PlayerInventory>();
            if (inventory == null) return;

            // Find and burn the ingredient
            var items = inventory.GetItems();
            foreach (var item in items)
            {
                if (CanGrillIngredient(item))
                {
                    var burntItem = new InventoryItem
                    {
                        ItemId = item.ItemId,
                        Type = item.Type,
                        Quality = 0f // Burnt
                    };

                    if (inventory.TryRemoveItem(item) && inventory.TryAddItem(burntItem))
                    {
                        PlayBurnEffectClientRpc();
                        ResetStationServerRpc();
                        break;
                    }
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void ResetStationServerRpc()
        {
            _grillProgress.Value = 0f;
            _burnProgress.Value = 0f;
            _isAttended.Value = false;
            _currentUserId.Value = 0;
            CurrentState.Value = StateType.Available;
        }

        [ClientRpc]
        private void PlayBurnEffectClientRpc()
        {
            if (fireEffect != null)
            {
                fireEffect.Play();
            }

            if (audioSource != null && burnSound != null)
            {
                audioSource.PlayOneShot(burnSound);
            }
        }

        protected override void PlayEffectClientRpc(StationEffectType effectType)
        {
            base.PlayEffectClientRpc(effectType);

            if (effectType == StationEffectType.Start)
            {
                if (smokeEffect != null)
                {
                    smokeEffect.Play();
                }

                if (audioSource != null && sizzleSound != null)
                {
                    audioSource.PlayOneShot(sizzleSound);
                }
            }
        }
    }
}
