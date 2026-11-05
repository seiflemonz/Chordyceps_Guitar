using UnityEditor;
using UnityEngine;

namespace Chordyceps.Audio.Editor
{
    [CustomEditor(typeof(ChromaticAudioBank))]
    public class ChromaticAudioBankEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space(10);
            DrawPopulateButton();
            EditorGUILayout.Space(5);
            DrawOptimizeButton();
        }

        private void DrawPopulateButton()
        {
            if (GUILayout.Button("Populate & Sort Clips From Samples Folder", GUILayout.Height(30)))
            {
                PopulateClips((ChromaticAudioBank)target);
            }
        }

        private void DrawOptimizeButton()
        {
            if (GUILayout.Button("Optimize Audio Samples (Preload & Low Latency)", GUILayout.Height(30)))
            {
                OptimizeSampleImportSettings();
            }
        }

        [MenuItem("Tools/Chordyceps/Optimize Audio Samples for Low Latency")]
        public static void OptimizeSampleImportSettings()
        {
            AudioBankPopulator.OptimizeAllSampleImports();
        }

        private void PopulateClips(ChromaticAudioBank bank)
        {
            AudioClip[] sortedClips = new AudioClip[ChromaticAudioBank.TOTAL_NOTES];
            int assignedCount = AudioBankPopulator.Populate(sortedClips, AudioBankPopulator.AcousticSamplesFolder);

            bank.SetClips(sortedClips);
            EditorUtility.SetDirty(bank);
            AssetDatabase.SaveAssets();
            Debug.Log($"ChromaticAudioBank: Populated {assignedCount}/{ChromaticAudioBank.TOTAL_NOTES} clips.");
        }
    }
}
