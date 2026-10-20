using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using NaughtyBezierCurves;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;

namespace HeavenStudio.Games.Scripts_PackingPests
{
    public class ThrowObject : SuperCurveObject
    {   
        private enum State
        {
            None,
            CandyThrow,
            SpiderThrow,
            CandyCatch,
            CandyBarely,
            SpiderBarely,
            CandyWrong,
            SpiderWrong
        }
        private State currentState;
        private double startBeat = double.MinValue;
        private Path candyThrowPath;
        private Path spiderThrowPath;
        private Path candyCatchPath;
        private Path candyBarelyPath;
        private Path spiderBarelyPath;
        private Path candyWrongPath;
        private Path spiderWrongPath;
        private Path nonePath;

        float rotSpeed = 180f;
        public bool Rotation;

        public bool spider;
        public bool isSpider;

        private PackingPests game;

        private PlayerActionEvent wrongInput;
        private PlayerActionEvent rightInput;
        bool canJust, canWrong;

        private void Awake()
        {
            game = PackingPests.instance;
        }

        public void Init(double beat)
        {
            startBeat = Conductor.instance.GetUnSwungBeat(beat);
            spider = isSpider;
            canJust = true;
            canWrong = true;

            candyThrowPath = game.GetPath("candyThrow");
            candyCatchPath = game.GetPath("candyCatch");
            candyBarelyPath = game.GetPath("candyBarely");
            candyWrongPath = game.GetPath("candyWrong");
            nonePath = game.GetPath("None");

            rightInput = game.ScheduleInput(beat, 1, spider ? PackingPests.InputAction_BasicPress : PackingPests.InputAction_Alt, Just, Miss, Empty, CanJust);

            if (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch)
                wrongInput = game.ScheduleUserInput(beat, 1, spider ? PackingPests.InputAction_Alt : PackingPests.InputAction_BasicPress, WrongInput, Empty, Empty, CanWrong);

            currentState = State.CandyThrow;
            Update();
        }
        public void Init2(double beat)
        {
            startBeat = Conductor.instance.GetUnSwungBeat(beat);
            spider = isSpider;
            canJust = true;
            canWrong = true;

            spiderThrowPath = game.GetPath("spiderThrow");
            spiderBarelyPath = game.GetPath("spiderBarely");
            spiderWrongPath = game.GetPath("spiderWrong");
            nonePath = game.GetPath("None");

            rightInput = game.ScheduleInput(beat, 1, spider ? PackingPests.InputAction_Alt : PackingPests.InputAction_BasicPress, Just, Miss, Empty, CanJust);

            if (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch)
                wrongInput = game.ScheduleUserInput(beat, 1, spider ? PackingPests.InputAction_BasicPress : PackingPests.InputAction_Alt, WrongInput, Empty, Empty, CanWrong);

            currentState = State.SpiderThrow;
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
                            pos = GetPathPositionFromBeat(nonePath, Math.Max(beat, startBeat), startBeat);
                            break;
                        case State.CandyThrow:
                            pos = GetPathPositionFromBeat(candyThrowPath, Math.Max(beat, startBeat), startBeat);
                            break;
                        case State.SpiderThrow:
                            pos = GetPathPositionFromBeat(spiderThrowPath, Math.Max(beat, startBeat), startBeat);
                            break;
                        case State.CandyCatch:
                            pos = GetPathPositionFromBeat(candyCatchPath, Math.Max(beat, startBeat), startBeat);
                            rotSpeed = -60f;
                            if (beat >= startBeat + 2)
                            {
                                Destroy(gameObject);
                            }
                            break;
                        case State.CandyBarely:
                            pos = GetPathPositionFromBeat(candyBarelyPath, Math.Max(beat, startBeat), startBeat);
                            if (beat >= startBeat + 2)
                            {
                                Destroy(gameObject);
                            }
                            break;
                        case State.SpiderBarely:
                            pos = GetPathPositionFromBeat(spiderBarelyPath, Math.Max(beat, startBeat), startBeat);
                            if (beat >= startBeat + 2)
                            {
                                Destroy(gameObject);
                            }
                            break;
                        case State.CandyWrong:
                            pos = GetPathPositionFromBeat(candyWrongPath, Math.Max(beat, startBeat), startBeat);
                            if (beat >= startBeat + 6)
                            {
                                Destroy(gameObject);
                            }
                            break;
                        case State.SpiderWrong:
                            rotSpeed = 0f;
                            pos = GetPathPositionFromBeat(spiderWrongPath, Math.Max(beat, startBeat), startBeat);
                            if (beat >= startBeat + 6)
                            {
                                Destroy(gameObject);
                            }
                            break;
                    }
                    if (startBeat <= beat) transform.position = pos;
                    else transform.position = new Vector3(-80, -80);

