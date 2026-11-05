using UnityEngine;

namespace Chordyceps.Audio
{
    /// <summary>Acoustic sample bank: 50 recorded notes from C2 (index 0) to C#6.</summary>
    [CreateAssetMenu(fileName = "ChromaticAudioBank", menuName = "Chordyceps/Audio/Chromatic Audio Bank")]
    public class ChromaticAudioBank : PitchOrderedAudioBank
    {
    }
}
