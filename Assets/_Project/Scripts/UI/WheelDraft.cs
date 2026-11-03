using System.Collections.Generic;
using Chordyceps.Chords;

namespace Chordyceps.UI
{
    /// <summary>
    /// The working copy of one wheel layout while the Wheel Maker has it open: its wheels,
    /// which wheel is active, and whether anything has changed since it was loaded.
    ///
    /// Pure state — it neither renders nor stores anything. The built-in layout is opened
    /// as a new layout, so editing it always produces a separate named layout rather than
    /// changing what the main game ships with.
    /// </summary>
    public class WheelDraft
    {
        private readonly List<WheelEntry> _wheels = new List<WheelEntry>();

        /// <summary>Id of the stored layout this was loaded from; null while unnamed.</summary>
        public string SourceId { get; private set; }

        /// <summary>Catalog position the draft was opened from; -1 when it came from nowhere.</summary>
        public int SourceIndex { get; private set; }

        public string Name { get; private set; }

        /// <summary>True when this layout does not exist in the store yet.</summary>
        public bool IsNew { get; private set; }

        public bool IsActive { get; private set; }

        public bool IsDirty { get; private set; }

        public int ActiveWheelIndex { get; private set; }

        public int WheelCount => _wheels.Count;

        public IReadOnlyList<WheelEntry> Wheels => _wheels;

        public bool CanAddWheel => _wheels.Count < WheelLayoutEntry.MAX_WHEELS;

        public WheelEntry GetWheel(int index)
        {
            return index >= 0 && index < _wheels.Count ? _wheels[index] : null;
        }

        public string GetSector(int wheelIndex, int slot)
        {
            return GetWheel(wheelIndex)?.GetSector(slot);
        }

        /// <summary>Button label for a wheel: its authored label, or its position when unnamed.</summary>
        public string GetWheelLabel(int wheelIndex)
        {
            WheelEntry wheel = GetWheel(wheelIndex);
            if (wheel != null && !string.IsNullOrEmpty(wheel.Label))
            {
                return wheel.Label;
            }

            return (wheelIndex + 1).ToString();
        }

        /// <summary>
        /// Loads a copy of an existing layout. Pass true to open it as a new layout, which
        /// is how the built-in layout is opened so it cannot be edited in place.
        /// </summary>
        public void BeginFrom(WheelLayoutEntry layout, int sourceIndex, bool asNew)
        {
            CopyWheels(layout?.Wheels);

            SourceId = asNew ? null : layout?.Id;
            SourceIndex = sourceIndex;
            Name = layout?.Name;
            IsNew = asNew;
            IsDirty = false;
            IsActive = true;
            ActiveWheelIndex = 0;
        }

        /// <summary>Starts an empty layout holding a single blank wheel.</summary>
        public void BeginNew(int sourceIndex)
        {
            _wheels.Clear();
            _wheels.Add(new WheelEntry());

            SourceId = null;
            SourceIndex = sourceIndex;
            Name = null;
            IsNew = true;
            IsDirty = false;
            IsActive = true;
            ActiveWheelIndex = 0;
        }

        public bool AddWheel()
        {
            if (!IsActive || !CanAddWheel)
            {
                return false;
            }

            _wheels.Add(new WheelEntry());
            ActiveWheelIndex = _wheels.Count - 1;
            IsDirty = true;
            return true;
        }

        public bool SelectWheel(int index)
        {
            if (!IsActive || index < 0 || index >= _wheels.Count || index == ActiveWheelIndex)
            {
                return false;
            }

            ActiveWheelIndex = index;
            return true;
        }

        /// <summary>Assigns a chord to the active wheel's sector. Pass null to clear it.</summary>
        public bool SetSector(int slot, string chordId)
        {
            WheelEntry wheel = GetWheel(ActiveWheelIndex);
            if (!IsActive || wheel == null || wheel.GetSector(slot) == chordId)
            {
                return false;
            }

            wheel.SetSector(slot, chordId);
            IsDirty = true;
            return true;
        }

        /// <summary>Snapshot of the working copy, ready to be handed to the store.</summary>
        public WheelLayoutEntry ToEntry()
        {
            var entry = new WheelLayoutEntry { Id = SourceId, Name = Name, IsCustom = true };
            for (int i = 0; i < _wheels.Count; i++)
            {
                entry.Wheels.Add(_wheels[i].Clone());
            }

            return entry;
        }

        public void End()
        {
            _wheels.Clear();
            SourceId = null;
            SourceIndex = -1;
            Name = null;
            IsNew = false;
            IsDirty = false;
            IsActive = false;
            ActiveWheelIndex = 0;
        }

        private void CopyWheels(IReadOnlyList<WheelEntry> source)
        {
            _wheels.Clear();
            if (source != null)
            {
                for (int i = 0; i < source.Count && _wheels.Count < WheelLayoutEntry.MAX_WHEELS; i++)
                {
                    if (source[i] != null)
                    {
                        _wheels.Add(source[i].Clone());
                    }
                }
            }

            if (_wheels.Count == 0)
            {
                _wheels.Add(new WheelEntry());
            }
        }
    }
}
