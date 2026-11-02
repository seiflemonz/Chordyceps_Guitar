using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using Chordyceps.Chords;
using Chordyceps.Gameplay;
using Chordyceps.UI;

namespace Chordyceps.Editor
{
    /// <summary>
    /// Wires the Chord Maker window into GuitarPrototype.unity. Mirrors the
    /// AmpPanelSetup pattern: creates the document object and links it to the
    /// settings menu item that opens it.
    /// </summary>
    public static class ChordMakerSetup
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/GuitarPrototype.unity";
        private const string UXML_PATH = "Assets/_Project/UI/ChordMaker.uxml";
        private const string PANEL_SETTINGS_PATH = "Assets/_Project/UI/GuitarHUDPanelSettings.asset";
        private const string SCENE_OBJECT_NAME = "[ChordMaker]";
        private const int SORTING_ORDER = 3;

        [MenuItem("Tools/Chordyceps/Setup Chord Maker")]
        public static void SetupChordMaker()
        {
            var scene = EditorSceneManager.OpenScene(SCENE_PATH);

            ChordMakerController controller = ConfigureChordMakerObject();
            ConfigureSettingsLink(controller);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("<color=green>[ChordMakerSetup] Chord Maker configured and scene saved!</color>");
        }

        private static ChordMakerController ConfigureChordMakerObject()
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

            var controller = panelObject.GetComponent<ChordMakerController>() ?? panelObject.AddComponent<ChordMakerController>();
            SetObjectReference(controller, "_chordManager", Object.FindFirstObjectByType<ChordManager>());
            SetObjectReference(controller, "_guitarController", Object.FindFirstObjectByType<GuitarController>());
            EditorUtility.SetDirty(panelObject);
            return controller;
        }

        private static void ConfigureSettingsLink(ChordMakerController controller)
        {
            var settings = Object.FindFirstObjectByType<SettingsMenuController>();
            if (settings == null)
            {
                Debug.LogWarning("[ChordMakerSetup] SettingsMenuController not found. Run 'Tools/Chordyceps/Setup Guitar HUD' first, then re-run this setup.");
                return;
            }

            SetObjectReference(settings, "_chordMaker", controller);
        }

        private static void SetObjectReference(Object target, string fieldName, Object value)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            if (property == null)
            {
                Debug.LogError($"[ChordMakerSetup] Field '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
