using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using HeavenStudio.Util;
using DG.Tweening;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;

    public static class RvlBoardMeetingLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("boardMeeting", "Board Meeting", "d37912", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; BoardMeeting.instance.Bop(e.beat, e.length, e["bop"], e["auto"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop", "Toggle if the pigs & assistant should bop for the duration of this event."),
                        new Param("auto", false, "Bop (Auto)", "Toggle if the pigs & assistant should automatically bop until another Bop event is reached.")
                    }
                },
                new GameAction("prepare", "Prepare")
                {
                    function = delegate { BoardMeeting.instance.Prepare(); }
                },
                new GameAction("spinEqui", "Spin")
                {
                    function = delegate {var e = eventCaller.currentEntity; BoardMeeting.instance.SpinEqui(e.beat, e.length); },
                    resizable = true,
                    priority = 2
                },
                new GameAction("spin", "Spin (Range)")
                {
                    function = delegate {var e = eventCaller.currentEntity; BoardMeeting.instance.Spin(e["start"], e["end"]); },
                    parameters = new List<Param>()
                    {
                        new Param("start", new EntityTypes.Integer(1, 6, 1), "Starting Pig", "Choose the leftmost pig in the range to start spinning."),
                        new Param("end", new EntityTypes.Integer(1, 6, 4), "Ending Pig", "Choose the rightmost pig in the range to start spinning.")
                    },
                    priority = 2
                },
                new GameAction("stop", "Stop")
                {
                    function = delegate { var e = eventCaller.currentEntity; BoardMeeting.instance.Stop(e.beat, e.length); },
                    resizable = true,
                    priority = 1
                },
                new GameAction("assStop", "Assistant Stop")
                {
                    function = delegate { var e = eventCaller.currentEntity; BoardMeeting.instance.AssistantStop(e.beat); },
                    defaultLength = 3f
                },
                new GameAction("changeCount", "Change Pig Number")
                {
                    function = delegate { BoardMeeting.instance.ChangeExecutiveCount(eventCaller.currentEntity["amount"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("amount", new EntityTypes.Integer(3, 5, 4), "Pigs", "Set how many pigs there will be. The player is always the rightmost pig.")
                    }
                },
                new GameAction("bgAppear", "Background Appearance")
                {
                    function = delegate {var e = eventCaller.currentEntity; BoardMeeting.instance.BGColor
                    (e.beat, e.length, e["startCarp"], e["endCarp"], e["startWoodB"], e["endWoodB"], e["startWoodI"], e["endWoodI"], e["startWood"], e["endWood"], e["startWoodT"], e["endWoodT"], 
                    e["startWall"], e["endWall"], e["startWallB"], e["endWallB"], e["ease"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("startCarp",  BoardMeeting.carpetColor, "Start Color (Carpet)",   "Set the color at the start of the event."),
                        new Param("endCarp",    BoardMeeting.carpetColor, "End Color (Carpet)",     "Set the color at the end of the event."),

                        new Param("startWoodB",  BoardMeeting.woodBColor, "Start Color (Wood)",   "Set the color at the start of the event."),
                        new Param("endWoodB",    BoardMeeting.woodBColor, "End Color (Wood)",     "Set the color at the end of the event."),
                        new Param("startWoodI",  BoardMeeting.woodIColor, "Start Color (Wood)",   "Set the color at the start of the event."),
                        new Param("endWoodI",    BoardMeeting.woodIColor, "End Color (Wood)",     "Set the color at the end of the event."),
                        new Param("startWood",  BoardMeeting.woodColor, "Start Color (Wood)",   "Set the color at the start of the event."),
                        new Param("endWood",    BoardMeeting.woodColor, "End Color (Wood)",     "Set the color at the end of the event."),
                        new Param("startWoodT",  BoardMeeting.woodTColor, "Start Color (Wood)",   "Set the color at the start of the event."),
                        new Param("endWoodT",    BoardMeeting.woodTColor, "End Color (Wood)",     "Set the color at the end of the event."),

                        new Param("startWall",  BoardMeeting.wallColor, "Start Color (Wall)",   "Set the color at the start of the event."),
                        new Param("endWall",    BoardMeeting.wallColor, "End Color (Wall)",     "Set the color at the end of the event."),
                        new Param("startWallB",  BoardMeeting.wallBColor, "Start Color (Wall)",   "Set the color at the start of the event."),
                        new Param("endWallB",    BoardMeeting.wallBColor, "End Color (Wall)",     "Set the color at the end of the event."),

                        new Param("ease",           Util.EasingFunction.Ease.Linear,    "Ease", "Set the easing of the action.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x != (int)Util.EasingFunction.Ease.Instant, new string[] { "startCarp", "startWoodB", "startWoodI", "startWood", "startWoodT", "startWall", "startWallB", }),
                        }),
                    }
                },
                new GameAction("chairColor", "Chair Appearance")
                {
                    function = delegate {var e = eventCaller.currentEntity; BoardMeeting.instance.ChairColor(e["chair"], e["legs"]); },
                    parameters = new List<Param>()
                    {
                        new Param("chair", BoardMeeting.chairColor, "Chair Color", "Set the color of the executives' chairs."),
                        new Param("legs", BoardMeeting.legsColor, "Legs Color", "Set the color of the legs of the executives' chairs."),
                    },
                    defaultLength = 0.5f,
                }
            },
            new List<string>() { "rvl", "normal" },
            "rvlrotation", "en",
            new List<string>() { "en", "jp" },
            chronologicalSortKey: 8,
            font : "Matisse Minori"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_BoardMeeting;
    using System;

    public class BoardMeeting : Minigame
    {
        [Header("Components")]
        [SerializeField] Transform farLeft;
        [SerializeField] Transform farRight;
        [SerializeField] Animator assistantAnim;

        [Header("Properties")]
        [SerializeField] int executiveCount = 4;
        [SerializeField] List<BMExecutive> executives = new List<BMExecutive>();
        public BMExecutive firstSpinner;
        [SerializeField] float shakeIntensity = 0.5f;
        private bool assistantCanBop = true;
        private bool executivesCanBop = true;
        [NonSerialized] public Sound chairLoopSound = null;
        int missCounter = 0;
        private Tween shakeTween;

        public Material Chair;

        [Header("BG")]
        public Material Carpet;
        public Material Wood;
        public Material WoodGrain;
        public Material Wall;

        public static Color carpetColor = "FF0000".Hex2RGB();
        private ColorEase carpetEase = new(carpetColor);

        public static Color woodBColor = "7B4200".Hex2RGB(); //border
        private ColorEase woodBEase = new(woodBColor);
        public static Color woodIColor = "A55A00".Hex2RGB(); //in-border
        private ColorEase woodIEase = new(woodIColor);
        public static Color woodColor = "C66B00".Hex2RGB();
        private ColorEase woodEase = new(woodColor);
        public static Color woodTColor = "C66300".Hex2RGB(); //texture
        private ColorEase woodTEase = new(woodTColor);

        public static Color wallColor = "E7DEDE".Hex2RGB();
        private ColorEase wallEase = new(wallColor);
        public static Color wallBColor = "C6C6C6".Hex2RGB(); //border
        private ColorEase wallBEase = new(wallBColor);

        public static Color chairColor = "3942FF".Hex2RGB();
        public static Color legsColor = "8C8C8C".Hex2RGB();

        public static BoardMeeting instance;

        private void Awake()
        {
            instance = this;
            SetupBopRegion("boardMeeting", "bop", "auto");
            InitExecutives();
        }

        private void OnDestroy()
        {
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
            if (chairLoopSound != null)
            {
                chairLoopSound.KillLoop(0);
                chairLoopSound = null;
            }
        }

        // japanese voices: "one" is scheduled a little before the assistant's stop, see VoiceLeadCues
        VoiceLeadCues voiceCues;

        private void Update()
        {
            UpdateBGColor();
            var cond = Conductor.instance;
            voiceCues ??= new VoiceLeadCues("boardMeeting", e => { SoundByte.PlayOneShotGame("boardMeeting/one", e.beat); return true; }, "assStop");
            voiceCues.Update();

            if (cond.isPlaying && !cond.isPaused)
            {
                if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
                {
                    if (executives[executiveCount - 1].spinning)
                    {
                        executives[executiveCount - 1].Stop(false);
                        SoundByte.PlayOneShotGame("boardMeeting/miss");
                        SoundByte.PlayOneShot("miss");
                        ScoreMiss();
                    }
                }
            }
        }

        public override void OnBeatPulse(double beat)
        {
            if (!BeatIsInBopRegion(beat)) return;
            SingleBop();
        }

        public override void OnPlay(double beat)
        {
            UpdateBGColor();
            PersistBGColor(beat);
        }

        public override void OnGameSwitch(double beat)
        {
            OnPlay(beat);
        }

        void SingleBop()
        {
            if (assistantCanBop)
            {
                if (missCounter > 0) assistantAnim.DoScaledAnimationAsync("MissBop", 0.5f);
                else assistantAnim.DoScaledAnimationAsync("Bop", 0.5f);
            }
            if (missCounter > 0) missCounter--;
            if (!executivesCanBop) return;
            foreach (var executive in executives)
            {
                executive.Bop();
            }
        }

        public void Bop(double beat, float length, bool goBop, bool autoBop)
        {
            if (goBop)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate
                        {
                            SingleBop();
                        })
                    });
                }
            }
        }

        public void AssistantStop(double beat)
        {
            assistantCanBop = false;
            string twoSound = "boardMeeting/two";
            if (beat % 1 != 0) twoSound = "boardMeeting/twoUra";
            var sounds = new List<MultiSound.Sound>
            {
                new MultiSound.Sound("boardMeeting/one", beat),
                new MultiSound.Sound(twoSound, beat + 0.5f),
                new MultiSound.Sound("boardMeeting/three", beat + 1),
                new MultiSound.Sound("boardMeeting/stopAll", beat + 2)
            };
            if (voiceCues != null && voiceCues.Claim(beat)) sounds.RemoveAt(0);  // already scheduled
            MultiSound.Play(sounds);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { assistantAnim.DoScaledAnimationAsync("One", 0.5f); }),
                new BeatAction.Action(beat + 1, delegate { assistantAnim.DoScaledAnimationAsync("Three", 0.5f); }),
                new BeatAction.Action(beat + 2, delegate
                {
                    foreach (var executive in executives)
                    {
                        if (executive.player) continue;
                        executive.Stop();
                    }
                    if (!executives[executiveCount - 1].spinning)
                    {
                        if (chairLoopSound != null)
                        {
                            chairLoopSound.KillLoop(0);
                            chairLoopSound = null;
                        }
                    }
                }),
                new BeatAction.Action(beat + 2.5f, delegate { assistantCanBop = true; })
            });
            ScheduleInput(beat, 2f, InputAction_BasicPress, JustAssistant, MissAssistant, Empty, CanJust);
        }

        public void Stop(double beat, float length)
        {
            executivesCanBop = false;
            List<BeatAction.Action> stops = new List<BeatAction.Action>();
            for (int i = 0; i < executiveCount; i++)
            {
                if (executives[i].player) break;
                int index = i;
                int ex = executiveCount;
                if (executiveCount < 4) ex = 4;
                if (index < ex - 3)
                {
                    SoundByte.PlayOneShotGame("boardMeeting/stopA", beat + length * i);
                }
                else if (index == ex - 3)
                {
                    SoundByte.PlayOneShotGame("boardMeeting/stopB", beat + length * i);
                }
                else if (index == ex - 2)
                {
                    SoundByte.PlayOneShotGame("boardMeeting/stopC", beat + length * i);
                }
                stops.Add(new BeatAction.Action(beat + length * i, delegate
                {
                    if (index == executiveCount - 2 && !executives[executiveCount - 1].spinning)
                    {
                        if (chairLoopSound != null)
                        {
                            chairLoopSound.KillLoop(0);
                            chairLoopSound = null;
                        }
                    }
                    executives[index].Stop();
                }));
            }
            stops.Add(new BeatAction.Action(beat + length * executiveCount + 0.5f, delegate { executivesCanBop = true; }));
            BeatAction.New(instance, stops);
            ScheduleInput(beat, length * (executiveCount - 1), InputAction_BasicPress, Just, Miss, Empty, CanJust);
        }

        public void Prepare()
        {
            SoundByte.PlayOneShotGame("boardMeeting/prepare");
            foreach (var executive in executives)
            {
                executive.Prepare();
            }
        }

        public void SpinEqui(double beat, float length)
        {
            if (chairLoopSound == null) chairLoopSound = SoundByte.PlayOneShotGame("boardMeeting/chairLoop", -1, 1, 1, true);
            firstSpinner = executives[0];
            List<BeatAction.Action> rolls = new List<BeatAction.Action>();
            for (int i = 0; i < executiveCount; i++)
            {
                int index = i;
                rolls.Add(new BeatAction.Action(beat + length * i, delegate
                {
                    int ex = executiveCount;
                    string soundToPlay = "A";
                    if (executiveCount < 4) ex = 4;
                    if (index == ex - 3)
                    {
                        soundToPlay = "B";
                    }
                    else if (index == ex - 2)
                    {
                        soundToPlay = "C";
                    }
                    else if (index == ex - 1)
                    {
                        soundToPlay = "Player";
                    }
                    executives[index].Spin(soundToPlay);
                }));
            }
            BeatAction.New(instance, rolls);

        }

        public void Spin(int start, int end)
        {
            if (start > executiveCount || end > executiveCount) return;
            bool forceStart = false;
            chairLoopSound?.KillLoop(0);
            if (chairLoopSound == null)
            {
                chairLoopSound = SoundByte.PlayOneShotGame("boardMeeting/chairLoop", -1, 1, 1, true);
                firstSpinner = executives[start - 1];
                forceStart = true;
            }
            for (int i = start - 1; i < end; i++)
            {
                int ex = executiveCount;
                string soundToPlay = "A";
                if (executiveCount < 4) ex = 4;
                if (i == ex - 3)
                {
                    soundToPlay = "B";
                }
                else if (i == ex - 2)
                {
                    soundToPlay = "C";
                }
                else if (i == ex - 1)
                {
                    soundToPlay = "Player";
                }
                executives[i].Spin(soundToPlay, forceStart);
            }
        }

        public void StopChairLoopSoundIfLastToStop()
        {
            if (executives.FindAll(x => x.spinning).Count > 1) return;
            if (chairLoopSound != null)
            {
                chairLoopSound.KillLoop(0);
                chairLoopSound = null;
            }
        }

        public void InitExecutives()
        {
            float startPos = farLeft.position.x;
            float maxWidth = Mathf.Abs(farLeft.position.x - farRight.position.x);

            float betweenDistance = maxWidth / 3;

            maxWidth = betweenDistance * executiveCount;

            startPos = -(maxWidth / 2);

            for (int i = -1; i < executiveCount; i++)
            {
                if (i == -1)
                {
                    assistantAnim.transform.localPosition = new Vector3(startPos + 5.359f, 0);
                }
                else
                {
                    BMExecutive executive;
                    if (i == 0) executive = executives[0];
                    else executive = Instantiate(executives[0], transform);
                    executive.name = "Executive #" + i;

                    executive.transform.localPosition = new Vector3(startPos + betweenDistance * (i + 1), 0);
                    executive.GetComponent<SortingGroup>().sortingOrder = i;

                    if (i > 0)
                        executives.Add(executive);

                    if (i == executiveCount - 1)
                        executive.player = true;
                }
            }
        }

        public void ChangeExecutiveCount(int count)
        {
            for (int i = 1; i < executiveCount; i++)
            {
                Destroy(executives[i].gameObject);
            }
            executives.RemoveRange(1, executiveCount - 1);
            executiveCount = count;
            InitExecutives();
        }

        bool CanJust()
        {
            return executives[executiveCount - 1].spinning;
        }

        void Just(PlayerActionEvent caller, float state)
        {
            if (chairLoopSound != null)
            {
                chairLoopSound.KillLoop(0);
                chairLoopSound = null;
            }
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("boardMeeting/missThrough");
                SoundByte.PlayOneShot("miss");
                executives[executiveCount - 1].Stop(false);
                return;
            }
            SoundByte.PlayOneShotGame("boardMeeting/stopPlayer");
            executives[executiveCount - 1].Stop();
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.timer + caller.startBeat + 1f, delegate
                {
                    foreach (var executive in executives)
                    {
                        executive.Smile();
                    }
                })
            });
        }

        void JustAssistant(PlayerActionEvent caller, float state)
        {
            if (chairLoopSound != null)
            {
                chairLoopSound.KillLoop(0);
                chairLoopSound = null;
            }
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("boardMeeting/missThrough");
                SoundByte.PlayOneShot("miss");
                executives[executiveCount - 1].Stop(false);
                return;
            }
            if (shakeTween != null)
                shakeTween.Kill(true);

            DOTween.Punch(() => GameCamera.AdditionalPosition, x => GameCamera.AdditionalPosition = x, new Vector3(shakeIntensity, 0, 0),
                Conductor.instance.pitchedSecPerBeat * 0.5f, 18, 1f);
            executives[executiveCount - 1].Stop();
            assistantAnim.DoScaledAnimationAsync("Stop", 0.5f);
            SoundByte.PlayOneShotGame("boardMeeting/stopAllPlayer");
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.timer + caller.startBeat + 1f, delegate
                {
                    foreach (var executive in executives)
                    {
                        executive.Smile();
                    }
                })
            });
        }

        void Miss(PlayerActionEvent caller)
        {
            if (executives[executiveCount - 1].spinning)
            {
                executives[executiveCount - 1].Stop(false);
                SoundByte.PlayOneShotGame("boardMeeting/missThrough");
                SoundByte.PlayOneShot("miss");
                if (chairLoopSound != null)
                {
                    chairLoopSound.KillLoop(0);
                    chairLoopSound = null;
                }
            }
        }

        void MissAssistant(PlayerActionEvent caller)
        {
            if (executives[executiveCount - 1].spinning)
            {
                executives[executiveCount - 1].Stop(false);
                SoundByte.PlayOneShotGame("boardMeeting/missThrough");
                SoundByte.PlayOneShot("miss");
                if (chairLoopSound != null)
                {
                    chairLoopSound.KillLoop(0);
                    chairLoopSound = null;
                }
            }
            assistantAnim.Play("MissIdle", 0, 0);
            missCounter = 2;
        }

        void Empty(PlayerActionEvent caller) { }

        public void BGColor(double beat, float length, Color sCarp, Color eCarp, Color sWoodB, Color eWoodB, Color sWoodI, Color eWoodI, Color sWood, Color eWood, Color sWoodT, Color eWoodT,
                            Color sWall, Color eWall, Color sWallB, Color eWallB, int ease)
        {
            carpetEase = new ColorEase(beat, length, sCarp, eCarp, ease);
            woodBEase = new ColorEase(beat, length, sWoodB, eWoodB, ease);
            woodIEase = new ColorEase(beat, length, sWoodI, eWoodI, ease);
            woodEase = new ColorEase(beat, length, sWood, eWood, ease);
            woodTEase = new ColorEase(beat, length, sWoodT, eWoodT, ease);
            wallEase = new ColorEase(beat, length, sWall, eWall, ease);
            wallBEase = new ColorEase(beat, length, sWallB, eWallB, ease);
        }

        private void UpdateBGColor()
        {
            Carpet.SetColor("_Color", carpetEase.GetColor());
            Wood.SetColor("_ColorAlpha", woodBEase.GetColor());
            Wood.SetColor("_ColorBravo", woodIEase.GetColor());
            //two below share exact same space
            Wood.SetColor("_ColorDelta", woodEase.GetColor());
            WoodGrain.SetColor("_ColorDelta", woodEase.GetColor());
            //
            WoodGrain.SetColor("_ColorBravo", woodTEase.GetColor());
            Wall.SetColor("_ColorAlpha", wallEase.GetColor());
            Wall.SetColor("_ColorBravo", wallBEase.GetColor());
        }

        private void PersistBGColor(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("boardMeeting", new string[] { "bgAppear" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                BGColor(lastEvent.beat, lastEvent.length, 
                lastEvent["startCarp"], lastEvent["endCarp"], 
                lastEvent["startWoodB"], lastEvent["endWoodB"], 
                lastEvent["startWoodI"], lastEvent["endWoodI"], 
                lastEvent["startWood"], lastEvent["endWood"], 
                lastEvent["startWoodT"], lastEvent["endWoodT"], 
                lastEvent["startWall"], lastEvent["endWall"], 
                lastEvent["startWallB"], lastEvent["endWallB"], 
                lastEvent["ease"]);
            }

            allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("boardMeeting", new string[] { "chairColor" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                ChairColor(lastEvent["chair"], lastEvent["legs"]);
            }
            else
            {
                ChairColor(chairColor, legsColor);
            }

            UpdateBGColor();
        }

        public void ChairColor(Color chairC, Color legsC)
        {
            Chair.SetColor("_ColorDelta", chairC);
            Chair.SetColor("_ColorAlpha", legsC);            
        }
    }
}

