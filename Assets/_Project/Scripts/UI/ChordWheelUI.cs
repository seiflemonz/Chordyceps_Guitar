using UnityEngine;
using UnityEngine.UIElements;
using Chordyceps.Chords;

namespace Chordyceps.UI
{
    /// <summary>Manages the Circle of Chords wheel: 3 major + 3 minor buttons.</summary>
    public class ChordWheelUI : MonoBehaviour
    {
        private const int CHORD_COUNT = 6;
        private const string ACTIVE_CLASS = "chord-btn--active";

        [SerializeField] private ChordManager _chordManager;

        private Button[] _chordButtons;
        private Label[] _nameLabels;
        private Label[] _numeralLabels;
        private int _activeIndex = -1;

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
            _chordButtons  = new Button[CHORD_COUNT];
            _nameLabels    = new Label[CHORD_COUNT];
            _numeralLabels = new Label[CHORD_COUNT];

            for (int i = 0; i < CHORD_COUNT; i++)
            {
                BindSlot(root, i);
            }

            if (_chordManager != null)
            {
                _chordManager.OnScaleChanged += RefreshChordNames;
                _chordManager.OnChordChanged += RefreshActiveSlot;
                RefreshChordNames(_chordManager.CurrentScale);
                RefreshActiveSlot(_chordManager.CurrentChord);
            }
        }

        private void BindSlot(VisualElement root, int index)
        {
            Button btn = root.Q<Button>($"chord-{index}");
            if (btn == null) return;

            _chordButtons[index]  = btn;
            _nameLabels[index]    = btn.Q<Label>($"chord-{index}-name");
            _numeralLabels[index] = btn.Q<Label>($"chord-{index}-numeral");

            int captured = index;
            btn.RegisterCallback<ClickEvent>(_ => _chordManager?.SelectChord(captured));
        }

        private void RefreshChordNames(ScaleDefinition scale)
        {
            if (scale == null) return;

            for (int i = 0; i < CHORD_COUNT; i++)
            {
                ChordDefinition chord = scale.GetChord(i);
                if (chord == null) continue;

                if (_nameLabels[i] != null)    _nameLabels[i].text    = chord.ChordName;
                if (_numeralLabels[i] != null) _numeralLabels[i].text = chord.RomanNumeral;
            }
        }

        private void RefreshActiveSlot(ChordDefinition activeChord)
        {
            for (int i = 0; i < CHORD_COUNT; i++)
            {
                if (_chordButtons[i] == null) continue;
                _chordButtons[i].RemoveFromClassList(ACTIVE_CLASS);
            }

            int newActive = _chordManager != null ? _chordManager.CurrentChordIndex : -1;
            if (newActive >= 0 && newActive < CHORD_COUNT && _chordButtons[newActive] != null)
            {
                _chordButtons[newActive].AddToClassList(ACTIVE_CLASS);
            }
        }

        private void OnDestroy()
        {
            if (_chordManager == null) return;

            _chordManager.OnScaleChanged -= RefreshChordNames;
            _chordManager.OnChordChanged -= RefreshActiveSlot;
        }
    }
}
