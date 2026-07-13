using System;

namespace KitchenClash.Domain
{
    public interface ITutorialService
    {
        TutorialStep CurrentStep { get; }
        bool IsComplete { get; }
        bool IsActive { get; }

        /// <summary>Raised when the tutorial completes or is skipped.</summary>
        event Action OnCompleted;

        void StartTutorial();
        void AdvanceStep();
        void SkipTutorial();
    }
}
