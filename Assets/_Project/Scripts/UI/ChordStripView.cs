using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Chordyceps.UI
{
    /// <summary>
    /// The horizontal strip of chord-name chips. Renders the catalog and reports taps;
    /// it holds no chord data and makes no decisions about selection.
    /// </summary>
    public class ChordStripView
    {
        private const string CHIP_CLASS = "chord-maker-chip";
        private const string CHIP_ACTIVE_CLASS = "chord-maker-chip--active";
        private const string CHIP_CUSTOM_CLASS = "chord-maker-chip--custom";
        private const string CHIP_MODIFIED_CLASS = "chord-maker-chip--modified";

        private readonly ScrollView _strip;
        private readonly List<Button> _chips = new List<Button>();

        public ChordStripView(ScrollView strip)
        {
            _strip = strip;
        }

        /// <summary>Raised with the index of the tapped chip.</summary>
        public event Action<int> ItemSelected;

        /// <summary>Replaces every chip with one per catalog row.</summary>
        public void Build(IReadOnlyList<ChordListItem> items)
        {
            _chips.Clear();
            _strip?.Clear();
            if (_strip == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                _chips.Add(CreateChip(items[i], i));
            }
        }

        /// <summary>Marks one chip active; pass -1 to clear the selection.</summary>
        public void SetActiveIndex(int index)
        {
            for (int i = 0; i < _chips.Count; i++)
            {
                _chips[i].EnableInClassList(CHIP_ACTIVE_CLASS, i == index);
            }
        }

        private Button CreateChip(ChordListItem item, int index)
        {
            var chip = new Button(() => ItemSelected?.Invoke(index)) { text = item.Name };
            chip.focusable = false;
            chip.AddToClassList(CHIP_CLASS);

            if (item.IsCustom)
            {
                chip.AddToClassList(CHIP_CUSTOM_CLASS);
            }
            else if (item.IsModified)
            {
                chip.AddToClassList(CHIP_MODIFIED_CLASS);
            }

            _strip.Add(chip);
            return chip;
        }
    }
}
