using UnityEngine;
using UnityEngine.UIElements;
using Chordyceps.Chords;

namespace Chordyceps.UI
{
    /// <summary>
    /// Drives the main HUD's chord wheel. The chords come from <see cref="ChordManager"/>,
    /// which serves either the built-in scale for the selected key or the layout the player
    /// applied in the Wheel Maker, so this view stays the same in both cases.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class ChordWheelController : MonoBehaviour
    {
        private const string KEY_PREFIX = "KEY: ";
        private const string WHEEL_PREFIX = "WHEEL: ";

        [SerializeField] private ChordManager _chordManager;

        private UIDocument _uiDocument;
        private ChordWheelView _wheel;

        private void Awake()
        {
            CacheReferences();
            _wheel = new ChordWheelView(_uiDocument.rootVisualElement, ChordWheelMetrics.Hud, false);
        }

        private void CacheReferences()
        {
            _uiDocument = GetComponent<UIDocument>();
            if (_chordManager == null)
            {
                _chordManager = FindFirstObjectByType<ChordManager>();
            }
        }

        private void OnEnable()
        {
            SubscribeToEvents();
            SyncInitialState();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (_chordManager == null) return;
            _chordManager.OnWheelChanged += HandleWheelChanged;
            _chordManager.OnChordChanged += HandleChordChanged;
        }

        private void UnsubscribeFromEvents()
        {
            if (_chordManager == null) return;
            _chordManager.OnWheelChanged -= HandleWheelChanged;
            _chordManager.OnChordChanged -= HandleChordChanged;
        }

        private void SyncInitialState()
        {
            if (_chordManager == null) return;
            HandleWheelChanged();
        }

        private void HandleWheelChanged()
        {
            if (_chordManager == null) return;

            _wheel.SetHeaderPrefix(_chordManager.UsesCustomLayout ? WHEEL_PREFIX : KEY_PREFIX);
            _wheel.SetHeader(_chordManager.CurrentWheelHeader);

            for (int slot = 0; slot < ChordWheelView.SECTOR_COUNT; slot++)
            {
                _wheel.SetSlot(slot, _chordManager.GetWheelRoman(slot), _chordManager.GetWheelChordName(slot));
            }

            _wheel.SetActiveSlot(_chordManager.CurrentChordIndex);
        }

        private void HandleChordChanged()
        {
            if (_chordManager == null) return;
            _wheel.SetActiveSlot(_chordManager.CurrentChordIndex);
        }
    }
}
