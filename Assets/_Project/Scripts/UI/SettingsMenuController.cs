using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine.InputSystem;
#endif

namespace Chordyceps.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class SettingsMenuController : MonoBehaviour
    {
        private const string PANEL_NAME = "settings-panel";
        private const string SCROLL_NAME = "settings-scroll";
        private const string CLOSE_BTN = "settings-close-btn";
        private const string OPEN_CLASS = "settings-panel--open";
        private const float SCROLL_SPEED = 0.5f;

        private UIDocument _uiDocument;
        private VisualElement _panel;
        private ScrollView _scrollView;
        private bool _isOpen;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            BindElements();
        }

        private void BindElements()
        {
            VisualElement root = _uiDocument.rootVisualElement;
            _panel = root.Q<VisualElement>(PANEL_NAME);
            _scrollView = root.Q<ScrollView>(SCROLL_NAME);
            root.Q<Button>(CLOSE_BTN)?.RegisterCallback<ClickEvent>(_ => Close());
        }

        private void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            HandleMouseScroll();
#endif
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        private void HandleMouseScroll()
        {
            if (!_isOpen || _scrollView == null) return;

            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            float scrollY = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scrollY) > Mathf.Epsilon)
            {
                _scrollView.verticalScroller.value -= scrollY * SCROLL_SPEED;
            }
        }
#endif

        public void Open()
        {
            _isOpen = true;
            _panel.AddToClassList(OPEN_CLASS);
            _panel.pickingMode = PickingMode.Position;
        }

        public void Close()
        {
            _isOpen = false;
            _panel.RemoveFromClassList(OPEN_CLASS);
            _panel.pickingMode = PickingMode.Ignore;
        }
    }
}

