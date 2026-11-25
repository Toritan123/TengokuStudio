using System.Collections.Generic;
using UnityEngine;

using Jukebox;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System.Linq;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrLabLoader
{
    public static Minigame AddGame(EventCaller e)
    {
        RiqBeatmapData? ChartSystemUpdater(string riqVersion, RiqBeatmapData data)
        {
            List<RiqEntity> intervals = data.entities.FindAll(e => e.datamodel == "loveLab/beat intervals" && e.version == 0);
            foreach (RiqEntity interval in intervals)
            {
                double flaskBeat = double.MinValue;
                double flaskEndBeat = double.MinValue;
                float addDelay = 1f;
                int track = (int)interval["track"];
                List<double> shakeStarts = new();
                List<double> shakeEnds = new();

                List<RiqEntity> events = data.entities.FindAll(e => e.beat >= interval.beat && e.beat < interval.beat + interval.length && e.datamodel == "loveLab/boy shakes").OrderBy(e => e.beat).ToList();
                for (int i = 0; i < events.Count; i++)
                {
                    if (i == 0)
                    {
                        flaskBeat = events[i].beat;
                        track = (int)events[i]["track"];
                    }
                    else
                    {
                        shakeEnds.Add(events[i].beat);
                    }

                    if (i == events.Count - 1)
                    {
                        flaskEndBeat = events[i].beat + events[i].length;
                        switch ((int)events[i]["speed"])
                        {
                            case 1:
                                addDelay = 2f;
                                break;
                            case 2:
                                addDelay = 1.5f;
                                break;
                            default:
                                break;
                        }
                        /* the old events were done in this order with these delays:
                        fastFlask     | 1 beat delay
                        slowFlask     | 2 beat delay
                        midSlowFlask  | 1.5 beat delay
                        */
                    }
                    else
                    {
                        shakeStarts.Add(events[i].beat + events[i].length);
                    }

                    data.entities.Remove(events[i]);
                }

                shakeEnds.Add(interval.beat + interval.length); // just in case. im not sure if we really need this...
                interval.length += addDelay;
                interval.version = 1;

                if (flaskBeat != double.MinValue && flaskEndBeat != double.MinValue)
                {
                    RiqEntity e = new RiqEntity("riq__Entity", 0, "loveLab/flask", flaskBeat, (float)(flaskEndBeat - flaskBeat));
                    e.CreateProperty("track", track);
                    data.entities.Add(e);
                }
                for (int i = 0; i < shakeStarts.Count; i++)
                {
                    RiqEntity e = new RiqEntity("riq__Entity", 0, "loveLab/shake", shakeStarts[i], (float)(shakeEnds[i] - shakeStarts[i]));
                    e.CreateProperty("track", track + 1);
                    data.entities.Add(e);
                }
            }
            return data;
        }
        RiqBeatmap.OnUpdateBeatmap += ChartSystemUpdater;

        return new Minigame("loveLab", "Love Lab", "f2a900", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { LoveLab.instance.Bop(e.currentEntity.beat, e.currentEntity.length, 
                        e.currentEntity["toggle"], e.currentEntity["toggle2"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Bop", "Whether both will bop to the beat or not"),
                        new Param("toggle2", false, "Bop (Auto)", "Whether both will bop automatically to the beat or not")
                    }
                },

                new GameAction("beat intervals", "Start Interval")
                {
                    preFunction = delegate { LoveLab.instance.PreInterval(e.currentEntity.beat, e.currentEntity.length, true);   },
                    //parameters = new List<Param>()
                    //{
                    //   new Param("auto", true, "Auto Pass Turn", "Toggle if the turn should be passed automatically at the end of the start interval.")
                    //},
                    preFunctionLength = 1f,
                    defaultLength = 4f,
                    resizable = true,
                    priority = 2,
                    defaultVersion = 1,
                },
                new GameAction("flask", "Catch/Throw Flask")
                {
                    defaultLength = 3f,
                    resizable = true,
                    priority = 1
                },
                new GameAction("shake", "Shake Flask")
                {
                    defaultLength = 0.25f,
                    resizable = true,
                    priority = 1
                },

                new GameAction("girl blush", "Girl Blush")
                {
                    function = delegate { LoveLab.instance.girlBlush(e.currentEntity["toggle"]); },
                    defaultLength = .5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Auto", "Blush automatically?")
                    }

                },

                new GameAction("boxGuy", "Box Guy")
                {
                    function = delegate { LoveLab.instance.mainBoxGuy(e.currentEntity.beat, e.currentEntity["type"]); },
                    defaultLength = 2f,
                    resizable = false,
                    parameters = new List<Param>()
                    {
                        new Param("type", LoveLab.boxGuyAction.takeAway, "Action", "Box Guy's Actions")
                    }
                },

                new GameAction("set object colors", "Object Colors")
                {
                    function = delegate {
                        LoveLab.instance.UpdateMaterialColor(e.currentEntity["colorA"], e.currentEntity["colorB"], e.currentEntity["colorC"]);
                    },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("colorA", new Color(0.02909997f, 0.4054601f, 0.97f), "1st Flask Color", "1st Flask Color to show"),
                        new Param("colorB", new Color(0.81f, 0.2984211f, 0f), "2nd Flask Color", "2nd Flask Color to show"),
                        new Param("colorC", new Color(0.8313726f, 0.2039216f, 0.5058824f), "3rd Flask Color", "3rd Flask Color to show")
                    },
                    priority = 2
                },

                new GameAction("set time of day", "Time of Day")
                {
                    function = delegate
                    {
                        LoveLab.instance.setTimeOfDay(e.currentEntity["type"]);
                    },
                    defaultLength = .5f,
                    parameters = new List<Param>()
                    {
                        new Param("type", LoveLab.timeOfDay.Sunset, "Time", "Set the time of day.")
                    }
                },

                new GameAction("clouds", "Toggle Clouds")
                {
                    function = delegate { LoveLab.instance.cloudMove(e.currentEntity["toggle"]); },
                    defaultLength = .5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Move", "Move Clouds?")
                    }
                },

                new GameAction("spotlight", "Toggle Spotlight")
                {
                    function = delegate { LoveLab.instance.spotlight(e.currentEntity["toggle"], e.currentEntity["spotType"], e.currentEntity["posType"]); },
                    defaultLength = .5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Spotlight", "Activate Spotlight?"),
                        new Param("spotType", LoveLab.spotlightType.normal, "Spotlight Type", "Normal or Coned?"),
                        new Param("posType", LoveLab.spotlightPos.boy, "Spotlight Position", "(For cone) under the Boy or the Girl?")
                    },
                    priority = 1
                }
            },
        new List<string>() { "ntr", "repeat" },
        "ntrlab", "en",
        new List<string>() { },
        chronologicalSortKey: 19,
        font : "FOT-SlumpStd-DB SDF"
        );
    }
}
}

