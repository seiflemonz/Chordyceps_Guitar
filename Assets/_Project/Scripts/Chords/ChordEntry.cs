using System;

namespace Chordyceps.Chords
{
    /// <summary>
    /// Player-authored chord data persisted to PlayerPrefs. Either an override of a
    /// pre-existing chord definition (identified by the source chord's name) or a
    /// brand-new chord created in the Chord Maker (identified by a generated id).
    /// </summary>
    [Serializable]
    public class ChordEntry
    {
        public const int STRING_COUNT = 6;

        /// <summary>Stable lookup key: source chord name for overrides, generated id for customs.</summary>
        public string Id;

        /// <summary>Player-facing name shown in the Chord Maker strip.</summary>
        public string Name;

        /// <summary>Per-string fret offsets: -1 muted, 0 open, otherwise the absolute fret.</summary>
        public int[] Frets = new int[STRING_COUNT];

        /// <summary>False when this replaces a pre-existing definition, true for player-created chords.</summary>
        public bool IsCustom;

        public bool IsMajor = true;

        public ChordEntry Clone()
        {
            return new ChordEntry
            {
                Id = Id,
                Name = Name,
                Frets = Frets != null ? (int[])Frets.Clone() : new int[STRING_COUNT],
                IsCustom = IsCustom,
                IsMajor = IsMajor,
            };
        }
    }
}
