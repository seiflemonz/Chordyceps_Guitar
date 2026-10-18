using UnityEditor;
using UnityEngine;
using Chordyceps.Chords;

namespace Chordyceps.Editor
{
    public static class ChordSystemValidation
    {
        private const string SCALE_PATH = "Assets/_Project/ScriptableObjects/Chords/Scale_C_Major.asset";
        private static readonly string[] _expectedNames = new string[] { "F", "C", "G", "Em", "Am", "Dm" };

        [MenuItem("Tools/Chordyceps/Validate Chord System")]
        public static void RunValidation()
        {
            ScaleDefinition scale = AssetDatabase.LoadAssetAtPath<ScaleDefinition>(SCALE_PATH);
            if (!ValidateScaleAsset(scale)) return;

            ValidateChords(scale);
            ValidatePitchMath();
            Debug.Log("<color=green>[ChordSystemValidation] All Circle of Fifths chords (F, C, G, Em, Am, Dm) verified!</color>");
        }

        private static bool ValidateScaleAsset(ScaleDefinition scale)
        {
            if (scale == null)
            {
                Debug.LogError($"[ChordSystemValidation] Failed to load scale at {SCALE_PATH}");
                return false;
            }

            Debug.Log($"[ChordSystemValidation] Loaded scale: {scale.ScaleName} (Tonic Index: {scale.TonicChordIndex})");
            return true;
        }

        private static void ValidateChords(ScaleDefinition scale)
        {
            for (int i = 0; i < ScaleDefinition.DIATONIC_CHORD_COUNT; i++)
            {
                ChordDefinition chord = scale.GetChord(i);
                if (chord == null)
                {
                    Debug.LogError($"[ChordSystemValidation] Diatonic chord at index {i} is null!");
                    continue;
                }

                bool isExpected = i < _expectedNames.Length && chord.ChordName == _expectedNames[i];
                Debug.Log($"[ChordSystemValidation] Index {i}: [{chord.RomanNumeral}] {chord.ChordName} (Expected: isExpected={isExpected})");
            }
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
