using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Utilities;

namespace Core.EventSystem
{
    [CreateAssetMenu(fileName = "EventManagerConfig", menuName = "EventSystem/EventManagerConfig")]
    public class EventManagerConfig : ScriptableObject
    {
        public float networkEventProcessInterval = 0.1f;
        public int maxBatchSize = 10;
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public class EventManager : NetworkSingleton<EventManager>
    {
        [SerializeField] private EventManagerConfig config;

        private readonly SortedDictionary<int, Dictionary<Type, List<Delegate>>> _prioritizedLocalEventCallbacks = 
            new SortedDictionary<int, Dictionary<Type, List<Delegate>>>();
        private readonly SortedDictionary<int, Dictionary<Type, List<Delegate>>> _prioritizedNetworkEventCallbacks = 
            new SortedDictionary<int, Dictionary<Type, List<Delegate>>>();
        private readonly ConcurrentQueue<NetworkEventInfo> _networkEventQueue = new ConcurrentQueue<NetworkEventInfo>();
        private float _lastNetworkEventProcessTime;

        private readonly Dictionary<Type, EventTypeAttribute> _eventTypeCache = new Dictionary<Type, EventTypeAttribute>();

        protected override void Awake()
        {
            base.Awake();
            RegisterEventTypes();
        }

        public void Update()
        {
            if (IsServer && Time.time - _lastNetworkEventProcessTime >= config.networkEventProcessInterval)
            {
                ProcessNetworkEventQueue();
                _lastNetworkEventProcessTime = Time.time;
            }
        }

        private void RegisterEventTypes()
        {
            var eventTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<EventTypeAttribute>() != null);

            foreach (var type in eventTypes)
            {
                var attr = type.GetCustomAttribute<EventTypeAttribute>();
                _eventTypeCache[type] = attr;
            }
        }

        public void AddLocalListener<T>(Action<T> callback, int priority = 0) where T : class
        {
            AddListener(_prioritizedLocalEventCallbacks, typeof(T), callback, priority);
        }

        public void AddNetworkListener<T>(Action<T> callback, int priority = 0) where T : class, INetworkSerializable
        {
            AddListener(_prioritizedNetworkEventCallbacks, typeof(T), callback, priority);
        }

        private void AddListener(SortedDictionary<int, Dictionary<Type, List<Delegate>>> callbacks, Type eventType, Delegate callback, int priority)
        {
            if (!callbacks.ContainsKey(priority))
            {
                callbacks[priority] = new Dictionary<Type, List<Delegate>>();
            }

            if (!callbacks[priority].ContainsKey(eventType))
            {
                callbacks[priority][eventType] = new List<Delegate>();
            }

            callbacks[priority][eventType].Add(callback);
        }

        public void RemoveLocalListener<T>(Action<T> callback) where T : class
        {
            RemoveListener(_prioritizedLocalEventCallbacks, typeof(T), callback);
        }

        public void RemoveNetworkListener<T>(Action<T> callback) where T : class, INetworkSerializable
        {
            RemoveListener(_prioritizedNetworkEventCallbacks, typeof(T), callback);
        }

        private void RemoveListener(SortedDictionary<int, Dictionary<Type, List<Delegate>>> callbacks, Type eventType, Delegate callback)
        {
            foreach (var priorityLevel in callbacks)
            {
                if (priorityLevel.Value.TryGetValue(eventType, out var eventCallbacks))
                {
                    eventCallbacks.Remove(callback);
                }
            }
        }

        public void TriggerLocalEvent<T>(T args) where T : class
        {
            TriggerEvent(_prioritizedLocalEventCallbacks, typeof(T), args);
        }

        public async Task TriggerLocalEventAsync<T>(T args) where T : class
        {
            await TriggerEventAsync(_prioritizedLocalEventCallbacks, typeof(T), args);
        }

        public void TriggerNetworkEvent<T>(T args, ulong? targetClientId = null) where T : class, INetworkSerializable
        {
            if (IsServer)
            {
                var eventInfo = new NetworkEventInfo<T>
                {
                    EventArgs = args,
                    TargetClientId = targetClientId,
                    IsReliable = GetEventReliability<T>()
                };
                _networkEventQueue.Enqueue(eventInfo);
            }
            else
            {
                Debug.LogWarning("Attempting to trigger a network event from a client. This is not allowed.");
            }
        }

        private void TriggerEvent<T>(SortedDictionary<int, Dictionary<Type, List<Delegate>>> callbacks, Type eventType, T args)
        {
            foreach (var priorityLevel in callbacks)
            {
                if (priorityLevel.Value.TryGetValue(eventType, out var eventCallbacks))
                {
                    foreach (var callback in eventCallbacks)
                    {
                        (callback as Action<T>)?.Invoke(args);
                    }
                }
            }
        }

