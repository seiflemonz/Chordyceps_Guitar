using UnityEngine;
using UnityEngine.UIElements;
using Chordyceps.Gameplay;

namespace Chordyceps.UI
{
    /// <summary>
    /// Builds the six guitar strings inside the bottom-right HUD container.
    /// The gesture handling itself lives in the shared <see cref="GuitarStringPanel"/>,
    /// which the Chord Maker reuses for its test strings.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class GuitarStringPanelController : MonoBehaviour
    {
        [SerializeField] private GuitarController _guitarController;

        private const string CONTAINER_NAME = "string-panel-container";

        private UIDocument _uiDocument;
        private VisualElement _container;
        private GuitarStringPanel _panel;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();

            if (_guitarController == null)
            {
                _guitarController = FindFirstObjectByType<GuitarController>();
            }

            BuildStringPanel();
        }

        private void OnEnable()
        {
            if (_panel == null)
            {
                BuildStringPanel();
            }

            _panel?.Attach();
            GuitarModeState.Changed += HandleModeChanged;
        }

        private void OnDisable()
        {
            _panel?.Detach();
            GuitarModeState.Changed -= HandleModeChanged;
        }

        private void HandleModeChanged()
        {
            _panel?.RefreshTheme();
        }

        private void BuildStringPanel()
        {
            VisualElement root = _uiDocument != null ? _uiDocument.rootVisualElement : null;
            if (root == null)
            {
                return;
            }

            _container = root.Q(CONTAINER_NAME);
            if (_container == null)
            {
                Debug.LogWarning($"[GuitarStringPanelController] '{CONTAINER_NAME}' not found in GuitarHUD.uxml.");
                return;
            }

            _panel = new GuitarStringPanel(_container, _guitarController);
        }
    }
}
