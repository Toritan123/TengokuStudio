using System;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB;

using HeavenStudio.Common;
using HeavenStudio.Editor.Track;
using HeavenStudio.StudioDance;
using HeavenStudio.Util;

using Jukebox;
using UnityEditor;
using System.Linq;
using BurstLinq;
using HeavenStudio.InputSystem;
using UnityEngine.U2D;

#if UNITY_EDITOR
using UnityEditor.U2D;
using UnityEditor.Sprites;
#endif

namespace HeavenStudio.Editor
{
    public class Editor : MonoBehaviour
    {
        private GameInitializer Initializer;

        [SerializeField] public Canvas MainCanvas;
        private CanvasScaler MainCanvasScaler;
        [SerializeField] public Camera EditorCamera;

        [Header("Rect")]
        [SerializeField] private RenderTexture ScreenRenderTexture;
        [SerializeField] private RawImage Screen;
        [SerializeField] private RectTransform GridGameSelectorRect;
        public RectTransform eventSelectorBG;

        [Header("Components")]
        [SerializeField] private Timeline Timeline;
        [SerializeField] private TMP_Text GameEventSelectorTitle;
        [SerializeField] private TMP_Text BuildDateDisplay;
        [SerializeField] public StudioDanceManager StudioDanceManager;

        [Header("Toolbar")]
        [SerializeField] private Button NewBTN;
        [SerializeField] private Button OpenBTN;
        [SerializeField] private Button SaveBTN;
        [SerializeField] private Button UndoBTN;
        [SerializeField] private Button RedoBTN;
        [SerializeField] private Button CopyBTN;
        [SerializeField] private Button PasteBTN;
        [SerializeField] private Button MusicSelectBTN;
        [SerializeField] private Button FullScreenBTN;
        [SerializeField] private Button TempoFinderBTN;
        [SerializeField] private Button SnapDiagBTN;
        [SerializeField] private Button ChartParamBTN;
        [SerializeField] private Button SortAlphabetBTN;
        [SerializeField] private Button SortFavoritesBTN;
        [SerializeField] private Button SortChronologicBTN;
        [SerializeField] private TMP_InputField SearchBar;

        [Header("Confirm Quit")]
        [SerializeField] private GameObject _confirmQuitMain;
        [SerializeField] private Button _quitYes;
        [SerializeField] private Button _quitNo;
        [SerializeField] private GameObject _confirmSaveMain;
        [SerializeField] private Toggle _confirmSaveShowToggle;
        [SerializeField] private GameObject _confirmLoadAutosaveMain;
        [SerializeField] private TMP_Text _confirmLoadAutosaveLabel;
        private string _confirmSavePath;
        public static bool hideSavePopUp = false;

        [SerializeField] private Button EditorThemeBTN;
        [SerializeField] private Button EditorSettingsBTN;

        [Header("Dialogs")]
        [SerializeField] private Dialog[] Dialogs;

        [Header("Tooltip")]
        public TMP_Text tooltipText;

        [Header("Properties")]
        //private bool changedMusic = false;    Unused value - Marc
        //private bool loadedMusic = false;    Unused value - Marc
        public string currentRemixPath = "";
        public float autosaveTimer = 0;
        private string remixName = "";
        public bool fullscreen;
        public bool discordDuringTesting = false;
        public bool canSelect = true;
        public bool editingInputField = false;
        public bool inAuthorativeMenu = false;
        public bool isCursorEnabled = true;
        public bool isDiscordEnabled = true;

        public string currentFont = "Kurokane";

        public bool isShortcutsEnabled { get { return (!inAuthorativeMenu) && (!editingInputField); } }

        private Vector2 lastScreenSize = Vector2.zero;

        public static bool loadFromLibrary = false;
        private bool returnToLibrary = false;
        public static string libraryLevelPath = "";

        public static Editor instance { get; private set; }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            Initializer = GetComponent<GameInitializer>();
            MainCanvasScaler = MainCanvas.GetComponent<CanvasScaler>();
            canSelect = true;
        }

