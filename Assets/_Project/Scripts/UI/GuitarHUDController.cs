using UnityEngine;
using UnityEngine.UIElements;

namespace Chordyceps.UI
{
    /// <summary>Bootstraps all HUD sub-controllers from the single UIDocument.</summary>
    [RequireComponent(typeof(UIDocument))]
    public class GuitarHUDController : MonoBehaviour
    {
        [SerializeField] private CapoBarUI _capoBarUI;
        [SerializeField] private KeySelectorUI _keySelectorUI;
        [SerializeField] private ChordWheelUI _chordWheelUI;

        private UIDocument _document;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            FindSubControllersIfMissing();
        }

        private void FindSubControllersIfMissing()
        {
            if (_capoBarUI == null) _capoBarUI = GetComponent<CapoBarUI>();
            if (_keySelectorUI == null) _keySelectorUI = GetComponent<KeySelectorUI>();
            if (_chordWheelUI == null) _chordWheelUI = GetComponent<ChordWheelUI>();
        }

        private void OnEnable()
        {
            VisualElement root = _document.rootVisualElement;
            if (root == null) return;

            _capoBarUI?.Initialize(root);
            _keySelectorUI?.Initialize(root);
            _chordWheelUI?.Initialize(root);
        }
    }
}
