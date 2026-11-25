using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;
using Jukebox.Legacy;
using System.Runtime.InteropServices;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class CtrTangoLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("tangotronic", "Tangotronic 3000 \n<color=#eb5454>[WIP]</color>", "ffffff", false, false, new List<GameAction>()
            {
                new GameAction("walk", "Walk")
                {

                    function = delegate {var e = eventCaller.currentEntity; Tangotronic.instance.MoveDancers(e.beat, e.length, e["xDest"], e["zDest"], e["ease"]); },
                    resizable = true,
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("xDest", new EntityTypes.Float(-20f, 20f, 0), "X Location", "Set the X location of the walking destination."),
                        new Param("zDest", new EntityTypes.Float(-10f, 10f, 0), "Z Location", "Set the Z location of the walking destination."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    }

                },
                new GameAction("twostep", "Two Step Pose")
                {
                    function = delegate {var e = eventCaller.currentEntity; Tangotronic.instance.TwoStep(e.beat, e["bop"], e["cheer"], e["poseType"], e["poseLength"]); },
                    parameters = new List<Param>()
                    {
                        new Param("bop", false, "Bop Animation", "Toggle the preparation animation."),
                        new Param("cheer", true, "Play Cheer Sounds", "Toggle if the cheer sounds must be played"),
                        new Param("poseType", Tangotronic.PoseType.Corte, "Pose Type", "Select the ending pose."),
                        new Param("poseLength", new EntityTypes.Float(0.5f, 20f, 1.5f), "Pose Duration", "How many beats will it last.")
                    },
                    defaultLength = 3.0f,

                },
                new GameAction("threestep", "Three Step Colgada")
                {
                    function = delegate {var e = eventCaller.currentEntity; Tangotronic.instance.ThreeStep(e.beat, e["bop"], e["poseLength"]); },
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop Animation", "Toggle the preparation animation."),
                        new Param("poseLength", new EntityTypes.Float(1.5f, 20f, 2f), "Pose Duration", "How many beats will it last.")
                    },
                    defaultLength = 4.0f,
                },
                new GameAction("twirl", "Pose")
                {
                    function = delegate {var e = eventCaller.currentEntity; Tangotronic.instance.SpinPose(e.beat, e["bop"], e["cheer"], e["endPose"], e["poseLength"], e["roses"]); },
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop Animation", "Toggle the preparation animation."),
                        new Param("cheer", true, "Play Cheer Sounds", "Toggle if the cheer sounds must be played"),
                        new Param("endPose", Tangotronic.EndPoseType.Caida, "Pose Type", "Select the ending pose."),
                        new Param("roses", true, "Show roses", "Toggle if the roses should appear at the end."), // Show Roses.
                        new Param("poseLength", new EntityTypes.Float(1.5f, 10f, 2f), "Pose Duration", "How many beats will it last.")
                    },
                    defaultLength = 4.0f,
                },
                new GameAction("spotlight", "Toggle Spotlight")
                {
                    function = delegate {var e = eventCaller.currentEntity; Tangotronic.instance.ToggleSpotlight(e["toggle"]);},
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Toggle Spotlight", "Toggle if the spotlight should be present.")
                    },
                    defaultLength = 0.5f
                },
                new GameAction("turn", "Turn Dancers")
                {
                    function = delegate {var e = eventCaller.currentEntity; Tangotronic.instance.Turn(e.beat);},
                    defaultLength = 0.5f,
                    advanced = true
                },
                new GameAction("bgLights","Toggle Background Lights")
                {
                    function = delegate{var e = eventCaller.currentEntity; Tangotronic.instance.ToggleBG(e["toggle"]);},
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Toggle Background Lights", "Toggle the background lights, stars and dots.")
                    },
                    defaultLength = 0.5f,
                    advanced = true
                }

            },
            new List<string>() { "ctr", "normal" },
            "ctrtango", "en",
            new List<string>() { "en", "jp" },
            chronologicalSortKey: 10,
            font : "KanTeiRyu"
            );
                
        }
    }
}

namespace HeavenStudio.Games
{
    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
    public class Tangotronic : Minigame
    {

        public static Tangotronic instance;

        #region Definitions
        //Transforms
        public Transform dancersHolder;
        public Transform womanHead;
        public Transform rodriHead;
        public Transform matildaBody;
        public Transform rodriBody;
        public GameObject roses;

        //Anims
        public Animator dancersAnim;
        public Animator womanHeadAnim;
        public Animator rodriHeadAnim;
        public Animator rodriAnim;
        public Animator matildaAnim;

        // Stage
        [Header("Environment")]
        public Transform[] flowers; 
        public GameObject[] bgLights;
        private float rotationSpeed = 30f;

        [Header("Spotlight Config")]
        public Transform lightCone; // light room
        public Transform lightBase; // light bottom 
        public Transform dancersPosition; // Dancers Holder to have a refernce fot the light

