using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HeavenStudio.Common
{
    public static class PersistentDataManager
    {
        public enum PerfectChallengeType
        {
            Off,        // no perfect challenge
            Arcade,     // "arcade rule"
            Legacy,     // "legacy rule"
            On          // "megamix rule"
        }


        [NonSerialized] public static GameSettings gameSettings;

        // the Japanese edition keeps its levels apart from the english Heaven Studio's "Heaven Studio Levels" folder
        public const string LevelsFolderName = "天国スタジオ レベル";

        public static void CreateDefaultSettings()
        {
            gameSettings = new GameSettings(
                true,
                true,
                false,
                1,
                GlobalGameManager.DEFAULT_SCREEN_SIZES[0].width,
                GlobalGameManager.DEFAULT_SCREEN_SIZES[0].height,
                0.8f,
                340,
                48000,
                true,
                true,
                PerfectChallengeType.Off,
                true,
                false,
                true,
                true,
                true,
                0,
                false,
                1,
                true,
                false,
                true,
                0,
                false,
                false,
                false,
                true,
                false,
                false,
                0,
                "alphabetical",
                true,
                120,
                false,
                1f,
                1f,
                1f,
                false,
                false
            );

            // disable if platform is mac
            if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
                gameSettings.discordRPCEnable = false;
            else
                gameSettings.discordRPCEnable = true;

            gameSettings.timingDisplayComponents = new List<OverlaysManager.TimingDisplayComponent>()
            {
                OverlaysManager.TimingDisplayComponent.CreateDefaultSingle()
            };
            gameSettings.skillStarComponents = new List<OverlaysManager.SkillStarComponent>()
            {
                OverlaysManager.SkillStarComponent.CreateDefault()
            };
            gameSettings.sectionComponents = new List<OverlaysManager.SectionComponent>()
            {
                OverlaysManager.SectionComponent.CreateDefault()
            };

            SaveSettings();
        }

        public static void LoadSettings()
        {
            if (File.Exists(Application.persistentDataPath + "/settings.json"))
            {
                string json = File.ReadAllText(Application.persistentDataPath + "/settings.json");
                if (json == "")
                {
                    GlobalGameManager.IsFirstBoot = true;
                    CreateDefaultSettings();
                    return;
                }
                try
                {
                    // settings updater block
                    var jObj = JObject.Parse(json);

                    if (!jObj.ContainsKey("showFanGames"))
                    {
                        jObj["showFanGames"] = true;
                    }

                    if (!jObj.ContainsKey("showLoadingTips"))
                    {
                        jObj["showLoadingTips"] = true;
                        jObj["enabledTips"] = new JArray { HintGiver.HintTypes.General, HintGiver.HintTypes.Play, HintGiver.HintTypes.Editor, HintGiver.HintTypes.Silly, HintGiver.HintTypes.Fact };
                    }

                    if (!jObj.ContainsKey("libZoomLevel"))
                    {
                        jObj["libZoomLevel"] = 3;
                    }

                    if (!jObj.ContainsKey("limitFramerate"))
                    {
                        jObj["limitFramerate"] = true;
                        jObj["maxFps"] = 120;
                    }

                    if (!jObj.ContainsKey("resScale"))
                    {
                        jObj["resScale"] = 1f;
                    }

                    if (!jObj.ContainsKey("musicVolume"))
                    {
                        jObj["musicVolume"] = 1f;
                        jObj["sfxVolume"] = 1f;
                    }

                    gameSettings = jObj.ToObject<GameSettings>();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error while loading settings, creating default settings;\n{e.Message}");
                    GlobalGameManager.IsFirstBoot = true;
                    CreateDefaultSettings();
                }
            }
            else
            {
                GlobalGameManager.IsFirstBoot = true;
                CreateDefaultSettings();
            }
        }

        public static void SaveSettings()
        {
            string json = JsonUtility.ToJson(gameSettings);
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
        public struct GameSettings
        {
            // default settings constructor
            public GameSettings(
                bool showSplash = false,
                bool showLoadingTips = true,
                bool isFullscreen = false,
                int resolutionIndex = 0,
                int resolutionWidth = 1280,
                int resolutionHeight = 720,
                float masterVolume = 0.8f,
                int dspSize = 340,
                int sampleRate = 44100,
                bool editorCursorEnable = true,
                bool discordRPCEnable = true,
                PerfectChallengeType perfectChallengeType = PerfectChallengeType.On,
                bool isMedalOn = true,
                bool timingDisplayMinMode = false,
                bool overlaysInEditor = true,
                bool letterboxBgEnable = true,
                bool letterboxFxEnable = true,
                int editorScale = 0,
                bool scaleWScreenSize = false,
                int showParamTooltips = 1,
                bool previewNoteSounds = true,
                bool simpleIcons = false,
                bool showFanGames = true,
                int fontType = 0,
                bool hasOpenedEditor = false,
                bool advancedFeatures = false,
                bool hasOpenedLibrary = false,
                bool libSortAscend = true,
                bool forceFallbackFileBrowser = false,
                bool perfectNoSlam = false,
                int perfectSfx = 0,
                string sortMode = "alphabetical",
                bool limitFramerate = true,
                int maxFps = 120,
                bool vSync = false,
                float resScale = 1f,
                float musicVolume = 1f,
                float sfxVolume = 1f,
                bool editorMinigameColorEvents = false,
                bool disableVfx = false
                )
            {
                this.showSplash = showSplash;
                this.showLoadingTips = showLoadingTips;
                this.enabledTips = new(){HintGiver.HintTypes.General, HintGiver.HintTypes.Play, HintGiver.HintTypes.Editor, HintGiver.HintTypes.Silly, HintGiver.HintTypes.Fact};
                this.isFullscreen = isFullscreen;

                this.resolutionIndex = resolutionIndex;
                this.resolutionWidth = resolutionWidth;
                this.resolutionHeight = resolutionHeight;
                this.resScale = resScale;
                this.disableVfx = disableVfx;

                this.masterVolume = masterVolume;
                this.musicVolume = musicVolume;
                this.sfxVolume = sfxVolume;
                this.dspSize = dspSize;
                this.sampleRate = sampleRate;

                this.forceFallbackFileBrowser = forceFallbackFileBrowser;

                this.editorCursorEnable = editorCursorEnable;
                if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
                    this.discordRPCEnable = false;
                else
                    this.discordRPCEnable = true;
                this.editorScale = editorScale;
                this.scaleWScreenSize = scaleWScreenSize;
                this.showParamTooltips = showParamTooltips;
                this.editorMinigameColorEvents = editorMinigameColorEvents;

                this.previewNoteSounds = previewNoteSounds;
                this.simpleIcons = simpleIcons;
                this.showFanGames = showFanGames;
                this.fontType = fontType;
                this.hasOpenedEditor = hasOpenedEditor;
                this.advancedFeatures = advancedFeatures;

                this.isMedalOn = isMedalOn;
                this.timingDisplayMinMode = timingDisplayMinMode;
                this.overlaysInEditor = overlaysInEditor;
                this.letterboxBgEnable = true;
                this.letterboxFxEnable = true;
                this.perfectNoSlam = false;
                this.perfectSfx = 0;
                this.sortMode = sortMode;
                this.limitFramerate = limitFramerate;
                this.maxFps = maxFps;
                this.vSync = vSync;

                this.timingDisplayComponents = new List<OverlaysManager.TimingDisplayComponent>()
                {
                    OverlaysManager.TimingDisplayComponent.CreateDefaultDual()
                };
                this.skillStarComponents = new List<OverlaysManager.SkillStarComponent>()
                {
                    OverlaysManager.SkillStarComponent.CreateDefault()
                };
                this.sectionComponents = new List<OverlaysManager.SectionComponent>()
                {
                    OverlaysManager.SectionComponent.CreateDefault()
                };

                this.libSortType = LibraryManager.LibrarySortType.Filename;
                this.libSortAscend = libSortAscend;
                this.libZoomLevel = 3;
                this.hasOpenedLibrary = hasOpenedLibrary;
                this.lastLibraryOpenedBuild = GlobalGameManager.buildInt;

                string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
                libLevelsDirectory = Application.platform is not RuntimePlatform.WindowsPlayer and not RuntimePlatform.WindowsEditor
                    ? Path.Combine(documentsPath, "Documents", LevelsFolderName)
                    : Path.Combine(documentsPath, LevelsFolderName);
            }

            // Display / Audio Settings
            public bool showSplash;

            public bool showLoadingTips;
            public List<HintGiver.HintTypes> enabledTips;
            public bool isFullscreen;

            public int resolutionIndex;
            public int resolutionWidth;
            public int resolutionHeight;
            public float resScale;
            public bool limitFramerate;
            public int maxFps;
            public bool vSync;
            public bool disableVfx;

            public float masterVolume;
            public float musicVolume;
            public float sfxVolume;
            public int dspSize;
            public int sampleRate;
            public bool forceFallbackFileBrowser;

            // Editor Settings
            public bool editorCursorEnable;
            public bool discordRPCEnable;
            public int editorScale;
            public bool scaleWScreenSize;
            public int showParamTooltips;
            public bool previewNoteSounds;
            public bool simpleIcons;
            public bool editorMinigameColorEvents;

            public bool showFanGames;
            public bool advancedFeatures;
            public int fontType;

            public bool hasOpenedEditor;
            // public bool showCornerTooltips;

            // Library Settings
            public bool hasOpenedLibrary;
            public LibraryManager.LibrarySortType libSortType;
            public bool libSortAscend;
            public string libLevelsDirectory;
            public int libZoomLevel;
            public int lastLibraryOpenedBuild;

            // Gameplay Settings
            public bool isMedalOn;
            public bool timingDisplayMinMode;
            public bool overlaysInEditor;
            public bool letterboxBgEnable;
            public bool letterboxFxEnable;
            public bool perfectNoSlam;
            public int perfectSfx;
            public string sortMode;
            public List<OverlaysManager.TimingDisplayComponent> timingDisplayComponents;
            public List<OverlaysManager.SkillStarComponent> skillStarComponents;
            public List<OverlaysManager.SectionComponent> sectionComponents;
            // public List<OverlaysManager.DurationComponent> durationComponents;
            // public List<OverlaysManager.WordJudgementComponent> wordJudgementComponents;
        }
    }
}