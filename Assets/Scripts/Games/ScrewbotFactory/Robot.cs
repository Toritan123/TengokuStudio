using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using System;

namespace HeavenStudio.Games.Scripts_ScrewbotFactory
{
    public class Robot : MonoBehaviour
    {
        private bool canMove = true;
        [SerializeField] private Transform objectToMove;
        [SerializeField] private float movementSpd = 3.4f; 
        [SerializeField] Animator robotAnim;
        [SerializeField] Animator headAnim;
        [SerializeField] Animator neckAnim;
        private ScrewbotFactory game;
        private Conductor cond;
        private Transform trans;
        public double initBeat;
        public int type;
        public float headPreset = 0f;

        private bool releaseSuccess = false;
        private bool releasedEarly = false;

        private Sound grindLoopSound;
        private double pressBeat;
        private MultiSound activeTickSounds;

        public void Init()
        {
            this.enabled = true; // i just want it off the damn prefab
            game = ScrewbotFactory.instance;
            cond = Conductor.instance;
            trans = gameObject.transform;

            if (objectToMove == null) objectToMove = transform;

            if (headAnim != null)
            {
                headAnim.gameObject.SetActive(true);
                headAnim.DoNormalizedAnimation(type == 0 ? "robot_robot_head_1" : "robot_robot_head_2", headPreset);
                headAnim.speed = 0f;
            }

            if (robotAnim != null)
            {
                StartCoroutine(PlayLandAnim());
            }

            game.ScheduleInput(initBeat, 1f, ScrewbotFactory.InputAction_RobotPress, JustPress, Empty, Empty);
        }

        private IEnumerator PlayLandAnim() 
        {
            yield return new WaitUntil(() => robotAnim.gameObject.activeInHierarchy); // this feels stupid but idk what else to do
            robotAnim.Update(0f);
            robotAnim.DoScaledAnimationAsync("robot_robot_land_" + (type + 1), 0.5f);
        }

        public void Update()
        {
            if (objectToMove.localPosition.x < -15f) //This is Ok
            {
                Destroy(gameObject);
                return;
            }

            if (canMove && !game.pauseConveyor)
            {
                float speed = Mathf.Max(movementSpd, 0.1f); 
                objectToMove.localPosition = new Vector2(objectToMove.localPosition.x - (speed * Time.deltaTime / cond.pitchedSecPerBeat), objectToMove.localPosition.y);
            }

            if (grindLoopSound != null && grindLoopSound.audioSource != null)
            {
                double currentBeat = cond.songPositionInBeatsAsDouble;
                double beatDiff = currentBeat - pressBeat;
                if (beatDiff < 0) beatDiff = 0;

                // 26 semitone pitch over 1 beat 
                float pitchSemis = Mathf.Min((float)beatDiff * 26f, 26f);
                grindLoopSound.audioSource.pitch = SoundByte.GetPitchFromCents((int)(pitchSemis * 100), false);

                // 60% volume
                if (beatDiff > 1.0)
                {
                    float volDropProg = Mathf.Clamp01((float)(beatDiff - 1.0));
                    grindLoopSound.audioSource.volume = Mathf.Lerp(1f, 0.6f, volDropProg);
                }
                else
                {
                    grindLoopSound.audioSource.volume = 1f;
                }
            }
        }

        private void StartBopping(double beat)
        {
            if (this == null || gameObject == null || !gameObject.activeInHierarchy) return;
            if (robotAnim != null) robotAnim.DoScaledAnimationAsync("robot_robot_beat_" + (type + 1), 0.5f);

            BeatAction.New(game, new List<BeatAction.Action>() {
                new BeatAction.Action(beat + 1, delegate {
                    StartBopping(beat + 1);
                }),
            });
        }

        private static int clickCycle = 0;

        private void JustPress(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("screwbotFactory/SE_ROBOT_EN_ARM_BOTH");

            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("screwbotFactory/SE_ROBOT_EN_CATCH_ZURE");
                
                string anim = "robot_arm_down_miss_" + (state >= 1f ? "b_" : "f_") + (type + 1);
                game.clawAnim.DoScaledAnimationAsync(anim, 0.5f);
                robotAnim.DoScaledAnimationAsync("robot_robot_down_miss_" + (state >= 1f ? "b_" : "f_") + (type + 1), 0.5f);
            }
            else
            {
                SoundByte.PlayOneShotGame("screwbotFactory/SE_ROBOT_EN_CATCH");
                game.clawAnim.DoScaledAnimationAsync("robot_arm_down_just_" + (type + 1), 0.5f);
                robotAnim.DoScaledAnimationAsync("robot_robot_down_just_" + (type + 1), 0.5f);
            }

            pressBeat = initBeat + 1;
            
            List<MultiSound.Sound> ticks = new List<MultiSound.Sound>();
            ticks.Add(new MultiSound.Sound("screwbotFactory/SE_ROBOT_EN_MOVE_ROBOT", pressBeat + 24f / 48f));
            
