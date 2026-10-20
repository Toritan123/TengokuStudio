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

    /// Platform codes are as follows:
    /// Agb: Gameboy Advance    ("Advance Gameboy")
    /// Ntr: Nintendo DS        ("Nitro")
    /// Rvl: Nintendo Wii       ("Revolution")
    /// Ctr: Nintendo 3DS       ("Centrair")
    /// Mob: Mobile
    /// Pco: PC / Other

    /// Fill in the loader class label, "*prefab name*", and "*Display Name*" with the relevant information
    /// For help, feel free to reach out to us on our discord, in the #development channel.
    public static class RvlVersusLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            RiqEntity IgnoreDisplayUpdater(string datamodel, RiqEntity entity)
            {
                if (datamodel == "rhythmFighter/start interval")
                {
                    if (entity.version == 0)
                    {
                        entity.CreateProperty("ignoreDisplay", true);
                        entity.version = 1;
                    }
                    return entity;
                }
                return null;
            }
            RiqBeatmap.OnUpdateEntity += IgnoreDisplayUpdater;

            return new Minigame("rhythmFighter", "Rhythm Fighter", "ffffff", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out RhythmFighter instance)) {
                            instance.Bop(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity["whoBops"], eventCaller.currentEntity["whoBopsAuto"]);
                        }
                    },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("whoBops", RhythmFighter.WhoBops.Both, "Bop", "Set the character(s) to bop for the duration of this event."),
                        new Param("whoBopsAuto", RhythmFighter.WhoBops.None, "Bop (Auto)", "Set the character(s) to automatically bop until another Bop event is reached."),
                    }
                },
                new GameAction("start interval", "Start Interval")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.TryGetMinigame(out RhythmFighter instance)) {
                            instance.QueueStartInterval(e.beat-1, e.length, e["autoPass"], e["display"], e["type"], e["ignoreDisplay"]);
                            //instance.FightersFall(e.beat-4);
                        }
                    },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new("autoPass", true, "Auto Pass Turn", "Toggle if the turn should be passed automatically at the end of the start interval."),
                        new("display", true, "Note Display", "Toggle if the Note Display should be active.", new()
                        {
                            new((x, _) => (bool)x, "type"),
                        }),
                        new("type", RhythmFighter.DisplayType.Normal, "Type", "Choose the type of the Note Display."),
                        new("ignoreDisplay", false, "Ignore Note Display Timing", "Allow the placing of inputs regardless of the Note Display timing.", advanced: true),
                    },
                    defaultVersion = 1
                },
                new GameAction("punch", "Punch")
                {
                    defaultLength = 0.5f,
                },
                new GameAction("kick", "Kick")
                {
                    defaultLength = 0.5f,
                },
                new GameAction("pass turn", "Pass Turn")
                {
                    preFunction = delegate
                    {
                        SoundByte.PlayOneShotGame("rhythmFighter/ready", eventCaller.currentEntity.beat, forcePlay: true);
                        //SoundByte.PlayOneShotGame("rhythmFighter/ready2", eventCaller.currentEntity.beat+0.5);
                        SoundByte.PlayOneShotGame("rhythmFighter/fight", eventCaller.currentEntity.beat+2, forcePlay: true);

                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.TryGetMinigame(out RhythmFighter instance)) {
                            instance.PassTurn(e.beat, e["display"], e["type"]);
                        }
                    },
                    //resizable = true,
                    //preFunctionLength = 1f
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new("display", true, "Note Display", "Toggle if the Note Display should be active.", new()
                        {
                            new((x, _) => (bool)x, "type"),
                        }),
                        new("type", RhythmFighter.DisplayType.Normal, "Type", "Choose the type of the Note Display.")
                    }
                },
                new GameAction("display", "Show Note Display")
                {
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.TryGetMinigame(out RhythmFighter instance)) {
                            instance.Display(e.beat-1, e.length, e["type"], e["delay"]);
                        }
                    },
                    resizable = true,
                    preFunctionLength = 1f,
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new("type", RhythmFighter.DisplayType.Normal, "Type", "Choose the type of the Note Display."),
                        new("delay", new EntityTypes.Float(0f, 100f, 1f), "Delay", "Set the delay before the Note Display disappears")
                    }
                },
                new GameAction("fall", "Return Position")
                {
                    function = delegate
                    {
                        if (eventCaller.gameManager.TryGetMinigame(out RhythmFighter instance)) {
                            instance.FightersFall(eventCaller.currentEntity.beat);
                        }
                    },
                    defaultLength = 4f
                }
            },
            new List<string>() { "rvl", "repeat" },
            "rvlversus", "en",
            new List<string>() { },
            chronologicalSortKey: 204,
            font : "Comet"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
    public class RhythmFighter : Minigame
    {
        public enum WhoBops
        {
            Both = 0,
            Opponent = 1,
            Player = 2,
            None = 3,
        }

        public enum DisplayType
        {
            Normal = 0,
            Quick = 1,
        }

        public enum DisplayTypeX
        {
            Automatic = 0,
            Normal = 1,
            Quick = 2
        }

        [SerializeField] private Animator fighterR;
        [SerializeField] private Animator fighterL;
        [SerializeField] private Animator holderR;
        [SerializeField] private Animator holderL;
        [SerializeField] private Animator displayHolderAnim;
        [SerializeField] private GameObject displayHolder;
        [SerializeField] private GameObject musicNote;
        [SerializeField] private Animator lightsL;
        [SerializeField] private Animator lightsR;
        [SerializeField] private Animator fightText;
        [SerializeField] private Animator spotLight;

        private List<RiqEntity> callEntities;
        private bool fighterDistance = true;
        private bool canDodge = false;

        private bool lBop = true;
        private bool rBop = true;
        private bool canBop = true;

        const int IA_AltPress = IAMAXCAT;
        protected static bool IA_TouchPress(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt);
        }

        protected static bool IA_PadAltPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltPress(out double dt)
        {
            return PlayerInput.GetSqueezeDown(out dt);
        }

        public static PlayerInput.InputAction InputAction_Nrm =
            new("RvlVersusNrm", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_PadBasicPress, IA_TouchPress, IA_BatonBasicPress);
        public static PlayerInput.InputAction InputAction_Alt =
            new("RvlVersusAlt", new int[] { IA_AltPress, IA_AltPress, IA_AltPress },
            IA_PadAltPress, IA_TouchPress, IA_BatonAltPress);

        private void Update()
        {
            if (PlayerInput.GetIsAction(InputAction_Nrm) && !IsExpectingInputNow(InputAction_Nrm) && canDodge)
            {
                if (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch || !IsExpectingInputNow(InputAction_Alt))
                {
                    fighterR.DoScaledAnimationAsync("Whiff", 0.5f);
                }
            }
            if (PlayerInput.GetIsAction(InputAction_Alt) && !IsExpectingInputNow(InputAction_Alt) && PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch && canDodge)
            {
                fighterR.DoScaledAnimationAsync("Whiff", 0.5f);
            }
        }

        public override void OnGameSwitch(double beat)
        {
            StoreAllCallEntities();
            DoInactivePassTurns(beat, false);
            DoInactiveStartInterval(beat, false);
            DoFightersFall(beat);

            RiqEntity displayEntity = gameManager.Beatmap.Entities.FindLast(e => (e.beat - 1) < beat && e.beat + e.length >= beat && e.datamodel == "rhythmFighter/display");
            if (displayEntity != null)
            {
                RiqEntity e = displayEntity;
                Display(e.beat - 1, e.length, e["type"], e["delay"]);
            }
        }

        public override void OnPlay(double beat)
        {
            StoreAllCallEntities();
            DoInactivePassTurns(beat, true);
            DoInactiveStartInterval(beat, true);
            DoFightersFall(beat);
        }

        public override void OnLateBeatPulse(double beat)
        {
            if (lBop) DoBop(true);
            if (rBop) DoBop(false);
        }

        private void StoreAllCallEntities()
        {
            callEntities = gameManager.Beatmap.Entities.FindAll(e => e.datamodel == "rhythmFighter/punch" || e.datamodel == "rhythmFighter/kick");
        }

        private void DoInactiveStartInterval(double beat, bool fromPlay)
        {
            RiqEntity startIntervalEntity = gameManager.Beatmap.Entities.FindLast(e => (fromPlay ? e.beat : e.beat - 2) < beat && e.beat + e.length >= beat && e.datamodel == "rhythmFighter/start interval");
            //Debug.Log("startIntervalEntity.beat : " + (startIntervalEntity?.beat ?? -1));
            if (startIntervalEntity != null)
            {
                RiqEntity e = startIntervalEntity;
                QueueStartInterval(e.beat-1, e.length, e["autoPass"], e["display"], e["type"], e["ignoreDisplay"]);
            }
        }

        private void DoInactivePassTurns(double beat, bool fromPlay)
        {
            RiqEntity passTurnEntity = gameManager.Beatmap.Entities.FindLast(e => (fromPlay ? e.beat : e.beat - 2) < beat && e.beat + e.length >= beat && e.datamodel == "rhythmFighter/pass turn");
            if (passTurnEntity != null)
            {
                RiqEntity e = passTurnEntity;
                bool inactive = true;
                if (!fromPlay) inactive = !(passTurnEntity.beat >= beat);
                PassTurn(e.beat, e["display"], e["type"], inactive);
            }
        }

        public void Bop(double beat, float length, int whoBops, int whoBopsAuto)
        {
            lBop = (whoBopsAuto == 0 || whoBopsAuto == 1);
            rBop = (whoBopsAuto == 0 || whoBopsAuto == 2);

            List<BeatAction.Action> actions = new();
            for (int i = 0; i < length; i++) 
            {
                actions.Add(new(beat + i, delegate 
                {
                    if (whoBops == 0 || whoBops == 1) DoBop(true);
                    if (whoBops == 0 || whoBops == 2) DoBop(false);
                }));
            }
            BeatAction.New(this, actions);
        }

        private void DoBop(bool opponent)
        {
            if (canBop && fighterDistance) 
            {
                if (opponent)
                {
                    if (holderL.IsAnimationNotPlaying() || holderL.IsPlayingAnimationNames("Idle")) fighterL.DoScaledAnimationAsync("Bop", 0.5f);
                }
                else
                {
                    if (holderR.IsAnimationNotPlaying() || holderR.IsPlayingAnimationNames("Idle")) fighterR.DoScaledAnimationAsync("Bop", 0.5f);
                }
            }
        }

        private void DoFightersFall(double beat)
        {
            List<RiqEntity> startIntervals = gameManager.Beatmap.Entities.FindAll(e => e.datamodel == "rhythmFighter/start interval" && e.beat >= beat);
            List<BeatAction.Action> actions = new();

            foreach (RiqEntity interval in startIntervals)
            {
                actions.Add(new(interval.beat - 4, () =>
                {
                    FightersFall(interval.beat - 4);
                }));
            }

            _ = BeatAction.New(this, actions);
        }

        public void FightersFall(double beat)
        {
            List<BeatAction.Action> actions = new();
            if (!fighterDistance)
            {
                actions.Add(new(beat, () => {
                    holderL.DoScaledAnimationAsync("Fall", 0.5f);
                    holderR.DoScaledAnimationAsync("Fall", 0.5f);
                    fighterDistance = true;
                }));
                actions.Add(new(beat, () => {
                    fighterL.DoScaledAnimationAsync("Retreat", 0.5f);
                    fighterR.DoScaledAnimationAsync("Retreat", 0.5f);
                }));
                actions.Add(new(beat+1, () => {
                    fighterL.DoScaledAnimationAsync("Retreat", 0.5f);
                    fighterR.DoScaledAnimationAsync("Retreat", 0.5f);
                }));
                actions.Add(new(beat+2, () => {
                    fighterL.DoScaledAnimationAsync("Retreat", 0.5f);
                    fighterR.DoScaledAnimationAsync("Retreat", 0.5f);
                }));
            }
            _ = BeatAction.New(this, actions);
        }

        public void QueueStartInterval(double beat, float length, bool autoPass, bool noteDisplay, int type, bool ignoreDisplay, double startBeat = double.MinValue)
        {
            List<RiqEntity> neededCalls = GetNeededCalls(beat+1, length);
            if (neededCalls.Count <= 0) return;
            

            if (startBeat < beat + 1 + length)
            {
                List<MultiSound.Sound> sounds = new();
                List<BeatAction.Action> actions = new();

                if (noteDisplay) { actions.Add(new(beat, () => { Display(beat, length, type, 0, true, true); })); }
                /*
                actions.Add(new(beat-4, () => {
                    if (!fighterDistance)
                    {
                        holderL.DoScaledAnimationAsync("Fall", 0.5f);
                        holderR.DoScaledAnimationAsync("Fall", 0.5f);
                        fighterDistance = true;
                    }
                }));
                */

                actions.Add(new(beat, () => { canBop = false; }));

                foreach (RiqEntity call in neededCalls)
                {
                    if (call.beat < startBeat) continue;

                    if (!ignoreDisplay)
                    {
                        float step = type == 0 ? 1f : 0.5f;
                        double diff = call.beat - beat;

                        if (diff % step != 0) continue;
                    }

                    sounds.Add(new(call.datamodel, call.beat));
                    actions.Add(new(call.beat, () => {
                        if (call.datamodel == "rhythmFighter/kick")
                        {
                            fighterL.DoScaledAnimationAsync("Kick", 0.5f);
                        }
                        else
                        {
                            fighterL.DoScaledAnimationAsync("Punch", 0.5f);
                        }
                    }));
                }

                actions.Add(new(beat + length, () => { canBop = true; }));

                if (autoPass)
                {
                    float v = 0;
                    for (int i = 1; i < (length + 4f); i = i * 2)
                    {
                        v = i * 2;
                    }
                    v = v - 3;
                    actions.Add(new(beat + v, delegate {
                        PassTurn(beat + v, noteDisplay, type, false, beat+1, length, ignoreDisplay, neededCalls, true);
                    }));
                }
                _ = MultiSound.Play(sounds);
                _ = BeatAction.New(this, actions);
            }
        }

        public void PassTurn(double beat, bool noteDisplay, int type, bool inactive = false, double startIntervalBeat = double.NaN, float startIntervalLength = float.NaN, bool ignoreDisplay = false, List<RiqEntity> neededCalls = null, bool fromAutoPass = false)
        {
            if (double.IsNaN(startIntervalBeat) || double.IsNaN(startIntervalLength))
            {
                RiqEntity startInterval = gameManager.Beatmap.Entities.FindLast(e => e.datamodel == "rhythmFighter/start interval" && e.beat + e.length <= beat);
                if (startInterval == null) return;
                startIntervalBeat = startInterval.beat;
                startIntervalLength = startInterval.length;
                ignoreDisplay = startInterval["ignoreDisplay"];
            }
            neededCalls ??= GetNeededCalls(startIntervalBeat, startIntervalLength);
            if (neededCalls.Count <= 0) return; // do the actual stuff under here

            if (fromAutoPass)
            {
                SoundByte.PlayOneShotGame("rhythmFighter/ready", beat, forcePlay: true);
                SoundByte.PlayOneShotGame("rhythmFighter/fight", beat + 2, forcePlay: true);
            }

            List<MultiSound.Sound> sounds = new();
            List<BeatAction.Action> actions = new();

            if (fighterDistance)
            {
                if (!inactive)
                {
                    actions.Add(new(beat, () =>
                    {


                        holderL.DoScaledAnimationAsync("Advance", 0.5f);
                        holderR.DoScaledAnimationAsync("Advance", 0.5f);
                        fighterDistance = false;

                    }));

                    actions.Add(new(beat, () =>
                    {
                        fighterL.DoScaledAnimationAsync("Walk", 0.5f);
                        fighterR.DoScaledAnimationAsync("Walk", 0.5f);
                    }));

                    actions.Add(new(beat + 1, () =>
                    {
                        fighterL.DoScaledAnimationAsync("Walk", 0.5f);
                        fighterR.DoScaledAnimationAsync("Walk", 0.5f);
                    }));

                    actions.Add(new(beat + 2, () =>
                    {
                        fighterL.DoScaledAnimationAsync("Walk", 0.5f);
                        fighterR.DoScaledAnimationAsync("Walk", 0.5f);
                    }));
                }
                else
                {
                    fighterDistance = false;
                    holderL.DoScaledAnimationAsync("Advanced", 0.5f);
                    holderR.DoScaledAnimationAsync("Advanced", 0.5f);
                }
            }

            actions.Add(new(beat + 2, () => { fightText.DoScaledAnimationAsync("Show", 0.5f); }));
            if (noteDisplay) { 
                actions.Add(new(beat + 3, () => { Display(beat + 3, startIntervalLength, type, 0, false, true); })); 
            }
            actions.Add(new(beat + 4, () => { canDodge = true; } ));

            

            foreach (RiqEntity call in neededCalls)
            {

                double relativeBeat = call.beat - startIntervalBeat;

                if (!ignoreDisplay)
                {
                    float step = type == 0 ? 1f : 0.5f;
                    double diff = call.beat - beat;

                    if (diff % step != 0) continue;
                }
                    
                //sounds.Add(new(call.datamodel, beat+4+relativeBeat));
                actions.Add(new(beat + 4 + relativeBeat, () => {
                    if (call.datamodel == "rhythmFighter/kick")
                    {
                        fighterL.DoScaledAnimationAsync("Kick", 0.5f);
                    }
                    else
                    {
                        fighterL.DoScaledAnimationAsync("Punch", 0.5f);
                    }
                }));
                if (call.datamodel == "rhythmFighter/kick")
                {
                    _ = ScheduleInput(beat, relativeBeat + 4, InputAction_Alt, KickHit, KickMiss, null);
                }
                else
                {
                    _ = ScheduleInput(beat, relativeBeat + 4, InputAction_Nrm, PunchHit, PunchMiss, null);
                }
                
            }

            actions.Add(new(beat + 4 + startIntervalLength, () => { canDodge = false; }));

            //_ = MultiSound.Play(sounds);
            _ = BeatAction.New(this, actions);
        }

        private List<RiqEntity> GetNeededCalls(double beat, float length)
        {
            return callEntities.FindAll(e => e.beat >= beat && e.beat <= beat + length);
        }

        private void PunchHit(PlayerActionEvent caller, float state)
        {
            fighterR.DoScaledAnimationAsync("Duck", 0.5f);
            SoundByte.PlayOneShotGame("rhythmFighter/punch_dodge");
        }

        private void PunchMiss(PlayerActionEvent caller)
        {
            fighterR.DoScaledAnimationAsync("Punched", 0.5f);
            SoundByte.PlayOneShotGame("rhythmFighter/punch_hit");
            SoundByte.PlayOneShotGame("rhythmFighter/punch");
        }

        private void KickHit(PlayerActionEvent caller, float state)
        {
            fighterR.DoScaledAnimationAsync("Jump", 0.5f);
            SoundByte.PlayOneShotGame("rhythmFighter/kick_dodge");
        }

        private void KickMiss(PlayerActionEvent caller)
        {
            fighterR.DoScaledAnimationAsync("Kicked", 0.5f);
            SoundByte.PlayOneShotGame("rhythmFighter/kick_hit");
            SoundByte.PlayOneShotGame("rhythmFighter/kick");
        }

        public void Display(double beat, float length, int type, float delay, bool call = false, bool disappear = false)
        {
            float noteX = (length > 4 ? 2.91f : 1.17f);
            float noteDistance = noteX * 2 / ((MathF.Ceiling(length) * (type == 1 ? 2 : 1)) - 1);

            if (call)
            {
                displayHolder.transform.position = new Vector3(-3.2f, 1.93f, 0f);
            }
            else
            {
                displayHolder.transform.position = new Vector3(0f, 2.781f, 0f);
            }

            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    displayHolder.SetActive(true);
                    displayHolderAnim.DoScaledAnimationAsync(call ? "Call" : "Normal", 0.5f, animLayer: 0);
                    if (call) spotLight.DoScaledAnimationAsync("Show", 0.5f);
                    if (length > 4)
                    {
                        displayHolderAnim.DoScaledAnimationAsync("Eight", 0.5f, animLayer: 1);
                    }
                    else
                    {
                        displayHolderAnim.DoScaledAnimationAsync("Four", 0.5f, animLayer: 1);
                    }

                })
            });

            

            List<MultiSound.Sound> sounds = new();
            //List<BeatAction.Action> actions = new();
            GameObject[] notes = new GameObject[Convert.ToInt32(MathF.Ceiling(length) * (type == 1 ? 2 : 1))];

            for (int i = 0; i < (MathF.Ceiling(length) * (type == 1 ? 2 : 1)); i++) 
            {
                notes[i] = Instantiate(musicNote, transform);
                notes[i].transform.position = new Vector3(0 - noteX + noteDistance * i + displayHolder.transform.position.x, displayHolder.transform.position.y, 0f);
                notes[i].SetActive(disappear);
                if (disappear)
                {
                    if (i % (type == 1 ? 8 : 4) == 0)
                    {
                        notes[i].GetComponent<Animator>().DoScaledAnimationAsync(type == 1 ? "EightO" : "QuarterO");
                    }
                    else
                    {
                        notes[i].GetComponent<Animator>().DoScaledAnimationAsync(type == 1 ? "EightW" : "QuarterW");
                    }
                }

                SpawnNote(beat+1, notes[i], disappear, i, type == 1, call);

            }

            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat+length+delay+(type == 1 ? 0.5 : 0), delegate
                {
                    displayHolder.SetActive(false);
                    for (int i = 0; i < (MathF.Ceiling(length) * (type == 1 ? 2 : 1)); i++)
                    {
                        Destroy(notes[i]);
                    }
                }),
                new BeatAction.Action(beat+length+delay+1, delegate
                {
                    if (call) spotLight.DoScaledAnimationAsync("Hide", 0.5f);
                })
            });

            //_ = BeatAction.New(this, actions);
        } 

        private void SpawnNote(double beat, GameObject note, bool disappear, double index, bool quick, bool call)
        {
            SoundByte.PlayOneShotGame(index % (quick ? 8 : 4) == 0 ? "rhythmFighter/ding_first" : "rhythmFighter/ding", beat + index / (quick ? 2 : 1));
            BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + index / (quick ? 2 : 1), delegate { note.SetActive(!disappear);}),
                    new BeatAction.Action(beat + index / (quick ? 2 : 1), delegate {
                        if (!disappear)
                        {
                            if (index % (quick ? 8 : 4) == 0) {
                                note.GetComponent<Animator>().DoScaledAnimationAsync(quick ? "EightO" : "QuarterO");
                            }
                            else
                            {
                                note.GetComponent<Animator>().DoScaledAnimationAsync(quick ? "EightW" : "QuarterW");
                            }
                        }
                        lightsL.DoScaledAnimationAsync("Flash", 0.5f);
                        lightsR.DoScaledAnimationAsync("Flash", 0.5f);
                    }),
                });
        }
    }
}