using System;
using UnityEngine;
using UnityEngine.UIElements;
using Chordyceps.Chords;

namespace Chordyceps.UI
{
    /// <summary>
    /// Draws a vertical guitar neck: six string lines, a nut and five fret spaces.
    /// Shows one chord's fingering, with an optional left-hand window label naming the
    /// base fret. Dots are green while viewing and amber while editing.
    ///
    /// Fingerings that imply a barre — a lowest fretted note shared by two or more
    /// strings, with other frets above it, or covering more than four strings on its
    /// own — get one drawn behind the dots, worked out from the frets by
    /// <see cref="ChordBarre"/>.
    /// </summary>
    public class ChordNeckElement : VisualElement
    {
        public const int STRING_COUNT = 6;
        public const int FRET_COUNT = 5;
        public const int MIN_BASE_FRET = 1;
        public const int MAX_BASE_FRET = 12;
        public const int MUTED = -1;
        public const int OPEN = 0;

        private const float PADDING_TOP = 42f;
        private const float PADDING_BOTTOM = 16f;
        private const float PADDING_LEFT = 96f;
        private const float PADDING_RIGHT = 30f;

        private const float LABEL_WIDTH = 76f;
        private const float LABEL_HEIGHT = 40f;
        private const float SHIFT_BUTTON_HEIGHT = 34f;
        private const float MARKER_OFFSET = 20f;
        private const float MARKER_BAND_HEIGHT = 36f;
        private const float MARKER_RADIUS = 9f;

        /// <summary>Barre thickness as a fraction of a dot's diameter.</summary>
        private const float BARRE_THICKNESS = 0.62f;

        private const float BARRE_ALPHA = 0.55f;

        private static readonly Color StringColor = new Color(0.86f, 0.88f, 0.92f, 0.85f);
        private static readonly Color NutColor = new Color(0.93f, 0.94f, 0.96f, 1f);
        private static readonly Color FretWireColor = new Color(1f, 1f, 1f, 0.22f);
        private static readonly Color MarkerColor = new Color(1f, 1f, 1f, 0.72f);
        private static readonly Color ViewDotColor = new Color(0.36f, 0.85f, 0.45f, 1f);
        private static readonly Color EditDotColor = new Color(0.94f, 0.72f, 0.26f, 1f);

        private readonly VisualElement _fretControls;
        private readonly FretShiftButton _shiftUpButton;
        private readonly FretShiftButton _shiftDownButton;
        private readonly Label _fretLabel;

        private int[] _frets = new int[STRING_COUNT];
        private int _baseFret = MIN_BASE_FRET;
        private bool _editable;
        private Color _dotColor = ViewDotColor;

        /// <summary>Raised when a fret cell is tapped while editing. Fret is absolute, not a row index.</summary>
        public event Action<int, int> FretTapped;

        /// <summary>Raised when the X/O marker above the nut is tapped while editing.</summary>
        public event Action<int> StringMarkerTapped;

        public event Action ShiftUpRequested;
        public event Action ShiftDownRequested;

        public int BaseFret => _baseFret;

        public ChordNeckElement()
        {
            pickingMode = PickingMode.Ignore;
            style.flexGrow = 1f;
            generateVisualContent += OnGenerateVisualContent;
            RegisterCallback<PointerDownEvent>(HandlePointerDown);
            RegisterCallback<GeometryChangedEvent>(_ => PositionFretControls());

            _fretControls = new VisualElement { pickingMode = PickingMode.Ignore };
            _fretControls.AddToClassList("chord-maker-fret-controls");
            _fretControls.style.position = Position.Absolute;
            _fretControls.style.width = LABEL_WIDTH;
            _fretControls.style.height = SHIFT_BUTTON_HEIGHT + LABEL_HEIGHT + SHIFT_BUTTON_HEIGHT;
            _fretControls.style.flexDirection = FlexDirection.Column;

            _shiftUpButton = new FretShiftButton(true, () => ShiftUpRequested?.Invoke());
            _shiftUpButton.style.height = SHIFT_BUTTON_HEIGHT;
            _shiftDownButton = new FretShiftButton(false, () => ShiftDownRequested?.Invoke());
            _shiftDownButton.style.height = SHIFT_BUTTON_HEIGHT;

            _fretLabel = new Label();
            _fretLabel.AddToClassList("chord-maker-fret-label");
            _fretLabel.style.height = LABEL_HEIGHT;
            _fretLabel.pickingMode = PickingMode.Ignore;

            _fretControls.Add(_shiftUpButton);
            _fretControls.Add(_fretLabel);
            _fretControls.Add(_shiftDownButton);

            Add(_fretControls);
            RefreshFretControls();
        }

