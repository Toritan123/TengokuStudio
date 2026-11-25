using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

namespace HeavenStudio.Games.Scripts_ExhibitionMatch
{
    public class PitcherController : MonoBehaviour
    {

        [Header("Components")]
        public ExhibitionMatch game;
        [SerializeField] Animator PitcherAnim;
        [SerializeField] Animator PitcherHeadAnim;
        [SerializeField] GameObject hat;
        [SerializeField] SpriteRenderer hatSprite;
        [SerializeField] SpriteRenderer headHat;
        private int state = 0;
        private bool crying = false;
        private bool isPitching = false;
        public bool hasCap = true; //The one downside of imported anims *Starts crying
        
        [SerializeField] public GameObject sweatObj;
        [SerializeField] public Animator sweatAnim;

        void Start()
        {
            game = ExhibitionMatch.instance;

            if (sweatObj != null)
            {
                sweatObj.SetActive(false);
            }
            
            SetPitcherState(state);
        }

        private bool wasHeadActive = true;

        void Update()
        {
            if (PitcherHeadAnim != null)
            {
                bool isHeadActive = PitcherHeadAnim.gameObject.activeInHierarchy;
                if (isHeadActive && !wasHeadActive)
                {
                    SetPitcherState(state);
                }
                wasHeadActive = isHeadActive;
            }
        }

        public void PitchBall(double beat)
        {
            isPitching = true;
            BeatAction.New(ExhibitionMatch.instance, new List<BeatAction.Action>() {
                new BeatAction.Action(beat, () => {
                    PitcherAnim.DoScaledAnimationAsync("batting_01_pitcher_ready", 0.5f);
                    if (sweatObj != null) sweatObj.SetActive(false);
                }),
                new BeatAction.Action(beat + 1.75f, () => {
                    PitcherAnim.DoScaledAnimationAsync("batting_01_pitcher_premotion", 0.5f);
                }),
                new BeatAction.Action(beat + 2, () => {
                    if (hasCap)
                    {
                        PitcherAnim.DoScaledAnimationAsync("batting_01_pitcher_pitch01", 0.58f);
                    }
                    else
                    {
                        PitcherAnim.DoScaledAnimationAsync("batting_01_pitcher_pitch02", 0.58f);
                    }
                }),
                new BeatAction.Action(beat + 5, () => {
                    isPitching = false;
                    PitcherAnim.DoScaledAnimationAsync("batting_01_pitcher_wait", 0.5f);
                    SetPitcherState(state);
                }),
                });
        }
        public void SetPitcherState(int newState)
        {
            state = newState;
            if(state < -1) state = -1;
            if(state > 2) state = 2;

            string capSuffix = !hasCap ? "_no_cap" : "";

            if(crying)
            {
                PitcherHeadAnim.DoScaledAnimationAsync("batting_01_pitcher_face_tired" + capSuffix, 0.5f);
                return;
            }

            if (state > 0)
            {
                PitcherHeadAnim.DoScaledAnimationAsync("batting_01_pitcher_face_angry" + capSuffix, 0.5f);
            }
            else if (state < 0)
            {
                PitcherHeadAnim.DoScaledAnimationAsync("batting_01_pitcher_face_smile" + capSuffix, 0.5f);
            }
            else
            {
                PitcherHeadAnim.DoScaledAnimationAsync("batting_01_pitcher_face_normal" + capSuffix, 0.5f);
            }
        }
        public int GetPitcherState()
        {
            return state;
        }
        public void ShockAnimation()
        {
            string capSuffix = !hasCap ? "_no_cap" : "";
            PitcherHeadAnim.DoScaledAnimationAsync("batting_01_pitcher_face_shock" + capSuffix, 0.5f);
        }
        public void BopAnimation()
        {
            if (isPitching) return;
            if((PitcherAnim.IsAnimationNotPlaying() || PitcherAnim.GetCurrentAnimatorStateInfo(0).IsName("batting_01_pitcher_wait")))
            {
                PitcherAnim.DoScaledAnimationAsync("batting_01_pitcher_beat", 0.5f);
                SetPitcherState(state);
                if (crying && sweatObj != null && sweatAnim != null)
                {
                    sweatObj.SetActive(true);
                    sweatAnim.DoScaledAnimationAsync("batting_01_pitcher_sweat", 0.5f);
                }
                else if (sweatObj != null)
                {
                    sweatObj.SetActive(false);
                }
            }
        }
        public void ThrowHat(double beat, bool force, bool instant)
        {
            if(instant)
            {
                hat.SetActive(true);
                hatSprite.enabled = true;
                hasCap = false;
                SetPitcherState(state);
                return;
            }
            if(state == 2 || force)
            {
                BeatAction.New(ExhibitionMatch.instance, new List<BeatAction.Action>() {
                new BeatAction.Action(beat, () => {
                    PitcherAnim.DoScaledAnimationAsync("batting_01_pitcher_cap_off_ready", 0.5f);
                }),
                new BeatAction.Action(beat + 1f, () => {
                    PitcherAnim.DoScaledAnimationAsync("batting_01_pitcher_cap_off", 0.5f);
                    hasCap = false;
                    SetPitcherState(state);
                }),
                new BeatAction.Action(beat + 2.5f, () => {
                    hat.SetActive(true);
                    hatSprite.enabled = true;
                    SetPitcherState(state);
                }),
                });
            }
        }
        public void StartCry(bool forceCry) // or sweat
        {
            if (forceCry || (!hasCap && !game.miss && game.SluggerState != -1))
            {
                crying = true;
                SetPitcherState(state);
                if (sweatObj != null && sweatAnim != null && !isPitching)
                {
                    sweatObj.SetActive(true);
                    sweatAnim.DoScaledAnimationAsync("batting_01_pitcher_sweat", 0.5f);
                }
            }
        }
    }
}
