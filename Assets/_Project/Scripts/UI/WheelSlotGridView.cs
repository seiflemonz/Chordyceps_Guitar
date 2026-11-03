using System;
using UnityEngine.UIElements;

namespace Chordyceps.UI
{
    /// <summary>
    /// The 6x2 grid of wheel buttons above the large wheel, plus the "+" that adds the
    /// next wheel. Cells past the last wheel are inert, and the "+" disappears once the
    /// layout holds <see cref="Chordyceps.Chords.WheelLayoutEntry.MAX_WHEELS"/> wheels.
    ///
    /// Tapping a wheel button makes it the active one; it does not edit anything.
    /// </summary>
    public class WheelSlotGridView
    {
        private const int CELL_COUNT = 12;

        private const string CELL_CLASS = "wheel-maker-slot";
        private const string CELL_ACTIVE_CLASS = "wheel-maker-slot--active";
        private const string CELL_EMPTY_CLASS = "wheel-maker-slot--empty";
        private const string CELL_ADD_CLASS = "wheel-maker-slot--add";

        private readonly VisualElement _grid;
        private readonly Button[] _wheelButtons = new Button[CELL_COUNT];

        public WheelSlotGridView(VisualElement grid)
        {
            _grid = grid;
        }

        /// <summary>Raised with the index of the tapped wheel.</summary>
        public event Action<int> WheelSelected;

        /// <summary>Raised when the "+" cell is tapped.</summary>
        public event Action AddRequested;

        /// <summary>Rebuilds every cell from the draft's wheels.</summary>
        public void Build(WheelDraft draft)
        {
            _grid?.Clear();
            if (_grid == null)
            {
                return;
            }

            int wheelCount = draft != null ? draft.WheelCount : 0;
            for (int cell = 0; cell < CELL_COUNT; cell++)
            {
                _wheelButtons[cell] = null;

                if (cell < wheelCount)
                {
                    _wheelButtons[cell] = CreateWheelButton(draft, cell);
                    continue;
                }

                if (cell == wheelCount && draft != null && draft.CanAddWheel)
                {
                    _grid.Add(CreateAddButton());
                    continue;
                }

                _grid.Add(CreateEmptyCell());
            }
        }

        /// <summary>Marks one wheel button active; cells without a wheel are unaffected.</summary>
        public void SetActiveIndex(int index)
        {
            for (int cell = 0; cell < CELL_COUNT; cell++)
            {
                _wheelButtons[cell]?.EnableInClassList(CELL_ACTIVE_CLASS, cell == index);
            }
        }

        private Button CreateWheelButton(WheelDraft draft, int index)
        {
            var button = new Button(() => WheelSelected?.Invoke(index)) { text = draft.GetWheelLabel(index) };
            button.focusable = false;
            button.AddToClassList(CELL_CLASS);
            _grid.Add(button);
            return button;
        }

        private Button CreateAddButton()
        {
            var button = new Button(() => AddRequested?.Invoke()) { text = "+" };
            button.focusable = false;
            button.AddToClassList(CELL_CLASS);
            button.AddToClassList(CELL_ADD_CLASS);
            return button;
        }

        private VisualElement CreateEmptyCell()
        {
            var cell = new VisualElement { pickingMode = PickingMode.Ignore };
            cell.AddToClassList(CELL_CLASS);
            cell.AddToClassList(CELL_EMPTY_CLASS);
            return cell;
        }
    }
}
