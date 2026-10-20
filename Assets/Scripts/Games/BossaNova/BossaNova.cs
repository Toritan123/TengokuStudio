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
    public static class RvlReceiveLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            RiqEntity ThrowCueUpdater(string datamodel, RiqEntity entity)
            {
                if (datamodel == "bossaNova/throwCube" || datamodel == "bossaNova/throwBall")
                {
                    if (entity.version >= 1) return entity;

                    entity.beat += 1f;
                    entity.version = 1;

                    return entity;
                }
                return null;
            }
            RiqBeatmap.OnUpdateEntity += ThrowCueUpdater;

            return new Minigame("bossaNova", "Bossa Nova", "318552", false, false, new List<GameAction>()
            {
                new GameAction("pattern", "Pattern")
                {
                    preFunction = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out BossaNova instance)) {
                            var e = eventCaller.currentEntity;
                            instance.Pattern(e.beat);
                        }
                    },
                    preFunctionLength = 2f,
                    defaultLength = 4f
                },
                new GameAction("throwBall", "Throw Ball")
                {
                    preFunction = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out BossaNova instance)) {
                            var e = eventCaller.currentEntity;
                            instance.ThrowBall(e.beat - 1, e["voice"]);
                        }
                    },
                    defaultLength = 0.5f,
                    defaultVersion = 1,
                    preFunctionLength = 1f,
                    advanced = true,
                    parameters = new List<Param>()
                    {
                        new Param("voice", BossaNova.HitVoiceMale.MaleBounce1, "Voice Line", "Choose the voice line that will play upon hit."),
                    },
                },
                new GameAction("throwCube", "Throw Cube")
                {
                    preFunction = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out BossaNova instance)) {
                            var e = eventCaller.currentEntity;
                            instance.ThrowCube(e.beat - 1, e["voice"]);
                        }
                    },
                    defaultLength = 0.5f,
                    defaultVersion = 1,
                    preFunctionLength = 1f,
                    advanced = true,
                    parameters = new List<Param>()
                    {
                        new Param("voice", BossaNova.HitVoiceFemale.FemaleBounce4, "Voice Line", "Choose the voice line that will play upon hit."),
                    },
                },
                new GameAction("spin", "Switch Positions")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out BossaNova instance)) {
                            var e = eventCaller.currentEntity;
                            instance.Spin(e.beat);
                        }
                    },
                    defaultLength = 2f
                },
                new GameAction("forcePosition", "Force Positions")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out BossaNova instance)) {
                            var e = eventCaller.currentEntity;
                            instance.ForcePosition(e.beat, e["side"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("side", BossaNova.BossaSide.Left, "Bossa Side", "Choose the side Bossa will end up in."),
                    },
                },
                new GameAction("emotion", "Set Voice Variants")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out BossaNova instance)) {
                            var e = eventCaller.currentEntity;
                            instance.SetVoice(e.beat, e["voice"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("voice", BossaNova.VoiceVariant.Normal, "Voice Variant", "Choose the variant of the voices."),
                    },
                },
                new GameAction("setBG", "Background Appearance")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out BossaNova instance)) {
                            var e = eventCaller.currentEntity;
                            instance.SetBG(e.beat, e.length, e["type"], e["colorStart"], e["colorEnd"], e["ease"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("type", BossaNova.BackgroundType.OnePlayer, "Background Type", "Choose the variant of the background.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, y) => (int)x == (int)BossaNova.BackgroundType.TwoPlayer, new string[] { "colorStart", "colorEnd", "ease" })
                        }),
                        new Param("colorStart", new Color(0.4509804f, 0.4509804f, 0.4509804f), "Start BG Color", "Set the color at the start of the event."),
                        new Param("colorEnd", new Color(0.4509804f, 0.4509804f, 0.4509804f), "End BG Color", "Set the color at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Instant, "Ease", "Set the easing of the action.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, y) => (int)x != (int)Util.EasingFunction.Ease.Instant, new string[] { "colorStart" })
                        }),
                    },
                    defaultLength = 1f,
                    resizable = true,
                },
            },
            new List<string>() { "rvl", "keep" },
            "rvlreceive", "en",
            new List<string>() { "en", "jp" },
            chronologicalSortKey: 28,
            font: "FOT-SlumpStd-DB SDF"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_BossaNova;

    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
    public class BossaNova : Minigame
    {
        public enum HitVoiceline
        {
            MaleBounce1,
            MaleBounce3,
            MaleBounce4,
            MaleBounce6,
            MaleSpin,
            FemaleBounce2,
            FemaleBounce4,
            FemaleSpin,
        }
        public enum HitVoiceMale
        {
            MaleBounce1,
            MaleBounce3,
            MaleBounce4,
            MaleBounce6,
            MaleSpin,
        }
        public enum HitVoiceFemale
        {
            FemaleBounce2 = 5,
            FemaleBounce4 = 6,
            FemaleSpin = 7,
        }
        public enum VoiceVariant
        {
            Normal = 1,
            Playful = 2,
        }
        public enum BossaSide
        {
            Left,
            Right
        }
        public enum BackgroundType
        {
            OnePlayer,
            TwoPlayer
        }


        [SerializeField] public Animator bossaAnim;
        [SerializeField] public Animator novaAnim;
        [SerializeField] public ParticleSystem ringL;
        [SerializeField] public ParticleSystem ringR;
        [SerializeField] Animator cloudAnim;
        [SerializeField] Animator positionAnim;
        [SerializeField] Shape ballShape;
        [SerializeField] Shape cubeShape;
        [SerializeField] GameObject bgOne;
        [SerializeField] GameObject bgTwo;
        [SerializeField] SpriteRenderer bgTwoSR;

        private bool bossaR = false;
        private bool bossaWhiffR = false;

        public int angerLevel;

        int voiceVariant;
        private bool alternateSpinVoice;

        public int emotion = 1; // 0 = miss, 1 = default, 2 = playful

        public int bossaPlayfulSpinRandomization;

        private void Update()
        {
            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            {
                bossaAnim.DoScaledAnimationAsync("Whiff", 0.5f);
                SoundByte.PlayOneShotGame(bossaWhiffR ? "bossaNova/SE_BOSSA_EN_SWING_BALL" : "bossaNova/SE_BOSSA_EN_SWING_NUT");
            }
            BackgroundColorUpdate();
        }

        public void BackgroundColorUpdate()
        {
            bgTwoSR.color = bgColorEase.GetColor();
        }

        //call this in OnPlay(double beat) and OnGameSwitch(double beat)
        private void PersistColor(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("bossaNova", new string[] { "setBG" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var e = allEventsBeforeBeat[^1];
                SetBG(e.beat, e.length, e["type"], e["colorStart"], e["colorEnd"], e["ease"]);
            }
        }

        public override void OnGameSwitch(double beat)
        {
            DoInactivePattern(beat);
            PersistColor(beat);
        }

        public override void OnPlay(double beat)
        {
            PersistColor(beat);
        }

        private void DoInactivePattern(double beat)
        {
            RiqEntity patternEntity = gameManager.Beatmap.Entities.FindLast(e => e.beat - 2 < beat && e.beat + e.length >= beat && e.datamodel == "bossaNova/pattern");
            if (patternEntity != null)
            {
                RiqEntity e = patternEntity;
                Pattern(e.beat);
            }
        }

        public void Pattern(double beat)
        {

            bool spin = gameManager.Beatmap.Entities.FindAll(e => e.datamodel == "bossaNova/spin" && e.beat > beat && e.beat < beat + 4).Count > 0;

            var forceSide = gameManager.Beatmap.Entities.FindAll(e => e.datamodel == "bossaNova/forcePosition" && e.beat <= beat && e.beat > beat - 2);
            if (forceSide.Count > 0)
            {
                int side = forceSide.Count - 1;
                ForcePosition(forceSide[side].beat, forceSide[side]["side"]);
            }

            voiceVariant = (voiceVariant + 1) % (emotion == 2 ? 3 : 2);

            BeatAction.New(this, new List<BeatAction.Action>
            {
                new(beat - 1, delegate {
                    ThrowBall(beat - 1, 0, spin); 
                    if (!bossaR) SoundByte.PlayOneShotGame("bossaNova/Nova/SE_BOSSA_EN_36", beat, volume: 0.55f);
                }),
                new(beat - 0.25, delegate { if (angerLevel > 4) angerLevel = 4; }),
                new(beat, delegate {
                    ThrowCube(beat, 5, spin);
                }),
                new(beat + 0.5, delegate { ThrowBall(beat + 0.5, 1, spin); }),
                new(beat + 1, delegate { ThrowBall(beat + 1, spin ? 4 : 2, spin); }),
                new(beat + 1.5, delegate { ThrowCube(beat + 1.5, spin ? 7 : 6, spin); }),
                new(beat + 2.5, delegate { if (!spin) ThrowBall(beat + 2.5, 3); }),
            });
        }

        public void ThrowBall(double beat, int voiceline = 0, bool spin = false)
        {
            Shape newBall = Instantiate(ballShape, transform);
            newBall.startBeat = beat;
            newBall.isEntering = true;
            newBall.forBossa = bossaR;
            newBall.voiceline = voiceline;
            newBall.spin = spin;
            newBall.voiceVariant = voiceVariant;
            newBall.gameObject.SetActive(true);
        }

        public void ThrowCube(double beat, int voiceline = 5, bool spin = false)
        {
            Shape newCube = Instantiate(cubeShape, transform);
            newCube.startBeat = beat;
            newCube.isEntering = true;
            newCube.forBossa = !bossaR;
            newCube.voiceline = voiceline;
            newCube.spin = spin;
            newCube.voiceVariant = voiceVariant;
            newCube.gameObject.SetActive(true);
        }

        public void Spin(double beat)
        {
            bossaR = !bossaR;
            MultiSound.Play(new List<MultiSound.Sound>()
            {
                new("bossaNova/SE_BOSSA_EN_CHANGE_PUSH", beat),
                new("bossaNova/SE_BOSSA_EN_CHANGE_ROLL", beat+1),
                new("bossaNova/SE_BOSSA_EN_CHANGE_ROLL_1", beat+1),
                new("bossaNova/SE_BOSSA_EN_CHANGE_ROLL_2", beat+1.25),
                new("bossaNova/SE_BOSSA_EN_CHANGE_ROLL_3", beat+1.5),
                new("bossaNova/SE_BOSSA_EN_CHANGE_ROLL_4", beat+1.75),
            });

            BeatAction.New(this, new List<BeatAction.Action> {
                new(beat, delegate {
                    cloudAnim.DoScaledAnimationAsync("Sink", 0.5f);
                    positionAnim.DoScaledAnimationAsync(!bossaR ? "Sink Right" : "Sink Left", 0.5f);
                    SpinVoice(beat + 1);
                }),
                new(beat + 1, delegate {
                bossaWhiffR = bossaR;

                bossaAnim.DoScaledAnimationAsync(bossaR ? "Spin Right" : "Spin Left", 0.5f);
                novaAnim.DoScaledAnimationAsync(bossaR ? "Spin Left" : "Spin Right", 0.5f);
                positionAnim.DoScaledAnimationAsync(bossaR ? "Spin Right" : "Spin Left", 0.5f);
                cloudAnim.DoScaledAnimationAsync("Spin", 0.5f);
            }) });
        }

        public void SetVoice(double beat, int voice)
        {
            if (emotion != 0) emotion = voice;
            if (emotion != 1 && voiceVariant == 2) voiceVariant = 0;
        }

        private void SpinVoice(double beat)
        {
            if (bossaR) alternateSpinVoice = !alternateSpinVoice;
            switch (emotion)
            {
                case 1:
                    if (bossaR) SoundByte.PlayOneShotGame(alternateSpinVoice ? "bossaNova/Bossa/SE_BOSSA_EN_60" : "bossaNova/Bossa/SE_BOSSA_EN_61", beat, volume: 0.62f);
                    else SoundByte.PlayOneShotGame(alternateSpinVoice ? "bossaNova/Nova/SE_BOSSA_EN_64" : "bossaNova/Nova/SE_BOSSA_EN_65", beat, volume: 0.22f);
                    break;
                case 2:
                    if (bossaR) SoundByte.PlayOneShotGame(alternateSpinVoice ? "bossaNova/Bossa/SE_BOSSA_EN_62" : "bossaNova/Bossa/SE_BOSSA_EN_63", beat, volume: 0.62f);
                    else SoundByte.PlayOneShotGame(alternateSpinVoice ? "bossaNova/Nova/SE_BOSSA_EN_66" : "bossaNova/Nova/SE_BOSSA_EN_67", beat, volume: 0.22f);
                    break;
            }
        }

        public void ForcePosition(double beat, int bossaPos)
        {
            bossaR = bossaPos == 1;
            positionAnim.DoScaledAnimationAsync(bossaR ? "IdleR" : "IdleL", 0.5f);
        }

        private ColorEase bgColorEase = new(new Color(0.4509804f, 0.4509804f, 0.4509804f));
        public void SetBG(double beat, float length, int type, Color colorStart, Color colorEnd, int ease)
        {
            if (type == 0)
            {
                bgOne.SetActive(true);
                bgTwo.SetActive(false);
            }
            else
            {
                bgOne.SetActive(false);
                bgTwo.SetActive(true);
                bgColorEase = new ColorEase(beat, length, colorStart, colorEnd, ease);
            }
        }
    }
}