using System;

namespace Chordyceps.Chords
{
    /// <summary>
    /// The barre a fingering implies: its lowest fretted note, held on two or more
    /// strings, spanning from the lowest-pitched string holding it to the highest — a
    /// span with no string left open or muted inside it, since one finger lies across.
    ///
    /// A fingering whose only fretted note is that shared one is not a barre — holding
    /// one note flat across a few strings is just how it is played — unless that note
    /// spans more than four strings, which no other fingering can produce.
    ///
    /// Derived entirely from the frets, so it applies to definition assets, overrides
    /// and player-created chords alike — nothing is authored per chord.
    /// </summary>
    public readonly struct ChordBarre
    {
        /// <summary>
        /// Strings the lowest note must cover to imply a barre on its own, when nothing
        /// is fretted above it.
        /// </summary>
        private const int BARRED_STRING_LIMIT = 4;

        /// <summary>The shared fret the barre sits on.</summary>
        public readonly int Fret;

        /// <summary>Index of one end of the barre, on the lowest-pitched of the two strings.</summary>
        public readonly int FirstString;

        /// <summary>Index of the other end of the barre.</summary>
        public readonly int LastString;

        private ChordBarre(int fret, int firstString, int lastString)
        {
            Fret = fret;
            FirstString = firstString;
            LastString = lastString;
        }

        /// <summary>
        /// Finds the barre a fingering implies. Returns false when no note is held on two
        /// or more strings, when the lowest fretted note is the only fretted note and
        /// covers no more than <see cref="BARRED_STRING_LIMIT"/> strings, or when the
        /// span leaves a string open or muted between its ends.
        /// </summary>
        public static bool TryFind(int[] frets, out ChordBarre barre)
        {
            barre = default;
            if (frets == null)
            {
                return false;
            }

            int lowest = LowestFrettedNote(frets);
            if (lowest <= 0)
            {
                return false;
            }

            int first = -1;
            int last = -1;
            int held = 0;
            bool reachesHigher = false;
            for (int i = 0; i < frets.Length; i++)
            {
                int fret = frets[i];
                if (fret > lowest)
                {
                    reachesHigher = true;
                    continue;
                }

                if (fret != lowest)
                {
                    continue;
                }

                if (first < 0)
                {
                    first = i;
                }

                last = i;
                held++;
            }

            // A note shared by two strings can just be how a shape is fingered, so on its
            // own it implies nothing. Past four strings there is no other way to play it,
            // so that stands as a barre even with nothing fretted above it.
            if (held < 2 || (!reachesHigher && held <= BARRED_STRING_LIMIT))
            {
                return false;
            }

            // One finger lies flat across the whole span, so it cannot cross a string the
            // shape leaves open or muted.
            if (SpansUnplayableString(frets, first, last))
            {
                return false;
            }

            barre = new ChordBarre(lowest, first, last);
            return true;
        }

        /// <summary>True when a string strictly between the two ends is open or muted.</summary>
        private static bool SpansUnplayableString(int[] frets, int first, int last)
        {
            for (int i = first + 1; i < last; i++)
            {
                if (frets[i] <= 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Smallest positive fret held, or 0 when every string is open or muted.</summary>
        private static int LowestFrettedNote(int[] frets)
        {
            int lowest = int.MaxValue;
            for (int i = 0; i < frets.Length; i++)
            {
                if (frets[i] > 0)
                {
                    lowest = Math.Min(lowest, frets[i]);
                }
            }

            return lowest == int.MaxValue ? 0 : lowest;
        }
    }
}
