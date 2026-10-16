using UnityEngine;
using UnityEngine.InputSystem;
using Chordyceps.Gameplay;
using Chordyceps.Chords;

namespace Chordyceps.Input
{
    public class GuitarKeyboardInput : MonoBehaviour
    {
        [SerializeField] private GuitarController _targetController;
        [SerializeField] private ChordManager _chordManager;

        private const float DEFAULT_KEYBOARD_VELOCITY = 1.0f;

        private void Awake()
        {
            FindControllerIfMissing();
            FindChordManagerIfMissing();
        }

        private void FindControllerIfMissing()
        {
            if (_targetController == null)
            {
                _targetController = FindFirstObjectByType<GuitarController>();
            }
        }

        private void FindChordManagerIfMissing()
        {
            if (_chordManager == null)
            {
                _chordManager = FindFirstObjectByType<ChordManager>();
            }
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            ProcessKeyboardPlucks(keyboard);
            ProcessKeyboardChords(keyboard);
            ProcessKeyboardCapo(keyboard);
        }

        private void ProcessKeyboardPlucks(Keyboard keyboard)
        {
            if (_targetController == null)
            {
                return;
            }

            CheckNumericKeys(keyboard);
        }

        private void CheckNumericKeys(Keyboard keyboard)
        {
            if (keyboard.digit1Key.wasPressedThisFrame) SendPluck(0);
            else if (keyboard.digit2Key.wasPressedThisFrame) SendPluck(1);
            else if (keyboard.digit3Key.wasPressedThisFrame) SendPluck(2);
            else if (keyboard.digit4Key.wasPressedThisFrame) SendPluck(3);
            else if (keyboard.digit5Key.wasPressedThisFrame) SendPluck(4);
            else if (keyboard.digit6Key.wasPressedThisFrame) SendPluck(5);
        }

        private void SendPluck(int stringIndex)
        {
            var data = new StringInputData(stringIndex, StringInputType.Tap, DEFAULT_KEYBOARD_VELOCITY, Vector2.zero);
            _targetController.OnStringInteracted(data);
        }

        private void ProcessKeyboardChords(Keyboard keyboard)
        {
            if (_chordManager == null)
            {
                return;
            }

            CheckMajorChordKeys(keyboard);
            CheckMinorChordKeys(keyboard);
            CheckKeyNavKeys(keyboard);
        }

        private void CheckMajorChordKeys(Keyboard keyboard)
        {
            if (keyboard.qKey.wasPressedThisFrame) _chordManager.SelectChord(0);
            else if (keyboard.wKey.wasPressedThisFrame) _chordManager.SelectChord(1);
            else if (keyboard.eKey.wasPressedThisFrame) _chordManager.SelectChord(2);
        }

        private void CheckMinorChordKeys(Keyboard keyboard)
        {
            if (keyboard.aKey.wasPressedThisFrame) _chordManager.SelectChord(3);
            else if (keyboard.sKey.wasPressedThisFrame) _chordManager.SelectChord(4);
            else if (keyboard.dKey.wasPressedThisFrame) _chordManager.SelectChord(5);
        }

        private void CheckKeyNavKeys(Keyboard keyboard)
        {
            if (keyboard.leftBracketKey.wasPressedThisFrame) _chordManager.DecrementKey();
            else if (keyboard.rightBracketKey.wasPressedThisFrame) _chordManager.IncrementKey();
        }

        private void ProcessKeyboardCapo(Keyboard keyboard)
        {
            if (_chordManager == null)
            {
                return;
            }

            if (keyboard.equalsKey.wasPressedThisFrame || keyboard.numpadPlusKey.wasPressedThisFrame)
            {
                _chordManager.IncrementCapo();
            }
            else if (keyboard.minusKey.wasPressedThisFrame || keyboard.numpadMinusKey.wasPressedThisFrame)
            {
                _chordManager.DecrementCapo();
            }
        }
    }
}
