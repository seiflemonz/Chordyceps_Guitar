using System;
using UnityEngine;
using UnityEngine.UIElements;
using Chordyceps.Chords;
using Chordyceps.Gameplay;

namespace Chordyceps.UI
{
    /// <summary>
    /// Full-screen chord maker. Browses every chord definition plus any player-created
    /// chords, and lets the user edit a definition's fingering and save it.
    ///
    /// Edits to pre-existing chords are stored as overrides that ChordManager applies
    /// game-wide, so a chord changed here plays that way in the main game until it is
    /// reset to default. Player-created chords live in the Chord Maker only.
    ///
    /// This class only coordinates and owns no chord state itself:
    /// <see cref="ChordCatalog"/> holds the listed chords, <see cref="ChordDraft"/> the
    /// shape being edited, and <see cref="ChordStripView"/> the chip strip.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class ChordMakerController : MonoBehaviour
    {
        private const string ROOT_NAME = "chord-maker-root";
        private const string STRIP_NAME = "chord-maker-strip";
        private const string NECK_HOST_NAME = "chord-maker-neck-host";
        private const string NAME_LABEL_NAME = "chord-maker-name";
        private const string BACK_BTN_NAME = "chord-maker-back-btn";
        private const string ADD_BTN_NAME = "chord-maker-add-btn";
        private const string DELETE_BTN_NAME = "chord-maker-delete-btn";
        private const string RESET_BTN_NAME = "chord-maker-reset-btn";
        private const string CLEAR_BTN_NAME = "chord-maker-clear-btn";
        private const string EDIT_BTN_NAME = "chord-maker-edit-btn";
        private const string CANCEL_BTN_NAME = "chord-maker-cancel-btn";
        private const string SAVE_BTN_NAME = "chord-maker-save-btn";
        private const string STRINGS_CONTAINER_NAME = "chord-maker-strings";
        private const string MODAL_LAYER_NAME = "modal-layer";

        private const string VISIBLE_CLASS = "chord-maker-root--visible";

        private const string DEFAULT_NEW_CHORD_NAME = "New Chord";

        [SerializeField] private ChordManager _chordManager;
        [SerializeField] private GuitarController _guitarController;

        private readonly ChordCatalog _catalog = new ChordCatalog();
        private readonly ChordDraft _draft = new ChordDraft();

        private UIDocument _uiDocument;
        private VisualElement _root;
        private VisualElement _neckHost;
        private Label _nameLabel;
        private ChordNeckElement _neck;
        private ModalDialog _modal;
        private GuitarStringPanel _stringPanel;
        private ChordStripView _stripView;

        private Button _deleteButton;
        private Button _resetButton;
        private Button _clearButton;
        private Button _editButton;
        private Button _cancelButton;
        private Button _saveButton;

        private int _selectedIndex = -1;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            if (_chordManager == null)
            {
                _chordManager = FindFirstObjectByType<ChordManager>();
            }

            if (_guitarController == null)
            {
                _guitarController = FindFirstObjectByType<GuitarController>();
            }

            BindElements();
        }

        private void OnEnable()
        {
            _stringPanel?.Attach();
        }

        private void OnDisable()
        {
            _stringPanel?.Detach();

            // The audition must never outlive the window.
            _chordManager?.ClearPreview();
        }

        private void BindElements()
        {
            VisualElement root = _uiDocument != null ? _uiDocument.rootVisualElement : null;
            if (root == null)
            {
                return;
            }

            _root = root.Q(ROOT_NAME);
            _neckHost = root.Q(NECK_HOST_NAME);
            _nameLabel = root.Q<Label>(NAME_LABEL_NAME);

            BindButton(root, ADD_BTN_NAME, RequestNewChord);
            _deleteButton = BindButton(root, DELETE_BTN_NAME, RequestDelete);
            _resetButton = BindButton(root, RESET_BTN_NAME, RequestReset);
            _clearButton = BindButton(root, CLEAR_BTN_NAME, ClearDraft);
            _editButton = BindButton(root, EDIT_BTN_NAME, BeginEdit);
            _cancelButton = BindButton(root, CANCEL_BTN_NAME, CancelEdit);
            _saveButton = BindButton(root, SAVE_BTN_NAME, () => SaveCurrentDraft(null));
            BindButton(root, BACK_BTN_NAME, Close);

            _stripView = new ChordStripView(root.Q<ScrollView>(STRIP_NAME));
            _stripView.ItemSelected += RequestSelect;

            CreateNeck();
            _modal = new ModalDialog(root.Q(MODAL_LAYER_NAME));
            CreateTestStrings();

            RefreshCatalogAndStrip();
            ApplySelection(_catalog.Count > 0 ? 0 : -1);

            SetVisible(false);

            // A selection is previewed only while the window is open.
            _chordManager?.ClearPreview();
        }

