using System;

namespace Core.GameFramework.Time
{
    /// <summary>
    ///     Interface for managing game time, timers, and time-based game mechanics
    /// </summary>
    public interface ITimeManager
    {
        /// <summary>
        ///     Current game time in seconds
        /// </summary>
        float GameTime { get; }

        /// <summary>
        ///     Delta time adjusted for time scale and pause state
        /// </summary>
        float DeltaTime { get; }

        /// <summary>
        ///     Fixed delta time adjusted for time scale and pause state
        /// </summary>
        float FixedDeltaTime { get; }

        /// <summary>
        ///     Current time scale (1.0f is normal speed)
        /// </summary>
        float TimeScale { get; }

        /// <summary>
        ///     Whether the game is currently paused
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        ///     Current network latency in seconds
        /// </summary>
        float Latency { get; }

        /// <summary>
        ///     Predicted time accounting for network latency
        /// </summary>
        float PredictedTime { get; }

        /// <summary>
        ///     Pause the game time
        /// </summary>
        void Pause();

        /// <summary>
        ///     Resume the game time
        /// </summary>
        void Resume();

        /// <summary>
        ///     Set the time scale
        /// </summary>
        /// <param name="scale">New time scale value</param>
        void SetTimeScale(float scale);

        /// <summary>
        ///     Create a new timer
        /// </summary>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="isLooping">Whether the timer should loop</param>
        /// <returns>New Timer instance</returns>
        Timer CreateTimer(float duration, bool isLooping = false);

        /// <summary>
        ///     Create a new timer with callback
        /// </summary>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="onComplete">Callback when timer completes</param>
        /// <param name="isLooping">Whether the timer should loop</param>
        /// <returns>New Timer instance</returns>
        Timer CreateTimer(float duration, Action onComplete, bool isLooping = false);

        /// <summary>
        ///     Create a new timer with progress callback
        /// </summary>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="onUpdate">Callback for progress updates</param>
        /// <param name="onComplete">Callback when timer completes</param>
        /// <param name="isLooping">Whether the timer should loop</param>
        /// <returns>New Timer instance</returns>
        Timer CreateTimer(float duration, Action<float> onUpdate, Action onComplete, bool isLooping = false);

        /// <summary>
        ///     Update all active timers
        /// </summary>
        void UpdateTimers();

        /// <summary>
        ///     Cancel all active timers
        /// </summary>
        void CancelAllTimers();

        /// <summary>
        ///     Get predicted time for an action accounting for network latency
        /// </summary>
        /// <param name="actionDuration">Duration of the action</param>
        /// <returns>Predicted completion time</returns>
        float GetPredictedTimeForAction(float actionDuration);

        /// <summary>
        ///     Schedule a delayed action
        /// </summary>
        /// <param name="delay">Delay in seconds</param>
        /// <param name="action">Action to execute</param>
        /// <returns>Timer instance for the delayed action</returns>
        Timer Schedule(float delay, Action action);

        /// <summary>
        ///     Schedule a repeating action
        /// </summary>
        /// <param name="interval">Interval in seconds</param>
        /// <param name="action">Action to execute</param>
        /// <returns>Timer instance for the repeating action</returns>
        Timer ScheduleRepeating(float interval, Action action);

        /// <summary>
        ///     Get the server time adjusted for network latency
        /// </summary>
        /// <returns>Adjusted server time</returns>
        double GetAdjustedServerTime();

        /// <summary>
        ///     Convert local time to server time
        /// </summary>
        /// <param name="localTime">Local time to convert</param>
        /// <returns>Corresponding server time</returns>
        double LocalToServerTime(double localTime);

        /// <summary>
        ///     Convert server time to local time
        /// </summary>
        /// <param name="serverTime">Server time to convert</param>
        /// <returns>Corresponding local time</returns>
        double ServerToLocalTime(double serverTime);
    }
}