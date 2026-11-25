using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

namespace HeavenStudio.Games.Scripts_RhythmRally
{
    public class Paddlers : MonoBehaviour
    {
        private RhythmRally game;
        private Animator playerAnim;
        private Animator opponentAnim;
        private Conductor cond;

        public bool PlayerDown { get { return playerDown; } }
        bool playerDown = false;

        public void Init()
        {
            game = RhythmRally.instance;
            playerAnim = game.playerAnim;
            opponentAnim = game.opponentAnim;
            cond = Conductor.instance;
        }

        void Update()
        {
            if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch && !GameManager.instance.Autoplay)
            {
                if (PlayerInput.GetIsAction(RhythmRally.InputAction_BasicPress))
                {
                    playerAnim.Play("Ready1");
                    playerDown = true;
                }
                if (PlayerInput.GetIsAction(RhythmRally.InputAction_BasicRelease))
                {
                    playerAnim.Play("UnReady1");
                    playerDown = false;
                }
            }
            else
            {
                if (!game.started) return;
            }
            if (PlayerInput.GetIsAction(RhythmRally.InputAction_FlickPress) && !game.IsExpectingInputNow(RhythmRally.InputAction_FlickPress))
            {
                SoundByte.PlayOneShotGame("rhythmRally/SE_PINPON_SWING");
                playerAnim.DoScaledAnimationAsync("Swing", 0.5f);
                playerDown = false;
            }
        }

        void Ace()
        {
            game.served = false;

            var hitBeat = cond.songPositionInBeatsAsDouble;

            var bounceBeat = game.serveBeat + game.targetBeat + 1f;

            if (game.rallySpeed == RhythmRally.RallySpeed.Slow)
            {
                bounceBeat = game.serveBeat + game.targetBeat + 2f;
            }
            else if (game.rallySpeed == RhythmRally.RallySpeed.SuperFast)
            {
                bounceBeat = game.serveBeat + game.targetBeat + 0.5f;
            }

            playerAnim.DoScaledAnimationAsync("Swing", 0.5f);
            playerDown = false;
            MultiSound.Play(new MultiSound.Sound[] 
            { 
                new MultiSound.Sound("rhythmRally/SE_PINPON_ATTACK_PLAYER_m100p100", hitBeat, SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-100, 101), false)), 
                new MultiSound.Sound("rhythmRally/SE_PINPON_TABLE_COM_m50p50", bounceBeat, SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-50, 51), false)), 
            });

            game.ball.SetActive(true);
        }

        void NearMiss(float state)
        {
            MissBall();
            SoundByte.PlayOneShot("miss");
            playerAnim.DoScaledAnimationAsync("Swing", 0.5f);
            playerDown = false;

            game.missCurve.KeyPoints[0].Position = game.ball.transform.position;
            game.missCurve.transform.localScale = new Vector3(-state, 1f, 1f);
            game.missBeat = cond.songPositionInBeatsAsDouble;
            game.ball.SetActive(true);
        }

        void MissBall()
        {
            game.served = false;
            game.missed = true;

            var whistleBeat = game.serveBeat + game.targetBeat + 1f;

            if (game.rallySpeed == RhythmRally.RallySpeed.Slow)
            {
                whistleBeat = game.serveBeat + game.targetBeat + 2f;
            }
            else if (game.rallySpeed == RhythmRally.RallySpeed.SuperFast)
            {
                whistleBeat = game.serveBeat + game.targetBeat + 0.5f;
            }

            MultiSound.Play(new MultiSound.Sound[] 
            { 
                new MultiSound.Sound("rhythmRally/SE_PINPON_START", whistleBeat) 
            });
        }

        public void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                NearMiss(state);
                return;
            }
            Ace();
        }

        public void Miss(PlayerActionEvent caller)
        {
            MissBall();
        }

        public void Out(PlayerActionEvent caller) { }
    }
}
