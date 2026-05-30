using System;
using KitchenClash.Domain;

namespace KitchenClash.Application.Services
{
    public class TutorialService : ITutorialService
    {
        private const string TutorialKey = "tutorial_progress";

        private readonly ISaveService _saveService;

        private TutorialStep _currentStep;
        private bool _isActive;
        private bool _isComplete;

        public TutorialStep CurrentStep => _currentStep;
        public bool IsComplete => _isComplete;
        public bool IsActive => _isActive;

        public event Action<TutorialStep> OnStepChanged;
        public event Action OnTutorialComplete;

        public TutorialService(ISaveService saveService)
        {
            _saveService = saveService;
            LoadState();
        }

        public void StartTutorial()
        {
            if (_isComplete)
            {
                return;
            }

            _isActive = true;
            _currentStep = TutorialStep.Welcome;
            OnStepChanged?.Invoke(_currentStep);
        }

        public void AdvanceStep()
        {
            if (!_isActive || _isComplete)
            {
                return;
            }

            int next = (int)_currentStep + 1;
            var values = (TutorialStep[])Enum.GetValues(typeof(TutorialStep));

            if (next >= values.Length || (TutorialStep)next == TutorialStep.Complete)
            {
                CompleteTutorial();
                return;
            }

            _currentStep = (TutorialStep)next;
            OnStepChanged?.Invoke(_currentStep);
            SaveState();
        }

        public void SkipTutorial()
        {
            CompleteTutorial();
        }

        private void CompleteTutorial()
        {
            _currentStep = TutorialStep.Complete;
            _isActive = false;
            _isComplete = true;
            SaveState();
            OnStepChanged?.Invoke(_currentStep);
            OnTutorialComplete?.Invoke();
        }

        private void SaveState()
        {
            var data = new TutorialSaveData
            {
                CurrentStep = (int)_currentStep,
                IsComplete = _isComplete
            };
            _saveService.Save(TutorialKey, data);
        }

        private void LoadState()
        {
            TutorialSaveData data = _saveService.Load(TutorialKey, new TutorialSaveData());
            _currentStep = (TutorialStep)data.CurrentStep;
            _isComplete = data.IsComplete;
        }

        [Serializable]
        private class TutorialSaveData
        {
            public int CurrentStep;
            public bool IsComplete;
        }
    }
}
