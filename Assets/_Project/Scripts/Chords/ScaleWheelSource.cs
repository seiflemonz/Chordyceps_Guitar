namespace Chordyceps.Chords
{
    /// <summary>
    /// The built-in wheel source: one wheel per scale asset, each holding that scale's six
    /// diatonic chords. This is what the game shows before any layout is applied, and what
    /// it returns to when the built-in layout is applied again.
    /// </summary>
    public class ScaleWheelSource : IWheelSource
    {
        private readonly ScaleDefinition[] _scales;

        public ScaleWheelSource(ScaleDefinition[] scales)
        {
            _scales = scales;
        }

        public bool IsCustom => false;

        public int WheelCount => _scales != null ? _scales.Length : 0;

        public string Header(int wheel)
        {
            return Scale(wheel)?.ScaleName;
        }

        public string Roman(int wheel, int slot)
        {
            return Scale(wheel)?.GetRomanNumeral(slot);
        }

        public string ChordId(int wheel, int slot)
        {
            return Scale(wheel)?.GetChord(slot)?.ChordName;
        }

        public int TonicSlot(int wheel)
        {
            return Scale(wheel)?.TonicChordIndex ?? 0;
        }

        private ScaleDefinition Scale(int wheel)
        {
            return _scales != null && wheel >= 0 && wheel < _scales.Length ? _scales[wheel] : null;
        }
    }
}
