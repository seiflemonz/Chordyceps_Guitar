using UnityEngine;
using UnityEngine.UIElements;
using Chordyceps.Chords;

namespace Chordyceps.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class CapoBarController : MonoBehaviour
    {
        [SerializeField] private ChordManager _chordManager;
        [SerializeField] private StyleSheet _styleSheet;

        private const string DECREMENT_BTN_NAME = "capo-decrement-btn";
        private const string INCREMENT_BTN_NAME = "capo-increment-btn";
        private const string VALUE_LABEL_NAME = "capo-value-label";
        private const string DISABLED_CLASS_NAME = "capo-btn--disabled";

        private UIDocument _uiDocument;
        private Button _decrementButton;
        private Button _incrementButton;
        private Label _valueLabel;

        private void Awake()
        {
            CacheReferences();
            ApplyStyleSheet();
            BindElements();
        }

        private void CacheReferences()
        {
            _uiDocument = GetComponent<UIDocument>();
            if (_chordManager == null)
            {
                _chordManager = FindFirstObjectByType<ChordManager>();
            }
        }

        private void ApplyStyleSheet()
        {
            if (_styleSheet == null || _uiDocument == null) return;
            VisualElement root = _uiDocument.rootVisualElement;
            if (root != null && !root.styleSheets.Contains(_styleSheet))
            {
                root.styleSheets.Add(_styleSheet);
            }
        }

        private void BindElements()
        {
            VisualElement root = _uiDocument.rootVisualElement;
            if (root == null) return;

            _decrementButton = root.Q<Button>(DECREMENT_BTN_NAME);
            _incrementButton = root.Q<Button>(INCREMENT_BTN_NAME);
            _valueLabel = root.Q<Label>(VALUE_LABEL_NAME);
        }

        private void OnEnable()
        {
            ApplyStyleSheet();
            RegisterButtonCallbacks();
            SubscribeToChordManager();
            SyncInitialState();
        }

        private void OnDisable()
        {
            UnregisterButtonCallbacks();
            UnsubscribeFromChordManager();
        }

        private void RegisterButtonCallbacks()
        {
            _decrementButton?.RegisterCallback<ClickEvent>(HandleDecrementClicked);
            _incrementButton?.RegisterCallback<ClickEvent>(HandleIncrementClicked);
        }

        private void UnregisterButtonCallbacks()
        {
            _decrementButton?.UnregisterCallback<ClickEvent>(HandleDecrementClicked);
            _incrementButton?.UnregisterCallback<ClickEvent>(HandleIncrementClicked);
        }

        private void SubscribeToChordManager()
        {
            if (_chordManager == null) return;
            _chordManager.OnCapoChanged += HandleCapoChanged;
        }

        private void UnsubscribeFromChordManager()
        {
            if (_chordManager == null) return;
            _chordManager.OnCapoChanged -= HandleCapoChanged;
        }

        private void SyncInitialState()
        {
            if (_chordManager == null) return;
            HandleCapoChanged(_chordManager.CurrentCapo);
        }

        private void HandleDecrementClicked(ClickEvent evt)
        {
            if (_chordManager == null) return;
            _chordManager.DecrementCapo();
        }

        private void HandleIncrementClicked(ClickEvent evt)
        {
            if (_chordManager == null) return;
            _chordManager.IncrementCapo();
        }

        private void HandleCapoChanged(int newCapo)
        {
            UpdateValueDisplay(newCapo);
            UpdateButtonStates(newCapo);
        }

        private void UpdateValueDisplay(int capo)
        {
            if (_valueLabel == null) return;
            _valueLabel.text = capo.ToString();
        }

        private void UpdateButtonStates(int capo)
        {
            bool isMin = (capo <= ChordManager.MIN_CAPO);
            bool isMax = (capo >= ChordManager.MAX_CAPO);
            _decrementButton?.EnableInClassList(DISABLED_CLASS_NAME, isMin);
            _incrementButton?.EnableInClassList(DISABLED_CLASS_NAME, isMax);
        }
    }
}
