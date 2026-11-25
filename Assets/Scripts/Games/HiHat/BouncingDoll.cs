using HeavenStudio.Games.Scripts_MeatGrinder;
using HeavenStudio.InputSystem;
using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HeavenStudio.Games.Scripts_HiHat
{
    public class BouncingDoll : MonoBehaviour
    {
        public bool isBig = false;
        public double hitBeat;
        public bool willDespawn = false;

        public double interval = 4;
        public int patternType = 0;
        public bool hasHit = false;
        public bool canHit = false;
        public bool canBounce = false;
        public bool hasBounced = false;
        public bool missed = false;
        public bool isFirst = false;
        public bool behind = false;
        public bool shadowHigher = false;

        public Animator anim;
        public SortingGroup sorting;
        public GameObject shadow;
        public GameObject boundEffect;
        public GameObject keyContainer;
        private Vector3 defaultShadowPosition;

        public HiHat game;
        public PlayerActionEvent input;
        private void Awake()
        {
            if (shadowHigher) shadow.transform.localPosition += new Vector3(0.0f, 0.23f);

            defaultShadowPosition = shadow.transform.localPosition;
            if (behind) sorting.sortingOrder = 35;

            StartTiming(hitBeat);
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            bool isOpen = true;
            if (game.hiHatAnim.IsPlayingAnimationNames("hihat_open_00", "hihat_open_shot_00", "hihat_open_miss_00")) isOpen = true;
            else if (game.hiHatAnim.IsPlayingAnimationNames("hihat_close_00", "hihat_close_shot_00", "hihat_close_miss_00") || game.isClosed) isOpen = false;
            
            if (!isOpen) shadow.transform.localPosition = defaultShadowPosition;
            else shadow.transform.localPosition = defaultShadowPosition + new Vector3(0.0f, 0.23f);

            if (!hasHit && canHit && input != null)
            {
                if (!isBig && PlayerInput.GetIsAction(HiHat.InputAction_BasicPress) && input.IsExpectingInputNow(Conductor.instance))
                {
                    input.Hit(0, 1); // input manual hit. its always gonna be an ace because i honestly don't know how to get the proper timing for it
                    hasHit = true;
                }
                if (isBig && (PlayerInput.GetIsAction(HiHat.InputAction_BasicRelease) || PlayerInput.GetIsAction(HiHat.InputAction_FlickRelease)) && input.IsExpectingInputNow(Conductor.instance))
                {
                    input.Hit(0, 1);
                    hasHit = true;
                }
            }

            if (canHit && hasHit && !hasBounced && !willDespawn) canBounce = true;

            if (isBig && canBounce)
            {
                BeatAction.New(game, new()
                {
                    new(hitBeat, delegate {
                        anim.DoScaledAnimationAsync("dwarf_doll_L", 0.375f);
                        hasBounced = true;
                    }),
                });

                canBounce = false;
            }
            else if (!isBig && canBounce)
            {
                BeatAction.New(game, new()
                {
                    new(hitBeat, delegate {
                        anim.DoScaledAnimationAsync("dwarf_doll_S", 0.375f);
                        hasBounced = true;
                    }),
                });

                canBounce = false;
            }
        }

        private void StartTiming(double beat)
        {
            if (!isBig)
            {
                BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat - 1, delegate 
                    { 
                        if (isFirst) game.ScheduleAutoplayInput(beat - 1, 1f, HiHat.InputAction_None, game.AutoClose, null, null); 
                        input = game.ScheduleInput(beat - 1, 1f, HiHat.InputAction_None, game.HitSmall, Miss, null, null); 

                        /* temp reduce input weight. planned formula is as follows:

                        - first bounce gives 0.2 of a normal input
                        - last bounce gives 0.3 of a normal input
                        - inbetween bounces give 1/(number of inbetween bounces) each
                        totalling to 1.6 inputs */
                        if (!isFirst) input.weight = 0.1f; 
                    }),
                    new BeatAction.Action(beat, delegate 
                    {
                        if ((game.NextPattern(beat)?.beat <= beat + interval) || (game.NextDoll(beat)?.beat <= beat + interval && patternType != 0))
                        {
                            Despawn(beat);
                        }

                        canHit = true;
                        if (game.isClosed && !hasHit && input.canHit && !GameManager.instance.Autoplay)
                        {
                            input.Hit(0, 1); // input auto hit if applicable
                            hasHit = true;
                        }
                        if (GameManager.instance.Autoplay && !willDespawn) canBounce = true;
                        hitBeat = beat;
                    }),
                    new BeatAction.Action(beat + interval - 1, delegate 
                    {
                        if (game.nextGameSwitch > beat + interval)
                        {
                            StartTiming(beat + interval);
                            canHit = false;
                            hasHit = false;
                            hasBounced = false;
                        }
                    }),
                });
            }
            else
            {
                BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat - 1, delegate { input = game.ScheduleInput(beat - 1, 1f, HiHat.InputAction_None, game.Hit, Miss, null, null); }),
                    new BeatAction.Action(beat, delegate 
                    {
                        if ((game.NextPattern(beat)?.beat <= beat + interval) || (game.NextDoll(beat)?.beat <= beat + interval && patternType != 0))
                        {
                            Despawn(beat);
                        }

                        canHit = true;
                        if (!game.isClosed && !hasHit && input.canHit && !GameManager.instance.Autoplay)
                        {
                            input.Hit(0, 1);
                            hasHit = true;
                        }
                        if (GameManager.instance.Autoplay && !willDespawn) canBounce = true;
                        hitBeat = beat;
                    }),
                    new BeatAction.Action(beat + interval - 1, delegate 
                    {
                        if (game.nextGameSwitch > beat + interval)
                        {
                            StartTiming(beat + interval);
                            canHit = false;
                            hasHit = false;
                            hasBounced = false;
                        }
                    }),
                });
            }
        }

        public void Despawn(double beat)
        {
            willDespawn = true;
            BeatAction.New(game, new()
            {
                new(beat, delegate {
                    if (isBig) anim.DoScaledAnimationAsync("dwarf_doll_L_despawn", 0.5f);
                    else anim.DoScaledAnimationAsync("dwarf_doll_S_despawn", 0.5f);
                }),
            });

            BeatAction.New(game, new()
            {
                new(beat + 0.75f, delegate {
                    Destroy(gameObject);
                }),
            });
        }

        public void Miss(PlayerActionEvent caller)
        {
            hasHit = false;
            canHit = false;
            missed = true;

            BeatAction.New(game, new()
            {
                new(hitBeat, delegate {
                    if (isBig) anim.DoScaledAnimationAsync(transform.localPosition.x > 0f ? "dwarf_doll_L_miss_right" : "dwarf_doll_L_miss_left", 0.5f);
                    else anim.DoScaledAnimationAsync(transform.localPosition.x > 0f ? "dwarf_doll_S_miss_right" : "dwarf_doll_S_miss_left", 0.5f);
                }),
            });

            BeatAction.New(game, new()
            {
                new(hitBeat + interval - 1, delegate {
                    game.SpawnDoll(hitBeat + interval, isBig, transform.localPosition, patternType, isFirst, behind, shadowHigher, interval);
                    Destroy(gameObject);
                }),
            });

            game.Miss(caller);
        }
    }
}
