using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Chordyceps.Audio
{
    /// <summary>
    /// Audible owner of the electric amp signal chain. Translates the amp
    /// panel's normalised knob values into exposed AudioMixer parameters.
    /// </summary>
    public class AmpMixerController : MonoBehaviour
    {
        [SerializeField] private AudioMixer _mixer;

        private readonly HashSet<string> _warnedParameters = new HashSet<string>();

        public bool IsReady => _mixer != null;

        public void ApplyDefaults()
        {
            foreach (AmpParameter parameter in AmpMixerParameters.All)
            {
                SetParameter(parameter, AmpMixerParameters.GetDefaultNormalized(parameter));
            }
        }

        public void SetParameter(AmpParameter parameter, float normalized)
        {
            if (_mixer == null)
            {
                WarnMissingMixer();
                return;
            }

            string name = AmpMixerParameters.GetExposedName(parameter);
            float value = AmpMixerParameters.ToMixerValue(parameter, normalized);

            if (!_mixer.SetFloat(name, value))
            {
                WarnMissingParameter(name);
            }

            // If this is the Gain knob, also try to set the Pre-Distortion Drive (AmpDrive)
            if (parameter == AmpParameter.Gain)
            {
                float driveValue = AmpMixerParameters.ToDriveValue(normalized);
                _mixer.SetFloat(AmpMixerParameters.DRIVE_NAME, driveValue);
            }
        }

        /// <summary>True when the assigned mixer exposes the given parameter.</summary>
        public bool HasParameter(AmpParameter parameter)
        {
            if (_mixer == null)
            {
                return false;
            }

            return _mixer.GetFloat(AmpMixerParameters.GetExposedName(parameter), out _);
        }

        private void WarnMissingMixer()
        {
            if (_warnedParameters.Contains(string.Empty))
            {
                return;
            }

            _warnedParameters.Add(string.Empty);
            Debug.LogWarning("[AmpMixerController] No AudioMixer assigned; amp knobs have no effect.");
        }

        private void WarnMissingParameter(string name)
        {
            if (!_warnedParameters.Add(name))
            {
                return;
            }

            Debug.LogWarning($"[AmpMixerController] AudioMixer does not expose '{name}'. " +
                             "Run Tools/Chordyceps/Validate Amp Mixer for the expected parameter list.");
        }
    }
}