        // Walking logic
        double walkStartBeat;
        float walkLength;
        float xDestLast = 0; float zDestLast = 0;
        float xDestNext = 0; float zDestNext = 0;
        Util.EasingFunction.Ease lastEase;
        private float floorAngle = 14f; 

        // Extra variables
        private Sound rollHeadSound;
        private bool currentCheer = true;
        bool shouldTurn = false;
        private bool showRoses = false;
        private int currentPoseType = 0;
        private bool missedInput = false; // When you miss one input, you cannot do any of the following inputs so this bool will check that, ez pz coding
        private bool wasPlaying = false; // For pausing
        private double lastClean =0; // Animators are cruel

        public enum PoseType
        {
            Corte,
            Cruzada,
            WalkPose
        }
        public enum EndPoseType
        {
            Caida,
            Sentada,
            Quebrada
        } // Reference https://en.wikipedia.org/wiki/Figures_of_Argentine_tango

        // Definitely not stolen from Frog Hop
        const int IAAltDownCat = IAMAXCAT;
        const int IAAltUpCat = IAMAXCAT + 1;

        protected static bool IA_PadAltPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltPress(out double dt)
        {
            return PlayerInput.GetSqueezeDown(out dt);
        }
        
        protected static bool IA_PadAltRelease(out double dt)
        {
            return PlayerInput.GetPadUp(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltRelease(out double dt)
        {
            return PlayerInput.GetSqueezeUp(out dt);
        }

        public static PlayerInput.InputAction InputAction_AltPress =
            new("CtrTangoAltPress", new int[] { IAAltDownCat, IAAltDownCat, IAAltDownCat },
            IA_PadAltPress, IA_TouchBasicPress, IA_BatonAltPress);
            
        public static PlayerInput.InputAction InputAction_AltRelease =
            new("CtrTangoAltRelease", new int[] { IAAltUpCat, IAFlickCat, IAAltUpCat },
            IA_PadAltRelease, IA_TouchFlick, IA_BatonAltRelease);

        #endregion

        #region Global Methods
        public override void OnGameSwitch(double beat)
        {
            if (roses != null) roses.SetActive(false);
            missedInput = false; 

            var lastBGLights = GameManager.instance.Beatmap.Entities.FindLast(e => e.datamodel == "tangotronic/bgLights" && e.beat <= beat);
            bool bgLightsActive = lastBGLights != null ? lastBGLights["toggle"] : true;
            ToggleBG(bgLightsActive);

            if (dancersAnim != null) 
            { 
                dancersAnim.DoScaledAnimationAsync("Empty", 0.5f); 
                dancersAnim.Update(0f); 
            }
            if (womanHeadAnim != null) 
            { 
                womanHeadAnim.DoScaledAnimationAsync("face_W_looking_side", 0.5f); 
                womanHeadAnim.Update(0f); 
            }
            if (rodriHeadAnim != null) 
            { 
                rodriHeadAnim.DoScaledAnimationAsync("face_M_grabbing", 0.5f); 
                rodriHeadAnim.Update(0f); 
            }
            FlipHead(womanHead, false);
            FlipHead(rodriHead, false);
            
            var pastPoses = GameManager.instance.Beatmap.Entities.FindAll(e => e.datamodel == "tangotronic/twirl" && e.beat <= beat);
            if (pastPoses.Count > 0)
            {
                var lastPose = pastPoses[^1];
                double poseStart = lastPose.beat + 4.0;
                double poseEnd = lastPose.beat + 3.0 + lastPose["poseLength"];
                
                if (beat >= poseStart && beat < poseEnd)
                {
                    if (lastPose["roses"] && roses != null) roses.SetActive(true);
                    
                    int poseType = lastPose["endPose"];
                    if (dancersAnim != null)
                    {
                        if (poseType == 0) dancersAnim.DoScaledAnimationAsync("just_pose1", 0.5f);
                        else if (poseType == 1) dancersAnim.DoScaledAnimationAsync("just_pose2", 0.5f);
                        else if (poseType == 2) dancersAnim.DoScaledAnimationAsync("just_pose3", 0.5f);
                        
                        dancersAnim.Update(0f);
                    }
                }
            }// a bunch of quick bugs I fixed before pushing everything

            // My last attempt was mediocre and had a lot of bugs hopefully this is better :earnest: 
            // Ok this is the one
            var pastWalkEvents = GameManager.instance.Beatmap.Entities
                .FindAll(e => e.datamodel == "tangotronic/walk" && e.beat <= beat)
                .OrderBy(e => e.beat).ToList();

            if (pastWalkEvents.Count > 0)
            {
                if (pastWalkEvents.Count > 1)
                {
                    var prevWalk = pastWalkEvents[^2];
                    xDestLast = prevWalk["xDest"];
                    zDestLast = prevWalk["zDest"];
                }
                else
                {
                    xDestLast = 0f;
                    zDestLast = 0f;
                }
                
                var lastWalk = pastWalkEvents[^1];
                walkStartBeat = lastWalk.beat;
                walkLength = lastWalk.length;
                xDestNext = lastWalk["xDest"];
                zDestNext = lastWalk["zDest"];
                lastEase = (Util.EasingFunction.Ease)lastWalk["ease"];

                if (!Mathf.Approximately(xDestNext, xDestLast))
                {
                    shouldTurn = xDestNext < xDestLast;
                }
            }
            else
            {
                walkStartBeat = 0;
                walkLength = 0;
                xDestLast = 0f;
                zDestLast = 0f;
                xDestNext = 0f;
                zDestNext = 0f;
                shouldTurn = false;
            }

            shouldTurn = false;
            float prevX = 0f;

            var pastFlips = GameManager.instance.Beatmap.Entities.FindAll(e => (e.datamodel == "tangotronic/walk" || e.datamodel == "tangotronic/turn") && e.beat <= beat).OrderBy(e => e.beat).ToList();

            foreach (var ev in pastFlips)
            {
                if(ev.datamodel == "tangotronic/walk")
                {
                    float targetX = ev["xDest"];
                    if(!Mathf.Approximately(targetX, prevX))
                    {
                        shouldTurn = targetX < prevX;
                    }
                    prevX = targetX;
                }
                else if (ev.datamodel == "tangotronic/turn")
                {
                    shouldTurn = !shouldTurn;
                }
            }
        }

        public override void OnPlay(double beat)
        {
            // Also part of the buggy buggy game bahaviourr
            OnGameSwitch(beat);
        }

        private void Awake()
        {
            instance = this;
        }

        void Update()
        {

            GameCamera.AdditionalPosition = new Vector3(0f, -2.5f, 2.5f); // this was originally on OnGameSwitch but I realize now why it belongs here

            var cond = Conductor.instance;

            if (cond != null && wasPlaying && !cond.isPlaying && !cond.isPaused)
            {
                if (dancersAnim != null) { dancersAnim.DoScaledAnimationAsync("Empty", 0.5f); dancersAnim.Update(0f); }
                if (womanHeadAnim != null) { womanHeadAnim.DoScaledAnimationAsync("face_W_looking_side", 0.5f); womanHeadAnim.Update(0f); }
                if (rodriHeadAnim != null) { rodriHeadAnim.DoScaledAnimationAsync("face_M_grabbing", 0.5f); rodriHeadAnim.Update(0f); }
                
                FlipHead(womanHead, false);
                FlipHead(rodriHead, false);
                
                if (roses != null) roses.SetActive(false);
                missedInput = false;
            }

            if (cond != null) wasPlaying = cond.isPlaying;

            if (cond != null && (cond.isPlaying || cond.isPaused))
            {
                double currentBeat = cond.songPositionInBeatsAsDouble;
                double expectedStep = cond.isPlaying ? (Time.unscaledDeltaTime / cond.pitchedSecPerBeat) : 0.0;
                if (currentBeat < lastClean - 0.02 || System.Math.Abs(currentBeat - (lastClean + expectedStep)) > 0.5) 
                {
                    OnGameSwitch(currentBeat);
                }
                lastClean = currentBeat;
            }
            else if (cond != null && !cond.isPlaying && !cond.isPaused)
            {
                lastClean = cond.songPositionInBeatsAsDouble;
            } // Thank you Sick Beats, now the game can be paused without the inputs breaking :earnest: 
                
            {
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(lastEase);
            
                float safeLength = Mathf.Max(0.001f, walkLength);
                float prog = Conductor.instance.GetPositionFromBeat(walkStartBeat, safeLength, true);
                
                prog = Mathf.Clamp01(prog);

                float newPosX = func(xDestLast, xDestNext, prog);
                float newPosZ = func(zDestLast, zDestNext, prog);

                float newPosY = newPosZ * Mathf.Tan(floorAngle * Mathf.Deg2Rad); // GUESS WHAT?? TRIG IS USED DUE TO STAGE SKEW

                dancersHolder.localPosition = new Vector3(newPosX, newPosY, newPosZ);

                dancersHolder.localRotation = Quaternion.Euler(0, shouldTurn ? 180f : 0f, 0);
            } // Reverted to its original code 
            
            if (flowers.Length == 3)
            {
                flowers[0].Rotate(0, 0, rotationSpeed * Time.deltaTime);
                flowers[1].Rotate(0, 0, rotationSpeed * Time.deltaTime);
                flowers[2].Rotate(0, 0, rotationSpeed * Time.deltaTime);
            } // should have been a for cycle lololololol 

        }
        private void LateUpdate()
        {
            // what am i doing man
            GameCamera.AdditionalPosition = new Vector3(0f, -2.5f, 2.5f);

            if (lightBase != null && dancersPosition != null)
            {
                float floorSlop = dancersPosition.position.z * Mathf.Tan(floorAngle * Mathf.Deg2Rad);
                lightBase.position = new Vector3(dancersPosition.position.x, -6.0f + floorSlop + 0.05f, dancersPosition.position.z); // More trig but man 
                // idk how to make it use only one variable
            }
        } /* Okay so the camera behaves weirdly and I don't exactly know how spaceball handles its own camera
        fortunately, LateUpdate exists and I can avoid those 1 frame weird moves*/
        #endregion

        #region Tangotronic Methods
        // These are logical methods for the game, solutions I came with for every bug I found while playtesting

        private bool IsEventPlaying(double currentBeat)
        {
            var latestEvent = GameManager.instance.Beatmap.Entities
            .Where(e => e.datamodel == "tangotronic/twostep" || e.datamodel == "tangotronic/threestep" || e.datamodel == "tangotronic/twirl")
            .OrderBy(e => e.beat)
            .LastOrDefault(e => e.beat <= currentBeat + 0.01);

            if(latestEvent != null)
            {
                return currentBeat < latestEvent.beat + latestEvent.length - 0.01;
            }
            return false; //

           /* var overlappingEvent = GameManager.instance.Beatmap.Entities.Find(e => (e.datamodel == "tangotronic/twostep" || e.datamodel == "tangotronic/threestep" || e.datamodel == "tangotronic/twirl") && currentBeat > e.beat + 0.01 && currentBeat < e.beat + e.length - 0.01);
            return overlappingEvent != null;*/
        } // Ok wow I'm terrible at bugfixes anyone reading the code who knows id there's a better way to avoid the anim priorities hit my dms please
        private void FlipHead(Transform head, bool flipX)
        {
            float sign = flipX ? -1f : 1f;
            head.localScale = new Vector3(sign * Mathf.Abs(head.localScale.x), head.localScale.y, head.localScale.z);
        } // Think this is a better implementation of how the heads turn mid animations

        private void Idle(double targetBeat, bool resetBody)
        {
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(targetBeat, delegate
                {
                    //if (IsEventPlaying(targetBeat)) return; 

                    womanHeadAnim.DoScaledAnimationAsync("face_W_looking_side", 0.5f);
                    rodriHeadAnim.DoScaledAnimationAsync("face_M_grabbing", 0.5f);
                    FlipHead(womanHead, false);
                    FlipHead(rodriHead, false);

                    if (IsEventPlaying(targetBeat)) return; // yes I just changed the order of this and it works, wow, genius

                    if (!resetBody) currentPoseType = -1;

                    if (resetBody && !IsWalking(targetBeat))
                        dancersAnim.DoScaledAnimationAsync("Empty", 0.5f); // ok please work
                })
            });
        } /* Another logical method I came with because there are some strange interactions bro I don't get it
        Thing is I don't think I can add the bop like any other minigame because, even though, the animation is there, the game never 
        uses it in the conventional way */

