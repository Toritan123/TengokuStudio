using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using DG.Tweening;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using HeavenStudio.Common;
using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbHairLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            RiqEntity IntervalSwapUpdater(string datamodel, RiqEntity e)
            {
                if ((datamodel == "rhythmTweezers/simple interval" || datamodel == "rhythmTweezers/start interval") && e.version < 1)
                {
                    e.CreateProperty("autoSwap", false);
                    e.version = 1;

                    return e;
                }
                return null;
            }
            RiqBeatmap.OnUpdateEntity += IntervalSwapUpdater;

            RiqEntity ColorVeggieUpdater(string datamodel, RiqEntity e)
            {
                if (datamodel.Contains("rhythmTweezers/") && datamodel != "rhythmTweezers/set veggie color")
                {
                    if (e["colorB"] != null && e["type"] != null)
                    {
                        if (e["type"] == 1) e["colorA"] = e["colorB"];
                        e.dynamicData.Remove("colorB");
                    }
                    return e;
                }
                return null;
            }
            RiqBeatmap.OnUpdateEntity += ColorVeggieUpdater;

            return new Minigame("rhythmTweezers", "Rhythm Tweezers", "a14fa1", false, false, new List<GameAction>()
            {
                new GameAction("simple interval", "Start Interval")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        RhythmTweezers.PreInterval(e.beat, e.length, e["auto"], e["autoSwap"], e["type"], e["uniqueColor"], e["colorA"], e.version);
                    },
                    defaultLength = 4f,
                    defaultVersion = 1,
                    parameters = new List<Param>()
                    {
                        new Param("auto", true, "Auto Pass Turn", "Toggle if the turn should be passed automatically at the end of the start interval.", advanced: true),
                        new Param("autoSwap", true, "Auto Swap Vegetable", "Toggle if the vegetable should automatically swap before this interval." , new List<Param.CollapseParam>()
                        {
                        new Param.CollapseParam((x, _) => (bool)x, new string[] {"type", "uniqueColor", "colorA", "colorB", "colorC", "colorD", "colorE", "colorF", "colorG"}),
                        }),
                        new Param("type", RhythmTweezers.VegetableType.Onion, "Type", "Set the type of vegetable to switch to"),
                        new Param("uniqueColor", false, "Use Unique Color", "Make this vegetable a different color" , new List<Param.CollapseParam>()
                        {
                        new Param.CollapseParam((x, _) => (bool)x, new string[] {"colorA"}),
                        }),
                        new Param("colorA", RhythmTweezers.defaultOnionColor, "Veggie Color", "The color of the veggie."),
                    }
                },
                new GameAction("start interval", "Start Interval (Stretchy)")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        RhythmTweezers.PreInterval(e.beat, e.length, e["auto"], e["autoSwap"], e["type"], e["uniqueColor"], e["colorA"], e.version);
                    },
                    defaultLength = 4f,
                    defaultVersion = 1,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("auto", true, "Auto Pass Turn", "Toggle if the turn should be passed automatically at the end of the start interval.", advanced: true),
                        new Param("autoSwap", true, "Auto Swap Vegetable", "Toggle if the vegetable should automatically swap before this interval." , new List<Param.CollapseParam>()
                        {
                        new Param.CollapseParam((x, _) => (bool)x, new string[] {"type", "uniqueColor", "colorA", "colorB"}),
                        }),
                        new Param("type", RhythmTweezers.VegetableType.Onion, "Type", "Set the type of vegetable to switch to"),
                        new Param("uniqueColor", false, "Use Unique Color", "Make this vegetable a different color" , new List<Param.CollapseParam>()
                        {
                        new Param.CollapseParam((x, _) => (bool)x, new string[] {"colorA"}),
                        }),
                        new Param("colorA", RhythmTweezers.defaultOnionColor, "Veggie Color", "The color of the veggie."),
                    },
                    advanced = true,
                },
                new GameAction("short hair", "Short Hair")
                {
                    defaultLength = 0.5f
                },
                new GameAction("long hair", "Curly Hair")
                {
                    defaultLength = 0.5f
                },
                new GameAction("passTurn", "Pass Turn")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; RhythmTweezers.PrePassTurn(e.beat); },
                    advanced = true,
                },
                new GameAction("next vegetable", "Swap Vegetable")
                {
                    function = delegate 
                    { 
                        var e = eventCaller.currentEntity; 
                        if (!e["instant"]) RhythmTweezers.instance.NextVegetable(e.beat, e["type"], e["uniqueColor"], e["colorA"]); 
                        else RhythmTweezers.instance.ChangeVegetableImmediate(e["type"], e["uniqueColor"], e["colorA"]);
                    }, 
                    defaultLength = 0.5f, 
                    parameters = new List<Param>() 
                    {
                        new Param("type", RhythmTweezers.VegetableType.Onion, "Type", "Set the type of vegetable to switch to"),
                        new Param("uniqueColor", false, "Use Unique Color", "Make this vegetable a different color" , new List<Param.CollapseParam>()
                        {
                        new Param.CollapseParam((x, _) => (bool)x, new string[] {"colorA"}),
                        }),
                        new Param("colorA", RhythmTweezers.defaultOnionColor, "Veggie Color", "The color of the veggie."),
                        new Param("instant", false, "Instant", "Toggle if the vegetable should skip the moving animation and instantly swap.")
                    },
                    priority = 3
                },
                new GameAction("noPeek", "No Peeking Sign")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; RhythmTweezers.PreNoPeeking(e.beat, e.length, e["type"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("type", RhythmTweezers.NoPeekSignType.Full, "Side", "Set the side for the sign to appear on.")
                    }
                },
                new GameAction("fade background color", "Background Appearance")
                {
                    function = delegate 
                    { 
                        var e = eventCaller.currentEntity;
                        RhythmTweezers.instance.BackgroundColor(e.beat, e.length, e["colorA"], e["colorB"], e["ease"]);
                    },
                    resizable = true, 
                    parameters = new List<Param>() 
                    {
                        
                        new Param("colorA", Color.white, "Start Color", "Set the color at the start of the event."),
                        new Param("colorB", RhythmTweezers.defaultBgColor, "End Color", "Set the color at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    } 
                },
                new GameAction("set veggie color", "Set Default Vegetable Colors")
                {
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        RhythmTweezers.instance.DefaultVeggieColor(e.beat, e["colorA"], e["colorB"],  e["colorC"], e["colorD"], e["colorE"], e["colorF"], e["colorG"]);
                    },
                    resizable = false,
                    defaultLength = 0.5f, 
                    parameters = new List<Param>()
                    {
                        new Param("colorA", RhythmTweezers.defaultOnionColor, "Onion Color", "The default color of the onions."),
                        new Param("colorB", RhythmTweezers.defaultPotatoColor, "Potato Color", "The default color of the potatos."),
                        new Param("colorC", RhythmTweezers.defaultBeetColor, "Beet Color", "The default color of the beets."),
                        new Param("colorD", RhythmTweezers.defaultPumpkinColor, "Pumpkin Color", "The default color of the pumpkins."),
                        new Param("colorE", RhythmTweezers.defaultRadishColor, "Radish Color", "The default color of the radishes."),
                        new Param("colorF", RhythmTweezers.defaultMelonColor, "Watermelon Color", "The default color of the Watermelons."),
                        new Param("colorG", RhythmTweezers.defaultBulbColor, "Lightbulb Color", "The default color of the Lightbulbs.")
                    }
                },
                new GameAction("altSmile", "Use Alt Smile")
                {
                    function = delegate
                    {
                    var e = eventCaller.currentEntity; 
		    RhythmTweezers.instance.SetSmile(e.beat, e["type"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("type", RhythmTweezers.SmileType.Default, "Smile Type", "Set the smile to set for the vegatables."),
                    },
                    defaultLength = 0.5f
                },
                //backwards compatibility
                new GameAction("change vegetable", "Change Vegetable (Instant)")
                {
                    function = delegate { var e = eventCaller.currentEntity; RhythmTweezers.instance.ChangeVegetableImmediate(e["type"], e["uniqueColor"], e["colorA"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("type", RhythmTweezers.VegetableType.Onion, "Type", "The vegetable to switch to"),
                        new Param("uniqueColor", false, "Use Unique Color", "Make this vegetable a different color" , new List<Param.CollapseParam>()
                        {
                        new Param.CollapseParam((x, _) => (bool)x, new string[] {"colorA"}),
                        }),
                        new Param("colorA", RhythmTweezers.defaultOnionColor, "Veggie Color", "The color of the veggie."),
                    },
                    hidden = true,
                },
            },
            new List<string>() {"agb", "repeat"},
            "agbhair", "en",
            new List<string>() {},
            chronologicalSortKey: 2,
            font : "Matisse Minori"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Jukebox;
    using Scripts_RhythmTweezers;

    public class RhythmTweezers : Minigame
    {
        public enum VegetableType
        {
            Onion,
            Potato,
	    Beet,
	    Pumpkin,
	    Radish,
	    Watermelon,
	    Lightbulb
        }

        public enum NoPeekSignType
        {
            Full,
            HalfRight,
            HalfLeft
        }

        private struct QueuedPeek
        {
            public double beat;
            public float length;
            public int type;
        }

        public enum SmileType
        {
            Default,
            Tengoku,
            Alt
        }

        [Header("References")]
        public Transform VegetableHolder;
        public SpriteRenderer Vegetable;
        public SpriteRenderer VegetableDupe;
        public Animator VegetableAnimator;
        public SpriteRenderer bg;
        public Tweezers Tweezers;
        private Tweezers currentTweezers;
        public GameObject hairBase;
        public GameObject longHairBase;
        public GameObject pluckedHairBase;
        public GameObject LightbulbBase, LightbulbBasenext, LightHolder;
        [SerializeField] NoPeekingSign noPeekingRef;

        public GameObject HairsHolder;
        public GameObject DroppedHairsHolder;

        public SpriteRenderer Hair, Stubble, Missed, Loop, LongHair, LongStubble, Plucked, PlayerPluck;

        [Header("Variables")]
        private static List<QueuedPeek> queuedPeeks = new List<QueuedPeek>();

        [Header("Sprites")]
        public Sprite pluckedHairSprite;
        public Sprite missedHairSprite;
        public Sprite onionSprite;
        public Sprite potatoSprite;
        public Sprite beetSprite;
        public Sprite pumpkinSprite;
        public Sprite radishSprite;
        public Sprite watermelonSprite;
        public Sprite bulbSprite;

        public bool transitioning = false;

        private static Color _defaultOnionColor;
        public static Color defaultOnionColor = new(0.7843137254901961f, 0.588235294117f, 0f);
        //{
        //    get
        //    {
       //         ColorUtility.TryParseHtmlString("#C89600", out _defaultOnionColor);
        //        return _defaultOnionColor;
        //    }
        //}

        public Color onionDefault = defaultOnionColor;
        private Color onionColorGlobal = defaultOnionColor;

        private static Color _defaultPotatoColor;
        public static Color defaultPotatoColor = new(1f, 0.8627450980392157f, 0f);
        //{
        //    get
        //    {
        //        ColorUtility.TryParseHtmlString("#FFDC00", out _defaultPotatoColor);
        //        return _defaultPotatoColor;
        //    }
        //}

        public Color potatoDefault = defaultPotatoColor;
        private Color potatoColorGlobal = defaultPotatoColor;

        private static Color _defaultBeetColor;
        public static Color defaultBeetColor = new(1f, 0.5411765f, 0.9607843f);
        //{
        //    get
        //    {
        //        ColorUtility.TryParseHtmlString("#FF8AF5FF", out _defaultBeetColor);
        //        return _defaultBeetColor;
        //    }
        //}

        public Color beetDefault = defaultBeetColor;
        private Color beetColorGlobal = defaultBeetColor;

        private static Color _defaultPumpkinColor;
        public static Color defaultPumpkinColor = new(1f, 0.454902f, 0.2392157f);
        //{
        //    get
        //    {
        //        ColorUtility.TryParseHtmlString("#FF743DFF", out _defaultPumpkinColor);
        //        return _defaultPumpkinColor;
        //    }
        //}

        public Color pumpkinDefault = defaultPumpkinColor;
        private Color pumpkinColorGlobal = defaultPumpkinColor;

        private static Color _defaultRadishColor;
        public static Color defaultRadishColor = new(1f, 0.9215686f, 0.7215686f);
        //{
        //    get
        //    {
        //        ColorUtility.TryParseHtmlString("#FFEBB8FF", out _defaultRadishColor);
        //        return _defaultRadishColor;
        //    }
        //}

        public Color radishDefault = defaultRadishColor;
        private Color radishColorGlobal = defaultRadishColor;

        private static Color _defaultMelonColor;
        public static Color defaultMelonColor = new(0.1294118f, 1f, 0f);
        //{
        //    get
        //    {
        //        ColorUtility.TryParseHtmlString("#21FF00", out _defaultMelonColor);
        //        return _defaultMelonColor;
        //    }
        //}

        public Color melonDefault = defaultMelonColor;
        private Color melonColorGlobal = defaultMelonColor;

        private static Color _defaultBulbColor;
        public static Color defaultBulbColor = new(0.6179246f, 0.7467621f, 1f, 0.5f);
        //{
        //    get
        //    {
        //        ColorUtility.TryParseHtmlString("#21FF00", out _defaultBulbColor);
        //        return _defaultBulbColor;
        //    }
        //}

        public Color bulbDefault = defaultBulbColor;
        private Color bulbColorGlobal = defaultBulbColor;

        public static Color defaultBgColor = new(0.631f, 0.31f, 0.631f);

        private bool firstVegetable = true;

        public Color hairColorDefault;
        public Color hairColorBulb;

        public static RhythmTweezers instance { get; set; }
        private static CallAndResponseHandler crHandlerInstance;

        private List<Hair> spawnedHairs = new List<Hair>();
        private List<LongHair> spawnedLongs = new List<LongHair>();

        private static List<double> passedTurns = new();
        private struct QueuedInterval
        {
            public double beat;
            public float interval;
            public bool autoPassTurn;
            public bool autoSwapVeggie;
            public int type;
            public bool autoColor;
            public Color colorA;
        }
        private static List<QueuedInterval> queuedIntervals = new List<QueuedInterval>();

        protected static bool IA_PadAnyDown(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.East, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Up, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Down, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Right, out dt);
        }
        protected static bool IA_PadAnyUp(out double dt)
        {
            bool face = PlayerInput.GetPadUp(InputController.ActionsPad.East, out dt)
                && !(PlayerInput.GetPad(InputController.ActionsPad.Up)
                    || PlayerInput.GetPad(InputController.ActionsPad.Down)
                    || PlayerInput.GetPad(InputController.ActionsPad.Left)
                    || PlayerInput.GetPad(InputController.ActionsPad.Right));
            bool pad = (PlayerInput.GetPadUp(InputController.ActionsPad.Up)
                    || PlayerInput.GetPadUp(InputController.ActionsPad.Down)
                    || PlayerInput.GetPadUp(InputController.ActionsPad.Left)
                    || PlayerInput.GetPadUp(InputController.ActionsPad.Right))
                && !PlayerInput.GetPad(InputController.ActionsPad.East);
            return face || pad;
        }
        public static PlayerInput.InputAction InputAction_Press =
            new("AgbHairPress", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_PadAnyDown, IA_TouchBasicPress, IA_BatonBasicPress);
        public static PlayerInput.InputAction InputAction_Release =
            new("AgbHairRelease", new int[] { IAReleaseCat, IAReleaseCat, IAReleaseCat },
            IA_PadAnyUp, IA_TouchBasicRelease, IA_BatonBasicRelease);

        private void Awake()
        {
            instance = this;
            if (crHandlerInstance != null && crHandlerInstance.queuedEvents.Count > 0)
            {
                foreach (var crEvent in crHandlerInstance.queuedEvents)
                {
                    if (crEvent.tag == "Hair")
                    {
                        Hair hair = Instantiate(hairBase, HairsHolder.transform).GetComponent<Hair>();
                        spawnedHairs.Add(hair);
                        hair.gameObject.SetActive(true);
                        hair.GetComponent<Animator>().Play("SmallAppear", 0, 1);
                        float rot = -58f + 116 * MathUtils.Normalize((float)crEvent.relativeBeat, 0, crHandlerInstance.intervalLength - 1);
                        hair.transform.eulerAngles = new Vector3(0, 0, rot);
                        hair.createBeat = crEvent.beat;
                    }
                    else if (crEvent.tag == "Long")
                    {
                        LongHair hair = Instantiate(longHairBase, HairsHolder.transform).GetComponent<LongHair>();
                        spawnedLongs.Add(hair);
                        hair.gameObject.SetActive(true);
                        hair.GetComponent<Animator>().Play("LongAppear", 0, 1);
                        float rot = -58f + 116 * MathUtils.Normalize((float)crEvent.relativeBeat, 0, crHandlerInstance.intervalLength - 1);
                        hair.transform.eulerAngles = new Vector3(0, 0, rot);
                        hair.createBeat = crEvent.beat;
                    }
                }
            }
        }

        public override void OnPlay(double beat)
        {
            crHandlerInstance = null;
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
            queuedIntervals.Clear();
            PersistBlocks(beat);
        }

        private void OnDestroy()
        {
            if (!conductor.isPlaying)
            {
                crHandlerInstance = null;
            }
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
            // note: this doesn't properly clear queued intervals if we don't switch to the game before stopping playback
            queuedIntervals.Clear();
        }

        private void SpawnHairInactive(double beat)
        {
            if (crHandlerInstance.queuedEvents.Count > 0 && crHandlerInstance.queuedEvents.Find(x => x.beat == beat || (beat >= x.beat && beat <= x.beat + x.length)) != null) return;
            crHandlerInstance.AddEvent(beat, 0, "Hair");
            Hair hair = Instantiate(hairBase, HairsHolder.transform).GetComponent<Hair>();
            spawnedHairs.Add(hair);
            hair.gameObject.SetActive(true);
            hair.GetComponent<Animator>().Play("SmallAppear", 0, 1);
            float rot = -58f + 116 * crHandlerInstance.GetIntervalProgressFromBeat(conductor.GetUnSwungBeat(beat), 1);
            hair.transform.eulerAngles = new Vector3(0, 0, rot);
            hair.createBeat = beat;
        }

        private void SpawnLongHairInactive(double beat)
        {
            if (crHandlerInstance.queuedEvents.Count > 0 && crHandlerInstance.queuedEvents.Find(x => x.beat == beat || (beat >= x.beat && beat <= x.beat + x.length)) != null) return;
            crHandlerInstance.AddEvent(beat, 0.5f, "Long");
            LongHair hair = Instantiate(longHairBase, HairsHolder.transform).GetComponent<LongHair>();
            spawnedLongs.Add(hair);
            hair.gameObject.SetActive(true);
            hair.GetComponent<Animator>().Play("LongAppear", 0, 1);
            float rot = -58f + 116 * crHandlerInstance.GetIntervalProgressFromBeat(conductor.GetUnSwungBeat(beat), 1);
            hair.transform.eulerAngles = new Vector3(0, 0, rot);
            hair.createBeat = beat;
        }

        public void SpawnHair(double beat)
        {
            if (crHandlerInstance.queuedEvents.Count > 0 && crHandlerInstance.queuedEvents.Find(x => x.beat == beat || (beat >= x.beat && beat <= x.beat + x.length)) != null) return;
            // End transition early if the next hair is a lil early.
            StopTransitionIfActive();

            crHandlerInstance.AddEvent(beat, 0, "Hair");

            SoundByte.PlayOneShotGame("rhythmTweezers/shortAppear", beat);
            Hair hair = Instantiate(hairBase, transform).GetComponent<Hair>();
            spawnedHairs.Add(hair);

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    hair.gameObject.SetActive(true);
                    hair.GetComponent<Animator>().Play("SmallAppear", 0, 0);
                    hair.transform.SetParent(HairsHolder.transform, false);
                })
            });

            float rot = -58f + 116 * crHandlerInstance.GetIntervalProgressFromBeat(conductor.GetUnSwungBeat(beat), 1);
            hair.transform.eulerAngles = new Vector3(0, 0, rot);
            hair.createBeat = beat;
        }

        public void SpawnLongHair(double beat)
        {
            if (crHandlerInstance.queuedEvents.Count > 0 && crHandlerInstance.queuedEvents.Find(x => x.beat == beat || (beat >= x.beat && beat <= x.beat + x.length)) != null) return;
            StopTransitionIfActive();

            crHandlerInstance.AddEvent(beat, 0.5f, "Long");

            SoundByte.PlayOneShotGame("rhythmTweezers/longAppear", beat);
            LongHair hair = Instantiate(longHairBase, transform).GetComponent<LongHair>();
            spawnedLongs.Add(hair);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    hair.gameObject.SetActive(true);
                    hair.GetComponent<Animator>().Play("LongAppear", 0, 0);
                    hair.transform.SetParent(HairsHolder.transform, false);
                })
            });

            float rot = -58f + 116 * crHandlerInstance.GetIntervalProgressFromBeat(conductor.GetUnSwungBeat(beat), 1);
            hair.transform.eulerAngles = new Vector3(0, 0, rot);
            hair.createBeat = beat;
        }

        private void SetIntervalStart(double beat, double gameSwitchBeat, float interval, bool autoPassTurn, bool autoSwapVeggie, int type, bool autoColor, Color colorA)
        {
            if (instance.firstVegetable)
            {
                instance.firstVegetable = false;
                if (autoSwapVeggie) instance.ChangeVegetableImmediate(type, autoColor, colorA);
            }
            else if (autoSwapVeggie)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat - 0.5f, delegate { instance.NextVegetable(beat - 0.5f, type, autoColor, colorA); })
                });
            }
                
            StopTransitionIfActive();
            CallAndResponseHandler newHandler = new();
            crHandlerInstance = newHandler;
            crHandlerInstance.StartInterval(beat, interval);
            List<RiqEntity> relevantHairEvents = GetAllHairsInBetweenBeat(beat, beat + interval);
            foreach (var hairEvent in relevantHairEvents)
            {
                if (hairEvent.beat >= gameSwitchBeat)
                {
                    if (hairEvent.datamodel == "rhythmTweezers/short hair")
                    {
                        SpawnHair(hairEvent.beat);
                    }
                    else
                    {
                        SpawnLongHair(hairEvent.beat);
                    }
                }
                else
                {
                    if (hairEvent.datamodel == "rhythmTweezers/short hair")
                    {
                        SpawnHairInactive(hairEvent.beat);
                    }
                    else
                    {
                        SpawnLongHairInactive(hairEvent.beat);
                    }
                }
            }
            if (autoPassTurn)
            {
                PassTurn(beat + interval, interval, newHandler);
            }
        }

        public static void PreInterval(double beat, float interval, bool autoPassTurn, bool autoSwapVeggie, int type, bool autoColor, Color colorA, int version)
        {
            if (version == 0) autoSwapVeggie = false;

            if (GameManager.instance.currentGame == "rhythmTweezers")
            {
                instance.SetIntervalStart(beat, beat, interval, autoPassTurn, autoSwapVeggie, type, autoColor, colorA);
            }
            else
            {
                queuedIntervals.Add(new QueuedInterval()
                {
                    beat = beat,
                    interval = interval,
                    autoPassTurn = autoPassTurn,
                    autoSwapVeggie = autoSwapVeggie,
                    type = type,
                    autoColor = autoColor,
                    colorA = colorA,
                });
            }
        }

        private static List<RiqEntity> GetAllHairsInBetweenBeat(double beat, double endBeat)
        {
            List<RiqEntity> hairEvents = EventCaller.GetAllInGameManagerList("rhythmTweezers", new string[] { "short hair", "long hair"});
            List<RiqEntity> tempEvents = new();

            foreach (var entity in hairEvents)
            {
                if (entity.beat >= beat && entity.beat < endBeat)
                {
                    tempEvents.Add(entity);
                }
            }
            return tempEvents;
        }

        private void PassTurnStandalone(double beat)
        {
            if (crHandlerInstance != null) PassTurn(beat, crHandlerInstance.intervalLength, crHandlerInstance);
        }

        private void PassTurn(double beat, double length, CallAndResponseHandler crHandler)
        {
            Tweezers spawnedTweezers = Instantiate(Tweezers, transform);
            spawnedTweezers.gameObject.SetActive(true);
            spawnedTweezers.Init(beat, beat + length);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 0.25, delegate
                {
                    if (crHandler.queuedEvents.Count > 0)
                    {
                        currentTweezers = spawnedTweezers;
                        spawnedTweezers.hairsLeft = crHandler.queuedEvents.Count;
                        foreach (var crEvent in crHandler.queuedEvents)
                        {
                            if (crEvent.tag == "Hair")
                            {
                                Hair hairToInput = spawnedHairs.Find(x => x.createBeat == crEvent.beat);
                                hairToInput.StartInput(beat, crEvent.relativeBeat, spawnedTweezers);
                            }
                            else if (crEvent.tag == "Long")
                            {
                                LongHair hairToInput = spawnedLongs.Find(x => x.createBeat == crEvent.beat);
                                hairToInput.StartInput(beat, crEvent.relativeBeat, spawnedTweezers);
                            }
                        }
                        crHandler.queuedEvents.Clear();
                    }

                }),
            });
        }

        public static void PrePassTurn(double beat)
        {
            if (GameManager.instance.currentGame == "rhythmTweezers")
            {
                instance.PassTurnStandalone(beat);
            }
            else
            {
                passedTurns.Add(beat);
            }
        }

        Tween transitionTween;

        const float vegDupeOffset = 16.7f;
        public void NextVegetable(double beat, int type, bool unique, Color onionColor)
        {
            transitioning = true;

            SoundByte.PlayOneShotGame("rhythmTweezers/register", beat);

            //Sprite nextVeggieSprite = type == 0 ? onionSprite : potatoSprite;
		Sprite nextVeggieSprite = onionSprite;
		switch (type)
		{
		case 0:
		nextVeggieSprite = onionSprite;
		break;
		case 1:
		nextVeggieSprite = potatoSprite;
		break;
		case 2:
	 	nextVeggieSprite = beetSprite;
		break;
		case 3:
	 	nextVeggieSprite = pumpkinSprite;
		break;
		case 4:
	 	nextVeggieSprite = radishSprite;
		break;
		case 5:
	 	nextVeggieSprite = watermelonSprite;
		break;
		case 6:
	 	nextVeggieSprite = bulbSprite;
		break;
		}

            //Color oldColor = type == 0 ? onionColorGlobal:potatoColorGlobal;
            //Color nextColor = type == 0 ? onionColor : potatoColor;

		Color oldColor = onionColorGlobal;

		switch (type)
		{
		case 0:
		oldColor = onionColorGlobal;
		break;
		case 1:
		oldColor = potatoColorGlobal;
		break;
		case 2:
	 	oldColor = beetColorGlobal;
		break;
		case 3:
	 	oldColor = pumpkinColorGlobal;
		break;
		case 4:
	 	oldColor = radishColorGlobal;
		break;
		case 5:
	 	oldColor = melonColorGlobal;
		break;
		case 6:
	 	oldColor = bulbColorGlobal;
		break;
		}

		Color nextColor = onionColor;

            if (unique)
            {
                VegetableDupe.sprite = nextVeggieSprite;
                VegetableDupe.color = nextColor;
		if (type == 6)
		{
		LightbulbBasenext.SetActive(true);
		}
		else
		{
		LightbulbBasenext.SetActive(false);
		}
            }
            else
            {
                VegetableDupe.sprite = nextVeggieSprite;
                VegetableDupe.color = oldColor;
		if (type == 6)
		{
		LightbulbBasenext.SetActive(true);
		}
		else
		{
		LightbulbBasenext.SetActive(false);
		}
            }

            // Move both vegetables to the left by vegDupeOffset, then reset their positions.
            // On position reset, reset state of core vegetable.
            transitionTween = VegetableHolder.DOLocalMoveX(-vegDupeOffset, Conductor.instance.secPerBeat * 0.45f / Conductor.instance.musicSource.pitch)
                .OnComplete(() => {

                var holderPos = VegetableHolder.localPosition;
                VegetableHolder.localPosition = new Vector3(0f, holderPos.y, holderPos.z);

                if (unique)
                {
                    VegetableDupe.sprite = nextVeggieSprite;
                    VegetableDupe.color = nextColor;
		    LightbulbBasenext.SetActive(false);
                    Vegetable.sprite = nextVeggieSprite;
                    Vegetable.color = nextColor;
		    if (type == 6)
		    {
		    LightbulbBase.SetActive(true);
		LightHolder.SetActive(true);

		    }
		    else
		    {
		    LightbulbBase.SetActive(false);
		LightHolder.SetActive(false);
		    }
                }
                else
                {
                    VegetableDupe.sprite = nextVeggieSprite;
                    VegetableDupe.color = oldColor;
		    LightbulbBasenext.SetActive(false);
                    Vegetable.sprite = nextVeggieSprite;
                    Vegetable.color = oldColor;
		    if (type == 6)
		    {
		    LightbulbBase.SetActive(true);
		LightHolder.SetActive(true);

		    }
		    else
		    {
		    LightbulbBase.SetActive(false);
		    LightHolder.SetActive(false);
		    }
                }

                ResetVegetable();
                transitioning = false;

            }).SetEase(Ease.InOutSine);
        }

        public void ChangeVegetableImmediate(int type, bool unique, Color onionColor)
        {
            StopTransitionIfActive();
            
            //Sprite newSprite = type == 0 ? onionSprite : potatoSprite;
            //Color oldColor = type == 0 ? onionColorGlobal:potatoColorGlobal;
            //Color newColor = type == 0 ? onionColor : potatoColor;

            //Sprite nextVeggieSprite = type == 0 ? onionSprite : potatoSprite;
		Sprite newSprite = onionSprite;
		switch (type)
		{
		case 0:
		newSprite = onionSprite;
		break;
		case 1:
		newSprite = potatoSprite;
		break;
		case 2:
	 	newSprite = beetSprite;
		break;
		case 3:
	 	newSprite = pumpkinSprite;
		break;
		case 4:
	 	newSprite = radishSprite;
		break;
		case 5:
	 	newSprite = watermelonSprite;
		break;
		case 6:
	 	newSprite = bulbSprite;
		break;
		}

            //Color oldColor = type == 0 ? onionColorGlobal:potatoColorGlobal;
            //Color nextColor = type == 0 ? onionColor : potatoColor;

		Color oldColor = onionColorGlobal;

		switch (type)
		{
		case 0:
		oldColor = onionColorGlobal;
		break;
		case 1:
		oldColor = potatoColorGlobal;
		break;
		case 2:
	 	oldColor = beetColorGlobal;
		break;
		case 3:
	 	oldColor = pumpkinColorGlobal;
		break;
		case 4:
	 	oldColor = radishColorGlobal;
		break;
		case 5:
	 	oldColor = melonColorGlobal;
		break;
		case 6:
	 	oldColor = bulbColorGlobal;
		break;
		}

		Color newColor = onionColor;

            Vegetable.sprite = newSprite;
            
            VegetableDupe.sprite = newSprite;
            

            if (unique)
            {
                Vegetable.color = newColor;
                VegetableDupe.color = newColor;
		    if (type == 6)
		    {
        		//Hair.color = hairColorBulb;
        		//Stubble.color = hairColorBulb;
        		//Missed.color = hairColorBulb;
       	 		//Loop.color = hairColorBulb;
        		//LongHair.color = hairColorBulb;
        		//LongStubble.color = hairColorBulb;
        		//Plucked.color = hairColorBulb;
        		//PlayerPluck.color = hairColorBulb;
		    LightbulbBase.SetActive(true);
		    LightbulbBasenext.SetActive(true);
		LightHolder.SetActive(true);

		    }
		    else
		    {
        		//Hair.color = hairColorDefault;
        		//Stubble.color = hairColorDefault;
        		//Missed.color = hairColorDefault;
       	 		//Loop.color = hairColorDefault;
        		//LongHair.color = hairColorDefault;
        		//LongStubble.color = hairColorDefault;
        		//Plucked.color = hairColorDefault;
        		//PlayerPluck.color = hairColorDefault;
		    LightbulbBase.SetActive(false);
		    LightbulbBasenext.SetActive(false);
		LightHolder.SetActive(false);
		    }
            }
            else
            {
                Vegetable.color = oldColor;
                VegetableDupe.color = oldColor;
		    if (type == 6)
		    {
        		//Hair.color = hairColorBulb;
        		//Stubble.color = hairColorBulb;
        		//Missed.color = hairColorBulb;
       	 		//Loop.color = hairColorBulb;
        		//LongHair.color = hairColorBulb;
        		//LongStubble.color = hairColorBulb;
        		//Plucked.color = hairColorBulb;
        		//PlayerPluck.color = hairColorBulb;
		    LightbulbBase.SetActive(true);
		    LightbulbBasenext.SetActive(true);
		LightHolder.SetActive(true);
		    }
		    else
		    {
        		//Hair.color = hairColorDefault;
        		//Stubble.color = hairColorDefault;
        		//Missed.color = hairColorDefault;
       	 		//Loop.color = hairColorDefault;
        		//LongHair.color = hairColorDefault;
        		//LongStubble.color = hairColorDefault;
        		//Plucked.color = hairColorDefault;
        		//PlayerPluck.color = hairColorDefault;
		    LightbulbBase.SetActive(false);
		    LightbulbBasenext.SetActive(false);
		LightHolder.SetActive(false);
		    }
            }
        }

        private ColorEase bgColorEase = new(defaultBgColor);

        public void BackgroundColor(double beat, float length, Color startColor, Color endColor, int ease)
        {
            bgColorEase = new(beat, length, startColor, endColor, ease);
        }

        public void DefaultVeggieColor(double beat, Color onionColor, Color potatoColor, Color beetColor, Color pumpkinColor, Color radishColor, Color melonColor, Color bulbColor)
        {
            onionColorGlobal = onionColor;
            potatoColorGlobal = potatoColor;
            beetColorGlobal = beetColor;
            pumpkinColorGlobal = pumpkinColor;
            radishColorGlobal = radishColor;
            melonColorGlobal = melonColor;
            bulbColorGlobal = bulbColor;
        }

        //call this in OnPlay(double beat) and OnGameSwitch(double beat)
        private void PersistBlocks(double beat)
        {
            var allEventsBeforeBeat = GameManager.instance.Beatmap.Entities.FindAll(x => x.datamodel.Split('/')[0] == "rhythmTweezers" && x.beat < beat);
            var lastColorEvent = allEventsBeforeBeat.FindLast(x => x.datamodel == "rhythmTweezers/fade background color");
            if (lastColorEvent != null)
            {
                var e = lastColorEvent;
                BackgroundColor(e.beat, e.length, e["colorA"], e["colorB"], e["ease"]);
            }
            var lastVeggieEvent = allEventsBeforeBeat.FindLast(x => x.datamodel == "rhythmTweezers/set veggie color");
            if (lastVeggieEvent !=null)
            {
                var ex = lastVeggieEvent;
                DefaultVeggieColor(ex.beat, ex["colorA"], ex["colorB"], ex["colorC"], ex["colorD"], ex["colorE"], ex["colorF"], ex["colorG"]);
            }
            var allAltFaceEventsBeforeBeat = allEventsBeforeBeat.FindAll(x => x.datamodel == "rhythmTweezers/altSmile");
            VegetableAnimator.SetBool("UseAltSmile", allAltFaceEventsBeforeBeat.Count % 2 == 1);
        }

        public static void PreNoPeeking(double beat, float length, int type)
        {
            if (GameManager.instance.currentGame == "rhythmTweezers")
            {
                instance.NoPeeking(beat, length, type);
            }
            else
            {
                queuedPeeks.Add(new QueuedPeek()
                {
                    beat = beat,
                    length = length,
                    type = type
                });
            }
        }

        public void NoPeeking(double beat, float length, int type)
        {
            NoPeekingSign spawnedNoPeekingSign = Instantiate(noPeekingRef, transform);
            spawnedNoPeekingSign.gameObject.SetActive(true);
            spawnedNoPeekingSign.Init(beat, length, type);
        }

        private void Update()
        {
            if (Conductor.instance.isPlaying && !Conductor.instance.isPaused)
            {
                if (passedTurns.Count > 0)
                {
                    foreach (var turn in passedTurns)
                    {
                        PassTurnStandalone(turn);
                    }
                    passedTurns.Clear();
                }
                if (queuedPeeks.Count > 0)
                {
                    foreach (var peek in queuedPeeks)
                    {
                        NoPeeking(peek.beat, peek.length, peek.type);
                    }
                    queuedPeeks.Clear();
                }
            }

            bg.color = bgColorEase.GetColor();
        }

        public override void OnGameSwitch(double beat)
        {
            if (Conductor.instance.isPlaying && !Conductor.instance.isPaused)
            {
                if (queuedIntervals.Count > 0)
                {
                    foreach (var interval in queuedIntervals)
                    {
                        SetIntervalStart(interval.beat, beat, interval.interval, interval.autoPassTurn, interval.autoSwapVeggie, interval.type, interval.autoColor, interval.colorA);
                    }
                    queuedIntervals.Clear();
                }
            }
            PersistBlocks(beat);
        }

        private void ResetVegetable()
        {
            // If the tweezers happen to be holding a hair, drop it immediately so it can be destroyed below.
            currentTweezers?.DropHeldHair();

            foreach (Transform t in HairsHolder.transform)
            {
                GameObject.Destroy(t.gameObject);
            }

            foreach (Transform t in DroppedHairsHolder.transform)
            {
                GameObject.Destroy(t.gameObject);
            }

            VegetableAnimator.Play("Idle", 0, 0);
        }

        private void StopTransitionIfActive()
        {
            if (transitioning)
            {
                if (transitionTween != null)
                {
                    transitionTween.Complete(true);
                    transitionTween.Kill();
                }
            }
        }

	public void SetSmile(double beat, int type)
	{
		switch (type)
		{
			case 1:
			VegetableAnimator.SetBool("UseAltSmile", false);
			VegetableAnimator.SetBool("UseOpenEyeSmile", true);
			break;
			case 2:
			VegetableAnimator.SetBool("UseAltSmile", true);
			VegetableAnimator.SetBool("UseOpenEyeSmile", false);
			break;
			default:
			VegetableAnimator.SetBool("UseAltSmile", false);
			VegetableAnimator.SetBool("UseOpenEyeSmile", false);
			break;
		}
	}
    }
}
