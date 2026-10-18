using System;
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

        public const int TOTAL_KEYS = 12;
        public const int MIN_CAPO = 0;
        public const int MAX_CAPO = 12;
        public const int MINOR_SCALE_OFFSET = 9;

        private static readonly string[] _rootNames = new string[]
        {
            "C", "G", "D", "A", "E", "B", "F#", "Db", "Ab", "Eb", "Bb", "F"
        };

        public event Action<ScaleDefinition> OnScaleChanged;
        public event Action<ChordDefinition> OnChordChanged;
        public event Action<int> OnCapoChanged;
        public event Action<string, KeyMode> OnKeySelectionChanged;

        public ScaleDefinition CurrentScale => _currentScale;
        public ChordDefinition CurrentChord => _currentScale != null ? _currentScale.GetChord(_currentChordIndex) : null;
        public int CurrentKeyIndex => _currentKeyIndex;
        public int CurrentChordIndex => _currentChordIndex;
        public int CurrentCapo => _currentCapo;
        public int CurrentRootIndex => _currentRootIndex;
        public KeyMode CurrentMode => _currentMode;
        public string CurrentRootName => _rootNames[_currentRootIndex];
        public int TotalKeys => _allScales != null ? _allScales.Length : 0;

        private void Awake()
        {
            InitializeDefaultScale();
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

        private void Start()
        {
            PublishInitialState();
        }

        private void PublishInitialState()
        {
            if (_currentScale != null)
            {
                OnScaleChanged?.Invoke(_currentScale);
                OnChordChanged?.Invoke(CurrentChord);
            }
            OnCapoChanged?.Invoke(_currentCapo);
            OnKeySelectionChanged?.Invoke(CurrentRootName, _currentMode);
        }

        public void SelectChord(int chordIndex)
        {
            if (_currentScale == null || chordIndex < 0 || chordIndex >= ScaleDefinition.DIATONIC_CHORD_COUNT)
            {
                return;
            }

            _currentChordIndex = chordIndex;
            OnChordChanged?.Invoke(CurrentChord);
        }

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

        public void SetScale(ScaleDefinition newScale)
        {
            if (newScale == null || _currentScale == newScale)
            {
                return;
            }

            _currentScale = newScale;
            _currentChordIndex = newScale.TonicChordIndex;
            OnScaleChanged?.Invoke(_currentScale);
            OnChordChanged?.Invoke(CurrentChord);
        }

        public void SetKey(int rootIndex)
        {
            if (_allScales == null || _allScales.Length == 0) return;

            _currentRootIndex = (rootIndex % TOTAL_KEYS + TOTAL_KEYS) % TOTAL_KEYS;
            int scaleIndex = ResolveScaleIndex(_currentRootIndex, _currentMode);
            _currentKeyIndex = scaleIndex;
            SetScale(_allScales[scaleIndex]);
            OnKeySelectionChanged?.Invoke(CurrentRootName, _currentMode);
        }

        public void SetKeyMode(KeyMode mode)
        {
            if (_currentMode == mode) return;
            _currentMode = mode;
            SetKey(_currentRootIndex);
        }

        public void ToggleKeyMode()
        {
            SetKeyMode(_currentMode == KeyMode.Major ? KeyMode.Minor : KeyMode.Major);
        }

        public void IncrementKey()
        {
            SetKey(_currentRootIndex + 1);
        }

        public void DecrementKey()
        {
            SetKey(_currentRootIndex - 1);
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
            InitializeDefaultScale();
        }

        public int GetFretOffset(int stringIndex)
        {
            ChordDefinition chord = CurrentChord;
            if (chord == null)
            {
                return 0;
            }

            return chord.GetFretOffset(stringIndex);
        }

        public bool IsStringMuted(int stringIndex)
        {
            ChordDefinition chord = CurrentChord;
            if (chord == null)
            {
                return false;
            }

            return chord.IsStringMuted(stringIndex);
        }
    }
}
