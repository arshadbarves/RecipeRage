using RecipeRage.Bots;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Net
{
    /// <summary>
    /// Fills empty roster slots with bots at match start (server only).
    /// Practice mode: 1 human + bots. Quick match: fill to team size.
    /// </summary>
    public sealed class BotSpawnManager : MonoBehaviour
    {
        [SerializeField] private NetworkBot _botPrefab;
        [SerializeField] private Transform[] _teamSpawnPoints;

        public void FillSlotsWithBots(int humansInMatch, int teamSize)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            var totalSlots = teamSize * 2;
            var botsToSpawn = totalSlots - humansInMatch;

            for (int i = 0; i < botsToSpawn; i++)
            {
                var spawnPoint = _teamSpawnPoints[i % _teamSpawnPoints.Length];
                var bot = Instantiate(_botPrefab, spawnPoint.position, Quaternion.identity);
                bot.TeamId.Value = (humansInMatch + i) < teamSize ? 0 : 1;
                bot.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
