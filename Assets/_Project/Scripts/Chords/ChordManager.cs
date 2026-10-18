using System;
using UnityEngine;

namespace Chordyceps.Chords
{
    public class ChordManager : MonoBehaviour
    {
        [SerializeField] private ScaleDefinition[] _allScales = new ScaleDefinition[TOTAL_KEYS];
        [SerializeField] private ScaleDefinition _currentScale;
        [SerializeField] private int _currentKeyIndex = 0;
        [SerializeField] private int _currentChordIndex = 1;
        [SerializeField] private int _currentCapo = 0;

        public const int TOTAL_KEYS = 12;
        public const int MIN_CAPO = 0;
        public const int MAX_CAPO = 12;

        public event Action<ScaleDefinition> OnScaleChanged;
        public event Action<ChordDefinition> OnChordChanged;
        public event Action<int> OnCapoChanged;

        public ScaleDefinition CurrentScale => _currentScale;
        public ChordDefinition CurrentChord => _currentScale != null ? _currentScale.GetChord(_currentChordIndex) : null;
        public int CurrentKeyIndex => _currentKeyIndex;
        public int CurrentChordIndex => _currentChordIndex;
        public int CurrentCapo => _currentCapo;
        public int TotalKeys => _allScales != null ? _allScales.Length : 0;

        private void Awake()
        {
            InitializeDefaultScale();
        }

        private void InitializeDefaultScale()
        {
            if (_allScales != null && _allScales.Length > _currentKeyIndex && _allScales[_currentKeyIndex] != null)
            {
                _currentScale = _allScales[_currentKeyIndex];
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

        public void SetKey(int keyIndex)
        {
            if (_allScales == null || _allScales.Length == 0 || keyIndex < 0 || keyIndex >= _allScales.Length)
            {
                return;
            }

            _currentKeyIndex = keyIndex;
            SetScale(_allScales[keyIndex]);
        }

        public void IncrementKey()
        {
            int nextIndex = (_currentKeyIndex + 1) % TOTAL_KEYS;
            SetKey(nextIndex);
        }

        public void DecrementKey()
        {
            int prevIndex = (_currentKeyIndex - 1 + TOTAL_KEYS) % TOTAL_KEYS;
            SetKey(prevIndex);
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