        /// <summary>Shows a chord for reading: green dots and an automatically derived base fret.</summary>
        public void ShowChord(int[] frets)
        {
            _frets = Normalize(frets);
            _baseFret = DeriveBaseFret(_frets);
            _editable = false;
            _dotColor = ViewDotColor;
            ApplyInteractivity();
            RefreshFretControls();
            MarkDirtyRepaint();
        }

        /// <summary>Shows the working copy of a chord: amber dots and a caller-controlled base fret.</summary>
        public void ShowDraft(int[] frets, int baseFret)
        {
            _frets = Normalize(frets);
            _baseFret = Mathf.Clamp(baseFret, MIN_BASE_FRET, MAX_BASE_FRET);
            _editable = true;
            _dotColor = EditDotColor;
            ApplyInteractivity();
            RefreshFretControls();
            MarkDirtyRepaint();
        }

        /// <summary>
        /// The fret window to show for a chord: the first five frets unless the chord reaches
        /// past them, in which case it follows the chord's lowest fretted note.
        /// </summary>
        public static int DeriveBaseFret(int[] frets)
        {
            if (frets == null)
            {
                return MIN_BASE_FRET;
            }

            int lowest = int.MaxValue;
            int highest = 0;
            for (int i = 0; i < frets.Length; i++)
            {
                int fret = frets[i];
                if (fret <= OPEN)
                {
                    continue;
                }

                lowest = Mathf.Min(lowest, fret);
                highest = Mathf.Max(highest, fret);
            }

            if (highest <= FRET_COUNT)
            {
                return MIN_BASE_FRET;
            }

            return Mathf.Clamp(lowest, MIN_BASE_FRET, MAX_BASE_FRET);
        }

        private static int[] Normalize(int[] frets)
        {
            var copy = new int[STRING_COUNT];
            if (frets != null)
            {
                Array.Copy(frets, copy, Mathf.Min(frets.Length, STRING_COUNT));
            }

            return copy;
        }

        private void ApplyInteractivity()
        {
            pickingMode = _editable ? PickingMode.Position : PickingMode.Ignore;
        }

