using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;
using Chordyceps.Audio;
using Chordyceps.Gameplay;
using Chordyceps.Input;
using Chordyceps.UI;

namespace Chordyceps.Editor
{
    /// <summary>
    /// Wires the electric mode / amp feature into GuitarPrototype.unity and
    /// verifies the GuitarMixer contract. Mirrors the GuitarHUDSetup pattern.
    /// </summary>
    public static class AmpPanelSetup
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/GuitarPrototype.unity";
        private const string AMP_UXML_PATH = "Assets/_Project/UI/AmpPanel.uxml";
        private const string PANEL_SETTINGS_PATH = "Assets/_Project/UI/GuitarHUDPanelSettings.asset";
        private const string MIXER_PATH = "Assets/_Project/Audio/GuitarMixer.mixer";
        private const string ELECTRIC_BANK_PATH = "Assets/_Project/ScriptableObjects/Audio/DefaultElectricBank.asset";
        private const string DI_CLIP_PATH = "Assets/_Project/Audio/Audio(12).wav";
        private const string SCENE_NAME_BY_NAME = "[AmpPanel]";
        private const string ELECTRIC_GROUP_NAME = "Electric";
        private const int ELECTRIC_POOL_SIZE = 4;
        private const int AMP_PANEL_SORTING_ORDER = 1;

        [MenuItem("Tools/Chordyceps/Setup Amp Panel")]
        public static void SetupAmpPanel()
        {
            var scene = EditorSceneManager.OpenScene(SCENE_PATH);

            GameObject systemsRoot = GameObject.Find("[Systems]");
            GameObject gameplayRoot = GameObject.Find("[Gameplay]");
            GameObject uiRoot = GameObject.Find("[UI]");

            if (systemsRoot == null || gameplayRoot == null || uiRoot == null)
            {
                Debug.LogError("[AmpPanelSetup] Expected scene roots ([Systems], [Gameplay], [UI]) not found.");
                return;
            }

            AudioMixerGroup electricGroup = FindMixerGroup(ELECTRIC_GROUP_NAME);
            ElectricAudioBank bank = EnsureElectricBank();

            AmpMixerController mixerController = ConfigureMixerController(systemsRoot);
            AudioVoicePool electricPool = ConfigureElectricPool(systemsRoot, electricGroup);
            ElectricGuitarController electricController = ConfigureElectricController(gameplayRoot, electricPool, bank);
            ConfigureKeyboardInput(systemsRoot, electricController);

            AmpPanelController ampPanel = ConfigureAmpPanelObject(mixerController);
            ConfigureElectricModeController(uiRoot, ampPanel);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            ValidateAmpMixer();
            Debug.Log("<color=green>[AmpPanelSetup] Electric mode + amp panel configured and scene saved!</color>");
        }

        private static AudioMixerGroup FindMixerGroup(string groupName)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(MIXER_PATH);
            foreach (Object asset in assets)
            {
                if (asset is AudioMixerGroup group && group.name == groupName)
                {
                    return group;
                }
            }

