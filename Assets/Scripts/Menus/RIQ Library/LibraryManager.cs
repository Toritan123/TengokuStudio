using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HeavenStudio.Common;
using HeavenStudio.InputSystem;
using HeavenStudio.Util;
using Jukebox;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


namespace HeavenStudio
{
    public class LibraryManager : MonoBehaviour
    {
        public static LibraryManager instance;
        [Header("Paths")]
        [SerializeField] public static string levelsPath;
        [SerializeField] public string selectedLevelPath;

        [Header("Selector")]
        [SerializeField] private GameObject levelIconPrefab;
        [SerializeField] private GameObject folderIconPrefab;
        [SerializeField] private GridLayoutGroup levelsGrid;
        [SerializeField] public static Dictionary<string, IconInfo> allIconInfo = new();
        [SerializeField] private int zoomLevel = 3;
        [SerializeField] public static string currentFolder;
        [SerializeField] private LibraryFolderIcon favoritesFolderIcon;
        [SerializeField] public ScrollRect levelsScrollRect;
        public static string lastFolder;
        public static float lastScroll;
        public List<string> sortedPaths;
        public List<string> visibleLevelPaths;
        public List<GameObject> visibleIcons;
        private Coroutine scrollCo;

        [Header("Main Content")]
        [SerializeField] public LibraryInfoPanel infoPanel;
        [SerializeField] private LibraryFolderEditor folderEditor;
        [SerializeField] private SettingsDialog settingsPanel;
        [SerializeField] private TMP_Text pathText;
        [SerializeField] private LibrarySelectable importFilesButton;
        [SerializeField] private LibrarySelectable folderInfoButton;

        [Header("Music & Camera")]
        public AudioSource bgmSource;
        [SerializeField] AudioClip music;
        [SerializeField] float fullVolume;
        [SerializeField] float fadeTime;
        [SerializeField] public Camera cam;
        [SerializeField] public Coroutine fadeInCoroutine;
        [SerializeField] public Coroutine fadeOutCoroutine;

        [Header("Sorting")]
        [SerializeField] private LibrarySortType currentSortType;
        [SerializeField] private bool currentlyAscending = true;
        [SerializeField] Image sortButtonIcon;
        [SerializeField] AspectRatioFitter sortButtonAspectRatioFitter;
        [SerializeField] Image sortButtonArrow;
        [SerializeField] Image levelNameArrow;
        [SerializeField] Image fileNameArrow;
        [SerializeField] Image artistArrow;
        [SerializeField] Image charterArrow;
        [SerializeField] Image scoreArrow;
        [SerializeField] private Button levelNameButton;
        [SerializeField] private Button fileNameButton;
        [SerializeField] private Button artistButton;
        [SerializeField] private Button charterButton;
        [SerializeField] private Button scoreButton;
        [SerializeField] private Button lastPlayedButton;
        [SerializeField] public Button customButton;
        [SerializeField] private LibraryPopup sortPopup;
        [SerializeField] Sprite levelNameIcon;
        [SerializeField] Sprite fileNameIcon;
        [SerializeField] Sprite artistIcon;
        [SerializeField] Sprite charterIcon;
        [SerializeField] Sprite scoreIcon;
        [SerializeField] Sprite lastPlayedIcon;
        [SerializeField] Sprite customIcon;


        [Header("Setup")]
        [SerializeField] private GameObject setupPanel;
        [SerializeField] private TMP_Text setupHeader;
        [SerializeField] private TMP_Text setupStatusLabel;
        [SerializeField] private Slider setupProgressSlider;
        [SerializeField] private LibrarySelectable setupCloseButton;
        [SerializeField] public static int refreshStatus = -1;
        private bool setupFailed = false;

        [Header("Data")]
        public static bool refreshComplete = false;
        [SerializeField] private static int filesToProcess;
        [SerializeField] private static int filesProcessed;
        [SerializeField] private static int iconsToProcess;
        [SerializeField] private static int iconsProcessed;
        [SerializeField] private int filesToMove;
        [SerializeField] private int filesMoved;
        [SerializeField] private List<string> movedFilePaths;
        private static Dictionary<string, string> currentPathsAndHashes = new();
        private static ConcurrentBag<KeyValuePair<string, string>> concurrentPathsAndHashes = new();
        private static ConcurrentBag<KeyValuePair<string, CachedLevelInfo>> concurrentTempCache = new();
        [SerializeField] private float downloadPercent;
        public static Dictionary<string, string> savedPathsAndHashes;
        public List<string> hashesPresent => allIconInfo.Values.Where(iconInfo => !iconInfo.isFolder).Select(iconInfo => iconInfo.hash).ToList();

        [Header("Other")]
        [SerializeField] public bool exiting;
        [SerializeField] private TMP_InputField linkInput;
        [SerializeField] private TMP_InputField searchBar;

        [Header("The room between... There is a room between.")]
        [SerializeField] private RectTransform PLACE_MUSEUM;
        [SerializeField] private AudioSource DEVICE_AUDIOPLAYER;
        [SerializeField] private Image DEVICE_CAR;
        [SerializeField] private GameObject DEVICE_IMPACT;
        [SerializeField] private Sprite IMAGE_CAR_0;
        [SerializeField] private Sprite IMAGE_CAR_1;
        [SerializeField] private AudioClip AUDIO_DASH;
        [SerializeField] private AudioClip AUDIO_IMPACT;
        [SerializeField] private AudioClip AUDIO_MUSEUM;


        #region Library Setup & Initialization

        // Start is called before the first frame update
        void Start()
        {
            instance = this;
            LibrarySaveManager.LoadSaveData();
            PersistentDataManager.LoadSettings();

            if (string.IsNullOrEmpty(PersistentDataManager.gameSettings.libLevelsDirectory))
            {
                PersistentDataManager.gameSettings.libLevelsDirectory = SetDefaultLibraryPath();
            }


            zoomLevel = PersistentDataManager.gameSettings.libZoomLevel;
            levelsPath = PersistentDataManager.gameSettings.libLevelsDirectory;

            if (UnityEngine.Random.Range(0, 666) != 0)
            {
                _ = InitializeLibrary();

                bgmSource.clip = music;
                //    fadeInCoroutine = StartCoroutine(FadeInMusic());
                bgmSource.Play();
            }
            else
            {
                StartCoroutine(Egg());
            }
        }

        public static string SetDefaultLibraryPath()
        {
            string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);

            return Application.platform is not RuntimePlatform.WindowsPlayer and not RuntimePlatform.WindowsEditor
                ? Path.Combine(documentsPath, "Documents", PersistentDataManager.LevelsFolderName)
                : Path.Combine(documentsPath, PersistentDataManager.LevelsFolderName);
        }


        private async Task InitializeLibrary()
        {
            bool levelsDirExists = Directory.Exists(levelsPath);

            if (!levelsDirExists)
            {
                SetupLibrary(levelsPath, false);
            }

            // Packin update has been moved to GlobalGameManager init

            if (GlobalGameManager.transitioningToLibraryFromGameplay)
            {
                await Refresh(savedPathsAndHashes);
            }
            else
            {
                await Refresh();
            }

            if (GlobalGameManager.transitioningToLibraryFromGameplay && allIconInfo.ContainsKey(GlobalGameManager.PlayOpenFilePath))
            {
                string path = GlobalGameManager.PlayOpenFilePath;
                GlobalGameManager.instance.ClearOpenFile();
                GlobalGameManager.transitioningToLibraryFromGameplay = false;

                SelectFolder(lastFolder);
                StartCoroutine(ScrollToPosition(lastScroll));

                SelectLevel(path, false);
                IconInfo icon = allIconInfo[path];

                if (icon.isFolder)
                {
                    icon.folderIcon.OnNavigateTo(true, false);
                }
                else
                {
                    icon.levelIcon.OnNavigateTo(true, false);
                }
            }
            else
            {
                IconInfo icon = GetFirstVisibleIcon();

                if (icon == null)
                {
                    LibraryMenuCursor.instance.SetSelectable(LibraryMenuCursor.instance.defaultToolbarSelectable);
                    LibraryMenuCursor.instance.levelsInGrid = false;
                }
                else if (LibraryMenuCursor.usingMouse)
                {
                    LibraryMenuCursor.instance.SetSelectable(LibraryMenuCursor.instance.defaultToolbarSelectable);
                    LibraryMenuCursor.instance.levelsInGrid = true;
                }
                else if (icon.isFolder)
                {
                    icon.folderIcon.OnNavigateTo(false);
                    LibraryMenuCursor.instance.levelsInGrid = true;
                }
                else
                {
                    icon.levelIcon.OnNavigateTo(false);
                    LibraryMenuCursor.instance.levelsInGrid = true;
                }

                if (!PersistentDataManager.gameSettings.hasOpenedLibrary) PersistentDataManager.gameSettings.hasOpenedLibrary = true;
            }


            PersistentDataManager.SaveSettings();
        }