        /// <summary>
        /// Keeps the shift buttons and the base-fret label anchored to the fret they describe.
        /// The buttons hold their slots while hidden so the label never drifts between modes.
        /// </summary>
        private void RefreshFretControls()
        {
            _fretLabel.text = _baseFret > MIN_BASE_FRET ? $"{_baseFret}{GetOrdinalSuffix(_baseFret)} Fret" : string.Empty;
            _fretLabel.style.visibility = _baseFret > MIN_BASE_FRET ? Visibility.Visible : Visibility.Hidden;

            _shiftUpButton.style.visibility = _editable && _baseFret < MAX_BASE_FRET
                ? Visibility.Visible
                : Visibility.Hidden;
            _shiftDownButton.style.visibility = _editable && _baseFret > MIN_BASE_FRET
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        /// <summary>
        /// Anchors the shift buttons and label so they sit centred on the first fret row
        /// of the current window, keeping them bound to the fret they describe.
        /// </summary>
        private void PositionFretControls()
        {
            Rect rect = contentRect;
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            float top = PADDING_TOP;
            float fretSpacing = (rect.height - PADDING_BOTTOM - top) / FRET_COUNT;
            float controlHeight = SHIFT_BUTTON_HEIGHT + LABEL_HEIGHT + SHIFT_BUTTON_HEIGHT;

            _fretControls.style.left = (LABEL_WIDTH - PADDING_LEFT) * 0.5f;
            _fretControls.style.top = top + (fretSpacing * 0.5f) - (controlHeight * 0.5f);
        }

        private static string GetOrdinalSuffix(int value)
        {
            int lastTwoDigits = value % 100;
            if (lastTwoDigits >= 11 && lastTwoDigits <= 13)
            {
                return "th";
            }

            switch (value % 10)
            {
                case 1: return "st";
                case 2: return "nd";
                case 3: return "rd";
                default: return "th";
            }
        }

        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
            Rect rect = contentRect;
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            float left = PADDING_LEFT;
            float right = rect.width - PADDING_RIGHT;
            float top = PADDING_TOP;
            float bottom = rect.height - PADDING_BOTTOM;
            float stringSpacing = (right - left) / (STRING_COUNT - 1);
            float fretSpacing = (bottom - top) / FRET_COUNT;

            Painter2D painter = context.painter2D;

            DrawFretWires(painter, left, right, top, fretSpacing);
            DrawNut(painter, left, right, top);
            DrawStrings(painter, left, top, bottom, stringSpacing);
            DrawBarre(painter, left, stringSpacing, top, fretSpacing);

            for (int stringIndex = 0; stringIndex < STRING_COUNT; stringIndex++)
            {
                float x = left + stringIndex * stringSpacing;
                int fret = _frets[stringIndex];

                DrawMarker(painter, x, top - MARKER_OFFSET, fret);

                if (fret <= OPEN)
                {
                    continue;
                }

                int windowIndex = fret - _baseFret + 1;
                if (windowIndex < 1 || windowIndex > FRET_COUNT)
                {
                    continue;
                }

                float centerY = top + (windowIndex - 0.5f) * fretSpacing;
                float radius = Mathf.Min(stringSpacing, fretSpacing) * 0.34f;
                painter.fillColor = _dotColor;
                painter.BeginPath();
                painter.Arc(new Vector2(x, centerY), radius, Angle.Degrees(0f), Angle.Degrees(360f));
                painter.Fill();
            }
        }

        /// <summary>
        /// Draws the barre the fingering implies, as a capsule so its ends are rounded.
        /// Called before the dots so the individual notes stay legible on top of it, and
        /// skipped when the barre's fret falls outside the visible window.
        /// </summary>
        private void DrawBarre(Painter2D painter, float left, float stringSpacing, float top, float fretSpacing)
        {
            if (!ChordBarre.TryFind(_frets, out ChordBarre barre))
            {
                return;
            }

            int windowIndex = barre.Fret - _baseFret + 1;
            if (windowIndex < 1 || windowIndex > FRET_COUNT)
            {
                return;
            }

            float firstX = left + (barre.FirstString * stringSpacing);
            float lastX = left + (barre.LastString * stringSpacing);
            float centerY = top + ((windowIndex - 0.5f) * fretSpacing);
            float radius = Mathf.Min(stringSpacing, fretSpacing) * BARRE_THICKNESS * 0.5f;

            painter.fillColor = new Color(_dotColor.r, _dotColor.g, _dotColor.b, BARRE_ALPHA);
            painter.BeginPath();
            painter.MoveTo(new Vector2(firstX, centerY - radius));
            painter.LineTo(new Vector2(lastX, centerY - radius));
            painter.Arc(new Vector2(lastX, centerY), radius, Angle.Degrees(-90f), Angle.Degrees(90f));
            painter.LineTo(new Vector2(firstX, centerY + radius));
            painter.Arc(new Vector2(firstX, centerY), radius, Angle.Degrees(90f), Angle.Degrees(270f));
            painter.ClosePath();
            painter.Fill();
        }

        private static void DrawFretWires(Painter2D painter, float left, float right, float top, float fretSpacing)
        {
            painter.strokeColor = FretWireColor;
            painter.lineWidth = 2f;
            for (int i = 1; i <= FRET_COUNT; i++)
            {
                float y = top + (i * fretSpacing);
                painter.BeginPath();
                painter.MoveTo(new Vector2(left, y));
                painter.LineTo(new Vector2(right, y));
                painter.Stroke();
            }
        }

        private static void DrawNut(Painter2D painter, float left, float right, float top)
        {
            painter.strokeColor = NutColor;
            painter.lineWidth = 6f;
            painter.BeginPath();
            painter.MoveTo(new Vector2(left, top));
            painter.LineTo(new Vector2(right, top));
            painter.Stroke();
        }

