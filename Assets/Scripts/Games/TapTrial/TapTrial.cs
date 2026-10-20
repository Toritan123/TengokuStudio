using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbTapLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("tapTrial", "Tap Trial", "94ffb5", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; TapTrial.instance.Bop(e.beat, e.length, e["toggle"], e["toggle2"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Bop", "Toggle if the characters should bop for the duration of this event."),
                        new Param("toggle2", false, "Bop (Auto)", "Toggle if the characters should automatically bop until another Bop event is reached.")
                    }
                },
                new GameAction("tap", "Tap")
                {
                    function = delegate { TapTrial.instance.Tap(eventCaller.currentEntity.beat); },
                    defaultLength = 2.0f
                },
                new GameAction("double tap", "Double Tap")
                {
                    function = delegate { TapTrial.instance.DoubleTap(eventCaller.currentEntity.beat); },
                    defaultLength = 2.0f
                },
                new GameAction("triple tap", "Triple Tap")
                {
                    function = delegate { TapTrial.instance.TripleTap(eventCaller.currentEntity.beat); },
                    defaultLength = 4.0f
                },
                new GameAction("jump tap prep", "Jump Prepare")
                {
                    function = delegate { TapTrial.instance.JumpPrepare(); },
                },
                new GameAction("jump tap", "Jump Tap")
                {
                    function = delegate { var e = eventCaller.currentEntity; TapTrial.instance.JumpTap(e.beat, e["final"], e["altfinalpose"]); },
                    defaultLength = 2.0f,
                    parameters = new List<Param>()
                    {
                        new Param("final", false, "Final", "Toggle if this jump should be the final one of the set."),
                        new Param("altfinalpose", false, "Alt Final Pose", "Toggle if this jump should use the tap trial 2 pose."),
                    }
                },
                new GameAction("scroll event", "Scroll Background")
                {
                    function = delegate { var e = eventCaller.currentEntity; TapTrial.instance.Scroll(e["toggle"], e["flash"], e["m"]); },
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Scroll", "Toggle if the background should scroll.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "flash", "m"})
                        }),
                        new Param("flash", true, "White Fade", "Toggle if the background will have a white overlay."),
                        new Param("m", new EntityTypes.Float(0, 10, 1), "Speed", "Set how fast the background should scroll.")
                    }
                },
                new GameAction("background color", "Background Appearance")
                {
                    function = delegate { var e = eventCaller.currentEntity; TapTrial.instance.BackgroundColor(e.beat, e.length, e["colorFrom"], e["colorTo"], e["colorFrom2"], e["colorTo2"], e["colorFrom3"], e["colorTo3"], e["colorFromStage"], e["colorToStage"], e["colorFromStage2"], e["colorToStage2"], e["ease"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("colorFrom", TapTrial.defaultColorA, "Color A Start", "Set the color of the squares at the start of the event."),
                        new Param("colorTo", TapTrial.defaultColorA, "Color A End", "Set the color of the squares at the end of the event."),
                        new Param("colorFrom2", TapTrial.defaultColorB, "Color B Start", "Set the color of the squares at the start of the event."),
                        new Param("colorTo2", TapTrial.defaultColorB, "Color B End", "Set the color of the squares at the end of the event."),
                        new Param("colorFrom3", TapTrial.defaultColorC, "Color C Start", "Set the color of the squares at the start of the event."),
                        new Param("colorTo3", TapTrial.defaultColorC, "Color C End", "Set the color of the squares at the end of the event."),
                        new Param("colorFromStage", TapTrial.defaultColorStageA, "Stage Top Start", "Set the color of top of the stage at the start of the event."),
                        new Param("colorToStage", TapTrial.defaultColorStageA, "Stage Top End", "Set the color of the top of the stage at the end of the event."),
                        new Param("colorFromStage2", TapTrial.defaultColorStageB, "Stage Rim Start", "Set the color of the side of the stage at the start of the event."),
                        new Param("colorToStage2", TapTrial.defaultColorStageB, "Stage Rim End", "Set the color of the side of the stage at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x != (int)Util.EasingFunction.Ease.Instant, new string[] { "colorFrom", "colorFrom2", "colorFrom3", "colorFromStage", "colorFromStage2" }),
                        }),

                    },
                },
                new GameAction("char color", "Character Appearance")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out TapTrial instance)) {
                            instance.RecolorChars(e["color1"], e["color2"], e["color3"], e["color4"], e["color5"], e["color6"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("color1", TapTrial.charColors[0], "Girl Skin Color", "The color to set Yuka's skin to."),
                        new Param("color2", TapTrial.charColors[1], "Girl Dress Color", "The color to set Yuka's dress and shoes to."),
                        new Param("color3", TapTrial.charColors[2], "Girl Blush Color", "The color to set Yuka's blush to."),
                        new Param("color4", TapTrial.charColors[3], "Animal Color A", "The color to set the animals to."),
                        new Param("color5", TapTrial.charColors[4], "Animal Color B", "The color to set the animals to."),
                        new Param("color6", TapTrial.charColors[5], "Monkey Blush Color", "The color to set the monkeys' blush to."),
                    },
                    defaultLength = 0.5f,
                },
                new GameAction("giraffe events", "Giraffe Animations")
                {
                    function = delegate { var e = eventCaller.currentEntity; TapTrial.instance.GiraffeAnims(e.beat, e.length, e["toggle"], e["instant"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", TapTrial.GiraffeAnimation.Enter, "Animation", "Set the animation for the giraffe to perform."),
                        new Param("instant", EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    },
                    defaultVersion = 1,
                },
                // backwards-compatibility
                new GameAction("final jump tap", "Final Jump Tap")
                {
                    function = delegate { var e = eventCaller.currentEntity; TapTrial.instance.JumpTap(e.beat, true, false); },
                    defaultLength = 2.0f,
                    hidden = true
                },
            },
            new List<string>() { "agb", "normal" },
            "agbtap", "en",
            new List<string>() { },
            chronologicalSortKey: 29,
            font : "Matisse Minori"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using HeavenStudio.Common;
    using Scripts_TapTrial;

    public class TapTrial : Minigame
    {
        [Header("Components")]
        [SerializeField] private TapTrialPlayer player;
        [SerializeField] private Animator monkeyL, monkeyR, giraffe;
        [SerializeField] private ParticleSystem monkeyTapLL, monkeyTapLR, monkeyTapRL, monkeyTapRR;
        [SerializeField] private Transform rootPlayer, rootMonkeyL, rootMonkeyR;
        [SerializeField] private CanvasScroll bgScroll;
        [SerializeField] private SpriteRenderer flash;
        [Header("Values")]
        [SerializeField] private float jumpHeight = 4f;
        [SerializeField] private float monkeyJumpHeight = 3f;
        [SerializeField] private float maxFlashOpacity = 0.8f;
        private bool canBop = true;
        private double jumpStartBeat = double.MinValue;

        [Header("Colors")]
        private double colorStartBeat = -1;
        private float colorLength = 0f;
        private Color colorStartA;
        private Color colorStartB;
        private Color colorStartC;
        private Color colorStartStageA;
        private Color colorStartStageB;
        private Color colorEndA;
        private Color colorEndB;
        private Color colorEndC;
        private Color colorEndStageA;
        private Color colorEndStageB;

        public static Color defaultColorA = new Color(181 / 255f, 1f, 206 / 255f);
        public static Color defaultColorB = new Color(132 / 255f, 231 / 255f, 165 / 255f);
        public static Color defaultColorC = new Color(148 / 255f, 1f, 181 / 255f);
        public static Color defaultColorStageA = new Color(165 / 255f, 231 / 255f, 1f);
        public static Color defaultColorStageB = new Color(74 / 255f, 140 / 255f, 189 / 255f);

        private static readonly Dictionary<int, string> charColorLookup = new()
        {
            { 0, "#FFFFFF" }, //skin
            { 1, "#E70010" }, //dress
            { 2, "#F7B5B5" }, //blush
            { 3, "#EF6B21" }, //orange animal
            { 4, "#FFFFFF" }, //white animal
            { 5, "#F7B5B5" }, //blush animal
        };
        private static List<Color> _charColors = new List<Color>();
        public static List<Color> charColors
        {
            get
            {
                for (int i = 0; i < charColorLookup.Count; i++)
                {
                    Color colorTemp;
                    ColorUtility.TryParseHtmlString(charColorLookup[i], out colorTemp);
                    _charColors.Add(colorTemp);
                }
                return _charColors;
            }
        }

        private Util.EasingFunction.Ease colorEase;

        [SerializeField] Material backgroundMaterial;
        [SerializeField] Material stageMaterial;
        [SerializeField] Material girlMaterial;
        [SerializeField] Material monkeyMaterial;

        public static TapTrial instance;

        private void Awake()
        {
            instance = this;
            SetupBopRegion("tapTrial", "bop", "toggle2");

            colorStartA = defaultColorA;
            colorStartB = defaultColorB;
            colorStartC = defaultColorC;
            colorStartStageA = defaultColorStageA;
            colorStartStageB = defaultColorStageB;
            colorEndA = defaultColorA;
            colorEndB = defaultColorB;
            colorEndC = defaultColorC;
            colorEndStageA = defaultColorStageA;
            colorEndStageB = defaultColorStageB;
        }

        public override void OnBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat)) SingleBop();
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                GiraffeUpdate(cond);
                JumpUpdate(cond);
                ScrollUpdate(cond);
            }
            BackgroundColorUpdate();
        }

        public override void OnGameSwitch(double beat)
        {
            RecolorChars(charColors[0], charColors[1], charColors[2], charColors[3], charColors[4], charColors[5]);
            BackgroundColor(beat, 0f, defaultColorA, defaultColorA, defaultColorB, defaultColorB, defaultColorC, defaultColorC, defaultColorStageA, defaultColorStageA, defaultColorStageB, defaultColorStageB, 1);
            PersistColor(beat);
        }

        public void Scroll(bool scroll, bool flash, float multiplier)
        {
            scrolling = scroll;
            flashing = flash;
            scrollMultiplier = multiplier;
            ResetScroll();
        }

        public void ResetScroll()
        {
            currentScrollSpeed = 0;
            currentNormalizedY = 0;
            flash.color = new Color(1, 1, 1, 0);
        }

        public void RecolorChars(Color skinColor, Color dressColor, Color blushColor, Color orangeAnimalColor, Color whiteAnimalColor, Color blushAnimalColor)
        {
            girlMaterial.SetColor("_ColorAlpha", dressColor);
            girlMaterial.SetColor("_ColorBravo", skinColor);
            girlMaterial.SetColor("_ColorDelta", blushColor);

            monkeyMaterial.SetColor("_ColorAlpha", orangeAnimalColor);
            monkeyMaterial.SetColor("_ColorBravo", whiteAnimalColor);
            monkeyMaterial.SetColor("_ColorDelta", blushAnimalColor);
        }

        public void BackgroundColor(double beat, float length, Color startA, Color endA, Color startB, Color endB, Color startC, Color endC, Color startStageA, Color endStageA, Color startStageB, Color endStageB, int ease)
        {
            colorStartBeat = beat;
            colorLength = length;
            colorStartA = startA;
            colorEndA = endA;
            colorStartB = startB;
            colorEndB = endB;
            colorStartC = startC;
            colorEndC = endC;
            colorStartStageA = startStageA;
            colorEndStageA = endStageA;
            colorStartStageB = startStageB;
            colorEndStageB = endStageB;
            colorEase = (Util.EasingFunction.Ease)ease;
        }

        private void BackgroundColorUpdate()
        {
            float normalizedBeat = Mathf.Clamp01(Conductor.instance.GetPositionFromBeat(colorStartBeat, colorLength));

            var func = Util.EasingFunction.GetEasingFunction(colorEase);

            float newRA = func(colorStartA.r, colorEndA.r, normalizedBeat); 
            float newGA = func(colorStartA.g, colorEndA.g, normalizedBeat);
            float newBA = func(colorStartA.b, colorEndA.b, normalizedBeat);
            float newAA = func(colorStartA.a, colorEndA.a, normalizedBeat);

            float newRB = func(colorStartB.r, colorEndB.r, normalizedBeat);
            float newGB = func(colorStartB.g, colorEndB.g, normalizedBeat);
            float newBB = func(colorStartB.b, colorEndB.b, normalizedBeat);
            float newAB = func(colorStartB.a, colorEndB.a, normalizedBeat);

            float newRC = func(colorStartC.r, colorEndC.r, normalizedBeat); 
            float newGC = func(colorStartC.g, colorEndC.g, normalizedBeat);
            float newBC = func(colorStartC.b, colorEndC.b, normalizedBeat);
            float newAC = func(colorStartC.a, colorEndC.a, normalizedBeat);

            // Stage
            float newRSA = func(colorStartStageA.r, colorEndStageA.r, normalizedBeat); 
            float newGSA = func(colorStartStageA.g, colorEndStageA.g, normalizedBeat);
            float newBSA = func(colorStartStageA.b, colorEndStageA.b, normalizedBeat);
            float newASA = func(colorStartStageA.a, colorEndStageA.a, normalizedBeat);

            // Stage Rim
            float newRSB = func(colorStartStageB.r, colorEndStageB.r, normalizedBeat); 
            float newGSB = func(colorStartStageB.g, colorEndStageB.g, normalizedBeat);
            float newBSB = func(colorStartStageB.b, colorEndStageB.b, normalizedBeat);
            float newASB = func(colorStartStageB.a, colorEndStageB.a, normalizedBeat);

            backgroundMaterial.SetColor("_ColorAlpha", new Color(newRA, newGA, newBA, newAA)); //Red
            backgroundMaterial.SetColor("_ColorBravo", new Color(newRB, newGB, newBB, newAB)); //Green
            backgroundMaterial.SetColor("_ColorDelta", new Color(newRC, newGC, newBC, newAC)); //Blue

            stageMaterial.SetColor("_ColorBravo", new Color(newRSA, newGSA, newBSA, newASA)); //Green
            stageMaterial.SetColor("_ColorDelta", new Color(newRSB, newGSB, newBSB, newASB)); //Blue
        }

        private void PersistColor(double beat)
        {
            var allEvents = GameManager.instance.Beatmap.Entities.FindAll(e => e.datamodel.Split('/')[0] is "tapTrial");
            var eventsBefore = allEvents.FindAll(e => e.beat < beat);

            var lastPersistEvent = eventsBefore.FindLast(e => e.datamodel == "tapTrial/background color");
            if (lastPersistEvent != null)
            {
                var e = lastPersistEvent;
                BackgroundColor(e.beat, e.length, e["colorFrom"], e["colorTo"], e["colorFrom2"], e["colorTo2"], e["colorFrom3"], e["colorTo3"], e["colorFromStage"], e["colorToStage"], e["colorFromStage2"], e["colorToStage2"], e["ease"]);
            }

            lastPersistEvent = eventsBefore.FindLast(e => e.datamodel == "tapTrial/char color");
            if (lastPersistEvent != null)
            {
                var e = lastPersistEvent;
                RecolorChars(e["color1"], e["color2"], e["color3"], e["color4"], e["color5"], e["color6"]);
            }

            lastPersistEvent = eventsBefore.FindLast(e => e.datamodel == "tapTrial/giraffe events");
            if (lastPersistEvent != null)
            {
                var e = lastPersistEvent;
                if (e.version == 1 && e["toggle"] == (int)GiraffeAnimation.Exit)
                {
                    GiraffeAnims(beat, e.length, e["toggle"], (int)EasingFunction.Ease.Instant);
                }
            }
        }

        private bool scrolling;
        private bool flashing;
        [SerializeField] private float maxScrollSpeed = 0.25f;
        [SerializeField] private float accelerationSpeed = 0.01f;
        private float currentScrollSpeed = 0;
        private float currentNormalizedY = 0;
        private float scrollMultiplier = 1;
        private void ScrollUpdate(Conductor cond)
        {
            if (!scrolling)
            {
                bgScroll.Normalized = Vector2.zero;
                ResetScroll();
                return;
            }
            currentNormalizedY += currentScrollSpeed * Time.deltaTime;
            bgScroll.NormalizedY = currentNormalizedY * scrollMultiplier;
            if (flashing) flash.color = new Color(1, 1, 1, Mathf.Lerp(0, maxFlashOpacity, currentNormalizedY));
            currentScrollSpeed += accelerationSpeed * Time.deltaTime;
            currentScrollSpeed = Mathf.Min(maxScrollSpeed, currentScrollSpeed);
        }

        private void GiraffeUpdate(Conductor cond)
        {
            float normalizedGiraffeBeat = cond.GetPositionFromBeat(animStartBeat, animLength);
            EasingFunction.Function func = EasingFunction.GetEasingFunction(currentEase);

            if (normalizedGiraffeBeat <= 1f && normalizedGiraffeBeat >= 0f)
            {
                switch (currentAnim)
                {
                    case GiraffeAnimation.Enter:
                        giraffe.DoNormalizedAnimation("Enter", func(0, 1, normalizedGiraffeBeat));
                        break;
                    case GiraffeAnimation.Exit:
                        giraffe.DoNormalizedAnimation("Exit", func(0, 1, normalizedGiraffeBeat));
                        break;
                    default: break;
                }
            }
        }

        private void JumpUpdate(Conductor cond)
        {
            float normalizedJumpBeat = cond.GetPositionFromBeat(jumpStartBeat, 1);

            if (normalizedJumpBeat >= 0 && normalizedJumpBeat <= 1)
            {
                if (normalizedJumpBeat >= 0.5f)
                {
                    float normalizedUp = cond.GetPositionFromBeat(jumpStartBeat, 0.5);
                    EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseOutQuad);
                    float newPlayerY = func(0, jumpHeight, normalizedUp);
                    float newMonkeyY = func(0, monkeyJumpHeight, normalizedUp);
                    rootPlayer.localPosition = new Vector3(0, newPlayerY);
                    rootMonkeyL.localPosition = new Vector3(0, newMonkeyY);
                    rootMonkeyR.localPosition = new Vector3(0, newMonkeyY);
                }
                else
                {
                    float normalizedDown = cond.GetPositionFromBeat(jumpStartBeat + 0.5, 0.5);
                    EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInQuad);
                    float newPlayerY = func(jumpHeight, 0, normalizedDown);
                    float newMonkeyY = func(monkeyJumpHeight, 0, normalizedDown);
                    rootPlayer.localPosition = new Vector3(0, newPlayerY);
                    rootMonkeyL.localPosition = new Vector3(0, newMonkeyY);
                    rootMonkeyR.localPosition = new Vector3(0, newMonkeyY);
                }
            }
            else
            {
                rootPlayer.localPosition = Vector3.zero;
                rootMonkeyL.localPosition = Vector3.zero;
                rootMonkeyR.localPosition = Vector3.zero;
            }
        }

        public enum GiraffeAnimation
        {
            Enter,
            Exit,
            Blink
        }
        private GiraffeAnimation currentAnim = GiraffeAnimation.Enter;
        private double animStartBeat = -1;
        private float animLength = 0;
        private EasingFunction.Ease currentEase = EasingFunction.Ease.Linear;

        public void GiraffeAnims(double beat, float length, int type, int ease)
        {
            animStartBeat = beat;
            animLength = length;
            currentAnim = (GiraffeAnimation)type;
            currentEase = (EasingFunction.Ease)ease;
            if (currentAnim == GiraffeAnimation.Blink) giraffe.DoScaledAnimationAsync("Blink", 0.5f);
        }

        public void Bop(double beat, float length, bool bop, bool autoBop)
        {
            if (bop)
            {
                List<BeatAction.Action> actions = new();
                for (int i = 0; i < length; i++)
                {
                    actions.Add(new BeatAction.Action(beat + i, delegate { SingleBop(); }));
                }
                BeatAction.New(this, actions);
            }
        }

        private void SingleBop()
        {
            if (!canBop) return;
            PlayMonkeyAnimationScaledAsync("Bop", 0.5f);
            player.Bop();
        }

        private bool IsEventAtBeat(double beat, double endBeat)
        {
            return EventCaller.GetAllInGameManagerList("tapTrial", new string[] { "tap", "double tap", "triple tap", "jump tap" }).Find(x => x.beat >= beat && x.beat < endBeat) != null;
        }

        public void Tap(double beat)
        {
            canBop = false;
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    PlayMonkeyAnimationScaledAsync("TapPrepare", 0.5f);
                    player.PrepareTap();
                }),
                new BeatAction.Action(beat + 1, delegate
                {
                    PlayMonkeyAnimationScaledAsync("Tap", 0.5f);
                    MonkeyParticles(true);
                }),
                new BeatAction.Action(beat + 1.5, delegate
                {
                    if (!IsEventAtBeat(beat + 1, beat + 2)) canBop = true;
                })
            });

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("tapTrial/ook", beat),
                new MultiSound.Sound("tapTrial/tapMonkey", beat + 1, 1.4f, 0.5f),
            });

            ScheduleInput(beat, 1, InputAction_BasicPress, JustTap, Miss, Empty);
        }

        public void DoubleTap(double beat)
        {
            canBop = false;
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    PlayMonkeyAnimationScaledAsync("DoubleTapPrepare", 0.5f);
                    player.PrepareTap(true);
                }),
                new BeatAction.Action(beat + 0.5, delegate
                {
                    PlayMonkeyAnimationScaledAsync("DoubleTapPrepare_2", 0.5f);
                }),
                new BeatAction.Action(beat + 1, delegate
                {
                    PlayMonkeyAnimationScaledAsync("DoubleTap", 0.5f);
                    MonkeyParticles(false);
                }),
                new BeatAction.Action(beat + 1.5, delegate
                {
                    PlayMonkeyAnimationScaledAsync("DoubleTap", 0.5f);
                    MonkeyParticles(false);
                    if (!IsEventAtBeat(beat + 1, beat + 2)) canBop = true;
                }),
            });

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("tapTrial/ookook", beat),
                new MultiSound.Sound("tapTrial/ookook", beat + 0.5),
                new MultiSound.Sound("tapTrial/tapMonkey", beat + 1, 1.4f, 0.5f),
                new MultiSound.Sound("tapTrial/tapMonkey", beat + 1.5, 1.4f, 0.5f),
            });

            ScheduleInput(beat, 1, InputAction_BasicPress, JustDoubleTap, Miss, Empty);
            ScheduleInput(beat, 1.5, InputAction_BasicPress, JustDoubleTap, Miss, Empty);
        }

        public void TripleTap(double beat)
        {
            canBop = false;
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    player.PrepareTripleTap(beat);
                    PlayMonkeyAnimationScaledAsync("PostPrepare_1", 0.5f);
                }),
                new BeatAction.Action(beat + 0.5, delegate
                {
                    PlayMonkeyAnimationScaledAsync("PostPrepare_2", 0.5f);
                }),
                new BeatAction.Action(beat + 2, delegate
                {
                    PlayMonkeyAnimationScaledAsync("PostTap", 0.5f);
                    MonkeyParticles(true);
                }),
                new BeatAction.Action(beat + 2.5, delegate
                {
                    PlayMonkeyAnimationScaledAsync("PostTap_2", 0.5f);
                    MonkeyParticles(false);
                }),
                new BeatAction.Action(beat + 3, delegate
                {
                    PlayMonkeyAnimationScaledAsync("PostTap", 0.5f);
                    MonkeyParticles(true);
                }),
            });

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("tapTrial/ooki1", beat),
                new MultiSound.Sound("tapTrial/ooki2", beat + 0.5),
                new MultiSound.Sound("tapTrial/tapMonkey", beat + 2, 1.4f, 0.5f),
                new MultiSound.Sound("tapTrial/tapMonkey", beat + 2.5, 1.4f, 0.5f),
                new MultiSound.Sound("tapTrial/tapMonkey", beat + 3, 1.4f, 0.5f),
            });

            ScheduleInput(beat, 2, InputAction_BasicPress, JustTripleTap, Miss, Empty);
            ScheduleInput(beat, 2.5, InputAction_BasicPress, JustTripleTap, Miss, Empty);
            ScheduleInput(beat, 3, InputAction_BasicPress, JustTripleTap, Miss, Empty);
        }

        public void JumpPrepare()
        {
            canBop = false;
            player.PrepareJump();
            PlayMonkeyAnimationScaledAsync("JumpPrepare", 0.5f);
        }

        public void JumpTap(double beat, bool final, bool alt)
        {
            canBop = false;
            jumpStartBeat = beat;
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    player.Jump(final);
                    PlayMonkeyAnimationScaledAsync(final ? "Jump" : "JumpTap", 0.5f);
                }),
                new BeatAction.Action(beat + 1, delegate
                {
                    PlayMonkeyAnimationScaledAsync(final ? alt ? "FinalJumpTapAlt" : "FinalJumpTap" : "Jumpactualtap", 0.5f);
                    MonkeyParticles(true);
                    MonkeyParticles(false);
                }),
                new BeatAction.Action(beat + 1.5, delegate
                {
                    if (!IsEventAtBeat(beat + 1, beat + 2)) canBop = final;
                })
            });

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound(final ? "tapTrial/jumptap2" : "tapTrial/jumptap1", beat),
                new MultiSound.Sound("tapTrial/tapMonkey", beat + 1, 1.4f, 0.5f),
            });

            ScheduleInput(beat, 1, InputAction_BasicPress, final ? alt ? JustJumpTapFinalAlt : JustJumpTapFinal : JustJumpTap, final ? MissJumpFinal : MissJump, Empty);
        }

        private void JustJumpTap(PlayerActionEvent caller, float state)
        {
            player.JumpTap(state < 1f && state > -1f, false, false);
        }

        private void JustJumpTapFinal(PlayerActionEvent caller, float state)
        {
            player.JumpTap(state < 1f && state > -1f, true, false);
        }

        private void JustJumpTapFinalAlt(PlayerActionEvent caller, float state)
        {
            player.JumpTap(state < 1f && state > -1f, true, true);
        }

        private void MissJump(PlayerActionEvent caller)
        {
            player.JumpTapMiss(false);
            if (giraffe.IsAnimationNotPlaying() && currentAnim != GiraffeAnimation.Exit) giraffe.DoScaledAnimationAsync("Miss", 0.5f);
            ResetScroll();
        }

        private void MissJumpFinal(PlayerActionEvent caller)
        {
            player.JumpTapMiss(true);
            if (giraffe.IsAnimationNotPlaying() && currentAnim != GiraffeAnimation.Exit) giraffe.DoScaledAnimationAsync("Miss", 0.5f);
            ResetScroll();
        }

        private void JustTap(PlayerActionEvent caller, float state)
        {
            player.Tap(state < 1f && state > -1f);
        }

        private void JustDoubleTap(PlayerActionEvent caller, float state)
        {
            player.Tap(state < 1f && state > -1f, true);
        }

        private void JustTripleTap(PlayerActionEvent caller, float state)
        {
            player.TripleTap(state < 1f && state > -1f);
        }

        private void Miss(PlayerActionEvent caller)
        {
            if (giraffe.IsAnimationNotPlaying() && currentAnim != GiraffeAnimation.Exit) giraffe.DoScaledAnimationAsync("Miss", 0.5f);
            ResetScroll();
        }

        private void Empty(PlayerActionEvent caller) { }

        private void PlayMonkeyAnimationScaledAsync(string name, float timeScale)
        {
            monkeyL.DoScaledAnimationAsync(name, timeScale);
            monkeyR.DoScaledAnimationAsync(name, timeScale);
        }

        private void MonkeyParticles(bool left)
        {
            ParticleSystem spawnedEffectL = Instantiate(left ? monkeyTapLL : monkeyTapLR, transform);
            spawnedEffectL.Play();

            ParticleSystem spawnedEffectR = Instantiate(left ? monkeyTapRL : monkeyTapRR, transform);
            spawnedEffectR.Play();
        }
    }
}