using UnityEngine;
using VContainer;
using Core.Localization;

namespace Core.DebugTools
{
    /// <summary>
    /// Simple debugger to toggle languages.
    /// Usage: Add to any GameObject in the scene context (e.g. GameLifetimeScope).
    /// Press 'L' to toggle between English and Spanish.
    /// </summary>
    public class LocalizationDebugger : MonoBehaviour
    {
        private ILocalizationManager _localization;

        [Inject]
        public void Construct(ILocalizationManager localization)
        {
            _localization = localization;
        }

        private void Update()
        {
            // Only run if injected
            if (_localization == null) return;

            if (UnityEngine.Input.GetKeyDown(KeyCode.L))
            {
                ToggleLanguage();
            }
        }

        private void ToggleLanguage()
        {
            string current = _localization.CurrentLanguage;
            string target = current == "English" ? "Spanish" : "English";

            _localization.SetLanguage(target);
            Debug.Log($"[LocalizationDebugger] Switched language to {target}");
        }
    }
}
