using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;
using HeavenStudio.Games.Scripts_ScrewbotFactory;
using TMPro;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlRobotLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("screwbotFactory", "Screwbot Factory", "ffffff", false, false, new List<GameAction>()
            {
                new GameAction("botGray", "Gray Screwbot")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity; 
                        ScrewbotFactory.RobotSfx(e.beat, e.length, (int)ScrewbotFactory.RobotType.Gray);
                        ScrewbotFactory.instance.RobotBlock(e.beat, e.length, (int)ScrewbotFactory.RobotType.Gray);
                    },
                    preFunctionLength = 1f,
                    defaultLength = 2f,
                    resizable = true
                },
                new GameAction("botWhite", "White Screwbot")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity; 
                        ScrewbotFactory.RobotSfx(e.beat, e.length, (int)ScrewbotFactory.RobotType.White);
                        ScrewbotFactory.instance.RobotBlock(e.beat, e.length, (int)ScrewbotFactory.RobotType.White);
                    },
                    preFunctionLength = 1f,
                    defaultLength = 2f,
                    resizable = true
                },
                /* It's broken...
                new GameAction("craneText", "Set Crane Text")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        ScrewbotFactory.instance.SetCraneText(e["text"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("text", "安全第一", "Crane Text", "Sets the text written on the crane."),
                    },
                },
                */
            },
            new List<string>() {"rvl", "normal"},
            "rvlrobot", "en",
            new List<string>() { "en", "jp" },
            chronologicalSortKey: 2,
            font: "Slump"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    public class ScrewbotFactory : Minigame
    {
        [Header("Components")]
        [SerializeField] Animator beltAnim;
        [SerializeField] Animator screwAnim;
        [SerializeField] Animator craneAnim;
        public Animator clawAnim;
        public Animator clawHeadW;
        public Animator clawHeadG;
        [SerializeField] Animator craneHeadG;
        [SerializeField] Animator craneHeadW;

        [SerializeField] Transform baseG;
        [SerializeField] Transform baseW;

        [SerializeField] GameObject robotGray;
        [SerializeField] GameObject robotWhite;

        [SerializeField] TMP_Text craneText;

        [Header("Variables")]
        private bool craneCanAnimate = true;
        private double lastCueBeat = Double.MinValue;
        private float lastCueLength = 0;
        private bool hasStartedConveyor = false;
        public bool pauseConveyor = false;
        public bool shouldBeHolding = false;
        public bool clawBusy = false;
        public Robot currentRobot;

        public enum RobotType
        {
            White,
            Gray
        }

        public static ScrewbotFactory instance;
        private Conductor cond;
        
        protected static bool IA_PadLPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.ButtonL, out dt);
        }
        protected static bool IA_PadRPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.ButtonR, out dt);
        }

        protected static bool IA_BatonAltPress(out double dt)
        {
            return PlayerInput.GetBatonDown(InputController.ActionsBaton.Trigger, out dt);
        }

        protected static bool IA_BatonPress(out double dt)
        {
            return PlayerInput.GetSqueezeDown(out dt);
        }
        protected static bool IA_BatonRelease(out double dt)
        {
            return PlayerInput.GetSqueezeUp(out dt);
        }

        public static PlayerInput.InputAction InputAction_RobotPress =
            new("RvlRobotPress", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_PadBasicPress, IA_TouchBasicPress, IA_BatonPress);
        public static PlayerInput.InputAction InputAction_RobotRelease =
            new("RvlRobotRelease", new int[] { IAReleaseCat, IAReleaseCat, IAReleaseCat },
            IA_PadBasicRelease, IA_TouchBasicRelease, IA_BatonRelease);

        public static PlayerInput.InputAction InputAction_RobotLeft =
            new("RvlRobotLeft", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_PadLPress, null, IA_BatonBasicPress);
        public static PlayerInput.InputAction InputAction_RobotRight =
            new("RvlRobotRight", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_PadRPress, null, IA_BatonAltPress);

        
        public void Start()
        {
            instance = this;
            cond = Conductor.instance;
            if (robotWhite != null) robotWhite.SetActive(false);
            if (robotGray != null) robotGray.SetActive(false);

            if (beltAnim != null) beltAnim.DoScaledAnimationAsync("robot_belt_loop", 0.5f);
            if (screwAnim != null) screwAnim.DoScaledAnimationAsync("robot_belt_screw_loop", 0.5f);

            SetCraneText("");
        }

        public void Update()
        {
            if (pauseConveyor || !cond.isPlaying || !hasStartedConveyor)
            {
                if (beltAnim != null) beltAnim.speed = 0f;
                if (screwAnim != null) screwAnim.speed = 0f;
            }
            else
            {
                beltAnim.speed = 0.5f / cond.pitchedSecPerBeat;
                screwAnim.speed = 0.5f / cond.pitchedSecPerBeat;
            }

            if (PlayerInput.GetIsAction(InputAction_RobotPress) && !IsExpectingInputNow(InputAction_RobotPress))
            {
                SoundByte.PlayOneShotGame("screwbotFactory/SE_ROBOT_EN_ARM_BOTH");
                clawAnim.DoScaledAnimationAsync("robot_arm_down_out");
            }
        }

        public override void OnGameSwitch(double beat)
        {
            DoInactiveRobots(beat);
        }

        private void DoInactiveRobots(double beat)
        {
            RiqEntity robotEntity = gameManager.Beatmap.Entities.FindLast(e => e.beat - 1 <= beat 
                && e.beat + e.length >= beat 
                && e.datamodel is "screwbotFactory/botGray" or "screwbotFactory/botWhite");

            if (robotEntity != null)
            {
                RiqEntity e = robotEntity;
                RobotBlock(e.beat, e.length, e.datamodel == "screwbotFactory/botWhite" ? 0 : 1);
            }
        }

        public void RobotBlock(double beat, float length, int type)
        {
            if (GameManager.instance.currentGame != "screwbotFactory") return;

            if (length < 1) length = 1;

            if (beat - lastCueBeat <= lastCueLength) return;
            lastCueBeat = beat;
            lastCueLength = length;
            bool hasAnimated = false;
            float headPreset = UnityEngine.Random.Range(0f, 1f);

            BeatAction.New(instance, new List<BeatAction.Action>() {
                new BeatAction.Action(beat - 1, delegate {
                    if (craneCanAnimate)
                    {
                        craneAnim.DoScaledAnimationAsync("robot_crane_down_" + (type + 1), 0.5f);
                        hasAnimated = true;
                        craneCanAnimate = false;
                    }
                }),
                new BeatAction.Action(beat - 0.5f, delegate {
                    if (craneCanAnimate && !hasAnimated)
                    {
                        craneAnim.DoScaledAnimationAsync("robot_crane_down_" + (type + 1), 0.5f);
                        hasAnimated = true;
                        craneCanAnimate = false;
                    }
                }),
                new BeatAction.Action(beat, delegate {
                    craneAnim.DoScaledAnimationAsync("robot_crane_up_" + (type + 1), 0.5f);
                    StartCoroutine(ApplyCraneHeadPreset(type, headPreset));
                }),
                new BeatAction.Action(beat + length, delegate {
                    hasStartedConveyor = true;
                    craneAnim.DoScaledAnimationAsync("robot_crane_open_" + (type + 1), 0.5f);
                    if (!shouldBeHolding && !clawBusy) clawAnim.DoScaledAnimationAsync("robot_arm_ready", 0.5f);
                    pauseConveyor = false;

                    Robot robot;
                    switch ((RobotType)type)
                    {
                        case RobotType.White:
                            robot = Instantiate(robotWhite, transform).GetComponent<Robot>();
                            robot.gameObject.SetActive(true);
                            robot.gameObject.transform.position = baseW.position;
                            robot.gameObject.transform.localScale = baseW.localScale;
                            break;
                        default:
                            robot = Instantiate(robotGray, transform).GetComponent<Robot>();
                            robot.gameObject.SetActive(true);
                            robot.gameObject.transform.position = baseG.position;
                            robot.gameObject.transform.localScale = baseG.localScale;
                            break;
                    }
                    robot.initBeat = beat + length;
                    robot.type = type;
                    robot.headPreset = headPreset;
                    robot.Init();
                }),
                new BeatAction.Action(beat + length + 0.25, delegate {
                    craneCanAnimate = true;
                }),
            });
        }

        public static void RobotSfx(double beat, float length, int type)
        {
            if (length < 1) length = 1;

            float cuePitch = SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-100, 101), false);
            float landPitch = SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-12, 13), false);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("screwbotFactory/SE_ROBOT_EN_MOVE_ROBOT", beat),
                new MultiSound.Sound("screwbotFactory/SE_ROBOT_EN_MOVE_1_PITCH100", beat, pitch: cuePitch),
                new MultiSound.Sound("screwbotFactory/SE_ROBOT_EN_MOVE_2_PITCHSAME", beat + 0.5f, pitch: cuePitch),
                new MultiSound.Sound("screwbotFactory/SE_ROBOT_EN_LAND_" + (type == 0 ? "PITCH12" : "BIG_PITCH12"), beat + length, pitch: landPitch),
            }, forcePlay: true);

            if (type == 0)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("screwbotFactory/SE_ROBOT_EN_MOVE_SIREN_1", beat),
                    new MultiSound.Sound("screwbotFactory/SE_ROBOT_EN_MOVE_SIREN2", beat + 0.5f),
                }, forcePlay: true);
            }
        }

        public void SetCraneText(string text)
        {
            if (craneText != null)
            {
                craneText.text = text;
            }
        }

        private IEnumerator ApplyCraneHeadPreset(int type, float headPreset) //Sorry but not really
        {
            Animator targetAnim = (type == 0) ? craneHeadW : craneHeadG;
            if (targetAnim == null) yield break;

            float timeout = 2f;
            while (!targetAnim.gameObject.activeInHierarchy && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            if (targetAnim.gameObject.activeInHierarchy)
            {
                targetAnim.DoNormalizedAnimation(type == 0 ? "robot_robot_head_1" : "robot_robot_head_2", headPreset);
                targetAnim.speed = 0f;
            }
        }
    }
}
