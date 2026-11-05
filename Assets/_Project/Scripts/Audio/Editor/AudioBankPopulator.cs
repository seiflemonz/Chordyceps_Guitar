using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Chordyceps.Audio.Editor
{
    /// <summary>
    /// Shared editor helper that fills a pitch-ordered audio bank from a folder of
    /// note-named clips and applies low-latency import settings to them.
    /// Note names use scientific pitch (C2, C#2, A#5); index 0 is C2.
    /// </summary>
    internal static class AudioBankPopulator
    {
        public const string AcousticSamplesFolder = "Assets/_Project/Audio/samples(acoustic)-full";
        public const string ElectricSamplesFolder = "Assets/_Project/Audio/samples(electric)-full";

        private const int BASE_NOTE_SEMITONE = 24; // MIDI C2 = 24
        private const int SEMITONES_PER_OCTAVE = 12;

        public static int Populate(AudioClip[] target, string folderPath)
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { folderPath });
            int assignedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip != null && AssignClipToIndex(clip, target))
                {
                    assignedCount++;
                }
            }

            return assignedCount;
        }

        public static void OptimizeAllSampleImports()
        {
            int modifiedCount = OptimizeFolders(AcousticSamplesFolder, ElectricSamplesFolder);
            AssetDatabase.Refresh();
            Debug.Log($"AudioBankPopulator: Optimized {modifiedCount} audio samples for low latency.");
        }

        private static int OptimizeFolders(params string[] folderPaths)
        {
            AssetDatabase.StartAssetEditing();
            try
            {
                int count = 0;
                foreach (string folderPath in folderPaths)
                {
                    count += OptimizeFolder(folderPath);
                }
                return count;
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        private static int OptimizeFolder(string folderPath)
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { folderPath });
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

        private static bool AssignClipToIndex(AudioClip clip, AudioClip[] target)
        {
            int index = ParseNoteIndex(clip.name);
            if (index >= 0 && index < target.Length)
            {
                target[index] = clip;
                return true;
            }

            Debug.LogWarning($"AudioBankPopulator: Out of range clip '{clip.name}' (index {index})");
            return false;
        }

        private static int ParseNoteIndex(string clipName)
        {
            // "C#2" and "Csharp2" must both resolve to the same semitone.
            Match match = Regex.Match(clipName.Replace("#", "sharp"), @"^([A-Za-z]+)(\d)");
            if (!match.Success) return -1;

            string noteName = match.Groups[1].Value;
            int octave = int.Parse(match.Groups[2].Value);
            int semitone = GetNoteSemitone(noteName);
            if (semitone < 0) return -1;

            return (octave * SEMITONES_PER_OCTAVE + semitone) - BASE_NOTE_SEMITONE;
        }

        private static int GetNoteSemitone(string noteName)
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
