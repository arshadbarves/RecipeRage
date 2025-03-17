using RecipeRage.Core.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace RecipeRage.Gameplay.Cooking
{
    /// <summary>
    /// Represents a cookable ingredient in the game
    /// </summary>
    public class Ingredient : NetworkBehaviour, IPickupable
    {
        #region Properties

        public string IngredientId => _ingredientData.ingredientId;
        public string DisplayName => _ingredientData.displayName;
        public Sprite CurrentIcon => _ingredientData.GetIconForState(CurrentState);
        public CookingState CurrentState { get; private set; } = CookingState.Raw;
        public bool IsProcessed => CurrentState != CookingState.Raw;
        public bool CanBeChopped => _ingredientData != null && _ingredientData.validCookingMethods.Contains(CookingMethod.Chop);

        #endregion

        #region Serialized Fields

        [Header("Ingredient Settings")] [SerializeField]
        private IngredientData _ingredientData;

        [Header("Visual Settings")] [SerializeField]
        private Image _iconImage;
        [SerializeField] private GameObject _rawVisual;
        [SerializeField] private GameObject _cookedVisual;
        [SerializeField] private GameObject _burntVisual;
        [SerializeField] private ParticleSystem _cookingEffect;

        [Header("Audio")] [SerializeField]
        private AudioClip _pickupSound;
        [SerializeField] private AudioClip _dropSound;

        #endregion

        #region Private Fields

        private AudioSource _audioSource;
        private readonly NetworkVariable<CookingState> _networkState = new NetworkVariable<CookingState>();

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            UpdateVisuals(CurrentState);
        }

        public override void OnNetworkSpawn()
        {
            _networkState.OnValueChanged += OnStateChanged;
            UpdateVisuals(CurrentState);
        }

        public override void OnNetworkDespawn()
        {
            _networkState.OnValueChanged -= OnStateChanged;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates the cooking state of the ingredient
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void UpdateStateServerRpc(CookingState newState)
        {
            _networkState.Value = newState;
        }

        /// <summary>
        /// Checks if this ingredient can be combined with another
        /// </summary>
        public bool CanCombineWith(Ingredient other)
        {
            if (other == null || _ingredientData == null)
                return false;

            return _ingredientData.validCombinations.Contains(other.IngredientId);
        }

        /// <summary>
        /// Gets the GameObject for pickup functionality
        /// </summary>
        public GameObject GetGameObject()
        {
            return gameObject;
        }

        /// <summary>
        /// Called when the ingredient has been chopped
        /// </summary>
        public void OnChopped()
        {
            if (IsServer)
            {
                UpdateStateServerRpc(CookingState.Cooked);
            }
        }

        #endregion

        #region Private Methods

        private void OnStateChanged(CookingState previousValue, CookingState newValue)
        {
            CurrentState = newValue;
            UpdateVisuals(newValue);
        }

        private void UpdateVisuals(CookingState state)
        {
            // Update icon
            if (_iconImage != null)
            {
                _iconImage.sprite = _ingredientData.GetIconForState(state);
            }

            // Update mesh/visual based on state
            if (_rawVisual != null) _rawVisual.SetActive(state == CookingState.Raw);
            if (_cookedVisual != null) _cookedVisual.SetActive(state == CookingState.Cooked);
            if (_burntVisual != null) _burntVisual.SetActive(state == CookingState.Burnt);

            // Play cooking effect when state changes to cooked
            if (state == CookingState.Cooked && _cookingEffect != null)
            {
                _cookingEffect.Play();
            }
        }

        #endregion
    }
}