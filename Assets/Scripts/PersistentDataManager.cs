using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace HeavenStudio.Common
{
    public static class PersistentDataManager
    {
        [NonSerialized] public static GameSettings gameSettings;

        // the Japanese edition keeps its levels apart from the english Heaven Studio's "Heaven Studio Levels" folder
        public const string LevelsFolderName = "天国スタジオ レベル";

        public static void LoadSettings(bool reset = false)
        {
            string settingsPath = Application.persistentDataPath + "/settings.json";
            GameSettings newSettings = new();

            if (File.Exists(settingsPath) && !reset)
            {
                string json = File.ReadAllText(settingsPath);
                if (json == "")
                {
                    UnityEngine.Debug.Log("Settings do not exist, creating new game settings");
                }
                else
                {
                    try
                    {
                        newSettings = JsonConvert.DeserializeObject<GameSettings>(json);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError($"Error while loading settings, creating default settings;\n{e.Message}");
                        GlobalGameManager.IsFirstBoot = true;
                        newSettings = PopulateDefaults(newSettings);
                    }
                }
            }
            else
            {
                GlobalGameManager.IsFirstBoot = true;
                newSettings = PopulateDefaults(newSettings);
            }

            gameSettings = newSettings;
        }

        private static GameSettings PopulateDefaults(GameSettings settings)
        {
            // List defaults are here instead of in the class since otherwise they get re-added each time the settings are loaded
            settings.enabledTips = new() { HintGiver.HintTypes.General, HintGiver.HintTypes.Play, HintGiver.HintTypes.Editor, HintGiver.HintTypes.Silly, HintGiver.HintTypes.Fact };
            settings.timingDisplayComponents = new() { OverlaysManager.TimingDisplayComponent.CreateDefaultSingle() };
            settings.skillStarComponents = new() { OverlaysManager.SkillStarComponent.CreateDefault() };
            settings.sectionComponents = new() { OverlaysManager.SectionComponent.CreateDefault() };

            settings.libLevelsDirectory = LibraryManager.SetDefaultLibraryPath();
            return settings;
        }

        public static void SaveSettings()
        {
            string json = JsonConvert.SerializeObject(gameSettings, Formatting.Indented);
            // save json to persistentDataPath
            FileStream file = File.Create(Application.persistentDataPath + "/settings.json");
            file.Write(System.Text.Encoding.UTF8.GetBytes(json));
            file.Close();
        }

        public static void SaveTheme(string json)
        {
            // save json to persistentDataPath
            FileStream file = File.Create(Application.persistentDataPath + "/editorTheme.json");
            file.Write(System.Text.Encoding.ASCII.GetBytes(json), 0, json.Length);
            file.Close();
        }

        [Serializable]
        public class GameSettings
        {
            // General Settings
            public bool showSplash = false;
            public bool showLoadingTips = true;
            public List<HintGiver.HintTypes> enabledTips = new(){};
            public float masterVolume = 0.8f;
            public int dspSize = 340;
            public int sampleRate = 48000;
            public bool forceFallbackFileBrowser = false;
            public float musicVolume = 1f;
            public float sfxVolume = 1f;

            // Graphics Settings
            public int resolutionIndex = 0;
            public int resolutionWidth = 1280;
            public int resolutionHeight = 720;
            public bool isFullscreen = false;
            public bool limitFramerate = true;
            public int maxFps = 120;
            public bool vSync = false;
            public float resScale = 1f;
            public bool disableVfx = false;

            // Editor Settings
            public bool editorCursorEnable = true;
            public bool discordRPCEnable = false;
            public int editorScale = 0;
            public bool scaleWScreenSize = false;
            public int showParamTooltips = 1;
            public bool previewNoteSounds = true;
            public bool previewRichText = true;
            public bool simpleIcons = false;
            public bool showFanGames = true;
            public int fontType = 0;
            public bool hasOpenedEditor = false;
            public bool advancedFeatures = false;
            public string sortMode = "alphabetical";
            public bool autosaveSetToDefault = true;
            public int autosaveInterval = 5;
            public bool editorMinigameColorEvents = false;

            public DateTime lastManualSave;
            public DateTime lastAutoSave;
            public string lastAutoSavePath = "";

            // Library Settings
            public bool hasOpenedLibrary = false;
            public bool libSortAscend = true;
            public LibraryManager.LibrarySortType libSortType = LibraryManager.LibrarySortType.Filename;
            public string libLevelsDirectory = "";
            public int libZoomLevel = 3;
            public int lastLibraryOpenedBuild = 0;

            // Gameplay Settings
            public bool isMedalOn = true;
            public bool timingDisplayMinMode = false;
            public bool overlaysInEditor = true;
            public bool letterboxBgEnable = true;
            public bool letterboxFxEnable = true;
            public bool perfectNoSlam = false;
            public int perfectSfx = 0;
            public List<OverlaysManager.TimingDisplayComponent> timingDisplayComponents = new List<OverlaysManager.TimingDisplayComponent>();
            public List<OverlaysManager.SkillStarComponent> skillStarComponents = new List<OverlaysManager.SkillStarComponent>();
            public List<OverlaysManager.SectionComponent> sectionComponents = new List<OverlaysManager.SectionComponent>();
        }
    }
}