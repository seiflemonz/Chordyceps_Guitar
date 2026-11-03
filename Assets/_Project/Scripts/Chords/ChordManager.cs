using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chordyceps.Chords
{
    public class ChordManager : MonoBehaviour
    {
        public enum KeyMode { Major, Minor }

        [SerializeField] private ScaleDefinition[] _allScales = new ScaleDefinition[TOTAL_KEYS];
        [SerializeField] private ScaleDefinition _currentScale;
        [SerializeField] private int _currentKeyIndex = 0;
        [SerializeField] private int _currentChordIndex = 1;
        [SerializeField] private int _currentCapo = 0;
        [SerializeField] private int _currentRootIndex = 0;
        [SerializeField] private KeyMode _currentMode = KeyMode.Major;

        /// <summary>Chord Maker audition overriding the current chord without changing the selection.</summary>
        private int[] _previewFrets;

        /// <summary>
        /// Where the wheel's chords come from: the scale assets, or the layout the player
        /// applied in the Wheel Maker. Everything the HUD shows goes through this.
        /// </summary>
        private IWheelSource _wheelSource;

        /// <summary>Position within <see cref="_wheelSource"/> while a custom layout is applied.</summary>
        private int _currentWheelIndex;

        /// <summary>Every definition asset, keyed by chord name; rebuilt when the scales change.</summary>
        private Dictionary<string, ChordDefinition> _definitionsByName;

        public const int TOTAL_KEYS = 12;
        public const int MIN_CAPO = 0;
        public const int MAX_CAPO = 12;
        public const int MINOR_SCALE_OFFSET = 9;

        private static readonly string[] _rootNames = new string[]
        {
            "C", "G", "D", "A", "E", "B", "F#", "Db", "Ab", "Eb", "Bb", "F"
        };

        /// <summary>Raised when the wheel's contents change: a new key, a new wheel, or a new layout.</summary>
        public event Action OnWheelChanged;
        public event Action OnChordChanged;
        public event Action<int> OnCapoChanged;
        public event Action<string, KeyMode> OnKeySelectionChanged;

        public ScaleDefinition CurrentScale => _currentScale;
        public ChordDefinition CurrentChord => !UsesCustomLayout && _currentScale != null ? _currentScale.GetChord(_currentChordIndex) : null;
        public int CurrentKeyIndex => _currentKeyIndex;
        public int CurrentChordIndex => _currentChordIndex;
        public int CurrentCapo => _currentCapo;
        public int CurrentRootIndex => _currentRootIndex;
        public KeyMode CurrentMode => _currentMode;
        public string CurrentRootName => _rootNames[_currentRootIndex];
        public int TotalKeys => _allScales != null ? _allScales.Length : 0;

        /// <summary>True when the player has applied a layout of their own to the main game.</summary>
        public bool UsesCustomLayout => _wheelSource != null && _wheelSource.IsCustom;

        /// <summary>How many wheels the player can step through.</summary>
        public int WheelCount => _wheelSource != null ? _wheelSource.WheelCount : 0;

        /// <summary>The wheel on display: the current key's scale, or the current custom wheel.</summary>
        public int CurrentWheelIndex => UsesCustomLayout ? _currentWheelIndex : _currentKeyIndex;

        /// <summary>Header drawn above the HUD wheel — a scale name, or the wheel number.</summary>
        public string CurrentWheelHeader => _wheelSource != null ? _wheelSource.Header(CurrentWheelIndex) : string.Empty;

        /// <summary>
        /// What the key selector shows: the root note normally, or the wheel number when a
        /// custom layout is applied, since keys have no meaning there.
        /// </summary>
        public string CurrentWheelLabel => UsesCustomLayout ? (CurrentWheelIndex + 1).ToString() : CurrentRootName;

        private void Awake()
        {
            RebuildDefinitionIndex();
            InitializeDefaultScale();
            LoadWheelSource();
            ResetChordToTonic();
        }

        private void Start()
        {
            PublishInitialState();
        }

        private void InitializeDefaultScale()
        {
            int scaleIndex = ResolveScaleIndex(_currentRootIndex, _currentMode);
            _currentKeyIndex = scaleIndex;
            if (_allScales != null && _allScales.Length > scaleIndex && _allScales[scaleIndex] != null)
            {
                _currentScale = _allScales[scaleIndex];
                _currentChordIndex = _currentScale.TonicChordIndex;
            }
        }

        /// <summary>Adopts the layout the player last applied, or the built-in scales when there is none.</summary>
        private void LoadWheelSource()
        {
            WheelLayoutEntry layout = CustomWheelStore.GetCustom(CustomWheelStore.ActiveLayoutId);
            _wheelSource = layout != null ? new CustomWheelSource(layout) : new ScaleWheelSource(_allScales);
            _currentWheelIndex = 0;
        }

        private void ResetChordToTonic()
        {
            _currentChordIndex = _wheelSource != null
                ? Mathf.Clamp(_wheelSource.TonicSlot(CurrentWheelIndex), 0, ScaleDefinition.DIATONIC_CHORD_COUNT - 1)
                : 0;
        }

        private void RebuildDefinitionIndex()
        {
            _definitionsByName = new Dictionary<string, ChordDefinition>();
            if (_allScales == null)
            {
                return;
            }

            for (int key = 0; key < _allScales.Length; key++)
            {
                ScaleDefinition scale = _allScales[key];
                if (scale == null)
                {
                    continue;
                }

                for (int slot = 0; slot < ScaleDefinition.DIATONIC_CHORD_COUNT; slot++)
                {
                    ChordDefinition chord = scale.GetChord(slot);
                    if (chord != null && !_definitionsByName.ContainsKey(chord.ChordName))
                    {
                        _definitionsByName.Add(chord.ChordName, chord);
                    }
                }
            }
        }

        private void PublishInitialState()
        {
            OnWheelChanged?.Invoke();
            OnChordChanged?.Invoke();
            OnCapoChanged?.Invoke(_currentCapo);
            OnKeySelectionChanged?.Invoke(CurrentWheelLabel, _currentMode);
        }

        // ---------------------------------------------------------------------
        // Chord selection
        // ---------------------------------------------------------------------

        public void SelectChord(int chordIndex)
        {
            if (_wheelSource == null || chordIndex < 0 || chordIndex >= ScaleDefinition.DIATONIC_CHORD_COUNT)
            {
                return;
            }

            _currentChordIndex = chordIndex;
            OnChordChanged?.Invoke();
        }

        // ---------------------------------------------------------------------
        // Wheel navigation
        // ---------------------------------------------------------------------

        /// <summary>
        /// Steps to the next wheel. In the built-in layout a wheel is a key, so this moves
        /// to the next key; with a custom layout it moves to the next numbered wheel.
        /// </summary>
        public void IncrementKey()
        {
            if (UsesCustomLayout)
            {
                StepWheel(1);
                return;
            }

            SetKey(_currentRootIndex + 1);
        }

        public void DecrementKey()
        {
            if (UsesCustomLayout)
            {
                StepWheel(-1);
                return;
            }

            SetKey(_currentRootIndex - 1);
        }

        private void StepWheel(int delta)
        {
            int count = WheelCount;
            if (count <= 0)
            {
                return;
            }

            _currentWheelIndex = ((_currentWheelIndex + delta) % count + count) % count;
            ResetChordToTonic();
            OnWheelChanged?.Invoke();
            OnChordChanged?.Invoke();
            OnKeySelectionChanged?.Invoke(CurrentWheelLabel, _currentMode);
        }

        // ---------------------------------------------------------------------
        // Key selection
        // ---------------------------------------------------------------------

        public void SetKey(int rootIndex)
        {
            if (_allScales == null || _allScales.Length == 0 || UsesCustomLayout) return;

            _currentRootIndex = (rootIndex % TOTAL_KEYS + TOTAL_KEYS) % TOTAL_KEYS;
            int scaleIndex = ResolveScaleIndex(_currentRootIndex, _currentMode);
            _currentKeyIndex = scaleIndex;
            SetScale(_allScales[scaleIndex]);
            OnKeySelectionChanged?.Invoke(CurrentWheelLabel, _currentMode);
        }

        public void SetKeyMode(KeyMode mode)
        {
            if (UsesCustomLayout || _currentMode == mode) return;
            _currentMode = mode;
            SetKey(_currentRootIndex);
        }

        public void ToggleKeyMode()
        {
            SetKeyMode(_currentMode == KeyMode.Major ? KeyMode.Minor : KeyMode.Major);
        }

        private int ResolveScaleIndex(int rootIndex, KeyMode mode)
        {
            if (mode == KeyMode.Minor)
            {
                return (rootIndex + MINOR_SCALE_OFFSET) % TOTAL_KEYS;
            }
            return rootIndex % TOTAL_KEYS;
        }

        public ScaleDefinition GetScale(int index)
        {
            if (_allScales == null || index < 0 || index >= _allScales.Length)
            {
                return null;
            }

            return _allScales[index];
        }

        public void SetAllScales(ScaleDefinition[] scales)
        {
            _allScales = scales;
            RebuildDefinitionIndex();
            InitializeDefaultScale();

            // A default source wraps the old array, so it has to be rebuilt around the new one.
            if (!UsesCustomLayout)
            {
                _wheelSource = new ScaleWheelSource(_allScales);
            }
        }

        public void SetScale(ScaleDefinition newScale)
        {
            if (newScale == null || _currentScale == newScale)
            {
                return;
            }

            _currentScale = newScale;
            _currentChordIndex = newScale.TonicChordIndex;
            OnWheelChanged?.Invoke();
            OnChordChanged?.Invoke();
        }

        // ---------------------------------------------------------------------
        // Wheel contents
        // ---------------------------------------------------------------------

        /// <summary>Roman numeral for a slot on the current wheel; empty when there is none.</summary>
        public string GetWheelRoman(int slot)
        {
            return _wheelSource != null ? _wheelSource.Roman(CurrentWheelIndex, slot) : string.Empty;
        }

        /// <summary>Chord id for a slot on the current wheel; empty when the slot is unassigned.</summary>
        public string GetWheelChordId(int slot)
        {
            return _wheelSource != null ? _wheelSource.ChordId(CurrentWheelIndex, slot) : string.Empty;
        }

        /// <summary>
        /// Display name for a slot's chord: a custom chord's name, or the definition's name.
        /// Ids that no longer resolve leave the slot blank.
        /// </summary>
        public string GetWheelChordName(int slot)
        {
            string chordId = GetWheelChordId(slot);
            if (string.IsNullOrEmpty(chordId))
            {
                return string.Empty;
            }

            ChordEntry custom = CustomChordStore.GetCustom(chordId);
            return custom != null ? custom.Name : chordId;
        }

        /// <summary>
        /// Re-reads the layout the player applied and swaps the wheel source, so the HUD
        /// follows an Apply in the Wheel Maker without a reload.
        /// </summary>
        public void ApplyWheelSource()
        {
            LoadWheelSource();
            ResetChordToTonic();
            PublishInitialState();
        }

        // ---------------------------------------------------------------------
        // Fret resolution
        // ---------------------------------------------------------------------

        public int GetFretOffset(int stringIndex)
        {
            // Audition first, then whichever wheel is on display.
            if (_previewFrets != null && stringIndex >= 0 && stringIndex < _previewFrets.Length)
            {
                return _previewFrets[stringIndex];
            }

            return UsesCustomLayout ? CustomWheelFretOffset(stringIndex) : ScaleChordFretOffset(stringIndex);
        }

        /// <summary>A stored override of the selected chord, then the definition asset itself.</summary>
        private int ScaleChordFretOffset(int stringIndex)
        {
            ChordDefinition chord = _currentScale != null ? _currentScale.GetChord(_currentChordIndex) : null;
            if (chord == null)
            {
                return 0;
            }

            ChordEntry overridden = CustomChordStore.GetOverride(chord.ChordName);
            if (overridden != null && overridden.Frets != null
                && stringIndex >= 0 && stringIndex < overridden.Frets.Length)
            {
                return overridden.Frets[stringIndex];
            }

            return chord.GetFretOffset(stringIndex);
        }

        /// <summary>
        /// Resolves a custom wheel's slot by chord id: a custom chord, then a definition
        /// (or its override). An unassigned slot plays nothing.
        /// </summary>
        private int CustomWheelFretOffset(int stringIndex)
        {
            string chordId = GetWheelChordId(_currentChordIndex);
            if (string.IsNullOrEmpty(chordId))
            {
                return ChordDefinition.MUTED_STRING_FRET;
            }

            ChordEntry custom = CustomChordStore.GetCustom(chordId);
            if (custom != null && custom.Frets != null && stringIndex >= 0 && stringIndex < custom.Frets.Length)
            {
                return custom.Frets[stringIndex];
            }

            ChordEntry overridden = CustomChordStore.GetOverride(chordId);
            if (overridden != null && overridden.Frets != null
                && stringIndex >= 0 && stringIndex < overridden.Frets.Length)
            {
                return overridden.Frets[stringIndex];
            }

            return _definitionsByName != null && _definitionsByName.TryGetValue(chordId, out ChordDefinition definition)
                ? definition.GetFretOffset(stringIndex)
                : ChordDefinition.MUTED_STRING_FRET;
        }

        public bool IsStringMuted(int stringIndex)
        {
            return GetFretOffset(stringIndex) == ChordDefinition.MUTED_STRING_FRET;
        }

        /// <summary>
        /// Auditions a chord definition in place of the selected one. Used by the Chord
        /// Maker so its test strings sound the chord being viewed or edited. Cleared on close.
        /// </summary>
        public void SetPreviewFrets(int[] frets)
        {
            _previewFrets = frets != null && frets.Length == ChordDefinition.STRING_COUNT
                ? (int[])frets.Clone()
                : null;
        }

        public void ClearPreview()
        {
            _previewFrets = null;
        }

        // ---------------------------------------------------------------------
        // Capo
        // ---------------------------------------------------------------------

        public void SetCapo(int fret)
        {
            int clampedFret = Mathf.Clamp(fret, MIN_CAPO, MAX_CAPO);
            if (_currentCapo == clampedFret)
            {
                return;
            }

            _currentCapo = clampedFret;
            OnCapoChanged?.Invoke(_currentCapo);
        }

        public void IncrementCapo()
        {
            SetCapo(_currentCapo + 1);
        }

        public void DecrementCapo()
        {
            SetCapo(_currentCapo - 1);
        }
    }
}
