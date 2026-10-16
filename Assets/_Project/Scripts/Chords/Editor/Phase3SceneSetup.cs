using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Chordyceps.Chords;
using Chordyceps.UI;

namespace Chordyceps.Editor
{
    public static class Phase3SceneSetup
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/GuitarPrototype.unity";
        private const string PANEL_SETTINGS_PATH = "Assets/_Project/UI/GuitarHUDPanelSettings.asset";
        private const string UXML_PATH = "Assets/_Project/UI/GuitarHUD.uxml";
        private const string SCALES_DIR = "Assets/_Project/ScriptableObjects/Chords/Keys";

        private static readonly string[] _scaleKeys = new[]
        {
            "C_Major", "G_Major", "D_Major", "A_Major",
            "E_Major", "B_Major", "Fs_Major", "Db_Major",
            "Ab_Major", "Eb_Major", "Bb_Major", "F_Major"
        };

        [InitializeOnLoadMethod]
        private static void AutoRunIfPending()
        {
            EditorApplication.delayCall += () =>
            {
                if (!System.IO.File.Exists(PANEL_SETTINGS_PATH))
                {
                    RunSetup();
                }
            };
        }

        [MenuItem("Tools/Chordyceps/Setup Phase 3 Scene")]
        public static void RunSetup()
        {
            EnsurePanelSettings();
            ChordAssetGenerator.GenerateAllKeys();
            ConfigureScene();
            Debug.Log("[Phase3SceneSetup] Completed successfully.");
        }

        private static void EnsurePanelSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PANEL_SETTINGS_PATH);
            if (settings != null) return;

            settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(1080, 2400);
            settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            settings.match = 0.5f;
            settings.sortingOrder = 1;

            AssetDatabase.CreateAsset(settings, PANEL_SETTINGS_PATH);
            AssetDatabase.SaveAssets();
        }

        private static void ConfigureScene()
        {
            Scene scene = EditorSceneManager.OpenScene(SCENE_PATH);
            ChordManager chordManager = ConfigureChordManager();
            ConfigureUIHierarchy(chordManager);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static ChordManager ConfigureChordManager()
        {
            ChordManager manager = Object.FindFirstObjectByType<ChordManager>();
            if (manager == null) return null;

            SerializedObject so = new SerializedObject(manager);
            SerializedProperty scalesProp = so.FindProperty("_allScales");
            scalesProp.arraySize = _scaleKeys.Length;

            for (int i = 0; i < _scaleKeys.Length; i++)
            {
                string path = $"{SCALES_DIR}/{_scaleKeys[i]}/Scale_{_scaleKeys[i]}.asset";
                var scaleAsset = AssetDatabase.LoadAssetAtPath<ScaleDefinition>(path);
                scalesProp.GetArrayElementAtIndex(i).objectReferenceValue = scaleAsset;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(manager);
            return manager;
        }

        private static void ConfigureUIHierarchy(ChordManager chordManager)
        {
            GameObject uiRoot = GameObject.Find("[UI]");
            if (uiRoot == null) return;

            SetupUIDocument(uiRoot);
            SetupUIControllers(uiRoot, chordManager);
            DisableOldCanvas(uiRoot);
        }

        private static void SetupUIDocument(GameObject uiRoot)
        {
            UIDocument doc = uiRoot.GetComponent<UIDocument>();
            if (doc == null) doc = uiRoot.AddComponent<UIDocument>();

            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_PATH);
            var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PANEL_SETTINGS_PATH);

            doc.visualTreeAsset = uxml;
            doc.panelSettings = panelSettings;
            EditorUtility.SetDirty(doc);
        }

        private static void SetupUIControllers(GameObject uiRoot, ChordManager chordManager)
        {
            CapoBarUI capoUI = GetOrAddComponent<CapoBarUI>(uiRoot);
            KeySelectorUI keyUI = GetOrAddComponent<KeySelectorUI>(uiRoot);
            ChordWheelUI wheelUI = GetOrAddComponent<ChordWheelUI>(uiRoot);
            GuitarHUDController hudCtrl = GetOrAddComponent<GuitarHUDController>(uiRoot);

            WireChordManager(capoUI, chordManager);
            WireChordManager(keyUI, chordManager);
            WireChordManager(wheelUI, chordManager);
            WireHUDController(hudCtrl, capoUI, keyUI, wheelUI);
        }

        private static T GetOrAddComponent<T>(GameObject target) where T : Component
        {
            T comp = target.GetComponent<T>();
            if (comp == null) comp = target.AddComponent<T>();
            return comp;
        }

        private static void WireChordManager(Component uiComp, ChordManager chordManager)
        {
            SerializedObject so = new SerializedObject(uiComp);
            SerializedProperty prop = so.FindProperty("_chordManager");
            if (prop != null)
            {
                prop.objectReferenceValue = chordManager;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(uiComp);
            }
        }

        private static void WireHUDController(GuitarHUDController hud, CapoBarUI capo, KeySelectorUI key, ChordWheelUI wheel)
        {
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("_capoBarUI").objectReferenceValue = capo;
            so.FindProperty("_keySelectorUI").objectReferenceValue = key;
            so.FindProperty("_chordWheelUI").objectReferenceValue = wheel;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(hud);
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
    }
}
