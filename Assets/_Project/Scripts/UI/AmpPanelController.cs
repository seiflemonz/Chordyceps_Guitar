using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Chordyceps.Audio;

namespace Chordyceps.UI
{
    /// <summary>
    /// Builds the seven amp knobs and forwards their values to the amp mixer.
    /// Owns only presentation; audio routing lives in AmpMixerController.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class AmpPanelController : MonoBehaviour
    {
        [SerializeField] private AmpMixerController _mixer;

        private const string PANEL_NAME = "amp-panel";
        private const string GRID_NAME = "amp-knob-grid";
        private const string BACK_BUTTON_NAME = "amp-back-btn";
        private const string VISIBLE_CLASS_NAME = "amp-panel--visible";

        private readonly Dictionary<AmpParameter, AmpKnobElement> _knobs =
            new Dictionary<AmpParameter, AmpKnobElement>();

        private UIDocument _uiDocument;
        private VisualElement _panel;

        /// <summary>Raised when the player asks to leave amp mode.</summary>
        public event Action BackRequested;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();

            if (_mixer == null)
            {
                _mixer = FindFirstObjectByType<AmpMixerController>();
            }

            BindPanel();
        }

        private void Start()
        {
            InitializeMixerToMatchUI();
        }

        private void InitializeMixerToMatchUI()
        {
            if (_mixer == null)
            {
                return;
            }

            foreach (var kvp in _knobs)
            {
                _mixer.SetParameter(kvp.Key, kvp.Value.Value);
            }
        }

        private void BindPanel()
        {
            VisualElement root = _uiDocument != null ? _uiDocument.rootVisualElement : null;
            if (root == null)
            {
                return;
            }

            _panel = root.Q(PANEL_NAME);
            if (_panel == null)
            {
                Debug.LogWarning($"[AmpPanelController] '{PANEL_NAME}' not found in AmpPanel.uxml.");
                return;
            }

            BuildKnobs(root.Q(GRID_NAME));
            root.Q<Button>(BACK_BUTTON_NAME)?.RegisterCallback<ClickEvent>(_ => BackRequested?.Invoke());
            SetVisible(false);
        }

        private void BuildKnobs(VisualElement grid)
        {
            if (grid == null)
            {
                return;
            }

            foreach (AmpParameter parameter in AmpMixerParameters.All)
            {
                var knob = new AmpKnobElement(parameter, AmpMixerParameters.GetDefaultNormalized(parameter));
                knob.ValueChanged += HandleKnobChanged;
                _knobs[parameter] = knob;
                grid.Add(knob);
                _mixer?.SetParameter(parameter, knob.Value);
            }
        }

        private void HandleKnobChanged(AmpParameter parameter, float value)
        {
            _mixer?.SetParameter(parameter, value);
        }

        public void SetVisible(bool visible)
        {
            _panel?.EnableInClassList(VISIBLE_CLASS_NAME, visible);
        }
    }
}
