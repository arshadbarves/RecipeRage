using System;
using RecipeRage.Core.Interaction;
using RecipeRage.Core.Player;
using RecipeRage.Gameplay.Cooking;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Gameplay.Interactables
{
    /// <summary>
    /// A station where players can chop ingredients
    /// </summary>
    public class ChoppingStation : BaseStation, IInteractable
    {
        #region Properties

        public bool CanInteract => !_isBeingUsed.Value && !_isChopping;
        public InteractionType InteractionType => InteractionType.Cook;
        public InteractionState CurrentState { get; private set; } = InteractionState.Idle;

        #endregion

        #region Serialized Fields

        [Header("Chopping Settings")] [SerializeField]
        private float _baseChoppingTime = 3f;

        [SerializeField] private int _requiredChops = 5;

        [Header("Visual Feedback")] [SerializeField]
        private GameObject _progressBar;

        [SerializeField] private Animator _knifeAnimator;

        [Header("Audio")] [SerializeField] private AudioClip _startChoppingSound;

        [SerializeField] private AudioClip _chopSound;
        [SerializeField] private AudioClip _finishChoppingSound;

        #endregion

        #region Private Fields

        private readonly NetworkVariable<bool> _isBeingUsed = new NetworkVariable<bool>();
        private bool _isChopping;
        private GameObject _currentIngredient;
        private Ingredient _currentIngredientComponent;
        private float _choppingProgress;
        private int _currentChops;
        private PlayerController _currentPlayer;

        #endregion

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
        }

        public override void OnNetworkSpawn()
        {
            _isBeingUsed.OnValueChanged += OnBeingUsedChanged;
            UpdateVisuals();
        }

        public override void OnNetworkDespawn()
        {
            _isBeingUsed.OnValueChanged -= OnBeingUsedChanged;
        }

        #endregion

        #region IInteractable Implementation

        public bool StartInteraction(PlayerController player, Action onComplete)
        {
            if (!CanInteract || !IsServer)
                return false;

            if (player.HeldItem != null)
            {
                var ingredient = player.HeldItem.GetComponent<Ingredient>();
                if (ingredient != null && ingredient.CanBeChopped)
                {
                    StartChoppingServerRpc(player.NetworkObjectId);
                    return true;
                }
            }

            return false;
        }

        public void CancelInteraction(PlayerController player)
        {
            if (_currentPlayer == player) CancelChoppingServerRpc();
        }

        public bool ContinueInteraction(PlayerController player)
        {
            if (!_isChopping || _currentPlayer != player)
                return false;

            ContinueChoppingServerRpc();
            return true;
        }

        #endregion

        #region Server RPCs

        [ServerRpc(RequireOwnership = false)]
        private void StartChoppingServerRpc(ulong playerId)
        {
            if (_isBeingUsed.Value)
                return;

            _isBeingUsed.Value = true;
            StartChoppingClientRpc(playerId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ContinueChoppingServerRpc()
        {
            if (!_isChopping)
                return;

            _currentChops++;
            _choppingProgress = Mathf.Min(1f, (float)_currentChops / _requiredChops);

            if (_currentChops >= _requiredChops)
                CompleteChopping();
            else
                ContinueChoppingClientRpc(_choppingProgress);
        }

        [ServerRpc(RequireOwnership = false)]
        private void CancelChoppingServerRpc()
        {
            if (!_isChopping)
                return;

            _isBeingUsed.Value = false;
            CancelChoppingClientRpc();
        }

        #endregion

        #region Client RPCs

        [ClientRpc]
        private void StartChoppingClientRpc(ulong playerId)
        {
            var playerObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerId];
            var player = playerObject.GetComponent<PlayerController>();

            if (player != null)
            {
                _currentPlayer = player;
                _currentIngredient = player.HeldItem;
                _currentIngredientComponent = _currentIngredient.GetComponent<Ingredient>();

                InitializeChopping();
            }
        }

        [ClientRpc]
        private void ContinueChoppingClientRpc(float progress)
        {
            _choppingProgress = progress;
            PlayChopEffect();
            UpdateProgressBar();
        }

        [ClientRpc]
        private void CancelChoppingClientRpc()
        {
            StopChopping(false);
        }

        [ClientRpc]
        private void CompleteChoppingClientRpc()
        {
            StopChopping(true);
        }

        #endregion

        #region Private Methods

        private void InitializeChopping()
        {
            _isChopping = true;
            _choppingProgress = 0f;
            _currentChops = 0;
            CurrentState = InteractionState.InProgress;

            // Position the ingredient
            _currentIngredient.transform.SetParent(interactionPoint);
            _currentIngredient.transform.localPosition = Vector3.zero;
            _currentIngredient.transform.localRotation = Quaternion.identity;

            // Start effects
            PlayParticles();

            if (_knifeAnimator != null) _knifeAnimator.SetBool("IsChopping", true);

            PlayStartSound();
            UpdateProgressBar();
        }

        private void CompleteChopping()
        {
            if (_currentIngredientComponent != null) _currentIngredientComponent.OnChopped();

            CompleteChoppingClientRpc();
            _isBeingUsed.Value = false;
        }

        private void StopChopping(bool completed)
        {
            _isChopping = false;
            CurrentState = completed ? InteractionState.Completed : InteractionState.Canceled;

            PlayParticles(false);

            if (_knifeAnimator != null) _knifeAnimator.SetBool("IsChopping", false);

            if (completed) PlayFinishSound();

            _currentIngredient = null;
            _currentIngredientComponent = null;
            _currentPlayer = null;
            _choppingProgress = 0f;
            _currentChops = 0;

            UpdateProgressBar();
            UpdateVisuals();
        }

        private void OnBeingUsedChanged(bool previousValue, bool newValue)
        {
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            SetHighlight(CanInteract);
            SetStationMaterial(_isChopping ? StationMaterialState.Processing : StationMaterialState.Normal);
        }

        private void UpdateProgressBar()
        {
            if (_progressBar != null)
            {
                _progressBar.transform.localScale = new Vector3(_choppingProgress, 1, 1);
                _progressBar.SetActive(_isChopping);
            }
        }

        private void PlayChopEffect()
        {
            if (_knifeAnimator != null) _knifeAnimator.SetTrigger("Chop");

            PlayChopSound();
        }

        private void PlayStartSound()
        {
            PlaySound(_startChoppingSound);
        }

        private void PlayChopSound()
        {
            PlaySound(_chopSound);
        }

        private void PlayFinishSound()
        {
            PlaySound(_finishChoppingSound);
        }

        #endregion
    }
}