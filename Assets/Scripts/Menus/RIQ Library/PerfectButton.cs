using System.Collections;
using System.Collections.Generic;
using HeavenStudio.Common;
using UnityEngine;
using UnityEngine.UI;

namespace HeavenStudio
{
    public class PerfectButton : MonoBehaviour
    {
        [SerializeField] private RectTransform rect;
        [SerializeField] private Image icon;
        [SerializeField] private GameObject text;
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private Sprite inactiveSprite;
        [SerializeField] private float animationLength;

        // Start is called before the first frame update
        void Start()
        {
            icon.sprite = inactiveSprite;
            text.SetActive(false);
            rect.sizeDelta = new(40f, 40f);
            GlobalGameManager.perfectCampaign = false;
        }

        public void TogglePerfect()
        {
            if (GlobalGameManager.perfectCampaign)
            {
                DisableChallenge();
            }
            else
            {
                EnableChallenge();
            }
        }

        private void EnableChallenge()
        {
            StopAllCoroutines();
            StartCoroutine(AnimateEnable(rect.sizeDelta.x));

            GlobalGameManager.perfectCampaign = true;
        }

        private void DisableChallenge()
        {
            StopAllCoroutines();
            StartCoroutine(AnimateDisable(rect.sizeDelta.x));

            GlobalGameManager.perfectCampaign = false;
        }

        private IEnumerator AnimateEnable(float startWidth)
        {
            float time = 0;
            float normalizedTime = 0;

            Util.EasingFunction.Function easeFunction = Util.EasingFunction.GetEasingFunction(Util.EasingFunction.Ease.EaseOutQuint);

            float endWidth = 245f;

            icon.sprite = activeSprite;
            text.SetActive(true);

            while (normalizedTime < 1f)
            {
                normalizedTime = Mathf.InverseLerp(0f, animationLength, time);

                float newWidth = easeFunction(startWidth, endWidth, normalizedTime);

                rect.sizeDelta = new(newWidth, 40f);

                time += Time.deltaTime;
                yield return null;
            }

            rect.sizeDelta = new(endWidth, 40f);

            yield return null;
        }

        private IEnumerator AnimateDisable(float startWidth)
        {
            float time = 0;
            float normalizedTime = 0;

            Util.EasingFunction.Function easeFunction = Util.EasingFunction.GetEasingFunction(Util.EasingFunction.Ease.EaseOutQuint);

            float endWidth = 40f;

            icon.sprite = inactiveSprite;

            while (normalizedTime < 1f)
            {
                normalizedTime = Mathf.InverseLerp(0f, animationLength, time);

                float newWidth = easeFunction(startWidth, endWidth, normalizedTime);

                rect.sizeDelta = new(newWidth, 40f);

                time += Time.deltaTime;
                yield return null;
            }

            rect.sizeDelta = new(endWidth, 40f);
            text.SetActive(true);

            yield return null;
        }
    }
}