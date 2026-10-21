using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Chordyceps.Gameplay;
using Chordyceps.Input;

namespace Chordyceps.UI
{
    /// <summary>
    /// Builds the six guitar strings inside the bottom-right HUD container and
    /// turns pointer gestures into the same StringInputData intents the
    /// world-space GuitarTouchInput used to produce.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class GuitarStringPanelController : MonoBehaviour
    {
        [SerializeField] private GuitarController _guitarController;

        private const string CONTAINER_NAME = "string-panel-container";
        private const float DEFAULT_VELOCITY = 1.0f;
        private const float VELOCITY_FLOOR = 0.3f;
        private const float MAX_EXPECTED_SPEED = 25f;
        private const float MIN_FRAME_TIME = 0.0001f;
        private const float MIN_CROSSING_DELTA = 0.0001f;

        private readonly GuitarStringElement[] _stringElements = new GuitarStringElement[GuitarController.STRING_COUNT];
        private readonly Dictionary<int, Vector2> _pointerPreviousPositions = new Dictionary<int, Vector2>();
        private readonly Dictionary<int, int> _pointerLastStringHits = new Dictionary<int, int>();

        private UIDocument _uiDocument;
        private VisualElement _container;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();

            if (_guitarController == null)
            {
                _guitarController = FindFirstObjectByType<GuitarController>();
            }

            BuildStringPanel();
        }

        private void OnEnable()
        {
            if (_container == null)
            {
                BuildStringPanel();
            }

            SubscribeToController();
        }

        private void OnDisable()
        {
            UnsubscribeFromController();
            _pointerPreviousPositions.Clear();
            _pointerLastStringHits.Clear();
        }

        private void BuildStringPanel()
        {
            VisualElement root = _uiDocument != null ? _uiDocument.rootVisualElement : null;
            if (root == null)
            {
                return;
            }

            _container = root.Q(CONTAINER_NAME);
            if (_container == null)
            {
                Debug.LogWarning($"[GuitarStringPanelController] '{CONTAINER_NAME}' not found in GuitarHUD.uxml.");
                return;
            }

            // Added top-down so string 5 (high e) sits at the top edge, matching the
            // original scene layout where low E was at the bottom.
            for (int i = GuitarController.STRING_COUNT - 1; i >= 0; i--)
            {
                var element = new GuitarStringElement(i, GuitarController.STRING_COUNT);
                element.RegisterCallback<PointerDownEvent>(HandleStringPointerDown);
                _stringElements[i] = element;
                _container.Add(element);
            }

            _container.RegisterCallback<PointerMoveEvent>(HandlePointerMoved);
            _container.RegisterCallback<PointerUpEvent>(HandlePointerUp);
            _container.RegisterCallback<PointerCancelEvent>(HandlePointerCancel);
        }

        private void SubscribeToController()
        {
            if (_guitarController == null)
            {
                return;
            }

            _guitarController.OnStringPlucked += HandleStringPlucked;
        }

        private void UnsubscribeFromController()
        {
            if (_guitarController == null)
            {
                return;
            }

            _guitarController.OnStringPlucked -= HandleStringPlucked;
        }

        private void HandleStringPlucked(StringInputData data)
        {
            if (data.StringIndex < 0 || data.StringIndex >= _stringElements.Length)
            {
                return;
            }

            _stringElements[data.StringIndex]?.PlayPluckFeedback();
        }

        private void HandleStringPointerDown(PointerDownEvent evt)
        {
            var element = evt.currentTarget as GuitarStringElement;
            if (element == null)
            {
                return;
            }

            int pointerId = evt.pointerId;
            Vector2 panelPosition = evt.position;

            _pointerPreviousPositions[pointerId] = panelPosition;
            _pointerLastStringHits[pointerId] = element.StringIndex;
            _container.CapturePointer(pointerId);

            DispatchPluck(element.StringIndex, StringInputType.Tap, DEFAULT_VELOCITY, panelPosition);
        }

