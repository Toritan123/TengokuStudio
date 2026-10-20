using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using HeavenStudio.Common;

using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class CtrFruitBasket
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("fruitBasket", "Fruit Basket", "adb5e7", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        FruitBasket.instance.Bop(e.beat, e.length, e["bop"], e["bopAuto"]);
                    },

                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop", "Should the cats bop?"),
                        new Param("bopAuto", false, "Bop (Auto)", "Should the cats auto bop?"),
                    },

                    resizable = true,
                },

                new GameAction("apple", "Apple")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        FruitBasket.instance.SendFruit(eventCaller.currentEntity.beat, e["side"], (int)FruitBasket.Fruit.Apple, 0, e["successExpression"], e["missExpression"], e["expressionDuration"]);
                    },

                    parameters = new List<Param>()
                    {
                        new Param("side", FruitBasket.Side.Left, "Side", "The side the apple rolls from"),
                        new Param("successExpression", FruitBasket.Expression.None, "Success Expression", "The expression Courtney will make when the fruit is successfully hit"),
                        new Param("missExpression", FruitBasket.Expression.None, "Miss Expression", "The expression Courtney will make when the fruit is missed"),
                        new Param("expressionDuration", new EntityTypes.Float(0, 100, 0), "Expression Duration", "How long will Courtney make the expression?")
                    },

                    defaultLength = 3.0f,
                },

                new GameAction("lemon", "Lemon")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        FruitBasket.instance.SendFruit(eventCaller.currentEntity.beat, e["side"], (int)FruitBasket.Fruit.Lemon, 0, e["successExpression"], e["missExpression"], e["expressionDuration"]);
                    },

                    parameters = new List<Param>()
                    {
                        new Param("side", FruitBasket.Side.Left, "Side", "The side the lemon rolls from"),
                        new Param("successExpression", FruitBasket.Expression.None, "Success Expression", "The expression Courtney will make when the fruit is successfully hit"),
                        new Param("missExpression", FruitBasket.Expression.None, "Miss Expression", "The expression Courtney will make when the fruit is missed"),
                        new Param("expressionDuration", new EntityTypes.Float(0, 100, 0), "Expression Duration", "How long will Courtney make the expression?")
                    },

                    defaultLength = 5.0f,
                },

                new GameAction("melon", "Melon")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        FruitBasket.instance.SendFruit(eventCaller.currentEntity.beat, e["side"], (int)FruitBasket.Fruit.Melon, 0, e["successExpression"], e["missExpression"], e["expressionDuration"]);
                    },

                    parameters = new List<Param>()
                    {
                        new Param("side", FruitBasket.Side.Left, "Side", "The side the melon rolls from"),
                        new Param("successExpression", FruitBasket.Expression.None, "Success Expression", "The expression Courtney will make when the fruit is successfully hit"),
                        new Param("missExpression", FruitBasket.Expression.None, "Miss Expression", "The expression Courtney will make when the fruit is missed"),
                        new Param("expressionDuration", new EntityTypes.Float(0, 100, 0), "Expression Duration", "How long will Courtney make the expression?")
                    },

                    defaultLength = 6.0f,
                },

                new GameAction("daydream", "Daydream")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        FruitBasket.instance.Daydream(e.beat, e.length, e["thought"], e["daydreamExpression"]);
                    },

                    parameters = new List<Param>()
                    {
                        new Param("thought", FruitBasket.DaydreamType.Random, "Daydream", "What will Courtney daydream about?"),
                        new Param("daydreamExpression", FruitBasket.DaydreamExpression.None, "Daydream Expression", "What expression will Courtney make while daydreaming?")
                    },

                    defaultLength = 4.0f,
                    resizable = true
                },

                new GameAction("expression", "Expression")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        FruitBasket.instance.ExpressionCue(e.beat, e.length, e["expression"]);
                    },

                    parameters = new List<Param>()
                    {
                        new Param("expression", FruitBasket.AllExpressions.Happy, "Expression", "What expression will Courtney make?")
                    },

                    defaultLength = 4.0f,
                    resizable = true
                },
                new GameAction("color", "Courtney Appearance")
                {
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        FruitBasket.instance.Recolor(e.beat, e.length, e["colorStart"], e["colorEnd"], e["ease"]);
                    },
                    defaultLength = 1,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("colorStart", new Color(13 / 255f, 144 / 255f, 227 / 255f), "Start Color", "Set the color at the start of the event."),
                        new Param("colorEnd", new Color(13 / 255f, 144 / 255f, 227 / 255f), "End Color", "Set the color at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Instant, "Ease", "Set the easing of the action.")
                    }
                }
            },
            new List<string>() { "ctr", "normal" },
            "ctrfruitbasket", "en",
            new List<string>() { },
            chronologicalSortKey: 2
            );
        }
    }
}
namespace HeavenStudio.Games
{
    public class FruitBasket : Minigame
    {
        public static FruitBasket instance;

