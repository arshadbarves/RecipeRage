using System;

namespace Core.GameFramework.Time
{
    public class Timer
    {

        private readonly Action _completeCallback;
        private readonly Action<float> _updateCallback;

        public Timer(float duration, Action onComplete = null, Action<float> onUpdate = null, bool isLooping = false)
        {
            Duration = duration;
            RemainingTime = duration;
            IsLooping = isLooping;
            IsComplete = false;
            IsPaused = false;
            _completeCallback = onComplete;
            _updateCallback = onUpdate;
        }
        public float Duration { get; }
        public float RemainingTime { get; private set; }
        public bool IsLooping { get; }
        public bool IsComplete { get; private set; }
        public bool IsPaused { get; private set; }
        public float Progress => 1f - RemainingTime / Duration;

        public event Action OnComplete;
        public event Action<float> OnUpdate;
        public event Action OnLoop;

        public bool Update(float deltaTime)
        {
            if (IsComplete || IsPaused) return IsComplete;

            RemainingTime -= deltaTime;

            // Update progress
            float currentProgress = Progress;
            OnUpdate?.Invoke(currentProgress);
            _updateCallback?.Invoke(currentProgress);

            if (RemainingTime <= 0f)
            {
                if (IsLooping)
                {
                    RemainingTime = Duration + RemainingTime;
                    OnLoop?.Invoke();
                    OnComplete?.Invoke();
                    _completeCallback?.Invoke();
                    return false;
                }
                IsComplete = true;
                OnComplete?.Invoke();
                _completeCallback?.Invoke();
                return true;
            }

            return false;
        }

        public void Reset()
        {
            RemainingTime = Duration;
            IsComplete = false;
            IsPaused = false;
        }

        public void Pause()
        {
            IsPaused = true;
        }

        public void Resume()
        {
            IsPaused = false;
        }

        public void Cancel()
        {
            IsComplete = true;
        }
    }
}