                    float rot = Rotation ? rotSpeed : -rotSpeed;
                    transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (rot * Time.deltaTime / cond.pitchedSecPerBeat));
                }
            }
        }
        private void Just(PlayerActionEvent caller, float state)
        {
            if (wrongInput != null)
            {
                canWrong = false;
                wrongInput.Disable();
                wrongInput.CleanUp();
            }
            if (currentState is State.CandyCatch or State.CandyBarely) return;
            double beat = Conductor.instance.unswungSongPositionInBeatsAsDouble;
            startBeat = beat;

            if (state >= 1f || state <= -1f)
            {
                game.boxfront.GetComponent<Renderer>().sortingOrder = -2;
                if(spider)
                {
                    currentState = State.SpiderBarely;
                    game.spiderAnim.GetComponent<Animator>().DoScaledAnimationAsync("Enter", 0.5f);
                    game.upperHandAnim.GetComponent<Animator>().DoScaledAnimationAsync("HitOut", 0.5f);
                    SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_OSII");
                }
                else
                {
                    currentState = State.CandyBarely;
                    game.upperHandAnim.GetComponent<Animator>().DoScaledAnimationAsync("CatchOut", 0.5f);
                    game.lowerHandAnim.GetComponent<Animator>().DoScaledAnimationAsync("CatchOut", 0.5f);
                    SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_OSII");
                }
                Update();
                return;
            }

            game.boxfront.GetComponent<Renderer>().sortingOrder = 1;
            if(spider)
            {
                currentState = State.None;
                game.spiderCrawlAnim.GetComponent<Animator>().DoScaledAnimationAsync("Hit", 0.5f);
                game.upperHandAnim.GetComponent<Animator>().DoScaledAnimationAsync("HitJust", 0.5f);

                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_INSECT_ATTACK_A");
                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_INSECT_ATTACK_B");
            }
            else
            {
                currentState = State.None;
                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_BALL_CATCH_A");
                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_BALL_CATCH_B");
                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_BALL_CATCH_C");

                BeatAction.New(game, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.startBeat + caller.timer + 0, delegate { 
                        game.upperHandAnim.GetComponent<Animator>().DoScaledAnimationAsync("CatchCandy", 0.5f);
                        game.lowerHandAnim.GetComponent<Animator>().DoScaledAnimationAsync("CatchJust00", 0.5f);
                    }),
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.5, delegate { 
                        game.upperHandAnim.GetComponent<Animator>().DoScaledAnimationAsync("CatchJust02", 0.5f);
                        game.lowerHandAnim.GetComponent<Animator>().DoScaledAnimationAsync("CatchJust01", 0.5f);
                        currentState = State.CandyCatch;
                    }),
                });
            }
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
            if (currentState is State.CandyCatch or State.CandyBarely) return;
            double beat = Conductor.instance.unswungSongPositionInBeatsAsDouble;

            if(spider)
            {
                game.boxfront.GetComponent<Renderer>().sortingOrder = -2;
                currentState = State.None;

                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_MISS_INSECT_CATCH_A");
                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_MISS_INSECT_CATCH_B");

                BeatAction.New(game, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.startBeat + caller.timer + 0, delegate { 
                        game.upperHandAnim.GetComponent<Animator>().DoScaledAnimationAsync("CatchBug", 0.5f);
                        game.lowerHandAnim.GetComponent<Animator>().DoScaledAnimationAsync("CatchBug", 0.5f);
                    }),
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.5, delegate { 
                        game.spiderAnim.GetComponent<Animator>().DoScaledAnimationAsync("Enter", 0.5f);
                        game.upperHandAnim.GetComponent<Animator>().DoScaledAnimationAsync("CatchMiss", 0.5f);
                        game.lowerHandAnim.GetComponent<Animator>().DoScaledAnimationAsync("CatchMiss", 0.5f);
                        currentState = State.SpiderWrong;
                    }),
                });
            }
            else
            {
                game.boxfront.GetComponent<Renderer>().sortingOrder = -2;
                game.upperHandAnim.GetComponent<Animator>().DoScaledAnimationAsync("HitOut", 0.5f);

                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_MISS_BALL_ATTACK_A");
                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_MISS_BALL_ATTACK_B");
                SoundByte.PlayOneShot("miss");

                currentState = State.CandyWrong;
            }
            game.ScoreMiss();
            Update();
        }
        private void Miss(PlayerActionEvent caller)
        {
            if(spider)
            {
                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_MISS_INSECT_THROUGH_A");
                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_MISS_INSECT_THROUGH_B");
                game.upperHandAnim.GetComponent<Animator>().DoScaledAnimationAsync("Damage", 0.5f);
                game.lowerHandAnim.GetComponent<Animator>().DoScaledAnimationAsync("Damage", 0.5f);
            }
            else
            {
                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_MISS_BALL_THROUGH_A");
                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_MISS_BALL_THROUGH_B");

                game.handAnim.GetComponent<Animator>().DoScaledAnimationAsync("Through", 0.5f);
                game.upperHandAnim.GetComponent<Animator>().DoScaledAnimationAsync("Through", 0.5f);
                game.lowerHandAnim.GetComponent<Animator>().DoScaledAnimationAsync("Through", 0.5f);
            }
        }
        private void Empty(PlayerActionEvent caller) {}

        bool CanJust()
        {
            return canJust;
        }

        bool CanWrong()
        {
            return canWrong;
        }
    }
}