        private static Button BindButton(VisualElement root, string name, Action onClick)
        {
            Button button = root.Q<Button>(name);
            if (button != null)
            {
                button.RegisterCallback<ClickEvent>(_ => onClick());
            }

            return button;
        }

        private void CreateNeck()
        {
            if (_neckHost == null)
            {
                return;
            }

            _neck = new ChordNeckElement();
            _neck.FretTapped += HandleFretTapped;
            _neck.StringMarkerTapped += HandleStringMarkerTapped;
            _neck.ShiftUpRequested += ShiftBaseFretUp;
            _neck.ShiftDownRequested += ShiftBaseFretDown;
            _neckHost.Add(_neck);
        }

        private void CreateTestStrings()
        {
            VisualElement container = _uiDocument.rootVisualElement.Q(STRINGS_CONTAINER_NAME);
            if (container == null || _guitarController == null)
            {
                return;
            }

            _stringPanel = new GuitarStringPanel(container, _guitarController);
        }

        // ---------------------------------------------------------------------
        // Chord list
        // ---------------------------------------------------------------------

        private void RefreshCatalogAndStrip()
        {
            _catalog.Rebuild(_chordManager);
            _stripView?.Build(_catalog.Items);
        }

        private void ApplySelection(int index)
        {
            _selectedIndex = index;
            _stripView?.SetActiveIndex(index);
            UpdateToolbar();

            ChordListItem item = _catalog.Get(index);
            if (_nameLabel != null)
            {
                _nameLabel.text = item != null ? item.Name : string.Empty;
            }

            if (item == null)
            {
                return;
            }

            _neck?.ShowChord(item.Frets);
            _chordManager?.SetPreviewFrets(item.Frets);
        }

        private void RequestSelect(int index)
        {
            ChordListItem item = _catalog.Get(index);
            if (item == null)
            {
                return;
            }

            if (HasUnsavedChanges)
            {
                GuardUnsavedChanges(() => FinishSwitch(item.Id), () => SaveCurrentDraft(item.Id));
                return;
            }

            FinishSwitch(item.Id);
        }

        private void FinishSwitch(string id)
        {
            _draft.End();
            RefreshCatalogAndStrip();

            int index = _catalog.IndexOf(id);
            ApplySelection(index >= 0 ? index : (_catalog.Count > 0 ? 0 : -1));
        }

        // ---------------------------------------------------------------------
        // Editing
        // ---------------------------------------------------------------------

        private bool HasUnsavedChanges => _draft.IsActive && _draft.IsDirty;

        private bool IsCustomDraft
        {
            get
            {
                ChordListItem item = _catalog.Get(_selectedIndex);
                return _draft.IsNew || (item != null && item.IsCustom);
            }
        }

        private string CurrentDraftName()
        {
            if (_draft.IsNew)
            {
                return DEFAULT_NEW_CHORD_NAME;
            }

            return _catalog.Get(_selectedIndex)?.Name ?? string.Empty;
        }

        /// <summary>Asks whether to save the open draft before moving on.</summary>
        private void GuardUnsavedChanges(Action discard, Action save)
        {
            _modal.ShowMessage(
                $"Save changes to \"{CurrentDraftName()}\"?",
                new ModalDialog.Option("Cancel", null),
                new ModalDialog.Option("Don't Save", discard),
                new ModalDialog.Option("Save", save, isPrimary: true));
        }

