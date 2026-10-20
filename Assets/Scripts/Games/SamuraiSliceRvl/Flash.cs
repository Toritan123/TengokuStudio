using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

namespace HeavenStudio.Games.Scripts_SamuraiSliceRvl
{
    public class Flash : MonoBehaviour
    {
        [Header("Components")]
        public SamuraiSliceRvl game;
        [SerializeField] Animator LightningAnim;
        [SerializeField] Animator FlashAnim;
        public double startBeat;
        public int strikeNumber;

        [Header("Sprites Renderers")]
        [SerializeField] public SpriteRenderer Lightning;
        [SerializeField] public SpriteRenderer FlashEffect;

        void Start()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                Destroy(gameObject);
                return;
            }
            game = SamuraiSliceRvl.instance;
            FlashEffect.gameObject.SetActive(true);
            Lightning.enabled = true;
            FlashAnim.DoScaledAnimationAsync("Flash");
            LightningAnim.DoScaledAnimationAsync($"LightningStrike{strikeNumber}");
            BeatAction.New(this, new List<BeatAction.Action>() {
                new BeatAction.Action(startBeat + 1, () => {
                    if(FlashAnim.IsAnimationNotPlaying() && LightningAnim.IsAnimationNotPlaying())
                    { 
                        Destroy(gameObject);
                    }
                })
            });
        }

        void Update()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                Destroy(gameObject);
                return;
            }
        }
    }
}