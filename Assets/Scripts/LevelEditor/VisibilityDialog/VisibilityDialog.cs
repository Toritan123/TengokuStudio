using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HeavenStudio.Common;
using HeavenStudio.Editor.Track;

using TMPro;

namespace HeavenStudio.Editor 
{
    public class VisibilityDialog : Dialog
    {
        [SerializeField] RectTransform btnRectTransform;
        [SerializeField] Sprite EyeOpenSprite;
        [SerializeField] Sprite EyeHalfSprite;
        [SerializeField] Sprite EyeClosedSprite;
        [SerializeField] Image minigameButtonSprite;
        [SerializeField] Image vfxButtonSprite;
        [SerializeField] Image otherButtonSprite;
        [SerializeField] Image mainButtonSprite;

        private Timeline timeline;

        private void Awake()
        {
            timeline = Timeline.instance;
        }

        public void SwitchPreviewDialog()
        {
            if (dialog.activeSelf) {
                dialog.SetActive(false);
            } else {
                ResetAllDialogs();
                SetPosRelativeToButtonPos(btnRectTransform);
                rectTransform.SetParent(btnRectTransform);
                rectTransform.localPosition = new Vector2(134, 112);
                rectTransform.SetParent(Editor.instance.MainCanvas.transform, true);
                dialog.SetActive(true);
            }
        }

        public void OnMinigameButtonClicked()
        {
            switch (timeline.minigameVisibility)
            {
                case Timeline.EventVisibility.Visible:
                    timeline.minigameVisibility = Timeline.EventVisibility.Transparent;
                    TimelineBlockManager.Instance.UpdateMarkerColors();
                    minigameButtonSprite.sprite = EyeHalfSprite;
                    break;
                case Timeline.EventVisibility.Transparent:
                    timeline.minigameVisibility = Timeline.EventVisibility.Hidden;
                    TimelineBlockManager.Instance.UpdateMarkerColors();
                    minigameButtonSprite.sprite = EyeClosedSprite;
                    break;
                case Timeline.EventVisibility.Hidden:
                    timeline.minigameVisibility = Timeline.EventVisibility.Visible;
                    TimelineBlockManager.Instance.UpdateMarkerColors();
                    minigameButtonSprite.sprite = EyeOpenSprite;
                    break;
            }

            SetMainButtonSprite();
        }

        public void OnVFXButtonClicked()
        {
            switch (timeline.vfxVisibility)
            {
                case Timeline.EventVisibility.Visible:
                    timeline.vfxVisibility = Timeline.EventVisibility.Transparent;
                    TimelineBlockManager.Instance.UpdateMarkerColors();
                    vfxButtonSprite.sprite = EyeHalfSprite;
                    break;
                case Timeline.EventVisibility.Transparent:
                    timeline.vfxVisibility = Timeline.EventVisibility.Hidden;
                    TimelineBlockManager.Instance.UpdateMarkerColors();
                    vfxButtonSprite.sprite = EyeClosedSprite;
                    break;
                case Timeline.EventVisibility.Hidden:
                    timeline.vfxVisibility = Timeline.EventVisibility.Visible;
                    TimelineBlockManager.Instance.UpdateMarkerColors();
                    vfxButtonSprite.sprite = EyeOpenSprite;
                    break;
            }

            SetMainButtonSprite();
        }
        public void OnOtherButtonClicked()
        {
            switch (timeline.otherVisibility)
            {
                case Timeline.EventVisibility.Visible:
                    timeline.otherVisibility = Timeline.EventVisibility.Transparent;
                    TimelineBlockManager.Instance.UpdateMarkerColors();
                    otherButtonSprite.sprite = EyeHalfSprite;
                    break;
                case Timeline.EventVisibility.Transparent:
                    timeline.otherVisibility = Timeline.EventVisibility.Hidden;
                    TimelineBlockManager.Instance.UpdateMarkerColors();
                    otherButtonSprite.sprite = EyeClosedSprite;
                    break;
                case Timeline.EventVisibility.Hidden:
                    timeline.otherVisibility = Timeline.EventVisibility.Visible;
                    TimelineBlockManager.Instance.UpdateMarkerColors();
                    otherButtonSprite.sprite = EyeOpenSprite;
                    break;
            }

            SetMainButtonSprite();
        }

        private void SetMainButtonSprite()
        {
            if (timeline.minigameVisibility == Timeline.EventVisibility.Visible &&
                timeline.vfxVisibility == Timeline.EventVisibility.Visible &&
                timeline.otherVisibility == Timeline.EventVisibility.Visible)
            {
                mainButtonSprite.sprite = EyeOpenSprite;
            }
            else if (timeline.minigameVisibility == Timeline.EventVisibility.Hidden &&
                     timeline.vfxVisibility == Timeline.EventVisibility.Hidden &&
                     timeline.otherVisibility == Timeline.EventVisibility.Hidden)
            {
                mainButtonSprite.sprite = EyeClosedSprite;
            }
            else
            {
                mainButtonSprite.sprite = EyeHalfSprite;
            }
        }
    }  
}