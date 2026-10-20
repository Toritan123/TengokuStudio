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
    public static class PcoSheriffLoader
    {

        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("rhythmSheriff", "Rhythm Sheriff \n<color=#eb5454>[WIP]</color>", "BBB594", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; RhythmSheriff.instance.Bop(e.beat, e.length, e["toggle2"], e["toggle"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle2", true, "Bop", "Toggle if Dog Sheriff should bop for the duration of this event."),
                        new Param("toggle", false, "Bop (Auto)", "Toggle if Dog Sheriff should automatically bop until another Bop event is reached.")
                    }
                },

                new GameAction("slowtarget", "Slow Target")
                {
                    function = delegate { var e = eventCaller.currentEntity; RhythmSheriff.instance.SpawnTarget(e.beat, 0, e["auto"], e["holbeat"]); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("auto", true, "Auto Reholster", "Toggle if Dog Sheriff should reholster his gun on the next bop.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => !(bool)x, new string[] { "beat" })
                        }),
                        new Param("holbeat", new EntityTypes.Float(2.5f, 100f, 3f), "Reholster Beat", "Choose what beat Dog Sheriff should reholster his gun after shooting.")
                    }
                },

                new GameAction("fasttarget", "Fast Target")
                {
                    function = delegate { var e = eventCaller.currentEntity; RhythmSheriff.instance.SpawnTarget(e.beat, 1, e["auto"], e["holbeat"]); },
                    defaultLength = 2.5f,
                    parameters = new List<Param>()
                    {
                        new Param("auto", true, "Auto Reholster", "Toggle if Dog Sheriff should reholster his gun on the next bop.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => !(bool)x, new string[] { "beat" })
                        }),
                        new Param("holbeat", new EntityTypes.Float(2.5f, 100f, 3f), "Reholster Beat", "Choose what beat Dog Sheriff should reholster his gun after shooting.")
                    }
                },

                new GameAction("particle effects", "Tumbleweed Particles")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        RhythmSheriff.instance.TumbleweedParticles(e.beat, e["instant"], e["intensity"], e["color"], e["sizeBack"], e["sizeFront"], e["sizeVar"]);
                    },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        //new Param("type", SamuraiSliceNtr.ParticleType.None, "Particle", "Set the type of particle effect to spawn. Using \"None\" will stop all effects."),
                        new Param("instant", false, "Instant", "Toggle if the tumbleweeds should start or stop instantly."),
                        //new Param("valA", new EntityTypes.Float(0f, 15f, 1f), "Wind Strength", "Set the strength of the particle wind."), DEB55A 
                        new Param("intensity", new EntityTypes.Float(0f, 25f, 0.1f), "Intensity", "Set the intensity of the tumbleweeds."),
                        new Param("sizeBack", new EntityTypes.Float(0f, 3f, 1.2f), "Size (Back)", "Set the size of the tumbleweeds in the back."),
                        new Param("sizeFront", new EntityTypes.Float(0f, 3f, 1.6f), "Size (Front)", "Set the size of the tumbleweeds in the front."),
                        new Param("sizeVar", new EntityTypes.Float(0f, 3f, 0.1f), "Size Variation", "The amount to randomly grow or shrink the size of a tumbleweed."),
                        new Param("color", new Color(0.87f, 0.71f, 0.35f, 1), "Color", "Set the color of the tumbleweeds.")
                    },
                },

                new GameAction("tumbleweedstorm", "Tumbleweed Storm")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        RhythmSheriff.instance.TumbleweedStorm(e.beat, e.length, e["color"]);
                    },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        //new Param("type", SamuraiSliceNtr.ParticleType.None, "Particle", "Set the type of particle effect to spawn. Using \"None\" will stop all effects."),
                        //new Param("instant", false, "Instant", "Toggle if the tumbleweeds should start or stop instantly."),
                        //new Param("valA", new EntityTypes.Float(0f, 15f, 1f), "Wind Strength", "Set the strength of the particle wind."), DEB55A 
                        //new Param("intensity", new EntityTypes.Float(0f, 25f, 0.1f), "Intensity", "Set the intensity of the tumbleweeds."),
                        //new Param("sizeBack", new EntityTypes.Float(0f, 3f, 1.2f), "Size (Back)", "Set the size of the tumbleweeds in the back."),
                        //new Param("sizeFront", new EntityTypes.Float(0f, 3f, 1.6f), "Size (Front)", "Set the size of the tumbleweeds in the front."),
                        //new Param("sizeVar", new EntityTypes.Float(0f, 3f, 0.1f), "Size Variation", "The amount to randomly grow or shrink the size of a tumbleweed."),
                        new Param("color", new Color(0.87f, 0.71f, 0.35f, 1), "Color", "Set the color of the tumbleweeds.")
                    },
                },
            },

            new List<string>() { "pco", "normal" },
            "pcosheriff", "en",
            chronologicalSortKey: 20191101,
            font : "Matisse Minori",
            plusGame: true
            );
        }
    }
}

