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
    public static class agbSamuraiSlice
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("agbSamuraiSlice", "Samurai Slice \n<color=#adadad>(Iai Giri)</color>", "b6b5b6", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate
                    {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out SamuraiSliceAgb instance))
                        {
                            instance.Bop(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity["auto"], eventCaller.currentEntity["toggle"]);
                        }
                    },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Bop", "Toggle if the samurai should bop for the duration of this event."),
                        new Param("auto", true, "Bop (Auto)", "Toggle if the samurai should automatically bop until another Bop event is reached."),
                    }
                },
                new GameAction("demon", "Demon")
                {
                    function = delegate
                    {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out SamuraiSliceAgb instance)) {
                            instance.Demon(eventCaller.currentEntity.beat, eventCaller.currentEntity["type"], eventCaller.currentEntity["slowDown"]);
                        }
                    },
                    defaultLength = 7,
                    parameters = new List<Param>()
                    {
                        new Param("type", SamuraiSliceAgb.YokaiType.Small, "Demon Type", "Choose the type of the demon."),
                        new Param("slowDown", false, "Slowdown", "Toggle if there should be a slowdown effect when the demon gets sliced."),
                    },
                },
                new GameAction("fog", "Set Fog Opacity")
                {
                    function = delegate
                    {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out SamuraiSliceAgb instance))
                        {
                            instance.Fog(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity["opacity"], eventCaller.currentEntity["long"], eventCaller.currentEntity["mute"]);
                        }
                    },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("opacity", new EntityTypes.Float(0, 100, 100), "Opacity", "Set the fog's opacity."),
                        new Param("long", false, "Long Sound", "Toggle if the fog should use the long version of the sound."),
                        new Param("mute", true, "Mute Sound", "Toggle if the sound of the fog should be muted."),
                    },
                },
                new GameAction("rest", "Rest")
                {
                    function = delegate
                    {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out SamuraiSliceAgb instance))
                        {
                            instance.Rest(eventCaller.currentEntity.beat);
                        }
                    },
                    defaultLength = 4f,
                },
                new GameAction("enter", "Enter")
                {
                    function = delegate
                    {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out SamuraiSliceAgb instance))
                        {
                            instance.Enter(eventCaller.currentEntity.beat, eventCaller.currentEntity.length);
                        }
                    },
                    defaultLength = 16f,
                    resizable = true,
                },
                new GameAction("set state", "Set Samurai State")
                {
                    function = delegate
                    {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out SamuraiSliceAgb instance))
                        {
                            instance.SetState(eventCaller.currentEntity.beat, eventCaller.currentEntity["state"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("state", new EntityTypes.Integer(0, 8, 0), "State"),
                    },
                    defaultLength = 0.5f,
                },
            },
            new List<string>() { "agb", "aim" },
            "agbsamurai", "en",
            chronologicalSortKey: 8,
            font : "KanTeiRyu");
        }
    }
}

namespace HeavenStudio.Games
{
    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
    using Scripts_SamuraiSliceAgb;
    public class SamuraiSliceAgb : Minigame
    {
        bool samuraiBop = true; // bopping automatically because thats how the game works in remixes, even though theres no bops in the minigame itself
        bool fireDisappearing;
        public bool fireColorNumberSix = true;
        [SerializeField] Animator samuraiAnim;
        [SerializeField] Yokai yokaiEntity;
        [SerializeField] Yokai mayaFeyFromAceAttorney;
        [SerializeField] float stepDistance;
        [SerializeField] float startPosition;
        [SerializeField] GameObject samuraiObject;
        [SerializeField] Animator fireAnim;
        [SerializeField] float fireNormalY;
        [SerializeField] GameObject fireParent;
        [SerializeField] SpriteRenderer fireSprite;
        [SerializeField] Animator fogAnim;
        [SerializeField] List<SpriteRenderer> fogSprite = new List<SpriteRenderer>();
        public Color[] fireColors;
        public Color fireColorStart;
        public Color fireColorEnd;
        public int samuraiState;
        public int fireState;
        public int sliceState;
        double startStepBeat = double.MaxValue;
        double fireLeaveBeat;
        double fogBeat = double.MaxValue;
        int currentAnim = 0;
        float stepLength = 16f;
        float fogLength;
        float fogOpacity;
        float fogFadeStart = 0f;
        bool fogActive;
        public enum YokaiType
        {
            Small,
            Ribbon,
            Medium,
            Flying,
            Big,
            Maya,
        }
        public enum FireRecoloring
        {
            Two,
            Six,
        }

