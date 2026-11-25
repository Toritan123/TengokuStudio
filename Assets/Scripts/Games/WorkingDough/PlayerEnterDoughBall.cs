using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;

namespace HeavenStudio.Games.Scripts_WorkingDough
{
    public class PlayerEnterDoughBall : SuperCurveObject
    {
        private enum State
        {
            None,
            Entering,
            Hit,
            Barely,
            Miss,
            Weak
        }
        private State currentState;

        private double startBeat = double.MinValue;

        private bool big;

        private Path enterPath;
        private Path hitPath;
        private Path barelyPath;
        private Path missPath;
        private Path weakPath;

        private WorkingDough game;

        private PlayerActionEvent wrongInput;
        private PlayerActionEvent rightInput;

        private Color color1;
        private Color color2;

        bool canJust, canWrong;

        [SerializeField] private GameObject gandw;

        private void Awake()
        {
            game = WorkingDough.instance;
        }

        public void Init(double beat, bool isBig, Color flash, bool hasGandw)
        {
            startBeat = Conductor.instance.GetUnSwungBeat(beat);
            big = isBig;
            canJust = true;
            canWrong = true;
            enterPath = game.GetPath("PlayerEnter");
            hitPath = game.GetPath("PlayerHit");
            barelyPath = game.GetPath("PlayerBarely");
            missPath = game.GetPath("PlayerMiss");
            weakPath = game.GetPath("PlayerWeak");
            color1 = flash;
            color2 = flash;
            color2.a = 0f;
            Debug.Log(color2);
            rightInput = game.ScheduleInput(beat, 1, isBig ? WorkingDough.InputAction_Alt : WorkingDough.InputAction_Nrm, Just, Miss, Empty, CanJust);

            if (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch)
                wrongInput = game.ScheduleUserInput(beat, 1, isBig ? WorkingDough.InputAction_Nrm : WorkingDough.InputAction_Alt, WrongInput, Empty, Empty, CanWrong);

            currentState = State.Entering;
            if (gandw != null) gandw.SetActive(hasGandw);

            Update();
        }

