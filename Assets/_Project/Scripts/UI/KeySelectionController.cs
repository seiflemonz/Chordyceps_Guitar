using UnityEngine;
using UnityEngine.UIElements;
using Chordyceps.Chords;

namespace Chordyceps.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class KeySelectionController : MonoBehaviour
    {
        [SerializeField] private ChordManager _chordManager;
        [SerializeField] private StyleSheet _styleSheet;

        private const string PREV_BTN_NAME = "key-prev-btn";
        private const string NEXT_BTN_NAME = "key-next-btn";
        private const string ROOT_LABEL_NAME = "key-root-label";
        private const string MAJOR_BTN_NAME = "key-mode-major-btn";
        private const string MINOR_BTN_NAME = "key-mode-minor-btn";
        private const string MODE_THUMB_NAME = "key-mode-thumb";
        private const string MINOR_THUMB_CLASS = "key-mode-thumb--minor";
        private const string ACTIVE_BTN_CLASS = "key-mode-btn--active";

        private UIDocument _uiDocument;
        private Button _prevButton;
        private Button _nextButton;
        private Button _majorButton;
        private Button _minorButton;
        private Label _rootLabel;
        private VisualElement _modeThumb;

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

            _prevButton = root.Q<Button>(PREV_BTN_NAME);
            _nextButton = root.Q<Button>(NEXT_BTN_NAME);
            _rootLabel = root.Q<Label>(ROOT_LABEL_NAME);
            _majorButton = root.Q<Button>(MAJOR_BTN_NAME);
            _minorButton = root.Q<Button>(MINOR_BTN_NAME);
            _modeThumb = root.Q(MODE_THUMB_NAME);
        }

        private void OnEnable()
        {
            ApplyStyleSheet();
            RegisterCallbacks();
            SubscribeToChordManager();
            SyncInitialState();
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
            UnsubscribeFromChordManager();
        }

        private void RegisterCallbacks()
        {
            _prevButton?.RegisterCallback<ClickEvent>(HandlePrevClicked);
            _nextButton?.RegisterCallback<ClickEvent>(HandleNextClicked);
            _majorButton?.RegisterCallback<ClickEvent>(HandleMajorClicked);
            _minorButton?.RegisterCallback<ClickEvent>(HandleMinorClicked);
        }

        private void UnregisterCallbacks()
        {
            _prevButton?.UnregisterCallback<ClickEvent>(HandlePrevClicked);
            _nextButton?.UnregisterCallback<ClickEvent>(HandleNextClicked);
            _majorButton?.UnregisterCallback<ClickEvent>(HandleMajorClicked);
            _minorButton?.UnregisterCallback<ClickEvent>(HandleMinorClicked);
        }

        private void SubscribeToChordManager()
        {
            if (_chordManager == null) return;
            _chordManager.OnKeySelectionChanged += HandleKeySelectionChanged;
        }

        private void UnsubscribeFromChordManager()
        {
            if (_chordManager == null) return;
            _chordManager.OnKeySelectionChanged -= HandleKeySelectionChanged;
        }

        private void SyncInitialState()
        {
            if (_chordManager == null) return;
            HandleKeySelectionChanged(_chordManager.CurrentRootName, _chordManager.CurrentMode);
        }

        private void HandlePrevClicked(ClickEvent evt)
        {
            if (_chordManager == null) return;
            _chordManager.DecrementKey();
        }

        private void HandleNextClicked(ClickEvent evt)
        {
            if (_chordManager == null) return;
            _chordManager.IncrementKey();
        }

        private void HandleMajorClicked(ClickEvent evt)
        {
            if (_chordManager == null) return;
            _chordManager.SetKeyMode(ChordManager.KeyMode.Major);
        }

        private void HandleMinorClicked(ClickEvent evt)
        {
            if (_chordManager == null) return;
            _chordManager.SetKeyMode(ChordManager.KeyMode.Minor);
        }

        private void HandleKeySelectionChanged(string rootName, ChordManager.KeyMode mode)
        {
            UpdateRootLabel(rootName);
            UpdateModeVisuals(mode);
        }

        private void UpdateRootLabel(string rootName)
        {
            if (_rootLabel == null) return;
            _rootLabel.text = rootName;
        }

        private void UpdateModeVisuals(ChordManager.KeyMode mode)
        {
            bool isMinor = (mode == ChordManager.KeyMode.Minor);
            _modeThumb?.EnableInClassList(MINOR_THUMB_CLASS, isMinor);
            _majorButton?.EnableInClassList(ACTIVE_BTN_CLASS, !isMinor);
            _minorButton?.EnableInClassList(ACTIVE_BTN_CLASS, isMinor);
        }
    }
}
