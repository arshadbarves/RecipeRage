using KitchenClash.Domain;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Playcenter.Shell;
using Playcenter.Services;

namespace KitchenClash.Infrastructure.Network.Stations
{
    /// <summary>
    /// Kitchen Brawler v2 delivery point.
    /// Interact while carrying a collected dish → flat ScoreService points → clear carry.
    /// No order matching, no combo, no NetworkScoreManager.
    /// </summary>
    public class DeliveryZone : StationBase
    {
        [Header("Delivery Zone")]
        [SerializeField] private int _teamId;
        [SerializeField] private GameObject _successVisual;
        [SerializeField] private GameObject _failureVisual;
        [SerializeField] private AudioClip _successSound;
        [SerializeField] private AudioClip _failureSound;

        [Inject] private IScoreService _scoreService;
        [Inject] private IMatchContext _matchContext;

        public int TeamId => _teamId;

        protected override void Awake()
        {
            base.Awake();
            _stationName = "Delivery Zone";

            LifetimeScope scope = LifetimeScope.Find<LifetimeScope>();
            if (scope != null)
            {
                scope.Container.Inject(this);
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _matchContext?.Refresh();
        }

        public override bool CanInteract(object playerObj)
        {
            if (playerObj is not PlayerController player)
            {
                return false;
            }

            if (!player.HasCarriedDish)
            {
                return false;
            }

            // Own-team delivery only when team is assigned (parity with ServingStation).
            return _teamId < 0 || player.TeamId == _teamId;
        }

        public override string GetInteractionPrompt()
        {
            return "Deliver Dish";
        }

        protected override void HandleInteraction(PlayerController player)
        {
            if (player == null || !player.HasCarriedDish)
            {
                ShowFailure();
                return;
            }

            // Optional team gate: only own-team delivery zones accept dishes.
            if (_teamId >= 0 && player.TeamId != _teamId)
            {
                GameLogger.Log($"[DeliveryZone] Team {player.TeamId} cannot deliver at team {_teamId} zone.");
                ShowFailure();
                return;
            }

            if (!player.TryConsumeCarriedDish(out CarriedItemData dish))
            {
                ShowFailure();
                return;
            }

            if (_scoreService == null)
            {
                GameLogger.LogError("[DeliveryZone] IScoreService missing — dish consumed without score.");
                ShowFailure();
                return;
            }

            Playcenter.Services.TeamId team = player.TeamId == 0
                ? Playcenter.Services.TeamId.TeamA
                : Playcenter.Services.TeamId.TeamB;
            int tier = Mathf.Clamp(dish.RecipeTier, 1, 3);
            _scoreService.AddScore(team, ScoreEvent.Delivered(tier));

            ShowSuccess();
            GameLogger.Log($"[DeliveryZone] Team {team} delivered T{tier} dish ({dish.IngredientType}).");
        }

        private void ShowSuccess()
        {
            if (_successVisual != null)
            {
                _successVisual.SetActive(true);
            }

            if (_successSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(_successSound);
            }
        }

        private void ShowFailure()
        {
            if (_failureVisual != null)
            {
                _failureVisual.SetActive(true);
            }

            if (_failureSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(_failureSound);
            }
        }
    }
}
