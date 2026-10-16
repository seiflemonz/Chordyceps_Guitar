using System;
using UnityEngine;

namespace Chordyceps.Chords
{
    public class ChordManager : MonoBehaviour
    {
        [SerializeField] private ScaleDefinition _currentScale;
        [SerializeField] private int _currentChordIndex = 1;
        [SerializeField] private int _currentCapo = 0;

        public const int MIN_CAPO = 0;
        public const int MAX_CAPO = 12;

        public event Action<ScaleDefinition> OnScaleChanged;
        public event Action<ChordDefinition> OnChordChanged;
        public event Action<int> OnCapoChanged;

        public ScaleDefinition CurrentScale => _currentScale;
        public ChordDefinition CurrentChord => _currentScale != null ? _currentScale.GetChord(_currentChordIndex) : null;
        public int CurrentChordIndex => _currentChordIndex;
        public int CurrentCapo => _currentCapo;

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
