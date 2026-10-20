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
    /// Minigame loaders handle the setup of your minigame.
    /// Here, you designate the game prefab, define entities, and mark what AssetBundle to load

    /// Names of minigame loaders follow a specific naming convention of `PlatformcodeNameLoader`, where:
    /// `Platformcode` is a three-leter platform code with the minigame's origin
    /// `Name` is a short internal name
    /// `Loader` is the string "Loader"

    /// Platform codes are as follows:
    /// Agb: Gameboy Advance    ("Advance Gameboy")
    /// Ntr: Nintendo DS        ("Nitro")
    /// Rvl: Nintendo Wii       ("Revolution")
    /// Ctr: Nintendo 3DS       ("Centrair")
    /// Mob: Mobile
    /// Pco: PC / Other

    /// Fill in the loader class label, "*prefab name*", and "*Display Name*" with the relevant information
    /// For help, feel free to reach out to us on our discord, in the #development channel.
    public static class PcoGardenLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("gardenDance", "Garden Dance", "92cbe8", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out GardenDance instance)) {
                            instance.Bop(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity["auto"], eventCaller.currentEntity["toggle"]);
                        }
                    },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", GardenDance.WhoBops.Both, "Bop", "Set the characters to bop for the duration of this event."),
                        new Param("auto", GardenDance.WhoBops.None, "Bop (Auto)", "Set the characters to automatically bop until another Bop event is reached."),
                    }
                },
                new GameAction("dance", "Start Dancing")
                {
                    preFunction = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out GardenDance instance)) {
                            instance.Dance(eventCaller.currentEntity.beat);
                        }
                    },
                    preFunctionLength = 1
                },
                new GameAction("stop", "Stop Dancing")
                {
                    preFunction = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out GardenDance instance)) {
                            instance.Stop(eventCaller.currentEntity.beat);
                        }
                    },
                },
                new GameAction("pose", "Pose")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out GardenDance instance)) {
                            instance.Pose(eventCaller.currentEntity.beat);
                        }
                    },
                    defaultLength = 4f
                },
                new GameAction("triplet", "Triplet Cue")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out GardenDance instance)) {
                            instance.Triplet(eventCaller.currentEntity.beat);
                        }
                    },
                    defaultLength = 4f
                },
                new GameAction("force", "Force Dancing")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out GardenDance instance)) {
                            instance.ForceDancing(eventCaller.currentEntity.beat, eventCaller.currentEntity.length);
                        }
                    },
                    resizable = true,
                    defaultLength = 4f
                },
                new GameAction("bird", "Mr. Bird")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out GardenDance instance)) {
                            instance.Bird(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity["side"]);
                        }
                    },
                    resizable = true,
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("side", GardenDance.BirdAnimation.Left, "Side", "Chooses which side Mr. Bird will fly to."),
                    }
                },
                new GameAction("sun", "Sun Animation")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out GardenDance instance)) {
                            instance.Sun(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity["whichAnim"], eventCaller.currentEntity["instant"]);
                        }
                    },
                    resizable = true,
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("whichAnim", GardenDance.SunAnimation.Enter, "Animation", "Chooses what animation the sun should play."),
                        new Param("instant", false, "Instant", "Toggle if the animation should be played instantly.")
                    }
                },
            },
            new List<string>() { "pco", "keep" },
            "pcogarden", "en",
            new List<string>() { },
            chronologicalSortKey: 20250603,
            plusGame: true,
            font: "FOT-SlumpStd-DB SDF"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
    using Scripts_GardenDance;
    public class GardenDance : Minigame
    {
        [SerializeField] Flower flowerPlayer;
        [SerializeField] List<Flower> flowers = new List<Flower>();
        [SerializeField] Animator sunAnim;
        [SerializeField] Animator birdAnim;
        private bool flowerBop = true;

        public int npcAnger;

        private bool sunBop = true;
        private bool sunActive = true;
        private double sunBeat;
        private float sunLength;
        private string sunAnimation = "";

        private bool birdActive;
        private double birdBeat;
        private float birdLength;
        private string birdAnimation = "";

        double wantDance = double.MinValue;
        List<double> queuedDances = new();
        bool keepDancing;
        //double startBackHop = double.MinValue;
        double startNoDance = double.MinValue;
        double startRegularDance = double.MinValue;
        double endDance = double.MinValue;


        public enum WhoBops
        {
            Flowers = 0,
            Sun = 1,
            Both = 2,
            None = 3
        }
        public enum SunAnimation
        {
            Enter = 0, 
            Exit = 1
        }
        public enum BirdAnimation
        {
            Left,
            Right,
        }
        
        private void Awake()
        {

        }

        private void Update()
        {
            if (sunAnimation != "")
            {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(sunBeat, sunLength);
                if (normalizedBeat > 1) sunAnimation = "";
                sunAnim.DoNormalizedAnimation(sunAnimation, normalizedBeat, 0);
            }

            if (birdAnimation != "")
            {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(birdBeat, birdLength);
                if (normalizedBeat > 1) 
                { 
                    birdAnimation = "";
                    birdActive = false;
                }
                birdAnim.DoNormalizedAnimation(birdAnimation, normalizedBeat, 0);
            }

            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            {
                flowerPlayer.Dance(false);
                foreach (var flower in flowers) { flower.Glare(); }
            }

            // ktb

            if (wantDance != double.MinValue)
            {
                queuedDances.Add(wantDance);
                keepDancing = true;
                wantDance = double.MinValue;
            }

            if (Conductor.instance.isPlaying && !Conductor.instance.isPaused)
            {
                if (queuedDances.Count > 0)
                {
                    foreach (var dance in queuedDances)
                    {
                        var actions = new List<BeatAction.Action>();

                        bool betweenDanceValues = dance + 1 < startRegularDance && dance + 1 >= startNoDance;
                        if (!betweenDanceValues) ScheduleInput(dance, 1, InputAction_BasicPress, DanceHit, DanceMiss, null);

                        betweenDanceValues = dance + 1 < startRegularDance && dance + 1 >= startNoDance;
                        if (!betweenDanceValues) actions.Add(new BeatAction.Action(dance + 1, delegate { NPCDance(); }));

                        if (keepDancing) actions.Add(new BeatAction.Action(dance, delegate { queuedDances.Add(dance + 1); }));

                        BeatAction.New(this, actions);
                    }
                    queuedDances.Clear();
                }

            }

        }

        public override void OnLateBeatPulse(double beat)
        {
            if (flowerBop) DoBop(0);
            if (sunBop) DoBop(1);
            if (birdActive) birdAnim.DoScaledAnimationAsync("Flap", 0.5f, animLayer: 1);
        }

        public void Bop(double beat, float length, int auto, int bop)
        {
            flowerBop = auto == (int)WhoBops.Flowers || auto == (int)WhoBops.Both;
            sunBop = auto == (int)WhoBops.Sun || auto == (int)WhoBops.Both;
            for (int i = 0; i < length; i++)
            {
                BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + i, delegate
                    {
                        DoBop(bop);
                    })
                });
            }
        }

        private void DoBop(int bop)
        {
            bool canFlowerBop = !keepDancing && (conductor.songPositionInBeatsAsDouble < startNoDance || conductor.songPositionInBeatsAsDouble > startRegularDance) && conductor.songPositionInBeatsAsDouble > endDance;
            switch (bop)
            {
                case 0:
                    if (canFlowerBop) 
                    {
                        foreach (var flower in flowers)
                        {
                            flower.Bop();
                        }
                        flowerPlayer.Bop();
                        npcAnger = 0;
                    }
                    break;
                case 1:
                    if (sunActive) sunAnim.DoScaledAnimationAsync("Bop", 0.5f);
                    break;
                case 2:
                    if (canFlowerBop)
                    {
                        foreach (var flower in flowers)
                        {
                            flower.Bop();
                        }
                        flowerPlayer.Bop();
                        npcAnger = 0;
                    }
                    if (sunActive) sunAnim.DoScaledAnimationAsync("Bop", 0.5f);
                    break;
                default:
                    break;
            }
        }

        public void Dance(double beat)
        {
            wantDance = beat - 1;
        }

        public void Stop(double beat)
        {
            keepDancing = false;
            endDance = beat;
        }

        private void DanceHit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                flowerPlayer.Dance(true, true);
                //foreach (var flower in flowers) { flower.Glare(); }
            }
            else
            {
                flowerPlayer.Dance(true, false);
            }
            if (npcAnger > 0) npcAnger -= 1;
        }

        private void PDanceHit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                flowerPlayer.Dance(true, true, true);
                //foreach (var flower in flowers) { flower.Glare(); }
            }
            else
            {
                flowerPlayer.Dance(true, false, true);
            }
            if (npcAnger > 0) npcAnger -= 1;
        }

        private void TripletHit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                flowerPlayer.Triplet(true, true);
                //foreach (var flower in flowers) { flower.Glare(); }
            }
            else
            {
                flowerPlayer.Triplet(true, false);
            }
            if (npcAnger > 0) npcAnger -= 1;
        }

        private void DanceMiss(PlayerActionEvent caller)
        {
            foreach (var flower in flowers) { flower.Glare(); }
            flowerPlayer.Miss();
            npcAnger = 3;
        }

        private void NPCDance(bool pose = false)
        {
            foreach (var flower in flowers) { flower.Dance(true, false, pose, npcAnger > 0); }
        }

        public void Pose(double beat)
        {
            startNoDance = beat + 2;
            startRegularDance = beat + 4;

            var sounds = new List<MultiSound.Sound>();

            sounds.Add(new MultiSound.Sound("gardenDance/whistle1", beat));
            sounds.Add(new MultiSound.Sound("gardenDance/whistle2", beat + 0.5));
            sounds.Add(new MultiSound.Sound("gardenDance/whistle3", beat + 1));

            MultiSound.Play(sounds, forcePlay: true);

            ScheduleInput(beat, 2, InputAction_BasicPress, PDanceHit, DanceMiss, null);
            ScheduleInput(beat, 2.5, InputAction_BasicPress, PoseHit, DanceMiss, null);

            var actions = new List<BeatAction.Action>();

            actions.Add(new BeatAction.Action(beat, delegate { BirdWhistle(); }));
            actions.Add(new BeatAction.Action(beat + 0.5, delegate { BirdWhistle(); }));
            actions.Add(new BeatAction.Action(beat + 1, delegate { BirdWhistle(); }));

            actions.Add(new BeatAction.Action(beat + 2, delegate { NPCDance(true); }));
            actions.Add(new BeatAction.Action(beat + 2.5, delegate { NPCPose(); }));

            BeatAction.New(this, actions);
        }

        private void NPCPose()
        {
            foreach (var flower in flowers) { flower.Pose(false, false, npcAnger > 0); }
        }

        private void NPCTriplet()
        {
            foreach (var flower in flowers) { flower.Triplet(false, false, npcAnger > 0); }
        }

        private void PoseHit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                flowerPlayer.Pose(true, true);
                //foreach (var flower in flowers) { flower.Glare(); }
            }
            else
            {
                flowerPlayer.Pose(true, false);
            }
            if (npcAnger > 0) npcAnger -= 1;
        }

        public void Triplet(double beat)
        {
            startNoDance = beat + 2;
            startRegularDance = beat + 4;

            var sounds = new List<MultiSound.Sound>();

            sounds.Add(new MultiSound.Sound("gardenDance/tripletWhistle", beat, ignoreSwing: true));
            sounds.Add(new MultiSound.Sound("gardenDance/tripletWhistle", beat + 1, ignoreSwing: true));

            MultiSound.Play(sounds, forcePlay: true);

            ScheduleInput(beat, 2, InputAction_BasicPress, TripletHit, DanceMiss, null, ignoreSwing: true);
            ScheduleInput(beat, 2.66667, InputAction_BasicPress, PDanceHit, DanceMiss, null, ignoreSwing: true);
            ScheduleInput(beat, 3.3333302, InputAction_BasicPress, PoseHit, DanceMiss, null, ignoreSwing: true);

            var actions = new List<BeatAction.Action>();

            actions.Add(new BeatAction.Action(beat, delegate { BirdWhistle(); }));
            actions.Add(new BeatAction.Action(beat + 1, delegate { BirdWhistle(); }));

            actions.Add(new BeatAction.Action(beat + 2, delegate { NPCTriplet(); }, ignoreSwing: true));
            actions.Add(new BeatAction.Action(beat + 2.66667, delegate { NPCDance(true); }, ignoreSwing: true));
            actions.Add(new BeatAction.Action(beat + 3.3333302, delegate { NPCPose(); }, ignoreSwing: true));

            BeatAction.New(this, actions);
        }

        public void Bird(double beat, float length, int anim)
        {
            birdActive = true;
            birdBeat = beat;
            birdLength = length;
            if (anim == 0)
            {
                birdAnimation = "FlyLeft";
            }
            else
            {
                birdAnimation = "FlyRight";
            }
        }

        private void BirdWhistle()
        {
            if (birdActive) birdAnim.DoScaledAnimationAsync("Whistle", 0.5f, animLayer: 2);
        }

        public void Sun(double beat, float length, int anim, bool instant)
        {
            if (!instant) 
            {
                sunBeat = beat;
                sunLength = length;
                if (anim == 0)
                {
                    sunAnimation = "Enter";
                    sunActive = true;
                }
                else
                {
                    sunAnimation = "Leave";
                    sunActive = false;
                }
            }
            else
            {
                if (anim == 0)
                {
                    sunAnim.DoScaledAnimationAsync("Idle", 0.5f);
                    sunActive = true;
                }
                else
                {
                    sunAnim.DoScaledAnimationAsync("Hide", 0.5f);
                    sunActive = false;
                }
            }
        }

        public void ForceDancing(double beat, float length)
        {
            var actions = new List<BeatAction.Action>();

            for (int i = 0; i < length; i++)
            {
                actions.Add(new BeatAction.Action(beat + i, delegate {
                    NPCDance();
                }));
                ScheduleInput(beat, i, InputAction_BasicPress, DanceHit, DanceMiss, null);
            }

            BeatAction.New(this, actions);
        }

    }
}