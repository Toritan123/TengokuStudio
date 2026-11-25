using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;
using TMPro;

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
    public static class PcoValiantVolleyLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("valiantVolley", "ValiANT Volley \n<color=#eb5454>[WIP]</color>", "4d0808", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        ValiantVolley.instance.Bop(e.length, e["whichBop"], e["singleBop"], e["keepBop"]);
                    },
                    parameters = new List<Param>() {
                        new Param("singleBop", true, "Bop", "Toggle if the worker ants should bop for the duration of this event. Since this event is not stretchable, they will only bop once."),
                        new Param("keepBop", false, "Bop (Auto)", "Toggle if the worker ants should automatically bop until another Bop event is reached."),
                        new Param("whichBop", ValiantVolley.Bops.Bop, "Which Bop", "Set the type of bop."),
                    },
                },
                new GameAction("startInterval", "Start Interval")
                {
                    preFunctionLength = 4f,
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        if (GameManager.instance.currentGame != "valiantVolley") return;
                        ValiantVolley.instance.PreInterval(e.beat, e.length, true, -1);
                    },
                    defaultLength = 2f,
                    resizable = true,
                    priority = 5,
                },
                new GameAction("dirtHit", "Dirt")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        ValiantVolley.instance.AntAction(e.beat, e.length, "dirtHit");
                    },
                    resizable = true,
                    parameters = new List<Param>() {
                        new Param("shouldPrep", true, "Prepare", "Toggle if the worker ants should automatically prepare for this cue.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "prepBeats" })
                        }),
                        new Param("prepBeats", new EntityTypes.Float(0, 4, 1), "Prepare Beats", "Set how many beats before the cue the worker ants should prepare."),
                    },
                    preFunctionLength = 2f,
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        if (GameManager.instance.currentGame != "valiantVolley") return;
                        ValiantVolley.instance.PreInterval(e.beat, e.length, false, 0);
                        if (e["shouldPrep"]) ValiantVolley.queuePrepare = e.beat - e["prepBeats"];
                    },
                    priority = 1,
                },
                new GameAction("fruitHit", "Fruit")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        ValiantVolley.instance.AntAction(e.beat, e.length, "fruitHit");
                    },
                    resizable = true,
                    parameters = new List<Param>() {
                        new Param("shouldPrep", true, "Prepare", "Toggle if the worker ants should automatically prepare for this cue.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "prepBeats" })
                        }),
                        new Param("prepBeats", new EntityTypes.Float(0, 4, 1), "Prepare Beats", "Set how many beats before the cue the worker ants should prepare."),
                    },
                    preFunctionLength = 2f,
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        if (GameManager.instance.currentGame != "valiantVolley") return;
                        ValiantVolley.instance.PreInterval(e.beat, e.length, false, 1);
                        if (e["shouldPrep"]) ValiantVolley.queuePrepare = e.beat - e["prepBeats"];
                    },
                    priority = 1,
                },
            },
            new List<string>() { "pco", "repeat" },
            "pcovolley", "en",
            new List<string>() { },
            chronologicalSortKey: 20260123,
            plusGame: true,
            font: "Slump"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using JetBrains.Annotations;

    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override. <summary>
    /// This class handles the minigame logic.
    using Scripts_ValiantVolley;
    public class ValiantVolley : Minigame
    {
        [Header("Objects")]
        [SerializeField] SpriteRenderer bg;
        [SerializeField] Material mat;
        [SerializeField] public Ant[] ants;
        [SerializeField] VolleyObject volleyObject;

        public static double queuePrepare = double.MaxValue;

        [Header("Variables")]
        public bool hasMissed;
        public int bopStatus = 0;
        public bool multiInputInterval;
        public bool intervalStarted;
        public double multiIntervalStartBeat = double.MaxValue;
        public float multiIntervalLength;
        public List<VolleyObject> spawnedObjects = new();
        public static List<double> intervalInputList0 = new(); // Interval input list for the dirt
        public static List<double> intervalInputList1 = new(); // Interval input list for the fruit
        int bopIterate = 0;
        bool autoAction;
        double intervalStartBeat;
        float beatInterval = 1f;
        public float hitPitch = 1f;

        public static List<double> queuedDirts = new();
        static List<double> queuedFruits = new();
        static List<VolleyObject> activeObjects = new();
        public List<RiqEntity> queuedDirtEntities = new();
        public List<RiqEntity> queuedFruitEntities = new();

        public static ValiantVolley instance;

        public enum Bops
        {
            Bop,
            Happy,
            Angry,
        }

        public enum Actives
        {
            Activate,
            Deactivate,
        }

        const int IAAltDownCat = IAMAXCAT;
        const int IAAltUpCat = IAMAXCAT + 1;

        protected static bool IA_PadAltPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.South, out dt);
        }

        protected static bool IA_BatonAltPress(out double dt)
        {
            return PlayerInput.GetSqueezeDown(out dt);
        }

        public static PlayerInput.InputAction InputAction_AltPress =
            new("PcoValiantVolleyAltPress", new int[] { IAAltDownCat, IAAltDownCat, IAAltDownCat },
            IA_PadAltPress, IA_TouchBasicPress, IA_BatonAltPress);

        void Awake()
        {
            instance = this;
            SetupBopRegion("valiantVolley", "bop", "keepBop");
        }

        private void Start()
        {
            bopStatus = 0;

        }

        void OnDestroy()
        {
            if (queuedDirts.Count > 0) queuedDirts.Clear();
            if (queuedFruits.Count > 0) queuedFruits.Clear();
            if (activeObjects.Count > 0) activeObjects.Clear();
            queuePrepare = double.MaxValue;

            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        public override void OnBeatPulse(double beat)
        {
            if (bopIterate >= 3)
            {
                bopStatus =
                bopIterate = 0;
                autoAction = false;
            }

            if (autoAction) bopIterate++;

            bool keepBop = BeatIsInBopRegion(beat);

            foreach (var bug in ants)
            {
                bug.cantBop = !keepBop;
                bug.RequestBop();
            }
        }

        private void Update()
        {
            if (queuePrepare <= Conductor.instance.songPositionInBeatsAsDouble)
            {
                foreach (var bug in ants) bug.queuePrepare = queuePrepare;
                queuePrepare = double.MaxValue;
            }
        }

        public void AntAction(double beat, float length, string action)
        {
            var queuedList = action switch
            {
                "fruitHit" => queuedFruits,
                _ => queuedDirts,
            };

            queuedList.Add(beat - intervalStartBeat);
        }
        public VolleyObject SpawnObject(double beat, int objectType, double distance, bool beingJuggled = false,
            List<float> juggleLengths = null, double lastJuggle = 0, double lastJuggleLength = 0, List<double> intervalInputList = null) // objectType 0 => Dirt || 1 => Fruit
        {
            VolleyObject newObject = Instantiate(volleyObject, transform);
            newObject.startBeat = beat;
            newObject.isEntering = true;
            newObject.objectType = objectType;
            newObject.distance = distance;
            newObject.beingJuggled = beingJuggled;
            activeObjects.Add(newObject);
            newObject.juggleLengths = juggleLengths;
            newObject.lastJuggle = lastJuggle;
            newObject.lastJuggleLength = lastJuggleLength;
            newObject.intervalInputList = intervalInputList;

            newObject.gameObject.SetActive(true);

            return newObject;
        }

        public void Bop(double length, int whichBop, bool singleBop, bool keepBop)
        {
            foreach (var bug in ants)
            {
                if (singleBop) bug.PlayAnimation(whichBop);
                if (keepBop) bopStatus = whichBop;
            }
        }

        public override void OnPlay(double beat)
        {

            if (queuedDirts.Count > 0) queuedDirts.Clear();
            if (queuedFruits.Count > 0) queuedFruits.Clear();
            if (queuedDirtEntities.Count > 0) queuedDirtEntities.Clear();
            if (queuedFruitEntities.Count > 0) queuedFruitEntities.Clear();
            if (activeObjects.Count > 0) activeObjects.Clear();
            if (spawnedObjects.Count > 0) spawnedObjects.Clear();

            queuePrepare = double.MaxValue;
            multiInputInterval = false;
            multiIntervalLength = 0;
            multiIntervalStartBeat = double.MaxValue;
        }

        public override void OnGameSwitch(double beat)
        {
            DoInactiveInterval(beat);
        }

        private void DoInactiveInterval(double beat)
        {
            RiqEntity intervalEntity = gameManager.Beatmap.Entities.Find(e => e.beat - 4 <= beat && e.beat >= beat && e.datamodel == "valiantVolley/startInterval");
            if (intervalEntity != null)
            {
                RiqEntity e = intervalEntity;
                PreInterval(e.beat, e.length, true, -1);
            }

            List<RiqEntity> objectEntities = gameManager.Beatmap.Entities.FindAll(e => e.beat - 2 <= beat && e.beat + (e.length * 2) >= beat && (e.datamodel is "valiantVolley/dirtHit" or "valiantVolley/fruitHit"));
            foreach (RiqEntity e in objectEntities)
            {
                PreInterval(e.beat, e.length, false, e.datamodel == "valiantVolley/dirtHit" ? 0 : 1);
                if (e["shouldPrep"]) queuePrepare = e.beat - e["prepBeats"];
            }
        }

        public void PreInterval(double beat, float length, bool isMultiInput = false, int objectType = 0)
        {
            if (isMultiInput)
            {
                multiIntervalStartBeat = beat;
                multiIntervalLength = length;
            }

            if (isMultiInput || (beat >= multiIntervalStartBeat && beat < multiIntervalStartBeat + multiIntervalLength)) multiInputInterval = true;

            if (objectType != -1)
            {
                if (!multiInputInterval || beat < multiIntervalStartBeat || beat >= multiIntervalStartBeat + multiIntervalLength)
                {
                    instance.SpawnObject(beat - length, objectType, length);
                }
                else
                {
                    List<RiqEntity> dirtEntities = gameManager.Beatmap.Entities.FindAll(e => e.datamodel == "valiantVolley/dirtHit"
                    && e.beat >= multiIntervalStartBeat && e.beat < multiIntervalStartBeat + multiIntervalLength);
                    List<RiqEntity> fruitEntities = gameManager.Beatmap.Entities.FindAll(e => e.datamodel == "valiantVolley/fruitHit"
                    && e.beat >= multiIntervalStartBeat && e.beat < multiIntervalStartBeat + multiIntervalLength);

                    List<double> intervalInputList = new();

                    List<float> juggleLengths = new();
                    double lastJuggle = 0;
                    double lastJuggleLength = 0;

                    List<int> types = new();

                    foreach (var i in spawnedObjects)
                    {
                        if (i.startBeat + multiIntervalLength < multiIntervalStartBeat) spawnedObjects.Remove(i);
                        types.Add(i.objectType);
                    }

                    if (objectType == 0)
                    {
                        List<double> juggleTimes = new();
                        foreach (var j in dirtEntities)
                        {
                            juggleTimes.Add(conductor.GetSongPosFromBeat(j.beat));

                            if (j.beat + j.length == multiIntervalStartBeat + multiIntervalLength)
                            {
                                lastJuggle = j.beat;
                                lastJuggleLength = j.length;
                            }
                        }
                        for (int i = 0; i < juggleTimes.Count; i++) 
                        {
                            if (i + 1 != juggleTimes.Count) juggleLengths.Add((float)(conductor.GetRestFromRealTime((juggleTimes[i + 1] - juggleTimes[i]))));
                        }
                    }
                    else
                    {
                        List<double> juggleTimes = new();
                        foreach (var j in fruitEntities)
                        {
                            juggleTimes.Add(conductor.GetSongPosFromBeat(j.beat));

                            if (j.beat + j.length == multiIntervalStartBeat + multiIntervalLength)
                            {
                                lastJuggle = j.beat;
                                lastJuggleLength = j.length;
                            }
                        }
                        for (int i = 0; i < juggleTimes.Count; i++)
                        {
                            if (i + 1 != juggleTimes.Count) juggleLengths.Add((float)(conductor.GetRestFromRealTime((juggleTimes[i + 1] - juggleTimes[i]))));
                        }
                    }

                    if (!types.Contains(objectType))
                    {
                        
                        if (objectType == 0)
                        {
                            foreach (var j in dirtEntities)
                            {
                                if (j.beat < multiIntervalStartBeat + multiIntervalLength) intervalInputList.Add(j.beat);
                            }

                            if (intervalInputList.Count > 0) intervalInputList.RemoveAt(0);
                        }
                        if (objectType == 1)
                        {
                            foreach (var j in fruitEntities)
                            {
                                if (j.beat < multiIntervalStartBeat + multiIntervalLength) intervalInputList.Add(j.beat);
                            }
                            if (intervalInputList.Count > 0) intervalInputList.RemoveAt(0);
                        }

                        VolleyObject obj = instance.SpawnObject(beat - multiIntervalLength, objectType, multiIntervalLength, true,
                            juggleLengths, lastJuggle, lastJuggleLength + beat - multiIntervalStartBeat, intervalInputList);
                        spawnedObjects.Add(obj);
                    }
                }
            }
            if (multiInputInterval)
            {
                instance.StartInterval(beat, multiIntervalLength, beat, isMultiInput);
            }
            else
            {
                instance.StartInterval(beat, length, beat, isMultiInput);
            }
        }

        public void StartInterval(double beat, float length, double gameSwitchBeat, bool isMultiInput = false)
        {
            intervalStartBeat = beat;
            beatInterval = length;
            intervalStarted = true;
            foreach (var thisAnt in ants) thisAnt.cantBop = true;
            if (isMultiInput)
            {
                multiInputInterval = true;
                multiIntervalStartBeat = beat;
                multiIntervalLength = length;
            }

            BeatAction.New(this, new List<BeatAction.Action>() {
                new BeatAction.Action(beat + length, delegate {
                    PassTurn(beat + length, length, isMultiInput);
                }),
            });
        }

        public void PassTurn(double beat, double intervalLength, bool wasMultiInput = false)
        {
            intervalStarted = false;
            
            if (wasMultiInput)
            {
                spawnedObjects.Clear();
                multiInputInterval = false;
            }
            else
            {
                
            }

            var queuedInputs = new List<BeatAction.Action>();

            foreach (var volley in queuedDirts)
            {
                
                queuedInputs.Add(new BeatAction.Action(beat - 0.5f + intervalLength * 2, delegate { 
                    foreach (var thisAnt in ants) thisAnt.justHit = false; }));
                
            }
            foreach (var fruit in queuedFruits)
            {
                
                queuedInputs.Add(new BeatAction.Action(beat - 0.5f + intervalLength * 2, delegate {
                    foreach (var thisAnt in ants) thisAnt.justHit = false; }));
                
            }

            queuedDirts.Clear();
            queuedFruits.Clear();
            
            queuedInputs.Sort((s1, s2) => s1.beat.CompareTo(s2.beat));
            BeatAction.New(this, queuedInputs);
        }

        public void VolleyHit(PlayerActionEvent caller, float state, float pitch)
        {
            ants[2].AntAction("dirtHit", pitch);
            if (!ants[0].isPreparing || !ants[0].justHit)
            {
                foreach (var thisAnt in ants) thisAnt.cantBop = false;
            }
            if (state <= -1f || state >= 1f)
            {
                SoundByte.PlayOneShotGame("nearMiss");
                bopStatus = 2;
                ants[2].cantBop = false;
                ants[2].isPreparing = false;
            }
        }

        public void FruitHit(PlayerActionEvent caller, float state, float pitch)
        {
            ants[2].AntAction("fruitHit", pitch);
            if (!ants[0].isPreparing || !ants[0].justHit)
            {
                foreach (var thisAnt in ants) thisAnt.cantBop = false;
            }
            if (state <= -1f || state >= 1f)
            {
                SoundByte.PlayOneShotGame("nearMiss");
                bopStatus = 2;
                
            }
        }

        public void Miss(PlayerActionEvent caller)
        {
            
        }

    }
}