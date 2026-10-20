using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Common
{
    public class GoForAPerfect : MonoBehaviour
    {
        public static GoForAPerfect instance { get; set; }

        [SerializeField] Animator texAnim;
        [SerializeField] Animator pAnim;

        public bool active = false;
        private bool hiddenActive = false;

        public bool perfect;

        Conductor cond;
        double lastReportedBeat = 0f;
        double currentBeat = 0f;
        long currentBlink = 0;


        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            perfect = true;
            cond = Conductor.instance;
        }

        private void Update() {
            gameObject.SetActive(hiddenActive);
            if (!active) return;
            if (!OverlaysManager.OverlaysEnabled) return;
            if (cond == null || !cond.isPlaying) return;

            if (cond.ReportBeat(ref lastReportedBeat))
            {
                currentBeat = lastReportedBeat;
                if (currentBlink != 0)
                {
                    currentBlink++;
                    if (currentBlink % 2 == 0)
                    {
                        texAnim.Play("GoForAPerfect_Blink", -1, 0);
                    }
                    else
                    {
                        texAnim.Play("GoForAPerfect_Blink2", -1, 0);
                    }
                }
                else
                {
                    currentBlink++;
                }
            }
            else if (cond.songPositionInBeatsAsDouble < lastReportedBeat)
            {
                lastReportedBeat = System.Math.Round(cond.songPositionInBeatsAsDouble);
            }
        }

        public void Hit()
        {
            if (!active) return;
            if (!OverlaysManager.OverlaysEnabled) return;
            pAnim.Play("PerfectIcon_Hit", 0, 0);
        }

        public void Miss()
        {
            perfect = false;
            if (!active) return;
            SetInactive();
            if (!OverlaysManager.OverlaysEnabled)
            {
                hiddenActive = false;
                return;
            }

            texAnim.Play("GoForAPerfect_Miss");
            pAnim.Play("PerfectIcon_Miss", -1, 0);

            string sfx = "empty"; // an empty sound effect. causes issues with playing the last game sfx if nothing plays so this is needed
            switch (PersistentDataManager.gameSettings.perfectSfx)
            {
                case 0:
                    sfx = "perfectMiss";      // classic
                    break;
                case 1:
                    sfx = "miss";             // doink
                    break;
                case 2:
                    sfx = "nearMiss";         // tink
                    break;
                case 3:
                    sfx = "audienceSad";      // aww
                    break;
                default:                      // none
                    break;
            }
            SoundByte.PlayOneShot(sfx); 

            if (GameProfiler.instance != null)
                GameProfiler.instance.perfect = false;
        }

        public void Enable(double startBeat)
        {
            SetActive();
            gameObject.SetActive(true);
            pAnim.gameObject.SetActive(true);
            texAnim.gameObject.SetActive(true);
            texAnim.Play("GoForAPerfect_Idle");

            currentBlink = 0;
        }

        public void Disable()
        {
            SetInactive();
            gameObject.SetActive(false);
        }

        public void SetActive()
        {
            hiddenActive = true;
            active = true;
        }
        public void SetInactive()
        {
            active = false;
        }
    }

}