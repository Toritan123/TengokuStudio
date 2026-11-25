using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

namespace HeavenStudio.Games.Scripts_ExhibitionMatch
{
    public class BigBall : MonoBehaviour
    {

        [Header("Components")]
        public ExhibitionMatch game;
        private bool hit;
        private float state;

        [Header("Properties")]
        private double startBeat;
        private Vector3 startPosition;

        void Start()
        {
            if (!Conductor.instance.isPlaying)
            {
                return;
            }
            game = ExhibitionMatch.instance;
            hit = true;
        }
        public void Initialize(double beat, float hitState)
        {
            startBeat = beat;
            startPosition = transform.position;
            game = ExhibitionMatch.instance;
            state = hitState / 2;
            gameObject.SetActive(true);
        }

        void Update()
        {
            if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused)
            {
                Destroy(gameObject);
                return;
            }
            if (hit)
            {
                Vector3 p = GameCamera.Position + GameCamera.AdditionalPosition;
                Vector3 targetEndPos = new(state, 0f, p.z);
                float spawnPos = game.conductor.GetPositionFromBeat(startBeat, 0.6666f, ignoreSwing: true);
                
                transform.position = Vector3.LerpUnclamped(startPosition, targetEndPos, spawnPos);
                transform.rotation = Quaternion.Euler(Vector3.forward * 100.0f * spawnPos);
                if (spawnPos > 1f)
                {
                    Destroy(gameObject);
                }
            }  
        }
    }
}