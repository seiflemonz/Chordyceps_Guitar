using UnityEngine;
using UnityEngine.UIElements;

namespace Chordyceps.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class HUDMenuButtonBinder : MonoBehaviour
    {
        private const string MENU_BTN_NAME = "hud-menu-btn";

        [SerializeField] private SettingsMenuController _settingsMenu;

        private void Awake()
        {
            UIDocument doc  = GetComponent<UIDocument>();
            VisualElement root = doc.rootVisualElement;
            root.Q<Button>(MENU_BTN_NAME)?.RegisterCallback<ClickEvent>(_ => _settingsMenu.Open());
        }
    }
}