            //TICKS
            int numTicks = (type == 0) ? 12 : 24; 
            for (int i = 0; i < numTicks; i++)
            {
                string soundName = "screwbotFactory/screw/SE_ROBOT_EN_SCREW" + ((i % 12) + 1);
                ticks.Add(new MultiSound.Sound(soundName, pressBeat + (i * 4f / 48f)));
            }

            //CLICKS
            int numClicks = (type == 0) ? 2 : 4;
            for (int i = 0; i < numClicks; i++)
            {
                string clickSoundName = "screwbotFactory/screw/SE_ROBOT_EN_SCREW_CLICK" + (clickCycle + 1);
                ticks.Add(new MultiSound.Sound(clickSoundName, pressBeat + 0.5f + (i * 0.5f)));
                clickCycle = (clickCycle + 1) % 4;
            }

            //GRIND 
            activeTickSounds = MultiSound.Play(ticks);
            grindLoopSound = SoundByte.PlayOneShotGame("screwbotFactory/screw/SE_ROBOT_EN_SCREW_GRIND_LOOP", pressBeat, 1f, 1f, true);

            game.pauseConveyor = true;
            objectToMove.localPosition = new Vector2(-2.2f, objectToMove.localPosition.y);
            game.currentRobot = this;
            game.ScheduleInput(initBeat + 1, type + 1, ScrewbotFactory.InputAction_RobotRelease, JustRelease, MissRelease, ReleaseEmpty);

            BeatAction.New(game, new List<BeatAction.Action>() {
                new BeatAction.Action(initBeat + type + 3f, delegate {
                    if (!game.shouldBeHolding) game.pauseConveyor = false;
                }),
            });

            
            BeatAction.New(game, new List<BeatAction.Action>() {
                new BeatAction.Action(initBeat + 1.25f, delegate {
                    if (releasedEarly) return;
                    game.clawAnim.DoScaledAnimationAsync("robot_arm_roll_" + (type + 1), 0.57f);
                    robotAnim.DoScaledAnimationAsync("robot_robot_body_" + (type + 1), 0.5f);
                    if (neckAnim != null) 
                    {
                        neckAnim.gameObject.SetActive(true);
                        neckAnim.Update(0f);
                        neckAnim.DoScaledAnimationAsync("robot_robot_neck_roll_" + (type + 1), 0.57f);
                    }
                    if (type == 0 && game.clawHeadW != null)
                    {
                        game.clawHeadW.gameObject.SetActive(true);
                        game.clawHeadW.Update(0f);
                        game.clawHeadW.DoScaledAnimationAsync("robot_robot_head_roll_1", 0.57f);
                    }
                    else if (type == 1 && game.clawHeadG != null)
                    {
                        game.clawHeadG.gameObject.SetActive(true);
                        game.clawHeadG.Update(0f);
                        game.clawHeadG.DoScaledAnimationAsync("robot_robot_head_roll_2", 0.57f);
                    }
                }),
            });

            if (type == 1)
            {
                BeatAction.New(game, new List<BeatAction.Action>() {
                    new BeatAction.Action(initBeat + 2f, delegate {
                        if (releasedEarly) return;
                        if (game.shouldBeHolding)
                            robotAnim.DoScaledAnimationAsync("robot_robot_body_2_beat", 0.5f);
                    }),
                });
            }

            canMove = false;
            game.shouldBeHolding = true;
        }

        private void StopScrewSounds()
        {
            if (activeTickSounds != null)
            {
                activeTickSounds.StopAll(true);
                activeTickSounds = null;
            }
            if (grindLoopSound != null)
            {
                grindLoopSound.KillLoop();
                grindLoopSound = null;
            }
        }

