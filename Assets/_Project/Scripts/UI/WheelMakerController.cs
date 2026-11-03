using System;
using UnityEngine;
using UnityEngine.UIElements;
using Chordyceps.Chords;

namespace Chordyceps.UI
{
    /// <summary>
    /// Full-screen wheel maker. Browses the built-in layout plus the player's saved layouts,
    /// lets the user rearrange which chords sit on each wheel, add wheels, and save the
    /// result as a named layout that can be applied to the main game.
    ///
    /// Editing the built-in layout is deliberately one-way: it opens as a new layout, so the
    /// shipped layout can never be changed in place. Chord data itself is untouched here —
    /// this screen only decides which chord belongs on which wheel.
    ///
    /// This class only coordinates and owns no state of its own:
    /// <see cref="WheelCatalog"/> holds the listed layouts, <see cref="WheelDraft"/> the
    /// layout being edited, and the three views render it.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class WheelMakerController : MonoBehaviour
    {
        private const string ROOT_NAME = "wheel-maker-root";
        private const string WHEEL_NAME = "wheel-maker-wheel";
        private const string GRID_NAME = "wheel-maker-grid";
        private const string LAYOUT_SCROLL_NAME = "wheel-maker-layout-scroll";
        private const string BACK_BTN_NAME = "wheel-maker-back-btn";
        private const string CREATE_BTN_NAME = "wheel-maker-create-btn";
        private const string APPLY_BTN_NAME = "wheel-maker-apply-btn";
        private const string SAVE_BTN_NAME = "wheel-maker-save-btn";
        private const string MODAL_LAYER_NAME = "modal-layer";

        private const string VISIBLE_CLASS = "wheel-maker-root--visible";

        private const string DEFAULT_NEW_LAYOUT_NAME = "New Layout";

        /// <summary>
        /// The large wheel, scaled up from the HUD's 380px wheel by the same factor its
        /// markup is, so the sectors, labels and glow keep their proportions.
        /// </summary>
        private static readonly ChordWheelMetrics LargeWheelMetrics = new ChordWheelMetrics(260f, 98f, 222f, 160f, 2f, 4.8f);

        [SerializeField] private ChordManager _chordManager;

        private readonly WheelCatalog _catalog = new WheelCatalog();
        private readonly WheelDraft _draft = new WheelDraft();
        private readonly ChordCatalog _chordCatalog = new ChordCatalog();

        private UIDocument _uiDocument;
        private VisualElement _root;
        private ChordWheelView _wheel;
        private ModalDialog _modal;
        private WheelLayoutListView _layoutList;
        private WheelSlotGridView _slotGrid;
        private ChordLibraryPanel _library;

        private int _selectedIndex = -1;
        private int _librarySlot = -1;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            if (_chordManager == null)
            {
                _chordManager = FindFirstObjectByType<ChordManager>();
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

            _root = root.Q(ROOT_NAME);

            BindButton(root, BACK_BTN_NAME, Close);
            BindButton(root, CREATE_BTN_NAME, RequestCreateLayout);
            BindButton(root, APPLY_BTN_NAME, RequestApply);
            BindButton(root, SAVE_BTN_NAME, () => RequestSave());

            VisualElement wheelHost = root.Q(WHEEL_NAME);
            _wheel = wheelHost != null ? new ChordWheelView(wheelHost, LargeWheelMetrics, true) : null;
            if (_wheel != null)
            {
                _wheel.SectorTapped += HandleSectorTapped;
            }

            _slotGrid = new WheelSlotGridView(root.Q(GRID_NAME));
            _slotGrid.WheelSelected += HandleWheelSelected;
            _slotGrid.AddRequested += HandleAddWheel;

            _layoutList = new WheelLayoutListView(root.Q<ScrollView>(LAYOUT_SCROLL_NAME));
            _layoutList.LayoutSelected += RequestSelectLayout;

            _library = new ChordLibraryPanel(root);
            _library.ChordChosen += HandleChordChosen;

            _modal = new ModalDialog(root.Q(MODAL_LAYER_NAME));

            RefreshCatalog();
            ApplySelection(_catalog.Count > 0 ? 0 : -1);

            SetVisible(false);
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

        // ---------------------------------------------------------------------
        // Catalog + selection
        // ---------------------------------------------------------------------

        private void RefreshCatalog()
        {
            _catalog.Rebuild(_chordManager);
            _layoutList?.Build(_catalog.Items);

            _chordCatalog.Rebuild(_chordManager);
            _library?.Build(_chordCatalog.Items);
        }

        /// <summary>
        /// Opens a catalog layout for viewing and editing. The built-in layout is opened as
        /// a new layout so that saving it produces a separate, named layout.
        /// </summary>
        private void ApplySelection(int index)
        {
            _selectedIndex = index;
            _layoutList?.SetActiveIndex(index);

            WheelLayoutEntry layout = _catalog.Get(index);
            if (layout == null)
            {
                _draft.End();
            }
            else
            {
                _draft.BeginFrom(layout, index, layout.Id == WheelCatalog.DEFAULT_ID);
            }

            RefreshSlotGrid();
            RefreshWheel();
            CloseLibrary();
        }

        /// <summary>Discards the open draft and selects the given catalog index.</summary>
        private void SwitchTo(int index)
        {
            _draft.End();
            RefreshCatalog();
            ApplySelection(index);
        }

        /// <summary>Discards the open draft and returns to whatever layout it came from.</summary>
        private void DiscardDraft()
        {
            string sourceId = _draft.SourceId;
            int sourceIndex = _draft.SourceIndex;
            _draft.End();
            RefreshCatalog();

            int index = _catalog.IndexOf(sourceId);
            if (index < 0)
            {
                index = sourceIndex;
            }

            ApplySelection(index >= 0 && index < _catalog.Count ? index : (_catalog.Count > 0 ? 0 : -1));
        }

        private void RequestSelectLayout(int index)
        {
            if (index == _selectedIndex)
            {
                return;
            }

            if (HasUnsavedChanges)
            {
                GuardUnsavedChanges(() => SwitchTo(index), () => SaveCurrentDraft(() => SwitchTo(index)));
                return;
            }

            ApplySelection(index);
        }

        // ---------------------------------------------------------------------
        // Rendering
        // ---------------------------------------------------------------------

        private void RefreshSlotGrid()
        {
            _slotGrid?.Build(_draft);
            _slotGrid?.SetActiveIndex(_draft.ActiveWheelIndex);
        }

        private void RefreshWheel()
        {
            if (_wheel == null)
            {
                return;
            }

            if (!_draft.IsActive)
            {
                _wheel.SetHeader(string.Empty);
                for (int slot = 0; slot < ChordWheelView.SECTOR_COUNT; slot++)
                {
                    _wheel.SetSlot(slot, null, string.Empty);
                }

                _wheel.SetActiveSlot(-1);
                return;
            }

            int wheelIndex = _draft.ActiveWheelIndex;
            _wheel.SetHeader(_draft.GetWheelLabel(wheelIndex));
            for (int slot = 0; slot < ChordWheelView.SECTOR_COUNT; slot++)
            {
                _wheel.SetSlot(slot, null, ChordNameOf(_draft.GetSector(wheelIndex, slot)));
            }
        }

        /// <summary>
        /// Display name for a stored chord id. Ids that no longer resolve — a custom chord
        /// deleted since the layout was saved — leave their sector blank.
        /// </summary>
        private string ChordNameOf(string chordId)
        {
            if (string.IsNullOrEmpty(chordId))
            {
                return string.Empty;
            }

            return _chordCatalog.Get(_chordCatalog.IndexOf(chordId))?.Name ?? string.Empty;
        }

        private void CloseLibrary()
        {
            _librarySlot = -1;
            _library?.Close();
            _wheel?.SetActiveSlot(-1);
        }

        // ---------------------------------------------------------------------
        // Editing
        // ---------------------------------------------------------------------

        private void HandleSectorTapped(int slot)
        {
            if (!_draft.IsActive)
            {
                return;
            }

            _librarySlot = slot;
            _library?.Open(slot, _draft.GetSector(_draft.ActiveWheelIndex, slot));
            _wheel?.SetActiveSlot(slot);
        }

        private void HandleChordChosen(string chordId)
        {
            if (_librarySlot < 0)
            {
                return;
            }

            if (_draft.SetSector(_librarySlot, chordId))
            {
                RefreshWheel();
            }

            CloseLibrary();
        }

        private void HandleWheelSelected(int index)
        {
            if (_draft.SelectWheel(index))
            {
                _slotGrid?.SetActiveIndex(_draft.ActiveWheelIndex);
                RefreshWheel();
            }

            CloseLibrary();
        }

        private void HandleAddWheel()
        {
            if (!_draft.AddWheel())
            {
                return;
            }

            RefreshSlotGrid();
            RefreshWheel();
            CloseLibrary();
        }

        private void RequestCreateLayout()
        {
            if (HasUnsavedChanges)
            {
                GuardUnsavedChanges(StartNewLayoutDraft, () => SaveCurrentDraft(() => StartNewLayoutDraft()));
                return;
            }

            StartNewLayoutDraft();
        }

        private void StartNewLayoutDraft()
        {
            _draft.BeginNew(_selectedIndex);
            _selectedIndex = -1;
            _layoutList?.SetActiveIndex(-1);

            RefreshSlotGrid();
            RefreshWheel();
            CloseLibrary();
        }

        // ---------------------------------------------------------------------
        // Saving / applying
        // ---------------------------------------------------------------------

        private bool HasUnsavedChanges => _draft.IsActive && _draft.IsDirty;

        private string CurrentDraftName()
        {
            return string.IsNullOrEmpty(_draft.Name) ? DEFAULT_NEW_LAYOUT_NAME : _draft.Name;
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

        private void RequestSave()
        {
            SaveCurrentDraft(null);
        }

        private void SaveCurrentDraft(Action afterSave)
        {
            if (!_draft.IsActive)
            {
                afterSave?.Invoke();
                return;
            }

            string existingId = _draft.IsNew ? null : _draft.SourceId;

            _modal.ShowNameInput(
                "Name this layout",
                CurrentDraftName(),
                value => ValidateName(value, existingId),
                value =>
                {
                    string savedId = existingId;
                    if (savedId == null)
                    {
                        savedId = CustomWheelStore.CreateCustom(value, _draft.Wheels).Id;
                    }
                    else
                    {
                        CustomWheelStore.UpdateCustom(savedId, value, _draft.Wheels);
                    }

                    FinishSave(savedId, afterSave);
                },
                null);
        }

        private void FinishSave(string savedId, Action afterSave)
        {
            _draft.End();
            RefreshCatalog();

            if (afterSave != null)
            {
                afterSave();
                return;
            }

            int index = _catalog.IndexOf(savedId);
            ApplySelection(index >= 0 ? index : (_catalog.Count > 0 ? 0 : -1));
        }

        /// <summary>Layout names must be non-empty and unique; the built-in name is reserved.</summary>
        private string ValidateName(string value, string ignoreId)
        {
            string trimmed = (value ?? string.Empty).Trim();
            if (trimmed.Length == 0)
            {
                return "Enter a name for the layout.";
            }

            return _catalog.IsNameTaken(trimmed, ignoreId) ? "That name is already taken." : null;
        }

        private void RequestApply()
        {
            if (HasUnsavedChanges)
            {
                GuardUnsavedChanges(
                    () =>
                    {
                        DiscardDraft();
                        ApplySelectedLayout();
                    },
                    () => SaveCurrentDraft(() => ApplySelectedLayout()));
                return;
            }

            ApplySelectedLayout();
        }

        /// <summary>
        /// Hands the selected layout to the main game. The built-in layout is recorded as a
        /// null id, which means "use the scale-driven wheel" — the standard behaviour.
        /// </summary>
        private void ApplySelectedLayout()
        {
            WheelLayoutEntry layout = _catalog.Get(_selectedIndex);
            bool isDefault = layout == null || layout.Id == WheelCatalog.DEFAULT_ID;

            CustomWheelStore.SetActiveLayout(isDefault ? null : layout.Id);
            _chordManager?.ApplyWheelSource();

            string appliedName = layout != null ? layout.Name : WheelCatalog.DefaultLayoutName;
            _modal.ShowMessage(
                $"\"{appliedName}\" is now the main game's wheel layout.",
                new ModalDialog.Option("OK", null, isPrimary: true));
        }

        // ---------------------------------------------------------------------
        // Visibility
        // ---------------------------------------------------------------------

        public void Open()
        {
            // The chord library can have changed while the Chord Maker was open.
            RefreshCatalog();
            SetVisible(true);
            ApplySelection(_catalog.Get(_selectedIndex) != null ? _selectedIndex : (_catalog.Count > 0 ? 0 : -1));
        }

        public void Close()
        {
            _draft.End();
            _modal?.Hide();
            CloseLibrary();
            ApplySelection(_selectedIndex);
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            _root?.EnableInClassList(VISIBLE_CLASS, visible);
        }
    }
}
