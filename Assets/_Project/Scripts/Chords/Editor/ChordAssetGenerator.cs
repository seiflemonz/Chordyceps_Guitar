using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Chordyceps.Chords;

namespace Chordyceps.Editor
{
    public static class ChordAssetGenerator
    {
        private const string OUTPUT_ROOT = "Assets/_Project/ScriptableObjects/Chords/Keys";
        private const int DIATONIC_COUNT = 6;
        private const int TONIC_INDEX = 1;

        private struct KeyInfo
        {
            public string folder;
            public string displayName;
            public string[] chordNames;
        }

        private static readonly KeyInfo[] _allKeys = new KeyInfo[]
        {
            new KeyInfo { folder = "C_Major", displayName = "C Major", chordNames = new[] { "F", "C", "G", "Em", "Am", "Dm" } },
            new KeyInfo { folder = "G_Major", displayName = "G Major", chordNames = new[] { "C", "G", "D", "Bm", "Em", "Am" } },
            new KeyInfo { folder = "D_Major", displayName = "D Major", chordNames = new[] { "G", "D", "A", "F#m", "Bm", "Em" } },
            new KeyInfo { folder = "A_Major", displayName = "A Major", chordNames = new[] { "D", "A", "E", "C#m", "F#m", "Bm" } },
            new KeyInfo { folder = "E_Major", displayName = "E Major", chordNames = new[] { "A", "E", "B", "G#m", "C#m", "F#m" } },
            new KeyInfo { folder = "B_Major", displayName = "B Major", chordNames = new[] { "E", "B", "F#", "D#m", "G#m", "C#m" } },
            new KeyInfo { folder = "Fs_Major", displayName = "F# Major", chordNames = new[] { "B", "F#", "C#", "A#m", "D#m", "G#m" } },
            new KeyInfo { folder = "Db_Major", displayName = "Db Major", chordNames = new[] { "Gb", "Db", "Ab", "Fm", "Bbm", "Ebm" } },
            new KeyInfo { folder = "Ab_Major", displayName = "Ab Major", chordNames = new[] { "Db", "Ab", "Eb", "Cm", "Fm", "Bbm" } },
            new KeyInfo { folder = "Eb_Major", displayName = "Eb Major", chordNames = new[] { "Ab", "Eb", "Bb", "Gm", "Cm", "Fm" } },
            new KeyInfo { folder = "Bb_Major", displayName = "Bb Major", chordNames = new[] { "Eb", "Bb", "F", "Dm", "Gm", "Cm" } },
            new KeyInfo { folder = "F_Major", displayName = "F Major", chordNames = new[] { "Bb", "F", "C", "Am", "Dm", "Gm" } }
        };

        private static readonly string[] _romanNumerals = new[] { "IV", "I", "V", "iii", "vi", "ii" };
        private static readonly bool[] _isMajorFlags = new[] { true, true, true, false, false, false };

        private static readonly Dictionary<string, int[]> _fretVoicings = new Dictionary<string, int[]>
        {
            { "C",   new[] { -1,  3,  2,  0,  1,  0 } },
            { "G",   new[] {  3,  2,  0,  0,  0,  3 } },
            { "D",   new[] { -1, -1,  0,  2,  3,  2 } },
            { "A",   new[] { -1,  0,  2,  2,  2,  0 } },
            { "E",   new[] {  0,  2,  2,  1,  0,  0 } },
            { "B",   new[] { -1,  2,  4,  4,  4,  2 } },
            { "F#",  new[] {  2,  4,  4,  3,  2,  2 } },
            { "Gb",  new[] {  2,  4,  4,  3,  2,  2 } },
            { "Db",  new[] { -1,  4,  6,  6,  6,  4 } },
            { "C#",  new[] { -1,  4,  6,  6,  6,  4 } },
            { "Ab",  new[] {  4,  6,  6,  5,  4,  4 } },
            { "Eb",  new[] { -1,  6,  8,  8,  8,  6 } },
            { "Bb",  new[] { -1,  1,  3,  3,  3,  1 } },
            { "F",   new[] {  1,  3,  3,  2,  1,  1 } },

            { "Am",  new[] { -1,  0,  2,  2,  1,  0 } },
            { "Em",  new[] {  0,  2,  2,  0,  0,  0 } },
            { "Bm",  new[] { -1,  2,  4,  4,  3,  2 } },
            { "F#m", new[] {  2,  4,  4,  2,  2,  2 } },
            { "C#m", new[] { -1,  4,  6,  6,  5,  4 } },
            { "G#m", new[] {  4,  6,  6,  4,  4,  4 } },
            { "D#m", new[] { -1,  6,  8,  8,  7,  6 } },
            { "Ebm", new[] { -1,  6,  8,  8,  7,  6 } },
            { "A#m", new[] { -1,  1,  3,  3,  2,  1 } },
            { "Bbm", new[] { -1,  1,  3,  3,  2,  1 } },
            { "Fm",  new[] {  1,  3,  3,  1,  1,  1 } },
            { "Cm",  new[] { -1,  3,  5,  5,  4,  3 } },
            { "Gm",  new[] {  3,  5,  5,  3,  3,  3 } },
            { "Dm",  new[] { -1, -1,  0,  2,  3,  1 } }
        };

