using Brawlers;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Station
{
    public enum StationState
    {
        Idle,
        InProgress,
        Completed,
        Burned
    }

    public class BaseStation : NetworkBehaviour
    {
        protected readonly NetworkVariable<bool> IsOccupied = new NetworkVariable<bool>();

        protected virtual void Awake()
        {
            // progressUI = GetComponentInChildren<ProgressUI>();
            // indicatorUI = GetComponentInChildren<IndicatorUI>();
        }

        public virtual void Interact(BaseController player) { }

        protected void UpdateVisibility()
        {
            bool isVisible = IsVisibleToCamera();
            // progressUI.gameObject.SetActive(isVisible);
            // indicatorUI.gameObject.SetActive(!isVisible);
        }

        protected bool IsVisibleToCamera()
        {
            if (Camera.main != null)
            {
                Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
                return screenPoint is { z: > 0, x: > 0 } and { x: < 1, y: > 0 and < 1 };
            }

            return false;
        }
    }
}