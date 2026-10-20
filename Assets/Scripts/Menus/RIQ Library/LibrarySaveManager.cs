using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using static HeavenStudio.Minigames;

namespace HeavenStudio.Util
{
    [Serializable]
    public static class LibrarySaveManager
    {
        public static LevelSaveDataHolder savedata = new();

        /// <summary>
        /// Load the library cache from cacheData.json
        /// </summary>
        /// <returns>Returns the LibraryCache loaded from the cache file</returns>
        public static void LoadSaveData()
        {
            string cacheJsonPath = Path.Combine(Application.persistentDataPath, "save.json");
            LevelSaveDataHolder newHolder = new();

            if (File.Exists(cacheJsonPath))
            {
                string json = File.ReadAllText(cacheJsonPath);
                if (json == "")
                {
                    Debug.LogWarning("Save Data json empty, creating new");
                }
                else
                {
                    try
                    {
                        newHolder = JsonConvert.DeserializeObject<LevelSaveDataHolder>(json);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error while loading save data\n{e.Message}");
                    }
                }
            }

            savedata = newHolder;
        }

        /// <summary>
        /// Saves this LibraryCache to cacheData.json
        /// </summary>
        public static void SaveSaveData()
        {
            savedata.lastSaveTime = DateTime.Now.ToString();

            string json = JsonConvert.SerializeObject(savedata, Formatting.Indented);
            // save json to persistentDataPath

            string saveJsonPath = Path.Combine(Application.persistentDataPath, "save.json");
            FileStream file = File.Create(saveJsonPath);

            file.Write(System.Text.Encoding.UTF8.GetBytes(json));
            file.Close();
        }

        public static void DeleteLevelSaveData(string hash)
        {
            if (savedata.levelInfo.ContainsKey(hash))
            {
                savedata.levelInfo.Remove(hash);
            }
        }

        public static void SetAndSaveLevelSaveData(string hash, int score, bool skillStar, List<bool> sections, bool perfect)
        {
            LevelSaveData levelSaveData = new();
            if (savedata.levelInfo.ContainsKey(hash))
            {
                levelSaveData.score = Math.Max(score, savedata[hash].score);
                levelSaveData.skillStar = savedata[hash].skillStar || skillStar;
                levelSaveData.perfect = savedata[hash].perfect || perfect;

                for (int i = 0; i < sections.Count; i++)
                {
                    levelSaveData.sectionInfo.Add(savedata[hash].sectionInfo[i] || sections[i]);
                }

                savedata.levelInfo.Remove(hash);
            }
            else
            {
                levelSaveData.score = score;
                levelSaveData.skillStar = skillStar;
                levelSaveData.sectionInfo = sections;
                levelSaveData.perfect = perfect;
            }

            levelSaveData.playTime = DateTime.Now.ToString();

            savedata.levelInfo.Add(hash, levelSaveData);

            SaveSaveData();
        }
    }

    [Serializable]
    public class LevelSaveDataHolder
    {
        public string lastSaveTime;
        public Dictionary<string, LevelSaveData> levelInfo = new();

        public LevelSaveData this[string hash]
        {
            get
            {
                return levelInfo[hash];
            }
        }
    }

    [Serializable]
    public class LevelSaveData
    {
        public string playTime;
        public int score;
        public bool skillStar;
        public bool perfect;
        public List<bool> sectionInfo = new();
    }
}