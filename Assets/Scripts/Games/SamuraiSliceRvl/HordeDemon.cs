using System.Collections;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_SamuraiSliceRvl
{
    public class HordeDemon : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Animator anim;
        [SerializeField] private SpriteRenderer sr;

        [Header("Settings")]
        [SerializeField] private float minRushDuration = 0.5f;
        [SerializeField] private float maxRushDuration = 0.9f;
        [SerializeField] private Vector3 targetPosition = new Vector3(-2f, 0f, 0f); // Player position (approx)

        [Header("Physics (Fail Fall)")]
        [SerializeField] private float gravity = 80f;
        [SerializeField] private Vector2 fallVelocity = new Vector2(0f, 10f);
        [SerializeField] private float rotationSpeed = -450f;

        private bool isRushing = false;
        private bool isFalling = false; // For fail state (OSII)
        private bool hasArrived = false;
        private bool bitten = false;
        private Vector3 startPosition;
        private Vector3 spawnPosition;
        private float sliceX;
        private float rushTimer = 0f;
        private float currentRushDuration;
        private float fallTimer = 0f;
        private SamuraiSliceRvl game;
        private HordeSequence sequence;
        private Quaternion startRot;

        public void Init(SamuraiSliceRvl gameInstance, HordeSequence sequenceInstance, int sortOrder, bool isFinal, Vector3? customTarget = null)
        {
            game = gameInstance;
            sequence = sequenceInstance;
            if (sr != null) sr.sortingOrder = sortOrder; // Ensure demons behind render behind
            
            if (customTarget.HasValue)
                targetPosition = customTarget.Value;
            
            // Random variation so some go past the target
            targetPosition.x += Random.Range(-1.5f, 0f);

            sliceX = Random.Range(-0.25f, 0.25f);

            spawnPosition = transform.localPosition;
            
            float beatDuration = Conductor.instance.secPerBeat;
            currentRushDuration = beatDuration / 7f;

            // Spawn animation
            if (anim != null) anim.DoScaledAnimationAsync(isFinal ? "HordeSummonFinal" : "HordeSummon", 1f);
        }

        public void Rush()
        {
            if (isRushing) return;
            isRushing = true;
            startPosition = spawnPosition;
            rushTimer = 0f;
            hasArrived = false;
            
            if (anim != null) anim.DoScaledAnimationAsync("HordeRush", 1f);
        }

        public void Kill(DemonSliced slicedPrefab, bool isFinal)
        {
            if (isFinal)
            {
                transform.localPosition = targetPosition;
                if (sr != null) sr.sortingOrder = 5;
                sliceX = -1f;
            }

            transform.localPosition = new Vector3(sliceX - 1f, targetPosition.y, 1f);

            if (slicedPrefab != null)
            {
                // Instantiate slice effect
                DemonSliced sliced = Instantiate(slicedPrefab, transform.localPosition, transform.rotation);
                
                if (isFinal) sliced.transform.localScale = new Vector3(1.35f, 1.35f, 1f);
                sliced.InitHorde(!isFinal);
            }
            
            Destroy(gameObject, 1f);
        }

        public void FallFail()
        {
            // Behavior for the last demon if missed (OSII)
            isRushing = false;
            isFalling = true;
            hasArrived = false;
            
            transform.localPosition = targetPosition;
            
            fallTimer = 0f;
            startRot = transform.rotation;
            startPosition = transform.localPosition; // Use current position (target) as fall start

            if (anim != null) anim.DoScaledAnimationAsync("HordeIdle", 1f);
            
            // Ensure it renders on top
            if (sr != null) sr.sortingOrder = 5;

            // Destroy after falling
            Destroy(gameObject, 2.0f);
        }

        void Update()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused || GameManager.instance.currentGame != "samuraiSliceRvl")
            {
                Destroy(gameObject);
                return;
            }
        }

        private void LateUpdate()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused) return;

            if (isRushing)
            {
                rushTimer += Time.deltaTime;
                float t = rushTimer / currentRushDuration;
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                
                transform.localPosition = new Vector3(sliceX, targetPosition.y, 1f);

                if (t >= 1f)
                {
                    isRushing = false;
                    hasArrived = true;
                }
            }
            else if (isFalling)
            {
                fallTimer += Time.deltaTime;

                // Fall physics identical to DemonSliced (top part only)
                float yDisp = (fallVelocity.y * fallTimer) - (0.5f * gravity * fallTimer * fallTimer);
                transform.localPosition = startPosition + new Vector3(0, yDisp, 0);
                transform.rotation = startRot * Quaternion.Euler(0, 0, rotationSpeed * fallTimer);
            }
            else
            {
                // Force position to prevent Animator from resetting to 0,0
                if (rushTimer == 0f)
                    transform.localPosition = spawnPosition;
                else if (bitten)
                    transform.localPosition = targetPosition;
                else
                    transform.localPosition = new Vector3(sliceX, targetPosition.y, 1f);

                // Only bite if arrived AND game indicates combo miss
                if (hasArrived && sequence != null && sequence.IsComboMissed)
                {
                    Bite();
                }
            }
        }

        private void Bite()
        {
            isRushing = false;
            hasArrived = false;
            bitten = true;
            if (sr != null) sr.sortingOrder = 2;
            if (anim != null) anim.Play("HordeBite");
            
            // Call game Miss
            if (game != null)
            {
                game.ComboDemonMiss(null, sequence);
            }

            // Destroy shortly after biting
            float beatDuration = Conductor.instance.secPerBeat;
            Destroy(gameObject, beatDuration); 
        }
    }
}
