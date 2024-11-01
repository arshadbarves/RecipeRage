using System;
using Unity.Netcode;

namespace Core.EventSystem
{
    public interface IEventArgs
    {
    }

    public interface INetworkEventArgs : IEventArgs, INetworkSerializable
    {
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class EventTypeAttribute : Attribute
    {

        public EventTypeAttribute(string eventName, bool isReliable = true)
        {
            EventName = eventName;
            IsReliable = isReliable;
        }
        public string EventName { get; }
        public bool IsReliable { get; }
    }
}