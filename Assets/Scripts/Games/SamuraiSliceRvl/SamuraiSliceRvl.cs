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
    /// Minigame loaders handle the setup of your minigame.
    /// Here, you designate the game prefab, define entities, and mark what AssetBundle to load

    /// Names of minigame loaders follow a specific naming convention of `PlatformcodeNameLoader`, where:
    /// `Platformcode` is a three-leter platform code with the minigame's origin
    /// `Name` is a short internal name
    /// `Loader` is the string "Loader"
    /// 
    /// Platform codes are as follows:
    /// Agb: Gameboy Advance    ("Advance Gameboy")
    /// Ntr: Nintendo DS        ("Nitro")
    /// Rvl: Nintendo Wii       ("Revolution")
    /// Ctr: Nintendo 3DS       ("Centrair")
    /// Mob: Mobile
    /// Pco: PC / Other
    /// 
    /// Fill in the loader class label, "*prefab name*", and "*Display Name*" with the relevant information
    /// For help, feel free to reach out to us on our discord, in the #development channel.
    public static class SamuraiRvlLoader
    {
        
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("samuraiSliceRvl", "Samurai Slice", "595939", false, false, new List<GameAction>()
            {
                new GameAction ("bop", "Bop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        SamuraiSliceRvl.instance.Bop(e.beat, e.length, e["auto"], e["bop"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop", "Whether the character should bop during this event."),
                        new Param("auto", false, "Bop (Auto)", "Whether the character should automatically bop until another Bop event."),
                    },
                    resizable = true,
                    defaultLength = 1.0f,
                },
                 new GameAction("demon", "Demon")
                    {
                        function = delegate
                        {
                            var e = eventCaller.currentEntity;
                            SamuraiSliceRvl.instance.SpawnDemon(e.beat, (DemonType)e["type"], e["prepareBeat"]);
                        },
                        defaultLength = 3.0f,
                        parameters = new()
                        {
                            new Param("type", DemonType.Small, "Type", "The type of demon to spawn."),
                            new Param("prepareBeat", new EntityTypes.Integer(1, 2, 1), "Prepare Beat", "The beat after the slice when the samurai will prepare again.")
                        }
                    },
                new GameAction("combodemon", "Horde of Demons")
                {
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        SamuraiSliceRvl.instance.ComboDemon(eventCaller.currentEntity.beat);
                    },
                    defaultLength = 4.0f,
                },
                new GameAction ("thunder", "Toggle Thunder")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        SamuraiSliceRvl.instance.Thunder(e.beat, e["thunder"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("thunder", true, "Enable Thunder", "If this is enabled, thunder will appear on each slice."),
                    },
                    defaultLength = 0.5f,
                },
                new GameAction("smog control", "Smog Animation")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        SamuraiSliceRvl.instance.SmogControl(e.beat, e.length, e["type"], e["ease"]);
                    },
                    defaultLength = 4.0f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("type", true, "In / Out", "Toggle if the smog should appear (In) or disappear (Out)."),
                        new Param("ease", Util.EasingFunction.Ease.EaseOutQuad, "Ease", "Set the easing of the action.")
                    }
                },
                new GameAction("toggleFG", "Toggle Foreground")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        SamuraiSliceRvl.instance.ToggleForeground(e.beat, e["show"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("show", false, "Show Foreground", "If true, the foreground will be shown. If false, it will be hidden."),
                    },
                    defaultLength = 0.5f,
                    advanced = true,
                },

            },
            new List<string>() { "rvl", "normal" },
            "rvlsamurai", "en",
            new List<string>() { "en" },
            chronologicalSortKey: 19,
            font : "KanTeiRyu"
            );
            
        }
        
    }
}

namespace HeavenStudio.Games
{
    using Scripts_SamuraiSliceRvl;
    using Jukebox;
    using NaughtyBezierCurves;
    // Demon's type
    public enum DemonType
    {
        Small,
        Medium,
        Big,
        Huge
    }
    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
     public class SamuraiSliceRvl : Minigame
    {
        public static SamuraiSliceRvl instance;

        [SerializeField] Animator SamuraiAnim;
        private bool autoBop = true;
        // This is a counter used to determine if there were 2 successful hits in a row.
        private double lastSuccessfulBeat = -1;
        // This checks if there was a successful hit to block the default sounds in Slice and ComboSlice.
        private bool isDemonSuccess = false;

