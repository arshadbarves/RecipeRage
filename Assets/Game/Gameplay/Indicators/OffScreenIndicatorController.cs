using System.Collections.Generic;
using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Mirrors off-screen station progress onto HUD edges. For each active
    /// CookingStation outside the camera view, places an indicator on the
    /// nearest screen edge with an arrow pointing toward it. Indicators on the
    /// same edge stack vertically.
    /// </summary>
    public sealed class OffScreenIndicatorController : MonoBehaviour
    {
        [SerializeField] private OffScreenIndicator _indicatorPrefab;
        [SerializeField] private RectTransform _indicatorRoot;
        [SerializeField] private float _edgePadding = 60f;
        [SerializeField] private float _stackSpacing = 56f;

        private readonly Dictionary<CookingStation, OffScreenIndicator> _active =
            new Dictionary<CookingStation, OffScreenIndicator>();

        private System.Func<System.Collections.Generic.IReadOnlyList<CookingStation>> _stationsProvider;
        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
            var registry = FindFirstObjectByType<Net.MatchRuntimeRegistry>();
            _stationsProvider = () => registry != null
                ? registry.CookingStations
                : (System.Collections.Generic.IReadOnlyList<CookingStation>)new List<CookingStation>();
        }

        private void LateUpdate()
        {
            var edgeCounts = new Dictionary<int, int>();
            var stations = _stationsProvider();

            foreach (var station in stations)
            {
                var shouldShow = station.IsActive && !IsOnScreen(station.transform.position);

                if (shouldShow && !_active.ContainsKey(station))
                {
                    var indicator = Instantiate(_indicatorPrefab, _indicatorRoot);
                    indicator.Bind(station);
                    _active.Add(station, indicator);
                }
                else if (!shouldShow && _active.TryGetValue(station, out var stale))
                {
                    Destroy(stale.gameObject);
                    _active.Remove(station);
                }
            }

            foreach (var kvp in _active)
            {
                var viewport = _camera.WorldToViewportPoint(kvp.Key.transform.position);
                var edge = GetEdge(viewport, out var rotation);
                var stackIndex = edgeCounts.TryGetValue(edge, out var count) ? count : 0;
                edgeCounts[edge] = stackIndex + 1;

                var pos = GetEdgePosition(edge, stackIndex);
                kvp.Value.SetEdgePosition(pos, rotation);
            }
        }

        private bool IsOnScreen(Vector3 worldPosition)
        {
            var viewport = _camera.WorldToViewportPoint(worldPosition);
            return viewport.z > 0f
                && viewport.x >= 0f && viewport.x <= 1f
                && viewport.y >= 0f && viewport.y <= 1f;
        }

        private int GetEdge(Vector3 viewport, out float rotationZ)
        {
            // 0=left, 1=right, 2=top, 3=bottom
            if (viewport.x < 0f) { rotationZ = 180f; return 0; }
            if (viewport.x > 1f) { rotationZ = 0f; return 1; }
            if (viewport.y > 1f) { rotationZ = 90f; return 2; }
            rotationZ = -90f;
            return 3;
        }

        private Vector2 GetEdgePosition(int edge, int stackIndex)
        {
            var rect = _indicatorRoot.rect;
            var offset = _edgePadding + stackIndex * _stackSpacing;
            return edge switch
            {
                0 => new Vector2(-rect.width / 2f + _edgePadding, -rect.height / 2f + offset),
                1 => new Vector2(rect.width / 2f - _edgePadding, -rect.height / 2f + offset),
                2 => new Vector2(-rect.width / 2f + offset, rect.height / 2f - _edgePadding),
                _ => new Vector2(-rect.width / 2f + offset, -rect.height / 2f + _edgePadding),
            };
        }
    }
}
