using HeavenStudio.Games.Scripts_HiHat;
using HeavenStudio.Games.Scripts_MeatGrinder;
using HeavenStudio.Games.Scripts_ValiantVolley;
using HeavenStudio.InputSystem;
using HeavenStudio.Util;
using Jukebox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static HeavenStudio.Games.Minigame;
using static HeavenStudio.Games.SumoBrothers;

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
    public static class RvlHiHatLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("hiHat", "Hi-Hat", "ffce3a", false, false, new List<GameAction>()
            {
                new GameAction("4beat", "4-Beat Pattern")
                {
                    preFunctionLength = 1f,
                    preFunction = delegate {
                        HiHat.instance.currentPattern = 4;
                        if (GameManager.instance.currentGame != "hiHat") return;
                        var e = eventCaller.currentEntity; HiHat.instance.SpawnPattern(e.beat, 4);
                    },
                    defaultLength = 4f,
                    resizable = false,
                },

                new GameAction("8beat", "8-Beat Pattern")
                {
                    preFunctionLength = 1f,
                    preFunction = delegate {
                        HiHat.instance.currentPattern = 8;
                        if (GameManager.instance.currentGame != "hiHat") return;
                        var e = eventCaller.currentEntity; HiHat.instance.SpawnPattern(e.beat, 8);
                    },
                    defaultLength = 4f,
                    resizable = false,

                },

                new GameAction("16beat", "16-Beat Pattern")
                {
                    preFunctionLength = 1f,
                    preFunction = delegate {
                        HiHat.instance.currentPattern = 16;
                        if (GameManager.instance.currentGame != "hiHat") return;
                        var e = eventCaller.currentEntity; HiHat.instance.SpawnPattern(e.beat, 16);
                    },
                    defaultLength = 4f,
                    resizable = false,
                },

                new GameAction("singleDoll", "Spawn Single Doll")
                {
                    preFunctionLength = 1f,
                    preFunction = delegate {
                        if (GameManager.instance.currentGame != "hiHat") return;
                        var e = eventCaller.currentEntity;
                        var position = new Vector2();
                        List<Vector2> posList = new List<Vector2>{HiHat.instance.pos41, HiHat.instance.pos42, HiHat.instance.pos43, HiHat.instance.pos44,
                        HiHat.instance.pos81, HiHat.instance.pos82, HiHat.instance.pos42, HiHat.instance.pos84, HiHat.instance.pos85, HiHat.instance.pos43, HiHat.instance.pos87, HiHat.instance.pos88,
                        HiHat.instance.pos161, HiHat.instance.pos162, HiHat.instance.pos81, HiHat.instance.pos82, HiHat.instance.pos42, HiHat.instance.pos84, HiHat.instance.pos85, HiHat.instance.pos43, HiHat.instance.pos87, HiHat.instance.pos88,
                        HiHat.instance.pos1610, HiHat.instance.pos1611,HiHat.instance.pos1612, HiHat.instance.pos1613};
                        if (e["basePositions"] == 0) position = posList[e["pos4"] - 1];
                        if (e["basePositions"] == 1) position = posList[e["pos8"] + 3];
                        if (e["basePositions"] == 2) position = posList[e["pos16"] + 11];

                        var behind = false;
                        if (e["basePositions"] == 2 && (e["pos16"] <= 2 || e["pos16"] >= 11)) behind = true;

                        var isShadowHigher = false;
                        if (e["isBig"])
                        {
                            if (e["basePositions"] == 0)
                            {
                                if (e["pos4"] <= 3) isShadowHigher = true;
                            }
                            else if (e["basePositions"] == 1)
                            {
                                if (e["pos8"] <= 7) isShadowHigher = true;
                            }
                            else if (e["basePositions"] == 2)
                            {
                                if (e["pos16"] > 2 && e["pos16"] <= 10) isShadowHigher = true;
                            }
                        }

                        HiHat.instance.SpawnDoll(e.beat, e["isBig"], position, 0, false, behind, isShadowHigher);
                    },
                    defaultLength = 4f,
                    resizable = false,
                    advanced = true,
                    parameters = new List<Param>()
                    {
                        new Param("isBig", false, "Big Doll", "Set if the doll spawned is a big doll."),
                        new Param("basePositions", HiHat.PatternTypes.Four, "Base Position", "Choose which pattern the slider will base its positions on.",new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x == (int)HiHat.PatternTypes.Four, new string[] { "pos4" }),
                            new Param.CollapseParam((x, _) => (int)x == (int)HiHat.PatternTypes.Eight, new string[] { "pos8" }),
                            new Param.CollapseParam((x, _) => (int)x == (int)HiHat.PatternTypes.Sixteen, new string[] { "pos16" }),
                        }),
                        new Param("pos4", new EntityTypes.Integer(1, 4, 1), "Position", "Set the position of the doll."),
                        new Param("pos8", new EntityTypes.Integer(1, 8, 1), "Position", "Set the position of the doll."),
                        new Param("pos16", new EntityTypes.Integer(1, 13, 1), "Position", "Set the position of the doll.")

                    }
                },

                new GameAction("clear", "Clear Dolls")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        HiHat.instance.ClearDolls(e["instant"]); 
                    },
                    defaultLength = 0.5f,
                    resizable = false,
                    advanced = true,
                    parameters = new List<Param>()
                    {
                        new Param("instant", false, "Instant", "Set whether to instantly remove all of the currently active dolls."),
                    }
                },

                new GameAction("dollColor", "Doll Appearance")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        HiHat.instance.DollAppearance(e.beat, e["lightColor"], e["lightBackColor"], e["faceColor"]);
                    },
                    defaultLength = 0.5f,
                    resizable = false,
                    parameters = new List<Param>()
                    {
                        new Param("lightColor", HiHat.defaultDollLightColor, "Light Color", "Set the color of the beeping light."),
                        new Param("lightBackColor", HiHat.defaultDollLightBackColor, "Back Light Color", "Set the back color of the beeping light."),
                        new Param("faceColor", Color.white, "Face Color", "Set the face color of the dolls."),
                    }
                },

                new GameAction("changeBG", "Background Appearance")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        HiHat.instance.BackgroundAppearance(e.beat, e.length, e["start"], e["end"], e["ease"]);
                    },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("start", HiHat.defaultBGColor, "Start Color", "Set the color of the background at the start."),
                        new Param("end", HiHat.defaultBGColor, "End Color", "Set the color of the background at the end."),

                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the color change.")
                    }
                },

                new GameAction("bgScroll", "Scrolling Background")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        var colors = new List<Color>();
                        colors.Add(e["birdColor"]);
                        colors.Add(e["onionColor"]);
                        colors.Add(e["trioColor"]);
                        colors.Add(e["demoColor"]);
                        HiHat.instance.ScrollingBackground(e.beat, e.length, e["scrollVisible"], e["bgType"], colors, e["speed"], e["progressiveOpacity"]);
                    },
                    defaultLength = 0.5f,
                    resizable = false,
                    parameters = new List<Param>()
                    {
                        new Param("scrollVisible", true, "Visible", "Set the visibility of the scrolling background.", new List<Param.CollapseParam>()
                        {
                             new Param.CollapseParam((x, _) => (bool)x, new string[] {"bgType", "speed"}),
                        }),
                        new Param("bgType", HiHat.ScrollingBackgrounds.Birds, "Scrolling Background Type", "Set which scrolling background will be displayed.",new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x == (int)HiHat.ScrollingBackgrounds.Birds, new string[] { "birdColor" }),
                            new Param.CollapseParam((x, _) => (int)x == (int)HiHat.ScrollingBackgrounds.Onion, new string[] { "onionColor" }),
                            new Param.CollapseParam((x, _) => (int)x == (int)HiHat.ScrollingBackgrounds.Trio, new string[] { "trioColor" }),
                            new Param.CollapseParam((x, _) => (int)x == (int)HiHat.ScrollingBackgrounds.Demo, new string[] { "demoColor" }),
                        }),
                        new Param("birdColor", HiHat.defaultScrollColor1, "Color", "Set the color of the scrolling background."),
                        new Param("onionColor", HiHat.defaultScrollColor2, "Color", "Set the color of the scrolling background."),
                        new Param("trioColor", HiHat.defaultScrollColor3, "Color", "Set the color of the scrolling background."),
                        new Param("demoColor", HiHat.defaultScrollColor4, "Color", "Set the color of the scrolling background."),
                        new Param("progressiveOpacity", true, "Progressive Opacity", "Set if the opacity of the scrolling background depends on hit inputs."),
                        new Param("speed", new EntityTypes.Float(0f, 2f, 1f), "Scroll Speed", "Set the speed of the scrolling background.")
                    }
                },
            },
            new List<string>() { "rvl", "aim", "keep", "toy" },
            "rvlhihat", "en",
            new List<string>() { },
            chronologicalSortKey: 203,
            font: "Fever UI"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
    public class HiHat : Minigame
    {
        public static HiHat instance;

        [Header("Components")]
        [SerializeField] BouncingDoll doll;
        [SerializeField] BouncingDoll dollBig;

        public Animator hiHatAnim;
        public SpriteRenderer background;
        public GameObject scrollBG;
        public Material dollMaterial;
        public Material dollLightMaterial;
        public static bool scrollVisible = false;

        public static Color defaultDollLightColor = new Color(1 / 255f, 251 / 255f, 0 / 255f);
        public static Color defaultDollLightBackColor = new Color(57 / 255f, 61 / 255f, 8 / 255f);
        public Color dollLightColor = defaultDollLightColor;
        public Color dollLightBackColor = defaultDollLightBackColor;
        public static Color defaultBGColor = new Color(132 / 255f, 251 / 255f, 247 / 255f);

        public static Color defaultScrollColor1 = new Color(90 / 255f, 183 / 255f, 255 / 255f); // Birds
        public static Color defaultScrollColor2 = new Color(255 / 255f, 247 / 255f, 0 / 255f); // Onion
        public static Color defaultScrollColor3 = new Color(255 / 255f, 175 / 255f, 247 / 255f); // Trio
        public static Color defaultScrollColor4 = new Color(82 / 255f, 210 / 255f, 198 / 255f); // Demo

        public enum ScrollingBackgrounds
        {
            Birds,
            Onion,
            Trio,
            Demo

        }
        public enum PatternTypes
        {
            Four,
            Eight,
            Sixteen

        }

        public Vector2 pos41 = new Vector2(-1.58f, -0.66f);
        public Vector2 pos42 = new Vector2(-0.9f, -1.43f);
        public Vector2 pos43 = new Vector2(0.9f, -1.43f);
        public Vector2 pos44 = new Vector2(1.58f, -0.43f); // Big doll

        public Vector2 pos81 = new Vector2(-1.58f, -0.95f);
        public Vector2 pos82 = new Vector2(-1.34f, -1.225f);
        // 3rd is same as 2nd on 4 pattern
        public Vector2 pos84 = new Vector2(-0.31f, -1.55f);
        public Vector2 pos85 = new Vector2(0.31f, -1.55f);
        // 6th is same as 3rd on 4 pattern
        public Vector2 pos87 = new Vector2(1.34f, -1.225f);
        public Vector2 pos88 = new Vector2(1.58f, -0.725f); // Big doll

        public Vector2 pos161 = new Vector2(-1.335f, -0.389f);
        public Vector2 pos162 = new Vector2(-1.576f, -0.663f);
        // These middle ones are the same as 1-7 of the 8 pattern
        public Vector2 pos1610 = new Vector2(1.58f, -0.95f);
        public Vector2 pos1611 = new Vector2(1.576f, -0.663f);
        public Vector2 pos1612 = new Vector2(1.335f, -0.389f);
        public Vector2 pos1613 = new Vector2(0.885f, 0.076f); // Big doll

        private int currentScroll = 0;
        private float scrollOpacity = 0f;
        private bool progressiveOpacity = true;
        private ColorEase colorEase = new(defaultBGColor);
        private Color scrollColor = defaultScrollColor1;

        [NonSerialized] public bool isClosed = false;
        [NonSerialized] public int currentPattern = 0;
        [NonSerialized] public bool patternStarted = false;
        [NonSerialized] public List<RiqEntity> patternList = new();
        [NonSerialized] public List<RiqEntity> singleDollList = new();
        [NonSerialized] public double nextGameSwitch = 0;
        private List<BouncingDoll> activeDolls = new();
        private List<BeatAction.Action> queuedActions = new();

        private Sound openHatSound;

        protected static bool IA_PadNotPressing(out double dt)
        {
            dt = 0;
            return !PlayerInput.GetPad(InputController.ActionsPad.East);
        }
        protected static bool IA_TouchNotPressing(out double dt)
        {
            dt = 0;
            return !PlayerInput.GetTouch(InputController.ActionsTouch.Tap);
        }
        protected static bool IA_BatonNotPressing(out double dt)
        {
            dt = 0;
            return !PlayerInput.GetBaton(InputController.ActionsBaton.Face);
        }

        public static PlayerInput.InputAction InputAction_NotPressing =
            new("RvlHiHatNotPressing", new int[] { IAEmptyCat, IAEmptyCat, IAEmptyCat },
            IA_PadNotPressing, IA_TouchNotPressing, IA_BatonNotPressing);

        protected static bool IA_None(out double dt)
        {
            dt = 0;
            return false;
        }

        public static PlayerInput.InputAction InputAction_None =
            new("RvlHiHatNone", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_None, IA_None, IA_None);

        private void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            currentPattern = 0;
            if (patternList.Count > 0) patternList.Clear();
            nextGameSwitch = 0;
        }

        private void Update()
        {
            BackgroundUpdate();

            if (gameManager.Autoplay) return;
            if (PlayerInput.GetIsAction(InputAction_BasicPress))
            {
                if (!isClosed) Close();
            }
            if (PlayerInput.GetIsAction(InputAction_NotPressing))
            {
                if (isClosed) Open();
            }
            if (PlayerInput.GetIsAction(InputAction_BasicPressing))
            {
                isClosed = true;
            }
        }

        private void BackgroundUpdate()
        {
            if (background != null) background.color = colorEase.GetColor();

            List<SpriteRenderer> scrollSprites = new();

            foreach (Transform child in scrollBG.transform.GetChild(0).transform)
            {
                SpriteRenderer sprite = child.GetComponent<SpriteRenderer>();

                if (progressiveOpacity) scrollColor.a = scrollOpacity;
                sprite.color = scrollColor;
            }
        }

        public void BackgroundAppearance(double beat, float length, Color start, Color end, int ease)
        {
            colorEase = new ColorEase(beat, length, start, end, ease);
        }

        public void DollAppearance(double beat, Color lightColor, Color lightBackColor, Color faceColor)
        {
            dollLightColor = lightColor;
            dollLightBackColor = lightBackColor;

            dollLightMaterial.SetColor("_ColorAlpha", lightColor);
            dollMaterial.SetColor("_ColorBravo", faceColor);
            dollMaterial.SetColor("_ColorDelta", lightBackColor);
        }


        public void ScrollingBackground(double beat, float length, bool scrollVisible, int bgType, List<Color> color, float scrollSpeed, bool pOpacity)
        {
            currentScroll = bgType;

            scrollBG.SetActive(scrollVisible);

            Animator scrollAnimator = scrollBG.GetComponent<Animator>();
            scrollAnimator.speed = scrollSpeed;
            scrollAnimator.Play("hiHat_bg_scroll_" + bgType, 0);

            scrollColor = color[bgType];
            progressiveOpacity = pOpacity;
        }

        private void PersistBG(double beat)
        {
            var changeBGEventsBeforeBeat = EventCaller.GetAllInGameManagerList("hiHat", new string[] { "changeBG" }).FindAll(x => x.beat < beat);
            if (changeBGEventsBeforeBeat.Count > 0)
            {
                changeBGEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat));
                var lastEvent = changeBGEventsBeforeBeat[^1];

                BackgroundAppearance(lastEvent.beat, lastEvent.length, lastEvent["start"], lastEvent["end"], lastEvent["ease"]);
            }
            var bgScrollEventsBeforeBeat = EventCaller.GetAllInGameManagerList("hiHat", new string[] { "bgScroll" }).FindAll(x => x.beat < beat);
            if (bgScrollEventsBeforeBeat.Count > 0)
            {
                bgScrollEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat));
                var lastEvent = bgScrollEventsBeforeBeat[^1];

                var colors = new List<Color>();
                colors.Add(lastEvent["birdColor"]);
                colors.Add(lastEvent["onionColor"]);
                colors.Add(lastEvent["trioColor"]);
                colors.Add(lastEvent["demoColor"]);

                ScrollingBackground(lastEvent.beat, lastEvent.length, lastEvent["scrollVisible"], lastEvent["bgType"], colors, lastEvent["speed"], lastEvent["progressiveOpacity"]);
            }
        }

        private void PersistDolLColor(double beat)
        {
            var dollColorEventsBeforeBeat = EventCaller.GetAllInGameManagerList("hiHat", new string[] { "dollColor" }).FindAll(x => x.beat < beat);
            if (dollColorEventsBeforeBeat.Count > 0)
            {
                dollColorEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat));
                var lastEvent = dollColorEventsBeforeBeat[^1];

                DollAppearance(lastEvent.beat, lastEvent["lightColor"], lastEvent["lightBackColor"], lastEvent["faceColor"]);
            }
        }

        public override void OnPlay(double beat)
        {
            PersistBG(beat);
            dollLightColor = defaultDollLightColor;
            dollLightBackColor = defaultDollLightBackColor;
            dollLightMaterial.SetColor("_ColorAlpha", defaultDollLightColor);
            dollMaterial.SetColor("_ColorDelta", defaultDollLightBackColor);

            PersistDolLColor(beat);
            if (PlayerInput.GetIsAction(InputAction_BasicPressing)) hiHatAnim.DoScaledAnimationAsync("hihat_close_00", 0.5f);

            GetCurrentPatterns(beat);
        }
        public override void OnGameSwitch(double beat)
        {
            PersistBG(beat);
            dollLightColor = defaultDollLightColor;
            dollLightBackColor = defaultDollLightBackColor;
            dollLightMaterial.SetColor("_ColorAlpha", defaultDollLightColor);
            dollMaterial.SetColor("_ColorDelta", defaultDollLightBackColor);

            PersistDolLColor(beat);
            if (PlayerInput.GetIsAction(InputAction_BasicPressing)) hiHatAnim.DoScaledAnimationAsync("hihat_close_00", 0.5f);

            GetCurrentPatterns(beat);

            DoInactivePattern(beat);
            //var actionList = gameManager.Beatmap.Entities.FindAll(e => e.beat - 2 <= beat && e.beat + (e.length * 2) >= beat && (e.datamodel is "hiHat/4beat" or "hiHat/8beat" or "hiHat/16beat"));
        }
        private void DoInactivePattern(double beat)
        {
            RiqEntity patternEntity = NextPattern(beat);
            if (patternEntity != null && patternEntity.beat >= beat && patternEntity.beat - beat <= 1)
            {
                RiqEntity e = patternEntity;
                var type = 0;
                if (e.datamodel == "hiHat/4beat") type = 4;
                if (e.datamodel == "hiHat/8beat") type = 8;
                if (e.datamodel == "hiHat/16beat") type = 16;
                currentPattern = type;
                SpawnPattern(e.beat, type, true);
            }

            List<RiqEntity> dollEntities = QueuedDolls(beat);

            List<Vector2> posList = new List<Vector2>{HiHat.instance.pos41, HiHat.instance.pos42, HiHat.instance.pos43, HiHat.instance.pos44,
                        HiHat.instance.pos81, HiHat.instance.pos82, HiHat.instance.pos42, HiHat.instance.pos84, HiHat.instance.pos85, HiHat.instance.pos43, HiHat.instance.pos87, HiHat.instance.pos88,
                        HiHat.instance.pos161, HiHat.instance.pos162, HiHat.instance.pos81, HiHat.instance.pos82, HiHat.instance.pos42, HiHat.instance.pos84, HiHat.instance.pos85, HiHat.instance.pos43, HiHat.instance.pos87, HiHat.instance.pos88,
                        HiHat.instance.pos1610, HiHat.instance.pos1611,HiHat.instance.pos1612, HiHat.instance.pos1613};

            foreach (RiqEntity e in dollEntities)
            {
                var position = new Vector2();
                if (e["basePositions"] == 0) position = posList[e["pos4"] - 1];
                if (e["basePositions"] == 1) position = posList[e["pos8"] + 3];
                if (e["basePositions"] == 2) position = posList[e["pos16"] + 11];

                var behind = false;
                if (e["basePositions"] == 2 && (e["pos16"] <= 2 || e["pos16"] >= 11)) behind = true;

                var isShadowHigher = false;
                if (e["isBig"])
                {
                    if (e["basePositions"] == 0)
                    {
                        if (e["pos4"] <= 3) isShadowHigher = true;
                    }
                    else if (e["basePositions"] == 1)
                    {
                        if (e["pos8"] <= 7) isShadowHigher = true;
                    }
                    else if (e["basePositions"] == 2)
                    {
                        if (e["pos16"] > 2 && e["pos16"] <= 10) isShadowHigher = true;
                    }
                }

                HiHat.instance.SpawnDoll(e.beat, e["isBig"], position, 0, false, behind, isShadowHigher);
            }
        }

        public void GetCurrentPatterns(double beat)
        {
            List<RiqEntity> entitiesList;
            double currentGameSwitchBeat = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame", "end" }).Find(x => x.beat <= beat)?.beat ?? 0;
            double nextGameSwitchBeat = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame", "end" }).Find(x => x.beat > beat)?.beat ?? double.MaxValue;
            entitiesList = EventCaller.GetAllInGameManagerList("hiHat").FindAll(e => e.beat >= currentGameSwitchBeat && (nextGameSwitchBeat + 1) > e.beat);
            nextGameSwitch = nextGameSwitchBeat;
            foreach (RiqEntity entity in entitiesList)
            {
                if (entity.datamodel == "hiHat/4beat" || entity.datamodel == "hiHat/8beat" || entity.datamodel == "hiHat/16beat") patternList.Add(entity);
                if (entity.datamodel == "hiHat/singleDoll") singleDollList.Add(entity);
            }
        }

        public RiqEntity NextPattern(double beat)
        {
            return patternList.Find(e => e.beat > beat);
        }
        public RiqEntity NextDoll(double beat)
        {
            return singleDollList.Find(e => e.beat > beat);
        }

        public List<RiqEntity> QueuedDolls(double beat)
        {
            return singleDollList.FindAll(e => e.beat > beat && e.beat - 1 < beat);
        }

        public void SpawnDoll(double beat, bool isBig, Vector2 position, int type = 0, bool isFirst = false, bool behind = false, bool shadowHigher = false, double interval = 4)
        {
            if (type == currentPattern || type == 0)
            {
                BouncingDoll newDoll = Instantiate(isBig ? dollBig : doll, transform);
                newDoll.hitBeat = beat;
                newDoll.transform.localPosition = position;
                newDoll.interval = interval;
                newDoll.patternType = type;
                newDoll.isFirst = isFirst;
                newDoll.behind = behind;
                newDoll.shadowHigher = shadowHigher;

                activeDolls.Add(newDoll);

                if (isBig) newDoll.anim.DoScaledAnimationAsync("dwarf_doll_L_spawn", 0.5f);
                else newDoll.anim.DoScaledAnimationAsync("dwarf_doll_S_spawn", 0.5f);
                newDoll.gameObject.SetActive(true);
            }
        }

        public void ClearDolls(bool instant = false)
        {
            List<BouncingDoll> toRemove = new();
            foreach (var doll in activeDolls)
            {
                if (patternStarted || doll.patternType != currentPattern || doll.patternType == 0)
                {
                    if (instant) Destroy(doll.gameObject);
                    else doll.Despawn(doll.hitBeat + doll.interval);
                    toRemove.Add(doll);
                }
            }

            foreach (var doll in toRemove) activeDolls.Remove(doll);

            patternStarted = false;
        }

        public void SpawnPattern(double beat, int type, bool inactiveCall = false)
        {
            if (beat < nextGameSwitch || nextGameSwitch == 0 || inactiveCall)
            {

                patternStarted = false;

                if (type == 4)
                {

                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        //new BeatAction.Action(beat, delegate {ScheduleAutoplayInput(beat, 0, InputAction_BasicPress, HitSmall, Miss, null);}),
                        new BeatAction.Action(beat - 1, delegate { SpawnDoll(beat, false, pos41, type, true); }),
                        new BeatAction.Action(beat, delegate { SpawnDoll(beat + 1, false, pos42, type); }),
                        new BeatAction.Action(beat, delegate { patternStarted = true; }),
                        new BeatAction.Action(beat + 1, delegate { SpawnDoll(beat + 2, false, pos43, type); }),

                        //new BeatAction.Action(beat + 3, delegate {ScheduleInput(beat, 3, InputAction_NotPressing, Hit, Miss, null);}),
                        new BeatAction.Action(beat + 2, delegate { SpawnDoll(beat + 3, true, pos44, type); }),
                    });
                }
                else if (type == 8)
                {
                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat - 1, delegate { SpawnDoll(beat, false, pos81, type, true); }),
                        new BeatAction.Action(beat - 0.5f, delegate { SpawnDoll(beat + 0.5f, false, pos82, type); }),
                        new BeatAction.Action(beat, delegate { SpawnDoll(beat + 1, false, pos42, type); }),
                        new BeatAction.Action(beat, delegate { patternStarted = true; }),
                        new BeatAction.Action(beat + 0.5f, delegate { SpawnDoll(beat + 1.5f, false, pos84, type); }),
                        new BeatAction.Action(beat + 1, delegate { SpawnDoll(beat + 2, false, pos85, type); }),
                        new BeatAction.Action(beat + 1.5f, delegate { SpawnDoll(beat + 2.5f, false, pos43, type); }),
                        new BeatAction.Action(beat + 2, delegate { SpawnDoll(beat + 3, false, pos87, type); }),

                        new BeatAction.Action(beat + 2.5f, delegate { SpawnDoll(beat + 3.5f, true, pos88, type); }),
                        //new BeatAction.Action(beat + 3.5f, delegate {ScheduleInput(beat, 3.5f, InputAction_NotPressing, Hit, Miss, null);}),
                    });
                }
                else if (type == 16)
                {
                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat - 1, delegate { SpawnDoll(beat, false, pos161, type, true, true); }),
                        new BeatAction.Action(beat - 0.75f, delegate { SpawnDoll(beat + 0.25f, false, pos162, type, false, true); }),
                        new BeatAction.Action(beat - 0.5f, delegate { SpawnDoll(beat + 0.5f, false, pos81, type); }),
                        new BeatAction.Action(beat - 0.25f, delegate { SpawnDoll(beat + 0.75f, false, pos82, type); }),
                        new BeatAction.Action(beat, delegate { SpawnDoll(beat + 1, false, pos42, type); }),
                        new BeatAction.Action(beat, delegate { patternStarted = true; }),
                        new BeatAction.Action(beat + 0.25f, delegate { SpawnDoll(beat + 1.25f, false, pos84, type); }),
                        new BeatAction.Action(beat + 0.5f, delegate { SpawnDoll(beat + 1.5f, false, pos85, type); }),
                        new BeatAction.Action(beat + 0.75f, delegate { SpawnDoll(beat + 1.75f, false, pos43, type); }),
                        new BeatAction.Action(beat + 1, delegate { SpawnDoll(beat + 2, false, pos87, type); }),
                        new BeatAction.Action(beat + 1.25f, delegate { SpawnDoll(beat + 2.25f, false, pos1610, type, false, true); }),
                        new BeatAction.Action(beat + 1.5f, delegate { SpawnDoll(beat + 2.5f, false, pos1611, type, false, true); }),
                        new BeatAction.Action(beat + 1.75f, delegate { SpawnDoll(beat + 2.75f, false, pos1612, type, false, true); }),

                        new BeatAction.Action(beat + 2, delegate { SpawnDoll(beat + 3, true, pos1613, type, false, true); }),
                        //new BeatAction.Action(beat + 3, delegate {ScheduleInput(beat, 3, InputAction_NotPressing, Hit, Miss, null);}),
                    });
                }
            }
        }

        public void Hit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("nearMiss");
                hiHatAnim.DoScaledAnimationAsync("hihat_open_miss_00", 0.5f);
                if (currentPattern == 0) scrollOpacity -= 0.05f;
                else scrollOpacity -= 0.2f / currentPattern;
            }
            else
            {
                hiHatAnim.DoScaledAnimationAsync("hihat_open_shot_00", 0.5f);
                openHatSound = SoundByte.PlayOneShotGame("hiHat/bounce_big", returnSound: true);
                if (scrollOpacity < 1)
                {
                    if (currentPattern == 0) scrollOpacity += 0.05f;
                    else scrollOpacity += 0.2f / currentPattern;
                }
            }
            isClosed = false;
        }

        public void HitSmall(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("nearMiss");
                hiHatAnim.DoScaledAnimationAsync("hihat_open_miss_00", 0.5f);
                if (currentPattern == 0) scrollOpacity -= 0.05f;
                else scrollOpacity -= 0.2f / currentPattern;
            }
            else
            {
                hiHatAnim.DoScaledAnimationAsync("hihat_close_shot_00", 0.5f);
                SoundByte.PlayOneShotGame("hiHat/bounce");
                if (currentPattern == 0) scrollOpacity += 0.05f;
                else scrollOpacity += 0.2f / currentPattern;
            }
        }

        public void AutoClose(PlayerActionEvent caller, float state)
        {
            Close();
        }

        public void Close()
        {
            if (!hiHatAnim.IsPlayingAnimationNames("hihat_close_shot_00")) hiHatAnim.DoScaledAnimationAsync("hihat_close_00", 0.5f);
            if (openHatSound != null) openHatSound.KillLoop();
            SoundByte.PlayOneShotGame("hiHat/close");
            isClosed = true;
        }

        public void Open()
        {
            hiHatAnim.DoScaledAnimationAsync("hihat_open_00", 0.5f);
            isClosed = false;
        }

        public void Miss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShot("nearMiss");
            if (isClosed) hiHatAnim.DoScaledAnimationAsync("hihat_close_miss_00", 0.5f);
            else hiHatAnim.DoScaledAnimationAsync("hihat_open_miss_00", 0.5f);
            if (currentPattern == 0) scrollOpacity -= 0.05f;
            else scrollOpacity -= 0.2f / currentPattern;
        }
    }
}