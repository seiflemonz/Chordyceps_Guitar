using UnityEngine;

namespace Chordyceps.UI
{
    /// <summary>
    /// The working copy of one chord while it is being edited: which fret each string
    /// holds, the visible fret window, and whether anything changed since the draft
    /// started. Pure state — it neither renders nor stores anything.
    /// </summary>
    public class ChordDraft
    {
        /// <summary>Fingering being edited: -1 muted, 0 open, otherwise the absolute fret.</summary>
        public int[] Frets { get; private set; }

        /// <summary>Lowest fret shown on the neck while editing.</summary>
        public int BaseFret { get; private set; } = ChordNeckElement.MIN_BASE_FRET;

        /// <summary>True while an edit session is open.</summary>
        public bool IsActive { get; private set; }

        /// <summary>True when the draft is a chord that does not exist yet.</summary>
        public bool IsNew { get; private set; }

        /// <summary>True once the fingering differs from the shape the draft started from.</summary>
        public bool IsDirty { get; private set; }

        /// <summary>Id of the chord the draft started from; null for a new chord.</summary>
        public string SourceId { get; private set; }

        /// <summary>Strip index that was selected when the draft started; -1 for none.</summary>
        public int SourceIndex { get; private set; } = -1;

        /// <summary>Starts an edit session on a listed chord's current fingering.</summary>
        public void BeginFrom(ChordListItem item, int index)
        {
            Frets = (int[])item.Frets.Clone();
            BaseFret = ChordNeckElement.DeriveBaseFret(Frets);
            SourceId = item.Id;
            SourceIndex = index;
            IsNew = false;
            IsDirty = false;
            IsActive = true;
        }

        /// <summary>Starts a blank chord with every string open, on the lowest fret window.</summary>
        public void BeginNew(int selectedIndex, string selectedId)
        {
            Frets = new int[ChordNeckElement.STRING_COUNT];
            BaseFret = ChordNeckElement.MIN_BASE_FRET;
            SourceId = selectedId;
            SourceIndex = selectedIndex;
            IsNew = true;
            IsDirty = false;
            IsActive = true;
        }

        /// <summary>
        /// Sets a string on <paramref name="fret"/>, or releases it back to open when it
        /// already held that fret. Returns true when the fingering changed.
        /// </summary>
        public bool ToggleFret(int stringIndex, int fret)
        {
            if (!IsEditable(stringIndex))
            {
                return false;
            }

            Frets[stringIndex] = Frets[stringIndex] == fret ? ChordNeckElement.OPEN : fret;
            IsDirty = true;
            return true;
        }

        /// <summary>Mutes a string, or unmutes it to open when it was already muted.</summary>
        public bool ToggleMute(int stringIndex)
        {
            if (!IsEditable(stringIndex))
            {
                return false;
            }

            Frets[stringIndex] = Frets[stringIndex] == ChordNeckElement.MUTED
                ? ChordNeckElement.OPEN
                : ChordNeckElement.MUTED;

            IsDirty = true;
            return true;
        }

        /// <summary>
        /// Slides the visible fret window. This changes what is shown, not the fingering,
        /// so it does not mark the draft dirty.
        /// </summary>
        public bool ShiftBaseFret(int delta)
        {
            if (!IsActive)
            {
                return false;
            }

            BaseFret = Mathf.Clamp(BaseFret + delta, ChordNeckElement.MIN_BASE_FRET, ChordNeckElement.MAX_BASE_FRET);
            return true;
        }

        /// <summary>Resets the working copy to every string open, on the lowest fret window.</summary>
        public bool Clear()
        {
            if (!IsActive)
            {
                return false;
            }

            for (int i = 0; i < Frets.Length; i++)
            {
                Frets[i] = ChordNeckElement.OPEN;
            }

            BaseFret = ChordNeckElement.MIN_BASE_FRET;
            IsDirty = true;
            return true;
        }

        /// <summary>Closes the edit session, discarding the working copy.</summary>
        public void End()
        {
            IsActive = false;
            IsNew = false;
            IsDirty = false;
            SourceId = null;
            SourceIndex = -1;
        }

        private bool IsEditable(int stringIndex)
        {
            return IsActive && Frets != null && stringIndex >= 0 && stringIndex < Frets.Length;
        }
    }
}