        public void Init()
        {
            GameManager.instance.StaticCamera.targetTexture = ScreenRenderTexture;
            GameManager.instance.CursorCam.targetTexture = ScreenRenderTexture;
            GlobalGameManager.perfectCampaign = false;
            Screen.texture = ScreenRenderTexture;

            GlobalGameManager.ResetGameRenderTexture(0.5f);

            GameManager.instance.Init();
            Timeline.Init();

            InitIcons();

            UpdateEditorStatus(true);

#if HEAVENSTUDIO_PROD
            BuildDateDisplay.text = GlobalGameManager.friendlyReleaseName;
#else
            BuildDateDisplay.text = GlobalGameManager.buildTime;
#endif

            var settings = PersistentDataManager.gameSettings;
            isCursorEnabled = settings.editorCursorEnable;
            isDiscordEnabled = settings.discordRPCEnable;
            GameManager.instance.CursorCam.enabled = isCursorEnabled;

            if (!settings.hasOpenedEditor)
            {
                GlobalGameManager.ShowErrorMessage("Welcome to the Editor!", $"<size=110%>Are you ready to make your own levels?\nTo start, pick a game and <u>drag</u> the <i>Switch Game</i> to the timeline.\n\nFor more information, click the Help button in the top right.\n\nHave fun!");
                settings.hasOpenedEditor = true;
                PersistentDataManager.SaveSettings();
            }

            if (!settings.autosaveSetToDefault)
            {
                settings.autosaveInterval = 5;
                settings.autosaveSetToDefault = true;
            }

            if (loadFromLibrary)
            {
                loadFromLibrary = false;
                returnToLibrary = true;
                StartCoroutine(OpenLibraryLevel());
            }
            else if (settings.lastAutoSave > settings.lastManualSave && !string.IsNullOrEmpty(settings.lastAutoSavePath))
            {
                ShowAutosaveDialog();
            }
        }

        public void InitIcons()
        {
            foreach (var minigame in EventCaller.instance.minigames.Values)
                AddIcon(minigame);
            GridGameSelectorRect.GetComponent<GridGameSelector>().Sort(PersistentDataManager.gameSettings.sortMode);

        }

        public void AddIcon(Minigames.Minigame minigame)
        {
            if (minigame.hidden) return;
            GameObject GameIcon_ = Instantiate(GridGameSelectorRect.GetChild(0).gameObject, GridGameSelectorRect);
            GameIcon_.GetComponent<Image>().sprite = GameIcon(minigame.name);
            GameIcon_.GetComponent<GridGameSelectorGame>().MaskTex = GameIconMask(minigame.name);
            GameIcon_.GetComponent<GridGameSelectorGame>().UnClickIcon();
            GameIcon_.SetActive(true);
            GameIcon_.name = minigame.name;

            var ggs = GridGameSelectorRect.GetComponent<GridGameSelector>();
            (minigame.fxOnly ? ggs.fxActive : ggs.mgsActive).Add(GameIcon_.GetComponent<RectTransform>());
        }


        public void ShowQuitPopUp(bool show)
        {
            if (fullscreen) Fullscreen();
            _confirmQuitMain.SetActive(show);
            SetAuthoritiveMenu(show);
        }

        public void ShowSavePopUp(bool show)
        {
            if (fullscreen) Fullscreen();
            if (_confirmSaveShowToggle.isOn) hideSavePopUp = true;
            _confirmSaveMain.SetActive(show);
            SetAuthoritiveMenu(show);
        }

        public void SaveLibraryLevel(bool save)
        {
            if (save) SaveRemixFile(_confirmSavePath, true);
            _confirmSavePath = null;
        }

        public bool ShouldQuit = false;

        public void QuitGame()
        {
            ShouldQuit = true;
            Application.Quit();
        }

        public void Update()
        {
            if (PersistentDataManager.gameSettings.autosaveInterval > 0)
            {
                string path = currentRemixPath;
                if (string.IsNullOrEmpty(path)) path = Application.persistentDataPath + "/Unsaved level";

                autosaveTimer += Time.deltaTime;
                if ((autosaveTimer >= PersistentDataManager.gameSettings.autosaveInterval * 60) && !Conductor.instance.isPlaying)
                {
                    if (GameManager.instance.Beatmap.Entities.Count > 0)
                    {
                        SaveRemixFile(path + ".bak", true, true, true);
                    }
                    autosaveTimer = 0;
                }
            }

            if (PersistentDataManager.gameSettings.scaleWScreenSize)
            {
                MainCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                float scaleFactor = 1.0f - (0.125f * PersistentDataManager.gameSettings.editorScale);
                MainCanvasScaler.referenceResolution = new Vector2(1920, 1080) * scaleFactor;
                MainCanvasScaler.matchWidthOrHeight = 1f;
            }
            else
            {
                MainCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                float scaleFactor = 1.0f + (0.25f * PersistentDataManager.gameSettings.editorScale);
                MainCanvas.scaleFactor = scaleFactor;
            }
        }

