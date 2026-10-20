using System.Collections;
using System.Collections.Generic;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using UnityEngine;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_Showtime
{
    public class Ball : MonoBehaviour
    {
        BezierCurve3D currentCurve;
        Util.EasingFunction.Ease currentEase = Util.EasingFunction.Ease.EaseOutCubic;

        public bool isMiss;

        public double startBeat;
        double currentStartBeat;
        double fallBeat;

        float currentTime;

        public Showtime game;

        private void Start()
        {
            game = Showtime.instance;
            
            currentStartBeat = startBeat;
            fallBeat = startBeat+1;
            Launch();
        }

        private void Launch()
        {
             BeatAction.New(game, new List<BeatAction.Action>() {
                
                new BeatAction.Action(startBeat, delegate{currentCurve = game.ballUpCurve; currentStartBeat = startBeat; currentTime = 1f;}),
                new BeatAction.Action(fallBeat, delegate{if(isMiss){currentCurve = game.ballDownCurve; currentStartBeat = fallBeat; currentEase = Util.EasingFunction.Ease.EaseInCubic; currentTime = .75f;} else {Destroy(this.gameObject);}})
             });
        }

        void Update()
        {
            if (currentCurve != null)
            {
                float normalizedBeat = game.conductor.GetPositionFromBeat(currentStartBeat, currentTime);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(currentEase);
                float posStart = func(0, 1, normalizedBeat);
                if (posStart < 1f)
                {
                    transform.position = currentCurve.GetPoint(posStart);
                }
                
            }
        }
        
    }

}