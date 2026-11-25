using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class CtrTeppanLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            RiqEntity ClapUpdater(string datamodel, RiqEntity entity)
            {
                if (datamodel == "kitties/clap")
                {
                    if (entity["toggle2"] == null) return null;

                    entity.CreateProperty("despawn", entity["toggle2"] ? 2 : 1);

                    entity.dynamicData.Remove("toggle2");
                    return entity;
                }
                return null;
            }
            RiqBeatmap.OnUpdateEntity += ClapUpdater;

            RiqEntity RollUpdater(string datamodel, RiqEntity entity)
            {
                if (datamodel == "kitties/roll")
                {
                    if (entity["toggle"] == null) return null;

                    entity.CreateProperty("despawn", entity["toggle"] ? 2 : 1);

                    entity.dynamicData.Remove("toggle");
                    return entity;
                }
                return null;
            }
            RiqBeatmap.OnUpdateEntity += RollUpdater;

            return new Minigame("kitties", "Kitties!", "ffffff", false, false, new List<GameAction>()
            {
                new GameAction("clap", "Cat Clap")
                {
                    function = delegate { Kitties.instance.Clap(eventCaller.currentEntity["toggle"], eventCaller.currentEntity["toggle1"], eventCaller.currentEntity["despawn"],
                        eventCaller.currentEntity.beat, eventCaller.currentEntity["type"], eventCaller.currentEntity["end"]); },

                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("type", Kitties.SpawnType.Straight, "Direction", "Choose the direction that the kitties will spawn in."),
                        new Param("toggle", false, "Mice", "Toggle if the non-player kitties should be replaced with mice."),
                        new Param("toggle1", false, "Invert Direction", "Toggle if the spawn direction should be inverted."),
                        new Param("end", false, "The End", "Toggle if the \"The End\" text will display."),
                        new Param("despawn", Kitties.DespawningType.Auto, "Despawn Cats", "Change if the kitties automatically despawn, don't despawn, or are forced to despawn."),
                    }
                },

                new GameAction("roll", "Group Spin")
                    {
                        function = delegate { Kitties.instance.Roll(eventCaller.currentEntity["despawn"], eventCaller.currentEntity.beat);  },

                        defaultLength = 4f,

                        parameters = new List<Param>()
                        {
                            new Param("despawn", Kitties.DespawningType.Auto, "Despawn Cats", "Change if the kitties automatically despawn, don't despawn, or are forced to despawn."),
                        }
                    },

                new GameAction ("fish", "Fish")
                {
                    function = delegate { Kitties.instance.CatchFish(eventCaller.currentEntity["despawn"], eventCaller.currentEntity.beat); },
                    parameters = new List<Param>()
                    {
                        new Param("despawn", Kitties.DespawningType.Auto, "Despawn Cats", "Change if the kitties automatically despawn, don't despawn, or are forced to despawn."),
                    },
                    defaultLength = 4f,
                },

                new GameAction("instantSpawn", "Force Spawn")
                {
                    function = delegate { Kitties.instance.InstantSpawn(eventCaller.currentEntity["toggle"], eventCaller.currentEntity["toggle1"],
                        eventCaller.currentEntity.beat,  eventCaller.currentEntity["type"]); },

                    defaultLength = .5f,
                    parameters = new List<Param>()
                    {
                        new Param("type", Kitties.SpawnType.Straight, "Direction", "Choose the direction that the kitties will spawn in."),
                        new Param("toggle", false, "Mice", "Toggle if the non-player kitties should be replaced with mice."),
                        new Param("toggle1", false, "Invert Direction", "Toggle if the spawn direction should be inverted."),
                    },
                    advanced = true,
                },

                new GameAction("bgcolor", "Background Appearance")
                {
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        Kitties.instance.BackgroundColor(e.beat, e.length, e["colorStart"], e["colorEnd"], e["ease"]);
                    },
                    defaultLength = 4,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("colorStart", Color.white, "Start Color", "Set the color at the start of the event."),
                        new Param("colorEnd", Color.white, "End Color", "Set the color at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    }
                }
            },
            new List<string>() { "ctr", "normal" },
            "ctrteppan", "en",
            new List<string>() { },
            chronologicalSortKey: 13,
            font : "FOT-SlumpStd-DB SDF"
            );
        }
    }
}
namespace HeavenStudio.Games
{
    using Jukebox;
    using Scripts_Kitties;
    public class Kitties : Minigame
    {
        public CtrTeppanPlayer player;
        public Animator[] kitties;
        public GameObject[] Cats;

