using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;


using HeavenStudio.Util;
using HeavenStudio.InputSystem;

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
    public static class AgbShowtimeLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("showtime", "Showtime \n<color=#adadad>(Shōtaimu)</color>", "00DAE3", false, false, new List<GameAction>()
            {

                new GameAction("bop", "Bop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        Showtime.instance.ToggleBop(e.beat, e.length, e["toggle"], e["autobop"]); 
                    },
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Bop", "Toggle if the monkey should bop for the duration of this event."),
                        new Param("autobop", false, "Bop (Auto)", "Toggle if the monkey should automatically bop until another Bop event is reached."),
                    },
                    resizable = true
                },
                
                new GameAction("penguinGray", "Gray Penguin")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        Showtime.RequestGrayPenguin(e.beat, 2);
                    },
                    //parameters = new List<Param>()
                    //{
                    //    new Param("chute", Showtime.ChuteStatus.Random, "Parachute?", "Set if a successful catch deploys a parachute."),
                    //},
                    preFunctionLength = 1f,
                    defaultLength = 4f,
                    resizable = false
                },

                new GameAction("penguinWhite", "White Penguin")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        Showtime.RequestWhitePenguin(e.beat, 2);
                    },
                    //parameters = new List<Param>()
                    //{
                    //    new Param("chute", Showtime.ChuteStatus.Random, "Parachute?", "Set if a successful catch deploys a parachute."),
                    //},
                    defaultLength = 4f,
                    resizable = false
                },

                new GameAction("penguinBig", "Big Penguin")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        Showtime.RequestBigPenguin(e.beat, 2);
                    },
                    //parameters = new List<Param>()
                    //{
                        //new Param("chute", Showtime.ChuteStatus.Random, "Parachute?", "Set if a successful catch deploys a parachute."),
                    //},
                    defaultLength = 6f,
                    resizable = false,
                },

                new GameAction("background", "Background Colors")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        Showtime.instance.BackgroundColor(e.beat, e.length, e["startA"], e["endA"], e["startB"], e["endB"], e["startC"], e["endC"], e["ease"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("startB", Showtime.defaultB, "Color 1 Start", "Sets the first color at the start of the event."),
                        new Param("endB", Showtime.defaultB, "Color 1 End", "Sets the first color at the end of the event."),
                        new Param("startA", Showtime.defaultA, "Color 2 Start", "Sets the second color at the start of the event."),
                        new Param("endA", Showtime.defaultA, "Color 2 End", "Sets the second color at the end of the event."),
                        new Param("startC", Showtime.defaultC, "Color 3 Start", "Sets the third color at the start of the event."),
                        new Param("endC", Showtime.defaultC, "Color 3 End", "Sets the third color at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    },
                    defaultLength = 4f,
                    resizable = true
                }
            },
            new List<string>() {"agb", "normal"},
            "agbshowtime", "en",
            new List<string>() {},
            chronologicalSortKey: 14
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_Showtime;
    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
    public class Showtime : Minigame
    {
        [Header("Objects")]
        [SerializeField] PenguinGray penguinGray;
        [SerializeField] PenguinWhite penguinWhite;
        [SerializeField] PenguinBig penguinBig;

        [SerializeField] Ball ball;

        [Header("Animators")]

        public Animator MonkeyAnim;
        public Animator ButtonAnim;
        public Animator LauncherAnim;

        public Animator blockOneAnim;
        public Animator blockTwoAnim;

        [SerializeField] GameObject penguinStart;

        public BezierCurve3D entryCurve;
        public BezierCurve3D hopCurve;
        public BezierCurve3D leapCurve;
        public BezierCurve3D fallCurve;
        public BezierCurve3D exitCurve;
        public BezierCurve3D chuteCurve;

        public Transform ballStart;
        public Transform leapStart;
        public Transform fallStart;
        public Transform destroyerPoint;
        public Transform slideStart;
        //public BezierCurve3D failureCurve;

        public BezierCurve3D ballUpCurve;
        public BezierCurve3D ballDownCurve;

        public static Color defaultA =  new Color(0.2666f, 1f, 1f);
        public static Color defaultB = new Color(0f, 0.8588235294117647f, 0.8941176470588236f);
        public static Color defaultC = new Color(0f, 0.6745098039215687f, 0.666f);

        [SerializeField] private Material bgMaterial;
        private ColorEase[] colorEases = new ColorEase[3];
        

        public static Showtime instance;

        public static List<QueuedPenguin> queuedPenguins = new List<QueuedPenguin>();

        public enum ChuteStatus : int
        {
            Random,
            Chute,
            NoChute
        }

        public enum PenguinType
        {
            PenguinGray,
            PenguinWhite,
            PenguinBig
        }

        public struct QueuedPenguin
        {
            public double beat;
            public PenguinType type;
            public int chute;
        }

         private void Awake()
        {
            colorEases = new ColorEase[] {
                new(defaultA),
                new(defaultB),
                new(defaultC)
            };
            instance = this;
            SetupBopRegion("showtime", "bop", "auto");
        }

        public override void OnBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat))
            {
                Bop(beat);
            }

        }

        public override void OnPlay(double beat)
        {
            queuedPenguins.Clear();
            OnGameSwitch(beat);
        }

        private void OnDestroy() 
        {
            queuedPenguins.Clear();
        }

        public void BackgroundColor(double beat, float length, Color startA, Color endA, Color startB, Color endB, Color startC, Color endC, int ease)
        {
            colorEases = new ColorEase[] {
                new(beat, length, startA, endA, ease),
                new(beat, length, startB, endB, ease),
                new(beat, length, startC, endC, ease)
            };
        }

        private void ColorUpdate()
        {
            bgMaterial.SetColor("_ColorBravo", colorEases[0].GetColor());
            bgMaterial.SetColor("_ColorAlpha", colorEases[1].GetColor());
            bgMaterial.SetColor("_ColorDelta", colorEases[2].GetColor());
        }

        private void PersistColor(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("showtime", new string[] { "background" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                BackgroundColor(lastEvent.beat, lastEvent.length, lastEvent["startA"], lastEvent["endA"], lastEvent["startB"], lastEvent["endB"], lastEvent["startC"], lastEvent["endC"], lastEvent["ease"]);
            }
        }

        public override void OnGameSwitch(double beat)
        {
            PersistColor(beat);
        }


        private void Update() 
        {
            var cond = Conductor.instance;
            var currentBeat = cond.unswungSongPositionInBeatsAsDouble;
            ColorUpdate();
            if (cond.isPlaying && !cond.isPaused)
            {
                if (queuedPenguins.Count != 0)
                {
                    foreach (QueuedPenguin p in queuedPenguins)
                    {
                        switch (p.type)
                        {
                            case PenguinType.PenguinGray:
                                SpawnGrayPenguin(p.beat, p.chute);
                                break;
                            case PenguinType.PenguinWhite:
                                SpawnWhitePenguin(p.beat, p.chute);
                                break;
                            case PenguinType.PenguinBig:
                                SpawnBigPenguin(p.beat, p.chute);
                                break;
                            
                        }
                    }
                    queuedPenguins.Clear();
                }
            }
            else
            {
                if ((!conductor.isPaused) && queuedPenguins.Count != 0)
                {
                    queuedPenguins.Clear();
                }
            }
            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress)) {
                if (!MonkeyAnim.IsPlayingAnimationNames("Hit"))
                    {
                        MonkeyAnim.DoScaledAnimationAsync("Hit", 0.5f);
                        SoundByte.PlayOneShotGame("showtime/hit", currentBeat);
                        SpawnBall(currentBeat, true);
                        
                    }
            }
        }

        public static void RequestGrayPenguin(double beat, int chute)
        {
            if (GameManager.instance.currentGame != "showtime")
            {
                queuedPenguins.Add(new QueuedPenguin()
                {
                    beat = beat,
                    type = PenguinType.PenguinGray
                });
            }
            else
            {
                instance.SpawnGrayPenguin(beat, chute);
            }
        }

        public void SpawnGrayPenguin(double beat, int chute)
        {

            SoundByte.PlayOneShotGame("sneakySpirits/moving", beat, pitch: 2.1f, forcePlay: true);
            SoundByte.PlayOneShotGame("sneakySpirits/moving", beat + 1f, pitch: 1.4f, forcePlay: true);
            

            PenguinGray newPenguin = Instantiate(penguinGray, penguinStart.transform);
            newPenguin.startBeat = beat - 1;
            switch (chute)
                {
                    case 0:
                        if (UnityEngine.Random.Range(0, 1f) >= 0.5f)
                        {newPenguin.chute = true;}
                        break;
                    case 1:
                         newPenguin.chute = true;
                        break;
                    case 2:
                        newPenguin.chute = false;
                         break;
                }

            newPenguin.gameObject.SetActive(true);
            
        }

        public static void RequestWhitePenguin(double beat, int chute)
        {
            if (GameManager.instance.currentGame != "showtime")
            {
                queuedPenguins.Add(new QueuedPenguin()
                {
                    beat = beat,
                    type = PenguinType.PenguinWhite
                });
            }
            else
            {
                instance.SpawnWhitePenguin(beat, chute);
            }
        }

        public void SpawnWhitePenguin(double beat, int chute)
        {
            SoundByte.PlayOneShotGame("sneakySpirits/moving", beat, pitch: 3.1f, forcePlay: true);
            SoundByte.PlayOneShotGame("sneakySpirits/moving", beat + 0.5f, pitch: 2f, forcePlay: true);
            SoundByte.PlayOneShotGame("sneakySpirits/moving", beat + 1f, pitch: 3.1f, forcePlay: true);
            SoundByte.PlayOneShotGame("sneakySpirits/moving", beat + 1.5f, pitch: 2f, forcePlay: true);

            PenguinWhite newPenguin = Instantiate(penguinWhite, penguinStart.transform);
            newPenguin.startBeat = beat-0.5;
            switch (chute)
                {
                    case 0:
                        if (UnityEngine.Random.Range(0, 1f) >= 0.5f)
                        {newPenguin.chute = true;}
                        break;
                    case 1:
                         newPenguin.chute = true;
                        break;
                    case 2:
                        newPenguin.chute = false;
                         break;
                }

            newPenguin.gameObject.SetActive(true);

        }

        public static void RequestBigPenguin(double beat, int chute)
        {
            if (GameManager.instance.currentGame != "showtime")
            {
                queuedPenguins.Add(new QueuedPenguin()
                {
                    beat = beat,
                    type = PenguinType.PenguinBig
                });
            }
            else
            {
                instance.SpawnBigPenguin(beat, chute);
            }
        }

        public void SpawnBigPenguin(double beat, int chute)
        {
            SoundByte.PlayOneShotGame("sneakySpirits/moving", beat, pitch: 1.5f, forcePlay: true);
            SoundByte.PlayOneShotGame("sneakySpirits/moving", beat + 2f, pitch: 1.2f, forcePlay: true);
            

            PenguinBig newPenguin = Instantiate(penguinBig, penguinStart.transform);
            newPenguin.startBeat = beat - 1;
            switch (chute)
                {
                    case 0:
                        if (UnityEngine.Random.Range(0, 1f) >= 0.5f)
                        {newPenguin.chute = true;}
                        break;
                    case 1:
                         newPenguin.chute = true;
                        break;
                    case 2:
                        newPenguin.chute = false;
                         break;
                }

            newPenguin.gameObject.SetActive(true);

        }

        public void SpawnBall(double beat, bool miss)
        {
            ButtonAnim.DoScaledAnimationAsync("Press",0.5f);
            LauncherAnim.DoScaledAnimationAsync("Launch", 0.5f);
            Ball newBall = Instantiate(ball, ballStart);
            newBall.isMiss = miss;
            newBall.startBeat = beat;
            newBall.gameObject.SetActive(true);
        }


        public void ToggleBop(double beat, float length, bool bopOrNah, bool autoBop)
        {
            if (bopOrNah)
            {
                for (int i = 0; i < length; i++)
                {
                    var currentBeat = beat + i;
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(currentBeat, delegate { Bop(currentBeat);})
                    });
                }
            }
        }

        public void Bop(double beat)
        {
            if (!MonkeyAnim.IsPlayingAnimationNames("Bop", "Hit"))
            {
                MonkeyAnim.DoScaledAnimationAsync("Bop", 0.5f);
            }
        }

        

    }
}

