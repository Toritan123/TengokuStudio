using HeavenStudio.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AgbNightWalk
{
    public class AgbPlatformHandler : MonoBehaviour
    {
        private AgbNightWalk game;
        [Header("Properties")]
        [SerializeField] private AgbPlatform platformRef;
        [SerializeField] private AgbStarHandler starHandler;
        public float defaultYPos = -11.76f;
        public float heightAmount = 2;
        public float platformDistance = 3.80f;
        public float playerXPos = -6.78f;
        [SerializeField] private float starLength = 16;
        [SerializeField] private float starHeight = 0.0625f;
        [Range(1, 100)]
        public int platformCount = 20;
        private float lastHeight = 0;
        private float heightToRaiseTo = 0;
        private double raiseBeat = double.MinValue;
        [NonSerialized] public List<AgbPlatform> allPlatforms = new();
        private int lastHeightUnits;
        private int currentHeightUnits;
        private bool stopStars;

        private void Awake()
        {
            game = AgbNightWalk.instance;
        }

        public void SpawnPlatforms(double beat)
        {
            Debug.Log("game.countInBeat : " + game.countInBeat);
            double firstInputBeat = Math.Ceiling(beat);
            int lastUnits = game.FindHeightUnitsAtBeat(firstInputBeat - 1);
            int currentUnits = game.FindHeightUnitsAtBeat(firstInputBeat);
            RaiseHeight(firstInputBeat - 1, lastUnits, currentUnits);

            if (game.countInBeat != double.MinValue)
            {
                //modifies to only spawn the end platforms and before accomodating count-in

                int effectivePlatformCount = platformCount;
                if (game.endBeat != double.MaxValue) {
                    double endInputBeat = Math.Ceiling(game.endBeat);
                    double platformStartBeat = game.countInBeat + game.countInLength;
                    int totalBeats = (int)Math.Floor((endInputBeat + 2) - platformStartBeat);
                    effectivePlatformCount = totalBeats > -1 ? totalBeats : 0;
                }

                for (int i = 0; i < effectivePlatformCount; i++)
                {
                    AgbPlatform platform = Instantiate(platformRef, transform);
                    allPlatforms.Add(platform);
                    platform.handler = this;
                    platform.StartInput(game.countInBeat + i + game.countInLength - (effectivePlatformCount * 0.5), game.countInBeat + i + game.countInLength);
                    platform.gameObject.SetActive(true);
                }
            }
            else
            {
                for (int i = 0; i < platformCount; i++)
                {
                    AgbPlatform platform = Instantiate(platformRef, transform);
                    allPlatforms.Add(platform);
                    platform.handler = this;
                    platform.StartInput(beat, firstInputBeat + i - platformCount);
                    platform.gameObject.SetActive(true);
                }
                if (lastUnits != currentUnits)
                {
                    game.playYan.Jump(firstInputBeat - 1);
                }
            }
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (raiseBeat != double.MinValue)
                {
                    float normalizedBeat = Mathf.Clamp(cond.GetPositionFromBeat(raiseBeat, 1), 0, 1);
                    EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseOutQuint);

                    float newPosY = func(lastHeight, heightToRaiseTo, normalizedBeat);

                    transform.localPosition = new Vector3(0, -newPosY, 0);
                    starHandler.normalizedY = -func(starHeight * lastHeightUnits, starHeight * currentHeightUnits, normalizedBeat);
                }
                if (stopStars) return;
                float normalizedValue = cond.GetPositionFromBeat(0, starLength);

                starHandler.normalizedX = -normalizedValue;
            }
        }

        public void StopAll()
        {
            foreach (var platform in allPlatforms)
            {
                platform.Stop();
            }
            stopStars = true;
        }

        public void DevolveAll()
        {
            starHandler.Devolve();
        }

        public bool PlatformsStopped()
        {
            return allPlatforms[0].stopped;
        }

        public void DestroyPlatforms(double startBeat, double firstBeat, double lastBeat)
        {
            List<AgbPlatform> platformsToDestroy = allPlatforms.FindAll(x => x.endBeat >= firstBeat && x.endBeat <= lastBeat);
            platformsToDestroy.Sort((x, y) => x.endBeat.CompareTo(y.endBeat));
            List<BeatAction.Action> actions = new();
            for (int i = 0; i < platformsToDestroy.Count; i++)
            {
                AgbPlatform currentPlatformToDdestroy = platformsToDestroy[i];
                double fallBeat = startBeat + i;
                actions.Add(new BeatAction.Action(fallBeat, delegate
                {
                    currentPlatformToDdestroy.Disappear(fallBeat);
                }));
            }
            BeatAction.New(this, actions);
        }

        public void RaiseHeight(double beat, int lastUnits, int currentUnits)
        {
            raiseBeat = beat;
            lastHeight = lastUnits * heightAmount * transform.localScale.y;
            heightToRaiseTo = currentUnits * heightAmount * transform.localScale.y;
            currentHeightUnits = currentUnits;
            lastHeightUnits = lastUnits;
            Update();
        }
    }
}

