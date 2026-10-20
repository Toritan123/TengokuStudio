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

        [Header("Tempo")]
        [SerializeField] private TMP_Text bpmButtonText;
        [SerializeField] private TMP_Text startBpmText;
        [SerializeField] private TMP_Text minBpmText;
        [SerializeField] private TMP_Text maxBpmText;
        [SerializeField] private LibraryPopup bpmPopup;

        [Header("Buttons")]
        [SerializeField] public LibrarySelectable playButton;
        [SerializeField] private LibrarySelectable moreButton; 
        [SerializeField] private LibrarySelectable openInEditorButton; 
        [SerializeField] private LibrarySelectable tempoInfo; 
        [SerializeField] private TMP_Text reccomendedControlsText;
        [SerializeField] private GameObject warningButton;

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
        [SerializeField] private bool moreBpm = false;

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

            topRect.gameObject.SetActive(false);
            bottomRect.gameObject.SetActive(false);
        }

        public void ShowLevelInfo(string hash)
        {
            CachedLevelInfo info = LibraryCacheManager.cache.cachedInfo[hash];

            levelTitle.text = info.levelName;
            levelCharter.text = info.levelCharter;
            levelDesc.text = info.description;

            if (info.songName.Equals(info.levelName) || string.IsNullOrWhiteSpace(info.songName) || info.songName.Equals("Song Name"))
            {
                songArtist.text = "♪ " + info.songArtist;
            }
            else
            {
                songArtist.text = "♪ " + info.songName + " - " + info.songArtist;
            }

            reccomendedControlsText.text = Localization.F("Recommended Control Style: {0}", info.controlStyle);
            controlStyle.SetActive(info.controlStyle != "Any");
            flashWarning.SetActive(info.flashWarning);

            SetBpms(info);

            if (info.warnings.Count > 0)
            {
                songLength.gameObject.SetActive(false);
                warningButton.SetActive(true);
            }
            else
            {
                songLength.gameObject.SetActive(true);
                warningButton.SetActive(false);
                songLength.text = FormatTime(info.length);
            }

            foreach (Transform medal in sectionMedalHolder.transform)
            {
                Destroy(medal.gameObject);
            }

            LoadSavedSataForLevel(hash, info);

            ShowMainLayout();
        }

        public void ShowFolderInfo(string path)
        {
            LibraryFolderIcon folderIcon = LibraryManager.instance.allIconInfo[path].folderIcon;

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
                if (LibrarySaveManager.savedata.levelInfo.ContainsKey(LibraryManager.instance.allIconInfo[levelPath].hash))
                {
                    int levelScore = LibrarySaveManager.savedata.levelInfo[LibraryManager.instance.allIconInfo[levelPath].hash].score;

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
            string firstBPM = info.firstBpm.ToString();
            string minBPM = info.lowBpm.ToString();
            string maxBPM = info.highBpm.ToString();

            if ((info.lowBpm != info.firstBpm) || (info.highBpm != info.firstBpm))
            {
                bpmButtonText.text = firstBPM + " •••";
                moreBpm = true;
            }
            else
            {
                bpmButtonText.text = firstBPM;
                moreBpm = false;
            }

            startBpmText.text = firstBPM;
            minBpmText.text = minBPM;
            maxBpmText.text = maxBPM;
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
            LibraryManager.instance.SelectFolder(LibraryManager.instance.currentFolder);

            SoundByte.PlayOneShot("ui/Library/delete");

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
            ShowLevelInfo(LibraryManager.instance.allIconInfo[LibraryManager.instance.selectedLevelPath].hash);
        }

        public void ShowBpmPopup()
        {
            if (moreBpm) bpmPopup.ShowPopup();
        }

        public void HideBpmPopup()
        {
            if (moreBpm) bpmPopup.HidePopup();
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
            animating = true;

            float time = 0;
            float normalizedTime = 0;

            Util.EasingFunction.Function easeFunction = Util.EasingFunction.GetEasingFunction(Util.EasingFunction.Ease.EaseOutQuint);
            float topYEnd = -6.5f;
            float bottomYEnd = 5f;

            topRect.gameObject.SetActive(true);
            bottomRect.gameObject.SetActive(true);

            while (normalizedTime < 1f)
            {
                normalizedTime = Mathf.InverseLerp(0f, animationLength, time);

                float newYTop = easeFunction(100f, topYEnd, normalizedTime);
                float newYBottom = easeFunction(-90f, bottomYEnd, normalizedTime);

                topRect.anchoredPosition = new(0f, newYTop);
                bottomRect.anchoredPosition = new(0f, newYBottom);

                time += Time.deltaTime;
                yield return null;
            }

            topRect.anchoredPosition = new(0f, topYEnd);
            bottomRect.anchoredPosition = new(0f, bottomYEnd);

            animating = false;
            yield return null;
        }

        private IEnumerator AnimateTitleOut()
        {
            animating = true;

            float time = 0;
            float normalizedTime = 0;

            Util.EasingFunction.Function easeFunction = Util.EasingFunction.GetEasingFunction(Util.EasingFunction.Ease.EaseOutQuint);
            float topYEnd = 100f;
            float bottomYEnd = -90f;

            float topYStart = topRect.anchoredPosition.y;
            float bottomYStart = bottomRect.anchoredPosition.y;

            topRect.gameObject.SetActive(true);
            bottomRect.gameObject.SetActive(true);

            while (normalizedTime < 1f)
            {
                normalizedTime = Mathf.InverseLerp(0f, animationLength, time);

                float newYTop = easeFunction(topYStart, topYEnd, normalizedTime);
                float newYBottom = easeFunction(topYEnd, bottomYEnd, normalizedTime);

                topRect.anchoredPosition = new(0f, newYTop);
                bottomRect.anchoredPosition = new(0f, newYBottom);

                time += Time.deltaTime;
                yield return null;
            }

            topRect.anchoredPosition = new(0f, topYEnd);
            bottomRect.anchoredPosition = new(0f, bottomYEnd);

            topRect.gameObject.SetActive(false);
            bottomRect.gameObject.SetActive(false);

            animating = false;
            yield return null;
        }

        public void PlayLevel()
        {
            if (LibraryManager.instance.exiting) return;
            LibraryManager.instance.exiting = true;
            SoundByte.PlayOneShot("ui/UIEnter");
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
                GlobalGameManager.PlayOpenFileHash = LibraryManager.instance.allIconInfo[LibraryManager.instance.selectedLevelPath].hash;
                GlobalGameManager.returnToLibrary = true;
                GlobalGameManager.playbackSpeed = mods.tempoMult / 100f;
                GlobalGameManager.autoplay = mods.autoplayOn;
                LibraryManager.lastFolder = LibraryManager.instance.currentFolder;
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
            SoundByte.PlayOneShot("ui/UIEnter");
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
