using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Jukebox;
using TMPro;
using HeavenStudio.StudioDance;
using UnityEngine.UIElements;
using HeavenStudio.Editor.Commands;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class PcoMamboLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("warioDeMambo", "Wario de Mambo", "005b64", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {var e = eventCaller.currentEntity; WarioDeMambo.instance.ToggleBop(e.beat, e.length, e["auto"], e["mambo"], e["dancers"], e["lights"]);},
                    resizable = true,
                    priority = 4,
                    parameters = new List<Param>()
                    {
                        new Param("auto", true, "Auto Bop", "Toggle if the bop should happen automatically on every beat during the interval.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => !(bool)x, new string[] { "mambo", "dancers", "lights" })
                        }),
                        new Param("mambo", true, "Wario Bop", "Toggle if Wario should bop."),
                        new Param("dancers", true, "Dancers Bop", "Toggle if the dancers should bop."),
                        new Param("lights", true, "Lights Bop", "Toggle if the lights should bop.")
                    }
                },

                new GameAction("beat intervals", "Start Interval")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity;
                    WarioDeMambo.PreInterval(e.beat, e.length, 
                    e["auto"], 
                    e["memorize"],
                    e["numbers"],
                    e["text"],
                    e["left"],
                    e["resetColor"]
                    ); }, 

                    defaultLength = 8f,
                    preFunctionLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("auto", true, "Auto Pass Turn", "Toggle if the turn should be passed automatically at the end of the start interval.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "numbers", "resetColor" })
                        }),
                        new Param("memorize", true, "Memorize", "Toggle if the 'Memorize!' voice clip plays." ),
                        new Param("numbers", true, "Count-In", "Toggle if the count-in plays."),
                        new Param ("text", true, "Display Text", "Toggle if the text displays."),
                        new Param ("resetColor", true, "Reset Color", "Toggle if the color should be reset to blue after the reaction."),
                        new Param ("left", false, "Start on Left?", "Toggle if the first turn goes to the left. Pointing direction automatically switches between left and right.")
                    }
                },

                new GameAction("turn", "Turn")
                {
                    //function = delegate {var e = eventCaller.currentEntity; WarioDeMambo.instance.TurnInputController(e.beat, e["whichDirection"]);},
                    defaultLength = 1f,
                    //parameters = new List<Param>()
                    //{
                        //new Param("whichDirection", WarioDeMambo.WhichDirection.Automatic, "Direction", "The direction the dancers point in."),
                    //}
                },

                new GameAction("jump", "Jump")
                {
                    //function = delegate {var e = eventCaller.currentEntity; WarioDeMambo.instance.JumpInputController(e.beat);},
                    defaultLength = 1f,
                },

                new GameAction("pass turn", "Pass Turn")
                {
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity; WarioDeMambo.PrePassTurn(e.beat, e.length, e["numbers"], e["text"]);
                    },
                    resizable = true,
                    defaultLength = 8f,
                    preFunctionLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("numbers", true, "Count-In", "Toggle if the count-in plays."),
                        new Param("text", true, "Display Text", "Toggle if the text displays.")
                    },
                    advanced = true,
                },

                new GameAction("showText", "Text Command")
                {
                    function = delegate {var e = eventCaller.currentEntity; WarioDeMambo.instance.SetText(e.beat, e.length, e["text"]);},
                    resizable = true,
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("text", "Turn it up!", "Command", "Sets the text the command gives.")
                    }
                },

                new GameAction("reaction", "Success/Failure Reaction")
                {
                    function = delegate {var e = eventCaller.currentEntity; WarioDeMambo.instance.IntervalReaction(e.beat, e.length, e["resetColor"]);},
                    resizable = true,
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param ("resetColor", true, "Reset Color", "Toggle if the color should be reset to blue after the reaction.")
                    },
                    advanced = true,
                },

                new GameAction("lightsStage", "Change Lights")
                {
                    function = delegate {var e = eventCaller.currentEntity; WarioDeMambo.instance.ChangeLights(e.beat, (WarioDeMambo.LightsStage)e["stage"]);},
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("stage", WarioDeMambo.LightsStage.Stage1, "Light Stage", "Sets the light stage.")
                    },
                },

    
                new GameAction("introVoice", "Ladies and Gentlemen...")
                {
                    function = delegate {var e = eventCaller.currentEntity; WarioDeMambo.instance.IntroVoice(e.beat);},
                    defaultLength = 16f,
                },

                new GameAction("dance", "Dance")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        WarioDeMambo.instance.Dance(e.beat, e.length, (WarioDeMambo.DanceType)e["dance"]);
                    },
                    resizable = true,
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("dance", WarioDeMambo.DanceType.Stationary, "Dance Type", "Sets the dance type.")
                    }
                },


                new GameAction("changeColors", "Force Change Colors")
                {    
                    function = delegate {var e = eventCaller.currentEntity; WarioDeMambo.instance.SetColors(e["red"], e["dim"]);},
                    parameters = new List<Param>()
                    {
                        new Param("red", true, "Red", "Toggle if the color should be red or blue."),
                        new Param("dim", true, "Dim Lights", "Toggle if the color should be dimmed.")
                    },
                    defaultLength = 0.5f,
                    advanced = true
                },

                

                new GameAction("defaultText", "Set Default Text (WIP)")
                {
                    defaultLength = 0.5f,
                    hidden = true,
                },
            },
            new List<string>() {"miw", "repeat"},
            "pcomambo", "en", // this really should be miwmambo, but the games implemented already and im too lazy...
            new List<string>() { "en", "jp" },
            chronologicalSortKey: 1000,
            font: "WarioWareIncMerge SDF"
            );
            
        }
    }
}

namespace HeavenStudio.Games
{
    public class WarioDeMambo : Minigame
    {
        [Header("Variables")]
        float misses;
        static bool shouldBeLeft = false; // i unno any better than you do
        bool warioLeft = false;
        bool dancerLeft = false;
        bool dancerArmCentered = true;
        bool armCentered = true;
        bool canBop = true;
        bool autoBop = true;
        bool isDancing = false;
        bool gameDim = true;
        bool hasFlicked = false;
        UpdateEasing danceEase;
        UpdateEasing spotLEase;
        UpdateEasing spotREase;
        string danceAnim;
        BopState bopState = BopState.Normal;
        BopState dancerBopState = BopState.Normal;
        LightsStage lightsStage = LightsStage.Stage1;
        SpotsPos spotsPos = SpotsPos.Wario;

