using UnityEngine;
using UnityEngine.UIElements;

namespace KitchenClash.Presentation.Controls
{
    /// <summary>
    /// A custom VisualElement that draws a radial gradient background.
    /// </summary>
    [UxmlElement("RadialGradientElement")]
    public partial class RadialGradientElement : VisualElement
    {
        [UxmlAttribute("center-color")]
        public Color CenterColor
        {
            get => _centerColor;
            set
            {
                if (_centerColor != value)
                {
                    _centerColor = value;
                    GenerateGradientTexture();
                }
            }
        }
        private Color _centerColor = new Color(0.1f, 0.1f, 0.1f, 1f);

        [UxmlAttribute("edge-color")]
        public Color EdgeColor
        {
            get => _edgeColor;
            set
            {
                if (_edgeColor != value)
                {
                    _edgeColor = value;
                    GenerateGradientTexture();
                }
            }
        }
        private Color _edgeColor = Color.black;

        [UxmlAttribute("center-point")]
        [Tooltip("Center point of the gradient in normalized coordinates (0-1).")]
        public Vector2 CenterPoint
        {
            get => _centerPoint;
            set
            {
                if (_centerPoint != value)
                {
                    _centerPoint = value;
                    GenerateGradientTexture();
                }
            }
        }
        private Vector2 _centerPoint = new Vector2(0.5f, 0.5f);

        [UxmlAttribute("radius-scale")]
        [Tooltip("Scale of the gradient radius relative to the element size.")]
        public float RadiusScale
        {
            get => _radiusScale;
            set
            {
                if (!Mathf.Approximately(_radiusScale, value))
                {
                    _radiusScale = value;
                    GenerateGradientTexture();
                }
            }
        }
        private float _radiusScale = 1.0f;

        public RadialGradientElement()
        {
            RegisterCallback<AttachToPanelEvent>(evt => GenerateGradientTexture());
        }

        private void GenerateGradientTexture()
        {
            int width = 256;
            int height = 256;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            Color[] pixels = new Color[width * height];
            Vector2 center = new Vector2(width * _centerPoint.x, height * _centerPoint.y);

            float maxDist = Mathf.Sqrt(width * width + height * height) * 0.5f * _radiusScale;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float t = Mathf.Clamp01(dist / maxDist);
                    pixels[y * width + x] = Color.Lerp(_centerColor, _edgeColor, t);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            style.backgroundImage = new StyleBackground(texture);
        }
    }
}
