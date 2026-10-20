using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;
using HeavenStudio.Games.Loaders;

using Debug = UnityEngine.Debug;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class CtrSamuraiLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("superSamuraiSlice", "Super Samurai Slice", "435e90", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out SuperSamuraiSlice instance)) {
                            instance.Bop(e.beat, e.length, e["auto"], e["bop"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop", "Toggle if the Samurai should bop for the duration of this event."),
                        new Param("auto", false, "Bop (Auto)", "Toggle if the Samurai should automatically bop until another Bop event is reached."),
                    },
                    resizable = true,
                    defaultLength = 1f
                },
                new GameAction("smallDemon", "Small Demon")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out SuperSamuraiSlice instance)) {
                            instance.SpawnDemon(e.beat, e["auto"], e["count"], e["autoBoom"], e["boomBeat"], e["flipback"], e["enableBoom"]);
                        }
                    },
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out SuperSamuraiSlice instance)) {
                            instance.CanFlip(e.beat);
                        }
                    },
                    preFunctionLength = 0.25f,
                    parameters = new List<Param>()
                    {
                        new Param("auto", true, "Auto Position", "Toggle whether position will be auto.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => !(bool)x, new string[] { "count"})
                        }),
                        new Param("count", new EntityTypes.Integer(1, 4, 1), "Demon Positions", "What position will the demon be in?"),
                        new Param("flipback", new EntityTypes.Integer(0, 2, 1), "Flip Delay", "Set the amount of beats the Samurai should flip back after"),
                        new Param("autoBoom", true, "Auto Explode", "Toggle if the booms should be auto.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => !(bool)x, new string[] { "boomBeat"})
                        }),
                        new Param("boomBeat", new EntityTypes.Float(0, 3, 1), "Explosion Beat Delay", "How many beats after the input will the explosion happen?"),
                        new Param("enableBoom", true, "Enable Explosion SFX", "Toggle if the explosion should be enabled."),
                    },
                    defaultLength = 3f
                },
                new GameAction("bigDemon", "Large Demon")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out SuperSamuraiSlice instance)) {
                            instance.SpawnBigDemon(e.beat, e["invert"], e["autoBoom"], e["boomBeat"], e["enableBoom"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("invert", false, "Opposite Direction", "Toggle if the Large Demon should come from the left side."),
                        new Param("autoBoom", true, "Auto Explode", "Toggle if the booms should be auto.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => !(bool)x, new string[] { "boomBeat"})
                        }),
                        new Param("boomBeat", new EntityTypes.Float(0, 3, 1), "Explosion Beat Delay", "How many beats after the input will the explosion happen?"),
                        new Param("enableBoom", true, "Enable Explosion SFX", "Toggle if the explosion should be enabled."),
                    },
                    defaultLength = 6f
                },
                new GameAction("scroll", "Scroll Settings")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out SuperSamuraiSlice instance)) {
                            instance.Scroll(e.beat, e["speed"], (SuperSamuraiSlice.Platform)e["platform"], e["instant"], e["off"], e["offset"], e["mute"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("speed", new EntityTypes.Float(0, 3, 1), "Scroll Speed", "Set the Scroll speed."),
                        new Param("platform", SuperSamuraiSlice.Platform.Skateboard, "Platform", "Choose which Platform that should appear."),
                        new Param("instant", false, "Instant", "Toggle if it should start with the Samurai already on."),
                        new Param("off", false, "Get Off Platform", "Toggle if the Samurai should get off.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => !(bool)x, new string[] { "speed", "instant", "platform" })
                        }),
                        new Param("mute", false, "Mute Sound", "Toggle to mute the Eagle."),
                        new Param("offset", new EntityTypes.Float(1, 8, 1), "Offset", "Set what beat the Samurai should get off"),
                    },
                    defaultLength = 0.5f
                },
                new GameAction("fogToggle", "Toggle Environment Elements")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out SuperSamuraiSlice instance)) {
                            instance.ToggleFog(e["fog"], e["water"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("fog", false, "Enable Fog", "Toggle if the fog is enabled."),
                        new Param("water", true, "Enable Water", "Toggle if the water is enabled."),
                    },
                    defaultLength = 0.5f
                },
            },
            new List<string>() { "ctr", "normal" },
            "ctrdotsamurai", "en",
            new List<string>() { },
            chronologicalSortKey: 6,
            font: "KH Dot"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_SuperSamuraiSlice;
    public class SuperSamuraiSlice : Minigame
    {
        public static SuperSamuraiSlice instance;

        [Header("Game Logic")]
        public bool samuraiBop = true;
        public GameObject SamuraiShadow;
        public GameObject Samurai;
        public Transform BG;
        public Transform Water;
        public GameObject waterGO;
        public Transform FG;
        public Transform Fog;
        public GameObject fogGO;
        public Transform Cloud;

        public Animator flash;
        public GameObject Skateboard;
        public GameObject Eagle;
        public Animator BGAnim;
        public Animator CloudAnim;
        public Animator Samurai1Anim;
        public Animator Skateboard1Anim;
        public Animator Eagle1Anim;
        public Animator Rocket1Anim;

        public bool isSkateboardActive = false;
        public bool isEagleActive = false;
        public bool isRocketActive = false;
        public bool isWaterActive = true;

        public bool isOn = false;
        bool isOff = false;

        public int direction = 0;

        float DistancePerBeat = 0f;
        float BGWidth = 12.446f;
        float FGWidth = 6.803f;
        float WaterWidth = 6.43f;
        float FogWidth = 12.94f;
        float CloudWidth = 6.4f;
        public enum Platform
        {
            Skateboard,
            Eagle,
            //Rocket
        }
        double lastBeat = Double.MinValue;
        double lastBeat2 = Double.MinValue;
        List<RiqEntity> demonEntities;
        public List<double> boomBeats;
        public List<bool> enabledBooms;
        bool currentAutoBoom;
        public int whichInput = -1;

        public int slashType = 0;
        public int demonAnims = 0;
        public bool isLargeDemonActive = false;

        [Header("Animations")]
        [SerializeField] public Animator SamuraiAnim;
        public Animator SkateboardAnim;
        public Animator EagleAnim;
        public Animator RocketAnim;
        public ParticleSystem lightning;
        public ParticleSystem waterL;
        public ParticleSystem waterR;

        [Header("Demons")]
        public Demon SmallDemon;
        public Demon MediumDemon;
        public Demon BigDemon;

        [Header("Curves")]
        [SerializeField] SuperCurveObject.Path[] demonPaths;

        public override void OnBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat))
            {
                if (samuraiBop)
                {
                    SamuraiAnim.DoScaledAnimationAsync("Beat", 0.5f);
                }
            }
        }
        private void Awake()
        {
            instance = this;
            SetupBopRegion("superSamuraiSlice", "bop", "auto");
        }
        public void Update()
        {
            var cond = Conductor.instance;
            double beat = conductor.unswungSongPositionInBeatsAsDouble;

            if (PlayerInput.GetIsAction(InputAction_Press) && !IsExpectingInputNow(InputAction_Press) && !isLargeDemonActive)
            {
                SamuraiAnim.DoScaledAnimationAsync("Slash00", 0.5f);
                SoundByte.PlayOneShotGame("superSamuraiSlice/SE_IAI_NEW_SWING1", UnityEngine.Random.Range(-1.0f, 1.0f));
            }

            Vector3 BGPos = BG.localPosition;
            double newBGPos = BGPos.x + (-DistancePerBeat * Time.deltaTime);
            newBGPos %= BGWidth;
            BG.localPosition = new Vector3((float)newBGPos, BGPos.y, BGPos.z);

            Vector3 FGPos = FG.localPosition;
            double newFGPos = FGPos.x + (-DistancePerBeat * Time.deltaTime);
            newFGPos %= FGWidth;
            FG.localPosition = new Vector3((float)newFGPos, FGPos.y, FGPos.z);

            Vector3 WPos = Water.localPosition;
            double newWPos = WPos.x + (-DistancePerBeat * Time.deltaTime);
            newWPos %= WaterWidth;
            Water.localPosition = new Vector3((float)newWPos, WPos.y, WPos.z);

            Vector3 FPos = Fog.localPosition;
            double newFPos = FPos.x + (-DistancePerBeat * Time.deltaTime);
            newFPos %= FogWidth;
            Fog.localPosition = new Vector3((float)newFPos, FPos.y, FPos.z);

            Vector3 CPos = Cloud.localPosition;
            double newCPos = CPos.x + (-DistancePerBeat * Time.deltaTime);
            newCPos %= CloudWidth;
            Cloud.localPosition = new Vector3((float)newCPos, CPos.y, CPos.z);

        }

        public void Scroll(double beat, float speed, Platform platform, bool instant, bool off, float offset, bool mute)
        {
            if (off && isOn)
            {
                CloudAnim.DoScaledAnimationAsync("fade", 0.5f);
                DistancePerBeat = 0f;

                if (isEagleActive)
                {
                    BGAnim.DoScaledAnimationAsync("down", 0.5f);
                }

                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + offset, delegate {
                        if (isSkateboardActive)
                        {
                            if (isEagleActive) { }
                            else
                            {
                                Samurai1Anim.DoScaledAnimationAsync("exit", 0.5f);
                                Skateboard1Anim.DoScaledAnimationAsync("exit", 0.5f);
                            }
                        }
                    }),
                    new BeatAction.Action(beat + 3 + offset, delegate {
                        if (isEagleActive)
                        {
                            Samurai1Anim.DoScaledAnimationAsync("idle", 0.5f);
                            Eagle1Anim.DoScaledAnimationAsync("exit", 0.5f);
                        }
                    }),
                    new BeatAction.Action(beat + offset + 4, delegate {
                        Skateboard.SetActive(false);
                        Eagle.SetActive(false);
                        SamuraiShadow.SetActive(true);
                        isOn = false;

                        CloudAnim.DoScaledAnimationAsync("idle", 0.5f);
                    }),
                });
            }
            else
            {
                DistancePerBeat = speed;
                SamuraiShadow.SetActive(false);
                isOn = true;
                if (instant)
                {
                    Samurai1Anim.DoScaledAnimationAsync("instant", 0.5f);

                    switch (platform)
                    {
                        case Platform.Skateboard:
                            if (isEagleActive) { }
                            else if (isSkateboardActive) { }
                            else
                            {
                                Skateboard1Anim.DoScaledAnimationAsync("idle", 0.5f);
                                isSkateboardActive = true;
                                Skateboard.SetActive(true);
                            }
                            break;
                        case Platform.Eagle:
                            if (isEagleActive) { }
                            else if (isSkateboardActive) { Skateboard.SetActive(false); }
                            else
                            {
                                BGAnim.DoScaledAnimationAsync("instant", 0.5f);
                                Skateboard.SetActive(false);
                                Eagle.SetActive(true);

                                if (mute) { }
                                else
                                {
                                    SoundByte.PlayOneShotGame("superSamuraiSlice/SE_IAI_NEW_BIRD_1");
                                }
                                Eagle1Anim.DoScaledAnimationAsync("idle", 0.5f);
                                isEagleActive = true;
                            }
                            break;
                    }
                }
                else
                {
                    switch (platform)
                    {
                        case Platform.Skateboard:
                            if (isEagleActive) { }
                            else if (isSkateboardActive) { }
                            else
                            {
                                Skateboard.SetActive(true);
                                Samurai1Anim.DoScaledAnimationAsync("enter", 0.5f);
                                Skateboard1Anim.DoScaledAnimationAsync("enter", 0.5f);
                                isSkateboardActive = true;
                            }
                            break;
                        case Platform.Eagle:
                            if (isEagleActive) { }
                            else if (isSkateboardActive)
                            {
                                BGAnim.DoScaledAnimationAsync("up", 0.5f);
                                Eagle.SetActive(true);
                                Samurai1Anim.DoScaledAnimationAsync("enter2", 0.5f);
                                Skateboard1Anim.DoScaledAnimationAsync("exit", 0.5f);
                                Eagle1Anim.DoScaledAnimationAsync("enter", 0.5f);

                                if (mute) { }
                                else
                                {
                                    SoundByte.PlayOneShotGame("superSamuraiSlice/SE_IAI_NEW_BIRD_1");
                                }

                                isEagleActive = true;

                                BeatAction.New(instance, new List<BeatAction.Action>()
                                {
                                    new BeatAction.Action(beat + 1, delegate {
                                        Skateboard.SetActive(false);
                                    }),
                                });
                            }
                            else
                            {
                                BGAnim.DoScaledAnimationAsync("up", 0.5f);
                                Skateboard.SetActive(false);
                                Eagle.SetActive(true);
                                Samurai1Anim.DoScaledAnimationAsync("enter", 0.5f);
                                Eagle1Anim.DoScaledAnimationAsync("enter", 0.5f);
                                if (mute) { }
                                else
                                {
                                    SoundByte.PlayOneShotGame("superSamuraiSlice/SE_IAI_NEW_BIRD_1");
                                }
                                isSkateboardActive = true;
                                isEagleActive = true;
                            }
                            break;
                    }
                }
            }
        }

        int autoCount = 0;
        public int totalEvents = -1;
        int totalEvents2 = 0;
        public int whichDemon;
        public void SpawnDemon(double beat, bool auto, int count, bool autoBoom, float boomBeat, float flipback, bool enableBoom)
        {
            double inputBeat = beat + 2;

            if (lastBeat + 2 < beat)
            {
                autoCount = 0;
                whichInput = -1;
                totalEvents = -1;
                totalEvents2 = 0;
                slashType = 0;
                demonAnims = 0;
            }

            if (lastBeat + 3 < beat)
            {
                lastBeat = beat;

                demonEntities = gameManager.Beatmap.Entities.FindAll(e => e.datamodel == "superSamuraiSlice/smallDemon" && e.beat >= beat && e.beat < beat + 3);
                boomBeats = new List<double> { };
                enabledBooms = new List<bool> { };
                if (autoBoom)
                {
                    // this is kind of jank but have you got any better ideas
                    currentAutoBoom = true;
                    boomBeats = demonEntities.Count switch
                    {
                        1 => new List<double> { inputBeat + 1, },

                        2 when demonEntities[1].beat - demonEntities[0].beat < 1 =>
                            new List<double> { inputBeat + 1d, demonEntities[1].beat + 3d },

                        2 => new List<double> { inputBeat + 2d, demonEntities[1].beat + 4d },

                        3 => new List<double> { inputBeat + 2d, inputBeat + 2.5d, inputBeat + 3d },

                        4 => new List<double> { inputBeat + 2d, inputBeat + 2.25d, inputBeat + 2.5d, inputBeat + 3d },

                        _ => new List<double> { },
                    };
                    // failsafe
                    if (boomBeats.Count == 0)
                        autoBoom = false;
                }
                else currentAutoBoom = false;
            }
            enabledBooms.Add(enableBoom);

            if (!currentAutoBoom)
            {
                boomBeats.Add(inputBeat + boomBeat);
            }

            var demon = Instantiate(SmallDemon, transform).GetComponent<Demon>();

            totalEvents++;

            if (auto)
            {
                autoCount++;

                switch (autoCount)
                {
                    case 1:
                        PlaySFX(beat, 1);
                        demon.SetState(Demon.DemonState.RightUp, beat);
                        whichDemon = 1;
                        demon.Init(beat, 1);
                        break;
                    case 2:
                        PlaySFX(beat, 1);
                        demon.SetState(Demon.DemonState.LeftUp, beat);
                        whichDemon = 2;
                        demon.Init(beat, 1);

                        if (totalEvents2 < 1)
                        {
                            FlipSamurai(beat + 1, 1);
                            FlipSamurai(beat + 2 + flipback, 0);
                        }
                        else { FlipSamurai(beat + 1, 0); }
                        break;
                    case 3:
                        PlaySFX(beat, 2);
                        demon.SetState(Demon.DemonState.RightDown, beat);
                        whichDemon = 3;
                        demon.Init(beat, 1);
                        break;
                    case 4:
                        PlaySFX(beat, 2);
                        demon.SetState(Demon.DemonState.LeftDown, beat);
                        whichDemon = 4;
                        demon.Init(beat, 1);

                        if (totalEvents2 < 1)
                        {
                            FlipSamurai(beat + 1, 1);
                            FlipSamurai(beat + 2 + flipback, 0);
                        }
                        else { FlipSamurai(beat + 1, 0); }
                        break;
                }
            }
            else
            {
                switch (count)
                {
                    case 1:
                        PlaySFX(beat, 1);
                        demon.SetState(Demon.DemonState.RightUp, beat);
                        whichDemon = 1;
                        demon.Init(beat, 1);
                        break;
                    case 2:
                        PlaySFX(beat, 1);
                        demon.SetState(Demon.DemonState.LeftUp, beat);
                        whichDemon = 2;
                        demon.Init(beat, 1);

                        if (totalEvents2 < 1)
                        {
                            FlipSamurai(beat + 1, 1);
                            FlipSamurai(beat + 2 + flipback, 0);
                        }
                        else { FlipSamurai(beat + 1, 0); }
                        break;
                    case 3:
                        if (isWaterActive && !(isEagleActive || isRocketActive))
                        {
                            BeatAction.New(instance, new List<BeatAction.Action>()
                            {
                                new BeatAction.Action(beat + 1, delegate { waterR.Play(); }),
                            });
                        }

                        PlaySFX(beat, 2);
                        demon.SetState(Demon.DemonState.RightDown, beat);
                        whichDemon = 3;
                        demon.Init(beat, 1);
                        break;
                    case 4:
                        if (isWaterActive && !(isEagleActive || isRocketActive))
                        {
                            BeatAction.New(instance, new List<BeatAction.Action>()
                            {
                                new BeatAction.Action(beat + 1, delegate { waterL.Play(); }),
                            });
                        }

                        PlaySFX(beat, 2);
                        demon.SetState(Demon.DemonState.LeftDown, beat);
                        whichDemon = 4;
                        demon.Init(beat, 1);

                        if (totalEvents2 < 1)
                        {
                            FlipSamurai(beat + 1, 1);
                            FlipSamurai(beat + 2 + flipback, 0);
                        }
                        else { FlipSamurai(beat + 1, 0); }
                        break;
                }
            }
        }

        public float boombeat2;
        public bool enableBoom2;
        public void SpawnBigDemon(double beat, bool invert, bool autoBoom, float boomBeat, bool enableBoom)
        {
            boombeat2 = boomBeat; 
            enableBoom2 = enableBoom;
            if (isLargeDemonActive) { }
            else
            {
                var demon = Instantiate(MediumDemon, transform).GetComponent<Demon>();
                demon.Init(beat, 2);
                isLargeDemonActive = true;

                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("superSamuraiSlice/SE_IAI_NEW_ENEMY_MID_1", beat),
                    new MultiSound.Sound("superSamuraiSlice/SE_IAI_NEW_ENEMY_MID_2", beat + 0.5),
                    new MultiSound.Sound("superSamuraiSlice/SE_IAI_NEW_ENEMY_MID_3", beat + 1),
                }, forcePlay: true);

                if (invert)
                {
                    direction = 1;
                    demon.SetState(Demon.DemonState.Left, beat);

                    FlipSamurai(beat + 1, 1);
                    FlipSamurai(beat + 6, 0);
                }
                else
                {
                    direction = 0;
                    demon.SetState(Demon.DemonState.Right, beat);
                }
            }
        }

        public void PlaySFX(double beat, int sfx)
        {
            switch (sfx)
            {
                case 1:
                    SoundByte.PlayOneShotGame("superSamuraiSlice/SE_IAI_NEW_ENEMY_SMALL");
                    break;
                case 2:
                    if (!isWaterActive || (isEagleActive || isRocketActive))
                    {
                        MultiSound.Play(new MultiSound.Sound[]
                        {
                            new MultiSound.Sound("superSamuraiSlice/SE_IAI_NEW_ENEMY_SMALL", beat),
                        }, forcePlay: true);
                    }
                    else
                    {
                        MultiSound.Play(new MultiSound.Sound[]
                        {
                            new MultiSound.Sound("superSamuraiSlice/SE_IAI_NEW_ENEMY_SMALL", beat),
                            new MultiSound.Sound("superSamuraiSlice/SE_IAI_NEW_ENEMY_SMALL_WATER", beat + 1, UnityEngine.Random.Range(1f, 1.2f) ),
                        }, forcePlay: true);
                    }
                    break;
            }
        }

        public void FlipSamurai(double beat, int state)
        {
            switch (state)
            {
                case 0:
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat, delegate { Samurai.transform.localScale = new Vector3(4.2f, 4.2f, 1f); }),
                    });
                    break;
                case 1:
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat, delegate { Samurai.transform.localScale = new Vector3(-4.2f, 4.2f, 1f); }),
                    });
                    break;
            }
        }

        public void CanFlip(double beat)
        {
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 0.25, delegate { totalEvents2++; }),
            });
        }
        public void Bop(double beat, float length, bool auto, bool bop)
        {
            samuraiBop = auto;

            if (samuraiBop)
            {
                List<BeatAction.Action> actions = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                {
                    actions.Add(new BeatAction.Action(beat + i, delegate
                    {
                        SamuraiAnim.DoScaledAnimationAsync("Beat", 0.5f);
                    }));
                }
                BeatAction.New(instance, actions);
            }
        }

        public void ToggleFog(bool fog, bool water)
        {
            fogGO.SetActive(fog);
            waterGO.SetActive(water);
            isWaterActive = water;
        }
        const int IAAltDownCat = IAMAXCAT;
        const int IAAltUpCat = IAMAXCAT + 1;

        public static PlayerInput.InputAction InputAction_Press =
            new("CtrSamuraiPress", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_PadAnyDown, IA_TouchBasicPress, IA_BatonBasicPress);
        public static PlayerInput.InputAction InputAction_AltDown =
            new("CtrSamuraiAltDown", new int[] { IAAltDownCat, IAAltDownCat, IAAltDownCat },
            IA_PadAltDown, IA_TouchAltDown, IA_BatonAltDown);
        public static PlayerInput.InputAction InputAction_AltUp =
            new("CtrSamuraiAltUp", new int[] { IAAltUpCat, IAAltUpCat, IAAltUpCat },
            IA_PadAltUp, IA_TouchAltUp, IA_BatonAltUp);

        protected static bool IA_PadAnyDown(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.East, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Up, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Down, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Right, out dt);
        }

        protected static bool IA_PadAltDown(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltDown(out double dt)
        {
            return PlayerInput.GetSqueezeDown(out dt);
        }
        protected static bool IA_TouchAltDown(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && instance.IsExpectingInputNow(InputAction_AltDown);
        }

        protected static bool IA_PadAltUp(out double dt)
        {
            return PlayerInput.GetPadUp(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltUp(out double dt)
        {
            return PlayerInput.GetSqueezeUp(out dt);
        }
        protected static bool IA_TouchAltUp(out double dt)
        {
            return PlayerInput.GetFlick(out dt)
                && instance.IsExpectingInputNow(InputAction_AltUp);
        }

        new void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            foreach (SuperCurveObject.Path path in demonPaths)
            {
                if (path.preview)
                {
                    SmallDemon.DrawEditorGizmo(path);
                }
            }
        }
        public SuperCurveObject.Path GetPath(string name)
        {
            foreach (SuperCurveObject.Path path in demonPaths)
            {
                if (path.name == name)
                {
                    return path;
                }
            }
            return default(SuperCurveObject.Path);
        }
    }
}