using UnityEngine;

namespace Chordyceps.Audio
{
    /// <summary>
    /// Shared base for pitch-ordered guitar sample banks. Clips are stored in
    /// chromatic order from the lowest recorded note upward, so any string can be
    /// tuned to any note through a single index.
    /// </summary>
    public abstract class PitchOrderedAudioBank : ScriptableObject
    {
        [SerializeField] private AudioClip[] _audioClips = new AudioClip[TOTAL_NOTES];

        /// <summary>
        /// Recorded notes run C2 (index 0) to C#6 (index 49) inclusive.
        /// </summary>
        public const int TOTAL_NOTES = 50;

        // Standard tuning note indices relative to index 0 (C2): E2 A2 D3 G3 B3 E4.
        private static readonly int[] _standardTuningOffsets = new int[] { 4, 9, 14, 19, 23, 28 };

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
