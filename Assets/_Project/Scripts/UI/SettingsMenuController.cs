using UnityEngine;
using UnityEngine.UIElements;

namespace Chordyceps.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class SettingsMenuController : MonoBehaviour
    {
        private const string PANEL_NAME  = "settings-panel";
        private const string CLOSE_BTN   = "settings-close-btn";
        private const string OPEN_CLASS  = "settings-panel--open";

        private UIDocument    _uiDocument;
        private VisualElement _panel;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            BindElements();
        }

        private void BindElements()
        {
            VisualElement root = _uiDocument.rootVisualElement;
            _panel = root.Q<VisualElement>(PANEL_NAME);
            root.Q<Button>(CLOSE_BTN)?.RegisterCallback<ClickEvent>(_ => Close());
        }

        public void Open()
        {
            _panel.AddToClassList(OPEN_CLASS);
            _panel.pickingMode = PickingMode.Position;
        }

        public void Close()
        {
            _panel.RemoveFromClassList(OPEN_CLASS);
            _panel.pickingMode = PickingMode.Ignore;
        }
    }
}
