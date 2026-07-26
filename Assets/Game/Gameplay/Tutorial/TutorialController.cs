using System;
using Playcenter;
using Playcenter.Services;
using Playcenter.UI;
using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Guided first-launch tutorial. Steps advance on gameplay events and drive
    /// the tutorial HUD via ITutorialHUD (implemented in RecipeRage.UI).
    /// No legacy UI, no TextMeshPro — UI Toolkit only.
    /// </summary>
    public sealed class TutorialController : MonoBehaviour
    {
        [SerializeField] private TutorialStep[] _steps;

        private int _currentStep;
        private IEventBus _eventBus;
        private Vector3 _playerStartPosition;
        private PlayerController _player;
        private ITutorialHUD _hud;

        public event Action OnTutorialCompleted;

        private void Start()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            SubscribeEvents();
            ShowStep(0);
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _eventBus.Subscribe<IngredientFetchedEvent>(OnFetched);
            _eventBus.Subscribe<IngredientChoppedEvent>(OnChopped);
            _eventBus.Subscribe<CookingStartedEvent>(OnCookingStarted);
            _eventBus.Subscribe<CookingCompletedEvent>(OnCookingCompleted);
            _eventBus.Subscribe<PlateTakenEvent>(OnPlateTaken);
            _eventBus.Subscribe<IngredientPlatedEvent>(OnPlated);
            _eventBus.Subscribe<RecipeServedEvent>(OnServed);
            _eventBus.Subscribe<IngredientBurntEvent>(OnBurnt);
        }

        private void UnsubscribeEvents()
        {
            _eventBus.Unsubscribe<IngredientFetchedEvent>(OnFetched);
            _eventBus.Unsubscribe<IngredientChoppedEvent>(OnChopped);
            _eventBus.Unsubscribe<CookingStartedEvent>(OnCookingStarted);
            _eventBus.Unsubscribe<CookingCompletedEvent>(OnCookingCompleted);
            _eventBus.Unsubscribe<PlateTakenEvent>(OnPlateTaken);
            _eventBus.Unsubscribe<IngredientPlatedEvent>(OnPlated);
            _eventBus.Unsubscribe<RecipeServedEvent>(OnServed);
            _eventBus.Unsubscribe<IngredientBurntEvent>(OnBurnt);
        }

        private void Update()
        {
            // Lazy-find player (spawner may create it after our Start ran)
            if (_player == null)
            {
                _player = FindFirstObjectByType<PlayerController>();
                if (_player != null)
                {
                    _playerStartPosition = _player.transform.position;
                }
                return;
            }

            // Lazy-find HUD (shown when tutorial scene loads)
            if (_hud == null)
            {
                if (ServiceLocator.TryGet<IUIService>(out var ui) && ui.Current is ITutorialHUD hud)
                {
                    _hud = hud;
                    ShowStep(_currentStep); // refresh now that HUD exists
                }
                return;
            }

            if (Current == null)
            {
                return;
            }

            if (Current.Condition == TutorialCondition.MovedDistance
                && Vector3.Distance(_player.transform.position, _playerStartPosition) > 2f)
            {
                Advance();
            }

            // Live numeric progress for tracked steps
            if (Current.TrackProgress)
            {
                UpdateNumericProgress();
            }
        }

        private void UpdateNumericProgress()
        {
            if (_hud == null)
            {
                return;
            }

            // Chop progress: nearest cutting station with an ingredient
            var cutting = FindFirstObjectByType<CuttingStation>();
            if (cutting != null && cutting.HasIngredient)
            {
                _hud.SetProgress(cutting.Progress01, Mathf.RoundToInt(cutting.Progress01 * 100f) + "%");
            }
        }

        private TutorialStep Current =>
            _currentStep < _steps.Length ? _steps[_currentStep] : null;

        private void ShowStep(int index)
        {
            _currentStep = index;
            if (Current == null)
            {
                OnTutorialCompleted?.Invoke();
                return;
            }

            _hud?.ShowStep(_currentStep, _steps.Length, Current);
        }

        private void Advance() => ShowStep(_currentStep + 1);

        private void AdvanceIf(TutorialCondition condition)
        {
            if (Current != null && Current.Condition == condition)
            {
                Advance();
            }
        }

        private void OnFetched(IngredientFetchedEvent e) => AdvanceIf(TutorialCondition.FetchedIngredient);
        private void OnChopped(IngredientChoppedEvent e) => AdvanceIf(TutorialCondition.ChoppedIngredient);
        private void OnCookingStarted(CookingStartedEvent e) => AdvanceIf(TutorialCondition.CookingStarted);
        private void OnCookingCompleted(CookingCompletedEvent e) => AdvanceIf(TutorialCondition.CookingCollected);
        private void OnPlateTaken(PlateTakenEvent e) => AdvanceIf(TutorialCondition.PlateTaken);
        private void OnPlated(IngredientPlatedEvent e) => AdvanceIf(TutorialCondition.IngredientPlated);
        private void OnServed(RecipeServedEvent e) => AdvanceIf(TutorialCondition.RecipeServed);
        private void OnBurnt(IngredientBurntEvent e) => AdvanceIf(TutorialCondition.BurnWarningShown);
    }
}
