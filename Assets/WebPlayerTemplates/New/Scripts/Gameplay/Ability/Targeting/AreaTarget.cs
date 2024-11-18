using UnityEngine;

namespace Gameplay.Ability.Targeting
{
    public class AreaTarget : ITargeting
    {
        private readonly float _radius;
        private readonly float _range;
        private readonly LayerMask _targetLayer;

        public AreaTarget(float radius, float range, LayerMask targetLayer)
        {
            _radius = radius;
            _range = range;
            _targetLayer = targetLayer;
        }

        public bool ValidateTarget(Vector3 position, GameObject target = null)
        {
            return Vector3.Distance(position, target.transform.position) <= _range;
        }

        public Vector3 GetTargetPosition(Vector3 position, GameObject target = null)
        {
            return position;
        }

        public GameObject[] GetAffectedTargets(Vector3 position, GameObject source)
        {
            Collider[] colliders = Physics.OverlapSphere(position, _radius, _targetLayer);
            GameObject[] targets = new GameObject[colliders.Length];

            for (int i = 0; i < colliders.Length; i++)
            {
                targets[i] = colliders[i].gameObject;
            }

            return targets;
        }
    }
}