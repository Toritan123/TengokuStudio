using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HeavenStudio.Common;
using HeavenStudio.Util;
using Jukebox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HeavenStudio
{
    public class LibraryInfoPanel : MonoBehaviour
    {
        [Header("Layouts")]
        [SerializeField] private GameObject mainLayout;
        [SerializeField] private GameObject defaultLayout;
        [SerializeField] private GameObject importLayout;
        [SerializeField] private GameObject folderLayout;
        [SerializeField] private GameObject folderEditLayout;
        [SerializeField] private GameObject customSortEditLayout;

        [Header("MAIN LAYOUT")]

        [Header("Info Elements")]
        [SerializeField] private TMP_Text levelTitle;
        [SerializeField] private LibraryLevelIcon levelIcon;
        [SerializeField] private TMP_Text levelCharter;
        [SerializeField] private TMP_Text songArtist;
        [SerializeField] private TMP_Text levelDesc;
        [SerializeField] private GameObject flashWarning;
        [SerializeField] private GameObject controlStyle;
        [SerializeField] private TMP_Text songLength;
        [SerializeField] private TMP_Text score;
        [SerializeField] private GameObject sectionMedalHolder;
        [SerializeField] private Image skillStar;
        [SerializeField] private LibraryMods mods;
        [SerializeField] private LibraryPopup morePopup;
        [SerializeField] private Transform tagsHolder;
        [SerializeField] private LibraryTag tagPrefab;
        [SerializeField] private GameObject tagsSection;
        [SerializeField] private GameObject tagButton;
        [SerializeField] private List<ContentSizeFitter> tagPopupRects;

        [Header("Tempo")]
        [SerializeField] private TMP_Text bpmText;

        [Header("Buttons")]
        [SerializeField] public LibrarySelectable playButton;
        [SerializeField] public Button perfectOnButton;
        [SerializeField] public Button perfectOffButton;
        [SerializeField] private Button modsButton;
        [SerializeField] private LibrarySelectable openInEditorButton;
        [SerializeField] private TMP_Text reccomendedControlsText;
        [SerializeField] private GameObject warningButton;
        [SerializeField] private List<Button> tagsTargetButtons;

        [Header("Vars")]
        [SerializeField] private bool animating;
        [SerializeField] private RectTransform topRect;
        [SerializeField] private RectTransform bottomRect;
        [SerializeField] private float animationLength;
        [SerializeField] private ScoreColors superbColors;
        [SerializeField] private ScoreColors okColors;
        [SerializeField] private ScoreColors tryAgainColors;
        [SerializeField] private ScoreColors unplayedColors;
        [SerializeField] private GameObject medalEnabledTemplate;
        [SerializeField] private GameObject medalDisabledTemplate;
        [SerializeField] private Sprite skillStarInactive;
        [SerializeField] private Sprite skillStarActive;
        [SerializeField] private Image perfectIcon;
        [SerializeField] private Sprite perfectIconInactive;
        [SerializeField] private Sprite perfectIconActive;

        [Header("Play Button")]
        [SerializeField] private Image playButtonBackground;
        [SerializeField] private Image playButtonIcon;
        [SerializeField] private TMP_Text playButtonText;
        [SerializeField] private TMP_Text playButtonTextShadow;
        private Coroutine perfectEnableCoroutine;
        private Coroutine perfectDisableCoroutine;
        [SerializeField] private Sprite playButtonNormalIcon;
        [SerializeField] private Color playButtonNormalBgColor = Color.white;
        [SerializeField] private Color playButtonNormalTextColor = Color.white;
        [SerializeField] private Color playButtonNormalShadowColor = Color.white;

        [SerializeField] private Sprite playButtonPerfectIcon;
        [SerializeField] private Color playButtonPerfectBgColor = Color.white;
        [SerializeField] private Color playButtonPerfectTextColor = Color.white;
        [SerializeField] private Color playButtonPerfectShadowColor = Color.white;

        [SerializeField] private Sprite autoplayEnable;
        [SerializeField] private bool autoplayOn = false;

        [Header("IMPORT INFO")]
        [SerializeField] public TMP_Text pathText;

        [Header("FOLDER INFO")]
        [SerializeField] private TMP_Text folderName;
        [SerializeField] private TMP_Text folderCreator;
        [SerializeField] private TMP_Text folderDesc;
        [SerializeField] private GameObject folderStatsHolder;
        [SerializeField] private TMP_Text folderFlow;
        [SerializeField] private TMP_Text folderLevelsComplete;
        [SerializeField] private TMP_Text folderTotalLevels;

        [Header("STATS")]
        [SerializeField] private TMP_Text medalsCollected;
        [SerializeField] private TMP_Text perfectsCollected;
        [SerializeField] private TMP_Text megaflow;




        void Start()
        {
            defaultLayout.SetActive(true);
            mainLayout.SetActive(false);
            importLayout.SetActive(false);
            folderLayout.SetActive(false);
            folderEditLayout.SetActive(false);
            customSortEditLayout.SetActive(false);

            topRect = levelTitle.transform.parent.GetComponent<RectTransform>();
            bottomRect = playButton.transform.parent.GetComponent<RectTransform>();
            if (levelIcon != null) levelIcon.interactable = false;
        }

        public void ShowLevelInfo(string hash)
        {
            CachedLevelInfo info = LibraryCacheManager.cache.cachedInfo[hash];

            ShowMainLayout();

            levelTitle.text = info.levelName;
            levelCharter.text = string.IsNullOrWhiteSpace(info.levelCharter) ? "???" : info.levelCharter;
            levelDesc.text = info.description;

            if (levelIcon != null)
            {
                levelIcon.gameObject.SetActive(true);
                levelIcon.StopAllCoroutines();
                levelIcon.loadedIcon = false;
                levelIcon.SetIconInfo(LibraryManager.instance.selectedLevelPath, hash);
                levelIcon.LoadCustomIcon();
            }

            string artistDisplay = string.IsNullOrWhiteSpace(info.songArtist) ? "???" : info.songArtist;

            if (info.songName.Equals(info.levelName) || string.IsNullOrWhiteSpace(info.songName) || info.songName.Equals("Song Name"))
            {
                songArtist.text = artistDisplay;
            }
            else
            {
                songArtist.text = info.songName + " - " + artistDisplay;
            }

            controlStyle.SetActive(info.controlStyle != "Any");
            reccomendedControlsText.text = Localization.T(info.controlStyle);
            flashWarning.SetActive(info.flashWarning);

            SetBpms(info);

            if (info.warnings.Count > 0)
            {
                songLength.gameObject.SetActive(false);
                // warningButton.SetActive(true);
            }
            else
            {
                songLength.gameObject.SetActive(true);
                // warningButton.SetActive(false);
                songLength.text = FormatTime(info.length);
            }

            foreach (Transform medal in sectionMedalHolder.transform)
            {
                Destroy(medal.gameObject);
            }

            LoadSavedSataForLevel(hash, info);

            bool hasTags = !string.IsNullOrEmpty(info.tags);
            GameObject tagsToggleTarget = tagsSection != null ? tagsSection : tagButton;
            tagsToggleTarget.SetActive(hasTags);

            Button tagNavTarget = null;
            if (hasTags)
            {
                tagButton.SetActive(true);

                if (tagsHolder != null && tagPrefab != null)
                {
                    print("updating tags");
                    UpdateTags(info.tags);
                }

                // tagPopup.GetComponent<ContentSizeFitter>().
                Canvas.ForceUpdateCanvases();
                foreach (ContentSizeFitter rect in tagPopupRects)
                {
                    rect.SetLayoutVertical();
                }

                tagNavTarget = tagButton.GetComponent<Button>();
            }

            foreach (Button button in tagsTargetButtons)
            {
                Navigation nav = button.navigation;
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnUp = tagNavTarget;
                button.navigation = nav;
            }
        }

        void UpdateTags(string tagString)
        {
            foreach (Transform child in tagsHolder.transform)
            {
                Destroy(child.gameObject);
            }

            SortedSet<string> currentTags = LibraryTag.ParseTagString(tagString);

            foreach (string tag in currentTags)
            {
                InitTagObject(tag, tagsHolder.transform);
            }

            SortChildren(tagsHolder.transform);
        }

        private void InitTagObject(string tag, Transform parent)
        {
            LibraryTag newTag = Instantiate(tagPrefab, parent);
            newTag.SetTagText(tag);
            newTag.transform.localScale = new(0.5f, 0.5f);
        }

        private void SortChildren(Transform transform)
        {
            List<Transform> children = new();

            foreach (Transform child in transform)
            {
                children.Add(child);
            }

            children = children.OrderBy(t => t.gameObject.name).ToList();

            for (int i = 0; i < children.Count; i++)
            {
                children[i].SetSiblingIndex(i);
            }
        }

        public void ShowFolderInfo(string path)
        {
            LibraryFolderIcon folderIcon = LibraryManager.allIconInfo[path].folderIcon;

            folderName.text = folderIcon.folderName;

            folderCreator.text = Localization.F("By {0}", folderIcon.folderData.creator);
            folderCreator.gameObject.SetActive(!string.IsNullOrEmpty(folderIcon.folderData.creator));

            folderDesc.text = folderIcon.folderData.description;

            folderIcon.GetLevelsInFolder();
            folderTotalLevels.text = folderIcon.levelsInFolder.Count.ToString();

            float totalScore = 0f;
            int levelsCompleted = 0;

            foreach (string levelPath in folderIcon.levelsInFolder)
            {
                if (LibrarySaveManager.savedata.levelInfo.ContainsKey(LibraryManager.allIconInfo[levelPath].hash))
                {
                    int levelScore = LibrarySaveManager.savedata.levelInfo[LibraryManager.allIconInfo[levelPath].hash].score;

                    totalScore += levelScore;

                    if (levelScore >= 60)
                    {
                        levelsCompleted++;
                    }
                }
            }

            folderLevelsComplete.text = levelsCompleted.ToString();

            int flow = 0;
            if (folderIcon.levelsInFolder.Count > 0)
            {
                flow = Mathf.RoundToInt(totalScore / (float)folderIcon.levelsInFolder.Count);
            }

            folderFlow.text = flow.ToString();

            folderStatsHolder.SetActive(folderIcon.folderData.showStats);
            ShowFolderLayout();

        }

        public void ShowHelpAndStats()
        {
            medalsCollected.text = LibrarySaveManager.savedata.levelInfo.Where(kvp => LibraryManager.instance.hashesPresent.Contains(kvp.Key)).Count(kvp => kvp.Value.score >= 80).ToString();
            perfectsCollected.text = LibrarySaveManager.savedata.levelInfo.Where(kvp => LibraryManager.instance.hashesPresent.Contains(kvp.Key)).Count(kvp => kvp.Value.perfect).ToString();

            int totalScore = 0;
            foreach (KeyValuePair<string, LevelSaveData> kvp in LibrarySaveManager.savedata.levelInfo)
            {
                if (LibraryManager.instance.hashesPresent.Contains(kvp.Key))
                {
                    LevelSaveData levelInfo = kvp.Value;
                    totalScore += levelInfo.score;
                }
            }

            megaflow.text = totalScore.ToString();

            ShowDefaultLayout();
        }

        private void SetBpms(CachedLevelInfo info)
        {
            string minBPM = info.lowBpm.ToString("0.##");
            string maxBPM = info.highBpm.ToString("0.##");

            bpmText.text = minBPM != maxBPM ? $"{minBPM} - {maxBPM} BPM" : $"{minBPM} BPM";
        }

        private void LoadSavedSataForLevel(string hash, CachedLevelInfo info)
        {
            bool hasSavedData = LibrarySaveManager.savedata.levelInfo.ContainsKey(hash);

            score.text = hasSavedData ? LibrarySaveManager.savedata[hash].score.ToString() : "--";

            if (hasSavedData)
            {
                SetScoreColors(LibrarySaveManager.savedata[hash].score);

                for (int i = 0; i < info.numberOfMedals; i++)
                {
                    GameObject template = LibrarySaveManager.savedata[hash].sectionInfo[i] ? medalEnabledTemplate : medalDisabledTemplate;
                    Instantiate(template, sectionMedalHolder.transform).SetActive(true);
                }
            }
            else
            {
                SetScoreColors(-1);

                for (int i = 0; i < info.numberOfMedals; i++)
                {
                    Instantiate(medalDisabledTemplate, sectionMedalHolder.transform).SetActive(true);
                }
            }

            if (info.hasSkillStar)
            {
                skillStar.gameObject.SetActive(true);
                if (hasSavedData)
                {
                    skillStar.sprite = LibrarySaveManager.savedata[hash].skillStar ? skillStarActive : skillStarInactive;
                }
                else
                {
                    skillStar.sprite = skillStarInactive;
                }
            }
            else
            {
                skillStar.gameObject.SetActive(false);
            }

            if (perfectIcon != null)
            {
                if (hasSavedData)
                {
                    perfectIcon.sprite = LibrarySaveManager.savedata[hash].perfect ? perfectIconActive : perfectIconInactive;
                    perfectIcon.gameObject.SetActive(LibrarySaveManager.savedata[hash].perfect);
                }
                else
                {
                    perfectIcon.gameObject.SetActive(false);
                }
            }
        }

        private void SetScoreColors(int score)
        {
            ScoreColors color;
            if (score < 0)
            {
                color = unplayedColors;
            }
            else if (score < Games.Minigame.rankOkThreshold * 100)
            {
                color = tryAgainColors;
            }
            else if (score < Games.Minigame.rankHiThreshold * 100)
            {
                color = okColors;
            }
            else
            {
                color = superbColors;
            }

            this.score.colorGradient = new(color.mainColor, color.mainColor, color.subColor, color.subColor);
        }

        public void DeleteLevel()
        {
            LibraryManager.instance.DeleteIconAndFile(LibraryManager.instance.selectedLevelPath);
            LibraryManager.instance.SelectFolder(LibraryManager.currentFolder);

            SoundByte.PlayOneShot("ui/Library/delete", ui: true);

            morePopup.HidePopup();
        }

        public void SelectMoreButton()
        {
            LibraryMenuCursor.instance.CursorSelect(openInEditorButton, true);
            LibraryMenuCursor.instance.currentPopup = morePopup;

            morePopup.ShowPopup();
        }

        public void ResetScore()
        {
            LibraryManager.instance.DeleteSaveDataForLevel(LibraryManager.instance.selectedLevelPath);
            LibraryMenuCursor.instance.CursorBack();
            ShowLevelInfo(LibraryManager.allIconInfo[LibraryManager.instance.selectedLevelPath].hash);
        }

        public void ShowMainLayout()
        {
            if (!mainLayout.activeSelf)
            {
                StopAllCoroutines();
                StartCoroutine(AnimateTitleIn());

                mainLayout.SetActive(true);
                defaultLayout.SetActive(false);
                importLayout.SetActive(false);
                folderLayout.SetActive(false);
                folderEditLayout.SetActive(false);
                customSortEditLayout.SetActive(false);
            }
        }

        public void ShowDefaultLayout()
        {
            if (!defaultLayout.activeSelf)
            {
                if (mainLayout.activeSelf)
                {
                    StopAllCoroutines();
                    StartCoroutine(AnimateTitleOut());
                }

                mainLayout.SetActive(false);
                defaultLayout.SetActive(true);
                importLayout.SetActive(false);
                folderLayout.SetActive(false);
                folderEditLayout.SetActive(false);
                customSortEditLayout.SetActive(false);

                LibraryManager.instance.selectedLevelPath = "";
            }
        }

        public void ShowImportLayout()
        {
            if (!importLayout.activeSelf)
            {
                if (mainLayout.activeSelf)
                {
                    StopAllCoroutines();
                    StartCoroutine(AnimateTitleOut());
                }

                mainLayout.SetActive(false);
                defaultLayout.SetActive(false);
                importLayout.SetActive(true);
                folderLayout.SetActive(false);
                folderEditLayout.SetActive(false);
                customSortEditLayout.SetActive(false);

                LibraryManager.instance.selectedLevelPath = "";
            }
        }

        public void ShowFolderLayout()
        {
            if (!folderLayout.activeSelf)
            {
                if (mainLayout.activeSelf)
                {
                    StopAllCoroutines();
                    StartCoroutine(AnimateTitleOut());
                }

                mainLayout.SetActive(false);
                defaultLayout.SetActive(false);
                importLayout.SetActive(false);
                folderLayout.SetActive(true);
                folderEditLayout.SetActive(false);
                customSortEditLayout.SetActive(false);

                LibraryManager.instance.selectedLevelPath = "";
            }
        }

        public void ShowFolderEditLayout()
        {
            if (!folderEditLayout.activeSelf)
            {
                if (mainLayout.activeSelf)
                {
                    StopAllCoroutines();
                    StartCoroutine(AnimateTitleOut());
                }

                mainLayout.SetActive(false);
                defaultLayout.SetActive(false);
                importLayout.SetActive(false);
                folderLayout.SetActive(false);
                folderEditLayout.SetActive(true);
                customSortEditLayout.SetActive(false);

                LibraryManager.instance.selectedLevelPath = "";
            }
        }

        public void ShowCustomSortEditLayout()
        {
            if (!customSortEditLayout.activeSelf)
            {
                if (mainLayout.activeSelf)
                {
                    StopAllCoroutines();
                    StartCoroutine(AnimateTitleOut());
                }

                mainLayout.SetActive(false);
                defaultLayout.SetActive(false);
                importLayout.SetActive(false);
                folderLayout.SetActive(false);
                folderEditLayout.SetActive(false);
                customSortEditLayout.SetActive(true);

                LibraryManager.instance.selectedLevelPath = "";
            }
        }

        private IEnumerator AnimateTitleIn()
        {
            yield return null;
        }

        private IEnumerator AnimateTitleOut()
        {
            yield return null;
        }

        public void PlayLevel()
        {
            if (LibraryManager.instance.exiting) return;
            LibraryManager.instance.exiting = true;
            SoundByte.PlayOneShot("ui/UIEnter", ui: true);
            PersistentDataManager.SaveSettings();

            try
            {
                RiqFileHandler.ClearCache();
                string tmpDir = RiqFileHandler.ExtractRiq(LibraryManager.instance.selectedLevelPath);
                Debug.Log("tmpDir : " + tmpDir);
                Debug.Log("Imported RIQ successfully!");
                RiqBeatmap beatmap = RiqFileHandler.ReadRiq();
                Minigames.PreProcessBeatmap(beatmap.data);
                GlobalGameManager.PlayOpenFilePath = LibraryManager.instance.selectedLevelPath;
                GlobalGameManager.PlayOpenFileHash = LibraryManager.allIconInfo[LibraryManager.instance.selectedLevelPath].hash;
                GlobalGameManager.returnToLibrary = true;
                GlobalGameManager.playbackSpeed = mods.tempoMult / 100f;
                GlobalGameManager.autoplay = mods.autoplayOn;
                GlobalGameManager.lastScore = LibraryManager.GetLevelScore(GlobalGameManager.PlayOpenFileHash);
                LibraryManager.lastFolder = LibraryManager.currentFolder;
                LibraryManager.lastScroll = LibraryManager.instance.levelsScrollRect.verticalNormalizedPosition;
                LibraryManager.instance.SavePathsAndHashes();

                string filename = Path.GetFileName(LibraryManager.instance.selectedLevelPath);
            }
            catch (System.Exception e)
            {
                Debug.Log($"Error importing RIQ: {e.Message}");
                Debug.LogException(e);
                GlobalGameManager.ShowErrorMessage("Error Loading RIQ", e.Message + "\n\n" + e.StackTrace);
                return;
            }

            LibraryManager.instance.fadeOutCoroutine = LibraryManager.instance.StartCoroutine(LibraryManager.instance.FadeOutMusic());

            GlobalGameManager.LoadScene("Game", 0.35f, -1, continueLoading: true);
        }

        public void OpenInEditor()
        {
            if (LibraryManager.instance.exiting) return;
            LibraryManager.instance.exiting = true;
            SoundByte.PlayOneShot("ui/UIEnter", ui: true);
            PersistentDataManager.SaveSettings();

            Editor.Editor.loadFromLibrary = true;
            Editor.Editor.libraryLevelPath = LibraryManager.instance.selectedLevelPath;

            LibraryManager.instance.fadeOutCoroutine = LibraryManager.instance.StartCoroutine(LibraryManager.instance.FadeOutMusic());


            GlobalGameManager.LoadScene("Editor");
        }

        public void NoAction()
        {

        }

        #region Song Length
        public static double GetSongLength(double remixEnd, RiqBeatmap chart)
        {
            float bpm = 120f;

            double totalTime = 0f;

            double lastTempoChangeBeat = 0f;


            remixEnd = GetUnSwungBeat(chart, remixEnd);


            foreach (RiqEntity tempoChange in chart.TempoChanges)
            {
                if (tempoChange.beat > remixEnd)
                {
                    break;
                }

                totalTime += (tempoChange.beat - lastTempoChangeBeat) * 60 / bpm;
                bpm = (float)tempoChange["tempo"];
                lastTempoChangeBeat = tempoChange.beat;
            }

            totalTime += (remixEnd - lastTempoChangeBeat) * 60 / bpm;

            return totalTime;
        }

        private static double GetUnSwungBeat(RiqBeatmap chart, double beat)
        {
            GetBpmAtBeat(chart, beat, out float ratio, out float division);
            int floor = (int)Math.Floor(beat / division);

            return (floor * division) + GetUnSwingOffset(beat, ratio, division);
        }

        private static double GetUnSwingOffset(double beatFrac, float ratio, float division)
        {
            beatFrac %= division;
            if (beatFrac <= ratio)
            {
                return 2 * ratio * beatFrac;
            }
            else
            {
                return 2 * (beatFrac - 0.5f) * (1f - ratio) + ratio;
            }
        }

        private static float GetBpmAtBeat(RiqBeatmap chart, double beat, out float swingRatio, out float swingDivision)
        {
            swingRatio = 0.5f;
            swingDivision = 1f;
            if (chart.TempoChanges.Count == 0)
                return 120f;

            RiqEntity tempo = null;
            foreach (RiqEntity t in chart.TempoChanges)
            {
                if (t.beat > beat)
                {
                    break;
                }
                tempo = t;
            }
            tempo ??= chart.TempoChanges[0];
            swingRatio = (float)tempo["swing"] + 0.5f;
            swingDivision = (float)(tempo["swingDivision"] ?? 1f);
            return (float)tempo["tempo"];
        }

        private string FormatTime(double time)
        {
            int minutes = (int)time / 60;
            int seconds = (int)time - 60 * minutes;
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        #endregion

        #region Perfect Buttons
        public void EnableChallenge()
        {
            if (perfectDisableCoroutine != null)
            {
                StopCoroutine(perfectDisableCoroutine);
                perfectDisableCoroutine = null;
            }

            perfectEnableCoroutine = StartCoroutine(AnimatePerfectEnable());

            Button playButtonButton = playButton.GetComponent<Button>();
            playButtonButton.navigation = new()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = playButtonButton.navigation.selectOnUp,
                selectOnDown = playButtonButton.navigation.selectOnDown,
                selectOnLeft = perfectOffButton,
                selectOnRight = modsButton,
            };

            modsButton.navigation = new()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = modsButton.navigation.selectOnUp,
                selectOnDown = modsButton.navigation.selectOnDown,
                selectOnLeft = playButtonButton,
                selectOnRight = modsButton.navigation.selectOnRight,
            };

            if (!LibraryMenuCursor.usingMouse)
            {
                LibraryMenuCursor.instance.CursorSelect(playButton, false);
            }

            GlobalGameManager.perfectCampaign = true;
        }

        public void DisableChallenge()
        {
            if (perfectEnableCoroutine != null)
            {
                StopCoroutine(perfectEnableCoroutine);
                perfectEnableCoroutine = null;
            }

            perfectDisableCoroutine = StartCoroutine(AnimatePerfectDisable());

            Button playButtonButton = playButton.GetComponent<Button>();
            playButtonButton.navigation = new()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = playButtonButton.navigation.selectOnUp,
                selectOnDown = playButtonButton.navigation.selectOnDown,
                selectOnLeft = null,
                selectOnRight = perfectOnButton,
            };

            modsButton.navigation = new()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = modsButton.navigation.selectOnUp,
                selectOnDown = modsButton.navigation.selectOnDown,
                selectOnLeft = perfectOnButton,
                selectOnRight = modsButton.navigation.selectOnRight,
            };

            if (!LibraryMenuCursor.usingMouse)
            {
                LibraryMenuCursor.instance.CursorSelect(playButton, false);
            }

            GlobalGameManager.perfectCampaign = false;
        }

        private IEnumerator AnimatePerfectEnable()
        {
            float time = 0;
            float normalizedTime = 0;

            Util.EasingFunction.Function easeFunction = Util.EasingFunction.GetEasingFunction(Util.EasingFunction.Ease.EaseOutQuint);

            Color bgStart = playButtonBackground.color;
            Color textStart = playButtonText.color;
            Color shadowStart = playButtonTextShadow.color;

            Color bgEnd = playButtonPerfectBgColor;
            Color textEnd = playButtonPerfectTextColor;
            Color shadowEnd = playButtonPerfectShadowColor;

            RectTransform playButtonRect = playButtonBackground.GetComponent<RectTransform>();

            SetPivot(playButtonRect, new(1, 0.5f));

            Vector3 oldPos = playButtonRect.position;

            playButtonRect.anchorMin = new(1, 0.5f);
            playButtonRect.anchorMax = new(1, 0.5f);


            playButtonRect.position = oldPos;

            float startX = playButtonRect.anchoredPosition.x;
            float endX = 0;

            playButtonIcon.sprite = playButtonPerfectIcon;
            if (autoplayOn) playButtonIcon.sprite = autoplayEnable;

            while (normalizedTime < 1f)
            {
                normalizedTime = Mathf.InverseLerp(0f, 0.5f, time);

                float newX = easeFunction(startX, endX, normalizedTime);

                float newBgR = easeFunction(bgStart.r, bgEnd.r, normalizedTime);
                float newBgG = easeFunction(bgStart.g, bgEnd.g, normalizedTime);
                float newBgB = easeFunction(bgStart.b, bgEnd.b, normalizedTime);

                float newTextR = easeFunction(textStart.r, textEnd.r, normalizedTime);
                float newTextG = easeFunction(textStart.g, textEnd.g, normalizedTime);
                float newTextB = easeFunction(textStart.b, textEnd.b, normalizedTime);

                float newShadowR = easeFunction(shadowStart.r, shadowEnd.r, normalizedTime);
                float newShadowG = easeFunction(shadowStart.g, shadowEnd.g, normalizedTime);
                float newShadowB = easeFunction(shadowStart.b, shadowEnd.b, normalizedTime);

                playButtonRect.anchoredPosition = new(newX, playButtonRect.anchoredPosition.y);
                playButtonBackground.color = new(newBgR, newBgG, newBgB);
                playButtonText.color = new(newTextR, newTextG, newTextB);
                playButtonTextShadow.color = new(newShadowR, newShadowG, newShadowB);

                time += Time.deltaTime;
                yield return null;
            }

            playButtonBackground.color = bgEnd;
            playButtonText.color = textEnd;
            playButtonTextShadow.color = shadowEnd;

            playButtonRect.anchoredPosition = new(endX, playButtonRect.anchoredPosition.y);

            perfectEnableCoroutine = null;

            yield return null;
        }

        private IEnumerator AnimatePerfectDisable()
        {
            float time = 0;
            float normalizedTime = 0;

            Util.EasingFunction.Function easeFunction = Util.EasingFunction.GetEasingFunction(Util.EasingFunction.Ease.EaseOutQuint);

            Color bgStart = playButtonBackground.color;
            Color textStart = playButtonText.color;
            Color shadowStart = playButtonTextShadow.color;

            Color bgEnd = playButtonNormalBgColor;
            Color textEnd = playButtonNormalTextColor;
            Color shadowEnd = playButtonNormalShadowColor;

            RectTransform playButtonRect = playButtonBackground.GetComponent<RectTransform>();

            SetPivot(playButtonRect, new(0, 0.5f));

            Vector3 oldPos = playButtonRect.position;

            playButtonRect.anchorMin = new(0, 0.5f);
            playButtonRect.anchorMax = new(0, 0.5f);

            playButtonRect.position = oldPos;

            float startX = playButtonRect.anchoredPosition.x;
            float endX = 0;

            playButtonIcon.sprite = playButtonNormalIcon;
            if (autoplayOn) playButtonIcon.sprite = autoplayEnable;

            while (normalizedTime < 1f)
            {
                normalizedTime = Mathf.InverseLerp(0f, 0.5f, time);

                float newX = easeFunction(startX, endX, normalizedTime);

                float newBgR = easeFunction(bgStart.r, bgEnd.r, normalizedTime);
                float newBgG = easeFunction(bgStart.g, bgEnd.g, normalizedTime);
                float newBgB = easeFunction(bgStart.b, bgEnd.b, normalizedTime);

                float newTextR = easeFunction(textStart.r, textEnd.r, normalizedTime);
                float newTextG = easeFunction(textStart.g, textEnd.g, normalizedTime);
                float newTextB = easeFunction(textStart.b, textEnd.b, normalizedTime);

                float newShadowR = easeFunction(shadowStart.r, shadowEnd.r, normalizedTime);
                float newShadowG = easeFunction(shadowStart.g, shadowEnd.g, normalizedTime);
                float newShadowB = easeFunction(shadowStart.b, shadowEnd.b, normalizedTime);

                playButtonRect.anchoredPosition = new(newX, playButtonRect.anchoredPosition.y);
                playButtonBackground.color = new(newBgR, newBgG, newBgB);
                playButtonText.color = new(newTextR, newTextG, newTextB);
                playButtonTextShadow.color = new(newShadowR, newShadowG, newShadowB);

                time += Time.deltaTime;
                yield return null;
            }

            playButtonBackground.color = bgEnd;
            playButtonText.color = textEnd;
            playButtonTextShadow.color = shadowEnd;

            playButtonRect.anchoredPosition = new(endX, playButtonRect.anchoredPosition.y);

            perfectEnableCoroutine = null;

            yield return null;
        }

        public void SetAutoplay(bool enable)
        {
            autoplayOn = enable;
            if (autoplayOn) playButtonIcon.sprite = autoplayEnable;
            else if (GlobalGameManager.perfectCampaign) playButtonIcon.sprite = playButtonPerfectIcon;
            else playButtonIcon.sprite = playButtonNormalIcon;
        }

        // https://discussions.unity.com/t/set-a-recttranforms-pivot-without-changing-its-position/139741/2
        public static void SetPivot(RectTransform rectTransform, Vector2 pivot)
        {
            if (rectTransform == null) return;

            Vector3 scale = rectTransform.localScale;
            Vector2 size = rectTransform.rect.size;
            Vector2 deltaPivot = rectTransform.pivot - pivot;
            Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x * scale.x, deltaPivot.y * size.y * scale.y);
            rectTransform.pivot = pivot;
            rectTransform.localPosition -= deltaPosition;
        }


        public enum WarningTypes
        {
            NoEnd
        }

        [Serializable]
        public struct ScoreColors
        {
            public Color mainColor;
            public Color subColor;
        }
    }
}
#endregion
