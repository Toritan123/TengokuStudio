using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

using Newtonsoft.Json.Linq;

namespace HeavenStudio.Common
{
    /// <summary>
    /// Translates UI and minigame strings. English is the source language: every lookup is keyed by the
    /// original English text (or a context key such as "gameName/actionName"), and falls back to it.
    /// Tables live in Resources/Localization/{lang}/*.json, and can be overridden by placing json files in
    /// {persistentDataPath}/Localization/{lang}/.
    /// games.json   : { "internalGameName": "Name" } or { "internalGameName": { "name": "Name", "sort": "reading" } }
    /// enums.json   : { "EnumMember": "Name", "EnumType.EnumMember": "Name" }
    /// voice_timing.json : { "game/sound": { "beat": 0.5, "offset": 0.01, "volume": 1.2, "mute": false, "extra": [{ "sound": "name", "beat": 0.25 }] } }
    ///                (only applied while the game plays japanese voices, see VoiceTiming)
    /// anything else: { "English text": "Translated text", "game/action": "...", "game/action/param": "...", "game/action/param:tooltip": "..." }
    /// </summary>
    public static class Localization
    {
        public const string SourceLanguage = "en";
        // this build of Heaven Studio is the Japanese edition, so it always runs in Japanese
        public const string DefaultLanguage = "ja";
        // minigame voice asset bundles use "jp" for Japanese (ex. agbmarcher/locale.jp)
        public const string VoiceLocale = "jp";

        /// <summary>
        /// True when a minigame is playing japanese voices. Some japanese cues have a different rhythm
        /// (ex. "a-lley-oop" is "so-re"), so games can schedule their voice lines differently.
        /// </summary>
        public static bool UsesJapaneseVoice(string gameName) => EventCaller.instance?.GetMinigame(gameName)?.AudioLocale == VoiceLocale;

        // Resources/Sfx folders that have a japanese version in a "jp" subfolder
        private static readonly HashSet<string> localizedSfxFolders = new() { "count-ins" };

        /// <summary>
        /// The japanese version of a sound in Resources/Sfx (ex. "count-ins/one1" -> "count-ins/jp/one1"), or the name itself.
        /// Only sounds directly inside a localized folder are redirected, so other count-in sets (count-ins/gba/...) are left alone.
        /// </summary>
        public static string LocalizedSfx(string name)
        {
            if (IsSource || string.IsNullOrEmpty(name)) return name;
            int slash = name.LastIndexOf('/');
            if (slash < 0 || !localizedSfxFolders.Contains(name[..slash])) return name;
            return $"{name[..slash]}/{VoiceLocale}/{name[(slash + 1)..]}";
        }

        /// <summary>
        /// Japanese timing for a game sound ("game/sound"; folders in between are ignored), if it has one
        /// and the game is playing japanese voices.
        /// </summary>
        public static bool TryGetVoiceTiming(string soundName, out VoiceTiming timing)
        {
            timing = default;
            if (voiceTimings.Count == 0 || string.IsNullOrEmpty(soundName)) return false;
            string[] split = soundName.Split('/');
            if (split.Length < 2) return false;
            return voiceTimings.TryGetValue($"{split[0]}/{split[^1]}", out timing) && UsesJapaneseVoice(split[0]);
        }

        public static string CurrentLanguage { get; private set; } = SourceLanguage;
        public static bool IsSource => CurrentLanguage == SourceLanguage;

        private static readonly Dictionary<string, string> strings = new();
        private static readonly Dictionary<string, string> enums = new();
        private static readonly Dictionary<string, string> gameNames = new();
        private static readonly Dictionary<string, string> gameSortKeys = new();
        private static readonly Dictionary<string, VoiceTiming> voiceTimings = new();

        /// <summary>
        /// How a japanese voice clip is moved from where the game schedules the english one.
        /// The japanese cues don't always have the same rhythm (ex. "zen-tai" vs "at-ten-tion"),
        /// so they can be shifted (in beats), nudged (in seconds), made louder or quieter, or muted.
        /// </summary>
        public struct VoiceTiming
        {
            public double beat;
            public double offset;
            public float volume;
            public bool mute;
            // more sounds to play with this one, for japanese lines with more syllables than the english ones
            public List<(string sound, double beat)> extra;
        }

        public static void Init()
        {
            Load(DefaultLanguage);
        }

        /// <summary>Load a language's tables. Passing SourceLanguage turns translation off.</summary>
        public static void Load(string code)
        {
            strings.Clear();
            enums.Clear();
            gameNames.Clear();
            gameSortKeys.Clear();
            voiceTimings.Clear();
            CurrentLanguage = code;
            if (IsSource) return;

            foreach (TextAsset asset in Resources.LoadAll<TextAsset>($"Localization/{CurrentLanguage}"))
            {
                LoadTable(asset.name, asset.text);
            }

            string overrideDir = Path.Combine(Application.persistentDataPath, "Localization", CurrentLanguage);
            if (Directory.Exists(overrideDir))
            {
                foreach (string file in Directory.GetFiles(overrideDir, "*.json"))
                {
                    LoadTable(Path.GetFileNameWithoutExtension(file), File.ReadAllText(file));
                }
            }

            Debug.Log($"Loaded {CurrentLanguage} localization: {strings.Count} strings, {enums.Count} enum names, {gameNames.Count} game names, {voiceTimings.Count} voice timings");
        }

