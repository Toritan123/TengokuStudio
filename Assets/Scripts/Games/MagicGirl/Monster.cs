using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_MagicGirl
{
    public class Monster : MonoBehaviour
    {
        public double relativeBeat;
        public bool hasSpawned = false;
        public bool isFleeing = false;
        double fleeBeat;

        [SerializeField] Animator anim;
        [SerializeField] int location;
        [SerializeField] BezierCurve3D fleeCurve;
        [SerializeField] Vector3 normalLocation = new Vector3();
        [SerializeField] ParticleSystem hitEffect;

        public MagicGirl game;

        //Animator Mako = game.Mako;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (isFleeing)
            {
                float flyPos = game.conductor.GetPositionFromBeat(fleeBeat, 0.75f);
                transform.position = fleeCurve.GetPoint(flyPos);
                if (flyPos > 1f) 
                {
                    isFleeing = false;
                    transform.position = normalLocation;
                }
            }
        }

        public void Spawn(double rBeat)
        {
            anim.DoScaledAnimationAsync("MonsterAppear", 0.5f);
            relativeBeat = rBeat;
            hasSpawned = true;
        }

        public void PassTurn(double beat, float length)
        {
            if (!hasSpawned) return;

            game.ScheduleInput(beat, relativeBeat+length, MagicGirl.InputAction_Press, Hit, Miss, null);

            fleeBeat = beat + relativeBeat + length;
        }

        private void Hit(PlayerActionEvent caller, float state)
        {
            anim.DoScaledAnimationAsync("MonsterScared", 0.5f);
            hasSpawned = false;
            isFleeing = true;

            switch (location)
            {
                case 0:
                    game.Mako.DoScaledAnimationAsync("U_Left_Move", 0.5f, animLayer: 0);
                    break;
                case 1:
                    game.Mako.DoScaledAnimationAsync("U_Right_Move", 0.5f, animLayer: 0);
                    break;
                case 2:
                    game.Mako.DoScaledAnimationAsync("D_Left_Move", 0.5f, animLayer: 0);
                    break;
                case 3:
                    game.Mako.DoScaledAnimationAsync("D_Right_Move", 0.5f, animLayer: 0);
                    break;
            }

            SoundByte.PlayOneShotGame("magicGirl/enemy");

            if (state is >= 1f or <= (-1f)) // barely
            {
                SoundByte.PlayOneShot("nearMiss", volume: 2);
                game.MakoFace.DoScaledAnimationAsync("FaceBarely", 0.5f);
            }
            else
            {
                SoundByte.PlayOneShotGame("magicGirl/hit");
                game.MakoFace.DoScaledAnimationAsync("FaceWink", 0.5f);
                hitEffect.Play();
            }
        }

        private void Miss(PlayerActionEvent caller)
        {
            if (location > 1) { 
                anim.DoScaledAnimationAsync("MonsterAttackD", 0.5f); 
            }
            else
            {
                anim.DoScaledAnimationAsync("MonsterAttackU", 0.5f);
            }

            game.MakoFace.DoScaledAnimationAsync("FaceMiss", 0.5f);
            SoundByte.PlayOneShotGame("magicGirl/doingoing");
            if (location == 0 || location == 2)
            {
                game.Mako.DoScaledAnimationAsync("Hurt_L", 0.5f, animLayer: 0);
            }
            else
            {
                game.Mako.DoScaledAnimationAsync("Hurt_R", 0.5f, animLayer: 0);
            }
            hasSpawned = false;
        }
    }
}
