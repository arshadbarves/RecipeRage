To use this system, you need to:

1. Register it in your dependency injection container:

```csharp
public class GameInstaller : MonoInstaller
{
    public override void Install(IContainerBuilder builder)
    {
        builder.RegisterComponent(new GameObject("NetworkTimeManager")
            .AddComponent<NetworkTimeManager>())
            .As<ITimeManager>()
            .AsSelf();
    }
}
```

2. Example usage in a gameplay component:

````csharp
public class CookingStation : NetworkBehaviour
{
    [Inject] private ITimeManager timeManager;
    private Timer cookingTimer;
    private NetworkVariable<double> cookStartTime = new NetworkVariable<double>();

    public void StartCooking(float duration)
    {
        if (IsServer)
        {
            cookStartTime.Value = timeManager.GetAdjustedServerTime();
            cookingTimer = timeManager.CreateTimer(
                duration,
                progress => OnCookingProgress(progress),
                () => OnCookingComplete()
            );
        }
    }

    private void OnCookingProgress(float progress)
    {
        // Update visuals with prediction
        float predictedProgress = GetPredictedProgress();
        UpdateVisuals(predictedProgress);
    }

    private float GetPredictedProgress()
    {
        double currentTime = timeManager.GetAdjustedServerTime();
        float elapsedTime = (float)(currentTime - cookStartTime.Value);
        return Mathf.Clamp01(elapsedTime / cook

Can you continue?

Here's the continuation of the example usage and additional utility classes:

```csharp
// CookingStation.cs (continued)
public class CookingStation : NetworkBehaviour
{
    [Inject] private ITimeManager timeManager;
    private Timer cookingTimer;
    private NetworkVariable<double> cookStartTime = new NetworkVariable<double>();
    private NetworkVariable<float> cookDuration = new NetworkVariable<float>();
    private NetworkVariable<bool> isCooking = new NetworkVariable<bool>();

    [SerializeField] private ParticleSystem cookingVFX;
    [SerializeField] private AudioSource cookingSound;

    public void StartCooking(float duration)
    {
        if (IsServer)
        {
            cookStartTime.Value = timeManager.GetAdjustedServerTime();
            cookDuration.Value = duration;
            isCooking.Value = true;

            cookingTimer = timeManager.CreateTimer(
                duration,
                progress => OnCookingProgress(progress),
                () => OnCookingComplete()
            );
        }
    }

    private void Update()
    {
        if (isCooking.Value)
        {
            float predictedProgress = GetPredictedProgress();
            UpdateVisuals(predictedProgress);
        }
    }

    private float GetPredictedProgress()
    {
        double currentTime = timeManager.GetAdjustedServerTime();
        float elapsedTime = (float)(currentTime - cookStartTime.Value);
        return Mathf.Clamp01(elapsedTime / cookDuration.Value);
    }

    private void UpdateVisuals(float progress)
    {
        // Update cooking effects
        if (cookingVFX != null)
        {
            var emission = cookingVFX.emission;
            emission.rateOverTime = Mathf.Lerp(1f, 10f, progress);
        }

        if (cookingSound != null)
        {
            cookingSound.pitch = Mathf.Lerp(0.8f, 1.2f, progress);
        }
    }

    private void OnCookingProgress(float progress)
    {
        // Server-side progress updates
        if (IsServer)
        {
            // Check for cooking milestones
            if (progress >= 0.5f && !hasReachedHalfway)
            {
                OnHalfwayMilestone();
            }
        }
    }

    private void OnCookingComplete()
    {
        if (IsServer)
        {
            isCooking.Value = false;
            CompleteCookingServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CompleteCookingServerRpc()
    {
        // Notify all clients
        CompleteCookingClientRpc();
    }

    [ClientRpc]
    private void CompleteCookingClientRpc()
    {
        // Play completion effects
        PlayCompletionEffects();
    }
}

// TimeUtility.cs - Helper class for common time operations
public static class TimeUtility
{
    public static class Format
    {
        public static string ToMMSS(float timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        public static string ToTimerFormat(float timeInSeconds, bool includeMilliseconds = false)
        {
            if (includeMilliseconds)
            {
                int milliseconds = Mathf.FloorToInt((timeInSeconds * 100f) % 100f);
                return $"{ToMMSS(timeInSeconds)}.{milliseconds:00}";
            }
            return ToMMSS(timeInSeconds);
        }
    }

    public static class Prediction
    {
        public static float GetInterpolationFactor(float previousTime, float currentTime, float targetTime)
        {
            return Mathf.Clamp01((targetTime - previousTime) / (currentTime - previousTime));
        }

        public static Vector3 InterpolatePosition(Vector3 previous, Vector3 current, float factor)
        {
            return Vector3.Lerp(previous, current, factor);
        }

        public static Quaternion InterpolateRotation(Quaternion previous, Quaternion current, float factor)
        {
            return Quaternion.Slerp(previous, current, factor);
        }
    }
}

// TimedAction.cs - Utility class for creating timed actions with prediction
public class TimedAction
{
    private readonly ITimeManager timeManager;
    private readonly float duration;
    private readonly Action<float> onProgress;
    private readonly Action onComplete;
    private double startTime;
    private bool isActive;

    public bool IsActive => isActive;
    public float Progress => GetProgress();

    public TimedAction(ITimeManager timeManager, float duration, Action<float> onProgress = null, Action onComplete = null)
    {
        this.timeManager = timeManager;
        this.duration = duration;
        this.onProgress = onProgress;
        this.onComplete = onComplete;
    }

    public void Start()
    {
        startTime = timeManager.GetAdjustedServerTime();
        isActive = true;
    }

    public void Update()
    {
        if (!isActive) return;

        float progress = GetProgress();
        onProgress?.Invoke(progress);

        if (progress >= 1f)
        {
            Complete();
        }
    }

    public void Cancel()
    {
        isActive = false;
    }

    private void Complete()
    {
        isActive = false;
        onComplete?.Invoke();
    }

    private float GetProgress()
    {
        if (!isActive) return 0f;

        double currentTime = timeManager.GetAdjustedServerTime();
        float elapsedTime = (float)(currentTime - startTime);
        return Mathf.Clamp01(elapsedTime / duration);
    }
}

// Example of a CountdownTimer UI component
public class CountdownTimerUI : MonoBehaviour
{
    [Inject] private ITimeManager timeManager;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image progressFill;

    private NetworkVariable<double> endTime = new NetworkVariable<double>();
    private NetworkVariable<float> duration = new NetworkVariable<float>();

    public void StartCountdown(float countdownDuration)
    {
        if (IsServer)
        {
            duration.Value = countdownDuration;
            endTime.Value = timeManager.GetAdjustedServerTime() + countdownDuration;
        }
    }

    private void Update()
    {
        if (endTime.Value > 0)
        {
            double currentTime = timeManager.GetAdjustedServerTime();
            float remainingTime = Mathf.Max(0f, (float)(endTime.Value - currentTime));

            // Update UI with prediction
            UpdateTimerUI(remainingTime);
        }
    }

    private void UpdateTimerUI(float remainingTime)
    {
        // Update text
        timerText.text = TimeUtility.Format.ToTimerFormat(remainingTime);

        // Update progress
        float progress = 1f - (remainingTime / duration.Value);
        progressFill.fillAmount = progress;

        // Add visual feedback for last few seconds
        if (remainingTime <= 3f)
        {
            timerText.color = Color.red;
            timerText.transform.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 10f) * 0.2f);
        }
    }
}

// Example of using timers for game rounds
public class RoundManager : NetworkBehaviour
{
    [Inject] private ITimeManager timeManager;

    private NetworkVariable<GameState> currentState = new NetworkVariable<GameState>();
    private NetworkVariable<double> roundStartTime = new NetworkVariable<double>();
    private NetworkVariable<float> roundDuration = new NetworkVariable<float>();

    private Timer roundTimer;
    private Timer warmupTimer;

    public enum GameState
    {
        WarmUp,
        Playing,
        RoundEnd
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartWarmup();
        }
    }

    private void StartWarmup()
    {
        currentState.Value = GameState.WarmUp;
        warmupTimer = timeManager.CreateTimer(
            5f, // 5 second warmup
            progress => OnWarmupProgress(progress),
            () => StartRound()
        );
    }

    private void StartRound()
    {
        if (!IsServer) return;

        currentState.Value = GameState.Playing;
        roundStartTime.Value = timeManager.GetAdjustedServerTime();
        roundDuration.Value = 300f; // 5 minute round

        roundTimer = timeManager.CreateTimer(
            roundDuration.Value,
            progress => OnRoundProgress(progress),
            () => EndRound()
        );
    }

    private void OnWarmupProgress(float progress)
    {
        // Update warmup visuals
    }

    private void OnRoundProgress(float progress)
    {
        // Update round progress
        if (progress >= 0.75f)
        {
            // Final quarter of the round
            NotifyFinalStretch();
        }
    }

    private void EndRound()
    {
        if (!IsServer) return;

        currentState.Value = GameState.RoundEnd;
        // Handle round end
    }
}
````

This implementation provides:

1. Robust timer system with prediction
2. Network-synchronized countdowns
3. Time-based action handling
4. Formatting utilities
5. Interpolation helpers
6. Visual feedback systems

The system handles:

- Network latency compensation
- Predictive visual updates
- Smooth progress tracking
- Time synchronization
- State management
- UI feedback

Would you like me to:

1. Add more examples of time-based gameplay mechanics?
2. Show how to implement additional time prediction features?
3. Add debugging tools for time synchronization?
4. Implement more utility functions?
