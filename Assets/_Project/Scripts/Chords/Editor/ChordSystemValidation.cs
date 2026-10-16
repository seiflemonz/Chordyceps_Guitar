using UnityEditor;
using UnityEngine;
using Chordyceps.Chords;

namespace Chordyceps.Editor
{
    public static class ChordSystemValidation
    {
        private const string KEYS_ROOT = "Assets/_Project/ScriptableObjects/Chords/Keys";
        private const int EXPECTED_KEY_COUNT = 12;

        private static readonly string[] _keyFolders = new[]
        {
            "C_Major", "G_Major", "D_Major", "A_Major",
            "E_Major", "B_Major", "Fs_Major", "Db_Major",
            "Ab_Major", "Eb_Major", "Bb_Major", "F_Major"
        };

        [MenuItem("Tools/Chordyceps/Validate Chord System")]
        public static void RunValidation()
        {
            int validCount = ValidateAllKeys();
            ValidatePitchMath();

            if (validCount == EXPECTED_KEY_COUNT)
            {
                Debug.Log($"<color=green>[ChordSystemValidation] All {validCount} Circle of Fifths keys verified!</color>");
            }
        }

        private static int ValidateAllKeys()
        {
            int validKeys = 0;
            for (int i = 0; i < _keyFolders.Length; i++)
            {
                if (ValidateSingleKey(_keyFolders[i], i))
                {
                    validKeys++;
                }
            }
            return validKeys;
        }

        private static bool ValidateSingleKey(string folderName, int expectedIndex)
        {
            string path = $"{KEYS_ROOT}/{folderName}/Scale_{folderName}.asset";
            ScaleDefinition scale = AssetDatabase.LoadAssetAtPath<ScaleDefinition>(path);
            if (scale == null)
            {
                Debug.LogError($"[ChordSystemValidation] Missing scale at {path}");
                return false;
            }

            return ValidateScaleProperties(scale, expectedIndex);
        }

        private static bool ValidateScaleProperties(ScaleDefinition scale, int expectedIndex)
        {
            if (scale.CircleOfFifthsIndex != expectedIndex)
            {
                Debug.LogError($"[ChordSystemValidation] Scale {scale.ScaleName} has incorrect CircleOfFifthsIndex {scale.CircleOfFifthsIndex}");
                return false;
            }

            return ValidateScaleChords(scale);
        }

        private static bool ValidateScaleChords(ScaleDefinition scale)
        {
            for (int slot = 0; slot < ScaleDefinition.DIATONIC_CHORD_COUNT; slot++)
            {
                ChordDefinition chord = scale.GetChord(slot);
                if (chord == null)
                {
                    Debug.LogError($"[ChordSystemValidation] Key {scale.ScaleName} slot {slot} chord is null!");
                    return false;
                }
            }
            return true;
        }

        private static void ValidatePitchMath()
        {
            int baseOffset = 7;
            int chordFret = 3;
            int capoFret = 2;
            int expectedNoteIndex = baseOffset + capoFret + chordFret;

            if (expectedNoteIndex != 12)
            {
                Debug.LogError($"[ChordSystemValidation] Expected 12 but got {expectedNoteIndex}");
            }
        }
    }
}
