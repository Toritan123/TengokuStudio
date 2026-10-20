using UnityEngine;
using UnityEngine.UI;

using Jukebox;
using HeavenStudio.Util;
using TMPro;
using System.Linq;
using System.Collections.Generic;

namespace HeavenStudio.Editor.Track
{
    public class TimelineGameSwitch : TimelineEventObj
    {
        [SerializeField] RectTransform contentPanel;
        [SerializeField] Image fadeEffect;
        [SerializeField] Image leftLine;
        [SerializeField] GameObject arrow;

        new public void SetEntity(RiqEntity entity)
        {
            this.entity = entity;
        }

        new public void SetMarkerInfo()
        {
            base.SetMarkerInfo();
            // SetColor(0);
        }

        new public void UpdateMarker()
        {
            base.UpdateMarker();
            entity["track"] = 0;
            entity.length = 0.5f;
            // SetColor(0);
        }

        new public int GetTrack()
        {
            return 0;
        }

        new public void LateUpdate()
        {
            entity["track"] = 0;
            entity.length = 0.5f;
            rectTransform.anchoredPosition = new Vector2((float)entity.beat * Timeline.instance.PixelsPerBeat, -contentPanel.anchoredPosition.y);
            SetWidthHeight();
        }

        new public void SetWidthHeight()
        {
            rectTransform.sizeDelta = new Vector2(entity.length * Timeline.instance.PixelsPerBeat, 0);
            if (Icon.gameObject.activeSelf)
            {
                Icon.rectTransform.sizeDelta = new Vector2(24, 24);
                eventLabel.rectTransform.offsetMin = new Vector2(Icon.rectTransform.anchoredPosition.x + Icon.rectTransform.sizeDelta.x + 4, eventLabel.rectTransform.offsetMin.y);
            }
            else
            {
                eventLabel.rectTransform.offsetMin = new Vector2(0, eventLabel.rectTransform.offsetMin.y);
            }
        }

        public override void SetColor()
        {
            Color c;
            var eventName = entity.datamodel;
            string[] split = eventName.Split('/');
            if (split[0] == "gameManager")
            {
                if (split[1] == "switchGame")
                {
                    c = StringUtils.Hex2RGB(EventCaller.instance.GetMinigame(split[2]).color);
                    foreach (var r in recolourables)
                    {
                        r.color = c;
                    }

                    c.a = 0.05f;
                    fadeEffect.color = c;

                    c.a = 0.15f;
                    leftLine.color = c;

                    arrow.SetActive(true);
                }
                else if (split[1] == "end")
                {
                    c = Color.white;
                    foreach (var r in recolourables)
                    {
                        r.color = c;
                    }
                    leftLine.color = c;

                    c.a = 0.25f;
                    fadeEffect.color = c;

                    arrow.SetActive(false);
                }
                else if (split[1] == "startSong")
                {
                    c = Color.white;
                    foreach (var r in recolourables)
                    {
                        r.color = c;
                    }
                    c.a = 0.5f;
                    leftLine.color = c;

                    c.a = 0.025f;
                    fadeEffect.color = c;

                    arrow.SetActive(false);
                }
            }
        }

        public override bool BoxSelectOverlapping(double startBeat, double endBeat, int startTrack, int endTrack)
        {
            return entity.beat.IsBetween(startBeat, endBeat) || (entity.beat + 0.25).IsBetween(startBeat, endBeat);
        }
    }
}