        public override void OnGameSwitch(double beat)
        {
            RiqEntity cueEntity = EventCaller.GetAllInGameManagerList("agbSamuraiSlice", new string[] 
            { "demon" }).FindLast(e => e.beat < beat && e.beat + 5 >= beat);

            if (cueEntity != null)
            {
                RiqEntity e = cueEntity;
                Demon(e.beat, e["type"], e["slowDown"]);
            }

            double nextGameSwitchBeat = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame", "end" }).Find(x => x.beat > beat)?.beat ?? double.MaxValue;

            cueEntity = EventCaller.GetAllInGameManagerList("agbSamuraiSlice", new string[] 
            { "enter" }).Find(e => e.beat > beat && e.beat < nextGameSwitchBeat);

            if (cueEntity != null)
            {
                samuraiObject.transform.localPosition = new Vector3(startPosition, samuraiObject.transform.localPosition.y);
            }
        }

        public override void OnPlay(double beat)
        {
            double nextGameSwitchBeat = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame", "end" }).Find(x => x.beat > beat)?.beat ?? double.MaxValue;

            RiqEntity cueEntity = EventCaller.GetAllInGameManagerList("agbSamuraiSlice", new string[] 
            { "enter" }).Find(e => e.beat > beat && e.beat < nextGameSwitchBeat);

            if (cueEntity != null)
            {
                samuraiObject.transform.localPosition = new Vector3(startPosition, samuraiObject.transform.localPosition.y);
            }
        }

