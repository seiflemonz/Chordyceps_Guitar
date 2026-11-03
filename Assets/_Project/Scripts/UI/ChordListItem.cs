using Chordyceps.Chords;

namespace Chordyceps.UI
{
    /// <summary>
    /// One row in the chord maker strip: either a pre-existing definition asset
    /// (possibly carrying a stored override) or a player-created chord.
    /// </summary>
    public class ChordListItem
    {
        /// <summary>Stable lookup key: chord name for definitions, generated id for customs.</summary>
        public string Id;

        /// <summary>Player-facing name shown in the strip.</summary>
        public string Name;

        /// <summary>Fingering currently in effect: -1 muted, 0 open, otherwise the absolute fret.</summary>
        public int[] Frets;

        /// <summary>Asset default for a pre-existing chord; null for custom chords.</summary>
        public int[] DefaultFrets;

        public bool IsCustom;

        /// <summary>True when a pre-existing chord carries a stored override.</summary>
        public bool IsModified;

        public bool IsMajor;

        /// <summary>Builds the row for a definition asset, applying any stored override.</summary>
        public static ChordListItem FromDefinition(ChordDefinition chord)
        {
            ChordEntry overridden = CustomChordStore.GetOverride(chord.ChordName);
            return new ChordListItem
            {
                Id = chord.ChordName,
                Name = chord.ChordName,
                Frets = overridden != null ? CopyFrets(overridden.Frets) : ReadFrets(chord),
                DefaultFrets = ReadFrets(chord),
                IsCustom = false,
                IsModified = overridden != null,
                IsMajor = chord.IsMajor,
            };
        }

        /// <summary>Builds the row for a player-created chord, which has no asset default.</summary>
        public static ChordListItem FromCustom(ChordEntry custom)
        {
            return new ChordListItem
            {
                Id = custom.Id,
                Name = custom.Name,
                Frets = CopyFrets(custom.Frets),
                IsCustom = true,
                IsModified = false,
                IsMajor = custom.IsMajor,
            };
        }

        /// <summary>True when <paramref name="frets"/> is the untouched asset default.</summary>
        public bool MatchesDefault(int[] frets)
        {
            if (DefaultFrets == null || frets == null || frets.Length != DefaultFrets.Length)
            {
                return false;
            }

            for (int i = 0; i < frets.Length; i++)
            {
                if (frets[i] != DefaultFrets[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static int[] ReadFrets(ChordDefinition chord)
        {
            var frets = new int[ChordNeckElement.STRING_COUNT];
            for (int i = 0; i < frets.Length; i++)
            {
                frets[i] = chord.GetFretOffset(i);
            }

            return frets;
        }

        private static int[] CopyFrets(int[] source)
        {
            return source != null ? (int[])source.Clone() : new int[ChordNeckElement.STRING_COUNT];
        }
    }
}
