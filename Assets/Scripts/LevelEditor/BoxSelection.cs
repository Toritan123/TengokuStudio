using UnityEngine;
using UnityEngine.UI;

using HeavenStudio.Util;
using HeavenStudio.Editor.Track;

using DG.Tweening;
using System;

namespace HeavenStudio.Editor
{
    public class BoxSelection : MonoBehaviour
    {
        /// <summary>
        /// Are we currently drag selecting?
        /// </summary>
        public bool ActivelySelecting { get; private set; } = false;

        // private Vector2 startPosition = Vector2.zero;
        // private Vector2 endPosition = Vector2.zero;
        private double startBeat, endBeat;
        private int startTrack, endTrack;
        private bool validClick = false;

        [SerializeField] private RectTransform boxVisual;
        private CanvasGroup boxGroup;
        private TMPro.TMP_Text sizeText;

        public static BoxSelection instance { get; private set; }

        private void Start()
        {
            instance = this;

            Color boxCol = EditorTheme.theme.properties.BoxSelectionCol.Hex2RGB();
            boxVisual.GetChild(0).GetComponent<Image>().color = new Color(boxCol.r, boxCol.g, boxCol.b, 0.3f);
            boxVisual.GetChild(0).GetChild(0).GetComponent<Image>().color = EditorTheme.theme.properties.BoxSelectionOutlineCol.Hex2RGB();

            sizeText = boxVisual.GetChild(0).GetChild(1).GetComponent<TMPro.TMP_Text>();
            sizeText.text = string.Empty;

            boxGroup = boxVisual.GetComponent<CanvasGroup>();
        }

        public void LayerSelectUpdate()
        {
            if (!Timeline.instance.timelineState.selected) return;
            if (Input.GetMouseButtonDown(0))
            {
                if (!Timeline.instance.MouseInTimeline || TimelineBlockManager.Instance.InteractingWithEvents || Conductor.instance.NotStopped())
                {
                    return;
                }

                validClick = true;
                startBeat = Math.Max(Timeline.instance.MousePos2Beat, 0);
                startTrack = Mathf.RoundToInt(Timeline.instance.MousePos2Layer);

                boxGroup.DOKill();
                boxGroup.alpha = 1.0f;
            }

            if (!validClick) return;

            Vector2 startPos = new Vector2((float)startBeat, startTrack);
            Vector2 endPos = Vector2.zero;

            if (Input.GetMouseButton(0))
            {
                endBeat = Math.Max(Timeline.instance.MousePos2Beat, 0);
                endTrack = Mathf.RoundToInt(Timeline.instance.MousePos2Layer);

                startPos = new Vector2((float)startBeat, Mathf.Clamp(startTrack, 0, Timeline.instance.LayerCount) + (endTrack >= startTrack ? 0 : 1));
                endPos = new Vector2((float)endBeat, Mathf.Clamp(endTrack, 0, Timeline.instance.LayerCount) + (endTrack >= startTrack ? 1 : 0));

                ActivelySelecting = true;

                if (Conductor.instance.NotStopped())
                {
                    validClick = false;
                    boxGroup.DOFade(0.0f, 0.3f).SetEase(Ease.OutExpo);

                    ActivelySelecting = false;
                    return;
                }
            }

            var start = new Vector2(Mathf.Min(startPos.x, endPos.x),
                Mathf.Min(startPos.y, endPos.y));
            var end = new Vector2(Mathf.Max(startPos.x, endPos.x),
                Mathf.Max(startPos.y, endPos.y));

            if (Input.GetMouseButtonUp(0))
            {
                validClick = false;
                boxGroup.DOFade(0.0f, 0.3f).SetEase(Ease.OutExpo);

                ActivelySelecting = false;
                return;
            }

            boxVisual.anchoredPosition = new Vector2(start.x * Timeline.instance.PixelsPerBeat, Timeline.instance.LayerToY(Mathf.FloorToInt(start.y)));
            boxVisual.sizeDelta = new Vector2((end.x - start.x) * Timeline.instance.PixelsPerBeat,
                (end.y - start.y) * Timeline.instance.LayerHeight());

            var boxLength = end.x - start.x;
            if (boxLength > 0.01f)
                sizeText.text = (boxLength).ToString("F");
            else
                sizeText.text = string.Empty;

            // Keeps the text always in view
            var sizeTextLeft = Timeline.instance.leftSide - start.x;
            sizeTextLeft = Mathf.Max(sizeTextLeft, 0);
            var sizeTextRight = -(Timeline.instance.rightSide - end.x);
            sizeTextRight = Mathf.Max(sizeTextRight, 0);

            var sizeTextTop = Timeline.instance.topSide - start.y;
            sizeTextTop = Mathf.Max(sizeTextTop, 0);
            var sizeTextBottom = -(Timeline.instance.bottomSide - end.y);
            sizeTextBottom = Mathf.Max(sizeTextBottom, 0);

            sizeText.rectTransform.offsetMin = new Vector2(sizeTextLeft * Timeline.instance.PixelsPerBeat, -sizeTextTop * Timeline.instance.LayerHeight());
            sizeText.rectTransform.offsetMax = new Vector2(-sizeTextRight * Timeline.instance.PixelsPerBeat, sizeTextBottom * Timeline.instance.LayerHeight());

            double finalStartBeat = startBeat, finalEndBeat = endBeat;
            int finalStartTrack = startTrack, finalEndTrack = endTrack;
            if (finalEndBeat < finalStartBeat)
            {
                var temp = finalStartBeat;
                finalStartBeat = finalEndBeat;
                finalEndBeat = temp;
            }
            if (finalEndTrack < finalStartTrack)
            {
                var temp = finalStartTrack;
                finalStartTrack = finalEndTrack;
                finalEndTrack = temp;
            }
            Select(finalStartBeat, finalEndBeat, finalStartTrack, finalEndTrack);
        }

        /// <summary>
        /// Selects the entity markers within the specified range.
        /// </summary>
        /// <param name="startBeat">The starting beat of the selection range.</param>
        /// <param name="endBeat">The ending beat of the selection range.</param>
        /// <param name="startTrack">The starting track of the selection range.</param>
        /// <param name="endTrack">The ending track of the selection range.</param>
        private void Select(double startBeat, double endBeat, int startTrack, int endTrack)
        {
            // This doesn't take into account blocks the user cannot see, this is intentional.
            // minenice: not ideal though, eventually make this select riq entities instead of timeline objects
            foreach (var marker in TimelineBlockManager.Instance.EntityMarkers.Values)
            {
                if (marker.BoxSelectOverlapping(startBeat, endBeat, startTrack, endTrack))
                {
                    if (!marker.selected && marker.visibility == Timeline.EventVisibility.Visible)
                        Selections.instance.DragSelect(marker);
                }
                else
                {
                    if (marker.selected && !Input.GetKey(KeyCode.LeftShift))
                        Selections.instance.Deselect(marker);
                }
            }
        }
    }
}