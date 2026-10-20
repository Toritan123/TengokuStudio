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
    public static class NtrMoaiDooWopLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("moaiDooWop", "Moai Doo-Wop", "019893", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        MoaiDooWop.instance.Bop(e.beat, e.length, e["auto"], e["bop"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop", "Toggle if the birds should bop for the duration of this event."),
                        new Param("auto", false, "Bop (Auto)", "Toggle if the birds should automatically bop until another Bop event is reached."),
                    },
                    resizable = true,
                    defaultLength = 1f
                },
                new GameAction("start interval", "Start Interval")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        MoaiDooWop.PreInterval(e.beat, e.length, e["auto"], e["autoMoai"], e["autoBeat"]);
                    },
                    preFunctionLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("auto", true, "Auto Pass Turn", "Toggle if the turn should be passed automatically at the end of the start interval."),
                        new Param("autoMoai", true, "Auto Move Moai", "Toggle if the moai should move automatically.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "autoBeat" })
                        }),
                        new Param("autoBeat", new EntityTypes.Float(-4, 0, -1.5f), "Move Moai Beat", "Set the number of beats before the interval to move the moai.")
                    },
                    defaultLength = 8f,
                    resizable = true,
                },
                new GameAction("pass turn", "Pass Turn")
                {
                    preFunction = delegate
                    {
                        MoaiDooWop.PrePassTurn(eventCaller.currentEntity.beat);
                    },
                    preFunctionLength = 1f
                },
                new GameAction("dooWop", "Doo-Wop")
                {
                    defaultLength = 1f,
                    resizable = true,
                },
                new GameAction("shout", "Shout")
                {
                    defaultLength = 0.5f,
                },
                new GameAction("setTime", "Set Time")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.TryGetMinigame(out MoaiDooWop instance)) {
                            instance.SetTime(e.beat, e.length, e["time"], e["ease"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("time", MoaiDooWop.TimeOfDay.Night, "Time", "Set which time it should be."),
                        new Param("ease", Util.EasingFunction.Ease.EaseInOutQuad, "Ease", "Set the easing of the action."),
                    },
                    defaultLength = 4f,
                    resizable = true,
                },
                new GameAction("moveMoai", "Move Moai")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.TryGetMinigame(out MoaiDooWop instance)) {
                            instance.MoaiMove(e["cpuPos"], e["playerPos"], e["instant"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("cpuPos", MoaiDooWop.MoaiPos.Up, "CPU Position", "Set which position the moai should be."),
                        new Param("playerPos", MoaiDooWop.MoaiPos.Up, "Player Position", "Set which position the moai should be."),
                        new Param("instant", false, "Instant", "Toggle if the action should be instant."),
                    },
                    defaultLength = 1f,
                },
                new GameAction("moaiColor", "Change Moai Appearance")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.TryGetMinigame(out MoaiDooWop instance)) {
                            instance.MoaiVisual(e["Mcolor"], e["MglassesColor"], e["Mlenscolor"], e["MhasGlasses"], e["MoaiMHead"], e["Fcolor"], e["FglassesColor"], e["Flenscolor"], e["FhasGlasses"], e["MoaiFHead"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("Mcolor", new Color(0.345098f, 0.3137255f, 0.1254902f), "Moaio Color", "Set what color Moaio should be."),
                        new Param("MglassesColor", new Color(0.9725f, 0f, 0f), "Moaio Glasses Color", "Set what color Moaio's glasses should be."),
                        new Param("Mlenscolor", new Color(0.5f, 0.313725f, 0f), "Moaio Lens Color", "Set what color Moaio's glasses lens should be."),
                        new Param("MhasGlasses", false, "Moaio Glasses", "Toggle if the moais have glasses."),
                        new Param("MoaiMHead", MoaiDooWop.MoaiMAccess.None, "Moaio Accessory", "Set accessory of Moaio."),
                        new Param("Fcolor", new Color(0.345098f, 0.3137255f, 0.1254902f), "Moaiko Color", "Set what color Moaiko should be."),
                        new Param("FglassesColor", new Color(0.9725f, 0f, 0f), "Moaiko Glasses Color", "Set what color Moaiko's glasses should be."),
                        new Param("Flenscolor", new Color(0.5f, 0.313725f, 0f), "Moaiko Lens Color", "Set what color Moaiko's glasses lens should be."),
                        new Param("FhasGlasses", false, "Moaiko Glasses", "Toggle if the moais have glasses."),
                        new Param("MoaiFHead", MoaiDooWop.MoaiAccess.Ribbon, "Moaiko Accessory", "Set accessory of Moaiko."),
                    },
                    defaultLength = 1f,
                },
            },
            new List<string>() {"ntr", "repeat"},
            "ntrmoai", "en",
            new List<string>() {},
            chronologicalSortKey: 8,
            font : "FOT-SlumpStd-DB SDF"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
    public class MoaiDooWop : Minigame
    {
        public enum MoaiAccess
        {
            None,
            Ribbon,
	    Flower,
        }

        public enum MoaiMAccess
        {
            None,
            Ribbon,
	    Flower,
        }

        [SerializeField] Animator cpuMoaiAnim;
        [SerializeField] Animator playerMoaiAnim;
        [SerializeField] Animator cpuMoaiMoveAnim;
        [SerializeField] Animator playerMoaiMoveAnim;
        [SerializeField] Animator bgAnim;

        [SerializeField] Material MoaiColorM, MoaiColorF;
	[SerializeField] GameObject GlassesM, GlassesF, MRibbon, FRibbon, MFlower, FFlower;

        [SerializeField] List<Animator> birdAnims;
        [SerializeField] List<Animator> bgBirdAnims;
        [SerializeField] List<Animator> poopAnims;

        Sound cpuSound;
        Sound playerSound;
        bool cpuUp = true;
        bool playerUp = true;
        double lastHitState;
        int missCount = 0;

        bool birdBop = true;

        List<PlayerActionEvent> scheduledShouts;
        int shoutsDone = -1;

        private UpdateEasing bgEase;

        public struct UpdateEasing
        {
            public double beat;
            public float length;
            public Util.EasingFunction.Ease ease;
        }

        string setTime = "BGIdle";

        public enum TimeOfDay
        {
            Day,
            Night,
        }

        public enum MoaiPos
        {
            Up,
            Down,
        }

        public static MoaiDooWop instance;

        protected static bool IA_None(out double dt)
        {
            dt = 0;
            return false;
        }

        public static PlayerInput.InputAction InputAction_None =
            new("NtrMoaiNone", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_None, IA_None, IA_None);

        void Awake()
        {
            instance = this;
            foreach (Animator b in bgBirdAnims)
            {
                b.DoScaledAnimationAsync("BirdBG", UnityEngine.Random.Range(0.35f, 0.65f), UnityEngine.Random.Range(0f, 1f));
            }
        }

        public override void OnStop(double beat) => EntityPreCheck(beat, false);
        public override void OnPlay(double beat) => EntityPreCheck(beat, true);

        private void EntityPreCheck(double beat, bool checkCues)
        {
            if (gameManager == null) return;
            RiqEntity obj = null;
            foreach (var e in gameManager.Beatmap.Entities)
            {
		    switch (e.datamodel)
                    {
                        case "moaiDooWop/moaiColor":
                            obj = e;
                            break;
                        default: break;
                    }
	    }
            if (obj != null) {
                MoaiVisual(obj["Mcolor"], obj["MglassesColor"], obj["Mlenscolor"], obj["MhasGlasses"], obj["MoaiMHead"], obj["Fcolor"], obj["FglassesColor"], obj["Flenscolor"], obj["FhasGlasses"], obj["MoaiFHead"]);
            } else {
                MoaiVisual(new Color(0.345098f, 0.3137255f, 0.1254902f),  new Color(0.9725f, 0f, 0f),  new Color(0.5f, 0.313725f, 0f), false, 0,
                                new Color(0.345098f, 0.3137255f, 0.1254902f),  new Color(0.9725f, 0f, 0f),  new Color(0.5f, 0.313725f, 0f), false, 1);
            }
	}

        void Update()
        {
            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            {
                playerMoaiAnim.DoScaledAnimationAsync("Moai_Shout_Open", 0.5f);
                if (playerSound == null) playerSound = SoundByte.PlayOneShotGame("moaiDooWop/rightDoo", -1, 1f, 1f, true);
                ScheduleUserInput(Conductor.instance.songPositionInBeatsAsDouble, 0, InputAction_BasicRelease, WhiffUserShoutRelease, null, null);
                ScoreMiss(0.5f);
                CallMiss();
            }
            if (PlayerInput.GetIsAction(InputAction_BasicRelease) && !IsExpectingInputNow(InputAction_BasicRelease))
            {
                playerMoaiAnim.DoScaledAnimationAsync("moai_Shout_Close", 0.5f);
                SoundByte.PlayOneShotGame("moaiDooWop/rightWop");
                if (playerSound != null) playerSound.KillLoop();
                playerSound = null;
            }

            if (passedTurns.Count > 0)
            {
                foreach (var pass in passedTurns)
                {
                    PassTurnStandalone(pass);
                }
                passedTurns.Clear();
            }

            if (bgEase.length != 0)
            {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(bgEase.beat, bgEase.length);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(bgEase.ease);
                float newPos = func(0f, 1f, normalizedBeat);
                bgAnim.DoNormalizedAnimation(setTime, newPos);
                if (normalizedBeat >= 1) bgEase.length = 0;
            }
        }

        public override void OnBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat))
            {
                if (birdBop)
                {
                    foreach (Animator b in birdAnims)
                    {
                        b.DoScaledAnimationAsync("Bird_Bop", 0.5f);
                    }
                }
            }
        }

        void OnDestroy()
        {
            if (cpuSound != null) cpuSound.KillLoop();
            cpuSound = null;
            if (playerSound != null) playerSound.KillLoop();
            playerSound = null;
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        private struct QueuedInterval
        {
            public double beat;
            public float interval;
            public bool autoPassTurn;
            public bool autoMoai;
            public float autoBeat;
        }

        private static List<QueuedInterval> queuedIntervals = new();

        public void Bop(double beat, float length, bool auto, bool bop)
        {
            birdBop = auto;

            if (bop)
            {
                List<BeatAction.Action> actions = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                {
                    actions.Add(new BeatAction.Action(beat + i, delegate
                    {
                        foreach (Animator b in birdAnims)
                        {
                            b.DoScaledAnimationAsync("Bird_Bop", 0.5f);
                        }
                    }));
                }
                BeatAction.New(instance, actions);
            }
        }

        private List<RiqEntity> GetAllInputsBetweenBeat(double beat, double endBeat)
        {
            return EventCaller.GetAllInGameManagerList("moaiDooWop", new string[] { "dooWop", "shout" }).FindAll(x => x.beat >= beat && x.beat < endBeat);
        }

        public static void PreInterval(double beat, float interval, bool autoPassTurn, bool autoMoai, float autoBeat)
        {
            if (GameManager.instance.currentGame == "moaiDooWop")
            {
                instance.StartInterval(beat, interval, beat, autoPassTurn, autoMoai, autoBeat);
            }
            else
            {
                queuedIntervals.Add(new QueuedInterval()
                {
                    beat = beat,
                    interval = interval,
                    autoPassTurn = autoPassTurn,
                    autoMoai = autoMoai,
                    autoBeat = autoBeat
                });
            }
        }

        public void StartInterval(double beat, float interval, double gameSwitchBeat, bool autoPassTurn, bool autoMoai, double autoBeat)
        {
            List<BeatAction.Action> actions = new();

            actions.Add(new BeatAction.Action(beat + autoBeat, delegate
            {
                bool force = false;
                if (beat - gameSwitchBeat < 2 && beat != gameSwitchBeat) force = true;
                if (autoMoai) MoveMoai(true, force);
            }));

            var relevantInputs = GetAllInputsBetweenBeat(beat, beat + interval);
            relevantInputs.Sort((x, y) => x.beat.CompareTo(y.beat));

            for (int i = 0; i < relevantInputs.Count; i++)
            {
                bool isShout = relevantInputs[i].datamodel == "moaiDooWop/shout";
                if (isShout)
                {
                    double inputBeat = relevantInputs[i].beat;
                    if (inputBeat >= gameSwitchBeat)
                    {
                        actions.Add(new BeatAction.Action(inputBeat, delegate
                        {
                            cpuMoaiAnim.DoScaledAnimationAsync("Moai_Shout", 0.5f);
                        }));
                        SoundByte.PlayOneShotGame("moaiDooWop/leftPah", inputBeat);
                    }
                }
                else
                {
                    double inputBeat = relevantInputs[i].beat;
                    double inputLength = relevantInputs[i].length;
                    if (inputBeat >= gameSwitchBeat)
                    {
                        var sound = SoundByte.PlayOneShotGame("moaiDooWop/leftDoo", inputBeat, 1f, 1f, true);
                        sound.SetLoopParams(inputBeat + inputLength, 0.05f);
                        SoundByte.PlayOneShotGame("moaiDooWop/leftWop", inputBeat + inputLength);

                        actions.Add(new BeatAction.Action(inputBeat, delegate
                        {
                            cpuMoaiAnim.DoScaledAnimationAsync("Moai_Shout_Open", 0.5f);
                        }));
                        actions.Add(new BeatAction.Action(inputBeat + inputLength, delegate
                        {
                            cpuMoaiAnim.DoScaledAnimationAsync("moai_Shout_Close", 0.5f);
                        }));
                    }
                }
            }
            BeatAction.New(this, actions);
            if (autoPassTurn)
            {
                PassTurn(beat + interval, beat, interval, autoMoai, autoBeat);
            }
        }

        private RiqEntity GetLastIntervalBeforeBeat(double beat)
        {
            return EventCaller.GetAllInGameManagerList("moaiDooWop", new string[] { "start interval" }).FindLast(x => x.beat <= beat);
        }

        public static void PrePassTurn(double beat)
        {
            if (GameManager.instance.currentGame == "moaiDooWop")
            {
                instance.PassTurnStandalone(beat);
            }
            else
            {
                passedTurns.Add(beat);
            }
        }

        private static List<double> passedTurns = new();

        private void PassTurnStandalone(double beat)
        {
            var lastInterval = GetLastIntervalBeforeBeat(beat);
            float length = EventCaller.GetAllInGameManagerList("moaiDooWop", new string[] { "pass turn" }).Find(x => x.beat == beat).length;
            if (lastInterval != null) PassTurn(beat, lastInterval.beat, lastInterval.length, lastInterval["autoMoai"], lastInterval["autoBeat"]);
        }

        public void PassTurn(double beat, double intervalBeat, float intervalLength, bool autoMoai, double autoBeat)
        {
            List<BeatAction.Action> actions = new();

            actions.Add(new BeatAction.Action(intervalBeat, delegate
            {
                scheduledShouts = new();
                shoutsDone = -1;
            }));

            actions.Add(new BeatAction.Action(beat + autoBeat, delegate
            {
                if (autoMoai) MoveMoai(false);
            }));

            var relevantInputs = GetAllInputsBetweenBeat(intervalBeat, intervalBeat + intervalLength);
            relevantInputs.Sort((x, y) => x.beat.CompareTo(y.beat));

            for (int i = 0; i < relevantInputs.Count; i++)
            {
                bool isShout = relevantInputs[i].datamodel == "moaiDooWop/shout";
                if (isShout)
                {
                    double inputBeat = relevantInputs[i].beat - intervalBeat;
                    PlayerActionEvent shout = ScheduleInput(beat, inputBeat, InputAction_None, JustShout, null, null);
                    shout.missable = false;  
                    actions.Add(new BeatAction.Action(intervalBeat, delegate
                    {
                        scheduledShouts.Add(shout);
                    }));

                    if (!GameManager.instance.Autoplay) ScheduleUserInput(beat, inputBeat, InputAction_BasicPress, JustUserShout, MissUserShout, null);
                }
                else
                {
                    double inputBeat = relevantInputs[i].beat - intervalBeat;
                    double inputLength = relevantInputs[i].length;
                    ScheduleInput(beat, inputBeat, InputAction_BasicPress, JustPress, Miss, null);
                    ScheduleInput(beat, inputBeat + inputLength, InputAction_BasicRelease, JustRelease, Miss, null);
                }
            }

            BeatAction.New(this, actions);
        }

        public override void OnGameSwitch(double beat)
        {
            if (queuedIntervals.Count > 0)
            {
                foreach (var interval in queuedIntervals)
                {
                    StartInterval(interval.beat, interval.interval, beat, interval.autoPassTurn, interval.autoMoai, interval.autoBeat);
                }
                queuedIntervals.Clear();
            }
            PersistTime(beat);
	    EntityPreCheck(beat, true);
        }

        private void MoveMoai(bool cpu, bool instant = false)
        {
            float speed = 0.5f;
            if (!instant) SoundByte.PlayOneShotGame("moaiDooWop/switch");
            else speed = 500f;
            if (cpu)
            {
                if (!cpuUp)
                {
                    cpuUp = true;
                    cpuMoaiMoveAnim.DoScaledAnimationAsync("MoaioUp", speed);
                }
                playerMoaiMoveAnim.DoScaledAnimationAsync("MoaikoDown", speed);
                playerUp = false;
            }
            else
            {
                cpuUp = false;
                playerUp = true;
                cpuMoaiMoveAnim.DoScaledAnimationAsync("MoaioDown", speed);
                playerMoaiMoveAnim.DoScaledAnimationAsync("MoaikoUp", speed);
            }
        }

        public void MoaiMove(int cpuPos, int playerPos, bool instant)
        {
            float speed = 0.5f;
            bool cpuMoved = false;
            bool playerMoved = false;

            if (instant) speed = 500f;

            if (cpuPos == 0)
            {
                if (!cpuUp)
                {
                    cpuUp = true;
                    cpuMoaiMoveAnim.DoScaledAnimationAsync("MoaioUp", speed);
                    cpuMoved = true;
                }
            }
            else
            {
                if (cpuUp)
                {
                    cpuUp = false;
                    cpuMoaiMoveAnim.DoScaledAnimationAsync("MoaioDown", speed);
                    cpuMoved = true;
                }
            }

            if (playerPos == 0)
            {
                if (!playerUp)
                {
                    playerUp = true;
                    playerMoaiMoveAnim.DoScaledAnimationAsync("MoaikoUp", speed);
                    playerMoved = true;
                }
            }
            else
            {
                if (playerUp)
                {
                    playerUp = false;
                    playerMoaiMoveAnim.DoScaledAnimationAsync("MoaikoDown", speed);
                    playerMoved = true;
                }
            }


            if (!instant && (cpuMoved || playerMoved)) SoundByte.PlayOneShotGame("moaiDooWop/switch");
        }

        public void MoaiVisual(Color MHead, Color MGlassesColor, Color MLens, bool Mglasses, int Mhat, Color FHead, Color FGlassesColor, Color FLens, bool Fglasses, int Fhat)
        {
		MoaiColorM.SetColor("_ColorAlpha", MHead);
		MoaiColorM.SetColor("_ColorDelta", MGlassesColor);
		MoaiColorM.SetColor("_ColorBravo", MLens);
		MoaiColorF.SetColor("_ColorAlpha", FHead);
		MoaiColorF.SetColor("_ColorDelta", FGlassesColor);
		MoaiColorF.SetColor("_ColorBravo", FLens);

		if (Mglasses)
		{
		GlassesM.SetActive(true);
		}
		else
		{
		GlassesM.SetActive(false);
		}

		if (Fglasses)
		{
		GlassesF.SetActive(true);
		}
		else
		{
		GlassesF.SetActive(false);
		}

		switch (Mhat)
		{
		case 0:
		MRibbon.SetActive(false);
		MFlower.SetActive(false);
		break;
		case 1:
		MRibbon.SetActive(true);
		MFlower.SetActive(false);
		break;
		case 2:
		MRibbon.SetActive(false);
		MFlower.SetActive(true);
		break;
		}

		switch (Fhat)
		{
		case 0:
		FRibbon.SetActive(false);
		FFlower.SetActive(false);
		break;
		case 1:
		FRibbon.SetActive(true);
		FFlower.SetActive(false);
		break;
		case 2:
		FRibbon.SetActive(false);
		FFlower.SetActive(true);
		break;
		}
	}

        public void SetTime(double beat, float length, int time, int ease)
        {
            if (time == 1)
            {
                setTime = "BGDayToNight";
            }
            else
            {
                setTime = "BGNightToDay";
            }
            bgEase = new()
            {
                beat = beat,
                length = length,
                ease = (Util.EasingFunction.Ease)ease,
            };
        }

        private void PersistTime(double beat)
        {
            var allEvents = GameManager.instance.Beatmap.Entities.FindAll(e => e.datamodel.Split('/')[0] is "moaiDooWop");
            var eventsBefore = allEvents.FindAll(e => e.beat < beat);

            var lastPersistEvent = eventsBefore.FindLast(e => e.datamodel == "moaiDooWop/setTime");
            if (lastPersistEvent != null)
            {
                var e = lastPersistEvent;
                instance.SetTime(beat, e.length, e["time"], (int)Util.EasingFunction.Ease.Instant);
            }
        }

        public void JustPress(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                CallMiss();
            }
            playerMoaiAnim.DoScaledAnimationAsync("Moai_Shout_Open", 0.5f);
            if (playerSound == null) playerSound = SoundByte.PlayOneShotGame("moaiDooWop/rightDoo", -1, 1f, 1f, true);
        }

        public void JustRelease(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                CallMiss();
            }
            playerMoaiAnim.DoScaledAnimationAsync("moai_Shout_Close", 0.5f);
            SoundByte.PlayOneShotGame("moaiDooWop/rightWop");
            if (playerSound != null) playerSound.KillLoop(0.05f);
            playerSound = null;
        }

        public void JustShout(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                CallMiss();
            }
            playerMoaiAnim.DoScaledAnimationAsync("Moai_Shout", 0.5f);
            SoundByte.PlayOneShotGame("moaiDooWop/rightPah");
        }


        public void JustUserShout(PlayerActionEvent caller, float state)
        {
            shoutsDone++;
            lastHitState = state;
            ScheduleUserInput(Conductor.instance.songPositionInBeatsAsDouble, 0, InputAction_BasicRelease, JustUserShoutRelease, MissUserShoutRelease, null);
            playerMoaiAnim.DoScaledAnimationAsync("Moai_Shout_Open", 0.5f);
            if (playerSound == null) playerSound = SoundByte.PlayOneShotGame("moaiDooWop/rightDoo", -1, 1f, 0.25f, true);
        }

        public void MissUserShout(PlayerActionEvent caller)
        {
            shoutsDone++;
            lastHitState = System.Double.MinValue;
            CallMiss();
        }

        public void JustUserShoutRelease(PlayerActionEvent caller, float state)
        {
            if (playerSound != null) playerSound.KillLoop();
            playerSound = null;
            scheduledShouts[shoutsDone].Hit(0, 1);
        }

        public void MissUserShoutRelease(PlayerActionEvent caller)
        {
            if (playerSound != null)
            {
                playerSound.FadeTo(1f, 0.2f);
            }

            CallMiss();
        }

        public void WhiffUserShoutRelease(PlayerActionEvent caller, float state)
        {
            if (playerSound != null) playerSound.KillLoop();
            playerSound = null;
            playerMoaiAnim.DoScaledAnimationAsync("Moai_Shout", 0.5f);
            SoundByte.PlayOneShotGame("moaiDooWop/rightPah");
        }

        public void Miss(PlayerActionEvent caller)
        {
            CallMiss();
        }

        private void CallMiss()
        {
            SoundByte.PlayOneShot("miss");
            cpuMoaiAnim.DoScaledAnimationAsync("Moai_Miss", 0.5f);
            poopAnims[missCount].DoScaledAnimationAsync($"PoopFall{missCount + 1}", 0.5f);
            missCount++;
            if (missCount == 5) missCount = 0;
        }
    }    
}