        private void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                if (startBeat > double.MinValue)
                {
                    Vector3 pos = new Vector3();
                    double beat = cond.unswungSongPositionInBeats;
                    switch (currentState)
                    {
                        case State.None:
                            break;
                        case State.Entering:
                            pos = GetPathPositionFromBeat(enterPath, Math.Max(beat, startBeat), startBeat);
                            break;
                        case State.Hit:
                            pos = GetPathPositionFromBeat(hitPath, Math.Max(beat, startBeat), startBeat);
                            if (beat >= startBeat + 1)
                            {
                                Destroy(gameObject);
                            }
                            break;
                        case State.Miss:
                            pos = GetPathPositionFromBeat(missPath, Math.Max(beat, startBeat), startBeat);
                            if (beat >= startBeat + 1)
                            {
                                Destroy(gameObject);
                            }
                            break;
                        case State.Weak:
                            pos = GetPathPositionFromBeat(weakPath, Math.Max(beat, startBeat), startBeat);
                            if (beat >= startBeat + 1)
                            {
                                Destroy(gameObject);
                            }
                            break;
                        case State.Barely:
                            pos = GetPathPositionFromBeat(barelyPath, Math.Max(beat, startBeat), startBeat);
                            if (beat >= startBeat + 2)
                            {
                                Destroy(gameObject);
                            }
                            break;
                    }
                    if (startBeat <= beat) transform.position = pos;
                    else transform.position = new Vector3(-80, -80);
                }
            }
            
        }

        bool CanJust()
        {
            return canJust;
        }

        bool CanWrong()
        {
            return canWrong;
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            if (wrongInput != null)
            {
                canWrong = false;
                wrongInput.Disable();
                wrongInput.CleanUp();
            }
            if (currentState is State.Hit or State.Barely or State.Weak) return;
            double beat = Conductor.instance.unswungSongPositionInBeatsAsDouble;
            startBeat = beat;
            game.playerImpact.SetActive(true);
            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.1, delegate { game.playerImpact.SetActive(false); }),
            });
            if (state >= 1f || state <= -1f)
            {
                currentState = State.Barely;
                SoundByte.PlayOneShot("miss");
                if (big)
            {
                SoundByte.PlayOneShotGame("workingDough/bigPlayer");
                SoundByte.PlayOneShotGame("workingDough/hitBigPlayer");
                game.playerBigAnim.DoScaledAnimationAsync("bun_00_bun_jump_L", 0.5f);
                
                if (!game.playerSmallAnim.GetCurrentAnimatorStateInfo(0).IsName("bun_00_bun_wait_S"))
                    game.playerSmallAnim.DoScaledAnimationAsync("bun_00_bun_wait_S", 0.5f);
                
                WorkingDough.instance.FlashColor(beat, color1, color2);
            }
            else
            {
                SoundByte.PlayOneShotGame("workingDough/smallPlayer");
                SoundByte.PlayOneShotGame("workingDough/hitSmallPlayer");
                game.playerSmallAnim.DoScaledAnimationAsync("bun_00_bun_jump_S", 0.5f);
                
                if (!game.playerBigAnim.GetCurrentAnimatorStateInfo(0).IsName("bun_00_bun_wait_L"))
                    game.playerBigAnim.DoScaledAnimationAsync("bun_00_bun_wait_L", 0.5f);
            }
                Update();
                return;
            }
            currentState = State.Hit;
            if (big)
            {
                SoundByte.PlayOneShotGame("workingDough/bigPlayer");
                SoundByte.PlayOneShotGame("workingDough/hitBigPlayer");
                game.playerBigAnim.DoScaledAnimationAsync("bun_00_bun_jump_L", 0.5f);
                    game.playerSmallAnim.DoScaledAnimationAsync("bun_00_bun_wait_S", 0.5f);
                WorkingDough.instance.FlashColor(beat, color1, color2);
            }
            else
            {
                SoundByte.PlayOneShotGame("workingDough/smallPlayer");
                SoundByte.PlayOneShotGame("workingDough/hitSmallPlayer");
                game.playerSmallAnim.DoScaledAnimationAsync("bun_00_bun_jump_S", 0.5f);
                    game.playerBigAnim.DoScaledAnimationAsync("bun_00_bun_wait_L", 0.5f);
            }
            bool hasGandw = false;
            if (gandw != null) hasGandw = gandw.activeSelf;
            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.9, delegate { game.arrowAnimRightPlayer.Play("bun_00_arrow_out", 0, 0); }),
                new BeatAction.Action(beat + 2, delegate { game.SpawnBGBall(beat + 2f, big, hasGandw); }),
            });
            Update();
        }

        private void WrongInput(PlayerActionEvent caller, float state)
        {
            if (rightInput != null)
            {
                canJust = false;
                rightInput.Disable();
                rightInput.CleanUp();
            }
            if (currentState is State.Hit or State.Barely or State.Weak) return;
            double beat = Conductor.instance.unswungSongPositionInBeats;
            game.playerImpact.SetActive(true);
            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.1f, delegate { game.playerImpact.SetActive(false); }),
            });
            if (big)
            {
                currentState = State.Weak;
                startBeat = beat;
                game.playerSmallAnim.DoScaledAnimationAsync("bun_00_bun_jump_S", 0.5f);
                    game.playerBigAnim.DoScaledAnimationAsync("bun_00_bun_wait_L", 0.5f);
                SoundByte.PlayOneShotGame("workingDough/smallPlayer");
                SoundByte.PlayOneShotGame("workingDough/tooBig");
                Update();
            }
            else
            {
                GameObject.Instantiate(game.breakParticleEffect, game.breakParticleHolder);
                game.playerBigAnim.DoScaledAnimationAsync("bun_00_bun_jump_L", 0.5f);
                    game.playerSmallAnim.DoScaledAnimationAsync("bun_00_bun_wait_S", 0.5f);
                SoundByte.PlayOneShotGame("workingDough/bigPlayer");
                SoundByte.PlayOneShotGame("workingDough/tooSmall");
                Destroy(gameObject);
            }
            game.ScoreMiss();
            game.TriggerGandWMiss(); //Ok
        }

        private void Miss(PlayerActionEvent caller)
        {
            double beat = caller.timer + caller.startBeat;
            currentState = State.Miss;
            startBeat = beat;
            Update();
            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.25f, delegate { game.missImpact.SetActive(true); }),
                new BeatAction.Action(beat + 0.25f, delegate { SoundByte.PlayOneShotGame("workingDough/BallMiss"); }),
                new BeatAction.Action(beat + 0.35f, delegate { game.missImpact.SetActive(false); }),
            });
            game.TriggerGandWMiss();
        }

        private void Empty(PlayerActionEvent caller) { }
    }
}


