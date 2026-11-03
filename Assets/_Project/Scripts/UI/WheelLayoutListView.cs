using System;
using System.Collections.Generic;
using Chordyceps.Chords;
using UnityEngine.UIElements;

namespace Chordyceps.UI
{
    /// <summary>
    /// The left column of layouts. Renders the catalog as a scrollable list and reports
    /// taps; it holds no layout data and makes no decisions about selection. The
    /// "Create Layout" button below it is owned by the controller, not built here, so it
    /// stays put while the list scrolls.
    /// </summary>
    public class WheelLayoutListView
    {
        private const string ROW_CLASS = "wheel-maker-layout-row";
        private const string ROW_ACTIVE_CLASS = "wheel-maker-layout-row--active";
        private const string ROW_CUSTOM_CLASS = "wheel-maker-layout-row--custom";

        private readonly ScrollView _scroll;
        private readonly List<Button> _rows = new List<Button>();

        public WheelLayoutListView(ScrollView scroll)
        {
            _scroll = scroll;
        }

        /// <summary>Raised with the index of the tapped layout.</summary>
        public event Action<int> LayoutSelected;

        /// <summary>Replaces every row with one per catalog entry.</summary>
        public void Build(IReadOnlyList<WheelLayoutEntry> layouts)
        {
            _rows.Clear();
            _scroll?.Clear();
            if (_scroll == null || layouts == null)
            {
                return;
            }

            for (int i = 0; i < layouts.Count; i++)
            {
                _rows.Add(CreateRow(layouts[i], i));
            }
        }

        /// <summary>Marks one row active; pass -1 to clear the selection.</summary>
        public void SetActiveIndex(int index)
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                _rows[i].EnableInClassList(ROW_ACTIVE_CLASS, i == index);
            }
        }

        private Button CreateRow(WheelLayoutEntry layout, int index)
        {
            var row = new Button(() => LayoutSelected?.Invoke(index)) { text = layout.Name };
            row.focusable = false;
            row.AddToClassList(ROW_CLASS);

            if (layout.IsCustom)
            {
                row.AddToClassList(ROW_CUSTOM_CLASS);
            }

            _scroll.Add(row);
            return row;
        }
    }
}
