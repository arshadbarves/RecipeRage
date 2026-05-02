using System;

namespace KitchenClash.Domain
{
    public interface IConnectivityService
    {
        bool IsOnline { get; }
        ConnectivityState CurrentState { get; }

        event Action<bool> OnConnectivityChanged;
        event Action<bool> OnConnectionStatusChanged;
        event Action<ConnectivityState> OnStateChanged;

        /// <summary>Transition offline detection to OfflineMatch mode (3 reconnect attempts, then forfeit).</summary>
        void NotifyMatchStarted();

        /// <summary>Transition back to OfflineMenu mode.</summary>
        void NotifyMatchEnded();

        /// <summary>Trigger HostDropped state (3s timeout then end match).</summary>
        void NotifyHostDropped();
    }
}
