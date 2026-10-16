using UnityEngine;
using UnityEngine.UIElements;
using Chordyceps.Chords;

namespace Chordyceps.UI
{
    /// <summary>Manages the upper-left Key Selector: < C Major ></summary>
    public class KeySelectorUI : MonoBehaviour
    {
        [SerializeField] private ChordManager _chordManager;

        private Button _prevBtn;
        private Button _nextBtn;
        private Label _keyLabel;

        private void Awake()
        {
            FindChordManagerIfMissing();
        }

        private void FindChordManagerIfMissing()
        {
            if (_chordManager == null)
            {
                _chordManager = FindFirstObjectByType<ChordManager>();
            }
        }

        public void Initialize(VisualElement root)
        {
            _prevBtn  = root.Q<Button>("key-prev");
            _nextBtn  = root.Q<Button>("key-next");
            _keyLabel = root.Q<Label>("key-label");

            _prevBtn?.RegisterCallback<ClickEvent>(_ => _chordManager?.DecrementKey());
            _nextBtn?.RegisterCallback<ClickEvent>(_ => _chordManager?.IncrementKey());

            if (_chordManager != null)
            {
                _chordManager.OnScaleChanged += UpdateLabel;
                UpdateLabel(_chordManager.CurrentScale);
            }
        }

        private void UpdateLabel(ScaleDefinition scale)
        {
            if (_keyLabel != null)
            {
                _keyLabel.text = scale != null ? scale.ScaleName : "—";
            }
        }

        private void OnDestroy()
        {
            if (_chordManager != null)
            {
                _chordManager.OnScaleChanged -= UpdateLabel;
            }
        }
    }
}