        public Transform fruitHolder;

        public GameObject applePrefab;
        public GameObject lemonPrefab;
        public GameObject melonPrefab;

        public Animator courtneyAnimator;
        public GameObject hoopL;
        public GameObject hoopR;

        public SpriteRenderer courtneySprite;
        public SpriteRenderer courtneyExtendedSprite;
        public SpriteRenderer courtneyHoleSprite;

        public GameObject thoughtBubblePrefab;

        public Animator catsAnimator;

        public bool isDaydreaming = false;

        private bool goBop = true;
        private bool playedDunkSfx = false;
        private bool expressing = false;

        public List<int> basketSidesThisFrame = new List<int>();
        public double basketBeat = -1;
        public GameEvent bop = new GameEvent();


        [SerializeField] SuperCurveObject.Path[] fruitPaths;
        static List<QueuedFruit> queuedFruits = new List<QueuedFruit>();
        struct QueuedFruit
        {
            public double beat;
            public Side side;
            public Fruit fruit;
            public int bothSidesRole;
        }

        public enum Side
        {
            Left,
            Right,
            Both
        }

        public enum Fruit
        {
            Apple,
            Lemon,
            Melon
        }

        public enum Expression
        {
            None,
            Happy,
            Cry
        }

        public enum DaydreamExpression
        {
            None,
            Kiss,
            Tongue,
            Worried
        }

        public enum AllExpressions
        {
            Happy,
            Cry,
            DaydreamNone,
            DaydreamKiss,
            DaydreamTongue,
            DaydreamWorried
        }

        public enum DaydreamType
        {
            Random,
            Burger,
            BurgerAndDrink,
            Girl,
            TicTacToe,
            Spider,
            Skateboard,
            Dog,
            BasketballPlayer,
            Panda,
            Pizza
        }

        private void Awake()
        {  
            instance = this;
        }

        private void Update()
        {
            playedDunkSfx = false;

            if (basketSidesThisFrame.Contains(0) && basketSidesThisFrame.Contains(1))
            {
                SoundByte.PlayOneShotGame("fruitBasket/basket", basketBeat);
            }
            else if (basketSidesThisFrame.Contains(1))
            {
                SoundByte.PlayOneShotGame("fruitBasket/basketL", basketBeat);
            }
            else if (basketSidesThisFrame.Contains(0))
            {
                SoundByte.PlayOneShotGame("fruitBasket/basketR", basketBeat);
            }

            basketSidesThisFrame.Clear();
            basketBeat = -1;

            if (Conductor.instance.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
            {
                if (goBop) BopSingle();
            }

            if (!IsExpectingInputNow(InputAction_BasicPress) && PlayerInput.GetIsAction(InputAction_BasicPress))
            {
                Whiff();
            }

            RecolorUpdate();
        }

        public SuperCurveObject.Path GetPath(string name)
        {
            foreach (SuperCurveObject.Path path in fruitPaths)
            {
                if (path.name == name)
                {
                    return path;
                }
            }
            return default(SuperCurveObject.Path);
        }

        new void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            foreach (SuperCurveObject.Path path in fruitPaths)
            {
                if (path.preview)
                {
                    applePrefab.GetComponent<Scripts_FruitBasket.Apple>().DrawEditorGizmo(path);
                }
            }
        }