        private IconInfo GetFirstVisibleIcon()
        {
            string firstVisiblePath;

            if ((visibleLevelPaths != null) && (visibleLevelPaths.Count > 0))
            {
                if ((visibleLevelPaths[0] == "Favorites") && (!favoritesFolderIcon.gameObject.activeSelf))
                {
                    if (visibleLevelPaths.Count > 1)
                    {
                        firstVisiblePath = visibleLevelPaths[1];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    firstVisiblePath = visibleLevelPaths[0];
                }
            }
            else
            {
                return null;
            }


            return allIconInfo[firstVisiblePath];
        }

        public static async Task Refresh(Dictionary<string, string> pathsAndHashesOverride = null, bool skipMenuStuff = false)
        {
            refreshComplete = false;
            SetupForCaching(skipMenuStuff);

            await ProcessPathsTask(pathsAndHashesOverride, skipMenuStuff);

            await SetupIconInfoTask();

            Task staleCacheTask = CreateStaleCacheTask();

            iconsToProcess = currentPathsAndHashes.Count;
            iconsProcessed = 0;

            refreshStatus = 2;

            SpawnGridObjects(skipMenuStuff); // not async

            await staleCacheTask;

            refreshStatus = -1;

            if ((instance != null) && !skipMenuStuff)
            {
                instance.setupPanel.SetActive(false);
            }

            refreshComplete = true;

        }

        /// <summary>
        /// Sets the levels path, resets the current folder to the top-level,
        /// clears allIconInfo and currentPathsAndHashes, and removes all gameobjects
        /// in the selection grid.
        /// </summary>
        private static void SetupForCaching(bool skipMenuStuff = false)
        {
            try
            {
                print("Refreshing");
                refreshComplete = false;

                levelsPath = PersistentDataManager.gameSettings.libLevelsDirectory;
                currentFolder = levelsPath;

                ClearLevels(skipMenuStuff);
                currentPathsAndHashes.Clear();

                if ((instance != null) && !skipMenuStuff)
                {
                    instance.infoPanel.ShowHelpAndStats();

                    if (!GlobalGameManager.transitioningToLibraryFromGameplay)
                    {
                        instance.setupPanel.SetActive(true);
                        refreshStatus = 0;
                        instance.setupHeader.text = Localization.T("Please Wait...");
                        instance.setupStatusLabel.gameObject.SetActive(true);
                        instance.setupStatusLabel.text = Localization.T("Getting file paths");
                        instance.setupProgressSlider.gameObject.SetActive(false);
                        instance.setupProgressSlider.value = 0;
                        instance.setupCloseButton.gameObject.SetActive(false);
                        LibraryMenuCursor.instance.HideCursor();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Refresh failed in block 1: {e}");
            }
        }


        /// <summary>
        /// Go through all paths in the library folder, cache all levels,
        /// and set up all folders. Also sets up the list of paths and hashes in the library.
        /// </summary>
        /// <param name="pathsAndHashesOverride">Skip processing if provided, and just use the provided paths and hashes.</param>
        /// <returns></returns>
        private static async Task ProcessPathsTask(Dictionary<string, string> pathsAndHashesOverride, bool skipMenuStuff = false)
        {
            if (pathsAndHashesOverride == null)
            {
                string[] filePaths = Directory.GetFiles(levelsPath, "*", new EnumerationOptions() { RecurseSubdirectories = true });
                string[] folderPaths = Directory.GetDirectories(levelsPath, "*", new EnumerationOptions() { RecurseSubdirectories = true });

                filesToProcess = filePaths.Length;
                filesProcessed = 0;

                refreshStatus = 1;

                if ((instance != null) && !skipMenuStuff)
                {
                    instance.setupProgressSlider.gameObject.SetActive(true);
                }

                concurrentPathsAndHashes = new();
                concurrentTempCache = new();

                Task folderPathsTask = CreateFolderPathsTask(folderPaths);
                Task cacheLevelsTask = CreateCacheLevelsTask(filePaths);

                await Task.WhenAll(cacheLevelsTask, folderPathsTask);

                UpdatePathsAndHashesAndCache();
            }
            else
            {
                currentPathsAndHashes = pathsAndHashesOverride;
            }
        }


        /// <summary>
        /// Transfers data from concurrentPathsAndHashes to currentPathsAndHashes,
        /// and updates the LibraryCache with the data from concurrentTempCache.
        /// </summary>
        private static void UpdatePathsAndHashesAndCache()
        {
            try
            {
                concurrentPathsAndHashes.ToList().ForEach(kvp => currentPathsAndHashes.Add(kvp.Key, kvp.Value));

                foreach (KeyValuePair<string, CachedLevelInfo> tempCacheEntry in concurrentTempCache)
                {
                    LibraryCacheManager.AddCacheEntry(tempCacheEntry.Key, tempCacheEntry.Value);
                }

            }
            catch (Exception e)
            {
                Debug.LogError($"Refresh failed in block 4: {e}");
            }
        }


        private static Task CreateFolderPathsTask(string[] folderPaths)
        {
            return Task.Run(() =>
            {
                try
                {
                    currentPathsAndHashes.Add(levelsPath, null);
                    currentPathsAndHashes.Add("Favorites", null);

                    foreach (string currentFolderPath in folderPaths)
                    {
                        currentPathsAndHashes.Add(currentFolderPath, null);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Refresh failed in block 2: {e}");
                }
            });
        }


        private static Task CreateCacheLevelsTask(string[] filePaths)
        {
            return Task.Run(() => Parallel.ForEach(filePaths, filePath =>
            {
                try
                {
                    CacheProcess(filePath);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Refresh failed in block 3: {e}");
                }
            }));
        }


        /// <summary>
        /// Returns a Task that removes cache entries where
        /// the hash isn't present in allIconInfo.
        /// </summary>
        /// <returns></returns>
        private static Task CreateStaleCacheTask()
        {
            return Task.Run(() =>
            {
                try
                {
                    List<string> hashesPresent = allIconInfo.Values.Where(iconInfo => !iconInfo.isFolder).Select(iconInfo => iconInfo.hash).ToList();
                    List<string> hashesMissing = LibraryCacheManager.cache.cachedInfo.Keys.Where(hash => !hashesPresent.Contains(hash)).ToList();

                    foreach (string hash in hashesMissing)
                    {
                        LibraryCacheManager.RemoveCacheEntry(hash);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Refresh failed in block 6: {e}");
                }
            });
        }


        /// <summary>
        /// For each entry in allIconInfo, spawns a
        /// folder gameobject or an icon gameobject
        /// and updates allIconInfo with a reference
        /// to that gameobject.
        /// </summary>
        private static void SpawnGridObjects(bool skipMenuStuff = false)
        {
            try
            {
                int spawnCount = 0;
                foreach (KeyValuePair<string, string> fileAndHash in currentPathsAndHashes)
                {
                    string path = fileAndHash.Key;
                    IconInfo iconInfo = allIconInfo[path];
                    try
                    {
                        if ((instance != null) && !skipMenuStuff)
                        {
                            if (iconInfo.isFolder)
                            {
                                instance.CreateFolder(path);
                            }
                            else
                            {
                                instance.CreateIcon(path, iconInfo.hash);
                            }
                        }

                        iconsProcessed++;

                        if (allIconInfo.ContainsKey(Path.GetDirectoryName(path))) allIconInfo[Path.GetDirectoryName(path)].pathsInFolder.Add(fileAndHash.Key);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Refresh failed in block 7: {e}");
                    }

                    spawnCount++;
                }

                if ((instance != null) && !skipMenuStuff)
                {
                    instance.AutoSort();
                    instance.StartCoroutine(instance.Zoom(true));

                    instance.SelectFolder(levelsPath, false);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Refresh failed in block 8: {e}");
            }
        }

        /// <summary>
        /// Creates the allIconInfo dictionary based on the
        /// present files, and marks them as being folders
        /// or their hashes, but does not fill the rest
        /// of the icon info.
        /// </summary>
        /// <returns></returns>
        private static async Task SetupIconInfoTask()
        {
            Task iconInfoTask = Task.Run(() =>
            {
                try
                {
                    foreach (var fileAndHash in currentPathsAndHashes)
                    {
                        allIconInfo.Add(fileAndHash.Key, new());

                        if (fileAndHash.Value == null)
                        {
                            allIconInfo[fileAndHash.Key].isFolder = true;
                        }
                        else
                        {
                            allIconInfo[fileAndHash.Key].hash = fileAndHash.Value;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Refresh failed in block 5: {e}");
                }
            });

            await iconInfoTask;
        }


        public async Task ImportProcess(string[] paths, bool filesAlreadyMoved = false, bool deleteOriginal = false)
        {
            currentPathsAndHashes.Clear();
            movedFilePaths.Clear();

            filesToMove = paths.Length;
            filesMoved = 0;

            setupPanel.SetActive(true);
            refreshStatus = 3;
            setupHeader.text = Localization.T("Importing Levels...");
            setupStatusLabel.gameObject.SetActive(true);
            setupStatusLabel.text = Localization.T("Moving files");
            setupProgressSlider.gameObject.SetActive(true);
            setupProgressSlider.value = 0;
            setupCloseButton.gameObject.SetActive(false);
            LibraryMenuCursor.instance.HideCursor();

            if (filesAlreadyMoved)
            {
                movedFilePaths.AddRange(paths);
            }
            else
            {
                Task moveTask = Task.Run(() =>
                {
                    foreach (var path in paths)
                    {
                        MoveLevelToLibraryProcess(path, deleteOriginal);
                    }

                });

                await moveTask;
            }

            List<string> filePaths = new();
            List<string> folderPaths = new();

            foreach (string path in movedFilePaths)
            {
                if (Directory.Exists(path))
                {
                    folderPaths.Add(path);
                    filePaths.AddRange(Directory.GetFiles(path, "*", new EnumerationOptions() { RecurseSubdirectories = true }));
                    folderPaths.AddRange(Directory.GetDirectories(path, "*", new EnumerationOptions() { RecurseSubdirectories = true }));
                }
                else
                {
                    filePaths.Add(path);
                }
            }

            filesToProcess = movedFilePaths.Count;
            filesProcessed = 0;

            refreshStatus = 1;
            setupProgressSlider.gameObject.SetActive(true);

            Task folderPathsTask = Task.Run(() =>
            {
                foreach (string currentFolderPath in folderPaths)
                {
                    currentPathsAndHashes.Add(currentFolderPath, null);
                }
            });

            concurrentPathsAndHashes = new();
            concurrentTempCache = new();

            Task cacheTask = Task.Run(() => Parallel.ForEach(filePaths, filePath =>
            {
                CacheProcess(filePath);
            }));

            await Task.WhenAll(cacheTask, folderPathsTask);

            concurrentPathsAndHashes.ToList().ForEach(kvp => currentPathsAndHashes.Add(kvp.Key, kvp.Value));
            foreach (KeyValuePair<string, CachedLevelInfo> tempCacheEntry in concurrentTempCache)
            {
                LibraryCacheManager.AddCacheEntry(tempCacheEntry.Key, tempCacheEntry.Value);
            }


            Task iconInfoTask = Task.Run(() =>
        {
            foreach (var fileAndHash in currentPathsAndHashes)
            {
                allIconInfo.Add(fileAndHash.Key, new());

                if (fileAndHash.Value == null)
                {
                    allIconInfo[fileAndHash.Key].isFolder = true;
                }
                else
                {
                    allIconInfo[fileAndHash.Key].hash = fileAndHash.Value;
                }
            }
        });


            await iconInfoTask;

            iconsToProcess = currentPathsAndHashes.Count;
            iconsProcessed = 0;

            refreshStatus = 2;

            int spawnCount = 0;
            foreach (KeyValuePair<string, string> fileAndHash in currentPathsAndHashes)
            {
                string path = fileAndHash.Key;
                IconInfo iconInfo = allIconInfo[path];
                try
                {
                    if (iconInfo.isFolder)
                    {
                        CreateFolder(path);
                    }
                    else
                    {
                        CreateIcon(path, iconInfo.hash);
                    }
                    if (allIconInfo.ContainsKey(Path.GetDirectoryName(path))) allIconInfo[Path.GetDirectoryName(path)].pathsInFolder.Add(fileAndHash.Key);

                }
                catch (Exception e)
                {
                    Debug.LogError($"Couldnt create icon {fileAndHash.Key}: {e}");
                }

                spawnCount++;
            }

            AutoSort();
            StartCoroutine(Zoom(true));

            if (movedFilePaths.Count == 1)
            {
                if (Directory.Exists(movedFilePaths[0]))
                {
                    SelectFolder(movedFilePaths[0]);
                }
                else
                {
                    SelectLevel(movedFilePaths[0], true);
                    allIconInfo[movedFilePaths[0]].levelIcon.LoadCustomIcon();
                }
            }
            else
            {
                SelectFolder(currentFolder);
            }

            refreshStatus = -1;
            setupPanel.SetActive(false);

            refreshComplete = true;
        }

        private static void CacheProcess(string path)
        {
            if (string.Equals(Path.GetExtension(path), ".riq", StringComparison.OrdinalIgnoreCase))
            {
                FileInfo fileInfo = new FileInfo(path);
                string fileName = fileInfo.Name;
                long lastModified = fileInfo.LastWriteTimeUtc.Ticks;

                var cachedMatch = LibraryCacheManager.cache.cachedInfo.FirstOrDefault(c => c.Value.fileName == fileName && c.Value.lastModified == lastModified);

                if (cachedMatch.Key != null)
                {
                    concurrentPathsAndHashes.Add(new KeyValuePair<string, string>(path, cachedMatch.Key));
                }
                else
                {
                    using (FileStream fileStream = new FileStream(path, FileMode.Open))
                    {
                        StringBuilder hashBuilder = new();
                        using (SHA256 sha256 = SHA256.Create())
                        {
                            byte[] hashBytes = sha256.ComputeHash(fileStream);
                            foreach (Byte b in hashBytes)
                            {
                                hashBuilder.Append(b.ToString("X2"));
                            }
                        }

                        if (!LibraryCacheManager.cache.cachedInfo.ContainsKey(hashBuilder.ToString()))
                        {
                            try
                            {
                                CacheLevel(hashBuilder.ToString(), path, fileStream);
                                concurrentPathsAndHashes.Add(new KeyValuePair<string, string>(path, hashBuilder.ToString()));
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"{path} could not be cached\n{e}");
                            }
                        }
                        else
                        {
                            CachedLevelInfo updatedInfo = LibraryCacheManager.cache.cachedInfo[hashBuilder.ToString()];
                            updatedInfo.fileName = fileName;
                            updatedInfo.lastModified = lastModified;
                            concurrentTempCache.Add(new(hashBuilder.ToString(), updatedInfo));

                            concurrentPathsAndHashes.Add(new KeyValuePair<string, string>(path, hashBuilder.ToString()));
                        }
                    }
                }
            }

            filesProcessed++;
        }

        private static void CacheLevel(string levelHash, string path, FileStream fileStream)
        {
            using (ZipArchive archive = new ZipArchive(fileStream, ZipArchiveMode.Read))
            {
                RiqBeatmap beatmap = null;

                ZipArchiveEntry json = archive.GetEntry("remix.json");
                using (StreamReader reader = new StreamReader(json.Open()))
                {
                    string beatmapText = reader.ReadToEnd();
                    beatmap = new RiqBeatmap(beatmapText);
                }

                FileInfo fileInfo = new FileInfo(path);

                CachedLevelInfo cachedLevelInfo = new()
                {
                    fileName = fileInfo.Name,
                    lastModified = fileInfo.LastWriteTimeUtc.Ticks,
                    levelName = beatmap.data.properties.ContainsKey("remixtitle") ? (string)beatmap["remixtitle"] : "New Remix",
                    icons = new string[4] { "noGame", "noGame", "noGame", "noGame" },
                    customIcon = false,
                    useNameInIcon = beatmap.data.properties.ContainsKey("libraryicontitle") ? (bool)beatmap["libraryicontitle"] : true,
                    levelCharter = beatmap.data.properties.ContainsKey("remixauthor") ? (string)beatmap["remixauthor"] : "",
                    songArtist = beatmap.data.properties.ContainsKey("idolcredit") ? (string)beatmap["idolcredit"] : "",
                    description = beatmap.data.properties.ContainsKey("remixdesc") ? (string)beatmap["remixdesc"] : "",
                    controlStyle = beatmap.data.properties.ContainsKey("playstyle") ? (string)beatmap["playstyle"] : "",
                    length = 0,
                    hasSkillStar = false,
                    numberOfMedals = 0,
                    warnings = new(),
                    flashWarning = beatmap.data.properties.ContainsKey("accessiblewarning") ? (bool)beatmap["accessiblewarning"] : false,
                    tags = beatmap.data.properties.ContainsKey("remixtags") ? (string)beatmap["remixtags"] : "",
                    songName = beatmap.data.properties.ContainsKey("idolsong") ? (string)beatmap["idolsong"] : "",
                };

                #region Icons
                List<RiqEntity> allGameSwitches = EventCaller.GetAllInBeatmap(beatmap, "gameManager", new string[] { "switchGame" });

                List<string> uniqueGames = new();
                uniqueGames.AddRange(allGameSwitches.Select(entity => entity.datamodel.Split('/')[2]));
                uniqueGames = uniqueGames.Distinct().ToList();

                int maxGames = Math.Min(uniqueGames.Count, 4);
                for (int i = 0; i < maxGames; i++)
                {
                    cachedLevelInfo.icons[i] = uniqueGames[i];
                }


                ZipArchiveEntry customLevelIcon = archive.GetEntry("Resources/Images/LibraryIcon/LibraryLevelIcon.png");
                if (customLevelIcon == null) customLevelIcon = archive.GetEntry("Resources/Images/LibraryIcon/LibraryLevelIcon.jpg");

                if (customLevelIcon != null)
                {
                    string iconDirectory = Path.Combine(LibraryCacheManager.cachePath, "LevelIcons", levelHash);

                    if (Directory.Exists(iconDirectory))
                    {
                        Directory.Delete(iconDirectory, true);
                    }

                    Directory.CreateDirectory(iconDirectory);

                    string iconFilename = Path.Combine(iconDirectory, "icon.png");
                    customLevelIcon.ExtractToFile(iconFilename);

                    cachedLevelInfo.customIcon = true;
                }


                #endregion

                #region BPM
                List<double> tempos = new();

                for (int i = 0; i < beatmap.TempoChanges.Count; i++)
                {
                    var tempo = beatmap.TempoChanges[i]["tempo"];
                    tempos.Add(tempo);
                }

                cachedLevelInfo.firstBpm = tempos[0];
                cachedLevelInfo.lowBpm = tempos.Min();
                cachedLevelInfo.highBpm = tempos.Max();
                #endregion

                #region Length
                RiqEntity endRemix = beatmap.Entities.FindLast(e => e.datamodel == "gameManager/end");

                if (endRemix != null)
                {
                    cachedLevelInfo.length = LibraryInfoPanel.GetSongLength(endRemix.beat, beatmap);
                }
                else
                {
                    cachedLevelInfo.warnings.Add(LibraryInfoPanel.WarningTypes.NoEnd);
                }

                #endregion

                #region Section Medals and Skill Star

                int totalMedals = 0;
                for (int i = 0; i < beatmap.SectionMarkers.Count; i++)
                {
                    if (beatmap.SectionMarkers[i]["sectionName"] != "")
                    {
                        totalMedals++;
                    }
                }

                cachedLevelInfo.numberOfMedals = totalMedals;

                List<RiqEntity> allSkillStars = EventCaller.GetAllInBeatmap(beatmap, "gameManager", new string[] { "skill star" });
                if (allSkillStars.Count > 0)
                {
                    cachedLevelInfo.hasSkillStar = true;
                }

                #endregion

                concurrentTempCache.Add(new(levelHash, cachedLevelInfo));
            }
        }

        private void CreateIcon(string path, string hash)
        {
            LibraryLevelIcon level = Instantiate(levelIconPrefab, levelsGrid.transform).GetComponent<LibraryLevelIcon>();
            allIconInfo[path].levelIcon = level;

            level.SetIconInfo(path, hash);
        }


        private void CreateFolder(string path)
        {
            LibraryFolderIcon folder;

            if (path == levelsPath)
            {
                allIconInfo[path].isFolder = true;
            }
            else if (path == "Favorites")
            {
                folder = favoritesFolderIcon;
                folder.folderName = "Favorites";
                folder.folderPath = "Favorites";
                allIconInfo[path].folderIcon = folder;
                allIconInfo[path].isFolder = true;

                LibraryCacheManager.cache.favorites = LibraryCacheManager.cache.favorites.Where(path => allIconInfo.ContainsKey(path)).ToList();
                allIconInfo[path].pathsInFolder.AddRange(LibraryCacheManager.cache.favorites);

                allIconInfo[levelsPath].pathsInFolder.Add("Favorites");
                RefreshFavoritesIcon();
            }
            else
            {
                folder = Instantiate(folderIconPrefab, levelsGrid.transform).GetComponent<LibraryFolderIcon>();

                allIconInfo[path].folderIcon = folder;
                allIconInfo[path].isFolder = true;

                folder.SetFolderInfo(path);

            }
        }

        /// <summary>
        /// Destroys all folder and level gameobjects,
        /// then clears allIconInfo.
        /// </summary>
        private static void ClearLevels(bool skipMenuStuff = false)
        {
            if ((instance != null) && !skipMenuStuff)
            {
                foreach (IconInfo iconInfo in allIconInfo.Values)
                {
                    if ((iconInfo.levelIcon != null) || (iconInfo.folderIcon != null))
                    {
                        if (!(iconInfo.isFolder && (iconInfo.folderIcon.folderPath == "Favorites")))
                        {
                            Destroy(iconInfo.gameObject);
                        }
                    }
                }
            }

            allIconInfo.Clear();
        }

        private void MoveLevelToLibraryProcess(string filePath, bool deleteOriginal = false)
        {
            string fileNameNoExt = Path.GetFileNameWithoutExtension(filePath);
            string fileExt = Path.GetExtension(filePath);
            string newPath;

            if (string.Equals(fileExt, ".riq", StringComparison.OrdinalIgnoreCase))
            {
                if (currentFolder == "Favorites")
                {
                    newPath = Path.Combine(levelsPath, fileNameNoExt);
                }
                else
                {
                    newPath = Path.Combine(currentFolder, fileNameNoExt);
                }

                bool validPath = false;

                while (!validPath)
                {
                    if (!File.Exists(newPath + ".riq"))
                    {
                        validPath = true;
                        newPath += ".riq";
                    }
                    else
                    {
                        newPath += " copy";
                    }
                }

                File.Copy(filePath, newPath);
                movedFilePaths.Add(newPath);
            }
            else if (string.Equals(fileExt, ".briq", StringComparison.OrdinalIgnoreCase))
            {
                using (ZipArchive archive = ZipFile.Open(filePath, ZipArchiveMode.Read))
                {
                    if (archive.Entries.Count > 0)
                    {
                        string folderName = archive.Entries[0].FullName.Split("/")[0].Split("\\")[0];

                        if (currentFolder == "Favorites")
                        {
                            newPath = Path.Combine(levelsPath, folderName);
                        }
                        else
                        {
                            newPath = Path.Combine(currentFolder, folderName);
                        }

                        if (!Directory.Exists(newPath))
                        {
                            Directory.CreateDirectory(newPath);
                        }

                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            entry.ExtractToFile(Path.Combine(newPath, entry.Name), true);
                        }

                        movedFilePaths.Add(newPath);
                    }
                }

                if (deleteOriginal)
                {
                    File.Delete(filePath);
                }
            }

            filesMoved++;
        }

        public void SavePathsAndHashes()
        {
            savedPathsAndHashes = new();
            foreach (KeyValuePair<string, IconInfo> kvp in allIconInfo)
            {
                savedPathsAndHashes.Add(kvp.Key, kvp.Value.hash);
            }
        }

        public static void SetupLibrary(string levelsPath, bool setupFailedOnce)
        {
            try
            {
                Debug.Log("Creating directory");
                Directory.CreateDirectory(levelsPath);

                Debug.Log("Copying directory");
                string packIns = Path.Combine(Application.streamingAssetsPath, "Library Pack-In");

                bool packInsDirExists = Directory.Exists(packIns);
                if (!packInsDirExists)
                {
                    Debug.LogWarning("Set up the Library with no pack-ins!");
                    return;
                }

                CopyDirectory(packIns, levelsPath, true);

                Debug.Log("Clearing meta files");
                foreach (string path in Directory.GetFiles(levelsPath, "*", new EnumerationOptions() { RecurseSubdirectories = true }))
                {
                    if (Path.GetExtension(path) == ".meta")
                    {
                        File.Delete(path);
                    }
                }

                if (!PersistentDataManager.gameSettings.hasOpenedLibrary)
                {
                    PersistentDataManager.gameSettings.libSortType = LibrarySortType.Filename;
                    PersistentDataManager.gameSettings.libSortAscend = true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error setting up Library: {ex}");
                if (!setupFailedOnce)
                {
                    Debug.LogWarning($"Trying again in: {Application.persistentDataPath}");
                    PersistentDataManager.gameSettings.libLevelsDirectory = Path.Combine(Application.persistentDataPath, PersistentDataManager.LevelsFolderName);
                    SetupLibrary(PersistentDataManager.gameSettings.libLevelsDirectory, true);  // retry in the new place, not the one that failed
                    return;
                }
                else
                {
                    Debug.LogError($"An error occurred setting up the Library. Please report this bug if you encounter it!\n\n{ex}");
                }
            }

            Debug.Log("Library setup complete");
        }

        //https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        void Update()
        {
            if (refreshStatus == -1)
            {
                if (Input.GetKey(InputKeyboard.MODIFIER))
                {
                    if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.Equals))
                    {
                        ZoomIn();
                    }
                    else if (Input.GetKeyDown(KeyCode.Minus))
                    {
                        ZoomOut();
                    }
                }
            }

            if (refreshStatus != -1)
            {
                UpdateRefreshStatusPanel();
            }
        }

        private void UpdateRefreshStatusPanel()
        {
            if (refreshStatus == 1) // has started caching files
            {
                setupStatusLabel.text = Localization.F("Getting level metadata: {0}/{1}", filesProcessed, filesToProcess);
                setupProgressSlider.value = (float)filesProcessed / (float)filesToProcess;
            }
            else if (refreshStatus == 2) // has started creating icons
            {
                setupStatusLabel.text = Localization.F("Creating level icons: {0}/{1}", iconsProcessed, iconsToProcess);
                setupProgressSlider.value = (float)iconsProcessed / (float)iconsToProcess;
            }
            else if (refreshStatus == 3) // is moving files
            {
                setupStatusLabel.text = Localization.F("Moving files: {0}/{1}", filesMoved, filesToMove);
                setupProgressSlider.value = (float)filesMoved / (float)filesToMove;
            }
            else if (refreshStatus == 4) // is downloading file
            {
                setupStatusLabel.text = Localization.F("Download progress: {0}%", Mathf.RoundToInt(downloadPercent * 100));
                setupProgressSlider.value = downloadPercent;
            }
        }

        private IEnumerator Zoom(bool sfx = false)
        {
            yield return null;
            if (!sfx) SoundByte.PlayOneShot("ui/Library/zoom", ui: true);

            zoomLevel = Math.Clamp(zoomLevel, 2, 6);

            PersistentDataManager.gameSettings.libZoomLevel = zoomLevel;

            levelsGrid.constraintCount = zoomLevel;

            RectTransform gridRect = levelsGrid.GetComponent<RectTransform>();

            float totalWidth = gridRect.rect.max.x - gridRect.rect.min.x;

            float size = (9 * totalWidth) / ((10 * zoomLevel) + 1);
            float padding = size / 9f;

            levelsGrid.cellSize = Vector2.one * size;
            levelsGrid.padding.left = Mathf.RoundToInt(padding);
            levelsGrid.padding.right = Mathf.RoundToInt(padding);
            levelsGrid.padding.top = Mathf.RoundToInt(padding * (3.5f / 2f));
            levelsGrid.padding.bottom = Mathf.RoundToInt(padding);
            levelsGrid.spacing = Vector2.one * padding;

            foreach (IconInfo iconInfo in allIconInfo.Values)
            {
                if (iconInfo.levelIcon != null || iconInfo.folderIcon != null)
                {
                    iconInfo.gameObject.transform.localScale = Vector3.one * (levelsGrid.cellSize.x / 180f);
                }
            }
        }

        private IEnumerator ScrollToPosition(float normalizedPos)
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            levelsScrollRect.verticalNormalizedPosition = normalizedPos;
        }

        public IEnumerator FadeInMusic()
        {
            if (fadeOutCoroutine != null)
            {
                StopCoroutine(fadeOutCoroutine);
            }

            float normalizedTime = 0;
            float currentVolume = bgmSource.volume;
            float timeStart = (float)AudioSettings.dspTime;

            while (normalizedTime < 1f)
            {
                normalizedTime = Mathf.InverseLerp(timeStart, timeStart + fadeTime, (float)AudioSettings.dspTime);

                float newVol = Mathf.Lerp(currentVolume, fullVolume, normalizedTime);

                bgmSource.volume = newVol;

                yield return null;
            }

            bgmSource.volume = fullVolume;

            yield return null;
        }

        public IEnumerator FadeOutMusic()
        {
            if (fadeInCoroutine != null)
            {
                StopCoroutine(fadeInCoroutine);
            }

            float normalizedTime = 0;
            float currentVolume = bgmSource.volume;
            float timeStart = (float)AudioSettings.dspTime;

            while (normalizedTime < 1f)
            {
                normalizedTime = Mathf.InverseLerp(timeStart, timeStart + fadeTime, (float)AudioSettings.dspTime);

                float newVol = Mathf.Lerp(currentVolume, 0, normalizedTime);

                bgmSource.volume = newVol;

                yield return null;
            }

            bgmSource.volume = 0;

            yield return null;
        }

        private IEnumerator DownloadFile(string link)
        {
            setupPanel.SetActive(true);
            refreshStatus = 4;
            setupHeader.text = Localization.T("Downloading File...");
            setupStatusLabel.gameObject.SetActive(true);
            setupStatusLabel.text = Localization.T("Download progress");
            setupProgressSlider.gameObject.SetActive(true);
            setupProgressSlider.value = 0;
            setupCloseButton.gameObject.SetActive(false);
            LibraryMenuCursor.instance.HideCursor();

            downloadPercent = 0;

            // https://stackoverflow.com/questions/1105593/get-file-name-from-uri-string-in-c-sharp
            Uri uri;
            if (!Uri.TryCreate(link, UriKind.Absolute, out uri))
            {
                refreshStatus = -1;
                setupHeader.text = Localization.T("Error!");
                setupStatusLabel.text = Localization.T("Not a compatible link");
                setupProgressSlider.gameObject.SetActive(false);
                setupCloseButton.gameObject.SetActive(true);
                LibraryMenuCursor.instance.CursorSelect(setupCloseButton, true);
                yield break;
            }

            string fileName = Path.GetFileName(uri.LocalPath);

            string savePath = Path.Combine(currentFolder, fileName);
            string fileExt = Path.GetExtension(savePath);

            if (!string.Equals(fileExt, ".riq", StringComparison.OrdinalIgnoreCase) && !string.Equals(fileExt, ".briq", StringComparison.OrdinalIgnoreCase))
            {
                refreshStatus = -1;
                setupHeader.text = Localization.T("Error!");
                setupStatusLabel.text = Localization.T("Not a compatible file");
                setupProgressSlider.gameObject.SetActive(false);
                setupCloseButton.gameObject.SetActive(true);
                LibraryMenuCursor.instance.CursorSelect(setupCloseButton, true);
                yield break;
            }

            bool validPath = false;

            while (!validPath)
            {
                if (!File.Exists(savePath))
                {
                    validPath = true;
                }
                else
                {
                    savePath = Path.Combine(currentFolder, Path.GetFileNameWithoutExtension(savePath));
                    savePath += " copy" + fileExt;
                }
            }

            using (UnityWebRequest www = UnityWebRequest.Get(link))
            {
                www.downloadHandler = new DownloadHandlerBuffer();

                www.SendWebRequest();
                while (!www.isDone)
                {
                    downloadPercent = www.downloadProgress;
                    yield return null;
                }

                if (www.result != UnityWebRequest.Result.Success)
                {
                    refreshStatus = -1;
                    setupHeader.text = Localization.T("Error!");
                    setupStatusLabel.text = Localization.T("Could not download file");
                    setupProgressSlider.gameObject.SetActive(false);
                    setupCloseButton.gameObject.SetActive(true);
                    LibraryMenuCursor.instance.CursorSelect(setupCloseButton, true);
                }
                else
                {
                    File.WriteAllBytes(savePath, www.downloadHandler.data);

                    if (string.Equals(fileExt, ".riq", StringComparison.OrdinalIgnoreCase))
                    {
                        _ = ImportProcess(new string[] { savePath }, true);
                    }
                    else if (string.Equals(fileExt, ".briq", StringComparison.OrdinalIgnoreCase))
                    {
                        _ = ImportProcess(new string[] { savePath }, false, true);
                    }
                }
            }

        }

        public void MovePath(string startPath, string targetPath)
        {
            RemoveIconRecursive(startPath);

            if (File.Exists(startPath))
            {
                File.Move(startPath, targetPath);
            }
            else if (Directory.Exists(startPath))
            {
                Directory.Move(startPath, targetPath);
            }

            SelectFolder(Path.GetDirectoryName(currentFolder));
            _ = ImportProcess(new string[] { targetPath }, true);
        }

        public void RemoveIconRecursive(string path)
        {
            foreach (string listedPath in allIconInfo[path].pathsInFolder)
            {
                if (allIconInfo[listedPath].isFolder)
                {
                    RemoveIconRecursive(listedPath);
                }
                else
                {
                    RemoveSingleIcon(listedPath);
                }
            }

            RemoveSingleIcon(path);
        }

        public void RemoveSingleIcon(string path)
        {
            string enclosingPath = Path.GetDirectoryName(path);
            allIconInfo[enclosingPath].pathsInFolder.Remove(path);
            if (enclosingPath != levelsPath)
            {
                allIconInfo[enclosingPath].folderIcon.levelsInFolder.Remove(path);
            }

            Destroy(allIconInfo[path].gameObject);
            allIconInfo.Remove(path);
        }

        #endregion

        #region Buttons

        public void ZoomIn()
        {
            zoomLevel--;
            StartCoroutine(Zoom());
        }

        public void ZoomOut()
        {
            zoomLevel++;
            StartCoroutine(Zoom());
        }

        public void RefreshButton()
        {
            _ = Refresh();
        }

        public void SelectLevel(string path, bool addToCursorStack, bool moveCursor = true)
        {
            selectedLevelPath = path;

            infoPanel.ShowLevelInfo(allIconInfo[path].hash);

            SoundByte.PlayOneShot("ui/UISelect", ui: true);

            if (moveCursor) LibraryMenuCursor.instance.CursorSelect(infoPanel.playButton, addToCursorStack);
        }

        public void SelectFolder(string path, bool selectFirstIcon = true)
        {
            currentFolder = path;

            if (currentFolder == levelsPath)
            {
                pathText.text = Localization.T("Main Library");
                infoPanel.pathText.text = path.Remove(0, Path.GetDirectoryName(levelsPath).Length);
            }
            else if (currentFolder == "Favorites")
            {
                pathText.text = Localization.T("Favorites");
                infoPanel.pathText.text = levelsPath.Remove(0, Path.GetDirectoryName(levelsPath).Length);
            }
            else
            {
                pathText.text = allIconInfo[path].folderIcon.folderName;
                infoPanel.pathText.text = path.Remove(0, Path.GetDirectoryName(levelsPath).Length);
            }

            foreach (KeyValuePair<string, IconInfo> kvp in allIconInfo)
            {
                if (kvp.Value.levelIcon != null || kvp.Value.folderIcon != null)
                {
                    bool shouldBeActive = allIconInfo[currentFolder].pathsInFolder.Contains(kvp.Key);

                    if (shouldBeActive)
                    {
                        if (kvp.Key == "Favorites")
                        {
                            RefreshFavoritesIcon();
                        }
                        else
                        {
                            if (!kvp.Value.gameObject.activeSelf) kvp.Value.gameObject.SetActive(true);
                            if (kvp.Value.isFolder)
                            {
                                kvp.Value.folderIcon.LoadCustomIcon();
                            }
                            else
                            {
                                kvp.Value.levelIcon.LoadCustomIcon();
                            }
                        }
                    }
                    else
                    {
                        if (kvp.Value.gameObject.activeSelf) kvp.Value.gameObject.SetActive(false);
                    }
                }
            }


            if (currentFolder == levelsPath)
            {
                infoPanel.ShowHelpAndStats();
            }
            else
            {
                infoPanel.ShowFolderInfo(currentFolder);
            }


            searchBar.SetTextWithoutNotify("");
            AutoSort();

            IconInfo icon = GetFirstVisibleIcon();

            StartCoroutine(ScrollToPosition(1f));
            if (selectFirstIcon)
            {
                if (icon == null)
                {
                    LibraryMenuCursor.instance.SetSelectable(LibraryMenuCursor.instance.defaultToolbarSelectable);
                    LibraryMenuCursor.instance.levelsInGrid = false;
                }
                else if (LibraryMenuCursor.usingMouse)
                {
                    LibraryMenuCursor.instance.SetSelectable(LibraryMenuCursor.instance.defaultToolbarSelectable);
                    LibraryMenuCursor.instance.levelsInGrid = true;
                }
                else if (icon.isFolder)
                {
                    icon.folderIcon.OnNavigateTo(true, false);
                    LibraryMenuCursor.instance.levelsInGrid = true;
                }
                else
                {
                    icon.levelIcon.OnNavigateTo(true, false);
                    LibraryMenuCursor.instance.levelsInGrid = true;
                }
            }
            else
            {
                if (icon == null)
                {
                    LibraryMenuCursor.instance.levelsInGrid = false;
                }
                else if (icon.isFolder)
                {
                    LibraryMenuCursor.instance.gridSelectable = icon.folderIcon.GetComponent<LibrarySelectable>();
                    LibraryMenuCursor.instance.levelsInGrid = true;
                }
                else
                {
                    LibraryMenuCursor.instance.gridSelectable = icon.levelIcon.GetComponent<LibrarySelectable>();
                    LibraryMenuCursor.instance.levelsInGrid = true;
                }
            }
        }

        private int GetVisibleRowCount()
        {
            int rowNumber = Mathf.CeilToInt((float)visibleLevelPaths.Count / (float)zoomLevel);

            return rowNumber;
        }

        public int GetRowOfIcon(GameObject level)
        {
            visibleIcons = allIconInfo.Where(kvp => kvp.Key != levelsPath).Where(kvp => kvp.Value.gameObject.activeSelf).Select(kvp => kvp.Value.gameObject).OrderBy(obj => obj.transform.GetSiblingIndex()).ToList();
            int levelIndex = visibleIcons.IndexOf(level);
            int rowNumber = Mathf.CeilToInt((float)(levelIndex + 1) / (float)zoomLevel);

            return rowNumber;
        }

        public bool IsIconFullyOnscreen(RectTransform iconRect)
        {
            Vector3[] objectCorners = new Vector3[4];
            iconRect.GetWorldCorners(objectCorners);

            Vector3[] viewportCorners = new Vector3[4];
            levelsGrid.transform.parent.GetComponent<RectTransform>().GetWorldCorners(viewportCorners);

            int visibleCorners = 0;
            for (var i = 0; i < objectCorners.Length; i++)
            {
                if ((objectCorners[i].y >= viewportCorners[0].y) && (objectCorners[i].y <= viewportCorners[1].y))
                {
                    visibleCorners++;
                }
            }
            return visibleCorners == 4;
        }

        public void ScrollToTarget(RectTransform target)
        {
            RectTransform viewport = levelsScrollRect.viewport;

            Vector3[] targetCorners = new Vector3[4];
            target.GetWorldCorners(targetCorners);

            Vector3[] viewportCorners = new Vector3[4];
            viewport.GetWorldCorners(viewportCorners);

            float targetTop = targetCorners[1].y;
            float targetBottom = targetCorners[0].y;

            float viewportTop = viewportCorners[1].y;
            float viewportBottom = viewportCorners[0].y;

            float offset = 0f;

            if (targetTop > viewportTop)
            {
                // scroll up
                offset = viewportTop - targetTop;
            }
            else if (targetBottom < viewportBottom)
            {
                // scroll down
                offset = viewportBottom - targetBottom;
            }

            Canvas.ForceUpdateCanvases();
            Vector3 pos = levelsScrollRect.content.position;

            if (scrollCo != null) StopCoroutine(scrollCo);
            scrollCo = StartCoroutine(ScrollEase(0.25f, pos, pos.y + offset));
        }

        private IEnumerator ScrollEase(float duration, Vector3 startPos, float endPos)
        {
            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                float pos = Util.EasingFunction.EaseOutQuint(startPos.y, endPos, time / duration);
                levelsScrollRect.content.position = new Vector3(startPos.x, pos, startPos.z);
                yield return null;
            }
            levelsScrollRect.content.position = new Vector3(startPos.x, endPos, startPos.z);
            yield return null;
        }

        public GameObject GetGridSelectableLeft(string path)
        {
            int index = visibleLevelPaths.IndexOf(path);

            if (index <= 0) return null;

            IconInfo targetIcon = allIconInfo[visibleLevelPaths[index - 1]];
            if ((targetIcon.isFolder) && (targetIcon.folderIcon.folderPath == "Favorites") && (!favoritesFolderIcon.gameObject.activeSelf))
            {
                return null;
            }

            GameObject leftIcon = allIconInfo[visibleLevelPaths[index - 1]].gameObject;

            return leftIcon;
        }

        public GameObject GetGridSelectableRight(string path)
        {
            int index = visibleLevelPaths.IndexOf(path);

            if (index == visibleLevelPaths.Count - 1) return null;

            IconInfo targetIcon = allIconInfo[visibleLevelPaths[index + 1]];
            if ((targetIcon.isFolder) && (targetIcon.folderIcon.folderPath == "Favorites") && (!favoritesFolderIcon.gameObject.activeSelf))
            {
                return null;
            }

            GameObject rightIcon = allIconInfo[visibleLevelPaths[index + 1]].gameObject;

            return rightIcon;
        }

        public GameObject GetGridSelectableUp(string path)
        {
            int index = visibleLevelPaths.IndexOf(path);

            if (index < zoomLevel) return null;

            IconInfo targetIcon = allIconInfo[visibleLevelPaths[index - zoomLevel]];
            if ((targetIcon.isFolder) && (targetIcon.folderIcon.folderPath == "Favorites") && (!favoritesFolderIcon.gameObject.activeSelf))
            {
                return null;
            }

            GameObject upIcon = allIconInfo[visibleLevelPaths[index - zoomLevel]].gameObject;

            return upIcon;
        }

        public GameObject GetGridSelectableDown(string path)
        {
            int index = visibleLevelPaths.IndexOf(path);

            if (index > visibleLevelPaths.Count - 1 - zoomLevel) return null;

            IconInfo targetIcon = allIconInfo[visibleLevelPaths[index + zoomLevel]];
            if ((targetIcon.isFolder) && (targetIcon.folderIcon.folderPath == "Favorites") && (!favoritesFolderIcon.gameObject.activeSelf))
            {
                return null;
            }

            GameObject downIcon = allIconInfo[visibleLevelPaths[index + zoomLevel]].gameObject;

            return downIcon;
        }

        public void Search(string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                SelectFolder(currentFolder, false);
            }
            else
            {
                List<LibraryLevelIcon> validLevelIcons = GetAllLevelsInFolderAndSubfolders(currentFolder);

                bool anyLevelsInGrid = false;

                List<string> filteredLevelPaths = new();

                foreach (LibraryLevelIcon levelIcon in allIconInfo.Values.Where(iconInfo => !iconInfo.isFolder).Select(iconInfo => iconInfo.levelIcon))
                {
                    if (validLevelIcons.Contains(levelIcon))
                    {
                        bool isVisible = System.Text.RegularExpressions.Regex.IsMatch(GetAllCachedText(allIconInfo[levelIcon.levelPath].hash), searchBar.text, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        levelIcon.gameObject.SetActive(isVisible);
                        if (isVisible)
                        {
                            filteredLevelPaths.Add(levelIcon.levelPath);
                            anyLevelsInGrid = true;
                        }
                    }
                    else
                    {
                        levelIcon.gameObject.SetActive(false);
                    }

                    visibleLevelPaths = filteredLevelPaths.OrderBy(path => sortedPaths.IndexOf(path)).ToList();
                }

                LibraryMenuCursor.instance.levelsInGrid = anyLevelsInGrid;
                if (anyLevelsInGrid)
                {
                    LibraryMenuCursor.instance.gridSelectable = GetFirstVisibleIcon().levelIcon.GetComponent<LibrarySelectable>();
                }
            }
        }

        private List<LibraryLevelIcon> GetAllLevelsInFolderAndSubfolders(string startPath)
        {
            List<LibraryLevelIcon> levelsInFolderAndSubfolders = new();

            foreach (string pathInFolder in allIconInfo[startPath].pathsInFolder)
            {
                IconInfo iconInfo = allIconInfo[pathInFolder];

                if (iconInfo.isFolder)
                {
                    levelsInFolderAndSubfolders.AddRange(GetAllLevelsInFolderAndSubfolders(pathInFolder));
                    iconInfo.folderIcon.gameObject.SetActive(false);
                }
                else
                {
                    levelsInFolderAndSubfolders.Add(iconInfo.levelIcon);
                }
            }

            return levelsInFolderAndSubfolders;
        }

        private string GetAllCachedText(string hash)
        {
            CachedLevelInfo cacheEntry = LibraryCacheManager.cache.cachedInfo[hash];

            return string.Join(' ', new[] { cacheEntry.levelName, cacheEntry.levelCharter, cacheEntry.songArtist, cacheEntry.songName, cacheEntry.SeparateTagsIntoString() });
        }

        private void AutoSort()
        {
            if ((currentFolder == levelsPath) || (currentFolder == "Favorites"))
            {
                print("sorting from settings");
                SortLevels(PersistentDataManager.gameSettings.libSortType, PersistentDataManager.gameSettings.libSortAscend);
                customButton.gameObject.SetActive(false);
            }
            else
            {
                LibraryFolderIcon folderIcon = allIconInfo[currentFolder].folderIcon;

                if (folderIcon.TryGetCustomSort(out List<string> customSortList))
                {
                    customButton.gameObject.SetActive(true);
                    if (folderIcon.folderData.sortType == LibrarySortType.Custom)
                    {
                        SortLevels(folderIcon.folderData.sortType, folderIcon.folderData.sortAscending, customSortList);
                    }
                    else
                    {
                        SortLevels(folderIcon.folderData.sortType, folderIcon.folderData.sortAscending);
                    }
                }
                else
                {
                    if (folderIcon.folderData.sortType == LibrarySortType.Custom)
                    {
                        SortLevels(LibrarySortType.Filename, true);
                        print("couldnt sort custom");
                    }
                    else
                    {
                        SortLevels(folderIcon.folderData.sortType, folderIcon.folderData.sortAscending);
                    }
                }


            }
        }

        public void SelectSortButton()
        {
            LibrarySelectable target = null;

            if (currentSortType == LibrarySortType.LevelName) target = levelNameButton.GetComponent<LibrarySelectable>();
            else if (currentSortType == LibrarySortType.Filename) target = fileNameButton.GetComponent<LibrarySelectable>();
            else if (currentSortType == LibrarySortType.Artist) target = artistButton.GetComponent<LibrarySelectable>();
            else if (currentSortType == LibrarySortType.Charter) target = charterButton.GetComponent<LibrarySelectable>();
            else if (currentSortType == LibrarySortType.Score) target = scoreButton.GetComponent<LibrarySelectable>();
            else if (currentSortType == LibrarySortType.LastPlayed) target = lastPlayedButton.GetComponent<LibrarySelectable>();
            else if (currentSortType == LibrarySortType.Custom) target = customButton.GetComponent<LibrarySelectable>();

            if (customButton.gameObject.activeSelf)
            {
                levelNameButton.navigation = new()
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = customButton,
                    selectOnDown = levelNameButton.navigation.selectOnDown,
                    selectOnLeft = levelNameButton.navigation.selectOnLeft,
                    selectOnRight = levelNameButton.navigation.selectOnRight,
                };
                lastPlayedButton.navigation = new()
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = lastPlayedButton.navigation.selectOnUp,
                    selectOnDown = customButton,
                    selectOnLeft = lastPlayedButton.navigation.selectOnLeft,
                    selectOnRight = lastPlayedButton.navigation.selectOnRight,
                };
            }
            else
            {
                levelNameButton.navigation = new()
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = lastPlayedButton,
                    selectOnDown = levelNameButton.navigation.selectOnDown,
                    selectOnLeft = levelNameButton.navigation.selectOnLeft,
                    selectOnRight = levelNameButton.navigation.selectOnRight,
                };
                lastPlayedButton.navigation = new()
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = lastPlayedButton.navigation.selectOnUp,
                    selectOnDown = levelNameButton,
                    selectOnLeft = lastPlayedButton.navigation.selectOnLeft,
                    selectOnRight = lastPlayedButton.navigation.selectOnRight,
                };
            }