        private void HandlePointerMoved(PointerMoveEvent evt)
        {
            int pointerId = evt.pointerId;
            if (!_pointerPreviousPositions.TryGetValue(pointerId, out Vector2 previousPosition))
            {
                return;
            }

            Vector2 currentPosition = evt.position;
            DetectCrossedStrings(pointerId, previousPosition, currentPosition, CalculateVelocity(previousPosition, currentPosition));
            _pointerPreviousPositions[pointerId] = currentPosition;
        }

        /// <summary>
        /// Normalised against the container height so strum strength does not
        /// depend on the device resolution.
        /// </summary>
        private float CalculateVelocity(Vector2 previousPosition, Vector2 currentPosition)
        {
            float containerHeight = Mathf.Max(1f, _container.contentRect.height);
            float speed = Vector2.Distance(previousPosition, currentPosition) / Mathf.Max(Time.deltaTime, MIN_FRAME_TIME);
            float normalisedSpeed = speed / containerHeight / MAX_EXPECTED_SPEED;
            return Mathf.Clamp01(normalisedSpeed + VELOCITY_FLOOR);
        }

        /// <summary>
        /// Mirrors the retired Physics2D.Linecast strum: plucks every string the
        /// movement segment crossed this frame, in the order it was crossed.
        /// </summary>
        private void DetectCrossedStrings(int pointerId, Vector2 previousPosition, Vector2 currentPosition, float velocity)
        {
            float deltaY = currentPosition.y - previousPosition.y;
            if (Mathf.Abs(deltaY) < MIN_CROSSING_DELTA)
            {
                return;
            }

            // Panel Y grows downward, so walking element indices in the opposite
            // direction of the vertical move visits strings in crossing order.
            bool movingDown = deltaY > 0f;
            int startIndex = movingDown ? _stringElements.Length - 1 : 0;
            int endIndex = movingDown ? -1 : _stringElements.Length;
            int step = movingDown ? -1 : 1;

            for (int i = startIndex; i != endIndex; i += step)
            {
                GuitarStringElement element = _stringElements[i];
                if (element == null)
                {
                    continue;
                }

                Rect bounds = element.worldBound;
                float centerY = bounds.center.y;
                float t = (centerY - previousPosition.y) / deltaY;
                if (t < 0f || t >= 1f)
                {
                    continue;
                }

                float xAtCrossing = Mathf.Lerp(previousPosition.x, currentPosition.x, t);
                if (xAtCrossing < bounds.xMin || xAtCrossing > bounds.xMax)
                {
                    continue;
                }

                TryDispatchStrum(pointerId, i, velocity, new Vector2(xAtCrossing, centerY));
            }
        }

        private void TryDispatchStrum(int pointerId, int stringIndex, float velocity, Vector2 panelPosition)
        {
            if (_pointerLastStringHits.TryGetValue(pointerId, out int lastIndex) && lastIndex == stringIndex)
            {
                return;
            }

            _pointerLastStringHits[pointerId] = stringIndex;
            DispatchPluck(stringIndex, StringInputType.Strum, velocity, panelPosition);
        }

        private void HandlePointerUp(PointerUpEvent evt)
        {
            EndPointer(evt.pointerId);
        }

        private void HandlePointerCancel(PointerCancelEvent evt)
        {
            EndPointer(evt.pointerId);
        }

        private void EndPointer(int pointerId)
        {
            if (_container.HasPointerCapture(pointerId))
            {
                _container.ReleasePointer(pointerId);
            }

            _pointerPreviousPositions.Remove(pointerId);
            _pointerLastStringHits.Remove(pointerId);
        }

        private void DispatchPluck(int stringIndex, StringInputType inputType, float velocity, Vector2 panelPosition)
        {
            if (_guitarController == null)
            {
                return;
            }

            var data = new StringInputData(stringIndex, inputType, velocity, panelPosition);
            _guitarController.OnStringInteracted(data);
        }
    }
}
