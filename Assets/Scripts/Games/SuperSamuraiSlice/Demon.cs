using System;
using UnityEngine;
using System.Collections.Generic;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Debug = UnityEngine.Debug;

namespace HeavenStudio.Games.Scripts_SuperSamuraiSlice
{
    public class Demon : SuperCurveObject
    {
        public enum DemonState
        {
            None,
            RightUp,
            RightDown,
            LeftUp,
            LeftDown,
            Left,
            Right
        }
        DemonState currentState;
        Path currentPath;
        SuperSamuraiSlice game;
        double startBeat;

        bool isDead = false;

        public Animator SmallDemonAnim;
        public Animator MediumDemonAnim;

        int explodeType;
        public GameObject windZone;
        public Transform groundPlane;
        public ParticleSystem Explode1;
        public ParticleSystem Explode2;
        public ParticleSystem Explode3;

        private void Awake()
        {
            game = SuperSamuraiSlice.instance;
        }
        private void Update()
        {
            var cond = Conductor.instance;
            double currentBeat = cond.unswungSongPositionInBeatsAsDouble;

            if (cond.isPlaying && !cond.isPaused && !isDead)
            {
                switch (currentState)
                {
                    default:
                        return;
                    case DemonState.None:
                    case DemonState.RightUp:
                        transform.position = GetPathPositionFromBeat(currentPath, Math.Max(startBeat, currentBeat), startBeat);
                        break;
                    case DemonState.RightDown:
                        transform.position = GetPathPositionFromBeat(currentPath, Math.Max(startBeat, currentBeat), startBeat);
                        break;
                    case DemonState.LeftUp:
                        transform.position = GetPathPositionFromBeat(currentPath, Math.Max(startBeat, currentBeat), startBeat);
                        break;
                    case DemonState.LeftDown:
                        transform.position = GetPathPositionFromBeat(currentPath, Math.Max(startBeat, currentBeat), startBeat);
                        break;
                    case DemonState.Left:
                        transform.position = GetPathPositionFromBeat(currentPath, Math.Max(startBeat, currentBeat), startBeat);
                        break;
                    case DemonState.Right:
                        transform.position = GetPathPositionFromBeat(currentPath, Math.Max(startBeat, currentBeat), startBeat);
                        break;
                }
            }
        }

