using System.Collections.Generic;

namespace RecipeRage.Bots
{
    /// <summary>
    /// Measures human performance over a rolling window (recipes/minute).
    /// Bots adapt to this — the match stays competitive without rubber-banding.
    /// </summary>
    public sealed class SkillTracker
    {
        private const float WindowSec = 60f;
        private readonly Queue<float> _completionTimes = new Queue<float>(16);

        public void TrackRecipeCompleted(float matchElapsed)
        {
            _completionTimes.Enqueue(matchElapsed);
            while (_completionTimes.Count > 0 && matchElapsed - _completionTimes.Peek() > WindowSec)
            {
                _completionTimes.Dequeue();
            }
        }

        public float HumanRecipesPerMinute =>
            _completionTimes.Count == 0 ? 0f : _completionTimes.Count / (WindowSec / 60f);
    }
}
