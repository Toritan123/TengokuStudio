using HeavenStudio.InputSystem;
using HeavenStudio.Util;
using Jukebox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    public static class PcoSheriffLoader
    {

        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("rhythmSheriff", "Rhythm Sheriff \n<color=#eb5454>[WIP]</color>", "BBB594", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; RhythmSheriff.instance.Bop(e.beat, e.length, e["toggle2"], e["toggle"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle2", true, "Bop", "Toggle if Dog Sheriff should bop for the duration of this event."),
                        new Param("toggle", false, "Bop (Auto)", "Toggle if Dog Sheriff should automatically bop until another Bop event is reached.")
                    }
                },

                new GameAction("slowtarget", "Slow Target")
                {
                    function = delegate { var e = eventCaller.currentEntity; RhythmSheriff.instance.SpawnTarget(e.beat, 0, e["auto"], e["holbeat"]); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("auto", true, "Auto Reholster", "Toggle if Dog Sheriff should reholster his gun on the next bop.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => !(bool)x, new string[] { "beat" })
                        }),
                        new Param("holbeat", new EntityTypes.Float(2.5f, 100f, 3f), "Reholster Beat", "Choose what beat Dog Sheriff should reholster his gun after shooting.")
                    }
                },

                new GameAction("fasttarget", "Fast Target")
                {
                    function = delegate { var e = eventCaller.currentEntity; RhythmSheriff.instance.SpawnTarget(e.beat, 1, e["auto"], e["holbeat"]); },
                    defaultLength = 2.5f,
                    parameters = new List<Param>()
                    {
                        new Param("auto", true, "Auto Reholster", "Toggle if Dog Sheriff should reholster his gun on the next bop.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => !(bool)x, new string[] { "beat" })
                        }),
                        new Param("holbeat", new EntityTypes.Float(2.5f, 100f, 3f), "Reholster Beat", "Choose what beat Dog Sheriff should reholster his gun after shooting.")
                    }
                },

                new GameAction("bgAppear", "Background Appearance")
                {
                    function = delegate { var e = eventCaller.currentEntity; RhythmSheriff.instance.BGColor
                    (e.beat, e.length, e["startSkyT"], e["endSkyT"], e["startSkyB"], e["endSkyB"], e["startGroT"], e["endGroT"], e["startGroB"], e["endGroB"],
                    e["startRocA"], e["endRocA"], e["startRocB"], e["endRocB"], e["startRocC"], e["endRocC"], e["startBusA"], e["endBusA"], e["startBusB"], e["endBusB"], e["startCac"], e["endCac"], e["obToggle"], e["ease"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("startSkyT",          RhythmSheriff.skyTopColor,           "Start Color (Sky)", "Set the color at the start of the event."),
                        new Param("endSkyT",            RhythmSheriff.skyTopColor,           "End Color (Sky)", "Set the color at the end of the event."),
                        new Param("startSkyB",          RhythmSheriff.skyBotColor,           "Start Color (Sky)", "Set the color at the start of the event."),
                        new Param("endSkyB",            RhythmSheriff.skyBotColor,           "End Color (Sky)", "Set the color at the end of the event."),

                        new Param("startGroT",          RhythmSheriff.groundColor,           "Start Color (Ground)", "Set the color at the start of the event."),
                        new Param("endGroT",            RhythmSheriff.groundColor,           "End Color (Ground)", "Set the color at the end of the event."),
                        new Param("startGroB",          RhythmSheriff.groundColor,           "Start Color (Ground)", "Set the color at the start of the event."),
                        new Param("endGroB",            RhythmSheriff.groundColor,           "End Color (Ground)", "Set the color at the end of the event."),
                        
                        new Param("startRocA",          RhythmSheriff.rockFrontColor,           "Start Color (Rock)", "Set the color at the start of the event."),
                        new Param("endRocA",            RhythmSheriff.rockFrontColor,           "End Color (Rock)", "Set the color at the end of the event."),
                        new Param("startRocB",          RhythmSheriff.rockSideColor,           "Start Color (Rock)", "Set the color at the start of the event."),
                        new Param("endRocB",            RhythmSheriff.rockSideColor,           "End Color (Rock)", "Set the color at the end of the event."),
                        new Param("startRocC",          RhythmSheriff.rockBackColor,           "Start Color (Rock)", "Set the color at the start of the event."),
                        new Param("endRocC",            RhythmSheriff.rockBackColor,           "End Color (Rock)", "Set the color at the end of the event."),

                        new Param("obToggle",           true, "Enable Foreground Objects", "Toggle if all the miscellaneous objects found on the ground are present.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "startBusA", "endBusA", "startBusB", "endBusB", "startCac", "endCac" })
                        }),

                        new Param("startBusA",          RhythmSheriff.bushFrontColor,           "Start Color (Bush)", "Set the color at the start of the event."),
                        new Param("endBusA",            RhythmSheriff.bushFrontColor,           "End Color (Bush)", "Set the color at the end of the event."),
                        new Param("startBusB",          RhythmSheriff.bushBackColor,           "Start Color (Bush)", "Set the color at the start of the event."),
                        new Param("endBusB",            RhythmSheriff.bushBackColor,           "End Color (Bush)", "Set the color at the end of the event."),

                        new Param("startCac",           RhythmSheriff.cactusColor,           "Start Color (Cactus)", "Set the color at the start of the event."),
                        new Param("endCac",             RhythmSheriff.cactusColor,           "End Color (Cactus)", "Set the color at the end of the event."),

                        new Param("ease",           Util.EasingFunction.Ease.Linear,    "Ease", "Set the easing of the action.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x != (int)Util.EasingFunction.Ease.Instant, new string[] { "startSkyT", "startSkyB", "startGroT", "startGroB", "startRocA", "startRocB", "startRocC", "startBusA", "startBusB", "startCac" }),
                        }),
                    },
                },

                new GameAction("sheriffColor", "Sheriff Appearance")
                {
                    function = delegate {var e = eventCaller.currentEntity; RhythmSheriff.instance.SheriffColor(e["fColor"], e["fShade"], e["cColor"], e["cShade"], e["bColor"], e["bShade"], e["belt"], e["gShade"], e["tColor"], e["tShade"]); },
                    parameters = new List<Param>()
                    {
                        new Param("fColor", RhythmSheriff.furColor, "Fur Color", "Set the color of the sheriff's fur."),
                        new Param("fShade", RhythmSheriff.furShading, "Fur Shading", "Set the color of the sheriff's fur."),
                        new Param("cColor", RhythmSheriff.clothesColor, "Clothes Color", "Set the color of the sheriff's clothes."),
                        new Param("cShade", RhythmSheriff.clothesShading, "Clothes Shading", "Set the color of the sheriff's clothes."),
                        new Param("bColor", RhythmSheriff.bandanaColor, "Bandana Color", "Set the color of the sheriff's bandana."),
                        new Param("bShade", RhythmSheriff.bandanaShading, "Bandana Shading", "Set the color of the sheriff's bandana."),
                        new Param("belt", RhythmSheriff.beltColor, "Belt Color", "Set the color of the sheriff's belt."),
                        new Param("gShade", RhythmSheriff.gunShading, "Gun Shading", "Set the color of the sheriff's gun barrel."),
                        new Param("tColor", RhythmSheriff.tearColor, "Tear Color", "Set the color of the sheriff's tear."),
                        new Param("tShade", RhythmSheriff.tearShading, "Tear Shading", "Set the color of the sheriff's tear."),
                    },
                    defaultLength = 0.5f,
                },

                new GameAction("particle effects", "Tumbleweed Particles")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        RhythmSheriff.instance.TumbleweedParticles(e.beat, e["instant"], e["intensity"], e["color"], e["sizeBack"], e["sizeFront"], e["sizeVar"]);
                    },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        //new Param("type", SamuraiSliceNtr.ParticleType.None, "Particle", "Set the type of particle effect to spawn. Using \"None\" will stop all effects."),
                        new Param("instant", false, "Instant", "Toggle if the tumbleweeds should start or stop instantly."),
                        //new Param("valA", new EntityTypes.Float(0f, 15f, 1f), "Wind Strength", "Set the strength of the particle wind."), DEB55A 
                        new Param("intensity", new EntityTypes.Float(0f, 25f, 0.1f), "Intensity", "Set the intensity of the tumbleweeds."),
                        new Param("sizeBack", new EntityTypes.Float(0f, 3f, 1.2f), "Size (Back)", "Set the size of the tumbleweeds in the back."),
                        new Param("sizeFront", new EntityTypes.Float(0f, 3f, 1.6f), "Size (Front)", "Set the size of the tumbleweeds in the front."),
                        new Param("sizeVar", new EntityTypes.Float(0f, 3f, 0.1f), "Size Variation", "The amount to randomly grow or shrink the size of a tumbleweed."),
                        new Param("color", new Color(0.87f, 0.71f, 0.35f, 1), "Color", "Set the color of the tumbleweeds.")
                    },
                },

                new GameAction("tumbleweedstorm", "Tumbleweed Storm")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        RhythmSheriff.instance.TumbleweedStorm(e.beat, e.length, e["color"]);
                    },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        //new Param("type", SamuraiSliceNtr.ParticleType.None, "Particle", "Set the type of particle effect to spawn. Using \"None\" will stop all effects."),
                        //new Param("instant", false, "Instant", "Toggle if the tumbleweeds should start or stop instantly."),
                        //new Param("valA", new EntityTypes.Float(0f, 15f, 1f), "Wind Strength", "Set the strength of the particle wind."), DEB55A 
                        //new Param("intensity", new EntityTypes.Float(0f, 25f, 0.1f), "Intensity", "Set the intensity of the tumbleweeds."),
                        //new Param("sizeBack", new EntityTypes.Float(0f, 3f, 1.2f), "Size (Back)", "Set the size of the tumbleweeds in the back."),
                        //new Param("sizeFront", new EntityTypes.Float(0f, 3f, 1.6f), "Size (Front)", "Set the size of the tumbleweeds in the front."),
                        //new Param("sizeVar", new EntityTypes.Float(0f, 3f, 0.1f), "Size Variation", "The amount to randomly grow or shrink the size of a tumbleweed."),
                        new Param("color", new Color(0.87f, 0.71f, 0.35f, 1), "Color", "Set the color of the tumbleweeds.")
                    },
                },
            },

            new List<string>() { "pco", "normal" },
            "pcosheriff", "en",
            chronologicalSortKey: 20191101,
            font : "Matisse Minori",
            plusGame: true
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using HeavenStudio.Games.Scripts_MeatGrinder;



    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
    

    using Scripts_RhythmSheriff;
    using static HeavenStudio.Games.Minigame;

    public class RhythmSheriff : Minigame
    {

        [Header("Animators")]

        public Animator dogSheriff;

        [Header("Properties")]
        public float ratPitch;
        public float ratLowerPitch;
        public float ratFinalPitch;
        public float catPitch;
        public float catLowerPitch;
        public float catFinalPitch;

        [Header("Target")]
        public GameObject targetObj;
        [Header("Particles")]
        public ParticleSystem tumbleweedBack;
        public ParticleSystem tumbleweedFront;
        public ParticleSystem tumbleweedOverlay;

        public enum SheriffBopType
        {
            Normal = 1,
            Ready = 2,
            Return = 3,
            Spin = 4,
        }
        public SheriffBopType bopType = SheriffBopType.Normal;

        [Header("Sheriff Materials")]
        public Material Fur;
        public Material Clothes;
        public Material Bandana;
        public Material WahWahWah;
        public Material Gun;

        public static Color furColor = "DDC56B".Hex2RGB();
        public static Color furShading = "DDC56B".Hex2RGB();

        public static Color clothesColor = "653300".Hex2RGB();
        public static Color clothesShading = "653300".Hex2RGB();

        public static Color bandanaColor = "9B0001".Hex2RGB();
        public static Color bandanaShading = "9B0001".Hex2RGB();

        public static Color beltColor = "666666".Hex2RGB();
        
        public static Color gunShading = "666666".Hex2RGB();

        public static Color tearColor = "6BF7FF".Hex2RGB();
        public static Color tearShading = "6BF7FF".Hex2RGB();

        [Header("BG Materials")]
        public Material Sky;
        public Material Ground;
        public Material Rock;
        public Material Bush;
        public Material Cactus;

        public static Color skyTopColor = "7B9FA2".Hex2RGB();
        private ColorEase skyTopEase = new(skyTopColor); //
        public static Color skyBotColor = "BBB594".Hex2RGB();
        private ColorEase skyBotEase = new(skyBotColor); //

        public static Color groundColor = "B0825A".Hex2RGB();
        private ColorEase groundTopEase = new(groundColor); //
        private ColorEase groundBotEase = new(groundColor); //

        public static Color rockFrontColor = "DA6967".Hex2RGB(); //(G)
        private ColorEase rockFrontEase = new(rockFrontColor); //
        public static Color rockSideColor = "4E6180".Hex2RGB(); //(B)
        private ColorEase rockSideEase = new(rockSideColor); //
        public static Color rockBackColor = "424B52".Hex2RGB(); //(R)
        private ColorEase rockBackEase = new(rockBackColor); //

        public static Color bushFrontColor = "B2A281".Hex2RGB(); //(B)
        private ColorEase bushFrontEase = new(bushFrontColor); //
        public static Color bushBackColor = "474D55".Hex2RGB(); //(G)
        private ColorEase bushBackEase = new(bushBackColor); //

        public static Color cactusColor = "50463C".Hex2RGB();
        private ColorEase cactusEase = new(cactusColor); //

        public GameObject groundObjects;
        private bool objectsPresent = true;

        public static RhythmSheriff instance;

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
            SetupBopRegion("rhythmSheriff", "bop", "toggle");
        }

        public override void OnPlay(double beat)
        {
            ParticleSystem particleSystem = tumbleweedBack;
            particleSystem.gameObject.SetActive(true);
            particleSystem = tumbleweedFront;
            particleSystem.gameObject.SetActive(true);
            UpdateBGColor();
            PersistBGColor(beat);
        }

        public override void OnGameSwitch(double beat)
        {
            OnPlay(beat);
        }

        void Update()
        {
            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            {
                SoundByte.PlayOneShotGame("rhythmSheriff/miss");
                SoundByte.PlayOneShotGame("rhythmSheriff/whiff");
                dogSheriff.DoScaledAnimationAsync("ShootNG", 0.5f);
            }

            UpdateBGColor();
        }


        public override void OnLateBeatPulse(double beat)
        {
            if (!dogSheriff.IsPlayingAnimationNames("ShootNG", "ShootJust", "Ready") && BeatIsInBopRegion(beat))
            {
                if (bopType == SheriffBopType.Normal) { dogSheriff.DoScaledAnimationAsync("Bop", 0.5f); }
                else if (bopType == SheriffBopType.Ready) { dogSheriff.DoScaledAnimationAsync("Ready", 0.5f); }
                else if (bopType == SheriffBopType.Return) { dogSheriff.DoScaledAnimationAsync("Return", 0.5f);
                bopType = SheriffBopType.Normal;
                SoundByte.PlayOneShotGame("rhythmSheriff/holster");}
            }
        }

        public void Bop(double beat, float length, bool manualBop, bool goBop)
        {
            if (manualBop)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate
                        {
                            if (bopType == SheriffBopType.Normal) { dogSheriff.DoScaledAnimationAsync("Bop", 0.5f); }
                            else if (bopType == SheriffBopType.Ready) { dogSheriff.DoScaledAnimationAsync("Ready", 0.5f); }
                            else if (bopType == SheriffBopType.Return) { dogSheriff.DoScaledAnimationAsync("Return", 0.5f);
                            bopType = SheriffBopType.Normal;
                            SoundByte.PlayOneShotGame("rhythmSheriff/holster");}
                        })
                    });
                }
            }
        }

        public void SpawnTarget(double beat, int variation, bool autoReholster, float reholsterBeat)
        {
            if (variation == 0)
            {
                MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("rhythmSheriff/targetCat", beat, pitch: catPitch),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.000, pitch: catPitch - catLowerPitch, volume:0.2f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.125, pitch: catPitch - catLowerPitch, volume:0.3f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.250, pitch: catPitch - catLowerPitch, volume:0.4f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.375, pitch: catPitch - catLowerPitch, volume:0.5f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.500, pitch: catPitch - catLowerPitch, volume:0.6f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.625, pitch: catPitch - catLowerPitch, volume:0.7f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.750, pitch: catPitch - catLowerPitch, volume:0.8f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.875, pitch: catPitch - catLowerPitch, volume:0.9f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 2, pitch: catFinalPitch),
                });
            } else {
                MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("rhythmSheriff/targetCat", beat, pitch: ratPitch),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 0.500, pitch: ratPitch - ratLowerPitch, volume:0.9f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 0.625, pitch: ratPitch - ratLowerPitch, volume:0.7f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 0.750, pitch: ratPitch - ratLowerPitch, volume:0.5f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 0.875, pitch: ratPitch - ratLowerPitch, volume:0.3f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.000, pitch: ratPitch - ratLowerPitch, volume:0.9f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.125, pitch: ratPitch - ratLowerPitch, volume:0.7f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.250, pitch: ratPitch - ratLowerPitch, volume:0.5f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.375, pitch: ratPitch - ratLowerPitch, volume:0.3f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.5, pitch: ratFinalPitch),
                });
            }

            Targets targetClone = Instantiate(targetObj, gameObject.transform).GetComponent<Targets>();
            targetClone.beat = beat;
            targetClone.targetType = (Targets.TargetType)variation;

            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 0.25, delegate
                    {
                        bopType = SheriffBopType.Ready;
                    }),
                    new BeatAction.Action(beat + 1, delegate
                    {
                        if (!BeatIsInBopRegion(beat))
                        {
                            dogSheriff.DoScaledAnimationAsync("Ready", 0.5f);
                        }
                    })
            });
        }

        public void SheriffColor(Color fColor, Color fShade, Color cColor, Color cShade, Color bColor, Color bShade, Color belt, Color gShade, Color tColor, Color tShade)
        {
            Fur.SetColor("_ColorDelta", fColor);
            Fur.SetColor("_ColorBravo", fShade);
            Clothes.SetColor("_ColorAlpha", belt);
            Clothes.SetColor("_ColorDelta", cColor);
            Clothes.SetColor("_ColorBravo", cShade);
            Bandana.SetColor("_ColorDelta", bColor);
            Bandana.SetColor("_ColorBravo", bShade);
            WahWahWah.SetColor("_ColorDelta", tColor);
            WahWahWah.SetColor("_ColorBravo", tShade);

            //set gun color using same as clothes (the fact the shading changes colors between TWO sprites has made me go insane)
            Gun.SetColor("_ColorAlpha", cColor);
            Gun.SetColor("_ColorDelta", gShade);
            Gun.SetColor("_ColorBravo", belt);
        }

        public void BGColor(double beat, float length, Color sSkyT, Color eSkyT, Color sSkyB, Color eSkyB, Color sGroT, Color eGroT, Color sGroB, Color eGroB,
            Color sRocA, Color eRocA, Color sRocB, Color eRocB, Color sRocC, Color eRocC, Color sBusA, Color eBusA, Color sBusB, Color eBusB, Color sCac, Color eCac, bool obTog, int ease)
        {
            skyTopEase = new ColorEase(beat, length, sSkyT, eSkyT, ease);
            skyBotEase = new ColorEase(beat, length, sSkyB, eSkyB, ease);
            groundTopEase = new ColorEase(beat, length, sGroT, eGroT, ease);
            groundBotEase = new ColorEase(beat, length, sGroB, eGroB, ease);
            rockFrontEase = new ColorEase(beat, length, sRocA, eRocA, ease);
            rockSideEase = new ColorEase(beat, length, sRocB, eRocB, ease);
            rockBackEase = new ColorEase(beat, length, sRocC, eRocC, ease);
            bushFrontEase = new ColorEase(beat, length, sBusA, eBusA, ease);
            bushBackEase = new ColorEase(beat, length, sBusB, eBusB, ease);
            cactusEase = new ColorEase(beat, length, sCac, eCac, ease);
            if (obTog) { objectsPresent = true; }
            else if (obTog == false) { objectsPresent = false; } //better safe then sorry they say
        }

        private void UpdateBGColor()
        {
            Sky.SetColor("_ColorDelta", skyTopEase.GetColor());
            Sky.SetColor("_ColorAlpha", skyBotEase.GetColor());
            Ground.SetColor("_ColorDelta", groundTopEase.GetColor());
            Ground.SetColor("_ColorAlpha", groundBotEase.GetColor());
            Rock.SetColor("_ColorBravo", rockFrontEase.GetColor());
            Rock.SetColor("_ColorDelta", rockSideEase.GetColor());
            Rock.SetColor("_ColorAlpha", rockBackEase.GetColor());
            Bush.SetColor("_ColorBravo", bushFrontEase.GetColor());
            Bush.SetColor("_ColorDelta", bushBackEase.GetColor());
            Cactus.SetColor("_Color", cactusEase.GetColor());
            groundObjects.SetActive(objectsPresent);
        }
        private void PersistBGColor(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("rhythmSheriff", new string[] { "bgAppear" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                BGColor(lastEvent.beat, lastEvent.length, lastEvent["startSkyT"], lastEvent["endSkyT"], lastEvent["startSkyB"], lastEvent["endSkyB"], lastEvent["startGroT"], lastEvent["endGroT"], lastEvent["startGroB"], lastEvent["endGroB"], lastEvent["startRocA"], lastEvent["endRocA"], lastEvent["startRocB"], lastEvent["endRocB"], lastEvent["startRocC"], lastEvent["endRocC"], lastEvent["startBusA"], lastEvent["endBusA"], lastEvent["startBusB"], lastEvent["endBusB"], lastEvent["startCac"], lastEvent["endCac"], lastEvent["obToggle"], lastEvent["ease"]);
            }           
        }

        public void TumbleweedParticles(double beat, bool instant, float particleStrength, Color color, float sizeBack, float sizeFront, float sizeVar)
        {
            ParticleSystem particleSystem = tumbleweedBack;
            ParticleStuff(particleSystem, instant, particleStrength, color, sizeBack, sizeVar);

            particleSystem = tumbleweedFront;
            ParticleStuff(particleSystem, instant, particleStrength, color, sizeFront, sizeVar);
        }

        private void ParticleStuff(ParticleSystem particleSystem, bool instant, float particleStrength, Color color, float size, float sizeVar)
        {
            particleSystem.gameObject.SetActive(true);
            particleSystem.Play();

            var emm = particleSystem.emission;
            var main = particleSystem.main;
            var col = particleSystem.colorOverLifetime;

            emm.rateOverTime = particleStrength;
            main.prewarm = instant;
            col.color = color;

            var randomVariation = UnityEngine.Random.Range(sizeVar*-1, sizeVar);

            print(randomVariation);

            main.startSize = new ParticleSystem.MinMaxCurve(size - sizeVar, size + sizeVar);
        }

        public void TumbleweedStorm(double beat, float length, Color color)
        {
            ParticleSystem particleSystem = tumbleweedOverlay;
            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate
                    {
                        particleSystem.gameObject.SetActive(true);
                        particleSystem.Play();
                        var col = particleSystem.colorOverLifetime;
                        col.color = color;
                    }),
                    new BeatAction.Action(beat + length, delegate
                    {
                        particleSystem.Stop();
                    })
            });
        }

    }
}