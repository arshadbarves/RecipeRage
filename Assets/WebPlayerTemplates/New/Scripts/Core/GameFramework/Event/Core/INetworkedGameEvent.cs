using Unity.Netcode;

namespace Core.GameFramework.Event.Core
{
    public interface INetworkedGameEvent : IGameEvent, INetworkSerializable
    {
    }
}