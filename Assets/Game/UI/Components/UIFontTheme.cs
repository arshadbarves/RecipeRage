using UnityEngine;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// Applies Clay Kitchen fonts at runtime: Fredoka for headings, Nunito for body.
    /// Fonts load from Resources/Fonts so no USS asset-GUID wiring is needed.
    /// Attach to UIRoot; it styles every registered screen's root on Show.
    /// </summary>
    public static class UIFontTheme
    {
        private static Font _heading;
        private static Font _body;

        public static void Apply(VisualElement root)
        {
            if (_heading == null)
            {
                _heading = Resources.Load<Font>("Fonts/Outfit");
            }
            if (_body == null)
            {
                _body = Resources.Load<Font>("Fonts/Outfit");
            }

            if (root == null)
            {
                return;
            }

            // Body default for everything
            if (_body != null)
            {
                root.style.unityFont = new StyleFont(_body);
            }

            // Headings override via the .heading class (and .display/.section-title)
            if (_heading != null)
            {
                root.Query(className: "heading").ForEach(e => e.style.unityFont = new StyleFont(_heading));
                root.Query(className: "display").ForEach(e => e.style.unityFont = new StyleFont(_heading));
                root.Query(className: "section-title").ForEach(e => e.style.unityFont = new StyleFont(_heading));
                root.Query<Button>().ForEach(e => e.style.unityFont = new StyleFont(_heading));
            }
        }
    }
}
