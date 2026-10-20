using UnityEngine;
using UnityEngine.UI;

using Jukebox;
using HeavenStudio.Util;
using TMPro;
using System;
using System.Linq;
using System.Collections.Generic;
using HeavenStudio.Common;

namespace HeavenStudio.Editor.Track
{
    public class TimelineEventObj : MonoBehaviour
    {
        protected RectTransform rectTransform;

        [Header("Components")]
        [SerializeField] protected Image[] recolourables;
        [SerializeField] protected Image Icon;
        [SerializeField] protected Image selectedImage;
        [SerializeField] protected TMP_Text eventLabel;

        [SerializeField] protected Image resizeGraphic;
        [SerializeField] protected GameObject resizeLObj;
        [SerializeField] protected GameObject resizeRObj;

        [SerializeField] protected Image outline;
        [SerializeField] protected Image hasPropertiesIcon;

        [SerializeField] protected RectTransform visibleRegion;

        [Header("Properties")]
        public RiqEntity entity;
        [NonSerialized] public float length;
        [NonSerialized] public bool selected;
        [NonSerialized] public bool mouseHovering;
        [NonSerialized] public bool resizable;
        [NonSerialized] public bool moving;
        [NonSerialized] public bool isCreating;
        [NonSerialized] public Timeline.EventVisibility visibility;
        [NonSerialized] public string[] split;

        public bool resizing => resizingLeft || resizingRight;
        private bool resizingLeft;
        private bool resizingRight;
        private bool inResizeRegion;
        private float resizingLeftBL;

        private double lastResizeBeat = 0;
        private float lastResizeLength = 0;


        private bool altWhenClicked = false;
        private bool dragging = false;

        protected double initMoveX = 0;
        protected float initMoveY = 0;
        protected double selectedMinInitMoveX, selectedMaxInitMoveX;
        protected float selectedMinInitMoveY, selectedMaxInitMoveY;

        private bool movedEntity = false;
        protected double lastBeat = 0;
        protected int lastLayer = 0;
        protected float lastLength = 0;

        private int lastSiblingIndex;
        private bool changedSiblingIndex;

        public float zPriority { get; protected set; }

        // Difference between mouseHovering is this is regardless if the user can see it.
        public bool mouseOver;

        private float clickTimer = 0.0f;

        private List<float> originalLengths = new List<float>();
        private List<double> originalBeats = new List<double>();
        private float selectionStartBeat;
        private float selectionEndBeat;
        private bool addLengths;
        private float changeLengthValue = 0f;
        private float resizeStartBeat;

        public void SetMarkerInfo()
        {
            rectTransform = GetComponent<RectTransform>();

            var eventName = entity.datamodel;

            split = eventName.Split('/');
            var action = EventCaller.instance.GetGameAction(split[0], split[1]);

            if (split[0] == "gameManager")
            {
                if (split[1] == "switchGame")
                {
                    entity["track"] = 0;
                    Icon.gameObject.SetActive(true);
                    Icon.sprite = Editor.GameIcon(split[2]);
                    eventLabel.text = EventCaller.instance.GetMinigame(split[2]).displayName;
                }
                else if (split[1] == "end")
                {
                    entity["track"] = 0;
                    Icon.gameObject.SetActive(false);
                    eventLabel.text = action.displayName;
                }
                else if (split[1] == "startSong")
                {
                    entity["track"] = 0;
                    Icon.gameObject.SetActive(false);
                    eventLabel.text = action.displayName;
                }
                else
                {
                    Icon.sprite = Editor.GameIcon(split[0]);
                    eventLabel.text = action.displayName;
                }
            }
            else
            {
                Icon.sprite = Editor.GameIcon(split[0]);
                eventLabel.text = action.displayName;
            }

            if (action != null)
            {
                resizable = action.resizable;
                if (action.resizable)
                {
                    if (entity != null)
                    {
                        length = entity.length;
                        rectTransform.sizeDelta = new Vector2(entity.length * Timeline.instance.PixelsPerBeat, Timeline.instance.LayerHeight());
                    }
                    else
                    {
                        length = action.defaultLength;
                        rectTransform.sizeDelta = new Vector2(action.defaultLength * Timeline.instance.PixelsPerBeat, Timeline.instance.LayerHeight());
                    }
                }
                else
                {
                    entity.length = action.defaultLength;
                    length = action.defaultLength;
                    rectTransform.sizeDelta = new Vector2(action.defaultLength * Timeline.instance.PixelsPerBeat, Timeline.instance.LayerHeight());
                }
            }

            rectTransform.anchoredPosition = new Vector2((float)entity.beat * Timeline.instance.PixelsPerBeat, (int)-entity["track"] * Timeline.instance.LayerHeight());
            if (resizeGraphic != null)
            {
                resizeGraphic.gameObject.SetActive(resizable);
            }
            
            if (hasPropertiesIcon != null)
            {
                hasPropertiesIcon.enabled = action.actionName != "switchGame" && action.parameters != null && action.parameters.Count > 0;
            }

            SetColor();
            SetWidthHeight();
            selectedImage.gameObject.SetActive(false);

            lastBeat = entity.beat;
            initMoveX = 0.0f;
            initMoveY = 0.0f;
            lastResizeBeat = 0;
            lastResizeLength = 0;
        }