namespace HeavenStudio.Games
{
    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
    

    using Scripts_RhythmSheriff;
    public class RhythmSheriff : Minigame
    {

        [Header("Animators")]

        public Animator dogSheriff;

        [Header("Properties")]
        public float ratPitch;
        public float ratLowerPitch;
        public float ratFinalPitch;
        public float catPitch;
        public float catLowerPitch;
        public float catFinalPitch;

        [Header("Target")]
        public GameObject targetObj;
        [Header("Particles")]
        public ParticleSystem tumbleweedBack;
        public ParticleSystem tumbleweedFront;
        public ParticleSystem tumbleweedOverlay;

        public enum SheriffBopType
        {
            Normal = 1,
            Ready = 2,
            Return = 3,
            Spin = 4,
        }
        public SheriffBopType bopType = SheriffBopType.Normal;
        


        public static RhythmSheriff instance;

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
            SetupBopRegion("rhythmSheriff", "bop", "toggle");
        }

        public override void OnPlay(double beat)
        {
            ParticleSystem particleSystem = tumbleweedBack;
            particleSystem.gameObject.SetActive(true);
            particleSystem = tumbleweedFront;
            particleSystem.gameObject.SetActive(true);
        }

        void Update()
        {
            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            {
                SoundByte.PlayOneShotGame("rhythmSheriff/miss");
                SoundByte.PlayOneShotGame("rhythmSheriff/whiff");
                dogSheriff.DoScaledAnimationAsync("ShootNG", 0.5f);
            }
        }


        public override void OnLateBeatPulse(double beat)
        {
            if (!dogSheriff.IsPlayingAnimationNames("ShootNG", "ShootJust", "Ready") && BeatIsInBopRegion(beat))
            {
                if (bopType == SheriffBopType.Normal) { dogSheriff.DoScaledAnimationAsync("Bop", 0.5f); }
                else if (bopType == SheriffBopType.Ready) { dogSheriff.DoScaledAnimationAsync("Ready", 0.5f); }
                else if (bopType == SheriffBopType.Return) { dogSheriff.DoScaledAnimationAsync("Return", 0.5f);
                bopType = SheriffBopType.Normal;
                SoundByte.PlayOneShotGame("rhythmSheriff/holster");}
            }
        }