namespace HeavenStudio.Games
{
    using Jukebox;
    using Scripts_LoveLab;

    public class LoveLab : Minigame
    {
        public static LoveLab instance { get; set; }

        [Header("Lab Guy")]
        [SerializeField] Animator labGuy;
        [SerializeField] Animator labGuyHead;
        [SerializeField] Animator labGuyArm;

        [Header("Lab Girl")]
        [SerializeField]  Animator labGirl;
        [SerializeField] Animator labGirlHead;
        [SerializeField] Animator labGirlArm;

        [Header("Lab Weird")]
        [SerializeField] Animator labAssistant;
        [SerializeField] Animator labAssistantHead;
        [SerializeField] Animator labAssistantArm;

        [Header("Flask")]
        public GameObject flask;
        [SerializeField] GameObject labFlaskObj;
        public GameObject labGirlFlaskObj;
        [SerializeField] ParticleSystem boyFlaskBreak;
        [SerializeField] ParticleSystem girlFlaskBreak;
        [SerializeField] Material flaskMatForBoy;
        [SerializeField] Material flaskMatForGirl;
        [SerializeField] Material flaskMatForWeird;
        [SerializeField] Color boyLiquidColor = "0868F8".Hex2RGB();
        [SerializeField] Color girlLiquidColor = "F86008".Hex2RGB();
        [SerializeField] Color weirdLiquidColor = "D43481".Hex2RGB();
        [SerializeField] SpriteRenderer flaskSpriteRend;
        [SerializeField] SpriteRenderer girlFlaskSpriteRend;
        [SerializeField] SpriteRenderer weirdFlaskSpriteRend;
        [SerializeField] List<string> flaskArcToBoy = new List<string>();
        [SerializeField] List<string> flaskArcToGirl = new List<string>();    

        [Header("Misc")]
        [SerializeField] Animator heartBox;
        [SerializeField] GameObject spotlightShader;
        [SerializeField] GameObject spotlightShaderCone;
        [SerializeField] GameObject spotlightCone;
        [SerializeField] GameObject labHeartBox;
        [SerializeField] Animator boxPerson;
        [SerializeField] Animator boxPersonDay;
        [SerializeField] GameObject heartContainer;
        [SerializeField] GameObject labGuyFlask;
        [SerializeField] GameObject labGirlFlask;
        public static List<QueuedFlask> queuedFlask = new List<QueuedFlask>();
        public List<LoveLabFlask> girlInstantiatedFlask = new List<LoveLabFlask>(); //boy to girl
        public List<LoveLabFlask> weirdInstantiatedFlask = new List<LoveLabFlask>(); //girl to weird

        [Header("Variables")]
        public bool hasMissed = false;
        bool canBop = false;
        public bool bopRight = false;
        timeOfDay currentTime = timeOfDay.Sunset;
        bool hasStartedInterval;
        [SerializeField] bool isHolding;
        [SerializeField] bool isHoldingFlask;
        public int counter = 0;
        bool canCloudsMove = true;
        bool hasShakenUp = false;
        public PlayerActionEvent release;

        [Header("Clouds")]
        [SerializeField] GameObject clouds;
        [SerializeField] float cloudSpeed;
        [SerializeField] float cloudDistance;

        [Header("Time Of Day")]
        [SerializeField] GameObject sunsetBG;
        [SerializeField] GameObject dayBG;
         [SerializeField] GameObject nightBG;
        [SerializeField] Material girlShader;
        [SerializeField] SpriteRenderer girlHeader;
        [SerializeField] GameObject girlHeaderShader;
        [SerializeField] Material boyShader;
        [SerializeField] SpriteRenderer boyHeader;
        [SerializeField] GameObject boyHeaderShader;
        [SerializeField] Material weirdShader;
        [SerializeField] SpriteRenderer weirdHeader;
        [SerializeField] GameObject weirdHeaderShader;

        [Header("Hearts")]
        //public List<Sprite> warningSprites = new List<Sprite>();
        public float speedForHeartsMultiplier = 3f;
        public List<heartDetails> reqHeartsContainer = new List<heartDetails>();
        [SerializeField] Transform heartHolder;
        [SerializeField] GameObject guyHeartObj;
        [SerializeField] GameObject girlHeartObj;
        [SerializeField] GameObject completeHeartObj;
        public List<LoveLabHearts> guyHearts;
        public List<LoveLabHearts> girlHearts;
        public List<LoveLabHearts> completeHearts;
        [SerializeField] List<int> currentHearts;
        [SerializeField] Transform endPoint;
        

        public enum timeOfDay
        {
            Sunset,
            Day
        }

        public void defaultShaders()
        {
            sunsetBG.SetActive(true);
            dayBG.SetActive(false);
            nightBG.SetActive(false);

            getShaderColors(0);

            girlHeaderShader.SetActive(true);
            boyHeaderShader.SetActive(true);
            weirdHeaderShader.SetActive(true);

            girlHeader.color  = Color.white;
            boyHeader.color   = Color.white;
            weirdHeader.color = Color.white;

            currentTime = timeOfDay.Sunset;
        }

