using UnityEngine;
using UnityEngine.UIElements;

namespace Chordyceps.UI
{
    /// <summary>
    /// Swaps the guitar HUD for the amp panel. Lives on the HUD document,
    /// which owns the toggle button; the amp panel owns its own back button.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class ElectricModeController : MonoBehaviour
    {
        [SerializeField] private AmpPanelController _ampPanel;

        private const string HUD_ROOT_NAME = "guitar-hud-root";
        private const string TOGGLE_BUTTON_NAME = "amp-mode-btn";
        private const string HUD_HIDDEN_CLASS_NAME = "guitar-hud__root--hidden";

        private UIDocument _uiDocument;
        private VisualElement _hudRoot;
        private bool _isElectricMode;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();

            if (_ampPanel == null)
            {
                _ampPanel = FindFirstObjectByType<AmpPanelController>();
            }

            BindElements();
        }

        private void BindElements()
        {
            VisualElement root = _uiDocument != null ? _uiDocument.rootVisualElement : null;
            if (root == null)
            {
                return;
            }

            _hudRoot = root.Q(HUD_ROOT_NAME);
            root.Q<Button>(TOGGLE_BUTTON_NAME)?.RegisterCallback<ClickEvent>(_ => Toggle());
        }

        private void OnEnable()
        {
            if (_ampPanel != null)
            {
                _ampPanel.BackRequested += HandleBackRequested;
            }
        }

        private void OnDisable()
        {
            if (_ampPanel != null)
            {
                _ampPanel.BackRequested -= HandleBackRequested;
            }
        }

        public void Toggle()
        {
            SetElectricMode(!_isElectricMode);
        }

        private void HandleBackRequested()
        {
            SetElectricMode(false);
        }

        private void SetElectricMode(bool isElectric)
        {
            _isElectricMode = isElectric;
            _hudRoot?.EnableInClassList(HUD_HIDDEN_CLASS_NAME, isElectric);
            _ampPanel?.SetVisible(isElectric);
        }
    }
}
