using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using HeavenStudio.Common;

using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlBirdLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("flockStep", "Flock Step", "6bff94", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out FlockStep instance)) {
                            instance.Bop(e.beat, e.length);
                        }
                    },
                    defaultLength = 1.0f,
                    resizable = true,
                },
                new GameAction("beginMarch", "Begin Marching")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out FlockStep instance)) {
                            instance.March(e.beat, e["prepare"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("prepare", true, "Prepare", "Toggle if The Huebirds should do the prepare animation a beat before this event."),
                    },
                    preFunctionLength = 1,
                },
                new GameAction("beginJump", "Jump")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out FlockStep instance)) {
                            instance.Jump(e.beat);
                        }
                    },
                    defaultLength = 4.0f,
                },
                new GameAction("stopMarch", "Stop Marching")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out FlockStep instance)) {
                            instance.Stop(e.beat);
                        }
                    },
                },
                new GameAction("flyTransition", "Flock Fly Away / Land")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out FlockStep instance)) {
                            if (e["fly"]) {
                                instance.FlyAway(e.beat, e.length, e["instant"], e["groupSize"]);
                            } else {
                                instance.Comeback(e.beat, e.length, e["instant"], e["size"], e["groupSize"]);
                            }
                        }
                    },
                    defaultLength = 4.0f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("fly", true, "Fly Away", "Toggle if the birds should fly away instead of landing.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => !(bool)x, new string[] { "size" })
                        }),
                        new Param("instant", false, "All", "Toggle if the entire flock should fly away or land together.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => !(bool)x, new string[] { "groupSize" })
                        }),
                        new Param("size", FlockStep.FlockSize.Medium, "Flock Size", "The size of the flock to land."),
                        new Param("groupSize", new EntityTypes.Integer(1, 80, 15), "Group Size", "How many birds fly/land per group.")
                    }
                },
                
                new GameAction("zoomSeq", "Zoom Sequence")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out FlockStep instance)) {
                            instance.ZoomOut(e.beat);
                        }
                    },
                    defaultLength = 8.0f,
                },
                new GameAction("zoomSeqIndividual", "Set Zoom Segment")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out FlockStep instance)) {
                            instance.SetZoomSegment(e.beat, e["segment"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("segment", FlockStep.ZoomLevel.MainToGlobe1, "Set Zoom Level", "Set which part of the zoom sequence to play."),
                    },
                    defaultLength = 0.5f,
                },
                new GameAction("birdState", "Set Flock Size")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out FlockStep instance)) {
                            instance.BGState((FlockStep.FlockSize)e["state"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("state", FlockStep.FlockSize.Medium, "Flock Size", "Set what size the flock should be set to."),
                    },
                    defaultLength = 0.5f,
                },
                new GameAction("birdSettings", "Huebird Colors")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out FlockStep instance)) {
                            instance.BirdSettings((FlockStep.BirdColors)e["colors"], e["random"], e["mainBirds"], e["bgBirds"], e["instant"], e["feather"], e["belly"], e["beak"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("mainBirds", false, "Color Main Birds?", "Set color to the main 4 birds."),
                        new Param("bgBirds", false, "Color Other Birds?", "Set color to the rest of the flock.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "instant" })
                        }),
                        new Param("instant", true, "Set Color of Current Birds?", "Set color to all birds currently in the flock. If this is disabled, the color will be set for the next birds that fly in."),
                        new Param("random", false, "Randomize Colors", "Toggle to randomly choose between the default colors.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => !(bool)x, new string[] { "colors" })
                        }),
                        new Param("colors", FlockStep.BirdColors.Pink, "Huebird Colors", "Set what color the birds should be.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x == (int)FlockStep.BirdColors.Custom, new string[] { "feather", "belly", "beak" })
                        }),
                        new Param("feather", FlockStep.birdColors[0], "Feather Color", "The color to set the feathers to."),
                        new Param("belly", FlockStep.birdColors[1], "Belly Color", "The color to set the belly to."),
                        new Param("beak", FlockStep.birdColors[2], "Beak Color", "The color to set the beak to."),
                    },
                    defaultLength = 0.5f,
                },
                
                new GameAction("bgColor", "Ground Color")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out FlockStep instance)) {
                            instance.RecolorBG(e["square"], e["grass"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("square", FlockStep.birdColors[15], "Ground Color", "The color to set the ground to."),
                        new Param("grass", FlockStep.birdColors[16], "Grass Color", "The color to set the grass to."),
                    },
                    defaultLength = 0.5f,
                },
            },
            new List<string>() { "rvl", "keep" },
            "rvlbird", "en",
            new List<string>() { },
            chronologicalSortKey: 20,
            font: "Comet"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_FlockStep;
    public class FlockStep : Minigame
    {
        public static FlockStep instance;

        [Header("Logic")]
        [SerializeField] StickyCanvas Background;
        [SerializeField] StickyCanvas ZoomSeq;

        //input logic
        int side = 0;
        int playerSide = 0;
        bool isCpuFacingRight;
        bool isFacingRight;
        bool canRelease;
        bool canJump;
        bool isFlying;
        bool jumpInput;

        //cam logic
        float lastCamPos = 0;
        float camWantPos = 0;
        double camWantBeat = 0;
        float camWantLength = 0;
        float targetCamPos = 0;

        //ktb logic

        double wantMarch = double.MinValue;
        
        List<double> queuedMarch = new();
        bool keepMarching;
        double startNoMarch = double.MinValue;
        double startRegularMarch = double.MinValue;
        double endMarch = double.MinValue;

        //other stuff
        int Migration = 1;
        bool isZoomedOut;

        //enums
        public enum FlockSize
        {
            Small,
            Medium,
            Large,
        }
        public enum BirdColors
        {
            Pink,
            Orange,
            Yellow,
            Green,
            Blue,
            Custom
        }
        public enum ZoomLevel
        {
            MainToGlobe1 = 1,
            MainToGlobe2,
            MainToGlobe3,
            GlobeToBirds1,
            GlobeToBirds2,
            GlobeToBirds3,
            BirdsToMain,
            Main
        }

        [Header("Huebirds")]
        [SerializeField] Huebird[] Huebirds;
        [SerializeField] Huebird HuebirdPlayer;
        [SerializeField] Huebird[] Huebirds2;
        [SerializeField] Huebird[] Huebirds3;
        [SerializeField] Material bgMaterial;
        [SerializeField] SpriteRenderer bgSquare;
        public GameObject otherBirds;
        public GameObject otherBirds2;

        [Header("Animators")]
        [SerializeField] public Animator StageEffect;
        [SerializeField] public Animator SceneHolder;

        private static readonly Dictionary<int, string> birdColorLookup = new()
        {
            //pink
            { 0, "#FF94FF" }, //feather
            { 1, "#FFFFFF" }, //belly
            { 2, "#FFFF5A" }, //beak
            //orange
            { 3, "#FFCE42" }, //feather
            { 4, "#FFFFFF" }, //belly
            { 5, "#FFFF5A" }, //beak
            //yellow
            { 6, "#FFFF00" }, //feather
            { 7, "#FFFFFF" }, //belly
            { 8, "#FFFF5A" }, //beak
            //green
            { 9, "#8CFF52" }, //feather
            { 10, "#FFFFFF" }, //belly
            { 11, "#FFFF5A" }, //beak
            //blue
            { 12, "#6BB5FF" }, //feather
            { 13, "#FFFFFF" }, //belly
            { 14, "#FFFF5A" }, //beak
            //
            { 15, "#6DFF93" }, //square
            { 16, "#5CD77A" }, //grass
        };
        private static List<Color> _birdColors = new List<Color>();
        public static List<Color> birdColors
        {
            get
            {
                if (_birdColors.Count == 0)
                {
                    for (int i = 0; i < birdColorLookup.Count; i++)
                    {
                        Color colorTemp;
                        ColorUtility.TryParseHtmlString(birdColorLookup[i], out colorTemp);
                        _birdColors.Add(colorTemp);
                    }
                }
                return _birdColors;
            }
        }
        
        
        public void OnDestroy()
        {
            if (bgMaterial != null) {
                bgMaterial.SetColor("_ColorAlpha", birdColors[16]);
                bgMaterial.color = birdColors[16];
            }
            
            foreach (var evt in scheduledInputs) //??
            {
                if (evt != null) evt.Disable();
            }
        }

        public void Awake()
        {
            instance = this;
            if (otherBirds != null) Huebirds2 = otherBirds.GetComponentsInChildren<Huebird>(true);
            if (otherBirds2 != null) Huebirds3 = otherBirds2.GetComponentsInChildren<Huebird>(true);

            if (Huebirds != null) foreach (var b in Huebirds) if (b != null) b.Init();
            if (Huebirds2 != null) foreach (var b in Huebirds2) if (b != null) b.Init();
            if (Huebirds3 != null) foreach (var b in Huebirds3) if (b != null) b.Init();
        }

        public override void OnPlay(double beat)
        {
            OnGameSwitch(beat);
        }

        public override void OnGameSwitch(double beat)
        {
            RecolorBG(birdColors[15], birdColors[16]);
            BGState(FlockSize.Medium);

            var entities = GameManager.instance.Beatmap.Entities;
            RiqEntity lastFly = null;
            RiqEntity lastSize = null;
            RiqEntity lastBgColor = null;
            RiqEntity lastSettings = null;

            foreach (var entity in entities)
            {
                if (entity.datamodel == "flockStep/beginMarch")
                {
                    double preFuncBeat = entity.beat - 1.0;
                    if (beat >= preFuncBeat && beat <= entity.beat)
                    {
                        March(entity.beat, entity["prepare"]);
                    }
                }

                if (entity.beat < beat)
                {
                    if (entity.datamodel == "flockStep/flyTransition") lastFly = entity;
                    if (entity.datamodel == "flockStep/birdState") lastSize = entity;
                    if (entity.datamodel == "flockStep/bgColor") lastBgColor = entity;
                    if (entity.datamodel == "flockStep/birdSettings") lastSettings = entity;
                }
            }

            if (lastFly != null && lastSize != null)
            {
                if (lastFly.beat > lastSize.beat)
                {
                    if (beat - lastFly.beat < 5.0)
                    {
                        if (lastFly["fly"]) FlyAway(lastFly.beat, lastFly.length, lastFly["instant"], lastFly["groupSize"]);
                        else Comeback(lastFly.beat, lastFly.length, lastFly["instant"], lastFly["size"], lastFly["groupSize"]);
                    }
                    else
                    {
                        BGState(lastFly["fly"] ? FlockSize.Small : (FlockSize)lastFly["size"]);
                    }
                }
                else
                {
                    BGState((FlockSize)lastSize["state"]);
                }
            }
            else if (lastFly != null)
            {
                if (beat - lastFly.beat < 5.0)
                {
                    if (lastFly["fly"]) FlyAway(lastFly.beat, lastFly.length, lastFly["instant"], lastFly["groupSize"]);
                    else Comeback(lastFly.beat, lastFly.length, lastFly["instant"], lastFly["size"], lastFly["groupSize"]);
                }
                else
                {
                    BGState(lastFly["fly"] ? FlockSize.Small : (FlockSize)lastFly["size"]);
                }
            }
            else if (lastSize != null)
            {
                BGState((FlockSize)lastSize["state"]);
            }
            if (lastBgColor != null) RecolorBG(lastBgColor["square"], lastBgColor["grass"]);
            if (lastSettings != null) BirdSettings((BirdColors)lastSettings["colors"], lastSettings["random"], lastSettings["mainBirds"], lastSettings["bgBirds"], lastSettings["instant"], lastSettings["feather"], lastSettings["belly"], lastSettings["beak"]);
        }

        private void UpdateBirdFly(Huebird b, double currentBeat)
        {
            if (b.flyState == 0 || b.flyState == 3) return;

            float syncedX = b.initialBirdX + (Huebirds[0].bird.localPosition.x - Huebirds[0].initialBirdX);

            if (currentBeat >= b.flyStartBeat)
            {
                float t = (float)((currentBeat - b.flyStartBeat) / b.flyDuration);

                if (b.flyState == 1) // flying state
                {
                    if (t >= 1f)
                    {
                        b.flyState = 3; // hidden state
                        b.gameObject.SetActive(false);
                    }
                    else
                    {
                        float xOffset = -(t * 7f);
                        float yOffset = (t * t) * 20f; 
                        b.bird.localPosition = new Vector3(syncedX + xOffset, b.initialBirdY + yOffset, b.initialBirdZ);
                    }
                }
                else if (b.flyState == 2) // landing state
                {
                    if (t >= 1f)
                    {
                        b.bird.localPosition = new Vector3(syncedX, b.initialBirdY, b.initialBirdZ);
                    }
                    else
                    {
                        float xOffset = ((1f - t) * (1f - t)) * 15f;
                        float yOffset = ((1f - t) * (1f - t)) * 15f;
                        b.bird.localPosition = new Vector3(syncedX + xOffset, b.initialBirdY + yOffset, b.initialBirdZ);
                    }
                }
            }
            else if (b.flyState == 2)
            {
                b.bird.localPosition = new Vector3(syncedX + 5f, b.initialBirdY + 15f, b.initialBirdZ);
            }
        }

        public void Update()
        {
            var cond = Conductor.instance;
            double beat = conductor.unswungSongPositionInBeatsAsDouble;

            int activeBirds = 0;
            if (Huebirds != null) { foreach (var b in Huebirds) { if (b != null && b.gameObject.activeInHierarchy && b.flyState == 0) activeBirds++; } }
            if (Huebirds2 != null) { foreach (var b in Huebirds2) { if (b != null && b.gameObject.activeInHierarchy && b.flyState == 0) activeBirds++; } }
            if (Huebirds3 != null) { foreach (var b in Huebirds3) { if (b != null && b.gameObject.activeInHierarchy && b.flyState == 0) activeBirds++; } }

            if (isZoomedOut) Migration = 3;
            else if (activeBirds >= 120) Migration = 2;
            else if (activeBirds >= 60) Migration = 1;
            else Migration = 0;

            if (Huebirds2 != null) { foreach (var b in Huebirds2) UpdateBirdFly(b, beat); }
            if (Huebirds3 != null) { foreach (var b in Huebirds3) UpdateBirdFly(b, beat); }

            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress) 
                && !(IsExpectingInputNow(InputAction_AltPress) && PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch))
            {
                PlayBirdAnim("birds_i_miss_front", false, true, false);
                if (Huebirds.Length > 1) Huebirds[1].TriggerMad(beat);
                PlayBumpSound();
                playerSide++;

                canRelease = false;
            }
            if (PlayerInput.GetIsAction(InputAction_BasicRelease) 
                && !(IsExpectingInputNow(InputAction_AltRelease) && PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch))
            {
                if (canRelease)
                {
                    if (isFacingRight)
                        {
                            PlayBirdAnim("birds_i_R_walk_00", false, true, false);
                            
                            if (Migration >= 3) SoundByte.PlayOneShotGame("flockStep/reverb/SE_BIRD_FOOT_UP_PLAYER_B_MANY_reverb");
                            else SoundByte.PlayOneShotGame("flockStep/SE_BIRD_FOOT_UP_PLAYER_B");
                        }
                        else
                        {
                            PlayBirdAnim("birds_i_L_walk_00", false, true, false);
                            
                            if (Migration >= 3) SoundByte.PlayOneShotGame("flockStep/reverb/SE_BIRD_FOOT_UP_PLAYER_A_MANY_reverb");
                            else SoundByte.PlayOneShotGame("flockStep/SE_BIRD_FOOT_UP_PLAYER_A");
                        }
                }
            }
            if (PlayerInput.GetIsAction(InputAction_AltPress) && !IsExpectingInputNow(InputAction_AltPress)
                && PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch)
            {
                canJump = true;
                if (isFacingRight)
                {
                    PlayBirdAnim("birds_i_L_squat", false, true, false, true);
                    jumpInput = false;
                }
                else
                {
                    PlayBirdAnim("birds_i_R_squat", false, true, false, true);
                    jumpInput = false;
                }
            }
            if (PlayerInput.GetIsAction(InputAction_AltRelease) && !IsExpectingInputNow(InputAction_AltRelease)
                && PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch)
            {
                playerSide++;
                if (canJump)
                {
                    canJump = false;
                    if (isFacingRight)
                    {
                        PlayBirdAnim("birds_i_L_jump", false, true, false, true);
                    }
                    else
                    {
                        PlayBirdAnim("birds_i_R_jump", false, true, false, true);
                    }

                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 0.5f, delegate {
                            PlayBirdAnim("birds_i_miss_front", false, true, false, true);
                            if (Huebirds.Length > 1) Huebirds[1].TriggerMad(beat + 0.5f);
                            PlayBumpSound();
                        })
                    });
                }
            }

            isCpuFacingRight = (side % 2 != 0);
            isFacingRight = (playerSide % 2 != 0);


            if (wantMarch != double.MinValue)
            {
                queuedMarch.Add(wantMarch);
                keepMarching = true;
                wantMarch = double.MinValue;
            }

            if (Conductor.instance.isPlaying && !Conductor.instance.isPaused)
            {
                if (queuedMarch.Count > 0)
                {
                    foreach (var march in queuedMarch)
                    {
                        var actions = new List<BeatAction.Action>();

                        bool betweenMarchValues = march + 1 < startRegularMarch && march + 1 >= startNoMarch;
                        if (!betweenMarchValues) ScheduleInput(march, 1, InputAction_BasicPress, Just, Miss, null);

                        betweenMarchValues = march + 1 < startRegularMarch && march + 1 >= startNoMarch;
                        if (!betweenMarchValues) actions.Add(new BeatAction.Action(march + 1, delegate { NPCStep(false); side++; }));
                        betweenMarchValues = march + 1.5 < startRegularMarch && march + 1.5 >= startNoMarch;
                        if (!betweenMarchValues) actions.Add(new BeatAction.Action(march + 1.5, delegate { NPCStep(true); }));
                        
                        if (keepMarching) actions.Add(new BeatAction.Action(march, delegate { queuedMarch.Add(march + 1); }));

                        BeatAction.New(this, actions);
                    }
                    queuedMarch.Clear();
                }
            }

            MoveCamera();

            
        }

        public void Bop(double beat, float length)
        {
            var actions = new List<BeatAction.Action>();

            for (int i = 0; i < length; i++)
            {
                actions.Add(new(beat + i, delegate { BopAnim(); }));
            }

            BeatAction.New(this, actions);
        }
        public void BopAnim()
        {
            if (isFacingRight)
            {
                PlayBirdAnim("birds_i_R_beat", false, true, false);
            }
            else
            {
                PlayBirdAnim("birds_i_L_beat", false, true, false);
            }

            if (isCpuFacingRight)
            {
                PlayBirdAnim("birds_i_R_beat", true, false, !isFlying);
            }
            else
            {
                PlayBirdAnim("birds_i_L_beat", true, false, !isFlying);
            }
        }
        public void March(double beat, bool prepare)
        {
            double currentBeat = Conductor.instance.unswungSongPositionInBeatsAsDouble;
        
            if (beat < currentBeat - 1.5) return;

            if (prepare)
            {
                Prepare(beat - 1);
            }
            wantMarch = beat - 1;
        }

        public void Stop(double beat)
        {
            keepMarching = false;
            endMarch = beat;
            wantMarch = double.MinValue;
        }

        public void Jump(double beat)
        {
            startNoMarch = beat + 1.6;
            startRegularMarch = beat + 4;

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 2, delegate { NPCJump(false); }),
                new BeatAction.Action(beat + 3, delegate { NPCJump(true); }),
                new BeatAction.Action(beat + 3.5, delegate { side++; }),
            });

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("flockStep/SE_BIRD_VOICE_1", beat),
                new MultiSound.Sound("flockStep/SE_BIRD_VOICE_2", beat + 0.5),
                new MultiSound.Sound("flockStep/SE_BIRD_VOICE_3", beat + 0.75),
                new MultiSound.Sound("flockStep/SE_BIRD_VOICE_4", beat + 1),
                new MultiSound.Sound("flockStep/SE_BIRD_VOICE_5", beat + 1.5),
                new MultiSound.Sound("flockStep/SE_BIRD_VOICE_6", beat + 2),
            }, forcePlay: true);

            ScheduleInput(beat, 2, InputAction_AltPress, JustHold, MissSquat, null);
            ScheduleInput(beat, 3, InputAction_AltRelease, JustRelease, MissJumpRelease, null);
        }

        private double lastMissBeat = double.MinValue;
        private bool didSquat = false;

        public void PlayBumpSound()
        {
            SoundByte.PlayOneShotGame($"flockStep/SE_BIRD_MISS_HIT_{UnityEngine.Random.Range(1, 7)}");
            
            double currentBeat = conductor.unswungSongPositionInBeatsAsDouble;
            if (currentBeat >= lastMissBeat + 4.0)
            {
                SoundByte.PlayOneShotGame("flockStep/SE_BIRD_MISS_HIT_VOICE_PITCH_100_CENTS", pitch: SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-100, 101), false));
                SoundByte.PlayOneShotGame("flockStep/SE_BIRD_MISS_HIT_EXTRA");
            }
            lastMissBeat = currentBeat;
        }

        public void Hit()
        {
        }

        public void FlyAway(double beat, float length, bool instant, int groupSize)
        {
            if (instant) SoundByte.PlayOneShotGame("flockStep/SE_BIRD_FLY_AWAY");

            List<Huebird> birdsToFly = new List<Huebird>();
            if (Huebirds2 != null) birdsToFly.AddRange(Huebirds2);
            if (Huebirds3 != null) birdsToFly.AddRange(Huebirds3);

            birdsToFly.RemoveAll(b => !b.gameObject.activeSelf || b.flyState == 3 || b.flyState == 1);

            if (birdsToFly.Count > 0)
            {
                var birdKeys = new System.Collections.Generic.Dictionary<Huebird, float>();
                foreach (var b in birdsToFly) birdKeys[b] = instant ? b.initialBirdX + UnityEngine.Random.Range(-15f, 15f) : UnityEngine.Random.value;
                
                var sortedBirds = new List<Huebird>(birdsToFly);
                sortedBirds.Sort((a, b) => birdKeys[b].CompareTo(birdKeys[a]));

                List<BeatAction.Action> flyActions = new List<BeatAction.Action>();
                for (int i = 0; i < sortedBirds.Count; i++)
                {
                    var b = sortedBirds[i];
                    double delay = instant ? Mathf.Lerp(0f, 2f, (float)i / (Mathf.Max(1, sortedBirds.Count - 1))) 
                                           : (i / groupSize) * length + UnityEngine.Random.Range(0f, 0.4f);
                    b.flyStartBeat = beat + delay;
                    b.flyDuration = 4.0; 
                    
                    double expectedStartBeat = b.flyStartBeat;
                    flyActions.Add(new BeatAction.Action(b.flyStartBeat, delegate {
                        if (b.flyStartBeat != expectedStartBeat) return;
                        b.flyState = 1; 
                        b.currentAnim = "birds_i_fly_loop"; b.birdAnim.DoScaledAnimationAsync("birds_i_fly_loop", 0.5f);
                    }));
                }
                
                if (flyActions.Count > 0)
                {
                    flyActions.Sort((a, x) => a.beat.CompareTo(x.beat));
                    BeatAction.New(instance, flyActions);
                }
                
                int halfIndex = sortedBirds.Count / 2;
                double halfDelay = instant ? Mathf.Lerp(0f, 2f, (float)halfIndex / Mathf.Max(1, sortedBirds.Count - 1)) : (halfIndex / groupSize) * length;
                
            }
        }
        public void Comeback(double beat, float length, bool instant, int size, int groupSize)
        {
            isFlying = false;

            if (instant) SoundByte.PlayOneShotGame("flockStep/SE_BIRD_FLY_AWAY");

            List<Huebird> birdsToLand = new List<Huebird>();
            if (size >= 1 && Huebirds2 != null) birdsToLand.AddRange(Huebirds2);
            if (size >= 2 && Huebirds3 != null) birdsToLand.AddRange(Huebirds3);

            birdsToLand.RemoveAll(b => b.gameObject.activeInHierarchy && b.flyState == 0);
            birdsToLand.RemoveAll(b => b.gameObject.activeInHierarchy && b.flyState == 2 && beat >= b.flyStartBeat);

            if (size >= 1 && otherBirds != null) otherBirds.SetActive(true);
            if (size >= 2 && otherBirds2 != null) otherBirds2.SetActive(true);

            if (birdsToLand.Count > 0)
            {
                var birdKeys = new System.Collections.Generic.Dictionary<Huebird, float>();
                foreach (var b in birdsToLand) birdKeys[b] = instant ? b.initialBirdX + UnityEngine.Random.Range(-15f, 15f) : UnityEngine.Random.value;
                
                var sortedBirds = new List<Huebird>(birdsToLand);
                sortedBirds.Sort((a, b) => birdKeys[b].CompareTo(birdKeys[a]));

                List<BeatAction.Action> landActions = new List<BeatAction.Action>();
                for (int i = 0; i < sortedBirds.Count; i++)
                {
                    var b = sortedBirds[i];
                    b.gameObject.SetActive(true);
                    b.UpdateColor();
                    double delay = instant ? Mathf.Lerp(0f, 2f, (float)i / (Mathf.Max(1, sortedBirds.Count - 1))) 
                                           : (i / groupSize) * length + UnityEngine.Random.Range(0f, 0.4f);
                    b.flyState = 2; 
                    b.flyStartBeat = beat + delay;
                    b.flyDuration = 1.0; 

                    double expectedStartBeat = b.flyStartBeat;
                    landActions.Add(new BeatAction.Action(b.flyStartBeat, delegate {
                        if (b.flyStartBeat != expectedStartBeat) return;
                        b.currentAnim = "birds_i_fly_loop"; b.birdAnim.DoScaledAnimationAsync("birds_i_fly_loop", 0.5f);
                    }));
                    landActions.Add(new BeatAction.Action(b.flyStartBeat + 1.0, delegate {
                        if (b.flyStartBeat != expectedStartBeat) return;
                        b.currentAnim = "birds_i_land"; b.birdAnim.DoScaledAnimationAsync("birds_i_land", 0.5f);
                        float syncedX = b.initialBirdX + (Huebirds[0].bird.localPosition.x - Huebirds[0].initialBirdX);
                        b.bird.localPosition = new Vector3(syncedX, b.initialBirdY, b.initialBirdZ);
                    }));
                    landActions.Add(new BeatAction.Action(b.flyStartBeat + 1.5f, delegate {
                        if (b.flyStartBeat != expectedStartBeat) return;
                        b.flyState = 0;
                        b.pendingMove = Huebirds[0].pendingMove;
                    }));
                }

                if (landActions.Count > 0)
                {
                    landActions.Sort((a, x) => a.beat.CompareTo(x.beat));
                    BeatAction.New(instance, landActions);
                }

                int halfIndex = sortedBirds.Count / 2;
                double halfDelay = instant ? Mathf.Lerp(0f, 2f, (float)halfIndex / Mathf.Max(1, sortedBirds.Count - 1)) : (halfIndex / groupSize) * length;
                
            }
            
        }
        public void NPCStep(bool lift)
        {
            if (isCpuFacingRight)
            {
                if (lift) 
                { 
                    PlayBirdAnim("birds_i_R_walk_00", true, false, !isFlying); 
                    MigrationSFX(true);
                }
                else
                {
                    PlayBirdAnim("birds_i_L_walk_01", true, false, !isFlying);
                    MigrationSFX(false);
                }
            }
            else
            {
                if (lift) 
                { 
                    PlayBirdAnim("birds_i_L_walk_00", true, false, !isFlying); 
                    MigrationSFX(true);
                }
                else
                {
                    PlayBirdAnim("birds_i_R_walk_01", true, false, !isFlying);
                    MigrationSFX(false);
                }
            }
        }

        public void NPCJump(bool jump)
        {
            if (jump) { MigrationSFX_Jump(); }
            else { MigrationSFX_Squat(); }

            if (isCpuFacingRight)
            {
                if (jump) { PlayBirdAnim("birds_i_L_jump", true, false, !isFlying); }
                else
                {
                    PlayBirdAnim("birds_i_L_squat", true, false, !isFlying);
                }
            }
            else
            {
                if (jump) { PlayBirdAnim("birds_i_R_jump", true, false, !isFlying); }
                else
                {
                    PlayBirdAnim("birds_i_R_squat", true, false, !isFlying);
                }
            }
        }

        public void Prepare(double beat)
        {
            

            if (isFacingRight)
            {
                PlayBirdAnim("birds_i_R_walk_00", false, true, false);
            }
            else
            {
                PlayBirdAnim("birds_i_L_walk_00", false, true, false);
            }

            if (isCpuFacingRight)
            {
                PlayBirdAnim("birds_i_R_walk_00", true, false, !isFlying);
            }
            else
            {
                PlayBirdAnim("birds_i_L_walk_00", true, false, !isFlying);
            }
        }
        int migrationSide = 0;
        public void MigrationSFX(bool lift)
        {
            if (lift) return;

            string step = migrationSide % 2 == 0 ? "A" : "B";

            switch (Migration)
            {
                case 0: 
                    SoundByte.PlayOneShotGame($"flockStep/SE_BIRD_FOOT_COM_{step}", volume: 0.708f);
                    break;
                case 1:
                    SoundByte.PlayOneShotGame($"flockStep/SE_BIRD_FOOT_COM_{step}", volume: 0.708f);
                    SoundByte.PlayOneShotGame($"flockStep/SE_BIRD_FOOT_LARGE_{step}", volume: 0.708f);
                    SoundByte.PlayOneShotGame("flockStep/SE_BIRD_FOOT_LARGE_SHAKE_PITCH_200_CENTS",
                        pitch: SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-200, 201), false), volume: 0.708f);
                    break;
                case 2:
                    SoundByte.PlayOneShotGame($"flockStep/SE_BIRD_FOOT_COM_{step}", volume: 0.708f);
                    SoundByte.PlayOneShotGame($"flockStep/SE_BIRD_FOOT_LARGE_{step}");
                    SoundByte.PlayOneShotGame("flockStep/SE_BIRD_FOOT_LARGE_SHAKE_PITCH_200_CENTS",
                        pitch: SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-200, 201), false));
                    break;
                case 3:
                    SoundByte.PlayOneShotGame($"flockStep/SE_BIRD_FOOT_COM_{step}_MANY", volume: 0.708f);
                    SoundByte.PlayOneShotGame($"flockStep/reverb/SE_BIRD_FOOT_LARGE_{step}_MANY_reverb");
                    SoundByte.PlayOneShotGame("flockStep/SE_BIRD_FOOT_LARGE_SHAKE_MANY_PITCH_200_CENTS",
                        pitch: SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-200, 201), false));
                    break;
            }
            migrationSide++;
        }

        public void MigrationSFX_Squat()
        {
            switch (Migration)
            {
                case 0: break;
                case 1: 
                    SoundByte.PlayOneShotGame("flockStep/SE_BIRD_FOOT_DOWN_LARGE", volume: 0.708f);
                    SoundByte.PlayOneShotGame("flockStep/SE_BIRD_FOOT_LARGE_SHAKE_MANY_PITCH_200_CENTS",
                        pitch: SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-200, 201), false), volume: 0.708f);
                    break;
                case 2:
                    SoundByte.PlayOneShotGame("flockStep/SE_BIRD_FOOT_DOWN_LARGE");
                    SoundByte.PlayOneShotGame("flockStep/SE_BIRD_FOOT_LARGE_SHAKE_MANY_PITCH_200_CENTS",
                        pitch: SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-200, 201), false));
                    break;
                case 3:
                    SoundByte.PlayOneShotGame("flockStep/SE_BIRD_FOOT_DOWN_LARGE_MANY_reverb");
                    SoundByte.PlayOneShotGame("flockStep/SE_BIRD_FOOT_LARGE_SHAKE_MANY_PITCH_200_CENTS",
                        pitch: SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-200, 201), false));
                    break;
            }
        }

        public void MigrationSFX_Jump()
        {
            switch (Migration)
            {
                case 0: break;
                case 1: 
                    SoundByte.PlayOneShotGame("flockStep/SE_BIRD_WING_LARGE", volume: 0.708f);
                    SoundByte.PlayOneShotGame("flockStep/SE_BIRD_FOOT_LARGE_SHAKE_PITCH_200_CENTS",
                        pitch: SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-200, 201), false), volume: 0.708f);
                    break;
                case 2:
                    SoundByte.PlayOneShotGame("flockStep/SE_BIRD_WING_LARGE"); 
                    SoundByte.PlayOneShotGame("flockStep/SE_BIRD_FOOT_LARGE_SHAKE_PITCH_200_CENTS",
                        pitch: SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-200, 201), false));
                    break;
                case 3: 
                    SoundByte.PlayOneShotGame("flockStep/SE_BIRD_WING_LARGE_MANY"); 
                    SoundByte.PlayOneShotGame("flockStep/SE_BIRD_FOOT_LARGE_SHAKE_MANY_PITCH_200_CENTS",
                        pitch: SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-200, 201), false));
                    break;
            }
        }
        public void Just(PlayerActionEvent caller, float state)
        {
            var beat = caller.startBeat + caller.timer;
            Hit();
            playerSide++;
            canRelease = true;

            if (isFacingRight)
            {
                PlayBirdAnim("birds_i_L_walk_01", false, true, false);
                
                if (Migration >= 3) SoundByte.PlayOneShotGame("flockStep/reverb/SE_BIRD_FOOT_PLAYER_B_MANY_reverb");
                else SoundByte.PlayOneShotGame("flockStep/SE_BIRD_FOOT_PLAYER_B");
            }
            else
            {
                PlayBirdAnim("birds_i_R_walk_01", false, true, false);

                if (Migration >= 3) SoundByte.PlayOneShotGame("flockStep/reverb/SE_BIRD_FOOT_PLAYER_A_MANY_reverb");
                else SoundByte.PlayOneShotGame("flockStep/SE_BIRD_FOOT_PLAYER_A");
            }

            if (GameManager.instance.Autoplay)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 0.5, delegate {
                        if (isFacingRight)
                        {
                            PlayBirdAnim("birds_i_R_walk_00", false, true, false);
                            
                            if (Migration >= 3) SoundByte.PlayOneShotGame("flockStep/reverb/SE_BIRD_FOOT_UP_PLAYER_B_MANY_reverb");
                            else SoundByte.PlayOneShotGame("flockStep/SE_BIRD_FOOT_UP_PLAYER_B");
                        }
                        else
                        {
                            PlayBirdAnim("birds_i_L_walk_00", false, true, false);
                            
                            if (Migration >= 3) SoundByte.PlayOneShotGame("flockStep/reverb/SE_BIRD_FOOT_UP_PLAYER_A_MANY_reverb");
                            else SoundByte.PlayOneShotGame("flockStep/SE_BIRD_FOOT_UP_PLAYER_A");
                        }
                    }),
                });
            }
        }

        public void JustHold(PlayerActionEvent caller, float state)
        {
            var beat = caller.startBeat + caller.timer;
            Hit();
            jumpInput = true;
            canRelease = false;
            didSquat = true;
            canJump = true;

            SoundByte.PlayOneShotGame("flockStep/SE_BIRD_FOOT_DOWN");

            if (isFacingRight)
            {
                PlayBirdAnim("birds_i_L_squat", false, true, false);
            }
            else
            {
                PlayBirdAnim("birds_i_R_squat", false, true, false);
            }
        }

        public void JustRelease(PlayerActionEvent caller, float state)
        {
            var beat = caller.startBeat + caller.timer;
            Hit();
            jumpInput = false;
            canRelease = false;
            canJump = false;

            SoundByte.PlayOneShotGame("flockStep/SE_BIRD_WING");

            if (isFacingRight)
            {
                PlayBirdAnim("birds_i_L_jump", false, true, false);
            }
            else
            {
                PlayBirdAnim("birds_i_R_jump", false, true, false);
            }

            if (!didSquat) HuebirdPlayer.pendingMove = -1.13f;
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.5, delegate { playerSide++; }),
            });
        }
        public void Miss(PlayerActionEvent caller)
        {
            canRelease = false;
            PlayBirdAnim("birds_i_miss_back", false, true, false);
            if (Huebirds.Length > 2) Huebirds[2].TriggerMad(conductor.unswungSongPositionInBeatsAsDouble);
            PlayBumpSound();
        }

        public void MissSquat(PlayerActionEvent caller)
        {
            canRelease = false;
            didSquat = false;
        }

        public void MissJumpRelease(PlayerActionEvent caller)
        {
            canRelease = false;
            PlayBirdAnim("birds_i_miss_back", false, true, false); 
            if (didSquat) HuebirdPlayer.pendingMove = -0.33f;
            else HuebirdPlayer.pendingMove = -0.73f;
            if (Huebirds.Length > 2) Huebirds[2].TriggerMad(conductor.unswungSongPositionInBeatsAsDouble);
            PlayBumpSound();
        }
        public void ZoomOut(double beat)
        {
            Migration = 3;
            isZoomedOut = true;

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0, delegate { StageEffect.DoScaledAnimationAsync("Effect1-1", 0.5f); }),
                new BeatAction.Action(beat + 1, delegate { StageEffect.DoScaledAnimationAsync("Effect1-2", 0.5f); }),
                new BeatAction.Action(beat + 2, delegate { StageEffect.DoScaledAnimationAsync("Effect2-1", 0.5f); }),
                new BeatAction.Action(beat + 4, delegate { StageEffect.DoScaledAnimationAsync("Effect2-2", 0.5f); }),
                new BeatAction.Action(beat + 5, delegate { StageEffect.DoScaledAnimationAsync("Effect2-3", 0.5f); }),
                new BeatAction.Action(beat + 6, delegate { StageEffect.DoScaledAnimationAsync("Effect3", 0.5f); }),
                new BeatAction.Action(beat + 7, delegate {
                    StageEffect.DoScaledAnimationAsync("idle", 0.5f);
                    SceneHolder.DoScaledAnimationAsync("ZoomOut", 0.5f); 
                }),
                new BeatAction.Action(beat + 7.5, delegate {
                    SceneHolder.DoScaledAnimationAsync("SceneIdle", 0.5f);
                    isZoomedOut = false;
                }),
            });
        }

        public void SetZoomSegment(double beat, int segment)
        {
            Migration = 3;
            isZoomedOut = true;

            switch (segment)
            {
                case (int)ZoomLevel.MainToGlobe1:
                    StageEffect.DoScaledAnimationAsync("Effect1-1", 0.5f);
                    break;
                case (int)ZoomLevel.MainToGlobe2:
                    StageEffect.DoScaledAnimationAsync("Effect1-2", 0.5f);
                    break;
                case (int)ZoomLevel.MainToGlobe3:
                    StageEffect.DoScaledAnimationAsync("Effect2-1", 0.5f);
                    break;
                case (int)ZoomLevel.GlobeToBirds1:
                    StageEffect.DoScaledAnimationAsync("Effect2-2", 0.5f);
                    break;
                case (int)ZoomLevel.GlobeToBirds2:
                    StageEffect.DoScaledAnimationAsync("Effect2-3", 0.5f);
                    break;
                case (int)ZoomLevel.GlobeToBirds3:
                    StageEffect.DoScaledAnimationAsync("Effect3", 0.5f);
                    break;
                case (int)ZoomLevel.BirdsToMain:
                    StageEffect.DoScaledAnimationAsync("idle", 0.5f);
                    SceneHolder.DoScaledAnimationAsync("ZoomOut", 0.5f); 
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 0.5, delegate {
                            SceneHolder.DoScaledAnimationAsync("SceneIdle", 0.5f);
                            isZoomedOut = false;
                        }),
                    });
                    break;
                case (int)ZoomLevel.Main:
                default:
                    StageEffect.DoScaledAnimationAsync("idle", 0.5f);
                    SceneHolder.DoScaledAnimationAsync("SceneIdle", 0.5f);
                    isZoomedOut = false;
                    break;
            }
        }

        private void SyncBirds(Huebird[] targetBirds)
        {
            if (Huebirds == null || Huebirds.Length == 0) return;
            var sourceBird = Huebirds[0];
            if (sourceBird == null || sourceBird.bird == null) return;

            float sourceDeltaX = sourceBird.bird.localPosition.x - sourceBird.initialBirdX;
            float targetPending = sourceBird.pendingMove;

            if (targetBirds != null)
            {
                foreach (var b in targetBirds)
                {
                    if (b != null && b.bird != null)
                    {
                        Vector3 pos = b.bird.localPosition;
                        pos.x = b.initialBirdX + sourceDeltaX;
                        b.bird.localPosition = pos;
                        b.pendingMove = targetPending;
                    }
                }
            }
        }

        public void BGState(FlockSize state)
        {
            switch (state)
            {
                case FlockSize.Small:
                    isFlying = true;
                    if (otherBirds != null) otherBirds.SetActive(false);
                    if (otherBirds2 != null) otherBirds2.SetActive(false);
                    break;
                case FlockSize.Medium:
                    isFlying = false;
                    if (otherBirds != null) otherBirds.SetActive(true);
                    if (otherBirds2 != null) otherBirds2.SetActive(false);
                    if (Huebirds2 != null) foreach (var b in Huebirds2) { b.gameObject.SetActive(true); b.flyState = 0; }
                    SyncBirds(Huebirds2);
                    break;
                case FlockSize.Large:
                    isFlying = false;
                    if (otherBirds != null) otherBirds.SetActive(true);
                    if (otherBirds2 != null) otherBirds2.SetActive(true);
                    if (Huebirds2 != null) foreach (var b in Huebirds2) { b.gameObject.SetActive(true); b.flyState = 0; }
                    if (Huebirds3 != null) foreach (var b in Huebirds3) { b.gameObject.SetActive(true); b.flyState = 0; }
                    SyncBirds(Huebirds2);
                    SyncBirds(Huebirds3);
                    break;
            }
        }
        const int IAAltDownCat = IAMAXCAT;
        const int IAAltUpCat = IAMAXCAT + 1;
        protected static bool IA_PadAltPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltPress(out double dt)
        {
            return PlayerInput.GetSqueezeDown(out dt);
        }

        protected static bool IA_PadAltRelease(out double dt)
        {
            return PlayerInput.GetPadUp(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltRelease(out double dt)
        {
            return PlayerInput.GetSqueezeUp(out dt);
        }

        public static PlayerInput.InputAction InputAction_AltPress =
            new("RvlBirdAltPress", new int[] { IAAltDownCat, IAAltDownCat, IAAltDownCat },
            IA_PadAltPress, IA_TouchBasicPress, IA_BatonAltPress);
        public static PlayerInput.InputAction InputAction_AltRelease =
            new("RvlBirdAltRelease", new int[] { IAAltUpCat, IAFlickCat, IAAltUpCat },
            IA_PadAltRelease, IA_TouchFlick, IA_BatonAltRelease);
        public static PlayerInput.InputAction InputAction_TouchRelease =
            new("RvlBirdTouchRelease", new int[] { IAEmptyCat, IAReleaseCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicRelease, IA_Empty);


        public void SetMoveCamera(double beat, float length, float value)
        {
            targetCamPos += value;
            lastCamPos = GameCamera.AdditionalPosition.x;
            camWantPos = targetCamPos - lastCamPos;
            camWantBeat = beat;
            camWantLength = length;

            MoveCamera();
        }

        public void MoveCamera()
        {
            //Debug.LogWarning(GameCamera.AdditionalPosition.x);
            // im gonna overcomment this code cause i dont wanna refigure this shit out later
            var cond = Conductor.instance;
            float normalizedBeat = camWantLength == 0 ? 0 : cond.GetPositionFromBeat(camWantBeat, camWantLength); // if the length is 0 the beat is always 0 so it doesnt divide by 0
            Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(Util.EasingFunction.Ease.EaseOutQuad); // i hardcoded the ease this cant be bad
            float camSetPos = func(0f, 1f, normalizedBeat);
            if (normalizedBeat >= 1f) camSetPos = 1f; // this fixes weird issues with the ease continuing for some reason

            float camAddPos = camSetPos * camWantPos; // the ease length multiplied by the distance the camera should move
            GameCamera.AdditionalPosition = new Vector3(lastCamPos + camAddPos, 0, 0);
        }

        double camWaitUntil = double.MinValue;

        public void PlayBirdAnim(string animName, bool main, bool player, bool bg, bool isWhiff = false)
        {
            double beat = conductor.unswungSongPositionInBeatsAsDouble;

            switch (animName)
            {
                case "birds_i_L_walk_01":
                    if (!player)
                    {
                        if (beat >= camWaitUntil)
                        {
                            SetMoveCamera(beat, 0.5f, -0.4f);
                        }
                    }
                    break;
                case "birds_i_R_walk_01":
                    if (!player)
                    {
                        if (beat >= camWaitUntil)
                        {
                            SetMoveCamera(beat, 0.5f, -0.4f);
                        }
                    }
                    break;
                case "birds_i_R_squat":
                    if (!player)
                    {
                        if (beat >= camWaitUntil)
                        {
                            SetMoveCamera(beat, 0.5f, -0.4f);
                        }
                    }
                    break;
                case "birds_i_L_squat":
                    if (!player)
                    {
                        if (beat >= camWaitUntil)
                        {
                            SetMoveCamera(beat, 0.5f, -0.4f);
                        }
                    }
                    break;
                case "birds_i_R_jump":
                    if (!player)
                    {
                        SetMoveCamera(beat, 0.5f, -1.13f);
                        camWaitUntil = beat + 1.5;
                    }
                    break;
                case "birds_i_L_jump":
                    if (!player)
                    {
                        SetMoveCamera(beat, 0.5f, -1.13f);
                        camWaitUntil = beat + 1.5;
                    }
                    break;
            }

            if (player && HuebirdPlayer != null) { HuebirdPlayer.PlayAnim(animName, isWhiff); }

            bool isBop = animName == "birds_i_L_beat" || animName == "birds_i_R_beat";
                        if (Huebirds != null)
            {
                foreach (Huebird bird in Huebirds)
                {
                    if (bird == null) continue;
                    if (main)
                    {
                        float offset = isBop ? 0f : UnityEngine.Random.Range(0f, 0.1f);
                        bird.ScheduleAnim(animName, beat + offset);
                    }
                }
            }
            if (Huebirds2 != null)
            {
                foreach (Huebird bgbird in Huebirds2)
                {
                    if (bgbird == null) continue;
                    if (bg && bgbird.flyState == 0)
                    {
                        float offset = isBop ? 0f : (isFlying ? UnityEngine.Random.Range(0f, 2f) : UnityEngine.Random.Range(0f, 0.1f));
                        bgbird.ScheduleAnim(animName, beat + offset);
                    }
                }
            }
            if (Huebirds3 != null)
            {
                foreach (Huebird bgbird2 in Huebirds3)
                {
                    if (bgbird2 == null) continue;
                    if (bg && bgbird2.flyState == 0)
                    {
                        float offset = isBop ? 0f : (isFlying ? UnityEngine.Random.Range(0f, 2f) : UnityEngine.Random.Range(0f, 0.1f));
                        bgbird2.ScheduleAnim(animName, beat + offset);
                    }
                }
            }
        }
        public void BirdSettings(BirdColors colors, bool random, bool mainBirds, bool bgBirds, bool instant, Color feather, Color belly, Color beak)
        {
            if (birdColors.Count < 20)
            {
                while(birdColors.Count < 20) _birdColors.Add(Color.white);
            }
            if (colors == BirdColors.Custom)
            {
                _birdColors[17] = feather;
                _birdColors[18] = belly;
                _birdColors[19] = beak;
            }

            if (mainBirds)
            {
                if (Huebirds != null) foreach (Huebird bird in Huebirds)
                {
                    if (bird == null) continue;
                    bird.birdColor = random ? UnityEngine.Random.Range(0, 5) : (int)colors;
                    bird.UpdateColor();
                }
                if (HuebirdPlayer != null)
                {
                    HuebirdPlayer.birdColor = random ? UnityEngine.Random.Range(0, 5) : (int)colors;
                    HuebirdPlayer.UpdateColor();
                }
            }
            if (bgBirds)
            {
                if (Huebirds2 != null) foreach (Huebird bird in Huebirds2)
                {
                    if (bird == null) continue;
                    bird.birdColor = random ? UnityEngine.Random.Range(0, 5) : (int)colors;
                    if (instant || bird.flyState != 0) bird.UpdateColor();
                }
                if (Huebirds3 != null) foreach (Huebird bird in Huebirds3)
                {
                    if (bird == null) continue;
                    bird.birdColor = random ? UnityEngine.Random.Range(0, 5) : (int)colors;
                    if (instant || bird.flyState != 0) bird.UpdateColor();
                }
            }
        }
        

        public void RecolorBG(Color Square, Color Grass)
        {
            if (bgMaterial != null) 
            {
                bgMaterial.SetColor("_ColorAlpha", Grass);
                bgMaterial.color = Grass;
            }
            if (bgSquare != null)
            {
                bgSquare.color = Square;
            }
        }

        private void PersistColor(double beat)
        {
            var allEvents = GameManager.instance.Beatmap.Entities.FindAll(e => e.datamodel.Split('/')[0] is "flockStep");
            var eventsBefore = allEvents.FindAll(e => e.beat < beat);

            var lastPersistEvent = eventsBefore.FindLast(e => e.datamodel == "flockStep/bgColor");
            if (lastPersistEvent != null)
            {
                var e = lastPersistEvent;
                RecolorBG(e["square"], e["grass"]);
            }
        }
    }
}