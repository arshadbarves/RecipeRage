using UnityEngine;

namespace Gameplay.Ability.Targeting
{
    public interface ITargeting
    {
        bool ValidateTarget(Vector3 position, GameObject target = null);
        Vector3 GetTargetPosition(Vector3 position, GameObject target = null);
        GameObject[] GetAffectedTargets(Vector3 position, GameObject source);
    }
}
