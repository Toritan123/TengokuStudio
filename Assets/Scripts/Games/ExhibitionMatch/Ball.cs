using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_ExhibitionMatch
{
    public class Ball : MonoBehaviour
    {

        [Header("Components")]
        private ExhibitionMatch game;

        [Header("Properties")]
        private double startBeat;
        private double missBeat;
        private double barelyBeat;

        private bool isSpawning;
        private bool isMissing;
        private bool isBarely;
        [SerializeField] BigBall bigBall;
        
        [Header("Curves")]
        [SerializeField] BezierCurve3D TossCurve;
        [SerializeField] BezierCurve3D MissCurve;
        [SerializeField] BezierCurve3D BarelyCurve;
        
        public void Initialize(double beat)
        {
            startBeat = beat;
            game = ExhibitionMatch.instance;
            gameObject.SetActive(true);
        }

        void Start()
        {
            isSpawning = true;
        }

        void Update()
        {
            if (!Conductor.instance.isPlaying)
            {
                return;
            }

            if (isSpawning)
            {
                if (TossCurve != null)
                {
                    float spawnPos = game.conductor.GetPositionFromBeat(startBeat, 0.525f, ignoreSwing: true);
                    transform.position = TossCurve.GetPoint(Mathf.Clamp01(spawnPos));
                    if (spawnPos > 1f)
                    {
                        isSpawning = false;
                        isMissing = true;
                        missBeat = startBeat + 0.525f;
                    }
                }
                else
                { 
                    Destroy(gameObject);
                }
            }
            else if (isMissing) //i literally couldve just split the curves up more  logically but i sat here tweaking it and it works soooooo (Walks away (in RP))
            {
                if (MissCurve != null)
                {
                    double currentBeat = game.conductor.unswungSongPositionInBeatsAsDouble;
                    double elapsedBeats = currentBeat - missBeat;
                    float misspos = 0f;
                    
                    if (elapsedBeats < 0.37)
                    {
                        misspos = (float)(elapsedBeats / 0.37) * 0.333333f;
                    }
                    else
                    {
                        double beatsPastPoint1 = elapsedBeats - 0.37;
                        float bpm = game.conductor.GetBpmAtBeat(missBeat);
                        float secPerBeat = 60f / bpm;
                        float secsPastPoint1 = (float)(beatsPastPoint1 * secPerBeat);
                        
                        misspos = 0.333333f + (secsPastPoint1 / 0.9f) * 0.666666f;
                    }
                    
                    transform.position = MissCurve.GetPoint(misspos);
                    if (misspos > 1f)
                    {
                        Destroy(gameObject);
                    }
                }
                else
                { 
                    Destroy(gameObject);
                }
            }
            else if (isBarely)
            {
                if (BarelyCurve != null)
                {
                    float barelypos = game.conductor.GetPositionFromBeat(barelyBeat, 0.525f, ignoreSwing: true);
                    transform.position = BarelyCurve.GetPoint(barelypos);
                    if (barelypos > 1f)
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

        public void Success(float state)
        {
            BigBall newBigBall = Instantiate(bigBall, game.transform);
            newBigBall.Initialize(game.conductor.unswungSongPositionInBeatsAsDouble, state);
            Destroy(gameObject);
        }

        public void Miss()
        {
            BeatAction.New(game, new System.Collections.Generic.List<BeatAction.Action>() {
                new BeatAction.Action(missBeat + 0.37, () => {
                    SoundByte.PlayOneShotGame("exhibitionMatch/BALL_BOUND_PITCH");
                })
            });
        }
        public void Barely()
        {
            isBarely = true;
            isMissing = false;
            isSpawning = false;
            barelyBeat = game.conductor.unswungSongPositionInBeatsAsDouble;
        }
    }
}
