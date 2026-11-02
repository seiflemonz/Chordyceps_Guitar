using System;
using System.Collections.Generic;
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
        private const string CHIP_CLASS = "chord-maker-chip";
        private const string CHIP_ACTIVE_CLASS = "chord-maker-chip--active";
        private const string CHIP_CUSTOM_CLASS = "chord-maker-chip--custom";
        private const string CHIP_MODIFIED_CLASS = "chord-maker-chip--modified";

        private const string DEFAULT_NEW_CHORD_NAME = "New Chord";

        [SerializeField] private ChordManager _chordManager;
        [SerializeField] private GuitarController _guitarController;

        /// <summary>One entry in the chord strip: a definition asset or a stored chord.</summary>
        private class ChordItem
        {
            public string Id;
            public string Name;
            public int[] Frets;

            /// <summary>Asset default for a pre-existing chord; null for custom chords.</summary>
            public int[] DefaultFrets;

            public bool IsCustom;
            public bool IsModified;
            public bool IsMajor;
        }

        private readonly List<ChordItem> _items = new List<ChordItem>();
        private readonly List<Button> _chips = new List<Button>();

        private UIDocument _uiDocument;
        private VisualElement _root;
        private ScrollView _strip;
        private VisualElement _neckHost;
        private Label _nameLabel;
        private ChordNeckElement _neck;
        private ModalDialog _modal;
        private GuitarStringPanel _stringPanel;

        private Button _addButton;
        private Button _deleteButton;
        private Button _resetButton;
        private Button _clearButton;
        private Button _editButton;
        private Button _cancelButton;
        private Button _saveButton;

        private int _selectedIndex = -1;

        private bool _editing;
        private bool _isNewDraft;
        private bool _dirty;
        private int[] _draftFrets;
        private int _draftBaseFret = ChordNeckElement.MIN_BASE_FRET;
        private int _indexBeforeNewDraft = -1;

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
            _strip = root.Q<ScrollView>(STRIP_NAME);
            _neckHost = root.Q(NECK_HOST_NAME);
            _nameLabel = root.Q<Label>(NAME_LABEL_NAME);

            _addButton = root.Q<Button>(ADD_BTN_NAME);
            _deleteButton = root.Q<Button>(DELETE_BTN_NAME);
            _resetButton = root.Q<Button>(RESET_BTN_NAME);
            _clearButton = root.Q<Button>(CLEAR_BTN_NAME);
            _editButton = root.Q<Button>(EDIT_BTN_NAME);
            _cancelButton = root.Q<Button>(CANCEL_BTN_NAME);
            _saveButton = root.Q<Button>(SAVE_BTN_NAME);

            root.Q<Button>(BACK_BTN_NAME)?.RegisterCallback<ClickEvent>(_ => Close());
            _addButton?.RegisterCallback<ClickEvent>(_ => RequestNewChord());
            _deleteButton?.RegisterCallback<ClickEvent>(_ => RequestDelete());
            _resetButton?.RegisterCallback<ClickEvent>(_ => RequestReset());
            _clearButton?.RegisterCallback<ClickEvent>(_ => ClearDraft());
            _editButton?.RegisterCallback<ClickEvent>(_ => BeginEdit());
            _cancelButton?.RegisterCallback<ClickEvent>(_ => CancelEdit());
            _saveButton?.RegisterCallback<ClickEvent>(_ => SaveCurrentDraft(null));

            CreateNeck();
            _modal = new ModalDialog(root.Q(MODAL_LAYER_NAME));
            CreateTestStrings();

            RefreshItemsAndStrip();
            ApplySelection(_items.Count > 0 ? 0 : -1);

            SetVisible(false);

            // A selection is previewed only while the window is open.
            _chordManager?.ClearPreview();
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

        /// <summary>Collects the distinct definition assets, then any stored chords.</summary>
        private void RebuildItems()
        {
            _items.Clear();
            if (_chordManager == null)
            {
                return;
            }

            var seenNames = new HashSet<string>();
            int keyCount = _chordManager.TotalKeys;
            for (int key = 0; key < keyCount; key++)
            {
                ScaleDefinition scale = _chordManager.GetScale(key);
                if (scale == null)
                {
                    continue;
                }

                for (int chordIndex = 0; chordIndex < ScaleDefinition.DIATONIC_CHORD_COUNT; chordIndex++)
                {
                    ChordDefinition chord = scale.GetChord(chordIndex);
                    if (chord == null || !seenNames.Add(chord.ChordName))
                    {
                        continue;
                    }

                    ChordEntry overridden = CustomChordStore.GetOverride(chord.ChordName);
                    _items.Add(new ChordItem
                    {
                        Id = chord.ChordName,
                        Name = chord.ChordName,
                        Frets = overridden != null ? (int[])overridden.Frets.Clone() : ReadFrets(chord),
                        DefaultFrets = ReadFrets(chord),
                        IsCustom = false,
                        IsModified = overridden != null,
                        IsMajor = chord.IsMajor,
                    });
                }
            }

            IReadOnlyList<ChordEntry> customs = CustomChordStore.Customs;
            for (int i = 0; i < customs.Count; i++)
            {
                ChordEntry custom = customs[i];
                _items.Add(new ChordItem
                {
                    Id = custom.Id,
                    Name = custom.Name,
                    Frets = (int[])custom.Frets.Clone(),
                    IsCustom = true,
                    IsModified = false,
                    IsMajor = custom.IsMajor,
                });
            }
        }

        private static int[] ReadFrets(ChordDefinition chord)
        {
            var frets = new int[ChordNeckElement.STRING_COUNT];
            for (int i = 0; i < frets.Length; i++)
            {
                frets[i] = chord.GetFretOffset(i);
            }

            return frets;
        }

        private static bool AreFretsEqual(int[] left, int[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }

        private void RefreshItemsAndStrip()
        {
            RebuildItems();
            RebuildStrip();
        }

        private void RebuildStrip()
        {
            _strip?.Clear();
            _chips.Clear();

            if (_strip == null)
            {
                return;
            }

            for (int i = 0; i < _items.Count; i++)
            {
                int index = i;
                ChordItem item = _items[i];
                var chip = new Button(() => RequestSelect(index)) { text = item.Name };
                chip.focusable = false;
                chip.AddToClassList(CHIP_CLASS);

                if (item.IsCustom)
                {
                    chip.AddToClassList(CHIP_CUSTOM_CLASS);
                }
                else if (item.IsModified)
                {
                    chip.AddToClassList(CHIP_MODIFIED_CLASS);
                }

                _strip.Add(chip);
                _chips.Add(chip);
            }
        }

        private void ApplySelection(int index)
        {
            _selectedIndex = index;

            for (int i = 0; i < _chips.Count; i++)
            {
                _chips[i].EnableInClassList(CHIP_ACTIVE_CLASS, i == index);
            }

            UpdateToolbar();

            if (index < 0 || index >= _items.Count)
            {
                if (_nameLabel != null)
                {
                    _nameLabel.text = string.Empty;
                }

                return;
            }

            ChordItem item = _items[index];
            if (_nameLabel != null)
            {
                _nameLabel.text = item.Name;
            }

            _neck?.ShowChord(item.Frets);
            _chordManager?.SetPreviewFrets(item.Frets);
        }

        private void RequestSelect(int index)
        {
            if (index < 0 || index >= _items.Count)
            {
                return;
            }

            string targetId = _items[index].Id;

            if (_editing && _dirty)
            {
                _modal.ShowMessage(
                    $"Save changes to \"{CurrentDraftName()}\"?",
                    new ModalDialog.Option("Cancel", null),
                    new ModalDialog.Option("Don't Save", () => FinishSwitch(targetId)),
                    new ModalDialog.Option("Save", () => SaveCurrentDraft(targetId), isPrimary: true));
                return;
            }

            FinishSwitch(targetId);
        }

        private void FinishSwitch(string id)
        {
            _editing = false;
            _dirty = false;

            RefreshItemsAndStrip();
            int index = _items.FindIndex(item => item.Id == id);
            ApplySelection(index >= 0 ? index : (_items.Count > 0 ? 0 : -1));
        }

        // ---------------------------------------------------------------------
        // Editing
        // ---------------------------------------------------------------------

        private bool IsCustomDraft => _isNewDraft || (_selectedIndex >= 0 && _items[_selectedIndex].IsCustom);

        private string CurrentDraftName()
        {
            if (_isNewDraft)
            {
                return DEFAULT_NEW_CHORD_NAME;
            }

            return _selectedIndex >= 0 ? _items[_selectedIndex].Name : string.Empty;
        }

        private void BeginEdit()
        {
            if (_selectedIndex < 0)
            {
                return;
            }

            ChordItem item = _items[_selectedIndex];

            _editing = true;
            _isNewDraft = false;
            _dirty = false;
            _draftFrets = (int[])item.Frets.Clone();
            _draftBaseFret = ChordNeckElement.DeriveBaseFret(_draftFrets);

            _neck?.ShowDraft(_draftFrets, _draftBaseFret);
            _chordManager?.SetPreviewFrets(_draftFrets);
            UpdateToolbar();
        }

        private void CancelEdit()
        {
            int restoreIndex = _indexBeforeNewDraft;
            bool wasNew = _isNewDraft;

            _editing = false;
            _isNewDraft = false;
            _dirty = false;
            _indexBeforeNewDraft = -1;

            if (wasNew)
            {
                // The blank chord never existed, so fall back to whatever was selected before.
                RefreshItemsAndStrip();
                int index = _items.FindIndex(item => item.Id == (_restoreId ?? string.Empty));
                ApplySelection(index >= 0 ? index : (restoreIndex >= 0 && restoreIndex < _items.Count ? restoreIndex : 0));
                return;
            }

            ApplySelection(_selectedIndex);
        }

        private string _restoreId;

        private void RequestNewChord()
        {
            if (_editing && _dirty)
            {
                _modal.ShowMessage(
                    $"Save changes to \"{CurrentDraftName()}\"?",
                    new ModalDialog.Option("Cancel", null),
                    new ModalDialog.Option("Don't Save", StartNewChordDraft),
                    new ModalDialog.Option("Save", () => SaveCurrentDraft(null, StartNewChordDraft), isPrimary: true));
                return;
            }

            StartNewChordDraft();
        }

        private void StartNewChordDraft()
        {
            _indexBeforeNewDraft = _selectedIndex;
            _restoreId = _selectedIndex >= 0 ? _items[_selectedIndex].Id : null;

            _selectedIndex = -1;
            _editing = true;
            _isNewDraft = true;
            _dirty = false;
            _draftFrets = new int[ChordNeckElement.STRING_COUNT];
            _draftBaseFret = ChordNeckElement.MIN_BASE_FRET;

            for (int i = 0; i < _chips.Count; i++)
            {
                _chips[i].EnableInClassList(CHIP_ACTIVE_CLASS, false);
            }

            if (_nameLabel != null)
            {
                _nameLabel.text = DEFAULT_NEW_CHORD_NAME;
            }

            _neck?.ShowDraft(_draftFrets, _draftBaseFret);
            _chordManager?.SetPreviewFrets(_draftFrets);
            UpdateToolbar();
        }

        private void HandleFretTapped(int stringIndex, int fret)
        {
            if (!_editing || _draftFrets == null || stringIndex < 0 || stringIndex >= _draftFrets.Length)
            {
                return;
            }

            // Tapping the fret a string already holds releases it back to open.
            _draftFrets[stringIndex] = _draftFrets[stringIndex] == fret
                ? ChordNeckElement.OPEN
                : fret;

            MarkDraftChanged();
        }

        private void HandleStringMarkerTapped(int stringIndex)
        {
            if (!_editing || _draftFrets == null || stringIndex < 0 || stringIndex >= _draftFrets.Length)
            {
                return;
            }

            _draftFrets[stringIndex] = _draftFrets[stringIndex] == ChordNeckElement.MUTED
                ? ChordNeckElement.OPEN
                : ChordNeckElement.MUTED;

            MarkDraftChanged();
        }

        private void ShiftBaseFretUp()
        {
            if (!_editing)
            {
                return;
            }

            _draftBaseFret = Mathf.Min(_draftBaseFret + 1, ChordNeckElement.MAX_BASE_FRET);
            _neck?.ShowDraft(_draftFrets, _draftBaseFret);
        }

        private void ShiftBaseFretDown()
        {
            if (!_editing)
            {
                return;
            }

            _draftBaseFret = Mathf.Max(_draftBaseFret - 1, ChordNeckElement.MIN_BASE_FRET);
            _neck?.ShowDraft(_draftFrets, _draftBaseFret);
        }

        /// <summary>Resets the working copy to every string open, on the lowest fret window.</summary>
        private void ClearDraft()
        {
            if (!_editing)
            {
                return;
            }

            for (int i = 0; i < _draftFrets.Length; i++)
            {
                _draftFrets[i] = ChordNeckElement.OPEN;
            }

            _draftBaseFret = ChordNeckElement.MIN_BASE_FRET;
            MarkDraftChanged();
        }

        private void MarkDraftChanged()
        {
            _dirty = true;
            _neck?.ShowDraft(_draftFrets, _draftBaseFret);

            // Audition the working copy so the test strings track the edit live.
            _chordManager?.SetPreviewFrets(_draftFrets);
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
            if (!_editing || _draftFrets == null)
            {
                afterSave?.Invoke();
                if (nextIdAfterSave != null)
                {
                    FinishSwitch(nextIdAfterSave);
                }

                return;
            }

            if (!IsCustomDraft)
            {
                // A pre-existing definition keeps its name; the override is applied game-wide.
                ChordItem item = _items[_selectedIndex];

                // Saved back to its stock shape, so there is nothing left to override.
                if (AreFretsEqual(_draftFrets, item.DefaultFrets))
                {
                    CustomChordStore.RemoveOverride(item.Id);
                }
                else
                {
                    CustomChordStore.SaveOverride(item.Id, _draftFrets, item.IsMajor);
                }

                FinishSave(item.Id, nextIdAfterSave, afterSave);
                return;
            }

            string existingId = _isNewDraft ? null : _items[_selectedIndex].Id;
            string suggestedName = _isNewDraft ? DEFAULT_NEW_CHORD_NAME : _items[_selectedIndex].Name;

            _modal.ShowNameInput(
                "Name this chord",
                suggestedName,
                value => ValidateName(value, existingId),
                value =>
                {
                    string savedId;
                    if (string.IsNullOrEmpty(existingId))
                    {
                        savedId = CustomChordStore.CreateCustom(value, _draftFrets, true).Id;
                    }
                    else
                    {
                        CustomChordStore.UpdateCustom(existingId, value, _draftFrets);
                        savedId = existingId;
                    }

                    FinishSave(savedId, nextIdAfterSave, afterSave);
                },
                null);
        }

        private void FinishSave(string savedId, string nextIdAfterSave, Action afterSave)
        {
            _editing = false;
            _isNewDraft = false;
            _dirty = false;
            _indexBeforeNewDraft = -1;
            _restoreId = null;

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

            for (int i = 0; i < _items.Count; i++)
            {
                ChordItem item = _items[i];
                if (item.Id == ignoreId)
                {
                    continue;
                }

                if (string.Equals(item.Name, trimmed, StringComparison.OrdinalIgnoreCase))
                {
                    return "That name is already taken.";
                }
            }

            return null;
        }

        private void RequestDelete()
        {
            if (_selectedIndex < 0 || _editing)
            {
                return;
            }

            ChordItem item = _items[_selectedIndex];
            if (!item.IsCustom)
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
            RefreshItemsAndStrip();
            ApplySelection(_items.Count > 0 ? 0 : -1);
        }

        private void RequestReset()
        {
            if (_editing)
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
            string keepId = _selectedIndex >= 0 ? _items[_selectedIndex].Id : null;
            CustomChordStore.ResetOverrides();

            RefreshItemsAndStrip();
            int index = _items.FindIndex(item => item.Id == keepId);
            ApplySelection(index >= 0 ? index : (_items.Count > 0 ? 0 : -1));
        }

        // ---------------------------------------------------------------------
        // Toolbar + visibility
        // ---------------------------------------------------------------------

        private void UpdateToolbar()
        {
            bool hasSelection = _selectedIndex >= 0;
            bool isCustom = hasSelection && _items[_selectedIndex].IsCustom;

            SetDisplay(_deleteButton, hasSelection && isCustom && !_editing);
            SetDisplay(_resetButton, !_editing);
            SetDisplay(_clearButton, _editing);
            SetDisplay(_editButton, hasSelection && !_editing);
            SetDisplay(_cancelButton, _editing);
            SetDisplay(_saveButton, _editing);
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
            ApplySelection(_selectedIndex >= 0 && _selectedIndex < _items.Count ? _selectedIndex : 0);
        }

        public void Close()
        {
            _editing = false;
            _isNewDraft = false;
            _dirty = false;
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
