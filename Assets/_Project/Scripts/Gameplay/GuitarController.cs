using System;
using UnityEngine;
using Chordyceps.Audio;
using Chordyceps.Input;
using Chordyceps.Chords;

namespace Chordyceps.Gameplay
{
    public class GuitarController : MonoBehaviour, IStringInputListener
    {
        [SerializeField] private AudioVoicePool _voicePool;
        [SerializeField] private ChromaticAudioBank _audioBank;
        [SerializeField] private ChordManager _chordManager;

        public const int STRING_COUNT = 6;

        public event Action<StringInputData> OnStringPlucked;

        private void Awake()
        {
            FindChordManagerIfMissing();
        }

        private void FindChordManagerIfMissing()
        {
            if (_chordManager == null)
            {
                _chordManager = FindFirstObjectByType<ChordManager>();
            }
        }

        public void OnStringInteracted(StringInputData data)
        {
            if (!IsValidStringIndex(data.StringIndex))
            {
                return;
            }

            PlayStringAudio(data.StringIndex, data.Velocity);
            NotifyPluckListeners(data);
        }

        private bool IsValidStringIndex(int index)
        {
            return index >= 0 && index < STRING_COUNT;
        }

        private void PlayStringAudio(int stringIndex, float velocity)
        {
            if (_audioBank == null || _voicePool == null)
            {
                return;
            }

            if (_chordManager != null && _chordManager.IsStringMuted(stringIndex))
            {
                return;
            }

            int noteIndex = CalculateNoteIndex(stringIndex);
            AudioClip clip = _audioBank.GetClip(noteIndex);
            if (clip != null)
            {
                _voicePool.Play(clip, velocity);
            }
        }

        private int CalculateNoteIndex(int stringIndex)
        {
            int baseOffset = _audioBank.GetStandardOpenStringOffset(stringIndex);
            if (_chordManager == null)
            {
                return baseOffset;
            }

            int fretOffset = _chordManager.GetFretOffset(stringIndex);
            return baseOffset + _chordManager.CurrentCapo + fretOffset;
        }

        private void NotifyPluckListeners(StringInputData data)
        {
            Debug.Log($"String {data.StringIndex} [{data.InputType}] Velocity: {data.Velocity:F2}");
            OnStringPlucked?.Invoke(data);
        }
    }
}
