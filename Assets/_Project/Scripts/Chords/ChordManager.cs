using System;
using UnityEngine;

namespace Chordyceps.Chords
{
    public class ChordManager : MonoBehaviour
    {
        [SerializeField] private ScaleDefinition[] _allScales = new ScaleDefinition[12];
        [SerializeField] private int _currentKeyIndex = 0;
        [SerializeField] private int _currentChordIndex = 1;
        [SerializeField] private int _currentCapo = 0;

        public const int MIN_CAPO = 0;
        public const int MAX_CAPO = 12;

        public event Action<ScaleDefinition> OnScaleChanged;
        public event Action<ChordDefinition> OnChordChanged;
        public event Action<int> OnCapoChanged;

        public ScaleDefinition CurrentScale => _allScales != null && _currentKeyIndex < _allScales.Length
            ? _allScales[_currentKeyIndex] : null;
        public ChordDefinition CurrentChord => CurrentScale != null ? CurrentScale.GetChord(_currentChordIndex) : null;
        public int CurrentChordIndex => _currentChordIndex;
        public int CurrentCapo => _currentCapo;

        private void Start()
        {
            PublishInitialState();
        }

        private void PublishInitialState()
        {
            if (CurrentScale != null)
            {
                OnScaleChanged?.Invoke(CurrentScale);
                OnChordChanged?.Invoke(CurrentChord);
            }
            OnCapoChanged?.Invoke(_currentCapo);
        }

        public void SelectChord(int chordIndex)
        {
            if (CurrentScale == null || chordIndex < 0 || chordIndex >= ScaleDefinition.DIATONIC_CHORD_COUNT)
            {
                return;
            }

            _currentChordIndex = chordIndex;
            OnChordChanged?.Invoke(CurrentChord);
        }

        public void IncrementKey()
        {
            SetKeyIndex((_currentKeyIndex + 1) % _allScales.Length);
        }

        public void DecrementKey()
        {
            int prev = (_currentKeyIndex - 1 + _allScales.Length) % _allScales.Length;
            SetKeyIndex(prev);
        }

        private void SetKeyIndex(int index)
        {
            if (_allScales == null || index < 0 || index >= _allScales.Length)
            {
                return;
            }

            ScaleDefinition newScale = _allScales[index];
            if (newScale == null || newScale == CurrentScale)
            {
                return;
            }

            _currentKeyIndex = index;
            _currentChordIndex = newScale.TonicChordIndex;
            OnScaleChanged?.Invoke(CurrentScale);
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

        public int GetFretOffset(int stringIndex)
        {
            ChordDefinition chord = CurrentChord;
            return chord == null ? 0 : chord.GetFretOffset(stringIndex);
        }

        public bool IsStringMuted(int stringIndex)
        {
            ChordDefinition chord = CurrentChord;
            return chord != null && chord.IsStringMuted(stringIndex);
        }
    }
}
