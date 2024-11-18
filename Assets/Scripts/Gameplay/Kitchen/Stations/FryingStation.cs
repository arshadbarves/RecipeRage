using UnityEngine;
using Unity.Netcode;
using RecipeRage.Gameplay.Core;
using RecipeRage.Gameplay.Core.States;
using RecipeRage.Gameplay.Player;

namespace RecipeRage.Gameplay.Kitchen.Stations
{
    public class FryingStation : BaseStation
    {
        [Header("Frying Settings")]
        [SerializeField] private float fryTime = 4f;
        [SerializeField] private float burnTime = 2f;
        [SerializeField] private float qualityBonus = 0.25f;
        [SerializeField] private int maxFlips = 3;
        [SerializeField] private float perfectFlipWindow = 0.2f;
        [SerializeField] private ParticleSystem oilEffect;
        [SerializeField] private ParticleSystem smokeEffect;
        [SerializeField] private AudioClip sizzleSound;
        [SerializeField] private AudioClip flipSound;
        [SerializeField] private AudioClip burnSound;

        // Network state
        private readonly NetworkVariable<float> _fryProgress = new();
        private readonly NetworkVariable<float> _burnProgress = new();
        private readonly NetworkVariable<int> _flipCount = new();
        private readonly NetworkVariable<float> _nextIdealFlipTime = new();
        private readonly NetworkVariable<bool> _isAttended = new();
        private readonly NetworkVariable<ulong> _currentUserId = new();

        private void Update()
        {
            if (!IsServer) return;
            if (CurrentState.Value != StateType.InUse) return;

            UpdateFrying();
        }

        private void UpdateFrying()
        {
            float deltaTime = Time.deltaTime;

            // Update frying progress
            if (_fryProgress.Value < 1f)
            {
                _fryProgress.Value = Mathf.Min(1f, _fryProgress.Value + deltaTime / fryTime);
                
                if (_fryProgress.Value >= 1f && _flipCount.Value >= maxFlips)
                {
                    var character = FindCharacter(_currentUserId.Value);
                    if (character != null)
                    {
                        CompleteFryingServerRpc(character.NetworkObjectId);
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

            // Check for fryable ingredients
            var items = inventory.GetItems();
            foreach (var item in items)
            {
                if (CanFryIngredient(item))
                {
                    _fryProgress.Value = 0f;
                    _burnProgress.Value = 0f;
                    _flipCount.Value = 0;
                    _isAttended.Value = true;
                    _currentUserId.Value = character.NetworkObjectId;
                    _nextIdealFlipTime.Value = Time.time + (fryTime / (maxFlips + 1));
                    return true;
                }
            }

            return false;
        }

        protected override bool OnStationUsageComplete(BaseNetworkCharacter character)
        {
            if (_fryProgress.Value < 1f || _flipCount.Value < maxFlips) return false;

            var inventory = character.GetComponent<PlayerInventory>();
            if (inventory == null) return false;

            // Find and improve fryable ingredient
            var items = inventory.GetItems();
            foreach (var item in items)
            {
                if (CanFryIngredient(item))
                {
                    var friedItem = new InventoryItem
                    {
                        ItemId = item.ItemId,
                        Type = item.Type,
                        Quality = Mathf.Min(1f, item.Quality + qualityBonus)
                    };

                    if (inventory.TryRemoveItem(item) && inventory.TryAddItem(friedItem))
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

        private bool CanFryIngredient(InventoryItem item)
        {
            return item.Type == ItemType.Ingredient && 
                   (item.ItemId == (int)IngredientType.Potato ||
                    item.ItemId == (int)IngredientType.Fish ||
                    item.ItemId == (int)IngredientType.Egg);
        }

        [ServerRpc(RequireOwnership = false)]
        public void FlipFoodServerRpc(ulong characterId)
        {
            if (CurrentState.Value != StateType.InUse) return;
            if (_flipCount.Value >= maxFlips) return;

            float timeDiff = Mathf.Abs(Time.time - _nextIdealFlipTime.Value);
            float qualityMod = timeDiff <= perfectFlipWindow ? 0.1f : -0.1f;

            var character = FindCharacter(characterId);
            if (character != null)
            {
                var inventory = character.GetComponent<PlayerInventory>();
                if (inventory != null)
                {
                    // Modify quality of ingredient based on flip timing
                    var items = inventory.GetItems();
                    foreach (var item in items)
                    {
                        if (CanFryIngredient(item))
                        {
                            var modifiedItem = item;
                            modifiedItem.Quality = Mathf.Clamp01(item.Quality + qualityMod);
                            inventory.UpdateItem(item, modifiedItem);
                            break;
                        }
                    }
                }
            }

            _flipCount.Value++;
            _nextIdealFlipTime.Value = Time.time + (fryTime / (maxFlips + 1));
            PlayFlipEffectClientRpc(timeDiff <= perfectFlipWindow);
        }

        [ServerRpc(RequireOwnership = false)]
        private void CompleteFryingServerRpc(ulong characterId)
        {
            var character = FindCharacter(characterId);
            if (character == null) return;

            CompleteInteraction(character);
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
                if (CanFryIngredient(item))
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
            _fryProgress.Value = 0f;
            _burnProgress.Value = 0f;
            _flipCount.Value = 0;
            _isAttended.Value = false;
            _currentUserId.Value = 0;
            CurrentState.Value = StateType.Available;
        }

        [ClientRpc]
        private void PlayFlipEffectClientRpc(bool perfect)
        {
            if (oilEffect != null)
            {
                oilEffect.Play();
            }

            if (audioSource != null && flipSound != null)
            {
                audioSource.pitch = perfect ? 1.2f : 1f;
                audioSource.PlayOneShot(flipSound);
                audioSource.pitch = 1f;
            }
        }

        [ClientRpc]
        private void PlayBurnEffectClientRpc()
        {
            if (smokeEffect != null)
            {
                smokeEffect.Play();
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
                if (oilEffect != null)
                {
                    oilEffect.Play();
                }

                if (audioSource != null && sizzleSound != null)
                {
                    audioSource.PlayOneShot(sizzleSound);
                }
            }
        }
    }
}