        public void SetState(DemonState state, double beat)
        {
            isDead = false;
            currentState = state;
            if (game.isSkateboardActive || game.isEagleActive || game.isRocketActive)
            {
                windZone.SetActive(true);
            }
            if (game.isEagleActive || game.isRocketActive)
            {
                groundPlane.localPosition = new Vector2(0f, -10000f); // that oughta do it 
            }
            startBeat = Conductor.instance.GetUnSwungBeat(beat);

            switch (currentState)
            {
                case DemonState.RightUp:
                    currentPath = game.GetPath("rightUp");
                    break;
                case DemonState.RightDown:
                    currentPath = game.GetPath("rightDown");
                    break;
                case DemonState.LeftUp:
                    currentPath = game.GetPath("leftUp");
                    gameObject.transform.Rotate(0.0f, 180.0f, 0.0f);
                    break;
                case DemonState.LeftDown:
                    currentPath = game.GetPath("leftDown");
                    gameObject.transform.Rotate(0.0f, 180.0f, 0.0f);
                    break;
                case DemonState.Left:
                    currentPath = game.GetPath("CurveL");
                    gameObject.transform.Rotate(0.0f, 180.0f, 0.0f);
                    break;
                case DemonState.Right:
                    currentPath = game.GetPath("CurveR");
                    break;
                default:
                    break;
            }

            Update();
        }
        public void SliceDemon(int whichDemon)
        {
            isDead = true;
            if (game.enabledBooms[whichDemon]) SoundByte.PlayOneShotGame("superSamuraiSlice/SE_IAI_NEW_ENEMY_DIE_SMALL", game.boomBeats[whichDemon], forcePlay: true);

            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(game.boomBeats[whichDemon], delegate {
                    SmallDemonAnim.DoScaledAnimationAsync("disappear", 0.5f);
                    switch (explodeType)
                    {
                        case 1:
                            Explode1.Play();
                            break;
                        case 2:
                            Explode2.Play();
                            break;
                        case 3:
                            Explode3.Play();
                            break;
                    }
                }),
                new BeatAction.Action(game.boomBeats[whichDemon] + 16, delegate {
                    Destroy(gameObject);
                }),
            });
        }
        public void Init(double beat, int demons)
        {
            switch (demons)
            {
                case 1:
                    game.ScheduleInput(beat, 2f, SuperSamuraiSlice.InputAction_Press, Just, Miss, null);
                    break;
                case 2:
                    game.ScheduleInput(beat, 3f, SuperSamuraiSlice.InputAction_AltDown, JustHold, MissHold, null);
                    break;
            }
        }

        public void Just(PlayerActionEvent caller, float state)
        {
            if (game.totalEvents >= 1)
            {
                game.slashType++;
                game.demonAnims++;
            }
            game.whichInput++;

            switch (game.slashType)
            {
                case 0:
                    game.SamuraiAnim.GetComponent<Animator>().DoScaledAnimationAsync("Slash00", 0.5f);
                    explodeType = 2;
                    break;
                case 1:
                    game.SamuraiAnim.GetComponent<Animator>().DoScaledAnimationAsync("Slash01", 0.5f);
                    explodeType = 1;
                    break;
                case 2:
                    game.SamuraiAnim.GetComponent<Animator>().DoScaledAnimationAsync("Slash02", 0.5f);
                    explodeType = 1;
                    break;
                case 3:
                    game.SamuraiAnim.GetComponent<Animator>().DoScaledAnimationAsync("Kick", 0.5f);
                    explodeType = 3;
                    break;
                case 4:
                    game.SamuraiAnim.GetComponent<Animator>().DoScaledAnimationAsync("Punch", 0.5f);
                    explodeType = 1;
                    break;
            }

            SliceDemon(game.whichInput);

            if (state <= -1f || state >= 1f)
            {
                SmallDemonAnim.DoScaledAnimationAsync("Break_Miss", 0.5f);
                SoundByte.PlayOneShotGame("superSamuraiSlice/SE_IAI_NEW_OSII");
            }
            else
            {
                switch (game.demonAnims)
                {
                    case 0:
                        SoundByte.PlayOneShotGame("superSamuraiSlice/SE_IAI_NEW_HIT1");
                        SmallDemonAnim.DoScaledAnimationAsync("Break_H", 0.5f);
                        break;
                    case 1:
                        SoundByte.PlayOneShotGame("superSamuraiSlice/SE_IAI_NEW_HIT1");
                        SmallDemonAnim.DoScaledAnimationAsync("Break_V", 0.5f);
                        break;
                    case 2:
                        SoundByte.PlayOneShotGame("superSamuraiSlice/SE_IAI_NEW_HIT2");
                        SmallDemonAnim.DoScaledAnimationAsync("Break_V", 0.5f);
                        break;
                    case 3:
                        SoundByte.PlayOneShotGame("superSamuraiSlice/SE_IAI_NEW_HIT_KICK");
                        SmallDemonAnim.DoScaledAnimationAsync("Break_K", 0.5f);
                        break;
                    case 4:
                        SoundByte.PlayOneShotGame("superSamuraiSlice/SE_IAI_NEW_KIAI_BARRIER_BASE");
                        SoundByte.PlayOneShotGame("superSamuraiSlice/SE_IAI_NEW_KIAI_BARRIER_SHOCK", UnityEngine.Random.Range(0f, 1.7f));
                        SmallDemonAnim.DoScaledAnimationAsync("Break_P", 0.5f);
                        game.lightning.Play();
                        break;
                }
            }
        }

        private void Miss(PlayerActionEvent caller)
        {
            game.whichInput++;
            SoundByte.PlayOneShotGame("superSamuraiSlice/SE_IAI_NEW_YARARE1");
            game.SamuraiAnim.GetComponent<Animator>().DoScaledAnimationAsync("Slash_Through_R", 0.5f);
            Destroy(gameObject);
        }

        private void Empty(PlayerActionEvent caller) { }

        private void JustHold(PlayerActionEvent caller, float state)
        {
            if (game.isSkateboardActive)
            {
                switch (game.direction)
                {
                    case 0:
                        game.SkateboardAnim.GetComponent<Animator>().DoScaledAnimationAsync("guard_FromR", 0.5f);
                        break;
                    case 1:
                        game.SkateboardAnim.GetComponent<Animator>().DoScaledAnimationAsync("guard_FromL", 0.5f);
                        break;
                }
            }
            if (game.isEagleActive)
            {
                switch (game.direction)
                {
                    case 0:
                        game.EagleAnim.GetComponent<Animator>().DoScaledAnimationAsync("fromR", 0.5f);
                        break;
                    case 1:
                        game.EagleAnim.GetComponent<Animator>().DoScaledAnimationAsync("fromL", 0.5f);
                        break;
                }
            }
            var beat = caller.startBeat + caller.timer;

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("superSamuraiSlice/SE_IAI_NEW_GUARD", beat),
                new MultiSound.Sound("superSamuraiSlice/SE_IAI_NEW_GUARD", beat + 0.5),
                new MultiSound.Sound("superSamuraiSlice/SE_IAI_NEW_GUARD", beat + 0.75),
                new MultiSound.Sound("superSamuraiSlice/SE_IAI_NEW_GUARD", beat + 1),
                new MultiSound.Sound("superSamuraiSlice/SE_IAI_NEW_GUARD", beat + 1.5),
            }, forcePlay: true);

            game.SamuraiAnim.GetComponent<Animator>().DoScaledAnimationAsync("Guard", 0.5f);
            MediumDemonAnim.DoScaledAnimationAsync("Attack", 0.5f);

            game.ScheduleInput(beat, 2f, SuperSamuraiSlice.InputAction_AltUp, JustRelease, MissRelease, null);
        }

        private void MissHold(PlayerActionEvent caller)
        {
            if (game.isSkateboardActive)
            {
                switch (game.direction)
                {
                    case 0:
                        game.SkateboardAnim.GetComponent<Animator>().DoScaledAnimationAsync("guard_FromR", 0.5f);
                        break;
                    case 1:
                        game.SkateboardAnim.GetComponent<Animator>().DoScaledAnimationAsync("guard_FromL", 0.5f);
                        break;
                }
            }
            if (game.isEagleActive)
            {
                switch (game.direction)
                {
                    case 0:
                        game.EagleAnim.GetComponent<Animator>().DoScaledAnimationAsync("fromR", 0.5f);
                        break;
                    case 1:
                        game.EagleAnim.GetComponent<Animator>().DoScaledAnimationAsync("fromL", 0.5f);
                        break;
                }
            }
            var beat = caller.startBeat + caller.timer;

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("superSamuraiSlice/SE_IAI_NEW_YARARE1", beat),
                new MultiSound.Sound("superSamuraiSlice/SE_IAI_NEW_YARARE1", beat + 0.5),
                new MultiSound.Sound("superSamuraiSlice/SE_IAI_NEW_YARARE1", beat + 0.75),
                new MultiSound.Sound("superSamuraiSlice/SE_IAI_NEW_YARARE1", beat + 1),
                new MultiSound.Sound("superSamuraiSlice/SE_IAI_NEW_YARARE1", beat + 1.5),
            }, forcePlay: true);

            game.SamuraiAnim.GetComponent<Animator>().DoScaledAnimationAsync("Guard_Through", 0.5f);
            MediumDemonAnim.DoScaledAnimationAsync("Attack", 0.5f);

            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer + 2, delegate {
                    game.isLargeDemonActive = false;
                    if (game.isSkateboardActive)
                    {
                        game.SkateboardAnim.GetComponent<Animator>().DoScaledAnimationAsync("idle", 0.5f);
                    }
                    if (game.isEagleActive)
                    {
                        game.EagleAnim.GetComponent<Animator>().DoScaledAnimationAsync("idle", 0.5f);
                    }
                    game.SamuraiAnim.GetComponent<Animator>().DoScaledAnimationAsync("Counter_Through", 0.5f);
                    MediumDemonAnim.DoScaledAnimationAsync("GoAway", 0.5f);
                    SoundByte.PlayOneShotGame("superSamuraiSlice/SE_IAI_NEW_YARARE1", forcePlay: true);
                }),
            });
        }

        private void JustRelease(PlayerActionEvent caller, float state)
        {
            game.isLargeDemonActive = false;
            if (game.isSkateboardActive)
            {
                game.SkateboardAnim.GetComponent<Animator>().DoScaledAnimationAsync("idle", 0.5f);
            }
            if (game.isEagleActive)
            {
                game.EagleAnim.GetComponent<Animator>().DoScaledAnimationAsync("idle", 0.5f);
            }

            var beat = caller.startBeat + caller.timer;

            game.SamuraiAnim.GetComponent<Animator>().DoScaledAnimationAsync("Counter", 0.5f);
            game.flash.GetComponent<Animator>().Play("flash");
            MediumDemonAnim.DoScaledAnimationAsync("Break", 0.5f);

            SoundByte.PlayOneShotGame("superSamuraiSlice/SE_IAI_NEW_HIT_MID", forcePlay: true);
            if (game.enableBoom2) SoundByte.PlayOneShotGame("superSamuraiSlice/SE_IAI_NEW_ENEMY_DIE_MID", caller.startBeat + caller.timer + game.boombeat2, forcePlay: true);

            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer + 1 , delegate {
                    game.flash.GetComponent<Animator>().Play("idle");
                }),
                new BeatAction.Action(caller.startBeat + caller.timer + game.boombeat2 , delegate {
                    MediumDemonAnim.DoScaledAnimationAsync("disappear", 0.5f);
                    Explode1.Play();
                }),
                new BeatAction.Action(caller.startBeat + caller.timer + game.boombeat2 + 16 , delegate {
                    Destroy(gameObject);
                }),
            });
        }

        private void MissRelease(PlayerActionEvent caller)
        {
            game.isLargeDemonActive = false;
            if (game.isSkateboardActive)
            {
                game.SkateboardAnim.GetComponent<Animator>().DoScaledAnimationAsync("idle", 0.5f);
            }
            if (game.isEagleActive)
            {
                game.EagleAnim.GetComponent<Animator>().DoScaledAnimationAsync("idle", 0.5f);
            }
            var beat = caller.startBeat + caller.timer;
            game.SamuraiAnim.GetComponent<Animator>().DoScaledAnimationAsync("Counter_Through", 0.5f);
            MediumDemonAnim.DoScaledAnimationAsync("GoAway", 0.5f);
            SoundByte.PlayOneShotGame("superSamuraiSlice/SE_IAI_NEW_YARARE1", forcePlay: true);

            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer + 1, delegate {
                    Destroy(gameObject);
                }),
            });
        }
    }
}