        private bool thundereffect = false;
        public bool IsReady { get; private set; } = false;
        public bool IsHolding { get; private set; } = false;
        private int nextPrepareBeat = 1;

        [SerializeField] GameObject fgHolder;

        const int IAAltDownCat = IAMAXCAT;

        const int IAAltUpCat = IAMAXCAT + 1;
        // Buttons created for the game's functionality, based on Rhythm Tweezers, Kitties, and Hole in One
        public static PlayerInput.InputAction InputAction_TouchPress =
            new("SamuraiSliceTouchPress", new int[] { IAEmptyCat, IAPressCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicPress, IA_Empty);

        public static PlayerInput.InputAction InputAction_TouchPressing =
            new("SamuraiSliceTouchPressing", new int[] { IAEmptyCat, IAPressCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicPressing, IA_Empty);

        public static PlayerInput.InputAction InputAction_TouchRelease =
            new("SamuraiSliceTouchRelease", new int[] { IAEmptyCat, IAReleaseCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicRelease, IA_Empty);

        public static PlayerInput.InputAction InputAction_ComboPress =
            new("ComboDemonPress", new int[] { IAAltDownCat, IAAltDownCat, IAAltDownCat },
            IA_PadAltPress, IA_TouchAltPress, IA_BatonAltPress);

        public static PlayerInput.InputAction InputAction_ComboRelease =
            new("ComboDemonRelease", new int[] { IAAltUpCat, IAAltUpCat, IAAltUpCat },
            IA_PadAltRelease, IA_TouchAltRelease, IA_BatonAltRelease);

        protected static bool IA_PadAltPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.South, out dt);
        }

        protected static bool IA_TouchAltPress(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt);
        }

        protected static bool IA_BatonAltPress(out double dt)
        {
            return PlayerInput.GetSqueezeDown(out dt);
        }

        protected static bool IA_PadAltRelease(out double dt)
        {
            return PlayerInput.GetPadUp(InputController.ActionsPad.South, out dt);
        }

        protected static bool IA_TouchAltRelease(out double dt)
        {
            return PlayerInput.GetTouchUp(InputController.ActionsTouch.Tap, out dt);
        }
        protected static bool IA_BatonAltRelease(out double dt)
        {
            return PlayerInput.GetSqueezeUp(out dt);
        }


        [Header("Prefabs")]
        [SerializeField] private Demon demonholder;
        [SerializeField] private Flash flashholder;
        [SerializeField] public DemonSliced[] slicedDemonPrefabs;
        [SerializeField] public DemonSliced hordeSlicedPrefab; // New field specific for horde
        [SerializeField] private SmogEffect smogEffectPrefab;
        [SerializeField] public HordeDemon hordeDemonPrefab;

        [Header("Curves")]
        [SerializeField] private BezierCurve3D spawnCurve;
        [SerializeField] private BezierCurve3D missCurve;
        [SerializeField] private BezierCurve3D walkCurve;
        private double lastDemonBeat = -10;

        [Header("Positions")]
        [SerializeField] public Vector2[] demonSpawnPositions;
        [SerializeField] public Vector2[] hordeSpawnPositions;
        private SmogEffect currentSmog;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            if (smogEffectPrefab != null)
            {
                currentSmog = Instantiate(smogEffectPrefab, transform);
                currentSmog.Play();
            }
        }

        public override void OnStop(double beat)
        {
            lastDemonBeat = -10;
        }

