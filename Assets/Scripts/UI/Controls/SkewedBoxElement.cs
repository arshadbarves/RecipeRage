using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Controls
{
    /// <summary>
    /// A custom VisualElement that renders a skewed box with optional fill, border, and border wipe animation.
    /// Professional and customizable control for UI elements requiring non-standard shapes.
    /// Uses the modern UxmlElement/UxmlAttribute system for better UI Builder integration.
    /// </summary>
    [UxmlElement("SkewedBoxElement")]
    public partial class SkewedBoxElement : VisualElement
    {
        #region UXML Attributes - Shape

        [UxmlAttribute("skew-angle")]
        [Tooltip("Horizontal skew angle in degrees. Negative values skew left.")]
        [Range(-45f, 45f)]
        public float SkewAngle
        {
            get => _skewAngle;
            set
            {
                _skewAngle = Mathf.Clamp(value, -45f, 45f);
                MarkDirtyRepaint();
            }
        }
        private float _skewAngle = 0f;

        [UxmlAttribute("corner-radius")]
        [Tooltip("Corner radius in pixels.")]
        [Range(0f, 500f)]
        public float CornerRadius
        {
            get => _cornerRadius;
            set
            {
                _cornerRadius = Mathf.Clamp(value, 0f, 500f);
                MarkDirtyRepaint();
            }
        }
        private float _cornerRadius = 10f;

        #endregion

        #region UXML Attributes - Fill

        [UxmlAttribute("enable-fill")]
        [Tooltip("Enable or disable fill rendering.")]
        public bool EnableFill
        {
            get => _enableFill;
            set
            {
                _enableFill = value;
                MarkDirtyRepaint();
            }
        }
        private bool _enableFill = false;

        [UxmlAttribute("fill-color")]
        [Tooltip("Fill color. Only rendered when EnableFill is true.")]
        public Color FillColor
        {
            get => _fillColor;
            set
            {
                _fillColor = value;
                MarkDirtyRepaint();
            }
        }
        private Color _fillColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        #endregion

        #region UXML Attributes - Border

        [UxmlAttribute("enable-border")]
        [Tooltip("Enable or disable border rendering.")]
        public bool EnableBorder
        {
            get => _enableBorder;
            set
            {
                _enableBorder = value;
                MarkDirtyRepaint();
            }
        }
        private bool _enableBorder = false;

        [UxmlAttribute("border-color")]
        [Tooltip("Border color. Only rendered when EnableBorder is true.")]
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                MarkDirtyRepaint();
            }
        }
        private Color _borderColor = Color.white;

        [UxmlAttribute("border-width")]
        [Tooltip("Border width in pixels.")]
        [Range(0f, 50f)]
        public float BorderWidth
        {
            get => _borderWidth;
            set
            {
                _borderWidth = Mathf.Clamp(value, 0f, 50f);
                MarkDirtyRepaint();
            }
        }
        private float _borderWidth = 2f;

        [UxmlAttribute("border-progress")]
        [Tooltip("Border progress for wipe animation. 0 = no border, 1 = full border.")]
        [Range(0f, 1f)]
        public float BorderProgress
        {
            get => _borderProgress;
            set
            {
                _borderProgress = Mathf.Clamp01(value);
                MarkDirtyRepaint();
            }
        }
        private float _borderProgress = 1f;

        #endregion

        #region Constructor

        public SkewedBoxElement()
        {
            generateVisualContent += OnGenerateVisualContent;
        }

        #endregion

        #region Rendering

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            var rect = contentRect;
            if (rect.width <= 0 || rect.height <= 0) return;

            var painter = mgc.painter2D;
            float skewOffset = Mathf.Tan(_skewAngle * Mathf.Deg2Rad) * rect.height;

            // Calculate corners
            Vector2 tl = new Vector2(-skewOffset * 0.5f, 0);
            Vector2 tr = new Vector2(rect.width - skewOffset * 0.5f, 0);
            Vector2 br = new Vector2(rect.width + skewOffset * 0.5f, rect.height);
            Vector2 bl = new Vector2(skewOffset * 0.5f, rect.height);

            float radius = Mathf.Min(_cornerRadius, rect.height * 0.5f, rect.width * 0.5f);

            // Draw fill
            if (_enableFill)
            {
                painter.fillColor = _fillColor;
                DrawSkewedRoundedRect(painter, tl, tr, br, bl, radius);
                painter.Fill();
            }

            // Draw border
            if (_enableBorder && _borderWidth > 0 && _borderProgress > 0.001f)
            {
                painter.strokeColor = _borderColor;
                painter.lineWidth = _borderWidth;
                painter.lineCap = LineCap.Round;
                painter.lineJoin = LineJoin.Round;

                if (_borderProgress >= 0.999f)
                {
                    DrawSkewedRoundedRect(painter, tl, tr, br, bl, radius);
                    painter.Stroke();
                }
                else
                {
                    DrawSkewedRoundedRectBorder(painter, tl, tr, br, bl, radius, _borderProgress);
                    painter.Stroke();
                }
            }
        }

        private void DrawSkewedRoundedRect(Painter2D painter, Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl, float radius)
        {
            painter.BeginPath();

            float topLen = Vector2.Distance(tl, tr);
            float rightLen = Vector2.Distance(tr, br);
            float bottomLen = Vector2.Distance(br, bl);
            float leftLen = Vector2.Distance(bl, tl);

            if (topLen < 0.001f || rightLen < 0.001f || bottomLen < 0.001f || leftLen < 0.001f) return;

            radius = Mathf.Min(radius, topLen * 0.5f, rightLen * 0.5f, bottomLen * 0.5f, leftLen * 0.5f);

            Vector2 tlStart = Vector2.Lerp(tl, tr, radius / topLen);
            painter.MoveTo(tlStart);

            Vector2 trEnd = Vector2.Lerp(tr, tl, radius / topLen);
            painter.LineTo(trEnd);

            Vector2 trCornerEnd = Vector2.Lerp(tr, br, radius / rightLen);
            painter.BezierCurveTo(tr, tr, trCornerEnd);

            Vector2 brEnd = Vector2.Lerp(br, tr, radius / rightLen);
            painter.LineTo(brEnd);

            Vector2 brCornerEnd = Vector2.Lerp(br, bl, radius / bottomLen);
            painter.BezierCurveTo(br, br, brCornerEnd);

            Vector2 blEnd = Vector2.Lerp(bl, br, radius / bottomLen);
            painter.LineTo(blEnd);

            Vector2 blCornerEnd = Vector2.Lerp(bl, tl, radius / leftLen);
            painter.BezierCurveTo(bl, bl, blCornerEnd);

            Vector2 tlEnd = Vector2.Lerp(tl, bl, radius / leftLen);
            painter.LineTo(tlEnd);

            painter.BezierCurveTo(tl, tl, tlStart);
            painter.ClosePath();
        }

        private void DrawSkewedRoundedRectBorder(Painter2D painter, Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl, float radius, float progress)
        {
            float topLen = Vector2.Distance(tl, tr);
            float rightLen = Vector2.Distance(tr, br);
            float bottomLen = Vector2.Distance(br, bl);
            float leftLen = Vector2.Distance(bl, tl);

            if (topLen < 0.001f || rightLen < 0.001f || bottomLen < 0.001f || leftLen < 0.001f) return;

            radius = Mathf.Min(radius, topLen * 0.5f, rightLen * 0.5f, bottomLen * 0.5f, leftLen * 0.5f);

            float cornerLen = radius * 0.5f * Mathf.PI;
            float totalLen = (topLen - 2 * radius) + (rightLen - 2 * radius) + 
                            (bottomLen - 2 * radius) + (leftLen - 2 * radius) + 4 * cornerLen;
            float targetLen = totalLen * progress;

            painter.BeginPath();
            float currentLen = 0f;

            Vector2 tlStart = Vector2.Lerp(tl, tr, radius / topLen);
            painter.MoveTo(tlStart);

            // Top edge
            float edgeLen = topLen - 2 * radius;
            if (currentLen + edgeLen <= targetLen)
            {
                Vector2 trEnd = Vector2.Lerp(tr, tl, radius / topLen);
                painter.LineTo(trEnd);
                currentLen += edgeLen;
            }
            else
            {
                float t = (targetLen - currentLen) / edgeLen;
                Vector2 trEnd = Vector2.Lerp(tr, tl, radius / topLen);
                painter.LineTo(Vector2.Lerp(tlStart, trEnd, t));
                return;
            }

            // Top-right corner
            if (currentLen + cornerLen <= targetLen)
            {
                Vector2 trCornerEnd = Vector2.Lerp(tr, br, radius / rightLen);
                painter.BezierCurveTo(tr, tr, trCornerEnd);
                currentLen += cornerLen;
            }
            else return;

            // Right edge
            edgeLen = rightLen - 2 * radius;
            if (currentLen + edgeLen <= targetLen)
            {
                Vector2 brEnd = Vector2.Lerp(br, tr, radius / rightLen);
                painter.LineTo(brEnd);
                currentLen += edgeLen;
            }
            else
            {
                float t = (targetLen - currentLen) / edgeLen;
                Vector2 start = Vector2.Lerp(tr, br, radius / rightLen);
                Vector2 end = Vector2.Lerp(br, tr, radius / rightLen);
                painter.LineTo(Vector2.Lerp(start, end, t));
                return;
            }

            // Bottom-right corner
            if (currentLen + cornerLen <= targetLen)
            {
                Vector2 brCornerEnd = Vector2.Lerp(br, bl, radius / bottomLen);
                painter.BezierCurveTo(br, br, brCornerEnd);
                currentLen += cornerLen;
            }
            else return;

            // Bottom edge
            edgeLen = bottomLen - 2 * radius;
            if (currentLen + edgeLen <= targetLen)
            {
                Vector2 blEnd = Vector2.Lerp(bl, br, radius / bottomLen);
                painter.LineTo(blEnd);
                currentLen += edgeLen;
            }
            else
            {
                float t = (targetLen - currentLen) / edgeLen;
                Vector2 start = Vector2.Lerp(br, bl, radius / bottomLen);
                Vector2 end = Vector2.Lerp(bl, br, radius / bottomLen);
                painter.LineTo(Vector2.Lerp(start, end, t));
                return;
            }

            // Bottom-left corner
            if (currentLen + cornerLen <= targetLen)
            {
                Vector2 blCornerEnd = Vector2.Lerp(bl, tl, radius / leftLen);
                painter.BezierCurveTo(bl, bl, blCornerEnd);
                currentLen += cornerLen;
            }
            else return;

            // Left edge
            edgeLen = leftLen - 2 * radius;
            if (currentLen + edgeLen <= targetLen)
            {
                Vector2 tlEnd = Vector2.Lerp(tl, bl, radius / leftLen);
                painter.LineTo(tlEnd);
                currentLen += edgeLen;
            }
            else
            {
                float t = (targetLen - currentLen) / edgeLen;
                Vector2 start = Vector2.Lerp(bl, tl, radius / leftLen);
                Vector2 end = Vector2.Lerp(tl, bl, radius / leftLen);
                painter.LineTo(Vector2.Lerp(start, end, t));
                return;
            }

            // Top-left corner
            if (currentLen + cornerLen <= targetLen)
            {
                painter.BezierCurveTo(tl, tl, tlStart);
            }
        }

        #endregion
    }
}
