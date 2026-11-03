using System;
using System.Collections.Generic;
using Chordyceps.Chords;

namespace Chordyceps.UI
{
    /// <summary>
    /// The wheel maker's layout list: the built-in layout, derived from the scale assets so
    /// it always matches what the main game shows, followed by the player's saved layouts.
    /// Read-only view of the data — writes go through <see cref="CustomWheelStore"/> and are
    /// followed by a rebuild.
    /// </summary>
    public class WheelCatalog
    {
        /// <summary>Id of the built-in layout, which is never stored.</summary>
        public const string DEFAULT_ID = "__default__";

        public const string DefaultLayoutName = "Circle of Fifths (Default)";

        private readonly List<WheelLayoutEntry> _items = new List<WheelLayoutEntry>();

        public IReadOnlyList<WheelLayoutEntry> Items => _items;

        public int Count => _items.Count;

        /// <summary>Re-reads the scale assets and stored layouts from scratch.</summary>
        public void Rebuild(ChordManager chordManager)
        {
            _items.Clear();
            _items.Add(BuildDefault(chordManager));

            IReadOnlyList<WheelLayoutEntry> customs = CustomWheelStore.Customs;
            for (int i = 0; i < customs.Count; i++)
            {
                _items.Add(customs[i]);
            }
        }

        public WheelLayoutEntry Get(int index)
        {
            return index >= 0 && index < _items.Count ? _items[index] : null;
        }

        public int IndexOf(string id)
        {
            return id == null ? -1 : _items.FindIndex(item => item.Id == id);
        }

        public bool IsDefault(int index)
        {
            return Get(index)?.Id == DEFAULT_ID;
        }

        /// <summary>Layout names must be unique; the built-in layout's name is reserved too.</summary>
        public bool IsNameTaken(string name, string ignoreId)
        {
            string trimmed = (name ?? string.Empty).Trim();
            if (trimmed.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < _items.Count; i++)
            {
                WheelLayoutEntry item = _items[i];
                if (item.Id == ignoreId)
                {
                    continue;
                }

                if (string.Equals(item.Name, trimmed, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>One wheel per key, labelled with the key and holding that scale's six chords.</summary>
        private static WheelLayoutEntry BuildDefault(ChordManager chordManager)
        {
            var layout = new WheelLayoutEntry { Id = DEFAULT_ID, Name = DefaultLayoutName, IsCustom = false };
            if (chordManager == null)
            {
                return layout;
            }

            int keyCount = chordManager.TotalKeys;
            for (int key = 0; key < keyCount; key++)
            {
                ScaleDefinition scale = chordManager.GetScale(key);
                if (scale == null)
                {
                    continue;
                }

                layout.Wheels.Add(BuildScaleWheel(scale));
            }

            return layout;
        }

        private static WheelEntry BuildScaleWheel(ScaleDefinition scale)
        {
            var wheel = new WheelEntry { Label = scale.ScaleName };
            for (int slot = 0; slot < WheelEntry.SECTOR_COUNT; slot++)
            {
                ChordDefinition chord = scale.GetChord(slot);
                if (chord != null)
                {
                    wheel.SetSector(slot, chord.ChordName);
                }
            }

            return wheel;
        }
    }
}
