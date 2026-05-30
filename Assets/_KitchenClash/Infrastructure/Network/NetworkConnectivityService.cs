using System;
using KitchenClash.Domain;
using UnityEngine;
using VContainer.Unity;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// GDD Section 5: Connectivity state machine.
    /// Online / OfflineMenu (retry 3s) / OfflineMatch (3×5s reconnect, then forfeit) / HostDropped (3s timeout).
    /// </summary>
    public sealed class NetworkConnectivityService : IConnectivityService, ITickable
    {
        // ── Constants ──
        private const float MENU_RETRY_INTERVAL = 3f;
        private const float MATCH_RECONNECT_INTERVAL = 5f;
        private const int MATCH_MAX_RECONNECT_ATTEMPTS = 3;
        private const float HOST_DROPPED_TIMEOUT = 3f;

        // ── State ──
        private bool _prev = true;
        private bool _inMatch;
        private float _timer;
        private int _reconnectAttempts;

        // ── Public ──
        public bool IsOnline { get; private set; } = true;
        public ConnectivityState CurrentState { get; private set; } = ConnectivityState.Online;

        public event Action<bool> OnConnectivityChanged;
        public event Action<bool> OnConnectionStatusChanged;
        public event Action<ConnectivityState> OnStateChanged;

        /// <summary>Published when match reconnect attempts are exhausted → caller should forfeit.</summary>
        public event Action OnMatchForfeit;

        /// <summary>Published when host-dropped timeout expires → caller should end match.</summary>
        public event Action OnHostDroppedTimeout;

        // ── IConnectivityService ──

        public void NotifyMatchStarted()
        {
            _inMatch = true;
            // If already offline, transition to match-offline immediately
            if (!IsOnline)
            {
                TransitionTo(ConnectivityState.OfflineMatch);
                _reconnectAttempts = 0;
                _timer = 0f;
            }
        }

        public void NotifyMatchEnded()
        {
            _inMatch = false;
            if (CurrentState == ConnectivityState.OfflineMatch || CurrentState == ConnectivityState.HostDropped)
            {
                TransitionTo(IsOnline ? ConnectivityState.Online : ConnectivityState.OfflineMenu);
            }
        }

        public void NotifyHostDropped()
        {
            _timer = 0f;
            TransitionTo(ConnectivityState.HostDropped);
        }

        // ── ITickable ──

        void ITickable.Tick()
        {
            bool now = UnityEngine.Application.internetReachability != NetworkReachability.NotReachable;

            // Connectivity edge detection
            if (now != _prev)
            {
                _prev = now;
                IsOnline = now;
                OnConnectivityChanged?.Invoke(now);
                OnConnectionStatusChanged?.Invoke(now);

                if (now)
                {
                    // Came back online
                    TransitionTo(ConnectivityState.Online);
                }
                else
                {
                    // Went offline
                    _timer = 0f;
                    if (_inMatch)
                    {
                        _reconnectAttempts = 0;
                        TransitionTo(ConnectivityState.OfflineMatch);
                    }
                    else
                    {
                        TransitionTo(ConnectivityState.OfflineMenu);
                    }
                }
            }

            // Timer-based logic per state
            float dt = Time.unscaledDeltaTime;

            switch (CurrentState)
            {
                case ConnectivityState.OfflineMenu:
                    _timer += dt;
                    if (_timer >= MENU_RETRY_INTERVAL)
                    {
                        _timer = 0f;
                        // Re-check is done naturally next frame via internetReachability
                        // (auto-dismiss on restore handled by edge detection above)
                    }
                    break;

                case ConnectivityState.OfflineMatch:
                    _timer += dt;
                    if (_timer >= MATCH_RECONNECT_INTERVAL)
                    {
                        _timer = 0f;
                        _reconnectAttempts++;
                        if (_reconnectAttempts >= MATCH_MAX_RECONNECT_ATTEMPTS)
                        {
                            // Exhausted → forfeit
                            OnMatchForfeit?.Invoke();
                            // Return to menu state
                            _inMatch = false;
                            TransitionTo(ConnectivityState.OfflineMenu);
                        }
                    }
                    break;

                case ConnectivityState.HostDropped:
                    _timer += dt;
                    if (_timer >= HOST_DROPPED_TIMEOUT)
                    {
                        OnHostDroppedTimeout?.Invoke();
                        _inMatch = false;
                        TransitionTo(IsOnline ? ConnectivityState.Online : ConnectivityState.OfflineMenu);
                    }
                    break;
            }
        }

        // ── Private ──

        private void TransitionTo(ConnectivityState newState)
        {
            if (CurrentState == newState)
            {
                return;
            }

            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}
