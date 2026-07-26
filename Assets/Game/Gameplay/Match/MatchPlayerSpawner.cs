using Playcenter;
using RecipeRage.Net;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Spawns the local player when a match scene loads. Offline/dev: instantiates
    /// a local PlayerController at the team-A spawn. Networked: the host/client
    /// player objects are spawned by Netcode (this spawner stays out of the way).
    /// </summary>
    public sealed class MatchPlayerSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private int _teamIndex = 0;
        [SerializeField] private int _spawnSlot = 1;

        private void Start()
        {
            // Networked: Netcode spawns player objects — don't double-spawn.
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                return;
            }

            SpawnLocalPlayer();
        }

        private void SpawnLocalPlayer()
        {
            var spawnPoint = FindSpawnPoint();
            var prefab = ResolvePlayerPrefab();
            if (prefab == null)
            {
                Debug.LogError("[Spawner] No player prefab available");
                return;
            }

            var player = Instantiate(prefab, spawnPoint, Quaternion.identity);

            // Apply the selected chef's ability modifier
            var modifier = ServiceLocator.Get<IChefProgressionService>().GetSelectedModifier();
            player.GetComponent<PlayerController>().ApplyChefModifier(modifier);

            Debug.Log($"[Spawner] Local player spawned at {spawnPoint}");
        }

        private Vector3 FindSpawnPoint()
        {
            var spawnName = $"Spawn_Team{(_teamIndex == 0 ? "A" : "B")}_{_spawnSlot}";
            var spawn = GameObject.Find(spawnName);
            if (spawn != null)
            {
                return spawn.transform.position;
            }

            // Fallback: any spawn point, else origin
            var anySpawn = GameObject.Find("SpawnPoints");
            if (anySpawn != null && anySpawn.transform.childCount > 0)
            {
                return anySpawn.transform.GetChild(0).position;
            }
            return Vector3.up * 0.5f;
        }

        private GameObject ResolvePlayerPrefab()
        {
            if (_playerPrefab != null)
            {
                return _playerPrefab;
            }

            // Fallback: load from Resources (prefab must be under Resources/)
            return Resources.Load<GameObject>("Prefabs/NetworkPlayer");
        }
    }
}
