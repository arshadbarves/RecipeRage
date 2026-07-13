using System;

namespace KitchenClash.Domain
{
    public sealed class CurrencyChangedEvent
    {
        public int Coins { get; set; }
        public int Gems { get; set; }
    }

    public sealed class LoginSuccessEvent
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
    }

    public sealed class LoginFailedEvent
    {
        public string Error { get; set; }
    }

    public sealed class LogoutEvent
    {
        public string UserId { get; set; }
    }

    public sealed class MaintenanceModeEvent
    {
        public bool IsMaintenanceMode { get; set; }
        public string EstimatedEndTime { get; set; }
        public string Message { get; set; }
        public bool AllowRetry { get; set; }
    }

    public sealed class ForceUpdateEvent
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRequired { get; set; }
    }

    public sealed class CameraShakeEvent
    {
        public float Intensity { get; set; }
        public float Duration { get; set; }

        public CameraShakeEvent(float intensity, float duration)
        {
            Intensity = intensity;
            Duration = duration;
        }
    }

    public sealed class LocalPlayerSpawnedEvent
    {
        /// <summary>Player transform (object to avoid Unity ref in Domain). Cast to Transform.</summary>
        public object PlayerTransform { get; set; }
        /// <summary>Player game object (object to avoid Unity ref in Domain). Cast to GameObject.</summary>
        public object PlayerObject { get; set; }
    }

    public sealed class LocalPlayerDespawnedEvent { }

    public sealed class MaintenanceCheckFailedEvent
    {
        public string Error { get; set; }
    }

    public sealed class ItemPurchasedEvent
    {
        public string ItemId { get; set; }
        public int Cost { get; set; }
        public string CurrencyType { get; set; }
    }

    public sealed class MatchRewardEvent
    {
        public int CoinsAwarded { get; set; }
        public bool Won { get; set; }
        public int Score { get; set; }
    }

    public sealed class DailyRewardClaimedEvent
    {
        public DailyStreakReward Reward { get; set; }
    }

    public sealed class CrateRewardEvent
    {
        public string CrateType { get; set; }
        public int Amount { get; set; }
    }

    public sealed class ChefTrialEvent
    {
        public int DurationHours { get; set; }
    }

    public sealed class BattlePassXpTokenEvent
    {
        public int Amount { get; set; }
    }

    /// <summary>
    /// Published when the account-upgrade prompt finishes (linked or continue-as-guest).
    /// AccountUpgradeState listens and routes to MainMenuState.
    /// </summary>
    public sealed class AccountUpgradeResultEvent
    {
        public bool Linked { get; }
        public string Provider { get; }
        public string ProductUserId { get; }

        public AccountUpgradeResultEvent(bool linked, string provider, string productUserId)
        {
            Linked = linked;
            Provider = provider;
            ProductUserId = productUserId;
        }
    }

    /// <summary>
    /// Published when a match ends so session-scoped handlers (e.g. MatchRewardHandler)
    /// can award economy rewards without GameOverState depending on session services.
    /// </summary>
    public sealed class MatchEndedEvent
    {
        public bool Won { get; }
        public int LocalTeamScore { get; }

        public MatchEndedEvent(bool won, int localTeamScore)
        {
            Won = won;
            LocalTeamScore = localTeamScore;
        }
    }

    /// <summary>
    /// Published when the player taps Retry on the no-connection popup.
    /// NoConnectionState restarts SessionLoadingState on receipt.
    /// </summary>
    public sealed class RetryConnectionEvent
    {
    }

    /// <summary>
    /// Published during long-running loads (tutorial kitchen, map load) so UI can show progress.
    /// </summary>
    public sealed class LoadingProgressEvent
    {
        public float Progress { get; }
        public string Message { get; }

        public LoadingProgressEvent(float progress, string message = null)
        {
            Progress = progress;
            Message = message;
        }
    }
}
