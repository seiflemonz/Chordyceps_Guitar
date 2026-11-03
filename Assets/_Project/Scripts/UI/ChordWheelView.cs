using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chordyceps.UI
{
    /// <summary>
    /// How large a wheel is drawn. The main HUD uses <see cref="Hud"/>; the Wheel Maker
    /// uses a larger set so its wheel can be rendered at its own size instead of being
    /// scaled up as a bitmap.
    /// </summary>
    public readonly struct ChordWheelMetrics
    {
        public readonly float Center;
        public readonly float InnerRadius;
        public readonly float OuterRadius;
        public readonly float LabelRadius;
        public readonly float StrokeIdle;
        public readonly float StrokeActive;

        public ChordWheelMetrics(float center, float innerRadius, float outerRadius, float labelRadius, float strokeIdle, float strokeActive)
        {
            Center = center;
            InnerRadius = innerRadius;
            OuterRadius = outerRadius;
            LabelRadius = labelRadius;
            StrokeIdle = strokeIdle;
            StrokeActive = strokeActive;
        }

        /// <summary>The main HUD's 380x380 wheel.</summary>
        public static readonly ChordWheelMetrics Hud = new ChordWheelMetrics(190f, 72f, 162f, 117f, 1.5f, 3.5f);
    }

    /// <summary>
    /// The six-sector chord wheel shared by the main HUD and the Wheel Maker. Binds the
    /// wheel markup inside a document, positions the six label boxes and draws the sectors
    /// with Painter2D. It holds no chord data of its own — callers push labels in.
    ///
    /// Expected element names, which both wheels must provide:
    /// "chord-wheel-key-name", "sector-N", "label-box-N", "roman-N" and "name-N" for N in 0..5.
    /// The roman labels are optional; a wheel without them shows only chord names.
    /// </summary>
    public class ChordWheelView
    {
        public const int SECTOR_COUNT = 6;

        private const int MAJOR_COUNT = 3;
        private const string HEADER_NAME = "chord-wheel-key-name";
        private const string HEADER_PREFIX_NAME = "chord-wheel-header-prefix";

        private static readonly float[] _startAngles = new float[] { 180f, 240f, 300f, 120f, 60f, 0f };
        private static readonly float[] _endAngles = new float[] { 240f, 300f, 360f, 180f, 120f, 60f };
        private static readonly float[] _midAngles = new float[] { 210f, 270f, 330f, 150f, 90f, 30f };

        // Maps the 60° wedge a tap falls in (0–60, 60–120, …) to the sector drawn there.
        private static readonly int[] _bucketToSlot = new int[] { 5, 4, 3, 0, 1, 2 };

        private static readonly Color _majorStroke = new Color(0.83f, 0.63f, 0.26f, 0.70f);
        private static readonly Color _minorStroke = new Color(0.40f, 0.52f, 0.64f, 0.70f);
        private static readonly Color _activeStroke = new Color(1.00f, 0.85f, 0.35f, 1.00f);
        private static readonly Color _idleFill = new Color(0.07f, 0.09f, 0.13f, 0.90f);
        private static readonly Color _activeFill = new Color(0.88f, 0.55f, 0.12f, 0.48f);

        private readonly ChordWheelMetrics _metrics;
        private readonly VisualElement[] _sectorElements = new VisualElement[SECTOR_COUNT];
        private readonly VisualElement[] _labelBoxes = new VisualElement[SECTOR_COUNT];
        private readonly Label[] _romanLabels = new Label[SECTOR_COUNT];
        private readonly Label[] _nameLabels = new Label[SECTOR_COUNT];
        private readonly Label _headerLabel;
        private readonly Label _headerPrefixLabel;

        // Sector 1 carries the active styling in the HUD markup, so start there and let the
        // first SetActiveSlot move it; that is what clears the baked-in class.
        private int _activeSlotIndex = 1;

        /// <summary>Raised when a sector is tapped. Only wired when the view is interactive.</summary>
        public event Action<int> SectorTapped;

        public int ActiveSlotIndex => _activeSlotIndex;

        public ChordWheelView(VisualElement root, ChordWheelMetrics metrics, bool interactive)
        {
            _metrics = metrics;
            _headerLabel = root.Q<Label>(HEADER_NAME);
            _headerPrefixLabel = root.Q<Label>(HEADER_PREFIX_NAME);

            for (int slot = 0; slot < SECTOR_COUNT; slot++)
            {
                BindSector(root, slot);
                PositionLabelBox(slot);
            }

            if (interactive)
            {
                EnableInteraction(root);
            }
        }

        private void BindSector(VisualElement root, int slot)
        {
            VisualElement sector = root.Q($"sector-{slot}");
            _sectorElements[slot] = sector;
            _labelBoxes[slot] = root.Q($"label-box-{slot}");
            _romanLabels[slot] = root.Q<Label>($"roman-{slot}");
            _nameLabels[slot] = root.Q<Label>($"name-{slot}");

            if (sector != null)
            {
                sector.generateVisualContent += mgc => DrawSector(mgc, slot);
            }
        }

        /// <summary>
        /// Every sector element spans the whole wheel, so hit-testing by element would always
        /// report whichever was added last. The wheel itself takes the tap instead, and the
        /// sector is derived from where the pointer sits around the centre.
        /// </summary>
        private void EnableInteraction(VisualElement root)
        {
            root.pickingMode = PickingMode.Position;
            for (int slot = 0; slot < SECTOR_COUNT; slot++)
            {
                if (_sectorElements[slot] != null)
                {
                    _sectorElements[slot].pickingMode = PickingMode.Ignore;
                }
            }

            root.RegisterCallback<PointerDownEvent>(HandleWheelPointerDown);
        }

        private void HandleWheelPointerDown(PointerDownEvent evt)
        {
            var centre = new Vector2(_metrics.Center, _metrics.Center);
            Vector2 offset = (Vector2)evt.localPosition - centre;
            float radius = offset.magnitude;
            if (radius < _metrics.InnerRadius || radius > _metrics.OuterRadius)
            {
                return;
            }

            float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            if (angle < 0f)
            {
                angle += 360f;
            }

            int bucket = Mathf.Clamp((int)(angle / 60f), 0, SECTOR_COUNT - 1);
            SectorTapped?.Invoke(_bucketToSlot[bucket]);
        }

        private void PositionLabelBox(int slot)
        {
            VisualElement box = _labelBoxes[slot];
            if (box == null)
            {
                return;
            }

            float angleRad = _midAngles[slot] * Mathf.Deg2Rad;
            box.style.left = _metrics.Center + _metrics.LabelRadius * Mathf.Cos(angleRad);
            box.style.top = _metrics.Center + _metrics.LabelRadius * Mathf.Sin(angleRad);
        }

        public void SetHeader(string text)
        {
            if (_headerLabel != null)
            {
                _headerLabel.text = text ?? string.Empty;
            }
        }

        /// <summary>
        /// Sets the label before the header, e.g. "KEY: " or "WHEEL: ". Wheels without a
        /// prefix label ignore this.
        /// </summary>
        public void SetHeaderPrefix(string text)
        {
            if (_headerPrefixLabel != null)
            {
                _headerPrefixLabel.text = text ?? string.Empty;
            }
        }

        /// <summary>Sets one sector's labels. Passing null or empty clears that label.</summary>
        public void SetSlot(int slot, string roman, string name)
        {
            if (slot < 0 || slot >= SECTOR_COUNT)
            {
                return;
            }

            if (_romanLabels[slot] != null)
            {
                _romanLabels[slot].text = roman ?? string.Empty;
            }

            if (_nameLabels[slot] != null)
            {
                _nameLabels[slot].text = name ?? string.Empty;
            }
        }

        /// <summary>Highlights one sector, or clears the highlight when passed a negative index.</summary>
        public void SetActiveSlot(int index)
        {
            if (index >= SECTOR_COUNT)
            {
                return;
            }

            if (index < 0)
            {
                index = -1;
            }

            if (index == _activeSlotIndex)
            {
                return;
            }

            int previous = _activeSlotIndex;
            _activeSlotIndex = index;
            ToggleSectorVisualClasses(previous, false);
            ToggleSectorVisualClasses(_activeSlotIndex, true);
            RepaintSector(previous);
            RepaintSector(_activeSlotIndex);
        }

        private void ToggleSectorVisualClasses(int slot, bool isActive)
        {
            if (slot < 0 || slot >= SECTOR_COUNT)
            {
                return;
            }

            _sectorElements[slot]?.EnableInClassList("chord-sector--active", isActive);
            _romanLabels[slot]?.EnableInClassList("chord-sector__roman--active", isActive);
            _nameLabels[slot]?.EnableInClassList("chord-sector__name--active", isActive);
        }

        private void RepaintSector(int slot)
        {
            if (slot >= 0 && slot < SECTOR_COUNT)
            {
                _sectorElements[slot]?.MarkDirtyRepaint();
            }
        }

        private void DrawSector(MeshGenerationContext mgc, int slot)
        {
            Painter2D painter = mgc.painter2D;
            bool isActive = (slot == _activeSlotIndex);
            var center = new Vector2(_metrics.Center, _metrics.Center);

            if (isActive)
            {
                DrawGlowBloom(painter, center, slot);
            }

            ApplySectorColors(painter, slot, isActive);
            BuildSectorPath(painter, center, _metrics.InnerRadius, _metrics.OuterRadius, slot);
            painter.Fill();
            painter.Stroke();
        }

        // The bloom keeps the same proportions to the wheel as the HUD's original 380px
        // wheel, so it grows with the radius instead of thinning out on the large wheel.
        private void DrawGlowBloom(Painter2D p, Vector2 c, int slot)
        {
            float outer = _metrics.OuterRadius;

            p.strokeColor = new Color(0.96f, 0.72f, 0.18f, 0.08f);
            p.lineWidth = outer * 0.1605f;
            DrawArcStrokeOnly(p, c, outer * 1.0247f, slot);

            p.strokeColor = new Color(0.98f, 0.78f, 0.22f, 0.18f);
            p.lineWidth = outer * 0.0864f;
            DrawArcStrokeOnly(p, c, outer * 1.0123f, slot);

            p.strokeColor = new Color(1.00f, 0.85f, 0.32f, 0.35f);
            p.lineWidth = outer * 0.0370f;
            DrawArcStrokeOnly(p, c, outer, slot);
        }

        private static void DrawArcStrokeOnly(Painter2D p, Vector2 c, float radius, int slot)
        {
            p.BeginPath();
            p.Arc(c, radius, Angle.Degrees(_startAngles[slot]), Angle.Degrees(_endAngles[slot]), ArcDirection.Clockwise);
            p.Stroke();
        }

        private void ApplySectorColors(Painter2D painter, int slot, bool isActive)
        {
            painter.lineWidth = isActive ? _metrics.StrokeActive : _metrics.StrokeIdle;
            if (isActive)
            {
                painter.strokeColor = _activeStroke;
                painter.fillColor = _activeFill;
                return;
            }

            bool isMajor = slot < MAJOR_COUNT;
            painter.strokeColor = isMajor ? _majorStroke : _minorStroke;
            painter.fillColor = _idleFill;
        }

        private static void BuildSectorPath(Painter2D p, Vector2 c, float innerRadius, float outerRadius, int slot)
        {
            p.BeginPath();
            p.Arc(c, outerRadius, Angle.Degrees(_startAngles[slot]), Angle.Degrees(_endAngles[slot]), ArcDirection.Clockwise);
            p.Arc(c, innerRadius, Angle.Degrees(_endAngles[slot]), Angle.Degrees(_startAngles[slot]), ArcDirection.CounterClockwise);
            p.ClosePath();
        }
    }
}
