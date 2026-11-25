using System;
using System.Collections.Generic;
using UnityEngine;
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
            SpiderWrong,
            SpiderHit // not a path just used for tracking
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

        [SerializeField] private Sprite[] candySprites;
        [SerializeField] private SpriteRenderer objSprite;
        [SerializeField] private GameObject wrapperObjs;

        float rotSpeed = 180f;
        public bool Rotation;

        public bool spider;
        public int sprite;
        [NonSerialized] public bool player;

        private PackingPests game;
        public EmployeeLayer layer;

        private PlayerActionEvent wrongInput;
        private PlayerActionEvent rightInput;
        bool canJust, canWrong;

        private void Awake()
        {
            game = PackingPests.instance;
        }

        public void InitCandy(double beat)
        {
            startBeat = Conductor.instance.GetUnSwungBeat(beat);
            canJust = true;
            canWrong = true;

            candyThrowPath = layer.GetPath("candyThrow");
            candyCatchPath = layer.GetPath("candyCatch");
            candyBarelyPath = layer.GetPath("candyBarely");
            candyWrongPath = layer.GetPath("candyWrong");
            nonePath = layer.GetPath("None");

            if (wrapperObjs != null) wrapperObjs.SetActive(sprite == 0);
            if (objSprite != null) objSprite.sprite = candySprites[sprite];

            if (player)
            {
                rightInput = game.ScheduleInput(beat, 1, PackingPests.InputAction_Alt, Just, Miss, Empty, CanJust);

                if (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch)
                    wrongInput = game.ScheduleUserInput(beat, 1, PackingPests.InputAction_BasicPress, WrongInput, Empty, Empty, CanWrong);
            }
            else
            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1, delegate { 
                    Hit(0);
                }),
            });


            currentState = State.CandyThrow;
            LateUpdate();
        }

        public void InitSpider(double beat)
        {
            startBeat = Conductor.instance.GetUnSwungBeat(beat);
            canJust = true;
            canWrong = true;

            spiderThrowPath = layer.GetPath("spiderThrow");
            spiderBarelyPath = layer.GetPath("spiderBarely");
            spiderWrongPath = layer.GetPath("spiderWrong");
            nonePath = layer.GetPath("None");

            if (player)
            {
                rightInput = game.ScheduleInput(beat, 1, PackingPests.InputAction_BasicPress, Just, Miss, Empty, CanJust);

                if (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch)
                    wrongInput = game.ScheduleUserInput(beat, 1, PackingPests.InputAction_Alt, WrongInput, Empty, Empty, CanWrong);
            }
            else
            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1, delegate { 
                    Hit(0);
                }),
            });

            currentState = State.SpiderThrow;
            LateUpdate();
        }

        private void LateUpdate()
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
                            if (beat >= startBeat + 1.5)
                            {
                                Destroy(gameObject);
                            }
                            break;
                        case State.CandyBarely:
                            pos = GetPathPositionFromBeat(candyBarelyPath, Math.Max(beat, startBeat), startBeat);
                            if (beat >= startBeat + 1.5)
                            {
                                Destroy(gameObject);
                            }
                            break;
                        case State.SpiderBarely:
                            pos = GetPathPositionFromBeat(spiderBarelyPath, Math.Max(beat, startBeat), startBeat);
                            if (beat >= startBeat + 1.5)
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
                        case State.SpiderHit:
                            pos = GetPathPositionFromBeat(nonePath, Math.Max(beat, startBeat), startBeat);
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
            Hit(state);
        }

        public void Hit(float state)
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
                layer.boxfront.GetComponent<Renderer>().sortingOrder = -2;
                if(spider)
                {
                    currentState = State.SpiderBarely;
                    layer.spiderAnim.DoScaledAnimationAsync("Enter", 0.5f);
                    layer.upperHandAnim.DoScaledAnimationAsync("HitOut", 0.5f);
                    layer.headAnim.DoScaledAnimationAsync("Hit", 0.5f);
                    if (player) SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_OSII");
                }
                else
                {
                    currentState = State.CandyBarely;
                    layer.upperHandAnim.DoScaledAnimationAsync("CatchOut", 0.5f);
                    layer.lowerHandAnim.DoScaledAnimationAsync("CatchOut", 0.5f);
                    layer.headAnim.DoScaledAnimationAsync("Catch", 0.5f);
                    if (player) SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_OSII");
                }
                LateUpdate();
                return;
            }

            layer.boxfront.GetComponent<Renderer>().sortingOrder = 1;
            if(spider)
            {
                currentState = State.SpiderHit;
                layer.spiderCrawlAnim.DoScaledAnimationAsync("Hit", 0.5f);
                layer.upperHandAnim.DoScaledAnimationAsync("HitJust", 0.5f);
                layer.headAnim.DoScaledAnimationAsync("Hit", 0.5f);

                if (player)
                {
                    SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_INSECT_ATTACK_A");
                    SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_INSECT_ATTACK_B");
                }
            }
            else
            {
                currentState = State.None;
                if (player)
                {
                    SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_BALL_CATCH_A");
                    SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_BALL_CATCH_B");
                    SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_BALL_CATCH_C");
                }

                BeatAction.New(game, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 0, delegate { 
                        if (sprite == 0)
                        {
                            layer.upperHandAnim.DoScaledAnimationAsync("CatchCandy", 0.5f);
                            layer.lowerHandAnim.DoScaledAnimationAsync("CatchJust00", 0.5f);
                            layer.headAnim.DoScaledAnimationAsync("Catch", 0.5f);
                        }
                        else
                        {
                            layer.upperHandAnim.DoScaledAnimationAsync("CatchJust01", 0.5f);
                            layer.lowerHandAnim.DoScaledAnimationAsync("CatchJust00", 0.5f);
                            layer.headAnim.DoScaledAnimationAsync("Catch", 0.5f);
                        }

                        layer.lowerHandObj.sprite = candySprites[sprite];
                        
                    }),
                    new BeatAction.Action(beat + 0.5, delegate { 
                        layer.upperHandAnim.DoScaledAnimationAsync("CatchJust02", 0.5f);
                        layer.lowerHandAnim.DoScaledAnimationAsync("CatchJust01", 0.5f);
                        currentState = State.CandyCatch;
                    }),
                });
            }
            LateUpdate();
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
                layer.boxfront.GetComponent<Renderer>().sortingOrder = -2;
                currentState = State.None;

                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_MISS_INSECT_CATCH_A");
                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_MISS_INSECT_CATCH_B");

                BeatAction.New(game, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.startBeat + caller.timer + 0, delegate { 
                        layer.upperHandAnim.DoScaledAnimationAsync("CatchBug", 0.5f);
                        layer.lowerHandAnim.DoScaledAnimationAsync("CatchBug", 0.5f);
                        layer.headAnim.DoScaledAnimationAsync("Damage", 0.5f);
                    }),
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.5, delegate { 
                        layer.spiderAnim.DoScaledAnimationAsync("Enter", 0.5f);
                        layer.upperHandAnim.DoScaledAnimationAsync("CatchMiss", 0.5f);
                        layer.lowerHandAnim.DoScaledAnimationAsync("CatchMiss", 0.5f);
                        currentState = State.SpiderWrong;
                    }),
                });
            }
            else
            {
                layer.boxfront.GetComponent<Renderer>().sortingOrder = -2;
                layer.upperHandAnim.DoScaledAnimationAsync("HitOut", 0.5f);
                layer.headAnim.DoScaledAnimationAsync("Hit", 0.5f);

                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_MISS_BALL_ATTACK_A");
                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_MISS_BALL_ATTACK_B");
                SoundByte.PlayOneShot("miss");

                currentState = State.CandyWrong;
            }
            game.ScoreMiss();
            LateUpdate();
        }
        private void Miss(PlayerActionEvent caller)
        {
            if(spider)
            {
                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_MISS_INSECT_THROUGH_A");
                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_MISS_INSECT_THROUGH_B");
                layer.upperHandAnim.DoScaledAnimationAsync("Damage", 0.5f);
                layer.lowerHandAnim.DoScaledAnimationAsync("Damage", 0.5f);
                layer.headAnim.DoScaledAnimationAsync("Damage", 0.5f);
            }
            else
            {
                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_MISS_BALL_THROUGH_A");
                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_MISS_BALL_THROUGH_B");

                layer.handAnim.DoScaledAnimationAsync("Through", 0.5f);
                layer.upperHandAnim.DoScaledAnimationAsync("Through", 0.5f);
                layer.lowerHandAnim.DoScaledAnimationAsync("Through", 0.5f);
                layer.headAnim.DoScaledAnimationAsync("Through", 0.5f);
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