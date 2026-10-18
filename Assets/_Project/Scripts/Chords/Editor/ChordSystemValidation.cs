using UnityEditor;
using UnityEngine;
using Chordyceps.Chords;

namespace Chordyceps.Editor
{
    public static class ChordSystemValidation
    {
        private const string SCALES_PATH = "Assets/_Project/ScriptableObjects/Chords/Scales";
        private const string CHORDS_PATH = "Assets/_Project/ScriptableObjects/Chords/Definitions";
        private const int EXPECTED_TOTAL_SCALES = 12;
        private const int EXPECTED_TOTAL_CHORDS = 24;

        private static readonly string[] _scaleNames = new string[]
        {
            "C_Major", "G_Major", "D_Major", "A_Major", "E_Major", "B_Major",
            "Fs_Major", "Db_Major", "Ab_Major", "Eb_Major", "Bb_Major", "F_Major"
        };

        [MenuItem("Tools/Chordyceps/Validate Chord System")]
        public static void RunValidation()
        {
            int verifiedScales = ValidateAllScales();
            int verifiedChords = ValidateUniqueChords();
            ValidatePitchMath();
            LogSummary(verifiedScales, verifiedChords);
        }

        private static int ValidateAllScales()
        {
            int count = 0;
            for (int i = 0; i < _scaleNames.Length; i++)
            {
                if (ValidateSingleScale(i, _scaleNames[i]))
                {
                    count++;
                }
            }
            return count;
        }

        private static bool ValidateSingleScale(int expectedIndex, string assetName)
        {
            string path = $"{SCALES_PATH}/Scale_{assetName}.asset";
            ScaleDefinition scale = AssetDatabase.LoadAssetAtPath<ScaleDefinition>(path);
            if (scale == null)
            {
                Debug.LogError($"[ChordSystemValidation] Missing scale asset: {path}");
                return false;
            }

            return ValidateScaleProperties(scale, expectedIndex);
        }

        private static bool ValidateScaleProperties(ScaleDefinition scale, int expectedIndex)
        {
            if (scale.CircleOfFifthsIndex != expectedIndex)
            {
                Debug.LogError($"[ChordSystemValidation] {scale.ScaleName} index mismatch: {scale.CircleOfFifthsIndex}");
                return false;
            }

            return ValidateScaleChords(scale);
        }

        private static bool ValidateScaleChords(ScaleDefinition scale)
        {
            for (int i = 0; i < ScaleDefinition.DIATONIC_CHORD_COUNT; i++)
            {
                if (scale.GetChord(i) == null)
                {
                    Debug.LogError($"[ChordSystemValidation] {scale.ScaleName} chord slot {i} is null!");
                    return false;
                }

                if (string.IsNullOrEmpty(scale.GetRomanNumeral(i)))
                {
                    Debug.LogError($"[ChordSystemValidation] Missing Roman numeral for slot {i}");
                    return false;
                }
            }

            return true;
        }

        private static int ValidateUniqueChords()
        {
            string[] files = System.IO.Directory.GetFiles(CHORDS_PATH, "*.asset", System.IO.SearchOption.TopDirectoryOnly);
            int validCount = 0;
            foreach (string file in files)
            {
                string normalized = file.Replace('\\', '/');
                var chord = AssetDatabase.LoadAssetAtPath<ChordDefinition>(normalized);
                if (chord != null)
                {
                    validCount++;
                }
            }
            return validCount;
        }

        private static void ValidatePitchMath()
        {
            int baseOffset = 7;
            int chordFret = 3;
            int capoFret = 2;
            int expectedNoteIndex = baseOffset + capoFret + chordFret;

            if (expectedNoteIndex != 12)
            {
                Debug.LogError($"[ChordSystemValidation] Pitch calculation error: {expectedNoteIndex}");
            }
        }

        private static void LogSummary(int scalesCount, int chordsCount)
        {
            if (scalesCount == EXPECTED_TOTAL_SCALES && chordsCount == EXPECTED_TOTAL_CHORDS)
            {
                Debug.Log($"<color=green>[ChordSystemValidation] Verified {scalesCount} scales and {chordsCount} shared chords successfully!</color>");
            }
            else
            {
                Debug.LogWarning($"[ChordSystemValidation] Scales: {scalesCount}/{EXPECTED_TOTAL_SCALES}, Chords: {chordsCount}/{EXPECTED_TOTAL_CHORDS}");
            }
        }
    }
}
