using UnityEngine;
using UnityEngine.UIElements;
using Chordyceps.Gameplay;

namespace Chordyceps.UI
{
    /// <summary>
    /// Owns the acoustic/electric mode for the HUD: applies the electric theme,
    /// shows the AMP button only in electric mode, and opens the amp panel.
    /// Lives on the HUD document, which owns the button.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class ElectricModeController : MonoBehaviour
    {
        [SerializeField] private AmpPanelController _ampPanel;

        private const string HUD_ROOT_NAME = "guitar-hud-root";
        private const string AMP_BUTTON_NAME = "amp-mode-btn";
        private const string THEME_ELECTRIC_CLASS_NAME = "theme--electric";
        private const string AMP_BUTTON_HIDDEN_CLASS_NAME = "hud-amp-btn--hidden";

        private UIDocument _uiDocument;
        private VisualElement _hudRoot;

        public bool IsElectric => GuitarModeState.IsElectric;

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
            root.Q<Button>(AMP_BUTTON_NAME)?.RegisterCallback<ClickEvent>(_ => OpenAmpPanel());
        }

        private void OnEnable()
        {
            if (_ampPanel != null)
            {
                _ampPanel.BackRequested += CloseAmpPanel;
            }

            GuitarModeState.Changed += HandleModeChanged;
            ApplyMode();
        }

        private void OnDisable()
        {
            if (_ampPanel != null)
            {
                _ampPanel.BackRequested -= CloseAmpPanel;
            }

            GuitarModeState.Changed -= HandleModeChanged;
        }

        /// <summary>Switches between acoustic and electric and remembers the choice.</summary>
        public void SetMode(bool isElectric)
        {
            GuitarModeState.SetElectric(isElectric);
        }

        private void OpenAmpPanel()
        {
            if (!GuitarModeState.IsElectric)
            {
                return;
            }

            _ampPanel?.SetVisible(true);
        }

        private void CloseAmpPanel()
        {
            _ampPanel?.SetVisible(false);
        }

        private void HandleModeChanged()
        {
            ApplyMode();
        }

        private void ApplyMode()
        {
            bool isElectric = GuitarModeState.IsElectric;
            _hudRoot?.EnableInClassList(THEME_ELECTRIC_CLASS_NAME, isElectric);
            _hudRoot?.Q<Button>(AMP_BUTTON_NAME)?.EnableInClassList(AMP_BUTTON_HIDDEN_CLASS_NAME, !isElectric);

            if (!isElectric)
            {
                CloseAmpPanel();
            }
        }
    }
}
