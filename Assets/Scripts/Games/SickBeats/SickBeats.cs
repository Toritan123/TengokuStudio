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

    public static class AgbSickBeats
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("sickBeats", "Sick Beats", "039303", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; SickBeats.instance.ToggleBop(e.beat, e.length, e["toggle2"], e["toggle"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle2", true, "Bop", "Toggle if the doctor should bop for the duration of this event."),
                        new Param("toggle", false, "Bop (Auto)", "Toggle if the doctor should automatically bop until another Bop event is reached.")
                    }
                },
                new GameAction("virus", "Move Virus")
                {
                    function = delegate { var e = eventCaller.currentEntity; SickBeats.instance.MoveVirus(e.beat, e["direction"], e["type"]); },
                    parameters = new List<Param>()
                    {
                        new Param("direction", SickBeats.Direction.Right, "Direction", "Determine which direction the virus will spawn from."),
                        new Param("type", SickBeats.VirusType.Blue, "Type", "Determine virus type."),
                    },
                    defaultLength = 2f,
                },
                new GameAction("appear", "Force Spawn Virus")
                {
                    function = delegate {var e = eventCaller.currentEntity; SickBeats.instance.VirusAppearMnl(e.beat, e["direction"], e["type"]); },
                    parameters = new List<Param>()
                    {
                        new Param("direction", SickBeats.Direction.Right, "Direction", "Determine which direction the virus will spawn from."),
                        new Param("type", SickBeats.VirusType.Blue, "Type", "Determine virus type."),
                    },
                    defaultLength = 2f,
                    advanced = true
                },
                new GameAction("dash", "Force Dash")
                {
                    function = delegate {var e = eventCaller.currentEntity; SickBeats.instance.VirusDashMnl(e.beat,
                        e["direction"], e["type"], new double[]{e["param1"], e["param2"], e["param3"]}); },
                    parameters = new List<Param>()
                    {
                        new Param("direction", SickBeats.Direction.Up, "Direction", "Determine which direction the virus will spawn from."),
                        new Param("type", SickBeats.VirusType.Blue, "Type", "Determine virus type."),
                        new Param("param1", new EntityTypes.Float(0, 1, 0), "Right Beat", "Decide the right Dash beat."),
                        new Param("param2", new EntityTypes.Float(0, 1, 0.125f), "Up Beat", "Decide the up Dash beat."),
                        new Param("param3", new EntityTypes.Float(0, 1, 0.25f), "Left Beat", "Decide the left Dash beat."),
                    },
                    defaultLength = 1f,
                    advanced = true
                },
                new GameAction("summon", "Force Virus Appear")
                {
                    function = delegate {var e = eventCaller.currentEntity; SickBeats.instance.VirusSummonMnl(e.beat, e["type"]); },
                    parameters = new List<Param>()
                    {
                        new Param("type", SickBeats.VirusType.Blue, "Type", "Determine virus type."),
                    },
                    defaultLength = 2f,
                    advanced = true
                },
                new GameAction("BGApp", "Background Appearance")
                {
                    function = delegate { var e = eventCaller.currentEntity; SickBeats.instance.BackgroundColor(e.beat, e.length, e["start"], e["end"], e["startPipe"], e["endPipe"], e["startOutline"], e["endOutline"], e["startCorner"], e["endCorner"], e["ease"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("start",          SickBeats.defaultBGColor,           "Start Color", "Set the color at the start of the event."),
                        new Param("end",            SickBeats.defaultBGColor,           "End Color", "Set the color at the end of the event."),
                        new Param("startPipe",      SickBeats.defaultPipeColor,         "Start Color (Pipes)", "Set the color at the start of the event."),
                        new Param("endPipe",        SickBeats.defaultPipeColor,         "End Color (Pipes)", "Set the color at the end of the event."),
                        new Param("startOutline",   SickBeats.defaultOutlineColor,      "Start Color (Outline)", "Set the color at the start of the event."),
                        new Param("endOutline",     SickBeats.defaultOutlineColor,      "End Color (Outline)", "Set the color at the end of the event."),
                        new Param("startCorner",    SickBeats.defaultCornerColor,       "Start Color (Corners)", "Set the color at the start of the event."),
                        new Param("endCorner",      SickBeats.defaultCornerColor,       "End Color (Corners)", "Set the color at the end of the event."),
                        new Param("ease",           Util.EasingFunction.Ease.Linear,    "Ease", "Set the easing of the action.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x != (int)Util.EasingFunction.Ease.Instant, new string[] { "start", "startPipe", "startOutline", "startCorner" }),
                        }),      
                    },
                },
                new GameAction("virusColor", "Change Virus Color")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        SickBeats.instance.UpdateMaterialColor(e["colorVirus1"], e["colorVirus2"], e["colorVirus3"], e["colorVirus4"]);
                    },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("colorVirus1", new Color(0f, 1f, 1f), "1st Color", "Set the color of the first virus."),
                        new Param("colorVirus2", new Color(1f, 0.25f, 0.75f), "2nd Color", "Set the color of the second virus."),
                        new Param("colorVirus3", new Color(0f, 0f, 0f), "3rd Color", "Set the color of the third virus."),
                        new Param("colorVirus4", new Color(1f, 1f, 1f), "4th Color", "Set the color of the fourth virus."),
                    }
                },
                new GameAction("moveDoctor", "Move Doctor")
                {
                    function = delegate { var e = eventCaller.currentEntity; SickBeats.instance.SetDoctorMovement(e); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("doMove", false, "Move", "Select this option if you want to move the Doctor.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "endMoveX", "endMoveY" } ),
                            new Param.CollapseParam((x, e) => (bool)x && (Util.EasingFunction.Ease)e["ease"] != Util.EasingFunction.Ease.Instant, new string[] { "startMoveX", "startMoveY" }),
                            new Param.CollapseParam((_, e) => (bool)e["doMove"] || (bool)e["doRotate"] || (bool)e["doScale"], new string[] { "ease" })
                        }),
                        new Param("startMoveX", new EntityTypes.Float(-20f, 20f, 0f), "Start X", "Set the X position from which to move."),
                        new Param("startMoveY", new EntityTypes.Float(-20f, 20f, 0f), "Start Y", "Set the Y position from which to move."),
                        new Param("endMoveX", new EntityTypes.Float(-20f, 20f, 0f), "End X", "Set the X position to move to."),
                        new Param("endMoveY", new EntityTypes.Float(-20f, 20f, 0f), "End Y", "Set the Y position to move to."),
                        new Param("doRotate", false, "Rotate", "Select this option if you want to the Doctor.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "endRotDegrees" } ),
                            new Param.CollapseParam((x, e) => (bool)x && (Util.EasingFunction.Ease)e["ease"] != Util.EasingFunction.Ease.Instant, new string[] { "startRotDegrees" }),
                            new Param.CollapseParam((_, e) => (bool)e["doMove"] || (bool)e["doRotate"] || (bool)e["doScale"], new string[] { "ease" })
                        }),
                        new Param("startRotDegrees", new EntityTypes.Float(-360f, 360f, 0f), "Start Rotation", "Set the number of degrees at which to begin rotating."),
                        new Param("endRotDegrees", new EntityTypes.Float(-360f, 360f, 0f), "End Rotation", "Set the number of degrees at which to finish rotating."),
                        new Param("doScale", false, "Scale", "Select this option if you want to change the Doctor's scale.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "endScaleX", "endScaleY" } ),
                            new Param.CollapseParam((x, e) => (bool)x && (Util.EasingFunction.Ease)e["ease"] != Util.EasingFunction.Ease.Instant, new string[] { "startScaleX", "startScaleY" }),
                            new Param.CollapseParam((_, e) => (bool)e["doMove"] || (bool)e["doRotate"] || (bool)e["doScale"], new string[] { "ease" })
                        }),
                        new Param("startScaleX", new EntityTypes.Float(-5f, 5f, 1f), "Start Scale X", "Set the desired scale on the X axis at which to start."),
                        new Param("startScaleY", new EntityTypes.Float(-5f, 5f, 1f), "Start Scale Y", "Set the desired scale on the Y axis at which to start."),
                        new Param("endScaleX", new EntityTypes.Float(-5f, 5f, 1f), "End Scale X", "Set the desired scale on the X axis at which to end."),
                        new Param("endScaleY", new EntityTypes.Float(-5f, 5f, 1f), "End Scale Y", "Set the desired scale on the Y axis at which to end."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing for the action.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, e) => (Util.EasingFunction.Ease)x != Util.EasingFunction.Ease.Instant && (bool)e["doMove"], new string[] { "startMoveX", "startMoveY" }),
                            new Param.CollapseParam((x, e) => (Util.EasingFunction.Ease)x != Util.EasingFunction.Ease.Instant && (bool)e["doRotate"], new string[] { "startRotDegrees" }),
                            new Param.CollapseParam((x, e) => (Util.EasingFunction.Ease)x != Util.EasingFunction.Ease.Instant && (bool)e["doScale"], new string[] { "startScaleX", "startScaleY" }),
                        }),
                        new Param("sticky", false, "Follow Camera", "Select this to make the Doctor follow the camera."),
                    }
                }
            },
            new List<string>() { "agb", "normal" }, "agbvirus", "en", new List<string>() { },
            chronologicalSortKey: 10,
            font : "FOT-SlumpStd-DB SDF"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_SickBeats;

    public class SickBeats : Minigame
    {
        [Header("References")]
        public Animator keyAnim;
        public Animator[] forkAnims;
        public Animator doctorAnim;
        public Animator radioAnim;
        public Animator orgAnim;
        public GameObject baseVirus;
        public Transform virusHolder;

        [Header("Colorable")]
        public Material[] RecolorMats;
        public Color[] color = {new Color(0f, 1f, 1f), new Color(1f, 0.25f, 0.75f),
                                new Color(0f, 0f, 0f), new Color(1f, 1f, 1f)};

        [Header("BG")]
        public SpriteRenderer BGSquare;
            [Header("Pipes")]
            public SpriteRenderer PipeRight;
            public SpriteRenderer PipeUp;
            public SpriteRenderer PipeLeft;
            public SpriteRenderer PipeDown;
            public SpriteRenderer PipeEnd;
            [Header("Outlines")]
            public SpriteRenderer OutlineRight;
            public SpriteRenderer OutlineUp;
            public SpriteRenderer OutlineLeft;
            public SpriteRenderer OutlineDown;
            public SpriteRenderer OutlineEnd;
            [Header("Corners")]
            public SpriteRenderer CornerTopRight;
            public SpriteRenderer CornerTopLeft;
            public SpriteRenderer CornerBottomLeft;
            public SpriteRenderer CornerBottomRight;

        [Header("Doctor")]
        [SerializeField] Transform DoctorTransform;
        private bool _DoctorIsMoving = false;
        private DoctorMoveArgs _CurrentDoctorMoveArgs;
        private bool _DoctorIsRotating = false;
        private DoctorRotateArgs _CurrentDoctorRotateArgs;
        private bool _DoctorIsScaling = false;
        private DoctorScaleArgs _CurrentDoctorScaleArgs;

        private Vector3 _DoctorBasePosition;
        private Vector3 _DoctorBaseEulerAngles;
        private Vector3 _DoctorBaseScale;

        private ColorEase bgColorEase = new(defaultBGColor);
        private ColorEase pipeColorEase = new(defaultPipeColor);
        private ColorEase outlineColorEase = new(defaultOutlineColor);
        private ColorEase cornerColorEase = new(defaultCornerColor);

        [SerializeField] StickyCanvas _StickyCanvas;

        [Header("Variables")]
        [SerializeField] double _refillBeat;
        public double RefillBeat 
        {
            get => _refillBeat;
            private set => _refillBeat = value;
        }

        [Serializable]
        public struct DashPatternItem
        {
            public double[] beat;
        }
        [SerializeField] DashPatternItem[] DashPatterns;

        [System.NonSerialized] public bool[] isForkPop = {true, true, true, true};
        [System.NonSerialized] public bool[] isMiss = {false, false, false, false};
        [System.NonSerialized] public bool[] isPrepare = {false, false, false, false};
        [System.NonSerialized] public bool orgAlive = true;
        [System.NonSerialized] public bool docShock = false;
        [System.NonSerialized] public double docShockBeat = Double.MinValue;

        public static Color defaultBGColor = new(0 / 255f, 149 / 255f, 0 / 255f);
        public static Color defaultPipeColor = new(43 / 255f, 255 / 255f, 0 / 255f);
        public static Color defaultOutlineColor = new(33 / 255f, 255 / 255f, 0 / 255f);
        public static Color defaultCornerColor = new(52 / 255f, 255 / 255f, 0 / 255f);

        public enum Direction
        {
            Right,
            Up,
            Left,
            Down,
        }
        public enum VirusType
        {
            Blue,
            Pink,
            Black,
            Custom,
        }

        public static SickBeats instance;

        const int IA_RightPress = IAMAXCAT;
        const int IA_UpPress = IAMAXCAT + 1;
        const int IA_LeftPress = IAMAXCAT + 2;
        const int IA_DownPress = IAMAXCAT + 3;

        protected static bool IA_PadRight(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.Right, out dt);
        }
        protected static bool IA_BatonRight(out double dt)
        {
            return PlayerInput.GetBatonDown(InputController.ActionsBaton.East, out dt);
        }
        
        protected static bool IA_PadUp(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.Up, out dt);
        }
        protected static bool IA_BatonUp(out double dt)
        {
            return PlayerInput.GetBatonDown(InputController.ActionsBaton.North, out dt);
        }
        
        protected static bool IA_PadLeft(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt);
        }
        protected static bool IA_BatonLeft(out double dt)
        {
            return PlayerInput.GetBatonDown(InputController.ActionsBaton.West, out dt);
        }
        
        protected static bool IA_PadDown(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.Down, out dt);
        }
        protected static bool IA_BatonDown(out double dt)
        {
            return PlayerInput.GetBatonDown(InputController.ActionsBaton.South, out dt);
        }

        public static PlayerInput.InputAction InputAction_Right =
            new("AgbSickBeatsRight", new int[] { IA_RightPress, IA_RightPress, IA_RightPress },
            IA_PadRight, IA_TouchFlick, IA_BatonRight);
        public static PlayerInput.InputAction InputAction_Up =
            new("AgbSickBeatsUp", new int[] { IA_UpPress, IA_UpPress, IA_UpPress },
            IA_PadUp, IA_TouchFlick, IA_BatonUp);
        public static PlayerInput.InputAction InputAction_Left =
            new("AgbSickBeatsLeft", new int[] { IA_LeftPress, IA_LeftPress, IA_LeftPress },
            IA_PadLeft, IA_TouchFlick, IA_BatonLeft);
        public static PlayerInput.InputAction InputAction_Down =
            new("AgbSickBeatsDown", new int[] { IA_DownPress, IA_DownPress, IA_DownPress },
            IA_PadDown, IA_TouchFlick, IA_BatonDown);

        public PlayerActionEvent ScheduleMissableInput(double startBeat,
            double timer,
            PlayerInput.InputAction inputAction,
            PlayerActionEvent.ActionEventCallbackState OnHit,
            PlayerActionEvent.ActionEventCallback OnMiss,
            PlayerActionEvent.ActionEventCallback OnBlank,
            PlayerActionEvent.ActionEventHittableQuery HittableQuery = null)
        {
            PlayerActionEvent evt = ScheduleInput(startBeat, timer, inputAction, OnHit, OnMiss, OnBlank, HittableQuery);
            evt.missable = true;
            return evt;
        }

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
            SetupBopRegion("sickBeats", "bop", "toggle");
            UpdateMaterialColor(color[0], color[1], color[2], color[3]);
            _DoctorBasePosition = DoctorTransform.localPosition;
            _DoctorBaseEulerAngles = DoctorTransform.localEulerAngles;
            _DoctorBaseScale = DoctorTransform.localScale;
        }

        public override void OnBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat)) Bop(beat);
        }

        [NonSerialized] public double gameEndBeat = double.MaxValue;
        public override void OnGameSwitch(double beat)
        {
            PersistColor(beat);
            PersistBGColor(beat);
            var entities = GameManager.instance.Beatmap.Entities;
            // find out when the next game switch (or remix end) happens
            RiqEntity firstEnd = entities.Find(c => (c.datamodel.StartsWith("gameManager/switchGame") || c.datamodel.Equals("gameManager/end")) && c.beat > beat);
            gameEndBeat = firstEnd?.beat ?? double.MaxValue;
        }

        public override void OnPlay(double beat)
        {
            OnGameSwitch(beat);
        }

        void Update()
        {            
            var cond = Conductor.instance;
            if (!cond.isPlaying || cond.isPaused) return;

            if (PlayerInput.PlayerHasControl() && PlayerInput.CurrentControlStyle is InputSystem.InputController.ControlStyles.Touch)
            {
                if (PlayerInput.GetIsAction(InputAction_BasicPress))
                {
                    keyAnim.Play("keep");
                }
                if (PlayerInput.GetIsAction(InputAction_BasicRelease))
                {
                    keyAnim.Play("up");
                }

                if (PlayerInput.GetIsAction(InputAction_FlickPress) && !IsExpectingInputNow(InputAction_FlickPress))
                {
                    var rand_dir = ChooseDirection(isMiss, isPrepare, isForkPop);
                    if (isForkPop[rand_dir]) OutFork(rand_dir);
                }
            }
            else
            {
                if (PlayerInput.GetIsAction(InputAction_Right) && !IsExpectingInputNow(InputAction_Right))
                {
                    if (isForkPop[(int)Direction.Right]) OutFork((int)Direction.Right);
                }
                if (PlayerInput.GetIsAction(InputAction_Up) && !IsExpectingInputNow(InputAction_Up))
                {
                    if (isForkPop[(int)Direction.Up]) OutFork((int)Direction.Up);
                }
                if (PlayerInput.GetIsAction(InputAction_Left) && !IsExpectingInputNow(InputAction_Left))
                {
                    if (isForkPop[(int)Direction.Left]) OutFork((int)Direction.Left);
                }
                if (PlayerInput.GetIsAction(InputAction_Down) && !IsExpectingInputNow(InputAction_Down))
                {
                    if (isForkPop[(int)Direction.Down]) OutFork((int)Direction.Down);
                }
            }

            // shamelessly stolen from cotd's code :smide:
            if (_DoctorIsMoving)
            {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(_CurrentDoctorMoveArgs.StartBeat, _CurrentDoctorMoveArgs.Length);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(_CurrentDoctorMoveArgs.Ease);
                float newPos = func(0f, 1f, normalizedBeat);
                Vector3 diff = _CurrentDoctorMoveArgs.EndPosition - _CurrentDoctorMoveArgs.StartPosition;
                DoctorTransform.localPosition = _DoctorBasePosition + _CurrentDoctorMoveArgs.StartPosition + (diff * newPos);

                if (normalizedBeat >= 1f)
                {
                    DoctorTransform.localPosition = _DoctorBasePosition + _CurrentDoctorMoveArgs.EndPosition;
                    _DoctorIsMoving = false;
                }
            }
            if (_DoctorIsRotating)
            {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(_CurrentDoctorRotateArgs.StartBeat, _CurrentDoctorRotateArgs.Length);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(_CurrentDoctorRotateArgs.Ease);
                float newPos = func(0f, 1f, normalizedBeat);
                float diff = _CurrentDoctorRotateArgs.EndRotation - _CurrentDoctorRotateArgs.StartRotation;
                DoctorTransform.localEulerAngles = _DoctorBaseEulerAngles + new Vector3(0, 0, _CurrentDoctorRotateArgs.StartRotation + (diff * newPos));

                if (normalizedBeat >= 1f)
                {
                    DoctorTransform.localEulerAngles = _DoctorBaseEulerAngles + new Vector3(0, 0, _CurrentDoctorRotateArgs.EndRotation);
                    _DoctorIsRotating = false;
                }
            }
            if (_DoctorIsScaling)
            {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(_CurrentDoctorScaleArgs.StartBeat, _CurrentDoctorScaleArgs.Length);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(_CurrentDoctorScaleArgs.Ease);
                float newPos = func(0f, 1f, normalizedBeat);
                Vector3 diff = _CurrentDoctorScaleArgs.EndScale - _CurrentDoctorScaleArgs.StartScale;
                DoctorTransform.localScale = new Vector3
                (
                    _DoctorBaseScale.x * (_CurrentDoctorScaleArgs.StartScale.x + (diff.x * newPos)),
                    _DoctorBaseScale.y * (_CurrentDoctorScaleArgs.StartScale.y + (diff.y * newPos)),
                    0
                );

                if (normalizedBeat >= 1f)
                {
                    DoctorTransform.localScale = new Vector3
                    (
                        _DoctorBaseScale.x * _CurrentDoctorScaleArgs.EndScale.x,
                        _DoctorBaseScale.y * _CurrentDoctorScaleArgs.EndScale.y,
                        0
                    );
                    _DoctorIsScaling = false;
                }
            }
            BackgroundColorUpdate();
        }

        private void OutFork(int dir)
        {
            var currentBeat = Conductor.instance.songPositionInBeatsAsDouble;

            var actions = new List<BeatAction.Action>();
            keyAnim.Play("push");
            forkAnims[dir].Play("out");
            SoundByte.PlayOneShotGame("sickBeats/fork"+UnityEngine.Random.Range(0, 3).ToString());
            BeatAction.New(instance, new() {new BeatAction.Action(currentBeat + RefillBeat, delegate {RepopFork(dir);})});

            isForkPop[dir] = false;
        }
        public void RepopFork(int dir)
        {
            forkAnims[dir].Play("repop");

            isForkPop[dir] = true;
        }

        public void ToggleBop(double beat, float length, bool bopOrNah, bool autoBop)
        {
            if (bopOrNah)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance, new() {new BeatAction.Action(beat + i, delegate {Bop(beat);}) });
                }
            }
        }

        public void Bop(double beat)
        {
            radioAnim.DoScaledAnimationAsync("bop", 0.5f);
            if (beat < docShockBeat || beat > docShockBeat + 2) doctorAnim.DoScaledAnimationAsync("bop", 0.5f);
            if (orgAlive) orgAnim.DoScaledAnimationAsync("bop", 0.5f);
        }

        public Virus SpawnVirus(double beat, int dir, int type)
        {
            var newVirus = Instantiate(baseVirus, virusHolder).GetComponent<Virus>();
            newVirus.startBeat = beat;
            newVirus.position = dir;
            newVirus.life = type;
            newVirus.gameObject.SetActive(true);
            newVirus.Init();

            return newVirus;
        }

        public void MoveVirus(double beat, int dir, int type)
        {
            var newVirus = SpawnVirus(beat, -1, type);

            var actions = new List<BeatAction.Action>();
            
            actions.Add(new BeatAction.Action(beat, delegate {
                newVirus.Summon();
                newVirus.position++;
            }));

            switch (dir)
            {
                case (int)Direction.Right:
                    newVirus.startBeat = beat + 2;
                    actions.Add(new BeatAction.Action(beat + 2, delegate {newVirus.Appear();}));
                    break;
                case (int)Direction.Up:
                case (int)Direction.Left:
                case (int)Direction.Down:
                    for(int i = 0; i < dir; i++)
                    {
                        actions.Add(new BeatAction.Action(beat + DashPatterns[dir-1].beat[i], delegate {
                            newVirus.Dash();
                            newVirus.position++;
                        }));
                    }
                    newVirus.startBeat = beat + 4;
                    actions.Add(new BeatAction.Action(beat + 4, delegate {newVirus.Appear();}));
                    break;
            }

            BeatAction.New(instance, actions);
            
        }

        public void VirusAppearMnl(double beat, int dir, int type)
        {
            var newVirus = SpawnVirus(beat, dir, type);
            newVirus.Appear();
        }

        public void VirusDashMnl(double beat, int dir, int type, double[] dashbeats)
        {
            if (dir<1) dir = 1;
            var newVirus = SpawnVirus(beat, 0, type);
            
            var actions = new List<BeatAction.Action>();

            for(int i = 0; i < dir; i++)
            {
                actions.Add(new BeatAction.Action(beat + dashbeats[i], delegate {
                    newVirus.Dash();
                    newVirus.position++;
                }));
            }
            actions.Add(new BeatAction.Action(beat + 2, delegate {Destroy(newVirus.gameObject);}));
            BeatAction.New(instance, actions);
        }

        public void VirusSummonMnl(double beat, int type)
        {
            var newVirus = SpawnVirus(beat, -1, type);

            var actions = new List<BeatAction.Action>();
            
            actions.Add(new BeatAction.Action(beat, delegate {
                newVirus.Summon();
                newVirus.position++;
            }));
            actions.Add(new BeatAction.Action(beat + 2, delegate {Destroy(newVirus.gameObject);}));
            BeatAction.New(instance, actions);
        }

        public void BackgroundColor(double beat, float length, Color startColorBG, Color endColorBG, Color startColorPipe, Color endColorPipe, Color startColorOutline, Color endColorOutline, Color startColorCorner, Color endColorCorner, int ease)
        {
            bgColorEase = new ColorEase(beat, length, startColorBG, endColorBG, ease);
            pipeColorEase = new ColorEase(beat, length, startColorPipe, endColorPipe, ease);
            outlineColorEase = new ColorEase(beat, length, startColorOutline, endColorOutline, ease) ;
            cornerColorEase = new ColorEase(beat, length, startColorCorner, endColorCorner, ease);
        }
        private void BackgroundColorUpdate()
        {
            //this feels like theres some better way to do this butttttt im lazy
            BGSquare.color = bgColorEase.GetColor();

            PipeRight.color = pipeColorEase.GetColor();
            PipeUp.color = pipeColorEase.GetColor();
            PipeLeft.color = pipeColorEase.GetColor();
            PipeDown.color = pipeColorEase.GetColor();
            PipeEnd.color = pipeColorEase.GetColor();

            OutlineRight.color = outlineColorEase.GetColor();
            OutlineUp.color = outlineColorEase.GetColor();
            OutlineLeft.color = outlineColorEase.GetColor();
            OutlineDown.color = outlineColorEase.GetColor();
            OutlineEnd.color = outlineColorEase.GetColor();

            CornerTopRight.color = cornerColorEase.GetColor();
            CornerTopLeft.color = cornerColorEase.GetColor();
            CornerBottomLeft.color = cornerColorEase.GetColor();
            CornerBottomRight.color = cornerColorEase.GetColor();
        }
        public void UpdateMaterialColor(Color virus1, Color virus2, Color virus3, Color virus4)
        {
            color[0] = virus1; color[1] = virus2; color[2] = virus3; color[3] = virus4; 

            Recolor(0, new Color(0.75f, 0f, 0f), virus2, virus1);
            Recolor(1, new Color(0.75f, 0f, 0f), virus3, virus2);
            Recolor(2, new Color(0.75f, 0f, 0f), virus4, virus3);
            Recolor(3, new Color(0.75f, 0f, 0f), virus4, virus4);

            void Recolor(int i, Color color1, Color color2, Color color3)
            {
                RecolorMats[i].SetColor("_ColorAlpha", color1);
                RecolorMats[i].SetColor("_ColorBravo", color2);
                RecolorMats[i].SetColor("_ColorDelta", color3);
            }
        }
        private void PersistBGColor(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("sickBeats", new string[] { "BGApp" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                BackgroundColor(lastEvent.beat, lastEvent.length, lastEvent["start"], lastEvent["end"], lastEvent["startPipe"], lastEvent["endPipe"], lastEvent["startOutline"], lastEvent["endOutline"], lastEvent["startCorner"], lastEvent["endCorner"], lastEvent["ease"]);
            }
        }
        public static int ChooseDirection(bool[] misses, bool[] preparing, bool[] isForkPop)
        {
            var missedDirections = Enumerable.Range(0, 4)
                                            .Where(i => misses[i] && isForkPop[i])
                                            .ToList();
            if (missedDirections.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, missedDirections.Count);
                return missedDirections[index];
            }

            var preparingDirections = Enumerable.Range(0, 4)
                                                .Where(i => preparing[i] && isForkPop[i])
                                                .ToList();
            if (preparingDirections.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, preparingDirections.Count);
                return preparingDirections[index];
            }

            var remainingDirections = Enumerable.Range(0, 4)
                                                .Where(i => isForkPop[i])
                                                .ToList();
            if (remainingDirections.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, remainingDirections.Count);
                return remainingDirections[index];
            }

            return UnityEngine.Random.Range(0, 4);
        }


        public void SetDoctorMovement(RiqEntity e)
        {
            if (e["doMove"])
            {
                _DoctorIsMoving = true;
                _CurrentDoctorMoveArgs = new DoctorMoveArgs(
                    e.beat, e.length,
                    new Vector3(e["startMoveX"], e["startMoveY"], 0),
                    new Vector3(e["endMoveX"], e["endMoveY"], 0),
                    e["ease"]
                );
            }
            if (e["doRotate"])
            {
                _DoctorIsRotating = true;
                _CurrentDoctorRotateArgs = new DoctorRotateArgs(
                    e.beat, e.length,
                    e["startRotDegrees"],
                    e["endRotDegrees"],
                    e["ease"]
                );
            }
            if (e["doScale"])
            {
                _DoctorIsScaling = true;
                _CurrentDoctorScaleArgs = new DoctorScaleArgs(
                    e.beat, e.length,
                    new Vector3(e["startScaleX"], e["startScaleY"], 1),
                    new Vector3(e["endScaleX"], e["endScaleY"], 1),
                    e["ease"]
                );
            }
            _StickyCanvas.Sticky = (bool)e["sticky"];
        }

        private void PersistColor(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("sickBeats", new string[] { "virusColor" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                UpdateMaterialColor(lastEvent["colorVirus1"], lastEvent["colorVirus2"], lastEvent["colorVirus3"], lastEvent["colorVirus4"]);
            }
        }

        private struct DoctorMoveArgs
        {
            public Vector3 StartPosition;
            public Vector3 EndPosition;
            public double StartBeat;
            public double Length;
            public Util.EasingFunction.Ease Ease;

            public DoctorMoveArgs(double startBeat, double length, Vector3 startPosition, Vector3 endPosition, int ease)
            {
                StartPosition = startPosition;
                EndPosition = endPosition;
                StartBeat = startBeat;
                Length = length;
                Ease = (Util.EasingFunction.Ease)ease;
            }
        }
        private struct DoctorRotateArgs
        {
            public float StartRotation;
            public float EndRotation;
            public double StartBeat;
            public double Length;
            public Util.EasingFunction.Ease Ease;

            public DoctorRotateArgs(double startBeat, double length, float startRotation, float endRotation, int ease)
            {
                StartRotation = startRotation;
                EndRotation = endRotation;
                StartBeat = startBeat;
                Length = length;
                Ease = (Util.EasingFunction.Ease)ease;
            }
        }
        private struct DoctorScaleArgs
        {
            public Vector3 StartScale;
            public Vector3 EndScale;
            public double StartBeat;
            public double Length;
            public Util.EasingFunction.Ease Ease;

            public DoctorScaleArgs(double startBeat, double length, Vector3 startScale, Vector3 endScale, int ease)
            {
                StartScale = startScale;
                EndScale = endScale;
                StartBeat = startBeat;
                Length = length;
                Ease = (Util.EasingFunction.Ease)ease;
            }
        }
    }
}