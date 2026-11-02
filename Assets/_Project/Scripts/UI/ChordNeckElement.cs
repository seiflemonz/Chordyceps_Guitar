using UnityEngine;
using UnityEngine.UIElements;
using Chordyceps.Chords;

namespace Chordyceps.UI
{
    /// <summary>
    /// Draws a vertical guitar neck (6 strings x 5 frets) for a chord definition
    /// using Painter2D, plus the optional base-fret label. Presentation only —
    /// reads fret data, never writes it.
    /// </summary>
    public class ChordNeckElement : VisualElement
    {
        private const int STRING_COUNT = ChordDefinition.STRING_COUNT;
        private const int FRET_COUNT = 5;

        private const float PADDING_TOP = 42f;
        private const float PADDING_LEFT = 96f;
        private const float PADDING_RIGHT = 30f;
        private const float PADDING_BOTTOM = 16f;
        private const float STRING_WIDTH = 2f;
        private const float NUT_WIDTH = 7f;
        private const float FRET_WIDTH = 2f;
        private const float DOT_RADIUS_RATIO = 0.38f;
        private const float MARKER_RADIUS = 10f;
        private const float MARKER_GAP = 20f;
        private const float LABEL_WIDTH = 76f;
        private const float LABEL_HEIGHT = 40f;
        private const string LABEL_CLASS = "chord-maker-fret-label";

        private static readonly Color NutColor = new Color(0.90f, 0.90f, 0.92f, 0.95f);
        private static readonly Color StringColor = new Color(0.72f, 0.74f, 0.80f, 0.85f);
        private static readonly Color FretColor = new Color(0.45f, 0.48f, 0.55f, 0.70f);
        private static readonly Color DotColor = new Color(0.94f, 0.72f, 0.26f, 1.00f);
        private static readonly Color MarkerColor = new Color(0.85f, 0.85f, 0.90f, 0.85f);

        private readonly Label _fretLabel;
        private ChordDefinition _chord;
        private int _baseFret = 1;

        public ChordNeckElement()
        {
            pickingMode = PickingMode.Ignore;
            style.flexGrow = 1f;

            _fretLabel = new Label
            {
                pickingMode = PickingMode.Ignore,
                style =
                {
                    position = Position.Absolute,
                    left = 0f,
                    width = LABEL_WIDTH,
                    height = LABEL_HEIGHT,
                }
            };
            _fretLabel.AddToClassList(LABEL_CLASS);
            Add(_fretLabel);

            generateVisualContent += DrawNeck;
            RegisterCallback<GeometryChangedEvent>(_ => UpdateFretLabel());
        }

        public void SetChord(ChordDefinition chord)
        {
            _chord = chord;
            _baseFret = ComputeBaseFret(chord);
            UpdateFretLabel();
            MarkDirtyRepaint();
        }

        /// <summary>
        /// Chords whose frets all sit inside the first five frets stay in place
        /// (no shift, no label). Only chords reaching past fret 5 shift the window
        /// up to their lowest fretted note and show the label.
        /// </summary>
        private static int ComputeBaseFret(ChordDefinition chord)
        {
            if (chord == null) return 1;

            int lowest = int.MaxValue;
            int highest = 0;

            for (int i = 0; i < STRING_COUNT; i++)
            {
                int fret = chord.GetFretOffset(i);
                if (fret <= 0) continue;

                if (fret < lowest) lowest = fret;
                if (fret > highest) highest = fret;
            }

            if (lowest == int.MaxValue || highest <= FRET_COUNT)
            {
                return 1;
            }

            return lowest;
        }

        private void UpdateFretLabel()
        {
            if (_chord == null || _baseFret <= 1)
            {
                _fretLabel.style.visibility = Visibility.Hidden;
                return;
            }

            float top = PADDING_TOP;
            float bottom = contentRect.height - PADDING_BOTTOM;
            if (bottom <= top)
            {
                _fretLabel.style.visibility = Visibility.Hidden;
                return;
            }

            float fretSpacing = (bottom - top) / FRET_COUNT;
            float rowCenter = top + 0.5f * fretSpacing;

            _fretLabel.text = $"{ToOrdinal(_baseFret)} Fret";
            _fretLabel.style.top = rowCenter - LABEL_HEIGHT * 0.5f;
            _fretLabel.style.visibility = Visibility.Visible;
        }

        private static string ToOrdinal(int value)
        {
            switch (value)
            {
                case 1: return "1st";
                case 2: return "2nd";
                case 3: return "3rd";
                default: return value + "th";
            }
        }

