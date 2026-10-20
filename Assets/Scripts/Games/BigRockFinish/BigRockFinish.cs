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
    public static class NtrShortliveLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("bigRockFinish", "Big Rock Finish", "019B95", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        BigRockFinish.instance.Bop(e.beat, e.length, e["booboo"], e["ecto"], e["crowd"], e["bop"], e["auto"]);
                    },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("booboo", true, "Boo-Boo", "Toggle if Boo-Boo (the player) bops."),
                        new Param("ecto", true, "Ecto", "Toggle if Ecto (the green ghost) bops."),
                        //new Param("spooky", false, "Spooky", "Toggle if Spooky (the drummer) bops."),
                        new Param("crowd", true, "Audience", "Toggle if the audience bops."),
                        new Param("bop", true, "Enable Bopping", "Toggle if the Tiny Ghosts and crowd should bop for the duration of this event."),
                        new Param("auto", false, "Bop (Auto)", "Toggle if the ghosts should automatically bop until another Bop event is reached."),
                    }
                },
                new GameAction("drum", "Play Drum Animation")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        BigRockFinish.instance.PlayDrums(e.beat, e["drumType"]);
                    },
                    resizable = false,
                    defaultLength = 0.25f,
                    parameters = new List<Param>()
                    {
                        new Param("drumType", BigRockFinish.DrumType.Kick, "Drum Type", "Pick the drum that Spooky plays.")
                    }
                },
                new GameAction("spotlight", "Spotlight Animation")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        BigRockFinish.instance.SpotlightAnim(e.beat, e["type"]);
                    },
                    resizable = true,
                    defaultLength = 1f,
                    parameters = new List<Param>
                    {
                        new Param("type", BigRockFinish.SpotlightType.None, "Spotlight Type", "Pick the color of the lighting.")
                    }
                },
                //new GameAction("strum", "Strum")
                //{
                //    function = delegate { var e = eventCaller.currentEntity; bigRockFinish.instance.Strum(e.beat, e.length, e["type"], e["jump"]);},
                //    resizable = true, 
                //    //defaultLength = 0.5f,
                //    parameters = new List<Param>()
                //    {
                //        new Param("type", new EntityTypes.Note(0, 3, 4, "bigRockFinish/guitar"), "Semitones", "Set the number of semitones up or down this note should be pitched."),
                //        new Param("jump", false, "Jump?", "Toggle if the Ghosts should jump on the final input")
                //    },
                //},
                new GameAction("countin2", "Big Rock Finish")
                {
                    function = delegate { var e = eventCaller.currentEntity; BigRockFinish.instance.CountinLong(e.beat, e.length, e["note1"], e["note2"], e["note3"], e["note4"], e["short"], e["alt"], e["whiff"], e["cheer"], e["crowdBop"], e["flash"]);},
                    defaultLength = 8f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("short", false, "Short Count-In", "Set if the count-in starts halfway in."),
                        new Param("alt", false, "Alt Count-In", "Set if the count-in uses alternate voice clips."),
                        new Param("flash", true, "Flashing Lights?", "Set if a light flashes onscreen when inputting."),
                        
                        new Param("note1", new EntityTypes.Note(0, 0, 4, "bigRockFinish/guitar"), "1st Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note2", new EntityTypes.Note(0, 0, 4, "bigRockFinish/guitar"), "2nd Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note3", new EntityTypes.Note(0, 0, 4, "bigRockFinish/guitar"), "3rd Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note4", new EntityTypes.Note(0, 0, 4, "bigRockFinish/guitar"), "4th Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("whiff", new EntityTypes.Note(0, 0, 4, "bigRockFinish/guitar"), "Whiff Note", "Set the pitch of the note that plays on an empty input."),
                        new Param("crowdBop", true, "Audience Bop?", "Set if the audience bops along to the inputs."),
                        new Param("cheer", true, "Audience Reaction?", "Set if the audience reacts after the input is complete."),
                    }
                },
                new GameAction("countin1", "Big Rock Finish (Fast)")
                {
                    function = delegate { var e = eventCaller.currentEntity; BigRockFinish.instance.Countin(e.beat, e.length, e["note1"], e["note2"], e["note3"], e["note4"], e["alt"], e["whiff"], e["cheer"], e["crowdBop"], e["flash"]);},
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("alt", true, "Alt", "Set if the count-in uses alternate voice clips."),
                        new Param("flash", true, "Flashing Lights?", "Set if a light flashes onscreen when inputting."),
                        
                        new Param("note1", new EntityTypes.Note(0, 0, 4, "bigRockFinish/guitar"), "1st Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note2", new EntityTypes.Note(0, 0, 4, "bigRockFinish/guitar"), "2nd Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note3", new EntityTypes.Note(0, 0, 4, "bigRockFinish/guitar"), "3rd Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note4", new EntityTypes.Note(0, 0, 4, "bigRockFinish/guitar"), "4th Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("whiff", new EntityTypes.Note(0, 0, 4, "bigRockFinish/guitar"), "Whiff Note", "Set the pitch of the note that plays on an empty input."),
                        new Param("crowdBop", true, "Audience Bop?", "Set if the audience bops along to the inputs."),
                        new Param("cheer", true, "Audience Reaction?", "Set if the audience reacts after the input is complete."),
                    }
                },
                new GameAction("thankyou", "Reaction")
                {
                    function = delegate { var e = eventCaller.currentEntity; BigRockFinish.instance.ThankYou(e.beat, e["alt"]);},
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("alt", false, "Alt Count-In", "Set if the success reaction uses an alternate voice clip."),
                    }
                }
            },
            new List<string>() { "ntr", "aim" },
            "ntrshortlive", "en",
            new List<string>() { },
            chronologicalSortKey: 22,
            font: "FOT-SlumpStd-DB SDF"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    public class BigRockFinish : Minigame
    {
        public static BigRockFinish instance;

        public static PlayerInput.InputAction InputAction_TouchPress =
            new("NtrShortliveTouchPress", new int[] { IAEmptyCat, IAPressCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicPress, IA_Empty);
        public static PlayerInput.InputAction InputAction_TouchRelease =
            new("NtrShortliveTouchRelease", new int[] { IAEmptyCat, IAReleaseCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicRelease, IA_Empty);
        public enum WhoBops
        {
            PlayerGhosts,
            Spooky,
            Audience,
            Both
        }

        public enum CountinType
        {
            Regular,
            Fast
        }
        public enum StrumType
        {
            Regular,
            Jump
        }

        public enum DrumType
        {
            Kick = 0,
            Snare = 1,
            TomLeft = 2,
            TomRight = 3,
            HiHat = 4,
            Cymbal = 5
        }

        public enum SpotlightType
        {
            Red = 0,
            Green = 1,
            Blue = 2,
            None = 3,
        }

        [Header("Animators")]
        [SerializeField] Animator playerGhost;
        [SerializeField] Animator greenGhost;
        [SerializeField] Animator drummerGhost;
        [SerializeField] Animator ghostHandL;
        [SerializeField] Animator ghostHandR;

        [SerializeField] Animator audience;
        [SerializeField] Animator spotlightMask;
        [SerializeField] Animator flash;

        [Header("Drum Animators")]
        [SerializeField] Animator Bass;
        [SerializeField] Animator Cymbal;
        [SerializeField] Animator TomL;
        [SerializeField] Animator TomR;
        [SerializeField] Animator Snare;
        [SerializeField] Animator Hihat;

        [SerializeField] SpriteRenderer UnlitArea;
        bool ghostBop = true;

        private Sound guitarSource;

        bool ghostCanBopTemp = true;
        bool isHolding = false;

        bool ectoBop = true;
        


        bool ghostCanBop = true;
        bool ectoCanBop = true;
        //bool spookyCanBop = false;
        bool crowdCanBop = true;

        bool playerPrepare = false;
        bool ectoPrepare = false;

        bool playerStrum = false;
        bool ectoStrum = false;

        bool crowdReact = true;
        bool crowdIsReacting = false;

        bool flashOn = true;

        bool ectoCanBopTemp = true;

        public bool ghostJump;
        public bool playerInput = false;

        int pitchOne = 0;
        int pitchTwo = 0;
        int pitchThree = 0;
        int pitchFour = 0;

        int whiffPitch = 0;

        float timeScale = 1f;

        float misses;
        
        public void Awake()
        {
            instance = this;
            SetupBopRegion("bigRockFinish", "bop", "auto");
        }
        public void Update()
        {
            double beat = conductor.unswungSongPositionInBeatsAsDouble;
            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !GameManager.instance.Autoplay && PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch)
            {
                playerStrum = false;
                playerPrepare = true;
                playerGhost.DoScaledAnimationAsync("Prepare", 0.5f);
                if (guitarSource != null)
                {
                    guitarSource.KillLoop(0.1f);
                    guitarSource = null;
                }

            }
            if (PlayerInput.GetIsAction(InputAction_BasicRelease) && (!IsExpectingInputNow(InputAction_BasicRelease)) && (!GameManager.instance.Autoplay)  && PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch)
            {
                playerGhost.DoScaledAnimationAsync("Release", 0.5f);
                
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 1, delegate { 
                    playerPrepare = false;
                    }),
                });
            }
            if (PlayerInput.GetIsAction(InputAction_FlickPress) && !IsExpectingInputNow(InputAction_FlickPress))
            {
                playerPrepare = false;
                playerStrum = true;
                playerGhost.DoScaledAnimationAsync("Strum", 0.5f);
                var pitch = SoundByte.GetPitchFromSemiTones(whiffPitch, true);
                guitarSource = SoundByte.PlayOneShotGame("bigRockFinish/guitar", beat, pitch, 1f, true);
                guitarSource.KillLoop(0.25f);
                //PlayGuitar(beat, length, pitch);

                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 1, delegate { 
                    playerStrum = false;
                    }),
                });
            }
        }
        public override void OnLateBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat))
            {
                BopAnimationBooBoo();
                BopAnimationEcto();
                //BopAnimationSpooky();
                BopAnimationCrowd();
            }
        }
        public void Bop(double beat, float length, bool booboo, bool ecto, bool audience, bool bop, bool autoBop)
        {
            ghostCanBop = booboo;
            ectoCanBop = ecto;
            //spookyCanBop = spooky;
            crowdCanBop = audience;

            playerPrepare = false;
            playerStrum = false;
            ectoPrepare = false;
            ectoStrum = false;

            if (bop)
            {
                var actions = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                { 
                    actions.Add(new(beat + i, delegate { BopAnimationBooBoo();}));
                    actions.Add(new(beat + i, delegate { BopAnimationEcto();}));
                    //actions.Add(new(beat + i, delegate { BopAnimationSpooky();}));
                    actions.Add(new(beat + i, delegate { BopAnimationCrowd();}));
                }
                BeatAction.New(this, actions);
            }
        }
        public void BopAnimationBooBoo()
        {
            if (ghostCanBop && !playerPrepare && !playerStrum && (playerGhost.IsAnimationNotPlaying() || playerGhost.IsPlayingAnimationNames("Idle")))
            {playerGhost.DoScaledAnimationAsync("Beat", 0.5f);}
        }
        public void BopAnimationEcto()
        {
            if (ectoCanBop && !ectoPrepare && !ectoStrum && (greenGhost.IsAnimationNotPlaying() || greenGhost.IsPlayingAnimationNames("Idle")))
            greenGhost.DoScaledAnimationAsync("Beat", 0.5f);
        }
        //public void BopAnimationSpooky()
       // {
            //if (spookyCanBop && (drummerGhost.IsAnimationNotPlaying() || drummerGhost.IsPlayingAnimationNames("Idle")))
            //{drummerGhost.DoScaledAnimationAsync("Beat", 0.5f);}
        //}
        public void BopAnimationCrowd()
        {
            if (crowdCanBop && !crowdIsReacting && (audience.IsAnimationNotPlaying() || audience.IsPlayingAnimationNames("Idle")))
            {audience.DoScaledAnimationAsync("Beat", 0.5f);}
        }

        public void Countin(double beat, float length, int noteOne, int noteTwo, int noteThree, int noteFour, bool alt, int whiff, bool cheer, bool crowdBop, bool flash)
        {


            float interval = (float)(beat + length);
            float noteTwoTime = length + (length*.1875f);
            float noteThreeTime = length + (length*0.375f);
            float noteFourTime = length + (length*.5f);
            DoAutoPrepare(beat, (float)(interval+(length*0.125f)), (float)(beat+noteTwoTime+(length*0.125f)), (float)(beat+noteThreeTime+(length*0.0625f)));

            whiffPitch = whiff;

            
            
            

            

            if (alt)
            {
                MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("bigRockFinish/countinOneFast", beat),
                        new MultiSound.Sound("bigRockFinish/countinTwoFast", beat + (length*.25)),
                        new MultiSound.Sound("bigRockFinish/countinThreeFast", beat + (length*.5)),
                        new MultiSound.Sound("bigRockFinish/comeOn", beat + (length*.75)),
                    }, forcePlay: true);
            }
            else
            {
                MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("bigRockFinish/countinOne", beat),
                        new MultiSound.Sound("bigRockFinish/countinTwo", beat + (length*.25)),
                        new MultiSound.Sound("bigRockFinish/countinThree", beat + (length*.5)),
                        new MultiSound.Sound("bigRockFinish/countInFour", beat + (length*.75)),
                    }, forcePlay: true);
            }

            
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action((beat + (length*.75)), delegate { 
                    //ghostBop = false;
                    //ectoBop = false;
                    
                    greenGhost.DoScaledAnimationAsync("Prepare", 0.5f);
                    ectoPrepare = true;
                    DoPrepare();
                    pitchOne = noteOne;pitchTwo = noteTwo;pitchThree = noteThree;pitchFour = noteFour;timeScale = length/8f;
                    crowdReact = cheer; crowdIsReacting = crowdBop; flashOn = flash;
                }),
                new BeatAction.Action(interval, delegate {greenGhost.DoScaledAnimationAsync("Strum", 0.5f);ectoStrum = true;ectoPrepare = false; CheckCrowdInputBop();}),
                new BeatAction.Action(interval+(length*0.125), delegate {greenGhost.DoScaledAnimationAsync("Prepare", 0.5f);ectoStrum = false;ectoPrepare = true;}),
                new BeatAction.Action(beat+noteTwoTime, delegate {greenGhost.DoScaledAnimationAsync("Strum", 0.5f);ectoStrum = true;ectoPrepare = false; CheckCrowdInputBop();}),
                new BeatAction.Action(beat+noteTwoTime+(length*0.125), delegate {greenGhost.DoScaledAnimationAsync("Prepare", 0.5f);ectoStrum = false;ectoPrepare = true;}),
                new BeatAction.Action(beat+noteThreeTime, delegate {greenGhost.DoScaledAnimationAsync("Strum", 0.5f);ectoStrum = true;ectoPrepare = false; CheckCrowdInputBop();}),
                new BeatAction.Action(beat+noteThreeTime+(length*0.0625), delegate {greenGhost.DoScaledAnimationAsync("Prepare", 0.5f);ectoStrum = false;ectoPrepare = true;}),
                new BeatAction.Action(beat+noteFourTime, delegate {greenGhost.DoScaledAnimationAsync("Jump", 0.5f);ectoStrum = true;ectoPrepare = false; CheckCrowdInputBop();}),
                new BeatAction.Action(beat+(length*2), delegate {ectoStrum = false;ectoPrepare = false; crowdIsReacting = false;}),
                new BeatAction.Action(beat+(length*2), delegate {playerStrum = false;playerPrepare = false;}),
            });

            ScheduleInput(beat, length, InputAction_FlickPress, strumSuccessOne, strumMiss, null);
            ScheduleInput(beat, noteTwoTime, InputAction_FlickPress, strumSuccessTwo, strumMiss, null);
            ScheduleInput(beat, noteThreeTime, InputAction_FlickPress, strumSuccessThree, strumMiss, null);
            ScheduleInput(beat, noteFourTime, InputAction_FlickPress, strumSuccessFour, strumMissLast, null);

        }

        public void CheckCrowdInputBop()
        {
            if (crowdIsReacting) {audience.DoScaledAnimationAsync("Beat", 0.5f);}
        }

        public void CheckInputFlash()
        {
            if (flashOn) {flash.DoScaledAnimationAsync("Flash", 0.5f);}
        }

        public void CountinLong(double beat, float length, int noteOne, int noteTwo, int noteThree, int noteFour, bool isShort, bool alt, int whiff, bool cheer, bool crowdBop, bool flash)
        {

            float interval = (float)(beat + length);
            float noteTwoTime = length + (length*.1875f);
            float noteThreeTime = length + (length* 0.375f);
            float noteFourTime = length + (length*.5f);
            DoAutoPrepare(beat, (float)(interval+(length*0.125f)), (float)(beat+noteTwoTime+(length*0.125f)), (float)(beat+noteThreeTime+(length*0.0625f)));



            whiffPitch = whiff;


            ScheduleInput(beat, length, InputAction_FlickPress, strumSuccessOne, strumMiss, null);
            ScheduleInput(beat, noteTwoTime, InputAction_FlickPress, strumSuccessTwo, strumMiss, null);
            ScheduleInput(beat, noteThreeTime, InputAction_FlickPress, strumSuccessThree, strumMiss, null);
            ScheduleInput(beat, noteFourTime, InputAction_FlickPress, strumSuccessFour, strumMissLast, null);
            DoAutoPrepare(beat, (float)(interval+(length*0.125f)), (float)(beat+noteTwoTime+(length*0.125f)), (float)(beat+noteThreeTime+(length*0.0625f)));

            if(alt)
            {
                if (!isShort)
                    {
                        MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("bigRockFinish/countinOneFast", beat),
                        new MultiSound.Sound("bigRockFinish/countinTwoFast", beat + (length*.25)),
                    }, forcePlay: true);
                    }

                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("bigRockFinish/countinOneFast", beat + (length*.5)),
                        new MultiSound.Sound("bigRockFinish/countinTwoFast", beat + (length*.625)),
                        new MultiSound.Sound("bigRockFinish/countinThreeFast", beat + (length*.75)),
                        new MultiSound.Sound("bigRockFinish/comeOn", beat + (length*.875)),
                    }, forcePlay: true);
            }
            else
            {
                if (!isShort)
                    {
                        MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("bigRockFinish/countinOne", beat),
                        new MultiSound.Sound("bigRockFinish/countinTwo", beat + (length*.25)),
                    }, forcePlay: true);
                    }

                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("bigRockFinish/countinOne", beat + (length*.5)),
                        new MultiSound.Sound("bigRockFinish/countinTwo", beat + (length*.625)),
                        new MultiSound.Sound("bigRockFinish/countinThree", beat + (length*.75)),
                        new MultiSound.Sound("bigRockFinish/countinFour", beat + (length*.875)),
                    }, forcePlay: true);
            }

            

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + (length*.875), delegate { 
                    //ghostBop = false;
                    //ectoBop = false;
                    //playerGhost.DoScaledAnimationAsync("Prepare", 0.5f);
                    greenGhost.DoScaledAnimationAsync("Prepare", 0.5f);
                    ectoPrepare = true;
                    DoPrepare();
                    pitchOne = noteOne;pitchTwo = noteTwo;pitchThree = noteThree;pitchFour = noteFour;timeScale = length/8f;
                    crowdReact = cheer; crowdIsReacting = crowdBop; flashOn = flash;
                }),
                new BeatAction.Action(interval, delegate {greenGhost.DoScaledAnimationAsync("Strum", 0.5f);ectoStrum = true;ectoPrepare = false; CheckCrowdInputBop();}),
                new BeatAction.Action(interval+(length*0.125), delegate {greenGhost.DoScaledAnimationAsync("Prepare", 0.5f);ectoStrum = false;ectoPrepare = true;}),
                new BeatAction.Action(beat+noteTwoTime, delegate {greenGhost.DoScaledAnimationAsync("Strum", 0.5f);ectoStrum = true;ectoPrepare = false; CheckCrowdInputBop();}),
                new BeatAction.Action(beat+noteTwoTime+(length*0.125), delegate {greenGhost.DoScaledAnimationAsync("Prepare", 0.5f);ectoStrum = false;ectoPrepare = true;}),
                new BeatAction.Action(beat+noteThreeTime, delegate {greenGhost.DoScaledAnimationAsync("Strum", 0.5f);ectoStrum = true;ectoPrepare = false; CheckCrowdInputBop();}),
                new BeatAction.Action(beat+noteThreeTime+(length*0.0625), delegate {greenGhost.DoScaledAnimationAsync("Prepare", 0.5f);ectoStrum = false;ectoPrepare = true;}),
                new BeatAction.Action(beat+noteFourTime, delegate {greenGhost.DoScaledAnimationAsync("Jump", 0.5f);ectoStrum = true;ectoPrepare = false; CheckCrowdInputBop();}),
                new BeatAction.Action(beat+(length*2), delegate {ectoStrum = false;ectoPrepare = false; crowdIsReacting = false;}),
                new BeatAction.Action(beat+(length*2), delegate {playerStrum = false;playerPrepare = false;}),
            });

            
        }

        public void ThankYou(double beat, bool alt)
        {
            if (misses > 0)
            {
                SoundByte.PlayOneShotGame("bigRockFinish/cough", beat);
            }
            else if (alt)
            {
                SoundByte.PlayOneShotGame("bigRockFinish/thankYouB", beat);
            }
            else
            {
                SoundByte.PlayOneShotGame("bigRockFinish/thankYouA", beat);
            }
            misses = 0f;
        }

        public void DoPrepare()
        {
            if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch && PlayerInput.PlayerHasControl()) return;
            playerPrepare = true;
            playerGhost.DoScaledAnimationAsync("Prepare", 0.5f);
        }

        public void DoAutoPrepare(double beat, float firstPrep, float secondPrep, float thirdPrep)
        {
            if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch && PlayerInput.PlayerHasControl()) return;
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(firstPrep, delegate {playerGhost.DoScaledAnimationAsync("Prepare", 0.5f);playerStrum = false;playerPrepare = true;}),
                new BeatAction.Action(secondPrep, delegate {playerGhost.DoScaledAnimationAsync("Prepare", 0.5f);playerStrum = false;playerPrepare = true;}),
                new BeatAction.Action(thirdPrep, delegate {playerGhost.DoScaledAnimationAsync("Prepare", 0.5f);playerStrum = false;playerPrepare = true;}),
            });
        }

        public void SetNonTouchLoops(Sound sound, float length)
        {
            if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch && PlayerInput.PlayerHasControl()) return;
            sound.SetLoopParams(length, 0.1f);
        }

        public void PlayDrums(double beat, int drum)
        {
            switch (drum)
                        {
                            case (int) DrumType.Kick:
                                drummerGhost.DoScaledAnimationAsync("Beat", 0.5f);
                                Bass.DoScaledAnimationAsync("Hit", 0.5f);
                                break;
                            case (int) DrumType.Snare:
                                ghostHandL.DoScaledAnimationAsync("Hit01", 0.5f);
                                Snare.DoScaledAnimationAsync("Hit", 0.5f);
                                break;
                            case (int) DrumType.TomLeft:
                                ghostHandL.DoScaledAnimationAsync("Hit01", 0.5f);
                                TomL.DoScaledAnimationAsync("Hit", 0.5f);
                                break;
                            case (int) DrumType.TomRight:
                                ghostHandR.DoScaledAnimationAsync("Hit01", 0.5f);
                                TomR.DoScaledAnimationAsync("Hit", 0.5f);
                                break;
                            case (int) DrumType.Cymbal:
                                ghostHandR.DoScaledAnimationAsync("Hit02", 0.5f);
                                Cymbal.DoScaledAnimationAsync("Hit", 0.5f);
                                break;
                            case (int) DrumType.HiHat:
                                ghostHandL.DoScaledAnimationAsync("Hit02", 0.5f);
                                Hihat.DoScaledAnimationAsync("Hit", 0.5f);
                                break;
                            default:               
                                break;
                        }
        }

        public void SpotlightAnim(double beat, int type)
        {
            switch (type)
                {
                    case (int) SpotlightType.Red:
                        spotlightMask.DoScaledAnimationAsync("Red", 0.5f);
                        UnlitArea.color = new Color(1f,0f,0f,.4f);
                        break;
                    case (int) SpotlightType.Green:
                        spotlightMask.DoScaledAnimationAsync("Green", 0.5f);
                        UnlitArea.color = new Color(0f,1f,0f,.4f);
                        break;
                    case (int) SpotlightType.Blue:
                        spotlightMask.DoScaledAnimationAsync("Blue", 0.5f);
                        UnlitArea.color = new Color(0f,0f,1f,.4f);
                        break;
                    case (int) SpotlightType.None:
                        spotlightMask.DoScaledAnimationAsync("None", 0.5f);
                        UnlitArea.color = new Color(0f,0f,0f,.4f);
                        break;
                    default:
                        break;
                }
        }

        public void Strum(double beat, float length, int pitch, bool ghostJump)
        {
            var guitarPitch = SoundByte.GetPitchFromSemiTones(pitch, true);
            guitarSource = SoundByte.PlayOneShotGame("bigRockFinish/guitar", -1, guitarPitch, 1f, true);

            guitarSource.SetLoopParams(beat + length, 0.1f);

            //this.ghostJump = ghostJump;

            playerStrum = true;
            ectoStrum = true;
            playerPrepare = false;
            ectoPrepare = false;
            //if(this.ghostJump)
            //{
            //    greenGhost.DoScaledAnimationAsync("Jump", 0.5f);
            //}
            //else
            //{
            //    greenGhost.DoScaledAnimationAsync("Strum", 0.5f);
            //}
            //ScheduleInput(beat, 0f, InputAction_BasicPress, strumSuccess, strumMiss, Empty);
        }



        public void strumSuccessOne(PlayerActionEvent caller, float state)
        {
            playerStrum = true;
            ectoStrum = true;
            playerPrepare = false;
            ectoPrepare = false;

            double hitBeat = Conductor.instance.songPositionInBeatsAsDouble;
            playerGhost.DoScaledAnimationAsync("Strum", 0.5f);

            if (state <= -1f || state >= 1f)
            {
                var pitch = SoundByte.GetPitchFromSemiTones(pitchOne, true);
                guitarSource = SoundByte.PlayOneShotGame("bigRockFinish/guitar", hitBeat, ((float)pitch+0.01f), 1f, true);
                
                SetNonTouchLoops(guitarSource, (float)(hitBeat + (timeScale *0.5f)));
                SoundByte.PlayOneShotGame("bigRockFinish/cymbal", hitBeat);
                CheckInputFlash();
                
            }
            else
            {
                var pitch = SoundByte.GetPitchFromSemiTones(pitchOne, true);
                guitarSource = SoundByte.PlayOneShotGame("bigRockFinish/guitar", hitBeat, pitch, 1f, true);
                SetNonTouchLoops(guitarSource, (float)(hitBeat + (timeScale *0.5f)));
                SoundByte.PlayOneShotGame("bigRockFinish/yeahA", hitBeat);
                SoundByte.PlayOneShotGame("bigRockFinish/cymbal", hitBeat);
                CheckInputFlash();
            }
            
            
        }
        public void strumSuccessTwo(PlayerActionEvent caller, float state)
        {
            double hitBeat = Conductor.instance.songPositionInBeatsAsDouble;
            
            if (state <= -1f || state >= 1f)
            {
                var pitch = SoundByte.GetPitchFromSemiTones(pitchTwo, true);
                guitarSource = SoundByte.PlayOneShotGame("bigRockFinish/guitar", hitBeat, ((float)pitch+0.01f), 1f, true);
                SetNonTouchLoops(guitarSource, (float)(hitBeat + (timeScale *0.5f)));
                SoundByte.PlayOneShotGame("bigRockFinish/cymbal", hitBeat);
                
            }
            else
            {
                var pitch = SoundByte.GetPitchFromSemiTones(pitchTwo, true);
                guitarSource = SoundByte.PlayOneShotGame("bigRockFinish/guitar", hitBeat, pitch, 1f, true);
                SetNonTouchLoops(guitarSource, (float)(hitBeat + (timeScale *0.5f)));
                SoundByte.PlayOneShotGame("bigRockFinish/yeahA", hitBeat);
                SoundByte.PlayOneShotGame("bigRockFinish/cymbal", hitBeat);
                CheckInputFlash();
            }
            
            playerGhost.DoScaledAnimationAsync("Strum", 0.5f);
        }
        public void strumSuccessThree(PlayerActionEvent caller, float state)
        {
            double hitBeat = Conductor.instance.songPositionInBeatsAsDouble;

            if (state <= -1f || state >= 1f)
            {
                var pitch = SoundByte.GetPitchFromSemiTones(pitchThree, true);
                guitarSource = SoundByte.PlayOneShotGame("bigRockFinish/guitar", hitBeat, ((float)pitch+0.01f), 1f, true);
                SetNonTouchLoops(guitarSource, (float)(hitBeat + (timeScale *0.5f)));
                SoundByte.PlayOneShotGame("bigRockFinish/cymbal", hitBeat);
                CheckInputFlash();
            }
            else
            {
                var pitch = SoundByte.GetPitchFromSemiTones(pitchThree, true);
                guitarSource = SoundByte.PlayOneShotGame("bigRockFinish/guitar", hitBeat, pitch, 1f, true);
                SetNonTouchLoops(guitarSource, (float)(hitBeat + (timeScale *0.5f)));
                SoundByte.PlayOneShotGame("bigRockFinish/yeahA", hitBeat);
                SoundByte.PlayOneShotGame("bigRockFinish/cymbal", hitBeat);
                CheckInputFlash();
            }
            playerGhost.DoScaledAnimationAsync("Strum", 0.5f);
        }
        public void strumSuccessFour(PlayerActionEvent caller, float state)
        {
            playerStrum = true;
            ectoStrum = true;
            playerPrepare = false;
            ectoPrepare = false;

            double hitBeat = Conductor.instance.songPositionInBeatsAsDouble;

            if (state <= -1f || state >= 1f)
            {
                var pitch = SoundByte.GetPitchFromSemiTones(pitchFour, true);
                guitarSource = SoundByte.PlayOneShotGame("bigRockFinish/guitar", hitBeat, ((float)pitch+0.01f), 1f, true);
                guitarSource.SetLoopParams((float)(hitBeat + (2*timeScale)), (float)(4*timeScale));
                SetNonTouchLoops(guitarSource, (float)(hitBeat + (timeScale *4f)));
                SoundByte.PlayOneShotGame("bigRockFinish/cymbal", hitBeat);
                CheckInputFlash();
            }
            else
            {
                var pitch = SoundByte.GetPitchFromSemiTones(pitchFour, true);
                guitarSource = SoundByte.PlayOneShotGame("bigRockFinish/guitar", hitBeat, pitch, 1f, true);
                guitarSource.SetLoopParams((float)(hitBeat + (2*timeScale)), (float)(4*timeScale));
                SetNonTouchLoops(guitarSource, (float)(hitBeat + (timeScale * 4f)));
                SoundByte.PlayOneShotGame("bigRockFinish/yeahB", hitBeat);
                SoundByte.PlayOneShotGame("bigRockFinish/cymbal", hitBeat);
                CheckInputFlash();
            }
                if (crowdReact)
                {
                    if (misses == 0)
                    {
                        SoundByte.PlayOneShotGame("bigRockFinish/cheering", hitBeat+0.5f);
                        BeatAction.New(instance, new List<BeatAction.Action>()
                            {
                                new BeatAction.Action(hitBeat, delegate { audience.DoScaledAnimationAsync("Beat", 0.5f);}),
                                new BeatAction.Action(hitBeat + (1f * timeScale), delegate { audience.DoScaledAnimationAsync("Cheer", 0.5f);}),
                                new BeatAction.Action(hitBeat + (5f * timeScale), delegate { audience.DoScaledAnimationAsync("Idle", 0.5f);}),
                            });
                    }
                    else
                    {
                        SoundByte.PlayOneShotGame("bigRockFinish/boo", hitBeat+0.5f);
                        BeatAction.New(instance, new List<BeatAction.Action>()
                            {
                                new BeatAction.Action(hitBeat + .5, delegate { 
                                audience.DoScaledAnimationAsync("Miss", 0.5f);
                                }),
                            });
                    }
                }
                
            
                
            playerGhost.DoScaledAnimationAsync("Jump", 0.5f);

        }
        public void strumMiss(PlayerActionEvent caller)
        {
            ghostBop = false;
            ectoBop = false;
            playerGhost.DoScaledAnimationAsync("Release", 0.5f);
            
            misses++;
        }
        public void strumMissLast(PlayerActionEvent caller)
        {
            double hitBeat = Conductor.instance.songPositionInBeatsAsDouble;
            ghostBop = false;
            ectoBop = false;
            playerGhost.DoScaledAnimationAsync("Release", 0.5f);
            misses++;
            if (crowdReact) SoundByte.PlayOneShotGame("bigRockFinish/boo", hitBeat);
            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(hitBeat + .5, delegate { 
                    audience.DoScaledAnimationAsync("Miss", 0.5f);
                    }),
                });
        }
        public void Empty(PlayerActionEvent caller) {}
    }
}