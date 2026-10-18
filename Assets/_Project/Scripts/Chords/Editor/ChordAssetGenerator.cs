using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Chordyceps.Chords;

namespace Chordyceps.Editor
{
    public static class ChordAssetGenerator
    {
        private const string CHORDS_PATH = "Assets/_Project/ScriptableObjects/Chords/Definitions";
        private const string SCALES_PATH = "Assets/_Project/ScriptableObjects/Chords/Scales";

        private struct ChordSeed
        {
            public string Name;
            public bool IsMajor;
            public int[] Frets;

            public ChordSeed(string name, bool isMajor, int[] frets)
            {
                Name = name;
                IsMajor = isMajor;
                Frets = frets;
            }
        }

        private struct ScaleMapping
        {
            public string Name;
            public string AssetName;
            public int CircleIndex;
            public string[] ChordNames;

            public ScaleMapping(string name, string assetName, int circleIndex, string[] chordNames)
            {
                Name = name;
                AssetName = assetName;
                CircleIndex = circleIndex;
                ChordNames = chordNames;
            }
        }

        [MenuItem("Tools/Chordyceps/Generate 12 Keys")]
        [MenuItem("Tools/Chordyceps/Generate 12")]
        public static void GenerateAllKeys()
        {
            EnsureDirectoryExists(CHORDS_PATH);
            EnsureDirectoryExists(SCALES_PATH);

            var chordLookup = GenerateUniqueChords();
            var scaleLookup = GenerateScaleAssets(chordLookup);
            UpdateSceneChordManager(scaleLookup);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ChordSystemValidation.RunValidation();
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static Dictionary<string, ChordDefinition> GenerateUniqueChords()
        {
            var lookup = new Dictionary<string, ChordDefinition>();
            var expectedPaths = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            ChordSeed[] seeds = BuildChordSeeds();

            foreach (ChordSeed seed in seeds)
            {
                string assetPath = $"{CHORDS_PATH}/Chord_{seed.Name}.asset";
                expectedPaths.Add(assetPath);
                lookup[seed.Name] = CreateOrUpdateChord(assetPath, seed);
            }

            CleanupExtraneousAssets(CHORDS_PATH, expectedPaths);
            return lookup;
        }

        private static ChordDefinition CreateOrUpdateChord(string assetPath, ChordSeed seed)
        {
            var asset = AssetDatabase.LoadAssetAtPath<ChordDefinition>(assetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<ChordDefinition>();
                AssetDatabase.CreateAsset(asset, assetPath);
            }

            asset.Initialize(seed.Name, seed.IsMajor, seed.Frets);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static Dictionary<string, ScaleDefinition> GenerateScaleAssets(Dictionary<string, ChordDefinition> chordLookup)
        {
            var scaleLookup = new Dictionary<string, ScaleDefinition>();
            var expectedPaths = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            ScaleMapping[] mappings = BuildScaleMappings();

            foreach (ScaleMapping map in mappings)
            {
                string assetPath = $"{SCALES_PATH}/Scale_{map.AssetName}.asset";
                expectedPaths.Add(assetPath);
                scaleLookup[map.AssetName] = CreateOrUpdateScale(assetPath, map, chordLookup);
            }

            CleanupExtraneousAssets(SCALES_PATH, expectedPaths);
            return scaleLookup;
        }

        private static ScaleDefinition CreateOrUpdateScale(string path, ScaleMapping map, Dictionary<string, ChordDefinition> lookup)
        {
            var asset = AssetDatabase.LoadAssetAtPath<ScaleDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<ScaleDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var chords = new ChordDefinition[ScaleDefinition.DIATONIC_CHORD_COUNT];
            for (int i = 0; i < map.ChordNames.Length; i++)
            {
                chords[i] = lookup[map.ChordNames[i]];
            }

            const int TONIC_INDEX = 1;
            asset.Initialize(map.Name, map.CircleIndex, TONIC_INDEX, chords);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void CleanupExtraneousAssets(string folderPath, HashSet<string> expectedPaths)
        {
            string[] existingFiles = Directory.GetFiles(folderPath, "*.asset", SearchOption.TopDirectoryOnly);
            foreach (string file in existingFiles)
            {
                string normalized = file.Replace('\\', '/');
                if (!expectedPaths.Contains(normalized))
                {
                    AssetDatabase.DeleteAsset(normalized);
                }
            }
        }

        private static void UpdateSceneChordManager(Dictionary<string, ScaleDefinition> scaleLookup)
        {
            var chordManager = Object.FindFirstObjectByType<ChordManager>();
            if (chordManager == null)
            {
                return;
            }

            ScaleMapping[] mappings = BuildScaleMappings();
            var scales = new ScaleDefinition[mappings.Length];
            for (int i = 0; i < mappings.Length; i++)
            {
                scales[i] = scaleLookup[mappings[i].AssetName];
            }

            chordManager.SetAllScales(scales);
            EditorUtility.SetDirty(chordManager);
            EditorSceneManager.MarkSceneDirty(chordManager.gameObject.scene);
            EditorSceneManager.SaveScene(chordManager.gameObject.scene);
        }

        private static ChordSeed[] BuildChordSeeds()
        {
            ChordSeed[] majorSeeds = BuildMajorSeeds();
            ChordSeed[] minorSeeds = BuildMinorSeeds();
            var allSeeds = new ChordSeed[majorSeeds.Length + minorSeeds.Length];
            majorSeeds.CopyTo(allSeeds, 0);
            minorSeeds.CopyTo(allSeeds, majorSeeds.Length);
            return allSeeds;
        }

        private static ChordSeed[] BuildMajorSeeds()
        {
            return new ChordSeed[]
            {
                new ChordSeed("C", true, new int[] { -1, 3, 2, 0, 1, 0 }),
                new ChordSeed("G", true, new int[] { 3, 2, 0, 0, 0, 3 }),
                new ChordSeed("D", true, new int[] { -1, -1, 0, 2, 3, 2 }),
                new ChordSeed("A", true, new int[] { -1, 0, 2, 2, 2, 0 }),
                new ChordSeed("E", true, new int[] { 0, 2, 2, 1, 0, 0 }),
                new ChordSeed("B", true, new int[] { -1, 2, 4, 4, 4, 2 }),
                new ChordSeed("F#", true, new int[] { 2, 4, 4, 3, 2, 2 }),
                new ChordSeed("Db", true, new int[] { -1, 4, 6, 6, 6, 4 }),
                new ChordSeed("Ab", true, new int[] { 4, 6, 6, 5, 4, 4 }),
                new ChordSeed("Eb", true, new int[] { -1, 6, 8, 8, 8, 6 }),
                new ChordSeed("Bb", true, new int[] { -1, 1, 3, 3, 3, 1 }),
                new ChordSeed("F", true, new int[] { 1, 3, 3, 2, 1, 1 })
            };
        }

        private static ChordSeed[] BuildMinorSeeds()
        {
            return new ChordSeed[]
            {
                new ChordSeed("Am", false, new int[] { -1, 0, 2, 2, 1, 0 }),
                new ChordSeed("Em", false, new int[] { 0, 2, 2, 0, 0, 0 }),
                new ChordSeed("Bm", false, new int[] { -1, 2, 4, 4, 3, 2 }),
                new ChordSeed("F#m", false, new int[] { 2, 4, 4, 2, 2, 2 }),
                new ChordSeed("C#m", false, new int[] { -1, 4, 6, 6, 5, 4 }),
                new ChordSeed("G#m", false, new int[] { 4, 6, 6, 4, 4, 4 }),
                new ChordSeed("D#m", false, new int[] { -1, 6, 8, 8, 7, 6 }),
                new ChordSeed("Bbm", false, new int[] { -1, 1, 3, 3, 2, 1 }),
                new ChordSeed("Fm", false, new int[] { 1, 3, 3, 1, 1, 1 }),
                new ChordSeed("Cm", false, new int[] { -1, 3, 5, 5, 4, 3 }),
                new ChordSeed("Gm", false, new int[] { 3, 5, 5, 3, 3, 3 }),
                new ChordSeed("Dm", false, new int[] { -1, -1, 0, 2, 3, 1 })
            };
        }

        private static ScaleMapping[] BuildScaleMappings()
        {
            return new ScaleMapping[]
            {
                new ScaleMapping("C Major", "C_Major", 0, new string[] { "F", "C", "G", "Em", "Am", "Dm" }),
                new ScaleMapping("G Major", "G_Major", 1, new string[] { "C", "G", "D", "Bm", "Em", "Am" }),
                new ScaleMapping("D Major", "D_Major", 2, new string[] { "G", "D", "A", "F#m", "Bm", "Em" }),
                new ScaleMapping("A Major", "A_Major", 3, new string[] { "D", "A", "E", "C#m", "F#m", "Bm" }),
                new ScaleMapping("E Major", "E_Major", 4, new string[] { "A", "E", "B", "G#m", "C#m", "F#m" }),
                new ScaleMapping("B Major", "B_Major", 5, new string[] { "E", "B", "F#", "D#m", "G#m", "C#m" }),
                new ScaleMapping("F# Major", "Fs_Major", 6, new string[] { "B", "F#", "Db", "Bbm", "D#m", "G#m" }),
                new ScaleMapping("Db Major", "Db_Major", 7, new string[] { "F#", "Db", "Ab", "Fm", "Bbm", "D#m" }),
                new ScaleMapping("Ab Major", "Ab_Major", 8, new string[] { "Db", "Ab", "Eb", "Cm", "Fm", "Bbm" }),
                new ScaleMapping("Eb Major", "Eb_Major", 9, new string[] { "Ab", "Eb", "Bb", "Gm", "Cm", "Fm" }),
                new ScaleMapping("Bb Major", "Bb_Major", 10, new string[] { "Eb", "Bb", "F", "Dm", "Gm", "Cm" }),
                new ScaleMapping("F Major", "F_Major", 11, new string[] { "Bb", "F", "C", "Am", "Dm", "Gm" })
            };
        }
    }
}