        public void getShaderColors(int tod)
        {
            Color[,] wantShader;

            switch ((timeOfDay)tod)
            {
                case timeOfDay.Sunset:
                    wantShader = shaderColorsForSunset;
                    break;
                //case timeOfDay.Night:
                //    wantShader = shaderColorsForNight;
                //    break;
                default:
                    wantShader = shaderColorsForDay;
                    break;
            }

            for (int x = 0; x < 3; x++)
            {
                girlShader.SetColor(shaderColorsDebug[x], wantShader[0, x]);
                boyShader.SetColor(shaderColorsDebug[x], wantShader[1, x]);
                weirdShader.SetColor(shaderColorsDebug[x], wantShader[2, x]);
            }
        }

        public Color[,] shaderColorsForDay = new Color[,]
        {
            // girl
            { "#D8F8E0".Hex2RGB(), "#03C43E".Hex2RGB(), "#F8C0A8".Hex2RGB() },
            // boy
            { "#FFFFFF".Hex2RGB(), "#646765".Hex2RGB(), "#F8B890".Hex2RGB() },
            // weird
            { "#FFFFFF".Hex2RGB(), "#C08A00".Hex2RGB(), "#F86008".Hex2RGB() },
        };

        public Color[,] shaderColorsForSunset = new Color[,]
        {
            // girl
            { "#FFE95A".Hex2RGB(), "#03C43E".Hex2RGB(), "#FEA884".Hex2RGB() },
            // boy
            { "#FFD66B".Hex2RGB(), "#AF686B".Hex2RGB(), "#E37634".Hex2RGB() },
            // weird
            { "#F8C868".Hex2RGB(), "#F8C008".Hex2RGB(), "#E43000".Hex2RGB() },
        };

        public Color[,] shaderColorsForNight = new Color[,]
        {
            // girl
            { "#ACCCE9".Hex2RGB(), "#009847".Hex2RGB(), "#CC94B1".Hex2RGB() },
            // boy
            { "#D3D3FF".Hex2RGB(), "#383B6E".Hex2RGB(), "#CC8C99".Hex2RGB() },
            // weird
            { "#D3D3FF".Hex2RGB(), "#945E09".Hex2RGB(), "#CC3411".Hex2RGB() },
        };

        public string[] shaderColorsDebug = new string[]
        {
            "_ColorAlpha",
            "_ColorBravo",
            "_ColorDelta",
        };

        public void setTimeOfDay(int tod)
        {
            currentTime = (timeOfDay)tod;
            switch (currentTime)
            {
                case timeOfDay.Sunset:
                    defaultShaders();
                    getShaderColors(tod);
                    break;
                case timeOfDay.Day:
                    sunsetBG.SetActive(false);
                    dayBG.SetActive(true);
                    nightBG.SetActive(false);

                    getShaderColors(tod);

                    girlHeader.color  = Color.white;
                    boyHeader.color   = Color.white;
                    weirdHeader.color = Color.white;

                    girlHeaderShader.SetActive(false);
                    boyHeaderShader.SetActive(false);
                    weirdHeaderShader.SetActive(false);
                    break;
                /*
                case timeOfDay.Night:
                    sunsetBG.SetActive(false);
                    dayBG.SetActive(false);
                    nightBG.SetActive(true);

                    getShaderColors(tod);

                    girlHeader.color  = "#C9C9FF".Hex2RGB();
                    boyHeader.color   = "#C9C9FF".Hex2RGB();
                    weirdHeader.color = "#C9C9FF".Hex2RGB();

                    girlHeaderShader.SetActive(false);
                    boyHeaderShader.SetActive(false);
                    weirdHeaderShader.SetActive(false);
                    break;
                */
            }
        }
        

        float smthPreteval;
        bool smthAutoPass;

        public Transform getHeartHolder()
        {
            return heartHolder;
        }
        public void playFlaskBreak(int whichFlask)
        {
            if(whichFlask == 0) //guy
            {
                boyFlaskBreak.Play();
            }
            else
            {
                girlFlaskBreak.Play();
            }
        }

        public void labGuyIdleState()
        {
            labGuyHead.DoScaledAnimationAsync("GuyFaceIdle");

            if (labGuyArm.IsAnimationNotPlaying())
            {
                labGuyArm.Play("ArmIdle");
            }
        }

