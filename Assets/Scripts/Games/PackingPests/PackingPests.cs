using System;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlSortLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            RiqEntity MoveWorkersUpdater(string datamodel, RiqEntity e)
            {
                if (datamodel == "packingPests/worker")
                {
                    if (e.version > 0) return null;

                    if (e["all"] == true || (e["player"] == true && e["allAlt"] == true))
                    {
                        e.CreateProperty("movedWorkers", (int)PackingPests.Workers.AllWorkers);
                    }
                    else if (e["player"] == true)
                    {
                        e.CreateProperty("movedWorkers", (int)PackingPests.Workers.PlayerWorker);
                    }
                    else
                    {
                        e.CreateProperty("movedWorkers", (int)PackingPests.Workers.BackgroundWorkers);
                    }

                    e.dynamicData.Remove("all");
                    e.dynamicData.Remove("allAlt");
                    e.dynamicData.Remove("player");

                    e.version = 1;

                    return e;
                }
                return null;
            }
            RiqBeatmap.OnUpdateEntity += MoveWorkersUpdater;

            return new Minigame("packingPests", "Packing Pests", "BAB89D", false, false, new List<GameAction>()
            {
                new GameAction("pattern", "Pattern")
                {
                    function = delegate { var e = eventCaller.currentEntity; PackingPests.instance.Pattern(e.beat, e["throwTo"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("throwTo", PackingPests.Workers.PlayerWorker, "Throw To", "Change who this cue is thrown to."),
                    }
                },
                new GameAction("pattern2", "Double Up!")
                {

                    function = delegate { var e = eventCaller.currentEntity; PackingPests.instance.Pattern2(e.beat, e["pitch"], e["throwTo"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("pitch", false, "Pitch Voiceline", "Changes the pitch of the \"double up!\" voice to sound better at lower tempos."),
                        new Param("throwTo", PackingPests.Workers.PlayerWorker, "Throw To", "Change who this cue is thrown to."),
                    }
                },
                new GameAction("patternArrange1", "Offbeat Pattern")
                {
                    function = delegate { var e = eventCaller.currentEntity; PackingPests.instance.PatternArrange1(e.beat, e["throwTo"]);},
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("throwTo", PackingPests.Workers.PlayerWorker, "Throw To", "Change who this cue is thrown to."),
                    }
                },
                new GameAction("patternArrange2", "Offbeat Candy Pattern")
                {
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity;
                        if (!e["warning"]) return;
                        PackingPests.instance.PrePatternDing(e.beat, true);
                    },
                    function = delegate 
                    { 
                        var e = eventCaller.currentEntity; PackingPests.instance.PatternArrange2(e.beat, e["throwTo"]);
                    },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("warning", true, "Play Warning", "Toggle if the 'ding' and 'hmm' sound effects should play beforehand."),
                        new Param("throwTo", PackingPests.Workers.PlayerWorker, "Throw To", "Change who this cue is thrown to."),
                    },
                    preFunctionLength = 0.5f
                },
                new GameAction("candy", "Throw Candy (Standalone)")
                {
                    function = delegate { var e = eventCaller.currentEntity; PackingPests.instance.ThrowCandy(e.beat, e["throwTo"], e["object"]); },
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("object", PackingPests.CandySprite.Candy, "Thrown Object", "Change the object that is thrown."),
                        new Param("throwTo", PackingPests.Workers.PlayerWorker, "Throw To", "Change who this cue is thrown to."),
                    }
                },
                new GameAction("spider", "Throw Spider (Standalone)")
                {
                    function = delegate { var e = eventCaller.currentEntity; PackingPests.instance.ThrowSpider(e.beat, e["throwTo"]); },
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("throwTo", PackingPests.Workers.PlayerWorker, "Throw To", "Change who this cue is thrown to."),
                    },
                },
                new GameAction("ding", "Offbeat Warning (Standalone)")
                {
                    function = delegate { var e = eventCaller.currentEntity; PackingPests.instance.Ding(e.beat, e["hmm"]); },
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("hmm", true, "Play Hum", "Toggle if the 'hmm' sound effect should play beforehand."),
                    },
                    advanced = true,
                },
                new GameAction("doubleUpWarning", "Double Up Warning (Standalone)")
                {
                    function = delegate { var e = eventCaller.currentEntity; PackingPests.instance.DoubleUpWarning(e.beat, e["pitch"]); },
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("pitch", false, "Pitch Voiceline", "Changes the pitch of the \"double up!\" voice to sound better at lower tempos."),
                    },
                    advanced = true,
                },
                new GameAction("cameraShift", "Shift Camera")
                {
                    function = delegate { var e = eventCaller.currentEntity; PackingPests.instance.ShiftCamera(e.beat, e.length, e["toggle"]); },
                    defaultLength = 8f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Return Camera", "Toggle if the camera should shift back at the end."),
                    },
                },
                new GameAction("worker", "Move Workers")
                {
                    function = delegate { var e = eventCaller.currentEntity; PackingPests.instance.MoveWorker(e.beat, e.length, e["movedWorkers"], e["inout"], e["ease"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("movedWorkers", PackingPests.Workers.BackgroundWorkers, "Move Workers", "Select which workers get moved by this event."),
                        new Param("inout", PackingPests.WorkerType.In, "Side", "Set The side the worker moves to."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    },
                    defaultVersion = 1
                },
                new GameAction("curtainNew", "Move Curtains")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        PackingPests.instance.MoveCurtain(e.beat, e.length, e["inout"], e["ease"]);
                    },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("inout", PackingPests.CurtainType.Enter, "Side", "Set the side the Curtain Slides to."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action")
                    }
                },
                new GameAction("randomThrows", "Toggle Background Cue Throws")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        PackingPests.instance.ToggleRandomThrows(e["toggle"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Enable Background Throws", "Toggles if the employees in the background will get cues thrown to them randomly."),
                    }
                },
            },
            new List<string>() { "rvl", "keep" },
            "rvlsorting", "en",
            new List<string>() { "en", "jp" },
            chronologicalSortKey: 17
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Cysharp.Threading.Tasks;
    using NUnit.Framework;
    using Scripts_PackingPests;
    public class PackingPests : Minigame
    {
        public static PackingPests instance;
        const int IA_AltPress = IAMAXCAT;

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
                && instance.IsExpectingInputNow(InputAction_Alt);
        }
        public static PlayerInput.InputAction InputAction_Alt =
            new("RvlSortAlt", new int[] { IA_AltPress, IA_AltPress, IA_AltPress },
            IA_PadAltPress, IA_TouchAltPress, IA_BatonAltPress);


        public enum WorkerType
        {
            In,
            Out,
        }

        public enum Workers
        {
            PlayerWorker,
            BackgroundWorkers,
            AllWorkers
        }

        public enum CandySprite
        {
            Candy,
            Paycheck,
            BonusPaycheck,
            Flowers
        }

        bool isMoving;
        bool isMovingBG;
        bool isSliding;
        double movingStartBeat;
        double movingLength;
        string moveAnim;
        double curtainStartBeat;
        double curtainLength;
        string CurtainAnim;
        Util.EasingFunction.Ease lastEase;

        public EmployeeLayer[] employees;

        public Animator curtainAnim;
        public Animator signAnim;

        [NonSerialized] public double nextOpenCueBeat = double.MinValue;

        public enum CurtainType
        {
            Enter,
            Exit
        }

        private bool randomThrows = true;

        public void Awake()
        {
            instance = this;
        }

        new void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            foreach (EmployeeLayer layer in employees)
            {
                layer.OnDrawGizmos();
            }
        }

        public override void OnGameSwitch(double beat)
        {
            PersistEvents(beat);
        }

        public override void OnPlay(double beat)
        {
            PersistEvents(beat);
        }

        private void PersistEvents(double beat)
        {
            RiqEntity e = gameManager.Beatmap.Entities.FindLast(e => e.beat < beat && e.datamodel == "packingPests/randomThrows");
            if (e != null)
            {
                ToggleRandomThrows(e["toggle"]);
            }
        }

        public override void OnLateBeatPulse(double beat)
        {
            if (!randomThrows || isMovingBG || moveAnim == "Exit") return;
            foreach (EmployeeLayer layer in employees)
            {
                layer.TryRandomThrow(beat);
            }
        }

        private void Update()
        {
            double currentBeat = conductor.unswungSongPositionInBeatsAsDouble;
            if(PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_Alt))
            {
                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_SWING");
                employees[0].upperHandAnim.DoScaledAnimationAsync("HitOut", 0.5f);
            }
            else if (PlayerInput.GetIsAction(InputAction_Alt) && !IsExpectingInputNow(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_Alt))
            {
                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_SWING_CATCH_A");
                SoundByte.PlayOneShotGame("packingPests/SE_SHIWAKE_EN_SWING_CATCH_B");
                employees[0].upperHandAnim.DoScaledAnimationAsync("CatchOut", 0.5f);
                employees[0].lowerHandAnim.DoScaledAnimationAsync("CatchOut", 0.5f);
            }

            if (!Conductor.instance.NotStopped())
            {
                curtainAnim.DoScaledAnimationAsync("idle", 0.5f);
            }
            

            if (isMoving) {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(movingStartBeat, movingLength);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(lastEase);
                float newPos = func(0f, 1f, normalizedBeat);
                employees[0].moveHandAnim.DoNormalizedAnimation(moveAnim, newPos);
                
                if (normalizedBeat >= 1f) isMoving = false;
            }
            if (isMovingBG) {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(movingStartBeat, movingLength);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(lastEase);
                float newPos = func(0f, 1f, normalizedBeat);
                foreach (EmployeeLayer layer in employees)
                {
                    if (!layer.player)
                    {
                        layer.moveHandAnim.DoNormalizedAnimation(moveAnim, newPos);
                    }
                }
                
                if (normalizedBeat >= 1f) isMovingBG = false;
            }
            if (isSliding) {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(curtainStartBeat, curtainLength);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(lastEase);
                float newPos = func(0f, 1f, normalizedBeat);
                curtainAnim.DoNormalizedAnimation(CurtainAnim, newPos);
                if (normalizedBeat >= 1f) isSliding = false;
            }
        }
        public void Pattern(double beat, int throwTo)
        {
            ThrowCandy(beat + 0, throwTo);
            ThrowSpider(beat + 2, throwTo);
        }

        public void PatternArrange1(double beat, int throwTo)
        {
            ThrowCandy(beat + 0, throwTo);
            ThrowSpider(beat + 1.5, throwTo);
        }
        public void PatternArrange2(double beat, int throwTo)
        {
            ThrowCandy(beat + 0.5, throwTo);
            ThrowSpider(beat + 2, throwTo);
        }

        public void Pattern2(double beat, bool isPitched, int throwTo)
        {
            if ((Workers)throwTo != Workers.BackgroundWorkers) DoubleUpWarning(beat, isPitched);

            ThrowCandy(beat + 0, throwTo);
            ThrowSpider(beat + 1.5, throwTo);
            ThrowSpider(beat + 2.5, throwTo);
        }

        public void DoubleUpWarning(double beat, bool isPitched)
        {
            signAnim.DoScaledAnimationAsync("Message00", 0.5f);
            foreach (EmployeeLayer layer in employees)
            {
                layer.headAnim.DoScaledAnimationAsync("Double", 0.5f);
            }

            float pitch = isPitched ? (Conductor.instance.GetBpmAtBeat(beat) / 136) * Conductor.instance.TimelinePitch : 1;
            double offset = isPitched ? (0.013 / (Conductor.instance.GetBpmAtBeat(beat) / 136)) : 0.013;
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("packingPests/SE_SHIWAKE_EN_VOICE_READY1", beat, pitch),
                new MultiSound.Sound("packingPests/SE_SHIWAKE_EN_VOICE_READY2", beat + 0.25f, pitch, offset: offset),
                new MultiSound.Sound("packingPests/SE_SHIWAKE_EN_VOICE_READY3", beat + 0.5f, pitch),
            }, forcePlay: true, ignoreSwing: isPitched);
        }

        public void PrePatternDing(double beat, bool hmm)
        {
            Ding(beat - 0.5, true);
        }


        public void ThrowCandy(double beat, int throwTo, int thrownObject = 0)
        {
            switch ((Workers)throwTo)
            {
                case Workers.AllWorkers:
                    foreach (EmployeeLayer layer in employees)
                    {
                        layer.ThrowCandy(beat, thrownObject);
                    }
                    nextOpenCueBeat = beat + 1.5;
                    break;

                case Workers.BackgroundWorkers:
                    foreach (EmployeeLayer layer in employees)
                    {
                        if (!layer.player) layer.ThrowCandy(beat, thrownObject);
                    }
                    nextOpenCueBeat = beat + 1.5;
                    break;

                default:
                    employees[0].ThrowCandy(beat, thrownObject);
                    break;
            }
        }

        public void ThrowSpider(double beat, int throwTo)
        {
            switch ((Workers)throwTo)
            {
                case Workers.AllWorkers:
                    foreach (EmployeeLayer layer in employees)
                    {
                        layer.ThrowSpider(beat);
                    }
                    nextOpenCueBeat = beat + 1.5;
                    break;

                case Workers.BackgroundWorkers:
                    foreach (EmployeeLayer layer in employees)
                    {
                        if (!layer.player) layer.ThrowSpider(beat);
                    }
                    nextOpenCueBeat = beat + 1.5;
                    break;

                default:
                    employees[0].ThrowSpider(beat);
                    break;
            }
        }

        public void ShiftCamera(double beat, float length, bool shiftBack)
        {
            foreach (EmployeeLayer layer in employees)
            {
                layer.ShiftCamera(beat, length, shiftBack);
            }
        }

        public void MoveWorker(double beat, double length, int moveWorkers, int goToSide, int ease)
        {
            switch ((Workers)moveWorkers)
            {
                case Workers.PlayerWorker:
                    isMoving = true;
                    isMovingBG = false;
                    break;
                case Workers.BackgroundWorkers:
                    isMoving = false;
                    isMovingBG = true;
                    break;
                case Workers.AllWorkers:
                    isMoving = true;
                    isMovingBG = true;
                    break;
            }

            movingStartBeat = beat;
            movingLength = length;
            moveAnim = (goToSide == 0 ? "Enter" : "Exit");
            lastEase = (Util.EasingFunction.Ease)ease;
        }
        public void Ding(double beat, bool hmm)
        {   
            List<MultiSound.Sound> sfx = new();
            sfx.Add(new MultiSound.Sound("packingPests/SE_SHIWAKE_EN_VOICE_REST_A", beat));
            if (hmm) {sfx.Add(new MultiSound.Sound("packingPests/SE_SHIWAKE_EN_VOICE_REST_B", beat+0.5));}
            MultiSound.Play(sfx, forcePlay : true);
            if (signAnim != null) {signAnim.DoScaledAnimationAsync("Message01", 0.5f);}
        }
        public void MoveCurtain(double beat, double length, int inout, int ease)
        {
            isSliding = true;
            curtainStartBeat = beat;
            curtainLength = length;
            CurtainAnim = (inout == 0 ? "Enter" : "Exit");
            lastEase = (Util.EasingFunction.Ease)ease;

            float normalizedBeat = Conductor.instance.GetPositionFromBeat(curtainStartBeat, curtainLength);
            Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(lastEase);
            float newPos = func(0f, 1f, normalizedBeat);
            curtainAnim.DoNormalizedAnimation(CurtainAnim, newPos);
        }

        public void ToggleRandomThrows(bool toggle)
        {
            randomThrows = toggle;
        }
    }
}
