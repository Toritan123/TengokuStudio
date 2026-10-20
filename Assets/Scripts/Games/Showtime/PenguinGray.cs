using System;
using System.Collections;
using System.Collections.Generic;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using UnityEngine;
using NaughtyBezierCurves;


namespace HeavenStudio.Games.Scripts_Showtime
{
    using HeavenStudio.Util;
    using HeavenStudio.InputSystem;

    public class PenguinGray: MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] Animator anim;
        Animator MonkeyAnim;
        Animator ButtonAnim;
        Animator LauncherAnim;
        private Conductor conductor = Conductor.instance;
        
        [SerializeField] Showtime game;

        public double startBeat; //occurs 1 beat before sound cue
        public double hopBeat; //1st beat of cue
        public double landBeat; //2nd beat of cue
        public double leapBeat; //3rd beat of cue, this is where the input is
        public double checkBeat; //4th beat, checks for success/failure
        public double slideBeat; //5th beat, does a slide if parachute wasnt activated

        public bool successCheck = false;

        public bool chute = false;
        public bool react;

        bool canBeCleaned = false;

        BezierCurve3D currentCurve;
        double currentStartBeat;
        float currentHopTime;

        Util.EasingFunction.Ease currentEase;

        [SerializeField] GameObject penguinStart;


        void Awake()
        {
            
        }
        

        void Start()
        {
            game = Showtime.instance;
            MonkeyAnim = game.MonkeyAnim;
            ButtonAnim = game.ButtonAnim;
            LauncherAnim = game.LauncherAnim;
            
            hopBeat = startBeat + 1;
            landBeat = startBeat + 2;
            leapBeat = startBeat + 3;
            checkBeat = startBeat + 4;
            slideBeat = startBeat + 4.5;
            currentEase = Util.EasingFunction.Ease.EaseInQuad;
            game.ScheduleInput(startBeat, 3f, Showtime.InputAction_BasicPress, Hit, Miss, null);
            
            Jump();

        }

        public void Jump()
        {
            BeatAction.New(game, new List<BeatAction.Action>() {
                
                new BeatAction.Action(startBeat, delegate{currentCurve = game.entryCurve;currentStartBeat = startBeat;currentHopTime = 1f;anim.DoScaledAnimationAsync("Idle", 0.5f);}),
                new BeatAction.Action(hopBeat, delegate{currentCurve = game.hopCurve;currentStartBeat = hopBeat;currentHopTime = 0.75f;currentEase = Util.EasingFunction.Ease.Linear; game.blockOneAnim.DoScaledAnimationAsync("Squish", 0.5f);}),
                new BeatAction.Action(landBeat, delegate{anim.DoScaledAnimationAsync("Land", 0.5f); transform.position = game.leapStart.position;}),
                new BeatAction.Action(landBeat +0.5, delegate{ anim.DoScaledAnimationAsync("Prepare", 1f); }),
                new BeatAction.Action(leapBeat, delegate{currentCurve = game.leapCurve;currentStartBeat = leapBeat; currentHopTime = 1f; anim.DoScaledAnimationAsync("Leap", 1f);currentEase = Util.EasingFunction.Ease.Linear;}),
                new BeatAction.Action(checkBeat, delegate{transform.position = game.fallStart.position; currentCurve = game.fallCurve;currentStartBeat=checkBeat;currentHopTime=.5f;currentEase = Util.EasingFunction.Ease.Linear;InputResult(checkBeat);}),
                new BeatAction.Action(slideBeat, delegate{currentStartBeat = slideBeat;currentCurve = game.exitCurve; currentEase = Util.EasingFunction.Ease.Linear; currentHopTime = 1f; Slide(); })
                
                }
            );

        }

        void Update()
        {
            
            if (currentCurve != null)
            {
                float normalizedBeat = game.conductor.GetPositionFromBeat(currentStartBeat, currentHopTime);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(currentEase);
                float posStart = func(0, 1, normalizedBeat);
                if (posStart < 1f)
                {
                    transform.position = currentCurve.GetPoint(posStart);
                }
                else if (posStart >= 1f)
                {
                    if (currentCurve == game.hopCurve)
                        {
                            currentCurve = null;
                        }

                if (this.transform.position == game.destroyerPoint.position)
                {
                    Destroy(this.gameObject);
                }

                }

            }
        }

        void Slide()
        {   
            transform.position = game.slideStart.position;
            currentCurve = game.exitCurve;
                        if (successCheck)
                        {
                            anim.DoScaledAnimationAsync("SlideBall", 0.5f);
                        }
                        else
                        {
                            anim.DoScaledAnimationAsync("Slide", 0.5f);
                        }
        }

        void InputResult(double beat)
        {

            if (successCheck)
            {
                SoundByte.PlayOneShotGame("showtime/medium4", beat);
                anim.DoScaledAnimationAsync("Catch", 0.5f);
            //    currentCurve = game.chuteCurve;
            //    currentHopTime = 8f;
            //    currentEase = Util.EasingFunction.Ease.Linear;
            }
            //else
            //{
                //currentCurve = game.fallCurve;
                //currentHopTime = 0.5f;
                //currentEase = Util.EasingFunction.Ease.Linear;
            //}


        }
        

        public void Hit (PlayerActionEvent caller, float state)
        {
            double hitBeat = game.conductor.songPositionInBeatsAsDouble;
            if (state >= 1f || state <= -1f) {

                successCheck = false;
                MonkeyAnim.DoScaledAnimationAsync("Miss", 0.5f);

                SoundByte.PlayOneShotGame("showtime/hit", hitBeat, forcePlay: true);
                game.SpawnBall(hitBeat, true);

            }
            else
            {
                successCheck = true;
                MonkeyAnim.DoScaledAnimationAsync("Hit", 0.5f);
                SoundByte.PlayOneShotGame("showtime/hit", hitBeat, forcePlay: true);
                game.SpawnBall(hitBeat, false);
                
            }
        }

        private void Miss (PlayerActionEvent caller)
        {
            successCheck = false;
        }
        
    }

}