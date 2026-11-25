using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_FlockStep
{
    public class Huebird : MonoBehaviour
    {
        private FlockStep game;
        [SerializeField] public int birdColor;
        [SerializeField] public Animator birdAnim;
        [SerializeField] public Transform bird;
        
        

        public GameObject mad;
        public string currentAnim = "";
        public float initialBirdX;
        public float initialBirdY;
        public float initialBirdZ;
        public int flyState = 0; // 0 = normal, 1 = flying away, 2 = landing, 3 = hidden
        public double flyStartBeat;
        public double flyDuration;
        private double madEndBeat = -1.0;
        private bool initialized = false;
        public void Init()
        {
            if (initialized) return;
            initialized = true;
            if (mad != null) mad.SetActive(false);
            UpdateColor();
            if (bird != null) 
            {
                initialBirdX = bird.localPosition.x;
                initialBirdY = bird.localPosition.y;
                initialBirdZ = bird.localPosition.z;
            }
        }
        public void Awake()
        {
            Init();
        }

        public void TriggerMad(double currentBeat)
        {
            if (mad != null)
            {
                mad.SetActive(true);
                madEndBeat = currentBeat + 5.0;
            }
        }

        public string pendingAnim;
        public double animStartTime = -1.0;
        public bool pendingIsWhiff = false;

        public void Update()
        {
            if (pendingAnim != null && Conductor.instance.unswungSongPositionInBeatsAsDouble >= animStartTime)
            {
                PlayAnim(pendingAnim, pendingIsWhiff);
                pendingAnim = null;
            }

            if (madEndBeat > 0)
            {
                if (Conductor.instance.songPositionInBeatsAsDouble >= madEndBeat)
                {
                    madEndBeat = -1.0;
                    if (mad != null) mad.SetActive(false);
                }
                else if (mad != null)
                {
                    bool isWalk = currentAnim == "birds_i_L_walk_00" || 
                                  currentAnim == "birds_i_L_walk_01" || 
                                  currentAnim == "birds_i_R_walk_00" || 
                                  currentAnim == "birds_i_R_walk_01";
                    if (mad.activeSelf != isWalk) mad.SetActive(isWalk);
                }
            }
        }

        public void ScheduleAnim(string animName, double beat, bool isWhiff = false)
        {
            pendingAnim = animName;
            animStartTime = beat;
            pendingIsWhiff = isWhiff;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateColor();
        }
#endif

        private Color _lastFeather = Color.clear;
        private Color _lastBelly = Color.clear;
        private Color _lastBeak = Color.clear;
        private Renderer[] _cachedRenderers;
        private static MaterialPropertyBlock _sharedBlock;

        public void UpdateColor()
        {
            if (birdColor < 0 || birdColor > 5) return;
            int idx = (birdColor == 5) ? 17 : birdColor * 3;
            if (FlockStep.birdColors == null || FlockStep.birdColors.Count <= idx + 2) return;
            
            Color feather = FlockStep.birdColors[idx];
            Color belly = FlockStep.birdColors[idx + 1];
            Color beak = FlockStep.birdColors[idx + 2];
            
            if (_lastFeather == feather && _lastBelly == belly && _lastBeak == beak) return;
            _lastFeather = feather;
            _lastBelly = belly;
            _lastBeak = beak;

            if (_sharedBlock == null) _sharedBlock = new MaterialPropertyBlock();
            if (_cachedRenderers == null) 
            {
                var srs = GetComponentsInChildren<Renderer>(true);
                var validRenderers = new System.Collections.Generic.List<Renderer>();
                foreach (var sr in srs)
                {
                    string n = sr.gameObject.name.ToLower();
                    if (!(n.Contains("shadow") || n.Contains("mad"))) validRenderers.Add(sr);
                }
                _cachedRenderers = validRenderers.ToArray();
            }

            foreach (var sr in _cachedRenderers)
            {
                sr.GetPropertyBlock(_sharedBlock);
                _sharedBlock.SetColor("_ColorAlpha", feather);
                _sharedBlock.SetColor("_ColorBravo", belly);
                _sharedBlock.SetColor("_ColorDelta", beak);
                sr.SetPropertyBlock(_sharedBlock);
            }
        }

        public void BeginLoopL()
        {
            birdAnim.DoScaledAnimationAsync("birds_i_L_squat", 0.5f);
        }

        public void BeginLoopR()
        {
            birdAnim.DoScaledAnimationAsync("birds_i_R_squat", 0.5f);
        }

        public float pendingMove = 0f;

        public void PlayAnim(string animName, bool isWhiff = false)
        {
            if (!gameObject.activeInHierarchy || flyState != 0) return;
            currentAnim = animName;
            Vector3 birdPos = bird.localPosition;

            if (!isWhiff)
            {
                if (pendingMove != 0f)
                {
                    birdPos.x += pendingMove;
                    bird.localPosition = new Vector3(birdPos.x, birdPos.y, birdPos.z);
                    pendingMove = 0f;
                }

                switch (animName)
                {
                    case "birds_i_L_walk_01":
                    case "birds_i_R_walk_01":
                    case "birds_i_miss_back":
                    case "birds_i_L_squat":
                    case "birds_i_R_squat":
                        bird.localPosition = new Vector3(birdPos.x - 0.4f, birdPos.y, birdPos.z);
                        break;
                    case "birds_i_L_jump":
                    case "birds_i_R_jump":
                        pendingMove = -0.73f;
                        break;
                }
            }
            birdAnim.DoScaledAnimationAsync(animName, 0.5f);
        }

        public void HuebirdColor(int color)
        {
            birdColor = color;
            UpdateColor();
        }
    }
}