        // Never mind I will add the bop!
        private bool IsWalking(double currentBeat)
        {
            var currentWalk = GameManager.instance.Beatmap.Entities.FindLast(e => e.datamodel == "tangotronic/walk" && e.length >= 0.9f && currentBeat >= e.beat - 0.05 && currentBeat < e.beat + e.length - 0.05);

            if (currentWalk == null) return false;

            var allWalks = GameManager.instance.Beatmap.Entities.FindAll(e => e.datamodel == "tangotronic/walk").OrderBy(e => e.beat).ToList();

            int index = allWalks.IndexOf(currentWalk);

            if (index > 0)
            {
                var prevWalk = allWalks[index - 1];
                return !Mathf.Approximately(currentWalk["xDest"], prevWalk["xDest"]) || !Mathf.Approximately(currentWalk["zDest"], prevWalk["zDest"]);
            }

            return !Mathf.Approximately(currentWalk["xDest"], 0f) || !Mathf.Approximately(currentWalk["zDest"], 0f);
        }/*More logical stuff I think can be done in easier ways but I'm not an expert in c# im a python coder man can you blame me???
        ok and I just had to make it this garbage because the fucking heads started doing bullshit and so this is my approach to a solution
        I need to read a lot of c# and unity docs*/

        private void StopRollHeadSound()
        {
            if (rollHeadSound != null)
            {
                rollHeadSound.KillLoop(0);
                rollHeadSound = null;
            }
        } // and one for the sounds

