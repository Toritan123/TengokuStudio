using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbGhostLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("sneakySpirits", "Sneaky Spirits", "5a5a9c", false, false, new List<GameAction>()
            {
                new GameAction("simpleSpawnGhost", "Ghost")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; SneakySpirits.PreSpawnGhost(e.beat, 1, e["volume1"], e["volume2"], e["volume3"], e["volume4"], e["volume5"], e["volume6"],
                        e["volume7"]); },
                    defaultLength = 8f,
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        SneakySpirits.instance.SpawnGhost(e.beat, e.beat, 1, e["slowDown"], new List<int>()
                        {
                            e["volume1"], e["volume2"], e["volume3"], e["volume4"], e["volume5"], e["volume6"],
                        e["volume7"]
                        });
                    },
                    parameters = new List<Param>()
                    {
                        new Param("slowDown", false, "Slowdown Effect", "Toggle if there should be a slowdown effect when the ghost is hit."),
                        new Param("volume1", new EntityTypes.Integer(0, 100, 100), "Move Volume 1", "Set the height and volume the ghost should have at this position."),
                        new Param("volume2", new EntityTypes.Integer(0, 100, 100), "Move Volume 2", "Set the height and volume the ghost should have at this position."),
                        new Param("volume3", new EntityTypes.Integer(0, 100, 100), "Move Volume 3", "Set the height and volume the ghost should have at this position."),
                        new Param("volume4", new EntityTypes.Integer(0, 100, 100), "Move Volume 4", "Set the height and volume the ghost should have at this position."),
                        new Param("volume5", new EntityTypes.Integer(0, 100, 100), "Move Volume 5", "Set the height and volume the ghost should have at this position."),
                        new Param("volume6", new EntityTypes.Integer(0, 100, 100), "Move Volume 6", "Set the height and volume the ghost should have at this position."),
                        new Param("volume7", new EntityTypes.Integer(0, 100, 100), "Move Volume 7", "Set the height and volume the ghost should have at this position."),
                    }
                },
                new GameAction("spawnGhost", "Ghost (Stretchy)")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; SneakySpirits.PreSpawnGhost(e.beat, e.length, e["volume1"], e["volume2"], e["volume3"], e["volume4"], e["volume5"], e["volume6"],
                        e["volume7"]); },
                    defaultLength = 1f,
                    resizable = true,
                    function = delegate 
                    { 
                        var e = eventCaller.currentEntity; 
                        SneakySpirits.instance.SpawnGhost(e.beat, e.beat, e.length, e["slowDown"], new List<int>()
                        {
                            e["volume1"], e["volume2"], e["volume3"], e["volume4"], e["volume5"], e["volume6"],
                        e["volume7"]
                        }); 
                    },
                    parameters = new List<Param>()
                    {
                        new Param("slowDown", false, "Slowdown Effect", "Toggle if there should be a slowdown effect when the ghost is hit."),
                        new Param("volume1", new EntityTypes.Integer(0, 100, 100), "Move Volume 1", "Set the height and volume the ghost should have at this position."),
                        new Param("volume2", new EntityTypes.Integer(0, 100, 100), "Move Volume 2", "Set the height and volume the ghost should have at this position."),
                        new Param("volume3", new EntityTypes.Integer(0, 100, 100), "Move Volume 3", "Set the height and volume the ghost should have at this position."),
                        new Param("volume4", new EntityTypes.Integer(0, 100, 100), "Move Volume 4", "Set the height and volume the ghost should have at this position."),
                        new Param("volume5", new EntityTypes.Integer(0, 100, 100), "Move Volume 5", "Set the height and volume the ghost should have at this position."),
                        new Param("volume6", new EntityTypes.Integer(0, 100, 100), "Move Volume 6", "Set the height and volume the ghost should have at this position."),
                        new Param("volume7", new EntityTypes.Integer(0, 100, 100), "Move Volume 7", "Set the height and volume the ghost should have at this position."),
                    },
                    advanced = true
                },
                new GameAction("fakeGhost", "Fake Ghost Appear")
                {
                    defaultLength = 1f,
                    resizable = true,
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        SneakySpirits.instance.FakeGhost(e.beat, e.length, e["position"], e["volume"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("position", new EntityTypes.Integer(1, 7, 1), "Ghost Position", "Set the position along the wall the ghost should have."),
                        new Param("volume", new EntityTypes.Integer(0, 100, 100), "Ghost Volume", "Set the height and volume the ghost should have at this position."),
                    },
                    advanced = true
                },
                new GameAction("movebow", "Bow Enter or Exit")
                {
                    function = delegate {var e = eventCaller.currentEntity; SneakySpirits.instance.MoveBow(e.beat, e.length, e["exit"], e["ease"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("exit", true, "Enter", "Toggle if the bow should enter or exit the scene."),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    }
                },
                new GameAction("forceReload", "Bow Force Reload")
                {
                    function = delegate { SneakySpirits.instance.ForceReload(); },
                    defaultLength = 1f,
                    advanced = true
                },
            },
            new List<string>() {"agb", "aim"},
            "agbghost", "en",
            new List<string>() {},
            chronologicalSortKey: 7,
            font : "FOT-SlumpStd-DB SDF"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_SneakySpirits;
    public class SneakySpirits : Minigame
    {
        [Header("Components")]
        [SerializeField] Animator bowAnim;
        [SerializeField] Animator bowHolderAnim;
        [SerializeField] Animator doorAnim;
        [SerializeField] SneakySpiritsGhost movingGhostPrefab;
        [SerializeField] SneakySpiritsGhostDeath deathGhostPrefab;
        [SerializeField] GameObject arrowMissPrefab;
        [SerializeField] GameObject ghostMissPrefab;
        [SerializeField] List<Transform> ghostPositions = new List<Transform>();
        [SerializeField] GameObject normalRain;
        [SerializeField] GameObject slowRain;
        [SerializeField] GameObject normalTree;
        [SerializeField] GameObject slowTree;

        private bool hasArrowLoaded;
        private bool hasArrowDrawn;
        float movingLength;
        double movingStartBeat;
        bool isMoving;
        string moveAnim;
        EasingFunction.Ease lastEase;

        public static SneakySpirits instance;

        void OnDestroy()
        {
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
            if (Conductor.instance.isPlaying)
            {
                Conductor.instance.SetMinigamePitch(1f);
            }
        }

        public override void OnGameSwitch(double beat)
        {
            InitGhosts(beat);
            Conductor.instance.SetMinigamePitch(1f);
        }

        private void InitGhosts(double beat)
        {
            var allGhosts = EventCaller.GetAllInGameManagerList("sneakySpirits", new string[] { "spawnGhost" });
            var allSimpleGhosts = EventCaller.GetAllInGameManagerList("sneakySpirits", new string[] { "simpleSpawnGhost" });

            foreach (var ghost in allGhosts)
            {
                if (ghost.beat < beat && ghost.beat + (ghost.length * 7) >= beat)
                {
                    SpawnGhost(ghost.beat, beat, ghost.length, ghost["slowDown"], new List<int>()
                    {
                        ghost["volume1"], ghost["volume2"], ghost["volume3"], ghost["volume4"], ghost["volume5"], ghost["volume6"],
                        ghost["volume7"],
                    });
                }
            }

            foreach (var ghost in allSimpleGhosts)
            {
                if (ghost.beat < beat && ghost.beat + 7 >= beat)
                {
                    SpawnGhost(ghost.beat, beat, 1, ghost["slowDown"], new List<int>()
                    {
                        ghost["volume1"], ghost["volume2"], ghost["volume3"], ghost["volume4"], ghost["volume5"], ghost["volume6"],
                        ghost["volume7"],
                    });
                }
            }
        }

        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch)
                {
                    if (PlayerInput.GetIsAction(InputAction_BasicPress) && hasArrowLoaded && !hasArrowDrawn)
                    {
                        hasArrowDrawn = true;
                        bowAnim.DoScaledAnimationAsync("BowDraw", 0.25f);
                    }
                    if (PlayerInput.GetIsAction(InputAction_BasicRelease) && hasArrowLoaded && hasArrowDrawn)
                    {
                        hasArrowDrawn = false;
                        bowAnim.DoScaledAnimationAsync("BowRelease", 0.5f);
                    }
                }
                if (PlayerInput.GetIsAction(InputAction_FlickPress) && !IsExpectingInputNow(InputAction_FlickPress) && hasArrowLoaded)
                {
                    WhiffArrow(cond.songPositionInBeatsAsDouble);
                    hasArrowDrawn = false;
                }
                if (isMoving)
                {
                    float normalizedBeat = cond.GetPositionFromBeat(movingStartBeat, movingLength);
                    EasingFunction.Function func = EasingFunction.GetEasingFunction(lastEase);
                    float newPos = func(0f, 1f, normalizedBeat);
                    bowHolderAnim.DoNormalizedAnimation(moveAnim, newPos);
                    if (normalizedBeat >= 1f)
                    {
                        isMoving = false;
                    }
                }
            }
        }

        public void ForceReload()
        {
            if (hasArrowLoaded) return;
            if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch && !GameManager.instance.Autoplay)
            {
                bowAnim.DoScaledAnimationAsync("BowIdle", 1f);
            }
            else
            {
                bowAnim.DoScaledAnimationAsync("BowDraw", 0.25f);
            }
            hasArrowLoaded = true;
            hasArrowDrawn = false;
        }

        public void MoveBow(double beat, float length, bool enter, int ease)
        {
            movingStartBeat = beat;
            movingLength = length;
            moveAnim = enter ? "Enter" : "Exit";
            isMoving = true;
            lastEase = (EasingFunction.Ease)ease;
        }

        public static void PreSpawnGhost(double beat, float length, int volume1, int volume2, int volume3, int volume4, int volume5, int volume6, int volume7)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("sneakySpirits/moving", beat, 1f, volume1 * 0.01f, false, 0.019),
                new MultiSound.Sound("sneakySpirits/moving", beat + length, 1f, volume2 * 0.01f, false, 0.019),
                new MultiSound.Sound("sneakySpirits/moving", beat + length * 2, 1f, volume3 * 0.01f, false, 0.019),
                new MultiSound.Sound("sneakySpirits/moving", beat + length * 3, 1f, volume4 * 0.01f, false, 0.019),
                new MultiSound.Sound("sneakySpirits/moving", beat + length * 4, 1f, volume5 * 0.01f, false, 0.019),
                new MultiSound.Sound("sneakySpirits/moving", beat + length * 5, 1f, volume6 * 0.01f, false, 0.019),
                new MultiSound.Sound("sneakySpirits/moving", beat + length * 6, 1f, volume7 * 0.01f, false, 0.019),
            }, forcePlay: true);
        }

        public void SpawnGhost(double beat, double gameSwitchBeat, float length, bool slowDown, List<int> volumes)
        {
            if (slowDown)
            {
                ScheduleInput(beat, length * 7, InputAction_FlickPress, Just, Miss, Out);
            }
            else
            {
                ScheduleInput(beat, length * 7, InputAction_FlickPress, JustNoSlowDown, Miss, Out);
            }
            if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch && !GameManager.instance.Autoplay)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { ForceReload(); })
                });
            }
            else
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length * 3, delegate { ForceReload(); })
                });
            }

            List<BeatAction.Action> ghostSpawns = new List<BeatAction.Action>();
            for(int i = 0; i < 7; i++)
            {
                double spawnBeat = beat + length * i;
                if (spawnBeat >= gameSwitchBeat)
                {
                    SneakySpiritsGhost spawnedGhost = Instantiate(movingGhostPrefab, ghostPositions[i], false);
                    spawnedGhost.transform.position = new Vector3(spawnedGhost.transform.position.x, spawnedGhost.transform.position.y - (1 - volumes[i] * 0.01f) * 2.5f, spawnedGhost.transform.position.z);
                    spawnedGhost.Init(spawnBeat, length);
                }
            }
        }

        public void FakeGhost(double beat, float length, int position, int volume)
        {
            SoundByte.PlayOneShotGame("sneakySpirits/moving", beat, 1f, volume * 0.01f, false, false, 0.019);
            SneakySpiritsGhost spawnedGhost = Instantiate(movingGhostPrefab, ghostPositions[position - 1], false);
            spawnedGhost.transform.position = new Vector3(spawnedGhost.transform.position.x, spawnedGhost.transform.position.y - (1 - volume * 0.01f) * 2.5f, spawnedGhost.transform.position.z);
            spawnedGhost.Init(beat, length);
        }

        void WhiffArrow(double beat)
        {
            GameObject spawnedArrow = Instantiate(arrowMissPrefab, transform);
            spawnedArrow.SetActive(true);
            spawnedArrow.GetComponent<Animator>().DoScaledAnimationAsync("ArrowRecoil", 0.5f);
            bowAnim.DoScaledAnimationAsync("BowRecoil", 0.25f);
            hasArrowLoaded = false;
            SoundByte.PlayOneShotGame("sneakySpirits/arrowMiss", -1, 2);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 3f, delegate { 
                    if (GameManager.instance.currentGame == "sneakySpirits") 
                    {
                        Destroy(spawnedArrow);
                    } 
                })
            });
        }

        void Just(PlayerActionEvent caller, float state)
        {
            if (!hasArrowLoaded) return;
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("sneakySpirits/ghostScared");
                WhiffArrow(caller.startBeat + caller.timer);
                GameObject spawnedGhost = Instantiate(ghostMissPrefab, transform);
                spawnedGhost.SetActive(true);
                spawnedGhost.GetComponent<Animator>().DoScaledAnimationAsync("GhostBarely", 0.5f);
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.startBeat + caller.timer + 2f, delegate {
                        if (GameManager.instance.currentGame == "sneakySpirits")
                        {
                            Destroy(spawnedGhost);
                        }
                    })
                });
                return;
            }
            Success(caller, true);
        }
        
        void JustNoSlowDown(PlayerActionEvent caller, float state)
        {
            if (!hasArrowLoaded) return;
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("sneakySpirits/ghostScared");
                WhiffArrow(caller.startBeat + caller.timer);
                GameObject spawnedGhost = Instantiate(ghostMissPrefab, transform);
                spawnedGhost.SetActive(true);
                spawnedGhost.GetComponent<Animator>().DoScaledAnimationAsync("GhostBarely", 0.5f);
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.startBeat + caller.timer + 2f, delegate {
                        if (GameManager.instance.currentGame == "sneakySpirits")
                        {
                            Destroy(spawnedGhost);
                        }
                    })
                });
                return;
            }
            Success(caller, false);
        }

        void Success(PlayerActionEvent caller, bool slowDown)
        {
            SneakySpiritsGhostDeath spawnedDeath = Instantiate(deathGhostPrefab, transform, false);
            int randomNumber = UnityEngine.Random.Range(0, 4);

            switch (randomNumber)
            {
                case 0:
                    spawnedDeath.animToPlay = "GhostDieNose";
                    break;
                case 1:
                    spawnedDeath.animToPlay = "GhostDieMouth";
                    break;
                case 2:
                    spawnedDeath.animToPlay = "GhostDieBody";
                    break;
                case 3:
                    spawnedDeath.animToPlay = "GhostDieCheek";
                    break;
            }
            hasArrowLoaded = false;
            spawnedDeath.startBeat = caller.startBeat + caller.timer;
            spawnedDeath.length = 1f;
            spawnedDeath.gameObject.SetActive(true);
            SoundByte.PlayOneShotGame("sneakySpirits/hit");
            bowAnim.DoScaledAnimationAsync("BowRecoil", 0.25f);
            if (slowDown) 
            {
                slowRain.SetActive(true);
                normalRain.SetActive(false);
                slowTree.SetActive(true);
                normalTree.SetActive(false);
                Conductor.instance.SetMinigamePitch(0.25f);
            }

            doorAnim.DoScaledAnimationAsync("DoorOpen", 0.5f);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer + 1f, delegate 
                { 
                    Conductor.instance.SetMinigamePitch(1f);
                    doorAnim.DoScaledAnimationAsync("DoorClose", 0.5f);
                    slowRain.SetActive(false);
                    normalRain.SetActive(true);
                    slowTree.SetActive(false);
                    normalTree.SetActive(true);
                })
            });
        }

        void Miss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("sneakySpirits/ghostEscape");
            GameObject spawnedGhost = Instantiate(ghostMissPrefab, transform);
            spawnedGhost.SetActive(true);
            spawnedGhost.GetComponent<Animator>().DoScaledAnimationAsync("GhostMiss", 0.5f);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer + 1f, delegate {
                    if (GameManager.instance.currentGame == "sneakySpirits")
                    {
                        SoundByte.PlayOneShotGame("sneakySpirits/laugh", -1, 1f);
                        spawnedGhost.GetComponent<Animator>().DoScaledAnimationAsync("GhostLaugh", 0.25f);
                    }
                }),
                new BeatAction.Action(caller.startBeat + caller.timer + 2.5f, delegate {
                    if (GameManager.instance.currentGame == "sneakySpirits")
                    {
                        Destroy(spawnedGhost);
                    }
                })
            });
        }

        void Out(PlayerActionEvent caller)
        {

        }
    }
}
