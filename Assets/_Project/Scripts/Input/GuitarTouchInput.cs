using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Chordyceps.Gameplay;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Chordyceps.Input
{
    public class GuitarTouchInput : MonoBehaviour
    {
        [SerializeField] private GuitarController _targetController;
        [SerializeField] private Camera _targetCamera;

        private readonly Collider2D[] _overlapResults = new Collider2D[MAX_OVERLAP_HITS];
        private readonly RaycastHit2D[] _linecastResults = new RaycastHit2D[MAX_LINECAST_HITS];
        private readonly Dictionary<int, Vector2> _pointerPreviousPositions = new Dictionary<int, Vector2>();
        private readonly Dictionary<int, int> _pointerLastStringHits = new Dictionary<int, int>();

        private const int MAX_OVERLAP_HITS = 4;
        private const int MAX_LINECAST_HITS = 8;
        private const float DEFAULT_VELOCITY = 1.0f;
        private const float MAX_EXPECTED_SPEED = 2000.0f;
        private const int MOUSE_POINTER_ID = -1;

        private ContactFilter2D _contactFilter;

        private void Awake()
        {
            FindReferencesIfMissing();
            InitializeContactFilter();
        }

        private void InitializeContactFilter()
        {
            _contactFilter = new ContactFilter2D();
            _contactFilter.useTriggers = true;
        }

        private void FindReferencesIfMissing()
        {
            if (_targetController == null)
            {
                _targetController = FindFirstObjectByType<GuitarController>();
            }
            if (_targetCamera == null)
            {
                _targetCamera = Camera.main;
            }
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
            _pointerPreviousPositions.Clear();
            _pointerLastStringHits.Clear();
        }

        private void Update()
        {
            if (_targetCamera == null || _targetController == null)
            {
                return;
            }

            ProcessTouchscreenInput();
            ProcessMouseSimulation();
        }

        private void ProcessTouchscreenInput()
        {
            var activeTouches = Touch.activeTouches;
            for (int i = 0; i < activeTouches.Count; i++)
            {
                HandleSingleTouch(activeTouches[i]);
            }
        }

        private void HandleSingleTouch(Touch touch)
        {
            Vector2 screenPos = touch.screenPosition;
            Vector2 worldPos = _targetCamera.ScreenToWorldPoint(screenPos);
            int touchId = touch.touchId;

            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                HandlePointerBegan(touchId, worldPos);
            }
            else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                HandlePointerMoved(touchId, worldPos);
            }
            else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended || touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                ClearPointer(touchId);
            }
        }

        private void ProcessMouseSimulation()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            Vector2 screenPos = mouse.position.ReadValue();
            Vector2 worldPos = _targetCamera.ScreenToWorldPoint(screenPos);

            if (mouse.leftButton.wasPressedThisFrame)
            {
                HandlePointerBegan(MOUSE_POINTER_ID, worldPos);
            }
            else if (mouse.leftButton.isPressed)
            {
                HandlePointerMoved(MOUSE_POINTER_ID, worldPos);
            }
            else if (mouse.leftButton.wasReleasedThisFrame)
            {
                ClearPointer(MOUSE_POINTER_ID);
            }
        }

        private void HandlePointerBegan(int pointerId, Vector2 worldPos)
        {
            _pointerPreviousPositions[pointerId] = worldPos;
            int count = Physics2D.OverlapPoint(worldPos, _contactFilter, _overlapResults);
            
            for (int i = 0; i < count; i++)
            {
                GuitarString guitarString = _overlapResults[i].GetComponent<GuitarString>();
                if (guitarString != null)
                {
                    DispatchPluck(guitarString.StringIndex, StringInputType.Tap, DEFAULT_VELOCITY, worldPos);
                    _pointerLastStringHits[pointerId] = guitarString.StringIndex;
                    break;
                }
            }
        }

        private void HandlePointerMoved(int pointerId, Vector2 currentWorldPos)
        {
            if (!_pointerPreviousPositions.TryGetValue(pointerId, out Vector2 prevWorldPos))
            {
                _pointerPreviousPositions[pointerId] = currentWorldPos;
                return;
            }

            float velocity = CalculateVelocity(prevWorldPos, currentWorldPos);
            DetectCrossedStrings(pointerId, prevWorldPos, currentWorldPos, velocity);
            _pointerPreviousPositions[pointerId] = currentWorldPos;
        }

        private float CalculateVelocity(Vector2 prev, Vector2 current)
        {
            float distance = Vector2.Distance(prev, current);
            float speed = distance / Mathf.Max(Time.deltaTime, 0.0001f);
            return Mathf.Clamp01(speed / MAX_EXPECTED_SPEED + 0.3f);
        }

        private void DetectCrossedStrings(int pointerId, Vector2 prev, Vector2 current, float velocity)
        {
            int count = Physics2D.Linecast(prev, current, _contactFilter, _linecastResults);
            for (int i = 0; i < count; i++)
            {
                GuitarString guitarString = _linecastResults[i].collider.GetComponent<GuitarString>();
                if (guitarString != null)
                {
                    TryDispatchStrum(pointerId, guitarString.StringIndex, velocity, _linecastResults[i].point);
                }
            }
        }

        private void TryDispatchStrum(int pointerId, int stringIndex, float velocity, Vector2 hitPoint)
        {
            if (_pointerLastStringHits.TryGetValue(pointerId, out int lastIndex) && lastIndex == stringIndex)
            {
                return;
            }

            _pointerLastStringHits[pointerId] = stringIndex;
            DispatchPluck(stringIndex, StringInputType.Strum, velocity, hitPoint);
        }

        private void DispatchPluck(int stringIndex, StringInputType type, float velocity, Vector2 pos)
        {
            var data = new StringInputData(stringIndex, type, velocity, pos);
            _targetController.OnStringInteracted(data);
        }

        private void ClearPointer(int pointerId)
        {
            _pointerPreviousPositions.Remove(pointerId);
            _pointerLastStringHits.Remove(pointerId);
        }
    }
}
