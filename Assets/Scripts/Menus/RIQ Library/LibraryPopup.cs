using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio
{
    public class LibraryPopup : MonoBehaviour
    {
        [SerializeField] private RectTransform main;
        [SerializeField] private RectTransform[] parts;
        [SerializeField] private float animationLength;
        private bool extended;

        private void Start()
        {
            if (extended)
            {
                transform.localScale = new(1, 1, 1);
                main.gameObject.SetActive(true);
            }
            else
            {
                transform.localScale = new(0, 0, 0);
                main.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (extended && Input.GetMouseButtonDown(0) && !UiUtils.MouseInAnyRectTransform(parts, LibraryManager.instance.cam))
            {
                HidePopup();
            }
        }

        

        public void TogglePopup()
        {
            if (!extended)
            {
                ShowPopup();
            }
            else
            {
                HidePopup();
            }
        }

        public void ShowPopup()
        {
            if (!extended)
            {
                StopAllCoroutines();
                StartCoroutine(AnimateOpen());
                extended = true;
            }
        }

        private IEnumerator AnimateOpen()
        {
            float time = 0;
            float normalizedTime = 0;

            Util.EasingFunction.Function easeFunction = Util.EasingFunction.GetEasingFunction(Util.EasingFunction.Ease.EaseOutQuint);
            float startScale = transform.localScale.x;

            main.gameObject.SetActive(true);

            while (normalizedTime < 1f)
            {
                normalizedTime = Mathf.InverseLerp(0f, animationLength, time);

                float newScale = easeFunction(startScale, 1f, normalizedTime);

                transform.localScale = new(newScale, newScale, newScale);

                time += Time.deltaTime;
                yield return null;
            }

            transform.localScale = new(1f, 1f, 1f);

            yield return null;
        }

        public void HidePopup()
        {
            if (extended)
            {
                StopAllCoroutines();
                StartCoroutine(AnimateClose());
                extended = false;
            }
        }

        private IEnumerator AnimateClose()
        {
            float time = 0;
            float normalizedTime = 0;

            Util.EasingFunction.Function easeFunction = Util.EasingFunction.GetEasingFunction(Util.EasingFunction.Ease.EaseOutQuint);
            float startScale = transform.localScale.x;

            while (normalizedTime < 1f)
            {
                normalizedTime = Mathf.InverseLerp(0f, animationLength, time);

                float newScale = easeFunction(startScale, 0f, normalizedTime);

                transform.localScale = new(newScale, newScale, newScale);

                time += Time.deltaTime;
                yield return null;
            }

            transform.localScale = new(0f, 0f, 0f);

            main.gameObject.SetActive(false);

            yield return null;
        }
    }
}
