using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Editor.Track;

using System.Linq;

using TMPro;
using HeavenStudio.Common;

namespace HeavenStudio.Editor 
{
    public class TracksDialog : Dialog
    {
        [SerializeField] private TMP_Text trackText;
        [SerializeField] RectTransform btnRectTransform;
        [SerializeField] private Timeline timeline;
        [SerializeField] private EditorTheme editorTheme;

        public void SwitchTracksDialog()
        {
            if (dialog.activeSelf) {
                dialog.SetActive(false);
            } else {
                ResetAllDialogs();
                SetPosRelativeToButtonPos(btnRectTransform, new Vector2(106, 70));
                trackText.text = Localization.F("{0} tracks", timeline.LayerCount);
                dialog.SetActive(true);
            }
        }

        public void AddTrack()
        {
            if (timeline.LayerCount == 50) return;

            timeline.LayerCount++;

            editorTheme.InitializeLayer(timeline.LayerCount - 1);
            timeline.InitializeLayers();

            trackText.text = Localization.F("{0} tracks", timeline.LayerCount);
        }

        public void RemoveTrack()
        {
            if (timeline.LayerCount == 5) return;
            if (GameManager.instance.Beatmap.Entities.Any(e => (int)e["track"] == timeline.LayerCount - 1))
            {
                return;
            }

            timeline.LayerCount--;

            editorTheme.DeleteLayer();

            trackText.text = Localization.F("{0} tracks", timeline.LayerCount);
        }
    }
}