        // This is the methods for the blocks
        public void MoveDancers(double beat, float length, float xDest, float zDest, int ease)
        {
            walkStartBeat = beat;
            walkLength = length;
            xDestLast = xDestNext;
            xDestNext = xDest;
            zDestLast = zDestNext;
            zDestNext = zDest;
            lastEase = (Util.EasingFunction.Ease)ease;

            bool isMoving = !Mathf.Approximately(xDest, xDestLast) || !Mathf.Approximately(zDest, zDestLast);

            if (!Mathf.Approximately(xDest, xDestLast))
            {
                shouldTurn = xDest < xDestLast;
            }

            var actions = new List<BeatAction.Action>();

            bool isLooping = length > 2.0f;

            string walkAnim = isLooping ? "stepA" : "RwalkB";
            float stepInterval = isLooping ? 1.5f : 2f;

            if (isMoving && length >= 0.9f)
            {
                for (float i = 0; i + 0.9f <= length; i += stepInterval)
                {
                    float t =i;
                    actions.Add(new BeatAction.Action(beat + i, delegate 
                    {
                        if (dancersAnim != null)
                        {
                            dancersAnim.DoScaledAnimationAsync(walkAnim, 0.5f);

                            if (!IsEventPlaying(beat + t))
                            {
                                if (currentPoseType != (int)PoseType.WalkPose)
                                {
                                womanHeadAnim.DoScaledAnimationAsync("face_W_looking_side", 0.5f);
                                rodriHeadAnim.DoScaledAnimationAsync("face_M_grabbing", 0.5f);
                                }
                                FlipHead(rodriHead, false);
                                FlipHead(womanHead, false);
                            }
                        }
                    }));
                }
            }

            else if (isMoving && length < 0.9f) // GUESS WHAT, ANOTHER BUG
            {
                Idle(beat, true);
            }

            if (actions.Count > 0)
            {
                BeatAction.New(instance, actions);
            }
        } // istg every time I fix a bug ten more appear, I will fix bugs later, pinky promise

