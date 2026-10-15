using System;
using UnityEngine;
using Chordyceps.Input;

namespace Chordyceps.Gameplay
{
    public class GuitarController : MonoBehaviour, IStringInputListener
    {
        [SerializeField] private GuitarString[] _strings = new GuitarString[STRING_COUNT];

        public const int STRING_COUNT = 6;

        public event Action<StringInputData> OnStringPlucked;

        private void Awake()
        {
            ValidateStringReferences();
        }

        private void ValidateStringReferences()
        {
            if (_strings == null || _strings.Length != STRING_COUNT)
            {
                Debug.LogWarning("GuitarController: Strings array is not properly configured.");
            }
        }

        public void OnStringInteracted(StringInputData data)
        {
            if (!IsValidStringIndex(data.StringIndex))
            {
                return;
            }

            TriggerStringFeedback(data.StringIndex);
            NotifyPluckListeners(data);
        }

        private bool IsValidStringIndex(int index)
        {
            return index >= 0 && index < STRING_COUNT;
        }

        private void TriggerStringFeedback(int index)
        {
            if (_strings[index] != null)
            {
                _strings[index].PlayPluckFeedback();
            }
        }

        private void NotifyPluckListeners(StringInputData data)
        {
            Debug.Log($"String {data.StringIndex} [{data.InputType}] Velocity: {data.Velocity:F2}");
            OnStringPlucked?.Invoke(data);
        }
    }
}
