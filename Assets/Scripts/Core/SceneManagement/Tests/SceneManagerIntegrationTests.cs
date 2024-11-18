using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace Core.SceneManagement.Tests
{
    [TestFixture]
    public class SceneManagerIntegrationTests
    {
        protected const int NumberOfClients = 2;
        protected const ushort PORT = 7778;

        private NetworkManager[] _clientManagers;
        private NetworkManager _hostManager;
        private NetworkSceneManager[] _sceneManagers;
        private GameObject[] _sceneManagerGOs;
        private SceneData _testSceneData;

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

            _sceneManagers = new NetworkSceneManager[NumberOfClients + 1];
            _sceneManagerGOs = new GameObject[NumberOfClients + 1];

            // Setup host scene manager
            _sceneManagerGOs[0] = CreateSceneManagerGO("HostSceneManager", _hostManager);
            _sceneManagers[0] = _sceneManagerGOs[0].GetComponent<NetworkSceneManager>();

            // Setup client scene managers
            for (int i = 0; i < NumberOfClients; i++)
            {
                _sceneManagerGOs[i + 1] = CreateSceneManagerGO($"ClientSceneManager_{i}", _clientManagers[i]);
                _sceneManagers[i + 1] = _sceneManagerGOs[i + 1].GetComponent<NetworkSceneManager>();
            }

            // Create test SceneData
            _testSceneData = ScriptableObject.CreateInstance<SceneData>();
            _testSceneData.sceneName = "TestScene";
            _testSceneData.networkTimeout = 5f;
            _testSceneData.requiresNetworkSync = true;
            _testSceneData.dependencies = new string[0];

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

        private GameObject CreateSceneManagerGO(string name, NetworkManager manager)
        {
            var go = new GameObject(name);
            var sceneManager = go.AddComponent<NetworkSceneManager>();
            var netObj = go.AddComponent<NetworkObject>();
            
            manager.NetworkConfig.Prefabs.Add(new NetworkPrefab { 
                Prefab = go
            });
            
            return go;
        }

        private IEnumerator StartNetworking()
        {
            _hostManager.StartHost();
            yield return new WaitForSeconds(0.2f);

            foreach (var client in _clientManagers)
            {
                client.StartClient();
                yield return new WaitForSeconds(0.1f);
            }

            yield return WaitForClientsConnected();
            
            foreach (var sceneManagerGO in _sceneManagerGOs)
            {
                var netObj = sceneManagerGO.GetComponent<NetworkObject>();
                if (!netObj.IsSpawned)
                {
                    netObj.Spawn();
                }
            }

            yield return new WaitForSeconds(0.5f);
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
        public IEnumerator TestSingleSceneLoad()
        {
            bool completed = false;
            string errorMessage = null;

            _sceneManagers[0].OnSceneLoadCompleted += (sceneName) => completed = true;

            yield return _sceneManagers[0].LoadSceneAsync(_testSceneData);

            Assert.IsTrue(completed, "Scene load did not complete");
            Assert.IsNull(errorMessage, "Scene load encountered an error");
        }

        [UnityTest]
        public IEnumerator TestSceneLoadWithDependencies()
        {
            // Create test scene with dependencies
            var dependencyScene = ScriptableObject.CreateInstance<SceneData>();
            dependencyScene.sceneName = "DependencyScene";
            dependencyScene.networkTimeout = 5f;
            dependencyScene.requiresNetworkSync = true;

            _testSceneData.dependencies = new[] { "DependencyScene" };

            int loadedScenes = 0;
            _sceneManagers[0].OnSceneLoadCompleted += (sceneName) => loadedScenes++;

            yield return _sceneManagers[0].LoadSceneAsync(_testSceneData);

            Assert.AreEqual(2, loadedScenes, "Not all scenes were loaded");
        }

        [UnityTest]
        public IEnumerator TestSceneLoadTimeout()
        {
            // Set very short timeout
            _testSceneData.networkTimeout = 0.1f;

            bool completed = false;
            _sceneManagers[0].OnSceneLoadCompleted += (sceneName) => completed = true;

            yield return _sceneManagers[0].LoadSceneAsync(_testSceneData);

            Assert.IsFalse(completed, "Scene load should have timed out");
        }

        [UnityTest]
        public IEnumerator TestClientDisconnectDuringLoad()
        {
            bool completed = false;
            _sceneManagers[0].OnSceneLoadCompleted += (sceneName) => completed = true;

            // Start loading
            var loadOperation = _sceneManagers[0].LoadSceneAsync(_testSceneData);
            
            // Disconnect a client
            _clientManagers[0].Shutdown();
            
            yield return loadOperation;

            Assert.IsTrue(completed, "Scene load should complete despite client disconnect");
        }

        [UnityTest]
        public IEnumerator TestMultipleSceneLoads()
        {
            var scenes = new List<SceneData>();
            for (int i = 0; i < 3; i++)
            {
                var sceneData = ScriptableObject.CreateInstance<SceneData>();
                sceneData.sceneName = $"TestScene_{i}";
                sceneData.networkTimeout = 5f;
                sceneData.requiresNetworkSync = true;
                scenes.Add(sceneData);
            }

            int loadedScenes = 0;
            _sceneManagers[0].OnSceneLoadCompleted += (sceneName) => loadedScenes++;

            foreach (var scene in scenes)
            {
                yield return _sceneManagers[0].LoadSceneAsync(scene);
            }

            Assert.AreEqual(scenes.Count, loadedScenes, "Not all scenes were loaded");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var sceneManager in _sceneManagers.Where(sm => sm != null))
            {
                if (sceneManager.NetworkObject.IsSpawned)
                {
                    sceneManager.NetworkObject.Despawn();
                }
            }

            foreach (var go in _sceneManagerGOs.Where(go => go != null))
            {
                Object.Destroy(go);
            }

            if (_hostManager != null)
            {
                Object.Destroy(_hostManager.gameObject);
            }

            foreach (var client in _clientManagers.Where(cm => cm != null))
            {
                Object.Destroy(client.gameObject);
            }

            Object.Destroy(_testSceneData);

            _sceneManagers = null;
            _sceneManagerGOs = null;
            _clientManagers = null;
            _hostManager = null;
            _testSceneData = null;

            yield return new WaitForSeconds(0.1f);
        }
    }
}