using UnityEngine;
using UnityEngine.UIElements;

namespace Chordyceps.UI
{
    /// <summary>
    /// Draws a single guitar string as a UI Toolkit line and reproduces the
    /// world-space LineRenderer pluck wobble previously owned by GuitarString.
    /// </summary>
    public class GuitarStringElement : VisualElement
    {
        private const int POINT_COUNT = 11;
        private const float WOBBLE_DURATION = 0.25f;
        private const float WOBBLE_FREQUENCY = 40f;
        private const float WOBBLE_AMPLITUDE_RATIO = 0.01f;
        private const float MAX_STRING_WIDTH = 5f;
        private const float MIN_STRING_WIDTH = 2f;
        private const float EDGE_PADDING = 12f;
        private const long TICK_INTERVAL_MS = 16;

        private static readonly Color NormalColor = new Color(0.85f, 0.85f, 0.9f, 0.85f);
        private static readonly Color HighlightColor = new Color(1f, 1f, 0.65f, 1f);

        private readonly int _stringIndex;
        private readonly float _lineWidth;

        private IVisualElementScheduledItem _wobbleTicker;
        private float _wobbleElapsed = -1f;

        public int StringIndex => _stringIndex;

        public GuitarStringElement(int stringIndex, int stringCount)
        {
            _stringIndex = stringIndex;
            _lineWidth = CalculateLineWidth(stringCount);

            pickingMode = PickingMode.Position;
            style.flexGrow = 1f;
            style.flexShrink = 0f;

            generateVisualContent += DrawString;
        }

        /// <summary>Starts the decaying sine-wave wobble, matching GuitarString.</summary>
        public void PlayPluckFeedback()
        {
            _wobbleElapsed = 0f;

            if (_wobbleTicker == null)
            {
                _wobbleTicker = schedule.Execute(AdvanceWobble).Every(TICK_INTERVAL_MS);
                _wobbleTicker.Pause();
            }

            _wobbleTicker.Resume();
            MarkDirtyRepaint();
        }

        private void AdvanceWobble()
        {
            _wobbleElapsed += Time.deltaTime;

            if (_wobbleElapsed >= WOBBLE_DURATION)
            {
                _wobbleElapsed = -1f;
                _wobbleTicker.Pause();
            }

            MarkDirtyRepaint();
        }

        private void DrawString(MeshGenerationContext context)
        {
            Rect rect = contentRect;
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            float centerX = rect.width * 0.5f;
            float centerY = rect.height * 0.5f;
            float halfLength = Mathf.Max(0f, centerX - EDGE_PADDING);
            float amplitude = rect.width * WOBBLE_AMPLITUDE_RATIO;

            Painter2D painter = context.painter2D;
            painter.lineWidth = _lineWidth;
            painter.lineCap = LineCap.Round;
            painter.strokeColor = IsWobbling ? HighlightColor : NormalColor;

            painter.BeginPath();

            for (int i = 0; i < POINT_COUNT; i++)
            {
                float t = (float)i / (POINT_COUNT - 1);
                float x = centerX + Mathf.Lerp(-halfLength, halfLength, t);
                float y = centerY + CalculateWobbleOffset(t, amplitude);

                if (i == 0)
                {
                    painter.MoveTo(new Vector2(x, y));
                }
                else
                {
                    painter.LineTo(new Vector2(x, y));
                }
            }

            painter.Stroke();
        }

        private bool IsWobbling => _wobbleElapsed >= 0f;

        private float CalculateWobbleOffset(float t, float amplitude)
        {
            if (!IsWobbling)
            {
                return 0f;
            }

            float progress = Mathf.Clamp01(_wobbleElapsed / WOBBLE_DURATION);
            float decay = 1f - progress;
            float wave = Mathf.Sin(_wobbleElapsed * WOBBLE_FREQUENCY) * amplitude * decay;
            float envelope = Mathf.Sin(t * Mathf.PI);
            return wave * envelope;
        }

        private float CalculateLineWidth(int stringCount)
        {
            float t = (float)_stringIndex / Mathf.Max(1, stringCount - 1);
            return Mathf.Lerp(MAX_STRING_WIDTH, MIN_STRING_WIDTH, t);
        }
    }
}
