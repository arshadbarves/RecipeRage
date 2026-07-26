using System;
using Playcenter;
using Playcenter.Services;
using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Guided first-launch tutorial. Steps advance on gameplay events. No timer,
    /// no failure — burnt items are cleared and the step retries.
    /// </summary>
    public sealed class TutorialController : MonoBehaviour
    {
        [SerializeField] private TutorialStep[] _steps;
        [SerializeField] private GameObject _highlightArrow;
        [SerializeField] private TMPro.TextMeshProUGUI _instructionLabel;

        private int _currentStep;
        private IEventBus _eventBus;
        private Vector3 _playerStartPosition;
        private PlayerController _player;

        public event Action OnTutorialCompleted;

        private void Start()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _player = FindObjectOfType<PlayerController>(); // tutorial scene: single player
            _playerStartPosition = _player.transform.position;

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
            if (Current == null)
            {
                return;
            }

            if (Current.Condition == TutorialCondition.MovedDistance
                && Vector3.Distance(_player.transform.position, _playerStartPosition) > 2f)
            {
                Advance();
            }
        }

        private TutorialStep Current =>
            _currentStep < _steps.Length ? _steps[_currentStep] : null;

        private void ShowStep(int index)
        {
            _currentStep = index;
            if (Current == null)
            {
                _instructionLabel.text = "You're ready. Let's cook!";
                _highlightArrow.SetActive(false);
                OnTutorialCompleted?.Invoke();
                return;
            }

            _instructionLabel.text = Current.Instruction;
            _highlightArrow.SetActive(Current.HighlightTarget != null);
            if (Current.HighlightTarget != null)
            {
                _highlightArrow.transform.position = Current.HighlightTarget.position + Vector3.up * 2f;
            }
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
