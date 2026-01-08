using Gameplay.Characters;
using Modules.Logging;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Cooking
{
    /// <summary>
    /// Represents an ingredient item in the game world.
    /// </summary>
    public class IngredientItem : ItemBase
    {
        [Header("Ingredient Visuals")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private GameObject _rawVisual;
        [SerializeField] private GameObject _cutVisual;
        [SerializeField] private GameObject _cookedVisual;
        [SerializeField] private GameObject _burnedVisual;
        [SerializeField] private bool _isPlate;

        /// <summary>
        /// The current state of the ingredient.
        /// </summary>
        private NetworkVariable<IngredientState> _state = new NetworkVariable<IngredientState>();

        /// <summary>
        /// The ingredient data.
        /// </summary>
        private Ingredient _ingredientData;

        /// <summary>
        /// Get the ingredient data.
        /// </summary>
        public Ingredient Ingredient => _ingredientData;

        /// <summary>
        /// Whether this ingredient is a plate.
        /// </summary>
        public bool IsPlate => _isPlate;

        /// <summary>
        /// Whether this ingredient is cut.
        /// </summary>
        public bool IsCut => _state.Value.IsCut;

        /// <summary>
        /// Whether this ingredient is cooked.
        /// </summary>
        public bool IsCooked => _state.Value.IsCooked;

        /// <summary>
        /// Whether this ingredient is burned.
        /// </summary>
        public bool IsBurned => _state.Value.IsBurned;

        /// <summary>
        /// Get the current state of the ingredient.
        /// </summary>
        public IngredientState GetState()
        {
            return _state.Value;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _state.OnValueChanged += OnStateChanged;
            UpdateVisuals();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _state.OnValueChanged -= OnStateChanged;
        }

        /// <summary>
        /// Set the ingredient data.
        /// </summary>
        public void SetIngredient(Ingredient ingredient)
        {
            if (!IsServer)
            {
                GameLogger.LogWarning("Only the server can set the ingredient data.");
                return;
            }

            _ingredientData = ingredient;

            IngredientState initialState = new IngredientState
            {
                IngredientId = ingredient.Id,
                IsCut = false,
                IsCooked = false,
                IsBurned = false,
                CookingProgress = 0f,
                CuttingProgress = 0f
            };

            _state.Value = initialState;
            UpdateVisuals();
        }

        /// <summary>
        /// Cut the ingredient.
        /// </summary>
        public void Cut()
        {
            if (!IsServer) return;
            if (_ingredientData == null || !_ingredientData.RequiresCutting || _isPlate) return;

            IngredientState currentState = _state.Value;
            if (currentState.IsCut) return;

            currentState.IsCut = true;
            currentState.CuttingProgress = 1.0f;
            _state.Value = currentState;
        }

        /// <summary>
        /// Cook the ingredient.
        /// </summary>
        public void Cook()
        {
            if (!IsServer) return;
            if (_ingredientData == null || !_ingredientData.RequiresCooking || _isPlate) return;

            IngredientState currentState = _state.Value;
            if (currentState.IsBurned) return;

            currentState.IsCooked = true;
            currentState.CookingProgress = 1.0f;
            _state.Value = currentState;
        }

        /// <summary>
        /// Burn the ingredient.
        /// </summary>
        public void Burn()
        {
            if (!IsServer || _isPlate) return;

            IngredientState currentState = _state.Value;
            currentState.IsBurned = true;
            currentState.CookingProgress = 2.0f;
            _state.Value = currentState;
        }

        private void OnStateChanged(IngredientState previousState, IngredientState newState)
        {
            UpdateVisuals();
        }

        /// <summary>
        /// Update the visual representation of the ingredient.
        /// </summary>
        protected override void UpdateVisuals()
        {
            base.UpdateVisuals();

            if (_ingredientData == null) return;

            // Update the sprite
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sprite = _ingredientData.Icon;
                Color color = _ingredientData.Color;

                if (_state.Value.IsBurned) color = Color.black;
                else if (_state.Value.IsCooked) color = Color.Lerp(_ingredientData.Color, Color.gray, 0.3f);

                _spriteRenderer.color = color;
            }

            // Update the visuals based on state
            if (_rawVisual != null)
                _rawVisual.SetActive(!_state.Value.IsCut && !_state.Value.IsCooked && !_state.Value.IsBurned);
            
            if (_cutVisual != null)
                _cutVisual.SetActive(_state.Value.IsCut && !_state.Value.IsCooked && !_state.Value.IsBurned);
            
            if (_cookedVisual != null)
                _cookedVisual.SetActive(_state.Value.IsCooked && !_state.Value.IsBurned);
            
            if (_burnedVisual != null)
                _burnedVisual.SetActive(_state.Value.IsBurned);
        }
    }
}
