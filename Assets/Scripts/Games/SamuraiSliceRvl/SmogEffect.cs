using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_SamuraiSliceRvl
{
    public class SmogEffect : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GameObject particlePrefab;
        [Tooltip("Factor to convert pixel coordinates to Unity units")]
        [SerializeField] private float pixelScale = 0.01f; 
        [SerializeField] private Vector2 baseOffset = new Vector2(62.5f, 7.5f);
        [SerializeField] private Vector2 defaultStartPos = new Vector2(-30.0f, -22.5f);
        [SerializeField] private float particleZPos = -3f;
        [SerializeField] private Vector3 manualOffset; // Extra manual adjustment from inspector

        private double animStartBeat = -1;
        private float animLength = 0;
        private float startScaleVal = 1f;
        private float endScaleVal = 1f;
        private Util.EasingFunction.Ease currentEase = Util.EasingFunction.Ease.Linear;
        private bool isAnimating = false;

        private struct SmogStep
        {
            public int frame;
            public Vector2 position;
        }

        private List<SmogStep> steps;

        private void Awake()
        {
            steps = new List<SmogStep>();

            // Data configuration (float.MinValue indicates '?' value replaced by defaultStartPos)
            AddStep(0, 0, 45.0f, 37.5f);
            AddStep(1, 5, 52.5f, 24.0f);
            AddStep(2, 10, 45.0f, float.MinValue); // Y is ?
            AddStep(3, 15, 6.0f, 45.0f);
            AddStep(4, 20, 30.0f, float.MinValue); // Y is ?
            AddStep(5, 25, 7.5f, 7.5f);
            AddStep(6, 30, 22.5f, 15.0f);
            AddStep(7, 35, 30.0f, 48.0f);
            AddStep(8, 40, float.MinValue, 24.0f); // X is ?
            AddStep(9, 10, 7.5f, 31.5f);
            AddStep(10, 20, 15.0f, 27.0f);
            AddStep(11, 5, float.MinValue, 15.0f); // X is ?
            AddStep(12, 10, 60.0f, 30.0f);
            AddStep(13, 15, 45.0f, 40.5f);
            AddStep(14, 30, 37.5f, 18.0f);
            AddStep(15, 5, 52.5f, 9.0f);
        }

        private void AddStep(int index, int frame, float x, float y)
        {
            float finalX = (x == float.MinValue) ? defaultStartPos.x : x;
            float finalY = (y == float.MinValue) ? defaultStartPos.y : y;

            steps.Add(new SmogStep
            {
                frame = frame,
                position = new Vector2(finalX, finalY)
            });
        }

        public void Play()
        {
            // Apply base offset to main container
            transform.localPosition += ((Vector3)baseOffset * pixelScale) + manualOffset;
            
            // Instantiate all particles at once to form static portal
            foreach (var step in steps)
            {
                SpawnParticle(step);
            }
        }

        public void Animate(double beat, float length, bool show, int ease)
        {
            animStartBeat = beat;
            animLength = length;
            startScaleVal = show ? 0f : 1f;
            endScaleVal = show ? 1f : 0f;
            currentEase = (Util.EasingFunction.Ease)ease;
            isAnimating = true;
            
            // If length is 0 (instant), apply immediately
            if (length <= 0)
            {
                transform.localScale = Vector3.one * endScaleVal;
                isAnimating = false;
            }
        }

        private void Update()
        {
            // Destroy if game is not active
            if (GameManager.instance.currentGame != "samuraiSliceRvl")
            {
                Destroy(gameObject);
                return;
            }

            if (isAnimating)
            {
                float normalizedBeat = Mathf.Clamp01(Conductor.instance.GetPositionFromBeat(animStartBeat, animLength));
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(currentEase);
                float newScale = func(startScaleVal, endScaleVal, normalizedBeat);
                transform.localScale = Vector3.one * newScale;

                if (normalizedBeat >= 1f)
                {
                    isAnimating = false;
                }
            }
        }

        private void SpawnParticle(SmogStep step)
        {
            if (particlePrefab == null) return;

            Vector3 pos = new Vector3(step.position.x * pixelScale, step.position.y * pixelScale, particleZPos);

            GameObject p = Instantiate(particlePrefab, transform);
            p.transform.localPosition = pos;
            p.transform.localScale = Vector3.one;

            SmogParticle sp = p.GetComponent<SmogParticle>();
            if (sp != null)
            {
                sp.Init();
            }
        }
    }
}
