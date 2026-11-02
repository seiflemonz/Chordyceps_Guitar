using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chordyceps.Chords
{
    /// <summary>
    /// Player-authored chord data, persisted to PlayerPrefs as JSON.
    ///
    /// Holds two kinds of record:
    ///  - Overrides: replacements for pre-existing chord definitions, keyed by the source
    ///    chord's name. ChordManager applies these game-wide, so an edit made in the Chord
    ///    Maker changes how the main game plays that chord until it is reset to default.
    ///  - Customs: brand-new chords, keyed by a generated id. Used by the Chord Maker only.
    /// </summary>
    public static class CustomChordStore
    {
        private const string PREFS_KEY = "chordyceps.chord_store_v1";

        [Serializable]
        private class Payload
        {
            public List<ChordEntry> Overrides = new List<ChordEntry>();
            public List<ChordEntry> Customs = new List<ChordEntry>();
        }

        private static Payload _payload;
        private static bool _loaded;

        public static IReadOnlyList<ChordEntry> Overrides
        {
            get
            {
                EnsureLoaded();
                return _payload.Overrides;
            }
        }

        public static IReadOnlyList<ChordEntry> Customs
        {
            get
            {
                EnsureLoaded();
                return _payload.Customs;
            }
        }

        public static void EnsureLoaded()
        {
            if (_loaded)
            {
                return;
            }

            _loaded = true;
            string json = PlayerPrefs.GetString(PREFS_KEY, string.Empty);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    _payload = JsonUtility.FromJson<Payload>(json);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"[CustomChordStore] Saved chords could not be read and were ignored: {exception.Message}");
                }
            }

            if (_payload == null)
            {
                _payload = new Payload();
            }

            _payload.Overrides ??= new List<ChordEntry>();
            _payload.Customs ??= new List<ChordEntry>();
            Sanitize(_payload.Overrides);
            Sanitize(_payload.Customs);
        }

        /// <summary>Drops malformed records and forces every fret array to the expected length.</summary>
        private static void Sanitize(List<ChordEntry> entries)
        {
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                ChordEntry entry = entries[i];
                if (entry == null || string.IsNullOrEmpty(entry.Id))
                {
                    entries.RemoveAt(i);
                    continue;
                }

                if (entry.Frets == null || entry.Frets.Length != ChordEntry.STRING_COUNT)
                {
                    var frets = new int[ChordEntry.STRING_COUNT];
                    if (entry.Frets != null)
                    {
                        Array.Copy(entry.Frets, frets, Mathf.Min(entry.Frets.Length, ChordEntry.STRING_COUNT));
                    }

                    entry.Frets = frets;
                }

                if (string.IsNullOrEmpty(entry.Name))
                {
                    entry.Name = entry.Id;
                }
            }
        }

        public static ChordEntry GetOverride(string sourceName)
        {
            return string.IsNullOrEmpty(sourceName) ? null : Find(Overrides, sourceName);
        }

        public static ChordEntry GetCustom(string id)
        {
            return string.IsNullOrEmpty(id) ? null : Find(Customs, id);
        }

        private static ChordEntry Find(IReadOnlyList<ChordEntry> entries, string id)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Id == id)
                {
                    return entries[i];
                }
            }

            return null;
        }

        /// <summary>True when the given name is already used by a stored chord.</summary>
        public static bool IsNameTaken(string name, string ignoreId)
        {
            string trimmed = (name ?? string.Empty).Trim();
            if (trimmed.Length == 0)
            {
                return false;
            }

            return IsNameTakenIn(Overrides, trimmed, ignoreId) || IsNameTakenIn(Customs, trimmed, ignoreId);
        }

        private static bool IsNameTakenIn(IReadOnlyList<ChordEntry> entries, string name, string ignoreId)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                ChordEntry entry = entries[i];
                if (entry.Id == ignoreId)
                {
                    continue;
                }

                if (string.Equals(entry.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Stores the edit of a pre-existing chord, replacing any previous override.</summary>
        public static void SaveOverride(string sourceName, int[] frets, bool isMajor)
        {
            if (string.IsNullOrEmpty(sourceName))
            {
                return;
            }

            EnsureLoaded();
            ChordEntry entry = GetOverride(sourceName);
            if (entry == null)
            {
                entry = new ChordEntry { Id = sourceName, Name = sourceName, IsCustom = false };
                _payload.Overrides.Add(entry);
            }

            entry.Frets = CopyFrets(frets);
            entry.IsMajor = isMajor;
            Persist();
        }

        /// <summary>
        /// Discards the override of a pre-existing chord so it falls back to its asset
        /// default. Used when an edit is saved back to the original shape.
        /// </summary>
        public static bool RemoveOverride(string sourceName)
        {
            EnsureLoaded();
            ChordEntry entry = GetOverride(sourceName);
            if (entry == null)
            {
                return false;
            }

            _payload.Overrides.Remove(entry);
            Persist();
            return true;
        }

        /// <summary>Stores a brand-new chord and returns the created record.</summary>
        public static ChordEntry CreateCustom(string name, int[] frets, bool isMajor)
        {
            EnsureLoaded();
            var entry = new ChordEntry
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = (name ?? string.Empty).Trim(),
                Frets = CopyFrets(frets),
                IsCustom = true,
                IsMajor = isMajor,
            };

            _payload.Customs.Add(entry);
            Persist();
            return entry;
        }

        public static void UpdateCustom(string id, string name, int[] frets)
        {
            EnsureLoaded();
            ChordEntry entry = GetCustom(id);
            if (entry == null)
            {
                return;
            }

            entry.Name = (name ?? string.Empty).Trim();
            entry.Frets = CopyFrets(frets);
            Persist();
        }

        public static bool DeleteCustom(string id)
        {
            EnsureLoaded();
            ChordEntry entry = GetCustom(id);
            if (entry == null)
            {
                return false;
            }

            _payload.Customs.Remove(entry);
            Persist();
            return true;
        }

        /// <summary>Restores every pre-existing chord to its asset default; custom chords are untouched.</summary>
        public static void ResetOverrides()
        {
            EnsureLoaded();
            if (_payload.Overrides.Count == 0)
            {
                return;
            }

            _payload.Overrides.Clear();
            Persist();
        }

        private static int[] CopyFrets(int[] frets)
        {
            var copy = new int[ChordEntry.STRING_COUNT];
            if (frets == null)
            {
                return copy;
            }

            Array.Copy(frets, copy, Mathf.Min(frets.Length, ChordEntry.STRING_COUNT));
            return copy;
        }

        private static void Persist()
        {
            PlayerPrefs.SetString(PREFS_KEY, JsonUtility.ToJson(_payload));
            PlayerPrefs.Save();
        }
    }
}