        Vector2 spotLlastPos;
        Vector2 spotRlastPos;
        Vector2 spotLtargetPos;
        Vector2 spotRtargetPos;

        [Header("Objects")]
        public TMP_Text commandText;
        public GameObject endPose;
        public SpriteRenderer spotlightLSprite;
        public SpriteRenderer spotlightRSprite;
        public Transform spotlightLTrans;
        public Transform spotlightRTrans;

        public Transform DancerLSpotPos;
        public Transform DancerRSpotPos;
        public Transform WarioSpotPos;

        [Header("Materials")]
        public Material mainMat;
        public Material lightMat;
        public Material floorLightMat;

        [Header("Colors")]
        [SerializeField] Color blueAddColor;
        [SerializeField] Color redAddColor;

        [Header("Animators")]
        [SerializeField] Animator textAnimator;

        [SerializeField] Animator dancerLeftAnim;
        [SerializeField] Animator dancerLeftArmAnim;
        [SerializeField] Animator dancerLeftHeadAnim;
        [SerializeField] Animator dancerLeftJumpAnim;

        [SerializeField] Animator dancerRightAnim;
        [SerializeField] Animator dancerRightArmAnim;
        [SerializeField] Animator dancerRightHeadAnim;
        [SerializeField] Animator dancerRightJumpAnim;

        [SerializeField] Animator warioBodyAnim;
        [SerializeField] Animator warioArmAnim;
        [SerializeField] Animator warioFaceAnim;
        [SerializeField] Animator warioJumpAnim;

        [SerializeField] Animator topLightAnim;
        [SerializeField] Animator leftLightAnim;
        [SerializeField] Animator rightLightAnim;

        private static CallAndResponseHandler crHandlerInstance;


        private struct PassedTurn
        {
            public double beat;
            public float length;
        }
       
        private struct QueuedInterval
        {
            public double beat;
            public float interval;
            public bool autoPassTurn;
            public bool count;
            public bool text;
        }

        public struct UpdateEasing
        {
            public double beat;
            public float length;
            public Util.EasingFunction.Ease ease;
        }

        private enum BopState
        {
            Normal,
            Think,
            Ready,
            Happy,
            Fail,
        }

        public enum DanceType
        {
            Stationary,
            Pose1,
            Pose2,
            End
        }

        public enum LightsStage
        {
            Off,
            Stage1,
            Stage2,
            Stage3
        }

        public enum SpotsPos
        {
            Dancers,
            Wario,
            Random
        }

        private static List<PassedTurn> passedTurns = new List<PassedTurn>();
        private static List<QueuedInterval> queuedIntervals = new List<QueuedInterval>();

        public Color callColor = new Color(0.37254901960784315f, 0.9686274509803922f, 0.9725490196078431f);

        //public enum WhichDirection
        //{
        //    Automatic,
        //    Right,
        //    Left
        //}

        public static WarioDeMambo instance;

        const int IAJumpCat = IAMAXCAT;
        const int IALeft = IAMAXCAT + 1;
        const int IARight = IAMAXCAT + 2;

        protected static bool IA_PadLeft(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt);
        }
        protected static bool IA_BatonLeft(out double dt)
        {
            return PlayerInput.GetBatonDown(InputController.ActionsBaton.West, out dt);
        }
        protected static bool IA_TouchLeft(out double dt)
        {
            return PlayerInput.GetSlide(out dt);
        }

