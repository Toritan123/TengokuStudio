using NaughtyBezierCurves;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrDogNinjaLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            RiqEntity ObjectUpdater(string datamodel, RiqEntity e)
            {
                if (datamodel == "dogNinja/ThrowObject" && e.version == 0)
                {
                    // e["diffObjs"] = e["direction"] == 2 && e["typeL"] != e["typeR"];
                    e.CreateProperty("diffObjs", e["direction"] == 2 && e["typeL"] != e["typeR"]);
                    e.CreateProperty("type", e["direction"] is 0 or 2 ? (int)e["typeL"] : e["typeR"]);
                    e.version = 1;

                    return e;
                }
                return null;
            }

            RiqBeatmap.OnUpdateEntity += ObjectUpdater;

            return new Minigame("dogNinja", "Dog Ninja", "554899", false, false, new List<GameAction>()
            {
                new GameAction("Bop", "Bop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.TryGetMinigame(out DogNinja instance)) {
                            instance.Bop(e.beat, e.length, e["auto"], e["toggle"]);
                        }
                    },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Bop", "Toggle if Dog Ninja should bop for the duration of this event."),
                        new Param("auto", false, "Bop (Auto)", "Toggle if Dog Ninja should automatically bop until another Bop event is reached."),
                    }
                },
                new GameAction("Prepare", "Prepare")
                {
                    function = delegate {
                        if (eventCaller.gameManager.TryGetMinigame(out DogNinja instance)) {
                            instance.DoPrepare();
                        }
                    },
                    defaultLength = 0.5f,
                },
                new GameAction("ThrowObject", "Throw Object")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        DogNinja.QueueObject(e.beat, e["direction"], e["diffObjs"], e["shouldPrepare"], e["muteThrow"], e);
                    },
                    defaultLength = 2,
                    parameters = new List<Param>()
                    {
                        new Param("direction", DogNinja.ObjectDirection.Alternate, "Which Side", "Choose the side(s) the object(s) should be thrown from."),
                        new Param("diffObjs", false, "Different Objects", "Toggle if the sides should be different.", new() {
                            new((x, _) => (bool)x, "typeL", "typeR"),
                            new((x, _) => !(bool)x, "type"),
                        }),
                        new Param("type", DogNinja.ObjectType.Random, "Object", "Choose the object to be thrown."),
                        new Param("typeL", DogNinja.ObjectType.Random, "Left Object", "Choose the object to be thrown from the left."),
                        new Param("typeR", DogNinja.ObjectType.Random, "Right Object", "Choose the object to be thrown from the right."),
                        new Param("shouldPrepare", true, "Prepare", "Toggle if Dog Ninja should automatically prepare for this cue."),
                        new Param("muteThrow", false, "Mute", "Toggle if the cue should be muted. This only applies when the cue is started from another game.", advanced: true),
                    },
                    defaultVersion = 1,
                },
                new GameAction("CutEverything", "Mister Eagle's Sign")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.TryGetMinigame(out DogNinja instance)) {
                            instance.CutEverything(e.beat, e.length, e["toggle"], e["text"]);
                        }
                    },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Play Sound", "Toggle if the sound effect should play for flying in."),
                        new Param("text", "Cut everything!", "Sign Text", "Set the text to be displayed on the sign.")
                    }
                },
                new GameAction("HereWeGo", "Here We Go!")
                {
                    preFunction = delegate { DogNinja.HereWeGo(eventCaller.currentEntity.beat); },
                    defaultLength = 2,
                    preFunctionLength = 1,
                },
            },
            new List<string>() { "ntr", "normal" },
            "ntrninja", "en",
            new List<string>() { "en", "jp" },
            chronologicalSortKey: 23,
            font : "KanTeiRyu"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using System.Diagnostics;
    using System.Linq;
    using Jukebox;
    using Scripts_DogNinja;
    using XPostProcessing;

    public class DogNinja : Minigame
    {
        [Header("Animators")]
        public Animator DogAnim;    // dog misc animations
        public Animator BirdAnim;   // bird flying in and out

        [Header("References")]
        [SerializeField] ThrowObject ObjectBase;
        [SerializeField] TMP_Text CutEverythingText;

        [SerializeField] Sprite[] ObjectTypes;

        private bool autoBop = true;
        public bool queuePrepare;
        public bool preparing;
        private bool lastSideL = false;

        public enum ObjectDirection
        {
            Left,
            Right,
            Both,
            Alternate,
        }

        public enum ObjectType : int
        {
            Random,     // random fruit
            Apple,      // fruit
            Broccoli,   // fruit
            Carrot,     // fruit
            Cucumber,   // fruit
            Pepper,     // fruit
            Potato,     // fruit
            Bone,       // bone
            Pan,        // pan
            Tire,       // tire
            // custom objects that aren't in the og game
            AirBatter,
            Karateka,
            IaiGiriGaiden,
            ThumpFarm,
            BattingShow,
            MeatGrinder,
            Idol,
            TacoBell,
            //YaseiNoIkiG3M4,
        }

        protected static bool IA_PadAny(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.East, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Up, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Down, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Right, out dt);
        }

        public static PlayerInput.InputAction InputAction_Press =
            new("NtrNinjaPress", new int[] { IAPressCat, IAFlickCat, IAPressCat },
            IA_PadAny, IA_TouchFlick, IA_BatonBasicPress);
        public static PlayerInput.InputAction InputAction_TouchPress =
            new("NtrNinjaTouchRelease", new int[] { IAEmptyCat, IAPressCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicPress, IA_Empty);
        public static PlayerInput.InputAction InputAction_TouchRelease =
            new("NtrNinjaTouchRelease", new int[] { IAEmptyCat, IAReleaseCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicRelease, IA_Empty);

        public override void OnLateBeatPulse(double beat)
        {
            if (autoBop && !preparing && !queuePrepare && (DogAnim.IsAnimationNotPlaying() || DogAnim.IsPlayingAnimationNames("Idle"))) {
                DogAnim.DoScaledAnimationAsync("Bop", 0.5f);
            }
        }

        public override void OnPlay(double beat)
        {
            foreach (var e in gameManager.Beatmap.Entities.FindAll(e => e.datamodel is "dogNinja/ThrowObject" && beat > e.beat && beat < e.beat + 1))
            {
                DogAnim.Play("Prepare", 0, 1);
                preparing = true;
                QueueObject(e.beat, e["direction"], e["diffObjs"], e["shouldPrepare"], true, e);
            }
        }

        public override void OnGameSwitch(double beat)
        {
            foreach (var e in gameManager.Beatmap.Entities.FindAll(e => e.datamodel is "dogNinja/ThrowObject" && beat >= e.beat - 2 && beat < e.beat + 1))
            {
                bool shouldPrepare = e["shouldPrepare"];
                if (beat > e.beat) {
                    shouldPrepare = false;
                    DogAnim.Play("Prepare", 0, 1);
                    preparing = true;
                }
                ThrowObject(e.beat, e["direction"], e["diffObjs"], shouldPrepare, e);
            }
        }

        private void Update()
        {
            // prepare queuing stuff
            if (queuePrepare && !preparing && (DogAnim.IsAnimationNotPlaying() || DogAnim.IsPlayingAnimationNames("Bop")))
            {
                DogAnim.DoScaledAnimationAsync("Prepare", 0.5f);
                preparing = true;
                queuePrepare = false;
            }

            // controls stuff
            if (PlayerInput.GetIsAction(InputAction_TouchPress) && !GameManager.instance.Autoplay)
            {
                // queuePrepare = true;
                DogAnim.DoScaledAnimationAsync("Prepare", 0.5f);
                preparing = true;
            }
            if (PlayerInput.GetIsAction(InputAction_TouchRelease) && (!IsExpectingInputNow(InputAction_Press)) && (!GameManager.instance.Autoplay))
            {
                StopPrepare();
                DogAnim.DoScaledAnimationAsync("Unprepare", 0.5f);
            }

            if (PlayerInput.GetIsAction(InputAction_Press) && !IsExpectingInputNow(InputAction_Press))
            {
                string slice = UnityEngine.Random.Range(0, 1f) < 0.5f ? "WhiffRight" : "WhiffLeft";
                DogAnim.DoScaledAnimationAsync(slice, 0.5f);

                SoundByte.PlayOneShotGame("dogNinja/whiff");
                StopPrepare();
            }
        }

        public void Bop(double beat, float length, bool auto, bool bop)
        {
            autoBop = auto;
            if (!bop) return;

            List<BeatAction.Action> actions = new();
            for (int i = 0; i < length; i++) {
                actions.Add(new(beat + i, delegate { DogAnim.DoScaledAnimationAsync("Bop", 0.5f); }));
            }
            if (actions.Count > 0) BeatAction.New(this, actions);
        }

        private static int[] GetTypesFromEntity(RiqEntity e) {
            return e["diffObjs"] ? new int[] { e["typeL"], e["typeR"] } : new int[] { e["type"], e["type"] };
        }

        private static int GetRandomObjectType(double beat, int entityId, bool isRightAndDiff) {
            var rand = new System.Random(Mathf.RoundToInt((float)beat) + entityId + (isRightAndDiff ? 1 : 0));
            return rand.Next((int)ObjectType.Apple, (int)ObjectType.Potato + 1);
        }

        public static string GetSfxFromType(ObjectType type) {
            return "dogNinja/" + (type <= ObjectType.Potato ? "fruit" : Enum.GetName(typeof(ObjectType), type));
        }

        public static void QueueObject(double beat, int direction, bool diffObjs, bool prepare, bool muteThrow, RiqEntity e)
        {
            var types = GetTypesFromEntity(e);
            if (!muteThrow) {
                for (int i = 0; i < (direction == 2 && diffObjs ? 2 : 1); i++) {
                    SoundByte.PlayOneShotGame(GetSfxFromType((ObjectType)types[i]) + "1", beat, forcePlay: true);
                }
            }

            if (GameManager.instance.minigame is DogNinja instance) {
                instance.ThrowObject(beat, direction, diffObjs, prepare, e);
            }
        }

        public void ThrowObject(double beat, int direction, bool diffObjs, bool prepare, RiqEntity e)
        {
            if (prepare) {
                BeatAction.New(this, new() {
                    new(beat, () => queuePrepare = PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch || GameManager.instance.Autoplay)
                });
            }

            int[] types = GetTypesFromEntity(e);

            for (int i = 0; i < (direction == 2 ? 2 : 1); i++)
            {
                bool altern = direction is 3 ? !lastSideL : direction == 0;
                bool l = direction is 2 ? i == 0 : altern;
                int cutdir = direction is 2 ? 2 : Convert.ToInt32(!altern);
                ThrowObject obj = Instantiate(ObjectBase, transform);
                obj.startBeat = beat;
                obj.direction = cutdir;
                obj.fromLeft = l;
                obj.type = l ? types[0] : types[1];
                if (obj.type == (int)ObjectType.Random) {
                    obj.type = GetRandomObjectType(beat, e.uid, !l && diffObjs);
                }
                obj.GetComponent<SpriteRenderer>().sprite = ObjectTypes[obj.type];
                if (direction == 2) obj.shouldSfx = diffObjs || l == (types[0] == types[1]);
            }

            lastSideL = direction is 3 ? !lastSideL : direction != 1;
        }

        public void CutEverything(double beat, float length, bool sound, string customText)
        {
            if (sound) SoundByte.PlayOneShotGame("dogNinja/bird_flap");
            BirdAnim.DoScaledAnimationAsync("FlyIn", 0.5f);
            CutEverythingText.text = customText;

            BeatAction.New(this, new() {
                new(beat + length, () => BirdAnim.Play("FlyOut", 0, 0))
            });
        }

        public void StopPrepare()
        {
            preparing = false;
            queuePrepare = false;
        }

        public void DoPrepare()
        {
            if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch && PlayerInput.PlayerHasControl()) return;
            DogAnim.DoScaledAnimationAsync("Prepare", 0.5f);
            preparing = true;
        }

        public static void HereWeGo(double beat)
        {
            MultiSound.Play(new List<MultiSound.Sound>() {
                new("dogNinja/here", beat + 0),
                new("dogNinja/we", beat + 0.5),
                new("dogNinja/go",   beat + 1),
            }, forcePlay: true);
        }
    }
}
