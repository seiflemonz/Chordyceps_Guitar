using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Chordyceps.Audio.Editor
{
    [CustomEditor(typeof(ChromaticAudioBank))]
    public class ChromaticAudioBankEditor : UnityEditor.Editor
    {
        private const string SAMPLES_FOLDER_PATH = "Assets/_Project/Audio/Samples";

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
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { SAMPLES_FOLDER_PATH });
            int modifiedCount = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                modifiedCount = ProcessAllSampleImports(guids);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();
            Debug.Log($"ChromaticAudioBank: Optimized {modifiedCount} audio samples for low latency.");
        }

        private static int ProcessAllSampleImports(string[] guids)
        {
            int count = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (ConfigureAudioImporter(path))
                {
                    count++;
                }
            }
            return count;
        }

        private static bool ConfigureAudioImporter(string path)
        {
            AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null) return false;

            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.PCM;
            settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings;

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
            return true;
        }

        private void PopulateClips(ChromaticAudioBank bank)
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { SAMPLES_FOLDER_PATH });
            AudioClip[] sortedClips = new AudioClip[ChromaticAudioBank.TOTAL_NOTES];
            int assignedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip != null && AssignClipToIndex(clip, sortedClips))
                {
                    assignedCount++;
                }
            }

            bank.SetClips(sortedClips);
            EditorUtility.SetDirty(bank);
            AssetDatabase.SaveAssets();
            Debug.Log($"ChromaticAudioBank: Populated {assignedCount}/{ChromaticAudioBank.TOTAL_NOTES} clips.");
        }

        private bool AssignClipToIndex(AudioClip clip, AudioClip[] targetArray)
        {
            int index = ParseNoteIndex(clip.name);
            if (index >= 0 && index < targetArray.Length)
            {
                targetArray[index] = clip;
                return true;
            }
            Debug.LogWarning($"ChromaticAudioBank: Out of range clip '{clip.name}' (index {index})");
            return false;
        }

        private int ParseNoteIndex(string clipName)
        {
            Match match = Regex.Match(clipName, @"^([A-Za-z]+)(\d)");
            if (!match.Success) return -1;

            string noteName = match.Groups[1].Value;
            int octave = int.Parse(match.Groups[2].Value);
            int semitone = GetNoteSemitone(noteName);
            if (semitone < 0) return -1;

            const int baseNoteSemitone = 26; // MIDI D2 = 26
            const int semitonesPerOctave = 12;
            return (octave * semitonesPerOctave + semitone) - baseNoteSemitone;
        }

        private int GetNoteSemitone(string noteName)
        {
            switch (noteName.ToLowerInvariant())
            {
                case "c": return 0;
                case "csharp": return 1;
                case "d": return 2;
                case "dsharp": return 3;
                case "e": return 4;
                case "f": return 5;
                case "fsharp": return 6;
                case "g": return 7;
                case "gsharp": return 8;
                case "a": return 9;
                case "asharp": return 10;
                case "b": return 11;
                default: return -1;
            }
        }
    }
}
