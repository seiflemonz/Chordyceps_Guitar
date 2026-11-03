using System;
using System.Collections.Generic;
using Chordyceps.Chords;

namespace Chordyceps.UI
{
    /// <summary>
    /// The chord maker's browsable list: every distinct definition asset across all keys
    /// and scales, followed by the player-created chords. Read-only view of the data —
    /// writes go through <see cref="CustomChordStore"/> and are followed by a rebuild.
    /// </summary>
    public class ChordCatalog
    {
        private readonly List<ChordListItem> _items = new List<ChordListItem>();

        public IReadOnlyList<ChordListItem> Items => _items;

        public int Count => _items.Count;

        /// <summary>Re-reads the definition assets and stored chords from scratch.</summary>
        public void Rebuild(ChordManager chordManager)
        {
            _items.Clear();
            if (chordManager == null)
            {
                return;
            }

            AppendDefinitions(chordManager);
            AppendCustoms();
        }

        public ChordListItem Get(int index)
        {
            return index >= 0 && index < _items.Count ? _items[index] : null;
        }

        public string IdAt(int index)
        {
            return Get(index)?.Id;
        }

        public int IndexOf(string id)
        {
            return id == null ? -1 : _items.FindIndex(item => item.Id == id);
        }

        /// <summary>Chord names must be unique across every listed chord.</summary>
        public bool IsNameTaken(string name, string ignoreId)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                ChordListItem item = _items[i];
                if (item.Id == ignoreId)
                {
                    continue;
                }

                if (string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void AppendDefinitions(ChordManager chordManager)
        {
            var seenNames = new HashSet<string>();
            int keyCount = chordManager.TotalKeys;
            for (int key = 0; key < keyCount; key++)
            {
                ScaleDefinition scale = chordManager.GetScale(key);
                if (scale == null)
                {
                    continue;
                }

                AppendScale(scale, seenNames);
            }
        }

        private void AppendScale(ScaleDefinition scale, HashSet<string> seenNames)
        {
            for (int chordIndex = 0; chordIndex < ScaleDefinition.DIATONIC_CHORD_COUNT; chordIndex++)
            {
                ChordDefinition chord = scale.GetChord(chordIndex);
                if (chord == null || !seenNames.Add(chord.ChordName))
                {
                    continue;
                }

                _items.Add(ChordListItem.FromDefinition(chord));
            }
        }

        private void AppendCustoms()
        {
            IReadOnlyList<ChordEntry> customs = CustomChordStore.Customs;
            for (int i = 0; i < customs.Count; i++)
            {
                _items.Add(ChordListItem.FromCustom(customs[i]));
            }
        }
    }
}
