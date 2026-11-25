using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

namespace HeavenStudio.Games.Scripts_ExhibitionMatch
{
    public class VSController : MonoBehaviour
    {
        [Header("Components")]
        public ExhibitionMatch game;
        [SerializeField] Animator VSAnim;
        [SerializeField] GameObject PitcherMegamix;
        [SerializeField] GameObject PitcherFever;
        [SerializeField] GameObject SluggerMegamix;
        [SerializeField] GameObject SluggerFever;
        void Start()
        {
            game = ExhibitionMatch.instance;
        }
        public void ShowVS(double beat, float length, bool mute, int type)
        {
            if(type == 0) // fever
            {
                PitcherFever.SetActive(true);
                SluggerFever.SetActive(true);
                PitcherMegamix.SetActive(false);
                SluggerMegamix.SetActive(false);
            }
            else // megamix
            {
                PitcherFever.SetActive(false);
                SluggerFever.SetActive(false);
                PitcherMegamix.SetActive(true);
                SluggerMegamix.SetActive(true);
            }
            BeatAction.New(ExhibitionMatch.instance, new List<BeatAction.Action>() {
                new BeatAction.Action(beat, () => {
                    VSAnim.DoScaledAnimationAsync("Intro", 0.75f);
                }),
                new BeatAction.Action(beat + length, () => {
                    VSAnim.DoScaledAnimationAsync("Outro", 0.75f);
                }),
            });
            if(!mute)
            {
                SoundByte.PlayOneShotGame("exhibitionMatch/VS");
            }
        }
    }
}
