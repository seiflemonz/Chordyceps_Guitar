using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chordyceps.Chords
{
    /// <summary>
    /// Player-authored wheel layouts, persisted to PlayerPrefs as JSON, plus which layout
    /// was last applied to the main game.
    ///
    /// Layouts are stored whole rather than as per-wheel overrides: a layout is authored in
    /// one sitting in the Wheel Maker and only becomes real once it has been named, so there
    /// is never a partially-edited layout to reconcile. The built-in layout is derived from
    /// the scale assets at runtime and is never stored.
    /// </summary>
    public static class CustomWheelStore
    {
        private const string PREFS_KEY = "chordyceps.wheel_store_v1";

        [Serializable]
        private class Payload
        {
            public List<WheelLayoutEntry> Customs = new List<WheelLayoutEntry>();

            /// <summary>Id of the layout applied to the main game; null means the built-in layout.</summary>
            public string ActiveLayoutId;
        }

        private static Payload _payload;
        private static bool _loaded;

        public static IReadOnlyList<WheelLayoutEntry> Customs
        {
            get
            {
                EnsureLoaded();
                return _payload.Customs;
            }
        }

        /// <summary>Id of the applied layout, or null when the built-in layout is in use.</summary>
        public static string ActiveLayoutId
        {
            get
            {
                EnsureLoaded();
                return _payload.ActiveLayoutId;
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
                    Debug.LogWarning($"[CustomWheelStore] Saved wheel layouts could not be read and were ignored: {exception.Message}");
                }
            }

            if (_payload == null)
            {
                _payload = new Payload();
            }

            _payload.Customs ??= new List<WheelLayoutEntry>();
            Sanitize(_payload.Customs);

            // An applied layout that no longer exists would silently strand the main game.
            if (!string.IsNullOrEmpty(_payload.ActiveLayoutId) && Find(_payload.Customs, _payload.ActiveLayoutId) == null)
            {
                _payload.ActiveLayoutId = null;
            }
        }

        /// <summary>
        /// Drops malformed records and forces every layout back into shape: at least one
        /// wheel, at most <see cref="WheelLayoutEntry.MAX_WHEELS"/>, and six slots per wheel.
        /// </summary>
        private static void Sanitize(List<WheelLayoutEntry> layouts)
        {
            for (int i = layouts.Count - 1; i >= 0; i--)
            {
                WheelLayoutEntry layout = layouts[i];
                if (layout == null || string.IsNullOrEmpty(layout.Id))
                {
                    layouts.RemoveAt(i);
                    continue;
                }

                if (string.IsNullOrEmpty(layout.Name))
                {
                    layout.Name = layout.Id;
                }

                layout.Wheels ??= new List<WheelEntry>();
                for (int wheel = layout.Wheels.Count - 1; wheel >= 0; wheel--)
                {
                    if (layout.Wheels[wheel] == null)
                    {
                        layout.Wheels.RemoveAt(wheel);
                    }
                }

                if (layout.Wheels.Count > WheelLayoutEntry.MAX_WHEELS)
                {
                    layout.Wheels.RemoveRange(WheelLayoutEntry.MAX_WHEELS, layout.Wheels.Count - WheelLayoutEntry.MAX_WHEELS);
                }

                if (layout.Wheels.Count == 0)
                {
                    layout.Wheels.Add(new WheelEntry());
                }

                for (int wheel = 0; wheel < layout.Wheels.Count; wheel++)
                {
                    SanitizeWheel(layout.Wheels[wheel]);
                }
            }
        }

        private static void SanitizeWheel(WheelEntry wheel)
        {
            if (wheel.SectorChordIds != null && wheel.SectorChordIds.Length == WheelEntry.SECTOR_COUNT)
            {
                return;
            }

            var slots = new string[WheelEntry.SECTOR_COUNT];
            if (wheel.SectorChordIds != null)
            {
                Array.Copy(wheel.SectorChordIds, slots, Mathf.Min(wheel.SectorChordIds.Length, WheelEntry.SECTOR_COUNT));
            }

            wheel.SectorChordIds = slots;
        }

        public static WheelLayoutEntry GetCustom(string id)
        {
            EnsureLoaded();
            return string.IsNullOrEmpty(id) ? null : Find(_payload.Customs, id);
        }

        private static WheelLayoutEntry Find(IReadOnlyList<WheelLayoutEntry> layouts, string id)
        {
            for (int i = 0; i < layouts.Count; i++)
            {
                if (layouts[i].Id == id)
                {
                    return layouts[i];
                }
            }

            return null;
        }

        /// <summary>True when the given name is already used by a stored layout.</summary>
        public static bool IsNameTaken(string name, string ignoreId)
        {
            EnsureLoaded();
            string trimmed = (name ?? string.Empty).Trim();
            if (trimmed.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < _payload.Customs.Count; i++)
            {
                WheelLayoutEntry layout = _payload.Customs[i];
                if (layout.Id == ignoreId)
                {
                    continue;
                }

                if (string.Equals(layout.Name, trimmed, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Stores a new layout and returns the created record.</summary>
        public static WheelLayoutEntry CreateCustom(string name, IReadOnlyList<WheelEntry> wheels)
        {
            EnsureLoaded();
            var entry = new WheelLayoutEntry
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = (name ?? string.Empty).Trim(),
                IsCustom = true,
                Wheels = CopyWheels(wheels),
            };

            _payload.Customs.Add(entry);
            Persist();
            return entry;
        }

        public static void UpdateCustom(string id, string name, IReadOnlyList<WheelEntry> wheels)
        {
            EnsureLoaded();
            WheelLayoutEntry entry = GetCustom(id);
            if (entry == null)
            {
                return;
            }

            entry.Name = (name ?? string.Empty).Trim();
            entry.Wheels = CopyWheels(wheels);
            Persist();
        }

        public static bool DeleteCustom(string id)
        {
            EnsureLoaded();
            WheelLayoutEntry entry = GetCustom(id);
            if (entry == null)
            {
                return false;
            }

            _payload.Customs.Remove(entry);
            if (_payload.ActiveLayoutId == id)
            {
                _payload.ActiveLayoutId = null;
            }

            Persist();
            return true;
        }

        /// <summary>
        /// Remembers which layout the main game should show. Passing null restores the
        /// built-in, scale-driven layout.
        /// </summary>
        public static void SetActiveLayout(string id)
        {
            EnsureLoaded();
            string resolved = string.IsNullOrEmpty(id) ? null : id;
            if (_payload.ActiveLayoutId == resolved)
            {
                return;
            }

            _payload.ActiveLayoutId = resolved;
            Persist();
        }

        private static List<WheelEntry> CopyWheels(IReadOnlyList<WheelEntry> wheels)
        {
            var copy = new List<WheelEntry>();
            if (wheels == null)
            {
                return copy;
            }

            for (int i = 0; i < wheels.Count && copy.Count < WheelLayoutEntry.MAX_WHEELS; i++)
            {
                if (wheels[i] != null)
                {
                    copy.Add(wheels[i].Clone());
                }
            }

            if (copy.Count == 0)
            {
                copy.Add(new WheelEntry());
            }

            return copy;
        }

        private static void Persist()
        {
            PlayerPrefs.SetString(PREFS_KEY, JsonUtility.ToJson(_payload));
            PlayerPrefs.Save();
        }
    }
}
