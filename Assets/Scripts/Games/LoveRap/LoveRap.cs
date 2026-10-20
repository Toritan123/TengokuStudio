using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;
using UnityEngine.Rendering.PostProcessing;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlRapLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("loveRap", "Love Rap", "5b595b", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out LoveRap instance)) {
                            instance.Bop(e.beat, e.length, e["bop"], e["auto"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop", "Toggle if the Rappers should bop for the duration of this event."),
                        new Param("auto", false, "Bop (Auto)", "Toggle if the Rappers should automatically bop until another Bop event is reached.")
                    },
                    resizable = true,
                    defaultLength = 1f,
                },
                new GameAction("intoYou", "Into you!")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out LoveRap instance)) {
                            instance.IntoYou(e.beat, e["heart"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("heart", false, "Heart", "Toggle if the cue's text should show a heart."),
                    },
                    preFunctionLength = 1f,
                    defaultLength = 2f,
                },
                new GameAction("crazyIntoYou", "Crazy into you!")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out LoveRap instance)) {
                            instance.CrazyIntoYou(e.beat);
                        }
                    },
                    preFunctionLength = 1f,
                    defaultLength = 3f,
                },
                new GameAction("foSho", "Fo' sho'!")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out LoveRap instance)) {
                            instance.FoSho(e.beat);
                        }
                    },
                    preFunctionLength = 1f,
                    defaultLength = 1.5f,
                },
                new GameAction("allAboutYou", "All about you!")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out LoveRap instance)) {
                            instance.AllAboutYou(e.beat);
                        }
                    },
                    preFunctionLength = 1f,
                    defaultLength = 2.5f,
                },
                new GameAction("setBG", "Background Appearance")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out LoveRap instance)) {
                            var e = eventCaller.currentEntity;
                            instance.SetBG(e.beat, e.length, e["colorStart"], e["colorEnd"], e["wetnessStart"], e["wetnessEnd"], e["ease"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("colorStart", Color.black, "Start BG Color", "Set the color at the start of the event."),
                        new Param("colorEnd", Color.black, "End BG Color", "Set the color at the end of the event."),
                        new Param("wetnessStart", new EntityTypes.Float(0f, 1f, 0f), "Start BG Wetness", "Set the wetness at the start of the event."),
                        new Param("wetnessEnd", new EntityTypes.Float(0f, 1f, 0f), "End BG Wetness", "Set the wetness at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Instant, "Ease", "Set the easing of the action.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, y) => (int)x != (int)Util.EasingFunction.Ease.Instant, new string[] { "colorStart", "wetnessStart" })
                        }),
                    },
                    defaultLength = 1f,
                    resizable = true,
                },
            },
            new List<string>() { "rvl", "normal" },
            "rvlrap", "en",
            new List<string>() { "en", "jp" },
            chronologicalSortKey: 29,
            font: "FOT-SlumpStd-DB SDF"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    public class LoveRap : Minigame
    {
        public static LoveRap instance;
        public bool canBop = true;
        public bool canBopOthers = true;
        public bool heart = false;
        private string lastAnim = "";
        private double lastBeat = double.MinValue;

        [Header("Player")]
        [SerializeField] Animator playerRapper;
        [SerializeField] Animator playerBody;
        [SerializeField] Animator playerLegs;
        [SerializeField] Animator playerFace;
        [SerializeField] Animator playerMouth;
        [SerializeField] Animator playerFlash;
        [SerializeField] Animator playerBubble;
        public TMP_Text playerText;

        [Header("CPU")]
        [SerializeField] Animator cpuRapper;
        [SerializeField] Animator cpuBody;
        [SerializeField] Animator cpuLegs;
        [SerializeField] Animator cpuFace;
        [SerializeField] Animator cpuMouth;
        [SerializeField] Animator cpuFlash;
        [SerializeField] Animator cpuBubble;
        [SerializeField] Transform cpuHeadTrans;
        public TMP_Text cpuText;

        [Header("MC Adore")]
        [SerializeField] Animator mcMouth;
        [SerializeField] Animator mcBody;
        [SerializeField] Animator mcLegs;
        [SerializeField] Animator car;
        [SerializeField] Animator car_lights;
        [SerializeField] Animator mcBubble;
        public TMP_Text mcText;

        [Header("BG")]
        [SerializeField] SpriteRenderer bgSR;
        [SerializeField] SpriteRenderer bgWetSR;

        /* input types:
         * 1 = into you
         * 2 = crazy into you
         * 3 = crazy into you (offbeat)
         * 4 = fo sho
         * 5 = fo sho (offbeat)
         * 6 = all about you
        */
        private int inputType;
        float voicePitch = 1f;

        void Awake()
        {
            instance = this;
            SetupBopRegion("loveRap", "bop", "auto");
            BackgroundColorUpdate();
        }

        public void Update()
        {
            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            {
                playerBody.DoScaledAnimationAsync("cough", 0.5f);
                playerMouth.DoScaledAnimationAsync("Cough", 0.5f);
                playerFace.DoScaledAnimationAsync("E", 0.5f);
                cpuFace.DoScaledAnimationAsync(cpuHeadTrans.localScale.x > 0 ? "F" : "G", 0.5f);
                cpuMouth.DoScaledAnimationAsync("Miss", 0.5f);
                SoundByte.PlayOneShot("miss");
            }
            BackgroundColorUpdate();
        }

        public override void OnPlay(double beat)
        {
            PersistColor(beat);
        }

        public override void OnGameSwitch(double beat)
        {
            PersistColor(beat);
            DoInactiveCues(beat);
        }

        private void DoInactiveCues(double beat)
        {
            RiqEntity cueEntity = EventCaller.GetAllInGameManagerList("loveRap", new string[] 
            { "intoYou", "crazyIntoYou", "foSho", "allAboutYou" }).FindLast(e => e.beat - 1 <= beat && e.beat + e.length >= beat);

            if (cueEntity != null)
            {
                RiqEntity e = cueEntity;
                switch (e.datamodel)
                {
                    case "loveRap/intoYou":
                        IntoYou(e.beat, e["heart"]);
                        break;
                    case "loveRap/crazyIntoYou":
                        CrazyIntoYou(e.beat);
                        break;
                    case "loveRap/foSho":
                        FoSho(e.beat);
                        break;
                    case "loveRap/allAboutYou":
                        AllAboutYou(e.beat);
                        break;
                }
            }
        }


        private ColorEase bgColorEase = new(Color.black);
        private ColorEase bgWetColorEase = new(new Color(1f, 1f, 1f, 0f));

        public void SetBG(double beat, float length, Color colorStart, Color colorEnd, float wetnessStart, float wetnessEnd,int ease)
        {
            bgColorEase = new ColorEase(beat, length, colorStart, colorEnd, ease);
            bgWetColorEase = new ColorEase(beat, length, new Color(1f, 1f, 1f, wetnessStart), new Color(1f, 1f, 1f, wetnessEnd), ease);
        }

        public void BackgroundColorUpdate()
        {
            bgSR.color = bgColorEase.GetColor();
            bgWetSR.color = bgWetColorEase.GetColor();
        }

        //call this in OnPlay(double beat) and OnGameSwitch(double beat)
        private void PersistColor(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("loveRap", new string[] { "setBG" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var e = allEventsBeforeBeat[^1];
                SetBG(e.beat, e.length, e["colorStart"], e["colorEnd"], e["wetnessStart"], e["wetnessEnd"], e["ease"]);
            }
        }

        public void IntoYou(double beat, bool heartCue)
        {
            PrepareAnim(beat, 1);
            cpuSFX(beat + 1, 1);

            
            string text = heartCue ? "Into you <color=#f20d7a>♥" : "Into you!";

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("loveRap/Woman/SE_RAP_EN_WOMAN_DAISUKI_1", beat),
                new MultiSound.Sound("loveRap/Woman/SE_RAP_EN_WOMAN_DAISUKI_2", beat + 0.5),
                new MultiSound.Sound("loveRap/Woman/SE_RAP_EN_WOMAN_DAISUKI_3", beat + 0.75, offset: 0.08),
            }, forcePlay: true);

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 0.25, delegate { 
                    inputType = 1;
                }),
                new BeatAction.Action(beat, delegate { 
                    PrepText(text); 
                    mcBubble.Play("Appear");
                    heart = heartCue;

                    mcBody.DoScaledAnimationAsync("D", 0.5f);
                    mcMouth.DoScaledAnimationAsync("D", 0.5f);
                    playerFace.DoScaledAnimationAsync("A", 0.5f);
                    playerMouth.DoScaledAnimationAsync("idle", 0.5f);
                    cpuMouth.DoScaledAnimationAsync("idle", 0.5f);
                    cpuFace.DoScaledAnimationAsync("A", 0.5f);
                }),
                new BeatAction.Action(beat + 1, delegate { 
                    mcBubble.Play("Hide");
                    cpuBubble.Play("Appear");
                    cpuBody.DoScaledAnimationAsync("D", 0.5f);
                    cpuMouth.DoScaledAnimationAsync("D", 0.5f);
                }),
                new BeatAction.Action(beat + 2, delegate { 
                    cpuBubble.Play("Hide");
                }),
            });

            ScheduleInput(beat, 1, InputAction_BasicPress, Just, Miss, null);
        }

        public void CrazyIntoYou(double beat)
        {
            PrepareAnim(beat, 2);

            int id = beat % 0.5 == 0.25 ? 3 : 2; // offbeat check


            if (id == 3)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("loveRap/Woman/SE_RAP_EN_WOMAN_MAJI_OFFBEAT_1", beat),
                    new MultiSound.Sound("loveRap/Woman/SE_RAP_EN_WOMAN_MAJI_OFFBEAT_2", beat + 0.25),
                    new MultiSound.Sound("loveRap/Woman/SE_RAP_EN_WOMAN_MAJI_OFFBEAT_3", beat + 0.5),
                    new MultiSound.Sound("loveRap/Woman/SE_RAP_EN_WOMAN_MAJI_OFFBEAT_4", beat + 0.75, offset: 0.015f),
                    new MultiSound.Sound("loveRap/Woman/SE_RAP_EN_WOMAN_MAJI_OFFBEAT_5", beat + 1.25, offset: 0.08f),
                }, forcePlay: true);
                cpuSFX(beat + 1.5, 3);
            }
            else
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("loveRap/Woman/SE_RAP_EN_WOMAN_MAJI_1", beat),
                    new MultiSound.Sound("loveRap/Woman/SE_RAP_EN_WOMAN_MAJI_2", beat + 0.25),
                    new MultiSound.Sound("loveRap/Woman/SE_RAP_EN_WOMAN_MAJI_3", beat + 0.5),
                    new MultiSound.Sound("loveRap/Woman/SE_RAP_EN_WOMAN_MAJI_4", beat + 1),
                    new MultiSound.Sound("loveRap/Woman/SE_RAP_EN_WOMAN_MAJI_5", beat + 1.25, offset: 0.072f),
                }, forcePlay: true);
                cpuSFX(beat + 1.5, 2);
            }

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 0.25, delegate { 
                    inputType = id;
                }),
                new BeatAction.Action(beat, delegate { 
                    PrepText("Crazy into you!"); 
                    mcBubble.Play("Appear");
                    mcBody.DoScaledAnimationAsync("MD", 0.5f);
                    mcMouth.DoScaledAnimationAsync(id == 3 ? "MD_ura" : "MD", 0.5f);
                    playerFace.DoScaledAnimationAsync("A", 0.5f);
                    playerMouth.DoScaledAnimationAsync("idle", 0.5f);
                    cpuMouth.DoScaledAnimationAsync("idle", 0.5f);
                    cpuFace.DoScaledAnimationAsync("A", 0.5f);
                }),
                new BeatAction.Action(beat + 1.5, delegate { 
                    cpuBubble.Play("Appear");
                    cpuBody.DoScaledAnimationAsync("MD", 0.5f);
                    cpuMouth.DoScaledAnimationAsync(id == 3 ? "MD_ura" : "MD", 0.5f);
                }),
                new BeatAction.Action(beat + 2, delegate { 
                    mcBubble.Play("Hide");
                }),
                new BeatAction.Action(beat + 2.5, delegate { 
                    cpuBubble.Play("Hide");
                }),
            });

            ScheduleInput(beat, 1.5, InputAction_BasicPress, Just, Miss, null);
        }

        public void FoSho(double beat)
        {
            PrepareAnim(beat, 3);

            int id = beat % 0.5 == 0.25 ? 5 : 4; // offbeat check

            if (id == 5)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("loveRap/Woman/SE_RAP_EN_WOMAN_HONTO_OFFBEAT_1", beat, offset: 0.021f),
                    new MultiSound.Sound("loveRap/Woman/SE_RAP_EN_WOMAN_HONTO_OFFBEAT_2", beat + 0.125),
                    new MultiSound.Sound("loveRap/Woman/SE_RAP_EN_WOMAN_HONTO_OFFBEAT_3", beat + 0.5, offset: 0.11f),
                }, forcePlay: true);
                cpuSFX(beat + 0.75, 5);
            }
            else
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("loveRap/Woman/SE_RAP_EN_WOMAN_HONTO_1", beat, offset: 0.017f),
                    new MultiSound.Sound("loveRap/Woman/SE_RAP_EN_WOMAN_HONTO_2", beat + 0.5, offset: 0.117f),
                }, forcePlay: true);
                cpuSFX(beat + 0.75, 4);
            }

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 0.25, delegate { 
                    inputType = id;

                }),
                new BeatAction.Action(beat, delegate { 
                    PrepText("Fo' sho'!");
                    mcBubble.Play("Appear");
                    mcBody.DoScaledAnimationAsync("H", 0.5f);
                    mcMouth.DoScaledAnimationAsync(id == 5 ? "H_ura" : "H", 0.5f);
                    
                    playerMouth.DoScaledAnimationAsync("HB", 0.5f);
                    cpuMouth.DoScaledAnimationAsync("HB", 0.5f);
                    playerFace.DoScaledAnimationAsync("B", 0.5f);
                    cpuFace.DoScaledAnimationAsync("B", 0.5f);
                }),
                new BeatAction.Action(beat + 0.75, delegate { 
                    cpuBubble.Play("Appear");
                    cpuBody.DoScaledAnimationAsync("H", 0.5f);
                    cpuMouth.DoScaledAnimationAsync(id == 5 ? "H_ura" : "H", 0.5f);
                }),
                new BeatAction.Action(beat + 1, delegate { 
                    mcBubble.Play("Hide");
                }),
                new BeatAction.Action(beat + 1.75, delegate { 
                    cpuBubble.Play("Hide");
                }),
            });

            ScheduleInput(beat, 0.75, InputAction_BasicPress, Just, Miss, null);
        }

        public void AllAboutYou(double beat)
        {
            PrepareAnim(beat, 4);
            cpuSFX(beat + 1.25, 6);

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("loveRap/Woman/SE_RAP_EN_WOMAN_SUKINANDA_OFFBEAT_1", beat),
                new MultiSound.Sound("loveRap/Woman/SE_RAP_EN_WOMAN_SUKINANDA_OFFBEAT_2", beat + 0.25),
                new MultiSound.Sound("loveRap/Woman/SE_RAP_EN_WOMAN_SUKINANDA_OFFBEAT_3", beat + 0.5),
                new MultiSound.Sound("loveRap/Woman/SE_RAP_EN_WOMAN_SUKINANDA_OFFBEAT_4", beat + 1, offset: 0.058f),
            }, forcePlay: true);

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 0.25, delegate { 
                    inputType = 6;
                    }),
                new BeatAction.Action(beat, delegate { 
                    PrepText("All about you!"); 
                    mcBubble.Play("Appear");
                    mcBody.DoScaledAnimationAsync("S", 0.5f);
                    mcMouth.DoScaledAnimationAsync("S", 0.5f);
                    playerMouth.DoScaledAnimationAsync("SB", 0.5f);
                    cpuMouth.DoScaledAnimationAsync("SB", 0.5f);
                    playerFace.DoScaledAnimationAsync("C", 0.5f);
                    cpuFace.DoScaledAnimationAsync("C", 0.5f);
                }),
                new BeatAction.Action(beat + 1, delegate { 
                    mcBubble.Play("Hide");
                }),
                new BeatAction.Action(beat + 1.25, delegate { 
                    cpuBubble.Play("Appear");
                    cpuBody.DoScaledAnimationAsync("S", 0.5f);
                    cpuMouth.DoScaledAnimationAsync("S", 0.5f);
                }),
                new BeatAction.Action(beat + 2.25, delegate { 
                    cpuBubble.Play("Hide");
                }),
            });

            ScheduleInput(beat, 1.25, InputAction_BasicPress, Just, Miss, null);
        }

        public void PrepareAnim(double beat, int id)
        {
            string anim = null;
            double length = double.MinValue;
            bool nextPrepare = true;

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 1, delegate { 
                    switch (id)
                    {
                        case 1:
                            anim = "D";
                            length = 2;
                            break;
                        case 2:
                            anim = "MD";
                            length = 3;
                            break;
                        case 3:
                            anim = "H";
                            length = 1.5;
                            break;
                        case 4:
                            anim = "S";
                            length = 2.5;
                            break;
                    }

                    nextPrepare = anim != lastAnim || lastBeat + (length + 0.5) <= beat;
                    if (nextPrepare) 
                    {
                        mcBody.DoScaledAnimationAsync(anim + "B", 0.5f);
                    }
                }),
                new BeatAction.Action(beat, delegate { 
                    if (nextPrepare) 
                    {
                        playerBody.DoScaledAnimationAsync(anim + "B", 0.5f);
                        cpuBody.DoScaledAnimationAsync(anim + "B", 0.5f);
                    }
                    lastAnim = anim;
                    lastBeat = beat;
                })
            });
        }

        public void PrepText(string text)
        {
            mcText.text = text;
            
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(Conductor.instance.songPositionInBeatsAsDouble + 0.5, delegate { 
                    cpuText.text = text;
                    playerText.text = text;
                }),
            });
        }

        

        public void cpuSFX(double beat, int id)
        {
            switch (id)
            {
                case 1:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("loveRap/Left/SE_RAP_EN_MAN_LEFT_DAISUKI_1", beat),
                        new MultiSound.Sound("loveRap/Left/SE_RAP_EN_MAN_LEFT_DAISUKI_2", beat + 0.5),
                        new MultiSound.Sound("loveRap/Left/SE_RAP_EN_MAN_LEFT_DAISUKI_3", beat + 0.75, offset: 0.034f),
                    }, forcePlay: true);
                    break;
                case 2:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("loveRap/Left/SE_RAP_EN_MAN_LEFT_MAJI_1", beat),
                        new MultiSound.Sound("loveRap/Left/SE_RAP_EN_MAN_LEFT_MAJI_2", beat + 0.25),
                        new MultiSound.Sound("loveRap/Left/SE_RAP_EN_MAN_LEFT_MAJI_3", beat + 0.5),
                        new MultiSound.Sound("loveRap/Left/SE_RAP_EN_MAN_LEFT_MAJI_4", beat + 1),
                        new MultiSound.Sound("loveRap/Left/SE_RAP_EN_MAN_LEFT_MAJI_5", beat + 1.25),
                    }, forcePlay: true);
                    break;
                case 3:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("loveRap/Left/SE_RAP_EN_MAN_LEFT_MAJI_OFFBEAT_1", beat),
                        new MultiSound.Sound("loveRap/Left/SE_RAP_EN_MAN_LEFT_MAJI_OFFBEAT_2", beat + 0.25),
                        new MultiSound.Sound("loveRap/Left/SE_RAP_EN_MAN_LEFT_MAJI_OFFBEAT_3", beat + 0.5),
                        new MultiSound.Sound("loveRap/Left/SE_RAP_EN_MAN_LEFT_MAJI_OFFBEAT_4", beat + 0.75),
                        new MultiSound.Sound("loveRap/Left/SE_RAP_EN_MAN_LEFT_MAJI_OFFBEAT_5", beat + 1.25),
                    }, forcePlay: true);
                    break;
                case 4:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("loveRap/Left/SE_RAP_EN_MAN_LEFT_HONTO_OFFBEAT_1", beat),
                        new MultiSound.Sound("loveRap/Left/SE_RAP_EN_MAN_LEFT_HONTO_OFFBEAT_2", beat + 0.125),
                        new MultiSound.Sound("loveRap/Left/SE_RAP_EN_MAN_LEFT_HONTO_OFFBEAT_3", beat + 0.5, offset: 0.098f),
                    }, forcePlay: true);
                    break;
                case 5:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("loveRap/Left/SE_RAP_EN_MAN_LEFT_HONTO_1", beat, offset: 0.028f),
                        new MultiSound.Sound("loveRap/Left/SE_RAP_EN_MAN_LEFT_HONTO_2", beat + 0.5, offset: 0.113f),
                    }, forcePlay: true);
                    break;
                case 6:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("loveRap/Left/SE_RAP_EN_MAN_LEFT_SUKINANDA_1", beat),
                        new MultiSound.Sound("loveRap/Left/SE_RAP_EN_MAN_LEFT_SUKINANDA_2", beat + 0.25),
                        new MultiSound.Sound("loveRap/Left/SE_RAP_EN_MAN_LEFT_SUKINANDA_3", beat + 0.5),
                        new MultiSound.Sound("loveRap/Left/SE_RAP_EN_MAN_LEFT_SUKINANDA_4", beat + 1),
                    }, forcePlay: true);
                    break;
            }
        }

        public void Just(PlayerActionEvent caller, float state)
        {
            int id = inputType;
            var beat = caller.startBeat + caller.timer;
            if (state >= 1f || state <= -1f)
            {
                if (state >= 1f)
                {
                    voicePitch = 0.8f;
                }
                else if (state <= -1f)
                {
                    voicePitch = 1.2f;
                }

                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { 
                        playerBubble.Play("AppearMiss");
                    }),
                    new BeatAction.Action(beat + 1, delegate { 
                        playerBubble.Play("HideMiss");
                    }),
                }); 
            }
            else
            {
                voicePitch = 1f;

                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { 
                        playerBubble.Play("Appear");
                        playerFlash.DoScaledAnimationAsync("FadeOut", 0.5f);
                        cpuFlash.DoScaledAnimationAsync("FadeOut", 0.5f);
                        car_lights.DoScaledAnimationAsync("light", 0.5f);
                    }),
                    new BeatAction.Action(beat + 1, delegate { 
                        playerBubble.Play("Hide");
                    }),
                });   
            }

            switch (id)
            {
                case 1:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("loveRap/Right/SE_RAP_EN_MAN_RIGHT_DAISUKI_1", beat, voicePitch),
                        new MultiSound.Sound("loveRap/Right/SE_RAP_EN_MAN_RIGHT_DAISUKI_2", beat + 0.5, voicePitch),
                        new MultiSound.Sound("loveRap/Right/SE_RAP_EN_MAN_RIGHT_DAISUKI_3", beat + 0.75, voicePitch, offset: 0.034f),
                    }, forcePlay: true);

                    playerBody.DoScaledAnimationAsync("D", 0.5f);
                    playerMouth.DoScaledAnimationAsync("D", 0.5f);
                    playerFace.DoScaledAnimationAsync(heart ? "D" : "A", 0.5f);
                    cpuFace.DoScaledAnimationAsync(heart ? "D" : "A", 0.5f);
                    break;
                case 2:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("loveRap/Right/SE_RAP_EN_MAN_RIGHT_MAJI_1", beat, voicePitch),
                        new MultiSound.Sound("loveRap/Right/SE_RAP_EN_MAN_RIGHT_MAJI_2", beat + 0.25, voicePitch),
                        new MultiSound.Sound("loveRap/Right/SE_RAP_EN_MAN_RIGHT_MAJI_3", beat + 0.5, voicePitch),
                        new MultiSound.Sound("loveRap/Right/SE_RAP_EN_MAN_RIGHT_MAJI_4", beat + 1, voicePitch),
                        new MultiSound.Sound("loveRap/Right/SE_RAP_EN_MAN_RIGHT_MAJI_5", beat + 1.25, voicePitch),
                    }, forcePlay: true);

                    playerBody.DoScaledAnimationAsync("MD", 0.5f);
                    playerMouth.DoScaledAnimationAsync("MD", 0.5f);
                    playerFace.DoScaledAnimationAsync("A", 0.5f);
                    cpuFace.DoScaledAnimationAsync("A", 0.5f);
                    break;
                case 3:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("loveRap/Right/SE_RAP_EN_MAN_RIGHT_MAJI_OFFBEAT_1", beat, voicePitch),
                        new MultiSound.Sound("loveRap/Right/SE_RAP_EN_MAN_RIGHT_MAJI_OFFBEAT_2", beat + 0.25, voicePitch),
                        new MultiSound.Sound("loveRap/Right/SE_RAP_EN_MAN_RIGHT_MAJI_OFFBEAT_3", beat + 0.5, voicePitch),
                        new MultiSound.Sound("loveRap/Right/SE_RAP_EN_MAN_RIGHT_MAJI_OFFBEAT_4", beat + 0.75, voicePitch),
                        new MultiSound.Sound("loveRap/Right/SE_RAP_EN_MAN_RIGHT_MAJI_OFFBEAT_5", beat + 1.25, voicePitch),
                    }, forcePlay: true);

                    playerBody.DoScaledAnimationAsync("MD", 0.5f);
                    playerMouth.DoScaledAnimationAsync("MD_ura", 0.5f);
                    playerFace.DoScaledAnimationAsync("A", 0.5f);
                    cpuFace.DoScaledAnimationAsync("A", 0.5f);

                    break;
                case 4:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("loveRap/Right/SE_RAP_EN_MAN_RIGHT_HONTO_OFFBEAT_1", beat, voicePitch),
                        new MultiSound.Sound("loveRap/Right/SE_RAP_EN_MAN_RIGHT_HONTO_OFFBEAT_2", beat + 0.125, voicePitch),
                        new MultiSound.Sound("loveRap/Right/SE_RAP_EN_MAN_RIGHT_HONTO_OFFBEAT_3", beat + 0.5, voicePitch, offset: 0.071f),
                    }, forcePlay: true);

                    playerBody.DoScaledAnimationAsync("H", 0.5f);
                    playerMouth.DoScaledAnimationAsync("H_ura", 0.5f);
                    playerFace.DoScaledAnimationAsync("B", 0.5f);
                    cpuFace.DoScaledAnimationAsync("B", 0.5f);

                    break;
                case 5:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("loveRap/Right/SE_RAP_EN_MAN_RIGHT_HONTO_1", beat, voicePitch, offset: 0.028f),
                        new MultiSound.Sound("loveRap/Right/SE_RAP_EN_MAN_RIGHT_HONTO_2", beat + 0.5, voicePitch, offset: 0.069f),
                    }, forcePlay: true);

                    playerBody.DoScaledAnimationAsync("H", 0.5f);
                    playerMouth.DoScaledAnimationAsync("H", 0.5f);
                    playerFace.DoScaledAnimationAsync("B", 0.5f);
                    cpuFace.DoScaledAnimationAsync("B", 0.5f);

                    break;
                case 6:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("loveRap/Right/SE_RAP_EN_MAN_RIGHT_SUKINANDA_1", beat, voicePitch),
                        new MultiSound.Sound("loveRap/Right/SE_RAP_EN_MAN_RIGHT_SUKINANDA_2", beat + 0.25, voicePitch),
                        new MultiSound.Sound("loveRap/Right/SE_RAP_EN_MAN_RIGHT_SUKINANDA_3", beat + 0.5, voicePitch),
                        new MultiSound.Sound("loveRap/Right/SE_RAP_EN_MAN_RIGHT_SUKINANDA_4", beat + 1, voicePitch),
                    }, forcePlay: true);

                    playerBody.DoScaledAnimationAsync("S", 0.5f);
                    playerMouth.DoScaledAnimationAsync("S", 0.5f);
                    playerFace.DoScaledAnimationAsync("C", 0.5f);
                    cpuFace.DoScaledAnimationAsync("C", 0.5f);

                    break;
            }
        }

        public void Miss(PlayerActionEvent caller)
        {
            
        }

        public override void OnBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat))
            {
                if (canBop)
                {
                    playerRapper.DoScaledAnimationAsync("Beat", 0.5f);
                }
                if (canBopOthers)
                {
                    cpuRapper.DoScaledAnimationAsync("Beat", 0.5f);
                    mcLegs.DoScaledAnimationAsync("Beat", 0.5f);
                    car.DoScaledAnimationAsync("beat", 0.5f);
                }
            }
        }
        public void Bop(double beat, float length, bool bop, bool auto)
        {
            if (bop)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate {
                            playerRapper.DoScaledAnimationAsync("Beat", 0.5f);
                            cpuRapper.DoScaledAnimationAsync("Beat", 0.5f);
                            mcLegs.DoScaledAnimationAsync("Beat", 0.5f);
                            car.DoScaledAnimationAsync("beat", 0.5f);
                        })
                    });
                }
            }
        }
    }
}