        private void BeginEdit()
        {
            ChordListItem item = _catalog.Get(_selectedIndex);
            if (item == null)
            {
                return;
            }

            _draft.BeginFrom(item, _selectedIndex);
            ShowDraft();
            UpdateToolbar();
        }

        private void CancelEdit()
        {
            bool wasNew = _draft.IsNew;
            string restoreId = _draft.SourceId;
            int restoreIndex = _draft.SourceIndex;
            _draft.End();

            if (!wasNew)
            {
                ApplySelection(_selectedIndex);
                return;
            }

            // The blank chord never existed, so fall back to whatever was selected before.
            RefreshCatalogAndStrip();
            int index = _catalog.IndexOf(restoreId);
            ApplySelection(index >= 0 ? index : (restoreIndex >= 0 && restoreIndex < _catalog.Count ? restoreIndex : 0));
        }

        private void RequestNewChord()
        {
            if (HasUnsavedChanges)
            {
                GuardUnsavedChanges(StartNewChordDraft, () => SaveCurrentDraft(null, StartNewChordDraft));
                return;
            }

            StartNewChordDraft();
        }

        private void StartNewChordDraft()
        {
            _draft.BeginNew(_selectedIndex, _catalog.IdAt(_selectedIndex));
            _selectedIndex = -1;

            _stripView?.SetActiveIndex(-1);
            if (_nameLabel != null)
            {
                _nameLabel.text = DEFAULT_NEW_CHORD_NAME;
            }

            ShowDraft();
            UpdateToolbar();
        }

        private void HandleFretTapped(int stringIndex, int fret)
        {
            if (_draft.ToggleFret(stringIndex, fret))
            {
                ShowDraft();
            }
        }

        private void HandleStringMarkerTapped(int stringIndex)
        {
            if (_draft.ToggleMute(stringIndex))
            {
                ShowDraft();
            }
        }

        private void ShiftBaseFretUp()
        {
            if (_draft.ShiftBaseFret(1))
            {
                ShowDraft();
            }
        }

        private void ShiftBaseFretDown()
        {
            if (_draft.ShiftBaseFret(-1))
            {
                ShowDraft();
            }
        }

        private void ClearDraft()
        {
            if (_draft.Clear())
            {
                ShowDraft();
            }
        }

        /// <summary>
        /// Pushes the working copy to the neck and the audition, so the fretboard and the
        /// test strings both track the edit live.
        /// </summary>
        private void ShowDraft()
        {
            _neck?.ShowDraft(_draft.Frets, _draft.BaseFret);
            _chordManager?.SetPreviewFrets(_draft.Frets);
        }

        // ---------------------------------------------------------------------
        // Saving / deleting / resetting
        // ---------------------------------------------------------------------

        private void SaveCurrentDraft(string nextIdAfterSave)
        {
            SaveCurrentDraft(nextIdAfterSave, null);
        }

        private void SaveCurrentDraft(string nextIdAfterSave, Action afterSave)
        {
            if (!_draft.IsActive)
            {
                afterSave?.Invoke();
                if (nextIdAfterSave != null)
                {
                    FinishSwitch(nextIdAfterSave);
                }

                return;
            }

            if (IsCustomDraft)
            {
                NameAndSaveCustom(nextIdAfterSave, afterSave);
                return;
            }

            SaveDefinitionEdit(nextIdAfterSave, afterSave);
        }

        /// <summary>
        /// A pre-existing definition keeps its name: the edit becomes an override that
        /// ChordManager applies game-wide, unless it was saved back to its stock shape.
        /// </summary>
        private void SaveDefinitionEdit(string nextIdAfterSave, Action afterSave)
        {
            ChordListItem item = _catalog.Get(_selectedIndex);

            if (item.MatchesDefault(_draft.Frets))
            {
                // Nothing left to override.
                CustomChordStore.RemoveOverride(item.Id);
            }
            else
            {
                CustomChordStore.SaveOverride(item.Id, _draft.Frets, item.IsMajor);
            }

            FinishSave(item.Id, nextIdAfterSave, afterSave);
        }

