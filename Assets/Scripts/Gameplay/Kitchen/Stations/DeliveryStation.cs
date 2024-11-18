using UnityEngine;
using Unity.Netcode;
using RecipeRage.Gameplay.Core;
using RecipeRage.Gameplay.Core.States;
using RecipeRage.Gameplay.Player;
using RecipeRage.Gameplay.Core.TeamManagement;

namespace RecipeRage.Gameplay.Kitchen.Stations
{
    public class DeliveryStation : BaseStation
    {
        [Header("Delivery Settings")]
        [SerializeField] private float deliveryTime = 1f;
        [SerializeField] private int pointsPerDelivery = 100;
        [SerializeField] private float qualityMultiplier = 2f;
        [SerializeField] private ParticleSystem deliveryEffect;
        [SerializeField] private AudioClip deliverySound;
        [SerializeField] private AudioClip perfectSound;

        // Network state
        private readonly NetworkVariable<float> _deliveryProgress = new();
        private readonly NetworkVariable<int> _deliveriesCompleted = new();
        private TeamManager _teamManager;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _teamManager = FindObjectOfType<TeamManager>();
        }

        protected override bool OnStationUsageStart(BaseNetworkCharacter character)
        {
            var inventory = character.GetComponent<PlayerInventory>();
            if (inventory == null) return false;

            // Check for completed dishes
            var items = inventory.GetItems();
            foreach (var item in items)
            {
                if (item.Type == ItemType.Dish)
                {
                    _deliveryProgress.Value = 0f;
                    return true;
                }
            }

            return false;
        }

        protected override bool OnStationUsageComplete(BaseNetworkCharacter character)
        {
            var inventory = character.GetComponent<PlayerInventory>();
            if (inventory == null) return false;

            // Find and deliver dish
            var items = inventory.GetItems();
            foreach (var item in items)
            {
                if (item.Type == ItemType.Dish)
                {
                    if (inventory.TryRemoveItem(item))
                    {
                        DeliverDishServerRpc(character.NetworkObjectId, item.Quality);
                        return true;
                    }
                }
            }

            return false;
        }

        protected override bool OnStationUsageCancel(BaseNetworkCharacter character)
        {
            _deliveryProgress.Value = 0f;
            return true;
        }

        [ServerRpc(RequireOwnership = false)]
        private void DeliverDishServerRpc(ulong characterId, float quality)
        {
            var character = FindCharacter(characterId);
            if (character == null) return;

            // Calculate points based on quality
            int points = Mathf.RoundToInt(pointsPerDelivery * (1f + (quality * qualityMultiplier)));
            
            // Add points to team score
            if (_teamManager != null)
            {
                var playerChar = character.GetComponent<PlayerCharacter>();
                if (playerChar != null)
                {
                    _teamManager.AddTeamScoreServerRpc(playerChar.TeamId, points);
                }
            }

            // Add points to player score
            var stats = character.GetComponent<CharacterStats>();
            if (stats != null)
            {
                stats.AddScore(points);
            }

            _deliveriesCompleted.Value++;
            PlayDeliveryEffectClientRpc(quality >= 0.9f);
        }

        [ClientRpc]
        private void PlayDeliveryEffectClientRpc(bool perfect)
        {
            if (deliveryEffect != null)
            {
                deliveryEffect.Play();
            }

            if (audioSource != null)
            {
                audioSource.PlayOneShot(perfect ? perfectSound : deliverySound);
            }
        }
    }
}