        void Update()
        {
            if(!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                lastSuccessfulBeat = -1;
                return;
            }
            if (PlayerInput.GetIsAction(InputAction_ComboPress))
            {
                SamuraiSliceRvl.instance.SliceCombo(null, Conductor.instance.songPositionInBeatsAsDouble, 1.0f);
            }
            if (PlayerInput.GetIsAction(InputAction_TouchPress))
            {
                SamuraiSliceRvl.instance.Holding(null, Conductor.instance.songPositionInBeatsAsDouble, 1.0f);
            }
            if (PlayerInput.GetIsAction(InputAction_TouchRelease) || PlayerInput.GetIsAction(InputAction_FlickPress))
            {
                IsReady = false;
                if(PlayerInput.GetIsAction(InputAction_TouchRelease))
                {
                    SamuraiAnim.DoScaledAnimationAsync("SamuraiBop", 1.0f);
                }
            }
            if (PlayerInput.GetIsAction(InputAction_ComboRelease) && PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch)
            {
                IsHolding = false;
            }
            if (PlayerInput.GetIsAction(InputAction_FlickPress))
            {
                SamuraiSliceRvl.instance.Slice(null, Conductor.instance.songPositionInBeatsAsDouble, 1.0f, true);
            }
        }
        public override void OnBeatPulse(double beat)
        {
            AnimatorStateInfo state = SamuraiAnim.GetCurrentAnimatorStateInfo(0);
            if (autoBop && (SamuraiAnim.IsAnimationNotPlaying() || state.IsName("SamuraiIdle")) && !IsReady)
            {
                // If the character is not in a bop, idle, or slash state, it will automatically bop
                // This is to prevent the character from being in a slash state and then bopping
                // It also prevents the character from being in a horde state and then bopping
                {
                    SamuraiAnim.DoScaledAnimationAsync("SamuraiBop", 1.0f);
                }
            }
            else if (!autoBop && (SamuraiAnim.IsAnimationNotPlaying() || state.IsName("SamuraiIdle")) && !IsReady) { 
                SamuraiAnim.DoScaledAnimationAsync("SamuraiIdle", 1.0f);
            }
        }

        public void Bop(double beat, float length, bool auto, bool bop)
        {
            autoBop = auto;
            if (!bop) return;

            List<BeatAction.Action> actions = new();
            for (int i = 0; i < length; i++)
            {
                actions.Add(new BeatAction.Action(beat + i, delegate
                {
                    SamuraiAnim.DoScaledAnimationAsync("SamuraiBop", 1.0f);
                }));
            }
            if (actions.Count > 0)
                BeatAction.New(this, actions);
        }

        public void SmogControl(double beat, float length, bool show, int ease)
        {
            if (currentSmog != null)
            {
                currentSmog.Animate(beat, length, show, ease);
            }
            if(show && ease != (int)Util.EasingFunction.Ease.Instant)
            {
                SoundByte.PlayOneShotGame("samuraiSliceRvl/CLOUD_PRERENDER");
            }
        }

        public void ToggleForeground(double beat, bool show)
        {
            fgHolder.SetActive(show);
        }

        public void SpawnDemon(double beat, DemonType type, int prepareBeat)
        {
            // Calls a function that determines which demon sound will be used
            DemonSound(beat, type);

            nextPrepareBeat = prepareBeat;

            // Instantiate the demon and set its properties
            Demon newDemon = Instantiate(demonholder, transform);
            BezierCurve3D newspawnCurve = Instantiate(spawnCurve, transform);
            Vector3 spawnPos = newspawnCurve.GetPoint(0);

            if (beat - 1 <= lastDemonBeat)
            {
                spawnPos += new Vector3(1f, -0.75f, 0);
            }
            else
            {
                lastDemonBeat = beat;
            }

            newspawnCurve.KeyPoints[0].Position = spawnPos;
            newDemon.transform.position = spawnPos;
            newDemon.startBeat = beat;
            newDemon.spawnCurve = newspawnCurve;
            newDemon.missCurve = missCurve;
            newDemon.walkCurve = walkCurve;
            newDemon.type = type;
            newDemon.gameObject.SetActive(true);
        }

        public void SpawnSlicedDemon(Vector3 position, Quaternion rotation, DemonType type)
        {
            int index = (int)type;
            DemonSliced prefabToSpawn = null;

            if (slicedDemonPrefabs != null)
            {
                // Intentar obtener el prefab específico para este tipo
                if (index < slicedDemonPrefabs.Length && slicedDemonPrefabs[index] != null)
                {
                    prefabToSpawn = slicedDemonPrefabs[index];
                }
                // Si no existe, usar el primero (Small) como respaldo para que al menos salga algo
                else if (slicedDemonPrefabs.Length > 0 && slicedDemonPrefabs[0] != null)
                {
                    prefabToSpawn = slicedDemonPrefabs[0];
                }
            }

            if (prefabToSpawn != null)
            {
                DemonSliced sliced = Instantiate(prefabToSpawn, position, rotation);
                sliced.Init(type);
            }
        }

