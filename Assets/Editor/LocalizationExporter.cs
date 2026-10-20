using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;

using HeavenStudio.Common;

namespace HeavenStudio.EditorTools
{
    /// <summary>
    /// Dumps every translatable English string (minigame definitions, scene & prefab UI text) to json,
    /// and reports which ones are missing from a language's tables.
    /// Batch mode: Unity -batchmode -quit -executeMethod HeavenStudio.EditorTools.LocalizationExporter.ExportBatch [-locOut path]
    /// </summary>
    public static class LocalizationExporter
    {
        static readonly string[] SceneFolders = { "Assets/Scenes" };
        static readonly string[] PrefabFolders = { "Assets/Prefabs", "Assets/Resources/Prefabs" };

        [MenuItem("Heaven Studio/Localization/Export Source Strings")]
        public static void ExportMenu()
        {
            string path = Export("Localization/source_strings.json");
            EditorUtility.RevealInFinder(path);
        }

        public static void ExportBatch()
        {
            string[] args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, "-locOut");
            string path = i >= 0 && i + 1 < args.Length ? args[i + 1] : "Localization/source_strings.json";
            Export(path);
        }

        static string Export(string path)
        {
            JObject root = new()
            {
                ["minigames"] = ExportMinigames(out JObject enums),
                ["enums"] = enums,
                ["ui"] = ExportUI(),
            };

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            File.WriteAllText(path, root.ToString(Formatting.Indented));
            Debug.Log($"Exported localization source strings to {Path.GetFullPath(path)}");
            return path;
        }

