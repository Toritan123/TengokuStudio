using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrCoinLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("coinToss", "Coin Toss", "f9ec3b", false, false, new List<GameAction>()
            {
                new GameAction("toss", "Toss Coin")
                {
                    function = delegate { CoinToss.instance.TossCoin(eventCaller.currentEntity.beat, eventCaller.currentEntity["type"], eventCaller.currentEntity["toggle"]); },
                    defaultLength = 7,
                    parameters = new List<Param>()
                    {
                        new Param("type", CoinToss.CoinVariation.Default, "Variation", "Choose the version of the cue to play."),
                        new Param("toggle", false, "Audience Reaction", "Toggle if there should be cheering or booing after the cue."),
                    }
                },
                new GameAction("set hand", "Set Hand")
                {
                    function = delegate { CoinToss.instance.SetHand(eventCaller.currentEntity["hand"]); },
                    defaultLength = 1,
                    parameters = new List<Param>()
                    {
                        new Param("hand", CoinToss.WhoseHand.Woman, "Hand", "Choose whose hand will be holding the coin.")
                    }
                },
                new GameAction("show image", "Show Image")
                {
                    function = delegate { CoinToss.instance.ShowImage(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity["instantShow"], eventCaller.currentEntity["instantHide"]); },
                    defaultLength = 4,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("instantShow", true, "Instant Show", "Toggle if the image will show instantly."),
                        new Param("instantHide", false, "Instant Hide", "Toggle if the image will hide instantly.")
                    }
                },
                new GameAction("fade background color", "Background Appearance")
                {
                    function = delegate { var e = eventCaller.currentEntity;
                        CoinToss.instance.BackgroundColor(e.beat, e.length, e["colorStart"], e["colorEnd"], e["colorStartF"], e["colorEndF"], e["ease"]); },
                    resizable = true,
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("colorStart", CoinToss.defaultBgColor, "BG Start Color", "Set the color for the outer ring at the start of the event."),
                        new Param("colorEnd", CoinToss.defaultBgColor, "BG End Color", "Set the color for the outer ring at the end of the event."),
                        new Param("colorStartF", CoinToss.defaultBgColor, "FG Start Color", "Set the color for the inner oval at the start of the event."),
                        new Param("colorEndF", CoinToss.defaultBgColor, "FG End Color", "Set the color for the inner oval at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing for the action.")
                    }
                },

                //left in for backwards-compatibility, but cannot be placed
                new GameAction("set background color", "Set Background Color")
                {
                    function = delegate { var e = eventCaller.currentEntity; CoinToss.instance.BackgroundColor(e.beat, e.length, e["colorA"], e["colorA"], e["colorB"], e["colorB"], (int)Util.EasingFunction.Ease.Instant); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("colorA", CoinToss.defaultBgColor, "Background Color", "The background color to change to"),
                        new Param("colorB", CoinToss.defaultFgColor, "Foreground Color", "The foreground color to change to")
                    },
                    hidden = true
                },
            },
            new List<string>() { "ntr", "aim" },
            "ntrcoin", "en",
            new List<string>() { },
            chronologicalSortKey: 101,
            font: "Comet"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    //using Scripts_CoinToss;
    public class CoinToss : Minigame
    {

        //Right now, you can only throw one coin at a time.
        //..Which makes sense, you only have one coin in the original game

        //Though it would need a bit of code rewrite to make it work with multiple coins

        public static PlayerInput.InputAction InputAction_TouchFlick =
            new("NtrCoinTouchFlick", new int[] { IAEmptyCat, IAFlickCat, IAEmptyCat },
            IA_Empty, IA_TouchFlick, IA_Empty);

        public static CoinToss instance { get; set; }

        public static Color defaultBgColor = new Color(0.97f, 0.97f, 0.26f);
        public static Color defaultFgColor = new Color(1f, 1f, 0.51f);


        [Header("Backgrounds")]
        public SpriteRenderer fg;
        public SpriteRenderer bg;
        public SpriteRenderer imageBG;

        [Header("Animators")]
        public Animator handAnimator;
        public Animator manHand;
        public Animator handHolder;
        public Animator manHolder;
        public Animator imageAnim;

        public Boolean isThrowing;

        public bool audienceReacting;

        public bool coinOnMan = false;

        public PlayerActionEvent coin;

        public enum CoinVariation
        {
            Default,
            Cowbell,
        }
        
        public enum WhoseHand
        {
            Woman,
            Man,
        }

        private void Awake()
        {
            instance = this;
            isThrowing = false;

            coin = null;

            BackgroundColorUpdate();
        }

        private void Update()
        {
            BackgroundColorUpdate();

            if (coin != null && (!coin.canHit) && PlayerInput.GetIsAction(InputAction_TouchFlick))
            {
                coin.CanHit(true);
                isThrowing = true;
                handAnimator.Play("Throw_empty", 0, 0);
            }
        }

        public override void OnPlay(double beat) => PersistColor(beat);
        public override void OnGameSwitch(double beat) => PersistColor(beat);

        public void TossCoin(double beat, int type, bool audienceReacting)
        {
            if (coin != null) return;

            //Play sound and animations
            SoundByte.PlayOneShotGame("coinToss/throw");
            if (coinOnMan)
            {
                manHand.Play("Toss", 0, 0);
                handHolder.DoScaledAnimationAsync("Enter", 0.5f);
                handAnimator.Play("Idle_open", 0, 0);

                BeatAction.New(instance, new List<BeatAction.Action>() {
                    new BeatAction.Action(beat + 1, delegate
                        {
                            manHolder.DoScaledAnimationAsync("ManExit", 0.5f);
                        })
                });
            }
            else
            {
                handAnimator.Play("Throw", 0, 0);
            }
            //Game state says the hand is throwing the coin
            isThrowing = true;
            coinOnMan = false;

            switch (type)
            {
                case (int)CoinVariation.Cowbell:
                    //this was intentional. it was to avoid the throw and cowbells to go offbeat.
                    SoundByte.PlayOneShotGame("coinToss/cowbell1");
                    MultiSound.Play(new MultiSound.Sound[] {
                        new MultiSound.Sound("coinToss/cowbell2", beat + 1f, offset: 0.01f),
                        new MultiSound.Sound("coinToss/cowbell1", beat + 2f, offset: 0.01f),
                        new MultiSound.Sound("coinToss/cowbell2", beat + 3f, offset: 0.01f),
                        new MultiSound.Sound("coinToss/cowbell1", beat + 4f, offset: 0.01f),
                        new MultiSound.Sound("coinToss/cowbell2", beat + 5f, offset: 0.01f),
                        new MultiSound.Sound("coinToss/cowbell1", beat + 6f, offset: 0.01f),
                        });
                    break;
                default:
                    break;
            }

            this.audienceReacting = audienceReacting;

            coin = ScheduleInput(beat, 6f, InputAction_BasicPress, CatchSuccess, CatchMiss, CatchEmpty);
            //coin.perfectOnly = true;
        }

        public void CatchSuccess(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("coinToss/catch");
            if (this.audienceReacting) SoundByte.PlayOneShot("applause");
            handAnimator.Play("Catch_success", 0, 0);

            isThrowing = false;
        }

        public void CatchMiss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("coinToss/miss");
            if (this.audienceReacting) SoundByte.PlayOneShot("audience/disappointed");
            handAnimator.Play("Pickup", 0, 0);

            isThrowing = false;
        }

        public void CatchEmpty(PlayerActionEvent caller)
        {
            handAnimator.Play("Catch_empty", 0, 0);
            isThrowing = false;

            coin.CanHit(false);
        }

        public void SetHand(int hand)
        {
            if (coin != null) return;
            coinOnMan = (hand == 1);

            if (coinOnMan)
            {
                manHolder.DoScaledAnimationAsync("ManShow", 0.5f);
                handHolder.DoScaledAnimationAsync("Offscreen", 0.5f);
            }
            else
            {
                manHolder.DoScaledAnimationAsync("Idle", 0.5f);
                handHolder.DoScaledAnimationAsync("Idle", 0.5f);
            }
        }

        public void ShowImage(double beat, float length, bool instantShow, bool instantHide)
        {
            imageAnim.DoScaledAnimationAsync(instantShow ? "ImageShow" : "ImageFadeIn", 0.5f);
            BeatAction.New(instance, new List<BeatAction.Action>() {
                    new BeatAction.Action(beat + length, delegate
                        {
                            imageAnim.DoScaledAnimationAsync(instantHide ? "Idle" : "ImageFadeOut", 0.5f);
                        })
                });
        }

        private ColorEase bgColorEase = new(defaultBgColor);
        private ColorEase bgFColorEase = new(defaultBgColor);
        private ColorEase imageBgColorEase = new(defaultBgColor);

        // call this in update
        private void BackgroundColorUpdate()
        {
            bg.color = bgColorEase.GetColor();
            fg.color = bgFColorEase.GetColor();
            imageBG.color = imageBgColorEase.GetColor();
        }

        public void BackgroundColor(double beat, float length, Color colorStartSet, Color colorEndSet, Color colorStartSetF, Color colorEndSetF, int ease)
        {
            bgColorEase = new ColorEase(beat, length, colorStartSet, colorEndSet, ease);
            bgFColorEase = new ColorEase(beat, length, colorStartSetF, colorEndSetF, ease);
            imageBgColorEase = new ColorEase(beat, length, colorStartSet, colorEndSet, ease);
        }

        //call this in OnPlay(double beat) and OnGameSwitch(double beat)
        private void PersistColor(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("coinToss", new string[] { "fade background color" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                BackgroundColor(lastEvent.beat, lastEvent.length, lastEvent["colorStart"], lastEvent["colorEnd"], lastEvent["colorStartF"], lastEvent["colorEndF"], lastEvent["ease"]);
            }
        }

    }
}