        private async Task TriggerEventAsync<T>(SortedDictionary<int, Dictionary<Type, List<Delegate>>> callbacks, Type eventType, T args)
        {
            foreach (var priorityLevel in callbacks)
            {
                if (priorityLevel.Value.TryGetValue(eventType, out var eventCallbacks))
                {
                    var tasks = eventCallbacks.Select(callback => Task.Run(() => (callback as Action<T>)?.Invoke(args)));
                    await Task.WhenAll(tasks);
                }
            }
        }

        private void ProcessNetworkEventQueue()
        {
            var batch = new NetworkEventBatch();
            while (_networkEventQueue.TryDequeue(out NetworkEventInfo eventInfo) && batch.Events.Count < config.maxBatchSize)
            {
                batch.Events.Add(new SerializableNetworkEventInfo
                {
                    EventTypeName = eventInfo.GetType().AssemblyQualifiedName,
                    EventArgs = eventInfo.GetNetworkVariable().Value,
                    TargetClientId = eventInfo.TargetClientId,
                    IsReliable = eventInfo.IsReliable
                });
            }

            if (batch.Events.Count > 0)
            {
                TriggerNetworkEventBatchClientRpc(batch);
            }
        }

        [ClientRpc]
        private void TriggerNetworkEventBatchClientRpc(NetworkEventBatch eventBatch)
        {
            foreach (var eventInfo in eventBatch.Events)
            {
                var args = eventInfo.EventArgs;
                var eventType = Type.GetType(eventInfo.EventTypeName);
                if (eventType != null)
                {
                    TriggerEvent(_prioritizedNetworkEventCallbacks, eventType, args);
                }
                else
                {
                    Debug.LogError($"Failed to find type {eventInfo.EventTypeName}");
                }
            }
        }

        public bool GetEventReliability<T>() where T : class, INetworkSerializable
        {
            if (_eventTypeCache.TryGetValue(typeof(T), out var attr))
            {
                return attr.IsReliable;
            }
            return true; // Default to reliable if not specified
        }

        public void SetConfig(EventManagerConfig eventManagerConfig)
        {
            config = eventManagerConfig;
        }

        public EventTypeAttribute GetEventTypeAttribute(Type eventType)
        {
            return _eventTypeCache.GetValueOrDefault(eventType);
        }
    }

    public struct NetworkEventBatch : INetworkSerializable
    {
        public List<SerializableNetworkEventInfo> Events;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                int count = 0;
                serializer.SerializeValue(ref count);
                Events = new List<SerializableNetworkEventInfo>(count);
                for (int i = 0; i < count; i++)
                {
                    SerializableNetworkEventInfo eventInfo = new SerializableNetworkEventInfo();
                    eventInfo.NetworkSerialize(serializer);
                    Events.Add(eventInfo);
                }
            }
            else
            {
                int count = Events.Count;
                serializer.SerializeValue(ref count);
                for (int i = 0; i < count; i++)
                {
                    Events[i].NetworkSerialize(serializer);
                }
            }
        }
    }

    public struct SerializableNetworkEventInfo : INetworkSerializable
    {
        public string EventTypeName;
        public INetworkSerializable EventArgs;
        public ulong? TargetClientId;
        public bool IsReliable;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref EventTypeName);
            
            if (serializer.IsReader)
            {
                Type eventType = Type.GetType(EventTypeName);
                if (eventType != null)
                {
                    EventArgs = (INetworkSerializable)Activator.CreateInstance(eventType);
                }
                else
                {
                    Debug.LogError($"Failed to find type {EventTypeName}");
                    return;
                }
            }

            EventArgs.NetworkSerialize(serializer);
            ulong tempTargetClientId = TargetClientId ?? 0;
            serializer.SerializeValue(ref tempTargetClientId);
            TargetClientId = tempTargetClientId == 0 ? null : tempTargetClientId;
            serializer.SerializeValue(ref IsReliable);
        }
    }

    public abstract class NetworkEventInfo
    {
        public ulong? TargetClientId;
        public bool IsReliable;

        public abstract NetworkVariable<INetworkSerializable> GetNetworkVariable();
    }

    public class NetworkEventInfo<T> : NetworkEventInfo where T : class, INetworkSerializable
    {
        public T EventArgs;

        public override NetworkVariable<INetworkSerializable> GetNetworkVariable()
        {
            return new NetworkVariable<INetworkSerializable>(EventArgs);
        }
    }
}