            Debug.LogWarning($"[AmpPanelSetup] No AudioMixerGroup named '{groupName}' found in {MIXER_PATH}. " +
                             "Create the mixer (see Tools/Chordyceps/Validate Amp Mixer output) and re-run this setup.");
            return null;
        }

        private static ElectricAudioBank EnsureElectricBank()
        {
            var bank = AssetDatabase.LoadAssetAtPath<ElectricAudioBank>(ELECTRIC_BANK_PATH);
            if (bank != null)
            {
                return bank;
            }

            bank = ScriptableObject.CreateInstance<ElectricAudioBank>();
            AssetDatabase.CreateAsset(bank, ELECTRIC_BANK_PATH);

            var clips = new AudioClip[ElectricAudioBank.TOTAL_NOTES];
            clips[0] = AssetDatabase.LoadAssetAtPath<AudioClip>(DI_CLIP_PATH);
            bank.SetClips(clips);

            EditorUtility.SetDirty(bank);
            AssetDatabase.SaveAssets();
            Debug.Log($"[AmpPanelSetup] Created {ELECTRIC_BANK_PATH} with the DI clip in slot 0.");
            return bank;
        }

        private static AmpMixerController ConfigureMixerController(GameObject systemsRoot)
        {
            var controller = systemsRoot.GetComponent<AmpMixerController>() ?? systemsRoot.AddComponent<AmpMixerController>();
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MIXER_PATH);
            SetObjectReference(controller, "_mixer", mixer);
            return controller;
        }

        private static AudioVoicePool ConfigureElectricPool(GameObject systemsRoot, AudioMixerGroup electricGroup)
        {
            AudioVoicePool pool = FindExistingElectricPool(systemsRoot);
            if (pool == null)
            {
                pool = systemsRoot.AddComponent<AudioVoicePool>();
            }

            SetIntValue(pool, "_poolSize", ELECTRIC_POOL_SIZE);
            SetObjectReference(pool, "_outputGroup", electricGroup);
            return pool;
        }

        /// <summary>
        /// The acoustic pool has no output group, so the first pool with one
        /// assigned is the electric pool from a previous run.
        /// </summary>
        private static AudioVoicePool FindExistingElectricPool(GameObject systemsRoot)
        {
            AudioVoicePool[] pools = systemsRoot.GetComponents<AudioVoicePool>();
            foreach (AudioVoicePool pool in pools)
            {
                var serialized = new SerializedObject(pool);
                if (serialized.FindProperty("_outputGroup").objectReferenceValue != null)
                {
                    return pool;
                }
            }

            return null;
        }

        private static ElectricGuitarController ConfigureElectricController(
            GameObject gameplayRoot, AudioVoicePool pool, ElectricAudioBank bank)
        {
            var controller = gameplayRoot.GetComponent<ElectricGuitarController>()
                             ?? gameplayRoot.AddComponent<ElectricGuitarController>();
            SetObjectReference(controller, "_voicePool", pool);
            SetObjectReference(controller, "_audioBank", bank);
            return controller;
        }

        private static void ConfigureKeyboardInput(GameObject systemsRoot, ElectricGuitarController electricController)
        {
            var input = systemsRoot.GetComponent<GuitarKeyboardInput>();
            if (input != null)
            {
                SetObjectReference(input, "_electricController", electricController);
            }
        }

        private static AmpPanelController ConfigureAmpPanelObject(AmpMixerController mixerController)
        {
            GameObject panelObject = GameObject.Find(SCENE_NAME_BY_NAME);
            if (panelObject == null)
            {
                panelObject = new GameObject(SCENE_NAME_BY_NAME);
            }

            var document = panelObject.GetComponent<UIDocument>() ?? panelObject.AddComponent<UIDocument>();
            document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AMP_UXML_PATH);
            document.panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PANEL_SETTINGS_PATH);
            document.sortingOrder = AMP_PANEL_SORTING_ORDER;
            EditorUtility.SetDirty(document);

            var controller = panelObject.GetComponent<AmpPanelController>() ?? panelObject.AddComponent<AmpPanelController>();
            SetObjectReference(controller, "_mixer", mixerController);
            EditorUtility.SetDirty(panelObject);
            return controller;
        }

        private static void ConfigureElectricModeController(GameObject uiRoot, AmpPanelController ampPanel)
        {
            var controller = uiRoot.GetComponent<ElectricModeController>() ?? uiRoot.AddComponent<ElectricModeController>();
            SetObjectReference(controller, "_ampPanel", ampPanel);
            EditorUtility.SetDirty(controller);
        }

        private static void SetObjectReference(Object target, string fieldName, Object value)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            if (property == null)
            {
                Debug.LogError($"[AmpPanelSetup] Field '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetIntValue(Object target, string fieldName, int value)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            if (property == null)
            {
                Debug.LogError($"[AmpPanelSetup] Field '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }

            property.intValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [MenuItem("Tools/Chordyceps/Validate Amp Mixer")]
        public static void ValidateAmpMixer()
        {
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MIXER_PATH);
            if (mixer == null)
            {
                Debug.LogError($"[AmpPanelSetup] No AudioMixer at {MIXER_PATH}. " +
                               "Create it via Assets > Create > Audio > Audio Mixer, add a child group named " +
                               $"'{ELECTRIC_GROUP_NAME}' with Distortion / ParamEQ / SFX Reverb effects, then expose " +
                               "the parameters listed in AmpMixerParameters.cs.");
                return;
            }

            int missing = 0;
            foreach (AmpParameter parameter in AmpMixerParameters.All)
            {
                string name = AmpMixerParameters.GetExposedName(parameter);
                if (mixer.GetFloat(name, out _))
                {
                    Debug.Log($"[AmpPanelSetup] Exposed parameter OK: {name}");
                }
                else
                {
                    missing++;
                    Debug.LogWarning($"[AmpPanelSetup] Missing exposed parameter: {name}");
                }
            }

            // Special check for the new AmpDrive pre-gain parameter
            if (!mixer.GetFloat(AmpMixerParameters.DRIVE_NAME, out _))
            {
                missing++;
                Debug.LogWarning($"[AmpPanelSetup] Missing recommended exposed parameter: {AmpMixerParameters.DRIVE_NAME} " +
                                 "(Add a Compressor effect BEFORE distortion and expose its Make-up Gain as AmpDrive).");
            }

            if (missing == 0)
            {
                Debug.Log("<color=green>[AmpPanelSetup] All amp parameters are exposed correctly.</color>");
            }
        }
    }
}
