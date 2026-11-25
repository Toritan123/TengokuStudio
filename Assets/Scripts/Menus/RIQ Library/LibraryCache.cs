using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using Newtonsoft.Json;
using UnityEngine;

namespace HeavenStudio.Util
{
    public static class LibraryCacheManager
    {
        public static LibraryCache cache;
        public static string cachePath;
        public static int currentCacheVer = 2;

        public static SortedSet<string> tagbank;
        public static string defaultTags;

        /// <summary>
        /// Load the library cache from cacheData.json
        /// </summary>
        public static void LoadCache()
        {
            defaultTags = Resources.Load<TextAsset>("defaulttags").ToString();
            defaultTags = string.Join(',', defaultTags.Split('\n'));

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

            RefreshTagbank();
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

        public static void AddCacheEntry(string hash, CachedLevelInfo levelInfo)
        {
            cache.cachedInfo[hash] = levelInfo;
            RefreshTagbank();
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

            RefreshTagbank();
        }

        private static void RefreshTagbank()
        {
            tagbank = new();
            tagbank.UnionWith(LibraryTag.ParseTagString(defaultTags));

            foreach ((string _, CachedLevelInfo levelInfo) in cache.cachedInfo)
            {
                tagbank.UnionWith(LibraryTag.ParseTagString(levelInfo.tags));
            }
            
            Debug.Log("tagbank: " + tagbank.Count);
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
        public string fileName;
        public long lastModified;
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