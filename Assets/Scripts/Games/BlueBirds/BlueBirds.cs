using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;

using Debug = UnityEngine.Debug;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrBirdLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("blueBirds", "Blue Birds", "49CD9C", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out BlueBirds instance)) {
                            instance.Bop(e.beat, e.length, e["bop"], e["auto"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop", "Toggle if the Birds should bop for the duration of this event."),
                        new Param("auto", false, "Bop (Auto)", "Toggle if the Birds should automatically bop until another Bop event is reached.")
                    },
                    resizable = true,
                    defaultLength = 1f,
                },
                new GameAction("peck", "Peck Your Beak")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out BlueBirds instance)) {
                            instance.PeckYourBeak(e.beat);
                        }
                    },
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out BlueBirds instance)) {
                            instance.NoBopping(e.beat);
                        }
                    },
                    preFunctionLength = 0.25f,
                    defaultLength = 4f,
                },
                new GameAction("stretch", "Stretch Out Your Neck")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out BlueBirds instance)) {
                            instance.StretchOutYourNeck(e.beat);
                        }
                    },
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out BlueBirds instance)) {
                            instance.NoBopping(e.beat);
                        }
                    },
                    preFunctionLength = 0.25f,
                    defaultLength = 5f,
                },
                new GameAction("showMemory", "Show Memory")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out BlueBirds instance)) {
                            instance.ShowMemory((BlueBirds.Memory)e["memory"], e.beat, e.length);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("memory", BlueBirds.Memory.Part1, "Memory", "Which Memory should appear?"),
                    },
                    resizable = true,
                    defaultLength = 4f,
                },
                new GameAction("showText", "Display Text")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out BlueBirds instance)) {
                            instance.ShowText(e["showtext"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("showtext", true, "Fin.", "Toggle if the text should appear."),
                    },
                    defaultLength = 0.5f,
                },
                new GameAction("hideCaptain", "Captain State")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out BlueBirds instance)) {
                            instance.CaptainBBState((BlueBirds.CaptainState)e["state"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("state", BlueBirds.CaptainState.Exit, "Hide/Show Captain", "Moves the Captain either off-screen or on-screen."),
                    },
                },
                new GameAction("moveCaptain", "Set Captain Position")
                {
                    function = delegate { var e = eventCaller.currentEntity; BlueBirds.instance.SetCaptMovement(e); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("doMove", false, "Move", "Select this option if you want to move the Captain.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "endMoveX", "endMoveY" } ),
                            new Param.CollapseParam((x, e) => (bool)x && (Util.EasingFunction.Ease)e["ease"] != Util.EasingFunction.Ease.Instant, new string[] { "startMoveX", "startMoveY" }),
                            new Param.CollapseParam((_, e) => (bool)e["doMove"], new string[] { "ease" })
                        }),
                        new Param("startMoveX", new EntityTypes.Float(-20f, 20f, 0f), "Start X", "Set the X position from which to move."),
                        new Param("startMoveY", new EntityTypes.Float(-20f, 20f, 0f), "Start Y", "Set the Y position from which to move."),
                        new Param("endMoveX", new EntityTypes.Float(-20f, 20f, 0f), "End X", "Set the X position to move to."),
                        new Param("endMoveY", new EntityTypes.Float(-20f, 20f, 0f), "End Y", "Set the Y position to move to."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing for the action.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, e) => (Util.EasingFunction.Ease)x != Util.EasingFunction.Ease.Instant && (bool)e["doMove"], new string[] { "startMoveX", "startMoveY" }),
                        }),
                    }
                },
                new GameAction("moveBirds", "Set Blue Birds Position")
                {
                    function = delegate { var e = eventCaller.currentEntity; BlueBirds.instance.SetBirdMovement(e); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("doMove", false, "Move", "Select this option if you want to move the Blue Birds.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "endMoveX", "endMoveY" } ),
                            new Param.CollapseParam((x, e) => (bool)x && (Util.EasingFunction.Ease)e["ease"] != Util.EasingFunction.Ease.Instant, new string[] { "startMoveX", "startMoveY" }),
                            new Param.CollapseParam((_, e) => (bool)e["doMove"], new string[] { "ease" })
                        }),
                        new Param("startMoveX", new EntityTypes.Float(-20f, 20f, 0f), "Start X", "Set the X position from which to move."),
                        new Param("startMoveY", new EntityTypes.Float(-20f, 20f, 0f), "Start Y", "Set the Y position from which to move."),
                        new Param("endMoveX", new EntityTypes.Float(-20f, 20f, 0f), "End X", "Set the X position to move to."),
                        new Param("endMoveY", new EntityTypes.Float(-20f, 20f, 0f), "End Y", "Set the Y position to move to."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing for the action.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, e) => (Util.EasingFunction.Ease)x != Util.EasingFunction.Ease.Instant && (bool)e["doMove"], new string[] { "startMoveX", "startMoveY" }),
                        }),
                    }
                },
                new GameAction("gradient", "Background Appearance")
                {
                    function = delegate { var e = eventCaller.currentEntity; BlueBirds.instance.BackgroundColor(e.beat, e.length,
                    e["startTop"], e["endTop"],
                    e["startBottom"], e["endBottom"],
                    e["ease"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("startTop", new Color(246 / 255f, 255 / 255f, 230 / 255f), "Gradient Top Start", "Set the color at the start of the event."),
                        new Param("endTop", new Color(246 / 255f, 255 / 255f, 230 / 255f), "Gradient Top End", "Set the color at the end of the event."),

                        new Param("startBottom", new Color(73 / 255f, 205 / 255f, 156 / 255f), "Gradient Bottom Start", "Set the color at the start of the event."),
                        new Param("endBottom", new Color(73 / 255f, 205 / 255f, 156 / 255f), "Gradient Bottom End", "Set the color at the end of the event."),

                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    },
                },
                new GameAction("pitching", "Enable Pitched Voices")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out BlueBirds instance)) {
                            instance.Pitching(e["enabled"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("enabled", false, "Enable Pitching", "Makes the Captains voice pitch up and down based on the song's tempo."),
                    },
                    defaultLength = 0.5f,
                },
            },
            new List<string>() { "ntr", "bird" },
            "ntrbluebirds", "en",
            new List<string>() { "en", "jp" },
            chronologicalSortKey: 7
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_BlueBirds;
    public class BlueBirds : Minigame
    {
        public static BlueBirds instance;

        public bool canBop = true;
        public bool canBopOthers = true;
        public bool canStretch = false;
        bool isSuccess = false;
        bool isStackedInputs = false;
        static float voicePitch;
        bool canPitch = false;
        public Animator captainAnim;
        public Animator captainHolderAnim;
        public Animator bird1Anim;
        public Animator bird2Anim;
        public Animator bird3Anim;

        public Animator effect1Anim;
        public Animator effect2Anim;
        public Animator effect3Anim;
        public Animator memoryAnim;
        public enum CaptainState
        {
            Exit,
            Enter,
        }
        public enum Memory
        {
            Part1,
            Part2,
            Part3,
            Part4,
            Part5,
            Part6,
        }

        public SpriteRenderer memorySprite;
        public List<Sprite> memoryImage = new List<Sprite>();
        public Material gradientMat;
        public GameObject finText;

        public Transform CaptainTransform;
        private bool CaptIsMoving = false;
        private CaptMoveArgs CurrentCaptMoveArgs;
        private Vector3 CaptBasePosition;

        private struct CaptMoveArgs
        {
            public Vector3 StartPosition;
            public Vector3 EndPosition;
            public double StartBeat;
            public double Length;
            public Util.EasingFunction.Ease Ease;

            public CaptMoveArgs(double startBeat, double length, Vector3 startPosition, Vector3 endPosition, int ease)
            {
                StartPosition = startPosition;
                EndPosition = endPosition;
                StartBeat = startBeat;
                Length = length;
                Ease = (Util.EasingFunction.Ease)ease;
            }
        }

        public Transform BirdTransform;
        private bool BirdIsMoving = false;
        private BirdMoveArgs CurrentBirdMoveArgs;
        private Vector3 BirdBasePosition;
        private struct BirdMoveArgs
        {
            public Vector3 StartPosition;
            public Vector3 EndPosition;
            public double StartBeat;
            public double Length;
            public Util.EasingFunction.Ease Ease;

            public BirdMoveArgs(double startBeat, double length, Vector3 startPosition, Vector3 endPosition, int ease)
            {
                StartPosition = startPosition;
                EndPosition = endPosition;
                StartBeat = startBeat;
                Length = length;
                Ease = (Util.EasingFunction.Ease)ease;
            }
        }

        void Awake()
        {
            CaptBasePosition = CaptainTransform.localPosition;
            BirdBasePosition = BirdTransform.localPosition;
            instance = this;
            SetupBopRegion("blueBirds", "bop", "auto");
        }

        public void Update()
        {
            var cond = Conductor.instance;
            double beat = conductor.unswungSongPositionInBeatsAsDouble;

            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            {
                bird3Anim.DoScaledAnimationAsync("Pick", 0.5f);
                effect3Anim.DoScaledAnimationAsync("Miss", 0.5f);
                SoundByte.PlayOneShot("miss");

                captainAnim.DoScaledAnimationAsync("Angry", 0.5f);

                ScoreMiss();
            }
            if (PlayerInput.GetIsAction(InputAction_BasicRelease) && PlayerInput.CurrentControlStyle == InputController.ControlStyles.Pad && !IsExpectingInputNow(InputAction_BasicRelease))
            {
                if (canStretch)
                {
                    bird3Anim.DoScaledAnimationAsync("Shout", 0.5f);
                    SoundByte.PlayOneShotGame("blueBirds/release");

                    captainAnim.DoScaledAnimationAsync("Angry", 0.5f);

                    canStretch = false;
                    ScoreMiss();
                }
                else
                {
                    if (isSuccess)
                    {
                        bird3Anim.DoScaledAnimationAsync("PickSuccessRelease", 0.5f);
                        isSuccess = false;
                    }
                    else
                    {
                        bird3Anim.DoScaledAnimationAsync("PickRelease", 0.5f);
                        isSuccess = false;
                    }
                }
            }
            if (PlayerInput.GetIsAction(InputAction_BasicRelease) && PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch)
            {
                if (isSuccess)
                {
                    bird3Anim.DoScaledAnimationAsync("PickSuccessRelease", 0.5f);
                    isSuccess = false;
                }
                else
                {
                    bird3Anim.DoScaledAnimationAsync("PickRelease", 0.5f);
                    isSuccess = false;
                }
            }
            if (PlayerInput.GetIsAction(InputAction_FlickRelease) && !IsExpectingInputNow(InputAction_FlickRelease) && PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch)
            {
                bird3Anim.DoScaledAnimationAsync("Shout", 0.5f);
                SoundByte.PlayOneShotGame("blueBirds/release");

                captainAnim.DoScaledAnimationAsync("Angry", 0.5f);

                canStretch = false;
                ScoreMiss();
            }

            if (CaptIsMoving)
            {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(CurrentCaptMoveArgs.StartBeat, CurrentCaptMoveArgs.Length);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(CurrentCaptMoveArgs.Ease);
                float newPos = func(0f, 1f, normalizedBeat);
                Vector3 diff = CurrentCaptMoveArgs.EndPosition - CurrentCaptMoveArgs.StartPosition;
                CaptainTransform.localPosition = CaptBasePosition + CurrentCaptMoveArgs.StartPosition + (diff * newPos);

                if (normalizedBeat >= 1f)
                {
                    CaptainTransform.localPosition = CaptBasePosition + CurrentCaptMoveArgs.EndPosition;
                    CaptIsMoving = false;
                }
            }
            if (BirdIsMoving)
            {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(CurrentBirdMoveArgs.StartBeat, CurrentBirdMoveArgs.Length);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(CurrentBirdMoveArgs.Ease);
                float newPos = func(0f, 1f, normalizedBeat);
                Vector3 diff = CurrentBirdMoveArgs.EndPosition - CurrentBirdMoveArgs.StartPosition;
                BirdTransform.localPosition = BirdBasePosition + CurrentBirdMoveArgs.StartPosition + (diff * newPos);

                if (normalizedBeat >= 1f)
                {
                    BirdTransform.localPosition = BirdBasePosition + CurrentBirdMoveArgs.EndPosition;
                    BirdIsMoving = false;
                }
            }

            BackgroundColorUpdate();
        }
        public override void OnPlay(double beat)
        {
            PersistColor(beat);
            PersistEvents(beat);
        }
        public override void OnGameSwitch(double beat)
        {
            PersistColor(beat);
            PersistEvents(beat);
        }

        public void PeckYourBeak(double beat)
        {
            voicePitch = canPitch ? (Conductor.instance.GetBpmAtBeat(beat) / 130) * Conductor.instance.TimelinePitch : 1;

            HandleStackedEvents(beat, 1);

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("blueBirds/peck", beat, voicePitch),
                new MultiSound.Sound("blueBirds/ur", beat + 0.5, voicePitch),
                new MultiSound.Sound("blueBirds/beak", beat + 1, voicePitch),
                new MultiSound.Sound("blueBirds/peck1", beat + 2),
                new MultiSound.Sound("blueBirds/peck1", beat + 2.5),
                new MultiSound.Sound("blueBirds/peck1", beat + 3),
            }, forcePlay: true);

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { captainAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
                new BeatAction.Action(beat + 0.5, delegate { captainAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
                new BeatAction.Action(beat + 1, delegate { captainAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
                new BeatAction.Action(beat + 2, delegate {
                    PlayEffectAnim(1);
                    bird1Anim.DoScaledAnimationAsync("PickSuccessAlt", 0.5f);
                    bird2Anim.DoScaledAnimationAsync("PickSuccessAlt", 0.5f);
                }),
                new BeatAction.Action(beat + 2.5, delegate {
                    PlayEffectAnim(1);
                    bird1Anim.DoScaledAnimationAsync("PickSuccessAlt", 0.5f);
                    bird2Anim.DoScaledAnimationAsync("PickSuccessAlt", 0.5f);
                }),
                new BeatAction.Action(beat + 3, delegate {
                    PlayEffectAnim(1);
                    bird1Anim.DoScaledAnimationAsync("PickSuccessAlt", 0.5f);
                    bird2Anim.DoScaledAnimationAsync("PickSuccessAlt", 0.5f);
                }),
            });

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1.75, delegate { isStackedInputs = true; }),
                new BeatAction.Action(beat + 2.90, delegate {
                    isStackedInputs = false;
                    canBop = true;
                    canBopOthers = true;
                }),
            });

            ScheduleInput(beat, 2, InputAction_BasicPress, JustPeck, Miss, null);
            ScheduleInput(beat, 2.5, InputAction_BasicPress, JustPeck, Miss, null);
            ScheduleInput(beat, 3, InputAction_BasicPress, JustPeck, Miss, null);
        }

        public void StretchOutYourNeck(double beat)
        {
            voicePitch = canPitch ? (Conductor.instance.GetBpmAtBeat(beat) / 130) * Conductor.instance.TimelinePitch : 1;

            HandleStackedEvents(beat, 2);

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("blueBirds/stretch", beat, voicePitch),
                new MultiSound.Sound("blueBirds/out", beat + 0.75, voicePitch),
                new MultiSound.Sound("blueBirds/your", beat + 1.50, voicePitch),
                new MultiSound.Sound("blueBirds/neck", beat + 2, voicePitch),
                new MultiSound.Sound("blueBirds/peck1", beat + 3),
                new MultiSound.Sound("blueBirds/flap", beat + 4),
            }, forcePlay: true);

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { captainAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
                new BeatAction.Action(beat + 0.75, delegate { captainAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
                new BeatAction.Action(beat + 1.50, delegate { captainAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
                new BeatAction.Action(beat + 2, delegate { captainAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
                new BeatAction.Action(beat + 3, delegate {
                    PlayEffectAnim(1);
                    bird1Anim.DoScaledAnimationAsync("TameSuccess", 0.5f);
                    bird2Anim.DoScaledAnimationAsync("TameSuccess", 0.5f);
                }),
                new BeatAction.Action(beat + 4, delegate {
                    bird1Anim.DoScaledAnimationAsync("Shout", 0.5f);
                    bird2Anim.DoScaledAnimationAsync("Shout", 0.5f);
                }),
            });

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 2.75, delegate { isStackedInputs = true; }),
                new BeatAction.Action(beat + 4.25, delegate {
                    isStackedInputs = false;
                    canBop = true;
                    canBopOthers = true;
                }),
            });

            ScheduleInput(beat, 3, InputAction_BasicPress, JustHold, Miss, null);
            ScheduleInput(beat, 4, InputAction_FlickRelease, JustShout, Miss, null);
        }

        public void JustPeck(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                bird3Anim.DoScaledAnimationAsync("Pick", 0.5f);
                SoundByte.PlayOneShot("miss");

                captainAnim.DoScaledAnimationAsync("Angry", 0.5f);
                effect3Anim.DoScaledAnimationAsync("Miss", 0.5f);

                isSuccess = false;
            }
            else
            {
                if (GameManager.instance.Autoplay)
                {
                    bird3Anim.DoScaledAnimationAsync("PickSuccessAlt", 0.5f);
                }
                else
                {
                    bird3Anim.DoScaledAnimationAsync("PickSuccess", 0.5f);
                }
                SoundByte.PlayOneShotGame("blueBirds/tap");

                effect3Anim.DoScaledAnimationAsync("Attack", 0.5f);

                isSuccess = true;
            }
        }

        public void Miss(PlayerActionEvent caller)
        {
            captainAnim.DoScaledAnimationAsync("Angry", 0.5f);
            isSuccess = false;
        }

        public void JustHold(PlayerActionEvent caller, float state)
        {
            var beat = caller.startBeat + caller.timer;

            if (state >= 1f || state <= -1f)
            {
                captainAnim.DoScaledAnimationAsync("Angry", 0.5f);
                bird3Anim.DoScaledAnimationAsync("Tame", 0.5f);

                effect3Anim.DoScaledAnimationAsync("Miss", 0.5f);
                SoundByte.PlayOneShot("miss");

                isSuccess = false;
            }
            else
            {
                bird3Anim.DoScaledAnimationAsync("TameSuccess", 0.5f);

                effect3Anim.DoScaledAnimationAsync("Attack", 0.5f);
                SoundByte.PlayOneShotGame("blueBirds/hold");

                isSuccess = true;
            }
        }

        public void JustShout(PlayerActionEvent caller, float state)
        {
            isSuccess = false;
            canStretch = false;

            if (state >= 1f || state <= -1f)
            {
                captainAnim.DoScaledAnimationAsync("Angry", 0.5f);

                bird3Anim.DoScaledAnimationAsync("Miss", 0.5f);
                SoundByte.PlayOneShot("miss");
            }
            else
            {
                captainAnim.DoScaledAnimationAsync("Smile", 0.5f);
                bird3Anim.DoScaledAnimationAsync("Shout", 0.5f);
                SoundByte.PlayOneShotGame("blueBirds/release");
            }
        }

        public void HandleStackedEvents(double beat, int inputType)
        {
            switch (inputType)
            {
                case 1:
                    if (!isStackedInputs)
                    {
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate {
                                bird1Anim.DoScaledAnimationAsync("PrePick01", 0.5f);
                                bird2Anim.DoScaledAnimationAsync("PrePick01", 0.5f);
                                bird3Anim.DoScaledAnimationAsync("PrePick01", 0.5f);
                            }),
                            new BeatAction.Action(beat + 1, delegate {
                                bird1Anim.DoScaledAnimationAsync("PrePick02", 0.5f);
                                bird2Anim.DoScaledAnimationAsync("PrePick02", 0.5f);
                                bird3Anim.DoScaledAnimationAsync("PrePick02", 0.5f);
                            }),
                        });
                    }
                    break;
                case 2:
                    if (!isStackedInputs)
                    {
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate {
                                bird1Anim.DoScaledAnimationAsync("PreShout01", 0.5f);
                            }),
                            new BeatAction.Action(beat + 0.75, delegate {
                                bird2Anim.DoScaledAnimationAsync("PreShout01", 0.5f);
                            }),
                            new BeatAction.Action(beat + 1.5, delegate {
                                bird3Anim.DoScaledAnimationAsync("PreShout01", 0.5f);
                            }),
                            new BeatAction.Action(beat + 2, delegate {
                                bird1Anim.DoScaledAnimationAsync("PreShout02", 0.5f);
                                bird2Anim.DoScaledAnimationAsync("PreShout02", 0.5f);
                                bird3Anim.DoScaledAnimationAsync("PreShout02", 0.5f);
                            }),
                        });
                    }
                    break;
            }
        }
        public void PlayEffectAnim(int anim)
        {
            switch (anim)
            {
                case 1:
                    effect1Anim.DoScaledAnimationAsync("Attack", 0.5f);
                    effect2Anim.DoScaledAnimationAsync("Attack", 0.5f);
                    break;
                case 2:
                    effect1Anim.DoScaledAnimationAsync("Miss", 0.5f);
                    effect2Anim.DoScaledAnimationAsync("Miss", 0.5f);
                    break;
            }   
        }
        public override void OnBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat))
            {
                if (canBop)
                {
                    if (!bird3Anim.IsPlayingAnimationNames("Pick", "PickRelease", "PickSuccess", "PickSuccessRelease", "PickSuccessAlt", "PrePick01", "PrePick02", "Tame", "TameLoop", "TameSuccess", "PreShout01", "PreShout02", "Shout", "Miss"))
                    {
                        bird3Anim.DoScaledAnimationAsync("Beat", 0.5f);
                    }
                }
                if (canBopOthers)
                {
                    if (!bird1Anim.IsPlayingAnimationNames("Pick", "PickRelease", "PickSuccess", "PickSuccessRelease", "PickSuccessAlt", "PrePick01", "PrePick02", "Tame", "TameLoop", "TameSuccess", "PreShout01", "PreShout02", "Shout", "Miss"))
                    {
                        bird1Anim.DoScaledAnimationAsync("Beat", 0.5f);
                    }
                    if (!bird2Anim.IsPlayingAnimationNames("Pick", "PickRelease", "PickSuccess", "PickSuccessRelease", "PickSuccessAlt", "PrePick01", "PrePick02", "Tame", "TameLoop", "TameSuccess", "PreShout01", "PreShout02", "Shout", "Miss"))
                    {
                        bird2Anim.DoScaledAnimationAsync("Beat", 0.5f);
                    }
                    if (!captainAnim.IsPlayingAnimationNames("Angry", "Smile", "Talk"))
                    {
                        captainAnim.DoScaledAnimationAsync("Beat", 0.5f);
                    }
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
                            bird1Anim.DoScaledAnimationAsync("Beat", 0.5f);
                            bird2Anim.DoScaledAnimationAsync("Beat", 0.5f);
                            bird3Anim.DoScaledAnimationAsync("Beat", 0.5f);
                            captainAnim.DoScaledAnimationAsync("Beat", 0.5f);
                        })
                    });
                }
            }
        }
        public void NoBopping(double beat)
        {
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 0.25, delegate {
                    canBop = false;
                    canBopOthers = false;
                }),
            });
        }

        public void Pitching(bool enabled)
        {
            canPitch = enabled;
        }

        public void ShowMemory(Memory memory, double beat, float length)
        {
            float actualLength = length + 1f - 2f;

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { memoryAnim.DoScaledAnimationAsync("fadeIn", 0.5f); }),
                new BeatAction.Action(beat + actualLength, delegate { memoryAnim.DoScaledAnimationAsync("fadeOut", 0.5f); }),
            });

            switch (memory)
            {
                case Memory.Part1:
                    memorySprite.sprite = memoryImage[0];
                    break;
                case Memory.Part2:
                    memorySprite.sprite = memoryImage[1];
                    break;
                case Memory.Part3:
                    memorySprite.sprite = memoryImage[2];
                    break;
                case Memory.Part4:
                    memorySprite.sprite = memoryImage[3];
                    break;
                case Memory.Part5:
                    memorySprite.sprite = memoryImage[4];
                    break;
                case Memory.Part6:
                    memorySprite.sprite = memoryImage[5];
                    break;
            }
        }

        public void ShowText(bool showtext)
        {
            if (showtext) finText.SetActive(true);
            else finText.SetActive(false);
        }
        public void CaptainBBState(CaptainState state)
        {
            switch (state)
            {
                case CaptainState.Exit:
                    captainHolderAnim.DoScaledAnimationAsync("MoveOut", 0.5f);
                    break;
                case CaptainState.Enter:
                    captainHolderAnim.DoScaledAnimationAsync("MoveIn", 0.5f);
                    break;
            }
        }
        public void SetCaptMovement(RiqEntity e)
        {
            if (e["doMove"])
            {
                CaptIsMoving = true;
                CurrentCaptMoveArgs = new CaptMoveArgs(
                    e.beat, e.length,
                    new Vector3(e["startMoveX"], e["startMoveY"], 0),
                    new Vector3(e["endMoveX"], e["endMoveY"], 0),
                    e["ease"]
                );
            }
        }
        public void SetBirdMovement(RiqEntity e)
        {
            if (e["doMove"])
            {
                BirdIsMoving = true;
                CurrentBirdMoveArgs = new BirdMoveArgs(
                    e.beat, e.length,
                    new Vector3(e["startMoveX"], e["startMoveY"], 0),
                    new Vector3(e["endMoveX"], e["endMoveY"], 0),
                    e["ease"]
                );
            }
        }
        private ColorEase gradTopEase = new(new Color(246 / 255f, 255 / 255f, 230 / 255f));
        private ColorEase gradBottomEase = new(new Color(73 / 255f, 205 / 255f, 156 / 255f));

        public void BackgroundColor(double beat, float length, Color startColor, Color endColor, Color startBottom, Color endBottom, int ease)
        {
            gradTopEase = new ColorEase(beat, length, startColor, endColor, ease);
            gradBottomEase = new ColorEase(beat, length, startBottom, endBottom, ease);
        }
        public void BackgroundColorUpdate()
        {
            gradientMat.SetColor("_ColorAlpha", gradBottomEase.GetColor());
            gradientMat.SetColor("_ColorDelta", gradTopEase.GetColor());
        }
        private void PersistEvents(double beat)
        {
            var allEvents = GameManager.instance.Beatmap.Entities.FindAll(e => e.datamodel.Split('/')[0] is "blueBirds");
            var eventsBefore = allEvents.FindAll(e => e.beat < beat);
            var lastPersistEvent = eventsBefore.FindLast(e => e.datamodel == "blueBirds/pitching");
            if (lastPersistEvent != null)
            {
                var e = lastPersistEvent;
                Pitching(e["enabled"]);
            }
        }
        private void PersistColor(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("blueBirds", new string[] { "gradient" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var e = allEventsBeforeBeat[^1];
                BackgroundColor(e.beat, e.length, e["startTop"], e["endTop"], e["startBottom"], e["endBottom"], e["ease"]);
            }
        }
    }
}