        protected static bool IA_PadRight(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.Right, out dt);
        }
        protected static bool IA_BatonRight(out double dt)
        {
            return PlayerInput.GetBatonDown(InputController.ActionsBaton.East, out dt);
        }
        protected static bool IA_TouchRight(out double dt)
        {
            return PlayerInput.GetSlide(out dt);
        }

        protected static bool IA_PadJump(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.East, out dt);
        }
        protected static bool IA_BatonJump(out double dt)
        {
            return PlayerInput.GetSqueezeDown(out dt);
        }
        
        protected static bool IA_TouchJump(out double dt)
        {
            return PlayerInput.GetFlick (out dt);
        }

        public static PlayerInput.InputAction InputAction_Jump =
            new("PcoMamboJump", new int[] { IAJumpCat, IAJumpCat, IAJumpCat },
            IA_PadJump, IA_TouchJump, IA_BatonJump);
        public static PlayerInput.InputAction InputAction_L =
            new("PcoMamboLeft", new int[] { IALeft, IALeft, IALeft },
            IA_PadLeft, IA_TouchLeft, IA_BatonLeft);
        public static PlayerInput.InputAction InputAction_R =
            new("PcoMamboRight", new int[] { IARight, IARight, IARight },
            IA_PadRight, IA_TouchRight, IA_BatonRight);
        
        

        

        void Awake()
        {
            instance = this;
            commandText.text = "";
        }

        public override void OnPlay(double beat)
        {
            crHandlerInstance = null;
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
            queuedIntervals.Clear();
            bopState = BopState.Normal;
            dancerBopState = BopState.Normal;

            canBop = true;
            endPose.SetActive(false);
            commandText.text = "";

            spotlightLTrans.position = WarioSpotPos.position;
            spotlightRTrans.position = WarioSpotPos.position;
            spotsPos = SpotsPos.Wario;

            SetColors(false, true);
            SetupBopRegion("warioDeMambo", "bop", "auto");
        }

        private void OnDestroy()
        {
            if (!Conductor.instance.isPlaying)
            {
                crHandlerInstance = null;
            }
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }



        void Update()
        {
            var cond = Conductor.instance;
            bool hasInputThisFrame = false;

            double touchDelay = cond.SecsToBeats(0.075, cond.songBpm);
            if (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch) touchDelay = 0;

            if (cond.isPlaying && !cond.isPaused)
            {
                if (!hasInputThisFrame && PlayerInput.GetIsAction(InputAction_Jump) && !IsExpectingInputNow(InputAction_Jump))
                {
                    hasFlicked = true;
                    if (warioJumpAnim.IsPlayingAnimationNames("WJump")) return;
                    ScoreMiss();
                    misses++;
                    warioJumpAnim.DoScaledAnimationAsync("WJump", 0.5f);
                    SoundByte.PlayOneShotGame("warioDeMambo/jump");
                    SoundByte.PlayOneShot("miss");
                    hasInputThisFrame = true;
                }
                if (!hasInputThisFrame && PlayerInput.GetIsAction(InputAction_R) && !IsExpectingInputNow(InputAction_R) && !IsExpectingInputNow(InputAction_L) && !IsExpectingInputNow(InputAction_Jump))
                {
                    if (warioLeft || armCentered)
                    {
                        hasFlicked = false;
                        hasInputThisFrame = true;
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(cond.songPositionInBeatsAsDouble + touchDelay, delegate
                            {
                                if (hasFlicked) return;
                                ScoreMiss();
                                misses++;
                                warioLeft = false;

                                if (armCentered)
                                {
                                    armCentered = false;
                                    warioArmAnim.DoScaledAnimationAsync("WHandCtoR", 0.5f);
                                    warioFaceAnim.DoScaledAnimationAsync("WFaceCtoR", 0.5f);
                                }
                                else
                                {
                                    warioArmAnim.DoScaledAnimationAsync("WHandLtoR", 0.5f);
                                    warioFaceAnim.DoScaledAnimationAsync("WFaceLtoR", 0.5f);
                                }
                                SoundByte.PlayOneShotGame("warioDeMambo/right");
                                SoundByte.PlayOneShot("miss");
                            }),
                        });
                    }
                }
                if (!hasInputThisFrame && PlayerInput.GetIsAction(InputAction_L) && !IsExpectingInputNow(InputAction_L) && !IsExpectingInputNow(InputAction_R) && !IsExpectingInputNow(InputAction_Jump))
                {
                    if (!warioLeft || armCentered)
                    {
                        hasFlicked = false;
                        hasInputThisFrame = true;
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(cond.songPositionInBeatsAsDouble + touchDelay, delegate
                            {
                                if (hasFlicked) return;
                                ScoreMiss();
                                misses++;
                                warioLeft = true;

                                if (armCentered)
                                {
                                    armCentered = false;
                                    warioArmAnim.DoScaledAnimationAsync("WHandCtoL", 0.5f);
                                    warioFaceAnim.DoScaledAnimationAsync("WFaceCtoL", 0.5f);
                                }
                                else
                                {
                                    warioArmAnim.DoScaledAnimationAsync("WHandRtoL", 0.5f);
                                    warioFaceAnim.DoScaledAnimationAsync("WFaceRtoL", 0.5f);
                                }
                                SoundByte.PlayOneShotGame("warioDeMambo/left");
                                SoundByte.PlayOneShot("miss");
                            }),
                        });
                    }
                }
            
            
                if (passedTurns.Count > 0)
                {
                    foreach (var turn in passedTurns)
                    {
                        PassTurnStandalone(turn.beat, turn.length);
                    }
                    passedTurns.Clear();
                }

                if (danceEase.length != 0)
                {
                    float normalizedBeat = cond.GetPositionFromBeat(danceEase.beat, danceEase.length);
                    Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(danceEase.ease);
                    float newPos = func(0f, 1f, normalizedBeat);
                    warioJumpAnim.DoNormalizedAnimation(danceAnim, newPos);
                    if (normalizedBeat >= 1) danceEase.length = 0;
                }

                if (spotLEase.length != 0)
                {
                    float normalizedBeat = cond.GetPositionFromBeat(spotLEase.beat, spotLEase.length);
                    Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(spotLEase.ease);
                    float newPos = func(0f, 1f, normalizedBeat);
                    spotlightLTrans.localPosition = Vector3.LerpUnclamped(spotLlastPos, spotLtargetPos, newPos);
                    if (normalizedBeat >= 1)
                    {
                        spotLEase.length = 0;
                        if (spotsPos == SpotsPos.Random) 
                        {
                            MoveSpotlights(cond.songPositionInBeatsAsDouble, UnityEngine.Random.Range(0.35f, 0.75f), false);
                        }
                    } 
                }
                if (spotREase.length != 0)
                {
                    float normalizedBeat = cond.GetPositionFromBeat(spotREase.beat, spotREase.length);
                    Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(spotREase.ease);
                    float newPos = func(0f, 1f, normalizedBeat);
                    spotlightRTrans.localPosition = Vector3.LerpUnclamped(spotRlastPos, spotRtargetPos, newPos);
                    if (normalizedBeat >= 1)
                    {
                        spotREase.length = 0;
                    } 
                }
            }
        }
        public override void OnGameSwitch(double beat)
        {
            OnPlay(beat);
            RiqEntity intervalEntity = gameManager.Beatmap.Entities.FindLast(e => e.beat - 4 < beat && e.beat + e.length >= beat && e.datamodel == "warioDeMambo/beat intervals");
            if (intervalEntity != null)
            {
                RiqEntity e = intervalEntity;
                PreInterval(e.beat, e.length, e["auto"], false, e["numbers"], e["text"], e["left"], e["resetColor"]);
            }
        }

        public override void OnBeatPulse(double beat)
        {
            if (!autoBop) return;

            if (BeatIsInBopRegion(beat))
            {
                Bop();
            }
        }

        public void ToggleBop(double beat, float length, bool auto, bool mambo, bool dancers, bool lights)
        {
            autoBop = auto;
            if (autoBop)
            {
                Bop();
                return;
            }

            for (int i = 0; i < length; i++)
            {
                double bopBeat = beat + i;
                BeatAction.New(instance, new() {new BeatAction.Action(bopBeat, delegate { Bop(mambo, dancers, lights);})});
            }
        }

        public void Bop(bool mambo = true, bool dancers = true, bool lights = true)
        {
            if (canBop)
            {
                if (mambo)
                {
                    switch (bopState)
                    {
                        case BopState.Normal:
                            warioBodyAnim.DoScaledAnimationAsync("WahBopNorm", 0.5f);
                            break;
                        case BopState.Think:
                            warioBodyAnim.DoScaledAnimationAsync("WahThink", 0.5f);
                            break;
                        case BopState.Ready:
                            warioBodyAnim.DoScaledAnimationAsync("WahBopReady", 0.5f);
                            break;
                        case BopState.Happy:
                            warioBodyAnim.DoScaledAnimationAsync("WahBopHap", 0.5f);
                            break;
                        case BopState.Fail:
                            warioBodyAnim.DoScaledAnimationAsync("WahBad", 0.5f);
                            break;
                    }
                }
                
                if (dancers)
                {
                    switch (dancerBopState)
                    {
                        case BopState.Normal:
                            dancerLeftAnim.DoScaledAnimationAsync("DancerDance", 0.5f);
                            dancerRightAnim.DoScaledAnimationAsync("DancerDance", 0.5f);
                            break;
                        case BopState.Think:
                            dancerLeftAnim.DoScaledAnimationAsync("ReadyDance", 0.5f);
                            dancerRightAnim.DoScaledAnimationAsync("ReadyDance", 0.5f);
                            break;
                        case BopState.Ready:
                            dancerLeftAnim.DoScaledAnimationAsync("DPreDancePose", 0.5f);
                            dancerRightAnim.DoScaledAnimationAsync("DPreDancePose", 0.5f);
                            break;
                        case BopState.Happy:
                            dancerLeftAnim.DoScaledAnimationAsync("DancerDance", 0.5f);
                            dancerRightAnim.DoScaledAnimationAsync("DancerDance", 0.5f);
                            break;
                        case BopState.Fail:
                            dancerLeftAnim.DoScaledAnimationAsync("DancerBad", 0.5f);
                            dancerRightAnim.DoScaledAnimationAsync("DancerBad", 0.5f);
                            break;
                    }
                }
            }
            
            if (lights)
            {
                switch (lightsStage)
                {
                    case LightsStage.Off:
                        topLightAnim.DoScaledAnimationAsync("WTLightsIdle", 0.5f);
                        leftLightAnim.DoScaledAnimationAsync("WBLightsIdle", 0.5f);
                        rightLightAnim.DoScaledAnimationAsync("WBLightsIdle", 0.5f);
                        break;
                    case LightsStage.Stage1:
                        topLightAnim.DoScaledAnimationAsync(gameDim ? "WTLightsStage1" : "WTLightsStage2", 0.5f);
                        leftLightAnim.DoScaledAnimationAsync(gameDim ? "WBLightsStage1" : "WBLightsStage2", 0.5f);
                        rightLightAnim.DoScaledAnimationAsync(gameDim ? "WBLightsStage1" : "WBLightsStage2", 0.5f);
                        break;
                    case LightsStage.Stage2:
                        topLightAnim.DoScaledAnimationAsync(gameDim ? "WTLightsStageAlt" : "WTLightsStageAlt2", 0.5f);
                        leftLightAnim.DoScaledAnimationAsync(gameDim ? "WBLightsStageAlt" : "WBLightsStageAlt2", 0.5f);
                        rightLightAnim.DoScaledAnimationAsync(gameDim ? "WBLightsStageAlt" : "WBLightsStageAlt2", 0.5f);
                        break;
                    case LightsStage.Stage3:
                        topLightAnim.DoScaledAnimationAsync(gameDim ? "WTLightsStage3" : "WTLightsStage4", 0.5f);
                        leftLightAnim.DoScaledAnimationAsync(gameDim ? "WBLightsStage3" : "WBLightsStage4", 0.5f);
                        rightLightAnim.DoScaledAnimationAsync(gameDim ? "WBLightsStage3" : "WBLightsStage4", 0.5f);
                        break;
                }
            }
        }

        private void SetIntervalStart(double beat, double gameSwitchBeat, float interval = 8f, bool autoPassTurn = true, bool count = false, bool text = false, bool resetColor = false)
        {
            bopState = BopState.Think;
            warioBodyAnim.DoScaledAnimationAsync("WahThink", 0.5f);
            DancerCenter(beat - 4);

            CallAndResponseHandler newHandler = new();
            crHandlerInstance = newHandler;
            crHandlerInstance.StartInterval(beat, interval);
            List<RiqEntity> relevantInputs = GetAllInputsBetweenBeat(beat, beat + interval);
            foreach (var wa in relevantInputs)
            {
                if (wa.datamodel == "warioDeMambo/turn" && wa.beat>= gameSwitchBeat)
                    {
                        TurnInputController(wa.beat);
                    }
                    else if (wa.beat >= gameSwitchBeat && wa.datamodel != "warioDeMambo/turn")
                    {
                        JumpInputController(wa.beat);
                    }
            }
            if (count && autoPassTurn)
                            {
                                var b = interval;
                                var c = beat+interval+interval;
                                if (c-4 >= 0) CountDownFour(c-4);
                                else if (c-3>=0) CountDownThree(c-3);
                                else if (c-2>=0) CountDownTwo(c-2);
                                else if (c-1>=0) CountDownOne(c-1);               
                            }
            if (text && autoPassTurn)
                            {
                                var g = interval;
                                var h = beat+interval+interval;
                                if (g-4 >= 0) SetText(h-4, 0f, "4");
                                if (g-3 >= 0) SetText(h-3, 0f, "3");
                                if (g-2 >= 0) SetText(h-2, 0f, "2");
                                if (g-1 >= 0) SetText(h-1, 1f, "1");
                            }

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 4, delegate
                {
                    dancerBopState = BopState.Think;
                    StartCoroutine(ResetArms(true));
                }),
                new BeatAction.Action(beat - 3, delegate
                {
                    spotLEase = new()
                    {
                        beat = beat - 3,
                        length = 2.5f,
                        ease = Util.EasingFunction.Ease.EaseOutQuint
                    };
                    spotLlastPos = spotlightLTrans.localPosition;
                    spotLtargetPos = DancerLSpotPos.localPosition;

                    spotREase = new()
                    {
                        beat = beat - 3,
                        length = 2.5f,
                        ease = Util.EasingFunction.Ease.EaseOutQuint
                    };
                    spotRlastPos = spotlightRTrans.localPosition;
                    spotRtargetPos = DancerRSpotPos.localPosition;

                    spotsPos = SpotsPos.Dancers;
                }),
                new BeatAction.Action(beat - 2.5, delegate
                {
                    dancerBopState = BopState.Ready;
                }),
                new BeatAction.Action(beat - 1.5, delegate
                {
                    dancerBopState = BopState.Normal;
                }),
                new BeatAction.Action(beat, delegate
                {
                    SetColors(false, false);
                }),
                new BeatAction.Action(beat + interval + 1, delegate
                {
                    if (spotsPos == SpotsPos.Dancers) 
                    {
                        MoveSpotlights(beat + interval + 1, 1.5f, true);
                        spotsPos = SpotsPos.Random;
                    }
                }),
                new BeatAction.Action(beat + (interval * 3) - 0.5, delegate
                {
                    if (autoPassTurn)
                    {
                        IntervalReaction(beat + (interval * 3) - 0.5, interval * 0.375f, resetColor);
                    }
                }),
            });
            
            if (autoPassTurn)
            {
                PassTurn(beat + interval, interval, newHandler);
            }
            if (text)
            {
                SetText(beat-2, 0f, "Memorize!");
            }
        }

        public void CountDownFour(double beat)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
            new MultiSound.Sound("warioDeMambo/four", beat),
            new MultiSound.Sound("warioDeMambo/three", beat+1f, offset: 0.05f),
            new MultiSound.Sound("warioDeMambo/two", beat+2f, offset: 0.04f),
            new MultiSound.Sound("warioDeMambo/one", beat+3f, offset: 0.07f),
            }, forcePlay: true);
        }

        public void CountDownThree(double beat)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
            new MultiSound.Sound("warioDeMambo/three", beat, offset: 0.05f),
            new MultiSound.Sound("warioDeMambo/two", beat+1f, offset: 0.04f),
            new MultiSound.Sound("warioDeMambo/one", beat+2f, offset: 0.07f),
            }, forcePlay: true);
        }


        public void CountDownTwo(double beat)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
            new MultiSound.Sound("warioDeMambo/two", beat, offset: 0.04f),
            new MultiSound.Sound("warioDeMambo/one", beat+1f, offset: 0.07f),
            }, forcePlay: true);
        }

        public void CountDownOne(double beat)
        {
            SoundByte.PlayOneShotGame("warioDeMambo/one", beat, offset: 0.07f);
        }


        private bool IntervalIsGoingOn()
        {
            double beat = Conductor.instance.songPositionInBeats;
            return EventCaller.GetAllInGameManagerList("warioDeMambo", new string[] { "beat intervals" }).Find(x => beat >= x.beat && beat < x.beat + x.length) != null;
        }

        private static List<RiqEntity> GetAllInputsBetweenBeat(double beat, double endBeat)
        {
            List<RiqEntity> danceEvents = EventCaller.GetAllInGameManagerList("warioDeMambo", new string[] { "turn", "jump"});
            List<RiqEntity> tempEvents = new();

            foreach (var entity in danceEvents)
            {
                if (entity.beat >= beat && entity.beat < endBeat)
                {
                    tempEvents.Add(entity);
                }
            }
            return tempEvents;
        }

        public static void PreInterval(double beat, float interval, bool autoPassTurn, bool memorize, bool count, bool text, bool left, bool resetColor)
        {
            if (memorize)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("warioDeMambo/memorize", beat-2),
                    //new MultiSound.Sound("warioDeMambo/memorize1", beat-2),
                    //new MultiSound.Sound("warioDeMambo/memorize2", beat-1.75, offset: 0.034f),
                    //new MultiSound.Sound("warioDeMambo/memorize3", beat-1.5, offset: 0.03f),
                }, forcePlay: true);
            }

            if (left)
            {
                shouldBeLeft = true;
            }
            else
            {
                shouldBeLeft = false;
            }
            
            if (GameManager.instance.currentGame == "warioDeMambo")
            {
                instance.SetIntervalStart(beat, beat, interval, autoPassTurn, count, text, resetColor);
            }
            else
            {
                // we dont need queued intervals. not in MY country...
            }

            
        }


        public void TurnInputController(double beat)
        {
            if (crHandlerInstance.queuedEvents.Count > 0 && crHandlerInstance.queuedEvents.Find(x => x.beat == beat || (beat >= x.beat && beat <= x.beat + x.length)) != null) return;         
             
            //switch(direction)
            //{
                

            //    case (int) WhichDirection.Automatic:

                if (shouldBeLeft)
                {
                    SoundByte.PlayOneShotGame("warioDeMambo/left", beat);
                    GirlsInput(beat, false, true);
                    shouldBeLeft = false;
                    crHandlerInstance.AddEvent(beat, tag: "turnLeft");  
                }
                else if (!shouldBeLeft)
                {
                    SoundByte.PlayOneShotGame("warioDeMambo/right", beat);
                    GirlsInput(beat, false, false);
                    shouldBeLeft = true;
                    crHandlerInstance.AddEvent(beat, tag: "turnRight");  
                }
            //    break;


            //    case (int) WhichDirection.Left:

            //    SoundByte.PlayOneShotGame("warioDeMambo/left", beat);
            //    GirlsInput(beat, false, true);
            //    shouldBeLeft = false;
            //    crHandlerInstance.AddEvent(beat, tag: "turnLeft");  
            //    break;

            //    case (int) WhichDirection.Right:

            //    SoundByte.PlayOneShotGame("warioDeMambo/right", beat);
            //    GirlsInput(beat, false, false);
            //    shouldBeLeft = true;
            //    crHandlerInstance.AddEvent(beat, tag: "turnRight");  
            //    break;

            //    default:
            //    break;
            //}
                       
        }

        public void JumpInputController(double beat)
        {
            if (crHandlerInstance.queuedEvents.Count > 0 && crHandlerInstance.queuedEvents.Find(x => x.beat == beat || (beat >= x.beat && beat <= x.beat + x.length)) != null) return;            
            crHandlerInstance.AddEvent(beat, tag: "jump");
            SoundByte.PlayOneShotGame("warioDeMambo/jump", beat);
            GirlsInput(beat, true, false);
        }   

        public void GirlsInput(double beat, bool jump, bool left)
        {
            if (jump)
            {
                DancerJump(beat);
            }
            else if (left)
            {
                DancerLeftTurn(beat);
            }
            else 
            {
                DancerRightTurn(beat);
            }
        }


        //private bool ShouldBeLeft(double beat, WhichDirection direction)
        //{
        //     switch (direction)
        //    {
        //        case WhichDirection.Automatic:

        //    }
        //}

        private RiqEntity GetLastIntervalBeforeBeat(double beat)
        {
            return EventCaller.GetAllInGameManagerList("warioDeMambo", new string[] { "beat intervals" }).FindLast(x => x.beat <= beat);
        }

        public static void PrePassTurn(double beat, float length, bool count, bool text)
        {
            if (count)
                            {
                                var b = length;
                                var c = beat+length;
                                if (b >= 4) instance.CountDownFour(c-4);
                                else if (b>=3) instance.CountDownThree(c-3);
                                else if (b>=2) instance.CountDownTwo(c-2);
                                else if (b>=1) instance.CountDownOne(beat);               
                            }
                            if (text)
                            {
                                var g = length;
                                var h = beat+length;
                                if (g-4 >= 0) instance.SetText(h-4, 0f, "4");
                                if (g-3 >= 0) instance.SetText(h-3, 0f, "3");
                                if (g-2 >= 0) instance.SetText(h-2, 0f, "2");
                                if (g-1 >= 0) instance.SetText(h-1, 0f, "1");
                            }
            
            if (GameManager.instance.currentGame == "warioDeMambo")
            {
                instance.PassTurnStandalone(beat, length);
                
            }
            else
            {
                passedTurns.Add(new PassedTurn()
                {
                    beat = beat,
                    length = length,
                });
            }
        }


        private void PassTurnStandalone(double beat, float length)
        {
            
            if (crHandlerInstance != null) PassTurn(beat, length, crHandlerInstance);
        }

        private void PassTurn(double beat, double length, CallAndResponseHandler crHandler)
        {
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    SetColors(true, true);
                    if (length > 4f) SetText(beat, 0f, "Your turn!");
                }),
                new BeatAction.Action(beat + Math.Max(length - 4, 0), delegate
                {
                    DancerCenter(beat + Math.Max(length - 4, 0));
                }),
                new BeatAction.Action(beat + Math.Max(length - 2.5, 0), delegate
                {
                    spotLEase = new()
                    {
                        beat = beat + Math.Max(length - 2.5, 0),
                        length = 2.5f,
                        ease = Util.EasingFunction.Ease.EaseOutQuint
                    };
                    spotLlastPos = spotlightLTrans.localPosition;
                    spotLtargetPos = WarioSpotPos.localPosition;
                    
                    spotREase = new()
                    {
                        beat = beat + Math.Max(length - 2.5, 0),
                        length = 2.5f,
                        ease = Util.EasingFunction.Ease.EaseOutQuint
                    };
                    spotRlastPos = spotlightRTrans.localPosition;
                    spotRtargetPos = WarioSpotPos.localPosition;

                    spotsPos = SpotsPos.Wario;
                    bopState = BopState.Normal;
                }),
                new BeatAction.Action(beat - 0.25 + length, delegate
                {
                    if (crHandler.queuedEvents.Count > 0)
                    {
                        foreach (var crEvent in crHandler.queuedEvents)
                        {
                            if (crEvent.tag == "turnLeft")
                            {
                                ScheduleInput(beat + length, crEvent.relativeBeat, InputAction_L, JustTurnL, Miss, Nothing);
                                DancerLeftTurn(beat + length + crEvent.relativeBeat); 
                            }
                            else if (crEvent.tag == "turnRight")
                            {
                                ScheduleInput(beat + length, crEvent.relativeBeat, InputAction_R, JustTurnR, Miss, Nothing);
                                DancerRightTurn(beat + length + crEvent.relativeBeat);
                            }
                            else if (crEvent.tag == "jump")
                            {
                                ScheduleInput(beat + length, crEvent.relativeBeat, InputAction_Jump, JustJump, Miss, Nothing);
                                DancerJump(beat + length + crEvent.relativeBeat);
                            }
                            
                        }
                        
                        crHandler.queuedEvents.Clear();
                    }
                }),
                new BeatAction.Action(beat + length, delegate
                {
                    SetColors(true, false);
                }),
            });
            
        }

        public void DancerLeftTurn(double beat)
        {
            if (isDancing) return;
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    dancerLeft = true;
                    if (dancerArmCentered)
                    {
                        dancerArmCentered = false;
                        dancerLeftArmAnim.DoScaledAnimationAsync("DArmCtoL", 0.5f);
                        dancerLeftHeadAnim.DoScaledAnimationAsync("DHeadCtoL", 0.5f);
                        dancerRightArmAnim.DoScaledAnimationAsync("DArmCtoL", 0.5f);
                        dancerRightHeadAnim.DoScaledAnimationAsync("DHeadCtoL", 0.5f);
                    }
                    else
                    {
                        dancerLeftArmAnim.DoScaledAnimationAsync("DArmRtoL", 0.5f);
                        dancerLeftHeadAnim.DoScaledAnimationAsync("DHeadRtoL", 0.5f);
                        dancerRightArmAnim.DoScaledAnimationAsync("DArmRtoL", 0.5f);
                        dancerRightHeadAnim.DoScaledAnimationAsync("DHeadRtoL", 0.5f);
                    }
                }),
            });
        }

        public void DancerRightTurn(double beat)
        {
            if (isDancing) return;
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    dancerLeft = false;
                    if (dancerArmCentered)
                    {
                        dancerArmCentered = false;
                        dancerLeftArmAnim.DoScaledAnimationAsync("DArmCtoR", 0.5f);
                        dancerLeftHeadAnim.DoScaledAnimationAsync("DHeadCtoR", 0.5f);
                        dancerRightArmAnim.DoScaledAnimationAsync("DArmCtoR", 0.5f);
                        dancerRightHeadAnim.DoScaledAnimationAsync("DHeadCtoR", 0.5f);
                    }
                    else
                    {
                        dancerLeftArmAnim.DoScaledAnimationAsync("DArmLtoR", 0.5f);
                        dancerLeftHeadAnim.DoScaledAnimationAsync("DHeadLtoR", 0.5f);
                        dancerRightArmAnim.DoScaledAnimationAsync("DArmLtoR", 0.5f);
                        dancerRightHeadAnim.DoScaledAnimationAsync("DHeadLtoR", 0.5f);
                    }
                }),
            });
        }

        public void DancerCenter(double beat)
        {
            if (isDancing) return;
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    if (dancerArmCentered)
                    {
                        return;
                    }
                    else
                    {
                        dancerArmCentered = true;
                        dancerLeftArmAnim.DoScaledAnimationAsync(dancerLeft ? "DArmLtoC" : "DArmRtoC", 0.5f);
                        dancerLeftHeadAnim.DoScaledAnimationAsync(dancerLeft ? "DHeadLtoC" : "DHeadRtoC", 0.5f);
                        dancerRightArmAnim.DoScaledAnimationAsync(dancerLeft ? "DArmLtoC" : "DArmRtoC", 0.5f);
                        dancerRightHeadAnim.DoScaledAnimationAsync(dancerLeft ? "DHeadLtoC" : "DHeadRtoC", 0.5f);
                    }
                }),
            });
        }

        public void DancerJump(double beat)
        {
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    dancerLeftJumpAnim.DoScaledAnimationAsync("DJump", 0.5f);
                    dancerRightJumpAnim.DoScaledAnimationAsync("DJump", 0.5f);
                }),
            });
        }

         public void JustJump(PlayerActionEvent caller, float state)
        {
            hasFlicked = true;
            SoundByte.PlayOneShotGame("warioDeMambo/jump");
            warioJumpAnim.DoScaledAnimationAsync("WJump", 0.5f);
        }

        public void JustTurnL(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("warioDeMambo/left");
            warioLeft = true;
            if (armCentered)
            {
                armCentered = false;
                warioArmAnim.DoScaledAnimationAsync("WHandCtoL", 0.5f);
                warioFaceAnim.DoScaledAnimationAsync("WFaceCtoL", 0.5f);
            }
            else
            {
                warioArmAnim.DoScaledAnimationAsync("WHandRtoL", 0.5f);
                warioFaceAnim.DoScaledAnimationAsync("WFaceRtoL", 0.5f);
            }
        }

        public void JustTurnR(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("warioDeMambo/right");
            warioLeft = false;
            if (armCentered)
            {
                armCentered = false;
                warioArmAnim.DoScaledAnimationAsync("WHandCtoR", 0.5f);
                warioFaceAnim.DoScaledAnimationAsync("WFaceCtoR", 0.5f);
            }
            else
            {
                warioArmAnim.DoScaledAnimationAsync("WHandLtoR", 0.5f);
                warioFaceAnim.DoScaledAnimationAsync("WFaceLtoR", 0.5f);
            }
        }

        public void Miss(PlayerActionEvent caller)
        {
            misses++;
            SoundByte.PlayOneShot("miss");
            if (!IntervalIsGoingOn())
            {
            }
        }

        public void IntervalReaction(double beat, float length, bool resetColor = false)
        {
            if (misses > 0)
            {
                warioBodyAnim.DoScaledAnimationAsync("WahBad", 0.5f);
                bopState = BopState.Fail;
                dancerBopState = BopState.Fail;
                SetText(beat, length, "No!");
            }
            else
            {
                SoundByte.PlayOneShotGame("warioDeMambo/applause");
                warioBodyAnim.DoScaledAnimationAsync("WahBopHap", 0.5f);
                bopState = BopState.Happy;
                dancerBopState = BopState.Happy;
            }

            BeatAction.New(instance, new List<BeatAction.Action>(){
                new BeatAction.Action(beat + length - 1, delegate {
                    if (!armCentered) 
                    {
                        warioArmAnim.DoScaledAnimationAsync(warioLeft ? "WHandLtoC" : "WHandRtoC", 0.5f);
                        warioFaceAnim.DoScaledAnimationAsync(warioLeft ? "WFaceLtoC" : "WFaceRtoC", 0.5f);
                    }
                    armCentered = true;
                    if (resetColor) SetColors(false, true);
                }),
                new BeatAction.Action(beat + length, delegate {
                    bopState = BopState.Normal;
                    if (resetColor)
                    {
                        if (spotsPos == SpotsPos.Wario) 
                        {
                            MoveSpotlights(beat + length, 1.5f, true);
                            spotsPos = SpotsPos.Random;
                        }
                    }
                }),
                new BeatAction.Action(beat + length + 1, delegate {
                    DancerCenter(beat + length + 1);
                }),
             });
            misses = 0;
        }

        public void Nothing(PlayerActionEvent caller) {}

        //Text and Aesthetics

        public void SetText(double beat, float length, string command)
        {
            BeatAction.New(instance, new List<BeatAction.Action>(){
                new BeatAction.Action(beat, delegate{
                    commandText.text = command;
                    textAnimator.DoScaledAnimationAsync("Text", 0.5f);
                }),
                new BeatAction.Action(beat+length, delegate{
                    if (length > 0f && commandText.text == command) commandText.text = "";
                }),
            });
        }

        public void IntroVoice(double beat)
        {
            SoundByte.PlayOneShotGame("warioDeMambo/ladiesandgentlemen", beat+0.5f);
            SoundByte.PlayOneShotGame("warioDeMambo/wariodemambo", beat+8.5f);

        }

        public void SetColors(bool red, bool dim)
        {
            gameDim = dim;
            if (red)
            {
                mainMat.SetFloat("_HueShift", 0.5f);
                lightMat.SetFloat("_HueShift", 0.5f);
                floorLightMat.SetFloat("_HueShift", 0.5f);
            }
            else
            {
                mainMat.SetFloat("_HueShift", 0f);
                lightMat.SetFloat("_HueShift", 0f);
                floorLightMat.SetFloat("_HueShift", 0f);
            }

            if (!dim)
            {
                mainMat.SetColor("_AddColor", red ? redAddColor : blueAddColor);
                lightMat.SetColor("_AddColor", red ? redAddColor : blueAddColor);
                floorLightMat.SetColor("_AddColor", red ? redAddColor : blueAddColor);
                spotlightLSprite.color = new Color(1f, 1f, 1f, 1f);
                spotlightRSprite.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                mainMat.SetColor("_AddColor", Color.black);
                lightMat.SetColor("_AddColor", Color.black);
                floorLightMat.SetColor("_AddColor", Color.black);
                spotlightLSprite.color = new Color(1f, 1f, 1f, 0.75f);
                spotlightRSprite.color = new Color(1f, 1f, 1f, 0.75f);
            }
        }

        public void Dance(double beat, float length, DanceType danceType)
         {
            canBop = false;
            isDancing = true;
            armCentered = true;
            int pose = 0;

            StartCoroutine(ResetArms(false));

            switch (danceType)
            {
                case DanceType.Stationary:
                    warioBodyAnim.DoScaledAnimationAsync("WDanceLoop", 0.5f);
                    dancerLeftAnim.DoScaledAnimationAsync("DancerFinalLoop", 0.5f);
                    dancerRightAnim.DoScaledAnimationAsync("DancerFinalLoop", 0.5f);
                    dancerLeftArmAnim.DoScaledAnimationAsync("DArmDanceLoop", 0.5f);
                    dancerRightArmAnim.DoScaledAnimationAsync("DArmDanceLoop", 0.5f);
                    dancerLeftHeadAnim.DoScaledAnimationAsync("DHeadDanceLoop", 0.5f);
                    dancerRightHeadAnim.DoScaledAnimationAsync("DHeadDanceLoop", 0.5f);
                    break;
                case DanceType.Pose1:
                    warioBodyAnim.DoScaledAnimationAsync("WDanceLoop", 0.5f);
                    dancerLeftAnim.DoScaledAnimationAsync("DancerFinalLoop", 0.5f);
                    dancerRightAnim.DoScaledAnimationAsync("DancerFinalLoop", 0.5f);
                    dancerLeftArmAnim.DoScaledAnimationAsync("DArmDanceLoop", 0.5f);
                    dancerRightArmAnim.DoScaledAnimationAsync("DArmDanceLoop", 0.5f);
                    dancerLeftHeadAnim.DoScaledAnimationAsync("DHeadDanceLoop", 0.5f);
                    dancerRightHeadAnim.DoScaledAnimationAsync("DHeadDanceLoop", 0.5f);
                    danceAnim = "WWalkCRC";
                    pose = 1;
                    break;
                case DanceType.Pose2:
                    warioBodyAnim.DoScaledAnimationAsync("WDanceLoop", 0.5f);
                    dancerLeftAnim.DoScaledAnimationAsync("DancerFinalLoop", 0.5f);
                    dancerRightAnim.DoScaledAnimationAsync("DancerFinalLoop", 0.5f);
                    dancerLeftArmAnim.DoScaledAnimationAsync("DArmDanceLoop", 0.5f);
                    dancerRightArmAnim.DoScaledAnimationAsync("DArmDanceLoop", 0.5f);
                    dancerLeftHeadAnim.DoScaledAnimationAsync("DHeadDanceLoop", 0.5f);
                    dancerRightHeadAnim.DoScaledAnimationAsync("DHeadDanceLoop", 0.5f);
                    danceAnim = "WWalkCLC";
                    pose = 2;
                    break;
                case DanceType.End:
                    warioBodyAnim.DoScaledAnimationAsync("WDanceLoop", 0.5f);
                    dancerLeftAnim.DoScaledAnimationAsync("DancerFinalLoop", 0.5f);
                    dancerRightAnim.DoScaledAnimationAsync("DancerFinalLoop", 0.5f);
                    dancerLeftArmAnim.DoScaledAnimationAsync("DArmDanceLoop", 0.5f);
                    dancerRightArmAnim.DoScaledAnimationAsync("DArmDanceLoop", 0.5f);
                    dancerLeftHeadAnim.DoScaledAnimationAsync("DHeadDanceLoop", 0.5f);
                    dancerRightHeadAnim.DoScaledAnimationAsync("DHeadDanceLoop", 0.5f);
                    danceAnim = "WWalkFinal";
                    pose = 3;
                    break;
            }

            if (pose != 0)
            {
                danceEase = new()
                {
                    beat = beat,
                    length = length,
                    ease = Util.EasingFunction.Ease.Linear
                };
            }

            BeatAction.New(instance, new List<BeatAction.Action>(){
                new BeatAction.Action(beat+length, delegate{
                    if (danceType != DanceType.Stationary)
                    {
                        if (pose == 3)
                        {
                            endPose.SetActive(true);
                            return;
                        }
                        warioBodyAnim.DoScaledAnimationAsync("WahPose" + pose, 0.5f);
                        dancerLeftAnim.DoScaledAnimationAsync("DancerPose" + pose, 0.5f);
                        dancerRightAnim.DoScaledAnimationAsync("DancerPose" + pose, 0.5f);
                        dancerLeftArmAnim.DoScaledAnimationAsync("DArmPose" + pose, 0.5f);
                        dancerRightArmAnim.DoScaledAnimationAsync("DArmPose" + pose, 0.5f);
                        dancerLeftHeadAnim.DoScaledAnimationAsync("DHeadPose" + pose, 0.5f);
                        dancerRightHeadAnim.DoScaledAnimationAsync("DHeadPose" + pose, 0.5f);
                    }
                }),
                new BeatAction.Action(beat+length + 0.25, delegate{
                    canBop = true;
                    isDancing = false;
                })
            });
         }


        private void MoveSpotlights(double beat, float length, bool start = false)
        {
            if (IntervalIsGoingOn())
            {
                return;
            }

            float tX = UnityEngine.Random.Range(-5f, 5f);
            float tY = UnityEngine.Random.Range(-3.5f, -1.1f);
            Vector2 newTarget = new Vector2(tX, tY);
            spotLEase = new()
            {
                beat = beat,
                length = length,
                ease = start ? Util.EasingFunction.Ease.EaseInQuad : Util.EasingFunction.Ease.Linear
            };
            spotLlastPos = spotlightLTrans.localPosition;
            spotLtargetPos = newTarget;

            tX = UnityEngine.Random.Range(-5f, 5f);
            tY = UnityEngine.Random.Range(-3.5f, -1.1f);
            newTarget = new Vector2(tX, tY);
            spotREase = new()
            {
                beat = beat,
                length = length,
                ease = start ? Util.EasingFunction.Ease.EaseInQuad : Util.EasingFunction.Ease.Linear
            };
            spotRlastPos = spotlightRTrans.localPosition;
            spotRtargetPos = newTarget;
        }


        IEnumerator ResetArms(bool enabled)
        {
            if (!enabled)
            {
                warioArmAnim.DoScaledAnimationAsync("WHandIdle", 0.5f);
                warioFaceAnim.DoScaledAnimationAsync("WFaceIdle", 0.5f);
                yield return null;
            }

            warioArmAnim.enabled = enabled;
            warioFaceAnim.enabled = enabled;
        }

        public void ChangeLights(double beat, LightsStage stage)
        {
            lightsStage = stage;

            switch (lightsStage)
            {
                case LightsStage.Off:
                    topLightAnim.DoScaledAnimationAsync("WTLightsIdle", 0.5f);
                    leftLightAnim.DoScaledAnimationAsync("WBLightsIdle", 0.5f);
                    rightLightAnim.DoScaledAnimationAsync("WBLightsIdle", 0.5f);
                    break;
                case LightsStage.Stage1:
                    topLightAnim.DoScaledAnimationAsync(gameDim ? "WTLightsStage1" : "WTLightsStage2", 0.5f);
                    leftLightAnim.DoScaledAnimationAsync(gameDim ? "WBLightsStage1" : "WBLightsStage2", 0.5f);
                    rightLightAnim.DoScaledAnimationAsync(gameDim ? "WBLightsStage1" : "WBLightsStage2", 0.5f);
                    break;
                case LightsStage.Stage2:
                    topLightAnim.DoScaledAnimationAsync(gameDim ? "WTLightsStageAlt" : "WTLightsStageAlt2", 0.5f);
                    leftLightAnim.DoScaledAnimationAsync(gameDim ? "WBLightsStageAlt" : "WBLightsStageAlt2", 0.5f);
                    rightLightAnim.DoScaledAnimationAsync(gameDim ? "WBLightsStageAlt" : "WBLightsStageAlt2", 0.5f);
                    break;
                case LightsStage.Stage3:
                    topLightAnim.DoScaledAnimationAsync(gameDim ? "WTLightsStage3" : "WTLightsStage4", 0.5f);
                    leftLightAnim.DoScaledAnimationAsync(gameDim ? "WBLightsStage3" : "WBLightsStage4", 0.5f);
                    rightLightAnim.DoScaledAnimationAsync(gameDim ? "WBLightsStage3" : "WBLightsStage4", 0.5f);
                    break;
            }
        }
    }
}