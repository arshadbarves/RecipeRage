using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace Core.Events.Tests
{
    [TestFixture]
    public class EventSystemIntegrationTests
    {
        protected const int NumberOfClients = 2;
        protected const ushort PORT = 7777;

        private NetworkManager[] _clientManagers;
        private NetworkManager _hostManager;
        private GameEventSystem[] _eventSystems;
        private GameObject[] _eventSystemGOs;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            // Create and setup NetworkManager instances
            _hostManager = CreateNetworkManager(PORT);
            _clientManagers = new NetworkManager[NumberOfClients];
            for (int i = 0; i < NumberOfClients; i++)
            {
                _clientManagers[i] = CreateNetworkManager(PORT);
            }

            _eventSystems = new GameEventSystem[NumberOfClients + 1];
            _eventSystemGOs = new GameObject[NumberOfClients + 1];

            // Setup host event system
            _eventSystemGOs[0] = CreateEventSystemGO("HostEventSystem", _hostManager);
            _eventSystems[0] = _eventSystemGOs[0].GetComponent<GameEventSystem>();

            // Setup client event systems
            for (int i = 0; i < NumberOfClients; i++)
            {
                _eventSystemGOs[i + 1] = CreateEventSystemGO($"ClientEventSystem_{i}", _clientManagers[i]);
                _eventSystems[i + 1] = _eventSystemGOs[i + 1].GetComponent<GameEventSystem>();
            }

            yield return StartNetworking();
        }

        private NetworkManager CreateNetworkManager(ushort port)
        {
            var go = new GameObject("NetworkManager");
            var networkManager = go.AddComponent<NetworkManager>();
            var transport = go.AddComponent<UnityTransport>();
            
            // Configure transport
            transport.ConnectionData = new UnityTransport.ConnectionAddressData
            {
                Address = "127.0.0.1",
                Port = port,
                ServerListenAddress = "0.0.0.0"
            };

            // Set transport on NetworkManager
            networkManager.NetworkConfig = new NetworkConfig();
            networkManager.NetworkConfig.NetworkTransport = transport;
            networkManager.NetworkConfig.ConnectionApproval = false;
            networkManager.NetworkConfig.ConnectionData = new byte[0];
            networkManager.NetworkConfig.EnableSceneManagement = true;
            networkManager.NetworkConfig.ForceSamePrefabs = true;
            networkManager.NetworkConfig.RecycleNetworkIds = true;
            networkManager.NetworkConfig.Prefabs = new NetworkPrefabs();

            return networkManager;
        }

        private GameObject CreateEventSystemGO(string name, NetworkManager manager)
        {
            GameObject go = new GameObject(name);
            GameEventSystem eventSystem = go.AddComponent<GameEventSystem>();
            NetworkObject netObj = go.AddComponent<NetworkObject>();

            // Register prefab with NetworkManager
            manager.NetworkConfig.Prefabs.Add(new NetworkPrefab {
                Prefab = go
            });

            return go;
        }

        private IEnumerator StartNetworking()
        {
            // Start host
            _hostManager.StartHost();
            yield return new WaitForSeconds(0.2f);

            // Start clients
            foreach (NetworkManager client in _clientManagers)
            {
                client.StartClient();
                yield return new WaitForSeconds(0.1f);
            }

            yield return WaitForClientsConnected();

            // Spawn event systems
            foreach (GameObject eventSystemGO in _eventSystemGOs)
            {
                NetworkObject netObj = eventSystemGO.GetComponent<NetworkObject>();
                if (!netObj.IsSpawned)
                {
                    netObj.Spawn();
                }
            }

            yield return new WaitForSeconds(0.5f); // Wait for spawning to complete
        }

        private IEnumerator WaitForClientsConnected()
        {
            float timeout = Time.time + 10f;
            while (Time.time < timeout)
            {
                if (_clientManagers.All(cm => cm.IsConnectedClient))
                {
                    yield break;
                }
                yield return new WaitForSeconds(0.1f);
            }
            Assert.Fail("Timeout waiting for clients to connect");
        }

        [UnityTest]
        public IEnumerator TestLocalEventDelivery()
        {
            TestLocalEvent testEvent = new TestLocalEvent {
                Value = "Test"
            };
            TestEventListener listener = new TestEventListener();
            _eventSystems[0].AddListener<TestLocalEvent>(listener);

            yield return null; // Wait for listener registration

            _eventSystems[0].RaiseEvent(testEvent);

            yield return WaitForEventReceived(listener);

            Assert.AreEqual(1, listener.ReceivedEvents.Count, "Event was not received");
            TestLocalEvent receivedEvent = (TestLocalEvent)listener.ReceivedEvents[0];
            Assert.AreEqual("Test", receivedEvent.Value, "Event value mismatch");
        }

        [UnityTest]
        public IEnumerator TestNetworkEventDelivery()
        {
            TestNetworkEvent testEvent = new TestNetworkEvent {
                Value = "NetworkTest"
            };
            TestEventListener[] listeners = new TestEventListener[NumberOfClients + 1];

            // Setup listeners
            for (int i = 0; i < NumberOfClients + 1; i++)
            {
                listeners[i] = new TestEventListener();
                _eventSystems[i].AddListener<TestNetworkEvent>(listeners[i]);
            }

            yield return null; // Wait for listener registration

            // Raise event from host
            _eventSystems[0].RaiseEvent(testEvent);

            yield return WaitForAllEventsReceived(listeners);

            // Verify all clients received the event
            foreach (TestEventListener listener in listeners)
            {
                Assert.AreEqual(1, listener.ReceivedEvents.Count, "Event count mismatch");
                TestNetworkEvent receivedEvent = (TestNetworkEvent)listener.ReceivedEvents[0];
                Assert.AreEqual("NetworkTest", receivedEvent.Value, "Event value mismatch");
            }
        }

        [UnityTest]
        public IEnumerator TestEventTargeting()
        {
            TestTargetedEvent serverOnlyEvent = new TestTargetedEvent {
                Value = "ServerOnly", Target = EventTarget.Server
            };

            TestEventListener[] listeners = new TestEventListener[NumberOfClients + 1];
            for (int i = 0; i < NumberOfClients + 1; i++)
            {
                listeners[i] = new TestEventListener();
                _eventSystems[i].AddListener<TestTargetedEvent>(listeners[i]);
            }

            yield return null; // Wait for listener registration

            // Raise event from client
            _eventSystems[1].RaiseEvent(serverOnlyEvent);

            yield return WaitForEventReceived(listeners[0]); // Wait for server to receive

            // Verify only server received the event
            Assert.AreEqual(1, listeners[0].ReceivedEvents.Count, "Server did not receive the event");
            TestTargetedEvent receivedEvent = (TestTargetedEvent)listeners[0].ReceivedEvents[0];
            Assert.AreEqual("ServerOnly", receivedEvent.Value, "Server received incorrect event value");

            // Verify clients did not receive the event
            for (int i = 1; i < listeners.Length; i++)
            {
                Assert.AreEqual(0, listeners[i].ReceivedEvents.Count, $"Client {i} incorrectly received event");
            }
        }

        private IEnumerator WaitForEventReceived(TestEventListener listener, float timeout = 2f)
        {
            float endTime = Time.time + timeout;
            while (Time.time < endTime && listener.ReceivedEvents.Count == 0)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        private IEnumerator WaitForAllEventsReceived(TestEventListener[] listeners, float timeout = 2f)
        {
            float endTime = Time.time + timeout;
            while (Time.time < endTime && !listeners.All(l => l.ReceivedEvents.Count > 0))
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // Cleanup event systems
            foreach (GameEventSystem eventSystem in _eventSystems.Where(es => es != null))
            {
                if (eventSystem.NetworkObject.IsSpawned)
                {
                    eventSystem.NetworkObject.Despawn();
                }
            }

            // Cleanup GameObjects
            foreach (GameObject go in _eventSystemGOs.Where(go => go != null))
            {
                Object.Destroy(go);
            }

            // Cleanup NetworkManagers
            if (_hostManager != null)
            {
                Object.Destroy(_hostManager.gameObject);
            }

            foreach (NetworkManager client in _clientManagers.Where(cm => cm != null))
            {
                Object.Destroy(client.gameObject);
            }

            _eventSystems = null;
            _eventSystemGOs = null;
            _clientManagers = null;
            _hostManager = null;

            yield return new WaitForSeconds(0.1f); // Wait for cleanup
        }

        private struct TestLocalEvent : ILocalEvent
        {
            public string Value;
            public bool ImmediateExecution => false;
        }

        private struct TestNetworkEvent : INetworkEvent
        {
            public string Value;
            public EventTarget Target => EventTarget.All;
        }

        private struct TestTargetedEvent : INetworkEvent
        {
            public string Value;
            public EventTarget Target { get; set; }
        }

        private class TestEventListener : IEventListener
        {
            public readonly List<IEvent> ReceivedEvents = new List<IEvent>();

            public void OnEventRaised(IEvent gameEvent)
            {
                ReceivedEvents.Add(gameEvent);
            }
        }
    }
}