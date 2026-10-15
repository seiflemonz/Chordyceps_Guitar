using UnityEngine;
using UnityEngine.InputSystem;
using Chordyceps.Gameplay;

namespace Chordyceps.Input
{
    public class GuitarKeyboardInput : MonoBehaviour
    {
        [SerializeField] private GuitarController _targetController;

        private const float DEFAULT_KEYBOARD_VELOCITY = 1.0f;

        private void Awake()
        {
            FindControllerIfMissing();
        }

        private void FindControllerIfMissing()
        {
            if (_targetController == null)
            {
                _targetController = FindFirstObjectByType<GuitarController>();
            }
        }

        private void Update()
        {
            ProcessKeyboardPlucks();
        }

        private void ProcessKeyboardPlucks()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || _targetController == null)
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
    }
}
