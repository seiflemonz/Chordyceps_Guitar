using System;
using UnityEngine.UIElements;

namespace Chordyceps.UI
{
    /// <summary>
    /// Reusable modal for confirmations and short prompts. It drives the markup that already
    /// ships inside the owning UIDocument (see the modal-layer block in ChordMaker.uxml),
    /// so no extra UIDocument is needed. Blocks input to everything behind it while open.
    /// </summary>
    public class ModalDialog
    {
        private const string MESSAGE_NAME = "modal-message";
        private const string ERROR_NAME = "modal-error";
        private const string NAME_FIELD_NAME = "modal-name-field";
        private const string BUTTONS_NAME = "modal-buttons";

        /// <summary>One button on the modal. The dialog closes before the action runs.</summary>
        public struct Option
        {
            public string Label;
            public Action OnClick;
            public bool IsPrimary;
            public bool IsDanger;

            public Option(string label, Action onClick, bool isPrimary = false, bool isDanger = false)
            {
                Label = label;
                OnClick = onClick;
                IsPrimary = isPrimary;
                IsDanger = isDanger;
            }
        }

        private readonly VisualElement _layer;
        private readonly Label _message;
        private readonly Label _error;
        private readonly TextField _nameField;
        private readonly VisualElement _buttons;

        public bool IsOpen { get; private set; }

        public ModalDialog(VisualElement layer)
        {
            _layer = layer;
            _message = layer?.Q<Label>(MESSAGE_NAME);
            _error = layer?.Q<Label>(ERROR_NAME);
            _nameField = layer?.Q<TextField>(NAME_FIELD_NAME);
            _buttons = layer?.Q(BUTTONS_NAME);

            Hide();
        }

        /// <summary>Shows a message with one or more choices.</summary>
        public void ShowMessage(string message, params Option[] options)
        {
            if (_layer == null)
            {
                return;
            }

            _message.text = message;
            HideNameField();
            ClearError();

            _buttons.Clear();
            if (options != null)
            {
                foreach (Option option in options)
                {
                    Option captured = option;
                    AddButton(captured.Label, captured.IsPrimary, captured.IsDanger, () =>
                    {
                        Hide();
                        captured.OnClick?.Invoke();
                    });
                }
            }

            SetOpen(true);
        }

        /// <summary>
        /// Shows a single-line name prompt. <paramref name="validate"/> returns an error message
        /// to display instead of confirming, or null when the value is acceptable.
        /// </summary>
        public void ShowNameInput(
            string message,
            string initialName,
            Func<string, string> validate,
            Action<string> onConfirm,
            Action onCancel)
        {
            if (_layer == null)
            {
                return;
            }

            _message.text = message;
            ClearError();
            _nameField.style.display = DisplayStyle.Flex;
            _nameField.SetValueWithoutNotify(initialName ?? string.Empty);

            _buttons.Clear();
            AddButton("Cancel", false, false, () =>
            {
                Hide();
                onCancel?.Invoke();
            });
            AddButton("Save", true, false, () =>
            {
                string value = (_nameField.value ?? string.Empty).Trim();
                string error = validate != null ? validate(value) : null;
                if (!string.IsNullOrEmpty(error))
                {
                    ShowError(error);
                    return;
                }

                Hide();
                onConfirm?.Invoke(value);
            });

            SetOpen(true);
            _nameField.schedule.Execute(() => _nameField.Focus()).StartingIn(1);
        }

        public void Hide()
        {
            _buttons?.Clear();
            HideNameField();
            ClearError();
            SetOpen(false);
        }

        private void HideNameField()
        {
            if (_nameField == null)
            {
                return;
            }

            _nameField.style.display = DisplayStyle.None;
            _nameField.SetValueWithoutNotify(string.Empty);
        }

        private void ClearError()
        {
            if (_error == null)
            {
                return;
            }

            _error.text = string.Empty;
            _error.style.display = DisplayStyle.None;
        }

        private void ShowError(string message)
        {
            if (_error == null)
            {
                return;
            }

            _error.text = message;
            _error.style.display = DisplayStyle.Flex;
        }

        private void AddButton(string label, bool isPrimary, bool isDanger, Action onClick)
        {
            var button = new Button(() => onClick?.Invoke()) { text = label };
            button.focusable = false;
            button.AddToClassList("modal-btn");

            if (isPrimary)
            {
                button.AddToClassList("modal-btn--primary");
            }

            if (isDanger)
            {
                button.AddToClassList("modal-btn--danger");
            }

            _buttons.Add(button);
        }

        private void SetOpen(bool open)
        {
            IsOpen = open;
            if (_layer != null)
            {
                _layer.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}
