using UnityEngine;
using UnityEngine.UI;

namespace RecipeRage
{
    /// <summary>
    /// One HUD-edge indicator: direction arrow, status icon, mirrored progress bar.
    /// Yellow/orange while cooking; pulsing red when burn grace is draining.
    /// </summary>
    public sealed class OffScreenIndicator : MonoBehaviour
    {
        [SerializeField] private RectTransform _arrow;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _progressFill;
        [SerializeField] private Color _cookingColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] private Color _burnColor = new Color(1f, 0.2f, 0.1f);
        [SerializeField] private float _pulseSpeed = 6f;

        private CookingStation _station;

        public CookingStation Station => _station;

        public void Bind(CookingStation station)
        {
            _station = station;
        }

        private void Update()
        {
            if (_station == null || !_station.IsActive)
            {
                return;
            }

            _progressFill.fillAmount = _station.Progress01;

            if (_station.HasReadyItem) // burn grace draining = urgent
            {
                var pulse = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) * 0.5f;
                _icon.color = Color.Lerp(_burnColor, Color.white, pulse * 0.5f);
                _progressFill.color = _burnColor;
            }
            else
            {
                _icon.color = _cookingColor;
                _progressFill.color = _cookingColor;
            }
        }

        public void SetEdgePosition(Vector2 anchoredPosition, float rotationZ)
        {
            ((RectTransform)transform).anchoredPosition = anchoredPosition;
            _arrow.localEulerAngles = new Vector3(0f, 0f, rotationZ);
        }
    }
}
