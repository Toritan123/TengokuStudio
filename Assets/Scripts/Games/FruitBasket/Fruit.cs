using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using HeavenStudio.Common;

namespace HeavenStudio.Games.Scripts_FruitBasket
{
    public class Fruit : SuperCurveObject
    {
        public double startBeat;
        public int bothSidesRole = 0;

        public FruitBasket.Side side;
        public FruitBasket.Fruit fruit;

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

        private float rollTimeElapsed;

        private Conductor cond;

        void Awake()
        {
            game = FruitBasket.instance;
            cond = Conductor.instance;

            isLeft = side == FruitBasket.Side.Left;

            sprite.transform.position = isLeft ? leftPipe.position : rightPipe.position;

            switch(fruit)
            {
                case FruitBasket.Fruit.Apple:
                    if (bothSidesRole == 0)
                    {
                        SoundByte.PlayOneShotGame("fruitBasket/" + (isLeft ? "appleL" : "appleR"));
                    }
                    else if (bothSidesRole == 1)
                    {
                        Debug.Log("sfx");
                        SoundByte.PlayOneShotGame("fruitBasket/apple");
                    }

                    game.ScheduleInput(startBeat, 2f, FruitBasket.InputAction_BasicPress, HitSuccess, HitMiss, HitEmpty);

                    break;
            }
            
        }

        void Update()
        {
            switch (fruitState)
            {
                case 0: // Fruit is rolling
                    FruitRollUpdate();
                    break;
                case 1: // Fruit was barely hit
                    FruitRollUpdate();
                    FruitBarelyUpdate();
                    break;
                case 2: // Fruit has been successfully hit
                    if (!fadingOut) { FruitRollUpdate(); }
                    HitFruitUpdate();
                    break;
            }

            if (cond.songPositionInBeatsAsDouble > startBeat + 5f)
            {
                GameObject.Destroy(gameObject);
            }

            if (fadingOut && !hoopAnimPlayed && !missed)
            {
                hoopAnimPlayed = true;
                game.HoopAnimation("hoopScore", !isLeft, FruitBasket.Fruit.Apple);
            }
        }

        void FruitRollUpdate()
        {
            float startX = isLeft ? -8f : 8f;
            float endX = isLeft ? 8f : -8f;

            float startRot = 0f;
            float endRot = isLeft ? -720f : 720f;

            sprite.transform.position = new Vector3(
                Mathf.Lerp(startX, endX, rollTimeElapsed / (4f * cond.pitchedSecPerBeat)),
                transform.position.y,
                0f
            );

            Quaternion rotation = sprite.transform.rotation;
            rotation.eulerAngles = new Vector3(0, 0,
                Mathf.Lerp(startRot, endRot, (rollTimeElapsed / (4f * cond.pitchedSecPerBeat))) * missedDir
            );

            sprite.transform.rotation = rotation;

            rollTimeElapsed += Time.deltaTime;
        }

        void FruitBarelyUpdate()
        {
            sprite.transform.position = GetPathPositionFromBeat(
                currentPath,
                Math.Max(startBeat + 2f,
                Conductor.instance.songPositionInBeatsAsDouble),
                out double height,
                startBeat + 2f);

            fadingOut = true;

            SpriteRenderer renderer = sprite.GetComponent<SpriteRenderer>();
            renderer.color = new Color(1, 1, 1, renderer.color.a - (Time.deltaTime * cond.songBpm / 120));
        }

        void HitFruitUpdate()
        {
            sprite.transform.position = GetPathPositionFromBeat(currentPath, Math.Max(startBeat + 2f, Conductor.instance.songPositionInBeatsAsDouble), out double height, startBeat + 2f);
            if (Conductor.instance.songPositionInBeatsAsDouble > startBeat + 3f)
            {
                fadingOut = true;

                sprite.transform.eulerAngles = new Vector3(0f, 0f, 0f);
                SpriteRenderer renderer = sprite.GetComponent<SpriteRenderer>();
                renderer.color = new Color(1, 1, 1, renderer.color.a - Time.deltaTime * cond.songBpm / 80);
            }
        }

        void HitSuccess(PlayerActionEvent caller, float state)
        {
            Debug.Log(state);
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
                    }
                    else
                    {
                        currentPath = game.GetPath(isLeft ? "ToRightMiss" : "ToLeftMiss");
                    }


                    missedDir = 1;
                }
                else if (state <= -1f)
                {
                    currentPath = game.GetPath(isLeft ? "ToLeftMiss" : "ToRightMiss");
                    missedDir = -1;
                }


                fruitState = 1;
            }
            else
            {
                game.HitFruitAnimation();

                currentPath = game.GetPath(isLeft ? "ToRightBasket" : "ToLeftBasket");

                fruitState = 2;
            }

        }

        void HitMiss(PlayerActionEvent caller)
        {
            fruitState = 0;
        }

        void HitEmpty(PlayerActionEvent caller)
        {

        }
    }
}

