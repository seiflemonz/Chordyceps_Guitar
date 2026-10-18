using UnityEngine;
using UnityEngine.UIElements;
using Chordyceps.Chords;

namespace Chordyceps.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class ChordShapeSelectorController : MonoBehaviour
    {
        [SerializeField] private ChordManager _chordManager;

        private const int SectorCount = 6;
        private const float CenterCoordinate = 150f;
        private const float InnerRadius = 62f;
        private const float OuterRadius = 132f;
        private const float LabelRadius = 96f;
        private const float DeadZoneRadius = 18f;
        private const float ThumbTravelRadius = 42f;

        private static readonly int[] _slotBySector = { 1, 2, 5, 4, 3, 0 };

        private UIDocument _uiDocument;
        private VisualElement _dial;
        private VisualElement _thumb;
        private Label[] _degreeLabels = new Label[SectorCount];
        private int _activeSector;
        private int _activePointerId = -1;

        private void Awake()
        {
            CacheReferences();
            BindElements();
            PositionDegreeLabels();
        }

        private void CacheReferences()
        {
            _uiDocument = GetComponent<UIDocument>();
            if (_chordManager == null)
            {
                _chordManager = FindFirstObjectByType<ChordManager>();
            }
        }

        private void BindElements()
        {
            VisualElement root = _uiDocument.rootVisualElement;
            _dial = root.Q("shape-selector-dial");
            _thumb = root.Q("shape-selector-thumb");

            for (int i = 0; i < SectorCount; i++)
            {
                _degreeLabels[i] = root.Q<Label>($"shape-degree-{i}");
            }

            Debug.Assert(_dial != null, "ChordShapeSelectorController requires shape-selector-dial.");
            Debug.Assert(_thumb != null, "ChordShapeSelectorController requires shape-selector-thumb.");
        }

        private void PositionDegreeLabels()
        {
            for (int i = 0; i < SectorCount; i++)
            {
                if (_degreeLabels[i] == null) continue;

                Vector2 position = GetPositionOnDial(i, LabelRadius);
                _degreeLabels[i].style.left = position.x - 24f;
                _degreeLabels[i].style.top = position.y - 24f;
            }
        }

        private void OnEnable()
        {
            RegisterDialCallbacks();
            SubscribeToChordManager();
            SyncInitialState();
        }

        private void OnDisable()
        {
            UnregisterDialCallbacks();
            UnsubscribeFromChordManager();
            ReleaseActivePointer();
        }

        private void OnDestroy()
        {
            if (_dial != null)
            {
                _dial.generateVisualContent -= DrawDial;
            }
        }

        private void RegisterDialCallbacks()
        {
            if (_dial == null) return;

            _dial.generateVisualContent += DrawDial;
            _dial.RegisterCallback<PointerDownEvent>(HandlePointerDown);
            _dial.RegisterCallback<PointerMoveEvent>(HandlePointerMove);
            _dial.RegisterCallback<PointerUpEvent>(HandlePointerUp);
            _dial.RegisterCallback<PointerCancelEvent>(HandlePointerCancelled);
        }

        private void UnregisterDialCallbacks()
        {
            if (_dial == null) return;

            _dial.generateVisualContent -= DrawDial;
            _dial.UnregisterCallback<PointerDownEvent>(HandlePointerDown);
            _dial.UnregisterCallback<PointerMoveEvent>(HandlePointerMove);
            _dial.UnregisterCallback<PointerUpEvent>(HandlePointerUp);
            _dial.UnregisterCallback<PointerCancelEvent>(HandlePointerCancelled);
        }

        private void SubscribeToChordManager()
        {
            if (_chordManager != null)
            {
                _chordManager.OnChordChanged += HandleChordChanged;
            }
        }

        private void UnsubscribeFromChordManager()
        {
            if (_chordManager != null)
            {
                _chordManager.OnChordChanged -= HandleChordChanged;
            }
        }

        private void SyncInitialState()
        {
            if (_chordManager != null)
            {
                SetActiveSlot(_chordManager.CurrentChordIndex);
            }
            ResetThumb();
        }

        private void HandlePointerDown(PointerDownEvent evt)
        {
            if (_activePointerId >= 0) return;

            _activePointerId = evt.pointerId;
            _dial.CapturePointer(_activePointerId);
            UpdateSelection(evt.localPosition);
            evt.StopPropagation();
        }

        private void HandlePointerMove(PointerMoveEvent evt)
        {
            if (evt.pointerId != _activePointerId) return;

            UpdateSelection(evt.localPosition);
            evt.StopPropagation();
        }

        private void HandlePointerUp(PointerUpEvent evt)
        {
            if (evt.pointerId != _activePointerId) return;

            ReleaseActivePointer();
            ResetThumb();
            evt.StopPropagation();
        }

        private void HandlePointerCancelled(PointerCancelEvent evt)
        {
            if (evt.pointerId != _activePointerId) return;

            ReleaseActivePointer();
            ResetThumb();
        }

        private void UpdateSelection(Vector2 localPosition)
        {
            Vector2 offset = localPosition - new Vector2(CenterCoordinate, CenterCoordinate);
            MoveThumb(offset);
            if (offset.magnitude < DeadZoneRadius) return;

            int sectorIndex = GetSectorIndex(offset);
            _chordManager?.SelectChord(_slotBySector[sectorIndex]);
        }

        private void MoveThumb(Vector2 offset)
        {
            if (_thumb == null) return;

            Vector2 clampedOffset = Vector2.ClampMagnitude(offset, ThumbTravelRadius);
            _thumb.style.left = CenterCoordinate + clampedOffset.x - 32f;
            _thumb.style.top = CenterCoordinate + clampedOffset.y - 32f;
        }

        private void ResetThumb()
        {
            if (_thumb == null) return;

            _thumb.style.left = CenterCoordinate - 32f;
            _thumb.style.top = CenterCoordinate - 32f;
        }

        private void ReleaseActivePointer()
        {
            if (_dial != null && _activePointerId >= 0)
            {
                _dial.ReleasePointer(_activePointerId);
            }

            _activePointerId = -1;
        }

        private int GetSectorIndex(Vector2 offset)
        {
            float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            return Mathf.FloorToInt(Mathf.Repeat(angle + 120f, 360f) / 60f);
        }

        private void HandleChordChanged(ChordDefinition chord)
        {
            if (_chordManager != null)
            {
                SetActiveSlot(_chordManager.CurrentChordIndex);
            }
        }

        private void SetActiveSlot(int chordSlot)
        {
            int sector = FindSectorForSlot(chordSlot);
            if (sector < 0) return;

            UpdateDegreeVisual(_activeSector, false);
            _activeSector = sector;
            UpdateDegreeVisual(_activeSector, true);
            _dial?.MarkDirtyRepaint();
        }

        private void UpdateDegreeVisual(int sectorIndex, bool isActive)
        {
            if (sectorIndex < 0 || sectorIndex >= SectorCount) return;

            _degreeLabels[sectorIndex]?.EnableInClassList("shape-selector-degree--active", isActive);
        }

        private int FindSectorForSlot(int chordSlot)
        {
            for (int i = 0; i < SectorCount; i++)
            {
                if (_slotBySector[i] == chordSlot) return i;
            }

            return -1;
        }

        private Vector2 GetPositionOnDial(int sectorIndex, float radius)
        {
            float angle = (-90f + sectorIndex * 60f) * Mathf.Deg2Rad;
            return new Vector2(
                CenterCoordinate + radius * Mathf.Cos(angle),
                CenterCoordinate + radius * Mathf.Sin(angle));
        }

        private void DrawDial(MeshGenerationContext context)
        {
            Painter2D painter = context.painter2D;
            Vector2 center = new Vector2(CenterCoordinate, CenterCoordinate);

            for (int i = 0; i < SectorCount; i++)
            {
                DrawSector(painter, center, i);
            }

            DrawOuterRing(painter, center);
        }

        private void DrawSector(Painter2D painter, Vector2 center, int sectorIndex)
        {
            bool isActive = sectorIndex == _activeSector;
            float startAngle = -120f + sectorIndex * 60f;
            float endAngle = startAngle + 60f;

            painter.lineWidth = isActive ? 3f : 1.5f;
            painter.strokeColor = isActive
                ? new Color(1f, 0.72f, 0.2f, 0.95f)
                : new Color(0.65f, 0.68f, 0.72f, 0.38f);
            painter.fillColor = isActive
                ? new Color(0.80f, 0.50f, 0.12f, 0.42f)
                : new Color(0.05f, 0.06f, 0.08f, 0.92f);

            painter.BeginPath();
            painter.Arc(center, OuterRadius, Angle.Degrees(startAngle), Angle.Degrees(endAngle), ArcDirection.Clockwise);
            painter.Arc(center, InnerRadius, Angle.Degrees(endAngle), Angle.Degrees(startAngle), ArcDirection.CounterClockwise);
            painter.ClosePath();
            painter.Fill();
            painter.Stroke();
        }

        private void DrawOuterRing(Painter2D painter, Vector2 center)
        {
            painter.strokeColor = new Color(0.9f, 0.92f, 0.95f, 0.35f);
            painter.lineWidth = 2f;
            painter.BeginPath();
            painter.Arc(center, OuterRadius + 8f, Angle.Degrees(0f), Angle.Degrees(360f), ArcDirection.Clockwise);
            painter.Stroke();
        }
    }
}
