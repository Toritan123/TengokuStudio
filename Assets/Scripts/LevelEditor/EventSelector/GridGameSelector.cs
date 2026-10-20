using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TMPro;
using DG.Tweening;

using HeavenStudio.Common;
using HeavenStudio.Util;
using HeavenStudio.Editor.Track;
using System.Text;
using System.Configuration;
using System;
using HeavenStudio.InputSystem;
using UnityEngine.U2D;

namespace HeavenStudio.Editor
{
    // I LOVE the antichrist!

    //I have no strong opinions towards the antichrist
    public class GridGameSelector : MonoBehaviour
    {
        public Minigames.Minigame SelectedMinigame;

        [Header("Components")]
        public GameObject SelectedGameIcon;
        public GameObject GameEventSelector;
        public GameObject EventRef;
        public GameObject CurrentSelected;
        public Scrollbar Scrollbar;
        public RectTransform GameSelectionRect;
        public RectTransform GameEventSelectorCanScroll;
        public TMP_InputField SearchBar;
        private RectTransform GameEventSelectorRect;
        private RectTransform eventsParent;

        [Header("Properties")]
        [SerializeField] private int currentEventIndex;
        public Texture Square;
        public Texture Circle;
        public List<RectTransform> mgsActive = new();
        public List<RectTransform> fxActive = new();
        public float posDif;
        public int ignoreSelectCount;
        private int dragTimes;
        private bool gameOpen;
        private float selectorHeight;
        private float eventSize;
        private float timeSinceUpdateIndex = 0.0f;
        string setFont;
        string currentSort;
        public bool showFanGames;
        public bool showAdvanced;

        public static GridGameSelector instance;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            showFanGames = PersistentDataManager.gameSettings.showFanGames;
            showAdvanced = PersistentDataManager.gameSettings.advancedFeatures;

            GameEventSelectorRect = GameEventSelector.GetComponent<RectTransform>();
            selectorHeight = GameEventSelectorRect.rect.height;
            eventSize = EventRef.GetComponent<RectTransform>().rect.height;

            eventsParent = EventRef.transform.parent.GetChild(2).GetComponent<RectTransform>();
            StartCoroutine(StartDelayed());

            RefreshGamesList();
        }

        private IEnumerator StartDelayed()
        {
            yield return null;
            SelectGame(fxActive[0].name);
        }

        private void Update()
        {
            if (Editor.instance.fullscreen) return;
            if (!EventParameterManager.instance.active)
            {
                if (gameOpen)
                {
                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        if (!IsPointerOverUIElement()) UpdateIndex(currentEventIndex + 1);
                    }
                    else if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        if (!IsPointerOverUIElement()) UpdateIndex(currentEventIndex - 1);
                    }
                }

