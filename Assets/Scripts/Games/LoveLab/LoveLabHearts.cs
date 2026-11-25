using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using System;
using DG.Tweening;
using HeavenStudio.Editor.Track;

namespace HeavenStudio.Games.Scripts_LoveLab
{
    public class LoveLabHearts : MonoBehaviour
    {
        [SerializeField] heartType whatHeartType;
        public double prevHeartBeat;
        public double heartBeat;
        public double nextHeartBeat;
        public Animator heartAnim;
        [SerializeField] ParticleSystem death;
        [SerializeField] GameObject outline;
        [SerializeField] GameObject fill;
        LoveLab game;
        Conductor cond;

        public bool stop;
        Vector3 origPos;
        public float addPos = 0;
        float speedMultiplier;
        public float startPos = 0f;
        public double length;
        double duration;
        double startTime;
        double endTime;
        public int heartCount;
        public bool hasChecked;
        public float playBackSpeed;
        public double intervalSpeed;
        public float _step;
        public bool tweenComplete;
        Tween movePos;
        public bool onlyOne = true;
        public Vector3 end;
        public double timer;
        public bool isWaiting = true;
        SpriteRenderer heartRenderer;
        public Vector3 dropStart;
        public double currentBeat;

        //y = 0 (endpoint)

        public int getHeartType()
        {
            return (int)whatHeartType;
        }

        enum heartType
        {
            guyHeart,
            girlHeart,
            completeHeart
        }

        private void Awake()
        {if (whatHeartType != heartType.completeHeart)
            {
            if(heartCount == 0)
            {
                transform.position = new Vector2(transform.position.x, transform.position.y + 2f);
            }
            else
            {
                transform.position = new Vector2(transform.position.x, transform.position.y + 4f);
            }
            }
            
            playBackSpeed = GlobalGameManager.playbackSpeed;
            addPos = 2.5f; //3f
            heartRenderer = this.GetComponent<SpriteRenderer>();
            
        }

        void Start()
        {
            DOTween.SetTweensCapacity(2000, 100);
            game = LoveLab.instance;
            cond = Conductor.instance;
            origPos = transform.position;

            speedMultiplier = game.speedForHeartsMultiplier;

            if(heartCount == 0)
            {
                _step = 1;
            }

            var a = (float)(length * cond.secPerBeat) / playBackSpeed;
            endValue = transform.position.y + addPos;
            upTween = transform.DOMoveY(endValue, (float)((length * cond.secPerBeat) / playBackSpeed)).SetEase(Ease.OutBack);
        }     

        public Tweener upTween;
        public float endValue;

        void Update()
        {
            currentBeat = cond.songPositionInBeatsAsDouble;

            if(!stop && whatHeartType != heartType.completeHeart)
            {
                goUp((float)((intervalSpeed * cond.secPerBeat / cond.GetBpmAtBeat(heartBeat)) * 60) * playBackSpeed);
            }
            else if (!isWaiting)
            {

                float normalizedBeat = Conductor.instance.GetPositionFromBeat(heartBeat, ((timer/2)*playBackSpeed));
                
                float newPosY = EasingFunction.EaseInQuad(dropStart.y, end.y, normalizedBeat);

                if (normalizedBeat<=1) transform.position = new Vector2(dropStart.x, newPosY);
                else Destroy(gameObject);
            }
        }

        public void goUp(float timer)
        {
            
            transform.DOMoveY(origPos.y + (addPos), timer).SetEase(Ease.OutBack);
        }

        public void tempDestroy(double startBeat)
        {
            BeatAction.New(game, new List<BeatAction.Action>()
            {
            new BeatAction.Action(startBeat + 1f, delegate
                {Destroy(this.gameObject);}),});

        }

        public async void deadHeart()
        {
            //just instantiate this shit idk
            outline.SetActive(false);
            fill.SetActive(false);
            death.Play();

            Destroy(gameObject, 0.5f);
        }

        private void OnDestroy()
        {
            DOTween.KillAll();
        }
    }
}

