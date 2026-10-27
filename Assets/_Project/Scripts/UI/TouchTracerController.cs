using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UIElements;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Chordyceps.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class TouchTracerController : MonoBehaviour
    {
        private const string OVERLAY_NAME = "touch-tracer-overlay";
        private const int MOUSE_POINTER_ID = -1;
        private const float TRAIL_LIFETIME = 0.25f;
        private const float MIN_POINT_DISTANCE = 4f;
        private const float HEAD_WIDTH = 22f;
        private const float TAIL_WIDTH = 2f;
        private const float HEAD_ALPHA = 0.55f;
        private const float TAIL_ALPHA = 0.0f;
        private const float HALO_RADIUS = 20f;
        private const float CORE_RADIUS = 7f;

        private static readonly Color BaseColor = new Color(0.94f, 0.72f, 0.26f, 1f);
        private static readonly Color HaloColor = new Color(0.94f, 0.72f, 0.26f, 0.35f);
        private static readonly Color CoreColor = new Color(1f, 0.95f, 0.85f, 0.75f);

        private UIDocument _uiDocument;
        private VisualElement _overlay;
        private readonly Dictionary<int, TouchTrail> _activeTrails = new Dictionary<int, TouchTrail>();
        private readonly Stack<TouchTrail> _trailPool = new Stack<TouchTrail>();
        private readonly List<int> _expiredTrailIds = new List<int>();
        private readonly List<Vector2> _smoothedPoints = new List<Vector2>(64);

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            BindOverlay();
        }

        private void OnDisable()
        {
            UnbindOverlay();
            EnhancedTouchSupport.Disable();
            ClearAllTrails();
        }

        private void BindOverlay()
        {
            VisualElement root = _uiDocument != null ? _uiDocument.rootVisualElement : null;
            if (root == null) return;

            _overlay = root.Q(OVERLAY_NAME);
            if (_overlay != null)
            {
                _overlay.generateVisualContent += DrawTrails;
            }
        }

        private void UnbindOverlay()
        {
            if (_overlay != null)
            {
                _overlay.generateVisualContent -= DrawTrails;
                _overlay = null;
            }
        }

        private void Update()
        {
            ProcessInputs();
            PruneExpiredPoints();

            if (_activeTrails.Count > 0 && _overlay != null)
            {
                _overlay.MarkDirtyRepaint();
            }
        }

        private void ProcessInputs()
        {
            ProcessTouchscreen();
            ProcessMouse();
        }

        private void ProcessTouchscreen()
        {
            var activeTouches = Touch.activeTouches;
            for (int i = 0; i < activeTouches.Count; i++)
            {
                ProcessTouch(activeTouches[i]);
            }
        }

        private void ProcessTouch(Touch touch)
        {
            int pointerId = touch.touchId;
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began ||
                touch.phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                touch.phase == UnityEngine.InputSystem.TouchPhase.Stationary)
            {
                AddPoint(pointerId, touch.screenPosition);
            }
            else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                     touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                EndTouch(pointerId);
            }
        }

        private void ProcessMouse()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame || mouse.leftButton.isPressed)
            {
                AddPoint(MOUSE_POINTER_ID, mouse.position.ReadValue());
            }
            else if (mouse.leftButton.wasReleasedThisFrame)
            {
                EndTouch(MOUSE_POINTER_ID);
            }
        }

        private void AddPoint(int pointerId, Vector2 screenPos)
        {
            if (_overlay == null || _overlay.panel == null) return;

            Vector2 topDownScreenPos = new Vector2(screenPos.x, Screen.height - screenPos.y);
            Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(_overlay.panel, topDownScreenPos);
            TouchTrail trail = GetOrCreateTrail(pointerId);

            if (trail.Points.Count > 0)
            {
                Vector2 lastPos = trail.Points[trail.Points.Count - 1].Position;
                if (Vector2.Distance(lastPos, panelPos) < MIN_POINT_DISTANCE) return;
            }

            trail.Points.Add(new TrailPoint { Position = panelPos, Time = UnityEngine.Time.time });
            trail.isFading = false;
        }

        private void EndTouch(int pointerId)
        {
            if (_activeTrails.TryGetValue(pointerId, out TouchTrail trail))
            {
                trail.isFading = true;
            }
        }

        private void PruneExpiredPoints()
        {
            float currentTime = UnityEngine.Time.time;
            _expiredTrailIds.Clear();

            foreach (var kvp in _activeTrails)
            {
                PruneTrail(kvp.Value, currentTime);
                if (kvp.Value.isFading && kvp.Value.Points.Count <= 1)
                {
                    _expiredTrailIds.Add(kvp.Key);
                }
            }

            RemoveExpiredTrails();
        }

        private void PruneTrail(TouchTrail trail, float currentTime)
        {
            while (trail.Points.Count > 0 && currentTime - trail.Points[0].Time > TRAIL_LIFETIME)
            {
                trail.Points.RemoveAt(0);
            }
        }

        private void RemoveExpiredTrails()
        {
            for (int i = 0; i < _expiredTrailIds.Count; i++)
            {
                int id = _expiredTrailIds[i];
                ReleaseTrail(_activeTrails[id]);
                _activeTrails.Remove(id);
            }
        }

        private TouchTrail GetOrCreateTrail(int pointerId)
        {
            if (!_activeTrails.TryGetValue(pointerId, out TouchTrail trail))
            {
                trail = _trailPool.Count > 0 ? _trailPool.Pop() : new TouchTrail();
                trail.PointerId = pointerId;
                trail.Points.Clear();
                trail.isFading = false;
                _activeTrails[pointerId] = trail;
            }

            return trail;
        }

        private void ReleaseTrail(TouchTrail trail)
        {
            trail.Points.Clear();
            _trailPool.Push(trail);
        }

        private void ClearAllTrails()
        {
            foreach (var trail in _activeTrails.Values)
            {
                ReleaseTrail(trail);
            }

            _activeTrails.Clear();
        }

        private void DrawTrails(MeshGenerationContext context)
        {
            Painter2D painter = context.painter2D;
            foreach (var trail in _activeTrails.Values)
            {
                DrawSingleTrail(painter, trail);
            }
        }

        private void DrawSingleTrail(Painter2D painter, TouchTrail trail)
        {
            if (trail.Points.Count == 0) return;

            SmoothPoints(trail.Points, _smoothedPoints);
            DrawTrailSegments(painter, _smoothedPoints);

            if (!trail.isFading)
            {
                DrawTrailHead(painter, trail.Points[trail.Points.Count - 1].Position);
            }
        }

        private void DrawTrailHead(Painter2D painter, Vector2 headPos)
        {
            painter.fillColor = HaloColor;
            painter.BeginPath();
            painter.Arc(headPos, HALO_RADIUS, Angle.Degrees(0f), Angle.Degrees(360f));
            painter.Fill();

            painter.fillColor = CoreColor;
            painter.BeginPath();
            painter.Arc(headPos, CORE_RADIUS, Angle.Degrees(0f), Angle.Degrees(360f));
            painter.Fill();
        }

        private void DrawTrailSegments(Painter2D painter, List<Vector2> points)
        {
            int count = points.Count;
            if (count < 2) return;

            painter.lineCap = LineCap.Round;
            painter.lineJoin = LineJoin.Round;

            for (int i = 1; i < count; i++)
            {
                float t = (float)i / (count - 1);
                painter.lineWidth = Mathf.Lerp(TAIL_WIDTH, HEAD_WIDTH, t);
                float alpha = Mathf.Lerp(TAIL_ALPHA, HEAD_ALPHA, t);
                painter.strokeColor = new Color(BaseColor.r, BaseColor.g, BaseColor.b, alpha);

                painter.BeginPath();
                painter.MoveTo(points[i - 1]);
                painter.LineTo(points[i]);
                painter.Stroke();
            }
        }

        private void SmoothPoints(List<TrailPoint> input, List<Vector2> output)
        {
            output.Clear();
            if (input.Count < 2)
            {
                if (input.Count == 1) output.Add(input[0].Position);
                return;
            }

            output.Add(input[0].Position);
            for (int i = 0; i < input.Count - 1; i++)
            {
                Vector2 p0 = input[i].Position;
                Vector2 p1 = input[i + 1].Position;
                // Corner-cutting subdivision produces smooth curves
                output.Add(Vector2.Lerp(p0, p1, 0.25f));
                output.Add(Vector2.Lerp(p0, p1, 0.75f));
            }
            output.Add(input[input.Count - 1].Position);
        }

        private struct TrailPoint
        {
            public Vector2 Position;
            public float Time;
        }

        private class TouchTrail
        {
            public int PointerId;
            public bool isFading;
            public readonly List<TrailPoint> Points = new List<TrailPoint>(64);
        }
    }
}
