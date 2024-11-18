using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using RecipeRage.Gameplay.Core;
using RecipeRage.Gameplay.Core.States;
using RecipeRage.Gameplay.Player;
using Unity.Collections;

namespace RecipeRage.Gameplay.Kitchen.Stations
{
    public class ContainerStation : BaseStation
    {
        [Header("Container Settings")]
        [SerializeField] private int maxItems = 8;
        [SerializeField] private ItemType[] allowedTypes;
        [SerializeField] private ParticleSystem storeEffect;
        [SerializeField] private AudioClip storeSound;
        [SerializeField] private AudioClip retrieveSound;

        // Network state
        private readonly NetworkList<InventoryItem> _storedItems;
        private readonly NetworkVariable<bool> _isLocked = new();

        public ContainerStation()
        {
            _storedItems = new NetworkList<InventoryItem>();
        }

        protected override bool OnStationUsageStart(BaseNetworkCharacter character)
        {
            if (_isLocked.Value) return false;

            var inventory = character.GetComponent<PlayerInventory>();
            if (inventory == null) return false;

            return true; // Always allow interaction to check items
        }

        protected override bool OnStationUsageComplete(BaseNetworkCharacter character)
        {
            return true; // Always allow completion to handle items
        }

        protected override bool OnStationUsageCancel(BaseNetworkCharacter character)
        {
            return true;
        }

        [ServerRpc(RequireOwnership = false)]
        public void StoreItemServerRpc(ulong characterId, InventoryItem item)
        {
            if (_isLocked.Value) return;
            if (_storedItems.Count >= maxItems) return;
            if (!IsItemAllowed(item)) return;

            var character = FindCharacter(characterId);
            if (character == null) return;

            var inventory = character.GetComponent<PlayerInventory>();
            if (inventory == null) return;

            if (inventory.TryRemoveItem(item))
            {
                _storedItems.Add(item);
                PlayContainerEffectClientRpc(true);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RetrieveItemServerRpc(ulong characterId, int itemIndex)
        {
            if (_isLocked.Value) return;
            if (itemIndex < 0 || itemIndex >= _storedItems.Count) return;

            var character = FindCharacter(characterId);
            if (character == null) return;

            var inventory = character.GetComponent<PlayerInventory>();
            if (inventory == null) return;

            var item = _storedItems[itemIndex];
            if (inventory.TryAddItem(item))
            {
                _storedItems.RemoveAt(itemIndex);
                PlayContainerEffectClientRpc(false);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void LockContainerServerRpc(bool locked)
        {
            _isLocked.Value = locked;
        }

        private bool IsItemAllowed(InventoryItem item)
        {
            if (allowedTypes == null || allowedTypes.Length == 0)
                return true;

            foreach (var type in allowedTypes)
            {
                if (item.Type == type)
                    return true;
            }
            return false;
        }

        public List<InventoryItem> GetStoredItems()
        {
            return new List<InventoryItem>(_storedItems);
        }

        [ClientRpc]
        private void PlayContainerEffectClientRpc(bool isStoring)
        {
            if (storeEffect != null)
            {
                storeEffect.Play();
            }

            if (audioSource != null)
            {
                audioSource.PlayOneShot(isStoring ? storeSound : retrieveSound);
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (IsServer)
            {
                _isLocked.Value = false;
            }
        }
    }
}
