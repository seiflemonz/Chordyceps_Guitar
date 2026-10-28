using UnityEngine;
using Chordyceps.Audio;

namespace Chordyceps.Gameplay
{
    /// <summary>
    /// Plays a raw DI electric guitar note through the amp-routed voice pool.
    /// Until the 43 DI samples exist, SPACE plays a single configurable note
    /// slot on the electric bank.
    /// </summary>
    public class ElectricGuitarController : MonoBehaviour
    {
        [SerializeField] private AudioVoicePool _voicePool;
        [SerializeField] private ElectricAudioBank _audioBank;
        [SerializeField] private int _noteIndex = 0;
        [SerializeField] private float _velocity = 1f;

        private void Awake()
        {
            if (_audioBank != null)
            {
                _audioBank.PreloadAllClips();
            }
        }

        /// <summary>One-shot playback of the configured DI note.</summary>
        public void PlaySingleNote()
        {
            if (_audioBank == null || _voicePool == null)
            {
                return;
            }

            AudioClip clip = _audioBank.GetClip(_noteIndex);
            if (clip != null)
            {
                _voicePool.Play(clip, _velocity);
            }
        }
    }
}
