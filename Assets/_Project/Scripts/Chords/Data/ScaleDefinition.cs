using UnityEngine;

namespace Chordyceps.Chords
{
    [CreateAssetMenu(fileName = "Scale_", menuName = "Chordyceps/Chords/Scale Definition")]
    public class ScaleDefinition : ScriptableObject
    {
        [SerializeField] private string _scaleName = "C Major";
        [SerializeField] private int _circleOfFifthsIndex = 0;
        [SerializeField] private int _tonicChordIndex = 1;
        [SerializeField] private ChordDefinition[] _diatonicChords = new ChordDefinition[DIATONIC_CHORD_COUNT];

        public const int DIATONIC_CHORD_COUNT = 6;
        public const int MAJOR_CHORD_COUNT = 3;

        private static readonly string[] _slotRomanNumerals = new string[] { "IV", "I", "V", "iii", "vi", "ii" };

        public string ScaleName => _scaleName;
        public int CircleOfFifthsIndex => _circleOfFifthsIndex;
        public int TonicChordIndex => _tonicChordIndex;

        public string GetRomanNumeral(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slotRomanNumerals.Length)
            {
                return string.Empty;
            }

            return _slotRomanNumerals[slotIndex];
        }

        public ChordDefinition GetChord(int index)
        {
            if (index < 0 || index >= _diatonicChords.Length)
            {
                return null;
            }

            return _diatonicChords[index];
        }

        public ChordDefinition GetMajorChord(int index)
        {
            if (index < 0 || index >= MAJOR_CHORD_COUNT)
            {
                return null;
            }

            return _diatonicChords[index];
        }

        public ChordDefinition GetMinorChord(int index)
        {
            int mappedIndex = MAJOR_CHORD_COUNT + index;
            return GetChord(mappedIndex);
        }

        public void Initialize(string scaleName, int circleOfFifthsIndex, int tonicChordIndex, ChordDefinition[] chords)
        {
            _scaleName = scaleName;
            _circleOfFifthsIndex = circleOfFifthsIndex;
            _tonicChordIndex = tonicChordIndex;
            _diatonicChords = chords;
        }
    }
}