        public void Bop(double beat, float length, bool manualBop, bool goBop)
        {
            if (manualBop)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate
                        {
                            if (bopType == SheriffBopType.Normal) { dogSheriff.DoScaledAnimationAsync("Bop", 0.5f); }
                            else if (bopType == SheriffBopType.Ready) { dogSheriff.DoScaledAnimationAsync("Ready", 0.5f); }
                            else if (bopType == SheriffBopType.Return) { dogSheriff.DoScaledAnimationAsync("Return", 0.5f);
                            bopType = SheriffBopType.Normal;
                            SoundByte.PlayOneShotGame("rhythmSheriff/holster");}
                        })
                    });
                }
            }
        }

        public void SpawnTarget(double beat, int variation, bool autoReholster, float reholsterBeat)
        {
            if (variation == 0)
            {
                MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("rhythmSheriff/targetCat", beat, pitch: catPitch),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.000, pitch: catPitch - catLowerPitch, volume:0.2f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.125, pitch: catPitch - catLowerPitch, volume:0.3f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.250, pitch: catPitch - catLowerPitch, volume:0.4f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.375, pitch: catPitch - catLowerPitch, volume:0.5f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.500, pitch: catPitch - catLowerPitch, volume:0.6f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.625, pitch: catPitch - catLowerPitch, volume:0.7f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.750, pitch: catPitch - catLowerPitch, volume:0.8f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.875, pitch: catPitch - catLowerPitch, volume:0.9f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 2, pitch: catFinalPitch),
                });
            } else {
                MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("rhythmSheriff/targetCat", beat, pitch: ratPitch),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 0.500, pitch: ratPitch - ratLowerPitch, volume:0.9f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 0.625, pitch: ratPitch - ratLowerPitch, volume:0.7f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 0.750, pitch: ratPitch - ratLowerPitch, volume:0.5f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 0.875, pitch: ratPitch - ratLowerPitch, volume:0.3f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.000, pitch: ratPitch - ratLowerPitch, volume:0.9f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.125, pitch: ratPitch - ratLowerPitch, volume:0.7f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.250, pitch: ratPitch - ratLowerPitch, volume:0.5f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.375, pitch: ratPitch - ratLowerPitch, volume:0.3f),
                new MultiSound.Sound("rhythmSheriff/targetCat", beat + 1.5, pitch: ratFinalPitch),
                });
            }

            Targets targetClone = Instantiate(targetObj, gameObject.transform).GetComponent<Targets>();
            targetClone.beat = beat;
            targetClone.targetType = (Targets.TargetType)variation;

            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 0.25, delegate
                    {
                        bopType = SheriffBopType.Ready;
                    }),
                    new BeatAction.Action(beat + 1, delegate
                    {
                        if (!BeatIsInBopRegion(beat))
                        {
                            dogSheriff.DoScaledAnimationAsync("Ready", 0.5f);
                        }
                    })
            });
        }

        public void TumbleweedParticles(double beat, bool instant, float particleStrength, Color color, float sizeBack, float sizeFront, float sizeVar)
        {
            ParticleSystem particleSystem = tumbleweedBack;
            ParticleStuff(particleSystem, instant, particleStrength, color, sizeBack, sizeVar);

            particleSystem = tumbleweedFront;
            ParticleStuff(particleSystem, instant, particleStrength, color, sizeFront, sizeVar);
        }

        private void ParticleStuff(ParticleSystem particleSystem, bool instant, float particleStrength, Color color, float size, float sizeVar)
        {
            particleSystem.gameObject.SetActive(true);
            particleSystem.Play();

            var emm = particleSystem.emission;
            var main = particleSystem.main;
            var col = particleSystem.colorOverLifetime;

            emm.rateOverTime = particleStrength;
            main.prewarm = instant;
            col.color = color;

            var randomVariation = UnityEngine.Random.Range(sizeVar*-1, sizeVar);

            print(randomVariation);

            main.startSize = new ParticleSystem.MinMaxCurve(size - sizeVar, size + sizeVar);
        }

        public void TumbleweedStorm(double beat, float length, Color color)
        {
            ParticleSystem particleSystem = tumbleweedOverlay;
            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate
                    {
                        particleSystem.gameObject.SetActive(true);
                        particleSystem.Play();
                        var col = particleSystem.colorOverLifetime;
                        col.color = color;
                    }),
                    new BeatAction.Action(beat + length, delegate
                    {
                        particleSystem.Stop();
                    })
            });
        }

    }
}