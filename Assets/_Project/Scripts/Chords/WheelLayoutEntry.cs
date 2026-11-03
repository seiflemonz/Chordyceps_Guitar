using System;
using System.Collections.Generic;

namespace Chordyceps.Chords
{
    /// <summary>
    /// A named set of wheels the player can step through on the main-game chord wheel.
    /// The built-in layout is derived from the scale assets at runtime; layouts the player
    /// authored are stored by <see cref="CustomWheelStore"/>.
    /// </summary>
    [Serializable]
    public class WheelLayoutEntry
    {
        public const int MAX_WHEELS = 12;

        /// <summary>Stable identity. Generated for player layouts, fixed for the built-in one.</summary>
        public string Id;

        public string Name;

        /// <summary>True for player-authored layouts, false for the built-in layout.</summary>
        public bool IsCustom;

        public List<WheelEntry> Wheels = new List<WheelEntry>();

        public WheelLayoutEntry Clone()
        {
            var copy = new WheelLayoutEntry { Id = Id, Name = Name, IsCustom = IsCustom };
            if (Wheels != null)
            {
                for (int i = 0; i < Wheels.Count; i++)
                {
                    if (Wheels[i] != null)
                    {
                        copy.Wheels.Add(Wheels[i].Clone());
                    }
                }
            }

            return copy;
        }
    }
}
