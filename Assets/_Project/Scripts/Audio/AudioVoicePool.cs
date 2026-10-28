using UnityEngine;
using UnityEngine.Audio;

namespace Chordyceps.Audio
{
    public class AudioVoicePool : MonoBehaviour
    {
        [SerializeField] private int _poolSize = DEFAULT_POOL_SIZE;

        // Optional routing target. Null keeps voices on the Master group, which
        // is the acoustic pool's behaviour and leaves it unaffected.
        [SerializeField] private AudioMixerGroup _outputGroup;

        private AudioSource[] _sources;
        private float[] _startTimes;
        private bool _isInitialized = false;

        public const int DEFAULT_POOL_SIZE = 18;

        public int PoolSize => _poolSize;

        private void Awake()
        {
            InitializePool();
        }

        private void InitializePool()
        {
            if (_isInitialized) return;

            GameObject voicesHolder = CreateVoicesHolder();
            _sources = new AudioSource[_poolSize];
            _startTimes = new float[_poolSize];

            for (int i = 0; i < _poolSize; i++)
            {
                _sources[i] = CreateAudioSource(voicesHolder, i);
                _startTimes[i] = -1f;
            }

            _isInitialized = true;
        }

        private GameObject CreateVoicesHolder()
        {
            GameObject holder = new GameObject("VoiceSources");
            holder.transform.SetParent(transform, false);
            return holder;
        }

        private AudioSource CreateAudioSource(GameObject holder, int index)
        {
            AudioSource source = holder.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = _outputGroup;
            return source;
        }

        public void Play(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            if (!_isInitialized) InitializePool();

            int voiceIndex = GetAvailableVoiceIndex();
            PlayClipOnVoice(voiceIndex, clip, volume);
        }

        private int GetAvailableVoiceIndex()
        {
            int idleIndex = FindIdleVoiceIndex();
            if (idleIndex >= 0)
            {
                return idleIndex;
            }

            return FindOldestVoiceIndex();
        }

        private int FindIdleVoiceIndex()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                if (!_sources[i].isPlaying)
                {
                    return i;
                }
            }

            return -1;
        }

        private int FindOldestVoiceIndex()
        {
            int oldestIndex = 0;
            float oldestTime = _startTimes[0];

            for (int i = 1; i < _poolSize; i++)
            {
                if (_startTimes[i] < oldestTime)
                {
                    oldestTime = _startTimes[i];
                    oldestIndex = i;
                }
            }

            return oldestIndex;
        }

        private void PlayClipOnVoice(int index, AudioClip clip, float volume)
        {
            AudioSource source = _sources[index];
            source.clip = clip;
            source.volume = Mathf.Clamp01(volume);
            source.Play();
            _startTimes[index] = Time.time;
        }
    }
}