        private void JustRelease(PlayerActionEvent caller, float state)
        {
            if (releasedEarly) return;
            StopScrewSounds();
            if (game.currentRobot == this) game.currentRobot = null;
            
            float pitch = SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-12, 13), false);

            if (state <= -1f) //early release
            {
                if (type == 0 && game.clawHeadW != null) game.clawHeadW.gameObject.SetActive(false);
                else if (type == 1 && game.clawHeadG != null) game.clawHeadG.gameObject.SetActive(false);
                SoundByte.PlayOneShot("miss");
                game.clawAnim.DoScaledAnimationAsync("robot_arm_up", 0.5f);
                robotAnim.DoScaledAnimationAsync("robot_robot_up_miss_f_" + (type + 1), 0.5f);
            }
            else if (state >= 1f) //late release
            {
                if (type == 0 && game.clawHeadW != null) game.clawHeadW.gameObject.SetActive(false);
                else if (type == 1 && game.clawHeadG != null) game.clawHeadG.gameObject.SetActive(false);
                SoundByte.PlayOneShotGame("screwbotFactory/SE_ROBOT_EN_HIBI");
                game.clawAnim.DoScaledAnimationAsync("robot_arm_up", 0.5f);
                robotAnim.DoScaledAnimationAsync("robot_robot_up_miss_b_" + (type + 1), 0.5f);
            }
            else //perfect release
            {
                releaseSuccess = true;
                SoundByte.PlayOneShotGame("screwbotFactory/SE_ROBOT_EN_SCREW_END");
                SoundByte.PlayOneShotGame("screwbotFactory/SE_ROBOT_EN_SCREW_SUCCESS_PITCH12", pitch: pitch);
                game.clawAnim.DoScaledAnimationAsync("robot_arm_up", 0.5f);
                robotAnim.DoScaledAnimationAsync("robot_robot_up_just_" + (type + 1), 0.5f);
            }

            if (releaseSuccess) switch (type)
            {
                case 0:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("screwbotFactory/SE_ROBOT_EN_SCREW_SUCCESS1_PITCHSAME", initBeat + type + 2.5f, pitch: pitch),
                        new MultiSound.Sound("screwbotFactory/SE_ROBOT_EN_SCREW_SUCCESS2_PITCHSAME", initBeat + type + 2.75f, pitch: pitch),
                        new MultiSound.Sound("screwbotFactory/SE_ROBOT_EN_SCREW_SUCCESS3_PITCHSAME", initBeat + type + 3f, pitch: pitch)
                    });
                    break;
                default:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("screwbotFactory/SE_ROBOT_EN_SCREW_SUCCESS_BIG1_PITCHSAME", initBeat + type + 2.5f, pitch: pitch),
                        new MultiSound.Sound("screwbotFactory/SE_ROBOT_EN_SCREW_SUCCESS_BIG2_PITCHSAME", initBeat + type + 3f, pitch: pitch, offset: 0.053f)
                    });
                    break;
            }
            

            BeatAction.New(game, new List<BeatAction.Action>() {
                new BeatAction.Action(initBeat + type + 3f, delegate {
                    if (!game.shouldBeHolding) game.pauseConveyor = false;
                }),
                new BeatAction.Action(initBeat + type + 3f, delegate {
                    if (releaseSuccess) StartBopping(initBeat + type + 3f);
                }),
            });

            canMove = true;
            game.shouldBeHolding = false;
        }

        public void OutEarly()
        {
            if (releasedEarly) return;
            releasedEarly = true;
            StopScrewSounds();
            if (game.currentRobot == this) game.currentRobot = null;
            
            if (type == 0 && game.clawHeadW != null) game.clawHeadW.gameObject.SetActive(false);
            else if (type == 1 && game.clawHeadG != null) game.clawHeadG.gameObject.SetActive(false);

            robotAnim.DoScaledAnimationAsync("robot_robot_up_out_" + (type + 1), 0.5f);
            canMove = true;
            game.shouldBeHolding = false;
        }

        private void MissRelease(PlayerActionEvent caller)
        {
            if (releasedEarly) return;
            
            StopScrewSounds();
            if (game.currentRobot == this) game.currentRobot = null;
            
            game.shouldBeHolding = false; 
            game.clawBusy = true;
            
            SoundByte.PlayOneShotGame("screwbotFactory/SE_ROBOT_EN_YORO");
            robotAnim.DoScaledAnimationAsync("robot_robot_up_through_" + (type + 1), 0.5f);
            
            if (type == 0 && game.clawHeadW != null) game.clawHeadW.gameObject.SetActive(false);
            else if (type == 1 && game.clawHeadG != null) game.clawHeadG.gameObject.SetActive(false);

            float targetRelease = (float)this.initBeat + type + 2f;
            bool botTooClose = false;
            Robot[] bots = FindObjectsOfType<Robot>();
            foreach (Robot r in bots) {
                if (r != this && r.gameObject.activeInHierarchy && r.initBeat > this.initBeat && r.initBeat <= targetRelease + 0.5f) {
                    botTooClose = true;
                    break;
                }
            }
            float holdTime = botTooClose ? 0.5f : 1f; //this probably isnt 100% accurate but
            
            BeatAction.New(game, new List<BeatAction.Action>() {
                new BeatAction.Action(cond.songPositionInBeatsAsDouble + holdTime, delegate {
                    game.clawAnim.DoScaledAnimationAsync("robot_arm_up", 0.5f);
                    robotAnim.DoScaledAnimationAsync("robot_robot_up_through2_" + (type + 1), 0.5f);
                    canMove = true;
                    game.clawBusy = false;
                })
            });
        }

        private void ReleaseEmpty(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShot("miss");
            game.clawAnim.DoScaledAnimationAsync("robot_arm_up", 0.5f);
            OutEarly();
        }

        private void Empty(PlayerActionEvent caller)
        {
            // "There's nothing."
            // "Thank you son."
            //Hi im here too

            StopScrewSounds();
            canMove = true;
            game.shouldBeHolding = false;
        }
    }
}