        public void SetEntity(RiqEntity entity)
        {
            this.entity = entity;
        }

        protected void Update()
        {
            clickTimer += Time.deltaTime;
        }

        public void UpdateMarker()
        {
            if (selected)
            {
                if (TimelineBlockManager.Instance.MovingAnyEvents)
                {
                    outline.color = Color.magenta;
                    SetColor();
                }
                else
                    outline.color = Color.cyan;
            }
            else
            {
                outline.color = new Color32(0, 0, 0, 51);
            }

            if (Conductor.instance.NotStopped())
            {
                if (moving)
                {
                    moving = false;
                }

                if (selected)
                {
                    Selections.instance.Deselect(this);
                    outline.color = new Color32(0, 0, 0, 51);
                }
                return;
            }

            if (resizingRight)
            {
                Resize(false);
            }
            else if (resizingLeft)
            {
                Resize(true);
            }
            else
            {
                if (Input.GetMouseButtonUp(0))
                {
                    if (moving)
                    {
                        moving = false;

                        if (!isCreating && movedEntity)
                        {
                            // NOTE (PELLY): Replace with arrays soon
                            List<double> lastBeats = new();
                            List<int> lastLayers = new();
                            foreach (var marker in Selections.instance.eventsSelected)
                            {
                                var entity = marker.entity;

                                lastBeats.Add(marker.entity.beat);
                                lastLayers.Add((int)marker.entity["track"]);

                                entity.beat = marker.lastBeat;
                                entity["track"] = marker.lastLayer;
                            }
                            CommandManager.Instance.AddCommand(new Commands.Move(Selections.instance.eventsSelected.Select(c => c.entity).ToList(), lastBeats, lastLayers));
                        }

                        isCreating = false;

                        GameManager.instance.SortEventsList();
                        GridGameSelector.instance.Sort(PersistentDataManager.gameSettings.sortMode);
                        TimelineBlockManager.Instance.SortMarkers();
                    }

                    altWhenClicked = false;
                    dragging = false;
                }

                if (moving)
                {
                    foreach (var marker in Selections.instance.eventsSelected)
                    {
                        var nextBeat = System.Math.Max(Timeline.instance.MousePos2BeatSnap, selectedMaxInitMoveX) - marker.initMoveX;
                        var nextTrack = Mathf.Clamp(Timeline.instance.MousePos2Layer, selectedMaxInitMoveY, Timeline.instance.LayerCount - 1 + selectedMinInitMoveY) - marker.initMoveY;
                        marker.entity.beat = System.Math.Max(nextBeat, 0);
                        marker.entity["track"] = Mathf.Clamp(nextTrack, 0, Timeline.instance.LayerCount - 1);
                        marker.SetColor();
                        marker.SetWidthHeight();
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                OnLeftUp();
                OnRightUp();
            }

            // should consider adding this someday
            // else if (moving && selected || mouseHovering && selected)
            // {
            //     Cursor.SetCursor(Resources.Load<Texture2D>("Cursors/move"), new Vector2(8, 8), CursorMode.Auto);
            // }

            zPriority = entity.length;

            if (selected)
                zPriority += 10000;
        }

        public void Resize(bool left)
        {
            if (moving) moving = false;

            float currentLength = entity.length;

            double newBeat = Timeline.instance.MousePos2BeatSnap;
            float newLength = Timeline.instance.MousePos2BeatSnap - (float)entity.beat;
            if (left)
            {
                newLength = (float)(entity.beat + entity.length) - (float)newBeat;
            }

            changeLengthValue += newLength - currentLength;

            var selected = Selections.instance.eventsSelected;

            if (originalLengths.Count == 0)
            {
                addLengths = true;
                resizeStartBeat = Timeline.instance.MousePos2BeatSnap;

                selectionStartBeat = float.MaxValue;
                selectionEndBeat   = float.MinValue;

                for (int i = 0; i < selected.Count; i++)
                {
                    var e = selected[i].entity;
                    float start = (float)e.beat;
                    float end = start + e.length;

                    selectionStartBeat = Mathf.Min(selectionStartBeat, start);
                    selectionEndBeat = Mathf.Max(selectionEndBeat, end);
                }
            }
            changeLengthValue = left 
                ? resizeStartBeat - Timeline.instance.MousePos2BeatSnap
                : Timeline.instance.MousePos2BeatSnap - resizeStartBeat;

            for (int i = 0; i < selected.Count; i++)
            {
                var entity = selected[i].entity;

                if (addLengths)
                {
                    originalLengths.Add(entity.length);
                    originalBeats.Add(entity.beat);
                }

                var entitySplit = entity.datamodel.Split('/');
                var action = EventCaller.instance.GetGameAction(entitySplit[0], entitySplit[1]);
        
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) // proportional resize
                {
                    float selectLength = left
                        ? Mathf.Max(selectionEndBeat - Timeline.instance.MousePos2BeatSnap, Timeline.instance.snapInterval)
                        : Mathf.Max(Timeline.instance.MousePos2BeatSnap - selectionStartBeat, Timeline.instance.snapInterval);

                    float scale = selectLength / (selectionEndBeat - selectionStartBeat);

                    double beatOffset = originalBeats[i] - selectionStartBeat;
                    double lengthOffset = selectionEndBeat - (originalBeats[i] + originalLengths[i]);

                    float lengthChanged;

                    if (left)
                    {
                        lengthChanged = Mathf.Max(originalLengths[i] * scale, Timeline.instance.snapInterval);
                        entity.beat = selectionEndBeat - (lengthOffset * scale) - lengthChanged;
                    }
                    else
                    {
                        lengthChanged = Mathf.Max(originalLengths[i] * scale, Timeline.instance.snapInterval);
                        entity.beat = selectionStartBeat + (beatOffset * scale); 
                    }

                    if (action.resizable) 
                    {
                        entity.length = lengthChanged;
                    }
                }
                else // just resize everything the same amount
                {
                    entity.beat = originalBeats[i];
                    if (action.resizable)
                    {
                        if (left)
                        {
                            entity.beat = Math.Min(originalBeats[i] - changeLengthValue, originalBeats[i] + originalLengths[i] - Timeline.instance.snapInterval);
                            entity.length = (float)(originalBeats[i] + originalLengths[i] - entity.beat);
                        }
                        else
                        {
                            entity.length = Mathf.Max(originalLengths[i] + changeLengthValue, Timeline.instance.snapInterval);
                        }
                    }
                }
            }
            addLengths = false;

            SetWidthHeight();
        }

        public void LateUpdate()
        {
            // event visibility
            bool shouldBeActive = visibility != Timeline.EventVisibility.Hidden;

            if (gameObject.activeSelf != shouldBeActive)
            {
                gameObject.SetActive(shouldBeActive);
            }

            // hover check
            mouseOver = Timeline.instance.timelineState.selected && Timeline.instance.MouseInTimeline &&
                MathUtils.IsBetween(Timeline.instance.MousePos2Beat, (float)entity.beat, (float)entity.beat + entity.length) &&
                Timeline.instance.MousePos2Layer == (int)entity["track"];

            if (mouseHovering && !mouseOver)
            {
                HoverExit();
            }

            eventLabel.overflowMode = (mouseHovering || moving || resizing || inResizeRegion) ? TextOverflowModes.Overflow : TextOverflowModes.Ellipsis;

            rectTransform.anchoredPosition = new Vector2((float)entity.beat * Timeline.instance.PixelsPerBeat, -(int)entity["track"] * Timeline.instance.LayerHeight());
            SetWidthHeight();

            var followXL = (Timeline.instance.leftSide - (float)entity.beat) * Timeline.instance.PixelsPerBeat;
            visibleRegion.offsetMin = new Vector2(
                Mathf.Clamp(followXL - 2, 0, (entity.length * Timeline.instance.PixelsPerBeat) - Timeline.instance.LayerHeight()),
                visibleRegion.offsetMin.y);

            var followXR = (Timeline.instance.rightSide - ((float)entity.beat + entity.length)) * Timeline.instance.PixelsPerBeat;
            visibleRegion.offsetMax = new Vector2(
                Mathf.Clamp(followXR, -(entity.length * Timeline.instance.PixelsPerBeat) + 8, 0),
                visibleRegion.offsetMax.y);
        }

        public void BeginMoving(bool setMovedEntity = true)
        {
            moving = true;

            foreach (var marker in Selections.instance.eventsSelected)
            {
                if (setMovedEntity) marker.movedEntity = true;
                marker.SetInitialMove();
            }
            selectedMinInitMoveX = Selections.instance.eventsSelected.Min(marker => marker.initMoveX);
            selectedMaxInitMoveX = Selections.instance.eventsSelected.Max(marker => marker.initMoveX);
            selectedMinInitMoveY = Selections.instance.eventsSelected.Min(marker => marker.initMoveY);
            selectedMaxInitMoveY = Selections.instance.eventsSelected.Max(marker => marker.initMoveY);
        }

        public void SetInitialMove()
        {
            lastBeat = entity.beat;
            lastLayer = (int)entity["track"];

            initMoveX = Timeline.instance.MousePos2BeatSnap - entity.beat;
            initMoveY = Timeline.instance.MousePos2Layer - (int)entity["track"];
        }
    
        public void SetInitialResize()
        {
            lastBeat = entity.beat;
            lastLength = entity.length;
        }


        #region ClickEvents

        public void HoverEnter()
        {
            if (visibility != Timeline.EventVisibility.Visible) return;

            if (!TimelineBlockManager.Instance.MovingAnyEvents)
            {
                lastSiblingIndex = gameObject.transform.GetSiblingIndex();
                gameObject.transform.SetAsLastSibling();
                changedSiblingIndex = true;
            }
            else
                changedSiblingIndex = false;

            mouseHovering = true;
            if (!selected && !TimelineBlockManager.Instance.MovingAnyEvents) selectedImage.gameObject.SetActive(true);
        }

        public void HoverExit()
        {
            if (changedSiblingIndex)
                gameObject.transform.SetSiblingIndex(lastSiblingIndex);

            mouseHovering = false;
            selectedImage.gameObject.SetActive(false);
        }

        public void OnDragMain()
        {
            if (Conductor.instance.NotStopped()) return;
            if (!Timeline.instance.timelineState.selected) return;
            if (visibility != Timeline.EventVisibility.Visible) return;

            if (Input.GetMouseButton(1) || Input.GetMouseButton(2)) return;
            if (!moving)
                if (!altWhenClicked)
                    altWhenClicked = Input.GetKey(KeyCode.LeftAlt);

            if (!altWhenClicked)
            {
                if (!selected)
                    Selections.instance.ClickSelect(this);
                if (!moving)
                    BeginMoving();

                return;
            }

            if (dragging) return;

            var entities = Selections.instance.eventsSelected;
            if (entities.Count == 0)
            {
                entities = new() { this };
            }
            CommandManager.Instance.AddCommand(new Commands.Duplicate(entities));

            dragging = true;
        }

        public void OnDown()
        {
            if (Conductor.instance.NotStopped()) return;
            if (moving) return;
            if (!Timeline.instance.timelineState.selected) return;
            if (visibility != Timeline.EventVisibility.Visible) return;

            if (Input.GetMouseButton(0))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    Selections.instance.ShiftClickSelect(this);
                }
                else
                {
                    if (selected && clickTimer < 0.315f)
                    {
                        foreach (var marker in TimelineBlockManager.Instance.EntityMarkers.Values)
                        {
                            if (marker == this) continue;
                            if (marker.mouseOver)
                            {
                                Selections.instance.ClickSelect(marker);
                                marker.clickTimer = 0;
                                break;
                            }
                        }
                    }
                    else if (!selected)
                        Selections.instance.ClickSelect(this);
                }
            }
            else if (Input.GetMouseButton(1))
            {
                EventParameterManager.instance.StartParams(entity);
            }
            else if (Input.GetMouseButton(2))
            {
                Debug.Log("Selected entity's datamodel : " + entity.datamodel);

                bool isSwitchGame = split[1] == "switchGame";
                var game = EventCaller.instance.minigames[split[isSwitchGame ? 2 : 0]];
                int block = isSwitchGame ? 0 : game.actions.FindIndex(c => c.actionName == split[1]) + 1;

                if (!isSwitchGame)
                {
                    // hardcoded stuff
                    // needs to happen because hidden blocks technically change the event index
                    if (game.fxOnly) block--;
                    if (split[0] == "gameManager") block--;
                }

                GridGameSelector.instance.SelectGame(split[isSwitchGame ? 2 : 0], block);
            }

