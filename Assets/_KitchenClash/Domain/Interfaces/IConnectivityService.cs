using System;

namespace KitchenClash.Domain
{
    public interface IConnectivityService
    {
        bool IsOnline { get; }
        event Action<bool> OnConnectivityChanged;
        event Action<bool> OnConnectionStatusChanged;
    }
}
