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
        [SerializeField] private AudioVoicePool _electricVoicePool;
        [SerializeField] private ElectricAudioBank _electricBank;
        [SerializeField] private ChordManager _chordManager;

        public const int STRING_COUNT = 6;

        public event Action<StringInputData> OnStringPlucked;

        private void Awake()
        {
            FindChordManagerIfMissing();
            PreloadAudioClips();
        }

        private void PreloadAudioClips()
        {
            _audioBank?.PreloadAllClips();
            _electricBank?.PreloadAllClips();
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
            PitchOrderedAudioBank bank = GuitarModeState.IsElectric ? _electricBank : (PitchOrderedAudioBank)_audioBank;
            AudioVoicePool pool = GuitarModeState.IsElectric ? _electricVoicePool : _voicePool;

            if (bank == null || pool == null)
            {
                return;
            }

            if (_chordManager != null && _chordManager.IsStringMuted(stringIndex))
            {
                return;
            }

            int noteIndex = CalculateNoteIndex(bank, stringIndex);
            AudioClip clip = bank.GetClip(noteIndex);
            if (clip != null)
            {
                pool.Play(clip, velocity);
            }
        }

        private int CalculateNoteIndex(PitchOrderedAudioBank bank, int stringIndex)
        {
            int baseOffset = bank.GetStandardOpenStringOffset(stringIndex);
            if (_chordManager == null)
            {
                return baseOffset;
            }

            int fretOffset = _chordManager.GetFretOffset(stringIndex);
            return baseOffset + _chordManager.CurrentCapo + fretOffset;
        }

        private void NotifyPluckListeners(StringInputData data)
        {
            OnStringPlucked?.Invoke(data);
        }
    }
}
