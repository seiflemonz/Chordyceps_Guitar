using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using Chordyceps.Chords;
using Chordyceps.Gameplay;
using Chordyceps.Input;
using Chordyceps.UI;

namespace Chordyceps.Editor
{
    public static class GuitarHUDSetup
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/GuitarPrototype.unity";
        private const string UXML_PATH = "Assets/_Project/UI/GuitarHUD.uxml";
        private const string USS_PATH = "Assets/_Project/UI/GuitarHUD.uss";
        private const string PANEL_SETTINGS_PATH = "Assets/_Project/UI/GuitarHUDPanelSettings.asset";

        [MenuItem("Tools/Chordyceps/Setup Guitar HUD")]
        [MenuItem("Tools/Chordyceps/Setup Chord Wheel HUD")]
        public static void SetupSceneHUD()
        {
            var scene = EditorSceneManager.OpenScene(SCENE_PATH);
            GameObject uiRoot = GameObject.Find("[UI]");
            if (uiRoot == null)
            {
                Debug.LogError("[GuitarHUDSetup] [UI] GameObject not found in scene!");
                return;
            }

            ConfigureUIDocument(uiRoot);
            ConfigureChordWheelController(uiRoot);
            ConfigureCapoBarController(uiRoot);
            ConfigureKeySelectionController(uiRoot);
            ConfigureShapeSelectorController(uiRoot);
            ConfigureStringPanelController(uiRoot);
            RetireWorldSpaceStrings();
            DisableOldCanvas(uiRoot);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("<color=green>[GuitarHUDSetup] Guitar HUD configured and scene saved!</color>");
        }

        /// <summary>
        /// Strings are now drawn by the UI panel, so the world-space string
        /// objects are removed from the scene. GuitarString.cs and
        /// GuitarTouchInput.cs are kept on disk, unused.
        /// </summary>
        private static void RetireWorldSpaceStrings()
        {
            GuitarString[] legacyStrings = Object.FindObjectsByType<GuitarString>(FindObjectsSortMode.None);
            for (int i = 0; i < legacyStrings.Length; i++)
            {
                Object.DestroyImmediate(legacyStrings[i].gameObject);
            }

            GuitarTouchInput[] legacyInputs = Object.FindObjectsByType<GuitarTouchInput>(FindObjectsSortMode.None);
            for (int i = 0; i < legacyInputs.Length; i++)
            {
                legacyInputs[i].enabled = false;
                EditorUtility.SetDirty(legacyInputs[i]);
            }
        }

        private static void DisableOldCanvas(GameObject uiRoot)
        {
            Transform oldCanvas = uiRoot.transform.Find("MainCanvas");
            if (oldCanvas != null)
            {
                oldCanvas.gameObject.SetActive(false);
                EditorUtility.SetDirty(oldCanvas.gameObject);
            }
        }

        private static void ConfigureUIDocument(GameObject uiRoot)
        {
            var doc = uiRoot.GetComponent<UIDocument>() ?? uiRoot.AddComponent<UIDocument>();
            doc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_PATH);
            doc.panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PANEL_SETTINGS_PATH);
            EditorUtility.SetDirty(doc);
        }

        private static void ConfigureChordWheelController(GameObject uiRoot)
        {
            var controller = uiRoot.GetComponent<ChordWheelController>() ?? uiRoot.AddComponent<ChordWheelController>();
            var chordManager = Object.FindFirstObjectByType<ChordManager>();
            var serializedObj = new SerializedObject(controller);
            serializedObj.FindProperty("_chordManager").objectReferenceValue = chordManager;
            serializedObj.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
        }

        private static void ConfigureCapoBarController(GameObject uiRoot)
        {
            var controller = uiRoot.GetComponent<CapoBarController>() ?? uiRoot.AddComponent<CapoBarController>();
            var chordManager = Object.FindFirstObjectByType<ChordManager>();
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(USS_PATH);
            var serializedObj = new SerializedObject(controller);
            serializedObj.FindProperty("_chordManager").objectReferenceValue = chordManager;
            serializedObj.FindProperty("_styleSheet").objectReferenceValue = styleSheet;
            serializedObj.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
        }

        private static void ConfigureKeySelectionController(GameObject uiRoot)
        {
            var controller = uiRoot.GetComponent<KeySelectionController>() ?? uiRoot.AddComponent<KeySelectionController>();
            var chordManager = Object.FindFirstObjectByType<ChordManager>();
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(USS_PATH);
            var serializedObj = new SerializedObject(controller);
            serializedObj.FindProperty("_chordManager").objectReferenceValue = chordManager;
            serializedObj.FindProperty("_styleSheet").objectReferenceValue = styleSheet;
            serializedObj.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
        }

        private static void ConfigureShapeSelectorController(GameObject uiRoot)
        {
            var controller = uiRoot.GetComponent<ChordShapeSelectorController>() ?? uiRoot.AddComponent<ChordShapeSelectorController>();
            var chordManager = Object.FindFirstObjectByType<ChordManager>();
            var serializedObj = new SerializedObject(controller);
            serializedObj.FindProperty("_chordManager").objectReferenceValue = chordManager;
            serializedObj.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
        }

        private static void ConfigureStringPanelController(GameObject uiRoot)
        {
            var controller = uiRoot.GetComponent<GuitarStringPanelController>() ?? uiRoot.AddComponent<GuitarStringPanelController>();
            var guitarController = Object.FindFirstObjectByType<GuitarController>();
            var serializedObj = new SerializedObject(controller);
            serializedObj.FindProperty("_guitarController").objectReferenceValue = guitarController;
            serializedObj.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
        }
    }
}
