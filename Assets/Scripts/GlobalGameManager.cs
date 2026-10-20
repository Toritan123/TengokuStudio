using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

using HeavenStudio.Common;
using HeavenStudio.Util;

namespace HeavenStudio
{
    public class GlobalGameManager : MonoBehaviour
    {
        public static GlobalGameManager instance { get; set; }

        [Header("Loading Screen")]
        [SerializeField] Image fadeImage;
        [SerializeField] TMP_Text loadingText;
		[SerializeField] TMP_Text adviceText;
        [SerializeField] HintGiver hintGiver;
        [SerializeField] GameObject memPanel;
        [SerializeField] MemRenderer memRenderer;

        [Header("Dialog Box")]
        [SerializeField] GameObject messagePanel;
        [SerializeField] TMP_Text messageHeader;
        [SerializeField] TMP_Text messageBody;
        [SerializeField] TMP_Text errorBuild;
        [SerializeField] Button errorOkButton;
        [SerializeField] Button errorLogButton;
        [SerializeField] Slider dialogProgress;

        [Header("Debug")]
        [SerializeField] private GameObject DebugHolder;

        [Header("Audio")]
        public static AudioMixer mainMixer;
        public static AudioMixerGroup sfxGroup;

        public static bool IsShowingDialog;
        public static string PlayOpenFilePath = null;
        public static string PlayOpenFileHash = null;
        public static bool returnToLibrary = false;
        public static bool transitioningToLibraryFromGameplay = false;
        public static float playbackSpeed = 1.0f;
        public static bool autoplay = false;
        public static bool perfectCampaign = false;

        public static string buildTime = "00/00/0000 00:00:00";
        public static int buildInt = 0;
        public static string friendlyReleaseName = "Bobby Build 2024/12/06";
        public static bool newPackins = false;

        public static bool HasShutDown = false;
        public static bool discordDuringTesting = false;

        static string loadedScene;
        static string lastLoadedScene;
        static AsyncOperation asyncLoad, asyncFree;

        public static string levelLocation;
        public static bool officialLevel;

        public static bool IsFirstBoot = false;

        public static int CustomScreenWidth = 1280;
        public static int CustomScreenHeight = 720;

        public static readonly (int width, int height)[] DEFAULT_SCREEN_SIZES = new[] { (1280, 720), (1920, 1080), (2560, 1440), (3840, 2160) };
        public static readonly string[] DEFAULT_SCREEN_SIZES_STRING = new[] { "1280x720", "1920x1080", "2560x1440", "3840x2160", "Custom" };
        public static int ScreenSizeIndex = 0;

        public static float MasterVolume = 0.8f;
        public static float MusicVolume = 1f;
        public static float SfxVolume = 1f;
        public static int currentDspSize = 512;
        public static int currentSampleRate = 44100;
        public static readonly int[] DSP_BUFFER_SIZES =
        {
            128, 256, 340, 480, 512, 1024
        };

        public static readonly int[] SAMPLE_RATES =
        {
            22050, 44100, 48000, 88200, 96000,
        };

        public static RenderTexture GameRenderTexture;
        public static RenderTexture VfxRenderTexture;
        public static RenderTexture BGRenderTexture;
        public static RenderTexture OverlayRenderTexture;

        public enum Scenes : int
        {
            SplashScreen = 0,
            Menu = 1,
            Editor = 2,
            Game = 3
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            HasShutDown = false;
            Application.wantsToQuit += WantsToQuit;
            Application.quitting += OnQuitting;

            mainMixer = Resources.Load<AudioMixer>("MainMixer");
            sfxGroup = mainMixer.FindMatchingGroups("SFX")[0];

            BasicCheck();

            Minigames.InitPreprocessor();
            loadedScene = SceneManager.GetActiveScene().name;

            instance.InitializeFileBrowser();

            PersistentDataManager.LoadSettings();
            Localization.Init();
            LibraryCacheManager.LoadCache();

            ScreenSizeIndex = PersistentDataManager.gameSettings.resolutionIndex;
            CustomScreenWidth = PersistentDataManager.gameSettings.resolutionWidth;
            CustomScreenHeight = PersistentDataManager.gameSettings.resolutionHeight;

