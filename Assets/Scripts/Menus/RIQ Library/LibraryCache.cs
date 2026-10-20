using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace HeavenStudio.Util
{
    public static class LibraryCacheManager
    {
        public static LibraryCache cache;
        public static string cachePath;
        public static int currentCacheVer = 1;

        /// <summary>
        /// Load the library cache from cacheData.json
        /// </summary>
        public static void LoadCache()
        {
            cachePath = Path.Combine(Application.persistentDataPath, "LibraryCache");
            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }

            string cacheJsonPath = Path.Combine(cachePath, "cacheData.json");

            if (File.Exists(cacheJsonPath))
            {
                string json = File.ReadAllText(cacheJsonPath);
                if (json == "")
                {
                    cache = new LibraryCache();
                }

                try
                {
                    cache = JsonConvert.DeserializeObject<LibraryCache>(json);
                    if (cache.cacheVer != currentCacheVer)
                    {
                        cache = new LibraryCache();
                        Debug.LogWarning("Cache version mismatch! Making new cache...");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error while loading level cache\n{e.Message}");
                    cache = new LibraryCache();
                }
            }
            else
            {
                cache = new LibraryCache();
            }
        }

        /// <summary>
        /// Saves the LibraryCache to cacheData.json
        /// </summary>
        public static void SaveCache()
        {
            Debug.Log("Saving Library Cache");
            cache.lastSaveTime = DateTime.Now.ToString();
            cache.cacheVer = currentCacheVer;

            string json = JsonConvert.SerializeObject(cache, Formatting.Indented);

            string cacheJsonPath = Path.Combine(cachePath, "cacheData.json");
            FileStream file = File.Create(cacheJsonPath);

            file.Write(System.Text.Encoding.UTF8.GetBytes(json));
            file.Close();
        }

        public static void RemoveCacheEntry(string hash)
        {
            if (cache.cachedInfo[hash].customIcon)
            {
                string iconDirectory = Path.Combine(cachePath, "LevelIcons", hash);

                if (File.Exists(Path.Combine(iconDirectory, "icon.png")))
                {
                    File.Delete(Path.Combine(iconDirectory, "icon.png"));
                }

                Directory.Delete(iconDirectory);
            }

            cache.cachedInfo.Remove(hash);
        }
    }

    [Serializable]
    public class LibraryCache
    {
        public int cacheVer;
        public string lastSaveTime;
        public Dictionary<string, CachedLevelInfo> cachedInfo = new();
        public List<string> favorites = new();
    }

    [Serializable]
    public struct CachedLevelInfo
    {
        public string[] icons;
        public bool customIcon;
        public bool useNameInIcon;
        public string levelName;
        public string levelCharter;
        public string songArtist;
        public string songName;
        public string description;
        public string tags;
        public double firstBpm;
        public double lowBpm;
        public double highBpm;
        public double length;
        public bool hasSkillStar;
        public int numberOfMedals;
        public string controlStyle;
        public bool flashWarning;
        public List<LibraryInfoPanel.WarningTypes> warnings;

        public string SeparateTagsIntoString()
        {
            if (string.IsNullOrWhiteSpace(tags)) return "";

            return string.Join(' ', tags.Split(','));
        }
    }
}