        private static void DrawStrings(Painter2D painter, float left, float top, float bottom, float stringSpacing)
        {
            painter.strokeColor = StringColor;
            painter.lineWidth = 2f;
            for (int i = 0; i < STRING_COUNT; i++)
            {
                float x = left + (i * stringSpacing);
                painter.BeginPath();
                painter.MoveTo(new Vector2(x, top));
                painter.LineTo(new Vector2(x, bottom));
                painter.Stroke();
            }
        }

        /// <summary>Draws the X (muted) or O (open) marker above the nut.</summary>
        private static void DrawMarker(Painter2D painter, float x, float y, int fret)
        {
            painter.strokeColor = MarkerColor;
            painter.lineWidth = 2.5f;

            if (fret == MUTED)
            {
                const float arm = MARKER_RADIUS * 0.75f;
                painter.BeginPath();
                painter.MoveTo(new Vector2(x - arm, y - arm));
                painter.LineTo(new Vector2(x + arm, y + arm));
                painter.Stroke();

                painter.BeginPath();
                painter.MoveTo(new Vector2(x + arm, y - arm));
                painter.LineTo(new Vector2(x - arm, y + arm));
                painter.Stroke();
                return;
            }

            if (fret == OPEN)
            {
                painter.BeginPath();
                painter.Arc(new Vector2(x, y), MARKER_RADIUS, Angle.Degrees(0f), Angle.Degrees(360f));
                painter.Stroke();
            }
        }

        private void HandlePointerDown(PointerDownEvent evt)
        {
            if (!_editable)
            {
                return;
            }

            Rect rect = contentRect;
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            float left = PADDING_LEFT;
            float right = rect.width - PADDING_RIGHT;
            float top = PADDING_TOP;
            float bottom = rect.height - PADDING_BOTTOM;
            float stringSpacing = (right - left) / (STRING_COUNT - 1);
            float fretSpacing = (bottom - top) / FRET_COUNT;

            Vector2 local = evt.localPosition;
            int stringIndex = Mathf.RoundToInt((local.x - left) / stringSpacing);
            if (stringIndex < 0 || stringIndex >= STRING_COUNT)
            {
                return;
            }

            if (Mathf.Abs(local.x - (left + stringIndex * stringSpacing)) > stringSpacing * 0.5f)
            {
                return;
            }

            if (local.y < top)
            {
                if (local.y >= top - MARKER_BAND_HEIGHT)
                {
                    StringMarkerTapped?.Invoke(stringIndex);
                }

                return;
            }

            if (local.y > bottom)
            {
                return;
            }

            int windowIndex = Mathf.Clamp(Mathf.FloorToInt((local.y - top) / fretSpacing), 0, FRET_COUNT - 1);
            FretTapped?.Invoke(stringIndex, _baseFret + windowIndex);
        }

        /// <summary>
        /// Square shift button drawn with Painter2D so it never depends on the
        /// project font containing an arrow glyph.
        /// </summary>
        private class FretShiftButton : Button
        {
            private static readonly Color ArrowColor = new Color(0.94f, 0.72f, 0.26f, 1f);

            private readonly bool _pointsUp;

            public FretShiftButton(bool pointsUp, Action onClick)
            {
                _pointsUp = pointsUp;
                focusable = false;
                AddToClassList("chord-maker-fret-shift-btn");
                clicked += onClick;
                generateVisualContent += DrawArrow;

                // Keep the neck's fret hit-testing from also reacting to a tap on this button.
                RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());
            }

            private void DrawArrow(MeshGenerationContext context)
            {
                Rect rect = contentRect;
                if (rect.width <= 0f || rect.height <= 0f)
                {
                    return;
                }

                float half = Mathf.Min(rect.width, rect.height) * 0.2f;
                Vector2 center = rect.center;
                float tipY = _pointsUp ? center.y - half : center.y + half;
                float baseY = _pointsUp ? center.y + half : center.y - half;

                Painter2D painter = context.painter2D;
                painter.fillColor = ArrowColor;
                painter.BeginPath();
                painter.MoveTo(new Vector2(center.x, tipY));
                painter.LineTo(new Vector2(center.x + half, baseY));
                painter.LineTo(new Vector2(center.x - half, baseY));
                painter.ClosePath();
                painter.Fill();
            }
        }
    }
}
