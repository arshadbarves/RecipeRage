using Brawlers;
using Gameplay.Data;
using Unity.Netcode;

namespace Gameplay.Station
{
    public class ChoppingStation : BaseStation
    {
        private readonly NetworkVariable<float> _choppingProgress = new NetworkVariable<float>();
        private readonly NetworkVariable<IngredientType> _currentIngredient = new NetworkVariable<IngredientType>();

        private void Update()
        {
            if (IsServer)
            {
                UpdateChopping();
            }

            UpdateVisibility();
            UpdateUI();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                IsOccupied.Value = false;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void StartChoppingServerRpc(IngredientType ingredient)
        {
            if (!IsOccupied.Value)
            {
                _currentIngredient.Value = ingredient;
                IsOccupied.Value = true;
                _choppingProgress.Value = 0f;
            }
        }

        private void UpdateChopping()
        {
            if (IsOccupied.Value && _choppingProgress.Value < 1f)
            {
                // _choppingProgress.Value += Time.deltaTime / _choppingTime;
                if (_choppingProgress.Value >= 1f)
                {
                    // Chopping is complete
                }
            }
        }

        private void UpdateUI()
        {
            float progress = _choppingProgress.Value;
            string stateText = IsOccupied.Value ? "Chopping" : "Empty";
            // progressUI.UpdateProgress(progress, stateText);
            // indicatorUI.UpdateIndicator(progress, stateText);
        }

        public override void Interact(BaseController player)
        {
            if (player.IsHoldingItem() && !IsOccupied.Value)
            {
                IngredientType ingredient = player.GetHeldIngredient();
                player.DropItem();
                StartChoppingServerRpc(ingredient);
            }
            else if (IsOccupied.Value && _choppingProgress.Value >= 1f)
            {
                player.PickUpItem(_currentIngredient.Value);
                ResetStationServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void ResetStationServerRpc()
        {
            IsOccupied.Value = false;
            _choppingProgress.Value = 0f;
            _currentIngredient.Value = IngredientType.Cheese; // Default value
        }
    }
}