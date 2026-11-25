using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

namespace HeavenStudio.Games.Scripts_ExhibitionMatch
{
    public class MonkeyController : MonoBehaviour
    {

        [Header("Components")]
        public ExhibitionMatch game;
        [SerializeField] Animator CurtainAnim;
        [SerializeField] Animator MonkeyAnim;
        [SerializeField] Ball ball;
        public Ball newBall;
        void Start()
        {
            game = ExhibitionMatch.instance;
        }
        public void ShowMonkey(bool instant, float length, bool mute)
        {
            float mult;
            if(instant)
            {
                mult = 0.000000001f;
            }
            else
            {
                mult = length;
                SoundByte.PlayOneShotGame("exhibitionMatch/CURTAIN", volume: mute ? 0 : 1);
            }
            if(CurtainAnim.GetCurrentAnimatorStateInfo(0).IsName("New State") || CurtainAnim.GetCurrentAnimatorStateInfo(0).IsName("batting_01_curtain_front"))
            {
                CurtainAnim.DoScaledAnimationAsync("batting_01_curtain", 0.5f / mult);
            }
            else 
            {
                CurtainAnim.DoScaledAnimationAsync("batting_01_curtain_front", 0.5f / mult);
            }
        }
        public void ThrowBallMonkey(double beat)
        {
            BeatAction.New(ExhibitionMatch.instance, new List<BeatAction.Action>() {
                new BeatAction.Action(beat, () => {
                    MonkeyAnim.DoScaledAnimationAsync("batting_01_monkey_catch_white", 0.5f);
                }),
                new BeatAction.Action(beat + 3.3, () => {
                    if(CurtainAnim.GetCurrentAnimatorStateInfo(0).IsName("batting_01_curtain"))
                    {
                        SoundByte.PlayOneShotGame("exhibitionMatch/THROW_MONKEY");
                    }
                    MonkeyAnim.DoScaledAnimationAsync("batting_01_monkey_pitch", 0.5f);
                    newBall = Instantiate(ball, game.transform);
                    newBall.Initialize(beat + 3.3f);
                }),
            });
        }
        public void BopMonkey()
        {
            if((MonkeyAnim.IsAnimationNotPlaying() || MonkeyAnim.GetCurrentAnimatorStateInfo(0).IsName("batting_01_monkey_beat")))
            {
                MonkeyAnim.DoScaledAnimationAsync("batting_01_monkey_beat", 0.5f);
            }
        }
        public void MonkeySuccess()
        {
            MonkeyAnim.DoScaledAnimationAsync("batting_01_monkey_tired", 0.5f);
        }
        public void MonkeyFail()
        {
            MonkeyAnim.DoScaledAnimationAsync("batting_01_monkey_tired_02", 0.5f);
        }
    }
}
