using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HeavenStudio.Common;
using HeavenStudio.Editor.Track;

using TMPro;

namespace HeavenStudio.Editor 
{
    public class PreviewDialog : Dialog
    {
        [SerializeField] RectTransform btnRectTransform;
        [SerializeField] Toggle PreviewTimingDisplayToggle;
        [SerializeField] Toggle PreviewSectionsToggle;
        [SerializeField] Toggle PreviewPerfectToggle;
        List<OverlaysManager.OverlayOption> lytElements = new List<OverlaysManager.OverlayOption>();

        private Timeline timeline;

        private void Awake()
        {
            timeline = Timeline.instance;
            lytElements = new List<OverlaysManager.OverlayOption>();
            foreach (var c in PersistentDataManager.gameSettings.timingDisplayComponents) { lytElements.Add(c); }
            foreach (var c in PersistentDataManager.gameSettings.skillStarComponents) { lytElements.Add(c); }
            foreach (var c in PersistentDataManager.gameSettings.sectionComponents) { lytElements.Add(c); }
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

            Timeline.instance.previewTimingDisplay = lytElements[0].enable;
            Timeline.instance.previewSections = lytElements[2].enable;

            PreviewTimingDisplayToggle.isOn = Timeline.instance.previewTimingDisplay;
            PreviewSectionsToggle.isOn = Timeline.instance.previewSections;
            PreviewPerfectToggle.isOn = Timeline.instance.previewPerfect;
        }

        public void ToggleTimingDisplay()
        {
            Timeline.instance.previewTimingDisplay = PreviewTimingDisplayToggle.isOn;
            lytElements[0].enable = PreviewTimingDisplayToggle.isOn;
        }

        public void ToggleSections()
        {
            Timeline.instance.previewSections = PreviewSectionsToggle.isOn;
            lytElements[2].enable = PreviewSectionsToggle.isOn;
        }

        public void TogglePerfect()
        {
            Timeline.instance.previewPerfect = PreviewPerfectToggle.isOn;
        }
    }
}