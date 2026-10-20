using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrDrummerDuelLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("drummerDuel", "Drummer Duel", "00E8F0", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; DrummerDuel.instance.Bop(e.beat, e.length, e["bopCheer"], e["bopDrummer"], e["bopCheerAuto"], e["bopDrummerAuto"]); },
                    parameters = new List<Param>()
                    {
                        new Param("bopCheer", true, "Cheer Leaders & Referee", "Whether the Cheer Leaders & Referee should bop."),
                        new Param("bopDrummer", true, "Taiko Drummers", "Whether the Taiko Drummers should bop."),
                        new Param("bopCheerAuto", false, "Cheer Leaders & Referee (Auto)", "Whether the Cheer Leaders & Referee should bop automatically."),
                        new Param("bopDrummerAuto", false, "Taiko Drummers (Auto)", "Whether the Taiko Drummers should bop automatically."),
                    },
                    defaultLength = 1f,
                    resizable = true
                },

                new GameAction("beat intervals", "Start Interval")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; DrummerDuel.PreInterval(e.beat, e.length, e["auto"], e["camMove"], e["successvoice"], e["pattern"]);   },
                    parameters = new List<Param>()
                    {
                        new Param("auto", true, "Auto Pass Turn", "Toggle if the turn should be passed automatically at the end of the start interval."),
                        new Param("camMove", true, "Auto Camera Movement", "Toggle if the camera will automatically follow the active drummer"),
                        new Param("successvoice", true, "Success Voiceline", "Toggle if the success voiceline should play after the interval."),
                        new Param("pattern", DrummerDuel.CheerPatterns.None, "Cheer Pattern", "Choose what cheering pattern the cheerleaders will play."),
                    },
                    defaultLength = 7f,
                    resizable = true,
                    priority = 2,
                },
                new GameAction("hitdrum", "Hit Drum")
                {
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("hitsound", DrummerDuel.HitSound.Automatic, "Hit Sound", "Choose what will be said."),
                    },
                    priority = 1
                },
                new GameAction("drummer turnover", "Pass Turn")
                {
                    preFunction = delegate { DrummerDuel.PrePassTurn(eventCaller.currentEntity.beat);  },
                    resizable = true,
                    preFunctionLength = 1
                },
                new GameAction("move camera", "Move Camera")
                {
                    function = delegate { var e = eventCaller.currentEntity; DrummerDuel.MoveCam(e.beat, e.length, e["camPos"], (Util.EasingFunction.Ease)e["ease"], false);   },
                    parameters = new List<Param>()
                    {
                        new Param("camPos", DrummerDuel.CameraLocation.Center, "Camera Position", "Choose where the camera should go to."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    },
                    defaultLength = 0.5f,
                    resizable = true,
                },
                new GameAction("angry", "Toggle Anger")
                {
                    function = delegate { var e = eventCaller.currentEntity; DrummerDuel.instance.Anger(e.beat, e["anger"]);  },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("anger", true, "Anger", "Choose to enable or disable the drummers' anger."),
                    },
                },
                new GameAction("chant", "Chant Voicelines")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; DrummerDuel.instance.playChant(e.beat, e["chantType"]);  },
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("chantType", DrummerDuel.ChantType.CountIn, "Chant Voiceline", "Choose what chant will be played.")
                    },
                },
                new GameAction("endPose", "End Pose")
                {
                    function = delegate { var e = eventCaller.currentEntity; DrummerDuel.instance.EndPose(e.beat);  },
                    defaultLength = 4f,
                },
                new GameAction("npctoggle", "Toggle Characters")
                {
                    function = delegate { var e = eventCaller.currentEntity; DrummerDuel.instance.NPCToggle(e.beat, e["cheer"], e["ref"], e["plat"]);  },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("cheer", true, "Cheer Leaders", "Choose to enable or disable the cheer leaders."),
                        new Param("ref", true, "Referee", "Choose to enable or disable the referee."),
                        new Param("plat", true, "Referee Platform", "Choose to enable or disable the referee's platform."),
                    },
                },
            },
            new List<string>() {"ntr", "repeat"},
            "ntrtaiko", "en",
            new List<string>(){},
            chronologicalSortKey: 18,
            font: "Comet"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    public class DrummerDuel : Minigame
    {
        // welcome to the fun zone

        public enum HitSound
        {
            Automatic = 0,
            Don = 1,
            Do = 2,
            Ko = 3
        }

        public enum CheerPatterns
        {
            None = 0,
            FirstPattern = 1,
            SecondPattern = 2,
            ThirdPattern = 3,
            FourthPattern = 4,
        }

        public enum CameraLocation
        {
            Center = 0,
            Left = 1,
            Right = 2
        }

        public enum ChantType
        {
            CountIn,
            Grunt,
            Anger
        }

        private struct QueuedInterval
        {
            public double beat;
            public float interval;
            public bool autoPassTurn;
            public bool camMove;
            public bool successVoice;
            public int pattern;
        }

        [Header("Animators")]
        [SerializeField] Animator referee;
        [SerializeField] Animator[] cheerLeadersLeft;
        [SerializeField] Animator[] cheerLeadersRight;
        [SerializeField] Animator taikoLeft, taikoRight;
        [SerializeField] Animator drummerLeft, drummerRight;
        [SerializeField] ParticleSystem taikoRightStars;

        [Header("Materials")]
        [SerializeField] Material drummerLeftFaceMaterial;
        [SerializeField] Material drummerRightFaceMaterial;

        [Header("Camera")]
        [SerializeField] private new Transform camera;
        private float cameraX = 0f;
        private float cameraXNew = 0f;
        private double passTurnBeat = double.MaxValue;
        private double autoPassTurnEndBeat = double.MaxValue;
        private const Util.EasingFunction.Ease defaultEase = Util.EasingFunction.Ease.Linear;
        private Util.EasingFunction.Ease currEase = defaultEase;
        //private CameraLocation? OnPlayPersistCamLoc = null;
        //private float? OnPlayPersistCameraX = null;
        public CameraLocation cameraLoc = CameraLocation.Center;
        public float currCamLength;
        public const float autoCamLength = 0.5f;
        public float cameraLeft;
        public float cameraCenter;
        public float cameraRight;
        public bool isAutoCamCurrMoving = false;
        public bool isManualCamCurrMoving = false;
        public bool isAutoCamEnabled = true;
        public bool isInitialload = false;

        private struct PersistCamCache
        {
            public List<RiqEntity> manualCamEvents;
            public List<RiqEntity> intervals;
            public List<RiqEntity> turnovers;
            public RiqEntity lastManualCamEvent;
            public RiqEntity lastInterval;
            public RiqEntity lastTurnover;
            public double lastManualCamBeat;
            public double lastIntervalBeat;
            public double lastTurnoverBeat;
            public double mostRecentEventBeat;
        }

        [Header("Properties")]
        private bool hasMissed, isDrumming = false;
        private bool isAngry = false;
        private bool isWhiffing = false;
        private bool goBopCheer;
        private bool goBopDrummer;
        private bool allowBopCheer;
        public bool allDon = true;
        public bool isRight = true;

        [Header("Hidable Objects")]
        public GameObject refereeObj;
        public GameObject refereePlatformObj;
        public GameObject cheerLeadersObj;

        private static List<QueuedInterval> queuedIntervals = new List<QueuedInterval>();
        private static List<double> passedTurns = new();
        List<BeatAction.Action> cheerActions = new();

        [Header("Colors")]
        private static Color defaultLeftFaceColor;
        private static Color defaultRightFaceColor;
        private static Color angryHeadColor = ConvertColorFromHex("#FF8181");

        private static List<Color> _leftDrummerFaceColors = null;
        public static List<Color> leftDrummerFaceColors
        {
            get
            {
                if (_leftDrummerFaceColors == null)
                {
                    _leftDrummerFaceColors = ConvertColorDictToList(LeftDrummerFaceLookup);
                }
                return _leftDrummerFaceColors;
            }
        }

        private static List<Color> _rightDrummerFaceColors = null;
        public static List<Color> rightDrummerFaceColors
        {
            get
            {
                if (_rightDrummerFaceColors == null)
                {
                    _rightDrummerFaceColors = ConvertColorDictToList(RightDrummerFaceLookup);
                }
                return _rightDrummerFaceColors;
            }
        }

        protected static bool IA_PadAny(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.East, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Up, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Down, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Right, out dt);
        }

        public static PlayerInput.InputAction InputAction_Press =
            new("NtrDrummerPress", new int[] { IAPressCat, IAFlickCat, IAPressCat },
            IA_PadAny, IA_TouchBasicPress, IA_BatonBasicPress);

        // Cheer Patterns
        private Dictionary<int, string> CheerPattern1 = new Dictionary<int, string>
        {
            [0] = "Cheer1R",
            [1] = "Cheer1R",
            [2] = "Cheer1L",
            [3] = "Cheer1L",
        };

        private Dictionary<int, string> CheerPattern2 = new Dictionary<int, string>
        {
            [0] = "Cheer2Ra",
            [1] = "Cheer2Rb",
            [2] = "Cheer2La",
            [3] = "Cheer2Lb",
        };

        private Dictionary<int, string> CheerPattern3 = new Dictionary<int, string>
        {
            [0] = "Cheer1R",
            [1] = "Cheer1L",
            [2] = "Cheer1R",
            [3] = "Cheer1L",
        };

        private Dictionary<int, string> CheerPattern4 = new Dictionary<int, string>
        {
            [0] = "Cheer1M",
            [1] = "Cheer1M",
            [2] = "Cheer1M",
            [3] = "Cheer1M",
        };

        //Recoloring stuffs
        private static readonly Dictionary<int, string> LeftDrummerFaceLookup = new() {
            { 0, "#FFEFC7" }, //skin
            { 1, "#EF6D6D" }, //blush
            { 2, "#FF8D12" }, //headband
        };

        private static readonly Dictionary<int, string> RightDrummerFaceLookup = new() {
            { 0, "#FFFFFF" }, //skin
            { 1, "#EFA7A7" }, //blush
            { 2, "#00FBFF" }, //headband
        };

        public static DrummerDuel instance;

        // todo?: Convert these 2 methods into utility class methods for multi-use?
        public static Color ConvertColorFromHex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }

        public static List<Color> ConvertColorDictToList(Dictionary<int, string> colorDict)
        {
            List<Color> colors = new List<Color>();
            for ( int i = 0; i < colorDict.Count; i++ )
            {
                colors.Add(ConvertColorFromHex(colorDict[i]));
            }
            return colors;
        }

        // Should this be in other C&R for queuedIntervals? 
        // Is Clear() just good enough in OnGameSwitch() only?
        void OnDestroy()
        {
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }

            if (!Conductor.instance.NotStopped()) {
                queuedIntervals.Clear();
                passedTurns.Clear();
            }
        }

        private void Awake()
        {
            goBopCheer = true;
            goBopDrummer = true;
            allowBopCheer = true;
            //allowBopDrummer = true;
            instance = this;

            defaultLeftFaceColor = leftDrummerFaceColors[0];
            defaultRightFaceColor = rightDrummerFaceColors[0];
            ApplyDrummerHeadColors();
            DrummerDuel.instance.referee.DoScaledAnimationAsync("HeadNormal", 0.5f);
            ResetCameraState(CameraLocation.Center, cameraCenter);
        }

        void Update()
        {
            var cond = Conductor.instance;
            UpdateCamera();

            if (PlayerInput.GetIsAction(InputAction_Press) && !IsExpectingInputNow(InputAction_Press) && isDrumming)
            {
                ScoreMiss();

                SoundByte.PlayOneShotGame("drummerDuel/drumRightWhiff");
                taikoRight.DoScaledAnimationAsync("Whiff", 0.5f);
                drummerRight.DoScaledAnimationAsync("HitArmF", 0.5f);
                drummerRight.DoScaledAnimationAsync("FaceWhiff", 0.5f);

                if (isAngry)
                {
                    isWhiffing = true;
                    ApplyDrummerHeadColors(); 
                }

                foreach (var anim in cheerLeadersRight)
                {
                    if (hasMissed != true)
                    {
                        hasMissed = true;
                        SoundByte.PlayOneShotGame("drummerDuel/miss");
                    }
                    if (anim.IsPlayingAnimationNames("Just"))
                    {
                    anim.DoScaledAnimationAsync("Miss", 0.5f);
                    }
                }
            }

            if (passedTurns.Count > 0)
            {
                foreach (var pass in passedTurns)
                {
                    PassTurnStandalone(pass);
                }
                passedTurns.Clear();
            }
        }

        public override void OnGameSwitch(double beat)
            {
                defaultLeftFaceColor = leftDrummerFaceColors[0];
                defaultRightFaceColor = rightDrummerFaceColors[0];

                //OnPlayPersistCamLoc = null;
                //OnPlayPersistCameraX = null;
                RestoreBopStates(beat);
                ResetCameraState(CameraLocation.Left, cameraLeft);
                DrummerDuel.instance.referee.DoScaledAnimationAsync("HeadNormal", 0.5f);
                DrummerDuel.instance.referee.DoScaledAnimationAsync("Idle", 0.5f);
                GameSwitchCamera(beat);

                if (Conductor.instance.isPlaying && !Conductor.instance.isPaused)
                {
                    
                    if (queuedIntervals.Count > 0)
                    {
                        foreach (var interval in queuedIntervals)
                        {
                            SetIntervalStart(interval.beat, interval.interval, beat, interval.autoPassTurn, interval.camMove, interval.successVoice, interval.pattern);
                        }
                        queuedIntervals.Clear();
                    }
                    
                }

                var lastAngerEvent = gameManager.Beatmap.Entities.FindLast(e => e.datamodel == "drummerDuel/angry" && e.beat < beat);
                if (lastAngerEvent != null)
                {
                    Anger(lastAngerEvent.beat, lastAngerEvent["anger"]);
                }
                else
                {
                    Anger(beat, false);
                }
            }

        public override void OnPlay(double beat)
        {
            ResetCameraState(CameraLocation.Right, cameraRight);
            DrummerDuel.instance.referee.DoScaledAnimationAsync("HeadNormal", 0.5f);
            DrummerDuel.instance.referee.DoScaledAnimationAsync("Idle", 0.5f);
            
            defaultLeftFaceColor = leftDrummerFaceColors[0];
            defaultRightFaceColor = rightDrummerFaceColors[0];

            DoAllCheerleaderAnimations("Idle", cheerLeadersLeft, 0.5f);
            DoAllCheerleaderAnimations("Idle", cheerLeadersRight, 0.5f);

            RestoreBopStates(beat);

            var lastAngerEvent = gameManager.Beatmap.Entities.FindLast(e => e.datamodel == "drummerDuel/angry" && e.beat < beat);
            if (lastAngerEvent != null)
            {
                Anger(lastAngerEvent.beat, lastAngerEvent["anger"]);
            }
            else
            {
                Anger(beat, false);
            }
            PersistOnPlayCamera(beat);
        }

        public override void OnLateBeatPulse(double beat)
        {
            if (goBopCheer && allowBopCheer)
            {
                if (!referee.IsPlayingAnimationNames("Fail", "Finish", "Left", "Prepare", "Right"))
                {
                    referee.DoScaledAnimationAsync("Bop", 0.5f);
                }
                foreach (var anim in cheerLeadersLeft)
                {
                    if (!anim.IsPlayingAnimationNames("Just", "Miss", "Cheer1L", "Cheer1R", "Cheer1M", "Cheer1L", "Cheer2La", "Cheer2Lb", "Cheer2Ra", "Cheer2Rb"))
                    {
                        anim.DoScaledAnimationAsync("Bop", 0.5f);
                    }
                }
                foreach (var anim in cheerLeadersRight)
                {
                    if (!anim.IsPlayingAnimationNames("Just", "Miss", "Cheer1L", "Cheer1R", "Cheer1M", "Cheer1L", "Cheer2La", "Cheer2Lb", "Cheer2Ra", "Cheer2Rb"))
                    {
                        anim.DoScaledAnimationAsync("Bop", 0.5f);
                    }
                }
            }

            if (goBopDrummer)
            {
                if (!drummerLeft.IsPlayingAnimationNames("Hit", "ArmF", "HitF", "ArmB", "HitB", "ArmMadF", "ArmMadB"))
                {
                    drummerLeft.DoScaledAnimationAsync("Bop", 0.5f);
                }
                if (!drummerRight.IsPlayingAnimationNames("Hit", "ArmF", "HitF", "ArmB", "HitB", "ArmMadF", "ArmMadB"))
                {
                    drummerRight.DoScaledAnimationAsync("Bop", 0.5f);

                    if (isWhiffing)
                    {
                        isWhiffing = false;
                        ApplyDrummerHeadColors();
                    }

                    if (isAngry) { drummerRight.DoScaledAnimationAsync("FaceAngry", 0.5f); }
                    else { drummerRight.DoScaledAnimationAsync("FaceIdle", 0.5f); }
                }
            }
        }

        public void Bop(double beat, float length, bool cheer, bool drummer, bool cheerAuto, bool drummerAuto)
        {
            goBopCheer = cheerAuto;
            goBopDrummer = drummerAuto;

            if (cheer || drummer)
            {
                List<BeatAction.Action> bopActions = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                {
                    bopActions.Add(new BeatAction.Action(beat + i, delegate
                    {
                        if (cheer)
                        {
                            bopActions.Add(new BeatAction.Action(beat, delegate
                            {
                                referee.DoScaledAnimationAsync("Bop", 0.5f);
                            }));
                            
                            bopActions.AddRange(CreateCheerAction(beat, "Bop", 0.5f, 0));
                        }
                        if (drummer)
                        {
                            drummerLeft.DoScaledAnimationAsync("Bop", 0.5f);
                            drummerRight.DoScaledAnimationAsync("Bop", 0.5f);
                            if (isAngry) { drummerRight.DoScaledAnimationAsync("FaceAngry", 0.5f); }
                            else { drummerRight.DoScaledAnimationAsync("FaceIdle", 0.5f); }
                        }
                    }));
                }
                BeatAction.New(instance, bopActions);
            }

        }

        public void RestoreBopStates(double beat)
        {
            var lastBopEvent = gameManager.Beatmap.Entities.FindLast(e => e.datamodel == "drummerDuel/bop" && e.beat < beat);
            if (lastBopEvent != null)
            {
                goBopCheer = lastBopEvent["bopCheerAuto"];
                goBopDrummer = lastBopEvent["bopDrummerAuto"];
                allowBopCheer = true;
            }
            else
            {
                goBopCheer = true;
                goBopDrummer = true;
                allowBopCheer = true;
            }
        }

        private void ApplyDrummerHeadColors()
        {
            Color leftFaceColorToUse = isAngry ? angryHeadColor : defaultLeftFaceColor;
            Color rightFaceColorToUse = (isAngry && !isWhiffing) ? angryHeadColor : defaultRightFaceColor;
            
            // Left drummer face colors  
            drummerLeftFaceMaterial.SetColor("_ColorAlpha", leftDrummerFaceColors[1]);  // Blush
            drummerLeftFaceMaterial.SetColor("_ColorBravo", leftFaceColorToUse);        // Skin
            drummerLeftFaceMaterial.SetColor("_ColorDelta", leftDrummerFaceColors[2]);  // Headband

            // Right drummer face colors  
            drummerRightFaceMaterial.SetColor("_ColorAlpha", rightDrummerFaceColors[1]);         
            drummerRightFaceMaterial.SetColor("_ColorBravo", rightFaceColorToUse);         
            drummerRightFaceMaterial.SetColor("_ColorDelta", rightDrummerFaceColors[2]);        

            //Debug.Log($"Left Skin: {leftFaceColorToUse}, Right Skin: {rightFaceColorToUse}, Angry: {isAngry}");
        }

        //todo: Rename all events with "Camera" to "Cam" for consistent naming
        public static void MoveCam(double beat, float length, int camPos, Util.EasingFunction.Ease ease, bool isAutoCamEnabled) //not bothered to do a static wrapper
        {
            if (GameManager.instance.currentGame == "drummerDuel")
            {
                var instance = DrummerDuel.instance;
                
                bool isAutoCamCurrTransitioning = false;
                if (instance.isAutoCamCurrMoving && instance.passTurnBeat != double.MaxValue)
                {
                    float normalizedBeat = Conductor.instance.GetPositionFromBeat(instance.passTurnBeat, autoCamLength);
                    isAutoCamCurrTransitioning = normalizedBeat >= 0f && normalizedBeat <= 1f;
                }

                // auto cam has priority
                if (isAutoCamCurrTransitioning && !isAutoCamEnabled) return;
                
                // kill manual cam
                if (instance.isManualCamCurrMoving && instance.passTurnBeat != double.MaxValue)
                {
                    float camLengthToUse = instance.isAutoCamCurrMoving ? autoCamLength : instance.currCamLength;
                    float normalizedBeat = Conductor.instance.GetPositionFromBeat(instance.passTurnBeat, camLengthToUse);
                    if (normalizedBeat <= 1f && normalizedBeat >= 0f)
                    {
                        Util.EasingFunction.Function easeFunc = Util.EasingFunction.GetEasingFunction(instance.currEase);

                        float camTargetPos = instance.GetCameraXForLocation(instance.cameraLoc);
                        float stoppedCamPos = easeFunc(instance.cameraX, camTargetPos, normalizedBeat); //stop and save cam pos at whatever normalized beat

                        instance.cameraX = stoppedCamPos;
                        GameCamera.AdditionalPosition = new Vector3((instance.cameraX * -1), 0, 0);
                    }
                }

                instance.cameraLoc = (CameraLocation)camPos;
                instance.passTurnBeat = beat;
                instance.currEase = ease;
                instance.currCamLength = isAutoCamEnabled ? autoCamLength : length;

                if (isAutoCamEnabled)
                {
                    instance.isAutoCamCurrMoving = true;
                    instance.isManualCamCurrMoving = false;
                }
                else 
                {
                    instance.isManualCamCurrMoving = true;
                    if (!isAutoCamCurrTransitioning) instance.isAutoCamCurrMoving = false;
                }
            }
        }

        private void ResetCameraState(CameraLocation cameraLoc, float cameraX) 
        {
            /**if (OnPlayPersistCamLoc == null) //gameswitch left
            {
                cameraLoc = CameraLocation.Left;
                cameraX = cameraLeft;
                cameraXNew = cameraLeft;
                OnPlayPersistCamLoc = CameraLocation.Left;
                OnPlayPersistCameraX = cameraLeft;
            }**/
            /**else //persist onplay
            {
                cameraLoc = OnPlayPersistCamLoc.Value;
                cameraX = OnPlayPersistCameraX.Value;
                cameraXNew = OnPlayPersistCameraX.Value;
            }**/
            this.cameraLoc = cameraLoc;
            this.cameraX = cameraX;
            this.cameraXNew = cameraX;
            passTurnBeat = double.MaxValue;
            autoPassTurnEndBeat = double.MaxValue;
            isAutoCamCurrMoving = false;
            isManualCamCurrMoving = false;
            GameCamera.AdditionalPosition = new Vector3((cameraX * -1), 0, 0);
        }

        public void GameSwitchCamera(double beat)
        {
            List<RiqEntity> nextInterval = gameManager.Beatmap.Entities.FindAll(e => e.datamodel is "drummerDuel/beat intervals" && e.beat >= beat && e.beat < (beat+1));
            if (nextInterval.Count > 0)
            {
                var interval = nextInterval[0];
                var isAutoCamEnabledNextInterval = interval["camMove"];
                if (isAutoCamEnabledNextInterval)
                {
                    cameraLoc = CameraLocation.Left;
                    cameraX = cameraLeft;
                    cameraXNew = cameraLeft;
                    //OnPlayPersistCamLoc = CameraLocation.Left;
                    //OnPlayPersistCameraX = cameraLeft;
                    GameCamera.AdditionalPosition = new Vector3((DrummerDuel.instance.cameraX * -1), 0, 0);
                }
            }
        }

        //UpdateOnPlayPersistCamState(), OnPlayPersistCamLoc, OnPlayPersistCameraX should all be redundant now
        // Didn't delete them entirely yet in the case it breaks somewhere somehow

        /**private void UpdateOnPlayPersistCamState()
        {
            if (passTurnBeat == double.MaxValue || (!isAutoCamCurrMoving && !isManualCamCurrMoving)) // PersistOnPlayCamera() will handle mid transition persists
            {
                OnPlayPersistCamLoc = cameraLoc;
                OnPlayPersistCameraX = cameraX;
            }
        }**/

        private PersistCamCache CreateCameraEntityCache(double beat)
        {
            var cache = new PersistCamCache();
            
            cache.manualCamEvents = gameManager.Beatmap.Entities.FindAll(e => e.datamodel == "drummerDuel/move camera");
            cache.intervals = gameManager.Beatmap.Entities.FindAll(e => e.datamodel == "drummerDuel/beat intervals");
            cache.turnovers = gameManager.Beatmap.Entities.FindAll(e => e.datamodel == "drummerDuel/drummer turnover");
            
            cache.lastManualCamEvent = cache.manualCamEvents.FindLast(e => e.beat <= beat);
            cache.lastInterval = cache.intervals.FindLast(e => e.beat <= beat);
            cache.lastTurnover = cache.turnovers.FindLast(e => e.beat <= beat);
            
            cache.lastManualCamBeat = cache.lastManualCamEvent?.beat ?? double.MinValue;
            cache.lastIntervalBeat = cache.lastInterval?.beat ?? double.MinValue;
            cache.lastTurnoverBeat = cache.lastTurnover?.beat ?? double.MinValue;
            cache.mostRecentEventBeat = Math.Max(Math.Max(cache.lastManualCamBeat, cache.lastIntervalBeat), cache.lastTurnoverBeat);
            
            return cache;
        }


        //todo?: Further refacotring by adjusting some helper methods using ".Where()"
        private void PersistOnPlayCamera(double beat) // i hate i hate you i hate you :)
        {
            //idc anymore ill split up the function into as many arbitarry helpers as i want
            CameraLocation finalTargetCamLoc;
            float finalTargetCamX;

            var camCache = CreateCameraEntityCache(beat);

            bool isWithinManualTransWindow = IsInManualCameraTransition(beat, camCache);
            bool isWithinAutoTransWindow = IsInAutoCameraTransition(beat, camCache);

            // Le persist priority order
            if (isWithinAutoTransWindow) // 1. active auto trans
            {
                finalTargetCamLoc = GetAutoCameraTargetForBeat(beat, camCache);
                finalTargetCamX = GetCameraXForLocation(finalTargetCamLoc);
            }
            else if (isWithinManualTransWindow) // 2. active manual trans
            {
                finalTargetCamLoc = GetManualCamLocation(beat, camCache);
                finalTargetCamX = GetCameraXForLocation(finalTargetCamLoc);
            }
            else if (HasCompletedManualCam(beat, camCache)) // 3. non-interrupted manual cam events
            {
                finalTargetCamLoc = (CameraLocation)camCache.lastManualCamEvent["camPos"];
                finalTargetCamX = GetCameraXForLocation(finalTargetCamLoc);
            }
            else if (IsInAutoCameraInterval(beat, camCache)) // 4. last auto cam state
            {
                finalTargetCamLoc = GetCameraLocationForInterval(beat, camCache);
                finalTargetCamX = GetCameraXForLocation(finalTargetCamLoc);
            }
            //condition 3 in theory should catch before this can happen
            /**else if (lastManualCamEvent != null && lastManualCamBeat == mostRecentEventBeat) // 5. last manual cam state
            {
                Debug.Log("last manual cam logic");
                finalTargetCamLoc = GetManualCamLocation(beat, lastManualCamEvent, lastTurnover);
                finalTargetCamX = GetCameraXForLocation(finalTargetCamLoc);
            }**/
            //all persists should be calculated on the spot which is all the above and below conditions
            /**else if (OnPlayPersistCamLoc != null && OnPlayPersistCameraX != null && mostRecentEventBeat != double.MinValue) // 6. existing persist logic
            {
                Debug.Log("existing persist logic");
                finalTargetCamLoc = OnPlayPersistCamLoc.Value;
                finalTargetCamX = OnPlayPersistCameraX.Value;
            }**/
            else if (!isInitialload && camCache.mostRecentEventBeat == double.MinValue) // 7. onAwake; center cam
            {
                finalTargetCamLoc = CameraLocation.Center;
                finalTargetCamX = cameraCenter;
                isInitialload = true;
            }
            else // 8. gameswitch/fallback; left cam
            {
                finalTargetCamLoc = CameraLocation.Left;
                finalTargetCamX = cameraLeft;
            }
            
            cameraLoc = finalTargetCamLoc;
            cameraX = finalTargetCamX;
            cameraXNew = finalTargetCamX;
            
            //OnPlayPersistCamLoc = finalTargetCamLoc;
            //OnPlayPersistCameraX = finalTargetCamX;
            GameCamera.AdditionalPosition = new Vector3((cameraX * -1), 0, 0);
        }

        private bool isWithinWindow(double beat, double startBeat, double length) //both inclusive
        {
            double windowEnd = startBeat + length;
            return beat >= startBeat && beat <= windowEnd;
        }

        private bool IsInManualCameraTransition(double beat, PersistCamCache camCache)
        {
            foreach (var camEvent in camCache.manualCamEvents)
            {
                if (isWithinWindow(beat, camEvent.beat, camEvent.length))
                {
                    var overlappingAutoInterval = camCache.intervals.FindLast(e => e["camMove"] && isWithinWindow(camEvent.beat, e.beat, e.length));

                    if (overlappingAutoInterval != null)
                    {
                        double intervalEnd = overlappingAutoInterval.beat + overlappingAutoInterval.length;
                        if (beat >= intervalEnd) return false;
                    }
                    
                    return true;
                }
            }
            return false;
        }

        private bool IsInAutoCameraTransition(double beat, PersistCamCache camCache)
        {
            var autoCamIntervals = camCache.intervals.FindAll(e => e["camMove"]);
            var turnovers = camCache.turnovers;
            
            //in trans window for response-to-call
            foreach (var interval in autoCamIntervals)
            {
                double transitionStart = interval.beat - 1;
                if (interval["auto"])
                {
                    if (isWithinWindow(beat, transitionStart, autoCamLength))
                    {
                        return true;
                    }
                }
            }
            
            // in trans window for call-to-response
            foreach (var turnover in turnovers)
            {
                var parentInterval = camCache.intervals.FindLast(e => isWithinWindow(e.beat, turnover.beat, e.length));
                    
                if (parentInterval != null)
                {
                    if (isWithinWindow(beat, turnover.beat, autoCamLength))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        private bool HasCompletedManualCam(double beat, PersistCamCache camCache)
        {
            if (camCache.lastManualCamEvent == null) return false;
            
            double manualEndBeat = camCache.lastManualCamEvent.beat + camCache.lastManualCamEvent.length;
            return manualEndBeat <= beat && camCache.lastManualCamBeat == camCache.mostRecentEventBeat;
        }

        private bool IsInAutoCameraInterval(double beat, PersistCamCache camCache)
        {
            var lastInterval = camCache.lastInterval;
            var lastTurnover = camCache.lastTurnover;

            if (lastInterval == null || !lastInterval["camMove"]) return false;

            double intervalEnd = lastInterval.beat + lastInterval.length;

            bool isWithinCallPhase = isWithinWindow(beat, lastInterval.beat, lastInterval.length);
            bool isWithinResponsePhase = (lastInterval["auto"] && beat > intervalEnd && beat <= intervalEnd + lastInterval.length);
            bool isWithinStandalonePassResponsePhase = (lastTurnover != null && 
                isWithinWindow(lastTurnover.beat, lastInterval.beat, lastInterval.length) && 
                (beat > intervalEnd && beat <= (lastTurnover.beat + lastInterval.length)));

            if (isWithinCallPhase || isWithinResponsePhase || isWithinStandalonePassResponsePhase) return true;

            return false;
        }

        private CameraLocation GetCameraLocationForInterval(double beat, PersistCamCache camCache)
        {
            var lastInterval = camCache.lastInterval;
            var lastTurnover = camCache.lastTurnover;
            double turnoverBeat = double.MaxValue;

            if (lastInterval["auto"]) //in auto pass response
            {
                turnoverBeat = lastInterval.beat + lastInterval.length;
            }

            if (lastTurnover != null && isWithinWindow(lastTurnover.beat, lastInterval.beat, lastInterval.length)) //in standalone pass response
            {
                if (beat >= lastTurnover.beat)
                {
                    turnoverBeat = lastTurnover.beat;
                }
            }

            if (turnoverBeat == double.MaxValue)
            {
                return CameraLocation.Left; // in call phasse
            }

            return beat >= turnoverBeat ? CameraLocation.Right : CameraLocation.Left;
        }

        private CameraLocation GetAutoCameraTargetForBeat(double beat, PersistCamCache camCache)
        {
            var turnovers = camCache.turnovers;
            foreach (var turnover in turnovers)
            {
                //only check turnovres within autocam intervals
                var parentInterval = camCache.intervals.FindLast(e => isWithinWindow(e.beat, turnover.beat, e.length) && e["camMove"]);
                    
                if (parentInterval != null && isWithinWindow(beat, turnover.beat, autoCamLength))
                {
                    return CameraLocation.Right;
                }
            }
            
            //must be in start turnover if not in pass turnover
            return CameraLocation.Left;
        }

        private CameraLocation GetManualCamLocation(double beat, PersistCamCache camCache)
        {
            var intervalToUse = camCache.intervals.FindLast(e => isWithinWindow(beat, e.beat, e.length) && e["camMove"]);

            if (intervalToUse != null)
            {
                return GetCameraLocationForInterval(beat, camCache);
            }
            else
            {
                return (CameraLocation)camCache.lastManualCamEvent["camPos"];
            }
        }

        private float GetCameraXForLocation(CameraLocation camLoc)
        {
            return camLoc switch
            {
                CameraLocation.Center => cameraCenter,
                CameraLocation.Left => cameraLeft,
                CameraLocation.Right => cameraRight,
                _ => cameraCenter
            };
        }

        public void UpdateCamera()
        {
            double currentBeat = Conductor.instance.songPositionInBeatsAsDouble;
            if (isAutoCamCurrMoving && (currentBeat >= autoPassTurnEndBeat))
            {
                //Debug.Log($"currentBeat: {currentBeat}, autoPassTurnEndBeat {autoPassTurnEndBeat}, normalizedBeat {Conductor.instance.GetPositionFromBeat(passTurnBeat, autoCamLength)}");
                isAutoCamCurrMoving = false;
                passTurnBeat = double.MaxValue;
            }

            if (passTurnBeat == double.MaxValue) return; //kill early

            float camLengthToUse = isAutoCamCurrMoving ? autoCamLength : currCamLength;
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(passTurnBeat, camLengthToUse);

            switch ((int)cameraLoc)
            {
                case 0:
                    cameraXNew = cameraCenter;
                    break;
                case 1:
                    cameraXNew = cameraLeft;
                    break;
                case 2:
                    cameraXNew = cameraRight;
                    break;
                default: // ya never know
                    cameraXNew = cameraCenter;
                    break;
            }

            if (normalizedBeat <= 1f)
            {
                Util.EasingFunction.Ease easeToUse = isAutoCamCurrMoving ? defaultEase : currEase;
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(easeToUse);
                float newPosX = func(cameraX, cameraXNew, normalizedBeat);
                GameCamera.AdditionalPosition = new Vector3((newPosX * -1), 0, 0);
            }
            else
            {
                if (!isAutoCamCurrMoving)
                {
                    cameraX = cameraXNew;
                    GameCamera.AdditionalPosition = new Vector3((cameraX * -1), 0, 0);
                    passTurnBeat = double.MaxValue;
                    isManualCamCurrMoving = false;
                    //UpdateOnPlayPersistCamState(); // update after manual cam finish
                }
            }
        }

        public void Anger(double beat, bool anger)
        {
            isAngry = anger;

            ApplyDrummerHeadColors();

            if (isAngry)
            {
                drummerLeft.DoScaledAnimationAsync("FaceAngry", 0.5f);
                drummerRight.DoScaledAnimationAsync("FaceAngry", 0.5f);
            }
            else
            {
                drummerLeft.DoScaledAnimationAsync("FaceIdle", 0.5f);
                drummerRight.DoScaledAnimationAsync("FaceIdle", 0.5f);
                defaultLeftFaceColor = leftDrummerFaceColors[0];
                defaultRightFaceColor = rightDrummerFaceColors[0];
            }
        }

        public void playChant(double beat, int type) 
        {
            switch (type)
            {
                case 0:
                    string[] voicelines = new string[] { "drummerDuel/one", "drummerDuel/two"};
                    var voiceline = new MultiSound.Sound[]
                    {
                        new MultiSound.Sound(voicelines[0], beat),
                        new MultiSound.Sound(voicelines[1], beat + 1f),
                    };

                    MultiSound.Play(voiceline, forcePlay: true);
                    break;

                case 1:
                    SoundByte.PlayOneShotGame("drummerDuel/grunt", beat, forcePlay: true);
                    break;
                case 2:
                    SoundByte.PlayOneShotGame("drummerDuel/angry", beat, forcePlay: true);
                    break;
            }
        }

        public void EndPose(double beat)
        {
            BeatAction.New(this, new List<BeatAction.Action>() 
            {
                new BeatAction.Action(beat, delegate 
                {
                    allowBopCheer = false;
                    goBopDrummer = false;
                    referee.DoScaledAnimationAsync("HeadNormal", 0.5f);
                    referee.DoScaledAnimationAsync("Finish", 0.5f);
            
                    DoAllCheerleaderAnimations("Cheer1M", cheerLeadersLeft, 0.5f);
                    DoAllCheerleaderAnimations("Cheer1M", cheerLeadersRight, 0.5f);
                }),
                new BeatAction.Action(beat + 1, delegate
                {
                    referee.speed = 0f;
                    for (int i = 0; i < cheerLeadersRight.Length; i++)
                    {
                        cheerLeadersLeft[i].speed = 0f;
                        cheerLeadersRight[i].speed = 0f;
                    }
                }),
                new BeatAction.Action(beat + 2, delegate 
                {
                    referee.SetScaledAnimationSpeed(0.5f);
                    for (int i = 0; i < cheerLeadersRight.Length; i++)
                    {
                        cheerLeadersLeft[i].SetScaledAnimationSpeed(0.5f);
                        cheerLeadersRight[i].SetScaledAnimationSpeed(0.5f);
                    }
                }),
                new BeatAction.Action(beat + 3, delegate 
                {
                    allowBopCheer = true;
                    goBopDrummer = true;
                })
            });
        }

        public void NPCToggle(double beat, bool cheer, bool referee, bool platform)
        {
            cheerLeadersObj.SetActive(cheer);
            refereeObj.SetActive(referee);
            refereePlatformObj.SetActive(platform);
        }

        private List<RiqEntity> GetAllSpeaksInBetweenBeat(double beat, double endBeat)
        {
            List<RiqEntity> speakEvents = EventCaller.GetAllInGameManagerList("drummerDuel", new string[] { "hitdrum" });
            List<RiqEntity> tempEvents = new();

            foreach (var entity in speakEvents)
            {
                if (entity.beat >= beat && entity.beat <= endBeat)
                {
                    tempEvents.Add(entity);
                }
            }
            return tempEvents;
        }

        private RiqEntity GetLastIntervalBeforeBeat(double beat)
        {
            List<RiqEntity> intervalEvents = EventCaller.GetAllInGameManagerList("drummerDuel", new string[] { "beat intervals" });
            if (intervalEvents.Count == 0) return null;
            var tempEvents = intervalEvents.FindAll(x => x.beat <= beat);
            tempEvents.Sort((x, y) => x.beat.CompareTo(y.beat));
            return tempEvents[^1];
        }

        public static void PreInterval(double beat, float interval, bool autoPassTurn, bool camMove, bool successVoice, int pattern)
        {
            if (GameManager.instance.currentGame == "drummerDuel")
            {
                instance.SetIntervalStart(beat, interval, beat, autoPassTurn, camMove, successVoice, pattern);
            }
            else
            {
                queuedIntervals.Add(new QueuedInterval
                {
                    beat = beat,
                    interval = interval,
                    autoPassTurn = autoPassTurn,
                    camMove = camMove,
                    successVoice = successVoice,
                    pattern = pattern
                });
            }
        }

        private void SetIntervalStart(double beat, float interval, double gameSwitchBeat, bool autoPassTurn, bool camMove, bool successVoice, int pattern)
        {
            allDon = true;
            isRight = true;
            isAutoCamEnabled = camMove;

            List<MultiSound.Sound> sounds = new();

            List<RiqEntity> relevantDrumHitEvents = GetAllSpeaksInBetweenBeat(beat, beat + interval);
            relevantDrumHitEvents.Sort((x, y) => x.beat.CompareTo(y.beat));

            List<BeatAction.Action> refereeActions = new();

            List<BeatAction.Action> cheerActions = CreateCheerPattern(beat, interval, pattern, false);
            cheerActions.AddRange(CreateCheerAction(beat + interval - 1, "Just", 0.5f, -1));

            List<BeatAction.Action> queuedHits = new()
            {
                new BeatAction.Action(beat, delegate
                {
                    if (autoPassTurn)
                    {
                        DrummerTurnOver(beat + interval, beat, beat + interval, 1);
                    }
                })
            };

            for (int i = 0; i < relevantDrumHitEvents.Count; i++)
            {
                double nextBeat = double.MaxValue;
                if (i != relevantDrumHitEvents.Count - 1) { nextBeat = relevantDrumHitEvents[i+1].beat; }

                RiqEntity drumHitEventToCheck = relevantDrumHitEvents[i];
                if (drumHitEventToCheck.beat >= gameSwitchBeat)
                {
                    queuedHits.Add(new BeatAction.Action(drumHitEventToCheck.beat, delegate
                    {
                        DrumHit(drumHitEventToCheck.beat, false);
                    }));
                    sounds.Add(new("drummerDuel/drumLeftHit", drumHitEventToCheck.beat));
                    sounds.Add(new($"drummerDuel/{GetHitSound(drumHitEventToCheck.beat, drumHitEventToCheck["hitsound"], nextBeat)}", drumHitEventToCheck.beat));
                }
            }

            foreach (var drumHitEvent in relevantDrumHitEvents)
            {
                if (drumHitEvent.beat % 1.0 != 0) allDon = false;
            }

            //cam actions
            if (isAutoCamEnabled)
            {
                DrummerDuel.instance.autoPassTurnEndBeat = beat - 1 + autoCamLength;

                BeatAction.New(this, new List<BeatAction.Action>() 
                {
                    new BeatAction.Action(beat - 1, delegate 
                    {
                        MoveCam(beat - 1, autoCamLength, (int)CameraLocation.Left, defaultEase, true);
                    }),
                    new BeatAction.Action(beat - 1 + autoCamLength, delegate 
                    {
                        cameraX = cameraLeft;
                        GameCamera.AdditionalPosition = new Vector3((cameraX * -1), 0, 0);
                        isAutoCamCurrMoving = false;
                        passTurnBeat = double.MaxValue;
                    })
                });
            }

            refereeActions.Add(new BeatAction.Action(beat - 1, delegate
            {
                DrummerDuel.instance.referee.DoScaledAnimationAsync("Left", 0.5f);
                DrummerDuel.instance.referee.DoScaledAnimationAsync("HeadNormal", 0.5f);
            }));
            refereeActions.Add(new BeatAction.Action(beat, delegate
            {
                DrummerDuel.instance.referee.DoScaledAnimationAsync("HeadNormal", 0.5f);
            }));

            MultiSound.Play(sounds);
            BeatAction.New(this, refereeActions);
            BeatAction.New(this, queuedHits);
            BeatAction.New(this, cheerActions);
        }

        private List<BeatAction.Action> CreateCheerPattern(double beat, float spanInBeats, int pattern, bool isTurnover)
        {
            List<BeatAction.Action> cheerActions = new();

            if (pattern < 1) return cheerActions;

            int animCount = isTurnover ? (int)spanInBeats : (int)spanInBeats - 1;

            var CurrPattern = pattern switch {
                    1 => DrummerDuel.instance.CheerPattern1,
                    2 => DrummerDuel.instance.CheerPattern2,
                    3 => DrummerDuel.instance.CheerPattern3,
                    4 => DrummerDuel.instance.CheerPattern4,
                    _ => DrummerDuel.instance.CheerPattern1,
                };
            
            for (int i = 0; i < animCount; i++)
            {
                var cheerAnim = CurrPattern[i % 4]; //rip i2 o7
                var cheerBeat = beat + (double)i;
                double cheerSide = isTurnover ? 1 : -1;
    
                //Debug.Log($"isTurnover: {isTurnover},\ni: {i},\ncheerAnim: {cheerAnim}\nbeat: {beat},\n spanInBeats: {spanInBeats},\ni={i}, cheerBeat: {beat + spanInBeats + i}");
                cheerActions.AddRange(CreateCheerAction(cheerBeat, cheerAnim, 0.5f, cheerSide));
            }
            return cheerActions;
        }

        private List<BeatAction.Action> CreateCheerAction(double beat, string animation, float speed, double side)
        {
            return new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate 
                {
                    var cheerLeaderSide = (side >= 0) ? cheerLeadersRight : cheerLeadersLeft;
                    
                    DoAllCheerleaderAnimations(animation, cheerLeaderSide, 0.5f);
                })
            };
        }

        private void DoAllCheerleaderAnimations(string animName, Animator[] cheerLeaderSide, float speed)
        {
            for (int i = 0; i < cheerLeaderSide.Length; i++)
            {
                string currAnim = animName;
                if (animName == "Cheer1M")
                {
                    currAnim = i switch {
                        0 => "Cheer1R",
                        1 => "Cheer1M",
                        2 => "Cheer1L",
                        _ => "Idle",
                    };
                }
                cheerLeaderSide[i].DoScaledAnimationAsync(currAnim, speed);
            }
        }

        private string GetHitSound(double beat, int hitSound, double nextBeat) 
        {
            if (hitSound == 0)
            {
                if ((beat) % 1 != 0 )
                    {
                        hitSound = 3;    // lol idk what any of these beat values mean and i cant bother to investigate
                    } else if ( nextBeat - beat < 1 )
                    {
                        hitSound = 2;
                    } else {
                        hitSound = 1;
                    }
            }

            var hitVoice = hitSound switch {
                1 => "drummerDon",
                2 => "drummerDo",
                3 => "drummerKo",
                _ => "drummerDon",
            };
            return hitVoice;
        }

        public void DrumHit(double beat, bool isPlayer)
        {
            isRight = !isRight;
        
            var drummerAnim = (isPlayer ? drummerRight : drummerLeft);
            var taikoAnim = (isPlayer ? taikoRight : taikoLeft);
            var isFront = (isPlayer ? !isRight : !isRight);

            drummerAnim.DoScaledAnimationAsync("Hit", 0.5f);
            taikoAnim.DoScaledAnimationAsync("Hit", 0.5f);

            if (allDon)
            {
                if (isFront)
                {
                    drummerAnim.DoScaledAnimationAsync($"{(isAngry ? "ArmMadF" : "ArmF")}", 0.5f);
                }
                else
                {
                    drummerAnim.DoScaledAnimationAsync($"{(isAngry ? "ArmMadB" : "ArmB")}", 0.5f);
                }
            }
            else if (!isPlayer)
            {
                if (isAngry)
                {
                    drummerAnim.DoScaledAnimationAsync($"{(beat % 1.0 == 0.0 ? "ArmMadF" : "ArmMadB")}", 0.5f);
                }
                else
                {
                    drummerAnim.DoScaledAnimationAsync($"{(beat % 1.0 == 0.0 ? "ArmF" : "ArmB")}", 0.5f);
                }
            }
        }

        public void DrummerSuccess(double beat, bool successVoice)
        {
            BeatAction.New(DrummerDuel.instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    if (!DrummerDuel.instance.hasMissed )
                    {
                        DrummerDuel.instance.referee.DoScaledAnimationAsync("HeadGood", 0.5f);
                        BeatAction.New(DrummerDuel.instance, CreateCheerAction(beat, "Just", 0.5f, 1));
                    }
                    else {
                        DrummerDuel.instance.referee.DoScaledAnimationAsync("HeadBad", 0.5f);
                        BeatAction.New(DrummerDuel.instance, CreateCheerAction(beat + 0.5, "Miss", 0.5f, 1));
                    }
                    DrummerDuel.instance.referee.DoScaledAnimationAsync("Prepare", 0.5f);
                }),
                new BeatAction.Action(beat + 1, delegate
                {
                    DrummerDuel.instance.isDrumming = false;
                }),
                new BeatAction.Action(beat, delegate
                {
                    if (successVoice) {
                        if (DrummerDuel.instance.hasMissed) {
                            SoundByte.PlayOneShotGame("drummerDuel/passToLeftBad", beat - 1);
                            SoundByte.PlayOneShotGame("drummerDuel/miss", beat + 0.5);
                        } 
                        else {
                            SoundByte.PlayOneShotGame("drummerDuel/passToLeft", beat - 1);
                        }
                    }
                })
            });

        }

        public static void PrePassTurn(double beat)
        {
            if (GameManager.instance.currentGame == "drummerDuel")
            {
                instance.PassTurnStandalone(beat);
            }
            else
            {
                passedTurns.Add(beat);
            }
        }

        public void PassTurnStandalone(double beat)
        {
            RiqEntity lastInterval = GetLastIntervalBeforeBeat(beat);
            float length = EventCaller.GetAllInGameManagerList("drummerDuel", new string[] { "drummer turnover" }).Find(x => x.beat == beat).length;
            if (lastInterval == null) return;
            DrummerTurnOver(beat, lastInterval.beat, lastInterval.beat + lastInterval.length, length);
        }

        private void DrummerTurnOver(double beat, double intervalBeat, double endBeat, float length)
        {
            isRight = true;

            var inputs = GetAllSpeaksInBetweenBeat(intervalBeat, endBeat);
            inputs.Sort((x, y) => x.beat.CompareTo(y.beat));
            List<BeatAction.Action> voiceActions = new();
            List<BeatAction.Action> cheerActions = new();

            for (int i = 0; i < inputs.Count; i++)
            {
                var input = inputs[i];
                double relativeBeat = input.beat - intervalBeat;
                double inputHitBeat = beat + length + relativeBeat;
                
                double nextBeat = double.MaxValue;
                if (i != inputs.Count - 1)
                {
                    nextBeat = inputs[i+1].beat - intervalBeat + beat + length;
                }
                var hitVoice = GetHitSound(inputHitBeat, input["hitsound"], nextBeat);

                //Debug.Log($"i: {i} hitVoice: {hitVoice}");
                if (hitVoice == "drummerKo")
                {
                    ScheduleInput(beat, length + relativeBeat, InputAction_Press, DrummerJustOffbeat, DrummerMiss, Empty);
                }
                else
                {
                    ScheduleInput(beat, length + relativeBeat, InputAction_Press, DrummerJustOnbeat, DrummerMiss, Empty);
                }

                voiceActions.Add(new BeatAction.Action(inputHitBeat, delegate
                {
                    if (GameManager.instance.currentGame == "drummerDuel")
                    {
                        SoundByte.PlayOneShotGame($"drummerDuel/{hitVoice}");
                    }
                }));
            }

            //cam actions
            if (isAutoCamEnabled)
            {
                autoPassTurnEndBeat = beat + autoCamLength;

                BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate
                    {
                        MoveCam(beat, autoCamLength, (int)CameraLocation.Right, defaultEase, true);
                    }),
                    new BeatAction.Action(beat + autoCamLength, delegate
                    {
                        cameraX = cameraRight;
                        GameCamera.AdditionalPosition = new Vector3((cameraX * -1), 0, 0);
                        isAutoCamCurrMoving = false;
                        passTurnBeat = double.MaxValue;
                    })
                });
            }

            // referee/drummer actions
            SoundByte.PlayOneShotGame("drummerDuel/passToRight", beat);
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    referee.DoScaledAnimationAsync("Right", 0.5f);
                    hasMissed = false;
                    isDrumming = true;
                }),
            });

            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("drummerDuel", new string[] { "beat intervals" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat));
                var lastEvent = allEventsBeforeBeat[^1];

                var pattern = lastEvent["pattern"];
                var successVoice = lastEvent["successvoice"]; 
                var intervalLength = beat - intervalBeat;
                
                instance.DrummerSuccess(endBeat + intervalLength, successVoice);

                float originalIntervalLength = (float)(endBeat - intervalBeat);
                cheerActions = CreateCheerPattern(beat + length, originalIntervalLength, pattern, true);

                if (cheerActions.Count > 0)
                {
                    //Debug.Log($"Passed if. Creating {cheerActions.Count} turnover cheer actions");
                    BeatAction.New(this, cheerActions);
                }
                if (voiceActions.Count > 0)
                {
                    BeatAction.New(this, voiceActions);
                }
            }

        }

        private void DrummerJustOnbeat(PlayerActionEvent caller, float state)
        {
            DrummerJust(state, false);
        }

        private void DrummerJustOffbeat(PlayerActionEvent caller, float state)
        {
            DrummerJust(state, true);
        }

        private void RestoreColorsFromWhiff()
        {
            if (isWhiffing)
            {
                isWhiffing = false;
                ApplyDrummerHeadColors();
            }
        }

        private void DrummerJust(float state, bool front)
        {
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("drummerDuel/nearMiss");
                drummerRight.DoScaledAnimationAsync("FaceWhiff", 0.5f);
                taikoRight.DoScaledAnimationAsync("Whiff", 0.5f);

                if (isAngry)
                {
                    isWhiffing = true;
                    ApplyDrummerHeadColors();
                }
            }
            else
            {
                if (isAngry) { drummerRight.DoScaledAnimationAsync("FaceAngry", 0.5f); }
                else { drummerRight.DoScaledAnimationAsync("FaceIdle", 0.5f); }

                taikoRight.DoScaledAnimationAsync("Hit", 0.5f);
            }
            SoundByte.PlayOneShotGame("drummerDuel/drumRightHit");

            if (allDon)
            {
                isRight = !isRight;
                DrumHit(Conductor.instance.songPositionInBeatsAsDouble, true);
            }
            else
            {
                drummerRight.DoScaledAnimationAsync("Hit", 0.5f);
                if (isAngry)
                {
                    drummerRight.DoScaledAnimationAsync($"{(front ? "ArmMadF" : "ArmMadB")}", 0.5f);
                }
                else
                {
                    drummerRight.DoScaledAnimationAsync($"{(front ? "ArmF" : "ArmB")}", 0.5f);
                }
            }
        }

        private void DrummerMiss(PlayerActionEvent caller)
        {
            hasMissed = true;
        }

        private void Empty(PlayerActionEvent caller)
        {

        }

    }
}