using System;
using KitchenClash.Domain;
using UnityEngine;
using VContainer.Unity;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// GDD Section 5: Pure C# ITickable, monitors network every frame.
    /// No MonoBehaviour.
    /// </summary>
    public sealed class NetworkConnectivityService : IConnectivityService, ITickable
    {
        private bool _prev = true;
        public bool IsOnline { get; private set; } = true;
        public event Action<bool> OnConnectivityChanged;
        public event Action<bool> OnConnectionStatusChanged;

        void ITickable.Tick()
        {
            bool now = UnityEngine.Application.internetReachability != NetworkReachability.NotReachable;
            if (now == _prev) return;
            _prev = now;
            IsOnline = now;
            OnConnectivityChanged?.Invoke(now);
            OnConnectionStatusChanged?.Invoke(now);
        }
    }
}
