using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using DG.Tweening;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_FruitBasket
{
    public class Lemon : SuperCurveObject
    {
        public double startBeat;
        public int bothSidesRole = 0;
        public FruitBasket.Side side;

        public FruitBasket.Expression successExpression;
        public FruitBasket.Expression missExpression;
        public double expressionDuration;

        [SerializeField] private GameObject sprite;

        [SerializeField] private Transform leftPipe;
        [SerializeField] private Transform rightPipe;

        private int fruitState = 0;

        private Path currentPath;
        private FruitBasket game;

        private bool fadingOut = false;
        private bool hoopAnimPlayed = false;
        private bool missed = false;
        private int missedDir = 1;

        private bool isLeft;

        private Conductor cond;

        private PathPos nextPos;
        private float rollTimeElapsed;
        private float fadeOutTimeElapsed;

        private float hitRotateTimeElapsed;

        void Awake()
        {
            game = FruitBasket.instance;
            isLeft = side == FruitBasket.Side.Left;

            sprite.transform.position = isLeft ? leftPipe.position : rightPipe.position;

            if (bothSidesRole == 0)
            {
                SoundByte.PlayOneShotGame("fruitBasket/" + (isLeft ? "lemonL" : "lemonR"));
            }
            else if (bothSidesRole == 1)
            {
                SoundByte.PlayOneShotGame("fruitBasket/lemon");
            }

            game.ScheduleInput(startBeat, 4f, FruitBasket.InputAction_BasicPress, HitSuccess, HitMiss, HitEmpty);

            currentPath = game.GetPath(isLeft ? "LemonRollRight" : "LemonRollLeft");

            cond = Conductor.instance;
        }

        void Update()
        {
            switch (fruitState)
            {
                case 0: // Fruit is rolling
                    FruitRollUpdate();
                    break;
                case 1: // Fruit was barely hit
                    FruitBarelyUpdate();
                    break;
                case 2: // Fruit has been successfully hit
                    HitFruitUpdate();
                    break;
            }

            if (cond.songPositionInBeatsAsDouble > startBeat + 8f) {
                GameObject.Destroy(gameObject);
            }

            if (fadingOut && !hoopAnimPlayed && !missed)
            {
                hoopAnimPlayed = true;
                game.HoopAnimation("hoopScore", !isLeft, FruitBasket.Fruit.Lemon);
            }
        }
        private static float QuadEase(float t, int type)
        {
            switch (type)
            {
                case 0: return t * t;                        
                case 1: return t * (2f - t);                 
                case 2:
                    return t < 0.5f                      
                ? 2f * t * t
                : -1f + (4f - 2f * t) * t;
            }

            return 0f;
        }

        void FruitRollUpdate()
        {
            // This updates currentPathPos
            GetPathPositionFromBeat(currentPath, Math.Max(startBeat, cond.songPositionInBeatsAsDouble), startBeat);

            if (currentPathPos.target == nextPos.target)
            {
                rollTimeElapsed = 0f;
            }

            nextPos = currentPath.positions[Array.IndexOf(currentPath.positions, currentPathPos) + 1];

            float startX = currentPathPos.target.position.x;
            float endX = nextPos.target.position.x;

            float startRot = currentPathPos.values[1].value;
            float endRot = nextPos.values[1].value;

            float startY;
            float endY;

            float fruitRollDuration = currentPathPos.duration * cond.pitchedSecPerBeat;
            int yLerpEase;

            // Enter point
            if (currentPathPos.target == currentPath.positions[0].target)
            {
                startY = -0.24f;
                endY = 0f;
                yLerpEase = 0;
            }
            // Exit point
            else if (currentPathPos.target == currentPath.positions[currentPath.positions.Length - 1].target)
            {
                startY = 0f;
                endY = -0.24f;
                yLerpEase = 1;
            }
            // Any point in the middle of the path
            else
            {
                if (rollTimeElapsed / fruitRollDuration < 0.5)
                {
                    yLerpEase = 2;
                    startY = 0f;
                    endY = -0.45f;
                }
                else
                {
                    yLerpEase = 2;
                    startY = -0.45f;
                    endY = 0f;
                }
            }

            float t = Mathf.Clamp01(rollTimeElapsed / fruitRollDuration);
            float easedT = QuadEase(t, 2); 
            float easedY = QuadEase(t, yLerpEase);

            sprite.transform.position = new Vector3(
                Mathf.Lerp(startX, endX, easedT),
                transform.position.y + Mathf.Lerp(startY, endY, easedY),
                0f
            );

            Quaternion rotation = sprite.transform.rotation;
            rotation.eulerAngles = new Vector3(0, 0,
                Mathf.Lerp(startRot, endRot, easedT)
            );

            sprite.transform.rotation = rotation;

            rollTimeElapsed += Time.deltaTime;
        }


        void FruitBarelyUpdate()
        {
            float startRot = 0f;
            float endRot = (missedDir == -1) ? 360f : -360f;

            Quaternion rotation = sprite.transform.rotation;
            rotation.eulerAngles = new Vector3(0, 0,
                Mathf.Lerp(startRot, endRot, hitRotateTimeElapsed / (2f * cond.pitchedSecPerBeat))
            );
            sprite.transform.rotation = rotation;
            hitRotateTimeElapsed += Time.deltaTime;

            sprite.transform.position = GetPathPositionFromBeat(
                currentPath,
                Math.Max(startBeat + 4f,
                Conductor.instance.songPositionInBeatsAsDouble),
                out double height,
                startBeat + 4f);

            fadingOut = true;

            missedDir = isLeft ? (int)currentPathPos.values[0].value : (int)-currentPathPos.values[0].value;

            SpriteRenderer renderer = sprite.GetComponent<SpriteRenderer>();
            renderer.color = new Color(1, 1, 1, renderer.color.a - (Time.deltaTime * cond.songBpm / 120));
        }

        void HitFruitUpdate()
        {
            if (!fadingOut)
            {
                float startRot = isLeft ? -90f : 90f;
                float endRot = isLeft ? -360f : 360f;

                Quaternion rotation = sprite.transform.rotation;
                rotation.eulerAngles = new Vector3(0, 0,
                    Mathf.Lerp(startRot, endRot, hitRotateTimeElapsed / (1f * cond.pitchedSecPerBeat))
                );
                sprite.transform.rotation = rotation;
                hitRotateTimeElapsed += Time.deltaTime;
            }
            

            if (Conductor.instance.songPositionInBeatsAsDouble > startBeat + 5f)
            {
                float startY = 0f;
                float endY = -7.0f;
                float fruitFadeOutDuration = currentPath.positions[1].duration * cond.pitchedSecPerBeat;
                
                sprite.transform.position = new Vector3(
                    currentPath.positions[1].target.position.x,
                    currentPath.positions[1].target.position.y + Mathf.Lerp(startY, endY, (fadeOutTimeElapsed / fruitFadeOutDuration) * (fadeOutTimeElapsed / fruitFadeOutDuration)),
                    0f
                );
                
                fadingOut = true;
                
                sprite.transform.eulerAngles = new Vector3(0f, 0f, 0f);
                SpriteRenderer renderer = sprite.GetComponent<SpriteRenderer>();
                renderer.color = new Color(1, 1, 1, renderer.color.a - Time.deltaTime * cond.songBpm / 80);
                
                fadeOutTimeElapsed += Time.deltaTime;
            }
            else
            {
                sprite.transform.position = GetPathPositionFromBeat(currentPath, Math.Max(startBeat + 4f, Conductor.instance.songPositionInBeatsAsDouble), out double height, startBeat + 4f);
            }
        }
        
        void DoExpression(string expression) {
            if (bothSidesRole == 2) { return; }
            if (expressionDuration == 0f) { return; }
            
            BeatAction.New(game, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(startBeat + 6f, delegate
                    {
                        game.CourtneyExpression(expression, startBeat + 6f + expressionDuration);
                    }),
                });
        }

        void HitSuccess(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1)
            {
                missed = true;
                Debug.Log(state);
                game.MissFruitAnimation(state);

                if (state >= 1f)
                {
                    // man i dunno how these two different miss animations are chosen, have at thee random number generator

                    if (UnityEngine.Random.Range(1, 2) == 1)
                    {
                        currentPath = game.GetPath(isLeft ? "ToRightBasketMiss" : "ToLeftBasketMiss");
                        sprite.GetComponent<SpriteRenderer>().sortingOrder = 7;
                    }
                    else
                    {
                        currentPath = game.GetPath(isLeft ? "ToRightMiss" : "ToLeftMiss");
                        sprite.GetComponent<SpriteRenderer>().sortingOrder = 7;
                    }
                }
                else if (state <= -1f)
                {
                    currentPath = game.GetPath(isLeft ? "ToLeftMiss" : "ToRightMiss");
                    sprite.GetComponent<SpriteRenderer>().sortingOrder = 7;
                }


                fruitState = 1;

                DoExpression(missExpression.ToString());
            }
            else
            {
                game.HitFruitAnimation();
                game.basketSidesThisFrame.Add(isLeft ? 0 : 1);
                game.basketBeat = startBeat + 5f;

                currentPath = game.GetPath(isLeft ? "ToRightBasket" : "ToLeftBasket");
                sprite.GetComponent<SpriteRenderer>().sortingOrder = 7;

                fruitState = 2;
                
                DoExpression(successExpression.ToString());
            }
        }

        void HitMiss(PlayerActionEvent caller)
        {
            DoExpression(missExpression.ToString());
        }

        void HitEmpty(PlayerActionEvent caller)
        {

        }
    }
}

