using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlSortLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("packingPests", "Packing Pests", "BAB89D", false, false, new List<GameAction>()
            {
                new GameAction("pattern", "Pattern")
                {
                    function = delegate { var e = eventCaller.currentEntity; PackingPests.instance.Pattern(e.beat); },
                    defaultLength = 4f,
                },
                new GameAction("pattern2", "Double Up!")
                {

                    function = delegate { var e = eventCaller.currentEntity; PackingPests.instance.Pattern2(e.beat, e["pitch"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("pitch", false, "Pitch Voiceline", "Changes the pitch of the \"double up!\" voice to sound better at lower tempos."),
                    }
                },
                new GameAction("patternArrange1", "Offbeat Pattern")
                {
                    function = delegate { var e = eventCaller.currentEntity; PackingPests.instance.PatternArrange1(e.beat);},
                    defaultLength = 4f,
                },
                new GameAction("patternArrange2", "Offbeat Candy Pattern")
                {
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity;
                        if (!e["warning"]) return;
                        PackingPests.instance.PrePatternDing(e.beat, true);
                    },
                    function = delegate 
                    { 
                        var e = eventCaller.currentEntity; PackingPests.instance.PatternArrange2(e.beat);
                    },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("warning", true, "Play Warning", "Toggle if the 'ding' and 'hmm' sound effects should play beforehand."),
                    },
                    preFunctionLength = 0.5f
                },
                new GameAction("candy", "Throw Candy (Standalone)")
                {
                    function = delegate { var e = eventCaller.currentEntity; PackingPests.instance.ThrowCandy(e.beat); },
                    defaultLength = 2f,
                },
                new GameAction("spider", "Throw Spider (Standalone)")
                {
                    function = delegate { var e = eventCaller.currentEntity; PackingPests.instance.ThrowSpider(e.beat); },
                    defaultLength = 2f,
                    advanced = true,
                },
                new GameAction("ding", "Offbeat Warning (Standalone)")
                {
                    function = delegate { var e = eventCaller.currentEntity; PackingPests.instance.Ding(e.beat, e["hmm"]); },
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("hmm", true, "Play Hum", "Toggle if the 'hmm' sound effect should play beforehand."),
                    },
                    advanced = true,
                },
                new GameAction("worker", "Move Workers")
                {
                    function = delegate { var e = eventCaller.currentEntity; PackingPests.instance.MoveWorker(e.beat, e.length, e["player"], e["all"], e["allAlt"], e["inout"], e["ease"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("player", false, "Player"),
                        new Param("all", true, "All"),
                        new Param("allAlt", false, "All (Except Player)"),
                        new Param("inout", PackingPests.WorkerType.In, "Side", "Set The side the worker moves to."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    }
                },
                new GameAction("curtainNew", "Move Curtains")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        PackingPests.instance.MoveCurtain(e.beat, e.length, e["inout"], e["ease"]);
                    },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("inout", PackingPests.CurtainType.Enter, "Side", "Set the side the Curtain Slides to."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action")
                    }
                },
                new GameAction("curtains", "Move Curtains")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        PackingPests.instance.SlideCurtain(e.beat);
                    },
                    hidden = true,
                },
            },
            new List<string>() { "rvl", "keep" },
            "rvlsorting", "en",
            new List<string>() { "en", "jp" },
            chronologicalSortKey: 17
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_PackingPests;
    public class PackingPests : Minigame
    {
        public static PackingPests instance;
        const int IA_AltPress = IAMAXCAT;

        protected static bool IA_PadAltPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltPress(out double dt)
        {
            return PlayerInput.GetSqueezeDown(out dt);
        }
        protected static bool IA_TouchAltPress(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && instance.IsExpectingInputNow(InputAction_Alt);
        }
        public static PlayerInput.InputAction InputAction_Alt =
            new("RvlSortAlt", new int[] { IA_AltPress, IA_AltPress, IA_AltPress },
            IA_PadAltPress, IA_TouchAltPress, IA_BatonAltPress);


        [Header("GameObjects")]
        [SerializeField] public GameObject Candy;
        [SerializeField] public GameObject Spider;
        [SerializeField] public SpriteRenderer boxfront;

        [Header("Background Objects")]

        [Header("Curves")]
        [SerializeField] SuperCurveObject.Path[] objectPaths;

        [Header("Animators")]
        public Animator handAnim;
        public Animator lowerHandAnim;
        public Animator upperHandAnim;
        public Animator signAnim;
        public Animator spiderCrawlAnim;
        public Animator spiderAnim;
        public Animator curtainAnim;

        public enum WorkerType
        {
            In,
            Out,
        }
        bool isMoving;
        bool isMovingAlt;
        bool isSliding;
        double movingStartBeat;
        double movingLength;
        string moveAnim;
        string CurtainAnim;
        Util.EasingFunction.Ease lastEase;

        public Animator HandAnimPlayer;
        public Animator HandAnim1;
        public Animator HandAnim2;
        public Animator HandAnim3;
        public Animator HandAnim4;
        public Animator HandAnim5;
        public Animator HandAnim6;
        public Animator HandAnim7;
        public Animator HandAnim8;

        public enum CurtainType
        {
            Enter,
            Exit
        }
        new void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            foreach (SuperCurveObject.Path path in objectPaths)
            {
                if (path.preview)
                {
                    Candy.GetComponent<ThrowObject>().DrawEditorGizmo(path);
                }
            }
        }
        public SuperCurveObject.Path GetPath(string name)
        {
            foreach (SuperCurveObject.Path path in objectPaths)
            {
                if (path.name == name)
                {
                    return path;
                }
            }
            return default(SuperCurveObject.Path);
        }
        public void Awake()
        {
            instance = this;
        }
        private void Update()
        {
            double currentBeat = conductor.unswungSongPositionInBeatsAsDouble;
            if(PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_Alt))
            {
                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_SWING");
                upperHandAnim.DoScaledAnimationAsync("HitOut", 0.5f);
            }
            else if (PlayerInput.GetIsAction(InputAction_Alt) && !IsExpectingInputNow(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_Alt))
            {
                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_SWING_CATCH_A");
                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_SWING_CATCH_B");
                upperHandAnim.DoScaledAnimationAsync("CatchOut", 0.5f);
                lowerHandAnim.DoScaledAnimationAsync("CatchOut", 0.5f);
            }

            if (!Conductor.instance.NotStopped()) 
            curtainAnim.DoScaledAnimationAsync("idle", 0.5f); HandAnimPlayer.DoScaledAnimationAsync("idle", 0.5f); 
            HandAnim1.DoScaledAnimationAsync("idle", 0.5f); HandAnim2.DoScaledAnimationAsync("idle", 0.5f); 
            HandAnim3.DoScaledAnimationAsync("idle", 0.5f); HandAnim4.DoScaledAnimationAsync("idle", 0.5f); 
            HandAnim5.DoScaledAnimationAsync("idle", 0.5f); HandAnim6.DoScaledAnimationAsync("idle", 0.5f); 
            HandAnim7.DoScaledAnimationAsync("idle", 0.5f); HandAnim8.DoScaledAnimationAsync("idle", 0.5f);

            if (isMoving) {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(movingStartBeat, movingLength);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(lastEase);
                float newPos = func(0f, 1f, normalizedBeat);
                HandAnimPlayer.DoNormalizedAnimation(moveAnim, newPos);
                
                //if (normalizedBeat >= 1f) isMoving = false; isMovingAlt = false;
            }
            if (isMovingAlt) {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(movingStartBeat, movingLength);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(lastEase);
                float newPos = func(0f, 1f, normalizedBeat);
                HandAnim1.DoNormalizedAnimation(moveAnim, newPos);
                HandAnim2.DoNormalizedAnimation(moveAnim, newPos);
                HandAnim3.DoNormalizedAnimation(moveAnim, newPos);
                HandAnim4.DoNormalizedAnimation(moveAnim, newPos);
                HandAnim5.DoNormalizedAnimation(moveAnim, newPos);
                HandAnim6.DoNormalizedAnimation(moveAnim, newPos);
                HandAnim7.DoNormalizedAnimation(moveAnim, newPos);
                HandAnim8.DoNormalizedAnimation(moveAnim, newPos);
                
                //if (normalizedBeat >= 1f) isMoving = false; isMovingAlt = false;
            }
            if (isSliding) {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(movingStartBeat, movingLength);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(lastEase);
                float newPos = func(0f, 1f, normalizedBeat);
                curtainAnim.DoNormalizedAnimation(CurtainAnim, newPos);
                if (normalizedBeat >= 1f) isSliding = false;
            }
        }
        public void Pattern(double beat)
        {
            ThrowCandy(beat + 0);
            ThrowSpider(beat + 2);
        }

        public void PatternArrange1(double beat)
        {
            ThrowCandy(beat + 0);
            ThrowSpider(beat + 1.5);
        }
        public void PatternArrange2(double beat)
        {
            ThrowCandy(beat + 0.5);
            ThrowSpider(beat + 2);
        }

        public void Pattern2(double beat, bool isPitched)
        {
            signAnim.DoScaledAnimationAsync("Message00", 0.5f);
            float pitch = isPitched ? (Conductor.instance.GetBpmAtBeat(beat) / 136) * Conductor.instance.TimelinePitch : 1;
            double offset = isPitched ? (0.013 / (Conductor.instance.GetBpmAtBeat(beat) / 136)) : 0.013;
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("packingPests/SE_SHIWAKE_EN_VOICE_READY1", beat, pitch),
                new MultiSound.Sound("packingPests/SE_SHIWAKE_EN_VOICE_READY2", beat + 0.25f, pitch, offset: offset),
                new MultiSound.Sound("packingPests/SE_SHIWAKE_EN_VOICE_READY3", beat + 0.5f, pitch),
            }, forcePlay: true);

            ThrowCandy(beat + 0);
            ThrowSpider(beat + 1.5);
            ThrowSpider(beat + 2.5);
        }

        public void PrePatternDing(double beat, bool hmm)
        {
            Ding(beat - 0.5, true);
        }

        public void ThrowCandy(double beat)
        {
            handAnim.DoScaledAnimationAsync("Beat", 0.5f);
            var candy = Instantiate(Candy, transform).GetComponent<ThrowObject>();
            candy.Init(beat);
            candy.spider = false;

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("packingPests/SE_SHIWAKE_EN_BALL_OUT", beat),
            }, forcePlay: true);  
        }
        public void ThrowSpider(double beat)
        {
            var spider = Instantiate(Spider, transform).GetComponent<ThrowObject>();
            spider.Init2(beat);
            spider.spider = true;

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("packingPests/SE_SHIWAKE_EN_INSECT_OUT", beat),
            }, forcePlay: true);
        }
        public void MoveWorker(double beat, double length, bool player, bool all, bool allAlt, int goToSide, int ease)
        {
            if(player)
            {
                isMoving = true;
                isMovingAlt = false;
            }

            if(all)
            {
                isMoving = true;
                isMovingAlt = true;
            }

            if(allAlt)
            {
                isMoving = false;
                isMovingAlt = true;
            }
            movingStartBeat = beat;
            movingLength = length;
            moveAnim = (goToSide == 0 ? "Enter" : "Exit");
            lastEase = (Util.EasingFunction.Ease)ease;
        }
        public void Ding(double beat, bool hmm)
        {   
            List<MultiSound.Sound> sfx = new();
            sfx.Add(new MultiSound.Sound("packingPests/SE_SHIWAKE_EN_VOICE_REST_A", beat));
            if (hmm) {sfx.Add(new MultiSound.Sound("packingPests/SE_SHIWAKE_EN_VOICE_REST_B", beat+0.5));}
            MultiSound.Play(sfx, forcePlay : true);
            if (signAnim != null) {signAnim.DoScaledAnimationAsync("Message01", 0.5f);}
        }
        public void MoveCurtain(double beat, double length, int inout, int ease)
        {
            isSliding = true;
            movingStartBeat = beat;
            movingLength = length;
            CurtainAnim = (inout == 0 ? "Enter" : "Exit");
            lastEase = (Util.EasingFunction.Ease)ease;
        }

        public void SlideCurtain(double beat)
        {
            
        }
    }
}
