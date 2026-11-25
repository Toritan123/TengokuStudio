using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlWorkingDoughLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("workingDough", "Working Dough", "808080", false, false, new List<GameAction>()
            {
                new GameAction("beat intervals", "Start Interval")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; WorkingDough.PreSetIntervalStart(e.beat, e.length, e["auto"]);  },
                    defaultLength = 8f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("auto", true, "Auto Pass Turn", "Toggle if the turn should be passed automatically at the end of the start interval.")
                    }
                },
                new GameAction("small ball", "Small Ball")
                {
                    defaultLength = 0.5f,
                    priority = 1,
                },
                new GameAction("big ball", "Big Ball")
                {
                    defaultLength = 0.5f,
                    priority = 1,
                    parameters = new List<Param>()
                    {
                        new Param("hasGandw", false, "Mr. Game & Watch", "Toggle if Mr. Game & Watch should be riding on the ball."),
                        new Param("flashColor", Color.white, "Flash Color", "Set the color of the flash.")
                    }
                },
                new GameAction("passTurn", "Pass Turn")
                {
                    preFunction = delegate { WorkingDough.PrePassTurn(eventCaller.currentEntity.beat); },
                    preFunctionLength = 1
                },
                new GameAction("rise spaceship", "Rise Up Spaceship")
                {
                    function = delegate { var e = eventCaller.currentEntity; WorkingDough.instance.RiseUpShip(e.beat, e.length);  },
                    defaultLength = 8f,
                    resizable = true,
                    priority = 0
                },
                new GameAction("launch spaceship", "Launch Spaceship")
                {
                    function = delegate { var e = eventCaller.currentEntity; WorkingDough.instance.LaunchShip(e.beat, e.length);  },
                    defaultLength = 8f,
                    resizable = true,
                    priority = 0
                },
                new GameAction("lift dough dudes", "Lift Dough Dudes")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity; 
                        if (e["instant"])
                            WorkingDough.instance.InstantElevation(e["toggle"]);
                        else
                            WorkingDough.instance.Elevate(e.beat, e.length, e["toggle"]);  
                    },
                    defaultLength = 8f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Up", "Toggle if the dough dudes should go up or down."),
                        new Param("instant", false, "Instant", "Toggle to make it happen instantly.")
                    },
                    resizable = true,
                    priority = 0
                },
                new GameAction("mr game and watch enter or exit", "Mr. G&W Enter or Exit")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity; 
                        if (e["instant"])
                            WorkingDough.instance.InstantGANDWEnterOrExit(e["toggle"]);
                        else
                            WorkingDough.instance.GANDWEnterOrExit(e.beat, e.length, e["toggle"]);  
                    },
                    defaultLength = 8f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Exit", "Toggle if Mr. Game & Watch should exit or enter the scene."),
                        new Param("instant", false, "Instant", "Toggle to make it happen instantly.")
                    },
                    resizable = true,
                    priority = 0
                },
                new GameAction("disableBG", "Toggle Background")
                {
                    function = delegate { WorkingDough.instance.DisableBG(eventCaller.currentEntity["ship"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("ship", false, "Spaceship Only", "Toggle if the only the spaceship should be toggled.")
                    }
                },
                new GameAction("bgcolor", "Background Appearance")
                {
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        WorkingDough.instance.BackgroundColor(e.beat, e.length, e["colorStart"], e["colorEnd"], e["ease"]);
                    },
                    defaultLength = 4,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("colorStart", Color.black, "Start Color", "Set the color at the start of the event."),
                        new Param("colorEnd", Color.black, "End Color", "Set the color at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    }
                }
            },
            new List<string>() {"rvl", "repeat"},
            "rvldough", "en",
            new List<string>() {},
            chronologicalSortKey: 11,
            font : "FOT-SlumpStd-DB SDF"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Jukebox;
    using Scripts_WorkingDough;

    public class WorkingDough : Minigame
    {
        [Header("Components")]
        [SerializeField] GameObject npcImpact;
        public GameObject playerImpact;
        [SerializeField] GameObject smallBallNPC;
        [SerializeField] GameObject bigBallNPC;
        [SerializeField] Transform ballHolder;
        [SerializeField] public Animator arrowAnimLeftNPC;
        [SerializeField] public Animator arrowAnimRightNPC;
        [SerializeField] public Animator arrowAnimLeftPlayer;
        [SerializeField] public Animator arrowAnimRightPlayer;
        [SerializeField] GameObject NPCBallTransporters;
        [SerializeField] GameObject PlayerBallTransporters;
        [SerializeField] GameObject playerEnterSmallBall;
        [SerializeField] GameObject playerEnterBigBall;
        public GameObject missImpact;
        public Transform breakParticleHolder;
        public GameObject breakParticleEffect;
        [SerializeField] Animator conveyerAnimator;
        [SerializeField] GameObject smallBGBall;
        [SerializeField] GameObject bigBGBall;
        [SerializeField] Animator spaceshipAnimator;
        [SerializeField] Animator spaceshipHolder;
        [SerializeField] Animator doughDudesHolderAnim;
        [SerializeField] Animator gandwAnim;
        [SerializeField] public Animator playerBigAnim;
        [SerializeField] public Animator playerSmallAnim;
        [SerializeField] Animator npcBigAnim;
        [SerializeField] Animator npcSmallAnim;
        [SerializeField] Animator coverAnim;
        [SerializeField] Transform smokeHolder;

        public SpriteRenderer backgroundSR;
        public SpriteRenderer flashSR;

        [SerializeField] Animator bgAnim;
        [SerializeField] private GameObject[] bgObjects;
        [SerializeField] private GameObject shipObject;
        private bool bgDisabled;

        [Header("Variables")]
        float risingLength = 4f;
        double risingStartBeat;
        float liftingLength = 4f;
        double liftingStartBeat;
        float gandMovingLength = 4f;
        double gandMovingStartBeat;
        public bool bigMode;
        public bool bigModePlayer;
        static List<double> passedTurns = new List<double>();
        public bool spaceshipRisen = false;
        public bool spaceshipRising = false;
        bool liftingDoughDudes;
        string liftingAnimName;
        bool gandwHasEntered = true;
        bool gandwMoving;
        string gandwMovingAnimName;
        public bool gandwLeverIsUp = false;
        public bool isGandwMissState = false;
        private double gandwMissEndBeat = -1;
        

        public bool spaceshipLaunching = false;
        float startShipY = -7.18236f;
        float targetShipY = -7.18236f;
        int depositedBalls = 0;
        float launchingStartY;
        double launchingStartBeat;
        float launchingLength;
        float risingStartY;

        [Header("Curves")]
        [SerializeField] SuperCurveObject.Path[] ballBouncePaths;
        new void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            foreach (SuperCurveObject.Path path in ballBouncePaths)
            {
                if (path.preview)
                {
                    smallBallNPC.GetComponent<NPCDoughBall>().DrawEditorGizmo(path);
                }
            }
        }
        public SuperCurveObject.Path GetPath(string name)
        {
            foreach (SuperCurveObject.Path path in ballBouncePaths)
            {
                if (path.name == name)
                {
                    return path;
                }
            }
            return default(SuperCurveObject.Path);
        }

        [Header("Resources")]
        public Sprite whiteArrowSprite;
        public Sprite redArrowSprite;

        public static WorkingDough instance;

        const int IA_AltPress = IAMAXCAT;
        protected static bool IA_TouchNrmPress(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && !instance.IsExpectingInputNow(InputAction_Alt);
        }

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

        public static PlayerInput.InputAction InputAction_Nrm =
            new("RvlDoughAlt", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_PadBasicPress, IA_TouchNrmPress, IA_BatonBasicPress);
        public static PlayerInput.InputAction InputAction_Alt =
            new("RvlDoughAlt", new int[] { IA_AltPress, IA_AltPress, IA_AltPress },
            IA_PadAltPress, IA_TouchAltPress, IA_BatonAltPress);

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            conveyerAnimator.Play("ConveyerBelt", 0, 0);
            doughDudesHolderAnim.Play("OnGround", 0, 0);
            
            if (spaceshipAnimator != null) 
            {
                spaceshipAnimator.transform.localPosition = new Vector3(spaceshipAnimator.transform.localPosition.x, startShipY, spaceshipAnimator.transform.localPosition.z);
            }
        }

        private bool shipOnly;

        public void DisableBG(bool ship)
        {
            shipOnly = ship;
            bgDisabled = !bgDisabled;

            foreach (var bgObject in bgObjects)
            {
                bgObject.SetActive(!bgDisabled || shipOnly);
            }

            if (ship) 
            {
                bgAnim.DoScaledAnimationAsync("bun_01_bg_02");
            }
            else
            {
                bgAnim.DoScaledAnimationAsync("bun_01_bg_02_tutorial");
            }

            shipObject.SetActive(!bgDisabled && !shipOnly);
        }

        private List<RiqEntity> GetAllBallsInBetweenBeat(double beat, double endBeat)
        {
            List<RiqEntity> ballEvents = EventCaller.GetAllInGameManagerList("workingDough", new string[] { "small ball", "big ball" });
            List<RiqEntity> tempEvents = ballEvents.FindAll(x => x.beat >= beat && x.beat < endBeat);
            tempEvents.Sort((x, y) => x.beat.CompareTo(y.beat));
            return tempEvents;
        }

        private RiqEntity GetLastIntervalBeforeBeat(double beat)
        {
            List<RiqEntity> intervalEvents = EventCaller.GetAllInGameManagerList("workingDough", new string[] { "beat intervals" });
            if (intervalEvents.Count == 0) return null;
            var tempEvents = intervalEvents.FindAll(x => x.beat <= beat);
            tempEvents.Sort((x, y) => x.beat.CompareTo(y.beat));
            return tempEvents[^1];
        }

        public void SetIntervalStart(double beat, double gameSwitchBeat, float interval = 8f, bool autoPassTurn = true)
        {
            List<RiqEntity> relevantBalls = GetAllBallsInBetweenBeat(beat, beat + interval);
            bool hasBigBall = false;
            foreach (var ball in relevantBalls)
            {
                bool isBig = ball.datamodel == "workingDough/big ball";
                if (ball.beat >= gameSwitchBeat)
                {
                    SpawnBall(ball.beat - 1, isBig, isBig && ball["hasGandw"]);
                    OnSpawnBall(ball.beat, isBig);
                }
                if (isBig) hasBigBall = true;
            }
            if (autoPassTurn)
            {
                PassTurn(beat + interval, interval, beat);
            }
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 1, delegate
                {
                    bigMode = hasBigBall;

                    string openAnimNPC = bigMode ? "bun_00_box_open_r_large" : "bun_00_box_open_r_small";
                    
                    foreach (Transform child in NPCBallTransporters.transform)
                    {
                        Animator anim = child.GetComponent<Animator>();
                        if (anim != null) anim.Play(openAnimNPC, 0, 0);
                    }

                    instance.gandwLeverIsUp = true;
                    instance.UpdateGandwVisuals();
                }),
            });
        }

        public static void PrePassTurn(double beat)
        {
            if (GameManager.instance.currentGame == "workingDough")
            {
                instance.PassTurnStandalone(beat);
            }
            else
            {
                passedTurns.Add(beat);
            }
        }

        private void PassTurnStandalone(double beat)
        {
            RiqEntity lastInterval = GetLastIntervalBeforeBeat(beat);
            if (lastInterval == null) return;
            PassTurn(beat, lastInterval.length, lastInterval.beat);
        }

        private void PassTurn(double beat, double length, double startBeat)
        {
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 1, delegate
                {
                    var relevantBallEvents = GetAllBallsInBetweenBeat(startBeat, startBeat + length);
                    bigModePlayer = false;

                    if (relevantBallEvents.Count > 0)
                    {
                        foreach (var ball in relevantBallEvents)
                        {
                            double relativeBeat = ball.beat - startBeat;
                            bool isBig = ball.datamodel == "workingDough/big ball";
                            SpawnPlayerBall(beat + relativeBeat - 1, isBig, isBig ? ball["flashColor"] : Color.white, isBig && ball["hasGandw"]);
                            if (isBig) bigModePlayer = true;
                        }
                    }

                    string openAnim = bigModePlayer ? "bun_00_box_open_r_large" : "bun_00_box_open_r_small";
                    
                    foreach (Transform child in PlayerBallTransporters.transform)
                    {
                        Animator anim = child.GetComponent<Animator>();
                        if (anim != null) anim.Play(openAnim, 0, 0);
                    }
                }),
                new BeatAction.Action(beat - 1, delegate
                {
                     instance.gandwLeverIsUp = false;
                     instance.UpdateGandwVisuals();
                }),
                new BeatAction.Action(beat, delegate 
                { 
                    if (beat + 1 > GetLastIntervalBeforeBeat(beat + 1).beat + GetLastIntervalBeforeBeat(beat + 1).length) 
                    {
                        string closeAnimNPC = bigMode ? "bun_00_box_close_r_large" : "bun_00_box_close_r_small";
                        foreach (Transform child in NPCBallTransporters.transform)
                        {
                            Animator anim = child.GetComponent<Animator>();
                            if (anim != null) anim.Play(closeAnimNPC, 0, 0);
                        }
                    }
                    if (bigMode)
                    {
                        bigMode = false;
                    }
                }),
                new BeatAction.Action(beat + length, delegate 
                {
                    string closeAnimPlayer = bigModePlayer ? "bun_00_box_close_r_large" : "bun_00_box_close_r_small";
                    foreach (Transform child in PlayerBallTransporters.transform)
                    {
                        Animator anim = child.GetComponent<Animator>();
                        if (anim != null) anim.Play(closeAnimPlayer, 0, 0);
                    }

                    if (bigModePlayer)
                    {
                        bigModePlayer = false;
                    }
                }),
            });
        }

        public void SpawnBall(double beat, bool isBig, bool hasGandw)
        {
            var objectToSpawn = isBig ? bigBallNPC : smallBallNPC;
            var spawnedBall = GameObject.Instantiate(objectToSpawn, ballHolder);

            var ballComponent = spawnedBall.GetComponent<NPCDoughBall>();
            spawnedBall.SetActive(true);
            ballComponent.Init(beat, hasGandw);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { arrowAnimLeftNPC.Play("bun_00_arrow_out", 0, 0); }),
                new BeatAction.Action(beat + 1f, delegate { 
                    if (isBig)
                    {
                        npcBigAnim.DoScaledAnimationAsync("bun_00_bun_jump_L", 0.5f);
                        if (!npcSmallAnim.GetCurrentAnimatorStateInfo(0).IsName("bun_00_bun_wait_S"))
                            npcSmallAnim.DoScaledAnimationAsync("bun_00_bun_wait_S", 0.5f);
                    }
                    else
                    {
                        npcSmallAnim.DoScaledAnimationAsync("bun_00_bun_jump_S", 0.5f);
                        if (!npcBigAnim.GetCurrentAnimatorStateInfo(0).IsName("bun_00_bun_wait_L"))
                            npcBigAnim.DoScaledAnimationAsync("bun_00_bun_wait_L", 0.5f);
                    }
                }),
                new BeatAction.Action(beat + 1f, delegate { npcImpact.SetActive(true); }),
                new BeatAction.Action(beat + 1.1f, delegate { npcImpact.SetActive(false); }),
                new BeatAction.Action(beat + 1.9f, delegate { arrowAnimRightNPC.Play("bun_00_arrow_out", 0, 0); }),
            });
        }

        public void OnSpawnBall(double beat, bool isBig)
        {
            SoundByte.PlayOneShotGame(isBig ? "workingDough/hitBigOther" : "workingDough/hitSmallOther", beat);
            SoundByte.PlayOneShotGame(isBig ? "workingDough/bigOther" : "workingDough/smallOther", beat);
        }

        public void SpawnPlayerBall(double beat, bool isBig, Color flash, bool hasGandw)
        {
            var objectToSpawn = isBig ? playerEnterBigBall : playerEnterSmallBall;
            var spawnedBall = GameObject.Instantiate(objectToSpawn, ballHolder);

            var ballComponent = spawnedBall.GetComponent<PlayerEnterDoughBall>();
            spawnedBall.SetActive(true);
            ballComponent.Init(beat, isBig, flash, hasGandw);

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { arrowAnimLeftPlayer.Play("bun_00_arrow_out", 0, 0); })
            });
        }

        private struct QueuedInterval
        {
            public double beat;
            public float interval;
            public bool auto;
        }
        private static List<QueuedInterval> queuedIntervals = new();

        public static void PreSetIntervalStart(double beat, float interval, bool auto)
        {
            if (GameManager.instance.currentGame == "workingDough")
            {
                instance.SetIntervalStart(beat, beat, interval, auto);
            }
            else
            {
                queuedIntervals.Add(new QueuedInterval
                {
                    beat = beat,
                    interval = interval,
                    auto = auto
                });
            }
        }
        private ColorEase bgColorEase = new(Color.black);
        private ColorEase flashColorEase = new Minigame.ColorEase(new Color(1f, 1f, 1f, 0f));

        private void BackgroundColorUpdate()
        {
            backgroundSR.color = bgColorEase.GetColor();
            flashSR.color = flashColorEase.GetColor();
        }

        public void FlashColor(double beat, Color color1, Color color2)
        {
            flashColorEase = new ColorEase(beat, 0.5f, color1, color2, 0);
        }

        public void BackgroundColor(double beat, float length, Color colorStartSet, Color colorEndSet, int ease)
        {
            bgColorEase = new ColorEase(beat, length, colorStartSet, colorEndSet, ease);
        }

        private void PersistColor(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("workingDough", new string[] { "bgcolor" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); 
                var lastEvent = allEventsBeforeBeat[^1];
                BackgroundColor(lastEvent.beat, lastEvent.length, lastEvent["colorStart"], lastEvent["colorEnd"], lastEvent["ease"]);
            }
        }
        
        public override void OnGameSwitch(double beat)
        {
            if (Conductor.instance.isPlaying && !Conductor.instance.isPaused)
            {
                if (queuedIntervals.Count > 0)
                {
                    foreach (var interval in queuedIntervals)
                    {
                        SetIntervalStart(interval.beat, beat, interval.interval, interval.auto);
                    }
                    queuedIntervals.Clear();
                }
            }
            PersistColor(beat);
        }

        public override void OnPlay(double beat)
        {
            queuedIntervals.Clear();
            passedTurns.Clear();
            PersistColor(beat);

            depositedBalls = 0;
            spaceshipRisen = false;
            spaceshipRising = false;
            spaceshipLaunching = false;
            
            targetShipY = startShipY; 
            if (coverAnim != null && spaceshipHolder != null)
                {coverAnim.transform.SetParent(spaceshipHolder.transform, true);}
            if (spaceshipAnimator != null)
                spaceshipAnimator.transform.localPosition = new Vector3(spaceshipAnimator.transform.localPosition.x, startShipY, spaceshipAnimator.transform.localPosition.z);
        }

        void Update()
        {
            if (isGandwMissState && Conductor.instance.songPositionInBeatsAsDouble >= gandwMissEndBeat)
            {
                isGandwMissState = false;
                UpdateGandwVisuals();
            }

            if (liftingDoughDudes && !bgDisabled) doughDudesHolderAnim.DoScaledAnimation(liftingAnimName, liftingStartBeat, liftingLength);
            if (gandwMoving && !bgDisabled) gandwAnim.DoScaledAnimation(gandwMovingAnimName, gandMovingStartBeat, gandMovingLength);
            
            if (spaceshipAnimator != null && !bgDisabled)
            {
                if (spaceshipRising)
                {
                    float normalizedBeat = (float)(Conductor.instance.songPositionInBeatsAsDouble - risingStartBeat) / risingLength;
                    normalizedBeat = Mathf.Clamp01(normalizedBeat);
                    float newY = Mathf.Lerp(risingStartY, -1.36f, normalizedBeat);
                    spaceshipAnimator.transform.localPosition = new Vector3(spaceshipAnimator.transform.localPosition.x, newY, spaceshipAnimator.transform.localPosition.z);
                }
                else if (spaceshipLaunching)
                {
                    float normalizedBeat = (float)(Conductor.instance.songPositionInBeatsAsDouble - launchingStartBeat) / launchingLength;
                    normalizedBeat = Mathf.Clamp01(normalizedBeat);
                    
                    float newY = Mathf.Lerp(launchingStartY, 100f, normalizedBeat);
                    
                    if (spaceshipAnimator != null)
                        spaceshipAnimator.transform.localPosition = new Vector3(spaceshipAnimator.transform.localPosition.x, newY, spaceshipAnimator.transform.localPosition.z);
                }
                else if (!spaceshipRisen)
                {
                    float currentY = spaceshipAnimator.transform.localPosition.y;
                    float newY = Mathf.Lerp(currentY, targetShipY, Time.deltaTime * 5f);
                    spaceshipAnimator.transform.localPosition = new Vector3(spaceshipAnimator.transform.localPosition.x, newY, spaceshipAnimator.transform.localPosition.z);
                }
            }

            if (passedTurns.Count > 0)
            {
                foreach (var passTurn in passedTurns)
                {
                    PassTurnStandalone(passTurn);
                }
                passedTurns.Clear();
            }
            if (PlayerInput.GetIsAction(InputAction_Nrm) && !IsExpectingInputNow(InputAction_Nrm))
            {
                playerSmallAnim.DoScaledAnimationAsync("bun_00_bun_jump_S", 0.5f);
                
                if (!playerBigAnim.GetCurrentAnimatorStateInfo(0).IsName("bun_00_bun_wait_L"))
                    playerBigAnim.DoScaledAnimationAsync("bun_00_bun_wait_L", 0.5f);
                    
                SoundByte.PlayOneShotGame("workingDough/smallPlayer");
            }
            else if (PlayerInput.GetIsAction(InputAction_Alt) && !IsExpectingInputNow(InputAction_Alt))
            {
                playerBigAnim.DoScaledAnimationAsync("bun_00_bun_jump_L", 0.5f);
                
                if (!playerSmallAnim.GetCurrentAnimatorStateInfo(0).IsName("bun_00_bun_wait_S"))
                    playerSmallAnim.DoScaledAnimationAsync("bun_00_bun_wait_S", 0.5f);
                    
                SoundByte.PlayOneShotGame("workingDough/bigPlayer");
            }
            BackgroundColorUpdate();
        }

        public void SpawnBGBall(double beat, bool isBig, bool hasGandw)
        {
            var objectToSpawn = isBig ? bigBGBall : smallBGBall;
            var spawnedBall = GameObject.Instantiate(objectToSpawn, ballHolder);

            var ballComponent = spawnedBall.GetComponent<BGBall>();
            spawnedBall.SetActive(true);
            ballComponent.Init(beat, hasGandw);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 9f, delegate { 
                    if (!spaceshipRisen && !bgDisabled) 
                    {
                        spaceshipAnimator.Play("bun_01_robo_ball_in", 0, 0); 
                        
                        depositedBalls++;
                     
                        float incrementAmount = (-6.47f - startShipY) / 123f;
                        targetShipY = startShipY + (incrementAmount * depositedBalls);
                    }
                }),
                 new BeatAction.Action(beat + 9f, delegate { if (!spaceshipRisen && !bgDisabled) coverAnim.Play("bun_01_robo_cover", 0, 0); }),
            });
        }

        public void InstantElevation(bool isUp)
        {
            doughDudesHolderAnim.Play(isUp ? "InAir" : "OnGround", 0, 0);
        }

        public void Elevate(double beat, float length, bool isUp)
        {
            liftingAnimName = isUp ? "LiftUp" : "LiftDown";
            liftingStartBeat = beat;
            liftingLength = length;
            liftingDoughDudes = true;
            doughDudesHolderAnim.DoScaledAnimation(liftingAnimName, liftingStartBeat, liftingLength);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length - 0.1f, delegate { liftingDoughDudes = false; }),
            });
        } 

        public void LaunchShip(double beat, float length)
        {
            if (bgDisabled) return;
            spaceshipRisen = true;
            spaceshipAnimator.Play("bun_01_robo_vibe1", 0, 0);
            
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + (length / 2f), delegate { spaceshipAnimator.Play("bun_01_robo_vibe2", 0, 0); }),
                
                new BeatAction.Action(beat + length, delegate { 
                    spaceshipLaunching = true;
                    launchingStartBeat = beat + length;
                    launchingLength = 4f; 
                    if (spaceshipAnimator != null) launchingStartY = spaceshipAnimator.transform.localPosition.y;
                    
                    SoundByte.PlayOneShotGame("workingDough/LaunchRobot");
                }),
                
                new BeatAction.Action(beat + length + 4f, delegate { spaceshipLaunching = false; })
            });
        }

        public void RiseUpShip(double beat, float length)
        {
            if (bgDisabled) return;
            spaceshipRisen = true;
            spaceshipRising = true;
            risingLength = length;
            risingStartBeat = beat;
            
            if (spaceshipAnimator != null) risingStartY = spaceshipAnimator.transform.localPosition.y;

            spaceshipAnimator.Play("bun_01_robo_ready", 0, 0); 
            coverAnim.Play("bun_01_robo_cover_break", 0, 0); 

            if (coverAnim != null) coverAnim.transform.SetParent(this.transform, true);
            // This probably isnt accurate #HoleInOneGrass
            List<BeatAction.Action> riseActions = new List<BeatAction.Action>();

            if (smokeHolder != null)
            {
                Animator[] smokes = smokeHolder.GetComponentsInChildren<Animator>();
                float stretchRatio = length / 8f; 
                float smokeInterval = 2f / (smokes.Length - 1); 

                for (int i = 0; i < smokes.Length; i++)
                {
                    Animator smokeAnim = smokes[i];
                    double triggerBeat = beat + (i * smokeInterval * stretchRatio);
                    
                    riseActions.Add(new BeatAction.Action(triggerBeat, delegate { 
                        smokeAnim.Play("bun_01_smoke", 0, 0); 
                    }));
                }
            }

            riseActions.Add(new BeatAction.Action(beat + length - 0.1f, delegate { 
                spaceshipRising = false; 
                if (spaceshipAnimator != null) spaceshipAnimator.transform.localPosition = new Vector3(spaceshipAnimator.transform.localPosition.x, -1.36f, spaceshipAnimator.transform.localPosition.z);
            }));

            BeatAction.New(instance, riseActions);
        }

        public void UpdateGandwVisuals()
        {
            if (bgDisabled || !gandwHasEntered || gandwMoving) return;

            string animName = isGandwMissState ? "bun_00_boy_miss" : "bun_00_boy";
            
            float frame = gandwLeverIsUp ? 0f : 1f; 

            gandwAnim.Play(animName, 0, frame);
            gandwAnim.speed = 0f; 
        }

        public void TriggerGandWMiss()
        {
            isGandwMissState = true;
            gandwMissEndBeat = Conductor.instance.songPositionInBeatsAsDouble + 3.0;
            UpdateGandwVisuals();
        }
        
        public void GANDWEnterOrExit(double beat, float length, bool shouldExit)
        {
            if (bgDisabled) return;
            gandwMoving = true;
            gandwHasEntered = false;
            gandMovingLength = length;
            gandMovingStartBeat = beat;
            gandwMovingAnimName = shouldExit ? "bun_00_boy_leave" : "bun_00_boy_walk";
            gandwAnim.DoScaledAnimation(gandwMovingAnimName, gandMovingStartBeat, gandMovingLength);
            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length - 0.1f, delegate { gandwMoving = false; }),
                    new BeatAction.Action(beat + length, delegate { 
                        gandwHasEntered = !shouldExit; 
                        if (gandwHasEntered) UpdateGandwVisuals(); 
                    }),
                });
        }

       public void InstantGANDWEnterOrExit(bool shouldExit)
        {
            if (bgDisabled) return;
            gandwHasEntered = !shouldExit;

            if (shouldExit)
            {
                gandwAnim.Play("bun_00_boy_walk", 0, 0f);
                gandwAnim.speed = 0f; 
            }
            else
            {
                gandwLeverIsUp = false; 
                UpdateGandwVisuals();
            }
        }
    }
}