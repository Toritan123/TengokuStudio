using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using NaughtyBezierCurves;

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
    public static class PcoMagicGirlLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("magicGirl", "Magic Girl", "e39ec2", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out MagicGirl instance)) {
                            instance.BopToggle(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity["shouldBop"], eventCaller.currentEntity["auto"]);
                        }
                    },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("shouldBop", true, "Bop", "Toggle if Mako should bop for the duration of this event."),
                        new Param("auto", false, "Autobop", "Toggle if Mako should bop automatically until another Bop event is reached."),
                    }
                },
                new GameAction("start interval", "Start Interval")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out MagicGirl instance)) {
                            var e = eventCaller.currentEntity;
                            instance.StartInterval(e.beat, e.length, e["autoPass"]);
                        }
                    },
                    defaultLength = 3f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("autoPass", true, "Auto Pass Turn", "Toggle if the turn should be passed automatically at the end of the start interval.")
                    }
                },
                new GameAction("spawn", "Spawn Enemy")
                {
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("spawnLocation", MagicGirl.SpawnLocation.TopLeft, "Spawn Location", "Choose the location where the enemy will spawn.")
                    }
                },
                new GameAction("pass turn", "Pass Turn")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out MagicGirl instance)) {
                            var e = eventCaller.currentEntity;
                            instance.PassTurn(e.beat, e.length);
                            SoundByte.PlayOneShotGame("magicGirl/pass_turn", e.beat);
                        }
                    },
                    defaultLength = 1f,
                    resizable = true,
                },
                new GameAction("monster hand", "Monster Hand")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out MagicGirl instance)) {
                            var e = eventCaller.currentEntity;
                            instance.MonsterHand(e.beat);
                            SoundByte.PlayOneShotGame("magicGirl/hand1", e.beat);
                            SoundByte.PlayOneShotGame("magicGirl/hand2", e.beat+1);
                        }
                    },
                    defaultLength = 4f
                },
                new GameAction("transfcomp", "Show Transformation Component")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out MagicGirl instance)) {
                            var e = eventCaller.currentEntity;
                            instance.TransfComp(e.beat, e.length, e["type"], e["location"]);
                        }
                    },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("type", MagicGirl.TComponents.DressA, "Component", "Choose which component will be shown."),
                        new Param("location", MagicGirl.SpawnLocation.TopLeft, "Location", "Choose the location where the component will be shown.")
                    }
                },
                new GameAction("progress", "Progress Transformation")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out MagicGirl instance)) {
                            var e = eventCaller.currentEntity;
                            instance.ProgressTransform(e.beat);
                        }
                    },
                    defaultLength = 1f
                },
                new GameAction("set outfit", "Set Transformation")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out MagicGirl instance)) {
                            var e = eventCaller.currentEntity;
                            instance.SetOutfit(e.beat, e["outfit"]);
                        }
                    },
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("outfit", MagicGirl.Outfits.PhaseB, "Form", "Choose which form Mako will be set into.")
                    }
                },
                new GameAction("changeBG", "Background Appearance")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out MagicGirl instance)) {
                            var e = eventCaller.currentEntity;
                            instance.BackgroundColor(e.beat, e.length, e["startTop"], e["endTop"], e["startBottom"], e["endBottom"], e["startDots"], e["endDots"], e["ease"]);
                        }
                    },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("startTop", MagicGirl.defaultBGTop, "Gradient Top Start", "Set the color at the start of the event."),
                        new Param("endTop", MagicGirl.defaultBGTop, "Gradient Top End", "Set the color at the end of the event."),
                        new Param("startBottom", MagicGirl.defaultBGBottom, "Gradient Bottom Start", "Set the color at the start of the event."),
                        new Param("endBottom", MagicGirl.defaultBGBottom, "Gradient Bottom End", "Set the color at the end of the event."),
                        new Param("startDots", MagicGirl.defaultStarColor, "Star Color Start", "Set the color at the start of the event."),
                        new Param("endDots", MagicGirl.defaultStarColor, "Star Color End", "Set the color at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    }
                },
                new GameAction("scroll", "Scrolling Background")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out MagicGirl instance)) {
                            var e = eventCaller.currentEntity;
                            instance.UpdateScrollSpeed(e["x"], e["y"]);
                        }
                    },
                    defaultLength = 1f,
                    parameters = new List<Param>() {
                        new Param("x", new EntityTypes.Float(-10f, 10, 0.5f), "Horizontal Speed", "Set how fast the background will scroll horizontally."),
                        new Param("y", new EntityTypes.Float(-10, 10f, 0.2f), "Vertical Speed", "Set how fast the background will scroll vertically."),
                    }
                }
            },
            new List<string>() { "pco", "repeat" },
            "pcomagic", "en",
            new List<string>() { },
            chronologicalSortKey: 20250301,
            plusGame: true
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Jukebox;
    using Scripts_MagicGirl;

    /// stuff for background scrolling
    using HeavenStudio.Common;
    using UnityEngine.Rendering;
    using UnityEngine.UI;
    using System;

    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
    public class MagicGirl : Minigame
    {
        public enum SpawnLocation
        {
            TopLeft = 0,
            TopRight = 1,
            BottomLeft = 2,
            BottomRight = 3,
        }
        public enum Outfits
        {
            Uniform,
            PhaseA,
            PhaseB,
            PhaseC,
            PhaseD,
            Final
        }
        public enum TComponents
        {
            DressA,
            DressB,
            Hand,
            Legs
        }
        public static Color defaultBGTop = new(1f, 122/255f, 228/255f);
        public static Color defaultBGBottom = new(1f, 163/255f, 237/255f);
        public static Color defaultStarColor = new(1f, 215 / 255f, 1f);

        private bool goBop = true;
        private bool canBop = true;
        public bool preparing = false;
        private bool holding = false;

        private int transformPhase = 2;

        [SerializeField] public GameObject MakoObject;
        [SerializeField] public GameObject TransfComponent;
        [SerializeField] public Animator Mako;
        [SerializeField] public Animator MakoFace;
        [SerializeField] public Animator MonsterHands;
        [SerializeField] List<Monster> Monsters;
        [SerializeField] private CanvasScroll backgroundSprite;
        [SerializeField] private RawImage bgImage;
        [SerializeField] private Material gradientMat;
        [SerializeField] ParticleSystem jumpEffect;

        float xScrollMultiplier = 0.5f;
        float yScrollMultiplier = 0.2f;
        [SerializeField] private float xBaseSpeed = 1;
        [SerializeField] private float yBaseSpeed = 1;

        private double jumpStartTime = double.MinValue;
        private bool isJumping = false;

        private ColorEase gradTopEase = new(defaultBGTop);
        private ColorEase gradBottomEase = new(defaultBGBottom);
        private ColorEase starEase = new(defaultStarColor);

        private List<RiqEntity> callEntities;

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

        protected static bool IA_PadAltRelease(out double dt)
        {
            return PlayerInput.GetPadUp(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltRelease(out double dt)
        {
            return PlayerInput.GetSqueezeUp(out dt);
        }

        public static PlayerInput.InputAction InputAction_Press =
            new("PcoMagicGirlPress", new int[] { IAPressCat, IAFlickCat, IAPressCat },
            IA_PadAny, IA_TouchBasicPress, IA_BatonBasicPress);
        public static PlayerInput.InputAction InputAction_AltPress =
            new("PcoMagicGirlAltPress", new int[] { IAAltDownCat, IAAltDownCat, IAAltDownCat },
            IA_PadAltPress, IA_TouchBasicPress, IA_BatonAltPress);
        public static PlayerInput.InputAction InputAction_AltRelease =
            new("PcoMagicGirlAltRelease", new int[] { IAAltUpCat, IAFlickCat, IAAltUpCat },
            IA_PadAltRelease, IA_TouchFlick, IA_BatonAltRelease);
        public static PlayerInput.InputAction InputAction_TouchRelease =
            new("PcoMagicGirlTouchRelease", new int[] { IAEmptyCat, IAReleaseCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicRelease, IA_Empty);

        protected static bool IA_PadAny(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.East, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Up, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Down, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Right, out dt);
        }

        private void Awake()
        {
            //instance = this;
            //SetupBopRegion("magicGirl", "bop", "auto");

        }

        public override void OnLateBeatPulse(double beat)
        {
            if (goBop)
            {
                Bop();
            }

        }

        public void Update()
        {
            /*
            if (queuePrepare && !preparing && (Mako.IsAnimationNotPlaying() || Mako.IsPlayingAnimationNames("Bop")))
            {
                Mako.DoScaledAnimationAsync("Prepare", 0.5f);
                preparing = true;
                queuePrepare = false;
            }
            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !GameManager.instance.autoplay && PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch)
            {
                // queuePrepare = true;
                Mako.DoScaledAnimationAsync("Prepare", 0.5f);
                preparing = true;
            }
            if (PlayerInput.GetIsAction(InputAction_BasicRelease) && (!IsExpectingInputNow(InputAction_BasicRelease)) && (!GameManager.instance.autoplay) && PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch)
            {
                StopPrepare();
            }
            */

            //whiffs
            if (PlayerInput.GetIsAction(InputAction_Press) && !IsExpectingInputNow(InputAction_Press) && !isJumping && !holding && transformPhase != 0 && !Mako.IsPlayingAnimationNames("Hurt_L", "Hurt_R"))
            {
                if (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch || !IsExpectingInputNow(InputAction_AltPress))
                {
                    string whiffAnim = UnityEngine.Random.Range(0, 1f) < 0.5f ? "U_Left_Move" : "U_Right_Move";
                    Mako.DoScaledAnimationAsync(whiffAnim, 0.5f, animLayer: 0);
                    MakoFace.DoScaledAnimationAsync("FaceBarely", 0.5f);
                    SoundByte.PlayOneShot("miss");
                }
            }

            if (PlayerInput.GetIsAction(InputAction_AltPress) && !IsExpectingInputNow(InputAction_AltPress) && PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch && !isJumping && transformPhase != 0 && !Mako.IsPlayingAnimationNames("Hurt_L", "Hurt_R"))
            {
                Mako.DoScaledAnimationAsync("Hold", 0.5f, animLayer: 0);
                MakoFace.DoScaledAnimationAsync("FaceBarely", 0.5f);
                SoundByte.PlayOneShot("miss");
                holding = true;
            }

            if (PlayerInput.GetIsAction(InputAction_AltRelease) && !IsExpectingInputNow(InputAction_AltRelease) && !isJumping && transformPhase != 0 && !Mako.IsPlayingAnimationNames("Hurt_L", "Hurt_R"))
            {
                if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch || holding)
                {
                    Jump(conductor.unswungSongPositionInBeatsAsDouble);
                    MakoFace.DoScaledAnimationAsync("FaceBarely", 0.5f);
                    SoundByte.PlayOneShot("miss");
                }
            }

            if (PlayerInput.GetIsAction(InputAction_TouchRelease) && !IsExpectingInputNow(InputAction_TouchRelease) && !isJumping && transformPhase != 0 && !Mako.IsPlayingAnimationNames("Hurt_L", "Hurt_R"))
            {
                Mako.DoScaledAnimationAsync("Bop", 0.5f, animLayer: 0);
                holding = false;
            }

            //bg scroll
            backgroundSprite.NormalizedX -= xBaseSpeed * xScrollMultiplier * Time.deltaTime;
            backgroundSprite.NormalizedY += yBaseSpeed * yScrollMultiplier * Time.deltaTime;

            //mako jump
            float jumpPos = conductor.GetPositionFromBeat(jumpStartTime, 0.75f);
            if (conductor.unswungSongPositionInBeatsAsDouble >= jumpStartTime && conductor.unswungSongPositionInBeatsAsDouble < jumpStartTime + 0.75f)
            {
                float yMul = jumpPos * 2f - 1f;
                float yWeight = -(yMul * yMul) + 1f;
                MakoObject.transform.localPosition = new Vector3(0, 2f * yWeight + 0.25f + 0.7f);
                isJumping = true;
            }
            else
            {
                jumpStartTime = double.MinValue;
                MakoObject.transform.localPosition = new Vector3(0, 0.7f);
                isJumping = false;
            }

            //bg update
            BackgroundColorUpdate();
        }

        public override void OnGameSwitch(double beat)
        {
            StoreAllCallEntities();
            CheckMonsters(beat);
        }

        public override void OnPlay(double beat)
        {
            StoreAllCallEntities();
            CheckMonsters(beat);
        }

        private void StoreAllCallEntities()
        {
            callEntities = gameManager.Beatmap.Entities.FindAll(e => e.datamodel == "magicGirl/spawn");
        }

        public void DoPrepare()
        {
            //if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch && PlayerInput.PlayerHasControl()) return;
            Mako.DoScaledAnimationAsync("Prepare", 0.5f, animLayer: 0);
            preparing = true;
        }

        public void StopPrepare()
        {
            preparing = false;
            //queuePrepare = false;
        }

        public void BopToggle(double beat, float length, bool shouldBop, bool autoBop)
        {

            if (shouldBop)
            {
                List<BeatAction.Action> bops = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                {
                    bops.Add(new BeatAction.Action(beat + i, delegate { Bop(); }));
                }
                BeatAction.New(this, bops);
            }
            goBop = autoBop;
        }

        public void Bop()
        {
            if (canBop && !preparing && (Mako.IsAnimationNotPlaying() || Mako.IsPlayingAnimationNames("Idle")) && !holding) { 
                Mako.DoScaledAnimationAsync("Bop", 0.5f, animLayer: 0); 
                MakoFace.DoScaledAnimationAsync("FaceIdle", 0.5f);
            }
        }

        public void StartInterval(double beat, float length, bool autoPass, double startBeat = double.MinValue)
        {
            List<RiqEntity> neededCalls = GetNeededCalls(beat, length);
            if (neededCalls.Count <= 0) return;
            if (transformPhase == 0) return;

            if (startBeat < beat + length)
            {
                List<BeatAction.Action> actions = new();

                foreach (RiqEntity call in neededCalls)
                {
                    if (call.beat < startBeat) continue;
                    actions.Add(new(call.beat, () => {
                        SpawnMonster(call.beat, call.beat - beat, call["spawnLocation"]);
                    }));
                }

                if (autoPass)
                {
                    actions.Add(new(beat + length, delegate
                    {
                        PassTurn(beat + length, 1f);
                    }));
                    SoundByte.PlayOneShotGame("magicGirl/pass_turn", beat+length);
                }

                _ = BeatAction.New(this, actions);
            }
        }

        private void SpawnMonster(double beat, double relativeBeat, int location)
        {
            for (int i = 0; i < 4; i++)
            {
                int v = (location + i) % 4;

                if (Monsters[v].hasSpawned == false)
                {
                    SoundByte.PlayOneShotGame("magicGirl/spawn_monster", beat);
                    Monsters[v].Spawn(relativeBeat);
                    break;
                }
            }
        }

        public void PassTurn(double beat, float length)
        {
            if (transformPhase == 0) return;

            Mako.DoScaledAnimationAsync("Prepare", 0.5f, animLayer: 0);
            
            foreach (var v in Monsters) v.PassTurn(beat, length); 
        }

        private List<RiqEntity> GetNeededCalls(double beat, float length)
        {
            return callEntities.FindAll(e => e.beat >= beat && e.beat <= beat + length);
        }

        private void CheckMonsters(double beat)
        {
            
            RiqEntity startIntervalEntity = gameManager.Beatmap.Entities.FindLast(e => e.beat + e.length <= beat && e.datamodel == "magicGirl/start interval" && e["autoPass"] == false);
            if (startIntervalEntity != null)
            {
                if (gameManager.Beatmap.Entities.FindLast(e => e.beat < beat && e.beat >= startIntervalEntity.beat + startIntervalEntity.length && e.datamodel == "magicGirl/pass turn") != null) return;
                SpawnAllMonsters(beat, startIntervalEntity);
            }
        }

        private void SpawnAllMonsters(double beat, RiqEntity startIntervalEntity)
        {
            List<RiqEntity> neededCalls = GetNeededCalls(startIntervalEntity.beat, startIntervalEntity.length);
            if (neededCalls.Count <= 0) return;

            foreach (var call in neededCalls) 
            {
                int spawnLoc = call["spawnLocation"];

                for (int i = 0; i < 4; i++)
                {
                    int v = (spawnLoc + i) % 4;

                    if (Monsters[v].hasSpawned == false)
                    {
                        Monsters[v].Spawn(call.beat - startIntervalEntity.beat);
                        break;
                    }
                }
            }
        }

        public void Jump(double beat)
        {
            holding = false;
            jumpStartTime = conductor.GetUnSwungBeat(beat);
            Mako.DoScaledAnimationAsync("Jump", 0.5f, animLayer: 0);
        }

        public void MonsterHand(double beat)
        {
            if (transformPhase == 0) return;
            MonsterHands.DoScaledAnimationAsync("Appear", 0.5f);

            ScheduleInput(beat, 2, InputAction_AltPress, HoldHit, HoldMiss, null);
            ScheduleInput(beat, 3, InputAction_AltRelease, JumpHit, JumpMiss, null);
        }

        private void HoldHit(PlayerActionEvent caller, float state)
        {
            Mako.DoScaledAnimationAsync("Hold", 0.5f, animLayer: 0);
            MakoFace.DoScaledAnimationAsync("FaceIdle", 0.5f);
            SoundByte.PlayOneShotGame("magicGirl/hold");
            holding = true;
        }

        private void HoldMiss(PlayerActionEvent caller)
        {
            Mako.DoScaledAnimationAsync("Hurt_L", 0.5f, animLayer: 0);
            MakoFace.DoScaledAnimationAsync("FaceBarely", 0.5f);
            SoundByte.PlayOneShotGame("magicGirl/doingoing");
        }

        private void JumpHit(PlayerActionEvent caller, float state)
        {
            if (state is >= 1f or <= (-1f)) // barely
            {

            }
            else
            {
                jumpEffect.Play();
            }
            
            Jump(conductor.unswungSongPositionInBeatsAsDouble);
            MakoFace.DoScaledAnimationAsync("FaceSmile", 0.5f, animLayer: 0);
            SoundByte.PlayOneShotGame("magicGirl/hit");

            MonsterHands.DoScaledAnimationAsync("Hide", 0.5f);
        }

        private void JumpMiss(PlayerActionEvent caller)
        {
            Mako.DoScaledAnimationAsync("Hurt_R", 0.5f, animLayer: 0);
            MakoFace.DoScaledAnimationAsync("FaceMiss", 0.5f);
            SoundByte.PlayOneShotGame("magicGirl/doingoing");

            MonsterHands.DoScaledAnimationAsync("Attack", 0.5f);
        }

        public void ProgressTransform(double beat)
        {
            if (transformPhase < 5) 
            {
                switch (transformPhase)
                {
                    case 0:
                        Mako.DoScaledAnimationAsync("FullFlash", 0.5f, animLayer: 2);
                        break;
                    default:
                        Mako.DoScaledAnimationAsync("FullFlash", 0.5f, animLayer: 2);
                        break;
                }
                BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat+1, delegate { SetOutfit(beat+1, transformPhase+1); }),
                    new BeatAction.Action(beat+1, delegate { Mako.DoScaledAnimationAsync("IdleFlash", 0.5f, animLayer: 2); }),
                    new BeatAction.Action(beat+1, delegate { SoundByte.PlayOneShotGame("magicGirl/sparkle"); }),
                });
            }
        }

        public void SetOutfit(double beat, int outfit)
        {
            transformPhase = outfit;
            switch (outfit) 
            { 
                case 0:
                    Mako.DoScaledAnimationAsync("Uniform", 0.5f, animLayer: 1);
                    break;
                case 1:
                    Mako.DoScaledAnimationAsync("PhaseA", 0.5f, animLayer: 1);
                    break;
                case 2:
                    Mako.DoScaledAnimationAsync("PhaseB", 0.5f, animLayer: 1);
                    break;
                case 3:
                    Mako.DoScaledAnimationAsync("PhaseC", 0.5f, animLayer: 1);
                    break;
                case 4:
                    Mako.DoScaledAnimationAsync("PhaseD", 0.5f, animLayer: 1);
                    break;
                case 5:
                    Mako.DoScaledAnimationAsync("Final", 0.5f, animLayer: 1);
                    break;
            }
        }

        public void TransfComp(double beat, float length, int type, int location)
        {
            switch (location)
            {
                case 0:
                    TransfComponent.transform.position = new Vector3(-5.5f, 2.5f, 0);
                    break;
                case 1:
                    TransfComponent.transform.position = new Vector3(5.5f, 2.5f, 0);
                    break;
                case 2:
                    TransfComponent.transform.position = new Vector3(-5.5f, -2.5f, 0);
                    break;
                case 3:
                    TransfComponent.transform.position = new Vector3(5.5f, -2.5f, 0);
                    break;
            }
            TransfComponent.GetComponent<Animator>().DoScaledAnimationAsync("Appear", 0.5f, animLayer: 0);
            switch (type)
            {
                case 0:
                    TransfComponent.GetComponent<Animator>().DoScaledAnimationAsync("DressA", 0.5f, animLayer: 1);
                    break;
                case 1:
                    TransfComponent.GetComponent<Animator>().DoScaledAnimationAsync("DressB", 0.5f, animLayer: 1);
                    break;
                case 2:
                    TransfComponent.GetComponent<Animator>().DoScaledAnimationAsync("Hand", 0.5f, animLayer: 1);
                    break;
                case 3:
                    TransfComponent.GetComponent<Animator>().DoScaledAnimationAsync("Legs", 0.5f, animLayer: 1);
                    break;
            }


            BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat+length, delegate { TransfComponent.GetComponent<Animator>().DoScaledAnimationAsync("Hide", 0.5f, animLayer: 0); }),
                });
        }

        public void BackgroundColor(double beat, float length, Color gradTopStart, Color gradTopEnd, Color gradBottomStart, Color gradBottomEnd, Color startColorStars, Color endColorStars, int ease)
        {
            gradTopEase = new(beat, length, gradTopStart, gradTopEnd, ease);
            gradBottomEase = new(beat, length, gradBottomStart, gradBottomEnd, ease);
            starEase = new(beat, length, startColorStars, endColorStars, ease);
        }

        public void BackgroundColorUpdate()
        {
            gradientMat.SetColor("_ColorAlpha", gradTopEase.GetColor());
            gradientMat.SetColor("_ColorDelta", gradBottomEase.GetColor());

            bgImage.color = starEase.GetColor();
        }

        public void UpdateScrollSpeed(float scrollSpeedX, float scrollSpeedY)
        {
            xScrollMultiplier = scrollSpeedX;
            yScrollMultiplier = scrollSpeedY;
        }
    }
}
  