        public void Bop(double beat, float length, bool auto, bool toggle)
        {
            samuraiBop = auto;
            if (toggle)
            {
                List<BeatAction.Action> bops = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                {
                    bops.Add(new BeatAction.Action(beat + i, delegate { samuraiAnim.DoScaledAnimationAsync("Bop", 0.5f); }));
                }
                BeatAction.New(this, bops);
            }
        }
        public void Demon(double beat, int type, bool slowDown)
        {
            SoundByte.PlayOneShotGame("agbSamuraiSlice/yokaijump", beat + 5, forcePlay: true);

            Yokai newYokai = Instantiate(type == 5 ? mayaFeyFromAceAttorney : yokaiEntity, transform);
            newYokai.startBeat = beat;
            newYokai.gameObject.SetActive(true);
            newYokai.type = type % 5;
            newYokai.slowDown = slowDown;
        }
        public void Slice()
        {
            switch (samuraiState)
            {   
                case 0:
                samuraiAnim.DoScaledAnimationAsync("Slice", 0.5f);
                break;
                case 1:
                samuraiAnim.DoScaledAnimationAsync("Slice1", 0.5f);
                break;
                case 2:
                samuraiAnim.DoScaledAnimationAsync("Slice2", 0.5f);
                break;
            }
        }
        public override void OnLateBeatPulse(double beat)
        {   
            if (samuraiBop && (samuraiAnim.IsPlayingAnimationNames("Idle") || samuraiAnim.IsAnimationNotPlaying()))
            {   
                switch (samuraiState)
                {   
                    case 0:
                    samuraiAnim.DoScaledAnimationAsync("Bop", 0.5f);
                    break;
                    case 1:
                    samuraiAnim.DoScaledAnimationAsync("Bop1", 0.5f);
                    break;
                    case 2:
                    samuraiAnim.DoScaledAnimationAsync("Bop2", 0.5f);
                    break;
                }
            }
        }
        public void Update()
        {
            if (PlayerInput.GetIsAction(InputAction_FlickPress) && !IsExpectingInputNow(InputAction_FlickPress) && !samuraiAnim.IsPlayingAnimationNames("Miss"))
            {
                Slice();
                SoundByte.PlayOneShotGame("agbSamuraiSlice/swing");
            }
            if (conductor.songPositionInBeatsAsDouble >= startStepBeat + Mathf.Floor(stepLength))
            {
                
                startStepBeat = double.MaxValue;
                currentAnim = 0;
            }
            else if (conductor.songPositionInBeatsAsDouble >= startStepBeat)
            {
                currentAnim = (int)((conductor.songPositionInBeatsAsDouble - startStepBeat) / 1);
                double startAnimBeat = startStepBeat + 1 * currentAnim;
                double endAnimBeat = startAnimBeat + 1;
                float prog = (float)((conductor.songPositionInBeatsAsDouble - startAnimBeat) / (endAnimBeat - startAnimBeat + 0.25f));
                prog = Mathf.Clamp01(prog * 4);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(Util.EasingFunction.Ease.EaseOutQuint);
                float newPos = func(0f, 1f, prog);
                samuraiObject.transform.localPosition = new Vector3(startPosition + (stepDistance / Mathf.Floor(stepLength)) * currentAnim + (stepDistance / Mathf.Floor(stepLength)) * newPos, samuraiObject.transform.localPosition.y);
            }
            if (fireDisappearing)
            {
                float fireMovement = conductor.GetPositionFromBeat(fireLeaveBeat, 1f);
                fireParent.transform.position = new Vector3(fireParent.transform.position.x, fireNormalY + (-1.6f * fireMovement));
                if (fireMovement > 1f)
                {
                    fireAnim.gameObject.SetActive(false);
                    fireDisappearing = false;
                }
            }
            if (conductor.songPositionInBeatsAsDouble >= fogBeat && conductor.songPositionInBeats <= fogBeat + fogLength)
            {
                float fade = conductor.GetPositionFromBeat(fogBeat, fogLength);
                foreach (var fogPart in fogSprite)
                {
                    fogPart.color = new Color(1f, 1f, 1f, fade * (fogOpacity / 100) * (1f - fogFadeStart) + fogFadeStart);
                }
            }
        }
        public void Enter(double beat, float length)
        {   
            startStepBeat = beat;
            stepLength = length;
            string bopAnim = "Bop";
            switch (samuraiState)
            {   
                case 1:
                    bopAnim = "Bop1";
                    break;
                case 2:
                    bopAnim = "Bop2";
                    break;
                default:
                    break;
            }
            List<BeatAction.Action> bops = new List<BeatAction.Action>();
            for (int i = 0; i < length; i++)
            {
                bops.Add(new BeatAction.Action(beat + i, delegate { samuraiAnim.DoScaledAnimationAsync(bopAnim, 0.5f); }));
            }
            BeatAction.New(this, bops);
            samuraiObject.transform.localPosition = new Vector3(startPosition, samuraiObject.transform.localPosition.y);
        }
        public void SetState(double beat, int state)
        {
            samuraiState = (state > 2) ? 2 : state;
            fireState = state;
            if (fireState > 2)
            {   
                fireAnim.gameObject.SetActive(true);
                fireAnim.DoScaledAnimationAsync("Fire"+(fireState - 2), 0.5f);
            }
            switch (samuraiState)
            {
                case 0:
                samuraiAnim.DoScaledAnimationAsync("Idle", 0.5f);
                break;
                case 1:
                samuraiAnim.DoScaledAnimationAsync("Idle1", 0.5f);
                break;
                case 2:
                samuraiAnim.DoScaledAnimationAsync("Idle2", 0.5f);
                break;
            }
            sliceState = (state > 5) ? 5 : state;
        }
        public void Rest(double beat)
        {
            BeatAction.New(this, new()
            {
                new(beat, delegate {if(samuraiState >= 2){samuraiAnim.DoScaledAnimationAsync("Idle1", 0.5f);}}),
                new(beat+1, delegate
                {
                        samuraiAnim.DoScaledAnimationAsync("Rest", 0.5f);
                }),
            });
            fireDisappearing = true;
            fireLeaveBeat = beat;
        }
        public void Fog(double beat, float length, float opacity, bool longVersion, bool mute)
        {
            fogBeat = beat;
            fogLength = length;
            fogFadeStart = fogActive ? fogSprite[0].color.a : 0f;
            fogActive = opacity > 0f;
            fogAnim.DoScaledAnimationAsync("Idle", 0.5f);
            fogOpacity = opacity;
            if (!mute)
            {
                if (longVersion)
                {
                    // hello i love tenna and im saying this totally not becuz im getting held hostage by a shark guy
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("agbSamuraiSlice/fog1", beat),
                        new MultiSound.Sound("agbSamuraiSlice/fog2", beat+0.25f),
                        new MultiSound.Sound("agbSamuraiSlice/fog3", beat+0.5f),
                        new MultiSound.Sound("agbSamuraiSlice/fog4", beat+0.75f),
                        new MultiSound.Sound("agbSamuraiSlice/fog5", beat+1f),
                        new MultiSound.Sound("agbSamuraiSlice/fog6", beat+1.25f),
                        new MultiSound.Sound("agbSamuraiSlice/fog7", beat+1.5f),
                        new MultiSound.Sound("agbSamuraiSlice/fog8", beat+1.75f),
                        new MultiSound.Sound("agbSamuraiSlice/fog9", beat+2f),
                        new MultiSound.Sound("agbSamuraiSlice/fog10", beat+2.25f),
                        new MultiSound.Sound("agbSamuraiSlice/fog11", beat+2.5f),
                        new MultiSound.Sound("agbSamuraiSlice/fog12", beat+2.75f),
                        new MultiSound.Sound("agbSamuraiSlice/fog13", beat+3f),
                        new MultiSound.Sound("agbSamuraiSlice/fog14", beat+3.25f),
                        new MultiSound.Sound("agbSamuraiSlice/fog15", beat+3.5f),
                        new MultiSound.Sound("agbSamuraiSlice/fog16", beat+3.75f),
                        new MultiSound.Sound("agbSamuraiSlice/fog17", beat+4f),
                        new MultiSound.Sound("agbSamuraiSlice/fog18", beat+4.25f),
                        new MultiSound.Sound("agbSamuraiSlice/fog19", beat+4.5f),
                        new MultiSound.Sound("agbSamuraiSlice/fog20", beat+4.75f),
                        new MultiSound.Sound("agbSamuraiSlice/fog21", beat+5f),
                        new MultiSound.Sound("agbSamuraiSlice/fog22", beat+5.25f),
                        new MultiSound.Sound("agbSamuraiSlice/fog23", beat+5.5f),
                        new MultiSound.Sound("agbSamuraiSlice/fog24", beat+5.75f),
                        new MultiSound.Sound("agbSamuraiSlice/fog25short", beat+6f),
                        new MultiSound.Sound("agbSamuraiSlice/fog26", beat+6.25f),
                        new MultiSound.Sound("agbSamuraiSlice/fog27", beat+6.5f),
                        new MultiSound.Sound("agbSamuraiSlice/fog28", beat+6.75f),
                        new MultiSound.Sound("agbSamuraiSlice/fog29", beat+7f),
                        new MultiSound.Sound("agbSamuraiSlice/fog30", beat+7.25f),
                        new MultiSound.Sound("agbSamuraiSlice/fog31", beat+7.5f),
                        new MultiSound.Sound("agbSamuraiSlice/fog32", beat+7.75f),
                    });
                }
                else
                {
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("agbSamuraiSlice/fog1", beat),
                        new MultiSound.Sound("agbSamuraiSlice/fog2", beat+0.25f),
                        new MultiSound.Sound("agbSamuraiSlice/fog3", beat+0.5f),
                        new MultiSound.Sound("agbSamuraiSlice/fog4", beat+0.75f),
                        new MultiSound.Sound("agbSamuraiSlice/fog5", beat+1f),
                        new MultiSound.Sound("agbSamuraiSlice/fog6", beat+1.25f),
                        new MultiSound.Sound("agbSamuraiSlice/fog7", beat+1.5f),
                        new MultiSound.Sound("agbSamuraiSlice/fog8", beat+1.75f),
                        new MultiSound.Sound("agbSamuraiSlice/fog9", beat+2f),
                        new MultiSound.Sound("agbSamuraiSlice/fog10", beat+2.25f),
                        new MultiSound.Sound("agbSamuraiSlice/fog11", beat+2.5f),
                        new MultiSound.Sound("agbSamuraiSlice/fog12", beat+2.75f),
                        new MultiSound.Sound("agbSamuraiSlice/fog13", beat+3f),
                        new MultiSound.Sound("agbSamuraiSlice/fog14", beat+3.25f),
                        new MultiSound.Sound("agbSamuraiSlice/fog15", beat+3.5f),
                        new MultiSound.Sound("agbSamuraiSlice/fog16", beat+3.75f),
                        new MultiSound.Sound("agbSamuraiSlice/fog17", beat+4f),
                        new MultiSound.Sound("agbSamuraiSlice/fog18", beat+4.25f),
                        new MultiSound.Sound("agbSamuraiSlice/fog19", beat+4.5f),
                        new MultiSound.Sound("agbSamuraiSlice/fog20", beat+4.75f),
                        new MultiSound.Sound("agbSamuraiSlice/fog21", beat+5f),
                        new MultiSound.Sound("agbSamuraiSlice/fog22", beat+5.25f),
                        new MultiSound.Sound("agbSamuraiSlice/fog23", beat+5.5f),
                        new MultiSound.Sound("agbSamuraiSlice/fog24", beat+5.75f),
                        new MultiSound.Sound("agbSamuraiSlice/fog25long", beat+6f),
                    });
                }
            }
        }
        public void FogSlice(bool barely)
        {
            if (!fogActive) return;
            if (barely) foreach (var fogPart in fogSprite) { fogPart.color = new Color(1f, 1f, 1f, 0f); }
            else fogAnim.DoScaledAnimationAsync("SliceFog", 0.5f);
            fogActive = false;
        }
        public void FireColorSet(int colorNumbers, Color color1, Color color2, Color color3, Color color4, Color color5, Color color6, Color colorStart, Color colorEnd)
        {
            fireColorNumberSix = colorNumbers == 1;
        }
    }
}