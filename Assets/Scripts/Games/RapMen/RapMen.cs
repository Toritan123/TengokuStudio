using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    /// 

    /// Fill in the loader class label, "*prefab name*", and "*Display Name*" with the relevant information
    /// For help, feel free to reach out to us on our discord, in the #development channel.
    public static class AgbRapMenLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("rapMen", "RAPMEN \n<color=#adadad>(RAPPU MEN)</color>", "7BD6FF", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; RapMen.instance.Bop(e.beat, e.length, e["r"], e["y"], e["ra"], e["ya"]); },
                    parameters = new List<Param>()
                    {
                        new Param("r", true, "Red Rapper", "Toggle if the Red Rapper should bop for the duration of this event."),
                        new Param("y", true, "Yellow Rapper", "Toggle if the Yellow Rapper should bop for the duration of this event."),
                        new Param("ra", false, "Red Rapper (Auto)", "Toggle if the Red Rapper should automatically bop until another Bop event is reached."),
                        new Param("ya", false, "Yellow Rapper (Auto)", "Toggle if the Yellow Rapper should automatically bop until another Bop event is reached."),
                    },
                    defaultLength = 1f,
                    resizable = true
                },

                new GameAction("desuka", "Desuka")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; RapMen.SfxRaps(e.beat, e["gender"], e["voice"], e["voice2"], e["caption"], e["text"], "desuka", e["mute"]); },
                    //preFunction = delegate { var e = eventCaller.currentEntity; RapMen.instance.RapDesuka(e.beat, e["voice"], e["caption"], e["text"]); },
                    parameters = new List<Param>()
                    {
                        new Param("mute", false, "Mute Voice", "Toggle if the red rapper's voice should be muted.", advanced: true),
                        new Param("gender", RapMen.RedGender.Man, "Voice Type", "Choose which gender is used for the cueing.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x == (int)RapMen.RedGender.Man, new string[] { "voice" }),
                            new Param.CollapseParam((x, _) => (int)x == (int)RapMen.RedGender.Woman, new string[] { "voice2" }),
                        }),
                        new Param("voice", RapMen.RedDesukaLines.OyatsuDesuka, "Rap", "Choose what the red rapper should rap."),
                        new Param("voice2", RapMen.RedWomanDesukaLines.OyatsuDesuka, "Rap", "Choose what the red rapper should rap."),
                        new Param("caption", RapMen.CaptionTypesCue.Auto, "Subtitles", "Choose the subtitle type.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x == (int)RapMen.CaptionTypesCue.Custom, new string[] { "text" }),
                        }),
                        new Param("text", "It's time for a snack, |isn't it?", "Custom Rap", "The custom text that will appear in the rap's captions. Type a '|' to switch to the large font."),
                    },
                    defaultLength = 4f,
                    preFunctionLength = 0.5f,
                },

                new GameAction("kamone", "Kamone")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; RapMen.SfxRaps(e.beat, e["gender"], e["voice"], e["voice2"], e["caption"], e["text"], "kamone", e["mute"]); },
                    //function = delegate { var e = eventCaller.currentEntity; RapMen.instance.RapKamone(e.beat, e["voice"], e["caption"], e["text"]); },
                    parameters = new List<Param>()
                    {
                        new Param("mute", false, "Mute Voice", "Toggle if the red rapper's voice should be muted.", advanced: true),
                        new Param("gender", RapMen.RedGender.Man, "Voice Type", "Choose which gender is used for the cueing.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x == (int)RapMen.RedGender.Man, new string[] { "voice" }),
                            new Param.CollapseParam((x, _) => (int)x == (int)RapMen.RedGender.Woman, new string[] { "voice2" }),
                        }),
                        new Param("voice", RapMen.RedKamoneLines.OishīKamone, "Rap", "Choose what the red rapper should rap."),
                        new Param("voice2", RapMen.RedWomanKamoneLines.KareNoOyatsuDane, "Rap", "Choose what the red rapper should rap."),
                        new Param("caption", RapMen.CaptionTypesCue.Auto, "Subtitles", "Choose the subtitle type.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x == (int)RapMen.CaptionTypesCue.Custom, new string[] { "text" }),
                        }),
                        new Param("text", "Something sweet, it |could be", "Custom Rap", "The custom text that will appear in the rap's captions. Type a '|' to switch to the large font."),
                    },
                    defaultLength = 4f
                },

                new GameAction("saiko", "Saiko")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; RapMen.SfxRaps(e.beat, e["gender"], e["voice"], e["voice2"], e["caption"], e["text"], "saiko", e["mute"]); },
                    //function = delegate { var e = eventCaller.currentEntity; RapMen.instance.RapSaiko(e.beat, e["voice"], e["caption"], e["text"]); },
                    parameters = new List<Param>()
                    {
                        new Param("mute", false, "Mute Voice", "Toggle if the red rapper's voice should be muted.", advanced: true),
                        new Param("gender", RapMen.RedGender.Man, "Voice Type", "Choose which gender is used for the cueing.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x == (int)RapMen.RedGender.Man, new string[] { "voice" }),
                            new Param.CollapseParam((x, _) => (int)x == (int)RapMen.RedGender.Woman, new string[] { "voice2" }),
                        }),
                        new Param("voice", RapMen.RedSaikoLines.OyatsuwaSaikō, "Rap", "Choose what the red rapper should rap."),
                        new Param("voice2", RapMen.RedWomanSaikoLines.OyatsuwaSaikō, "Rap", "Choose what the red rapper should rap."),
                        new Param("caption", RapMen.CaptionTypesCue.Auto, "Subtitles", "Choose the subtitle type.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x == (int)RapMen.CaptionTypesCue.Custom, new string[] { "text" }),
                        }),
                        new Param("text", "That's why snacks are |the BEST!!", "Custom Rap", "The custom text that will appear in the rap's captions. Type a '|' to switch to the large font."),
                    },
                    defaultLength = 3f
                },

                new GameAction("banter", "Red Rapper Banter")
                {
                    function = delegate { var e = eventCaller.currentEntity; RapMen.instance.RedBanter(e.beat, e["gender"], e["voice"], e["anim"]); },
                    parameters = new List<Param>()
                    {
                        new Param("gender", RapMen.RedGender.Man, "Voice Type", "Choose which gender is used for the cueing."),
                        new Param("voice", RapMen.RedBanterLines.Yeah, "Voice Line", "Choose what the red rapper should rap."),
                        new Param("anim", true, "Play Animation", "Toggle if the Red Rapper should play the appropriate animation alongside his voice line."),
                    },
                    defaultLength = 1f
                },

                new GameAction("setText", "Set Text")
                {
                    function = delegate { var e = eventCaller.currentEntity; RapMen.instance.SetText(e.beat, e.length, e["text"], e["color"]); },
                    parameters = new List<Param>()
                    {
                        new Param("text", "We should do that again sometime.", "Text", "The custom text that will appear in the RAPMEN's captions. Type a '|' to switch to the large font."),
                        new Param("color", RapMen.CaptionColor.White, "Color", "Choose the color of the text."),
                    },
                    defaultLength = 4f,
                    resizable = true
                },

                new GameAction("background color", "Background Appearance")
                {
                    function = delegate { var e = eventCaller.currentEntity; RapMen.instance.BackgroundColor(e.beat, e.length, e["bg"], e["colorFrom"], e["colorTo"], e["colorFrom2"], e["colorTo2"], e["colorFrom3"], e["colorTo3"], e["colorFrom4"], e["colorTo4"], e["ease"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("bg", RapMen.BackgroundType.Stars, "Background Type", "Set the object appearing in the background grid spaces."),
                        new Param("colorFrom", RapMen.defaultColorA, "Color A Start", "Set the color of the squares with the star at the start of the event."),
                        new Param("colorTo", RapMen.defaultColorA, "Color A End", "Set the color of the squares with the star at the end of the event."),
                        new Param("colorFrom2", RapMen.defaultColorB, "Color B Start", "Set the color of the squares without the star at the start of the event."),
                        new Param("colorTo2", RapMen.defaultColorB, "Color B End", "Set the color of the squares without the star at the end of the event."),
                        new Param("colorFrom3", RapMen.defaultColorC, "Color C Start", "Set the color of the stars at the start of the event."),
                        new Param("colorTo3", RapMen.defaultColorC, "Color C End", "Set the color of the stars at the end of the event."),
                        new Param("colorFrom4", RapMen.defaultColorD, "Color D Start", "Set the color of speaker filling at the start of the event."),
                        new Param("colorTo4", RapMen.defaultColorD, "Color D End", "Set the bottom-most color of the speaker filling at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x != (int)Util.EasingFunction.Ease.Instant, new string[] { "colorFrom", "colorFrom2", "colorFrom3", "colorFrom4" }),
                        }),

                    },
                },

                new GameAction("language", "Subtitle Preset")
                {
                    function = delegate { var e = eventCaller.currentEntity; RapMen.instance.SubtitleType(e.beat, e["voice"]); },
                    parameters = new List<Param>()
                    {
                        new Param("voice", RapMen.CaptionTypes.Romaji, "Subtitle Preset", "Set the automatic subtitle preset for all the cues."),
                    },
                    defaultLength = 0.5f
                },

                new GameAction("womenToggle", "Rapper Toggle")
                {
                    function = delegate { var e = eventCaller.currentEntity; RapMen.instance.RapperToggle(e.beat, e["redsGender"], e["yellowsGender"]); },
                    parameters = new List<Param>()
                    {
                        new Param("redsGender", RapMen.RedGender.Man, "Red Gender", "Choose between the Male & Female Red Rapper."),
                        new Param("yellowsGender", RapMen.YellowGender.Man, "Yellow Gender", "Choose between the Male & Female \"Yellow\" Rapper."),
                    },
                    defaultLength = 0.5f
                },
            },
            new List<string>() { "agb", "normal" },
            "agbrap", "jp",
            new List<string>() { },
            chronologicalSortKey: 23,
            font: "Comet"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
    public class RapMen : Minigame
    {
        private static Color _defaultColorA;
        public static Color defaultColorA
        {
            get
            {
                ColorUtility.TryParseHtmlString("#47A1DF", out _defaultColorA);
                return _defaultColorA;
            }
        }

        private static Color _defaultColorB;
        public static Color defaultColorB
        {
            get
            {
                ColorUtility.TryParseHtmlString("#76CEFD", out _defaultColorB);
                return _defaultColorB;
            }
        }

        private static Color _defaultColorC;
        public static Color defaultColorC
        {
            get
            {
                ColorUtility.TryParseHtmlString("#D3F4FE", out _defaultColorC);
                return _defaultColorC;
            }
        }

        private static Color _defaultColorD;
        public static Color defaultColorD
        {
            get
            {
                ColorUtility.TryParseHtmlString("#FFFFFF", out _defaultColorD);
                return _defaultColorD;
            }
        }

        public enum BackgroundType
        {
            Stars = 1,
            Hearts = 2
        }

        public enum CaptionColor
        {
            White,
            Blue,
            Pink,
            Yellow
        }

        public enum RedBanterLines
        {
            Yeah = 1,
            Yo = 2
        }

        public enum CaptionTypes
        {
            Romaji = 1,
            Japanese = 2,
            Silver = 10,
            Advance = 11
        }

        public enum CaptionTypesCue
        {
            Auto = 0,
            Romaji = 1,
            Japanese = 2,
            Silver = 10,
            Advance = 11,
            Custom = 999
        }

        public enum RedDesukaLines
        {
            Wakarimasuka = 1,
            SanjiDesuka = 2,
            OyatsuDesuka = 3,
            HontoDesuka = 4,
        }
        public enum RedWomanDesukaLines
        {
            OyatsuDesuka = 1,
            JujiDesuka = 2,
            NaishoDesuka = 3,
        }

        public enum RedKamoneLines
        {
            TanoshīKamone = 1,
            OishīKamone = 2,
            HerushīKamone = 3,
            OrenoseiKamone = 4,
            SoremosōKamone = 5,
        }
        public enum RedWomanKamoneLines
        {
            KareNoOyatsuDane = 1,
            AketeiiKamone = 2,
            TabeteiiKamone = 3,
        }

        public enum RedSaikoLines
        {
            OyatsuwaSaikō = 1,
            KibunwaSaiko = 2,
            OretteSaikō = 3,
            KimitteSaikō = 4,
            OyatsugaNaiyō = 5,
            OreShiranaiyō = 6,
        }
        public enum RedWomanSaikoLines
        {
            OyatsuwaSaikō = 1,
            AmakuteSaikō = 2,
            KibunwaSaikō = 3,
            BetsubaraSaikō = 4,
            KareniwaNaishō = 5,
            DareNiMoNaishō = 6,
        }

        public enum RedGender
        {
            Man = 0,
            Woman = 1,
        }

        public enum YellowGender
        {
            Man = 0,
            Woman = 1,
        }

        [Header("Components")]
        [SerializeField] Animator rapperRed;
        [SerializeField] Animator rapperYellow;
        [SerializeField] Animator rapperCherry;
        [SerializeField] Animator rapperBlue;
        [SerializeField] GameObject rapperRedObj;
        [SerializeField] GameObject rapperYellowObj;
        [SerializeField] GameObject rapperCherryObj;
        [SerializeField] GameObject rapperBlueObj;
        [SerializeField] TMP_Text rapText;
        [SerializeField] TMP_ColorGradient[] gradients;
        [SerializeField] ParticleSystem[] justParticles;
        [SerializeField] ParticleSystem uhnParticle;

        [Header("Background")]
        [SerializeField] Material backgroundMaterial;
        [SerializeField] Material speakerMaterial;
        [SerializeField] Animator background;

        private double colorStartBeat = -1;
        private float colorLength = 0f;
        private Color colorStartA;
        private Color colorStartB;
        private Color colorStartC;
        private Color colorStartD;
        private Color colorEndA;
        private Color colorEndB;
        private Color colorEndC;
        private Color colorEndD;
        private Util.EasingFunction.Ease colorEase;

        [Header("Variables")]
        private bool redBop = true;
        private bool redCanBop = true;
        private bool yellowBop = true;
        private bool yellowCanBop = true;
        private CaptionTypes subtitleLanguage = CaptionTypes.Romaji;
        public float cymbalPitch;
        public float cymbalVolume;
        public float drumPitch;
        public float drumVolume;
        private bool redIsWoman = false;
        private bool yellowIsWoman = false;
        public static List<QueuedRap> queuedRaps = new List<QueuedRap>();
        public struct QueuedRap
        {
            public double beat;
            public int gender;
            public int voice;
            public int voice2;
            public int caption;
            public string text;
            public string rap;
        }



        #region Subtitle Text
        // Desuka
        private Dictionary<int, string> DesukaJP = new Dictionary<int, string>
        {
            [1] = "わかります|か?",
            [2] = "PM 3:00 です|か?",
            [3] = "おやつ です|か?",
            [4] = "ほんとです|か?",
        };
        private Dictionary<int, string> DesukaRM = new Dictionary<int, string>
        {
            [1] = "Yo, wakari|masuka?",
            [2] = "Yo, sanji |desuka?",
            [3] = "Yo, oyatsu |desuka?",
            [4] = "Honto |desuka?",
        };
        private Dictionary<int, string> DesukaSV = new Dictionary<int, string>
        {
            [1] = "It's understood, |isn't it?",
            [2] = "It's already 3PM, |isn't it?",
            [3] = "It's time for a snack, |isn't it?",
            [4] = "It's all good, |isn't it?"
        };
        private Dictionary<int, string> DesukaAD = new Dictionary<int, string>
        {
            [1] = "You get it now, |huh?",
            [2] = "It's 3PM, |huh?",
            [3] = "It's snack time, |huh?",
            [4] = "Maybe so, |huh?"
        };

        // WomanDesuka
        private Dictionary<int, string> WomanDesukaJP = new Dictionary<int, string>
        {
            [1] = "おやつ　です|か?",
            [2] = "AM 10:00　です|か?",
            [3] = "ナイショです|か?",
        };
        private Dictionary<int, string> WomanDesukaRM = new Dictionary<int, string>
        {
            [1] = "Yo, oyatsu |desuka?",
            [2] = "Yo, juji |desuka?",
            [3] = "Naisho |desuka?",
        };
        private Dictionary<int, string> WomanDesukaSV = new Dictionary<int, string>
        {
            [1] = "It's time for a snack, |isn't it?",
            [2] = "It's 10 AM, |isn't it?",
            [3] = "It's our little secret, |isn't it?",
        };
        private Dictionary<int, string> WomanDesukaAD = new Dictionary<int, string>
        {
            [1] = "It's snack time, |huh?",
            [2] = "It's 10 AM, |right?",
            [3] = "Who's to know, |right?",
        };

        // Kamone
        private Dictionary<int, string> KamoneJP = new Dictionary<int, string>
        {
            [1] = "たのしい かも|ネ",
            [2] = "おいしい かも|ネ",
            [3] = "ヘルシー かも|ネ",
            [4] = "オレの せいかも|ネ",
            [5] = "それもそう かも|ネ",
        };
        private Dictionary<int, string> KamoneRM = new Dictionary<int, string>
        {
            [1] = "Tanoshi, |kamone.",
            [2] = "Oishi, |kamone.",
            [3] = "Herushi, |kamone.",
            [4] = "Orenosei, |kamone.",
            [5] = "Soremoso, |kamone.",
        };
        private Dictionary<int, string> KamoneSV = new Dictionary<int, string>
        {
            [1] = "Havin' fun, it |could be",
            [2] = "Something sweet, it |could be",
            [3] = "Something salty, it |could be",
            [4] = "Some sugar candies, it |could be",
            [5] = "A bag of chips, it |could be"
        };
        private Dictionary<int, string> KamoneAD = new Dictionary<int, string>
        {
            [1] = "Havin' fun, |ya feel me?",
            [2] = "Tasty snacks, |ya feel me?",
            [3] = "Healthy snacks, |ya feel me?",
            [4] = "Might've been us, |ya feel me?",
            [5] = "That's how it be, |ya feel me?"
        };

        // WomanKamone
        private Dictionary<int, string> WomanKamoneJP = new Dictionary<int, string>
        {
            [1] = "カレのおやつ だ|ね",
            [2] = "あけていー　かも|ネ",
            [3] = "たべていー　かも|ネ",
        };
        private Dictionary<int, string> WomanKamoneRM = new Dictionary<int, string>
        {
            [1] = "Kare no oyatsu, |dane.",
            [2] = "Akete ii, |kamone.",
            [3] = "Tabete ii, |kamone.",
        };
        private Dictionary<int, string> WomanKamoneSV = new Dictionary<int, string>
        {
            [1] = "The Rap Men are out, it |could be",
            [2] = "Their door is open... |could it be?",
            [3] = "They've got snacks, I |could see",
        };
        private Dictionary<int, string> WomanKamoneAD = new Dictionary<int, string>
        {
            [1] = "These are all their snacks, |ya see?",
            [2] = "Open 'em up, |ya feel me?",
            [3] = "Eat 'em all up, |ya feel me?",
        };

        // Saiko
        private Dictionary<int, string> SaikoJP = new Dictionary<int, string>
        {
            [1] = "おやつは |サイコー!!",
            [2] = "きぶんは |サイコー!!",
            [3] = "オレって |サイコー!!",
            [4] = "キミって |サイコー!!",
            [5] = "おやつが |ナイヨー!!",
            [6] = "オレ, しらナイ|ヨー!!",
        };
        private Dictionary<int, string> SaikoRM = new Dictionary<int, string>
        {
            [1] = "Oyatsuwa |saiko!!",
            [2] = "Kibunwa |saiko!!",
            [3] = "Orette |saiko!!",
            [4] = "Kimitte |saiko!!",
            [5] = "Oyatsuga |naiyo!!",
            [6] = "Ore, shiranai|yo!!",
        };
        private Dictionary<int, string> SaikoSV = new Dictionary<int, string>
        {
            [1] = "That's why snacks are |the BEST!!",
            [2] = "They make you feel |the BEST!!",
            [3] = "I am |the BEST!!",
            [4] = "You are |the BEST!!",
            [5] = "Hey, who swiped |the rest?",
            [6] = "Lack of snacks makes me |depressed!!",
        };
        private Dictionary<int, string> SaikoAD = new Dictionary<int, string>
        {
            [1] = "Snacking is so |AWESOME!",
            [2] = "I feel so |AWESOME!",
            [3] = "We are so |AWESOME!",
            [4] = "You are so |AWESOME!",
            [5] = "Wait a sec, we |LOST SOME!",
            [6] = "Guess it's how we |LOST SOME!",
        };

        // Saiko
        private Dictionary<int, string> WomanSaikoJP = new Dictionary<int, string>
        {
            [1] = "おやつは |サイコー!!",
            [2] = "あまくて |サイコー!!",
            [3] = "きぶんは |サイコー!!",
            [4] = "べつばら |サイコー!!",
            [5] = "カレには |ナイヨー!!",
            [6] = "ダレにも |ナイヨー!!",
        };
        private Dictionary<int, string> WomanSaikoRM = new Dictionary<int, string>
        {
            [1] = "Oyatsuwa |saiko!!",
            [2] = "Amakute |saiko!!",
            [3] = "Kibunwa |saiko!!",
            [4] = "Betsubara |saiko!!",
            [5] = "Kareniwa |naisho!!",
            [6] = "Darenimo |naisho!!",
        };
        private Dictionary<int, string> WomanSaikoSV = new Dictionary<int, string>
        {
            [1] = "Snacks are |the BEST!!",
            [2] = "Sweet or salty is |the BEST!!",
            [3] = "They make you feel |the BEST!!",
            [4] = "Raps and snacks are |the BEST!!",
            [5] = "Secret snacking is |the BEST!!",
            [6] = "Now let's wrap this rap to |digest!!",
        };
        private Dictionary<int, string> WomanSaikoAD = new Dictionary<int, string>
        {
            [1] = "Snacking is so |AWESOME!",
            [2] = "Sweets are so |AWESOME!",
            [3] = "I feel so |AWESOME!",
            [4] = "Cakes are so |AWESOME!",
            [5] = "Clueless that we |STOLE 'EM!",
            [6] = "Tell 'em we are |LOATHSOME!",
        };

        #endregion

        public static RapMen instance;


        void Awake()
        {
            instance = this;

            colorStartA = defaultColorA;
            colorStartB = defaultColorB;
            colorStartC = defaultColorC;
            colorStartD = defaultColorD;
            colorEndA = defaultColorA;
            colorEndB = defaultColorB;
            colorEndC = defaultColorC;
            colorEndD = defaultColorD;

            rapText.text = "";
        }

        void OnDestroy()
        {
            if (queuedRaps.Count > 0) queuedRaps.Clear();
        }

        void Update()
        {
            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            {
                ScoreMiss();
                SoundByte.PlayOneShotGame("rapMen/whiff");
                RapperAnim("Player", "bop");
            }

            BackgroundColorUpdate();
        }

        void LateUpdate()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (queuedRaps.Count > 0)
                {
                    foreach (var p in queuedRaps)
                    {
                        if (p.rap == "desuka") { RapMen.instance.RapDesuka(p.beat, p.gender, p.voice, p.voice2, p.caption, p.text); }
                        else if (p.rap == "kamone") { RapMen.instance.RapKamone(p.beat, p.gender, p.voice, p.voice2, p.caption, p.text); }
                        else if (p.rap == "saiko") { RapMen.instance.RapSaiko(p.beat, p.gender, p.voice, p.voice2, p.caption, p.text); }
                    }
                    queuedRaps.Clear();
                }
            }
        }

        public override void OnGameSwitch(double beat)
        {
            PersistColor(beat);
        }


        public override void OnLateBeatPulse(double beat)
        {
            if (redBop && !(rapperRed.IsPlayingAnimationNames("yo", "kamone", "saiko", "just") || rapperCherry.IsPlayingAnimationNames("yo", "kamone", "saiko", "just")))
            {
                RapperAnim("Red", "bop");
            }
            if (yellowBop && !(rapperYellow.IsPlayingAnimationNames("bop", "just", "prepare", "miss") || rapperBlue.IsPlayingAnimationNames("bop", "just", "prepare", "miss")))
            {
                RapperAnim("Player", "bop");
            }

        }

        public void Bop(double beat, float length, bool red, bool yellow, bool redAuto, bool yellowAuto)
        {
            redBop = redAuto;
            yellowBop = yellowAuto;

            if (red || yellow)
            {
                List<BeatAction.Action> bops = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                {
                    bops.Add(new BeatAction.Action(beat + i, delegate
                    {
                        if (red)
                        {
                            RapperAnim("Red", "bop");
                        }
                        if (yellow)
                        {
                            RapperAnim("Player", "bop");
                        }
                    }));
                }
                BeatAction.New(instance, bops);
            }

        }

        public static void SfxRaps(double beat, int genderType, int voiceType, int womanVoiceType, int captionType, string customText, string cueType, bool mute)
        {
            if (GameManager.instance.currentGame == "rapMen")
            {
                if (cueType == "desuka") { RapMen.instance.RapDesuka(beat, genderType, voiceType, womanVoiceType, captionType, customText); }
                else if (cueType == "kamone") { RapMen.instance.RapKamone(beat, genderType, voiceType, womanVoiceType, captionType, customText); }
                else if (cueType == "saiko") { RapMen.instance.RapSaiko(beat, genderType, voiceType, womanVoiceType, captionType, customText); }
            }
            else
            {
                queuedRaps.Add(new QueuedRap { beat = beat, gender = genderType, voice = voiceType, voice2 = womanVoiceType, caption = captionType, text = customText, rap = cueType });
            }

            if (!mute)
            {
                if (cueType == "desuka")
                {
                    if (genderType == 1) /*if gender is female*/ { 
                    switch (womanVoiceType)
                    {
                        case 1:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/rapWoman/yoW", beat, offset: 0.03, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/oyatsuA1W", beat + 0.75f, offset: 0.05, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/oyatsuA2W", beat + 1f, offset: 0.01, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/desuAW", beat + 1.50f, offset: 0.00, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/kaAW", beat + 2.00f, offset: 0.015, panning: -0.25f)
                            }, forcePlay: true);
                            break;

                        case 2:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/rapWoman/yoW", beat, offset: 0.03, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/juW", beat + 0.75f, volume: 0.75f, offset: 0.003, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/jiW", beat + 1.25f, volume: 0.75f, offset: 0.001, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/desuAW", beat + 1.50f, offset: 0.00, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/kaAW", beat + 2.00f, offset: 0.015, panning: -0.25f)
                            }, forcePlay: true);
                            break;

                        case 3:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/rapWoman/naishoA1W", beat, offset: 0.1, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/naishoA2W", beat + 0.25f, offset: 0.035, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/desuBW", beat + 0.5f, offset: 0.01, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/kaBW", beat + 0.75f, offset: 0.02, panning: -0.25f)
                            }, forcePlay: true);
                            break;
                    }
                    }
                    else /*if gender is male*/ {
                    switch (voiceType)
                    {
                        case 1:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/yo", beat, offset: 0.03, panning: -0.25f),
                    new MultiSound.Sound("rapMen/wakari1", beat + 0.75f, offset: 0.025, panning: -0.25f),
                    new MultiSound.Sound("rapMen/wakari2", beat + 1.00f, offset: 0.012, panning: -0.25f),
                    new MultiSound.Sound("rapMen/wakari3", beat + 1.25f, offset: 0.022, panning: -0.25f),
                    new MultiSound.Sound("rapMen/masuka1", beat + 1.50f, offset: 0.093, panning: -0.25f),
                    new MultiSound.Sound("rapMen/masuka2", beat + 2.00f, offset: 0.017, panning: -0.25f)
                            }, forcePlay: true);
                            break;

                        case 2:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/yo", beat, offset: 0.03, panning: -0.25f),
                    new MultiSound.Sound("rapMen/sanji1", beat + 0.75f, offset: 0.094, panning: -0.25f),
                    new MultiSound.Sound("rapMen/sanji2", beat + 1.25f, offset: 0.009, panning: -0.25f),
                    new MultiSound.Sound("rapMen/desuA", beat + 1.50f, offset: 0.02, panning: -0.25f),
                    new MultiSound.Sound("rapMen/kaA", beat + 2.00f, offset: 0.003, panning: -0.25f)
                            }, forcePlay: true);
                            break;

                        case 3:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/yo", beat, offset: 0.03, panning: -0.25f),
                    new MultiSound.Sound("rapMen/oyatsu1", beat + 0.75f, offset: 0.03, panning: -0.25f),
                    new MultiSound.Sound("rapMen/oyatsu2", beat + 1.00f, offset: 0.02, panning: -0.25f),
                    new MultiSound.Sound("rapMen/desuA", beat + 1.50f, offset: 0.02, panning: -0.25f),
                    new MultiSound.Sound("rapMen/kaA", beat + 2.00f, offset: 0.003, panning: -0.25f)
                            }, forcePlay: true);
                            break;

                        case 4:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/honto1", beat - 0.25, offset: 0.029, panning: -0.25f),
                    new MultiSound.Sound("rapMen/honto2", beat + 0.25f, offset: 0.004, panning: -0.25f),
                    new MultiSound.Sound("rapMen/desuB", beat + 0.5f, offset: 0.01, panning: -0.25f),
                    new MultiSound.Sound("rapMen/kaB", beat + 1f, offset: 0.02, panning: -0.25f)
                            }, forcePlay: true);
                            break;
                    }
                    }
                }
                else if (cueType == "kamone")
                {
                    if (genderType == 1) /*if gender is female*/ {
                        switch (womanVoiceType)
                    {
                        case 1:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/rapWoman/kareA1W", beat, offset: 0.034, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/kareA2W", beat + 0.25f, offset: 0.008, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/noW", beat + 0.5f, offset: 0.017, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/oyatsuB1W", beat + 0.75f, offset: 0.0, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/oyatsuB2W", beat + 1.25f, offset: 0.2, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/dane1W", beat + 1.5f, offset: 0.031, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/dane2W", beat + 1.75f, offset: 0.037, panning: -0.25f)
                            }, forcePlay: true);
                            break;

                        case 2:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/rapWoman/akete1W", beat, offset: 0.007, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/akete2W", beat + 0.25f, offset: 0.016, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/akete3W", beat + 0.5f, offset: 0.014, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/iiAW", beat + 0.75f, offset: 0.014, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/kamone1W", beat + 1.25f, offset: 0.008, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/kamone2W", beat + 1.5f, offset: 0.017, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/kamone3W", beat + 1.75f, offset: 0.005, panning: -0.25f)
                            }, forcePlay: true);
                            break;

                        case 3:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/rapWoman/tabete1W", beat, offset: 0.007, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/tabete2W", beat + 0.25f, offset: 0.016, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/tabete3W", beat + 0.5f, offset: 0.014, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/iiAW", beat + 0.75f, offset: 0.014, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/kamone1W", beat + 1.25f, offset: 0.008, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/kamone2W", beat + 1.5f, offset: 0.017, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/kamone3W", beat + 1.75f, offset: 0.005, panning: -0.25f)
                            }, forcePlay: true);
                            break;
                    }
                    }
                    else /*if gender is male*/ {
                    switch (voiceType)
                    {
                        case 1:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/tanoshi1", beat, offset: 0.008, panning: -0.25f),
                    new MultiSound.Sound("rapMen/tanoshi2", beat + 0.25f, offset: 0.0, panning: -0.25f),
                    new MultiSound.Sound("rapMen/tanoshi3", beat + 0.5f, offset: 0.067, panning: -0.25f),
                    new MultiSound.Sound("rapMen/kamone1", beat + 1.25f, offset: 0.01, panning: -0.25f),
                    new MultiSound.Sound("rapMen/kamone2", beat + 1.5f, offset: 0.0, panning: -0.25f),
                    new MultiSound.Sound("rapMen/kamone3", beat + 1.75f, offset: 0.035, panning: -0.25f)
                            }, forcePlay: true);
                            break;

                        case 2:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/oishi1", beat, offset: 0.019, panning: -0.25f),
                    new MultiSound.Sound("rapMen/oishi3", beat + 0.5f, offset: 0.135, panning: -0.25f),
                    new MultiSound.Sound("rapMen/kamone1", beat + 1.25f, offset: 0.01, panning: -0.25f),
                    new MultiSound.Sound("rapMen/kamone2", beat + 1.5f, offset: 0.0, panning: -0.25f),
                    new MultiSound.Sound("rapMen/kamone3", beat + 1.75f, offset: 0.035, panning: -0.25f)
                            }, forcePlay: true);
                            break;

                        case 3:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/herushi1", beat, offset: 0.047, panning: -0.25f),
                    new MultiSound.Sound("rapMen/herushi2", beat + 0.25f, offset: 0.008, panning: -0.25f),
                    new MultiSound.Sound("rapMen/herushi3", beat + 0.5f, offset: 0.064, panning: -0.25f),
                    new MultiSound.Sound("rapMen/kamone1", beat + 1.25f, offset: 0.01, panning: -0.25f),
                    new MultiSound.Sound("rapMen/kamone2", beat + 1.5f, offset: 0.0, panning: -0.25f),
                    new MultiSound.Sound("rapMen/kamone3", beat + 1.75f, offset: 0.035, panning: -0.25f)
                            }, forcePlay: true);
                            break;

                        case 4:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/orenosei1", beat, offset: 0.0, panning: -0.25f),
                    new MultiSound.Sound("rapMen/orenosei2", beat + 0.25f, offset: 0.006, panning: -0.25f),
                    new MultiSound.Sound("rapMen/orenosei3", beat + 0.5f, offset: 0.022, panning: -0.25f),
                    new MultiSound.Sound("rapMen/orenosei4", beat + 0.75f, offset: 0.175, panning: -0.25f),
                    new MultiSound.Sound("rapMen/kamone1", beat + 1.25f, offset: 0.01, panning: -0.25f),
                    new MultiSound.Sound("rapMen/kamone2", beat + 1.5f, offset: 0.0, panning: -0.25f),
                    new MultiSound.Sound("rapMen/kamone3", beat + 1.75f, offset: 0.035, panning: -0.25f)
                            }, forcePlay: true);
                            break;

                        case 5:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/soremoso1", beat, offset: 0.038, panning: -0.25f),
                    new MultiSound.Sound("rapMen/soremoso2", beat + 0.25f, offset: 0.01, panning: -0.25f),
                    new MultiSound.Sound("rapMen/soremoso3", beat + 0.5f, offset: 0.05, panning: -0.25f),
                    new MultiSound.Sound("rapMen/soremoso4", beat + 0.75f, offset: 0.143, panning: -0.25f),
                    new MultiSound.Sound("rapMen/kamone1", beat + 1.25f, offset: 0.01, panning: -0.25f),
                    new MultiSound.Sound("rapMen/kamone2", beat + 1.5f, offset: 0.0, panning: -0.25f),
                    new MultiSound.Sound("rapMen/kamone3", beat + 1.75f, offset: 0.035, panning: -0.25f)
                            }, forcePlay: true);
                            break;
                    }
                    }
                }
                else if (cueType == "saiko")
                {
                    if (genderType == 1) /*if gender is female*/ {
                        switch (womanVoiceType)
                    {
                        case 1:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/rapWoman/oyatsuC1W", beat, offset: 0.008, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/oyatsuC2W", beat + 0.25f, offset: 0.009, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/oyatsuC3W", beat + 0.5f, offset: 0.121, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/waAW", beat + 0.75f, offset: 0.007, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/saikoA1W", beat + 1f, offset: 0.084, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/saikoA2W", beat + 1.5f, offset: 0.027, panning: -0.25f)
                            }, forcePlay: true);
                            break;

                        case 2:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/rapWoman/amakute1W", beat, offset: 0.01, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/amakute2W", beat + 0.25f, offset: 0.005, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/amakute3W", beat + 0.5f, offset: 0.03, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/amakute4W", beat + 0.75f, offset: 0.014, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/saikoA1W", beat + 1f, offset: 0.084, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/saikoA2W", beat + 1.5f, offset: 0.027, panning: -0.25f)
                            }, forcePlay: true);
                            break;

                        case 3:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/rapWoman/kibun1W", beat, offset: 0.014, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/kibun2W", beat + 0.25f, offset: 0.00, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/waBW", beat + 0.75f, offset: 0.024, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/saikoA1W", beat + 1f, offset: 0.084, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/saikoA2W", beat + 1.5f, offset: 0.027, panning: -0.25f)
                            }, forcePlay: true);
                            break;

                        case 4:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/rapWoman/betsuW", beat, offset: 0.032, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/baraW", beat + 0.5f, offset: 0.017, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/saikoA1W", beat + 1f, offset: 0.084, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/saikoA2W", beat + 1.5f, offset: 0.027, panning: -0.25f)
                            }, forcePlay: true);
                            break;

                        case 5:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/rapWoman/kareB1W", beat, offset: 0.024, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/kareB2W", beat + 0.25f, offset: 0.016, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/niBW", beat + 0.5f, offset: 0.029, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/waCW", beat + 0.75f, offset: 0.041, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/naishoB1W", beat + 1f, offset: 0.058, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/naishoB2W", beat + 1.5f, offset: 0.057, panning: -0.25f)
                            }, forcePlay: true);
                            break;

                        case 6:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/rapWoman/dare1W", beat, offset: 0.006, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/dare2W", beat + 0.25f, offset: 0.013, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/niAW", beat + 0.5f, offset: 0.026, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/moW", beat + 0.75f, offset: 0.036, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/naishoB1W", beat + 1f, offset: 0.058, panning: -0.25f),
                    new MultiSound.Sound("rapMen/rapWoman/naishoB2W", beat + 1.5f, offset: 0.057, panning: -0.25f)
                            }, forcePlay: true);
                            break;
                    }
                    }
                    else /*if gender is male*/ {             
                        switch (voiceType)
                    {
                        case 1:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/oyatsuwa1", beat, offset: 0.004, panning: -0.25f),
                    new MultiSound.Sound("rapMen/oyatsuwa2", beat + 0.25f, offset: 0.033, panning: -0.25f),
                    new MultiSound.Sound("rapMen/oyatsuwa3", beat + 0.5f, offset: 0.087, panning: -0.25f),
                    new MultiSound.Sound("rapMen/oyatsuwa4", beat + 0.75f, offset: 0.01, panning: -0.25f),
                    new MultiSound.Sound("rapMen/saiko1", beat + 1f, offset: 0.125, panning: -0.25f),
                    new MultiSound.Sound("rapMen/saiko2", beat + 1.5f, offset: 0.005, panning: -0.25f)
                            }, forcePlay: true);
                            break;

                        case 2:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/kibunwa1", beat, offset: 0.005, panning: -0.25f),
                    new MultiSound.Sound("rapMen/kibunwa2", beat + 0.25f, offset: 0.016, panning: -0.25f),
                    new MultiSound.Sound("rapMen/kibunwa3", beat + 0.75f, offset: 0.024, panning: -0.25f),
                    new MultiSound.Sound("rapMen/saiko1", beat + 1f, offset: 0.125, panning: -0.25f),
                    new MultiSound.Sound("rapMen/saiko2", beat + 1.5f, offset: 0.005, panning: -0.25f)
                            }, forcePlay: true);
                            break;

                        case 3:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/orette1", beat, offset: 0.006, panning: -0.25f),
                    new MultiSound.Sound("rapMen/orette2", beat + 0.25f, offset: 0.013, panning: -0.25f),
                    new MultiSound.Sound("rapMen/orette3", beat + 0.75f, offset: 0.003, panning: -0.25f),
                    new MultiSound.Sound("rapMen/saiko1", beat + 1f, offset: 0.125, panning: -0.25f),
                    new MultiSound.Sound("rapMen/saiko2", beat + 1.5f, offset: 0.005, panning: -0.25f)
                            }, forcePlay: true);
                            break;

                        case 4:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/kimitte1", beat, offset: 0.001, panning: -0.25f),
                    new MultiSound.Sound("rapMen/kimitte2", beat + 0.25f, offset: 0.038, panning: -0.25f),
                    new MultiSound.Sound("rapMen/kimitte3", beat + 0.75f, offset: 0.003, panning: -0.25f),
                    new MultiSound.Sound("rapMen/saiko1", beat + 1f, offset: 0.125, panning: -0.25f),
                    new MultiSound.Sound("rapMen/saiko2", beat + 1.5f, offset: 0.005, panning: -0.25f)
                            }, forcePlay: true);
                            break;

                        case 5:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/oyatsuga1", beat, offset: 0.004, panning: -0.25f),
                    new MultiSound.Sound("rapMen/oyatsuga2", beat + 0.25f, offset: 0.033, panning: -0.25f),
                    new MultiSound.Sound("rapMen/oyatsuga3", beat + 0.5f, offset: 0.087, panning: -0.25f),
                    new MultiSound.Sound("rapMen/oyatsuga4", beat + 0.75f, offset: 0.01, panning: -0.25f),
                    new MultiSound.Sound("rapMen/naiyo1", beat + 1f, offset: 0.07, panning: -0.25f),
                    new MultiSound.Sound("rapMen/naiyo2", beat + 1.5f, offset: 0.003, panning: -0.25f)
                            }, forcePlay: true);
                            break;

                        case 6:
                            MultiSound.Play(new MultiSound.Sound[]
                            {
                    new MultiSound.Sound("rapMen/oreshira1", beat, offset: 0.006, panning: -0.25f),
                    new MultiSound.Sound("rapMen/oreshira2", beat + 0.25f, offset: 0.083, panning: -0.25f),
                    new MultiSound.Sound("rapMen/oreshira3", beat + 0.5f, offset: 0.083, panning: -0.25f),
                    new MultiSound.Sound("rapMen/oreshira4", beat + 0.75f, offset: 0.009, panning: -0.25f),
                    new MultiSound.Sound("rapMen/naiyo1", beat + 1f, offset: 0.07, panning: -0.25f),
                    new MultiSound.Sound("rapMen/naiyo2", beat + 1.5f, offset: 0.003, panning: -0.25f)
                            }, forcePlay: true);
                            break;
                    }
                    }
                }
            }
        }

        public void SetText(double beat, float length, string text, int color)
        {
            text = text.Replace("|", "<size=135%>");
            BeatAction.New(instance, new List<BeatAction.Action>() {
                new BeatAction.Action(beat, delegate {
                    rapText.text = text;
                    rapText.colorGradientPreset = gradients[color];
                }),
                new BeatAction.Action(beat + length, delegate {
                    rapText.text = "";
                }),
            });
        }

        public void RapDesuka(double beat, int genderType, int voiceType, int womanVoiceType, int captionType, string customText)
        {
            var voiceText = "U Rappin Bad";

            var DesukaDict = DesukaRM;
            if (captionType == 0) { captionType = (int)subtitleLanguage; }

            switch (captionType)
            {
                case 1:
                    if (genderType == 1) { DesukaDict = WomanDesukaRM; }
                    else { DesukaDict = DesukaRM; }
                    break;

                case 2:
                    if (genderType == 1) { DesukaDict = WomanDesukaJP; }
                    else { DesukaDict = DesukaJP; }
                    break;

                case 10:
                    if (genderType == 1) { DesukaDict = WomanDesukaSV; }
                    else { DesukaDict = DesukaSV; }
                    break;
                case 11:
                    if (genderType == 1) { DesukaDict = WomanDesukaAD; }
                    else { DesukaDict = DesukaAD; }
                    break;
            }

            if (captionType != 999)
            {
                if (genderType == 1) { voiceText = DesukaDict[womanVoiceType]; }
                else { voiceText = DesukaDict[voiceType]; }
            }
            else
            {
                voiceText = customText;
            }
            voiceText = voiceText.Replace("|", "<size=135%>");

            if ((genderType == 0 && voiceType == 4) || (genderType == 1 && womanVoiceType == 3))
            {
                BeatAction.New(instance, new List<BeatAction.Action>() {
                new BeatAction.Action(beat - 0.25, delegate { rapText.text = voiceText; rapText.colorGradientPreset = gradients[1]; }),
                new BeatAction.Action(beat - 0.25, delegate { redCanBop = false; }),
                new BeatAction.Action(beat - 0.25, delegate { RapperAnim("Red", "kamone"); }),
                new BeatAction.Action(beat + 1.5, delegate { RapperAnim("Player", "prepare"); }),
                });
            }
            else
            {
                BeatAction.New(instance, new List<BeatAction.Action>() {
                new BeatAction.Action(beat, delegate { rapText.text = voiceText; rapText.colorGradientPreset = gradients[1];  }),
                new BeatAction.Action(beat, delegate { redCanBop = false; }),
                new BeatAction.Action(beat, delegate { RapperAnim("Red", "yo"); }),
                new BeatAction.Action(beat + 1, delegate { RapperAnim("Red", "yo"); }),
                new BeatAction.Action(beat + 2, delegate { RapperAnim("Red", "yo"); }),
                new BeatAction.Action(beat + 2.5, delegate { RapperAnim("Player", "prepare"); }),
                });
            }

            ScheduleInput(beat, ((genderType == 0 && voiceType == 4) || (genderType == 1 && womanVoiceType == 3) ? 2 : 3), InputAction_BasicPress, RapJust, RapMiss, Nothing);
        }

        public void RapKamone(double beat, int genderType, int voiceType, int womanVoiceType, int captionType, string customText)
        {
            var voiceText = "WildBreathG4M3R";

            var KamoneDict = KamoneRM;
            if (captionType == 0) { captionType = (int)subtitleLanguage; }

            switch (captionType)
            {
                case 1:
                    if (genderType == 1) { KamoneDict = WomanKamoneRM; }
                    else { KamoneDict = KamoneRM; }
                    break;

                case 2:
                    if (genderType == 1) { KamoneDict = WomanKamoneJP; }
                    else { KamoneDict = KamoneJP; }
                    break;

                case 10:
                    if (genderType == 1) { KamoneDict = WomanKamoneSV; }
                    else { KamoneDict = KamoneSV; }
                    break;
                case 11:
                    if (genderType == 1) { KamoneDict = WomanKamoneAD; }
                    else { KamoneDict = KamoneAD; }
                    break;
            }

            if (captionType != 999)
            {
                if (genderType == 1) { voiceText = KamoneDict[womanVoiceType]; }
                else { voiceText = KamoneDict[voiceType]; }
            }
            else
            {
                voiceText = customText;
            }
            voiceText = voiceText.Replace("|", "<size=135%>");
            BeatAction.New(instance, new List<BeatAction.Action>() {
                new BeatAction.Action(beat, delegate { rapText.text = voiceText; rapText.colorGradientPreset = gradients[2]; }),
                new BeatAction.Action(beat, delegate { redCanBop = false; }),
                new BeatAction.Action(beat, delegate { RapperAnim("Red", "kamone"); }),
                new BeatAction.Action(beat + 1.25, delegate { RapperAnim("Red", "yo"); }),
                new BeatAction.Action(beat + 2, delegate { RapperAnim("Player", "prepare"); }),
                new BeatAction.Action(beat + 2, delegate { rapText.text = ""; }),
                new BeatAction.Action(beat + 3, delegate { RapperAnim("Player", "prepare"); }),
            });

            ScheduleInput(beat, 2.5, InputAction_BasicPress, RapJust, RapMiss, Nothing);
            ScheduleInput(beat, 3.5, InputAction_BasicPress, RapJust, RapMiss, Nothing);
        }

        public void RapSaiko(double beat, int genderType, int voiceType, int womanVoiceType, int captionType, string customText)
        {
            var voiceText = "Surreptitiously";

            var SaikoDict = SaikoRM;
            if (captionType == 0) { captionType = (int)subtitleLanguage; }

            switch (captionType)
            {
                case 1:
                    if (genderType == 1) { SaikoDict = WomanSaikoRM; }
                    else { SaikoDict = SaikoRM; }
                    break;

                case 2:
                    if (genderType == 1) { SaikoDict = WomanSaikoJP; }
                    else { SaikoDict = SaikoJP; }
                    break;

                case 10:
                    if (genderType == 1) { SaikoDict = WomanSaikoSV; }
                    else { SaikoDict = SaikoSV; }
                    break;
                case 11:
                    if (genderType == 1) { SaikoDict = WomanSaikoAD; }
                    else { SaikoDict = SaikoAD; }
                    break;
            }

            if (captionType != 999)
            {
                if (genderType == 1) { voiceText = SaikoDict[womanVoiceType]; }
                else { voiceText = SaikoDict[voiceType]; }
            }
            else
            {
                voiceText = customText;
            }
            voiceText = voiceText.Replace("|", "<size=135%>");

            BeatAction.New(instance, new List<BeatAction.Action>() {
                new BeatAction.Action(beat, delegate { rapText.text = voiceText; rapText.colorGradientPreset = gradients[3]; }),
            });

            var rapAnim = "saiko";
            if (voiceType >= 5) { rapAnim = "yo"; }
            BeatAction.New(instance, new List<BeatAction.Action>() {
                new BeatAction.Action(beat, delegate { redCanBop = false; }),
                new BeatAction.Action(beat, delegate { RapperAnim("Red", rapAnim); }),
                new BeatAction.Action(beat + 1, delegate { RapperAnim("Red", rapAnim); }),
                new BeatAction.Action(beat + 1.5, delegate { RapperAnim("Player", "prepare"); }),
            });

            ScheduleInput(beat, 2, InputAction_BasicPress, RapJust, RapMiss, Nothing);
            ScheduleInput(beat, 2.5, InputAction_BasicPress, RapJustAlt, RapMiss, Nothing);
        }

        public void RedBanter(double beat, int genderType, int voiceType, bool playAnim)
        {
            switch (voiceType)
            {
                case 1:
                    if (genderType == 1) {var offset = SoundByte.GetClipLengthGame("rapMen/rapWoman/yeahW") - 0.391;
                    SoundByte.PlayOneShotGame("rapMen/rapWoman/yeahW", offset: offset, panning: -0.25f); }

                    else {var offset = SoundByte.GetClipLengthGame("rapMen/yeah") - 0.391;
                    SoundByte.PlayOneShotGame("rapMen/yeah", offset: offset, panning: -0.25f); }
                    if (playAnim)
                    {
                        RapperAnim("Red", "kamone");
                    }
                    break;

                case 2:
                    if (genderType == 1) { SoundByte.PlayOneShotGame("rapMen/rapWoman/yoW", panning: -0.25f); }
                    else { SoundByte.PlayOneShotGame("rapMen/yo", panning: -0.25f); }
                    if (playAnim)
                    {
                        RapperAnim("Red", "yo");
                    }
                    break;
            }
        }

        public void SubtitleType(double beat, int language)
        {
            subtitleLanguage = (CaptionTypes)language;
        }


        void RapJust(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("rapMen/drum", pitch: drumPitch, volume: drumVolume, panning: 0.25f);
            }
            else
            {
                SoundByte.PlayOneShotGame("rapMen/cymbal", pitch: cymbalPitch, volume: cymbalVolume, panning: 0.25f);
                SoundByte.PlayOneShotGame("rapMen/drum", pitch: drumPitch, volume: drumVolume - 0.3f, panning: 0.25f);

                foreach (var par in justParticles)
                {
                    ParticleSystem smokeParticle = Instantiate(par, transform);
                    smokeParticle.Play();
                }
            }

            if (yellowIsWoman) { SoundByte.PlayOneShotGame("rapMen/rapWomen/uhnW", panning: 0.25f); }
            else { SoundByte.PlayOneShotGame("rapMen/uhn", panning: 0.25f); }

            RapperAnim("Red", "just");
            RapperAnim("Player", "just");
            redCanBop = true;

            ParticleSystem uhn = Instantiate(uhnParticle, transform);
            uhn.Play();

            rapText.text = "";

        }

        void RapJustNoCymbal(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("rapMen/drum", pitch: drumPitch, volume: drumVolume, panning: 0.25f);
            }
            else
            {
                SoundByte.PlayOneShotGame("rapMen/drum", pitch: drumPitch, volume: drumVolume - 0.3f, panning: 0.25f);

                foreach (var par in justParticles)
                {
                    ParticleSystem smokeParticle = Instantiate(par, transform);
                    smokeParticle.Play();
                }
            }

            if (yellowIsWoman) { SoundByte.PlayOneShotGame("rapMen/rapWomen/uhnW", panning: 0.25f); }
            else { SoundByte.PlayOneShotGame("rapMen/uhn", panning: 0.25f); }

            RapperAnim("Red", "just");
            RapperAnim("Player", "just");
            redCanBop = true;

            ParticleSystem uhn = Instantiate(uhnParticle, transform);
            uhn.Play();

            rapText.text = "";

        }

        void RapJustAlt(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("rapMen/drum", pitch: drumPitch, volume: drumVolume, panning: 0.25f);
            }
            else
            {
                SoundByte.PlayOneShotGame("rapMen/cymbal", pitch: cymbalPitch, volume: cymbalVolume, panning: 0.25f);
                SoundByte.PlayOneShotGame("rapMen/drum", pitch: drumPitch, volume: drumVolume - 0.3f, panning: 0.25f);

                foreach (var par in justParticles)
                {
                    ParticleSystem smokeParticle = Instantiate(par, transform);
                    smokeParticle.Play();
                }
            }

            if (yellowIsWoman) { SoundByte.PlayOneShotGame("rapMen/rapWomen/uhnnnW", panning: 0.25f); }
            else { SoundByte.PlayOneShotGame("rapMen/uhnnn", panning: 0.25f); }

            RapperAnim("Red", "just");
            RapperAnim("Player", "just");
            redCanBop = true;

            ParticleSystem uhn = Instantiate(uhnParticle, transform);
            uhn.Play();

        }

        void RapMiss(PlayerActionEvent caller)
        {

            rapText.text = "";

            SoundByte.PlayOneShotGame("rapMen/miss", panning: 0.25f);

            RapperAnim("Player", "miss");
            redCanBop = true;


        }

        void Nothing(PlayerActionEvent caller) { }

        public void BackgroundColor(double beat, float length, int type, Color startA, Color endA, Color startB, Color endB, Color startC, Color endC, Color startD, Color endD, int ease)
        {
            colorStartBeat = beat;
            colorLength = length;
            colorStartA = startA;
            colorEndA = endA;
            colorStartB = startB;
            colorEndB = endB;
            colorStartC = startC;
            colorEndC = endC;
            colorStartD = startD;
            colorEndD = endD;
            colorEase = (Util.EasingFunction.Ease)ease;

            if (type == 1) { background.DoScaledAnimationAsync("backgroundMen", 0.5f); }
            else if (type == 2) { background.DoScaledAnimationAsync("backgroundWomen", 0.5f); }
        }

        private void BackgroundColorUpdate()
        {
            float normalizedBeat = Mathf.Clamp01(Conductor.instance.GetPositionFromBeat(colorStartBeat, colorLength));

            var func = Util.EasingFunction.GetEasingFunction(colorEase);

            float newRA = func(colorStartA.r, colorEndA.r, normalizedBeat); //Grid No-Star
            float newGA = func(colorStartA.g, colorEndA.g, normalizedBeat);
            float newBA = func(colorStartA.b, colorEndA.b, normalizedBeat);

            float newRB = func(colorStartB.r, colorEndB.r, normalizedBeat); //Grid Star & Speaker Rings
            float newGB = func(colorStartB.g, colorEndB.g, normalizedBeat);
            float newBB = func(colorStartB.b, colorEndB.b, normalizedBeat);

            float newRC = func(colorStartC.r, colorEndC.r, normalizedBeat); //Star
            float newGC = func(colorStartC.g, colorEndC.g, normalizedBeat);
            float newBC = func(colorStartC.b, colorEndC.b, normalizedBeat);

            float newRD = func(colorStartD.r, colorEndD.r, normalizedBeat); //Speaker
            float newGD = func(colorStartD.g, colorEndD.g, normalizedBeat);
            float newBD = func(colorStartD.b, colorEndD.b, normalizedBeat);

            backgroundMaterial.SetColor("_ColorAlpha", new Color(newRB, newGB, newBB)); //Red
            backgroundMaterial.SetColor("_ColorBravo", new Color(newRC, newGC, newBC)); //Green
            backgroundMaterial.SetColor("_ColorDelta", new Color(newRA, newGA, newBA)); //Blue

            speakerMaterial.SetColor("_ColorAlpha", new Color(newRB, newGB, newBB)); //Red
            //speakerMaterial.SetColor("_ColorBravo", new Color(newRT, newGT, newBT)); //Green
            speakerMaterial.SetColor("_ColorDelta", new Color(newRD, newGD, newBD)); //Blue
        }

        private void PersistColor(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("rapMen", new string[] { "background color" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                BackgroundColor(lastEvent.beat, lastEvent.length, lastEvent["bg"], lastEvent["colorFrom"], lastEvent["colorTo"], lastEvent["colorFrom2"], lastEvent["colorTo2"], lastEvent["colorFrom3"], lastEvent["colorTo3"], lastEvent["colorFrom4"], lastEvent["colorTo4"], lastEvent["ease"]);
            }

            allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("rapMen", new string[] { "womenToggle" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                RapperToggle(lastEvent.beat, lastEvent["redsGender"], lastEvent["yellowsGender"]);
            }
        }

        public void RapperToggle(double beat, int redT, int yellowT)
        {
            if (redT == 0) { redIsWoman = false; }
            else if (redT == 1) { redIsWoman = true; }
            if (yellowT == 0) { yellowIsWoman = false; }
            else if (yellowT == 1) { yellowIsWoman = true; }
            rapperRedObj.SetActive(redT == 0);
            rapperCherryObj.SetActive(redT == 1);
            rapperYellowObj.SetActive(yellowT == 0);
            rapperBlueObj.SetActive(yellowT == 1);
        }

        public void RapperAnim(String obj, String animName)
        {
            if (obj == "Red")
            {
                if (redIsWoman)
                {
                    rapperCherry.DoScaledAnimationAsync(animName, 0.5f);
                }
                else
                {
                    rapperRed.DoScaledAnimationAsync(animName, 0.5f);
                }
            }
            if (obj == "Player")
            {
                if (yellowIsWoman)
                {
                    rapperBlue.DoScaledAnimationAsync(animName, 0.5f);
                }
                else
                {
                    rapperYellow.DoScaledAnimationAsync(animName, 0.5f);
                }
            }
        }
    }
}