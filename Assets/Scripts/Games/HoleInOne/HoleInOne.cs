using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlHoleInOneLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("holeInOne", "Hole in One", "6ab99e", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        HoleInOne.instance.ToggleBop(e.beat, e.length, e["bop"], e["autoBop"], e["mandrillBopType"]);
                    },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop", "Toggle if the characters should bop for the duration of this event."),
                        new Param("autoBop", false, "Bop (Auto)", "Toggle if the characters should automatically bop until another Bop event is reached."),
                        new Param("mandrillBopType", HoleInOne.MandrillBopType.Bop1, "Mandrill Bop Type", "Sets the bop animation for the Mandrill.")
                    }
                },
                new GameAction("monkey", "Monkey")
                {
                    function = delegate { HoleInOne.instance.DoMonkey(eventCaller.currentEntity.beat); },
                    defaultLength = 3.0f,
                },
                new GameAction("mandrill", "Mandrill")
                {
                    function = delegate { HoleInOne.instance.DoMandrill(eventCaller.currentEntity.beat); },
                    defaultLength = 4.0f,
                },
                new GameAction("whale", "Whale")
                {
                    function = delegate { var e = eventCaller.currentEntity;  HoleInOne.instance.Whale(e.beat, e.length, e["ease"], e["appear"]); },
                    parameters = new List<Param>()
                    {
                        new Param("appear", true, "Enter", "Toggle if the whale should enter or exit the scene."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    },
                    resizable = true
                },
            },
            new List<string>() { "rvl", "normal" },
            "rvlgolf", "en",
            new List<string>() {},
            chronologicalSortKey: 1,
            font : "FOT-SlumpStd-DB SDF"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_HoleInOne;
    public class HoleInOne : Minigame
    {
        [SerializeField] GameObject baseBall;

        public Animator MonkeyAnim;
        public Animator MonkeyHeadAnim;
        public Animator MandrillAnim;
        public Animator GolferAnim;
        public GameObject Hole;
        public Animator HoleAnim;
        public Animator GrassEffectAnim;
        public Animator BallEffectAnim;
        [SerializeField] private GameObject grassEffectPrefab;
        [SerializeField] private RectTransform grassArea;
        private int _currentMandrillBopType = 0;
        private double _lastMonkeyThrowBeat = -1;
        public enum MandrillBopType
        {
            Bop1,
            Bop2,
            Bop3,
        }

        double whaleStartBeat;
        float whaleLength;
        Util.EasingFunction.Ease lastEase;
        public bool isWhale { get; private set; }
        public GameEvent bop = new GameEvent();
        public bool canBop = true;

        public static HoleInOne instance;

        public static PlayerInput.InputAction InputAction_TouchPress =
            new("RvlGolfTouchPress", new int[] { IAEmptyCat, IAPressCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicPress, IA_Empty);
        public static PlayerInput.InputAction InputAction_TouchPressing =
            new("RvlGolfTouchPressing", new int[] { IAEmptyCat, IAPressCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicPressing, IA_Empty);
        public static PlayerInput.InputAction InputAction_TouchRelease =
            new("RvlGolfTouchRelease", new int[] { IAEmptyCat, IAReleaseCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicRelease, IA_Empty);

        void Awake()
        {
            HoleInOne.instance = this;
            SetupBopRegion("holeInOne", "bop", "autoBop");
            isWhale = false;
            Hole.SetActive(false);
        }

        public override void OnBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat)) Bop(beat);
        }

        void Update()
        {
            if (!conductor.isPlaying || conductor.isPaused) return;

            string[] uninterruptibleAnims = { "GolferWhiff", "GolferJust", "GolferThroughMandrill" };

            if (PlayerInput.GetIsAction(InputAction_FlickPress) && !IsExpectingInputNow(InputAction_FlickPress) && !GolferAnim.IsPlayingAnimationNames(uninterruptibleAnims))
            {
                SoundByte.PlayOneShotGame("holeInOne/SE_GOLF_SWING");
               GolferAnim.DoScaledAnimationAsync("GolferWhiff", 1f);
            }

            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !(PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch) && !IsExpectingInputNow(InputAction_FlickPress) && !GolferAnim.IsPlayingAnimationNames(uninterruptibleAnims))
            {
                SoundByte.PlayOneShotGame("holeInOne/SE_GOLF_SWING");
                GolferAnim.Play("GolferWhiff", 0, 0);
            }

            if (PlayerInput.GetIsAction(InputAction_TouchPress) && !GameManager.instance.Autoplay)
            {
                GolferAnim.DoScaledAnimationAsync("GolferPrepare", 1f);
            }
            if (PlayerInput.GetIsAction(InputAction_TouchRelease) && (!IsExpectingInputNow(InputAction_FlickPress)) && (!GameManager.instance.Autoplay))
            {
                GolferAnim.DoScaledAnimationAsync("GolferBop", 1f);
            }

            UpdateWhale(conductor.unswungSongPositionInBeatsAsDouble);
        }

        public void ToggleBop(double beat, float length, bool shouldBop, bool autoBop, int mandrillBopType)
        {
            if (shouldBop)
            {
                _currentMandrillBopType = mandrillBopType;

                for (int i = 0; i < length; i++)
                {
                    var currentBeat = beat + i;
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(currentBeat, delegate { Bop(currentBeat); })
                    });
                }
            }
        }

        public void Bop(double beat)
        {
            var allBopEvents = EventCaller.GetAllInGameManagerList("holeInOne", new string[] { "bop" });
            RiqEntity lastBopType = null;
            foreach (var e in allBopEvents)
            {
                if (e.beat > beat) break;
                lastBopType = e;
            }

            if (lastBopType != null)
            {
                _currentMandrillBopType = lastBopType["mandrillBopType"];
            }
    
            string[] uninterruptibleMonkeyAnims = { "MonkeyPrepare", "MonkeyThrow", "MonkeySpin"};
            if (!MonkeyAnim.IsPlayingAnimationNames(uninterruptibleMonkeyAnims))
            {
                MonkeyAnim.DoScaledAnimationAsync("MonkeyBop", 1f);
            }

            string[] uninterruptibleMandrillAnims = { "MandrillReady1", "MandrillReady2", "MandrillReady3", "MandrillPitch" };
            if (!MandrillAnim.IsPlayingAnimationNames(uninterruptibleMandrillAnims))
            {
                switch (_currentMandrillBopType)
                {
                    case 1:
                        MandrillAnim.DoScaledAnimationAsync("MandrillBop2", 1f);
                        break;
                    case 2:
                        MandrillAnim.DoScaledAnimationAsync("MandrillBop3", 1f);
                        break;
                    default:
                        MandrillAnim.DoScaledAnimationAsync("MandrillBop", 1f);
                        break;
                }
            }
    
            if (canBop && !GolferAnim.IsPlayingAnimationNames("GolferJust", "GolferWhiff", "GolferPrepare", "GolferThrough", "GolferMiss", "GolferThroughMandrill") && !PlayerInput.GetIsAction(InputAction_TouchPressing))
            {
                GolferAnim.DoScaledAnimationAsync("GolferBop", 1f);
            }
        }
    
        public void Whale(double beat, float length, int ease, bool appear)
        {
            SoundByte.PlayOneShotGame("clappyTrio/sign");
            whaleStartBeat = beat;
            whaleLength = length;
            lastEase = (Util.EasingFunction.Ease)ease;
            isWhale = appear;
        }

        void UpdateWhale(double beat)
        {
            if (beat >= whaleStartBeat)
            {
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(lastEase);
                float whaleProg = Conductor.instance.GetPositionFromBeat(whaleStartBeat, whaleLength, true);
                whaleProg = Mathf.Clamp01(whaleProg);
                // float whale = func(whaleLast, whaleNext, whaleProg);
            }
        }

        private IEnumerator SpawnGrassEffects()
        {
            Vector3[] corners = new Vector3[4];
            grassArea.GetWorldCorners(corners);
            float leftX = corners[0].x;
            float rightX = corners[3].x;
            float bottomY = corners[0].y;
            float topY = corners[1].y;

            //left side inital spawn 
            float leftThird = Mathf.Lerp(leftX, rightX, 0.33f);
            for (int i = 0; i < 9; i++)
            {
                SpawnSingleGrassEffect(UnityEngine.Random.Range(leftX, leftThird), UnityEngine.Random.Range(bottomY, topY));
            }

            int remainingEffects = 35 - 9;
            if (remainingEffects <= 0) yield break;
            //if you divide by 0 it blows up so incase someone wants to remove the grass effects IG
            float effectDuration = (float)Conductor.instance.secPerBeat * 0.5f;
            float spawnInterval = effectDuration / remainingEffects;

            for (int i = 0; i < remainingEffects; i++)
            {
                float left2rite = (float)i / (remainingEffects - 1);
                float spawnX = Mathf.Lerp(leftX, rightX, left2rite);
                float spawnY = UnityEngine.Random.Range(bottomY, topY);
        
                float xOffset = UnityEngine.Random.Range(-0.5f, 0.5f);

                SpawnSingleGrassEffect(spawnX + xOffset, spawnY);

                yield return new WaitForSeconds(spawnInterval);
            }
        }

        private void SpawnSingleGrassEffect(float x, float y)
        {
            GameObject effectInstance = Instantiate(grassEffectPrefab, grassArea.transform);
            effectInstance.transform.position = new Vector3(x, y, 0);
            effectInstance.SetActive(true);

            Animator anim = effectInstance.GetComponent<Animator>();
            if (anim != null)
            {
                anim.speed = UnityEngine.Random.Range(0.6f, 2f);

                int randomAnimIndex = UnityEngine.Random.Range(1, 4); 
                anim.Play($"GrassEffect{randomAnimIndex}", 0, 0f);
            }

            Destroy(effectInstance, 4f);
        }

        public void DoMonkey(double beat, float length = 1f)
        {
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("holeInOne/SE_GOLF_MONKEY_READY", beat),
                new MultiSound.Sound("holeInOne/SE_GOLF_MONKEY", beat + length),
                new MultiSound.Sound("holeInOne/SE_GOLF_MONKEY_THROW", beat + length),
                new MultiSound.Sound("holeInOne/SE_GOLF_AUTO_SWING", beat + length + 1f, offset: 0.083f)
            });
            bool skipPrepare = (beat == _lastMonkeyThrowBeat);

            _lastMonkeyThrowBeat = beat + length;
    
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate {
                    if (!skipPrepare)
                    {
                        MonkeyAnim.DoScaledAnimationAsync("MonkeyPrepare", 1f);
                    }
                }),
                new BeatAction.Action(beat + length, delegate {
                    MonkeyAnim.DoScaledAnimationAsync("MonkeyThrow", 1f);
                    AutoPrepare(beat + length);
                }),
            });
            SpawnBall(beat, length);
        }

        public void DoMandrill(double beat)
        {
            // Mandrill Multisound
            //ScheduleInput(beat, 3f, InputAction_FlickPress, MandrillSuccess, MandrillMiss, Empty);
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("holeInOne/SE_GOLF_GORILLA4", beat),
                new MultiSound.Sound("holeInOne/SE_GOLF_GORILLA1", beat + 1f),
                new MultiSound.Sound("holeInOne/SE_GOLF_GORILLA2", beat + 2f),
                new MultiSound.Sound("holeInOne/SE_GOLF_GORILLA3", beat + 3f),
                new MultiSound.Sound("holeInOne/SE_GOLF_GORILLA5", beat + 3f),
            });
        
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat,       delegate { MandrillAnim.DoScaledAnimationAsync("MandrillReady1", 1f); }),
                new BeatAction.Action(beat + 1f,    delegate { MandrillAnim.DoScaledAnimationAsync("MandrillReady2", 1f); }),
                new BeatAction.Action(beat + 2f,    delegate {
                    MandrillAnim.DoScaledAnimationAsync("MandrillReady3", 1f);
                    AutoPrepare(beat + 2f);
                }),
                new BeatAction.Action(beat + 3f,    delegate {
                    MandrillAnim.DoScaledAnimationAsync("MandrillPitch", 1f);
                    MonkeyAnim.DoScaledAnimationAsync("MonkeySpin", 1f); 
                    StartCoroutine(SpawnGrassEffects());
                }),
            });
            SpawnBall(beat, 2, false);
        }

        public void SpawnBall(double beat, float length, bool monkey = true)
        {
            var newBall = Instantiate(baseBall, transform).GetComponent<Ball>();
            newBall.startBeat = beat;

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length, delegate {
                    newBall.gameObject.SetActive(true);
                    newBall.Init(monkey);
                }),
            });
        }

        public void AutoPrepare(double beat)
        {
            if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch && PlayerInput.PlayerHasControl()) return;
            if (GolferAnim.IsPlayingAnimationNames("GolferJust")) return;

            GolferAnim.DoScaledAnimationAsync("GolferPrepare", 1f);

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.7f, delegate {
                    if (!GolferAnim.IsPlayingAnimationNames("GolferJust"))
                    {
                        GolferAnim.DoScaledAnimationAsync("GolferThrough", 1f);
                    }
                })
            });
        }

        public void MonkeySuccess(PlayerActionEvent caller, float state)
        {
            if (state >= 1f)
            {
                double beat = caller.startBeat + caller.timer;

                SoundByte.PlayOneShotGame("holeInOne/SE_GOLF_MISS_SHOT_RIGHT");           
                SoundByte.PlayOneShotGame("holeInOne/SE_GOLF_WATER_IN_RIGHT", beat + 1f);  

                MonkeyHeadAnim.DoScaledAnimationAsync("MonkeyMissHead", 1f);
                GolferAnim.Play("GolferWhiff", 0, 0);
            }
            else if (state <= -1f)
            {
                double beat = caller.startBeat + caller.timer;

                SoundByte.PlayOneShotGame("holeInOne/SE_GOLF_MISS_SHOT_LEFT");   
                SoundByte.PlayOneShotGame("holeInOne/SE_GOLF_WATER_IN_LEFT", beat + 1f);  

                MonkeyHeadAnim.DoScaledAnimationAsync("MonkeyMissHead", 1f);
                GolferAnim.Play("GolferWhiff", 0, 0);
            }


            else
            {
                double beat = caller.startBeat + caller.timer;
                int randomSuccess = UnityEngine.Random.Range(1, 4);

                MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound((isWhale) ? "holeInOne/SE_GOLF_WHALE" : ("holeInOne/SE_GOLF_CUP_IN_ROLL"), beat + 2f)
                });

                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat,       delegate { MonkeyHeadAnim.DoScaledAnimationAsync("MonkeyJustHead", 1f);}),
                    new BeatAction.Action(beat,        delegate { GolferAnim.DoScaledAnimationAsync("GolferJust", 1f);}),
                    new BeatAction.Action(beat + 1.5f, delegate { Hole.SetActive(true);}),
                    new BeatAction.Action(beat + 2f,   delegate { HoleAnim.DoScaledAnimationAsync("ZoomSmall" + randomSuccess, 1f);}),
                });
            }
        }

        public void MonkeyMiss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame($"holeInOne/SE_GOLF_MISS_THROUGH{UnityEngine.Random.Range(1, 4)}");
            MonkeyHeadAnim.DoScaledAnimationAsync("MonkeySadHead", 1f);

            canBop = false;
            BeatAction.New(instance, new List<BeatAction.Action>() {
                new BeatAction.Action(caller.startBeat + caller.timer + 2f, delegate { canBop = true; })
            });
        }

        public void MandrillSuccess(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                double beat = caller.startBeat + caller.timer;

                SoundByte.PlayOneShotGame("holeInOne/SE_GOLF_MISS_SHOT_LEFT");           // temp should be both l/r
                SoundByte.PlayOneShotGame("holeInOne/SE_GOLF_WATER_IN_LEFT", beat + 1f);    // temp should be both l/r

                BallEffectAnim.Play("BallEffectJust");
                GolferAnim.Play("GolferWhiff", 0, 0);
            }
            else
            {
                double beat = caller.startBeat + caller.timer;

                MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound("holeInOne/SE_GOLF_SHOT_GORILLA_BIG_BALL", beat),
                    new MultiSound.Sound((isWhale) ? "holeInOne/SE_GOLF_WHALE" : ("holeInOne/SE_GOLF_CUP_IN_GORILLA"), beat + 2f)
                });

                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat,        delegate { GolferAnim.DoScaledAnimationAsync("GolferJust", 1f);}),
                    new BeatAction.Action(beat + 1.5f, delegate { Hole.SetActive(true);}),
                    new BeatAction.Action(beat + 2f,   delegate { HoleAnim.DoScaledAnimationAsync("ZoomBig", 1f);}),
                });
            }
            
        }

        public void MandrillMiss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("holeInOne/SE_GOLF_MISS_BALL_ATTACK_BIG_GORI");
            SoundByte.PlayOneShotGame($"holeInOne/SE_GOLF_MISS_BALL_ATTACK_BIG_VOICE{UnityEngine.Random.Range(1, 6)}");

            MonkeyAnim.Play("MonkeySpin");
            BallEffectAnim.Play("BallEffectThrough");
            GolferAnim.Play("GolferThroughMandrill");

            canBop = false;
            BeatAction.New(instance, new List<BeatAction.Action>() {
                new BeatAction.Action(caller.startBeat + caller.timer + 2f, delegate { canBop = true; })
            });
        }

        public void Empty(PlayerActionEvent caller) {}
    }
}