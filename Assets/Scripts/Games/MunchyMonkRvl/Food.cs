using System.Collections;
using UnityEngine;
using HeavenStudio.Games;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_MunchyMonkRvl
{
    public class Food : MonoBehaviour
    {
        public SpriteRenderer sprite;
        
        [Header("Physics Configuration")]
        public float waitTime = 0f;
        public Vector2 velocity = new Vector2(0f, 10f);
        [SerializeField] private float gravity = 100f;

        private double startBeat;
        private Vector3 startPos;
        private Vector3 startWorldPos;
        private bool isInitialized = false;

        public void Start()
        {
            startBeat = Conductor.instance.songPositionInBeatsAsDouble;
            startPos = transform.localPosition;
            startWorldPos = transform.localPosition;
        }

        private void Update()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                return;
            }

            float beatsPassed = Conductor.instance.GetPositionFromBeat(startBeat, 1f);
            float t = beatsPassed * (60f / 118f);

            if (t < waitTime) return;

            float fallT = t - waitTime;

            float XDisp = velocity.x * fallT;
            float YDisp = (velocity.y * fallT) - (0.5f * gravity * fallT * fallT);
            transform.position = startWorldPos + new Vector3(XDisp, YDisp, 0);

            if (transform.position.y < -10f)
            {
                Destroy(gameObject);
            }
        }
    }
}
