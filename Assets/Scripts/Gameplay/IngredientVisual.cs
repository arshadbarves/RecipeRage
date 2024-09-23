using Gameplay.Data;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay
{
    public class IngredientVisual : NetworkBehaviour
    {
        public IngredientData Data { get; private set; }
        public IngredientState State { get; private set; }

        private readonly NetworkVariable<IngredientState> _networkState = new NetworkVariable<IngredientState>();
        private GameObject _currentVisual;

        public override void OnNetworkSpawn()
        {
            _networkState.OnValueChanged += OnStateChanged;
        }

        public void Initialize(IngredientData data)
        {
            Data = data;
            UpdateVisual();
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangeStateServerRpc(IngredientState newState)
        {
            _networkState.Value = newState;
        }

        private void OnStateChanged(IngredientState previousValue, IngredientState newValue)
        {
            State = newValue;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (_currentVisual != null)
            {
                Destroy(_currentVisual);
            }

            GameObject visualPrefab = Data.IngredientStates.Find(pair => pair.State == State)?.GameObject;

            _currentVisual = Instantiate(visualPrefab, transform);
        }
        
        public void ResetState()
        {
            State = IngredientState.Raw;
            UpdateVisual();
        }
    }
}