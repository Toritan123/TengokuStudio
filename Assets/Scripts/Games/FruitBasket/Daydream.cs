using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace HeavenStudio.Games.Scripts_FruitBasket
{
    public class Daydream : MonoBehaviour
    {
        [SerializeField] private Animator bubbleAnimator;
        [SerializeField] private GameObject daydreamContainer;
        [SerializeField] private Animator daydreamAnimator;

        [SerializeField] private SpriteRenderer bubbleLRenderer;

        public double fadeOutTime;
        public FruitBasket.DaydreamType daydreamType;

        private bool fadingOut = false;
        
        void Start()
        {
            bubbleAnimator.Play("thoughtBubble");
            bubbleAnimator.Play("thoughtBubbleFadeIn");
            var dream = daydreamType;
            if (daydreamType == FruitBasket.DaydreamType.Random)
            {
                var values = (FruitBasket.DaydreamType[])Enum.GetValues(typeof(FruitBasket.DaydreamType));
                dream = values[UnityEngine.Random.Range(1, values.Length)];
            }
            daydreamAnimator.Play(dream.ToString());
        }

        void Update()
        {
            float bubbleTransparency = bubbleLRenderer.color.a;

            if (Conductor.instance.songPositionInBeatsAsDouble >= fadeOutTime)
            {
                bubbleAnimator.Play("thoughtBubbleFadeOut");
                if (bubbleTransparency == 0)
                {
                    Destroy(gameObject);
                }
            }



            Transform child = daydreamContainer.transform.Find(daydreamType.ToString());
            if (child == null)
            {
                return;
            }

            foreach (SpriteRenderer renderer in child.gameObject.GetComponentsInChildren<SpriteRenderer>())
            {
                renderer.material.color = new Color(1, 1, 1, renderer.color.a * bubbleTransparency);
            }
        }
    }
}
