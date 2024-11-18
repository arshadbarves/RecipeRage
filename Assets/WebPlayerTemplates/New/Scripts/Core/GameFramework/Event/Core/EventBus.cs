using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Core.GameFramework.Event.Core
{
    public class EventBus : NetworkBehaviour
    {

        private const int MaxEventsPerFrame = 100;
        private readonly Dictionary<Type, HashSet<IEventHandler>> _handlers =
            new Dictionary<Type, HashSet<IEventHandler>>();

        private readonly Queue<IGameEvent> _localEventQueue = new Queue<IGameEvent>();
        private readonly Queue<IGameEvent> _networkEventQueue = new Queue<IGameEvent>();

        private void Update()
        {
            ProcessEventQueues();
        }

        public void Subscribe<T>(IEventHandler handler) where T : IGameEvent
        {
            Type eventType = typeof(T);
            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new HashSet<IEventHandler>();
            }
            _handlers[eventType].Add(handler);
        }

        public void Unsubscribe<T>(IEventHandler handler) where T : IGameEvent
        {
            Type eventType = typeof(T);
            if (_handlers.ContainsKey(eventType))
            {
                _handlers[eventType].Remove(handler);
            }
        }

        public void Publish(IGameEvent gameEvent)
        {
            if (gameEvent.IsNetworked)
            {
                if (gameEvent is not INetworkedGameEvent networkedEvent)
                {
                    Debug.LogError($"Event {gameEvent.GetType().Name} is marked as networked but doesn't implement INetworkedGameEvent");
                    return;
                }

                if (IsSpawned)
                {
                    if (IsServer)
                    {
                        // PropagateNetworkEventClientRpc(networkedEvent);
                    }
                    // PropagateNetworkEventServerRpc(networkedEvent);
                }
            }
            else
            {
                _localEventQueue.Enqueue(gameEvent);
            }
        }

        private void ProcessEventQueues()
        {
            int processedEvents = 0;

            while (_localEventQueue.Count > 0 && processedEvents < MaxEventsPerFrame)
            {
                IGameEvent gameEvent = _localEventQueue.Dequeue();
                ProcessEvent(gameEvent);
                processedEvents++;
            }

            while (_networkEventQueue.Count > 0 && processedEvents < MaxEventsPerFrame)
            {
                IGameEvent gameEvent = _networkEventQueue.Dequeue();
                ProcessEvent(gameEvent);
                processedEvents++;
            }
        }

        private void ProcessEvent(IGameEvent gameEvent)
        {
            Type eventType = gameEvent.GetType();
            if (_handlers.TryGetValue(eventType, out HashSet<IEventHandler> eventHandlers))
            {
                foreach (IEventHandler handler in eventHandlers)
                {
                    try
                    {
                        handler.HandleEvent(gameEvent);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error handling event {eventType.Name}: {e}");
                    }
                }
            }
        }

        // [ClientRpc]
        // private void PropagateNetworkEventClientRpc<T>(T networkEvent) where T : INetworkedGameEvent
        // {
        //     if (!IsServer) // Don't process on server as it's already processed
        //     {
        //         _networkEventQueue.Enqueue(networkEvent);
        //     }
        // }
        //
        // [ServerRpc(RequireOwnership = false)]
        // private void PropagateNetworkEventServerRpc<T>(T networkEvent) where T : INetworkedGameEvent
        // {
        //     // Process on server
        //     _networkEventQueue.Enqueue(networkEvent);
        //     // Propagate to all clients
        //     PropagateNetworkEventClientRpc(networkEvent);
        // }
    }
}