            if (PersistentDataManager.gameSettings.dspSize == 0)
                PersistentDataManager.gameSettings.dspSize = 512;
            if (PersistentDataManager.gameSettings.sampleRate == 0)
                PersistentDataManager.gameSettings.sampleRate = 44100;
            currentDspSize = PersistentDataManager.gameSettings.dspSize;
            currentSampleRate = PersistentDataManager.gameSettings.sampleRate;

            // ChangeAudioSettings(currentDspSize, currentSampleRate);
            AudioConfiguration config = AudioSettings.GetConfiguration();
            config.dspBufferSize = currentDspSize;
            config.sampleRate = currentSampleRate;
            AudioSettings.Reset(config);

            QualitySettings.vSyncCount = 0;

            if (PersistentDataManager.gameSettings.limitFramerate)
            {
                Application.targetFrameRate = PersistentDataManager.gameSettings.maxFps;
                if (PersistentDataManager.gameSettings.vSync)
                {
                    QualitySettings.vSyncCount = 1;
                }
            }
            else
            {
                Application.targetFrameRate = -1;
            }

            QualitySettings.maxQueuedFrames = 1;
            if (PersistentDataManager.gameSettings.isFullscreen)
            {
                Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
                Screen.fullScreen = true;
            }
            else
            {
                Screen.fullScreenMode = FullScreenMode.Windowed;
                Screen.fullScreen = false;
                ChangeScreenSize();
            }
            instance.StartCoroutine(InitVolumes());
            PlayerInput.InitInputControllers();
#if HEAVENSTUDIO_PROD && !UNITY_EDITOR
            buildTime = Application.buildGUID.Substring(0, 8) + " " + AppInfo.Date.ToString("dd/MM/yyyy hh:mm:ss");
#elif UNITY_EDITOR
            buildTime = "(EDITOR) " + System.DateTime.UtcNow.ToString("dd/MM/yyyy hh:mm:ss");
#else
            buildTime = Application.buildGUID.Substring(0, 8) + " " + AppInfo.Date.ToString("dd/MM/yyyy hh:mm:ss");
#endif

            buildInt = AppInfo.Date.Day + (AppInfo.Date.Month * 100) + (AppInfo.Date.Year * 10000);

            Debug.Log($"Started Heaven Studio {buildTime} on {Application.platform}");
            Debug.Log($"We hope you enjoy as much as we did making it!");
        }

        private static IEnumerator InitVolumes()
        {
            yield return null;
            ChangeMasterVolume(PersistentDataManager.gameSettings.masterVolume);
            ChangeMixerVolumes(PersistentDataManager.gameSettings.musicVolume, PersistentDataManager.gameSettings.sfxVolume);
        }

        public void InitializeFileBrowser()
        {
            FileBrowserOpener.Initialize(this);
        }

        public void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            instance = this;
            fadeImage.gameObject.SetActive(false);
            loadingText.enabled = false;
            instance.adviceText.text = "";
            memPanel.SetActive(false);

            messagePanel.SetActive(false);
            IsShowingDialog = false;
        }

        private void Update()
        {
            PlayerInput.UpdateInputControllers();
        }

        public void ClearOpenFile()
        {
            PlayOpenFilePath = null;
            PlayOpenFileHash = null;
        }

        IEnumerator LoadSceneAsync(string scene, float fadeOut, bool continueLoading)
        {
            Application.backgroundLoadingPriority = ThreadPriority.Normal;

            asyncFree = Resources.UnloadUnusedAssets();
            while (!asyncFree.isDone)
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);

