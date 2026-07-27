using UnityEngine;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// Applies Playcenter fonts at runtime: Outfit for headings/body,
    /// Space Grotesk for mono labels (subtitles, brand subtitle).
    /// Fonts load from Resources/Fonts so no USS asset-GUID wiring is needed.
    /// </summary>
    public static class UIFontTheme
    {
        private static Font _heading;
        private static Font _body;
        private static Font _mono;

        public static void Apply(VisualElement root)
        {
            if (_heading == null)
            {
                _heading = Resources.Load<Font>("Fonts/Outfit-ExtraBold");
            }
            if (_body == null)
            {
                _body = Resources.Load<Font>("Fonts/Outfit-Regular");
            }
            if (_mono == null)
            {
                _mono = Resources.Load<Font>("Fonts/SpaceGrotesk-Medium");
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

            // Mono labels (subtitle, brand subtitle)
            if (_mono != null)
            {
                root.Query(className: "mono").ForEach(e => e.style.unityFont = new StyleFont(_mono));
                root.Query(className: "splash-subtitle").ForEach(e => e.style.unityFont = new StyleFont(_mono));
            }
        }
    }
}
