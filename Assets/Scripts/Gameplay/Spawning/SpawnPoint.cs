using Core.Enums;
using UnityEngine;

namespace Gameplay.Spawning
{
    /// <summary>
    /// Represents a spawn point in the scene with team categorization.
    /// Place these in your scene to define where players and bots can spawn.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        [Header("Spawn Point Settings")]
        [SerializeField] private TeamCategory _teamCategory = TeamCategory.Neutral;
        [SerializeField] private bool _isAvailable = true;
        [SerializeField] private float _spawnRadius = 0.5f;

        [Header("Visual Debug")]
        [SerializeField] private bool _showGizmos = true;
        [SerializeField] private Color _gizmoColor = Color.green;

        /// <summary>
        /// Team category for this spawn point
        /// </summary>
        public TeamCategory TeamCategory => _teamCategory;

        /// <summary>
        /// Whether this spawn point is currently available for use
        /// </summary>
        public bool IsAvailable
        {
            get => _isAvailable;
            set => _isAvailable = value;
        }

        /// <summary>
        /// Spawn radius for randomization
        /// </summary>
        public float SpawnRadius => _spawnRadius;

        /// <summary>
        /// Get spawn position with optional randomization
        /// </summary>
        public Vector3 GetSpawnPosition(bool randomize = false)
        {
            if (!randomize)
                return transform.position;

            Vector2 randomOffset = Random.insideUnitCircle * _spawnRadius;
            return transform.position + new Vector3(randomOffset.x, 0f, randomOffset.y);
        }

        /// <summary>
        /// Get spawn rotation
        /// </summary>
        public Quaternion GetSpawnRotation()
        {
            return transform.rotation;
        }

        private void OnDrawGizmos()
        {
            if (!_showGizmos) return;

            // Set color based on team
            Color color = _teamCategory switch
            {
                TeamCategory.TeamA => Color.blue,
                TeamCategory.TeamB => Color.red,
                TeamCategory.Neutral => Color.green,
                _ => Color.white
            };

            Gizmos.color = _isAvailable ? color : Color.gray;

            // Draw spawn point
            Gizmos.DrawWireSphere(transform.position, _spawnRadius);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1f);

            // Draw arrow for direction
            Vector3 arrowTip = transform.position + transform.forward * 1f;
            Vector3 arrowLeft = arrowTip - transform.forward * 0.3f - transform.right * 0.2f;
            Vector3 arrowRight = arrowTip - transform.forward * 0.3f + transform.right * 0.2f;
            Gizmos.DrawLine(arrowTip, arrowLeft);
            Gizmos.DrawLine(arrowTip, arrowRight);
        }

        private void OnDrawGizmosSelected()
        {
            if (!_showGizmos) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _spawnRadius);
        }
    }
}
