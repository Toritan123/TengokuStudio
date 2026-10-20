using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

namespace HeavenStudio.Games.Scripts_RhythmSheriff
{
    public class Targets : MonoBehaviour
    {

        private RhythmSheriff game;
        

        public enum TargetType
            {
                Cat = 0,
                Rat = 1,
            }

        public TargetType targetType;

        public double beat;
        public Animator target;
        private Animator dogSheriff;
        public ParticleSystem hole;

        private bool canWhiff = true;

        // Start is called before the first frame update
        void Awake()
        {
            game = RhythmSheriff.instance;

            dogSheriff = game.dogSheriff;
            
        }    
        // Start is called before the first frame update
        void Start()
        {
            float length = 2f;

            switch ((int)targetType)
            {
                case 0:
                    length = 2f;
                    break;

                case 1:
                    length = 1.5f;
                    break;
            }

            game.ScheduleInput(beat, length, RhythmSheriff.InputAction_BasicPress, ShootJust, ShootBad, Empty);

            target.DoScaledAnimationAsync($"{targetType.ToString()}Target", 0.5f);
            target.DoScaledAnimationAsync($"{targetType.ToString()}", 0.5f);

            if (UnityEngine.Random.Range(1, 3) == 1)
            { target.DoScaledAnimationAsync($"{targetType.ToString()}TargetLeft", 0.5f); }
            else { target.DoScaledAnimationAsync($"{targetType.ToString()}TargetRight", 0.5f); }

        }

        void KillYourselfNow()
        {
            GameObject.Destroy(gameObject);
        }

        // Update is called once per frame
        void Update()
        {
            if (PlayerInput.GetIsAction(RhythmSheriff.InputAction_BasicPress) && !game.IsExpectingInputNow(RhythmSheriff.InputAction_BasicPress) && canWhiff)
            {
                target.DoScaledAnimationAsync("WhiffSpin", 0.5f);
            }
        }

        //public override void OnBeatPulse(double beat)
        //{
        //    target.DoScaledAnimationAsync("Bop", 0.5f);
        //}

        private void ShootJust(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {

            }

            MultiSound.Play(new MultiSound.Sound[] {
            new MultiSound.Sound("rhythmSheriff/destroyTarget1", beat),
            new MultiSound.Sound("rhythmSheriff/destroyTarget3", beat),
            });

            ParticleSystem bulletHole = Instantiate(hole, gameObject.transform);
            bulletHole.Play();

            dogSheriff.DoScaledAnimationAsync("ShootJust", 0.5f);

            canWhiff = false;

            game.bopType = RhythmSheriff.SheriffBopType.Return;
        }

        private void ShootBad(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("rhythmSheriff/sink", beat: beat + 3);

            canWhiff = false;

            BeatAction.New(game, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 2.25, delegate
                    {
                        game.bopType = RhythmSheriff.SheriffBopType.Normal;
                    }),
            });

        }

        private void Empty(PlayerActionEvent caller)
        {

        }
    }
}
