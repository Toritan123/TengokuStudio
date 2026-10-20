using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_BalloonHunter
{
    public class Balloon : MonoBehaviour
    {
        public double startBeat;
        public float balloonSpeed;
        public bool isFive = false;
        public bool moose = false;

        private PlayerActionEvent popEvent;

        [Header("Components")]
        [SerializeField] Animator anim;
        [SerializeField] Animator hunterAnim;
        [SerializeField] Animator popEffect;
        [SerializeField] ParticleSystem popParticle;
        [SerializeField] GameObject mooseObject;

        bool mooseFalling = false;
        //float fallStartY;
        //double fallStartBeat;

        public BalloonHunter game;
        // Start is called before the first frame update
        private void Start()
        {
            popEvent = game.ScheduleInput(startBeat, balloonSpeed - (isFive? 2 : 1), Minigame.InputAction_BasicPress, Pop, Miss, null);

            if (isFive) mooseObject.SetActive(moose);
        }

        // Update is called once per frame
        private void Update()
        {
            if (mooseFalling)
            {
                //float normalizedBeat = Conductor.instance.GetPositionFromBeat(fallStartBeat, 0.5f);
                //if (normalizedBeat > 1) Destroy(gameObject);
                //Vector3 pos = transform.position;
                //pos.y = fallStartY + (6f * normalizedBeat);
                //transform.position = pos;
                //if (!anim.IsPlayingAnimationNames("MooseFall")) Destroy(gameObject);
            }
            else
            {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat, balloonSpeed);
                if (normalizedBeat > 1) Destroy(gameObject);
                anim.DoNormalizedAnimation("Move", normalizedBeat, 0);
            }

            if (isFive && PlayerInput.GetIsAction(BalloonHunter.InputAction_BasicPress) && !game.IsExpectingInputNow(BalloonHunter.InputAction_BasicPress) && popEvent != null)
            {
                popEvent.CanHit(false);
            }
        }

        public void Pop(PlayerActionEvent caller, float state)
        {
            if (isFive)
            {
                game.Toss(state is >= 1 or <= -1);
            }
            else
            {
                hunterAnim.DoScaledAnimationAsync("Shoot", 0.5f, animLayer: 0);
                hunterAnim.DoScaledAnimationAsync("Blow", 0.5f, animLayer: 1);
                SoundByte.PlayOneShotGame("balloonHunter/blow");
            }
            game.preparing = false;

            if (state is >= 1 or <= -1)
            {
                SoundByte.PlayOneShotGame("balloonHunter/miss");
                if (!isFive)
                {
                    anim.DoScaledAnimationAsync("Miss", 0.5f);
                    game.bopExpression = "Sad";
                }
                if (isFive) game.DoRockBarely(game.conductor.unswungSongPositionInBeatsAsDouble);
                if (moose)
                {
                    BeatAction.New(game, new()
                    {
                        new(startBeat + 3, delegate {
                            anim.DoScaledAnimationAsync("MooseRaspberry", 0.5f, animLayer: 1);
                            SoundByte.PlayOneShotGame("balloonHunter/moose_raspberry");
                        }),
                    });
                }
            }
            else
            {
                SoundByte.PlayOneShotGame("balloonHunter/pop", pitch: UnityEngine.Random.Range(0.9f, 1.1f));
                if (moose) SoundByte.PlayOneShotGame("balloonHunter/moose_uhoh");
                popEffect.DoScaledAnimationAsync("Pop", 0.5f);
                popParticle.Play();
                if (!isFive) game.bopExpression = "Happy";

                if (!moose) Destroy(gameObject);
                else
                {
                    //fallStartY = transform.position.y;
                    //fallStartBeat = game.conductor.unswungSongPositionInBeatsAsDouble;
                    anim.DoScaledAnimationAsync("MooseFall", 0.5f, animLayer: 1);
                    mooseFalling = true;
                    BeatAction.New(game, new()
                    {
                        new(startBeat + 4, delegate {
                            Destroy(gameObject);
                        }),
                    });
                }
            }
        }

        public void Miss(PlayerActionEvent caller)
        {
            game.bopExpression = isFive ? "Shock" : "Sad";
            if (game.preparing) 
            {
                hunterAnim.DoScaledAnimationAsync("Bop", 0.5f, animLayer: 0);
                hunterAnim.DoScaledAnimationAsync("Neutral", 0.5f, animLayer: 1);
                game.preparing = false; 
            }

            if (moose)
            {
                BeatAction.New(game, new()
                {
                    new(startBeat + 3, delegate {
                        anim.DoScaledAnimationAsync("MooseRaspberry", 0.5f, animLayer: 1);
                        SoundByte.PlayOneShotGame("balloonHunter/moose_raspberry");
                    }),
                });
            }
        }
    }
}