        private static void LoadTable(string tableName, string json)
        {
            JObject table;
            try
            {
                table = JObject.Parse(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse localization table {tableName}: {e.Message}");
                return;
            }

            foreach (var pair in table)
            {
                string key = pair.Key;
                JToken value = pair.Value;
                if (key.StartsWith("//")) continue;
                switch (tableName)
                {
                    case "games":
                        if (value.Type == JTokenType.Object)
                        {
                            string name = (string)value["name"];
                            if (!string.IsNullOrEmpty(name)) gameNames[key] = name;
                            string sort = (string)value["sort"];
                            if (!string.IsNullOrEmpty(sort)) gameSortKeys[key] = sort;
                        }
                        else if (!string.IsNullOrEmpty((string)value))
                        {
                            gameNames[key] = (string)value;
                        }
                        break;
                    case "enums":
                        if (!string.IsNullOrEmpty((string)value)) enums[key] = (string)value;
                        break;
                    case "voice_timing":
                        if (value.Type != JTokenType.Object) break;
                        voiceTimings[key] = new VoiceTiming
                        {
                            beat = (double?)value["beat"] ?? 0,
                            offset = (double?)value["offset"] ?? 0,
                            volume = (float?)value["volume"] ?? 1f,
                            mute = (bool?)value["mute"] ?? false,
                            extra = value["extra"] is JArray extras
                                ? extras.Select(x => ((string)x["sound"], (double?)x["beat"] ?? 0)).ToList()
                                : null,
                        };
                        break;
                    default:
                        if (!string.IsNullOrEmpty((string)value)) strings[Normalize(key)] = (string)value;
                        break;
                }
            }
        }

        private static string Normalize(string s) => s.Replace("\r\n", "\n");

        public static bool TryTranslate(string source, out string result)
        {
            result = source;
            if (string.IsNullOrEmpty(source) || strings.Count == 0) return false;

            string key = Normalize(source);
            if (strings.TryGetValue(key, out string translated))
            {
                result = translated;
                return true;
            }

            // keep surrounding whitespace / newlines the source text might have
            string trimmed = key.Trim();
            if (trimmed.Length != key.Length && trimmed.Length > 0 && strings.TryGetValue(trimmed, out translated))
            {
                int start = key.IndexOf(trimmed, StringComparison.Ordinal);
                result = key[..start] + translated + key[(start + trimmed.Length)..];
                return true;
            }
            return false;
        }

        /// <summary>
        /// Translate a string. Returns the source text if there is no translation.
        /// </summary>
        public static string T(string source) => TryTranslate(source, out string result) ? result : source;

        /// <summary>
        /// Translate a format string, then format it.
        /// </summary>
        public static string F(string format, params object[] args) => string.Format(T(format), args);

        /// <summary>
        /// Translate using a context key first (ex. "gameName/actionName"), then the source text.
        /// </summary>
        public static string TContext(string contextKey, string source)
        {
            if (strings.Count == 0) return source;
            if (contextKey != null && strings.TryGetValue(contextKey, out string translated)) return translated;
            return T(source);
        }

        public static string EnumName(Type enumType, string name)
        {
            if (enums.Count == 0 || name == null) return name;
            if (enums.TryGetValue($"{enumType.Name}.{name}", out string translated)) return translated;
            if (enums.TryGetValue(name, out translated)) return translated;
            return name;
        }

        public static string EnumName(Enum value) => EnumName(value.GetType(), value.ToString());

        /// <summary>
        /// Drop-in replacement for Enum.GetNames that returns translated names, in the same order.
        /// </summary>
        public static List<string> EnumNames(Type enumType) => Enum.GetNames(enumType).Select(n => EnumName(enumType, n)).ToList();

        public static string GameSortKey(Minigames.Minigame game)
        {
            if (gameSortKeys.TryGetValue(game.name, out string sort)) return sort;
            return null;
        }

        public static void ApplyToMinigames(IEnumerable<Minigames.Minigame> games)
        {
            foreach (var game in games)
            {
                if (game == null) continue;
                game.englishDisplayName ??= game.displayName;
                game.displayName = gameNames.TryGetValue(game.name, out string gameName) ? gameName : T(game.englishDisplayName);

                if (game.actions == null) continue;
                foreach (var action in game.actions)
                {
                    if (action == null) continue;
                    string actionKey = $"{game.name}/{action.actionName}";
                    action.englishDisplayName ??= action.displayName;
                    action.englishTabName ??= action.tabName;
                    action.displayName = TContext(actionKey, action.englishDisplayName);
                    action.tabName = action.englishTabName == null ? null : T(action.englishTabName);

                    if (action.parameters == null) continue;
                    foreach (var param in action.parameters)
                    {
                        if (param == null) continue;
                        string paramKey = $"{actionKey}/{param.propertyName}";
                        param.englishCaption ??= param.caption;
                        param.englishTooltip ??= param.tooltip;
                        param.caption = TContext(paramKey, param.englishCaption);
                        param.tooltip = TContext(paramKey + ":tooltip", param.englishTooltip);
                    }
                }
            }
        }
    }
}
