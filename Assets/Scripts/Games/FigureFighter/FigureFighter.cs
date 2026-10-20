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
    public static class RvlFigureLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("figureFighter", "Figure Fighter", "5DE1FB", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        FigureFighter.instance.Bop(e.beat, e.length, e["auto"], e["bop"], e["crowd"], e["crowdAuto"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Doll Bop", "Toggle if the doll should bop for the duration of this event."),
                        new Param("auto", false, "Doll Bop (Auto)", "Toggle if the doll should automatically bop until another Bop event is reached."),
                        new Param("crowd", true, "Crowd Bop", "Toggle if the crowd should bop for the duration of this event."),
                        new Param("crowdAuto", false, "Crowd Bop (Auto)", "Toggle if the crowd should automatically bop until another Bop event is reached.")
                    },
                    resizable = true,
                    defaultLength = 1f
                },
                new GameAction("crowdEngagement", "Crowd Engagement")
                {
                    function = delegate { var e = eventCaller.currentEntity; FigureFighter.instance.Engagement(e.beat, e["engagement"], e["chant"]);},
                    parameters = new List<Param>()
                    {
                        new Param("engagement", FigureFighter.CrowdEngagement.Neutral, "Set Engagement", "Set the engagement level of the crowd.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x == (int)FigureFighter.CrowdEngagement.FullyRiled, new string[] { "chant" })
                        }),
                        new Param("chant", true, "Chant?", "Set if the crowd chants along with successful inputs."),

                    },
                    defaultLength = 0.5f,
                    preFunctionLength = 0.5f,
                    priority = 1
                },
                new GameAction("bars", "Toggle Black Bars")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        FigureFighter.instance.Bars(e.beat, e["bars"], e["sticky"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("bars", true, "Enable Black Bars", "Toggles if black bars are enabled."),
                        new Param("sticky", false, "Follow Camera", "Toggles if black bars will move with the camera."),
                    },
                    defaultLength = 0.5f
                },
                new GameAction("jab", "Jab")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        FigureFighter.instance.Jab(e.beat, e["strong"]);
                    },
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity;
                        FigureFighter.instance.JabAnd(e.beat, e["and"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("and", false, "And", "Toggle if the announcer should say \"and\"."),
                        new Param("strong", false, "Break Bag", "Toggle if this input should break the punching bag."),
                    },
                    defaultLength = 2f
                },
                new GameAction("oneTwo", "One Two")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        FigureFighter.instance.OneTwo(e.beat, e["strong"]);
                    },
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity;
                        FigureFighter.instance.JabAnd(e.beat, e["and"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("and", false, "And", "Toggle if the announcer should say \"and\"."),
                        new Param("strong", false, "Break Bag", "Toggle if this input should break the punching bag."),
                    },
                    defaultLength = 4f
                },
                new GameAction("oneTwoFast", "Fast One Two")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        FigureFighter.instance.OneTwoFast(e.beat, e["strong"]);
                    },
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity;
                        FigureFighter.instance.OneTwoAnd(e.beat, e["and"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("and", false, "And", "Toggle if the announcer should say \"and\"."),
                        new Param("strong", false, "Break Bag", "Toggle if this input should break the punching bag."),
                    },
                    defaultLength = 2f
                },
                new GameAction("goGoGo", "Go Go Go!")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        FigureFighter.instance.GoGoGo(e.beat, e["strong"]);
                    },
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity;
                        FigureFighter.instance.OneTwoAnd(e.beat, e["and"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("and", false, "And", "Toggle if the announcer should say \"and\"."),
                        new Param("strong", false, "Break Bag", "Toggle if this input should break the punching bag."),
                    },
                    defaultLength = 4f
                },
            },
            new List<string>() { "rvl", "normal" },
            "rvlmuscle", "en",
            new List<string>() { "en", "jp" },
            chronologicalSortKey: 14,
            font: "Comet"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    public class FigureFighter : Minigame
    {
        public enum CrowdEngagement
        {
            Neutral = 0,
            HalfRiled = 1,
            FullyRiled = 2
        }

        [SerializeField] Animator dollAnim;
        [SerializeField] Animator crowdAnim;
        [SerializeField] Animator bagAnim;
        [SerializeField] Animator bagCopyAnim;
        [SerializeField] Animator buttonAnim;
        [SerializeField] Animator lightsAnim;
        [SerializeField] Animator topAnim;
        [SerializeField] Animator barsAnim;
        [SerializeField] Animator fartAnim;
        [SerializeField] StickyCanvas StickyLayer;
        [SerializeField] ParticleSystem chainParticles1;
        [SerializeField] ParticleSystem chainParticles2;
        public GameObject bagObject;

        public static FigureFighter instance;
        private bool canBop = true;
        private bool isPreparing = false;
        private bool dollBop = true;
        private bool crowdBop = true;
        private bool toggleBars = true;
        private bool crowdChant = true;
        private int crowdEngagement;
        private Renderer[] barRenderers;
        public bool strongPunch = false;
        private List<RiqEntity> engagementBlocks;
        private string[] priorityAnims = { "FigureJab1", "FigureJab2", "FigureFinisher", "FigureWhiff1", "FigureWhiff2", "FigureDeflate", "FigureDeflateBarely" };

        protected static bool IA_PadShoulders(out double dt)
        {
            dt = 0;
            return PlayerInput.GetPad(InputController.ActionsPad.ButtonL)
                    && PlayerInput.GetPad(InputController.ActionsPad.ButtonR);
        }

        protected static bool IA_TouchShoulders(out double dt)
        {
            dt = 0;
            return PlayerInput.GetTouch(InputController.ActionsTouch.ButtonL)
                    && PlayerInput.GetTouch(InputController.ActionsTouch.ButtonR);
        }

        public static PlayerInput.InputAction InputAction_Shoulders =
            new("ShouldersPress", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_PadShoulders, IA_TouchShoulders, null);

        public override void OnBeatPulse(double beat)
        {
            foreach (var e in engagementBlocks)
            {
                if (e.beat == beat)
                {
                    Engagement(e.beat, e["engagement"], e["chant"]);
                }
            }
            
            if (BeatIsInBopRegion(beat))
            {
                if (!dollAnim.IsPlayingAnimationNames(priorityAnims) && dollBop && canBop)
                {
                    dollAnim.DoScaledAnimationAsync("FigureBop", 0.5f, animLayer : 0);
                }
                if (crowdBop)
                {
                    crowdAnim.DoScaledAnimationAsync($"CrowdBop{crowdEngagement}", 0.5f, animLayer : 0);
                }
            }
        }

        void Awake()
        {
            engagementBlocks = EventCaller.GetAllInGameManagerList("figureFighter", new string[] { "crowdEngagement" });

            instance = this;
            SetupBopRegion("figureFighter", "bop", "auto");
            barRenderers = barsAnim.GetComponentsInChildren<Renderer>();
            HideBars();
        }

        public void Bop(double beat, float length, bool doll, bool auto, bool crowd, bool autocrowd)
        {
            dollBop = auto;
            crowdBop = autocrowd;
            if (doll)
            {
                var actions = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                {
                    actions.Add(new BeatAction.Action(beat + i, delegate
                    {
                        if (!dollAnim.IsPlayingAnimationNames(priorityAnims) && canBop)
                        {
                            dollAnim.DoScaledAnimationAsync("FigureBop", 0.5f);
                        }
                    }));
                }
                BeatAction.New(instance, actions);
            }
            if (crowd)
            {
                var actions = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                {
                    actions.Add(new BeatAction.Action(beat + i, delegate
                    {
                        //    if (!crowdAnim.IsPlayingAnimationNames(" "))
                        //    {
                        crowdAnim.DoScaledAnimationAsync($"CrowdBop{crowdEngagement}", 0.5f);
                        //    }
                    }));
                }
                BeatAction.New(instance, actions);
            }
        }


        public void Engagement(double beat, int engagement, bool chant)
        {
            crowdEngagement = engagement;
            crowdChant = chant;
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            {
                dollAnim.DoScaledAnimationAsync("FigureDeflate", 0.5f);
                buttonAnim.DoScaledAnimationAsync("ButtonPress", 0.5f);
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("figureFighter/whiffFart", cond.songPositionInBeats, UnityEngine.Random.Range(0.9f, 1.1f)),
                    new MultiSound.Sound("figureFighter/whiffPress", cond.songPositionInBeats),
                    new MultiSound.Sound("figureFighter/pump", cond.songPositionInBeats, volume: 0.4f),
                }, forcePlay: true);
                Fart();
            }
        }

        public override void OnGameSwitch(double beat)
        {
            PersistEngagement(beat);
        }

        private void PersistEngagement(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("figureFighter", new string[] { "crowdEngagement" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat));
                var lastEvent = allEventsBeforeBeat[^1];
                Engagement(beat, lastEvent["engagement"], lastEvent["chant"]);
            }
        }


        public void Jab(double beat, bool strong)
        {
            canBop = false;
            isPreparing = true;
            if (strong)
            {
                strongPunch = true;
            }
            else strongPunch = false;

            //dollAnim.DoScaledAnimationAsync("FigurePrep1", 0.5f);
            MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("figureFighter/jab", beat + 0.0),
                }, forcePlay: true);
            if (crowdEngagement == 2 && crowdChant)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                new MultiSound.Sound("figureFighter/crowdJab", beat + 1.0),
                }, forcePlay: true);
            }
            ScheduleInput(beat, 1f, InputAction_BasicPress, JustJab, MissJab, null);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1f, delegate
                {
                    isPreparing = false;
                }),
                new BeatAction.Action(beat + 1.75f, delegate
                {
                    if (isPreparing == false) canBop = true;
                }),
            });
        }

        public void Fart(float state = -1f)
        {
            if (!PlayerInput.GetIsAction(InputAction_Shoulders)) return;

            if (state < 0.2f && state > -0.2f)
            {
                fartAnim.DoScaledAnimationAsync("gas_L", 0.5f);
            }
            else
            {
                fartAnim.DoScaledAnimationAsync("gas_S", 0.5f);
            }
        }

        public void JabAnd(double beat, bool and)
        {
            if (and)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("figureFighter/and", beat - 0.5f),
                }, forcePlay: true);
            }
        }

        public void OneTwoAnd(double beat, bool and)
        {
            if (and)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("figureFighter/and2", beat - 0.5f),
                }, forcePlay: true);
            }
        }

        public void JustJab(PlayerActionEvent caller, float state)
        {
            HideBars();
            var beat = caller.startBeat + caller.timer;
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("figureFighter/barely");
                dollAnim.DoScaledAnimationAsync("FigureBarely1", 0.5f);
                buttonAnim.DoScaledAnimationAsync("ButtonPress", 0.5f);
                bagAnim.DoScaledAnimationAsync("BagBarely", 0.5f);
            }
            else
            {
                if (strongPunch)
                {
                    strongPunch = false;

                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("figureFighter/jabHit", beat),
                        new MultiSound.Sound($"figureFighter/break{UnityEngine.Random.Range(1, 9)}", beat),
                        new MultiSound.Sound("figureFighter/pump", beat, volume: 0.4f),
                    }, forcePlay: true);

                    dollAnim.DoScaledAnimationAsync("FigureFinisher", 0.5f);
                    buttonAnim.DoScaledAnimationAsync("ButtonPress2", 0.5f);

                    chainParticles1.Play();
                    bagObject.gameObject.SetActive(false);
                    bagCopyAnim.DoScaledAnimationAsync("Blow", 0.5f);

                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 1, delegate {
                            bagObject.gameObject.SetActive(true);
                            bagAnim.DoScaledAnimationAsync("BagSummon", 0.5f);
                        }),
                    });
                }
                else
                {
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("figureFighter/jabHit", beat),
                        new MultiSound.Sound("figureFighter/pump", beat, volume: 0.4f),
                    }, forcePlay: true);
                    dollAnim.DoScaledAnimationAsync("FigureJab", 0.5f);
                    buttonAnim.DoScaledAnimationAsync("ButtonPress", 0.5f);
                    bagAnim.DoScaledAnimationAsync("BagHit1", 0.5f);
                }
            }
            
            Fart(state);
        }

        public void MissJab(PlayerActionEvent caller)
        {
            HideBars(); //replace with FadeBars at some point
            dollAnim.DoScaledAnimationAsync("FigureWhiff1", 0.5f);
            bagAnim.DoScaledAnimationAsync("BagThrough", 0.5f);
            SoundByte.PlayOneShotGame("figureFighter/failHit");
        }

        private IEnumerator FigurePrepWaiter()
        {
            yield return new WaitUntil(() => dollAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
            dollAnim.DoScaledAnimationAsync("FigurePrep1", 0.5f);
        }

        public void OneTwo(double beat, bool strong)
        {
            if (strong)
            {
                strongPunch = true;
            } 
            else strongPunch = false;

            MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("figureFighter/oneTwo1", beat + 0.0),
                    new MultiSound.Sound("figureFighter/oneTwo2", beat + 1.0),
                }, forcePlay: true);
            if (crowdEngagement == 2 && crowdChant)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                new MultiSound.Sound("figureFighter/crowdOne", beat + 2.0),
                new MultiSound.Sound("figureFighter/crowdTwo", beat + 3.0),
                }, forcePlay: true);
            }
            ScheduleInput(beat, 2f, InputAction_BasicPress, JustOneTwoFirst, MissOneTwoFirst, null);
            ScheduleInput(beat, 3f, InputAction_BasicPress, JustOneTwoSecond, MissOneTwoSecond, null);

            canBop = false;
            isPreparing = true;
            if (!dollAnim.IsPlayingAnimationNames(priorityAnims))
            {
                dollAnim.DoScaledAnimationAsync("FigurePrep1", 0.5f);
            }
            else
            {
                StartCoroutine(FigurePrepWaiter());
            }
            if (toggleBars)
            {
                ShowBars();
                barsAnim.DoScaledAnimationAsync("CloseIn1", 0.5f);
            }

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1, delegate
                {
                    if (toggleBars)
                    {
                        barsAnim.DoScaledAnimationAsync("CloseIn2", 0.5f);
                    }
                }),
                new BeatAction.Action(beat + 2, delegate
                {
                    isPreparing = false;
                }),
                new BeatAction.Action(beat + 3.5f, delegate
                {
                    if (isPreparing == false) { canBop = true; }
                }),
            });

        }

        public void OneTwoFast(double beat, bool strong)
        {
            if(strong)
            {
                strongPunch = true;
            } else strongPunch = false;
            
            MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("figureFighter/fastOneTwo1", beat + 0.0),
                    new MultiSound.Sound("figureFighter/fastOneTwo2", beat + 0.5),
                }, forcePlay: true);
            if (crowdEngagement == 2 && crowdChant)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                new MultiSound.Sound("figureFighter/crowdOneFast", beat + 1.0),
                new MultiSound.Sound("figureFighter/crowdTwoFast", beat + 1.5),
                }, forcePlay: true);
            }
            ScheduleInput(beat, 1f, InputAction_BasicPress, JustOneTwoFirst, MissOneTwoFirst, null);
            ScheduleInput(beat, 1.5f, InputAction_BasicPress, JustOneTwoSecond, MissOneTwoSecond, null);

            canBop = false;

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 2f, delegate
                {
                    if (isPreparing == false) { canBop = true; }
                }),
            });
        }

        public void JustOneTwoFirst(PlayerActionEvent caller, float state)
        {
                HideBars();
                var beat = caller.startBeat + caller.timer;
                if (state >= 1f || state <= -1f)
                {
                    buttonAnim.DoScaledAnimationAsync("ButtonPress", 0.5f);
                    dollAnim.DoScaledAnimationAsync("FigureBarely2", 0.5f);
                    SoundByte.PlayOneShotGame("figureFighter/barely");
                    bagAnim.DoScaledAnimationAsync("BagBarely", 0.5f);
                }
                else
                {
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("figureFighter/oneTwoHit1", beat),
                        new MultiSound.Sound("figureFighter/pump", beat, volume: 0.4f),
                    }, forcePlay: true);
                    dollAnim.DoScaledAnimationAsync("FigureJab", 0.5f);
                    buttonAnim.DoScaledAnimationAsync("ButtonPress", 0.5f);
                    bagAnim.DoScaledAnimationAsync("BagHit1", 0.5f);
                }

                Fart(state);
        }

        public void JustOneTwoSecond(PlayerActionEvent caller, float state)
        {
            var beat = caller.startBeat + caller.timer;
                if (state >= 1f || state <= -1f)
                {
                    buttonAnim.DoScaledAnimationAsync("ButtonPress", 0.5f);
                    dollAnim.DoScaledAnimationAsync("FigureBarely3", 0.5f);
                    SoundByte.PlayOneShotGame("figureFighter/barely");
                    bagAnim.DoScaledAnimationAsync("BagBarely", 0.5f);
                }
                else
                {
                    if(strongPunch)
                    {
                        strongPunch = false;

                        MultiSound.Play(new MultiSound.Sound[]
                        {
                            new MultiSound.Sound("figureFighter/oneTwoHit2", beat),
                            new MultiSound.Sound($"figureFighter/break{UnityEngine.Random.Range(1, 9)}", beat),
                            new MultiSound.Sound("figureFighter/pump", beat, volume: 0.4f),
                        }, forcePlay: true);

                        dollAnim.DoScaledAnimationAsync("FigureFinisher", 0.5f);
                        buttonAnim.DoScaledAnimationAsync("ButtonPress2", 0.5f);

                        chainParticles1.Play();
                        bagObject.gameObject.SetActive(false);
                        bagCopyAnim.DoScaledAnimationAsync("Blow", 0.5f);

                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat + 1, delegate { 
                                bagObject.gameObject.SetActive(true);
                                bagAnim.DoScaledAnimationAsync("BagSummon", 0.5f);
                            }),
                        });
                    }
                    else
                    {
                        MultiSound.Play(new MultiSound.Sound[]
                        {
                            new MultiSound.Sound("figureFighter/oneTwoHit2", beat),
                            new MultiSound.Sound("figureFighter/pump", beat, volume: 0.4f),
                        }, forcePlay: true);
                        dollAnim.DoScaledAnimationAsync("FigureJab2", 0.5f);
                        buttonAnim.DoScaledAnimationAsync("ButtonPress", 0.5f);
                        bagAnim.DoScaledAnimationAsync("BagHit2", 0.5f);
                    }
                }

                Fart(state);

        }

        public void JustOneTwoBreakSecond(PlayerActionEvent caller, float state)
        {
            var beat = caller.startBeat + caller.timer;
                if (state >= 1f || state <= -1f)
                {
                    buttonAnim.DoScaledAnimationAsync("ButtonPress", 0.5f);
                    dollAnim.DoScaledAnimationAsync("FigureBarely4", 0.5f);
                    SoundByte.PlayOneShotGame("figureFighter/barely");
                    bagAnim.DoScaledAnimationAsync("BagBarely", 0.5f);
                }
            else
                {
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("figureFighter/oneTwoHit2", beat),
                        new MultiSound.Sound("figureFighter/pump", beat, volume: 0.4f),
                    }, forcePlay: true);
                    dollAnim.DoScaledAnimationAsync("FigureJab2", 0.5f);
                    buttonAnim.DoScaledAnimationAsync("ButtonPress", 0.5f);
                    bagAnim.DoScaledAnimationAsync("BagHit2", 0.5f);
                }

                Fart(state);

        }

        public void MissOneTwoFirst(PlayerActionEvent caller)
        {
            HideBars(); //replace with FadeBars at some point
            dollAnim.DoScaledAnimationAsync("FigureWhiff1", 0.5f);
            bagAnim.DoScaledAnimationAsync("BagThrough", 0.5f);
            SoundByte.PlayOneShotGame("figureFighter/failHit");
        }

        public void MissOneTwoSecond(PlayerActionEvent caller)
        {
            HideBars(); //replace with FadeBars at some point
            dollAnim.DoScaledAnimationAsync("FigureWhiff2", 0.5f);
            bagAnim.DoScaledAnimationAsync("BagThrough", 0.5f);
            SoundByte.PlayOneShotGame("figureFighter/failHit");
        }

        public void GoGoGo(double beat, bool strong)
        {
            if(strong)
            {
                strongPunch = true;
            } else strongPunch = false;
            MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("figureFighter/go1", beat + 0.0),
                    new MultiSound.Sound("figureFighter/go2", beat + 0.5),
                    new MultiSound.Sound("figureFighter/go3", beat + 1.0),
                }, forcePlay: true);
            if (crowdEngagement == 2 && crowdChant)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                new MultiSound.Sound("figureFighter/crowdGo1", beat + 2.0),
                new MultiSound.Sound("figureFighter/crowdGo2", beat + 2.5),
                new MultiSound.Sound("figureFighter/crowdGo3", beat + 3.0),
                }, forcePlay: true);
            }
            ScheduleInput(beat, 2f, InputAction_BasicPress, JustOneTwoFirst, MissOneTwoFirst, null);
            ScheduleInput(beat, 2.5f, InputAction_BasicPress, JustOneTwoFirst, MissOneTwoFirst, null);
            ScheduleInput(beat, 3f, InputAction_BasicPress, JustGo, MissGo, null);

            canBop = false;
            isPreparing = true;
            if (!dollAnim.IsPlayingAnimationNames(priorityAnims))
            {
                dollAnim.DoScaledAnimationAsync("FigurePrep1", 0.5f);
            }
            else
            {
                StartCoroutine(FigurePrepWaiter());
            }
            if (toggleBars)
            {
                ShowBars();
                barsAnim.DoScaledAnimationAsync("CloseIn1", 0.5f);
            }

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.5, delegate
                {
                    if (toggleBars)
                    {
                        barsAnim.DoScaledAnimationAsync("CloseIn2", 0.5f);
                    }
                }),
                new BeatAction.Action(beat + 1, delegate
                {
                    if (toggleBars)
                    {
                        barsAnim.DoScaledAnimationAsync("CloseIn3", 0.5f);
                    }
                }),
                new BeatAction.Action(beat + 2, delegate
                {
                    isPreparing = false;
                }),
                new BeatAction.Action(beat + 3.5f, delegate
                {
                    if (isPreparing == false) { canBop = true; }
                }),
            });

        }

        public void JustGo(PlayerActionEvent caller, float state)
        {
            var beat = caller.startBeat + caller.timer;
                if (state >= 1f || state <= -1f)
                {
                    SoundByte.PlayOneShotGame("figureFighter/barely");
                    dollAnim.DoScaledAnimationAsync("FigureBarely3", 0.5f);
                    buttonAnim.DoScaledAnimationAsync("ButtonPress", 0.5f);
                    bagAnim.DoScaledAnimationAsync("BagBarely", 0.5f);
                }
                else
                {
                    if(strongPunch)
                    {
                        strongPunch = false;

                        MultiSound.Play(new MultiSound.Sound[]
                        {
                            new MultiSound.Sound("figureFighter/oneTwoHit2", beat),
                            new MultiSound.Sound($"figureFighter/break{UnityEngine.Random.Range(1, 9)}", beat),
                            new MultiSound.Sound("figureFighter/pump", beat, volume: 0.4f),
                        }, forcePlay: true);

                        dollAnim.DoScaledAnimationAsync("FigureFinisher", 0.5f);
                        buttonAnim.DoScaledAnimationAsync("ButtonPress2", 0.5f);

                        chainParticles2.Play();
                        bagObject.gameObject.SetActive(false);
                        bagCopyAnim.DoScaledAnimationAsync("Blow", 0.5f);

                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat + 1, delegate { 
                                bagObject.gameObject.SetActive(true);
                                bagAnim.DoScaledAnimationAsync("BagSummon", 0.5f);
                            }),
                        });
                    }
                    else
                    {
                        MultiSound.Play(new MultiSound.Sound[]
                        {
                            new MultiSound.Sound("figureFighter/goLastHit", beat),
                            new MultiSound.Sound("figureFighter/pump", beat, volume: 0.4f),
                        }, forcePlay: true);
                        dollAnim.DoScaledAnimationAsync("FigureJab2", 0.5f);
                        buttonAnim.DoScaledAnimationAsync("ButtonPress", 0.5f);
                        bagAnim.DoScaledAnimationAsync("BagHit2", 0.5f);
                    }
                }
            
            Fart(state);
        }

        public void Bars(double beat, bool toggle, bool sticky)
        {
            toggleBars = toggle;
            StickyLayer.Sticky = sticky;
        }

        public void HideBars()
        {
            foreach (Renderer renderer in barRenderers)
            {
                renderer.enabled = false; 
            }
        }

        public void ShowBars()
        {
            foreach (Renderer renderer in barRenderers)
            {
                renderer.enabled = true;
            }
        }

        //public void FadeBars()
        //{

        //}


        public void MissGo(PlayerActionEvent caller)
        {
            {
                dollAnim.DoScaledAnimationAsync("FigureWhiff2", 0.5f);
                bagAnim.DoScaledAnimationAsync("BagThrough", 0.5f);
                SoundByte.PlayOneShotGame("figureFighter/failHit");
            }
        }
    }
}