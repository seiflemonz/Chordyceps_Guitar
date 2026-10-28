using UnityEngine;

namespace Chordyceps.Audio
{
    /// <summary>
    /// Pitch-ordered bank of raw DI electric guitar samples, mirroring
    /// ChromaticAudioBank so the 43 recorded notes can be dropped in without
    /// any code change. Index 0 is the lowest playable note (D2).
    /// </summary>
    [CreateAssetMenu(fileName = "ElectricAudioBank", menuName = "Chordyceps/Audio/Electric Audio Bank")]
    public class ElectricAudioBank : ScriptableObject
    {
        [SerializeField] private AudioClip[] _audioClips = new AudioClip[TOTAL_NOTES];

        public const int TOTAL_NOTES = 43;

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

        public void SetClips(AudioClip[] clips)
        {
            _audioClips = clips;
        }
    }
}
