using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using DG.Tweening;

using NaughtyBezierCurves;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using UnityEngine.UIElements;

using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlMunchyMonkLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("munchyMonkRvl", "Munchy Monk", "836410", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    //100% stolen bop code
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        MunchyMonkRvl.instance.Bop(e.beat, e.length, e["who"], e["bop"], e["auto"]);
                    },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("who", MunchyMonkRvl.WhoBops.both, "Who Bops?", "Which one bops."),
                        new Param("bop", true, "Enable Bopping", "Whether to bop to the beat or not."),
                        new Param("auto", false, "Automatic?", ""),
                    }
                },
                new GameAction("egg", "One egg!")
                {
                    function = delegate { var e = eventCaller.currentEntity; MunchyMonkRvl.instance.oneEgg(e.beat); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                    //    new Param("type", MunchyMonkRvl.Egg.Regular, "Egg Type", "Sets which egg the girl gives")
                    }
                },
                new GameAction("pea1", "Handful of peas! (5)")
                {
                    function = delegate { var e = eventCaller.currentEntity; MunchyMonkRvl.instance.PeaStart(e.beat, e.length - 3); },
                    defaultLength = 4f,
                },
                new GameAction("pea2", "Handful of peas! (9)")
                {
                    function = delegate { var e = eventCaller.currentEntity; MunchyMonkRvl.instance.PeaStart(e.beat, e.length - 3); },
                    defaultLength = 5f,
                },
                new GameAction("pea3", "Handful of peas! (13)")
                {
                    function = delegate { var e = eventCaller.currentEntity; MunchyMonkRvl.instance.PeaStart(e.beat, e.length - 3); },
                    defaultLength = 6f,
                },
                /* Mmmmm....nope. Not happening.
                new GameAction("pea4", "Handful of peas! (Stretchable)")
                {
                    function = delegate { var e = eventCaller.currentEntity; MunchyMonkRvl.instance.PeaStart(e.beat, e.length - 3); },
                    resizable = true,
                    defaultLength = 4f,
                },
                */
                new GameAction("bg", "Background Appearance")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity; 
                        MunchyMonkRvl.instance.SetBG((MunchyMonkRvl.BG)e["type"], e["instant"], (MunchyMonkRvl.CameoOptions)e["cameo"]); 
                    },
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("type", MunchyMonkRvl.BG.Sky, "Background", "Sets the environment used."),
                        new Param("cameo", MunchyMonkRvl.CameoOptions.Original, "Cameos", "Toggles if cameos should appear, if the selected BG has them."),
                        new Param("instant", true, "Instant", "Toggles if it should instantly change, or if it changes on the next cycle.")
                    },
                    
                },
                new GameAction("bgScroll", "Background Scroll Speed")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        MunchyMonkRvl.instance.ScrollSpeed(e.beat, e.length, e["ease"], e["speed"]);
                    },
                    resizable = true,
                    defaultLength = 1,
                    parameters = new List<Param>() {
                        new Param("speed", new EntityTypes.Float(0, 10, 1), "Speed", "Set the speed of the background scroll."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action"),
                    }
                },
            },
            new List<string>() { "rvl", "normal", "endless" },
            "rvleatmen", "en",
             new List<string>() { "en", "jp" },
            chronologicalSortKey: 102
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Jukebox;
    using Scripts_MunchyMonkRvl;
    public class MunchyMonkRvl : Minigame
    {
        [SerializeField] ParticleSystem flowerParticles;
        public GameObject EggDummy1; // kid mouth
        public GameObject EggDummy2; // girl hand
        public GameObject EggDummy3; // monk hand
        [SerializeField] GameObject missEgg;
        [SerializeField] GameObject missPea;
        [SerializeField] Transform barelyPos;

        [SerializeField] GameObject[] KidPeaDummies;
        [SerializeField] GameObject[] GirlArmADummies;
        [SerializeField] GameObject[] GirlArmBDummies;
        [SerializeField] GameObject[] MonkPeaDummies;

        [Header("Animators")]
        [SerializeField] public Animator monkAnim;
        [SerializeField] public Animator monkArmAnim;
        [SerializeField] public Animator girlAnim;
        [SerializeField] public Animator kidAnim;
        [SerializeField] public Animator armAnim;
        [SerializeField] public Animator arm2Anim;
        [SerializeField] public Animator peasAnim;
        [SerializeField] public Animator effectAnim;
        [SerializeField] public Animator bgStrip1;
        [SerializeField] public Animator bgStrip2;
        [SerializeField] public Animator scrollingBg;

        bool monkBop = true;
        bool girlBop = true;

        bool monkCanBop = true;
        bool girlCanBop = true;

        double monkCanBopBeat = Double.MinValue;

        bool monkCanBopTemp = true;
        bool girlCanBopTemp = true;
        bool isHolding = false;

        bool hasMissedPeas = false;

        PlayerActionEvent endEvent;

        Sound blowSound;
        int _blowSoundID = 0;
        private Conductor cond;
        private Vector2 kidHeadScale;

        BG currentBG;
        string currentBgAnim = "eatmen_bg_bg";
        BGSegments currentSegment; // segment of bgStrip2
        BGSegments queuedSegment; // segment to come
        CameoOptions cameoOptions = CameoOptions.Original;

        private float cameoChance = 0.25f;

        public enum WhoBops
        {
            player,
            girl,
            both,
        }

        public enum Egg
        {
            Regular,
            Golden,
        }
        public enum BG
        {
            Sky,
            CloudySky,
            Forest,
            Flowers,
            Sunset,
            TrainStation,
        }

        public enum BGSegments
        {
            ForestSigns1,
            ForestSigns2,
            ForestSigns3,
            ForestSigns4,
            ForestCameos1,
            ForestCameos2,
            Forest1,
            Forest2,
            Sky1,
            Sky2,
            Sunflowers1,
            SunflowersCameos1,
            SunflowersCameos2,
            SunsetCameos1,
            Sunset1,
            Tunnel1
        }

        public enum CameoOptions
        {
            None,
            Original,
            All
        }

        public float prevScrollSpeed = 0.05f;
        public float scrollSpeed = 0.05f;
        private float currentScroll = 0f;
        private float beatLastFrame = float.MinValue;
        private UpdateEasing scrollEase;
        const int IAAltDownCat = IAMAXCAT;
        const int IAAltUpCat = IAMAXCAT + 1;

        protected static bool IA_EmptyTouchUp(out double dt)
        {
            return PlayerInput.GetTouchUp(InputController.ActionsTouch.Tap, out dt) && !PlayerInput.GetFlick(out _);
        }
        
        protected static bool IA_PadAltDown(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltDown(out double dt)
        {
            return PlayerInput.GetSqueezeDown(out dt);
        }
        protected static bool IA_TouchAltDown(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && instance.IsExpectingInputNow(InputAction_AltDown);
        }

        protected static bool IA_PadAltUp(out double dt)
        {
            return PlayerInput.GetPadUp(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltUp(out double dt)
        {
            return PlayerInput.GetSqueezeUp(out dt);
        }
        protected static bool IA_TouchAltUp(out double dt)
        {
            return PlayerInput.GetFlick(out dt)
                && instance.IsExpectingInputNow(InputAction_AltUp);
        }

        public static PlayerInput.InputAction InputAction_AltDown =
            new("RvlShugyoAltDown", new int[] { IAAltDownCat, IAAltDownCat, IAAltDownCat },
            IA_PadAltDown, IA_TouchAltDown, IA_BatonAltDown);
        public static PlayerInput.InputAction InputAction_AltUp =
            new("RvlShugyoAltUp", new int[] { IAAltUpCat, IAAltUpCat, IAAltUpCat },
            IA_PadAltUp, IA_TouchBasicRelease, IA_BatonAltUp);
        public static PlayerInput.InputAction InputAction_TouchUp =
            new("RvlShugyoTouchUp", new int[] { IAEmptyCat, IAReleaseCat, IAEmptyCat },
            IA_PadAltUp, IA_EmptyTouchUp, IA_BatonAltUp);
        public static MunchyMonkRvl instance;

        public struct UpdateEasing
        {
            public double beat;
            public float length;
            public Util.EasingFunction.Ease ease;
        }

        public void Awake()
        {
            instance = this;
            SetupBopRegion("munchyMonkRvl", "bop", "auto");
            cond = Conductor.instance;
            kidHeadScale = kidAnim.transform.localScale;
            SetBGInstant(BG.Sky);
        }
        private void Update()
        {
            // inputs
            if (!cond.isPlaying) return;

            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_AltDown)
                && !monkAnim.IsPlayingAnimationNames("Blow", "BlowLoop", "BlowLoopEnd"))
                {
                    monkAnim.DoScaledAnimationAsync("Blow", 0.5f);
                    SoundByte.PlayOneShotGame("munchyMonkRvl/BREATH_SWING_PITCH50", pitch: SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-50, 51), false), returnSound: true);
                }
            else if (PlayerInput.GetIsAction(InputAction_AltDown) && !IsExpectingInputNow(InputAction_AltDown)
                && !monkAnim.IsPlayingAnimationNames("Blow", "BlowLoop"))
                {
                    var blowPitch = SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-50, 51), false);
                    if (blowSound == null || blowSound.soundId != _blowSoundID || !blowSound.audioSource.isPlaying)
                    {
                        blowSound = SoundByte.PlayOneShotGame("munchyMonkRvl/BREATH_SWING_MANY_PITCH50_LOOP", pitch: blowPitch, looping: true);
                        _blowSoundID = blowSound.soundId;
                        monkAnim.DoScaledAnimationAsync("BlowLoop", 0.5f);
                        girlAnim.DoScaledAnimationAsync("SadLoop", 0.5f);
                    }
                    isHolding = true;
                }
            else if (PlayerInput.GetIsAction(InputAction_AltUp) && !IsExpectingInputNow(InputAction_AltUp)
                && monkAnim.IsPlayingAnimationNames("BlowLoop") && isHolding)
                {
                    if (blowSound != null && blowSound.soundId == _blowSoundID) blowSound.KillLoop();
                    monkAnim.DoScaledAnimationAsync("BlowLoopEnd", 0.5f);
                    hasMissedPeas = true;
                    isHolding = false;
                }

            // bg
            var currentScrollSpeed = scrollSpeed;
            if (scrollEase.length != 0)
            {
                float normalizedBeat = cond.GetPositionFromBeat(scrollEase.beat, scrollEase.length);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(scrollEase.ease);
                currentScrollSpeed = func(prevScrollSpeed, scrollSpeed, normalizedBeat);
                if (normalizedBeat >= 1) scrollEase.length = 1;
            }

            if (beatLastFrame == float.MinValue) 
            {
                beatLastFrame = cond.songPositionInBeats;
            }
            currentScroll += currentScrollSpeed * (cond.songPositionInBeats - beatLastFrame);
            if (currentScroll >= 1f)
            {
                QueueNextBG();
                currentScroll -= 1f;
            }
            scrollingBg.DoNormalizedAnimation("Scroll", currentScroll);

            beatLastFrame = cond.songPositionInBeats;
        }

        public override void OnLateBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat))
            {
                if (monkBop) BopAnimationMonk(beat);
                if (girlBop) BopAnimationGirl(beat);
            }
        }
        public void Bop(double beat, float length, int whoBops, bool bop, bool autoBop)
        {
            monkBop = (whoBops is (int)WhoBops.player or (int)WhoBops.both) && autoBop;
            girlBop = (whoBops is (int)WhoBops.girl or (int)WhoBops.both) && autoBop;

            if (bop)
            {
                var actions = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                { 
                    if (whoBops is (int)WhoBops.player or (int)WhoBops.both) actions.Add(new(beat + i, delegate { BopAnimationMonk(beat + i); }));
                    if (whoBops is (int)WhoBops.girl or (int)WhoBops.both) actions.Add(new(beat + i, delegate { BopAnimationGirl(beat + i); }));
                }
                BeatAction.New(this, actions);
            }
        }
        public void BopAnimationMonk(double beat)
        {
            if (monkCanBop && monkCanBopTemp && !isHolding && monkCanBopBeat <= beat 
                && !monkAnim.IsPlayingAnimationNames("Blow", "BlowLoop", "BlowLoopEnd"))
            {
                monkAnim.DoScaledAnimationAsync("Beat", 0.5f);
            }
        }
        public void BopAnimationGirl(double beat)
        {
            if (girlCanBop && girlCanBopTemp && monkCanBopBeat <= beat
                && !girlAnim.IsPlayingAnimationNames("Catch"))
            {
                kidAnim.DoScaledAnimationAsync("idle", 0.5f);
                girlAnim.DoScaledAnimationAsync("Beat", 0.5f);
            }
        }

        public void oneEgg(double beat)
        {
            //WhichObject.sprite = ObjectTypes[0];
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("munchyMonkRvl/eggVoice1", beat),
                new MultiSound.Sound("munchyMonkRvl/eggVoice2", beat + 1f),
                new MultiSound.Sound("munchyMonkRvl/RIDE_ONE_PUT", beat + 1f),
            }, forcePlay: true);

            EggDummy2.SetActive(true);

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0, delegate { armAnim.DoScaledAnimationAsync("Up", 0.5f); }),
                new BeatAction.Action(beat + 1, delegate { EggDummy2.SetActive(false); }),
                new BeatAction.Action(beat + 1, delegate { EggDummy3.SetActive(true); }),
                new BeatAction.Action(beat + 1, delegate { monkArmAnim.DoScaledAnimationAsync("Ready", 0.5f); }),
                new BeatAction.Action(beat + 1, delegate { armAnim.DoScaledAnimationAsync("Set", 0.5f); }),
                new BeatAction.Action(beat + 1.5, delegate { armAnim.DoScaledAnimationAsync("Out", 0.5f); }),
            });
            ScheduleInput(beat, 2f, InputAction_BasicPress, EggHit, EggMiss, null);
        }
        public void EggHit(PlayerActionEvent caller, float state)
        {
            girlBop = false;
            monkBop = false;
            kidAnim.transform.localScale = kidHeadScale;
            EggDummy3.gameObject.SetActive(false);
            if (blowSound != null && blowSound.soundId == _blowSoundID) blowSound.KillLoop();
            
            if (state >= 1f || state <= -1f)
            {
                monkAnim.DoScaledAnimationAsync("Blow", 0.5f);
                kidAnim.DoScaledAnimationAsync("idle", 0.5f);
                girlAnim.DoScaledAnimationAsync(state >= 1f ? "Miss" : "Out", 0.5f);

                SoundByte.PlayOneShotGame("munchyMonkRvl/MISS_FACE");
                SoundByte.PlayOneShotGame("munchyMonkRvl/BREATH_SWING_PITCH50", pitch: SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-50, 51), false), returnSound: true);

                Food miss = Instantiate(missEgg, barelyPos.position, new Quaternion(0, 0, 0, 1), transform).GetComponent<Food>();
                miss.velocity = new Vector2(5f, 10f);
                miss.waitTime = 0.1f;

                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.startBeat + caller.timer + 1, delegate { monkBop = true; girlBop = true; }),
                });
            }
            else
            {
                MonkInput(caller.startBeat + caller.timer);
                SoundByte.PlayOneShotGame("munchyMonkRvl/GOKUN", caller.startBeat + caller.timer + 0.5f);
                EggDummy1.SetActive(true);
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.5, delegate { EggDummy1.SetActive(false); }),
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.5, delegate { flowerParticles.Play(); }),
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.5, delegate { girlAnim.DoScaledAnimationAsync("Eat", 0.5f); }),
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.5, delegate { kidAnim.DoScaledAnimationAsync("idle", 0.5f); }),
                    new BeatAction.Action(caller.startBeat + caller.timer + 1, delegate { monkBop = true; girlBop = true; }),
                });
            }
        }
        public void EggMiss(PlayerActionEvent caller)
        {
            Instantiate(missEgg, EggDummy3.transform.position, new Quaternion(0, 0, 0, 1), transform);
            EggDummy3.SetActive(false);
            girlAnim.DoScaledAnimationAsync("Sad", 0.5f);
            monkAnim.DoScaledAnimationAsync("Sad", 0.5f);
            if (blowSound != null && blowSound.soundId == _blowSoundID) blowSound.KillLoop();
            SoundByte.PlayOneShot("miss");
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer + 1, delegate { monkBop = true; girlBop = true; }),
            });
        }
        public void MonkInput(double beat)
        {
            monkCanBopBeat = beat + 2;
            SoundByte.PlayOneShotGame("munchyMonkRvl/HIT_ONE");
            SoundByte.PlayOneShotGame("munchyMonkRvl/BREATH_ONE_PITCH50", pitch: SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-50, 51), false), returnSound: true);
            effectAnim.DoScaledAnimationAsync("Spit", 0.5f);
            monkAnim.DoScaledAnimationAsync("Blow", 0.5f);
            girlAnim.DoScaledAnimationAsync("Catch", 0.5f);
            kidAnim.DoScaledAnimationAsync("Catch", 0.5f);
        }

        public void PeaStart(double beat, double length)
        {
            girlBop = false;
            monkBop = false;

            int peasTemp = (int)Math.Ceiling(length * 4) + 1;
            string peaSpecial = "munchyMonkRvl/RIDE_MANY_";

            if (length <= 1) peaSpecial += "5";
            else if (length <= 2) peaSpecial += "9";
            else { peaSpecial += "13"; }
            peaSpecial += "_PUT";
            int amountOfPeas = Math.Max(peasTemp, 1);

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("munchyMonkRvl/peasVoice1", beat),
                new MultiSound.Sound("munchyMonkRvl/peasVoice2", beat + 0.5f),
                new MultiSound.Sound("munchyMonkRvl/peasVoice3", beat + 0.75f),
                new MultiSound.Sound("munchyMonkRvl/peasVoice4", beat + 1f),
                new MultiSound.Sound(peaSpecial, beat + 1f),
            }, forcePlay: true);

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { 
                    armAnim.DoScaledAnimationAsync("Up", 0.5f); 
                    arm2Anim.DoScaledAnimationAsync("Up", 0.5f);

                    int peasArmA = (int)Math.Ceiling(amountOfPeas / 2f);
                    int peasArmB = (int)Math.Floor(amountOfPeas / 2f);

                    for (int i = 0; i < peasArmA ; i++)
                    {
                        GirlArmADummies[i].SetActive(true);
                    }
                    for (int i = 0; i < peasArmB ; i++)
                    {
                        GirlArmBDummies[i].SetActive(true);
                    }
                }),
                new BeatAction.Action(beat + 1, delegate { 
                    armAnim.DoScaledAnimationAsync("Set", 0.5f);
                    arm2Anim.DoScaledAnimationAsync("Set", 0.5f); 
                    monkArmAnim.DoScaledAnimationAsync("Ready", 0.5f);

                    girlCanBopTemp = false; 

                    foreach (GameObject pea in GirlArmADummies)
                    {
                        pea.SetActive(false);
                    }
                    foreach (GameObject pea in GirlArmBDummies)
                    {
                        pea.SetActive(false);
                    }

                    for (int i = 0; i < amountOfPeas; i++)
                    {
                        MonkPeaDummies[i].SetActive(true);
                    }
                }),
                new BeatAction.Action(beat + 1.5, delegate { armAnim.DoScaledAnimationAsync("Out", 0.5f); }),
                new BeatAction.Action(beat + 1.5, delegate { arm2Anim.DoScaledAnimationAsync("Out", 0.5f); }),
                new BeatAction.Action(beat + length - 0.5, delegate { girlCanBopTemp = true; }),
            });

            ScheduleInput(beat, 2f, InputAction_AltDown, (c, s) => PeaHit(c, s, amountOfPeas), PeaMiss, null);
        }
        public void PeaHit(PlayerActionEvent caller, float state, int amountOfPeas)
        {
            isHolding = true;

            var startBeat = caller.startBeat + caller.timer;
            double inputLength = (amountOfPeas - 1) / 4d;

            if (state >= 1f || state <= -1f)
            {
                hasMissedPeas = true;
            }
            else hasMissedPeas = false;

            monkCanBopBeat = startBeat + inputLength + 2;

            //stuff to do at the time of the input
            monkAnim.DoScaledAnimationAsync("BlowLoop", 0.5f);

            if (blowSound != null && blowSound.soundId == _blowSoundID) blowSound.KillLoop();
            blowSound = SoundByte.PlayOneShotGame("munchyMonkRvl/BREATH_MANY_PITCH50", pitch: SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-50, 51), false), returnSound: true);
            _blowSoundID = blowSound.soundId;
            //sets up the beat action
            var actions = new List<BeatAction.Action>();

            bool lastPeaHit = true;

            //peas
            for (int i = 0; i < amountOfPeas ; i++)
            {
                int num = i;
                float scale = 1f + (i * 0.1f);

                //in the beataction you have to use num instead of i for some reason 
                //because otherwise it'll always be the highest i instead of the current i

                var peaBeat = i == 0 ? 0 : startBeat + (i / 4d); // first one is on beat 0 so it actually happens instantly

                actions.Add(new BeatAction.Action(peaBeat, delegate {
                    if (hasMissedPeas)
                    {
                        kidAnim.DoScaledAnimationAsync("idle", 0.5f);
                        girlAnim.DoScaledAnimationAsync("Miss", 0.5f);
                        foreach (GameObject pea in KidPeaDummies)
                        {
                            pea.SetActive(false);
                        }

                        Food miss = Instantiate(missPea, barelyPos.position, new Quaternion(0, 0, 0, 1), transform).GetComponent<Food>();
                        miss.velocity = new Vector2(5f, 10f);
                        miss.waitTime = 0.1f;

                        if (lastPeaHit == true) ScoreMiss(1f);
                        lastPeaHit = false;

                        SoundByte.PlayOneShotGame("munchyMonkRvl/MISS_FACE");
                        endEvent.Disable();
                    }
                    else
                    {
                        kidAnim.DoScaledAnimationAsync("Catch", 0.5f);
                        girlAnim.DoScaledAnimationAsync("Catch", 0.5f);
                        kidAnim.transform.localScale = kidHeadScale * scale;
                        
                        if (num < KidPeaDummies.Length)
                        {
                            KidPeaDummies[num].SetActive(true);
                        }
                        SoundByte.PlayOneShotGame("munchyMonkRvl/HIT_MANY", pitch: SoundByte.GetPitchFromSemiTones(num, false));
                    }
                    MonkPeaDummies[num].SetActive(false);
                    monkArmAnim.DoScaledAnimationAsync("Blow", 0.5f);
                    effectAnim.DoScaledAnimationAsync("Spit", 0.5f);
                }));
            }

            //auto-release stuff
            actions.Add(new BeatAction.Action(caller.startBeat + caller.timer + inputLength, delegate { if (!hasMissedPeas && endEvent.canHit) endEvent.Hit(0, 1); }));

            BeatAction.New(instance, actions);

            endEvent = ScheduleInput(startBeat, inputLength, InputAction_AltUp, ReleaseSuccess, null, null);
            endEvent.Enable();
        }

        public void PeaMiss(PlayerActionEvent caller)
        {
            foreach (GameObject pea in MonkPeaDummies)
            {
                if (pea.activeSelf)
                {
                    Food miss = Instantiate(missPea, pea.transform.position, pea.transform.localRotation, transform).GetComponent<Food>();
                    miss.velocity = new Vector2(UnityEngine.Random.Range(5, 16), UnityEngine.Random.Range(-1, 1));
                    miss.sprite.sortingOrder = pea.GetComponent<SpriteRenderer>().sortingOrder + 50;
                }
                pea.SetActive(false);
            }
            
            girlAnim.DoScaledAnimationAsync("Sad", 0.5f);
            monkAnim.DoScaledAnimationAsync("Sad", 0.5f);

            foreach (GameObject pea in KidPeaDummies)
            {
                pea.SetActive(false);
            }

            SoundByte.PlayOneShot("miss");
            if (blowSound != null && blowSound.soundId == _blowSoundID) blowSound.KillLoop();
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer + 1, delegate { monkBop = true; girlBop = true; }),
            });
        }

        private void ReleaseSuccess(PlayerActionEvent caller, float state)
        {
            caller.Disable(); // prevent double inputs from auto release

            isHolding = false;

            if (state >= 1f || state <= -1f) // technically only early barely is possible but im too lazy to think about that rn
            {
                if (blowSound != null && blowSound.soundId == _blowSoundID) blowSound.KillLoop(0.1f);
                monkAnim.DoScaledAnimationAsync("BlowLoopEnd", 0.5f);
                hasMissedPeas = true;
                return;
            }

            if (blowSound != null && blowSound.soundId == _blowSoundID) blowSound.KillLoop(0.1f);
            monkAnim.DoScaledAnimationAsync("BlowLoopEnd", 0.5f);
            SoundByte.PlayOneShotGame("munchyMonkRvl/GOKUN_MANY", caller.startBeat + caller.timer + 0.5f);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer + 0.5, delegate 
                { 
                    flowerParticles.Play();
                    girlAnim.DoScaledAnimationAsync("Eat", 0.5f); 
                    kidAnim.DoScaledAnimationAsync("idle", 0.5f);
                    foreach (GameObject pea in KidPeaDummies)
                    {
                        pea.SetActive(false);
                    }

                }),
                new BeatAction.Action(caller.startBeat + caller.timer + 1, delegate { monkBop = true; girlBop = true; }),
            });
        }

        public void SetBGInstant(BG type)
        {
            currentBG = type;
            currentSegment = GetSegment();
            queuedSegment = GetSegment();

            QueueNextBG();
        }

        public void SetBG(BG type, bool instant, CameoOptions cameo)
        {
            cameoOptions = cameo;
            if (instant)
            {
                SetBGInstant(type);
                return;
            }
            
            if ((currentBG == BG.Sunset && type != BG.Sunset) || (currentBG != BG.Sunset && type == BG.Sunset)) 
            {
                queuedSegment = BGSegments.Tunnel1; // queues up a tunnel as a transition since the bgs are too different to instantly transition
                currentBG = type;
            }
            else
            {
                currentBG = type;
                queuedSegment = GetSegment();
            }

            if (currentScroll < 0.3f) // skip a segment and instantly set the second bg if its offscreen
            {
                QueueNextBG(true);
            }
        }

        public void QueueNextBG(bool skipBg1 = false)
        {
            if (!skipBg1) bgStrip1.DoNormalizedAnimation(currentBgAnim, (float)currentSegment/16);

            currentBgAnim = "eatmen_bg_bg";
            if (cameoOptions == CameoOptions.All && UnityEngine.Random.Range(0f, 1f) < 0.5f)
            {
                currentBgAnim = "eatmen_bg_bgCAMEO";
            }
            currentSegment = queuedSegment;
            bgStrip2.DoNormalizedAnimation(currentBgAnim, (float)currentSegment/16);

            queuedSegment = GetSegment();
        }

        public BGSegments GetSegment()
        {
            switch (currentBG)
            {
                case BG.Sky:
                    return BGSegments.Sky1;
                case BG.CloudySky:
                    return BGSegments.Sky2;
                case BG.Forest:
                    int forestSeg = (int)BGSegments.Forest1;
                    if (UnityEngine.Random.Range(0f, 1f) < cameoChance && cameoOptions != CameoOptions.None)
                    {
                        forestSeg = (int)BGSegments.ForestCameos1;
                    }
                    return (BGSegments)UnityEngine.Random.Range(forestSeg, forestSeg + 2);
                case BG.Flowers:
                    int flowerSeg = (int)BGSegments.Sunflowers1;
                    if (UnityEngine.Random.Range(0f, 1f) < cameoChance && cameoOptions != CameoOptions.None)
                    {
                        flowerSeg = (int)BGSegments.SunflowersCameos1;
                    }
                    return (BGSegments)UnityEngine.Random.Range(flowerSeg, flowerSeg + 2);
                case BG.Sunset:
                    if (UnityEngine.Random.Range(0f, 1f) < cameoChance && cameoOptions != CameoOptions.None)
                    {
                        return BGSegments.SunsetCameos1;
                    }
                    else return BGSegments.Sunset1;
                case BG.TrainStation:
                    return BGSegments.Tunnel1;
                default:
                    return BGSegments.Sky1;
            }
        }

        public void ScrollSpeed(double beat, float length, int ease, float speed)
        {
            scrollEase = new()
            {
                beat = beat,
                length = length,
                ease = (Util.EasingFunction.Ease)ease,
            };

            prevScrollSpeed = scrollSpeed;
            scrollSpeed = speed / 10;
        }

        void OnDestroy()
        {
            if (blowSound != null && blowSound.soundId == _blowSoundID) blowSound.KillLoop();
            blowSound = null;
        }
    }
}
