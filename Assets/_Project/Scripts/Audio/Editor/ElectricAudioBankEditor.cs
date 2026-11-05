using UnityEditor;
using UnityEngine;

namespace Chordyceps.Audio.Editor
{
    [CustomEditor(typeof(ElectricAudioBank))]
    public class ElectricAudioBankEditor : UnityEditor.Editor
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
                PopulateClips((ElectricAudioBank)target);
            }
        }

        private void DrawOptimizeButton()
        {
            if (GUILayout.Button("Optimize Audio Samples (Preload & Low Latency)", GUILayout.Height(30)))
            {
                AudioBankPopulator.OptimizeAllSampleImports();
            }
        }

        private void PopulateClips(ElectricAudioBank bank)
        {
            AudioClip[] sortedClips = new AudioClip[ElectricAudioBank.TOTAL_NOTES];
            int assignedCount = AudioBankPopulator.Populate(sortedClips, AudioBankPopulator.ElectricSamplesFolder);

            bank.SetClips(sortedClips);
            EditorUtility.SetDirty(bank);
            AssetDatabase.SaveAssets();
            Debug.Log($"ElectricAudioBank: Populated {assignedCount}/{ElectricAudioBank.TOTAL_NOTES} clips.");
        }
    }
}
