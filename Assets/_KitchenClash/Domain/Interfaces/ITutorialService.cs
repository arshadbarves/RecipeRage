using System;

namespace KitchenClash.Domain
{
    public interface ITutorialService
    {
        TutorialStep CurrentStep { get; }
        bool IsComplete { get; }
        bool IsActive { get; }

        void StartTutorial();
        void AdvanceStep();
        void SkipTutorial();

        event Action<TutorialStep> OnStepChanged;
        event Action OnTutorialComplete;
    }
}
