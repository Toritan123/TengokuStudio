using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using HeavenStudio.Util;
using System.Linq;
using Jukebox;

namespace HeavenStudio.Games.Global
{
    public class BlockChecker : MonoBehaviour
    {
        // abstracted from normal riqentities
        // lets you treat stretchy blocks and static blocks the same way, making behavior more consistent
        private readonly struct BasicEvent
        {
            public readonly double beat;
            public readonly bool enable;
            public BasicEvent(double beat, bool enable)
            {
                this.beat = beat;
                this.enable = enable;
            }
        }
        private List<BasicEvent> allStretchyToggleInputEvents = new List<BasicEvent>();
        private List<BasicEvent> allStretchyAutoplayEvents = new List<BasicEvent>();

        private void Awake()
        {
            GameManager.instance.onBeatChanged += OnBeatChanged;
        }

        public void OnBeatChanged(double beat)
        {
            allStretchyToggleInputEvents.Clear();
            allStretchyAutoplayEvents.Clear();
            double toggleInputEndBeat = double.MinValue;
            double autoplayEndBeat = double.MinValue;
            foreach (RiqEntity e in GameManager.instance.Beatmap.Entities)
            {
                switch (e.datamodel)
                {
                    case "gameManager/toggle inputs":
                        if (e.beat >= toggleInputEndBeat) {
                            allStretchyToggleInputEvents.Add(new(e.beat, e["toggle"]));
                        }
                        break;
                    case "gameManager/toggle inputs stretchy":
                        toggleInputEndBeat = e.beat + e.length;
                        allStretchyToggleInputEvents.Add(new(e.beat, false));
                        allStretchyToggleInputEvents.Add(new(toggleInputEndBeat, true));
                        break;
                    case "gameManager/toggle autoplay":
                        if (e.beat >= autoplayEndBeat) {
                            allStretchyAutoplayEvents.Add(new(e.beat, e["toggle"]));
                        }
                        break;
                    case "gameManager/toggle autoplay stretchy":
                        autoplayEndBeat = e.beat + e.length;
                        allStretchyAutoplayEvents.Add(new(e.beat, true));
                        allStretchyAutoplayEvents.Add(new(e.beat + e.length, false));
                        break;
                    default: break;
                }
            }
        }

        private void Update()
        {
            double songPos = Conductor.instance.songPositionInBeatsAsDouble;
            if (allStretchyToggleInputEvents.Count > 0) {
                int lastEventIndex = allStretchyToggleInputEvents.FindLastIndex(e => e.beat < songPos);
                // if index is invalid, toggle inputs on. else get the last event's enable bool
                bool toggleInputs = lastEventIndex < 0 || allStretchyToggleInputEvents[lastEventIndex].enable;
                GameManager.instance.ToggleInputs(toggleInputs);
            }
            if (allStretchyAutoplayEvents.Count > 0) {
                int lastEventIndex = allStretchyAutoplayEvents.FindLastIndex(e => e.beat < songPos);
                bool autoplay = lastEventIndex >= 0 && allStretchyAutoplayEvents[lastEventIndex].enable;
                GameManager.instance.ToggleScheduledAutoplay(autoplay);
            }
        }
    }
}