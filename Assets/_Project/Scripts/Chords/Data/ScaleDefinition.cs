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

        public string ScaleName => _scaleName;
        public int CircleOfFifthsIndex => _circleOfFifthsIndex;
        public int TonicChordIndex => _tonicChordIndex;

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
    }
}