                if (Input.mouseScrollDelta.y != 0)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(GameEventSelectorCanScroll, Input.mousePosition,Editor.instance.EditorCamera))
                    {
                        UpdateIndex(currentEventIndex - Mathf.RoundToInt(Input.mouseScrollDelta.y));
                    }
                }

            }

            //moved here so this updates dynamically with window scale
            UpdateScrollPosition();
        }

        public void RefreshGamesList()
        {
            for (int i = 0; i < mgsActive.Count; i++)
            {
                if (EventCaller.instance.GetMinigame(mgsActive[i].name).plusGame)
                {
                    if (!showFanGames)
                    {
                        mgsActive[i].gameObject.SetActive(false);
                    }
                    else mgsActive[i].gameObject.SetActive(true);
                }
            }
            for (int i = 0; i < fxActive.Count; i++)
            {
                if (EventCaller.instance.GetMinigame(fxActive[i].name).advanced)
                {
                    if (!showAdvanced)
                    {
                        fxActive[i].gameObject.SetActive(false);
                    }
                    else fxActive[i].gameObject.SetActive(true);
                }
            }
            Sort(PersistentDataManager.gameSettings.sortMode);
        }

        

        #region Functions

        public void UpdateIndex(int amount, bool updateCol = true)
        {
            currentEventIndex = amount;

            CurrentSelected.transform.DOKill();

            if (currentEventIndex < 0)
                currentEventIndex = eventsParent.childCount - 1;
            else if (currentEventIndex > eventsParent.childCount - 1)
                currentEventIndex = 0;

            CurrentSelected.transform.DOLocalMoveY(eventsParent.transform.GetChild(currentEventIndex).localPosition.y + eventsParent.transform.localPosition.y, 0.35f).SetEase(Ease.OutExpo);
            timeSinceUpdateIndex = 0;
        }

        private void UpdateScrollPosition()
        {
            selectorHeight = GameEventSelectorRect.rect.height;
            //eventSize = EventRef.GetComponent<RectTransform>().rect.height;
            
            Vector3 lastPos = EventRef.transform.parent.transform.localPosition;
            float end = 0;

            if ((currentEventIndex * eventSize >= selectorHeight/2) && (eventsParent.childCount * eventSize >= selectorHeight))
            {
                if (currentEventIndex * eventSize < eventsParent.childCount * eventSize - selectorHeight/2)
                    end = (currentEventIndex * eventSize) - selectorHeight/2;
                else
                    end = (eventsParent.childCount * eventSize) - selectorHeight + (eventSize * 0.33f);
            }
            EventRef.transform.parent.transform.localPosition = new Vector3(
                lastPos.x, 
                Mathf.Lerp(lastPos.y, end, 12 * Time.deltaTime),
                lastPos.z
            );

            timeSinceUpdateIndex += Time.deltaTime;

            CurrentSelected.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(
                    (Mathf.Cos(timeSinceUpdateIndex * 2.65f) * 12) + 12,
                    CurrentSelected.GetComponent<RectTransform>().anchoredPosition.y);
            SetColors();
        }

        // will automatically select game + game icon, and (eventually?) scroll to the game if it's offscreen
        // index is the event it will highlight (which was basically just added for pick block)
        // TODO: automatically scroll if the game is offscreen, because i can't figure out a good way to do it rn. -AJ
        public void SelectGame(string gameName, int index = 0)
        {
            if (SelectedGameIcon != null)
            {
                SelectedGameIcon.GetComponent<GridGameSelectorGame>().UnClickIcon();
            }

            SelectedMinigame = EventCaller.instance.GetMinigame(gameName);
            if (SelectedMinigame == null) {
                SelectGame("gameManager");
                Debug.LogWarning($"SelectGame() has failed, did you mean to input '{gameName}'?");
                return;
            }

            EventParameterManager.instance.Disable();

            gameOpen = true;

            DestroyEvents();
            AddEvents(index);

            SelectedGameIcon = transform.Find(gameName).gameObject;
            SelectedGameIcon.GetComponent<GridGameSelectorGame>().ClickIcon();

            currentEventIndex = index;
            UpdateIndex(index, false);
            if (PersistentDataManager.gameSettings.fontType == 1)
                {
                    Editor.instance.currentFont = "Rodin DB";
                }
            else if (PersistentDataManager.gameSettings.fontType == 0)
                {
                    Editor.instance.currentFont = $"{SelectedMinigame.font}";
                }
            
            setFont = Editor.instance.currentFont;
            // Editor.instance?.SetGameEventTitle($"Select game event for {SelectedMinigame.displayName.Replace("\n", "")}");
            string title = "";
            if (SelectedMinigame.tags.Count > 0)
            {
                title += $"<sprite=\"consoleIcons\" name=\"{SelectedMinigame.tags[0]}\"> ";
            }
            title += $"<font={setFont}>{SelectedMinigame.displayName.Replace("\n", "")}</font>";
            if (Editor.instance != null) Editor.instance.SetGameEventTitle(title);
        }

        public void RefreshTitle()
        {
            if (PersistentDataManager.gameSettings.fontType == 1)
                {
                    Editor.instance.currentFont = "Rodin DB";
                }
            else if (PersistentDataManager.gameSettings.fontType == 0)
                {
                    Editor.instance.currentFont = $"{SelectedMinigame.font}";
                }
            setFont = Editor.instance.currentFont;
            if (Editor.instance != null) Editor.instance.SetGameEventTitle($"<font={setFont}>{SelectedMinigame.displayName.Replace("\n", "")}</font>");
        }

        private void AddEvents(int index = 0)
        {
            if (!EventCaller.FXOnlyGames().Contains(SelectedMinigame))
            {
                GameObject sg = Instantiate(EventRef, eventsParent);
                sg.GetComponentInChildren<TMP_Text>().text = Localization.T("Switch Game");
                sg.SetActive(true);
                if (index == 0) {
                    sg.GetComponentInChildren<TMP_Text>().color = EditorTheme.theme.properties.EventSelectedCol.Hex2RGB();
                }
            } else {
                index++;
                if (SelectedMinigame.name == "gameManager") index++;
            }

            bool advancedEvents = PersistentDataManager.gameSettings.advancedFeatures;

            for (var i = 0; i < SelectedMinigame.actions.Count; i++)
            {
                var action = SelectedMinigame.actions[i];
                if (action.actionName == "switchGame" || action.hidden || (action.advanced && !advancedEvents)) continue;

                var g = Instantiate(EventRef, eventsParent);
                var label = g.GetComponentInChildren<TMP_Text>();

                label.text = action.displayName;
                if (advancedEvents && action.advanced)
                    label.text = "<color=#ff8>" + label.text;

                if (action.parameters != null && action.parameters.Count > 0)
                    g.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);

                if (index - 1 == i)
                    label.color = EditorTheme.theme.properties.EventSelectedCol.Hex2RGB();

                g.SetActive(true);
            }
        }

        private void DestroyEvents()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).GetChild(0).gameObject.SetActive(false);
            }

            for (int i = 0; i < eventsParent.childCount; i++)
            {
                Destroy(eventsParent.GetChild(i).gameObject);
            }
        }

        private void SetColors()
        {
            //CurrentSelected.GetComponent<Image>().color = EditorTheme.theme.properties.EventSelectedCol.Hex2RGB();

            for (int i = 0; i < eventsParent.transform.childCount; i++)
            {
                var eventTxt = eventsParent.GetChild(i).GetChild(0).GetComponent<TMP_Text>();
                var goalX = -25;
                if (i == currentEventIndex)
                {
                    eventTxt.color = EditorTheme.theme.properties.EventSelectedCol.Hex2RGB();
                    goalX = 16;
                }
                else
                {
                    eventTxt.color = EditorTheme.theme.properties.EventNormalCol.Hex2RGB();
                }
                eventTxt.rectTransform.anchoredPosition =
                    new Vector2(
                        Mathf.Lerp(eventTxt.rectTransform.anchoredPosition.x, goalX, Time.deltaTime * 12f),
                        eventTxt.rectTransform.anchoredPosition.y);
            }
        }

        public void Zoom()
        {
            if (!Input.GetKey(InputKeyboard.MODIFIER)) return;
            var glg = GetComponent<GridLayoutGroup>();
            int max = 20; // arbitrary

            if (glg.constraintCount + 1 > max && Input.mouseScrollDelta.y < 0) return;

            glg.constraintCount += (Input.mouseScrollDelta.y > 0) ? -1 : 1;

            // thanks to blank3times (tri) for helping me with this
            var size = (1 / (0.00317 * glg.constraintCount)) - 4.75248;

            // this, however, doesn't work
            // var totalWidth = Editor.instance.GridGameSelectorRect.rect.width;
            // var size = (totalWidth - glg.padding.right) * (glg.constraintCount + 1) / glg.constraintCount;

            glg.cellSize = Vector2.one * (float)size;
        }


        public void RefreshIcons()
        {
            List<RectTransform> mgList = mgsActive;
            foreach (var mg in mgList)
            {
                mg.GetComponent<Image>().sprite = Editor.GameIcon(mg.name);
                mg.GetComponent<GridGameSelectorGame>().MaskTex = Editor.GameIconMask(mg.name);
            }
        }


        

        // method called when clicking the sort button in the editor, skips sorting fx only "games"
        // sorts depending on which sorting button you click
        public void Sort(string type)
        {
            List<RectTransform> mgsSort = mgsActive;
            mgsSort.Sort((x, y) => string.Compare(x.name, y.name));

            if (currentSort != type) currentSort = type;

            Action<List<RectTransform>> action = type switch
            {
                "favorites" => SortFavorites,
                "chronologic" => SortChronologic,
                "usage" => SortUsage,
                _ => SortAlphabet
            };

            PersistentDataManager.gameSettings.sortMode = type;
            PersistentDataManager.SaveSettings();
            
            action.Invoke(mgsSort);
        }

        void SortAlphabet(List<RectTransform> mgs)
        {
            List<RectTransform> alph = mgs.OrderBy(AlphabetSortKey).ToList();
            
            for (int i = 0; i < alph.Count; i++) {
                alph[i].SetSiblingIndex(i + fxActive.Count + 1);
            }
        }
        string AlphabetSortKey(RectTransform minigame)
        {
            Minigames.Minigame mg = EventCaller.instance.GetMinigame(minigame.name);
            string localizedSortKey = Localization.GameSortKey(mg);
            if (localizedSortKey != null)
                return localizedSortKey;
            if (mg.displayName.StartsWith("the ", StringComparison.InvariantCultureIgnoreCase))
                return mg.displayName[4..];
            else if (mg.displayName.StartsWith("the☆", StringComparison.InvariantCultureIgnoreCase))
                return mg.displayName[4..];  
            else
                return mg.displayName;
        }

        // if there are no favorites, the games will sort alphabetically
        void SortFavorites(List<RectTransform> allMgs)
        {
            List<RectTransform> favs = allMgs.FindAll(mg => mg.GetComponent<GridGameSelectorGame>().StarActive).OrderBy(AlphabetSortKey).ToList();
            List<RectTransform> mgs  = allMgs.FindAll(mg => !mg.GetComponent<GridGameSelectorGame>().StarActive).OrderBy(AlphabetSortKey).ToList();

            if (Input.GetKey(KeyCode.LeftShift)) {
                foreach (var fav in favs)
                    fav.GetComponent<GridGameSelectorGame>().Star();
                return;
            }

            for (int i = 0; i < favs.Count; i++) {
                favs[i].SetSiblingIndex(i + fxActive.Count + 1);
            }
            for (int i = 0; i < mgs.Count; i++) {
                mgs[i].SetSiblingIndex(i + fxActive.Count + favs.Count + 1);
            }
        }

        void SortChronologic(List<RectTransform> mgs)
        {
            List<RectTransform> chrono = mgs.OrderBy(GameOriginSortKey).ThenBy(ChronologicSortKey).ThenBy(AlphabetSortKey).ToList();
            
            for (int i = 0; i < chrono.Count; i++) {
                chrono[i].SetSiblingIndex(i + fxActive.Count + 1);
            }
        }
        int GameOriginSortKey(RectTransform minigame)
        {
            Minigames.Minigame mg = EventCaller.instance.GetMinigame(minigame.name);
            if (mg.tags.Count > 0)
            {
                return mg.tags[0] switch
                {
                    "agb" => 0,
                    "ntr" => 1,
                    "rvl" => 2,
                    "ctr" => 3,
                    _ => 10,
                };
            }

            return 10;
        }
        uint ChronologicSortKey(RectTransform minigame)
        {
            Minigames.Minigame mg = EventCaller.instance.GetMinigame(minigame.name);
            if (mg.chronologicalSortKey is uint i)
                return i;

            return uint.MaxValue;
        }

        void SortUsage(List<RectTransform> mgs)
        {
            var beatTotals = CalculateMinigameActiveBeats();

            List<RectTransform> usage = mgs.OrderByDescending(mg => beatTotals.TryGetValue(mg.name, out var beats) ? beats : 0).ThenBy(AlphabetSortKey).ToList();

            for (int i = 0; i < usage.Count; i++)
            {
                usage[i].SetSiblingIndex(i + fxActive.Count + 1);
            }
        }
        Dictionary<string, double> CalculateMinigameActiveBeats()
        {
            var totals = new Dictionary<string, double>();

            var switches = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" }).OrderBy(x => x.beat).ToList();
            var ends = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "end" }).OrderBy(x => x.beat).ToList();

            double songLength = Conductor.instance.SongLengthInBeatsAsDouble();
            if (songLength == 0) songLength = 320; // the default timeline length
            if (ends.Count > 0) songLength = ends[0].beat;

            for (int i = 0; i < switches.Count; i++)
            {
                var current = switches[i];

                double startBeat = current.beat;
                double endBeat = (i < switches.Count - 1)
                    ? switches[i + 1].beat
                    : songLength;

                double duration = endBeat - startBeat;

                string[] parts = current.datamodel.Split('/');
                if (parts.Length < 3) continue;

                string minigameName = parts[2];

                if (!totals.ContainsKey(minigameName))
                    totals[minigameName] = 0;

                totals[minigameName] += duration;
            }

            return totals;
        }

        private static readonly Dictionary<string, string> keywordToTag = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // games
            { "rhythm heaven", "agb|ntr|rvl|ctr" },
            { "rhythm paradise", "agb|ntr|rvl|ctr" },
            { "official", "agb|ntr|rvl|ctr" },

            { "gba", "agb" },
            { "tengoku", "agb" },
            { "rhythm tengoku", "agb" },

            { "ds", "ntr" },
            { "nds", "ntr" },
            { "rhds", "ntr" },
            { "gold", "ntr" },
            { "rhythm heaven ds", "ntr" },
            { "rhythm paradise ds", "ntr" },

            { "wii", "rvl" },
            { "fever", "rvl" },
            { "rhythm heaven fever", "rvl" },
            { "btb", "rvl" },
            { "beat the beat", "rvl" },

            { "3ds", "ctr" },
            { "megamix", "ctr" },
            { "rhythm heaven megamix", "ctr" },
            { "rhythm paradise megamix", "ctr" },

            { "warioware", "miw" },
            { "wario", "miw" },

            { "fangame", "pco|mob" },
            { "fangames", "pco|mob" },
            { "fanmade", "pco|mob" },
            { "plus", "pco|mob" },
            { "unofficial", "pco|mob" },

            // cue types
            { "cue", "normal" },
            { "cue based", "normal" },
            { "cue-based", "normal" },

            { "ktb", "keep" },
            { "keep the beat", "keep" },

            { "c&r", "repeat" },
            { "cnr", "repeat" },
            { "call & response", "repeat" },
            { "call and response", "repeat" },
            { "repeat", "repeat" },
            { "simon says", "repeat" },

            { "aim", "aim" },
        };

        public void Search()
        {
            string input = SearchBar.text;
            string lowerInput = input.ToLowerInvariant();

            bool hasTagMatched = false;

            for (int i = 0; i < mgsActive.Count; i++)
            {
                var minigame = EventCaller.instance.GetMinigame(mgsActive[i].name);

                // also match the english name and the reading, so either can be searched for
                string searchableName = minigame.displayName;
                if (minigame.englishDisplayName != null && minigame.englishDisplayName != minigame.displayName)
                    searchableName += "\n" + minigame.englishDisplayName;
                string sortKey = Localization.GameSortKey(minigame);
                if (sortKey != null)
                    searchableName += "\n" + sortKey;

                bool nameMatch = System.Text.RegularExpressions.Regex.IsMatch(
                    searchableName,
                    input,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );

                bool tagMatch = false;

                if (minigame.tags != null)
                {
                    foreach (var pair in keywordToTag)
                    {
                        if (lowerInput == pair.Key)
                        {
                            tagMatch = pair.Value.Split('|').Any(tag => minigame.tags.Contains(tag));

                            if (tagMatch)
                            {
                                hasTagMatched = true;
                                break;
                            }
                        }
                    }
                }

                if (!minigame.plusGame || PersistentDataManager.gameSettings.showFanGames)
                {
                    mgsActive[i].gameObject.SetActive(hasTagMatched ? tagMatch : nameMatch);
                }
            }
        }

        public bool IsPointerOverUIElement()
        {
            return IsPointerOverUIElement(GetEventSystemRaycastResults());
        }

        private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
        {
            for (int index = 0; index < eventSystemRaysastResults.Count; index++)
            {
                RaycastResult curRaysastResult = eventSystemRaysastResults[index];
                if (curRaysastResult.gameObject.layer == 5)
                    return true;
            }
            return false;
        }

        static List<RaycastResult> GetEventSystemRaycastResults()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            List<RaycastResult> raysastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raysastResults);
            return raysastResults;
        }

        #endregion

        #region Events

        private List<Minigames.GameAction> visibleActions;

        public void Drag()
        {
            if (Conductor.instance.NotStopped() || Editor.instance.inAuthorativeMenu) {
                if (Conductor.instance.isPaused) {
                    Debug.Log("it's fuckin paused dude");
                }
                return;
            }
            
            if (Timeline.instance.MouseInTimeline && dragTimes < 1)
            {
                Timeline.instance.timelineState.SetState(Timeline.CurrentTimelineState.State.Selection);
                dragTimes++;

                TimelineEventObj eventObj;

                visibleActions = new List<Minigames.GameAction>();

                foreach (Minigames.GameAction action in SelectedMinigame.actions)
                {
                    if (!action.hidden)
                    {
                        if (!(action.advanced && !PersistentDataManager.gameSettings.advancedFeatures))
                            visibleActions.Add(action);
                    }
                }

                if (currentEventIndex == 0)
                {
                    if (EventCaller.FXOnlyGames().Contains(SelectedMinigame))
                    {
                        int index = currentEventIndex + 1;
                        if (currentEventIndex - 1 > visibleActions.Count)
                        {
                            index = currentEventIndex;
                        }
                        else if (currentEventIndex - 1 < 0)
                        {
                            if (visibleActions[0].actionName == "switchGame")
                                index = 1;
                            else
                                index = 0;
                        }

                        eventObj = Timeline.instance.AddEventObject(SelectedMinigame.name + "/" + visibleActions[index].actionName, true, new Vector3(0, 0), null, true);
                    }
                    else
                        eventObj = Timeline.instance.AddEventObject($"gameManager/switchGame/{SelectedMinigame.name}", true, new Vector3(0, 0), null, true);
                }
                else
                {
                    int index = currentEventIndex - 1;
                    if (SelectedMinigame.actions[0].actionName == "switchGame")
                    {
                        index = currentEventIndex + 1;
                    }
                    else if (EventCaller.FXOnlyGames().Contains(SelectedMinigame) && visibleActions[0].actionName != "switchGame")
                    {
                        index = currentEventIndex;
                    }


                    if (SelectedMinigame.actions[index].actionName.Contains('/'))
                    {
                        eventObj = Timeline.instance.AddEventObject(visibleActions[index].actionName, true, new Vector3(0, 0), null, true);
                    }
                    else { eventObj = Timeline.instance.AddEventObject(SelectedMinigame.name + "/" + visibleActions[index].actionName, true, new Vector3(0, 0), null, true); }
                }

                eventObj.isCreating = true;

                // CommandManager.instance.Execute(new Commands.Place(eventObj));
            }
        }

        public void Drop()
        {
            if (Conductor.instance.NotStopped()) return;

            dragTimes = 0;
        }

        #endregion
    }
}