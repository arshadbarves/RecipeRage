using System.Threading.Tasks;
using Gameplay.Ability.Components;
using Gameplay.Ability.Effects;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Ability.Abilities
{
    public class FireballAbility : BaseAbility
    {

        private const int ResourceCost = 25;
        [Header("Fireball Settings"), SerializeField]
        private float projectileSpeed = 10f;
        [SerializeField] private float explosionRadius = 3f;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private GameObject explosionPrefab;

        protected override async Task<bool> ExecuteAbility(Vector3 targetPosition, GameObject target)
        {
            if (!IsServer) return false;

            Vector3 direction = (targetPosition - transform.position).normalized;
            GameObject projectile = SpawnProjectile(direction);

            // Wait for projectile to reach target or hit something
            bool hit = await TrackProjectile(projectile, targetPosition);

            if (hit)
            {
                CreateExplosion(projectile.transform.position);
            }

            NetworkObject.Despawn(projectile);
            return hit;
        }

        private GameObject SpawnProjectile(Vector3 direction)
        {
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.LookRotation(direction));
            NetworkObject networkObject = projectile.GetComponent<NetworkObject>();
            networkObject.Spawn();

            return projectile;
        }

        private async Task<bool> TrackProjectile(GameObject projectile, Vector3 targetPosition)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            float travelTime = distanceToTarget / projectileSpeed;
            float elapsed = 0;

            while (elapsed < travelTime)
            {
                if (projectile == null) return false;

                projectile.transform.position = Vector3.Lerp(
                    transform.position,
                    targetPosition,
                    elapsed / travelTime
                );

                // Check for collisions
                if (Physics.SphereCast(projectile.transform.position, 0.5f, projectile.transform.forward, out RaycastHit hit, 0.1f))
                {
                    targetPosition = hit.point;
                    return true;
                }

                elapsed += Time.fixedDeltaTime;
                await Task.Yield();
            }

            return true;
        }

        private void CreateExplosion(Vector3 position)
        {
            GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
            NetworkObject networkObject = explosion.GetComponent<NetworkObject>();
            networkObject.Spawn();

            // Apply damage to nearby targets
            Collider[] colliders = Physics.OverlapSphere(position, explosionRadius);
            foreach (Collider hitCollider in colliders)
            {
                if (hitCollider.TryGetComponent(out IDamageable damageable))
                {
                    DamageEffect effect = explosion.GetComponent<DamageEffect>();
                    effect.Initialize(Owner.gameObject, hitCollider.gameObject);
                    _ = effect.ApplyEffect();
                }
            }
        }

        protected override int GetResourceCost()
        {
            return ResourceCost;
        }
    }
}