        public void InstantiateFruit(double beat, Side side, Fruit fruit, int bothSidesRole, Expression successExpression, Expression missExpression, double expressionDuration) 
        {

            switch(fruit)
            {
                case Fruit.Apple:
                    GameObject instantiatedFruit = GameObject.Instantiate(applePrefab, fruitHolder);
                    var appleComp = instantiatedFruit.GetComponent<Scripts_FruitBasket.Apple>();

                    appleComp.startBeat = beat;
                    appleComp.side = side;
                    appleComp.bothSidesRole = bothSidesRole;
                    appleComp.successExpression = successExpression;
                    appleComp.missExpression = missExpression;
                    appleComp.expressionDuration = expressionDuration;
                    instantiatedFruit.SetActive(true);

                    break;

                case Fruit.Lemon:
                    instantiatedFruit = GameObject.Instantiate(lemonPrefab, fruitHolder);
                    var lemonComp = instantiatedFruit.GetComponent<Scripts_FruitBasket.Lemon>();

                    lemonComp.startBeat = beat;
                    lemonComp.side = side;
                    lemonComp.bothSidesRole = bothSidesRole;
                    lemonComp.successExpression = successExpression;
                    lemonComp.missExpression = missExpression;
                    lemonComp.expressionDuration = expressionDuration;
                    instantiatedFruit.SetActive(true);

                    break;

                case Fruit.Melon:
                    instantiatedFruit = GameObject.Instantiate(melonPrefab, fruitHolder);
                    var melonComp = instantiatedFruit.GetComponent<Scripts_FruitBasket.Melon>();
                    melonComp.startBeat = beat;
                    melonComp.side = side;
                    melonComp.bothSidesRole = bothSidesRole;
                    melonComp.successExpression = successExpression;
                    melonComp.missExpression = missExpression;
                    melonComp.expressionDuration = expressionDuration;
                    instantiatedFruit.SetActive(true);

                    break;
            }
        }

