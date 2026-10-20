using System.Collections;
using UnityEngine;
using HeavenStudio.Games;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_SamuraiSliceRvl
{
    public class DemonSliced : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Transform topPart;
        [SerializeField] private Transform botPart;

        [Header("Physics Configuration")]
        [SerializeField] private float waitTime = 0.08f;
        [SerializeField] private float rotationSpeed = -450f; // Much faster rotation
        [SerializeField] private Vector2 topVelocity = new Vector2(0f, 10f); // Higher upward impulse to separate
        [SerializeField] private Vector2 botVelocity = new Vector2(0f, -5f);  // Immediate fall
        [SerializeField] private float gravity = 80f; // Heavy gravity to avoid floaty feel

        private double startBeat;
        private Vector3 startTopPos;
        private Vector3 startBotPos;
        private Quaternion startTopRot;
        private Quaternion startBotRot;
        private bool isInitialized = false;

        public void Init(DemonType type)
        {
            startBeat = Conductor.instance.songPositionInBeatsAsDouble;
            startTopPos = topPart.position;
            startBotPos = botPart.position;
            startTopRot = topPart.rotation;
            startBotRot = botPart.rotation;
            isInitialized = true;
        }

        public void InitHorde(bool topOnly)
        {
            Init(DemonType.Small); // Base initialization
            
            if (topOnly)
            {
                // Exaggerated physics for horde: Sharp jump up and right
                topVelocity = new Vector2(2.5f, 15f);
                botVelocity = new Vector2(2.75f, 14f); 
            }
        }

        private void Update()
        {
            if (!isInitialized) return;
            
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused || GameManager.instance.currentGame != "samuraiSliceRvl")
            {
                Destroy(gameObject);
                return;
            }

            // Calculate time passed in "reference seconds" (scaled to 130 BPM)
            // GetPositionFromBeat returns beats passed since startBeat
            float beatsPassed = Conductor.instance.GetPositionFromBeat(startBeat, 1f);
            float t = beatsPassed * (60f / 130f);

            if (t < waitTime) return;

            // Calculate fall time
            float fallT = t - waitTime;

            // Top Part
            float topXDisp = topVelocity.x * fallT;
            float topYDisp = (topVelocity.y * fallT) - (0.5f * gravity * fallT * fallT);
            topPart.position = startTopPos + new Vector3(topXDisp, topYDisp, 0);
            topPart.rotation = startTopRot * Quaternion.Euler(0, 0, rotationSpeed * fallT);

            // Bottom Part
            float botXDisp = botVelocity.x * fallT;
            float botYDisp = (botVelocity.y * fallT) - (0.5f * gravity * fallT * fallT);
            botPart.position = startBotPos + new Vector3(botXDisp, botYDisp, 0);
            
            // Delayed rotation for bottom part (0.1f delay in reference time)
            if (fallT > 0.1f)
            {
                float rotT = fallT - 0.1f;
                botPart.rotation = startBotRot * Quaternion.Euler(0, 0, rotationSpeed * 1.2f * rotT);
            }

            // Destroy if fallen too low
            if (topPart.position.y < -10f && botPart.position.y < -10f)
            {
                Destroy(gameObject);
            }
        }
    }
}