        static JArray ExportMinigames(out JObject enums)
        {
            enums = new JObject();
            GameObject go = new GameObject("LocalizationExporter EventCaller");
            try
            {
                EventCaller caller = go.AddComponent<EventCaller>();
                Minigames.Init(caller);

                JArray games = new();
                foreach (var game in caller.minigames.Values)
                {
                    JArray actions = new();
                    foreach (var action in game.actions ?? new())
                    {
                        JArray parameters = new();
                        foreach (var param in action.parameters ?? new())
                        {
                            JObject p = new()
                            {
                                ["name"] = param.propertyName,
                                ["caption"] = param.englishCaption ?? param.caption,
                                ["tooltip"] = param.englishTooltip ?? param.tooltip,
                            };
                            Type enumType = param.parameter switch
                            {
                                Enum e => e.GetType(),
                                EntityTypes.NoteSampleDropdown n => n.defaultValue?.GetType(),
                                _ => null,
                            };
                            if (enumType != null && enumType.IsEnum)
                            {
                                p["enum"] = enumType.Name;
                                enums[enumType.Name] = new JArray(Enum.GetNames(enumType));
                            }
                            else if (param.parameter is EntityTypes.Dropdown dropdown)
                            {
                                p["dropdown"] = new JArray(dropdown.values ?? new List<string>());
                            }
                            parameters.Add(p);
                        }
                        actions.Add(new JObject
                        {
                            ["name"] = action.actionName,
                            ["displayName"] = action.englishDisplayName ?? action.displayName,
                            ["tab"] = action.englishTabName ?? action.tabName,
                            ["hidden"] = action.hidden,
                            ["params"] = parameters,
                        });
                    }
                    games.Add(new JObject
                    {
                        ["name"] = game.name,
                        ["displayName"] = game.englishDisplayName ?? game.displayName,
                        ["tags"] = new JArray(game.tags),
                        ["plusGame"] = game.plusGame,
                        ["actions"] = actions,
                    });
                }
                return games;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [MenuItem("Heaven Studio/Localization/Check Japanese Coverage")]
        public static void CheckMenu()
        {
            Debug.Log(Check("ja"));
        }

        /// <summary>
        /// Batch mode: -executeMethod HeavenStudio.EditorTools.LocalizationExporter.CheckBatch [-locLang ja] [-locOut report.txt]
        /// </summary>
        public static void CheckBatch()
        {
            string[] args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, "-locLang");
            string lang = i >= 0 && i + 1 < args.Length ? args[i + 1] : "ja";
            int o = Array.IndexOf(args, "-locOut");
            string report = Check(lang);
            if (o >= 0 && o + 1 < args.Length) File.WriteAllText(args[o + 1], report);
            Debug.Log(report);
        }

        // loads a language and applies it to the minigame definitions, reporting what is still in english
        static string Check(string lang)
        {
            Localization.Load(lang);
            GameObject go = new GameObject("LocalizationExporter EventCaller");
            try
            {
                EventCaller caller = go.AddComponent<EventCaller>();
                Minigames.Init(caller);

                int games = 0, gamesDone = 0, actions = 0, actionsDone = 0, captions = 0, captionsDone = 0, tooltips = 0, tooltipsDone = 0;
                List<string> untranslated = new();
                foreach (var game in caller.minigames.Values)
                {
                    games++;
                    if (game.displayName != game.englishDisplayName) gamesDone++;
                    else untranslated.Add($"game: {game.name} \"{game.englishDisplayName}\"");
                    foreach (var action in game.actions ?? new())
                    {
                        actions++;
                        if (action.displayName != action.englishDisplayName) actionsDone++;
                        else untranslated.Add($"action: {game.name}/{action.actionName} \"{action.englishDisplayName}\"");
                        foreach (var param in action.parameters ?? new())
                        {
                            captions++;
                            if (param.caption != param.englishCaption) captionsDone++;
                            else untranslated.Add($"caption: {game.name}/{action.actionName}/{param.propertyName} \"{param.englishCaption}\"");
                            if (string.IsNullOrEmpty(param.englishTooltip)) continue;
                            tooltips++;
                            if (param.tooltip != param.englishTooltip) tooltipsDone++;
                            else untranslated.Add($"tooltip: {game.name}/{action.actionName}/{param.propertyName} \"{param.englishTooltip}\"");
                        }
                    }
                }

                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Localization check ({Localization.CurrentLanguage})");
                sb.AppendLine($"games    {gamesDone}/{games}");
                sb.AppendLine($"actions  {actionsDone}/{actions}");
                sb.AppendLine($"captions {captionsDone}/{captions}");
                sb.AppendLine($"tooltips {tooltipsDone}/{tooltips}");
                sb.AppendLine($"sample: karateman = {caller.minigames["karateman"].displayName}, " +
                    $"ease = {string.Join(",", Localization.EnumNames(typeof(Util.EasingFunction.Ease)).Take(3))}, " +
                    $"ui = {Localization.T("Editor Settings")} / {Localization.F("Playback Speed: {0}x", 1.5f)}");
                sb.AppendLine("untranslated:");
                foreach (string line in untranslated) sb.AppendLine("  " + line);
                return sb.ToString();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                Localization.Load(Localization.SourceLanguage);
            }
        }

        static JArray ExportUI()
        {
            JArray ui = new();
            HashSet<string> seen = new();

            void Add(string file, string kind, string text)
            {
                if (string.IsNullOrWhiteSpace(text)) return;
                if (!seen.Add(file + "\u0000" + text)) return;
                ui.Add(new JObject { ["file"] = file, ["kind"] = kind, ["text"] = text });
            }

            void Collect(string file, IEnumerable<GameObject> roots)
            {
                foreach (GameObject root in roots)
                {
                    HashSet<TMP_Text> inputTexts = new();
                    foreach (var input in root.GetComponentsInChildren<TMP_InputField>(true))
                        if (input.textComponent != null) inputTexts.Add(input.textComponent);
                    foreach (var dropdown in root.GetComponentsInChildren<TMP_Dropdown>(true))
                    {
                        if (dropdown.captionText != null) inputTexts.Add(dropdown.captionText);
                        foreach (var option in dropdown.options) Add(file, "dropdown", option.text);
                    }
                    foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
                    {
                        if (!inputTexts.Contains(text)) Add(file, "text", text.text);
                    }
                    foreach (var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
                    {
                        if (behaviour == null) continue;
                        SerializedObject so = new SerializedObject(behaviour);
                        SerializedProperty prop = so.GetIterator();
                        while (prop.Next(true))
                        {
                            if (prop.propertyType != SerializedPropertyType.String) continue;
                            if (prop.name == "TooltipText")
                                Add(file, "tooltip", prop.stringValue);
                            else if (prop.name == "hint")
                                Add(file, "hint", prop.stringValue);
                            else if (prop.name == "label" && behaviour.GetType().Name == "ChartInfoProperties")
                                Add(file, "property", prop.stringValue);
                            else if (prop.propertyPath.StartsWith("tabs.Array.data[") && prop.name == "name")
                                Add(file, "tab", prop.stringValue);
                        }
                    }
                }
            }

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", PrefabFolders))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null) Collect(path, new[] { prefab });
            }

            foreach (string guid in AssetDatabase.FindAssets("t:Scene", SceneFolders))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                Collect(path, scene.GetRootGameObjects());
            }

            return ui;
        }
    }
}
