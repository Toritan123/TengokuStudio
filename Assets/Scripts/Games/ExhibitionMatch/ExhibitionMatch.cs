using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using UnityEngine;
using UnityEngine.Rendering;

using DG.Tweening;
using Jukebox;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Loaders

{
    using static Minigames;
    public static class RvlExhibitionMatchLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("exhibitionMatch", "Exhibition Match", "ffffff", false, false, new List<GameAction>()
            {
                new GameAction ("bop", "Bop")
                {
                    function = delegate 
                    {
                        var e = eventCaller.currentEntity;
                        ExhibitionMatch.instance.Bop(e["auto"], e.beat, e.length, e["bop"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop", "Whether the character should bop during this event."),
                        new Param("auto", false, "Bop (Auto)", "Whether the character should automatically bop until another Bop event."),
                    },
                    resizable = true,
                    defaultLength = 1.0f,
                },
                new GameAction ("pitch", "Pitch Baseball")
                {
                    defaultLength = 5.0f,
                    preFunctionLength = 2.0f,
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity;
                        ExhibitionMatch.PreThrowBall(e.beat, e["mutePrepare"], e["force"], e["laugh"], true);
                    },
                    parameters = new List<Param>() 
                    {
                        new Param ("mutePrepare", false, "Mute Prepare", "Toggles if the preparation SFX should be muted." ),
                        new Param ("force", false, "Force Celebration", "Toggles if the Slugger should wave his bat regardless of how successful the player has been." ),
                        new Param ("laugh", true, "Laugh", "Toggles if the Slugger should laugh or not." ),
                    //    new Param ("crowd", true, "Audience Cheer", "Toggles if the audience should cheer or not." ),
                    //    The game sounds like crap if you turn off the audience cheer
                    } 
                },
                new GameAction ("throwhat", "Throw Hat")
                {
                    defaultLength = 1f,
                    function = delegate 
                    {
                        var e = eventCaller.currentEntity;
                        ExhibitionMatch.instance.pitcherHolder.ThrowHat(e.beat, e["hat"], e["instant"]);
                    },
                    parameters = new List<Param>() 
                    {
                        new Param("instant", false, "Instant", "Toggles if this event should happen instantly." ),
                        new Param("hat", false, "Force Throw", "Toggles if the hat should be thrown regardless of how successful the player has been."),
                    } 
                },
                new GameAction ("startSweat", "Pitcher Sweat")
                {
                    defaultLength = 0.5f,
                    function = delegate 
                    {
                        var e = eventCaller.currentEntity;
                        ExhibitionMatch.instance.pitcherHolder.StartCry(e["force"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("force", false, "Force Sweat", "Toggles if the pitcher should sweat regardless of how successful the player has been."),
                    }
                },
                new GameAction ("monkey", "Toggle Curtain")
                {
                    resizable = true,
                    defaultLength = 1.0f,
                    function = delegate 
                    {
                        var e = eventCaller.currentEntity;
                        ExhibitionMatch.instance.monkeyHolder.ShowMonkey(e["instant"], e.length, e["mute"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param ("instant", false, "Instant", "Toggles if this event should happen instantly." ),
                        new Param ("mute", false, "Mute", "Toggles if the sound effect plays." ),
                    }
                },
                new GameAction ("sluggerExpression", "Set Slugger Expression")
                {
                    defaultLength = 1f,
                    function = delegate 
                    {
                        var e = eventCaller.currentEntity;
                        ExhibitionMatch.instance.SluggerAnimation((SluggerExpression)e["changeexpression"], e["grunt"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param ("changeexpression", SluggerExpression.Normal, "Change Expression", "Changes the Slugger's expression and gestures."),
                        new Param ("grunt", false, "Play Grunt Sound", "Toggles if the Slugger should grunt.")
                    }
                },
                new GameAction ("pitcherState", "Set Pitcher Expression")
                {
                    defaultLength = 1f,
                    function = delegate 
                    {
                        var e = eventCaller.currentEntity;
                        ExhibitionMatch.instance.pitcherHolder.SetPitcherState(e["state"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param ("state", PitcherState.Normal, "Change Expression", "Changes the Pitcher's expression and gestures."),
                    }
                },
                new GameAction ("vsScreen", "Versus Screen")
                {
                    defaultLength = 4.0f,
                    resizable = true,
                    function = delegate 
                    {
                        var e = eventCaller.currentEntity;
                        ExhibitionMatch.instance.VSHolder.ShowVS(e.beat, e.length, e["mute"], e["vsType"]);
                    },
                    parameters = new List <Param>()
                    {
                        new Param ("vsType", VSType.Fever, "Portrait Type", "Toggles which portraits are used for the screen."),
                        new Param ("mute", false, "Mute", "Toggles if the sound effect plays."),         
                    }
                },
                new GameAction("camera", "Zoom Camera")
                {
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        float targetZoom = e["valA"];
                        if (e["zoomOut"]) targetZoom = -158f;
                        ExhibitionMatch.instance.ChangeCameraZoom(e.beat, e.length, targetZoom, (Util.EasingFunction.Ease)e["ease"]);
                    },
                    defaultLength = 4, 
                    resizable = true, 
                    parameters = new List<Param>() 
                    {
                        new Param("valA", new EntityTypes.Float(-1.8f, 1.8f, 0f), "Zoom", "Set how far you want the camera to zoom in."),
                        new Param("zoomOut", false, "Full Zoom Out", "Zoom all the way out, showing the entire audience.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => !(bool)x, new string[] { "valA" })
                        }),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.") 
                    } 
                },
                new GameAction("toggleSpots", "Toggle Spotlights")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        ExhibitionMatch.instance.ToggleSpotlights(e.beat, e["show"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("show", false, "Show Spotlights", "If enabl;ed, the spotlights will be shown. Otherwise, the will be hidden."),
                    },
                    defaultLength = 0.5f,
                    advanced = true,
                },
            },
            new List<string>() { "rvl", "aim" },
            "rvlbatting", "en",
            new List<string>() { },
            chronologicalSortKey: 23,
            font : "FOT-SlumpStd-DB SDF"
            );
        }
    }
}
//xd
namespace HeavenStudio.Games
{
    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
    using Scripts_ExhibitionMatch;
    public enum SluggerExpression
    {
        Normal,
        Blink,
        Look,
    }
    
    public enum PitcherState
    {
        Smiling = -1,
        Normal,
        Angry
    }

    public enum VSType
    {
        Fever,
        Megamix
    }


    public class ExhibitionMatch : Minigame
    {
        
        public static ExhibitionMatch instance;
        private bool throwed = false;
        public struct ThrowedBall
        {
            public double beat;
            public bool forceSuccess;
            public bool laugh;
        }

        private static List<ThrowedBall> queueBalls = new List<ThrowedBall>();
        private bool autoBop = true;
        private double skipToBeat = -1.0;
        public int SluggerState;
        private bool prepare = false;
        public bool miss = false;
        private bool success = false;
        private bool isBlink = false;
        private bool isLook = false;
        private double lastWhiffTime = -100;
        [SerializeField] Animator SluggerAnim;
        [SerializeField] Animator CrowdAnim;
        [SerializeField] Animator BGAnim;
        [SerializeField] public PitcherController pitcherHolder;
        [SerializeField] public VSController VSHolder;
        [SerializeField] public MonkeyController monkeyHolder;
        [SerializeField] GameObject spotsHolder;

        const int IAAltDownCat = IAMAXCAT;

        const int IAAltUpCat = IAMAXCAT + 1;

        public static PlayerInput.InputAction InputAction_TouchPress =
            new("SamuraiSliceTouchPress", new int[] { IAEmptyCat, IAPressCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicPress, IA_Empty);

        public static PlayerInput.InputAction InputAction_TouchRelease =
            new("SamuraiSliceTouchRelease", new int[] { IAEmptyCat, IAReleaseCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicRelease, IA_Empty);

        void Awake()
        {
            instance = this;
            SluggerState = -1;
        }
        private double cameraZoomBeat = double.MaxValue;
        private double cameraZoomLength = 0;
        private Util.EasingFunction.Ease cameraZoomEase = Util.EasingFunction.Ease.Linear;
        private float cameraZoomLast = 0f;
        private float cameraZoomNext = 0f;

        public void ChangeCameraZoom(double beat, double length, float zoom, Util.EasingFunction.Ease ease)
        {
            cameraZoomBeat = beat;
            cameraZoomLength = length;
            cameraZoomEase = ease;
            cameraZoomLast = cameraZoomNext;
            cameraZoomNext = zoom;
        }

        public override void OnPlay(double beat)
        {
            EntityPreCheck(beat, true);
        }

        public override void OnGameSwitch(double beat)
        {
            EntityPreCheck(beat);
        }

        void EntityPreCheck(double beat, bool onPlay = false)
        {
            cameraZoomBeat = double.MaxValue;
            cameraZoomLength = 0;
            cameraZoomEase = Util.EasingFunction.Ease.Linear;
            cameraZoomLast = 0f;
            cameraZoomNext = 0f;

            List<RiqEntity> prevEntities = GameManager.instance.Beatmap.Entities.FindAll(c => c.beat < beat && c.datamodel.Split('/')[0] == "exhibitionMatch");
            RiqEntity lastGameSwitch = GameManager.instance.Beatmap.Entities.FindLast(c => c.beat <= beat && c.datamodel == "gameManager/switchGame/exhibitionMatch");

            if (lastGameSwitch != null)
            {
                List<RiqEntity> cameraEntities = prevEntities.FindAll(c => c.beat >= lastGameSwitch.beat && c.datamodel == "exhibitionMatch/camera");
                foreach (var entity in cameraEntities)
                {
                    float targetZoom = entity["valA"];
                    if (entity["zoomOut"]) targetZoom = -158f;
                    ChangeCameraZoom(entity.beat, entity.length, targetZoom, (Util.EasingFunction.Ease)entity["ease"]);
                }
            }

            if (onPlay) return;

            RiqEntity pitchEntity = gameManager.Beatmap.Entities.FindLast(e => e.beat - 2 < beat && e.beat + 4 > beat && e.datamodel == "exhibitionMatch/pitch");
            if (pitchEntity != null)
            {
                RiqEntity e = pitchEntity;

                ThrowBall(e.beat, e["force"], e["laugh"]);
                pitcherHolder.PitchBall(e.beat - 2);
            }
        }

        void Update()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                SluggerState = -1;
                return;
            }

            if (cameraZoomLength > 0)
            {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(cameraZoomBeat, cameraZoomLength);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(cameraZoomEase);
                float prog = Mathf.Clamp01(normalizedBeat);
                float currentZoom = func(cameraZoomLast, cameraZoomNext, prog);
                GameCamera.AdditionalPosition = new Vector3(0, 0, currentZoom);
            }
            else if (cameraZoomBeat != double.MaxValue && Conductor.instance.unswungSongPositionInBeatsAsDouble >= cameraZoomBeat)
            {
                GameCamera.AdditionalPosition = new Vector3(0, 0, cameraZoomNext);
            }

            if(PlayerInput.GetIsAction(InputAction_FlickPress) && !IsExpectingInputNow(InputAction_FlickPress))
            {
                Hit();
                prepare = false;
            }
            if(PlayerInput.GetIsAction(InputAction_TouchPress))
            {
                ReadySlugger();
                prepare = true;
            }
            if(PlayerInput.GetIsAction(InputAction_TouchRelease))
            {
                SluggerAnim.DoScaledAnimationAsync(GetSluggerExpressionAnimation(), 0.5f);
                prepare = false;
            }
        }
        public override void OnLateBeatPulse(double beat)
        {
            if(autoBop && beat >= skipToBeat)
            {
                SluggerBop();
                pitcherHolder.BopAnimation();
                monkeyHolder.BopMonkey();
            }
            if(SluggerAnim.GetCurrentAnimatorStateInfo(0).IsName("batting_02_batter_blink_02") || SluggerAnim.GetCurrentAnimatorStateInfo(0).IsName("batting_02_batter_blink_00"))
            {
                if((SluggerAnim.IsAnimationNotPlaying() || SluggerAnim.GetCurrentAnimatorStateInfo(0).IsName("batting_02_batter_beat")) && !prepare)
                {   
                    ReadySlugger();
                }
            }
            BGAnim.DoScaledAnimationAsync("batting_bg_bg_light", 0.5f);
        }
        public void Bop(bool auto, double beat, float length, bool isBooping)
        {
            autoBop = auto;
            if (isBooping)
            {
                List<BeatAction.Action> actions = new();
                for (int i = 0; i < length; i++)
                {
                    double actionBeat = beat + i;
                    actions.Add(new BeatAction.Action(actionBeat, delegate
                    {
                        SluggerBop();
                        pitcherHolder.BopAnimation();
                        monkeyHolder.BopMonkey();
                    }));
                }
                if (actions.Count > 0)
                    BeatAction.New(this, actions);
            }
        }
        public static void PreThrowBall(double beat, bool mutePrep, bool forceSuccess, bool laugh = true, bool crowd = true)
        {
            if (!mutePrep) SoundByte.PlayOneShotGame("exhibitionMatch/READY", beat - 2.0f, forcePlay: true);
            SoundByte.PlayOneShotGame("exhibitionMatch/THROW_WOMAN", beat, forcePlay: true);

            if(GameManager.instance.currentGame == "exhibitionMatch")
            {
                BeatAction.New(instance, new List<BeatAction.Action>() {
                new BeatAction.Action(beat - 2, delegate {
                    instance.pitcherHolder.PitchBall(beat - 2);
                }),
                new BeatAction.Action(beat, delegate {
                    if (instance != null)
                    {
                        instance.ThrowBall(beat, forceSuccess, laugh, crowd);
                        instance.throwed = true;
                    }
                })
                });
            }
        }
        public void ThrowBall(double beat, bool forceSuccess, bool laugh = true, bool crowd = true)
        {
            success = forceSuccess;
            skipToBeat = beat + 8.0;
            ReadySlugger();
            monkeyHolder.ThrowBallMonkey(beat);

            ScheduleInput(beat, 4.0f, InputAction_FlickPress, (caller, state) => HitSuccess(caller, state, laugh, crowd), Miss, HitEmpty);

            BeatAction.New(instance, new List<BeatAction.Action>() {
                new BeatAction.Action(beat + 3.0f, delegate {
                    if (instance != null)
                    {
                        ScheduleUserInput(beat + 2f, 2.0f, InputAction_FlickPress, null, null, HitRoll);
                    }
                })
            });
        }
        public void HitSuccess(PlayerActionEvent caller, float state, bool laugh = true, bool crowd = true)
        {
            double currentBeat = Conductor.instance.unswungSongPositionInBeatsAsDouble;
            if (state >= 1f || state <= -1f)
            {
                HitMiss(caller, crowd);
                return;
            }

            SluggerAnim.DoScaledAnimationAsync(laugh ? "batting_02_batter_swing_laugh" : "batting_02_batter_swing", 0.5f);
            CrowdAnim.DoScaledAnimationAsync("batting_01_audience_c",0.5f);
            monkeyHolder.MonkeySuccess();
            if (monkeyHolder.newBall != null)
            {
                monkeyHolder.newBall.Success(state);
            }

            int numberRandom = UnityEngine.Random.Range(1, 4);
            List<MultiSound.Sound> sounds = new()
            {
                new MultiSound.Sound("exhibitionMatch/BALL", -1),
                new MultiSound.Sound("exhibitionMatch/HIT_NOLAUGH", -1),
                new MultiSound.Sound("exhibitionMatch/WHISTLE", currentBeat + (numberRandom - 1)),
            };

            if (laugh) sounds.Add(new MultiSound.Sound($"exhibitionMatch/LAUGH_{numberRandom}", currentBeat + 0.333333f, panning: 0.333333f));
            if (crowd) sounds.Add(new MultiSound.Sound("exhibitionMatch/HIT_FULL", -1));

            MultiSound.Play(sounds, ignoreSwing: true);

            if (SluggerState < 3)
            {
                SluggerState += 1;    
            }
            if(SluggerState == 3 || success)
            {
                BeatAction.New(instance, new List<BeatAction.Action>() {
                new BeatAction.Action(currentBeat + 2.0f, delegate {
                    SluggerAnim.DoScaledAnimationAsync("batting_02_batter_success", 0.5f);
                })
                });
            }
            BeatAction.New(instance, new List<BeatAction.Action>() {
                new BeatAction.Action(currentBeat, delegate {
                    pitcherHolder.ShockAnimation();
                }),
                new BeatAction.Action(currentBeat + 1.0f, delegate {
                    pitcherHolder.SetPitcherState(pitcherHolder.GetPitcherState() + 1);
                }),
            });
        }

        public void HitMiss(PlayerActionEvent caller, bool crowd)
        {
            SoundByte.PlayOneShotGame("exhibitionMatch/miss");
            if (crowd) SoundByte.PlayOneShotGame("exhibitionMatch/WARAI_FULL");

            SluggerAnim.DoScaledAnimationAsync("batting_02_batter_swing_miss", 0.5f);
            CrowdAnim.DoScaledAnimationAsync("batting_01_audience_c", 0.5f);
            SluggerState = 0;
            pitcherHolder.SetPitcherState(pitcherHolder.GetPitcherState() - 1);
            miss = true;
            monkeyHolder.MonkeyFail();
            if (monkeyHolder.newBall != null)
            {
                monkeyHolder.newBall.Barely();
            }
        }

        public void Miss(PlayerActionEvent caller)
        {
            AnimatorStateInfo state = SluggerAnim.GetCurrentAnimatorStateInfo(0);
            double currentBeat = Conductor.instance.unswungSongPositionInBeatsAsDouble;
            throwed = false;
            throwed = false;
            SluggerState = 0;
            pitcherHolder.SetPitcherState(-1);
            miss = true;
            monkeyHolder.MonkeyFail();
            monkeyHolder.newBall.Miss();
            if (!state.IsName("batting_02_batter_swing_out"))
            {
                SluggerAnim.DoScaledAnimationAsync("batting_02_batter_through", 0.5f);
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("exhibitionMatch/MISS_THROUGH", currentBeat),
                    new MultiSound.Sound("exhibitionMatch/FAIL_FULL", currentBeat),
                });
                return;
            }
            SoundByte.PlayOneShotGame("exhibitionMatch/FAIL_FULL");
        }

        private bool CheckWhiffCooldown()
        {
            double currentSec = Conductor.instance.songPositionAsDouble;
            if (currentSec - lastWhiffTime > 0 && currentSec - lastWhiffTime < 0.2) return false;
            lastWhiffTime = currentSec;
            return true;
        }

        public void HitEmpty(PlayerActionEvent caller)
        {
            if (!CheckWhiffCooldown()) return;
            SoundByte.PlayOneShotGame("exhibitionMatch/SWING_PITCH");
            SluggerAnim.DoScaledAnimationAsync("batting_02_batter_swing_air_to_ready", 0.5f);
            throwed = true;
        }
        public void HitRoll(PlayerActionEvent caller)
        {
            if (!CheckWhiffCooldown()) return;
            SluggerAnim.DoScaledAnimationAsync("batting_02_batter_swing_out", 0.5f);
            SoundByte.PlayOneShotGame($"exhibitionMatch/MISS_{UnityEngine.Random.Range(1, 3)}");
            throwed = true;
        }
        public void Hit()
        {
            if(throwed)
            {
                throwed = false;
                return;
            }

            if (!CheckWhiffCooldown()) return;

            SluggerAnim.DoScaledAnimationAsync("batting_02_batter_swing_air", 0.5f);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("exhibitionMatch/SWING_PITCH", -1),
                new MultiSound.Sound("exhibitionMatch/FOOT", -1),
            });
        }
        private bool IsSluggerReadyPose()
        {
            AnimatorStateInfo state = SluggerAnim.GetCurrentAnimatorStateInfo(0);
            return prepare
                || state.IsName("batting_02_batter_ready")
                || state.IsName("batting_02_batter_blink_02")
                || state.IsName("batting_02_batter_blink_00");
        }

        private string GetSluggerExpressionAnimation()
        {
            if(isBlink)
            {
                return IsSluggerReadyPose() ? "batting_02_batter_blink_02" : "batting_02_batter_blink_01";
            }
            if(isLook)
            {
                return IsSluggerReadyPose() ? "batting_02_batter_blink_00" : "batting_02_batter_beat_look";
            }
            return "batting_02_batter_beat";
        }

        public void SluggerBop()
        {
            if((SluggerAnim.IsAnimationNotPlaying() || SluggerAnim.GetCurrentAnimatorStateInfo(0).IsName("batting_02_batter_beat")) && !prepare)
            {
                SluggerAnim.DoScaledAnimationAsync(GetSluggerExpressionAnimation(), 0.5f);
            }
        }
        public void SluggerAnimation(SluggerExpression expression, bool grunt)
        {
            if(expression == SluggerExpression.Normal)
            {
                isBlink = false;
                isLook = false;
            }
            else if(expression == SluggerExpression.Blink)
            {
                isBlink = true;
                isLook = false;
            }
            else if(expression == SluggerExpression.Look)
            {
                isBlink = false;
                isLook = true;
            }
            SluggerBop();

            if (grunt)
            {
                SoundByte.PlayOneShotGame("exhibitionMatch/STOPPING_LAUGH");
            }
        }
        public void ReadySlugger()
        {
            string animation = "batting_02_batter_ready";
            if(isBlink)
            {
                animation = "batting_02_batter_blink_02";
            }
            else if(isLook)
            {
                animation = "batting_02_batter_blink_00";
            }
            SluggerAnim.DoScaledAnimationAsync(animation, 0.5f);
        }

        public void ToggleSpotlights(double beat, bool show)
        {
            spotsHolder.SetActive(show);
        }
    }
}
