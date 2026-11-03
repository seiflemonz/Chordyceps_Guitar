using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Chordyceps.UI
{
    /// <summary>
    /// The chord library that slides in from the right when a wheel sector is tapped.
    /// Lists the same chords as the Chord Maker — every definition plus any player-created
    /// chord — and reports the chosen chord's id. It owns no chord data.
    /// </summary>
    public class ChordLibraryPanel
    {
        private const string OPEN_CLASS = "wheel-maker-library--open";
        private const string ROW_CLASS = "wheel-maker-chord-row";
        private const string ROW_ACTIVE_CLASS = "wheel-maker-chord-row--active";
        private const string ROW_CUSTOM_CLASS = "wheel-maker-chord-row--custom";

        private const string PANEL_NAME = "wheel-maker-library";
        private const string TITLE_NAME = "wheel-maker-library-title";
        private const string SCROLL_NAME = "wheel-maker-library-scroll";

        private readonly VisualElement _panel;
        private readonly Label _title;
        private readonly ScrollView _scroll;
        private readonly List<Button> _rows = new List<Button>();

        public ChordLibraryPanel(VisualElement root)
        {
            _panel = root?.Q(PANEL_NAME);
            _title = root?.Q<Label>(TITLE_NAME);
            _scroll = root?.Q<ScrollView>(SCROLL_NAME);
        }

        /// <summary>Raised with the id of the chosen chord.</summary>
        public event Action<string> ChordChosen;

        public bool IsOpen { get; private set; }

        /// <summary>Replaces every row with one per catalog entry.</summary>
        public void Build(IReadOnlyList<ChordListItem> items)
        {
            _rows.Clear();
            _scroll?.Clear();
            if (_scroll == null || items == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                _rows.Add(CreateRow(items[i]));
            }
        }

        /// <summary>Opens the library for one sector, highlighting the chord already on it.</summary>
        public void Open(int slot, string currentChordId)
        {
            if (_panel == null)
            {
                return;
            }

            if (_title != null)
            {
                _title.text = $"SECTOR {slot + 1}";
            }

            SetActiveChord(currentChordId);
            IsOpen = true;
            _panel.AddToClassList(OPEN_CLASS);
        }

        public void Close()
        {
            IsOpen = false;
            _panel?.RemoveFromClassList(OPEN_CLASS);
        }

        private void SetActiveChord(string chordId)
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                _rows[i].EnableInClassList(ROW_ACTIVE_CLASS, _rows[i].userData as string == chordId);
            }
        }

        private Button CreateRow(ChordListItem item)
        {
            var row = new Button(() => ChordChosen?.Invoke(item.Id))
            {
                text = item.Name,
                userData = item.Id,
            };

            row.focusable = false;
            row.AddToClassList(ROW_CLASS);
            if (item.IsCustom)
            {
                row.AddToClassList(ROW_CUSTOM_CLASS);
            }

            _scroll.Add(row);
            return row;
        }
    }
}
