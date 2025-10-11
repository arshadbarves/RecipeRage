using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.UISystem.Components
{
    /// <summary>
    /// Advanced gradient visual element with multi-stop support, directional gradients, and animation
    /// Supports horizontal, vertical, diagonal, and radial gradients with unlimited color stops
    /// </summary>
    public class GradientElement : VisualElement
    {
        #region Enums

        public enum GradientType
        {
            Linear,
            Radial
        }

        public enum GradientDirection
        {
            Horizontal,
            Vertical,
            DiagonalTopLeftToBottomRight,
            DiagonalTopRightToBottomLeft,
            Custom
        }

        #endregion

        #region Nested Classes

        [Serializable]
        public class ColorStop
        {
            public Color Color;
            public float Position; // 0.0 to 1.0

            public ColorStop(Color color, float position)
            {
                Color = color;
                Position = Mathf.Clamp01(position);
            }
        }

        #endregion

        #region UXML Factory

        public new class UxmlFactory : UxmlFactory<GradientElement, GradientUxmlTraits> { }

        public class GradientUxmlTraits : UxmlTraits
        {
            private readonly UxmlColorAttributeDescription _startColor = new UxmlColorAttributeDescription
            {
                name = "start-color",
                defaultValue = new Color(1f, 0f, 0f, 1f)
            };

            private readonly UxmlColorAttributeDescription _endColor = new UxmlColorAttributeDescription
            {
                name = "end-color",
                defaultValue = new Color(0f, 0f, 1f, 1f)
            };

            private readonly UxmlEnumAttributeDescription<GradientDirection> _direction = new UxmlEnumAttributeDescription<GradientDirection>
            {
                name = "direction",
                defaultValue = GradientDirection.Horizontal
            };

            private readonly UxmlEnumAttributeDescription<GradientType> _type = new UxmlEnumAttributeDescription<GradientType>
            {
                name = "type",
                defaultValue = GradientType.Linear
            };

            private readonly UxmlFloatAttributeDescription _angle = new UxmlFloatAttributeDescription
            {
                name = "angle",
                defaultValue = 0f
            };

            private readonly UxmlIntAttributeDescription _segments = new UxmlIntAttributeDescription
            {
                name = "segments",
                defaultValue = 32
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                if (ve == null)
                    throw new ArgumentNullException(nameof(ve));

                var gradientElement = (GradientElement)ve;
                var startColor = _startColor.GetValueFromBag(bag, cc);
                var endColor = _endColor.GetValueFromBag(bag, cc);

                gradientElement.SetColorStops(new List<ColorStop>
                {
                    new ColorStop(startColor, 0f),
                    new ColorStop(endColor, 1f)
                });

                gradientElement.Direction = _direction.GetValueFromBag(bag, cc);
                gradientElement.Type = _type.GetValueFromBag(bag, cc);
                gradientElement.Angle = _angle.GetValueFromBag(bag, cc);
                gradientElement.Segments = _segments.GetValueFromBag(bag, cc);
            }
        }

        #endregion

        #region Properties

        private List<ColorStop> _colorStops = new List<ColorStop>
        {
            new ColorStop(Color.red, 0f),
            new ColorStop(Color.blue, 1f)
        };

        private GradientDirection _direction = GradientDirection.Horizontal;
        private GradientType _type = GradientType.Linear;
        private float _angle = 0f;
        private int _segments = 32;
        private Vector2 _customDirection = Vector2.right;
        private bool _animated = false;
        private float _animationSpeed = 1f;
        private float _animationOffset = 0f;

        /// <summary>
        /// Color stops for the gradient (min 2 required)
        /// </summary>
        public List<ColorStop> ColorStops
        {
            get => _colorStops;
            set
            {
                if (value == null || value.Count < 2)
                {
                    Debug.LogWarning("[GradientElement] At least 2 color stops required");
                    return;
                }
                _colorStops = value;
                SortColorStops();
                MarkDirtyRepaint();
            }
        }

        /// <summary>
        /// Direction of the gradient
        /// </summary>
        public GradientDirection Direction
        {
            get => _direction;
            set
            {
                if (_direction != value)
                {
                    _direction = value;
                    MarkDirtyRepaint();
                }
            }
        }

        /// <summary>
        /// Type of gradient (Linear or Radial)
        /// </summary>
        public GradientType Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    MarkDirtyRepaint();
                }
            }
        }

        /// <summary>
        /// Custom angle in degrees (0-360) when Direction is Custom
        /// </summary>
        public float Angle
        {
            get => _angle;
            set
            {
                var normalizedAngle = value % 360f;
                if (!Mathf.Approximately(_angle, normalizedAngle))
                {
                    _angle = normalizedAngle;
                    UpdateCustomDirection();
                    MarkDirtyRepaint();
                }
            }
        }

        /// <summary>
        /// Number of segments for radial gradients (higher = smoother)
        /// </summary>
        public int Segments
        {
            get => _segments;
            set
            {
                var clampedValue = Mathf.Max(8, value);
                if (_segments != clampedValue)
                {
                    _segments = clampedValue;
                    MarkDirtyRepaint();
                }
            }
        }

        /// <summary>
        /// Enable animated gradient
        /// </summary>
        public bool Animated
        {
            get => _animated;
            set
            {
                if (_animated != value)
                {
                    _animated = value;
                    if (_animated)
                    {
                        schedule.Execute(AnimateGradient).Every(16); // ~60 FPS
                    }
                }
            }
        }

        /// <summary>
        /// Animation speed multiplier
        /// </summary>
        public float AnimationSpeed
        {
            get => _animationSpeed;
            set => _animationSpeed = Mathf.Max(0.1f, value);
        }

        #endregion

        #region Constructor

        public GradientElement()
        {
            generateVisualContent += GenerateVisualContent;
        }

        #endregion

        #region Visual Content Generation

        private void GenerateVisualContent(MeshGenerationContext mgc)
        {
            var rect = contentRect;
            if (rect.width < 0.1f || rect.height < 0.1f || _colorStops.Count < 2)
                return;

            switch (_type)
            {
                case GradientType.Linear:
                    GenerateLinearGradient(mgc, rect);
                    break;
                case GradientType.Radial:
                    GenerateRadialGradient(mgc, rect);
                    break;
            }
        }

        private void GenerateLinearGradient(MeshGenerationContext mgc, Rect rect)
        {
            Vector2 gradientDir = GetGradientDirection();

            // Calculate perpendicular direction for strips
            Vector2 perpDir = new Vector2(-gradientDir.y, gradientDir.x);

            int stripCount = _colorStops.Count - 1;
            int verticesPerStrip = 4;
            int totalVertices = stripCount * verticesPerStrip;
            int totalIndices = stripCount * 6;

            Vertex[] vertices = new Vertex[totalVertices];
            ushort[] indices = new ushort[totalIndices];

            var center = new Vector2(rect.width * 0.5f, rect.height * 0.5f);
            float maxDist = Mathf.Max(rect.width, rect.height);

            for (int i = 0; i < stripCount; i++)
            {
                ColorStop currentStop = _colorStops[i];
                ColorStop nextStop = _colorStops[i + 1];

                float t1 = (currentStop.Position + _animationOffset) % 1f;
                float t2 = (nextStop.Position + _animationOffset) % 1f;

                Vector2 p1 = center + gradientDir * ((t1 - 0.5f) * maxDist * 2f);
                Vector2 p2 = center + gradientDir * ((t2 - 0.5f) * maxDist * 2f);

                float perpExtent = maxDist;
                Vector2 offset1 = perpDir * perpExtent;
                Vector2 offset2 = perpDir * -perpExtent;

                int vertexIndex = i * verticesPerStrip;

                // Create quad for this color strip
                vertices[vertexIndex + 0] = new Vertex
                {
                    position = new Vector3(p1.x + offset1.x, p1.y + offset1.y, Vertex.nearZ),
                    tint = currentStop.Color
                };
                vertices[vertexIndex + 1] = new Vertex
                {
                    position = new Vector3(p1.x + offset2.x, p1.y + offset2.y, Vertex.nearZ),
                    tint = currentStop.Color
                };
                vertices[vertexIndex + 2] = new Vertex
                {
                    position = new Vector3(p2.x + offset2.x, p2.y + offset2.y, Vertex.nearZ),
                    tint = nextStop.Color
                };
                vertices[vertexIndex + 3] = new Vertex
                {
                    position = new Vector3(p2.x + offset1.x, p2.y + offset1.y, Vertex.nearZ),
                    tint = nextStop.Color
                };

                // Create indices for this quad
                int indexBase = i * 6;
                ushort vBase = (ushort)(i * verticesPerStrip);
                indices[indexBase + 0] = vBase;
                indices[indexBase + 1] = (ushort)(vBase + 1);
                indices[indexBase + 2] = (ushort)(vBase + 2);
                indices[indexBase + 3] = vBase;
                indices[indexBase + 4] = (ushort)(vBase + 2);
                indices[indexBase + 5] = (ushort)(vBase + 3);
            }

            MeshWriteData mwd = mgc.Allocate(totalVertices, totalIndices);
            mwd.SetAllVertices(vertices);
            mwd.SetAllIndices(indices);
        }

        private void GenerateRadialGradient(MeshGenerationContext mgc, Rect rect)
        {
            var center = new Vector2(rect.width * 0.5f, rect.height * 0.5f);
            float maxRadius = Mathf.Max(rect.width, rect.height) * 0.5f;

            int ringCount = _colorStops.Count - 1;
            int segmentCount = _segments;
            int verticesPerRing = segmentCount + 1;
            int totalVertices = (ringCount + 1) * verticesPerRing + 1; // +1 for center vertex
            int totalIndices = ringCount * segmentCount * 6;

            Vertex[] vertices = new Vertex[totalVertices];
            ushort[] indices = new ushort[totalIndices];

            // Center vertex
            vertices[0] = new Vertex
            {
                position = new Vector3(center.x, center.y, Vertex.nearZ),
                tint = _colorStops[0].Color
            };

            // Generate rings
            int vertexIndex = 1;
            for (int ring = 0; ring <= ringCount; ring++)
            {
                ColorStop stop = _colorStops[ring];
                float radius = stop.Position * maxRadius;
                Color color = stop.Color;

                for (int seg = 0; seg <= segmentCount; seg++)
                {
                    float angle = (seg / (float)segmentCount) * Mathf.PI * 2f + _animationOffset;
                    float x = center.x + Mathf.Cos(angle) * radius;
                    float y = center.y + Mathf.Sin(angle) * radius;

                    vertices[vertexIndex++] = new Vertex
                    {
                        position = new Vector3(x, y, Vertex.nearZ),
                        tint = color
                    };
                }
            }

            // Generate indices
            int indexPos = 0;

            // Center to first ring
            for (int seg = 0; seg < segmentCount; seg++)
            {
                indices[indexPos++] = 0;
                indices[indexPos++] = (ushort)(1 + seg);
                indices[indexPos++] = (ushort)(1 + seg + 1);
            }

            // Ring to ring
            for (int ring = 0; ring < ringCount; ring++)
            {
                int currentRingStart = 1 + ring * verticesPerRing;
                int nextRingStart = 1 + (ring + 1) * verticesPerRing;

                for (int seg = 0; seg < segmentCount; seg++)
                {
                    ushort v0 = (ushort)(currentRingStart + seg);
                    ushort v1 = (ushort)(currentRingStart + seg + 1);
                    ushort v2 = (ushort)(nextRingStart + seg + 1);
                    ushort v3 = (ushort)(nextRingStart + seg);

                    indices[indexPos++] = v0;
                    indices[indexPos++] = v1;
                    indices[indexPos++] = v2;
                    indices[indexPos++] = v0;
                    indices[indexPos++] = v2;
                    indices[indexPos++] = v3;
                }
            }

            MeshWriteData mwd = mgc.Allocate(totalVertices, totalIndices);
            mwd.SetAllVertices(vertices);
            mwd.SetAllIndices(indices);
        }

        #endregion

        #region Helper Methods

        private Vector2 GetGradientDirection()
        {
            switch (_direction)
            {
                case GradientDirection.Horizontal:
                    return Vector2.right;
                case GradientDirection.Vertical:
                    return Vector2.up;
                case GradientDirection.DiagonalTopLeftToBottomRight:
                    return new Vector2(1f, -1f).normalized;
                case GradientDirection.DiagonalTopRightToBottomLeft:
                    return new Vector2(-1f, -1f).normalized;
                case GradientDirection.Custom:
                    return _customDirection;
                default:
                    return Vector2.right;
            }
        }

        private void UpdateCustomDirection()
        {
            float radians = _angle * Mathf.Deg2Rad;
            _customDirection = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        private void SortColorStops()
        {
            _colorStops.Sort((a, b) => a.Position.CompareTo(b.Position));
        }

        private void AnimateGradient()
        {
            if (!_animated) return;

            _animationOffset += Time.deltaTime * _animationSpeed * 0.1f;
            if (_animationOffset > 1f) _animationOffset -= 1f;

            MarkDirtyRepaint();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Set color stops from a list
        /// </summary>
        public void SetColorStops(List<ColorStop> stops)
        {
            if (stops == null || stops.Count < 2)
            {
                Debug.LogWarning("[GradientElement] At least 2 color stops required");
                return;
            }
            _colorStops = new List<ColorStop>(stops);
            SortColorStops();
            MarkDirtyRepaint();
        }

        /// <summary>
        /// Add a color stop at specified position
        /// </summary>
        public void AddColorStop(Color color, float position)
        {
            _colorStops.Add(new ColorStop(color, position));
            SortColorStops();
            MarkDirtyRepaint();
        }

        /// <summary>
        /// Clear all color stops and set new ones
        /// </summary>
        public void SetGradient(Color startColor, Color endColor)
        {
            _colorStops.Clear();
            _colorStops.Add(new ColorStop(startColor, 0f));
            _colorStops.Add(new ColorStop(endColor, 1f));
            MarkDirtyRepaint();
        }

        /// <summary>
        /// Set gradient with multiple colors evenly distributed
        /// </summary>
        public void SetGradient(params Color[] colors)
        {
            if (colors.Length < 2)
            {
                Debug.LogWarning("[GradientElement] At least 2 colors required");
                return;
            }

            _colorStops.Clear();
            for (int i = 0; i < colors.Length; i++)
            {
                float position = i / (float)(colors.Length - 1);
                _colorStops.Add(new ColorStop(colors[i], position));
            }
            MarkDirtyRepaint();
        }

        /// <summary>
        /// Create a simple two-color gradient
        /// </summary>
        public static GradientElement Create(Color startColor, Color endColor, GradientDirection direction = GradientDirection.Horizontal)
        {
            var gradient = new GradientElement
            {
                Direction = direction
            };
            gradient.SetGradient(startColor, endColor);
            return gradient;
        }

        /// <summary>
        /// Create a multi-stop gradient
        /// </summary>
        public static GradientElement CreateMultiStop(List<ColorStop> colorStops, GradientDirection direction = GradientDirection.Horizontal)
        {
            var gradient = new GradientElement
            {
                Direction = direction
            };
            gradient.SetColorStops(colorStops);
            return gradient;
        }

        /// <summary>
        /// Create a radial gradient
        /// </summary>
        public static GradientElement CreateRadial(Color centerColor, Color outerColor, int segments = 32)
        {
            var gradient = new GradientElement
            {
                Type = GradientType.Radial,
                Segments = segments
            };
            gradient.SetGradient(centerColor, outerColor);
            return gradient;
        }

        /// <summary>
        /// Create an animated gradient
        /// </summary>
        public static GradientElement CreateAnimated(Color[] colors, float speed = 1f)
        {
            var gradient = new GradientElement();
            gradient.SetGradient(colors);
            gradient.Animated = true;
            gradient.AnimationSpeed = speed;
            return gradient;
        }

        #endregion
    }
}
