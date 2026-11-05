using UnityEngine;
using UnityEngine.UIElements;
using Chordyceps.Gameplay;
#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine.InputSystem;
#endif

namespace Chordyceps.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class SettingsMenuController : MonoBehaviour
    {
        private const string ROOT_NAME = "settings-root";
        private const string PANEL_NAME = "settings-panel";
        private const string SCROLL_NAME = "settings-scroll";
        private const string CLOSE_BTN = "settings-close-btn";
        private const string SCRIM_NAME = "settings-scrim";
        private const string CHORD_MAKER_BTN = "settings-item-chord-maker";
        private const string WHEEL_MAKER_BTN = "settings-item-wheel-maker";
        private const string GUITAR_MODE_BTN = "settings-item-guitar-mode";
        private const string OPEN_CLASS = "settings-panel--open";
        private const string SCRIM_OPEN_CLASS = "settings-scrim--open";
        private const string THEME_ELECTRIC_CLASS_NAME = "theme--electric";
        private const float SCROLL_SPEED = 0.5f;

        [SerializeField] private ChordMakerController _chordMaker;
        [SerializeField] private WheelMakerController _wheelMaker;
        [SerializeField] private ElectricModeController _modeController;

        private UIDocument _uiDocument;
        private VisualElement _root;
        private VisualElement _panel;
        private VisualElement _scrim;
        private ScrollView _scrollView;
        private Button _guitarModeButton;
        private bool _isOpen;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();

            if (_modeController == null)
            {
                _modeController = FindFirstObjectByType<ElectricModeController>();
            }

            BindElements();
        }

        private void BindElements()
        {
            VisualElement root = _uiDocument.rootVisualElement;
            _root = root.Q(ROOT_NAME);
            _panel = root.Q<VisualElement>(PANEL_NAME);
            _scrim = root.Q<VisualElement>(SCRIM_NAME);
            _scrollView = root.Q<ScrollView>(SCROLL_NAME);
            _guitarModeButton = root.Q<Button>(GUITAR_MODE_BTN);
            root.Q<Button>(CLOSE_BTN)?.RegisterCallback<ClickEvent>(_ => Close());
            root.Q<Button>(CHORD_MAKER_BTN)?.RegisterCallback<ClickEvent>(_ => OpenChordMaker());
            root.Q<Button>(WHEEL_MAKER_BTN)?.RegisterCallback<ClickEvent>(_ => OpenWheelMaker());
            _guitarModeButton?.RegisterCallback<ClickEvent>(_ => ToggleGuitarMode());
            _scrim?.RegisterCallback<ClickEvent>(_ => Close());
        }

        private void OnEnable()
        {
            GuitarModeState.Changed += UpdateModeVisuals;
            UpdateModeVisuals();
        }

        private void OnDisable()
        {
            GuitarModeState.Changed -= UpdateModeVisuals;
        }

        private void ToggleGuitarMode()
        {
            _modeController?.SetMode(!GuitarModeState.IsElectric);
        }

        private void UpdateModeVisuals()
        {
            bool isElectric = GuitarModeState.IsElectric;
            _root?.EnableInClassList(THEME_ELECTRIC_CLASS_NAME, isElectric);

            if (_guitarModeButton != null)
            {
                _guitarModeButton.text = isElectric ? "Guitar: Electric" : "Guitar: Acoustic";
            }
        }

        private void OpenChordMaker()
        {
            Close();
            _chordMaker?.Open();
        }

        private void OpenWheelMaker()
        {
            Close();
            _wheelMaker?.Open();
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
            _scrim?.AddToClassList(SCRIM_OPEN_CLASS);
            if (_scrim != null) _scrim.pickingMode = PickingMode.Position;
        }

        public void Close()
        {
            _isOpen = false;
            _panel.RemoveFromClassList(OPEN_CLASS);
            _panel.pickingMode = PickingMode.Ignore;
            _scrim?.RemoveFromClassList(SCRIM_OPEN_CLASS);
            if (_scrim != null) _scrim.pickingMode = PickingMode.Ignore;
        }
    }
}
