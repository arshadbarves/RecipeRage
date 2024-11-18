using UnityEngine;

namespace Gameplay.Ability.Targeting
{
    public class SingleTarget : ITargeting
    {
        private readonly float _range;
        private readonly LayerMask _targetLayer;

        public SingleTarget(float range, LayerMask targetLayer)
        {
            _range = range;
            _targetLayer = targetLayer;
        }

        public bool ValidateTarget(Vector3 position, GameObject target = null)
        {
            if (target == null) return false;

            return Vector3.Distance(position, target.transform.position) <= _range &&
                   (_targetLayer.value & 1 << target.layer) != 0;
        }

        public Vector3 GetTargetPosition(Vector3 position, GameObject target = null)
        {
            return target != null ? target.transform.position : position;
        }

        public GameObject[] GetAffectedTargets(Vector3 position, GameObject source)
        {
            Collider[] colliders = Physics.OverlapSphere(position, 0.5f, _targetLayer);
            GameObject[] targets = new GameObject[colliders.Length];

            for (int i = 0; i < colliders.Length; i++)
            {
                targets[i] = colliders[i].gameObject;
            }

            return targets;
        }
    }
}