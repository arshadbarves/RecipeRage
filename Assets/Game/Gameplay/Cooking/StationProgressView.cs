using UnityEngine;
using UnityEngine.UI;

namespace RecipeRage
{
    /// <summary>
    /// World-space progress bar above a CookingStation. Visible to ALL players.
    /// Yellow while cooking, red pulsing during burn grace. Off-screen mirroring
    /// is handled by OffScreenIndicator (Task 7).
    /// </summary>
    public sealed class StationProgressView : MonoBehaviour
    {
        [SerializeField] private CookingStation _station;
        [SerializeField] private Image _fillImage;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Color _cookingColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] private Color _burnColor = new Color(1f, 0.2f, 0.1f);

        private void Update()
        {
            var active = _station.IsActive;
            _canvasGroup.alpha = active ? 1f : 0f;
            if (!active)
            {
                return;
            }

            _fillImage.fillAmount = _station.Progress01;
            _fillImage.color = _station.HasReadyItem ? _burnColor : _cookingColor;
        }
    }
}