        public void SendFruit(double beat, int side, int fruit, int bothSidesRole, int successExpression, int missExpression, double expressionDuration)
        {

            if ((Side)side == Side.Both)
            {
                SendFruit(beat, (int)Side.Left, fruit, 1, successExpression, missExpression, expressionDuration);
                SendFruit(beat, (int)Side.Right, fruit, 2, successExpression, missExpression, expressionDuration);
                return;
            }

            if (GameManager.instance.currentGame == "fruitBasket")
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate {
                        instance.InstantiateFruit(
                            beat, 
                            (Side)side, 
                            (Fruit)fruit, 
                            bothSidesRole, 
                            (Expression)successExpression,
                            (Expression)missExpression, 
                            expressionDuration); 
                    })
                });
            } 
            else
            {
                queuedFruits.Add(new QueuedFruit() 
                { 
                    beat = beat,
                    side = (Side)side,
                    fruit = (Fruit)fruit,
                    bothSidesRole = bothSidesRole
                });
            }
        }

        public void Daydream(double beat, double length, int daydreamType, int daydreamExpression)
        {
            GameObject instantiatedBubble = GameObject.Instantiate(thoughtBubblePrefab, transform);
            var daydreamComp = instantiatedBubble.GetComponent<Scripts_FruitBasket.Daydream>();

            daydreamComp.fadeOutTime = beat + length;
            daydreamComp.daydreamType = (DaydreamType)daydreamType;

            string expressionName = ((DaydreamExpression)daydreamExpression).ToString();
            
            courtneyAnimator.DoScaledAnimationAsync("Daydream" + expressionName + "Start", 0.5f);
            
            BeatAction.New(instance, new List<BeatAction.Action>()
            {     
                new BeatAction.Action(beat + length, delegate
                {
                    courtneyAnimator.DoScaledAnimationAsync("Daydream" + expressionName + "Stop", 0.5f);
                })
            });
            
            instantiatedBubble.SetActive(true);
        }

        public void HitFruitAnimation()
        {
            if (playedDunkSfx) { return; }
            playedDunkSfx = true;
            SoundByte.PlayOneShotGame("fruitBasket/dunk");
            courtneyAnimator.DoScaledAnimationAsync("fruit_hit", 0.5f);
        }
        
        public void MissFruitAnimation(float timing)
        {
            SoundByte.PlayOneShot("miss");
            courtneyAnimator.DoScaledAnimationAsync("miss", 0.5f);
        }

        public void HoopAnimation(string animName, bool isLeft, Fruit fruit)
        {
            GameObject hoop = isLeft ? hoopL : hoopR;
            Animator hoopAnimator = hoop.GetComponent<Animator>();
            ParticleSystem hoopParticle = hoop.transform.Find("AppleScore").gameObject.GetComponent<ParticleSystem>();

            switch (fruit)
            {
                case Fruit.Apple:
                    hoopParticle = hoop.transform.Find("AppleScore").gameObject.GetComponent<ParticleSystem>();
                    break;
                case Fruit.Lemon:
                    hoopParticle = hoop.transform.Find("LemonScore").gameObject.GetComponent<ParticleSystem>();
                    break;
                case Fruit.Melon:
                    hoopParticle = hoop.transform.Find("MelonScore").gameObject.GetComponent<ParticleSystem>();
                    break;
            }

            hoopAnimator.DoScaledAnimationAsync(animName, 0.5f);
            hoopParticle.Play();
        }
        
        public void Whiff()
        {
            courtneyAnimator.DoScaledAnimationAsync("whiff", 0.5f);
        }

        public void Bop(double beat, float length, bool bop, bool autoBop)
        {
            goBop = autoBop;
            for (int i = 0; i < length; i++)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + i, delegate
                    {
                        BopSingle();
                    })
                });
            }
        }

        public void BopSingle()
        {
            catsAnimator.DoScaledAnimationAsync("catsBop", 0.5f);
        }
        
        public void CourtneyExpression(string expressionName, double stopBeat)
        {
            if (expressing == true) { return; }
            if (expressionName == "None") { return; }
            
            expressing = true;
            
            courtneyAnimator.DoScaledAnimationAsync(expressionName + "Start", 0.5f);
            
            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(stopBeat, delegate
                    {
                        expressing = false;
                        courtneyAnimator.DoScaledAnimationAsync(expressionName + "Stop", 0.5f);
                    })
                });
        }

        public void ExpressionCue(double beat, float length, int expression)
        {
            string expressionName = ((AllExpressions)expression).ToString();

            CourtneyExpression(expressionName, beat + length);
        }

        private ColorEase courtneyColorEase = new(new Color(13 / 255f, 144 / 255f, 227 / 255f));

        //call this in update
        private void RecolorUpdate()
        {
            courtneySprite.color = courtneyColorEase.GetColor();
            courtneyExtendedSprite.color = courtneyColorEase.GetColor();
            courtneyHoleSprite.color = courtneyColorEase.GetColor();
        }

        public void Recolor(double beat, float length, Color colorStartSet, Color colorEndSet, int ease)
        {
            courtneyColorEase = new ColorEase(beat, length, colorStartSet, colorEndSet, ease);
        }

        //call this in OnPlay(double beat) and OnGameSwitch(double beat)
        private void PersistColor(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("fruitBasket", new string[] { "color" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                Recolor(lastEvent.beat, lastEvent.length, lastEvent["colorStart"], lastEvent["colorEnd"], lastEvent["ease"]);
            }
        }

        public override void OnPlay(double beat)
        {
            PersistColor(beat);
        }

        public override void OnGameSwitch(double beat)
        {
            PersistColor(beat);
        }
    }
}