            asyncLoad = SceneManager.LoadSceneAsync(scene);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            if (!continueLoading)
            {
                instance.fadeImage.DOKill();
                instance.loadingText.enabled = false;
                instance.adviceText.text = "";
                memPanel.SetActive(false);

                if (fadeOut < 0)
                {
                }
                else if (fadeOut == 0)
                {
                    instance.fadeImage.color = new Color(0, 0, 0, 0);
                    instance.fadeImage.gameObject.SetActive(false);
                }
                else
                {
                    instance.fadeImage.DOFade(0, fadeOut).OnComplete(() =>
                    {
                        instance.fadeImage.gameObject.SetActive(false);
                    });
                }
            }
        }

        IEnumerator ForceFadeAsync(float hold, float fadeOut)
        {
            if (hold > 0)
            {
                yield return new WaitForSeconds(hold);
            }
            instance.fadeImage.DOKill();
            instance.loadingText.enabled = false;
			instance.adviceText.text = "";
            memPanel.SetActive(false);
            instance.fadeImage.DOFade(0, fadeOut).OnComplete(() =>
            {
                instance.fadeImage.gameObject.SetActive(false);
            });
        }

        public void HideDialog()
        {
            Debug.Log("Hiding dialog");
            messagePanel.SetActive(false);
            IsShowingDialog = false;
        }

        public void OpenLogFolder()
        {
            // TODO
        }

        public static void BasicCheck()
        {
            if (FindGGM() == null)
            {
                // load the global game manager prefab
                GameObject ggm = Instantiate(Resources.Load("Prefabs/GlobalGameManager") as GameObject);
                DontDestroyOnLoad(ggm);
            }
        }

        public static GameObject FindGGM()
        {
            if (instance != null)
                return instance.gameObject;
            else
                return null;
        }

        public delegate void OnLoadSceneFadeInComplete();
        public static void LoadScene(string scene, float fadeIn = 0.35f, float fadeOut = 0.35f, OnLoadSceneFadeInComplete callback = null, bool continueLoading = false)
        {
            if (scene == loadedScene)
                return;
            lastLoadedScene = loadedScene;
            loadedScene = scene;

            instance.fadeImage.DOKill();
            instance.fadeImage.gameObject.SetActive(true);
            // instance.loadingText.enabled = true;
			instance.adviceText.text = instance.hintGiver.GenerateHint();
			instance.adviceText.enabled = true;
            instance.memPanel.SetActive(true);
            instance.memRenderer.ChangeMem();
            if (fadeIn <= 0)
            {
                instance.fadeImage.color = new Color(0, 0, 0, 1);
                callback?.Invoke();
                AssetBundle.UnloadAllAssetBundles(true);
                instance.StartCoroutine(instance.LoadSceneAsync(scene, fadeOut, continueLoading));
            }
            else
            {
                instance.fadeImage.color = new Color(0, 0, 0, 0);
                instance.fadeImage.DOFade(1, fadeIn).OnComplete(() =>
                {
                    callback?.Invoke();
                    AssetBundle.UnloadAllAssetBundles(true);
                    instance.StartCoroutine(instance.LoadSceneAsync(scene, fadeOut, continueLoading));
                });
            }
        }

        public static void ForceFade(float fadeIn, float hold, float fadeOut)
        {
            instance.fadeImage.DOKill();
            instance.fadeImage.gameObject.SetActive(true);
            instance.loadingText.enabled = false;
			instance.adviceText.text = "";
            instance.memPanel.SetActive(false);
            if (fadeIn > 0)
            {
                instance.fadeImage.color = new Color(0, 0, 0, 0);
                instance.fadeImage.DOFade(1, fadeIn).OnComplete(() =>
                {
                    instance.StartCoroutine(instance.ForceFadeAsync(hold, fadeOut));
                });
            }
            else
            {
                if (hold > 0 || fadeOut >= 0)
                {
                    instance.fadeImage.color = new Color(0, 0, 0, 1);
                    instance.StartCoroutine(instance.ForceFadeAsync(hold, fadeOut));
                }
                else
                {
                    instance.fadeImage.color = new Color(0, 0, 0, 1);
                    instance.fadeImage.gameObject.SetActive(true);
                }
            }
        }

        public static void ShowErrorMessage(string header, string message)
        {
            IsShowingDialog = true;
            if (Conductor.instance != null && Conductor.instance.isPlaying)
                Conductor.instance.Pause();

            instance.messageHeader.text = Localization.T(header);
            instance.messageBody.text = Localization.T(message);
            instance.errorOkButton.gameObject.SetActive(true);
            // instance.errorLogButton.gameObject.SetActive(true);
            instance.dialogProgress.gameObject.SetActive(false);

            instance.errorBuild.gameObject.SetActive(true);
#if UNITY_EDITOR
            instance.errorBuild.text = "(EDITOR) " + System.DateTime.UtcNow.ToString("dd/MM/yyyy hh:mm:ss");
#else
                instance.errorBuild.text = Application.buildGUID.Substring(0, 8) + " " + AppInfo.Date.ToString("dd/MM/yyyy hh:mm:ss");
#endif

            instance.messagePanel.SetActive(true);
        }

        public static void ShowLoadingMessage(string header, string message, float progress = -1)
        {
            Debug.Log("ShowLoadingMessage");
            IsShowingDialog = true;
            if (Conductor.instance != null && Conductor.instance.isPlaying)
                Conductor.instance.Pause();

            instance.messageHeader.text = Localization.T(header);
            instance.messageBody.text = Localization.T(message);
            instance.errorOkButton.gameObject.SetActive(false);
            instance.errorBuild.gameObject.SetActive(false);
            // instance.errorLogButton.gameObject.SetActive(false);
            if (progress >= 0)
            {
                instance.dialogProgress.gameObject.SetActive(true);
                instance.dialogProgress.value = progress;
            }
            instance.messagePanel.SetActive(true);
        }

        public static void SetLoadingMessageProgress(float progress)
        {
            if (IsShowingDialog)
            {
                instance.dialogProgress.gameObject.SetActive(true);
                instance.dialogProgress.value = progress;
            }
        }

        public static void WindowFullScreen()
        {
            if (!Screen.fullScreen)
            {
                // Set the resolution to the display's current resolution
                Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
                Screen.fullScreen = true;
                PersistentDataManager.gameSettings.isFullscreen = true;
            }
            else
            {
                Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
                Screen.fullScreen = false;
                PersistentDataManager.gameSettings.isFullscreen = false;
            }
        }

        public static void ChangeScreenSize()
        {
            FullScreenMode mode = Screen.fullScreenMode;
            if (ScreenSizeIndex == DEFAULT_SCREEN_SIZES_STRING.Length - 1)
            {
                Screen.SetResolution(CustomScreenWidth, CustomScreenHeight, mode);
                PersistentDataManager.gameSettings.resolutionWidth = CustomScreenWidth;
                PersistentDataManager.gameSettings.resolutionHeight = CustomScreenHeight;
                PersistentDataManager.gameSettings.resolutionIndex = DEFAULT_SCREEN_SIZES_STRING.Length - 1;
            }
            else
            {
                Screen.SetResolution(DEFAULT_SCREEN_SIZES[ScreenSizeIndex].width, DEFAULT_SCREEN_SIZES[ScreenSizeIndex].height, mode);
                PersistentDataManager.gameSettings.resolutionWidth = DEFAULT_SCREEN_SIZES[ScreenSizeIndex].width;
                PersistentDataManager.gameSettings.resolutionHeight = DEFAULT_SCREEN_SIZES[ScreenSizeIndex].height;
                PersistentDataManager.gameSettings.resolutionIndex = ScreenSizeIndex;
            }
        }

        public static void ResetGameRenderTexture(float widthRatio = 16f, float heightRatio = 9f)
        {
            // keep aspect ratio (16:9 by default)
            float resScale = PersistentDataManager.gameSettings.resScale;

            int width = Screen.width;
            int height = Screen.height;
            
            if ((float)width / widthRatio > (float)height / heightRatio)
            {
                width = (int)(height / heightRatio * widthRatio);
            }
            else
            {
                height = (int)(width / widthRatio * heightRatio);
            }

            if (GameRenderTexture != null)
            {
                GameRenderTexture.Release();

                GameRenderTexture.width = (int)(width * resScale);
                GameRenderTexture.height = (int)(height * resScale);

                GameRenderTexture.Create();
            }
            else
            {
                GameRenderTexture = new RenderTexture((int)(width * resScale), (int)(height * resScale), 24);
                GameRenderTexture.Create();
            }

            float vfxScale = resScale > 1f ? 1f : resScale;
            if (VfxRenderTexture != null)
            {
                VfxRenderTexture.Release();

                VfxRenderTexture.width = (int)(width * vfxScale);
                VfxRenderTexture.height = (int)(height * vfxScale);

                VfxRenderTexture.Create();
            }
            else
            {
                VfxRenderTexture = new RenderTexture((int)(width * vfxScale), (int)(height * vfxScale), 0);
                VfxRenderTexture.Create();
            }

            if (BGRenderTexture != null)
            {
                BGRenderTexture.Release();

                BGRenderTexture.width = (int)(width * vfxScale);
                BGRenderTexture.height = (int)(height * vfxScale);

                BGRenderTexture.Create();
            }
            else
            {
                BGRenderTexture = new RenderTexture((int)(width * vfxScale), (int)(height * vfxScale), 0);
                BGRenderTexture.Create();
            }

            if (OverlayRenderTexture != null)
            {
                OverlayRenderTexture.Release();

                OverlayRenderTexture.width = (int)(width * 1.5f);
                OverlayRenderTexture.height = (int)(height * 1.5f);

                OverlayRenderTexture.Create();
            }
            else
            {
                OverlayRenderTexture = new RenderTexture((int)(width * 1.5f), (int)(height * 1.5f), 0);
                OverlayRenderTexture.Create();
            }
        }


        public static void ChangeMasterVolume(float value)
        {
            MasterVolume = value;
            AudioListener.volume = MasterVolume;
        }

        public static void ChangeMixerVolumes(float music, float sfx)
        {
            Debug.Log(sfx);
            MusicVolume = music;
            SfxVolume = sfx;
            SetMixerVolume("MusicVolume", music);
            SetMixerVolume("SfxVolume", sfx);
        }

        private static void SetMixerVolume(string param, float value)
        {
            float dB;

            if (value <= 0f)
                dB = -80f;
            else
                dB = Mathf.Clamp(20f * Mathf.Log10(value), -80f, 0f);

            mainMixer.SetFloat(param, dB);
        }

        public static void ChangeMaxFramerate(int value)
        {
            int MaxFramerate = -1;
            int vSync = 0;
            if (PersistentDataManager.gameSettings.limitFramerate)
            {
                MaxFramerate = value;
                if (PersistentDataManager.gameSettings.vSync)
                {
                    vSync = 1;
                }
            }
            Application.targetFrameRate = MaxFramerate;
            QualitySettings.vSyncCount = vSync;
        }

        public static void ChangeAudioSettings(int dspSize, int sampleRate)
        {
            // this will apply on next boot
            PersistentDataManager.gameSettings.dspSize = dspSize;
            PersistentDataManager.gameSettings.sampleRate = sampleRate;
        }

        public static void UpdateDiscordStatus(string details, bool editor = false, bool updateTime = false)
        {
            Debug.Log("Discord Rich Presence temporarily disabled");
            return;
            // if (discordDuringTesting || !Application.isEditor)
            // {
            //     if (PersistentDataManager.gameSettings.discordRPCEnable)
            //     {
            //         try
            //         {
            //             DiscordRPC.DiscordRPC.UpdateActivity(editor ? "In Editor " : "Playing ", details, updateTime);
            //             Debug.Log("Discord status updated");
            //         }
            //         catch (System.Exception e)
            //         {
            //             Debug.Log("Discord status update failed: " + e.Message);
            //         }
            //     }
            // }
        }

        private static void OnQuitting()
        {
            if (!HasShutDown)
            {
                Debug.Log("Disconnecting JoyShocks...");
                PlayerInput.CleanUp();
                Debug.Log("Clearing RIQ Cache...");
                Jukebox.RiqFileHandler.ClearCache();
                PersistentDataManager.SaveSettings();
                LibraryCacheManager.SaveCache();
                // Debug.Log("Closing Discord GameSDK...");
                // DiscordRPC.DiscordController.instance?.Disconnect();

                HasShutDown = true;
            }
        }

        private static bool WantsToQuit()
        {
            if (SceneManager.GetActiveScene().name != "Editor") return true;
            Editor.Editor.instance.ShowQuitPopUp(true);
            return Editor.Editor.instance.ShouldQuit;
        }
    }
}
