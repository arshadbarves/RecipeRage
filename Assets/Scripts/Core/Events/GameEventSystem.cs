using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// Game event system handling both local and networked events for Recipe Rage.
    /// Provides efficient event handling with network synchronization and local event optimization.
    /// </summary>
    public class GameEventSystem : NetworkBehaviour
    {
        private static GameEventSystem _instance;
        private readonly Dictionary<Type, HashSet<IEventListener>> _listeners = new();
        private readonly Queue<EventData> _eventQueue = new();
        private const int MaxEventsPerFrame = 100;

        public static GameEventSystem Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            ProcessEventQueue();
        }

        private void ProcessEventQueue()
        {
            int processedEvents = 0;
            while (_eventQueue.Count > 0 && processedEvents < MaxEventsPerFrame)
            {
                EventData eventData = _eventQueue.Dequeue();
                ProcessEvent(eventData);
                processedEvents++;
            }
        }

        #region Event Registration

        public void AddListener<T>(IEventListener listener) where T : IEvent
        {
            Type eventType = typeof(T);
            if (!_listeners.ContainsKey(eventType))
            {
                _listeners[eventType] = new HashSet<IEventListener>();
            }
            _listeners[eventType].Add(listener);
        }

        public void RemoveListener<T>(IEventListener listener) where T : IEvent
        {
            Type eventType = typeof(T);
            if (_listeners.ContainsKey(eventType))
            {
                _listeners[eventType].Remove(listener);
                if (_listeners[eventType].Count == 0)
                {
                    _listeners.Remove(eventType);
                }
            }
        }

        public void RemoveAllListeners(IEventListener listener)
        {
            foreach (HashSet<IEventListener> listeners in _listeners.Values)
            {
                listeners.Remove(listener);
            }
        }

        #endregion

        #region Event Raising

        public void RaiseEvent<T>(T gameEvent) where T : IEvent
        {
            EventData eventData = new()
            {
                EventType = typeof(T),
                Event = gameEvent
            };

            // Handle local events
            if (gameEvent is ILocalEvent localEvent)
            {
                if (localEvent.ImmediateExecution)
                {
                    ProcessEvent(eventData);
                }
                else
                {
                    _eventQueue.Enqueue(eventData);
                }
                return;
            }

            // Handle network events
            if (gameEvent is INetworkEvent networkEvent)
            {
                if (!IsSpawned)
                {
                    Debug.LogError("GameEventSystem not spawned yet!");
                    return;
                }

                NetworkEventData netEventData = new()
                {
                    EventType = typeof(T).AssemblyQualifiedName,
                    EventData = JsonUtility.ToJson(networkEvent),
                    Target = networkEvent.Target,
                    SenderId = NetworkManager.LocalClientId
                };

                switch (networkEvent.Target)
                {
                    case EventTarget.All:
                        RaiseEventClientRpc(netEventData);
                        break;
                    case EventTarget.Server:
                        if (!IsServer)
                        {
                            RaiseEventServerRpc(netEventData);
                        }
                        else
                        {
                            ProcessNetworkEvent(netEventData);
                        }
                        break;
                    case EventTarget.Owner:
                        if (NetworkObject.OwnerClientId != NetworkManager.LocalClientId)
                        {
                            RaiseEventServerRpc(netEventData);
                        }
                        else
                        {
                            ProcessNetworkEvent(netEventData);
                        }
                        break;
                    case EventTarget.Others:
                        RaiseEventClientRpc(netEventData);
                        break;
                }
            }
        }

        [ClientRpc]
        private void RaiseEventClientRpc(NetworkEventData eventData)
        {
            if (eventData.SenderId != NetworkManager.LocalClientId)
            {
                ProcessNetworkEvent(eventData);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RaiseEventServerRpc(NetworkEventData eventData)
        {
            ProcessNetworkEvent(eventData);
            if (eventData.Target == EventTarget.All)
            {
                RaiseEventClientRpc(eventData);
            }
        }

        private void ProcessNetworkEvent(NetworkEventData eventData)
        {
            try
            {
                Type eventType = Type.GetType(eventData.EventType);
                if (eventType == null)
                {
                    Debug.LogError($"Unknown event type: {eventData.EventType}");
                    return;
                }

                INetworkEvent networkEvent = (INetworkEvent)JsonUtility.FromJson(eventData.EventData, eventType);
                
                EventData localEventData = new()
                {
                    EventType = eventType,
                    Event = networkEvent
                };

                ProcessEvent(localEventData);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error processing network event: {e}");
            }
        }

        private void ProcessEvent(EventData eventData)
        {
            try
            {
                if (!_listeners.TryGetValue(eventData.EventType, out HashSet<IEventListener> listeners))
                {
                    return;
                }

                foreach (IEventListener listener in listeners)
                {
                    try
                    {
                        listener.OnEventRaised(eventData.Event);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error processing event in listener: {e}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error processing event: {e}");
            }
        }

        #endregion

        public override void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
            _listeners.Clear();
            _eventQueue.Clear();
            base.OnDestroy();
        }
    }

    #region Supporting Types

    public interface IEvent { }

    public interface ILocalEvent : IEvent
    {
        bool ImmediateExecution { get; }
    }

    public interface INetworkEvent : IEvent
    {
        EventTarget Target { get; }
    }

    public interface IEventListener
    {
        void OnEventRaised(IEvent gameEvent);
    }

    public enum EventTarget
    {
        All,        // Send to all clients
        Server,     // Send to server only
        Owner,      // Send to object owner
        Others      // Send to all except sender
    }

    internal struct EventData
    {
        public Type EventType;
        public IEvent Event;
    }

    public struct NetworkEventData : INetworkSerializable
    {
        public string EventType;
        public string EventData;
        public EventTarget Target;
        public ulong SenderId;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref EventType);
            serializer.SerializeValue(ref EventData);
            serializer.SerializeValue(ref Target);
            serializer.SerializeValue(ref SenderId);
        }
    }

    #endregion
}
