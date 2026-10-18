using UnityEngine;
using UnityEngine.UIElements;
using Chordyceps.Chords;

namespace Chordyceps.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class ChordWheelController : MonoBehaviour
    {
        [SerializeField] private ChordManager _chordManager;

        private const int SECTOR_COUNT = 6;
        private const int MAJOR_COUNT = 3;
        private const float CENTER_COORD = 190f;
        private const float INNER_RADIUS = 72f;
        private const float OUTER_RADIUS = 162f;
        private const float LABEL_RADIUS = 117f;
        private const float STROKE_IDLE = 1.5f;
        private const float STROKE_ACTIVE = 3.5f;

        private static readonly float[] _startAngles = new float[] { 180f, 240f, 300f, 120f, 60f, 0f };
        private static readonly float[] _endAngles = new float[] { 240f, 300f, 360f, 180f, 120f, 60f };
        private static readonly float[] _midAngles = new float[] { 210f, 270f, 330f, 150f, 90f, 30f };

        private static readonly Color _majorStroke = new Color(0.83f, 0.63f, 0.26f, 0.70f);
        private static readonly Color _minorStroke = new Color(0.40f, 0.52f, 0.64f, 0.70f);
        private static readonly Color _activeStroke = new Color(1.00f, 0.85f, 0.35f, 1.00f);
        private static readonly Color _idleFill = new Color(0.07f, 0.09f, 0.13f, 0.90f);
        private static readonly Color _activeFill = new Color(0.88f, 0.55f, 0.12f, 0.48f);

        private UIDocument _uiDocument;
        private VisualElement[] _sectorElements = new VisualElement[SECTOR_COUNT];
        private VisualElement[] _labelBoxes = new VisualElement[SECTOR_COUNT];
        private Label[] _romanLabels = new Label[SECTOR_COUNT];
        private Label[] _nameLabels = new Label[SECTOR_COUNT];
        private Label _keyNameLabel;
        private int _activeSlotIndex = 1;

        private void Awake()
        {
            CacheReferences();
            InitializeElementBindings();
        }

        private void CacheReferences()
        {
            _uiDocument = GetComponent<UIDocument>();
            if (_chordManager == null)
            {
                _chordManager = FindFirstObjectByType<ChordManager>();
            }
        }

        private void InitializeElementBindings()
        {
            VisualElement root = _uiDocument.rootVisualElement;
            _keyNameLabel = root.Q<Label>("chord-wheel-key-name");
            for (int i = 0; i < SECTOR_COUNT; i++)
            {
                BindSectorElements(root, i);
                PositionLabelBox(i);
            }
        }

        private void BindSectorElements(VisualElement root, int slotIndex)
        {
            VisualElement sector = root.Q($"sector-{slotIndex}");
            _sectorElements[slotIndex] = sector;
            _labelBoxes[slotIndex] = root.Q($"label-box-{slotIndex}");
            _romanLabels[slotIndex] = root.Q<Label>($"roman-{slotIndex}");
            _nameLabels[slotIndex] = root.Q<Label>($"name-{slotIndex}");

            sector.generateVisualContent += (mgc) => DrawSector(mgc, slotIndex);
        }

        private void PositionLabelBox(int slotIndex)
        {
            VisualElement box = _labelBoxes[slotIndex];
            if (box == null) return;

            float angleRad = _midAngles[slotIndex] * Mathf.Deg2Rad;
            float x = CENTER_COORD + LABEL_RADIUS * Mathf.Cos(angleRad);
            float y = CENTER_COORD + LABEL_RADIUS * Mathf.Sin(angleRad);
            box.style.left = x;
            box.style.top = y;
        }

        private void OnEnable()
        {
            SubscribeToEvents();
            SyncInitialState();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (_chordManager == null) return;
            _chordManager.OnScaleChanged += HandleScaleChanged;
            _chordManager.OnChordChanged += HandleChordChanged;
        }

        private void UnsubscribeFromEvents()
        {
            if (_chordManager == null) return;
            _chordManager.OnScaleChanged -= HandleScaleChanged;
            _chordManager.OnChordChanged -= HandleChordChanged;
        }

        private void SyncInitialState()
        {
            if (_chordManager == null) return;
            if (_chordManager.CurrentScale != null)
            {
                HandleScaleChanged(_chordManager.CurrentScale);
            }
            SetActiveSlot(_chordManager.CurrentChordIndex);
        }

        private void HandleScaleChanged(ScaleDefinition scale)
        {
            if (scale == null) return;
            if (_keyNameLabel != null)
            {
                _keyNameLabel.text = scale.ScaleName;
            }

            for (int i = 0; i < SECTOR_COUNT; i++)
            {
                UpdateSlotLabels(scale, i);
            }
        }

        private void UpdateSlotLabels(ScaleDefinition scale, int slotIndex)
        {
            ChordDefinition chord = scale.GetChord(slotIndex);
            if (chord == null) return;

            _romanLabels[slotIndex].text = scale.GetRomanNumeral(slotIndex);
            _nameLabels[slotIndex].text = chord.ChordName;
        }

        private void HandleChordChanged(ChordDefinition chord)
        {
            if (_chordManager == null) return;
            SetActiveSlot(_chordManager.CurrentChordIndex);
        }

        private void SetActiveSlot(int newIndex)
        {
            if (newIndex < 0 || newIndex >= SECTOR_COUNT) return;
            int prevIndex = _activeSlotIndex;
            _activeSlotIndex = newIndex;

            ToggleSectorVisualClasses(prevIndex, false);
            ToggleSectorVisualClasses(_activeSlotIndex, true);
            RepaintSector(prevIndex);
            RepaintSector(_activeSlotIndex);
        }

        private void ToggleSectorVisualClasses(int slotIndex, bool isActive)
        {
            if (slotIndex < 0 || slotIndex >= SECTOR_COUNT) return;
            _sectorElements[slotIndex]?.EnableInClassList("chord-sector--active", isActive);
            _romanLabels[slotIndex]?.EnableInClassList("chord-sector__roman--active", isActive);
            _nameLabels[slotIndex]?.EnableInClassList("chord-sector__name--active", isActive);
        }

        private void RepaintSector(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < SECTOR_COUNT && _sectorElements[slotIndex] != null)
            {
                _sectorElements[slotIndex].MarkDirtyRepaint();
            }
        }

        private void DrawSector(MeshGenerationContext mgc, int slotIndex)
        {
            Painter2D painter = mgc.painter2D;
            bool isActive = (slotIndex == _activeSlotIndex);
            Vector2 center = new Vector2(CENTER_COORD, CENTER_COORD);

            if (isActive)
            {
                DrawGlowBloom(painter, center, slotIndex);
            }

            ApplySectorColors(painter, slotIndex, isActive);
            BuildSectorPath(painter, center, INNER_RADIUS, OUTER_RADIUS, slotIndex);
            painter.Fill();
            painter.Stroke();
        }

        private void DrawGlowBloom(Painter2D p, Vector2 c, int i)
        {
            p.strokeColor = new Color(0.96f, 0.72f, 0.18f, 0.08f);
            p.lineWidth = 26f;
            DrawArcStrokeOnly(p, c, OUTER_RADIUS + 4f, i);

            p.strokeColor = new Color(0.98f, 0.78f, 0.22f, 0.18f);
            p.lineWidth = 14f;
            DrawArcStrokeOnly(p, c, OUTER_RADIUS + 2f, i);

            p.strokeColor = new Color(1.00f, 0.85f, 0.32f, 0.35f);
            p.lineWidth = 6f;
            DrawArcStrokeOnly(p, c, OUTER_RADIUS, i);
        }

        private void DrawArcStrokeOnly(Painter2D p, Vector2 c, float radius, int i)
        {
            p.BeginPath();
            p.Arc(c, radius, Angle.Degrees(_startAngles[i]), Angle.Degrees(_endAngles[i]), ArcDirection.Clockwise);
            p.Stroke();
        }

        private void ApplySectorColors(Painter2D painter, int slotIndex, bool isActive)
        {
            painter.lineWidth = isActive ? STROKE_ACTIVE : STROKE_IDLE;
            if (isActive)
            {
                painter.strokeColor = _activeStroke;
                painter.fillColor = _activeFill;
                return;
            }

            bool isMajor = (slotIndex < MAJOR_COUNT);
            painter.strokeColor = isMajor ? _majorStroke : _minorStroke;
            painter.fillColor = _idleFill;
        }

        private void BuildSectorPath(Painter2D p, Vector2 c, float inR, float outR, int i)
        {
            p.BeginPath();
            p.Arc(c, outR, Angle.Degrees(_startAngles[i]), Angle.Degrees(_endAngles[i]), ArcDirection.Clockwise);
            p.Arc(c, inR, Angle.Degrees(_endAngles[i]), Angle.Degrees(_startAngles[i]), ArcDirection.CounterClockwise);
            p.ClosePath();
        }
    }
}
