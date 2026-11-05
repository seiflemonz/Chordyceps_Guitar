using System;
using UnityEngine;

namespace Chordyceps.Gameplay
{
    /// <summary>
    /// Single owner of the acoustic/electric mode. UI controllers drive it, audio
    /// picks its bank pair from <see cref="IsElectric"/>, and every themed colour
    /// resolves through <see cref="Accent"/> so the whole app changes together.
    /// </summary>
    public static class GuitarModeState
    {
        private const string MODE_PREFS_KEY = "chordyceps.guitar_mode_v1";

        // Acoustic golds.
        private static readonly Color AccentAcoustic = new Color(0.941f, 0.722f, 0.259f, 1f);     // #F0B842
        private static readonly Color AccentSoftAcoustic = new Color(1f, 0.902f, 0.580f, 1f);     // #FFE694

        // Electric deep reds.
        private static readonly Color AccentElectric = new Color(0.702f, 0.157f, 0.176f, 1f);     // #B3282D
        private static readonly Color AccentSoftElectric = new Color(0.910f, 0.514f, 0.478f, 1f); // #E8837A

        private static bool _isElectric;
        private static bool _loaded;

        /// <summary>Raised after <see cref="IsElectric"/> changes.</summary>
        public static event Action Changed;

        /// <summary>
        /// True while electric mode is active. Loaded from the saved preference on first
        /// access so controllers can read it regardless of their initialisation order.
        /// </summary>
        public static bool IsElectric
        {
            get
            {
                EnsureLoaded();
                return _isElectric;
            }
        }

        /// <summary>Primary accent: gold in acoustic mode, deep red in electric.</summary>
        public static Color Accent => IsElectric ? AccentElectric : AccentAcoustic;

        /// <summary>Secondary accent: pale gold in acoustic mode, soft red in electric.</summary>
        public static Color AccentSoft => IsElectric ? AccentSoftElectric : AccentSoftAcoustic;

        /// <summary>Switches modes and remembers the choice.</summary>
        public static void SetElectric(bool isElectric)
        {
            EnsureLoaded();
            if (_isElectric == isElectric)
            {
                return;
            }

            _isElectric = isElectric;
            PlayerPrefs.SetInt(MODE_PREFS_KEY, isElectric ? 1 : 0);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        private static void EnsureLoaded()
        {
            if (_loaded)
            {
                return;
            }

            _loaded = true;
            _isElectric = PlayerPrefs.GetInt(MODE_PREFS_KEY, 0) == 1;
        }
    }
}
