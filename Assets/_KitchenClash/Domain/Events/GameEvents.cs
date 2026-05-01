using System;

namespace KitchenClash.Domain
{
    public sealed class CurrencyChangedEvent
    {
        public int Coins { get; set; }
        public int Gems { get; set; }
    }

    public sealed class CurrencyResetEvent { }

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

    public sealed class StateChangedEvent
    {
        public string PreviousState { get; set; }
        public string CurrentState { get; set; }
    }

    public sealed class LobbyJoinedEvent
    {
        public string LobbyId { get; set; }
    }

    public sealed class LobbyLeftEvent
    {
        public string LobbyId { get; set; }
    }

    public sealed class AudioSettingsChangedEvent
    {
        public float MasterVolume { get; set; }
        public float MusicVolume { get; set; }
        public float SFXVolume { get; set; }
        public bool IsMuted { get; set; }
    }

    public sealed class ScreenShownEvent
    {
        public ScreenId Screen { get; set; }
    }

    public sealed class ScreenHiddenEvent
    {
        public ScreenId Screen { get; set; }
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
}