            LibraryMenuCursor.instance.CursorSelect(target, true);
            LibraryMenuCursor.instance.currentPopup = sortPopup;

            sortPopup.ShowPopup();
        }

        public void FolderInfoButton()
        {
            if (currentFolder == levelsPath)
            {
                infoPanel.ShowHelpAndStats();
            }
            else
            {
                infoPanel.ShowFolderInfo(currentFolder);
            }
        }

        public void AddToFavorites(string path)
        {
            if (!allIconInfo["Favorites"].pathsInFolder.Contains(path))
            {
                allIconInfo["Favorites"].pathsInFolder.Add(path);
                LibraryCacheManager.cache.favorites.Add(path);

                if (currentFolder == levelsPath) RefreshFavoritesIcon();
            }
        }

        public void RemoveFromFavorites(string path)
        {
            if (allIconInfo["Favorites"].pathsInFolder.Contains(path))
            {
                allIconInfo["Favorites"].pathsInFolder.Remove(path);
                LibraryCacheManager.cache.favorites.Remove(path);

                if (currentFolder == levelsPath) RefreshFavoritesIcon();
            }
        }

        private void RefreshFavoritesIcon()
        {
            if (allIconInfo["Favorites"].pathsInFolder.Count > 0)
            {
                favoritesFolderIcon.gameObject.SetActive(true);
                if ((currentFolder == levelsPath) && !visibleLevelPaths.Contains("Favorites")) visibleLevelPaths.Insert(0, "Favorites");
            }
            else
            {
                favoritesFolderIcon.gameObject.SetActive(false);
                if ((currentFolder == levelsPath) && visibleLevelPaths.Contains("Favorites")) visibleLevelPaths.Remove("Favorites");
            }
        }

        public void ReturnToPreviousFolder()
        {
            string targetPath;

            if (currentFolder == levelsPath)
            {
                SoundByte.PlayOneShot("ui/Error", ui: true);
                return;
            }
            else if (currentFolder == "Favorites")
            {
                targetPath = levelsPath;
            }
            else
            {
                targetPath = Path.GetDirectoryName(currentFolder);
            }

            SelectFolder(targetPath);
            SoundByte.PlayOneShot("ui/Library/folderBack", volume: 0.75f, ui: true);
        }

        public void NewFolder()
        {
            SoundByte.PlayOneShot("ui/Library/newFolder", ui: true);
            bool folderMade = false;
            string name = "New folder";
            int i = 0;
            while (!folderMade)
            {
                string folder = Path.Combine(currentFolder, name);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                    currentPathsAndHashes.Add(folder, null);
                    allIconInfo.Add(folder, new());
                    allIconInfo[folder].isFolder = true;
                    CreateFolder(folder);
                    allIconInfo[currentFolder].pathsInFolder.Add(folder);
                    allIconInfo[folder].folderIcon.LoadCustomIcon();
                    AutoSort();
                    StartCoroutine(Zoom(true));

                    folderMade = true;
                    return;
                }
                i++;
                name = $"New folder ({i})";
            }
        }

        public void EditFolder()
        {
            if ((currentFolder != levelsPath) && (currentFolder != "Favorites"))
            {
                if (!folderEditor.gameObject.activeSelf)
                {
                    SoundByte.PlayOneShot("ui/Library/button", ui: true);
                    folderEditor.StartEditing(allIconInfo[currentFolder].folderIcon);
                    infoPanel.ShowFolderEditLayout();
                }
            }
            else
            {
                SoundByte.PlayOneShot("ui/Error");
            }
        }

        public void MultiselectLevels()
        {
            bgmSource.PlayOneShot(Resources.Load<AudioClip>("Sfx/perfectMiss"));
            throw new NotImplementedException();
        }

        public void ButtonSound(string soundEffect)
        {
            if (String.IsNullOrEmpty(soundEffect)) soundEffect = "ui/Library/button";
            SoundByte.PlayOneShot(soundEffect, ui: true);
        }

        public void ExitToTitle()
        {
            if (exiting) return;
            exiting = true;
            SoundByte.PlayOneShot("ui/Exit", ui: true);
            PersistentDataManager.SaveSettings();

            fadeOutCoroutine = StartCoroutine(FadeOutMusic());

            GlobalGameManager.LoadScene("Title", 0.35f, 0.5f);
        }

        public void DeleteIconAndFile(string path)
        {
            if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
            {
                Directory.Delete(path, true);
            }
            else
            {
                File.Delete(path);
            }
            RemoveIconRecursive(path);
        }

        public void DeleteSaveDataForLevel(string path)
        {
            LibrarySaveManager.DeleteLevelSaveData(allIconInfo[path].hash);
            LibrarySaveManager.SaveSaveData();

            LibraryLevelIcon[] iconsWithHash = allIconInfo.Values.Where(iconInfo => iconInfo.hash == allIconInfo[path].hash).Select(iconInfo => iconInfo.levelIcon).ToArray();
            foreach (LibraryLevelIcon levelIcon in iconsWithHash)
            {
                levelIcon.SetIconInfo(levelIcon.levelPath, allIconInfo[levelIcon.levelPath].hash);
            }

            SoundByte.PlayOneShot("ui/Library/delete", ui: true);

            SelectLevel(path, false);
        }

        public void DeleteAllSaveData()
        {
            List<string> hashes = LibrarySaveManager.savedata.levelInfo.Keys.ToList();

            foreach (string hash in hashes)
            {
                LibrarySaveManager.DeleteLevelSaveData(hash);
            }

            SoundByte.PlayOneShot("ui/Library/delete", ui: true);

            LibrarySaveManager.SaveSaveData();
        }

        private void DeleteStaleSaveData()
        {
            List<string> hashesPresent = allIconInfo.Values.Where(iconInfo => !iconInfo.isFolder).Select(iconInfo => iconInfo.hash).ToList();
            List<string> hashesMissing = LibrarySaveManager.savedata.levelInfo.Keys.Where(hash => !hashesPresent.Contains(hash)).ToList();

            foreach (string hash in hashesMissing)
            {
                LibrarySaveManager.DeleteLevelSaveData(hash);
            }

            LibrarySaveManager.SaveSaveData();
        }

        public void ImportFilesButton()
        {
            var extensions = new[]
            {
                new ExtensionFilter("Heaven Studio Level", new string[] { "riq" }),
            };

            FileBrowserOpener.OpenFilePanelAsync("Import Levels", "", extensions, true, (string[] paths) =>
            {
                if (paths == null || paths.Length == 0 || string.IsNullOrEmpty(paths[0])) return;

                _ = ImportProcess(paths);
            });
        }

        public void ImportBriqButton()
        {
            var extensions = new[]
            {
                new ExtensionFilter("Heaven Studio Level Pack", new string[] { "briq" }),
            };

            FileBrowserOpener.OpenFilePanelAsync("Import Levels", "", extensions, false, (string[] paths) =>
            {
                if (paths == null || paths.Length == 0 || string.IsNullOrEmpty(paths[0])) return;

                _ = ImportProcess(paths);
            });
        }

        public void ImportLinkButton()
        {
            if (string.IsNullOrEmpty(linkInput.text)) return;

            StartCoroutine(DownloadFile(linkInput.text));
            linkInput.text = "";
        }

        public void ImportButton()
        {
            infoPanel.ShowImportLayout();
            LibraryMenuCursor.instance.CursorSelect(importFilesButton, true);
        }

        public void CloseSetupPanel()
        {
            setupPanel.SetActive(false);
            LibraryMenuCursor.instance.CursorBack();
        }

        public void SettingsPressed()
        {
            if (!settingsPanel.IsOpen)
            {
                settingsPanel.SwitchSettingsDialog();
            }
        }

        public void SettingsClose()
        {
            if (settingsPanel.IsOpen)
            {
                settingsPanel.SwitchSettingsDialog();
            }
        }

        #endregion

        #region Sorting

        public void SortByFilenameButton()
        {
            if (currentSortType == LibrarySortType.Filename)
            {
                SortLevels(LibrarySortType.Filename, !currentlyAscending);
            }
            else
            {
                SortLevels(LibrarySortType.Filename, currentlyAscending);
            }
        }

        public void SortByLevelNameButton()
        {
            if (currentSortType == LibrarySortType.LevelName)
            {
                SortLevels(LibrarySortType.LevelName, !currentlyAscending);
            }
            else
            {
                SortLevels(LibrarySortType.LevelName, currentlyAscending);
            }
        }
        public void SortByCharterButton()
        {
            if (currentSortType == LibrarySortType.Charter)
            {
                SortLevels(LibrarySortType.Charter, !currentlyAscending);
            }
            else
            {
                SortLevels(LibrarySortType.Charter, currentlyAscending);
            }
        }
        public void SortByArtistButton()
        {
            if (currentSortType == LibrarySortType.Artist)
            {
                SortLevels(LibrarySortType.Artist, !currentlyAscending);
            }
            else
            {
                SortLevels(LibrarySortType.Artist, currentlyAscending);
            }
        }
        public void SortByScoreButton()
        {
            if (currentSortType == LibrarySortType.Score)
            {
                SortLevels(LibrarySortType.Score, !currentlyAscending);
            }
            else
            {
                SortLevels(LibrarySortType.Score, currentlyAscending);
            }
        }
        public void SortByLastPlayedButton()
        {
            SortLevels(LibrarySortType.LastPlayed, false);
        }
        public void SortByCustomButton()
        {
            LibraryFolderIcon folderIcon = allIconInfo[currentFolder].folderIcon;
            if (folderIcon.TryGetCustomSort(out List<string> customSortList))
            {
                SortLevels(LibrarySortType.Custom, true, customSortList);
            }
            else
            {
                SortLevels(LibrarySortType.Filename, true);
            }
        }

        private void SortLevels(LibrarySortType sortType, bool ascending, List<string> customSortList = null)
        {
            print($"Sorting {sortType} by {ascending}");
            List<LibraryLevelIcon> orderedList = new();
            List<LibraryLevelIcon> allLevelIcons = allIconInfo.Values.Where(iconInfo => !iconInfo.isFolder).Select(iconInfo => iconInfo.levelIcon).ToList();
            int folderIconCount = allIconInfo.Count - allLevelIcons.Count;

            if (sortType == LibrarySortType.Filename)
            {
                orderedList = allLevelIcons.OrderBy(level => level.fileName).ToList();
            }
            else if (sortType == LibrarySortType.LevelName)
            {
                orderedList = allLevelIcons.OrderBy(level => level.GetLevelDisplayName()).ThenBy(level => level.fileName).ToList();
            }
            else if (sortType == LibrarySortType.Charter)
            {
                orderedList = allLevelIcons.OrderBy(level => LibraryCacheManager.cache.cachedInfo[allIconInfo[level.levelPath].hash].levelCharter).ThenBy(level => level.GetLevelDisplayName()).ThenBy(level => level.fileName).ToList();
            }
            else if (sortType == LibrarySortType.Artist)
            {
                orderedList = allLevelIcons.OrderBy(level => LibraryCacheManager.cache.cachedInfo[allIconInfo[level.levelPath].hash].songArtist).ThenBy(level => level.GetLevelDisplayName()).ThenBy(level => level.fileName).ToList();
            }
            else if (sortType == LibrarySortType.Score)
            {
                orderedList = allLevelIcons.OrderBy(GetLevelScore).ThenBy(level => level.GetLevelDisplayName()).ThenBy(level => level.fileName).ToList();
            }
            else if (sortType == LibrarySortType.LastPlayed)
            {
                orderedList = allLevelIcons.OrderBy(GetLevelLastPlayedTime).ThenBy(level => level.GetLevelDisplayName()).ThenBy(level => level.fileName).ToList();
                ascending = false;
            }
            else if (sortType == LibrarySortType.Custom)
            {
                List<LibraryLevelIcon> customSortedIcons = allLevelIcons.Where(level => allIconInfo[currentFolder].pathsInFolder.Contains(level.levelPath))
                                                                        .Where(level => customSortList.Contains(Path.GetFileNameWithoutExtension(level.fileName)))
                                                                        .ToList();

                List<LibraryLevelIcon> notSortedIcons = allLevelIcons.Where(level => !allIconInfo[currentFolder].pathsInFolder.Contains(level.levelPath) || !customSortList.Contains(Path.GetFileNameWithoutExtension(level.fileName)))
                                                                     .ToList();

                customSortedIcons = customSortedIcons.OrderBy(level => customSortList.IndexOf(Path.GetFileNameWithoutExtension(level.fileName))).ToList();
                notSortedIcons = notSortedIcons.OrderBy(level => level.GetLevelDisplayName()).ThenBy(level => level.fileName).ToList();

                orderedList.AddRange(customSortedIcons);
                orderedList.AddRange(notSortedIcons);
                ascending = true;
            }

            if (!ascending)
            {
                orderedList.Reverse();
            }

            List<LibraryLevelIcon> activeIcons = orderedList.Where(icon => allIconInfo[currentFolder].pathsInFolder.Contains(icon.levelPath)).ToList();

            for (int i = 0; i < activeIcons.Count; i++)
            {
                activeIcons[i].transform.SetSiblingIndex(i + folderIconCount);
            }

            currentSortType = sortType;

            if ((currentFolder == levelsPath) || (currentFolder == "Favorites"))
            {
                PersistentDataManager.gameSettings.libSortType = sortType;

                if ((currentSortType != LibrarySortType.LastPlayed) || (currentSortType != LibrarySortType.Custom))
                {
                    currentlyAscending = ascending;
                    PersistentDataManager.gameSettings.libSortAscend = ascending;
                }
            }
            else
            {
                LibraryFolderIcon folderIcon = allIconInfo[currentFolder].folderIcon;

                folderIcon.folderData.sortType = sortType;

                if ((currentSortType != LibrarySortType.LastPlayed) || (currentSortType != LibrarySortType.Custom))
                {
                    currentlyAscending = ascending;
                    folderIcon.folderData.sortAscending = ascending;
                }

                folderIcon.SaveFolderData(Path.Combine(currentFolder, "folderData.json"));
            }

            SortFolders();

            sortedPaths = orderedList.Select(icon => icon.levelPath).ToList();
            List<string> pathsInFolder = allIconInfo[currentFolder].pathsInFolder;
            visibleLevelPaths = pathsInFolder.OrderBy(path => allIconInfo[path].gameObject.transform.GetSiblingIndex()).ToList();
            if (visibleLevelPaths.Contains("Favorites") && !favoritesFolderIcon.gameObject.activeSelf) visibleLevelPaths.Remove("Favorites");

            SetSortIcon(sortType, ascending);
        }

        private void SortFolders()
        {
            List<LibraryFolderIcon> orderedList = new();
            List<LibraryFolderIcon> allFolderIcons = allIconInfo.Values.Where(iconInfo => (iconInfo.folderIcon != null) && iconInfo.isFolder).Select(iconInfo => iconInfo.folderIcon).ToList();
            orderedList = allFolderIcons.OrderBy(folder => folder.folderName).ToList();

            for (int i = 0; i < orderedList.Count; i++)
            {
                orderedList[i].transform.SetSiblingIndex(i);
            }

            favoritesFolderIcon.transform.SetSiblingIndex(0);
        }

        private void SetSortIcon(LibrarySortType sortType, bool ascending)
        {
            Sprite icon = levelNameIcon;
            Vector3 arrowScale = ascending ? new(1, 1, 1) : new(1, -1, 1);

            sortButtonArrow.color = new(1, 1, 1, 1);

            levelNameArrow.color = new(1, 1, 1, 0);
            fileNameArrow.color = new(1, 1, 1, 0);
            artistArrow.color = new(1, 1, 1, 0);
            charterArrow.color = new(1, 1, 1, 0);
            scoreArrow.color = new(1, 1, 1, 0);

            if (sortType == LibrarySortType.LevelName)
            {
                icon = levelNameIcon;
                levelNameArrow.transform.localScale = arrowScale;
                levelNameArrow.color = new(1, 1, 1, 1);
            }
            else if (sortType == LibrarySortType.Filename)
            {
                icon = fileNameIcon;
                fileNameArrow.transform.localScale = arrowScale;
                fileNameArrow.color = new(1, 1, 1, 1);
            }
            else if (sortType == LibrarySortType.Artist)
            {
                icon = artistIcon;
                artistArrow.transform.localScale = arrowScale;
                artistArrow.color = new(1, 1, 1, 1);
            }
            else if (sortType == LibrarySortType.Charter)
            {
                icon = charterIcon;
                charterArrow.transform.localScale = arrowScale;
                charterArrow.color = new(1, 1, 1, 1);
            }
            else if (sortType == LibrarySortType.Score)
            {
                icon = scoreIcon;
                scoreArrow.transform.localScale = arrowScale;
                scoreArrow.color = new(1, 1, 1, 1);
            }
            else if (sortType == LibrarySortType.LastPlayed)
            {
                icon = lastPlayedIcon;
                sortButtonArrow.color = new(1, 1, 1, 0);
            }
            else if (sortType == LibrarySortType.Custom)
            {
                icon = customIcon;
                sortButtonArrow.color = new(1, 1, 1, 0);
            }

            sortButtonAspectRatioFitter.aspectRatio = (float)icon.rect.width / (float)icon.rect.height;
            sortButtonIcon.sprite = icon;

            sortButtonArrow.transform.localScale = arrowScale * 0.75f;

        }

        private int GetLevelScore(LibraryLevelIcon level)
        {
            string hash = allIconInfo[level.levelPath].hash;
            return LibrarySaveManager.savedata.levelInfo.ContainsKey(hash) ? LibrarySaveManager.savedata[hash].score : -1;
        }
        public static int GetLevelScore(string hash)
        {
            return LibrarySaveManager.savedata.levelInfo.ContainsKey(hash) ? LibrarySaveManager.savedata[hash].score : -1;
        }

        private DateTime GetLevelLastPlayedTime(LibraryLevelIcon level)
        {
            string hash = allIconInfo[level.levelPath].hash;

            return LibrarySaveManager.savedata.levelInfo.ContainsKey(hash)
                ? DateTime.Parse(LibrarySaveManager.savedata[hash].playTime)
                : DateTime.MinValue;
        }

        #endregion

        #region Egg
        private IEnumerator Egg()
        {
            PLACE_MUSEUM.parent.gameObject.SetActive(true);
            bgmSource.clip = AUDIO_MUSEUM;
            fadeInCoroutine = StartCoroutine(FadeInMusic());
            bgmSource.Play();

            yield return new WaitForSeconds(3);

            bgmSource.Stop();
            DEVICE_AUDIOPLAYER.PlayOneShot(AUDIO_DASH);
            DEVICE_IMPACT.SetActive(false);

            Coroutine carMoveCoroutine = StartCoroutine(CarMove());
            Coroutine carAnimateCoroutine = StartCoroutine(CarAnimate());

            yield return new WaitUntil(() => PLACE_MUSEUM.localScale.x <= 0);
            yield return new WaitForSeconds(3);

            StopCoroutine(carMoveCoroutine);
            StopCoroutine(carAnimateCoroutine);
            StopCoroutine(fadeInCoroutine);
            bgmSource.clip = music;
            bgmSource.volume = 0;
            bgmSource.Play();
            fadeInCoroutine = StartCoroutine(FadeInMusic());

            PLACE_MUSEUM.parent.gameObject.SetActive(false);

            _ = InitializeLibrary();
        }

        private IEnumerator CarMove()
        {
            float carMaxSpeed = -16000;
            float carSpeed = carMaxSpeed;
            RectTransform carRect = DEVICE_CAR.GetComponent<RectTransform>();
            RectTransform bgRect = PLACE_MUSEUM.GetComponentInChildren<RectTransform>();

            while (true)
            {
                DEVICE_CAR.transform.localPosition = new(DEVICE_CAR.transform.localPosition.x + (carSpeed * Time.deltaTime),
                                                         DEVICE_CAR.transform.localPosition.y,
                                                         DEVICE_CAR.transform.localPosition.z);

                Vector3[] museumCorners = new Vector3[4];
                PLACE_MUSEUM.GetWorldCorners(museumCorners);

                Vector3[] carCorners = new Vector3[4];
                carRect.GetWorldCorners(carCorners);

                // print($"{carCorners[0].x}, {museumCorners[2].x}");

                if ((PLACE_MUSEUM.localScale.x > 0) && (carCorners[0].x <= museumCorners[2].x))
                {
                    carSpeed = 2000;
                    PLACE_MUSEUM.localScale = new(PLACE_MUSEUM.localScale.x - 0.1f,
                                                  PLACE_MUSEUM.localScale.y,
                                                  PLACE_MUSEUM.localScale.z);
                    StartCoroutine(CarImpact());
                }


                carSpeed -= 64000f * Time.deltaTime;
                if (carSpeed < carMaxSpeed)
                {
                    carSpeed = carMaxSpeed;
                }

                yield return null;
            }
        }

        private IEnumerator CarAnimate()
        {
            float time = 0;
            bool frame1 = false;

            while (true)
            {
                time += Time.deltaTime;

                if (time >= 0.15f)
                {
                    while (time >= 0.15f)
                    {
                        time -= 0.15f;
                    }

                    frame1 = !frame1;

                    DEVICE_CAR.sprite = frame1 ? IMAGE_CAR_1 : IMAGE_CAR_0;
                }

                yield return null;
            }
        }

        private IEnumerator CarImpact()
        {
            DEVICE_AUDIOPLAYER.PlayOneShot(AUDIO_IMPACT);
            DEVICE_IMPACT.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            DEVICE_IMPACT.SetActive(false);
        }
        #endregion

        public enum LibrarySortType
        {
            Filename,
            LevelName,
            Charter,
            Artist,
            Score,
            LastPlayed,
            Custom,
        }

        public class IconInfo
        {
            public string hash;
            public bool isFolder = false;
            public LibraryLevelIcon levelIcon;
            public LibraryFolderIcon folderIcon;
            public List<string> pathsInFolder = new();

            public GameObject gameObject { get => GetGameObject(); }

            private GameObject GetGameObject()
            {
                if (isFolder)
                {
                    return folderIcon.gameObject;
                }
                else
                {
                    return levelIcon.gameObject;
                }
            }
        }
    }
}