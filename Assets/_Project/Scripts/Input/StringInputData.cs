using UnityEngine;

namespace Chordyceps.Input
{
    public readonly struct StringInputData
    {
        public int StringIndex { get; }
        public StringInputType InputType { get; }
        public float Velocity { get; }
        public Vector2 WorldPosition { get; }

        public StringInputData(int stringIndex, StringInputType inputType, float velocity, Vector2 worldPosition)
        {
            StringIndex = stringIndex;
            InputType = inputType;
            Velocity = velocity;
            WorldPosition = worldPosition;
        }
    }
}