        [MenuItem("Tools/Chordyceps/Generate All Keys")]
        public static void GenerateAllKeys()
        {
            EnsureDirectoryExists(OUTPUT_ROOT);
            for (int keyIdx = 0; keyIdx < _allKeys.Length; keyIdx++)
            {
                GenerateKey(keyIdx);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ChordAssetGenerator] All 12 keys generated.");
        }

        private static void GenerateKey(int keyIdx)
        {
            KeyInfo info = _allKeys[keyIdx];
            string folder = $"{OUTPUT_ROOT}/{info.folder}";
            EnsureDirectoryExists(folder);

            ChordDefinition[] chords = new ChordDefinition[DIATONIC_COUNT];
            for (int slot = 0; slot < DIATONIC_COUNT; slot++)
            {
                chords[slot] = GenerateChord(info, slot, folder);
            }

            GenerateScale(info, keyIdx, chords, folder);
        }

        private static ChordDefinition GenerateChord(KeyInfo info, int slot, string folder)
        {
            string chordName = info.chordNames[slot];
            string assetPath = $"{folder}/Chord_{info.folder}_{slot}.asset";
            ChordDefinition asset = LoadOrCreate<ChordDefinition>(assetPath);

            asset.name = $"Chord_{chordName}";
            asset.SetFretOffsets(GetFretsForChord(chordName));
            SetChordMeta(asset, chordName, _romanNumerals[slot], _isMajorFlags[slot]);

            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static int[] GetFretsForChord(string chordName)
        {
            if (_fretVoicings.TryGetValue(chordName, out int[] frets))
            {
                return frets;
            }
            return new[] { -1, -1, -1, -1, -1, -1 };
        }

        private static void GenerateScale(KeyInfo info, int keyIdx, ChordDefinition[] chords, string folder)
        {
            string assetPath = $"{folder}/Scale_{info.folder}.asset";
            ScaleDefinition scale = LoadOrCreate<ScaleDefinition>(assetPath);

            scale.name = $"Scale_{info.folder}";
            SetScaleMeta(scale, info.displayName, keyIdx, chords);

            EditorUtility.SetDirty(scale);
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;

            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void SetChordMeta(ChordDefinition chord, string name, string roman, bool isMajor)
        {
            SerializedObject so = new SerializedObject(chord);
            so.FindProperty("_chordName").stringValue = name;
            so.FindProperty("_romanNumeral").stringValue = roman;
            so.FindProperty("_isMajor").boolValue = isMajor;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetScaleMeta(ScaleDefinition scale, string displayName, int index, ChordDefinition[] chords)
        {
            SerializedObject so = new SerializedObject(scale);
            so.FindProperty("_scaleName").stringValue = displayName;
            so.FindProperty("_circleOfFifthsIndex").intValue = index;
            so.FindProperty("_tonicChordIndex").intValue = TONIC_INDEX;

            SerializedProperty chordsArray = so.FindProperty("_diatonicChords");
            chordsArray.arraySize = chords.Length;
            for (int i = 0; i < chords.Length; i++)
            {
                chordsArray.GetArrayElementAtIndex(i).objectReferenceValue = chords[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace('\\', '/');
                string folder = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }
    }
}