        public void labGirlIdleState()
        {
            labGirlHead.Play("GirlIdleFace");
            if (instance.labGirlArm.IsAnimationNotPlaying())
            {
                labGirlArm.DoScaledAnimationAsync("ArmIdle");
            }
        }
        public void labWeirdEndState(double beat, float addBeat, GameObject flaskObj)
        {
            BeatAction.New(LoveLab.instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + addBeat, delegate
                {
                    Destroy(flaskObj);
                    weirdInstantiatedFlask.RemoveAt(0);

                    labAssistantArm.DoScaledAnimationAsync("MittenGrabStart", 1f);
                }),

                new BeatAction.Action(beat + (addBeat+1f), delegate
                {

                    labAssistantArm.DoScaledAnimationAsync("MittenGrab", 1f);
                }),
                

                new BeatAction.Action(beat + (addBeat + 1.5f), delegate
                {
                    labAssistantArm.DoScaledAnimationAsync("MittenLetGo", .5f);
                }),
            });
        }

        private void DoInactiveInterval(double beat)
        {
            RiqEntity intervalEntity = gameManager.Beatmap.Entities.FindLast(e => e.beat - 1 <= beat && e.beat >= beat && e.datamodel == "loveLab/beat intervals");
            if (intervalEntity != null)
            {
                RiqEntity e = intervalEntity;
                PreInterval(e.beat, e.length, true);
            }
        }

        public void PreInterval(double beat, float interval, bool autoPassTurn)
        {
            if (GameManager.instance.currentGame != "loveLab") return;
            smthPreteval = interval;
            smthAutoPass = autoPassTurn;
            SetIntervalStart(beat, interval, beat, autoPassTurn);
        }

        private void SetIntervalStart(double beat, float interval, double gameSwitchBeat, bool autoPassTurn)
        {
            RiqEntity flaskEvent = GetFlaskBetween(beat, beat + interval);
            List<RiqEntity> reqShakeEvents = GetAllShakeBetween(flaskEvent.beat, flaskEvent.beat + flaskEvent.length);

            if (flaskEvent == null) return;

            reqShakeEvents.Sort((x, y) => x.beat.CompareTo(y.beat));

            currentHearts.Add(reqShakeEvents.Count + 1);

            string reqArc = "";
            float flaskInterval = interval - flaskEvent.length;

            reqArc = flaskInterval <= 1 ? flaskArcToBoy[0] : flaskArcToBoy[1];

            spawnCustomFlask(flaskEvent.beat, reqArc);
            boyFlask(flaskEvent.beat, flaskEvent.length, flaskInterval, interval);

            this.counter = reqShakeEvents.Count;

            List<MultiSound.Sound> sounds = new()
            {
                new("loveLab/leftCatch", flaskEvent.beat),
                new("loveLab/leftThrow", flaskEvent.beat + flaskEvent.length)
            };

            List<BeatAction.Action> queuedFlasks = new()
            {
                new BeatAction.Action(beat, delegate
                {
                    //initial shakes
                    hasMissed = false;

                    if (autoPassTurn)
                    {
                        //pass to girl
                        BoyPassToGirl(flaskEvent.beat + flaskEvent.length, beat, interval, flaskInterval, flaskEvent.length);
                    }
                })
            };
            for (int i = 0; i < reqShakeEvents.Count; i++)
            {
                RiqEntity shakeEventToCheck = reqShakeEvents[i];
                if (shakeEventToCheck.beat >= gameSwitchBeat)
                {
                    queuedFlasks.Add(new BeatAction.Action(shakeEventToCheck.beat, delegate
                    {
                        //lab boy shake
                        boyShake(shakeEventToCheck.beat, shakeEventToCheck.length, beat, beat + interval, interval);
                    }));

                    sounds.Add(new("loveLab/shakeUp", shakeEventToCheck.beat));
                    sounds.Add(new("loveLab/shakeDown", shakeEventToCheck.beat + shakeEventToCheck.length));
                }
            }
            MultiSound.Play(sounds, forcePlay: true);
            BeatAction.New(this, queuedFlasks);
            
        }

        private RiqEntity GetFlaskBetween(double beat, double endBeat)
        {
            return gameManager.Beatmap.Entities.Find(e => e.beat >= beat && e.beat + e.length < endBeat && e.datamodel == "loveLab/flask");
        }

        private List<RiqEntity> GetAllShakeBetween(double beat, double endBeat)
        {
            List<RiqEntity> speakEvents = EventCaller.GetAllInGameManagerList("loveLab", new string[] { "shake" });
            List<RiqEntity> tempEvents = new();

            foreach (var entity in speakEvents)
            {
                if (entity.beat > beat && entity.beat + entity.length < endBeat)
                {
                    tempEvents.Add(entity);
                }
            }

            return tempEvents;
        }

        private void girlLook(double beat, double length)
        {
            float speedLook = 0;
            speedLook = length <= .5f ? 0 : .5f;

            labGuyHead.DoScaledAnimationAsync("GuyRightFace");

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + speedLook, delegate
                {
                    labGirlHead.Play("GirlLeftFace");
                })
            });
        }

        private void boyShake(double beat, double length, double firstBeatOfInterval, double lastBeatOfInterval, double intervalLength)
        {
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate {
                    if(lastBeatOfInterval > beat + length)
                    {
                        labGuyArm.Play("ShakeFlaskUp");

                        LoveLabHearts bh = Instantiate(guyHeartObj, heartHolder).GetComponent<LoveLabHearts>();
                        bh.heartBeat = beat;
                        bh.length = length;
                        bh.heartCount = guyHearts.Count;
                        bh.intervalSpeed = intervalLength;

                        guyHearts.Add(bh);
                        heartUp(guyHearts);
                    }
                }),

                new BeatAction.Action(beat + length, delegate {                
                    if(beat > firstBeatOfInterval)
                    {
                        labGuyArm.Play("ShakeFlaskDown");
                    }
                }),
            });
        }

        private void boyFlask(double beat, double length, double flaskInterval, double intervalLength)
        {
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate {                
                    girlLook(beat, flaskInterval);
                    labGuyArm.Play("GrabFlask");
                    LoveLabHearts bh = Instantiate(guyHeartObj, heartHolder).GetComponent<LoveLabHearts>();
                    bh.heartBeat = beat;
                    bh.length = length;
                    bh.heartCount = girlHearts.Count != 0 ? 0 : guyHearts.Count;
                    bh.intervalSpeed = intervalLength;
                    bh.addPos = length <= .5f && currentHearts[currentHearts.Count - 1] <= 1 ? 1.5f : 2.5f;    

                    guyHearts.Add(bh);
                    //heartUp(guyHearts);
                }),

                new BeatAction.Action(beat + length, delegate {
                    labGuyArm.Play("ThrowFlask");
                    int arc = 0;
                    switch (intervalLength - length)
                    {
                        case >= 2:
                            arc = 2;
                            break;
                        case > 1:
                            arc = 1;
                            break;
                    }
                    spawnFlaskForGirlCustom(beat + length + flaskInterval, arc, flaskInterval);

                    foreach(LoveLabHearts h in guyHearts)
                    {
                        h.stop = true;
                    }
                }),
            });
        }

        public List<int> howManyShakes = new List<int>();
        public List<int> howManyShakesCounter = new List<int>();
        public List<double> howManyShakesEndBeat = new List<double>();

        public void clearShakeList()
        {
            howManyShakes.RemoveAt(0);
            howManyShakesCounter.RemoveAt(0);
            howManyShakesEndBeat.RemoveAt(0);
        }



        private void BoyPassToGirl(double beat, double intervalBeat, float intervalLength, float flaskInterval, float flaskLength)
        {
            RiqEntity flaskInput = GetFlaskBetween(intervalBeat, intervalBeat + intervalLength);
            List<RiqEntity> inputs = GetAllShakeBetween(flaskInput.beat, flaskInput.beat + flaskInput.length);
            inputs.Sort((x, y) => x.beat.CompareTo(y.beat));

            howManyShakes.Add(inputs.Count);

            ScheduleInput(beat, flaskInterval, InputAction_BasicPress, (c, s) => onCatch(c, s, flaskLength, intervalLength), onCatchMiss, onEmpty);
            release = ScheduleInput(beat, flaskInterval + flaskInput.length, IA_FlickRelease, onRelease, onMiss, onEmpty);

            for (int i = 0; i < inputs.Count; i++)
            {
                var input = inputs[i];
                double shakeUpBeat = input.beat - flaskInput.beat;
                double shakeDownBeat = shakeUpBeat + input.length;
                
                ScheduleInput(beat, flaskInterval + shakeUpBeat, IA_DPadPressing, (c, s) => onUp(c, s, input.length), onMiss, onEmpty);
                ScheduleAutoplayInput(beat, flaskInterval + shakeDownBeat, IA_DPadRelease, onDownAuto, onMiss, onEmpty);
            }

            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    hasMissed = false;

                }),
                new BeatAction.Action(intervalBeat + intervalLength, delegate
                {
                    labGuyHead.Play("GuyFaceIdle");
                    if (labGuyArm.IsAnimationNotPlaying())
                    {
                        labGuyArm.Play("ArmIdle");
                    }
                })
            });
        }


        public void onCatch(PlayerActionEvent caller, float beat, float length, float intervalLength)
        {
            labGirlHead.Play("GirlIdleFace");
            LoveLabHearts gh = Instantiate(girlHeartObj, heartHolder).GetComponent<LoveLabHearts>();
            gh.heartBeat = (caller.startBeat + caller.timer);
            gh.heartCount = girlHearts.Count;
            gh.length = length;
            if (currentHearts.Any()) gh.addPos = gh.length <= .5f && currentHearts[currentHearts.Count - 1] <= 1 ? 1.5f : 2.5f;
            gh.intervalSpeed = intervalLength;

            if (!isHolding)
            {
                isHolding = true;
                isHoldingFlask = true;
                girlHearts.Add(gh);
                SoundByte.PlayOneShotGame("loveLab/RightCatch");
                labGirlArm.DoScaledAnimationAsync("GrabFlask");
                girlInstantiatedFlask[0].GetComponent<LoveLabFlask>().destroyThisObj();
            }
        }

        public void heartUp(List<LoveLabHearts> hearts)
        {
            var heartCount = hearts.Count;
            var constant = 1.5f;

            if (hearts[heartCount - 1].getHeartType() == 1)
            {
                hearts[heartCount - 1].intervalSpeed = guyHearts[heartCount - 1].intervalSpeed;
            }

            for (int x = 0; x < heartCount - 1; x++)
            {
                hearts[x].nextHeartBeat = hearts[x + 1].heartBeat;

            }

            for (int x = 0; x < heartCount; x++)
            {
                if (x > 0)
                {
                    hearts[x].prevHeartBeat = hearts[x - 1].heartBeat;
                }
            }

            var max = hearts.Count - 1;
            //Debug.LogWarning(max);

            for (int x = 0; x < heartCount; x++)
            {
                if (hearts.Count > 1) //should only check if hearts is more than 1
                {
                    hearts[x].onlyOne = false;
                    if (x == 0)
                    {
                        if (hearts[max].nextHeartBeat == 0)
                        {
                            hearts[x].addPos += (float)(hearts[max].heartBeat - hearts[max].prevHeartBeat) * constant;
                        }
                        else
                        {
                            hearts[0].addPos += (float)(hearts[max].nextHeartBeat - hearts[max].heartBeat) * constant;
                        }

                    }
                    else if (x == max)
                    {
                        continue;
                    }
                    else
                    {
                        hearts[x].addPos += (float)(hearts[max].heartBeat - hearts[max].prevHeartBeat) * constant;

                    }

                    /*  4.5 //only in 0 (next heart - heart)
                        4 (prev heart - heart)
                        3.5 (prev heart - heart)
                        2.5 (prev heart - heart)
                        2 (nothing)

                        should use the current heart spacing
                    */
                }
            }
        }

        public void onUp(PlayerActionEvent caller, float beat, float length)
        {
            if (GameManager.instance.Autoplay) onWhiffUp();
            if (hasMissed) return;

            LoveLabHearts gh = Instantiate(girlHeartObj, heartHolder).GetComponent<LoveLabHearts>();
            gh.heartBeat = (caller.startBeat + caller.timer);
            gh.heartCount = girlHearts.Count;
            gh.length = length;
            girlHearts.Add(gh);
            heartUp(girlHearts);
        }

        public void onDownAuto(PlayerActionEvent caller, float beat)
        {
            hasShakenUp = false;
            SoundByte.PlayOneShotGame("loveLab/shakeDown");
            labGirlArm.DoScaledAnimationAsync("ShakeFlaskDown");
        }

        public void onRelease(PlayerActionEvent caller, float beat)
        {
            hasShakenUp = false;
            labGirlHead.DoScaledAnimationAsync("GirlIdleFace");
            SoundByte.PlayOneShotGame("loveLab/rightThrowNoShake");
            labGirlArm.DoScaledAnimationAsync("ThrowFlask");
            isHolding = false;
            isHoldingFlask = false;
            hasStartedInterval = false;
            release = null;

            bool miss = hasMissed;

            //clearShakeList();

            foreach (LoveLabHearts h in girlHearts)
            {
                h.stop = true;
            }   

            spawnFlaskForWeird();

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action((caller.startBeat + caller.timer) + 1f, delegate
                {
                    labGirlIdleState();

                    if (miss) return;

                    SoundByte.PlayOneShotGame("loveLab/heartsCombine");

                     foreach(LoveLabHearts h in girlHearts)
                    {
                        h.stop = true;
                    }

                    var additionalHearts = 0.063f;

                    for(int x = 0; x < currentHearts[0]; x++)
                    {
                        guyHearts[0].heartAnim.Play("HeartMerge");
                        girlHearts[0].heartAnim.Play("HeartGirlMerge");

                        LoveLabHearts a = Instantiate(completeHeartObj, heartHolder).GetComponent<LoveLabHearts>();
                        a.transform.position = new Vector2(a.transform.position.x, girlHearts[0].transform.position.y);
                        var additionalHeartLength = (additionalHearts * girlHearts.Count);
                        a.heartBeat = caller.startBeat + caller.timer + 1f;
                        a.dropStart = a.transform.position;
                        a.end = endPoint.position;
                        completeHearts.Add(a);
                        a.timer = (2.25f + additionalHeartLength);
                        a.isWaiting = false;
                        //a.goDown();

                        guyHearts.RemoveAt(0);
                        girlHearts.RemoveAt(0);
                    }

                    currentHearts.RemoveAt(0);
                }),
            });

            var lastCounter = currentHearts.Count > 0 ? currentHearts[0] : -1;
            if (lastCounter == -1) return;

            for (int x = 0; x < lastCounter; x++)
            {
                var additionalHearts = 0.063f;
                var a = x;

                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action((caller.startBeat + caller.timer) + (2.25f + (additionalHearts * a)), delegate
                    {
                        if(lastCounter == 1)
                        {
                            SoundByte.PlayOneShotGame("loveLab/bagHeartLast");
                            heartBox.DoScaledAnimationAsync("HeartBoxSquish");
                        }
                        else
                        {
                            bagHeartSound(a);
                        }
                    })

                });
            }
        }

        public void onMiss(PlayerActionEvent caller = null)
        {
            //SoundByte.PlayOneShot("miss");
            if (hasMissed) return;

            labGuyHead.DoScaledAnimationAsync("GuyIdleFace");
            hasShakenUp = false;
            hasMissed = true;
            hasStartedInterval = false;

            foreach (LoveLabHearts bh in guyHearts)
            {
                bh.deadHeart();
            }

            if (girlHearts.Count != 0)
            {
                foreach (LoveLabHearts gh in girlHearts)
                {
                    gh.deadHeart();
                }
            }

            if (reqHeartsContainer.Count > 0)
            {
                reqHeartsContainer.RemoveAt(0);
            }

            if (currentHearts.Count > 0)
            {
                for (int i = 0; i < currentHearts[0]; i++)
                {
                    if (guyHearts.Count != 0)
                    {
                        guyHearts.RemoveAt(0);
                    }

                    if (girlHearts.Count != 0)
                    {
                        girlHearts.RemoveAt(0);
                    }
                }

                currentHearts.RemoveAt(0);
            }
        }

        public void onCatchMiss(PlayerActionEvent caller)
        {
            if (!isHoldingFlask)
            {
                release.CanHit(false);
            }
            boyFlaskBreak.Play();
            girlInstantiatedFlask[0].GetComponent<LoveLabFlask>().destroyThisObj();
            onMiss(caller);
        }

        public void onMissWhenHold(PlayerActionEvent caller)
        {
            Debug.LogWarning(caller.timer);
            reqHeartsContainer.RemoveAt(0);
            LoveLabFlask spawnedFlask = Instantiate(labGirlFlaskObj, transform).AddComponent<LoveLabFlask>();
            spawnedFlask.onMissWhenHold((caller.startBeat + caller.timer) + 1f);
        }

        public static void onEmpty(PlayerActionEvent caller)
        {
            //empty
        }

        public void bagHeartSound(int x)
        {
            var pitchHearts = 0.14f;
            SoundByte.PlayOneShotGame("loveLab/bagHeart", pitch: 1f + (pitchHearts * x));
            heartBox.DoScaledAnimationAsync("HeartBoxSquish");
        }

        public void onDownFlaskCustom() //for the player not autoplay
        {
            SoundByte.PlayOneShotGame("loveLab/shakeDown");
            labGirlArm.DoScaledAnimationAsync("ShakeFlaskDown");
        }

        public void spawnFlaskForGirlCustom(double beat, int flaskHeart, double flaskInterval) //boy to girl
        {
            //flaskMatForBoy.SetColor("_ColorAlpha", boyLiquidColor);
            LoveLabFlask spawnedFlask = Instantiate(labFlaskObj, transform).AddComponent<LoveLabFlask>();

            switch (flaskHeart)
            {
                case 0:
                    spawnedFlask.girlArc(beat, flaskArcToGirl[0], flaskInterval);
                    break;
                case 1:
                    spawnedFlask.girlArc(beat, flaskArcToGirl[1], flaskInterval);
                    break;
                case 2:
                    spawnedFlask.girlArc(beat, flaskArcToGirl[2], flaskInterval);
                    break;
            }

            girlInstantiatedFlask.Add(spawnedFlask.GetComponent<LoveLabFlask>());
        }

        public void spawnCustomFlask(double beat, string reqArc) //wall to boy
        {
            LoveLabFlask spawnedFlask = Instantiate(labFlaskObj, transform).AddComponent<LoveLabFlask>();
            spawnedFlask.customShakes(beat, reqArc);
        }

        public void cloudMove(bool canMove)
        {
            canCloudsMove = canMove;
        }
        
        public void girlBlush(bool autoBlush)
        {
            if (!autoBlush)
            {
                labGirlHead.Play("GirlBlushFace");
            }
            else
            {
                labGirlHead.Play("GirlBlushFace");
            }
        }
        [SerializeField] SuperCurveObject.Path[] flaskBouncePath;

        public void spotlight(bool active, int whichType, int whichPos)
        {
            if(active == true)
            {
                if (whichType == (int)spotlightType.normal)
                {
                    spotlightShader.SetActive(true);
                    spotlightShaderCone.SetActive(false);
                }
                else
                {
                    spotlightShader.SetActive(false);
                    spotlightShaderCone.SetActive(true);

                    if (whichPos == (int)spotlightPos.boy)
                    {
                        spotlightCone.transform.position = new Vector2(0, 0);
                    }
                    else
                    {
                        spotlightCone.transform.position = new Vector2(5f, 0);
                    }
                }   
            }
            else
            {
                spotlightShader.SetActive(false);
                spotlightShaderCone.SetActive(false);
            }
        }

        public enum spotlightType
        {
            normal,
            cone
        }

        public enum spotlightPos
        {
            boy,
            girl
        }

        public void UpdateMaterialColor(Color liquidCol, Color liquid2Col, Color liquid3Col)
        {
            boyLiquidColor = liquidCol;
            girlLiquidColor = liquid2Col;
            weirdLiquidColor = liquid3Col;

            flaskMatForBoy.SetColor("_ColorAlpha", boyLiquidColor);
            flaskMatForGirl.SetColor("_ColorAlpha", girlLiquidColor);
            flaskMatForWeird.SetColor("_ColorAlpha", weirdLiquidColor);
        }

        public override void OnGameSwitch(double beat)
        {
            preChecks(beat);
            DoInactiveInterval(beat);
            //PersistObjects(beat);
        }

        public override void OnPlay(double beat)
        {
            preChecks(beat);
            queuedFlask.Clear();
        }

        private void OnDestroy()
        {
            //preChecks(0f);
            queuedFlask.Clear();
            //destroyAllFlasks();
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        void preChecks(double beat)
        {
            List<RiqEntity> prevEntities = GameManager.instance.Beatmap.Entities.FindAll(c => c.datamodel.Split(0) == "loveLab");
            RiqEntity obj = prevEntities.FindLast(c => c.beat <= beat && c.datamodel == "loveLab/set object colors");

            if (obj != null)
            {
                UpdateMaterialColor(obj["colorA"], obj["colorB"], obj["colorC"]);
            }
            else
            {
                UpdateMaterialColor(new Color(0.02909997f, 0.4054601f, 0.97f), new Color(0.972549f, 0.3764706f, 0.03137255f), new Color(0.8313726f, 0.2039216f, 0.5058824f));
            }

            getShaderColors(0);

            try
            {
                RiqEntity obj2 = prevEntities.FindLast(c => c.beat <= beat && c.datamodel == "loveLab/bop");

                if (obj2 != null)
                {
                    Bop(obj2.beat, obj2.length, obj2["toggle"], obj2["toggle2"]);
                }
                else
                {
                    Bop(0, 0, true, true);
                }
            }
            catch { }
        }

        protected static bool IA_TouchDpadPress(out double dt)
        {
            
            return PlayerInput.GetSlide(out dt) &&  !instance.hasShakenUp;
        }
        protected static bool IA_TouchDpadRelease(out double dt)
        {
            
            return PlayerInput.GetSlide(out dt) &&  instance.hasShakenUp;
        }




        const int IAAltPressCat = IAMAXCAT + 1;
        const int IAAltReleaseCat = IAMAXCAT + 2;

        public static PlayerInput.InputAction IA_FlickRelease =
            new("NtrLabFlickRelease", new int[] { IAReleaseCat, IAFlickCat, IAReleaseCat },
            IA_PadBasicRelease, IA_TouchFlick, IA_BatonBasicRelease);
        public static PlayerInput.InputAction IA_DPadRelease =
            new("NtrLabDPadRelease", new int[] { IAAltReleaseCat, IAFlickCat, IAReleaseCat },
                IA_PadAnyUp, IA_TouchDpadRelease, IA_BatonBasicRelease);
        public static PlayerInput.InputAction IA_DPadPressing =
            new("NtrLabDPadPress", new int[] { IAAltPressCat, IAFlickCat, IAReleaseCat },
                IA_PadAnyPressingDown, IA_TouchDpadPress, IA_BatonBasicPressing);

        protected static bool IA_TouchNrmPress(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt);
        }

        protected static bool IA_PadAnyDown(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.Up, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Down, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Right, out dt);
        }

        protected static bool IA_PadAnyUp(out double dt)
        {
            return PlayerInput.GetPadUp(InputController.ActionsPad.Up, out dt)
                    || PlayerInput.GetPadUp(InputController.ActionsPad.Down, out dt)
                    || PlayerInput.GetPadUp(InputController.ActionsPad.Left, out dt)
                    || PlayerInput.GetPadUp(InputController.ActionsPad.Right, out dt);
        }

        protected static bool IA_PadAnyPressingDown(out double dt)
        {
            dt = 0;

            return PlayerInput.GetPadDown(InputController.ActionsPad.Up)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Down)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Left)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Right);
        }

        private void Awake()
        {
            instance = this;
            release = null;

            labGuyHead.DoScaledAnimation("GuyFaceIdle", 1f);
            labAssistantHead.DoScaledAnimation("WeirdFaceIdle", 1f);

            flaskSpriteRend.material = flaskMatForBoy;
            girlFlaskSpriteRend.material = flaskMatForGirl;
            weirdFlaskSpriteRend.material = flaskMatForWeird;
        }

        private void Start()
        {
            if (canCloudsMove) clouds.transform.position = Vector3.left * ((Time.realtimeSinceStartup * cloudSpeed) % cloudDistance);
            else clouds.transform.position = new Vector2(0, 0);
        }

        // Editor gizmo to draw trajectories
        new void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            foreach (SuperCurveObject.Path path in flaskBouncePath)
            {
                if (path.preview)
                {
                    try
                    {
                        labFlaskObj.GetComponent<LoveLabFlask>().DrawEditorGizmo(path);
                    }
                    catch { }
                }
            }
        }

        public SuperCurveObject.Path GetPath(string name)
        {
            foreach (SuperCurveObject.Path path in flaskBouncePath)
            {
                if (path.name == name)
                {
                    return path;
                }
            }
            return default(SuperCurveObject.Path);
        }

        public enum flaskHeart
        {
            oneHeart,
            oneOffHeart,
            twoHeart,
            threeHeart,
            fiveHeart
        }

        public struct QueuedFlask //for the offscreen stuff
        {
            public double beat;
            public flaskHeart type;
        }

        public struct heartDetails
        {
            public flaskHeart heartType;
            public int heartCounter;
        }

        public void spawnFlaskForWeird()
        {
            double beat = conductor.unswungSongPositionInBeatsAsDouble;

            LoveLabFlask spawnedFlask = Instantiate(labGirlFlaskObj, transform).AddComponent<LoveLabFlask>();
            spawnedFlask.ForWeirdInit(beat + 1f);
            weirdInstantiatedFlask.Add(spawnedFlask.GetComponent<LoveLabFlask>());
        }
        

        private void Update()
        {
            var cond = Conductor.instance;
            var songPos = cond.unswungSongPositionInBeatsAsDouble;
            
            if (cond.isPlaying && !cond.isPaused)
            {
                if (queuedFlask.Count != 0)
                {
                    queuedFlask.Clear();
                }
            }
            else
            {
                if ((!cond.isPaused) && queuedFlask.Count != 0)
                {
                    queuedFlask.Clear();
                }
            }

            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress) && !isHolding)
            {
                labGirlArm.DoScaledAnimationAsync("WhiffGrab");
                
                isHolding = true;
            }
            else if (PlayerInput.GetIsAction(InputAction_BasicRelease) && (!IsExpectingInputNow(InputAction_BasicRelease)))
            {
                labGirlArm.DoScaledAnimationAsync("ArmIdle");
                if (isHoldingFlask) 
                {
                    hasShakenUp = false;
                    LoveLabFlask spawnedFlask = Instantiate(labGirlFlaskObj, transform).AddComponent<LoveLabFlask>();
                    spawnedFlask.onMissWhenHold(songPos + 1f);
                    if (release != null)
                    {
                        release.CanHit(false);
                    }
                    onMiss();
                }
                isHolding = false;
                isHoldingFlask = false;
            }

            if (PlayerInput.GetIsAction(IA_DPadPressing) && PlayerInput.GetIsAction(InputAction_BasicPressing))
            {
                onWhiffUp();
            }
            else if (PlayerInput.GetIsAction(IA_DPadRelease) && PlayerInput.GetIsAction(InputAction_BasicPressing))
            {
                onWhiffDown();
            }
            

            if (canCloudsMove) clouds.transform.position = Vector3.left * ((Time.realtimeSinceStartup * cloudSpeed) % cloudDistance);
            else clouds.transform.position = new Vector2(0, 0);
        }

        public void Bopping()
        {
            if (bopRight && labGuy.IsAnimationNotPlaying())
            {
                labGuy.DoScaledAnimationAsync("GuyBopRight", .5f);
                labGirl.DoScaledAnimationAsync("GirlBopRight", .5f);
                labAssistant.DoScaledAnimationAsync("AssistantBopRight", .5f);
            }
            else
            {
                labGuy.DoScaledAnimationAsync("GuyBopLeft", .5f);
                labGirl.DoScaledAnimationAsync("GirlBopLeft", .5f);
                labAssistant.DoScaledAnimationAsync("AssistantBopLeft", .5f);
            }
            bopRight = !bopRight;
        }
        public void Bop(double beat, float length, bool bop, bool autoBop)
        {
            if (bop)
            {
                for (int i = 0; i < length; i++)
                {
                    var a = i;
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + a, delegate
                        {
                            Bopping();
                        })
                    });
                }
            }
            if (autoBop)
            {
                ToggleBop(true);
            }
            else
            {
                ToggleBop(false);
            }
        }

        public override void OnLateBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat)) SingleBop();
        }

        public void ToggleBop(bool go)
        {
            canBop = go;
        }

        void SingleBop()
        {
            if (canBop)
            {
                Bopping();
            }
        }

        


        public enum boxGuyAction
        {
            takeAway,
            putBack,
            noBox,
            instaBox
        }

        public void mainBoxGuy(double beat, int reqAction)
        {
            Animator box;

            if (currentTime == timeOfDay.Sunset)
            {
                box = boxPerson;
            }
            else
            {
                box = boxPersonDay;
            }

            switch (reqAction)
            {
                case 0:
                    box.DoScaledAnimationAsync("BoxTakeAway", .5f);
                    break;
                case 1:
                    box.DoScaledAnimationAsync("BoxPutBack", .5f);
                    break;
                case 2:
                    box.DoScaledAnimationAsync("NoBox", 0f);
                    break;
                case 3:
                    box.DoScaledAnimationAsync("BoxIdle", 0f);
                    break;
                default:
                    box.DoScaledAnimationAsync("BoxTakeAway", .5f);
                    break;
            }   
        }


        

        public void onWhiffUp()
        {
            if (hasShakenUp) return;
            hasShakenUp = true;

            if (isHoldingFlask)
            {
                SoundByte.PlayOneShotGame("loveLab/shakeUp");
                labGirlArm.DoScaledAnimationAsync("ShakeFlaskUp");
                return;
            }
            labGirlArm.DoScaledAnimationAsync("WhiffUp", 0.75f);
        }

        public void onWhiffDown()
        {
            if (!hasShakenUp) return;
            hasShakenUp = false;

            if (isHoldingFlask)
            {
                SoundByte.PlayOneShotGame("loveLab/shakeDown");
                labGirlArm.DoScaledAnimationAsync("ShakeFlaskDown");
                return;
            }
            labGirlArm.DoScaledAnimationAsync("WhiffDown", 0.75f);
        }
    }
}