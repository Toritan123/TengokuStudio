using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using HeavenStudio.Util;
using HeavenStudio.Common;
using HeavenStudio.InputSystem;

using System.Linq;
using BurstLinq;

using SFB;
using Jukebox;
using TMPro;

namespace HeavenStudio
{
    public class CreditsManager : MonoBehaviour
    {
        private AudioSource musicSource;
        private double songPosBeat;
        private double songPos;
        private double time;
        private double targetBopBeat;
        private double lastAbsTime;
        private double startTime;

        [SerializeField] private TextAsset creditsText;
        [SerializeField] private TMP_Text creditsDisplay;

        [SerializeField] private RawImage bg;
        [SerializeField] private float bgYSpeed;

        [SerializeField] private RectTransform scrollingCredits;
        [SerializeField] private RectTransform thanksMessage;

        [SerializeField] private float credYSpeed;

        [SerializeField] private float bpm = 130f;
        [SerializeField] private float offset = -0.23f;

        [SerializeField] private Animator logoAnim;
        [SerializeField] private GameObject logoGO;

        private bool startScrolling = false;
        private bool stopScrolling = false;
        private bool exiting = false;
        private bool logoRevealed = false;
        private bool altBop = false;

        private void Start()
        {
            musicSource = GetComponent<AudioSource>();
            musicSource.PlayScheduled(AudioSettings.dspTime);
            creditsDisplay.text = creditsText.text;
            startTime = Time.realtimeSinceStartupAsDouble;
            logoGO.SetActive(true);
            logoAnim.Play("Reveal", 0, 0);
            logoRevealed = true;
            Debug.Log("This credits scene is entirely a placeholder! Pretty it up sometime, it's very bland...");
        }

        private void Update()
        {
            bg.uvRect = new Rect(bg.uvRect.position + (new Vector2(0, bgYSpeed) * Time.deltaTime), bg.uvRect.size);

            double absTime = Time.realtimeSinceStartupAsDouble - startTime;
            double dt = absTime - lastAbsTime;
            lastAbsTime = absTime;

            time += dt;
            songPos = time + offset;
            songPosBeat = SecsToBeats(songPos);

            float normalizedBeat2 = GetPositionFromBeat(8, 1);

            if (normalizedBeat2 > 0 && normalizedBeat2 <= 1f && !startScrolling)
            {
                startScrolling = true;
            }

            if (songPosBeat - 1 >= targetBopBeat)
            {
                if (targetBopBeat > 2)
                {
                    logoAnim.Play(altBop ? "LogoBop2" : "LogoBop", 0, 0);
                    altBop = !altBop;
                }
                targetBopBeat += 1;
            }

            if (!stopScrolling && startScrolling)
            {
                scrollingCredits.anchoredPosition += new Vector2(0, credYSpeed * Time.deltaTime);

                Vector2 messagePos = RectTransformUtility.WorldToScreenPoint(Camera.main, thanksMessage.position);

                if (Mathf.Abs(messagePos.y - Screen.height / 2) < 10f)
                {
                    stopScrolling = true;
                }

            }
            var controllers = PlayerInput.GetInputControllers();
            foreach (var newController in controllers)
            {
                if (newController.GetLastButtonDown(true) > 0 && !exiting)
                {
                    SoundByte.PlayOneShot("ui/PauseQuit");
                    GlobalGameManager.LoadScene("Title");
                    musicSource.mute = true;
                    exiting = true;
                }
            }
        }

        public double SecsToBeats(double s)
        {
            return s / 60f * bpm;
        }

        public double BeatsToSecs(double beats, float bpm)
        {
            return beats / bpm * 60f;
        }

        public float GetPositionFromBeat(float startBeat, float length)
        {
            float a = MathUtils.Normalize((float)songPosBeat, startBeat, startBeat + length);
            return a;
        }
    }
}