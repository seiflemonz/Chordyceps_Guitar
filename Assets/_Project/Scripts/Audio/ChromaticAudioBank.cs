using UnityEngine;

namespace Chordyceps.Audio
{
    [CreateAssetMenu(fileName = "ChromaticAudioBank", menuName = "Chordyceps/Audio/Chromatic Audio Bank")]
    public class ChromaticAudioBank : ScriptableObject
    {
        [SerializeField] private AudioClip[] _audioClips = new AudioClip[TOTAL_NOTES];

        public const int TOTAL_NOTES = 43;
        private const int BASE_NOTE_SEMITONE = 26; // MIDI D2 = 26
        private const int SEMITONES_PER_OCTAVE = 12;

        private static readonly int[] _standardTuningOffsets = new int[] { 2, 7, 12, 17, 21, 26 };

        public int ClipCount => _audioClips != null ? _audioClips.Length : 0;

        public void PreloadAllClips()
        {
            if (_audioClips == null) return;

            for (int i = 0; i < _audioClips.Length; i++)
            {
                AudioClip clip = _audioClips[i];
                if (clip != null && clip.loadState != AudioDataLoadState.Loaded)
                {
                    clip.LoadAudioData();
                }
            }
        }

        public AudioClip GetClip(int noteIndex)
        {
            if (!IsValidNoteIndex(noteIndex))
            {
                return null;
            }

            return _audioClips[noteIndex];
        }

        public bool IsValidNoteIndex(int noteIndex)
        {
            return _audioClips != null && noteIndex >= 0 && noteIndex < _audioClips.Length;
        }

        public int GetStandardOpenStringOffset(int stringIndex)
        {
            if (stringIndex < 0 || stringIndex >= _standardTuningOffsets.Length)
            {
                return 0;
            }

            return _standardTuningOffsets[stringIndex];
        }

        public void SetClips(AudioClip[] clips)
        {
            _audioClips = clips;
        }
    }
}