        public void LateUpdate()
        {
            if (lastScreenSize != new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height))
            {
                // Timeline.OnScreenResize();
            }
            lastScreenSize = new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height);

            #region Keyboard Shortcuts
            if (isShortcutsEnabled)
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    Fullscreen();
                }

                if (Input.GetKeyDown(KeyCode.Escape) && fullscreen)
                {
                    Fullscreen(); // Esc can only exit fullscreen, not enter it
                }

                if ((Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace)) && !fullscreen && !EventParameterManager.instance.active)
                {
                    CommandManager.Instance.AddCommand(new Commands.Delete(Selections.instance.eventsSelected.Select(c => c.entity.guid).ToList()));
                    GridGameSelector.instance.Sort(PersistentDataManager.gameSettings.sortMode);
                }

                if (Input.GetKey(InputKeyboard.MODIFIER) && !fullscreen && !EventParameterManager.instance.active)
                {
                    if (Input.GetKeyDown(KeyCode.Z))
                    {
                        if (Input.GetKey(KeyCode.LeftShift))
                            CommandManager.Instance.RedoCommand();
                        else
                            CommandManager.Instance.UndoCommand();
                    }
                    else if (Input.GetKeyDown(KeyCode.Y))
                    {
                        CommandManager.Instance.RedoCommand();
                    }
                    else if (Input.GetKeyDown(KeyCode.C))
                    {
                        Timeline.instance.CopySelected();
                    }
                    else if (Input.GetKeyDown(KeyCode.X))
                    {
                        Timeline.instance.CopySelected();
                        CommandManager.Instance.AddCommand(new Commands.Delete(Selections.instance.eventsSelected.Select(c => c.entity.guid).ToList()));
                        GridGameSelector.instance.Sort(PersistentDataManager.gameSettings.sortMode);
                    }
                    else if (Input.GetKeyDown(KeyCode.V))
                    {
                        Timeline.instance.Paste();
                    }

                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        if (Input.GetKeyDown(KeyCode.I))
                        {
                            Timeline.instance.InsertSpace();
                        }
                        else if (Input.GetKeyDown(KeyCode.U))
                        {
                            Timeline.instance.DeleteSpace();
                        }
                    }
                }

                if (Input.GetKey(InputKeyboard.MODIFIER))
                {
                    if (Input.GetKeyDown(KeyCode.N))
                    {
                        if (fullscreen) Fullscreen();
                        NewBTN.onClick.Invoke();
                    }
                    else if (Input.GetKeyDown(KeyCode.O))
                    {
                        OpenRemix();
                    }
                    else if (Input.GetKey(KeyCode.LeftAlt))
                    {
                        if (Input.GetKeyDown(KeyCode.S))
                        {
                            SaveRemix(true);
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.S))
                    {
                        SaveRemix(false);
                    }

                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        if (Input.GetKeyDown(KeyCode.D))
                        {
                            ToggleDebugCam();
                        }
                    }
                }
            }
            #endregion

            // Undo+Redo
            UndoBTN.transform.GetChild(0).GetComponent<Image>().color = CommandManager.Instance.CanUndo() ? Color.white : Color.gray;
            RedoBTN.transform.GetChild(0).GetComponent<Image>().color = CommandManager.Instance.CanRedo() ? Color.white : Color.gray;

            // Copy+Paste
            CopyBTN.transform.GetChild(0).GetComponent<Image>().color = Selections.instance.eventsSelected.Count > 0 ? Color.white : Color.gray;
            PasteBTN.transform.GetChild(0).GetComponent<Image>().color = Timeline.instance.CopiedEntities.Count > 0 ? Color.white : Color.gray;
        }

        public static Sprite GameIcon(string name)
        {
            if ((!PersistentDataManager.gameSettings.simpleIcons) && (name == "rhythmTweezers"))
            {
                float rand = UnityEngine.Random.Range(0f, 1000f);
                if (rand <= 1f)
                {
                    name = "rhythmTweezers_alt";
                }
            }

            if (PersistentDataManager.gameSettings.simpleIcons) return Resources.Load<Sprite>($"Sprites/Editor/SimpleIcons/{name}") ?? Resources.Load<Sprite>($"Sprites/Editor/SimpleIcons/noGame");
            else { return Resources.Load<Sprite>($"Sprites/Editor/GameIcons/{name}") ?? Resources.Load<Sprite>($"Sprites/Editor/GameIcons/noGame"); }
        }

        public static Texture GameIconMask(string name)
        {
            if (PersistentDataManager.gameSettings.simpleIcons) return Resources.Load<Texture>($"Sprites/Editor/SimpleIcons/null_mask");
            else { return Resources.Load<Texture>($"Sprites/Editor/GameIcons/{name}_mask"); }
        }

        #region Dialogs

        public void SelectMusic()
        {
            if (Timeline.instance != null)
                Timeline.instance?.Stop(0);
            else
                GameManager.instance.Stop(0);

            var extensions = new[]
            {
                new ExtensionFilter("Music Files", "mp3", "ogg", "wav", "aiff", "aif", "aifc", "flac", "it", "mod", "xm")
            };

#if UNITY_STANDALONE_WINDOWS
            FileBrowserOpener.OpenFilePanelAsync("Open File", "", extensions, false, async (string[] paths) => 
            {
                if (paths.Length > 0)
                {
                    try
                    {
                        if (paths.Length == 0) return;
                        RiqFileHandler.WriteSong(paths[0]);
                        StartCoroutine(LoadMusic());
                        return;
                    }
                    catch (System.Exception e)
                    {
                        Debug.Log($"Error selecting music file: {e.Message}");
                        Debug.LogException(e);
                        GlobalGameManager.ShowErrorMessage("Error", e.Message);
                        return;
                    }
                }
                await Task.Yield();
            } 
            );
#else
            FileBrowserOpener.OpenFilePanelAsync("Open File", "", extensions, false, async (string[] paths) =>
            {
                if (paths.Length > 0)
                {
                    try
                    {
                        if (paths.Length == 0) return;
                        RiqFileHandler.WriteSong(paths[0]);
                        StartCoroutine(LoadMusic());
                        return;
                    }
                    catch (System.Exception e)
                    {
                        Debug.Log($"Error selecting music file: {e.Message}");
                        Debug.LogException(e);
                        GlobalGameManager.ShowErrorMessage("Error", e.Message);
                        return;
                    }
                }
                await Task.Yield();
            }
            );
#endif
        }

        IEnumerator LoadMusic()
        {
            yield return GameManager.instance.LoadMusic();
            Timeline.FitToSong();
            // Timeline.CreateWaveform();
        }

        public void SaveRemix(bool saveAs = true)
        {
            if (!(bool)GameManager.instance.Beatmap["propertiesmodified"])
            {
                foreach (var dialog in Dialogs)
                {
                    if (dialog is not null and RemixPropertiesDialog propDialog)
                    {
                        if (fullscreen) Fullscreen();
                        GlobalGameManager.ShowErrorMessage("Set Level Properties", "Set your level's properties before saving.");
                        propDialog.SwitchPropertiesDialog();
                        propDialog.SetSaveOnClose(true, saveAs);
                        return;
                    }
                }
            }
            else
            {
                if (saveAs || string.IsNullOrEmpty(currentRemixPath))
                {
                    SaveRemixFilePanel();
                }
                else
                {
                    SaveRemixFile(currentRemixPath);
                }
            }
        }

        private void SaveRemixFilePanel()
        {
            var extensions = new[]
            {
                new ExtensionFilter("Heaven Studio Remix File", "riq")
            };

            string name = Regex.Replace(GameManager.instance.Beatmap["remixtitle"], @"<[^>]*>", "");

            FileBrowserOpener.SaveFilePanelAsync("Save Remix As", "", name, extensions, (string path) =>
            {
                if (path != String.Empty)
                {
                    SaveRemixFile(path);
                    currentRemixPath = path;
                }
            });
        }

        private void SaveRemixFile(string path, bool force = false, bool silent = false, bool async = false)
        {
            if (!String.IsNullOrEmpty(PersistentDataManager.gameSettings.libLevelsDirectory) && !hideSavePopUp)
            {
                if (path.Contains(PersistentDataManager.gameSettings.libLevelsDirectory) && System.IO.File.Exists(path) && !force)
                {
                    ShowSavePopUp(true);
                    _confirmSavePath = path;
                    return;
                }
            }
            try
            {
                RiqFileHandler.WriteRiq(GameManager.instance.Beatmap);
                if (async) Task.Run(() => RiqFileHandler.PackRiq(path, false));
                else RiqFileHandler.PackRiq(path, false);
                Debug.Log("Packed RIQ successfully!\nPath: " + path);

                if (!silent) 
                {
                    SoundByte.PlayOneShot("save", ui: true);
                    PersistentDataManager.gameSettings.lastManualSave = DateTime.Now;
                }
                else
                {
                    PersistentDataManager.gameSettings.lastAutoSave = DateTime.Now;
                    PersistentDataManager.gameSettings.lastAutoSavePath = path;
                }

                return;
            }
            catch (System.Exception e)
            {
                Debug.Log($"Error packing RIQ: {e.Message}");
                return;
            }
        }

        public void NewRemix()
        {
            if (Timeline.instance != null)
                Timeline.instance.Stop(0);
            else
                GameManager.instance.Stop(0);
            LoadRemix(true);
        }

        public void LoadRemix(bool create = false)
        {
            if (fullscreen) Fullscreen();
            if (create)
            {
                GameManager.instance.NewRemix();
                currentRemixPath = string.Empty;
            }
            else
            {
                GameManager.instance.LoadRemix(true);
            }
            Timeline.instance.LoadRemix();
            Timeline.FitToSong();
        }

        public void OpenRemix()
        {
            if (fullscreen) Fullscreen();
            GameManager.instance.Stop(0);

            var extensions = new[]
            {
                #if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                new ExtensionFilter("Heaven Studio Remix File", "riq", "bak"),
                #else
                new ExtensionFilter("Heaven Studio Remix File", "riq", "riq.bak"),
                #endif
                new ExtensionFilter("All Files", "*"),
            };

            FileBrowserOpener.OpenFilePanelAsync("Open Remix", "", extensions, false, (string[] paths) =>
            {
                var path = Path.Combine(paths);
                if (path == string.Empty) return;

                try
                {
                    string tmpDir = RiqFileHandler.ExtractRiq(path);
                    Debug.Log("Imported RIQ successfully!");
                    LoadRemix();
                }
                catch (System.Exception e)
                {
                    Debug.Log($"Error importing RIQ: {e.Message}");
                    Debug.LogException(e);
                    GlobalGameManager.ShowErrorMessage("Error Loading RIQ", e.Message + "\n\n" + e.StackTrace);
                    return;
                }

                StartCoroutine(LoadMusic());

                if (path.EndsWith(".bak", System.StringComparison.OrdinalIgnoreCase))
                {
                    currentRemixPath = path[..^4];
                    CommandManager.Instance.Clear();
                    return;
                }
                currentRemixPath = path;
                autosaveTimer = 0;
                CommandManager.Instance.Clear();
                //    remixName = Path.GetFileName(path);
                //    UpdateEditorStatus(false);
            });
        }

        #endregion

        private IEnumerator OpenLibraryLevel()
        {
            yield return new WaitUntil(() => Timeline.instance.editorTheme.tracksInit == true);
            try
            {
                string tmpDir = RiqFileHandler.ExtractRiq(libraryLevelPath);
                Debug.Log("Imported RIQ successfully!");
                LoadRemix();
            }
            catch (System.Exception e)
            {
                Debug.Log($"Error importing RIQ: {e.Message}");
                Debug.LogException(e);
                GlobalGameManager.ShowErrorMessage("Error Loading RIQ", e.Message + "\n\n" + e.StackTrace);
                yield break;
            }
            StartCoroutine(LoadMusic());
            currentRemixPath = libraryLevelPath;
            CommandManager.Instance.Clear();
        }

        public void Fullscreen()
        {
            MainCanvas.gameObject.SetActive(fullscreen);
            if (fullscreen == false)
            {
                MainCanvas.enabled = false;
                EditorCamera.enabled = false;
                GameManager.instance.StaticCamera.targetTexture = null;
                GameManager.instance.CursorCam.enabled = (PlayerInput.CurrentControlStyle == InputSystem.InputController.ControlStyles.Touch)
                    && isCursorEnabled;
                GameManager.instance.CursorCam.targetTexture = null;

                GameManager.instance.CursorCam.rect = new Rect(0, 0, 1, 1);
                fullscreen = true;
                GlobalGameManager.ResetGameRenderTexture(1f);
            }
            else
            {
                MainCanvas.enabled = true;
                EditorCamera.enabled = true;
                GameManager.instance.StaticCamera.targetTexture = ScreenRenderTexture;
                GameManager.instance.CursorCam.enabled = true && isCursorEnabled;
                GameManager.instance.CursorCam.targetTexture = ScreenRenderTexture;
                fullscreen = false;
                GlobalGameManager.ResetGameRenderTexture(0.5f);

                GameCamera.instance.camera.rect = new Rect(0, 0, 1, 1);
                GameManager.instance.CursorCam.rect = new Rect(0, 0, 1, 1);
                GameManager.instance.OverlayCamera.rect = new Rect(0, 0, 1, 1);
                EditorCamera.rect = new Rect(0, 0, 1, 1);
            }
            GameManager.instance.CircleCursor.LockCursor(fullscreen);
            Timeline.AutoBtnUpdate();
        }

        private void UpdateEditorStatus(bool updateTime)
        {
            GlobalGameManager.UpdateDiscordStatus($"{remixName}", true, updateTime);
        }

        public void SetGameEventTitle(string txt)
        {
            GameEventSelectorTitle.text = txt;
        }

        public static bool MouseInRectTransform(RectTransform rectTransform)
        {
            return rectTransform.gameObject.activeSelf && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, Editor.instance.EditorCamera);
        }

        public void ReturnToTitle()
        {
            if (returnToLibrary)
            {
                GlobalGameManager.LoadScene("Library", callback: GameManager.instance.EndRemixEvents);
            }
            else
            {
                GlobalGameManager.LoadScene("Title", callback: GameManager.instance.EndRemixEvents);
            }
        }
        
        public void ShowAutosaveDialog(bool show = true)
        {
            _confirmLoadAutosaveMain.SetActive(show);
            SetAuthoritiveMenu(show);
            if (show)
            {
                string levelName = Path.GetFileNameWithoutExtension(PersistentDataManager.gameSettings.lastAutoSavePath);
                string manualSave = PersistentDataManager.gameSettings.lastManualSave.ToString();
                string autoSave = PersistentDataManager.gameSettings.lastAutoSave.ToString();

                if (PersistentDataManager.gameSettings.lastAutoSavePath.Contains(Application.persistentDataPath))
                {
                    manualSave = "N/A";
                }
                
                _confirmLoadAutosaveLabel.text = $"Level: {levelName}\nLast manual save: {manualSave}\nLast autosave: {autoSave}";
            }
        }

        public void LoadAutosave()
        {
            string path = PersistentDataManager.gameSettings.lastAutoSavePath;
            Debug.Log("autosave load for " + path + " now");
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                string tmpDir = RiqFileHandler.ExtractRiq(path);
                Debug.Log("Imported RIQ successfully!");
                LoadRemix();
            }
            catch (System.Exception e)
            {
                Debug.Log($"Error importing RIQ: {e.Message}");
                Debug.LogException(e);
                GlobalGameManager.ShowErrorMessage("Error Loading RIQ", e.Message + "\n\n" + e.StackTrace);
                return;
            }
            StartCoroutine(LoadMusic());
            currentRemixPath = libraryLevelPath;
            CommandManager.Instance.Clear();

            ClearAutosavePath();
        }

        public void ClearAutosavePath()
        {
            PersistentDataManager.gameSettings.lastAutoSavePath = "";
            PersistentDataManager.SaveSettings();
        }


        public void SetAuthoritiveMenu(bool state)
        {
            inAuthorativeMenu = state;
        }

        public void ToggleDebugCam()
        {
            var game = GameManager.instance.minigameObj;

            if (game != null)
            {
                foreach (FreeCam c in game.GetComponentsInChildren<FreeCam>(true))
                {
                    c.enabled = !c.enabled;
                }
            }
        }
    }
}
