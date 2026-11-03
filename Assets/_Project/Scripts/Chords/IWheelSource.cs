namespace Chordyceps.Chords
{
    /// <summary>
    /// Where the main game's chord wheel gets the six chords it shows on each wheel.
    /// <see cref="ScaleWheelSource"/> reads the scale assets — the shipped behaviour —
    /// while <see cref="CustomWheelSource"/> reads a layout the player applied in the
    /// Wheel Maker. Hiding the difference behind this interface lets
    /// <see cref="ChordManager"/> switch between them without knowing either.
    /// </summary>
    public interface IWheelSource
    {
        /// <summary>True when the wheels come from a player-authored layout.</summary>
        bool IsCustom { get; }

        /// <summary>How many wheels the player can step through.</summary>
        int WheelCount { get; }

        /// <summary>Text drawn above the wheel, e.g. the scale name or the wheel number.</summary>
        string Header(int wheel);

        /// <summary>Roman numeral above a slot, or null when the source has none.</summary>
        string Roman(int wheel, int slot);

        /// <summary>Chord id for a slot — a definition's name or a custom chord's id.</summary>
        string ChordId(int wheel, int slot);

        /// <summary>Slot selected when a wheel becomes the current one.</summary>
        int TonicSlot(int wheel);
    }
}
