using UnityEngine;
using UnityEngine.UIElements;
using Chordyceps.Chords;

namespace Chordyceps.UI
{
    /// <summary>Manages the top-center Capo bar: − label +</summary>
    [RequireComponent(typeof(UIDocument))]
    public class CapoBarUI : MonoBehaviour
    {
        [SerializeField] private ChordManager _chordManager;

        private Button _decrementBtn;
        private Button _incrementBtn;
        private Label _capoLabel;

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
            _decrementBtn = root.Q<Button>("capo-decrement");
            _incrementBtn = root.Q<Button>("capo-increment");
            _capoLabel    = root.Q<Label>("capo-label");

            _decrementBtn?.RegisterCallback<ClickEvent>(_ => _chordManager?.DecrementCapo());
            _incrementBtn?.RegisterCallback<ClickEvent>(_ => _chordManager?.IncrementCapo());

            if (_chordManager != null)
            {
                _chordManager.OnCapoChanged += UpdateLabel;
                UpdateLabel(_chordManager.CurrentCapo);
            }
        }

        private void UpdateLabel(int capo)
        {
            if (_capoLabel != null)
            {
                _capoLabel.text = capo == 0 ? "No Capo" : $"Capo: {capo}";
            }
        }

        private void OnDestroy()
        {
            if (_chordManager != null)
            {
                _chordManager.OnCapoChanged -= UpdateLabel;
            }
        }
    }
}
