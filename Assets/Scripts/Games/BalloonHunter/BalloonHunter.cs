using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using NaughtyBezierCurves;

using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    /// Minigame loaders handle the setup of your minigame.
    /// Here, you designate the game prefab, define entities, and mark what AssetBundle to load

    /// Names of minigame loaders follow a specific naming convention of `PlatformcodeNameLoader`, where:
    /// `Platformcode` is a three-leter platform code with the minigame's origin
    /// `Name` is a short internal name
    /// `Loader` is the string "Loader"

    /// Platform codes are as follows:
    /// Agb: Gameboy Advance    ("Advance Gameboy")
    /// Ntr: Nintendo DS        ("Nitro")
    /// Rvl: Nintendo Wii       ("Revolution")
    /// Ctr: Nintendo 3DS       ("Centrair")
    /// Mob: Mobile
    /// Pco: PC / Other

    /// Fill in the loader class label, "*prefab name*", and "*Display Name*" with the relevant information
    /// For help, feel free to reach out to us on our discord, in the #development channel.
    public static class PcoBalloonHunterLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("balloonHunter", "Balloon Hunter", "92cbe8", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out BalloonHunter instance)) {
                            instance.Bop(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity["auto"], eventCaller.currentEntity["toggle"], eventCaller.currentEntity["emote"]);
                        }
                    },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Bop", "Toggle if the characters should bop for the duration of this event."),
                        new Param("auto", true, "Bop (Auto)", "Toggle if the characters should automatically bop until another Bop event is reached."),
                        new Param("emote", true, "Reactions", "Toggle if the characters should react to the player's performance."),
                    }
                },
                new GameAction("prepare", "Prepare")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out BalloonHunter instance)) {
                            instance.Prepare();
                        }
                    },
                    defaultLength = 0.5f,
                },
                new GameAction("balloonSlow", "Normal Balloon")
                {
                    function = delegate {
                        var offset = SoundByte.GetClipLengthGame("balloonHunter/tweetN_base") - 0.05837;
                        SoundByte.PlayOneShotGame("balloonHunter/tweetN_base", eventCaller.currentEntity.beat, offset: offset, forcePlay: true);
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out BalloonHunter instance)) {
                            instance.QueueBalloonSlow(eventCaller.currentEntity.beat);
                        }
                    },
                    defaultLength = 3,
                },
                new GameAction("balloonFast", "Fast Balloon")
                {
                    function = delegate {
                        SoundByte.PlayOneShotGame("balloonHunter/tweetN_fast1", eventCaller.currentEntity.beat, forcePlay: true);
                        SoundByte.PlayOneShotGame("balloonHunter/tweetN_fast2", eventCaller.currentEntity.beat+0.75, forcePlay: true);
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out BalloonHunter instance)) {
                            instance.SendBalloonFast(eventCaller.currentEntity.beat);
                        }
                    },
                    defaultLength = 2.5f,
                },
                new GameAction("balloonBoth", "Double Balloons")
                {
                    function = delegate {
                        SoundByte.PlayOneShotGame("balloonHunter/tweetN_both", eventCaller.currentEntity.beat, forcePlay: true);
                        SoundByte.PlayOneShotGame("balloonHunter/tweetN_both", eventCaller.currentEntity.beat+0.75, forcePlay: true);
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out BalloonHunter instance)) {
                            instance.SendBalloonBoth(eventCaller.currentEntity.beat);
                        }
                    },
                    defaultLength = 3,
                },
                new GameAction("balloonFive", "Slow Balloon")
                {
                    function = delegate {
                        var offset = SoundByte.GetClipLengthGame("balloonHunter/charge");
                        SoundByte.PlayOneShotGame("balloonHunter/tweetN_slow", eventCaller.currentEntity.beat, forcePlay: true);
                        SoundByte.PlayOneShotGame("balloonHunter/tweetN_slow", eventCaller.currentEntity.beat+1, forcePlay: true);
                        SoundByte.PlayOneShotGame("balloonHunter/charge", eventCaller.currentEntity.beat+4, offset: offset, forcePlay: true);
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out BalloonHunter instance)) {
                            instance.SendBalloonFive(eventCaller.currentEntity.beat, eventCaller.currentEntity["moose"]);
                        }
                    },
                    defaultLength = 5,
                    parameters = new List<Param>()
                    {
                        new Param("moose", false, "Moose", "Toggle if a moose should be tied to the balloon.")
                    }
                },
                new GameAction("bgAnimal", "Spawn Background Animal")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out BalloonHunter instance)) {
                            var e = eventCaller.currentEntity;
                            instance.SpawnBgAnimal(e.beat, e.length, e["type"], e["direction"], e["startY"], e["endY"]);
                        }
                    },
                    defaultLength = 16,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("type", BalloonHunter.AnimalType.Rabbit, "Animal Type", "Choose which type of animal the Background Animal should be."),
                        new Param("direction", BalloonHunter.BGAnimalDirection.Left, "Direction", "Choose the direction the Background Animal should fly."),
                        new Param("custom", false, "Custom Movement", "Toggle if the Background Animal's movement should be customizable.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "startY" }),
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "endY" }),
                        }),
                        new Param("startY", new EntityTypes.Float(-7f, 7f, 0.1f), "Start Y", "Set the Y Position of the Background Animal at the start."),
                        new Param("endY", new EntityTypes.Float(-7f, 7f, 0.1f), "End Y", "Set the Y Position of the Background Animal at the end."),
                    }
                },
            },
            new List<string>() { "pco", "normal" },
            "pcoballoon", "en",
            new List<string>() { },
            chronologicalSortKey: 20241122,
            plusGame: true,
            font: "KanTeiRyu"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_BalloonHunter;
    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
    public class BalloonHunter : Minigame
    {
        public enum AnimalType
        {
            Rabbit,
            Boar,
            Moose,
        }
        public enum BGAnimalDirection
        {
            Left,
            Right,
        }

        //public static BalloonHunter instance;
        public string bopExpression = "Neutral";

        [Header("Objects")]
        [SerializeField] Balloon slowBalloon;
        [SerializeField] Balloon fastBalloon;
        [SerializeField] Balloon balloonFive;
        [SerializeField] BGAnimal bgAnimal;
        [SerializeField] GameObject rock;
        [SerializeField] BezierCurve3D rockMissCurve;

        [Header("Animators")]
        [SerializeField] Animator hunterAnim;
        [SerializeField] Animator birdAnim;
        [SerializeField] Animator rockSmear;

        private bool hunterBop = true;
        private bool birdBop = true;
        public bool preparing;
        public bool tossPreparing;
        private bool queueBopReset;

        private bool rockActive;
        private double rockStart = double.MinValue;
        private Vector3 rockNormalPosition;

        private List<BGAnimal> bgAnimals = new List<BGAnimal>();

        private void Awake()
        {
            //instance = this;
            slowBalloon.gameObject.SetActive(false);
            fastBalloon.gameObject.SetActive(false);
            balloonFive.gameObject.SetActive(false);

            rock.SetActive(false);
            rockNormalPosition = rock.transform.position;
        }


        public override void OnLateBeatPulse(double beat)
        {
            if (hunterBop && (hunterAnim.IsPlayingAnimationNames("Idle") || hunterAnim.IsAnimationNotPlaying()) && !preparing)
            {
                hunterAnim.DoScaledAnimationAsync("Bop", 0.5f, animLayer: 0);
                hunterAnim.DoScaledAnimationAsync(bopExpression, 0.5f, animLayer: 1);
                if (bopExpression != "Neutral") ResetBopExpression(beat);

            }
            if (birdBop && (birdAnim.IsPlayingAnimationNames("Idle") || birdAnim.IsAnimationNotPlaying()))
            {
                birdAnim.DoScaledAnimationAsync("Bop", 0.5f, animLayer: 0);
                birdAnim.DoScaledAnimationAsync(bopExpression, 0.5f, animLayer: 1);
            }
            foreach (var animal in bgAnimals)
            {
                animal.DoBop();
            }
        }

        public void Update()
        {
            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            {
                if (tossPreparing) Toss(true);
                else
                {
                    hunterAnim.DoScaledAnimationAsync("Miss", 0.5f, animLayer: 0);
                    hunterAnim.DoScaledAnimationAsync("Blow", 0.5f, animLayer: 1);
                    SoundByte.PlayOneShotGame("balloonHunter/blow");
                }
                preparing = false;
            }
            if (rockActive)
            {
                float flyPos = conductor.GetPositionFromBeat(rockStart, 0.5f);
                rock.transform.position = rockMissCurve.GetPoint(flyPos);
                if (flyPos > 1f)
                {
                    rockActive = false;
                    rock.transform.position = rockNormalPosition;
                    rock.SetActive(false);
                }
            }
        }

        public void Prepare()
        {
            if (preparing) return;
            hunterAnim.DoScaledAnimationAsync("Prepare", 0.5f, animLayer: 0);
            hunterAnim.DoScaledAnimationAsync("Hold", 0.5f, animLayer: 1);
            preparing = true;
            //hunterHold = true;
            //hunterBop = false;
        }

        public void Unprepare()
        {
            if (preparing)
            {
                hunterAnim.DoScaledAnimationAsync("Bop", 0.5f, animLayer: 0);
                preparing = false;
            }
        }

        public void TossPrepare()
        {
            hunterAnim.DoScaledAnimationAsync("HunterTossPrep", 0.5f, animLayer: 0);
            hunterAnim.DoScaledAnimationAsync("Prep Toss", 0.5f, animLayer: 1);
            preparing = true;
            tossPreparing = true;
        }

        public void Toss(bool miss = false)
        {
            hunterAnim.DoScaledAnimationAsync("HunterToss", 0.5f, animLayer: 0);
            hunterAnim.DoScaledAnimationAsync(miss ? "Shock" : "Determined", 0.5f, animLayer: 1);
            bopExpression = miss ? "Shock" : "Determined";
            preparing = false;
            tossPreparing = false;

            if (!miss) rockSmear.DoScaledAnimationAsync("RockSmear", 0.5f);
        }

        public void BirdCall()
        {
            // if (birdBop) birdBop = false;
            birdAnim.DoScaledAnimationAsync("Bop", 0.5f, animLayer: 0);
            birdAnim.DoScaledAnimationAsync("Call", 0.5f, animLayer: 1);
        }

        public void BirdFlap()
        {
            birdAnim.DoScaledAnimationAsync("Flap", 0.5f, animLayer: 0);
            birdAnim.DoScaledAnimationAsync("CallBig", 0.5f, animLayer: 1);
        }

        public void BirdScared()
        {
            birdAnim.DoScaledAnimationAsync("Scared", 0.5f, animLayer: 0);
            birdAnim.DoScaledAnimationAsync("CallScared", 0.5f, animLayer: 1);
        }

        public void BirdCover()
        {
            birdAnim.DoScaledAnimationAsync("Cover", 0.5f, animLayer: 0);
        }

        public void ResetBopExpression(double beat)
        {
            BeatAction.New(this, new()
            {
                new(beat + 1, delegate {
                    bopExpression = "Neutral";
                    queueBopReset = false;
                }),
            });
        }

        public void Bop(double beat, float length, bool auto, bool bop, bool emote)
        {
            hunterBop = auto;
            birdBop = auto;
            preparing = false;
            tossPreparing = false;
            if (bop)
            {
                List<BeatAction.Action> actions = new();
                for (int i = 0; i < length; i++)
                {
                    actions.Add(new(beat + i, delegate 
                    {
                        if ((hunterAnim.IsPlayingAnimationNames("Idle") || hunterAnim.IsAnimationNotPlaying()) && !preparing)
                        {
                            hunterAnim.DoScaledAnimationAsync("Bop", 0.5f, animLayer: 0);
                            hunterAnim.DoScaledAnimationAsync(emote ? bopExpression : "Neutral", 0.5f, animLayer: 1);
                        }
                        if (birdAnim.IsPlayingAnimationNames("Idle") || birdAnim.IsAnimationNotPlaying()) 
                        {
                            birdAnim.DoScaledAnimationAsync("Bop", 0.5f, animLayer: 0);
                            birdAnim.DoScaledAnimationAsync(emote ? bopExpression : "Neutral", 0.5f, animLayer: 1);
                        }
                    }));
                }
                if (actions.Count > 0) BeatAction.New(this, actions);
            }
            queueBopReset = true;
            if (bopExpression != "Neutral") ResetBopExpression(beat);
        }

        public void QueueBalloonSlow(double beat)
        {
            SendBalloonSlow(beat);
            //Unprepare();

            BeatAction.New(this, new List<BeatAction.Action>(){
                new BeatAction.Action(beat, delegate {BirdCall();}),
                new BeatAction.Action(beat+1, delegate {Prepare();}),
            });
        }

        public void SendBalloonSlow(double beat)
        {
            Balloon newSlowB = Instantiate(slowBalloon, transform);
            newSlowB.balloonSpeed = 3f;
            newSlowB.startBeat = beat;
            newSlowB.gameObject.SetActive(true);
        }

        public void SendBalloonFast(double beat)
        {
            Balloon newFastB = Instantiate(fastBalloon, transform);
            newFastB.balloonSpeed = 2.5f;
            newFastB.startBeat = beat;
            newFastB.gameObject.SetActive(true);

            //Unprepare();

            BeatAction.New(this, new List<BeatAction.Action>(){
                new BeatAction.Action(beat, delegate {BirdFlap();}),
                new BeatAction.Action(beat + 0.75, delegate {BirdFlap();}),
                new BeatAction.Action(beat + 0.75, delegate {Prepare();}),
            });
        }

        public void SendBalloonBoth(double beat)
        {
            SendBalloonSlow(beat);
            SendBalloonFast(beat);
        }

        public void SendBalloonFive(double beat, bool moose)
        {
            Balloon newFive = Instantiate(balloonFive, transform);
            newFive.balloonSpeed = 4f;
            newFive.startBeat = beat + 2;
            newFive.isFive = true;
            newFive.moose = moose;
            newFive.gameObject.SetActive(true);

            BeatAction.New(this, new List<BeatAction.Action>(){
                new BeatAction.Action(beat, delegate {BirdScared();}),
                new BeatAction.Action(beat+1, delegate {BirdScared();}),
                new BeatAction.Action(beat+2, delegate {TossPrepare();}),
                //new BeatAction.Action(beat+2, delegate {Unprepare();}),
                new BeatAction.Action(beat+2, delegate {BirdCover();}),
                //new BeatAction.Action(beat+4, delegate {Toss();})
            });
        }

        public void SpawnBgAnimal(double beat, float length, int type, int direction, float startY, float endY)
        {
            BGAnimal newAnimal = Instantiate(bgAnimal, transform);
            newAnimal.startBeat = beat; ;
            newAnimal.flyLength = length;
            newAnimal.right = direction == 1;
            newAnimal.startY = startY;
            newAnimal.endY = endY;
            newAnimal.type = type;
            newAnimal.gameObject.SetActive(true);

            bgAnimals.Add(newAnimal);
        }

        public void DoRockBarely(double beat)
        {
            rockActive = true;
            rockStart = beat;

            rock.SetActive(true);
        }

        public void SpawnFallingMoose(double beat)
        {

        }

    }
}