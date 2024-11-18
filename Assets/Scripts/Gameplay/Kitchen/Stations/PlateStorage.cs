using UnityEngine;
using Unity.Netcode;
using RecipeRage.Gameplay.Core;
using RecipeRage.Gameplay.Core.States;
using RecipeRage.Gameplay.Player;

namespace RecipeRage.Gameplay.Kitchen.Stations
{
    public class PlateStorage : BaseStation
    {
        [Header("Plate Settings")]
        [SerializeField] private int maxPlates = 5;
        [SerializeField] private float respawnTime = 3f;
        [SerializeField] private ParticleSystem plateEffect;
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private AudioClip respawnSound;

        // Network state
        private readonly NetworkVariable<int> _availablePlates = new();
        private readonly NetworkVariable<float> _nextRespawnTime = new();

        private void Update()
        {
            if (!IsServer) return;

            // Respawn plates
            if (_availablePlates.Value < maxPlates && Time.time >= _nextRespawnTime.Value)
            {
                RespawnPlateServerRpc();
            }
        }

        protected override bool OnStationUsageStart(BaseNetworkCharacter character)
        {
            var inventory = character.GetComponent<PlayerInventory>();
            if (inventory == null) return false;

            // Check if player can take a plate
            if (_availablePlates.Value > 0 && !HasPlate(inventory))
            {
                return true;
            }

            return false;
        }

        protected override bool OnStationUsageComplete(BaseNetworkCharacter character)
        {
            var inventory = character.GetComponent<PlayerInventory>();
            if (inventory == null) return false;

            // Give plate to player
            if (_availablePlates.Value > 0 && !HasPlate(inventory))
            {
                var plate = new InventoryItem
                {
                    ItemId = 0, // Plate ID
                    Type = ItemType.Plate,
                    Quality = 1f
                };

                if (inventory.TryAddItem(plate))
                {
                    _availablePlates.Value--;
                    _nextRespawnTime.Value = Time.time + respawnTime;
                    PlayPlateEffectClientRpc(true);
                    return true;
                }
            }

            return false;
        }

        protected override bool OnStationUsageCancel(BaseNetworkCharacter character)
        {
            return true;
        }

        private bool HasPlate(PlayerInventory inventory)
        {
            var items = inventory.GetItems();
            foreach (var item in items)
            {
                if (item.Type == ItemType.Plate)
                {
                    return true;
                }
            }
            return false;
        }

        [ServerRpc(RequireOwnership = false)]
        private void RespawnPlateServerRpc()
        {
            _availablePlates.Value++;
            _nextRespawnTime.Value = Time.time + respawnTime;
            PlayPlateEffectClientRpc(false);
        }

        [ClientRpc]
        private void PlayPlateEffectClientRpc(bool isPickup)
        {
            if (plateEffect != null)
            {
                plateEffect.Play();
            }

            if (audioSource != null)
            {
                audioSource.PlayOneShot(isPickup ? pickupSound : respawnSound);
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (IsServer)
            {
                _availablePlates.Value = maxPlates;
                _nextRespawnTime.Value = 0f;
            }
        }
    }
}
