namespace Chordyceps.Audio
{
    /// <summary>
    /// The seven amp controls exposed by the electric guitar signal chain.
    /// </summary>
    public enum AmpParameter
    {
        Gain,
        Volume,
        Bass,
        Mid,
        Treble,
        Presence,
        Reverb
    }

    /// <summary>
    /// Single source of truth for the amp's AudioMixer contract: the exposed
    /// parameter names the mixer must provide, and the mapping from a
    /// normalised knob value (0..1) to the mixer's native units.
    /// </summary>
    public static class AmpMixerParameters
    {
        public const string GAIN_NAME = "AmpGain";
        public const string DRIVE_NAME = "AmpDrive"; // Pre-distortion volume boost
        public const string VOLUME_NAME = "AmpVolume";
        public const string BASS_NAME = "AmpBass";
        public const string MID_NAME = "AmpMid";
        public const string TREBLE_NAME = "AmpTreble";
        public const string PRESENCE_NAME = "AmpPresence";
        public const string REVERB_NAME = "AmpReverb";

        // ParamEQ Frequency Gain is a multiplier (0.05 to 3.0). 
        // 1.0 is neutral. We map 0..1 knob to ~ -20dB to +9.5dB.
        private const float MIN_EQ_MULT = 0.1f;  // ~ -20dB
        private const float NEUTRAL_EQ_MULT = 1.0f; // 0dB
        private const float MAX_EQ_MULT = 3.0f;  // ~ +9.5dB

        private const float MIN_VOLUME_DB = -80f; // Unity standard for silence
        private const float MAX_VOLUME_DB = 0f;

        // Pre-distortion drive boost via Compressor Make-up Gain (0 to +30dB)
        private const float MIN_DRIVE_DB = 0f;
        private const float MAX_DRIVE_DB = 30f;

        // SFX Reverb wet level is expressed in millibels (-10000..0).
        private const float MIN_REVERB_MB = -10000f;
        private const float MAX_REVERB_MB = 0f;

        public static readonly AmpParameter[] All =
        {
            AmpParameter.Gain,
            AmpParameter.Volume,
            AmpParameter.Bass,
            AmpParameter.Mid,
            AmpParameter.Treble,
            AmpParameter.Presence,
            AmpParameter.Reverb
        };

        public static string GetExposedName(AmpParameter parameter)
        {
            switch (parameter)
            {
                case AmpParameter.Gain: return GAIN_NAME;
                case AmpParameter.Volume: return VOLUME_NAME;
                case AmpParameter.Bass: return BASS_NAME;
                case AmpParameter.Mid: return MID_NAME;
                case AmpParameter.Treble: return TREBLE_NAME;
                case AmpParameter.Presence: return PRESENCE_NAME;
                default: return REVERB_NAME;
            }
        }

        /// <summary>Label shown under the knob in the amp panel.</summary>
        public static string GetDisplayName(AmpParameter parameter)
        {
            switch (parameter)
            {
                case AmpParameter.Gain: return "GAIN";
                case AmpParameter.Volume: return "VOLUME";
                case AmpParameter.Bass: return "BASS";
                case AmpParameter.Mid: return "MID";
                case AmpParameter.Treble: return "TREBLE";
                case AmpParameter.Presence: return "PRESENCE";
                default: return "REVERB";
            }
        }

        public static float GetDefaultNormalized(AmpParameter parameter)
        {
            switch (parameter)
            {
                case AmpParameter.Gain: return 0.5f;
                case AmpParameter.Volume: return 0.8f;
                case AmpParameter.Reverb: return 0.2f;
                default: return 0.5f;
            }
        }

        /// <summary>Converts a normalised knob value into the mixer's native units.</summary>
        public static float ToMixerValue(AmpParameter parameter, float normalized)
        {
            float value = normalized < 0f ? 0f : (normalized > 1f ? 1f : normalized);

            switch (parameter)
            {
                case AmpParameter.Gain:
                    // Distortion Level (0..1). We use the full range now 
                    // because we are also boosting the input via AmpDrive.
                    return 0.1f + (value * 0.88f);

                case AmpParameter.Volume:
                    return Lerp(MIN_VOLUME_DB, MAX_VOLUME_DB, value);

                case AmpParameter.Reverb:
                    // Logarithmic-like response for reverb millibels.
                    float reverbT = 1f - UnityEngine.Mathf.Pow(1f - value, 2f);
                    return Lerp(MIN_REVERB_MB, MAX_REVERB_MB, reverbT);

                default:
                    // ParamEQ Multiplier mapping. 0.5 knob = 1.0 (Neutral).
                    if (value <= 0.5f)
                    {
                        return Lerp(MIN_EQ_MULT, NEUTRAL_EQ_MULT, value * 2f);
                    }
                    else
                    {
                        return Lerp(NEUTRAL_EQ_MULT, MAX_EQ_MULT, (value - 0.5f) * 2f);
                    }
            }
        }

        /// <summary>Returns the pre-distortion drive boost in dB.</summary>
        public static float ToDriveValue(float normalized)
        {
            float value = normalized < 0f ? 0f : (normalized > 1f ? 1f : normalized);
            // Drive starts kicking in after 20% gain.
            float t = UnityEngine.Mathf.Clamp01((value - 0.2f) / 0.8f);
            return Lerp(MIN_DRIVE_DB, MAX_DRIVE_DB, t);
        }

        private static float Lerp(float from, float to, float t)
        {
            return from + (to - from) * t;
        }
    }
}