        /// <summary>Player-created chords are named by the player before they are stored.</summary>
        private void NameAndSaveCustom(string nextIdAfterSave, Action afterSave)
        {
            ChordListItem item = _catalog.Get(_selectedIndex);
            string existingId = _draft.IsNew ? null : item?.Id;
            string suggestedName = existingId == null ? DEFAULT_NEW_CHORD_NAME : item.Name;

            _modal.ShowNameInput(
                "Name this chord",
                suggestedName,
                value => ValidateName(value, existingId),
                value =>
                {
                    string savedId = existingId;
                    if (savedId == null)
                    {
                        savedId = CustomChordStore.CreateCustom(value, _draft.Frets, true).Id;
                    }
                    else
                    {
                        CustomChordStore.UpdateCustom(savedId, value, _draft.Frets);
                    }

                    FinishSave(savedId, nextIdAfterSave, afterSave);
                },
                null);
        }

        private void FinishSave(string savedId, string nextIdAfterSave, Action afterSave)
        {
            _draft.End();

            if (afterSave != null)
            {
                afterSave();
                return;
            }

            FinishSwitch(nextIdAfterSave ?? savedId);
        }

        /// <summary>Chord names must be non-empty and unique across every listed chord.</summary>
        private string ValidateName(string value, string ignoreId)
        {
            string trimmed = (value ?? string.Empty).Trim();
            if (trimmed.Length == 0)
            {
                return "Enter a name for the chord.";
            }

            return _catalog.IsNameTaken(trimmed, ignoreId) ? "That name is already taken." : null;
        }

        private void RequestDelete()
        {
            ChordListItem item = _catalog.Get(_selectedIndex);
            if (item == null || !item.IsCustom || _draft.IsActive)
            {
                return;
            }

            _modal.ShowMessage(
                $"Delete \"{item.Name}\"? This cannot be undone.",
                new ModalDialog.Option("Cancel", null),
                new ModalDialog.Option("Delete", () => DeleteSelectedCustom(item.Id), isDanger: true));
        }

        private void DeleteSelectedCustom(string id)
        {
            if (!CustomChordStore.DeleteCustom(id))
            {
                return;
            }

            _selectedIndex = -1;
            RefreshCatalogAndStrip();
            ApplySelection(_catalog.Count > 0 ? 0 : -1);
        }

        private void RequestReset()
        {
            if (_draft.IsActive)
            {
                return;
            }

            _modal.ShowMessage(
                "Reset all pre-existing chords to their default configurations? "
                + "Your custom-made chords will not be affected.",
                new ModalDialog.Option("Cancel", null),
                new ModalDialog.Option("OK", ResetToDefaults, isPrimary: true));
        }

        private void ResetToDefaults()
        {
            string keepId = _catalog.IdAt(_selectedIndex);
            CustomChordStore.ResetOverrides();

            RefreshCatalogAndStrip();
            int index = _catalog.IndexOf(keepId);
            ApplySelection(index >= 0 ? index : (_catalog.Count > 0 ? 0 : -1));
        }

        // ---------------------------------------------------------------------
        // Toolbar + visibility
        // ---------------------------------------------------------------------

        private void UpdateToolbar()
        {
            ChordListItem item = _catalog.Get(_selectedIndex);
            bool hasSelection = item != null;
            bool editing = _draft.IsActive;

            SetDisplay(_deleteButton, hasSelection && item.IsCustom && !editing);
            SetDisplay(_resetButton, !editing);
            SetDisplay(_clearButton, editing);
            SetDisplay(_editButton, hasSelection && !editing);
            SetDisplay(_cancelButton, editing);
            SetDisplay(_saveButton, editing);
        }

        private static void SetDisplay(VisualElement element, bool visible)
        {
            if (element != null)
            {
                element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        public void Open()
        {
            SetVisible(true);
            ApplySelection(_catalog.Get(_selectedIndex) != null ? _selectedIndex : 0);
        }

        public void Close()
        {
            _draft.End();
            _modal?.Hide();
            ApplySelection(_selectedIndex);
            SetVisible(false);

            // Must come last: ApplySelection re-applies the audition, and leaving it set
            // would freeze every chord in the main game on the one shown here.
            _chordManager?.ClearPreview();
        }

        public void SetVisible(bool visible)
        {
            _root?.EnableInClassList(VISIBLE_CLASS, visible);
        }
    }
}
