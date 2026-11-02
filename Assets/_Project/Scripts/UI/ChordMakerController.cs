using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Chordyceps.Chords;

namespace Chordyceps.UI
{
    /// <summary>
    /// Full-screen chord maker: a horizontally scrollable strip of chord names,
    /// a fixed '+' placeholder, and a neck grid showing the tapped chord. Presentation only —
    /// it reads chord data from the ScriptableObjects and never mutates it.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class ChordMakerController : MonoBehaviour
    {
        private const string ROOT_NAME = "chord-maker-root";
        private const string STRIP_NAME = "chord-maker-strip";
        private const string NECK_HOST_NAME = "chord-maker-neck-host";
        private const string NAME_LABEL_NAME = "chord-maker-name";
        private const string BACK_BTN_NAME = "chord-maker-back-btn";

        private const string VISIBLE_CLASS = "chord-maker-root--visible";
        private const string CHIP_CLASS = "chord-maker-chip";
        private const string CHIP_ACTIVE_CLASS = "chord-maker-chip--active";

        [SerializeField] private ChordManager _chordManager;

        private readonly List<ChordDefinition> _chords = new List<ChordDefinition>();
        private readonly List<Button> _chips = new List<Button>();

        private UIDocument _uiDocument;
        private VisualElement _root;
        private ScrollView _strip;
        private VisualElement _neckHost;
        private Label _nameLabel;
        private ChordNeckElement _neck;
        private int _activeChipIndex = -1;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            if (_chordManager == null)
            {
                _chordManager = FindFirstObjectByType<ChordManager>();
            }

            BindElements();
        }

        private void BindElements()
        {
            VisualElement root = _uiDocument != null ? _uiDocument.rootVisualElement : null;
            if (root == null)
            {
                return;
            }

            _root = root.Q(ROOT_NAME);
            _strip = root.Q<ScrollView>(STRIP_NAME);
            _neckHost = root.Q(NECK_HOST_NAME);
            _nameLabel = root.Q<Label>(NAME_LABEL_NAME);
            root.Q<Button>(BACK_BTN_NAME)?.RegisterCallback<ClickEvent>(_ => Close());

            CreateNeck();
            BuildChordChips();
            SetVisible(false);
        }

        private void CreateNeck()
        {
            if (_neckHost == null) return;

            _neck = new ChordNeckElement();
            _neckHost.Add(_neck);
        }

        private void BuildChordChips()
        {
            _chords.Clear();
            _chips.Clear();
            _strip?.Clear();

            CollectChords();

            if (_strip == null) return;

            for (int i = 0; i < _chords.Count; i++)
            {
                int index = i;
                var chip = new Button(() => SelectChord(index)) { text = _chords[i].ChordName };
                chip.focusable = false;
                chip.AddToClassList(CHIP_CLASS);
                _strip.Add(chip);
                _chips.Add(chip);
            }

            if (_chords.Count > 0)
            {
                SelectChord(0);
            }
        }

        /// <summary>Gathers the distinct chord definitions across all scales (SO data only).</summary>
        private void CollectChords()
        {
            if (_chordManager == null) return;

            int keyCount = _chordManager.TotalKeys;
            for (int k = 0; k < keyCount; k++)
            {
                ScaleDefinition scale = _chordManager.GetScale(k);
                if (scale == null) continue;

                for (int c = 0; c < ScaleDefinition.DIATONIC_CHORD_COUNT; c++)
                {
                    ChordDefinition chord = scale.GetChord(c);
                    if (chord != null && !_chords.Contains(chord))
                    {
                        _chords.Add(chord);
                    }
                }
            }
        }

        private void SelectChord(int index)
        {
            if (index < 0 || index >= _chords.Count) return;

            UpdateActiveChip(index);
            ChordDefinition chord = _chords[index];

            _neck?.SetChord(chord);
            if (_nameLabel != null)
            {
                _nameLabel.text = chord.ChordName;
            }
        }

        private void UpdateActiveChip(int index)
        {
            if (_activeChipIndex >= 0 && _activeChipIndex < _chips.Count)
            {
                _chips[_activeChipIndex].RemoveFromClassList(CHIP_ACTIVE_CLASS);
            }

            _activeChipIndex = index;
            _chips[index].AddToClassList(CHIP_ACTIVE_CLASS);
        }

        public void Open()
        {
            SetVisible(true);
        }

        public void Close()
        {
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            _root?.EnableInClassList(VISIBLE_CLASS, visible);
        }
    }
}