        // What the fuck is a cent :sob:
        public void TwoStep (double beat, bool bop, bool cheer, int poseType, float poseLength)
        {
            missedInput =false;
            currentCheer = cheer;
            currentPoseType = poseType;

            Idle(beat, true);

            MultiSound.Play(new MultiSound.Sound[]
            {
             new MultiSound.Sound("tangotronic/whistle1", beat),
             new MultiSound.Sound("tangotronic/whistle2", beat + 0.5f),   
            });

            ScheduleInput(beat, 2.0, InputAction_BasicPress, TwoStepHitOne, TwoStepMissOne, EmptyAction);
            ScheduleInput(beat, 2.5, InputAction_BasicPress, TwoStepHitTwo, TwoStepMissTwo, EmptyAction);

            if (bop)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 1.0f, delegate {
                        if (!IsWalking(beat + 1.0f)) dancersAnim.DoScaledAnimationAsync("beat", 0.5f);
                    })
                });
            }

            bool shouldResetBody = (PoseType)currentPoseType != PoseType.WalkPose;
            Idle(beat + 2.5 + poseLength, shouldResetBody);
        }

        public void ThreeStep (double beat, bool bop, float poseLength)
        {
            missedInput = false; //!!!

            Idle(beat, true);

            MultiSound.Play(new MultiSound.Sound[]
            {
             new MultiSound.Sound("tangotronic/ThreeStepPrepare1", beat),
             new MultiSound.Sound("tangotronic/ThreeStepPrepare2", beat + 0.5f),
             new MultiSound.Sound("tangotronic/ThreeStepPrepare3", beat + 1.0f),   
            });

            ScheduleInput(beat, 2.0, InputAction_BasicPress, ThreeStepHitOne, ThreeStepMissOne, EmptyAction);
            ScheduleInput(beat, 2.5, InputAction_BasicPress, ThreeStepHitTwo, ThreeStepMissTwo, EmptyAction);
            ScheduleInput(beat, 3.0, InputAction_BasicPress, ThreeStepHitThree, ThreeStepMissThree, EmptyAction);

            if (bop)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 1.0f, delegate {dancersAnim.DoScaledAnimationAsync("beat", 0.5f);})
                });
            }
            
            Idle(beat + 3.0 + poseLength, true);
        }

        public void SpinPose (double beat, bool bop, bool cheer, int endPose, float lenghtPose, bool roses)
        {
            missedInput = false;
            currentCheer = cheer;
            currentPoseType = endPose;
            showRoses = roses;

            Idle(beat, true);

            MultiSound.Play(new MultiSound.Sound[]
            {
             new MultiSound.Sound("tangotronic/Ready1", beat),
             new MultiSound.Sound("tangotronic/Ready2", beat + 1.0f),   
            });
            ScheduleInput(beat, 2.0, InputAction_AltPress, TwirlHit, TwirlMiss, EmptyAction);
            ScheduleInput(beat, 3.0, InputAction_AltRelease, PoseHit, PoseMiss, EmptyAction);

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 3.0 + lenghtPose, delegate
                {
                    if (this.roses != null) this.roses.SetActive(false);
                })
            });

            if (bop)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 1.0f, delegate {dancersAnim.DoScaledAnimationAsync("beat", 0.5f);})
                });
            }

            Idle(beat + 3.0 + lenghtPose, true);
        }

        // Hits

        public void TwoStepHitOne(PlayerActionEvent caller, float state)
        {
            double beat = caller.startBeat + caller.timer;
            rollHeadSound = SoundByte.PlayOneShotGame("tangotronic/TwoStep1", -1, 1, 1, true);

            // Random HeadRoll A, B, C no repeats logic
            var sounds = new List<MultiSound.Sound>();
            string[] heads = {"tangotronic/HeadRollA", "tangotronic/HeadRollB", "tangotronic/HeadRollC"};
            int lastHead = -1;
            for (int i = 0; i<3; i++) // Try to optimize this
            {
                int randomHead;
                do
                {
                    randomHead = UnityEngine.Random.Range(0,3);
                } while (randomHead == lastHead);
                lastHead = randomHead;

                double rollBeats = beat + (i*(1.0/6.0));
                sounds.Add(new MultiSound.Sound(heads[randomHead], rollBeats));
            }
            MultiSound.Play(sounds.ToArray(), forcePlay: true); // Note to self, copy and paste for the pose cue and the beeps

            womanHeadAnim.DoScaledAnimationAsync("face_W_turn", 0.5f);
            rodriHeadAnim.DoScaledAnimationAsync("face_M_turn", 0.5f);
            if ((PoseType)currentPoseType != PoseType.WalkPose)
            {
                dancersAnim.DoScaledAnimationAsync("RwalkB_pose", 0.5f);
            }
        }

        public void TwoStepHitTwo(PlayerActionEvent caller, float state)
        {
            if (missedInput) return;
            StopRollHeadSound();
            double beat = caller.startBeat + caller.timer;
            SoundByte.PlayOneShotGame("tangotronic/TwoStep2");

            if (currentCheer)
            {
                // SUCCESS random pitch
                float pitch = SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-100, 101), false);
                SoundByte.PlayOneShotGame("tangotronic/SuccessApplause", pitch: pitch);

                bool whistlePlayed = false;

                // WHISTLE - 1/3 
                if (UnityEngine.Random.Range(0, 3) == 0)
                {
                    whistlePlayed = true;
                    double delay = UnityEngine.Random.Range(36f, 100f) / 48.0;

                    MultiSound.Play(new MultiSound.Sound[] {
                        new MultiSound.Sound("tangotronic/SuccessWhistle", beat + delay)
                    });
                }

                // CHEER - 1/2 
                if (!whistlePlayed && UnityEngine.Random.Range(0, 2) == 0)
                {
                    double delay = UnityEngine.Random.Range(3f, 85f) / 48.0;

                    MultiSound.Play(new MultiSound.Sound[] {
                        new MultiSound.Sound("tangotronic/SuccessCheer", beat + delay)
                    });
                }
            }

            switch ((PoseType)currentPoseType)
            {
                case PoseType.Corte:
                    if (!IsWalking(beat))
                    {
                        dancersAnim.DoScaledAnimationAsync("RwalkB_pose_face3",0.5f);
                        rodriHeadAnim.DoScaledAnimationAsync("Mpose", 0.5f);
                        womanHeadAnim.DoScaledAnimationAsync("Wpose", 0.5f);
                    }
                    break;
                case PoseType.Cruzada:
                    if (!IsWalking(beat))
                    {
                        dancersAnim.DoScaledAnimationAsync("RwalkB_pose_face2",0.5f);
                        rodriHeadAnim.DoScaledAnimationAsync("Mpose", 0.5f);
                        womanHeadAnim.DoScaledAnimationAsync("Wpose_b", 0.5f);  
                    }
                    break; // un saludo a mi primo el todocaso
                case PoseType.WalkPose:
                    rodriHeadAnim.DoScaledAnimationAsync("face_M_front", 0.5f);
                    womanHeadAnim.DoScaledAnimationAsync("Wpose_a", 0.5f);
                    break;
            }
        }

        public void ThreeStepHitOne(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("tangotronic/ThreeStep1");
            dancersAnim.DoScaledAnimationAsync("just_pose0", 0.5f);
            rodriHeadAnim.DoScaledAnimationAsync("face_M_grabbing",0.5f);

            /* Head doesn't rotate idk why but this is my soultion please do not steal it is highest quality code
            womanHead.localScale = new Vector3(womanHead.localScale.x * -1f, womanHead.localScale.y, womanHead.localScale.z);*/

            //Dsicard that, I think I found a better solution
            womanHeadAnim.DoScaledAnimationAsync("face_W_looking_side", 0.5f);
            FlipHead(womanHead, true);
        }

        public void ThreeStepHitTwo(PlayerActionEvent caller, float state)
        {
            if (missedInput) return;
            double beat = caller.startBeat + caller.timer;
            var sounds = new List<MultiSound.Sound>();
            
            SoundByte.PlayOneShotGame("tangotronic/ThreeStep2");

            for (int i=0; i<6; i++)
            {
                double beepBeat = beat + (i*(1.0/12.0));
                sounds.Add(new MultiSound.Sound($"tangotronic/ThreeStepBeep{i+1}", beepBeat));
            }
            MultiSound.Play(sounds.ToArray(), forcePlay: true);
            dancersAnim.DoScaledAnimationAsync("just_pose0_turnB", 0.5f);
            rodriHeadAnim.DoScaledAnimationAsync("face_M_turn", 0.5f);

            // Goog...
            FlipHead(womanHead, false);
        }

        public void ThreeStepHitThree(PlayerActionEvent caller, float state)
        {
            if (missedInput) return;
            double beat = caller.startBeat + caller.timer;
            
            SoundByte.PlayOneShotGame("tangotronic/ThreeStep3");
            SoundByte.PlayOneShotGame("tangotronic/ThreeStepBeep7");
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    rodriHeadAnim.DoScaledAnimationAsync("face_M_grabbing", 0.5f);
                    if (!IsWalking(beat))
                    {
                        FlipHead(rodriHead, true);
                    }
                }),
                new BeatAction.Action(beat + 1.0, delegate
                {
                    if (!IsWalking(beat + 1.0) && !IsEventPlaying(beat + 1.0))
                    {
                        womanHeadAnim.DoScaledAnimationAsync("face_W_front", 0.5f);
                    }
                }),
                new BeatAction.Action(beat + 2.0, delegate
                { // Claude Gemini pt can you help me? Claude gpt: 
                    FlipHead(rodriHead, false); // NOTE: I DID NOT USE AI
                    if (IsEventPlaying(beat + 2.0)) return;
                    womanHeadAnim.DoScaledAnimationAsync("face_W_looking_side", 0.5f);
                    if (!IsWalking(beat + 2.0))
                        dancersAnim.DoScaledAnimationAsync("Empty", 0.5f);
                })
            });
        } // Si no le contesto se desespera, piensa que con otra estoy haciendo lo que le hacía a ella ea ea ea como tu mente maquinea

        public void TwirlHit(PlayerActionEvent caller, float state)
        {
            double beat = caller.startBeat + caller.timer;
            var sounds = new List<MultiSound.Sound>();
            SoundByte.PlayOneShotGame("tangotronic/Twirl1");

            for (int i=0; i<12; i++)
            {
                double beepBeat = beat + (i*(1.0/12.0));
                sounds.Add(new MultiSound.Sound($"tangotronic/TwirlBeep{i+1}", beepBeat));
            }

            string[] heads = {"tangotronic/HeadRollA", "tangotronic/HeadRollB", "tangotronic/HeadRollC"};
            int lastHead = -1;
            for (int i = 0; i<6; i++) // Copypasted from two step
            {
                int randomHead;
                do
                {
                    randomHead = UnityEngine.Random.Range(0,3);
                } while (randomHead == lastHead);
                lastHead = randomHead;

                double rollBeats = beat + (i*(1.0/6.0));
                sounds.Add(new MultiSound.Sound(heads[randomHead], rollBeats));
            }

            MultiSound.Play(sounds.ToArray(), forcePlay: true);

            dancersAnim.DoScaledAnimationAsync("WMturn2",0.5f);
            rodriHeadAnim.DoScaledAnimationAsync("face_M_pose_spinlook", 0.5f);
            womanHeadAnim.DoScaledAnimationAsync("face_W_rotate");
        }

        public void PoseHit(PlayerActionEvent caller, float state)
        {
            if (missedInput) return;
            double beat = caller.startBeat + caller.timer;
            SoundByte.PlayOneShotGame("tangotronic/Twirl2");
            if(currentCheer)
            {
                SoundByte.PlayOneShotGame("tangotronic/Cheer");
            }

            switch ((EndPoseType)currentPoseType)
            {
                case EndPoseType.Caida:
                    dancersAnim.DoScaledAnimationAsync("just_pose1",0.5f);
                    rodriHeadAnim.DoScaledAnimationAsync("face_M_pose_hang", 0.5f);
                    womanHeadAnim.DoScaledAnimationAsync("face_W_looking_side", 0.5f);
                    FlipHead(womanHead, true);

                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 1.0f, delegate
                        {
                            FlipHead(womanHead, false);
                            womanHeadAnim.DoScaledAnimationAsync("face_W_just_smile", 0.5f);
                            rodriHeadAnim.DoScaledAnimationAsync("face_M_front", 0.5f);
                            if (showRoses && roses != null)
                            {
                                roses.SetActive(true);
                            }
                        })
                    });

                    break;
                case EndPoseType.Sentada:
                    dancersAnim.DoScaledAnimationAsync("just_pose2",0.5f);
                    rodriHeadAnim.DoScaledAnimationAsync("Mpose",0.5f);
                    womanHeadAnim.DoScaledAnimationAsync("Wpose_a", 0.5f);
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 1.0f, delegate
                        {
                            womanHeadAnim.DoScaledAnimationAsync("face_W_front", 0.5f);
                            rodriHeadAnim.DoScaledAnimationAsync("face_M_front", 0.5f);
                            if (showRoses && roses != null)
                            {
                                roses.SetActive(true);
                            }
                        })
                    });
                    break;
                case EndPoseType.Quebrada:
                    dancersAnim.DoScaledAnimationAsync("just_pose3",0.5f);
                    rodriHeadAnim.DoScaledAnimationAsync("face_M_grabbing",0.5f);
                    womanHeadAnim.DoScaledAnimationAsync("face_W_just_smile", 0.5f);
                    FlipHead(rodriHead, true);
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 1.0f, delegate
                        {
                            womanHeadAnim.DoScaledAnimationAsync("face_W_front", 0.5f);
                            rodriHeadAnim.DoScaledAnimationAsync("face_M_front", 0.5f);
                            FlipHead(rodriHead, false);
                            if (showRoses && roses != null)
                            {
                                roses.SetActive(true);
                            }
                        })
                    });
                    break;
            }

        }

        public void Turn(double beat)
        {
            shouldTurn = !shouldTurn; 
        }

        //Misses

        private void RodriMissAnim(double beat) // For every instance where the head boioioioioioingsss
        {
            SoundByte.PlayOneShotGame("tangotronic/Miss");
            
            rodriHeadAnim.DoScaledAnimationAsync("face_M_miss", 0.5f);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1.0, delegate 
                {
                    rodriHeadAnim.DoScaledAnimationAsync("face_M_front", 0.5f); 
                })
            });
        }

        public void TwoStepMissOne(PlayerActionEvent caller)
        {
            missedInput = true;
            RodriMissAnim(caller.startBeat + caller.timer);
        }

        public void TwoStepMissTwo(PlayerActionEvent caller)
        {
            if (missedInput) return;
            StopRollHeadSound();
            double beat = caller.startBeat + caller.timer;

            womanHeadAnim.DoScaledAnimationAsync("Wpose_a", 0.5f);
            RodriMissAnim(beat);
        }

        public void ThreeStepMissOne(PlayerActionEvent caller)
        {
            missedInput = true;
            double beat = caller.startBeat + caller.timer;
            RodriMissAnim(beat);

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 2.0, delegate
                {
                    womanHeadAnim.DoScaledAnimationAsync("face_W_front", 0.5f);
                    FlipHead(womanHead, false);
                })
            });
        }

        public void ThreeStepMissTwo(PlayerActionEvent caller)
        {
            if (missedInput) return;
            
            missedInput = true;
            
            double beat = caller.startBeat + caller.timer;
            RodriMissAnim(beat);

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1.0, delegate
                {
                    womanHeadAnim.DoScaledAnimationAsync("face_W_front", 0.5f);
                    FlipHead(womanHead, false);
                })
            });
        }

        public void ThreeStepMissThree(PlayerActionEvent caller)
        {
            if (missedInput) return;
            double beat = caller.startBeat + caller.timer;
            RodriMissAnim(beat);

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1.0, delegate
                {
                    womanHeadAnim.DoScaledAnimationAsync("face_W_front", 0.5f);
                    FlipHead(womanHead, false);
                })
            });
        }

        public void TwirlMiss(PlayerActionEvent caller)
        {
            missedInput = true;
            RodriMissAnim(caller.startBeat + caller.timer);
        }

        public void PoseMiss(PlayerActionEvent caller)
        {
            if (missedInput) return;
            double beat = caller.startBeat + caller.timer;
            FlipHead(womanHead, true);

            SoundByte.PlayOneShotGame("tangotronic/TwirlMiss"); 
            
            dancersAnim.DoScaledAnimationAsync("Mturn_get_miss1", 0.5f);
            rodriHeadAnim.DoScaledAnimationAsync("face_M_front", 0.5f);
            womanHeadAnim.DoScaledAnimationAsync("face_W_looking_side", 0.5f); 
            
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1.0, delegate
                {
                    womanHeadAnim.DoScaledAnimationAsync("face_W_front", 0.5f);
                    FlipHead(womanHead, false);
                })
            });

            if (roses != null)
            {
                roses.SetActive(false);
            }
        }

        public void EmptyAction(PlayerActionEvent caller) {}

        public void ToggleSpotlight(bool lights)
        {
            if (lightCone != null)
            {
                lightCone.gameObject.SetActive(lights);
            }
        }
        
        public void ToggleBG(bool active)
        {
            if (flowers != null)
            {
                foreach (var flower in flowers)
                {
                    if (flower != null) flower.gameObject.SetActive(active);
                }
            }

            if (bgLights != null)
            {
                foreach (var light in bgLights)
                {
                    if (light != null) light.SetActive(active);
                }
            }
        }

        #endregion
    }
}