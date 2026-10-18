using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using Chordyceps.Chords;
using Chordyceps.UI;

namespace Chordyceps.Editor
{
    public static class ChordWheelSetup
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/GuitarPrototype.unity";
        private const string UXML_PATH = "Assets/_Project/UI/ChordWheel.uxml";
        private const string PANEL_SETTINGS_PATH = "Assets/_Project/UI/GuitarHUDPanelSettings.asset";

        [MenuItem("Tools/Chordyceps/Setup Chord Wheel HUD")]
        public static void SetupSceneHUD()
        {
            var scene = EditorSceneManager.OpenScene(SCENE_PATH);
            GameObject uiRoot = GameObject.Find("[UI]");
            if (uiRoot == null)
            {
                Debug.LogError("[ChordWheelSetup] [UI] GameObject not found in scene!");
                return;
            }

            ConfigureUIDocument(uiRoot);
            ConfigureChordWheelController(uiRoot);
            DisableOldCanvas(uiRoot);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("<color=green>[ChordWheelSetup] Chord Wheel HUD configured and scene saved!</color>");
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
    }
}
