using System;
using Unity.Netcode;

namespace Core.EventSystem
{
    public interface IEventArgs { }

    public interface INetworkEventArgs : IEventArgs, INetworkSerializable
    {
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class EventTypeAttribute : Attribute
    {
        public string EventName { get; }
        public bool IsReliable { get; }

        public EventTypeAttribute(string eventName, bool isReliable = true)
        {
            EventName = eventName;
            IsReliable = isReliable;
        }
    }
}