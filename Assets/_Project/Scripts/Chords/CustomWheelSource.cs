namespace Chordyceps.Chords
{
    /// <summary>
    /// A player-authored layout as a wheel source: every wheel is numbered and holds the
    /// chords the Wheel Maker assigned to it. Wheels carry no roman numerals — the player
    /// picks the chords, so no scale degree is implied.
    /// </summary>
    public class CustomWheelSource : IWheelSource
    {
        private readonly WheelLayoutEntry _layout;

        public CustomWheelSource(WheelLayoutEntry layout)
        {
            _layout = layout;
        }

        public bool IsCustom => true;

        public int WheelCount => _layout?.Wheels != null ? _layout.Wheels.Count : 0;

        public string Header(int wheel)
        {
            return (wheel + 1).ToString();
        }

        public string Roman(int wheel, int slot)
        {
            return null;
        }

        public string ChordId(int wheel, int slot)
        {
            return Wheel(wheel)?.GetSector(slot);
        }

        public int TonicSlot(int wheel)
        {
            return 0;
        }

        private WheelEntry Wheel(int wheel)
        {
            return _layout?.Wheels != null && wheel >= 0 && wheel < _layout.Wheels.Count ? _layout.Wheels[wheel] : null;
        }
    }
}