        public void ComboDemon(double beat)
        {
            // Create a new sequence object for this horde event
            GameObject go = new GameObject("HordeSequence");
            HordeSequence sequence = go.AddComponent<HordeSequence>();
            sequence.Init(this);

            AnimatorStateInfo state = SamuraiAnim.GetCurrentAnimatorStateInfo(0);
            // Here I create a function for the demon horde, playing the sound divided into thirds
            if (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch || GameManager.instance.Autoplay)
            {
                IsReady = true;
            }
            if (Math.Abs(beat + nextPrepareBeat - lastSuccessfulBeat) <= 2f) 
            { 
                BeatAction.New(this, new List<BeatAction.Action>() {
                    new BeatAction.Action(beat + nextPrepareBeat, () => {
                        SamuraiAnim.DoScaledAnimationAsync("SamuraiReady", 1.0f);    
                    })
                }); 
            }
            else
            {
                SamuraiAnim.DoScaledAnimationAsync("SamuraiReady", 1.0f);
            }
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("samuraiSliceRvl/combo1", beat),
                new MultiSound.Sound("samuraiSliceRvl/combo2", beat + 0.3333f, ignoreSwing: true),
                new MultiSound.Sound("samuraiSliceRvl/combo3", beat + 0.6667f, ignoreSwing: true),
                new MultiSound.Sound("samuraiSliceRvl/combo4", beat + 1f),
            });
            
            // Schedule input passing the specific sequence
            ScheduleInput(beat, 2.0, InputAction_ComboPress, 
                (caller, state) => ComboDemonStart(caller, state, sequence), 
                (caller) => ComboDemonMiss(caller, sequence), 
                null);

            // Schedule horde spawn synchronized with sounds
            // Sound 1: 1 demon
            BeatAction.New(this, new List<BeatAction.Action>() {
                new BeatAction.Action(beat, () => sequence.SpawnBatch(1)),
                new BeatAction.Action(beat + 0.3333f, () => sequence.SpawnBatch(1), ignoreSwing: true),
                new BeatAction.Action(beat + 0.6667f, () => sequence.SpawnBatch(1), ignoreSwing: true),
                new BeatAction.Action(beat + 1.0f, () => sequence.SpawnBatch(4)),
                // At beat + 2 (when input starts), everyone rushes
                new BeatAction.Action(beat + 2.0f, () => sequence.ScheduleRushes(beat + 2.0))
            });
        }

        private void ComboDemonStart(PlayerActionEvent caller, float state, HordeSequence sequence)
        {
            // This function is called first to check if the button was pressed at the right time, playing the combo sound, and then
            // calling another function that checks when the button is released
            isDemonSuccess = true;
            SamuraiAnim.DoScaledAnimationAsync("SamuraiHordeStart", 1.0f);
            double inputBeat = caller.startBeat + caller.timer;

            SoundByte.PlayOneShotGame("samuraiSliceRvl/HIT6_START");
            var SamuraiOoh = SoundByte.PlayOneShotGame("samuraiSliceRvl/ROLLING_START", -1, 1, 1f, true);
            SamuraiOoh.SetLoopParams(inputBeat + 1, 0.1f);
            BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(inputBeat + 0.14f, delegate
                    {
                        SamuraiAnim.DoScaledAnimationAsync("SamuraiHordeLoop", 0.5f);
                    })
                });
            
            // List of times relative to inputBeat for combo "hits"
            // Generate 7 slice times: (i + 1) / 7.0
            float[] hitTimes = new float[7];
            for (int i = 0; i < 7; i++) hitTimes[i] = i / 7f;

            List<BeatAction.Action> killActions = new List<BeatAction.Action>();
            IsHolding = true;
            // Create sound and demon elimination actions
            List<MultiSound.Sound> hitSounds = new List<MultiSound.Sound>();
            
            for (int i = 0; i < hitTimes.Length; i++)
            {
                string soundName = "samuraiSliceRvl/HIT6_ALT";
                hitSounds.Add(new MultiSound.Sound(soundName, inputBeat + hitTimes[i], ignoreSwing: true));

                // Action to eliminate a demon on each combo "hit"
                // Eliminate 1 to 6. The 7th (last) is handled in ComboDemonSuccess
                if (i < hitTimes.Length - 1)
                {
                    killActions.Add(new BeatAction.Action(inputBeat + hitTimes[i], () => sequence.KillNext(false)));
                }
            }

            MultiSound.Play(hitSounds.ToArray());
            // Execute elimination actions
            BeatAction.New(this, killActions);

            if (state >= 1f || state <= -1f)
            {
                sequence.IsComboMissed = true;
                sequence.EndComboInput = ScheduleInput(inputBeat, 1.0, InputAction_ComboRelease, 
                    (c, s) => ComboDemonSuccess(c, s, sequence), 
                    null, 
                    (c) => ComboDemonEmpty(c, sequence));
                    
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(inputBeat + 1, delegate
                    {
                        if (sequence.IsComboMissed && state >= 1f)
                        {
                            sequence.EndComboInput.Hit(0, 1.75);
                        }
                        else if (sequence.IsComboMissed && state <= -1f)
                        {
                            sequence.EndComboInput.Hit(0, -1.75);
                        }
                    }),
                });
                return;
            }
            sequence.IsComboMissed = false;
            sequence.EndComboInput = ScheduleInput(inputBeat, 1.0, InputAction_ComboRelease, 
                (c, s) => ComboDemonSuccess(c, s, sequence), 
                (c) => ComboDemonMiss(c, sequence), 
                (c) => ComboDemonEmpty(c, sequence));
                
            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(inputBeat + 1, delegate
                    {
                        if (!sequence.IsComboMissed)
                        {
                            sequence.EndComboInput.Hit(0, 1);
                        }
                    }),
            });
        }

        private void ComboDemonSuccess(PlayerActionEvent caller, float state, HordeSequence sequence)
        {
            sequence.EndComboInput.Disable();
            if (state >= 1f || state <= -1f)
            {
                ComboDemonMiss(caller, sequence);
                return;
            }
            SamuraiAnim.DoScaledAnimationAsync("SamuraiHordeEnd", 1.0f);
            if (!sequence.IsComboMissed)
            {
                sequence.KillNext(true); // Slice the last demon (the one that splits in two)
                SoundByte.PlayOneShotGame("samuraiSliceRvl/ROLLING_ALL_HIT");
                if (thundereffect)
                {
                    string thunder = "";
                    thunder = UnityEngine.Random.value > 0.66f
                            ? "samuraiSliceRvl/THUNDER1"
                            : UnityEngine.Random.value > 0.33f
                                ? "samuraiSliceRvl/THUNDER2"
                                : "samuraiSliceRvl/THUNDER3";
                    SoundByte.PlayOneShotGame(thunder);
                    Flash flasheffect = Instantiate(flashholder, transform);
                    flasheffect.startBeat = Conductor.instance.songPositionInBeatsAsDouble;
                    flasheffect.strikeNumber = 3;
                    flasheffect.gameObject.SetActive(true);
                }
            }
            else
            {
                // OSII Case: The last demon doesn't die sliced, but falls failed
                sequence.FailRest();
                SoundByte.PlayOneShotGame("samuraiSliceRvl/OSII");
            } 
            if (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch || GameManager.instance.Autoplay)
            {
                IsReady = false;
            }
            
            // Clean up sequence object after a delay to allow animations to finish
            Destroy(sequence.gameObject, 5f);
        }
        public void ComboDemonMiss(PlayerActionEvent caller, HordeSequence sequence)
        {
            // If the combo is missed, this sound is played
            IsReady = false;
            sequence.IsComboMissed = true;
            if (caller == null)
            {
                SamuraiAnim.DoScaledAnimationAsync("SamuraiHordeMiss", 1.0f);
                SoundByte.PlayOneShotGame("samuraiSliceRvl/YARARE2");
            }
        }
        private void ComboDemonEmpty(PlayerActionEvent caller, HordeSequence sequence)
        {
            // If nothing is done, this sound is played
            sequence.IsComboMissed = true;
        }
        public void DemonSuccess(PlayerActionEvent caller, float state)
        {
            isDemonSuccess = true;
            // This variable is used to track the beat where the hit occurred, to check if two consecutive hits were made
            if (state >= 1f || state <= -1f)
            {
                return;
            }
            else
            {
                double currentBeat = Conductor.instance.songPositionInBeatsAsDouble;
                SamuraiAnim.DoScaledAnimationAsync("SamuraiSlash", 0.5f);
                string soundToPlay = "samuraiSliceRvl/HIT1";
                // Here it checks if a second consecutive hit was made and plays a random sound
                if (Math.Abs(currentBeat - lastSuccessfulBeat) <= 1.5f)
                {
                    SamuraiAnim.DoScaledAnimationAsync("SamuraiSlash2", 0.5f);
                    soundToPlay = UnityEngine.Random.value > 0.5f
                        ? "samuraiSliceRvl/HIT2_A"
                        : "samuraiSliceRvl/HIT2_B";
                    lastSuccessfulBeat = -1f;
                    BeatAction.New(this, new List<BeatAction.Action>() {
                        new BeatAction.Action(currentBeat + 1, () => {
                            if(lastSuccessfulBeat == -1f) 
                            { 
                                SamuraiAnim.DoScaledAnimationAsync("SamuraiIdle", 0.5f); 
                            }
                        })
                    });
                }
                else
                {
                    BeatAction.New(this, new List<BeatAction.Action>() {
                        new BeatAction.Action(currentBeat + nextPrepareBeat, () => {
                            if (Math.Abs(currentBeat + nextPrepareBeat - lastSuccessfulBeat) <= 1.5f) 
                            { SamuraiAnim.DoScaledAnimationAsync("SamuraiIdle", 0.5f); }
                        })
                    });
                    lastSuccessfulBeat = currentBeat;
                }
                // These are extra sounds that play when a hit is successful
                // This is more to give it a more impactful effect, as these sounds were found separately and not together
                SoundByte.PlayOneShotGame(soundToPlay);
                isDemonSuccess = true;
                // Marks that a hit was made to prevent the default slice and comboslice sounds from playing
                if (thundereffect)
                {
                    string thunder = "";
                    thunder = UnityEngine.Random.value > 0.66f
                            ? "samuraiSliceRvl/THUNDER1"
                            : UnityEngine.Random.value > 0.33f
                                ? "samuraiSliceRvl/THUNDER2"
                                : "samuraiSliceRvl/THUNDER3";
                    SoundByte.PlayOneShotGame(thunder);
                    Flash flasheffect = Instantiate(flashholder, transform);
                    flasheffect.startBeat = Conductor.instance.songPositionInBeatsAsDouble;
                    if (Math.Abs(currentBeat - lastSuccessfulBeat) <= 1.5f)
                    {
                        flasheffect.strikeNumber = 1;
                    }
                    else
                    {
                        flasheffect.strikeNumber = 2;
                    }
                    flasheffect.gameObject.SetActive(true);
                }
            }
        }

        public void DemonMiss(PlayerActionEvent caller)
        {
            double currentBeat = Conductor.instance.songPositionInBeatsAsDouble;
            SamuraiAnim.DoScaledAnimationAsync("SamuraiMiss", 1.0f);
            SoundByte.PlayOneShotGame("samuraiSliceRvl/YARARE1");
        }

        private void DemonSound(double beat, DemonType type)
        {
            // This function checks which sound is requested depending on the demon type
            List<MultiSound.Sound> sounds = new();

            switch (type)
            {
                case DemonType.Small:
                    sounds.Add(new MultiSound.Sound("samuraiSliceRvl/demon1_1", beat));
                    sounds.Add(new MultiSound.Sound("samuraiSliceRvl/demon1_2", beat + 0.5f));
                    sounds.Add(new MultiSound.Sound("samuraiSliceRvl/demon1_3", beat + 1f));
                    break;

                case DemonType.Medium:
                    sounds.Add(new MultiSound.Sound("samuraiSliceRvl/demon2_1", beat));
                    sounds.Add(new MultiSound.Sound("samuraiSliceRvl/demon2_2", beat + 0.5f));
                    sounds.Add(new MultiSound.Sound("samuraiSliceRvl/demon2_3", beat + 1f));
                    break;

                case DemonType.Big:
                    sounds.Add(new MultiSound.Sound("samuraiSliceRvl/demon3_1", beat));
                    sounds.Add(new MultiSound.Sound("samuraiSliceRvl/demon3_2", beat + 0.5f));
                    sounds.Add(new MultiSound.Sound("samuraiSliceRvl/demon3_3", beat + 1f));
                    break;

                case DemonType.Huge:
                    sounds.Add(new MultiSound.Sound("samuraiSliceRvl/demon4_1", beat));
                    sounds.Add(new MultiSound.Sound("samuraiSliceRvl/demon4_2", beat + 0.5f));
                    sounds.Add(new MultiSound.Sound("samuraiSliceRvl/demon4_3", beat + 1f));
                    break;
            }
            // I store them in a list to later play them all together with a MultiSound
            MultiSound.Play(sounds.ToArray());
        }

        public void Slice(PlayerActionEvent caller, double beat, double length, bool isFlick)
        {
            // Here it checks if a successful hit was made in the Demon function; if so, this function does nothing
            // and marks that no hit was made to reset the function
            if (isDemonSuccess)
            {
                isDemonSuccess = false;
                return;
            }

            if (PlayerInput.GetIsAction(InputAction_FlickPress) && isFlick)
            {
                SamuraiAnim.DoScaledAnimationAsync("SamuraiSlash", 0.5f);
                // If the hit wasn't made in Demon, the sound remains
                string soundToPlay = "";
                soundToPlay = UnityEngine.Random.value > 0.66f
                            ? "samuraiSliceRvl/SWING1_A"
                            : UnityEngine.Random.value > 0.33f
                                ? "samuraiSliceRvl/SWING1_B"
                                : "samuraiSliceRvl/SWING1_C";
                if (Math.Abs(beat - lastSuccessfulBeat) <= 1.5f)
                {
                    soundToPlay = UnityEngine.Random.value > 0.66f
                            ? "samuraiSliceRvl/SWING2_A"
                            : UnityEngine.Random.value > 0.33f
                                ? "samuraiSliceRvl/SWING2_B"
                                : "samuraiSliceRvl/SWING2_C";
                    SamuraiAnim.DoScaledAnimationAsync("SamuraiSlash2", 0.5f);
                    lastSuccessfulBeat += beat * 2;
                }
                else
                {
                    lastSuccessfulBeat = beat;
                }
                SoundByte.PlayOneShotGame(soundToPlay);
                AnimatorStateInfo statesamu = SamuraiAnim.GetCurrentAnimatorStateInfo(0);
            }
        }
        public void SliceCombo(PlayerActionEvent caller, double beat, double length)
        {
            // Does the same but with the combo function
            if (isDemonSuccess)
            {
                isDemonSuccess = false;
                return;
            }
            // Here I check that the button is pressed and not a touch press, because if this isn't checked
            // the sound would play when performing a prepare
            if (PlayerInput.GetIsAction(InputAction_ComboPress) && !PlayerInput.GetIsAction(InputAction_TouchPress))
            {
                IsReady = true;
                IsHolding = true;
                SamuraiAnim.DoScaledAnimationAsync("SamuraiHordeStart", 1.0f);
                
                    BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 0.14f, delegate
                    {
                        SamuraiAnim.DoScaledAnimationAsync("SamuraiHordeLoop", 0.5f);
                    }),
                    new BeatAction.Action(beat + 1.0f, delegate
                    {
                        SamuraiAnim.DoScaledAnimationAsync("SamuraiHordeEnd", 0.5f);
                    }),
                    new BeatAction.Action(beat + 2.0f, delegate
                    {
                        IsReady = false;
                        if (SamuraiAnim.IsAnimationNotPlaying() && !autoBop)
                        {
                        SamuraiAnim.DoScaledAnimationAsync("SamuraiIdle", 1.0f);
                        }
                    })
                });
                    var SamuraiOoh = SoundByte.PlayOneShotGame("samuraiSliceRvl/ROLLING_START", beat, 1.0f);
                    SamuraiOoh.SetLoopParams(beat + 1, 0.1f);
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                new MultiSound.Sound("samuraiSliceRvl/slice", beat, volume: 1.1f),
                new MultiSound.Sound("samuraiSliceRvl/slice", beat + 0.14f, volume: 1.1f, ignoreSwing: true),
                new MultiSound.Sound("samuraiSliceRvl/slice", beat + 0.28f, volume: 1.1f, ignoreSwing: true),
                new MultiSound.Sound("samuraiSliceRvl/slice", beat + 0.42f, volume: 1.1f, ignoreSwing: true),
                new MultiSound.Sound("samuraiSliceRvl/slice", beat + 0.56f, volume: 1.1f, ignoreSwing: true),
                new MultiSound.Sound("samuraiSliceRvl/slice", beat + 0.70f, volume: 1.1f, ignoreSwing: true),
                new MultiSound.Sound("samuraiSliceRvl/slice", beat + 0.84f, volume: 1.1f, ignoreSwing: true),
                new MultiSound.Sound("samuraiSliceRvl/slice", beat + 1f, volume: 1.1f),
                });
            }
                
        }
        public void Holding(PlayerActionEvent caller, double beat, double length)
        {
            if (PlayerInput.GetIsAction(InputAction_TouchPressing) || PlayerInput.GetIsAction(InputAction_TouchPress))
            {
                IsReady = true;
                SamuraiAnim.DoScaledAnimationAsync("SamuraiReady", 1.0f);
            }
        }
        public void Thunder(double beat, bool thunder)
        {
            thundereffect = thunder;
        }
    }
}