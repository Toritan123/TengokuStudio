using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;
using System.Runtime.InteropServices;

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
    public static class AgbRhythmTestGBALoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("rhythmTestGBA", "Rhythm Test \n<color=#adadad>(Rhythm-kan Check)</color>", "2DD816", false, false, new List<GameAction>()
            {

                new GameAction("countin", "Set Beeping")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity;
                        RhythmTestGBA.instance.KTBPrep(e.beat, e.length, e["toggle"], e["auto"], e["image"],
                            e["textFlash"], e["textDisplay"], e["hasSound"]); 
                    },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Toggle", "Toggle the beeping on or off."),
                        new Param("auto", false, "Auto", "Toggle if the machine should beep automatically."),
                        new Param("image", RhythmTestGBA.PulseOption.Note, "Screen Image", "Set what appears on the machine's screen.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x == (int)RhythmTestGBA.PulseOption.Text, new string[] { "textFlash", "textDisplay" }),
                        }),
                        new Param("textDisplay", "Get ready...", "Text to Display", "Changes the text displayed on the screen."),
                        new Param("textFlash", true, "Text Flash", "Toggle if the text on the screen pulses to the beat."),
                        new Param("hasSound", true, "Has Sound", "Toggle if the beeping plays sound or not.")
                        
                    },
                },

                new GameAction("button", "Start Pressing")
                {
                    preFunction = delegate { 
                        var e = eventCaller.currentEntity; 
                        RhythmTestGBA.PreStartKeepbeat(e.beat, e.length); 
                    },
                    preFunctionLength = 1f,
                    defaultLength = 1f,
                    resizable = false,

                },

                new GameAction("stopktb", "Stop Pressing")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity;
                        RhythmTestGBA.instance.StopKeepbeat(e.beat, e.length, e["mutecue"], e["finishText"], e["textDisplay"]); 
                    },
                    defaultLength = 4f,
                    resizable = false,
                    parameters = new List<Param>()
                    {
                        new Param("mutecue", false, "Mute Cue", "Mute the sound cue signifying the end of the keep-the-beat section."),
                        new Param("finishText", true, "Finish Text", "Set if text appears once the keep-the-beat section is finished.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "textDisplay" }),
                        }),
                        new Param("textDisplay", "Test complete!", "Text to Display", "Changes the text displayed on the screen."),
                    }
                },

                new GameAction("countdown", "Countdown Press")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        RhythmTestGBA.instance.PreCountDown(e.beat, e.length, e["val1"]);
                    },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("val1", new EntityTypes.Integer(1, 9, 3), "Beats", "Set how many beats there will be before the player has to input.")                        
                    }
                },

                new GameAction("hidecount", "Toggle Countdown")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        RhythmTestGBA.instance.HideCountdown(e["togglecount"]);
                    },
                    defaultLength = 0.5f,
                    resizable = false,
                    parameters = new List<Param>()
                    {
                        new Param("togglecount", true, "Toggle Countdown?", "Toggles whether the countdown appears or not, for skipped beats."),
                    }

                },

            },
            new List<string>() {"agb", "keep", "aim", "intro"},
            "agbrhythmtest", "en",
            new List<string>() { "en" },
            chronologicalSortKey: 0,
            font: "Comet"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using System.Threading;
    using NUnit.Framework.Internal;

    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
    using UnityEngine.InputSystem.EnhancedTouch;

    public class RhythmTestGBA : Minigame
    {
        public static RhythmTestGBA instance;
        

        bool goBeep;
        bool stopAutoBeep;
        bool keepPressing;
        bool shouldmute;
        bool disableCount;
        bool beatPulseTextFlash = false;
        bool canBeep = false;
        double beepBeat = -1;
        double stopBeat = double.MaxValue;
        //bool isBeeping = false;

        PlayerActionEvent currentScheduledInput;

        int screenFXType;


        [Header("Objects")]
        [SerializeField] GameObject noteFlash;
        [SerializeField] TMP_Text screenText;

        [Header("Animators")]
        [SerializeField] Animator buttonAnimator;
        [SerializeField] Animator flashAnimator;
        [SerializeField] Animator numberBGAnimator;
        [SerializeField] Animator numberAnimator;
        [SerializeField] Animator textAnimator;


        public enum PulseOption
        {
            Note,
            Text
        }

        public enum BeepNoise: int
        {
            defaultBeep = 0,
            countBeep = 1,
            dingBeep = 2,
            countBeepHigh = 3
        }

        public BeepNoise beepType = BeepNoise.defaultBeep;

        private void Awake()
        {
            instance = this;
            screenText.text = "";
            SetupBopRegion("rhythmTestGBA", "countin", "auto");
            var currentBeat = Conductor.instance.unswungSongPositionInBeatsAsDouble;
            KeepTheBeep(currentBeat, 1f, false, false, 0, true, false);
        }

        void OnDestroy()
        {
            foreach (var ktb in scheduledInputs)
            {
                ktb.Disable();
            }
        }
    

        private void Update()
        {
            var cond = Conductor.instance;

            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            {
                PressButton();
            }
        }

        public override void OnGameSwitch(double beat)
        {
            RiqEntity e = gameManager.Beatmap.Entities.FindLast(e => e.beat - 1 < beat && e.beat > beat && e.datamodel == "rhythmTestGBA/button");
            if (e != null)
            {
                PreStartKeepbeat(e.beat, e.length); 
            }
        }

        public void KTBPrep(double beat, float length, bool shouldBeep, bool autoBeep, int type, bool textFlash, string textDisplay, bool hasSound)
        {
            canBeep = autoBeep;
            if (type == 1)
            {
                screenText.text = textDisplay;
            }
            beepType = 0;
            screenFXType = type;
            KeepTheBeep(beat, length, shouldBeep, autoBeep, type, textFlash, hasSound);
        }

        public void KeepTheBeep(double beat, float length, bool shouldBeep, bool autoBeep, int type, bool textFlash, bool hasSound)
        {
            beatPulseTextFlash = textFlash;
            
            if (shouldBeep)
            {
                List<BeatAction.Action> beeps = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                {
                    int step = i;
                    beeps.Add(new BeatAction.Action(beat + step, delegate { 
                        PlayFlashFX(beat + step);
                        if (hasSound) Beep(beat + step, BeepNoise.defaultBeep);
                    }));
                }
                BeatAction.New(instance, beeps);
            }
        }

        public void PlayFlashFX(double beat)
        {
            numberAnimator.Play("Idle");
            numberBGAnimator.Play("Idle");

            if (screenFXType == 0)
            {
                noteFlash.SetActive(true);
                flashAnimator.DoScaledAnimationAsync("KTBPulse", 0.5f);
                screenText.text = "";
            }
            else if (beatPulseTextFlash)
            {
                noteFlash.SetActive(false);
                textAnimator.DoScaledAnimationAsync("TextFlash", 0.5f);
            }
            else
            {
                noteFlash.SetActive(false);
                textAnimator.DoScaledAnimationAsync("TextIdle", 0.5f);
            }

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat+0.9f, delegate {noteFlash.SetActive(false);})
            });
        }

        public void PlayFakeFlashFX(int fakeFXType, bool textFlash)
        {
            numberAnimator.Play("Idle");
            numberBGAnimator.Play("Idle");
            
            var currentBeat = Conductor.instance.songPositionInBeatsAsDouble;
            if (fakeFXType == 0)
            {
                noteFlash.SetActive(true);
                flashAnimator.DoScaledAnimationAsync("KTBPulse", 0.5f);
                screenText.text = "";
            }
            else if (textFlash){
                noteFlash.SetActive(false);
                textAnimator.DoScaledAnimationAsync("TextFlash", 0.5f);
            }
            else
            {
                noteFlash.SetActive(false);
                textAnimator.DoScaledAnimationAsync("TextIdle", 0.5f);
            }

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                //new BeatAction.Action(beat-1, delegate {KillBeeps(beat);}),
                new BeatAction.Action(currentBeat+0.9f, delegate {noteFlash.SetActive(false);})
            });
        }
        

        public void ChangeText(double beat, string newText, bool newTextFlash)
        {
            screenText.text = newText;
            beatPulseTextFlash = newTextFlash;
        }


        public void PressButton()
        {
            SoundByte.PlayOneShotGame("rhythmTestGBA/press");

            buttonAnimator.DoScaledAnimationAsync("Press", 0.5f);
        }

        public void StopKeepbeat(double beat, float length, bool shouldmute, bool hasFinish, string finishText)
        {
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { 
                    PlayFlashFX(beat); 
                    stopBeat = beat + 3;
                }),  
                new BeatAction.Action(beat + 1, delegate { 
                    PlayFlashFX(beat + 1); 
                }),                 
                new BeatAction.Action(beat + 2, delegate { 
                    PlayFlashFX(beat + 2);
                }), 
                new BeatAction.Action(beat + 3, delegate { 
                    if (hasFinish)
                    {
                        screenText.text = finishText;
                        textAnimator.DoScaledAnimationAsync("TextIdle", 0.5f);
                    }
                }),
            });

            Beep(beat,     BeepNoise.countBeep);
            Beep(beat + 1, BeepNoise.countBeep);
            Beep(beat + 2, BeepNoise.countBeep);
            Beep(beat + 3, BeepNoise.dingBeep);
        }

        public override void OnLateBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat) && canBeep && beat + 3 < stopBeat)
            {
                PlayFlashFX(beat);
                Beep(beat, beepType);
            }
        }

        public static void PreStartKeepbeat(double beat, float length)
        {
            if (GameManager.instance.currentGame == "rhythmTestGBA")
            {
                instance.StartKeepbeat(beat, length);
            }
        }

        public void StartKeepbeat(double beat, float length)
        {
            stopBeat = double.MaxValue;
            RecursiveKeepBeat(beat);
        }

        public void RecursiveKeepBeat(double beat)
        {
            if (beat <= stopBeat)
            {
                currentScheduledInput = ScheduleInput(beat - 1, 1f, InputAction_BasicPress, ButtonSuccess, ButtonFailure, ButtonEmpty);

                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate
                    {
                        if (gameManager.currentGame != "rhythmTestGBA") return;
                        RecursiveKeepBeat(beat + 1);
                    }),
                });
            }
        }

        public void HideCountdown(bool toggleCount)
        {
            if (toggleCount)
            {
                disableCount = false;
            }
            else
            {
                disableCount = true;
            }
        }

        public void PreCountDown(double startBeat, float length, int countdownNumber)
        {
            if (keepPressing) return;
            ScheduleInput(startBeat, length * countdownNumber, InputAction_BasicPress, ButtonSuccess, ButtonFailure, ButtonEmpty);

            List<RiqEntity> relevantEvents = GetAllTogglesInBetweenBeat(startBeat, startBeat + (length * countdownNumber));
            relevantEvents.Sort((x, y) => x.beat.CompareTo(y.beat));

            var actions = new List<BeatAction.Action>();
            for (int i = 0; i <= countdownNumber; i++)
            {
                int step = i;
                int number = countdownNumber - step;
                double beat = startBeat + (length * step);

                bool enabled = true;
                enabled = relevantEvents.FindLast(e => e.beat <= beat)?["togglecount"] ?? true;

                if (enabled || number == 0) Beep(beat, number == 0 ? BeepNoise.countBeepHigh : BeepNoise.countBeep);
                actions.Add(new(beat, delegate 
                { 
                    FlashNumber(number, !enabled); 
                }));
            }
            BeatAction.New(this, actions);
        }

        private List<RiqEntity> GetAllTogglesInBetweenBeat(double beat, double endBeat)
        {
            List<RiqEntity> toggleEvents = EventCaller.GetAllInGameManagerList("rhythmTestGBA", new string[] { "hidecount" });
            List<RiqEntity> tempEvents = new();

            foreach (var entity in toggleEvents)
            {
                if (entity.beat >= beat && entity.beat <= endBeat)
                {
                    tempEvents.Add(entity);
                }
            }
            return tempEvents;
        }

        public void FlashNumber(int number, bool disableCount)
        {
            if (!disableCount)
            {
                numberBGAnimator.DoScaledAnimationAsync("FlashBG", 0.5f);
                numberAnimator.DoScaledAnimationAsync(NumberName(number), 0.5f);
            }
            else
            {
                numberBGAnimator.Play("Idle");
                numberAnimator.Play("Idle");                
            }
        }

        public string NumberName(int number)
        {
            switch (number)
            {
                case 1:
                    return "One";
                case 2:
                    return "Two";
                case 3:
                    return "Three";
                case 4:
                    return "Four";
                case 5:
                    return "Five";
                case 6:
                    return "Six";
                case 7:
                    return "Seven";
                case 8:
                    return "Eight";
                case 9:
                    return "Nine";
                default:
                    return "Zero";
            }
        }


        public void ButtonSuccess(PlayerActionEvent caller, float state)
        {
            PressButton();
        }

        public void ButtonFailure(PlayerActionEvent caller)
        {
        }
        public void ButtonEmpty(PlayerActionEvent caller) 
        {
        }

        public void Beep(double beat, BeepNoise type)
        {
            if (beepBeat >= beat) return;   // prevent double beeps
            beepBeat = beat;

            switch (type)
            {
                case BeepNoise.defaultBeep:
                    SoundByte.PlayOneShotGame("rhythmTestGBA/blip", beat, volume: 0.667f);
                    break;
                case BeepNoise.countBeep:
                    SoundByte.PlayOneShotGame("rhythmTestGBA/blip2", beat, volume: 0.667f);
                    break;
                case BeepNoise.countBeepHigh:
                    SoundByte.PlayOneShotGame("rhythmTestGBA/blip3", beat, volume: 0.667f);
                    break;
                case BeepNoise.dingBeep:
                    SoundByte.PlayOneShotGame("rhythmTestGBA/end_ding", beat, volume: 0.667f);
                    break;
            }
        }
    }
}


