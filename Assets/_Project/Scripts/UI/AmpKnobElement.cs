using System;
using UnityEngine;
using UnityEngine.UIElements;
using Chordyceps.Audio;

namespace Chordyceps.UI
{
    /// <summary>
    /// Reusable rotary knob drawn with Painter2D. Dragging vertically changes
    /// the value, reported as a normalised 0..1 float.
    /// </summary>
    public class AmpKnobElement : VisualElement
    {
        public const string USS_CLASS_NAME = "amp-knob";

        private const string LABEL_CLASS_NAME = "amp-knob__label";
        private const string VALUE_CLASS_NAME = "amp-knob__value";
        private const string DIAL_CLASS_NAME = "amp-knob__dial";

        private const float MIN_ANGLE_DEGREES = -135f;
        private const float MAX_ANGLE_DEGREES = 135f;
        private const float DRAG_RANGE_PX = 180f;
        private const float TRACK_OUTER_RADIUS_RATIO = 0.46f;
        private const float TRACK_WIDTH_RATIO = 0.10f;
        private const float TRACK_START_DEGREES = 135f;
        private const float TRACK_SWEEP_DEGREES = 270f;
        private const int ARC_SEGMENTS = 48;

        private static readonly Color TrackColor = new Color(1f, 1f, 1f, 0.10f);
        private static readonly Color FillColor = new Color(0.94f, 0.72f, 0.26f, 1f);
        private static readonly Color PointerColor = new Color(0.95f, 0.95f, 0.97f, 1f);

        private readonly AmpParameter _parameter;
        private readonly VisualElement _dial;
        private readonly Label _valueLabel;

        private float _value;
        private float _dragStartValue;
        private float _dragStartY;

        public event Action<AmpParameter, float> ValueChanged;

        public AmpParameter Parameter => _parameter;

        public float Value
        {
            get => _value;
            set => SetValue(value, false);
        }

        public AmpKnobElement(AmpParameter parameter, float initialValue)
        {
            _parameter = parameter;

            AddToClassList(USS_CLASS_NAME);
            pickingMode = PickingMode.Position;

            Label label = new Label(AmpMixerParameters.GetDisplayName(parameter));
            label.AddToClassList(LABEL_CLASS_NAME);
            label.pickingMode = PickingMode.Ignore;
            Add(label);

            _dial = new VisualElement();
            _dial.AddToClassList(DIAL_CLASS_NAME);
            _dial.pickingMode = PickingMode.Ignore;
            _dial.generateVisualContent += DrawDial;
            Add(_dial);

            _valueLabel = new Label();
            _valueLabel.AddToClassList(VALUE_CLASS_NAME);
            _valueLabel.pickingMode = PickingMode.Ignore;
            Add(_valueLabel);

            RegisterCallback<PointerDownEvent>(HandlePointerDown);
            RegisterCallback<PointerMoveEvent>(HandlePointerMove);
            RegisterCallback<PointerUpEvent>(HandlePointerUp);
            RegisterCallback<PointerCancelEvent>(HandlePointerCancel);

            _value = Mathf.Clamp01(initialValue);
            RefreshVisuals();
        }

        private void HandlePointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0)
            {
                return;
            }

            PointerCaptureHelper.CapturePointer(this, evt.pointerId);
            _dragStartValue = _value;
            _dragStartY = evt.position.y;
            evt.StopPropagation();
        }

        private void HandlePointerMove(PointerMoveEvent evt)
        {
            if (!PointerCaptureHelper.HasPointerCapture(this, evt.pointerId))
            {
                return;
            }

            float delta = (_dragStartY - evt.position.y) / DRAG_RANGE_PX;
            SetValue(_dragStartValue + delta, true);
            evt.StopPropagation();
        }

        private void HandlePointerUp(PointerUpEvent evt)
        {
            if (PointerCaptureHelper.HasPointerCapture(this, evt.pointerId))
            {
                PointerCaptureHelper.ReleasePointer(this, evt.pointerId);
            }
        }

        private void HandlePointerCancel(PointerCancelEvent evt)
        {
            if (PointerCaptureHelper.HasPointerCapture(this, evt.pointerId))
            {
                PointerCaptureHelper.ReleasePointer(this, evt.pointerId);
            }
        }

        private void SetValue(float value, bool notify)
        {
            float clamped = Mathf.Clamp01(value);
            if (Mathf.Approximately(clamped, _value))
            {
                return;
            }

            _value = clamped;
            RefreshVisuals();

            if (notify)
            {
                ValueChanged?.Invoke(_parameter, _value);
            }
        }

        private void RefreshVisuals()
        {
            _valueLabel.text = Mathf.RoundToInt(_value * 100f).ToString();
            _dial.MarkDirtyRepaint();
        }

        private void DrawDial(MeshGenerationContext context)
        {
            Rect rect = _dial.contentRect;
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            Vector2 center = new Vector2(rect.width * 0.5f, rect.height * 0.5f);
            float radius = Mathf.Min(rect.width, rect.height) * TRACK_OUTER_RADIUS_RATIO;
            Painter2D painter = context.painter2D;
            painter.lineCap = LineCap.Round;

            DrawTrack(painter, center, radius);
            DrawValueArc(painter, center, radius);
            DrawPointer(painter, center, radius);
        }

        private void DrawTrack(Painter2D painter, Vector2 center, float radius)
        {
            painter.strokeColor = TrackColor;
            painter.lineWidth = radius * TRACK_WIDTH_RATIO;
            painter.BeginPath();
            painter.Arc(center, radius, TRACK_START_DEGREES, TRACK_START_DEGREES + TRACK_SWEEP_DEGREES);
            painter.Stroke();
        }

        private void DrawValueArc(Painter2D painter, Vector2 center, float radius)
        {
            if (_value <= 0f)
            {
                return;
            }

            painter.strokeColor = FillColor;
            painter.lineWidth = radius * TRACK_WIDTH_RATIO;
            painter.BeginPath();
            painter.Arc(center, radius, TRACK_START_DEGREES,
                TRACK_START_DEGREES + TRACK_SWEEP_DEGREES * _value, ArcDirection.Clockwise);
            painter.Stroke();
        }

        private void DrawPointer(Painter2D painter, Vector2 center, float radius)
        {
            float angle = Mathf.Lerp(MIN_ANGLE_DEGREES, MAX_ANGLE_DEGREES, _value);
            float radians = angle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Sin(radians), -Mathf.Cos(radians));

            painter.strokeColor = PointerColor;
            painter.lineWidth = Mathf.Max(2f, radius * 0.09f);
            painter.BeginPath();
            painter.MoveTo(center + direction * (radius * 0.45f));
            painter.LineTo(center + direction * (radius * 0.82f));
            painter.Stroke();
        }
    }
}
