using System;

namespace Chordyceps.Chords
{
    /// <summary>
    /// One wheel in a layout: the six chord slots drawn around the circle, in the same
    /// slot order the built-in wheel uses (IV, I, V, iii, vi, ii for a major scale).
    /// </summary>
    [Serializable]
    public class WheelEntry
    {
        public const int SECTOR_COUNT = 6;

        /// <summary>
        /// Button label. Left empty on player layouts, where the wheel is numbered instead.
        /// </summary>
        public string Label;

        /// <summary>
        /// Chord id per sector — a definition's name or a custom chord's id. Null or empty
        /// means the sector is unassigned.
        /// </summary>
        public string[] SectorChordIds = new string[SECTOR_COUNT];

        public string GetSector(int slot)
        {
            return slot >= 0 && slot < SectorChordIds.Length ? SectorChordIds[slot] : null;
        }

        public void SetSector(int slot, string chordId)
        {
            if (slot < 0 || slot >= SectorChordIds.Length)
            {
                return;
            }

            SectorChordIds[slot] = string.IsNullOrEmpty(chordId) ? null : chordId;
        }

        public WheelEntry Clone()
        {
            var copy = new WheelEntry { Label = Label };
            for (int slot = 0; slot < SECTOR_COUNT; slot++)
            {
                copy.SectorChordIds[slot] = GetSector(slot);
            }

            return copy;
        }
    }
}
