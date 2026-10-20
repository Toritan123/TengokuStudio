using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_SamuraiSliceRvl
{
    public class Demon : MonoBehaviour
    {
        [Header("Components")]
        public SamuraiSliceRvl game;
        [SerializeField] Animator SDemonAnim;
        [SerializeField] Animator MDemonAnim;
        [SerializeField] Animator LDemonAnim;
        [SerializeField] Animator XLDemonAnim;
        public DemonType type;

        [Header("Sprites Renderers")]
        public SpriteRenderer Sdemon;
        public SpriteRenderer Mdemon;
        public SpriteRenderer Ldemon;
        public SpriteRenderer XLdemon;

        [Header("Properties")]
        public double startBeat;
        private double missBeat;
        private double walkBeat;

        public bool isSpawning;
        private bool isMissing;
        private bool isWalking;

        [Header("Curves")]
        public BezierCurve3D spawnCurve;
        public BezierCurve3D missCurve;
        public BezierCurve3D walkCurve;

        void Start()
        {
            game = SamuraiSliceRvl.instance;
            DemonsType(type, "Summon");
            isSpawning = false; // Don't start moving immediately
            // Schedule the start of the movement to happen 1 beat after creation
            BeatAction.New(this, new List<BeatAction.Action>() {
                new BeatAction.Action(startBeat + 1, () => {
                    DemonsType(type, "Idle");
                    isSpawning = true;
                })
            });

            // The input is still scheduled relative to the original startBeat
            game.ScheduleInput(startBeat, 2f, SamuraiSliceRvl.InputAction_FlickPress, Hit, Walk, null);
        }

        void Update()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                Destroy(gameObject);
                return;
            }

            if (isSpawning)
            {
                // Movement starts at startBeat + 1 and lasts for 2 beats.
                if (spawnCurve != null)
                {
                    float spawnPos = game.conductor.GetPositionFromBeat(startBeat + 1, 1.1f);
                    transform.position = spawnCurve.GetPoint(spawnPos);
                    if (spawnPos > 1f)
                    {
                        isSpawning = false;
                        isWalking = true;
                        walkBeat = game.conductor.songPositionInBeatsAsDouble;
                    }
                }
                else
                { 
                    Destroy(gameObject);
                }
            }
            else if (isMissing)
            {
                if (missCurve != null)
                {
                    // The demon flees over 1 beat.
                    float missPos = game.conductor.GetPositionFromBeat(missBeat, 1f);
                    transform.position = missCurve.GetPoint(missPos);
                    if (missPos > 1f)
                    {
                        Destroy(gameObject);
                    }
                }
                else
                {
                    Destroy(gameObject);
                }
            }
            else if (isWalking)
            {
                if (walkCurve != null)
                { // The demon walks away over 1 beat.
                    float walkPos = game.conductor.GetPositionFromBeat(walkBeat, 1f);
                    transform.position = walkCurve.GetPoint(walkPos);
                    if (walkPos > 1f)
                    {
                        Destroy(gameObject);
                    }
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }

        public void Hit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f) // Barely
            {
                // Play miss animation and sound
                Sdemon.flipX = true;
                Mdemon.flipX = true;
                Ldemon.flipX = true;
                XLdemon.flipX = true;
                isSpawning = false;
                isMissing = true;
                missBeat = game.conductor.songPositionInBeatsAsDouble;
                SoundByte.PlayOneShotGame("samuraiSliceRvl/OSII");
            }
            else // Perfect
            {
                // Play success animation and sound
                game.DemonSuccess(caller, state);
                game.SpawnSlicedDemon(transform.position, transform.rotation, type);
                Destroy(gameObject);
            }
        }

        public void Walk(PlayerActionEvent caller)
        {
            DemonsType(type, "Waddle");
            game.DemonMiss(caller);
        }

        public void DemonsType(DemonType newtype, string anim)
        {
            switch (newtype)
            {
                case DemonType.Small:
                    SDemonAnim.DoScaledAnimationAsync($"Small{anim}");
                    Sdemon.enabled = true;
                    Mdemon.enabled = false;
                    Ldemon.enabled = false;
                    XLdemon.enabled = false;
                    break;
                case DemonType.Medium:
                    MDemonAnim.DoScaledAnimationAsync($"Medium{anim}");
                    Sdemon.enabled = false;
                    Mdemon.enabled = true;
                    Ldemon.enabled = false;
                    XLdemon.enabled = false;
                    break;
                case DemonType.Big:
                    LDemonAnim.DoScaledAnimationAsync($"Large{anim}");
                    Sdemon.enabled = false;
                    Mdemon.enabled = false;
                    Ldemon.enabled = true;
                    XLdemon.enabled = false;
                    break;
                case DemonType.Huge:
                    XLDemonAnim.DoScaledAnimationAsync($"XL{anim}");
                    Sdemon.enabled = false;
                    Mdemon.enabled = false;
                    Ldemon.enabled = false;
                    XLdemon.enabled = true;
                    break;
            }
        }
    }
}