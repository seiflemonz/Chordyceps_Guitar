using System.Collections;
using UnityEngine;

namespace Chordyceps.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public class GuitarString : MonoBehaviour
    {
        [SerializeField] private int _stringIndex = 0;
        [SerializeField] private LineRenderer _lineRenderer;

        private Collider2D _collider;
        private Coroutine _wobbleCoroutine;
        private bool _isInitialized = false;

        private const int POINT_COUNT = 11;
        private const float HALF_LENGTH = 9f;
        private const float MAX_STRING_WIDTH = 0.08f;
        private const float MIN_STRING_WIDTH = 0.025f;
        private const float WOBBLE_DURATION = 0.25f;
        private const float WOBBLE_AMPLITUDE = 0.18f;
        private const float WOBBLE_FREQUENCY = 40f;
        private const int SORTING_ORDER = 1;

        private readonly Color _normalColor = new Color(0.85f, 0.85f, 0.9f, 0.85f);
        private readonly Color _highlightColor = new Color(1f, 1f, 0.65f, 1f);

        public int StringIndex => _stringIndex;
        public Collider2D Collider => _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            InitializeLineRenderer();
        }

        private void InitializeLineRenderer()
        {
            if (_lineRenderer == null)
            {
                _lineRenderer = GetComponent<LineRenderer>();
            }

            if (_lineRenderer == null)
            {
                _lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            ConfigureLineRenderer();
            ResetStringPositions();
            _isInitialized = true;
        }

        private void ConfigureLineRenderer()
        {
            _lineRenderer.positionCount = POINT_COUNT;
            _lineRenderer.useWorldSpace = false;
            float width = CalculateStringWidth();
            _lineRenderer.startWidth = width;
            _lineRenderer.endWidth = width;
            _lineRenderer.startColor = _normalColor;
            _lineRenderer.endColor = _normalColor;
            _lineRenderer.sortingOrder = SORTING_ORDER;
            AssignLineMaterial();
        }

        private float CalculateStringWidth()
        {
            float t = (float)_stringIndex / (GuitarController.STRING_COUNT - 1);
            return Mathf.Lerp(MAX_STRING_WIDTH, MIN_STRING_WIDTH, t);
        }

        private void AssignLineMaterial()
        {
            if (_lineRenderer.sharedMaterial != null) return;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit");
            }

            if (shader != null)
            {
                _lineRenderer.material = new Material(shader);
            }
        }

        public void SetStringIndex(int index)
        {
            _stringIndex = index;
        }

        public void PlayPluckFeedback()
        {
            if (!_isInitialized) return;

            if (_wobbleCoroutine != null)
            {
                StopCoroutine(_wobbleCoroutine);
            }

            _wobbleCoroutine = StartCoroutine(WobbleRoutine());
        }

        private IEnumerator WobbleRoutine()
        {
            _lineRenderer.startColor = _highlightColor;
            _lineRenderer.endColor = _highlightColor;
            float elapsed = 0f;

            while (elapsed < WOBBLE_DURATION)
            {
                elapsed += Time.deltaTime;
                UpdateWobblePositions(elapsed);
                yield return null;
            }

            ResetStringPositions();
            _lineRenderer.startColor = _normalColor;
            _lineRenderer.endColor = _normalColor;
            _wobbleCoroutine = null;
        }

        private void UpdateWobblePositions(float elapsed)
        {
            float progress = elapsed / WOBBLE_DURATION;
            float decay = 1f - progress;
            float wave = Mathf.Sin(elapsed * WOBBLE_FREQUENCY) * WOBBLE_AMPLITUDE * decay;

            for (int i = 0; i < POINT_COUNT; i++)
            {
                float t = (float)i / (POINT_COUNT - 1);
                float x = Mathf.Lerp(-HALF_LENGTH, HALF_LENGTH, t);
                float envelope = Mathf.Sin(t * Mathf.PI);
                _lineRenderer.SetPosition(i, new Vector3(x, wave * envelope, 0f));
            }
        }

        private void ResetStringPositions()
        {
            for (int i = 0; i < POINT_COUNT; i++)
            {
                float t = (float)i / (POINT_COUNT - 1);
                float x = Mathf.Lerp(-HALF_LENGTH, HALF_LENGTH, t);
                _lineRenderer.SetPosition(i, new Vector3(x, 0f, 0f));
            }
        }
    }
}
