// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using NUnit.Framework;
// using UnityEngine;
// using UnityEngine.TestTools;
// using Unity.Netcode;
// using Unity.Netcode.Transports.UTP;
//
// namespace Core.Pooling.Tests
// {
//     [TestFixture]
//     public class NetworkObjectPoolIntegrationTests
//     {
//         protected const int NumberOfClients = 2;
//         protected const int TestPoolSize = 10;
//         protected const ushort PORT = 7779;
//
//         private NetworkManager[] _clientManagers;
//         private NetworkManager _hostManager;
//         private NetworkObjectPool[] _objectPools;
//         private GameObject[] _poolGOs;
//         private GameObject _testPrefab;
//         private List<NetworkObject> _spawnedObjects;
//
//         [UnitySetUp]
//         public IEnumerator Setup()
//         {
//             _spawnedObjects = new List<NetworkObject>();
//
//             // Create test prefab
//             _testPrefab = new GameObject("TestPrefab");
//             var netObj = _testPrefab.AddComponent<NetworkObject>();
//             _testPrefab.AddComponent<PooledNetworkObject>();
//
//             // Create and setup NetworkManager instances
//             _hostManager = CreateNetworkManager(PORT);
//             _clientManagers = new NetworkManager[NumberOfClients];
//             for (int i = 0; i < NumberOfClients; i++)
//             {
//                 _clientManagers[i] = CreateNetworkManager(PORT);
//             }
//
//             _objectPools = new NetworkObjectPool[NumberOfClients + 1];
//             _poolGOs = new GameObject[NumberOfClients + 1];
//
//             // Setup host pool
//             _poolGOs[0] = CreatePoolGO("HostPool", _hostManager);
//             _objectPools[0] = _poolGOs[0].GetComponent<NetworkObjectPool>();
//
//             // Setup client pools
//             for (int i = 0; i < NumberOfClients; i++)
//             {
//                 _poolGOs[i + 1] = CreatePoolGO($"ClientPool_{i}", _clientManagers[i]);
//                 _objectPools[i + 1] = _poolGOs[i + 1].GetComponent<NetworkObjectPool>();
//             }
//
//             yield return StartNetworking();
//         }
//
//         private NetworkManager CreateNetworkManager(ushort port)
//         {
//             var go = new GameObject("NetworkManager");
//             var networkManager = go.AddComponent<NetworkManager>();
//             var transport = go.AddComponent<UnityTransport>();
//             
//             // Configure transport
//             transport.ConnectionData = new UnityTransport.ConnectionAddressData
//             {
//                 Address = "127.0.0.1",
//                 Port = port,
//                 ServerListenAddress = "0.0.0.0"
//             };
//
//             // Set transport on NetworkManager
//             networkManager.NetworkConfig = new NetworkConfig();
//             networkManager.NetworkConfig.NetworkTransport = transport;
//             networkManager.NetworkConfig.ConnectionApproval = false;
//             networkManager.NetworkConfig.ConnectionData = new byte[0];
//             networkManager.NetworkConfig.EnableSceneManagement = true;
//             networkManager.NetworkConfig.ForceSamePrefabs = true;
//             networkManager.NetworkConfig.RecycleNetworkIds = true;
//             networkManager.NetworkConfig.Prefabs = new NetworkPrefabs();
//
//             // Register test prefab
//             networkManager.NetworkConfig.Prefabs.Add(new NetworkPrefab { 
//                 Prefab = _testPrefab
//             });
//
//             return networkManager;
//         }
//
//         private GameObject CreatePoolGO(string name, NetworkManager manager)
//         {
//             var go = new GameObject(name);
//             var pool = go.AddComponent<NetworkObjectPool>();
//             var netObj = go.AddComponent<NetworkObject>();
//             
//             // Configure pool
//             pool.Initialize(new Dictionary<GameObject, int> {
//                 { _testPrefab, TestPoolSize }
//             });
//
//             manager.NetworkConfig.Prefabs.Add(new NetworkPrefab { 
//                 Prefab = go
//             });
//             
//             return go;
//         }
//
//         private IEnumerator StartNetworking()
//         {
//             _hostManager.StartHost();
//             yield return new WaitForSeconds(0.2f);
//
//             foreach (var client in _clientManagers)
//             {
//                 client.StartClient();
//                 yield return new WaitForSeconds(0.1f);
//             }
//
//             yield return WaitForClientsConnected();
//             
//             foreach (var poolGO in _poolGOs)
//             {
//                 var netObj = poolGO.GetComponent<NetworkObject>();
//                 if (!netObj.IsSpawned)
//                 {
//                     netObj.Spawn();
//                 }
//             }
//
//             yield return new WaitForSeconds(0.5f);
//         }
//
//         private IEnumerator WaitForClientsConnected()
//         {
//             float timeout = Time.time + 10f;
//             while (Time.time < timeout)
//             {
//                 if (_clientManagers.All(cm => cm.IsConnectedClient))
//                 {
//                     yield break;
//                 }
//                 yield return new WaitForSeconds(0.1f);
//             }
//             Assert.Fail("Timeout waiting for clients to connect");
//         }
//
//         [UnityTest]
//         public IEnumerator TestObjectSpawnAndReturn()
//         {
//             var pool = _objectPools[0];
//             
//             // Spawn object
//             var obj = pool.GetNetworkObject(_testPrefab, Vector3.zero, Quaternion.identity);
//             Assert.IsNotNull(obj, "Failed to spawn object from pool");
//             _spawnedObjects.Add(obj);
//
//             yield return new WaitForSeconds(0.2f);
//
//             // Verify object is spawned on all clients
//             foreach (var clientPool in _objectPools.Skip(1))
//             {
//                 Assert.IsTrue(clientPool.HasSpawnedObject(obj.NetworkObjectId), 
//                     "Client does not see spawned object");
//             }
//
//             // Return object
//             obj.GetComponent<PooledNetworkObject>().ReturnToPool();
//             yield return new WaitForSeconds(0.2f);
//
//             // Verify object is returned on all clients
//             foreach (var clientPool in _objectPools)
//             {
//                 Assert.IsFalse(clientPool.HasSpawnedObject(obj.NetworkObjectId), 
//                     "Object not properly returned to pool");
//             }
//         }
//
//         [UnityTest]
//         public IEnumerator TestPoolExhaustion()
//         {
//             var pool = _objectPools[0];
//             
//             // Spawn more objects than pool size
//             for (int i = 0; i < TestPoolSize + 5; i++)
//             {
//                 var obj = pool.GetNetworkObject(_testPrefab, Vector3.zero, Quaternion.identity);
//                 if (obj != null)
//                 {
//                     _spawnedObjects.Add(obj);
//                 }
//             }
//
//             yield return new WaitForSeconds(0.2f);
//
//             // Verify pool expansion
//             Assert.AreEqual(TestPoolSize, _spawnedObjects.Count, 
//                 "Pool should not expand beyond initial size");
//         }
//
//         [UnityTest]
//         public IEnumerator TestConcurrentSpawnAndReturn()
//         {
//             var pool = _objectPools[0];
//             var objects = new List<NetworkObject>();
//             
//             // Rapidly spawn and return objects
//             for (int i = 0; i < TestPoolSize * 2; i++)
//             {
//                 var obj = pool.GetNetworkObject(_testPrefab, Vector3.zero, Quaternion.identity);
//                 if (obj != null)
//                 {
//                     objects.Add(obj);
//                     _spawnedObjects.Add(obj);
//                 }
//
//                 if (i % 2 == 0 && objects.Count > 0)
//                 {
//                     objects[0].GetComponent<PooledNetworkObject>().ReturnToPool();
//                     objects.RemoveAt(0);
//                 }
//
//                 yield return new WaitForSeconds(0.05f);
//             }
//
//             // Verify pool integrity
//             Assert.IsTrue(objects.Count <= TestPoolSize, 
//                 "Pool exceeded maximum size during concurrent operations");
//         }
//
//         [UnityTest]
//         public IEnumerator TestClientDisconnectWithSpawnedObjects()
//         {
//             var pool = _objectPools[0];
//             
//             // Spawn some objects
//             for (int i = 0; i < TestPoolSize / 2; i++)
//             {
//                 var obj = pool.GetNetworkObject(_testPrefab, Vector3.zero, Quaternion.identity);
//                 if (obj != null)
//                 {
//                     _spawnedObjects.Add(obj);
//                 }
//             }
//
//             yield return new WaitForSeconds(0.2f);
//
//             // Disconnect a client
//             _clientManagers[0].Shutdown();
//             yield return new WaitForSeconds(0.5f);
//
//             // Verify remaining client still sees objects
//             var remainingClientPool = _objectPools[2];
//             foreach (var obj in _spawnedObjects)
//             {
//                 Assert.IsTrue(remainingClientPool.HasSpawnedObject(obj.NetworkObjectId), 
//                     "Remaining client lost track of spawned objects after disconnect");
//             }
//         }
//
//         [UnityTearDown]
//         public IEnumerator TearDown()
//         {
//             // Return all spawned objects
//             foreach (var obj in _spawnedObjects.Where(o => o != null))
//             {
//                 var pooled = obj.GetComponent<PooledNetworkObject>();
//                 if (pooled != null)
//                 {
//                     pooled.ReturnToPool();
//                 }
//             }
//
//             foreach (var pool in _objectPools.Where(p => p != null))
//             {
//                 if (pool.NetworkObject.IsSpawned)
//                 {
//                     pool.NetworkObject.Despawn();
//                 }
//             }
//
//             foreach (var go in _poolGOs.Where(go => go != null))
//             {
//                 Object.Destroy(go);
//             }
//
//             if (_hostManager != null)
//             {
//                 Object.Destroy(_hostManager.gameObject);
//             }
//
//             foreach (var client in _clientManagers.Where(cm => cm != null))
//             {
//                 Object.Destroy(client.gameObject);
//             }
//
//             Object.Destroy(_testPrefab);
//
//             _objectPools = null;
//             _poolGOs = null;
//             _clientManagers = null;
//             _hostManager = null;
//             _testPrefab = null;
//             _spawnedObjects = null;
//
//             yield return new WaitForSeconds(0.1f);
//         }
//     }
// }