        public GameObject Fish;
        public GameObject TheEnd;
        public Animator TheEndAnim;
        public bool isInverted;
        public SpriteRenderer background;

        public Vector3[] positions;
        public Vector3 theEndPosition;
        public float[] rotationAngles;

        public enum SpawnType
        {
            Straight,
            DownDiagonal,
            UpDiagonal,
            CloseUp
        }

        public enum DespawningType
        {
            Auto,
            DespawnAfterCue,
            KeepSpawned
        }

        private List<RiqEntity> currentEntities;

        public static Kitties instance;

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
        protected static bool IA_TouchAltPress(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && instance.IsExpectingInputNow(InputAction_AltStart);
        }

        protected static bool IA_PadAltRelease(out double dt)
        {
            return PlayerInput.GetPadUp(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltRelease(out double dt)
        {
            return PlayerInput.GetSqueezeUp(out dt);
        }

        public static PlayerInput.InputAction InputAction_AltStart =
            new("CtrTeppanAltStart", new int[] { IAAltDownCat, IAAltDownCat, IAAltDownCat },
            IA_PadAltPress, IA_TouchAltPress, IA_BatonAltPress);
        public static PlayerInput.InputAction InputAction_AltFinish =
            new("CtrTeppanAltFinish", new int[] { IAAltUpCat, IAFlickCat, IAAltUpCat },
            IA_PadAltRelease, IA_TouchFlick, IA_BatonAltRelease);
        public static PlayerInput.InputAction InputAction_TouchRelease =
            new("CtrTeppanTouchRelease", new int[] { IAEmptyCat, IAReleaseCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicRelease, IA_Empty);

        void Awake()
        {
            instance = this;
            Fish.SetActive(false);
            TheEnd.SetActive(false);
            BackgroundColorUpdate();
        }

        // Update is called once per frame
        void Update()
        {
            BackgroundColorUpdate();
        }

        public void Clap(bool isMice, bool isInverse, int despawnType, double beat, int type, bool end)
        {
            Fish.SetActive(false);
            player.ScheduleClap(beat, type);
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound($"kitties/nya1_{UnityEngine.Random.Range(1, 3)}", beat),
                new MultiSound.Sound($"kitties/nya2_{UnityEngine.Random.Range(1, 3)}", beat + .75f),
                new MultiSound.Sound($"kitties/nya3_{UnityEngine.Random.Range(1, 3)}", beat + 1.5f),
                
                new MultiSound.Sound("kitties/CLAP_OTHER_FIRST", beat + 2.5f),
                new MultiSound.Sound("kitties/CLAP_OTHER_SECOND", beat + 3f)
            });

            if (type == 3)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { Spawn(type, 0, isMice, isInverse, true, false);}),
                    new BeatAction.Action(beat + 2.5f, delegate { kitties[0].DoScaledAnimationAsync("FaceClap", 0.5f);}),
                    new BeatAction.Action(beat + 3f, delegate { kitties[0].DoScaledAnimationAsync("FaceClap", 0.5f);}),
                });

                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + .75f, delegate { Spawn(type, 1, isMice, isInverse, false, false);}),
                    new BeatAction.Action(beat + 2.5f, delegate { kitties[1].DoScaledAnimationAsync("FaceClap", 0.5f);}),
                    new BeatAction.Action(beat + 3f, delegate { kitties[1].DoScaledAnimationAsync("FaceClap", 0.5f);}),
                });

                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 1.5f, delegate { Spawn(type, 2, isMice, isInverse, false, false);}),
                    new BeatAction.Action(beat + 1.5f, delegate { player.canClap = true;}),
                });
            }

            else if (!isMice)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { Spawn(type, 0, isMice, isInverse, true, false);}),
                    new BeatAction.Action(beat + 2.5f, delegate { kitties[0].DoScaledAnimationAsync("Clap1", 0.5f);}),
                    new BeatAction.Action(beat + 3f, delegate { kitties[0].DoScaledAnimationAsync("Clap2", 0.5f);}),
                });

                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + .75f, delegate { Spawn(type, 1, isMice, isInverse, false, false);}),
                    new BeatAction.Action(beat + 2.5f, delegate { kitties[1].DoScaledAnimationAsync("Clap1", 0.5f);}),
                    new BeatAction.Action(beat + 3f, delegate { kitties[1].DoScaledAnimationAsync("Clap2", 0.5f);}),
                });

                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 1.5f, delegate { Spawn(type, 2, isMice, isInverse, false, false);}),
                    new BeatAction.Action(beat + 1.5f, delegate { player.canClap = true;}),
                });
            }
            else
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { Spawn(type, 0, isMice, isInverse, true, false);}),
                    new BeatAction.Action(beat + 2.5f, delegate { kitties[0].DoScaledAnimationAsync("MiceClap1", 0.5f);}),
                    new BeatAction.Action(beat + 3f, delegate { kitties[0].DoScaledAnimationAsync("MiceClap2", 0.5f);}),
                });

                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + .75f, delegate { Spawn(type, 1, isMice, isInverse, false, false);}),
                    new BeatAction.Action(beat + 2.5f, delegate { kitties[1].DoScaledAnimationAsync("MiceClap1", 0.5f);}),
                    new BeatAction.Action(beat + 3f, delegate { kitties[1].DoScaledAnimationAsync("MiceClap2", 0.5f);}),
                });

                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 1.5f, delegate { Spawn(type, 2, isMice, isInverse, false, false);}),
                    new BeatAction.Action(beat + 1.5f, delegate { player.canClap = true;}),
                });

            }

            if (despawnType != (int)DespawningType.KeepSpawned)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 3.5f, delegate { RemoveCats(beat + 3.5f, false, despawnType);})
                });
            }

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 3f, delegate { 
                    TheEnd.SetActive(end);
                    TheEnd.transform.localPosition = theEndPosition;
                    if (TheEnd.activeSelf) TheEndAnim.DoScaledAnimationAsync("Japanese", 0.5f);  // the japanese edition shows おしまい
                })
            });
        }

        public void Roll(int despawnType, double beat)
        {
            if (!player.canClap)
                return;
            player.ScheduleRoll(beat);
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("kitties/roll1", beat, ignoreSwing: true),
                new MultiSound.Sound("kitties/roll2", beat + .5f, ignoreSwing: true),
                new MultiSound.Sound("kitties/roll3", beat + 1f, ignoreSwing: true),
                new MultiSound.Sound("kitties/roll4", beat + 1.5f, ignoreSwing: true),
                new MultiSound.Sound("kitties/spin1", beat + 2f, ignoreSwing: true),
                new MultiSound.Sound("kitties/spin2", beat + 2.075f, ignoreSwing: true),
                new MultiSound.Sound("kitties/spin3", beat + 2.15f, ignoreSwing: true),
                new MultiSound.Sound("kitties/spin4", beat + 2.225f, ignoreSwing: true),
                new MultiSound.Sound("kitties/spin5", beat + 2.3f, ignoreSwing: true),
                new MultiSound.Sound("kitties/spin6", beat + 2.375f, ignoreSwing: true),
                new MultiSound.Sound("kitties/spin7", beat + 2.45f, ignoreSwing: true),
                new MultiSound.Sound("kitties/spin8", beat + 2.525f, ignoreSwing: true),
                new MultiSound.Sound("kitties/spin9", beat + 2.6f, ignoreSwing: true),
                new MultiSound.Sound("kitties/spin10", beat + 2.675f, ignoreSwing: true),

            });
            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { kitties[0].DoScaledAnimationAsync("RollStart", 0.5f); }),
                    new BeatAction.Action(beat + .5f, delegate { kitties[0].DoScaledAnimationAsync("RollStart", 0.5f); }),
                    new BeatAction.Action(beat + 1f, delegate { kitties[0].DoScaledAnimationAsync("RollStart", 0.5f); }),
                    new BeatAction.Action(beat + 1.5f, delegate { kitties[0].DoScaledAnimationAsync("RollStart", 0.5f); }),
                    new BeatAction.Action(beat + 2f, delegate { kitties[0].DoScaledAnimationAsync("Rolling", 0.5f); }),
                    new BeatAction.Action(beat + 2.75f, delegate { kitties[0].DoScaledAnimationAsync("RollEnd", 0.5f); })
                });

            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { kitties[1].DoScaledAnimationAsync("RollStart", 0.5f); }),
                    new BeatAction.Action(beat + .5f, delegate { kitties[1].DoScaledAnimationAsync("RollStart", 0.5f); }),
                    new BeatAction.Action(beat + 1f, delegate { kitties[1].DoScaledAnimationAsync("RollStart", 0.5f); }),
                    new BeatAction.Action(beat + 1.5f, delegate { kitties[1].DoScaledAnimationAsync("RollStart", 0.5f); }),
                    new BeatAction.Action(beat + 2f, delegate { kitties[1].DoScaledAnimationAsync("Rolling", 0.5f); }),
                    new BeatAction.Action(beat + 2.75f, delegate { kitties[1].DoScaledAnimationAsync("RollEnd", 0.5f); })
                    });

            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { kitties[2].DoScaledAnimationAsync("RollStart", 0.5f); }),
                    new BeatAction.Action(beat + .5f, delegate { kitties[2].DoScaledAnimationAsync("RollStart", 0.5f); }),
                    new BeatAction.Action(beat + 1f, delegate { kitties[2].DoScaledAnimationAsync("RollStart", 0.5f); }),
                    new BeatAction.Action(beat + 1.5f, delegate { kitties[2].DoScaledAnimationAsync("RollStart", 0.5f); })
                });

            if (despawnType != (int)DespawningType.KeepSpawned)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 3.5f, delegate { RemoveCats(beat + 3.5f, false, despawnType);})
                });
            }
        }

        public void CatchFish(int despawnType, double beat)
        {
            //if (!player.canClap)
            //    return;

            player.ScheduleFish(beat);
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("kitties/fish1", beat + 2f),
                new MultiSound.Sound("kitties/fish2", beat + 2.25f),
                new MultiSound.Sound("kitties/fish3", beat + 2.5f),

            });

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { if (isInverted)
                    Fish.transform.localScale = new Vector3(-1f, 1f, 1f);
                    else
                        Fish.transform.localScale = new Vector3(1f, 1f, 1f); }),
                new BeatAction.Action(beat, delegate { Fish.SetActive(true); }),
                new BeatAction.Action(beat, delegate { Fish.GetComponent<Animator>().DoScaledAnimationAsync("FishDangle", 0.5f); }),
                new BeatAction.Action(beat + 2f, delegate { kitties[0].DoScaledAnimationAsync("FishNotice", 0.5f);  }),
                new BeatAction.Action(beat + 2.25f, delegate { kitties[1].DoScaledAnimationAsync("FishNotice2", 0.5f);  }),
                new BeatAction.Action(beat + 2.5f, delegate { kitties[2].DoScaledAnimationAsync("FishNotice3", 0.5f);  })
                });

            if (despawnType != (int)DespawningType.KeepSpawned)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 4f, delegate { RemoveCats(beat + 3.5f, true, despawnType);})
                });
            }
        }

        public void Spawn(int pos, int catNum, bool isMice, bool isInverse, bool firstSpawn, bool instant)
        {
            ResetRotation(catNum);
            if (firstSpawn)
            {
                isInverted = isInverse;
                switch ((SpawnType)pos)
                {
                    case SpawnType.Straight:

                        if (!isInverse)
                        {
                            positions[0] = new Vector3(-5.11f, -1.25f, 0f);
                            positions[1] = new Vector3(.32f, -1.25f, 0f);
                            positions[2] = new Vector3(5.75f, -1.25f, 0f);
                        }
                        else
                        {
                            positions[0] = new Vector3(5.75f, -1.25f, 0f);
                            positions[1] = new Vector3(.32f, -1.25f, 0f);
                            positions[2] = new Vector3(-5.11f, -1.5f, 0f);
                        }
                        theEndPosition = new Vector3(0f, 3.12f, 0f);
                        break;

                    case SpawnType.DownDiagonal:

                        if (!isInverse)
                        {
                            positions[0] = new Vector3(-6.61f, 1.75f, 6f);
                            positions[1] = new Vector3(.32f, -.25f, 2f);
                            positions[2] = new Vector3(4.25f, -1.75f, -2f);
                            theEndPosition = new Vector3(-2.95f, -3.77f, 0f);
                        }
                        else
                        {
                            positions[0] = new Vector3(6.61f, 1.75f, 6f);
                            positions[1] = new Vector3(.32f, -.25f, 2f);
                            positions[2] = new Vector3(-4.25f, -1.75f, -2f);
                            theEndPosition = new Vector3(3.35f, -3.77f, 0f);
                        }
                        break;

                    case SpawnType.UpDiagonal:

                        if (!isInverse)
                        {
                            positions[0] = new Vector3(4.25f, -1.75f, -2f);
                            positions[1] = new Vector3(.32f, -.25f, 2f);
                            positions[2] = new Vector3(-6.61f, 1.75f, 6f);
                            theEndPosition = new Vector3(-2.95f, -3.77f, 0f);
                        }
                        else
                        {
                            positions[0] = new Vector3(-4.25f, -1.75f, -2f);
                            positions[1] = new Vector3(.32f, -.25f, 2f);
                            positions[2] = new Vector3(6.61f, 1.75f, 6f);
                            theEndPosition = new Vector3(3.35f, -3.77f, 0f);
                        }
                        break;

                    case SpawnType.CloseUp:
                        if (firstSpawn)
                        {
                            rotationAngles = new float[] { -135f, 135f, 0f };
                            positions[0] = new Vector3(-8.21f, 3.7f, 0f);
                            positions[1] = new Vector3(7.51f, 4.2f, 0f);
                            positions[2] = new Vector3(.32f, -4.25f, 0f);
                            theEndPosition = Vector3.zero;
                        }
                        break;


                    default:
                        break;
                }
            }
            Cats[catNum].transform.position = positions[catNum];
            if (pos != 3)
            {
                if (!isInverse)
                    Cats[catNum].transform.localScale = new Vector3(1f, 1f, 1f);
                else
                    Cats[catNum].transform.localScale = new Vector3(-1f, 1f, 1f);
            }

            else
            {
                var rotationVector = Cats[catNum].transform.rotation.eulerAngles;
                rotationVector.z = rotationAngles[catNum];
                Cats[catNum].transform.rotation = Quaternion.Euler(rotationVector);
                Cats[catNum].transform.localScale = new Vector3(-1f, 1f, 1f);
            }

            Cats[catNum].transform.GetChild(0).gameObject.SetActive(true);

            if (!instant)
            {
                if (pos == 3)
                {
                    kitties[catNum].DoScaledAnimationAsync("FacePopIn", 0.5f);
                }
                else if (!isMice)
                    kitties[catNum].DoScaledAnimationAsync("PopIn", 0.5f);
                else if (catNum < 2)
                {
                    kitties[catNum].DoScaledAnimationAsync("MicePopIn", 0.5f);
                }
                else
                    kitties[catNum].DoScaledAnimationAsync("PopIn", 0.5f);
            }
            else
            {
                if (pos == 3)
                    kitties[catNum].DoScaledAnimationAsync("FaceIdle", 0.5f);
                else if (!isMice)
                    kitties[catNum].DoScaledAnimationAsync("Idle", 0.5f);
                else if (catNum < 2)
                    kitties[catNum].DoScaledAnimationAsync("MiceIdle", 0.5f);
                else
                    kitties[catNum].DoScaledAnimationAsync("Idle", 0.5f);
            }
        }

        public void ResetRotation(int catNum)
        {
            var rotationVector = Cats[0].transform.rotation.eulerAngles;
            rotationVector.z = 0;
            Cats[catNum].transform.rotation = Quaternion.Euler(rotationVector);
        }

        public void RemoveCats(double beat, bool fishing, int despawnType)
        {
            if (despawnType == (int)DespawningType.KeepSpawned) return;
            if (despawnType == (int)DespawningType.Auto && beat > 0)
            {
                if (NextEntity(beat) is "kitties/roll" or "kitties/fish" or null) return;
            }

            if (!fishing)
            {
                for (int x = 0; x < 3; x++)
                {
                    Cats[x].transform.GetChild(0).gameObject.SetActive(false);
                }
            }
            else
            {
                Fish.SetActive(false);
            }

            TheEnd.SetActive(false);
            player.canClap = false;
        }

        public void InstantSpawn(bool isMice, bool isInverse, double beat, int pos)
        {
            Spawn(pos, 0, isMice, isInverse, true, true);
            Spawn(pos, 1, isMice, isInverse, true, true);
            Spawn(pos, 2, isMice, isInverse, true, true);
            player.canClap = true;
        }

        public void GetCurrentEntities(double beat)
        {
            List<RiqEntity> entitiesList;
            double currentGameSwitchBeat = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame", "end" }).Find(x => x.beat <= beat)?.beat ?? 0;
            double nextGameSwitchBeat = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame", "end" }).Find(x => x.beat > beat)?.beat ?? double.MaxValue;
            entitiesList = EventCaller.GetAllInGameManagerList("kitties").FindAll(e => e.beat >= currentGameSwitchBeat && nextGameSwitchBeat > e.beat);
            currentEntities = entitiesList;
        }

        public string NextEntity(double beat)
        {
            return currentEntities.Find(e => e.beat >= beat)?.datamodel;
        }

        public RiqEntity PreviousEntity(double beat)
        {
            return currentEntities.FindLast(e => e.beat < beat && e.datamodel == "kitties/clap");
        }

        private ColorEase bgColorEase = new(Color.white);

        //call this in update
        private void BackgroundColorUpdate()
        {
            background.color = bgColorEase.GetColor();
        }

        public void BackgroundColor(double beat, float length, Color colorStartSet, Color colorEndSet, int ease)
        {
            bgColorEase = new ColorEase(beat, length, colorStartSet, colorEndSet, ease);
        }

        //call this in OnPlay(double beat) and OnGameSwitch(double beat)
        private void PersistColor(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("kitties", new string[] { "bgcolor" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                BackgroundColor(lastEvent.beat, lastEvent.length, lastEvent["colorStart"], lastEvent["colorEnd"], lastEvent["ease"]);
            }
        }

        public override void OnPlay(double beat)
        {
            OnGameSwitch(beat);
        }

        public override void OnGameSwitch(double beat)
        {
            PersistColor(beat);
            GetCurrentEntities(beat);
            if (NextEntity(beat) is "kitties/roll" or "kitties/fish")
            {
                RiqEntity e = PreviousEntity(beat);
                if (e != null)
                {
                    InstantSpawn(e["toggle"], e["toggle1"], beat,  e["type"]); 
                }
                else
                {
                    InstantSpawn(false, false, beat, (int)SpawnType.Straight);
                }
            }
        }
    }
}