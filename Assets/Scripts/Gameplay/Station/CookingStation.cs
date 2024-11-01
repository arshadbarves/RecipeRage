using Brawlers;
using Gameplay.Data;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Station
{
    public class CookingStation : BaseStation
    {
        private readonly float _burningTime = 5f;
        private readonly NetworkVariable<float> _cookingProgress = new NetworkVariable<float>();

        private readonly float _cookingTime = 10f;
        private readonly NetworkVariable<IngredientType> _currentIngredient = new NetworkVariable<IngredientType>();
        private readonly NetworkVariable<StationState> _currentState = new NetworkVariable<StationState>();

        protected void Update()
        {
            if (IsServer)
            {
                UpdateCooking();
            }
            UpdateVisibility();
            UpdateUI();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                _currentState.Value = StationState.Idle;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void StartCookingServerRpc(IngredientType ingredient)
        {
            if (_currentState.Value == StationState.Idle)
            {
                _currentIngredient.Value = ingredient;
                _currentState.Value = StationState.InProgress;
                _cookingProgress.Value = 0f;
            }
        }

        private void UpdateCooking()
        {
            if (_currentState.Value == StationState.InProgress)
            {
                _cookingProgress.Value += Time.deltaTime / _cookingTime;
                if (_cookingProgress.Value >= 1f)
                {
                    _currentState.Value = StationState.Completed;
                    _cookingProgress.Value = 0f;
                }
            }
            else if (_currentState.Value == StationState.Completed)
            {
                _cookingProgress.Value += Time.deltaTime / _burningTime;
                if (_cookingProgress.Value >= 1f)
                {
                    _currentState.Value = StationState.Burned;
                }
            }
        }

        private void UpdateUI()
        {
            float progress = _cookingProgress.Value;
            string stateText = _currentState.Value.ToString();
            // progressUI.UpdateProgress(progress, stateText);
            // indicatorUI.UpdateIndicator(progress, stateText);
        }

        public override void Interact(BaseController player)
        {
            if (player.IsHoldingItem())
            {
                if (_currentState.Value == StationState.Idle)
                {
                    IngredientType ingredient = player.GetHeldIngredient();
                    player.DropItem();
                    StartCookingServerRpc(ingredient);
                }
            }
            else if (_currentState.Value == StationState.Completed || _currentState.Value == StationState.Burned)
            {
                player.PickUpItem(_currentIngredient.Value);
                ResetStationServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void ResetStationServerRpc()
        {
            _currentState.Value = StationState.Idle;
            _cookingProgress.Value = 0f;
            _currentIngredient.Value = IngredientType.Cheese; // Default value
        }
    }
}