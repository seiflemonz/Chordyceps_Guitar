using UnityEngine;

namespace Chordyceps.Chords
{
    [CreateAssetMenu(fileName = "Chord_", menuName = "Chordyceps/Chords/Chord Definition")]
    public class ChordDefinition : ScriptableObject
    {
        [SerializeField] private string _chordName = "C";
        [SerializeField] private bool _isMajor = true;
        [SerializeField] private int[] _fretOffsets = new int[STRING_COUNT];

        public const int STRING_COUNT = 6;
        public const int MUTED_STRING_FRET = -1;

        public string ChordName => _chordName;
        public bool IsMajor => _isMajor;

        public int GetFretOffset(int stringIndex)
        {
            if (stringIndex < 0 || stringIndex >= _fretOffsets.Length)
            {
                return MUTED_STRING_FRET;
            }

            return _fretOffsets[stringIndex];
        }

        public bool IsStringMuted(int stringIndex)
        {
            return GetFretOffset(stringIndex) == MUTED_STRING_FRET;
        }

        public void Initialize(string chordName, bool isMajor, int[] fretOffsets)
        {
            _chordName = chordName;
            _isMajor = isMajor;
            _fretOffsets = fretOffsets;
        }

        public void SetFretOffsets(int[] frets)
        {
            _fretOffsets = frets;
        }
    }
}