        private void DrawNeck(MeshGenerationContext mgc)
        {
            Rect rect = contentRect;
            float left = PADDING_LEFT;
            float right = rect.width - PADDING_RIGHT;
            float top = PADDING_TOP;
            float bottom = rect.height - PADDING_BOTTOM;

            if (right <= left || bottom <= top)
            {
                return;
            }

            float stringSpacing = (right - left) / (STRING_COUNT - 1);
            float fretSpacing = (bottom - top) / FRET_COUNT;
            Painter2D painter = mgc.painter2D;

            DrawFretWires(painter, left, right, top, fretSpacing);
            DrawNut(painter, left, right, top);
            DrawStrings(painter, left, top, bottom, stringSpacing);
            DrawFretDots(painter, left, top, stringSpacing, fretSpacing);
            DrawStringMarkers(painter, left, top, stringSpacing);
        }

        private void DrawFretWires(Painter2D painter, float left, float right, float top, float fretSpacing)
        {
            painter.strokeColor = FretColor;
            painter.lineWidth = FRET_WIDTH;

            for (int i = 1; i <= FRET_COUNT; i++)
            {
                float y = top + i * fretSpacing;
                DrawLine(painter, new Vector2(left, y), new Vector2(right, y));
            }
        }

        private void DrawNut(Painter2D painter, float left, float right, float top)
        {
            painter.strokeColor = NutColor;
            painter.lineWidth = NUT_WIDTH;
            DrawLine(painter, new Vector2(left, top), new Vector2(right, top));
        }

        private void DrawStrings(Painter2D painter, float left, float top, float bottom, float stringSpacing)
        {
            painter.strokeColor = StringColor;
            painter.lineWidth = STRING_WIDTH;

            for (int i = 0; i < STRING_COUNT; i++)
            {
                float x = left + i * stringSpacing;
                DrawLine(painter, new Vector2(x, top), new Vector2(x, bottom));
            }
        }

        private void DrawFretDots(Painter2D painter, float left, float top, float stringSpacing, float fretSpacing)
        {
            if (_chord == null) return;

            float radius = Mathf.Min(stringSpacing, fretSpacing) * DOT_RADIUS_RATIO;
            painter.fillColor = DotColor;

            for (int i = 0; i < STRING_COUNT; i++)
            {
                int fret = _chord.GetFretOffset(i);
                if (fret <= 0) continue;

                int windowIndex = fret - _baseFret + 1;
                if (windowIndex < 1 || windowIndex > FRET_COUNT) continue;

                float x = left + i * stringSpacing;
                float y = top + (windowIndex - 0.5f) * fretSpacing;
                DrawFilledCircle(painter, new Vector2(x, y), radius);
            }
        }

        private void DrawStringMarkers(Painter2D painter, float left, float top, float stringSpacing)
        {
            if (_chord == null) return;

            float y = top - MARKER_GAP;
            painter.strokeColor = MarkerColor;
            painter.lineWidth = 2f;

            for (int i = 0; i < STRING_COUNT; i++)
            {
                int fret = _chord.GetFretOffset(i);
                if (fret > 0) continue;

                var center = new Vector2(left + i * stringSpacing, y);
                if (fret == ChordDefinition.MUTED_STRING_FRET)
                {
                    DrawMutedMarker(painter, center);
                }
                else
                {
                    DrawOpenMarker(painter, center);
                }
            }
        }

        private void DrawMutedMarker(Painter2D painter, Vector2 center)
        {
            float r = MARKER_RADIUS * 0.8f;
            DrawLine(painter, new Vector2(center.x - r, center.y - r), new Vector2(center.x + r, center.y + r));
            DrawLine(painter, new Vector2(center.x + r, center.y - r), new Vector2(center.x - r, center.y + r));
        }

        private void DrawOpenMarker(Painter2D painter, Vector2 center)
        {
            painter.BeginPath();
            painter.Arc(center, MARKER_RADIUS, Angle.Degrees(0f), Angle.Degrees(360f), ArcDirection.Clockwise);
            painter.Stroke();
        }

        private static void DrawLine(Painter2D painter, Vector2 from, Vector2 to)
        {
            painter.BeginPath();
            painter.MoveTo(from);
            painter.LineTo(to);
            painter.Stroke();
        }

        private static void DrawFilledCircle(Painter2D painter, Vector2 center, float radius)
        {
            painter.BeginPath();
            painter.Arc(center, radius, Angle.Degrees(0f), Angle.Degrees(360f), ArcDirection.Clockwise);
            painter.ClosePath();
            painter.Fill();
        }
    }
}
