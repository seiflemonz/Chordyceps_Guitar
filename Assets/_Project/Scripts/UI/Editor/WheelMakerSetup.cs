using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using Chordyceps.Chords;
using Chordyceps.UI;

namespace Chordyceps.Editor
{
    /// <summary>
    /// Wires the Wheel Maker window into GuitarPrototype.unity. Mirrors the
    /// ChordMakerSetup pattern: creates the document object and links it to the
    /// settings menu item that opens it.
    /// </summary>
    public static class WheelMakerSetup
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/GuitarPrototype.unity";
        private const string UXML_PATH = "Assets/_Project/UI/WheelMaker.uxml";
        private const string PANEL_SETTINGS_PATH = "Assets/_Project/UI/GuitarHUDPanelSettings.asset";
        private const string SCENE_OBJECT_NAME = "[WheelMaker]";
        private const int SORTING_ORDER = 4;

        [MenuItem("Tools/Chordyceps/Setup Wheel Maker")]
        public static void SetupWheelMaker()
        {
            var scene = EditorSceneManager.OpenScene(SCENE_PATH);

            WheelMakerController controller = ConfigureWheelMakerObject();
            ConfigureSettingsLink(controller);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("<color=green>[WheelMakerSetup] Wheel Maker configured and scene saved!</color>");
        }

        private static WheelMakerController ConfigureWheelMakerObject()
        {
            GameObject panelObject = GameObject.Find(SCENE_OBJECT_NAME);
            if (panelObject == null)
            {
                panelObject = new GameObject(SCENE_OBJECT_NAME);
            }

            var document = panelObject.GetComponent<UIDocument>() ?? panelObject.AddComponent<UIDocument>();
            document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_PATH);
            document.panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PANEL_SETTINGS_PATH);
            document.sortingOrder = SORTING_ORDER;
            EditorUtility.SetDirty(document);

            var controller = panelObject.GetComponent<WheelMakerController>() ?? panelObject.AddComponent<WheelMakerController>();
            SetObjectReference(controller, "_chordManager", Object.FindFirstObjectByType<ChordManager>());
            EditorUtility.SetDirty(panelObject);
            return controller;
        }

        private static void ConfigureSettingsLink(WheelMakerController controller)
        {
            var settings = Object.FindFirstObjectByType<SettingsMenuController>();
            if (settings == null)
            {
                Debug.LogWarning("[WheelMakerSetup] SettingsMenuController not found. Run 'Tools/Chordyceps/Setup Guitar HUD' first, then re-run this setup.");
                return;
            }

            SetObjectReference(settings, "_wheelMaker", controller);
        }

        private static void SetObjectReference(Object target, string fieldName, Object value)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            if (property == null)
            {
                Debug.LogError($"[WheelMakerSetup] Field '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