            clickTimer = 0;
        }

        #endregion

        #region ResizeEvents

        public void ResizeEnter()
        {
            if (Conductor.instance.NotStopped()) return;
            if (BoxSelection.instance.ActivelySelecting || !resizable || moving) return;

            inResizeRegion = true;

            Cursor.SetCursor(Timeline.instance.resizeCursor, new Vector2(14, 14), CursorMode.Auto);
            foreach (var marker in Selections.instance.eventsSelected)
            {
                marker.SetInitialResize();
            }
        }

        public void ResizeExit()
        {
            inResizeRegion = false;

            if (!resizing)
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        public void OnLeftDown()
        {
            if (BoxSelection.instance.ActivelySelecting) return;

            if (resizable && selected)
            {
                ResetResize();

                resizingLeft = true;

                resizingLeftBL = (float)entity.beat + entity.length;

                lastResizeBeat = entity.beat;
                lastResizeLength = entity.length;
            }
        }

        public void OnLeftUp()
        {
            if (resizable && resizingLeft)
            {
                ResetResize();
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

                CommandManager.Instance.AddCommand(new Commands.Resize(Selections.instance.eventsSelected.Select(c => c.entity).ToList(), originalBeats, originalLengths));

                originalLengths.Clear();
                originalBeats.Clear();
                changeLengthValue = 0f;
            }
        }

        public void OnRightDown()
        {
            if (BoxSelection.instance.ActivelySelecting) return;

            if (resizable && selected)
            {
                ResetResize();

                resizingRight = true;

                lastResizeBeat = entity.beat;
                lastResizeLength = entity.length;
            }
        }

        public void OnRightUp()
        {
            if (resizable && resizingRight)
            {
                ResetResize();
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

                CommandManager.Instance.AddCommand(new Commands.Resize(Selections.instance.eventsSelected.Select(c => c.entity).ToList(), originalBeats, originalLengths));

                originalLengths.Clear();
                originalBeats.Clear();
                changeLengthValue = 0f;
            }
        }

        private void ResetResize()
        {
            resizingLeft = false;
            resizingRight = false;
        }

        #endregion

        #region Extra

        public virtual void SetColor()
        {
            var c = EditorTheme.theme.LayerGradientIndex((int)entity["track"]); // set to track color (default)
            if (PersistentDataManager.gameSettings.editorMinigameColorEvents)
            {
                c = StringUtils.Hex2RGB(EventCaller.instance.GetMinigame(split[0]).color); // set to minigame color
            }

            // check visibility type
            if (split[0] == "vfx" || split[0] == "ppe") 
            {
                visibility = Timeline.instance.vfxVisibility;
            }
            else if (EventCaller.FXOnlyGames().Contains(EventCaller.instance.GetMinigame(split[0])))
            {
                visibility = Timeline.instance.otherVisibility;
            }
            else
            {
                visibility = Timeline.instance.minigameVisibility;
            }

            // and use it
            if (visibility == Timeline.EventVisibility.Transparent)
            {
                c.a = 0.35f;
                eventLabel.color = new Color(1f, 1f, 1f, 0.35f);
            }
            else if (visibility == Timeline.EventVisibility.Visible)
            {
                eventLabel.color = Color.white;
                gameObject.SetActive(true);
            }

            gameObject.SetActive(visibility != Timeline.EventVisibility.Hidden);

            foreach (var r in recolourables)
            {
                r.color = c;
            }

            if (resizable && resizeGraphic != null)
            {
                c = new Color(0, 0, 0, 0.35f);
                resizeGraphic.color = c;
            }
        }

        public void SetWidthHeight()
        {
            rectTransform.sizeDelta = new Vector2(entity.length * Timeline.instance.PixelsPerBeat, Timeline.instance.LayerHeight());
            Icon.rectTransform.sizeDelta = new Vector2(Timeline.instance.LayerHeight() - 8, Timeline.instance.LayerHeight() - 8);
            eventLabel.rectTransform.offsetMin = new Vector2(Icon.rectTransform.anchoredPosition.x + Icon.rectTransform.sizeDelta.x + 4, eventLabel.rectTransform.offsetMin.y);
        }

        public int GetTrack()
        {
            return (int)Mathf.Round(this.transform.localPosition.y / Timeline.instance.LayerHeight()) * -1;
        }

        public virtual bool BoxSelectOverlapping(double startBeat, double endBeat, int startTrack, int endTrack)
        {
            bool beatOverlaps = entity.beat <= endBeat && entity.beat + entity.length >= startBeat;

            bool trackOverlaps = ((int)entity["track"]).IsBetween(startTrack, endTrack);

            return beatOverlaps && trackOverlaps;
        }
        #endregion

        public virtual void OnSelect()
        {
            if (resizeLObj == null || resizeRObj == null) return;
            resizeLObj.SetActive(resizable);
            resizeRObj.SetActive(resizable);
        }

        public virtual void OnDeselect()
        {
            if (resizeLObj == null || resizeRObj == null) return;
            resizeLObj.SetActive(false);
            resizeRObj.SetActive(false);
        }
    }
}