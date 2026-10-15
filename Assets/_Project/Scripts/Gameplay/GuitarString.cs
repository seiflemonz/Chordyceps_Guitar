using UnityEngine;

namespace Chordyceps.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public class GuitarString : MonoBehaviour
    {
        [SerializeField] private int _stringIndex = 0;

        private Collider2D _collider;
        private SpriteRenderer _renderer;
        private Color _originalColor = Color.white;
        private bool _isInitialized = false;

        private const float HIGHLIGHT_ALPHA = 0.5f;
        private const float FEEDBACK_DURATION = 0.1f;

        public int StringIndex => _stringIndex;
        public Collider2D Collider => _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _renderer = GetComponent<SpriteRenderer>();
            CacheOriginalColor();
        }

        private void CacheOriginalColor()
        {
            if (_renderer != null)
            {
                _originalColor = _renderer.color;
                _isInitialized = true;
            }
        }

        public void SetStringIndex(int index)
        {
            _stringIndex = index;
        }

        public void PlayPluckFeedback()
        {
            if (_isInitialized)
            {
                HighlightVisual();
            }
        }

        private void HighlightVisual()
        {
            _renderer.color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, HIGHLIGHT_ALPHA);
            Invoke(nameof(ResetVisual), FEEDBACK_DURATION);
        }

        private void ResetVisual()
        {
            if (_isInitialized)
            {
                _renderer.color = _originalColor;
            }
        }
    }
}
