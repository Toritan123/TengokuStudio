using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

using HeavenStudio.VFX;
using HeavenStudio.Util;

using Jukebox;
using XPostProcessing;
using PPU;

namespace HeavenStudio.Games.Global
{
    public class VFXManager : MonoBehaviour
    {
        // Because of how HS serializes enums, we have to number these manually to make sure a level doesn't break if we add more.
        // Backwards compatibility is preserved as long as new elements are appended to the end of this enum. (!DO NOT TRY TO SORT THESE!)
        public enum FilterType
        {
            accent = 0,
            air = 1,
            atri = 2,
            bleach = 3,
            bleak = 4,
            blockbuster = 5,
            cinecold = 6,
            cinewarm = 7,
            colorshift = 8,
            dawn = 9,
            deepfry = 10,
            deuteranopia = 11,
            exposed = 12,
            friend = 13,
            friend_diffusion = 14,
            gamebob = 15,
            gamebob_2 = 16,
            gameboy = 17,
            gameboy_color = 18,
            glare = 19,
            grayscale = 20,
            grayscale_invert = 21,
            invert = 22,
            iso_blue = 23,
            iso_cyan = 24,
            iso_green = 25,
            iso_highlights = 26,
            iso_magenta = 27,
            iso_mid = 28,
            iso_red = 29,
            iso_shadows = 30,
            iso_yellow = 31,
            maritime = 32,
            moonlight = 33,
            nightfall = 34,
            polar = 35,
            poster = 36,
            protanopia = 37,
            redder = 38,
            sanic = 39,
            sepia = 40,
            sepier = 41,
            sepiest = 42,
            shareware = 43,
            shift_behind = 44,
            shift_left = 45,
            shift_right = 46,
            tina = 47,
            tiny_palette = 48,
            toxic = 49,
            tritanopia = 50,
            vibrance = 51,
            winter = 52,
            blackwhite = 53,
            blackwhite_2 = 54,
        }

        public readonly List<AmplifyColorEffect> amplifies = new();
        public readonly List<Texture2D> amplifyTextures = new(); // All available camera filters in texture format

        [Header("Transforms")]
        private Transform pooledObjectsParent;

        [Header("Post-Processing VFX")]
        [SerializeField] PostProcessVolume _volume;

        private VFXInstancePPE<Vignette> vignette;
        private VFXInstancePPE<ChromaticAberration> cabb;
        private VFXInstancePPE<Bloom> bloom;
        private VFXInstancePPE<LensDistortion> lensD;
        private VFXInstancePPE<Grain> grain;
        private VFXInstancePPE<ColorGrading> colorGrading;
        private VFXInstancePPE<CRT> retroTv;
        private VFXInstancePPE<VHS> vhs;
        private VFXInstancePPE<GlitchScanLineJitter> scanJitter;
        private VFXInstancePPE<GaussianBlur> gaussBlur;
        private VFXInstancePPE<GrainyBlur> grainBlur;
        private VFXInstancePPE<DirectionalBlur> dirBlur;
        private VFXInstancePPE<GlitchAnalogNoise> analogNoise;
        private VFXInstancePPE<GlitchScreenJump> screenJump;
        private VFXInstancePPE<EdgeDetectionSobelNeonV2> sobelNeon;
        private VFXInstancePPE<PixelizeQuad> pixelizeQuad;
        private VFXInstancePPE<ColorReplace> colorReplace;
        private VFXInstancePPE<ColorAdjustmentTechnicolor> colorBalance;
        private VFXInstancePPE<Drunk> liquidScreen;
        private VFXInstancePPE<EdgeDetectionSobel> edgeDetect;
        private VFXInstancePPE<AuroraVignette> aurora;

        private List<IVFXInstance> allPpeVfx;

        [Header("Single VFX")]
        private VFXInstance<UnityEngine.Object> stretchCamera;
        private VFXInstance<UnityEngine.Object> filter;
        private List<IVFXInstance> allSingleVfx;
        bool[] colorSlotCleared = new bool[5];
        bool[] colorSlotUsedThisFrame = new bool[5];

        [Header("Object VFX")]
        public IdolObject IdolObjectPrefab;

        private VFXInstanceObject<IdolObject> idol;
        private List<IVFXInstance> allObjectVfx;

        [Header("Pooled Object VFX")]
        [SerializeField] TextboxObject TextboxObject;
        [SerializeField] OpenCaptionsObject OpenCaptionsObject;
        [SerializeField] ClosedCaptionsObject ClosedCaptionsObject;
        [SerializeField] DecalObject DecalObject;
        [SerializeField] TextDecalObject TextDecalObject;

        private VFXInstancePooled<TextboxObject> textboxes;
        private VFXInstancePooled<OpenCaptionsObject> openCaptions;
        private VFXInstancePooled<ClosedCaptionsObject> closedCaptions;
        private VFXInstancePooled<DecalObject> decals;
        private VFXInstancePooled<TextDecalObject> textDecals;

        private List<IVFXInstance> allPooledVfx;

        private List<IVFXInstance> allVfx;
        private Conductor conductor;

        private void Awake()
        {
            InitVfxInstances();
        }

        public void Start()
        {
            GameManager.instance.onBeatChanged += OnBeatChanged;

            InitAmplifies();

            Update();
            conductor = Conductor.instance;
        }

        private void InitVfxInstances()
        {
            PostProcessVolume v = _volume;
            allPpeVfx = new() {
                (vignette     = new("ppe/vignette",     v, UpdateVignette           )),
                (cabb         = new("ppe/cabb",         v, UpdateChromaticAbberation)),
                (bloom        = new("ppe/bloom",        v, UpdateBloom              )),
                (lensD        = new("ppe/lensD",        v, UpdateLensDistortion     )),
                (grain        = new("ppe/grain",        v, UpdateGrain              )),
                (colorGrading = new("ppe/colorGrading", v, UpdateColorGrading       )),
                (colorBalance = new("ppe/colorGrading", v, UpdateColorBalance       )),
                (retroTv      = new("ppe/retroTv",      v, UpdateRetroTV            )),
                (vhs          = new("ppe/retroTv",      v, UpdateVHS                )),
                (scanJitter   = new("ppe/scanJitter",   v, UpdateScanJitter         )),
                (gaussBlur    = new("ppe/gaussBlur",    v, UpdateGaussianBlur       )),
                (grainBlur    = new("ppe/grainBlur",    v, UpdateGrainBlur          )),
                (dirBlur      = new("ppe/dirBlur",      v, UpdateDirBlur            )),
                (analogNoise  = new("ppe/analogNoise",  v, UpdateAnalogNoise        )),
                (screenJump   = new("ppe/screenJump",   v, UpdateScreenJump         )),
                (sobelNeon    = new("ppe/sobelNeon",    v, UpdateSobelNeon          )),
                (pixelizeQuad = new("ppe/pixelQuad",    v, UpdatePixelizeQuad       )),
                (colorReplace = new("ppe/colorReplace", v, UpdateColorReplace       )),
                (liquidScreen = new("ppe/liquidScreen", v, UpdateLiquidScreen       )),
                (edgeDetect   = new("ppe/edgeDetect",   v, UpdateEdgeDetect         )),
                (aurora       = new("ppe/aurora",       v, UpdateAurora             )),
            };

            allSingleVfx = new() {
                (stretchCamera = new("vfx/stretch camera", null, UpdateStretchCamera)),
                (filter        = new("vfx/filter",         null, UpdateFilter       )),
            };

            allObjectVfx = new()
            {
                (idol = new("vfx/display song artist", Instantiate(IdolObjectPrefab, transform), UpdateIdol, true)),
            };

            pooledObjectsParent = new GameObject("PooledObjectsParent").transform;
            pooledObjectsParent.parent = transform;

            allPooledVfx = new() {
                (textboxes      = new("vfx/display textbox",         TextboxObject,        pooledObjectsParent, UpdateTextObject)),
                (openCaptions   = new("vfx/display open captions",   OpenCaptionsObject,   pooledObjectsParent, UpdateTextObject)),
                (closedCaptions = new("vfx/display closed captions", ClosedCaptionsObject, pooledObjectsParent, UpdateTextObject)),
                (decals         = new("vfx/display decal",           DecalObject,          pooledObjectsParent, UpdateDecal     )),
                (textDecals     = new("vfx/display text decal",      TextDecalObject,      pooledObjectsParent, UpdateTextDecal     )),
            };

            allVfx = new(allPpeVfx.Count + allSingleVfx.Count + allObjectVfx.Count + allPooledVfx.Count);
            allVfx.AddRange(allPpeVfx);
            allVfx.AddRange(allSingleVfx);
            allVfx.AddRange(allObjectVfx);
            allVfx.AddRange(allPooledVfx);
        }

        private void InitAmplifies()
        {
            foreach (var filt in Enum.GetNames(typeof(FilterType)))
            {
                amplifyTextures.Add(Resources.Load<Texture2D>(Path.Combine("Filters/", filt)));
            }

            for (int i = 0; i < 10; i++)
            {
                var newAmplify = GameCamera.instance.gameObject.AddComponent<AmplifyColorEffect>();
                amplifies.Add(newAmplify);
            }
        }

        public void OnBeatChanged(double beat)
        {
            foreach (var vfx in allVfx)
            {
                vfx.ResetVfx();
            }
            EventCaller.instance.GamesHolder.transform.localScale = Vector3.one;

            foreach (var entity in GameManager.instance.Beatmap.Entities)
            {
                // optimize this somehow
                // i was thinking hashing, but that wouldn't work with multiple same datamodels
                foreach (var vfx in allVfx)
                {
                    if (vfx.Datamodel == entity.datamodel) {
                        vfx.AddEntity(entity);
                    }
                }
            }

            Update();

            if (!idolShown)
            {
                idol.vfxObject.Animator.Play("NoPose", -1, 0);
                idol.vfxObject.Animator.speed = 1;
            }
        }


        public void Update()
        {
            foreach (var a in amplifies)
            {
                a.LutTexture = null;
                a.BlendAmount = 1;
            }

            foreach (var vfx in allVfx)
            {
                foreach (var e in vfx.VfxEvents)
                {
                    float prog = conductor.GetPositionFromBeat(e.beat, e.length);
                    bool shouldBreak = vfx.UpdateVfx(e, prog);
                    if (shouldBreak) {
                        break;
                    }
                }
            }

            // this probably isnt the best way or place to do this but we ball
            for (int i = 0; i <= 4; i++)
            {
                if (!colorSlotUsedThisFrame[i] && !colorSlotCleared[i])
                {
                    ClearColorReplace(colorReplace.vfxObject, i + 1);
                    colorSlotCleared[i] = true;
                }
                colorSlotUsedThisFrame[i] = false;
            }
        }

        private bool UpdateVignette(RiqEntity e, float prog, Vignette v)
        {
            prog = Mathf.Clamp01(prog);
            var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

            float newIntensity = func(e["intenStart"], e["intenEnd"], prog);

            v.enabled.Override(newIntensity != 0 && e["enable"]);
            if (!v.enabled) return false;
            v.rounded.Override(e["rounded"]);

            Color newColor = VfxColorEase(e["colorStart"], e["colorEnd"], prog, func);

            v.color.Override(newColor);

            v.intensity.Override(newIntensity);

            float newSmoothness = func(e["smoothStart"], e["smoothEnd"], prog);
            v.smoothness.Override(newSmoothness);

            float newRoundness = func(e["roundStart"], e["roundEnd"], prog);
            v.roundness.Override(newRoundness);

            float newXPos = func(e["xLocStart"], e["xLocEnd"], prog);
            float newYPos = func(e["yLocStart"], e["yLocEnd"], prog);
            v.center.Override( new Vector2Parameter { value = new Vector2(newXPos, newYPos) });
            return false;
        }

        private bool UpdateChromaticAbberation(RiqEntity e, float prog, ChromaticAberration c)
        {
            prog = Mathf.Clamp01(prog);
            var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

            float newIntensity = func(e["intenStart"], e["intenEnd"], prog);
            c.enabled.Override(newIntensity != 0 && e["enable"]);
            if (!c.enabled) return false;
            c.intensity.Override(newIntensity);
            return false;
        }

        private bool UpdateBloom(RiqEntity e, float prog, Bloom b)
        {
            prog = Mathf.Clamp01(prog);
            var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

            float newIntensity = func(e["intenStart"], e["intenEnd"], prog);
            b.enabled.Override(newIntensity != 0 && e["enable"]);
            if (!b.enabled) return false;
            b.intensity.Override(newIntensity);

            Color newColor = VfxColorEase(e["colorStart"], e["colorEnd"], prog, func);

            b.color.Override(newColor);

            float newThreshold = func(e["thresholdStart"], e["thresholdEnd"], prog);
            b.threshold.Override(newThreshold);

            float newSoftKnee = func(e["softKneeStart"], e["softKneeEnd"], prog);
            b.softKnee.Override(newSoftKnee);

            float newAna = func(e["anaStart"], e["anaEnd"], prog);
            b.anamorphicRatio.Override(newAna);
            return false;
        }

        private bool UpdateLensDistortion(RiqEntity e, float prog, LensDistortion l)
        {
            prog = Mathf.Clamp01(prog);
            var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

            float newIntensity = func(e["intenStart"], e["intenEnd"], prog);
            l.enabled.Override(newIntensity != 0 && e["enable"]);
            if (!l.enabled) return false;
            l.intensity.Override(newIntensity);

            float newX = func(e["xStart"], e["xEnd"], prog);
            l.intensityX.Override(newX);

            float newY = func(e["yStart"], e["yEnd"], prog);
            l.intensityY.Override(newY);
            return false;
        }

        private bool UpdateGrain(RiqEntity e, float prog, Grain g)
        {
            prog = Mathf.Clamp01(prog);
            var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

            float newIntensity = func(e["intenStart"], e["intenEnd"], prog);
            g.enabled.Override(newIntensity != 0 && e["enable"]);
            if (!g.enabled) return false;
            g.intensity.Override(newIntensity);
            g.colored.Override(e["colored"]);

            float newSize = func(e["sizeStart"], e["sizeEnd"], prog);
            g.size.Override(newSize);
            return false;
        }

        private bool UpdateColorGrading(RiqEntity e, float prog, ColorGrading c)
        {
            prog = Mathf.Clamp01(prog);
            var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

            c.enabled.Override(e["enable"]);
            if (!c.enabled) return false;

            float newTemp = func(e["tempStart"], e["tempEnd"], prog);
            c.temperature.Override(newTemp);

            float newTint = func(e["tintStart"], e["tintEnd"], prog);
            c.tint.Override(newTint);

            Color newColor = VfxColorEase(e["colorStart"], e["colorEnd"], prog, func);
            c.colorFilter.Override(newColor);

            float newHue = func(e["hueShiftStart"], e["hueShiftEnd"], prog);
            c.hueShift.Override(newHue);

            float newSat = func(e["satStart"], e["satEnd"], prog);
            c.saturation.Override(newSat);

            float newBright = func(e["brightStart"], e["brightEnd"], prog);
            c.brightness.Override(newBright);

            float newCon = func(e["conStart"], e["conEnd"], prog);
            c.contrast.Override(newCon);
            return false;
        }

        private bool UpdateColorBalance(RiqEntity e, float prog, ColorAdjustmentTechnicolor cb)
        {
            cb.enabled.Override(e["technicolor"] && e["enable"]);
            if (!cb.enabled) return false;
            
            prog = Mathf.Clamp01(prog);
            var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]); 

            float newTech = func(e["techStart"], e["techEnd"], prog);
            cb.indensity.Override(newTech);

            float newExposure = func(e["exposureStart"], e["exposureEnd"], prog);
            cb.exposure.Override(newExposure);

            float newRed = func(e["redStart"], e["redEnd"], prog);
            cb.colorBalanceR.Override(newRed);

            float newGreen = func(e["greenStart"], e["greenEnd"], prog);
            cb.colorBalanceG.Override(newGreen);

            float newBlue = func(e["blueStart"], e["blueEnd"], prog);
            cb.colorBalanceB.Override(newBlue);
            return false;
        }

        private bool UpdateRetroTV(RiqEntity e, float prog, CRT t)
        {
            prog = Mathf.Clamp01(prog);
            var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

            float newIntensity = func(e["intenStart"], e["intenEnd"], prog);
            t.enabled.Override(newIntensity != 0 && e["enable"]);
            if (!t.enabled) return false;
            t.distort.Override(newIntensity);

            float newRGBBlend = func(e["rgbStart"], e["rgbEnd"], prog);
            t.RGBBlend.Override(newRGBBlend);

            float newBottomCollapse = func(e["bottomStart"], e["bottomEnd"], prog);
            t.BottomCollapse.Override(newBottomCollapse);

            float newNoiseAmount = func(e["noiseStart"], e["noiseEnd"], prog);
            t.NoiseAmount.Override(newNoiseAmount);
            return false;
        }

        private bool UpdateVHS(RiqEntity e, float prog, VHS tb)
        {
            tb.enabled.Override(e["HSonVHS"] && e["enable"]);
            if (!tb.enabled) return false;

            prog = Mathf.Clamp01(prog);
            var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);   

            float newBleedIntensity = func(e["bleedIntStart"], e["bleedIntEnd"], prog);
            tb.colorBleedingIntensity.Override(newBleedIntensity);              

            tb.colorBleedIterations.Override(e["bleedIteration"]) ;

            float newGrainIntensity = func(e["vhsGrainStart"], e["vhsGrainEnd"], prog);
            tb.grainIntensity.Override(newGrainIntensity); 

            float newGrainScale = func(e["grainScaleStart"], e["grainScaleEnd"], prog);
            tb.grainScale.Override(newGrainScale);

            float newStripeDensity = func(e["stripeDenStart"], e["stripeDenEnd"], prog);
            tb.stripeNoiseDensity.Override(newStripeDensity);

            float newStripeOpacity = func(e["stripeOpacStart"], e["stripeOpacEnd"], prog);
            tb.stripeNoiseOpacity.Override(newStripeOpacity);

            float newEdgeSharpness = func(e["edgeIntStart"], e["edgeIntEnd"], prog);
            tb.edgeIntensity.Override(newEdgeSharpness);

            float newEdgeDistance = func(e["edgeDistStart"], e["edgeDistEnd"], prog);
            tb.edgeDistance.Override(newEdgeDistance);
            return false;
        }

        private bool UpdateScanJitter(RiqEntity e, float prog, GlitchScanLineJitter j)
        {
            prog = Mathf.Clamp01(prog);
            var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

            float newIntensity = func(e["intenStart"], e["intenEnd"], prog);
            j.enabled.Override(newIntensity != 0 && e["enable"]);
            if (!j.enabled) return false;
            j.JitterIndensity.Override(newIntensity);
            return false;
        }

        private bool UpdateGaussianBlur(RiqEntity e, float prog, GaussianBlur g)
        {
            prog = Mathf.Clamp01(prog);
            var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

            float newIntensity = func(e["intenStart"], e["intenEnd"], prog);
            g.enabled.Override(newIntensity != 0 && e["enable"]);
            if (!g.enabled) return false;
            g.BlurRadius.Override(newIntensity);
            return false;
        }

        private bool UpdateGrainBlur(RiqEntity e, float prog, GrainyBlur gr)
        {
            prog = Mathf.Clamp01(prog);
            var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

            float newIntensity = func(e["intenStart"], e["intenEnd"], prog);
            gr.enabled.Override(newIntensity != 0 && e["enable"]);
            if (!gr.enabled) return false;
            gr.BlurRadius.Override(newIntensity);
            return false;
        }

        private bool UpdateLiquidScreen(RiqEntity e, float prog, Drunk ls)
        {
            //prog = Mathf.Clamp01(prog);
            //var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease.Linear));
            float newIntensity = e["intenEnd"];
            ls.enabled.Override(newIntensity != 0 && e["enable"]);

            if (!ls.enabled) return false;
            ls.Speed.Override(newIntensity);

            float newHoriz = e["horizEnd"];
            ls.Horizontal.Override(newHoriz);

            float newVert = e["vertEnd"];
            ls.Vertical.Override(newVert);
            
            return false;
        }

        private bool UpdateDirBlur(RiqEntity e, float prog, DirectionalBlur d)
        {
            prog = Mathf.Clamp01(prog);
            var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

            float newIntensity = func(e["intenStart"], e["intenEnd"], prog);
            d.enabled.Override(newIntensity != 0 && e["enable"]);
            if (!d.enabled) return false;
            d.BlurRadius.Override(newIntensity);

            float formula = Mathf.PI / 180;

            float newAngle = func(e["angleStart"] * formula, e["angleEnd"] * formula, prog);
            if (!d.enabled) return false;
            d.Angle.Override(newAngle);
            return false;
        }

        private bool UpdateAnalogNoise(RiqEntity e, float prog, GlitchAnalogNoise n)
        {
            prog = Mathf.Clamp01(prog);
            var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

            float newIntensity = func(e["intenStart"], e["intenEnd"], prog);
            n.enabled.Override(newIntensity != 0 && e["enable"]);
            if (!n.enabled) return false;
            n.NoiseSpeed.Override(newIntensity);

            float newFading = func(e["fadingStart"], e["fadingEnd"], prog);
            n.NoiseFading.Override(newFading);

            float newThreshold = func(e["thresholdStart"], e["thresholdEnd"], prog);
            n.LuminanceJitterThreshold.Override(newThreshold);
            return false;
        }

        private bool UpdateScreenJump(RiqEntity e, float prog, GlitchScreenJump sj)
        {
            prog = Mathf.Clamp01(prog);
            var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

            float newIntensity = func(e["intenStart"], e["intenEnd"], prog);
            sj.enabled.Override(newIntensity != 0 && e["enable"]);
            if (!sj.enabled) return false;
            sj.ScreenJumpIndensity.Override(newIntensity);
            return false;
        }

        private bool UpdateEdgeDetect(RiqEntity e, float prog, EdgeDetectionSobel ed)
        {
            prog = Mathf.Clamp01(prog);
            var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

            float newIntensity = func(e["intenStart"], e["intenEnd"], prog);
            ed.enabled.Override(newIntensity != 0.0 && e["enable"]);
            if (!ed.enabled) return false;
            ed.edgeWidth.Override(newIntensity);

            Color newColor = VfxColorEase(e["color"], e["color2"], prog, func);
            ed.edgeColor.Override(newColor);

            float newBg = func(e["bgStart"], e["bgEnd"], prog);
            ed.backgroundFade.Override(newBg);

            Color newBgColor = VfxColorEase(e["bgColor"], e["bgColor2"], prog, func);
            ed.backgroundColor.Override(newBgColor);
            return false;
        }

        private bool UpdateSobelNeon(RiqEntity e, float prog, EdgeDetectionSobelNeonV2 sn)
        {
            prog = Mathf.Clamp01(prog);
            var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

            float newIntensity = func(e["intenStart"], e["intenEnd"], prog);
            sn.enabled.Override(newIntensity != 0.1 && e["enable"]);
            if (!sn.enabled) return false;
            sn.EdgeNeonFade.Override(newIntensity);

            float newEdgeWidth = func(e["edgeWidthStart"], e["edgeWidthEnd"], prog);
            sn.EdgeWidth.Override(newEdgeWidth);

            float newBgFade = func(e["bgFadeStart"], e["bgFadeEnd"], prog);
            sn.BackgroundFade.Override(newBgFade);
            
            float newBrightness = func(e["brightnessStart"], e["brightnessEnd"], prog);
            sn.Brigtness.Override(newBrightness);
            return false;
        }

        private bool UpdateAurora(RiqEntity e, float prog, AuroraVignette av)
        {
            prog = Mathf.Clamp01(prog);
            var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

            float newIntensity = func(e["intenStart"], e["intenEnd"], prog);
            av.enabled.Override(newIntensity != 0.0 && e["enable"]);
            if (!av.enabled) return false;
            av.vignetteFading.Override(newIntensity);

            float newSize = func(e["sizeStart"], e["sizeEnd"], prog);
            av.vignetteArea.Override(newSize);

            float newSmoothness = func(e["smoothStart"], e["smoothEnd"], prog);
            av.vignetteSmothness.Override(newSize);

            float newSolidity = func(e["solidStart"], e["solidEnd"], prog);
            av.colorChange.Override(newSolidity);

            float newSpeed = e["speed"];
            av.flowSpeed.Override(newSpeed);

            float newColorR = func(e["redStart"], e["redEnd"], prog);
            av.colorFactorR.Override(newColorR);

            float newColorG = func(e["greenStart"], e["greenEnd"], prog);
            av.colorFactorG.Override(newColorG);

            float newColorB = func(e["blueStart"], e["blueEnd"], prog);
            av.colorFactorB.Override(newColorB);

            return false;

        }

        private bool UpdatePixelizeQuad(RiqEntity e, float prog, PixelizeQuad pq)
        {
            prog = Mathf.Clamp01(prog);
            var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

            float newPixelSize = func(e["pixelSizeStart"], e["pixelSizeEnd"], prog);
            pq.enabled.Override(newPixelSize != 0 && e["enable"]);
            if (!pq.enabled) return false;
            pq.pixelSize.Override(newPixelSize);

            float newPixelRatio = func(e["ratioStart"], e["ratioEnd"], prog);
            if (!pq.enabled) return false;
            pq.pixelRatio.Override(newPixelRatio);

            float newPixelXScale = func(e["xScaleStart"], e["xScaleEnd"], prog);
            if (!pq.enabled) return false;
            pq.pixelScaleX.Override(newPixelXScale);

            float newPixelYScale = func(e["yScaleStart"], e["yScaleEnd"], prog);
            if (!pq.enabled) return false;
            pq.pixelScaleY.Override(newPixelYScale);
            return false;
        }

        private bool UpdateColorReplace(RiqEntity e, float prog, ColorReplace cr)
        {
            cr.enabled.Override(e["enable"]);

            prog = Mathf.Clamp01(prog);
            var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

            int slot = e["colorSlot"];
            bool clear = e["clear"];

            if (clear)
            {
                if (!colorSlotCleared[slot - 1])
                {
                    ClearColorReplace(cr, slot);
                    colorSlotUsedThisFrame[slot - 1] = false;
                    colorSlotCleared[slot - 1] = true;
                }
                return false;
            }

            Color originalColor = VfxColorEase(e["originalColor"], e["originalColor2"], prog, func);
            Color newColor = VfxColorEase(e["newColor"], e["newColor2"], prog, func);
            float newRange = func(e["range"], e["range2"], prog);
            float newFuzziness = func(e["fuzziness"], e["fuzziness2"], prog);

            if (!cr.enabled) return false;
            colorSlotUsedThisFrame[slot - 1] = true;
            colorSlotCleared[slot - 1] = false;

            switch (slot)
            {
                case 1:
                    cr.FromColor1.Override(originalColor);
                    cr.ToColor1.Override(newColor);
                    cr.Range1.Override(newRange);
                    cr.Fuzziness1.Override(newFuzziness);
                    break;
                case 2:
                    cr.FromColor2.Override(originalColor);
                    cr.ToColor2.Override(newColor);
                    cr.Range2.Override(newRange);
                    cr.Fuzziness2.Override(newFuzziness);
                    break;
                case 3:
                    cr.FromColor3.Override(originalColor);
                    cr.ToColor3.Override(newColor);
                    cr.Range3.Override(newRange);
                    cr.Fuzziness3.Override(newFuzziness);
                    break;
                case 4:
                    cr.FromColor4.Override(originalColor);
                    cr.ToColor4.Override(newColor);
                    cr.Range4.Override(newRange);
                    cr.Fuzziness4.Override(newFuzziness);
                    break;
                case 5:
                    cr.FromColor5.Override(originalColor);
                    cr.ToColor5.Override(newColor);
                    cr.Range5.Override(newRange);
                    cr.Fuzziness5.Override(newFuzziness);
                    break;
                default:
                    break;
            }

            return false;
        }

        private void ClearColorReplace(ColorReplace cr, int slot)
        {
            switch (slot)
            {
                case 1:
                    cr.FromColor1.Override(Color.black);
                    cr.ToColor1.Override(Color.black);
                    cr.Range1.Override(0f);
                    cr.Fuzziness1.Override(0f);
                    break;
                case 2:
                    cr.FromColor2.Override(Color.black);
                    cr.ToColor2.Override(Color.black);
                    cr.Range2.Override(0f);
                    cr.Fuzziness2.Override(0f);
                    break;
                case 3:
                    cr.FromColor3.Override(Color.black);
                    cr.ToColor3.Override(Color.black);
                    cr.Range3.Override(0f);
                    cr.Fuzziness3.Override(0f);
                    break;
                case 4:
                    cr.FromColor4.Override(Color.black);
                    cr.ToColor4.Override(Color.black);
                    cr.Range4.Override(0f);
                    cr.Fuzziness4.Override(0f);
                    break;
                case 5:
                    cr.FromColor5.Override(Color.black);
                    cr.ToColor5.Override(Color.black);
                    cr.Range5.Override(0f);
                    cr.Fuzziness5.Override(0f);
                    break;
                default:
                    break;
            }
        }

        private bool UpdateStretchCamera(RiqEntity e, float prog, UnityEngine.Object _)
        {
            float clampNormal = Mathf.Clamp01(prog);
            var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

            StaticCamera.ViewAxis axis = (StaticCamera.ViewAxis)e["axis"];
            EventCaller.instance.GamesHolder.transform.localScale = new(
                axis is StaticCamera.ViewAxis.X or StaticCamera.ViewAxis.All ? func(e["x1"], e["x2"], clampNormal) : 1f,
                axis is StaticCamera.ViewAxis.Y or StaticCamera.ViewAxis.All ? func(e["y1"], e["y2"], clampNormal) : 1f,
                1 // if a text object's scale is 0, it completely breaks! fun fact.
            );

            return false;
        }

        private bool UpdateFilter(RiqEntity e, float prog, UnityEngine.Object _)
        {
            if (e.beat > Math.Max(conductor.songPositionInBeatsAsDouble, 0)) {
                return false;
            }

            var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);
            int slot = e["slot"];

            float intensity = func(1 - e["start"], 1 - e["end"], Mathf.Clamp01(prog));

            var amplify = amplifies[slot - 1];
            
            amplify.BlendAmount = intensity;

            if (e["custom"])
            {
                if (!GameManager.instance.ImportedSprites.TryGetValue(e["sprite"], out Sprite sprite)) return false;
                amplify.LutTexture = sprite.texture;
                return false;
            }

            amplify.LutTexture = amplifyTextures[(int)e["filter"]];

            return false;
        }

        private bool idolShown = false;
        private bool UpdateIdol(RiqEntity e, float prog, IdolObject idol)
        {
            if (prog is >= 0f and <= 1f)
            {
                float inp = e["instantOn"] ? 1 : conductor.GetPositionFromBeat(e.beat, 1);

                idol.SongLabel.text = e["text1"];
                idol.ArtistLabel.text = e["text2"];

                idol.Animator.Play("IdolShow", -1, Mathf.Min(inp, 1));
                idol.Animator.speed = 0;

                idolShown = true;

                return true;
            }
            else if (idolShown)
            {
                if (prog < 1f || e["instantOff"])
                {
                    idol.Animator.Play("NoPose", -1, 0);
                    idol.Animator.speed = 1;
                    idolShown = false;
                }
                else
                {
                    idol.Animator.Play("IdolHide", -1, 0);
                    idol.Animator.speed = (1f / conductor.pitchedSecPerBeat) * 0.5f;
                    idolShown = false;
                }
            }
            return false;
        }


        private bool UpdateTextObject(RiqEntity e, float prog, VFXObject obj)
        {
            obj.gameObject.SetActive(true);
            obj.UpdateObject(e, prog);
            return false;
        }

        private bool UpdateDecal(RiqEntity e, float prog, DecalObject decal)
        {
            if (!GameManager.instance.ImportedSprites.TryGetValue(e["sprite"], out Sprite sprite)) return false;
            var easingFunc = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);
            var newPos = new Vector3(
                easingFunc(e["sX"], e["eX"], prog),
                easingFunc(e["sY"], e["eY"], prog),
                easingFunc(e["sZ"], e["eZ"], prog)
            );
            var newSize = new Vector2(
                easingFunc(e["sWidth"], e["eWidth"], prog),
                easingFunc(e["sHeight"], e["eHeight"], prog)
            );
            var newRotation = easingFunc(e["sRot"], e["eRot"], prog);
            var newColor = VfxColorEase(e["sColor"], e["eColor"], prog, easingFunc);

            int displayLayer = 13;
            bool sticky = false;

            switch (e["displayLayer"])
            {
                case -1:
                    displayLayer = 29;
                    break;
                case 0:
                    displayLayer = 17;
                    sticky = e["sticky"];
                    break;
                case 2:
                    displayLayer = 18;
                    break;
                default:
                    break;
            }

            var filter = FilterMode.Bilinear;
            if (e["filter"] == 0) filter = FilterMode.Point;
            

            decal.UpdateDecal(sprite, filter, newPos, newSize, newRotation, newColor, e["layer"], displayLayer, sticky, true);
            return false;
        }

        private bool UpdateTextDecal(RiqEntity e, float prog, TextDecalObject decal)
        {
            var easingFunc = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);
            var newPos = new Vector2(
                easingFunc(e["sX"], e["eX"], prog),
                easingFunc(e["sY"], e["eY"], prog)
            );
            var newSize = new Vector2(
                easingFunc(e["sWidth"], e["eWidth"], prog),
                easingFunc(e["sHeight"], e["eHeight"], prog)
            );
            var newRotation = easingFunc(e["sRot"], e["eRot"], prog);

            var newColor = VfxColorEase(e["sColor"], e["eColor"], prog, easingFunc);

            var newOutlineColor = VfxColorEase(e["sOutlineColor"], e["eOutlineColor"], prog, easingFunc);
            var newOutlineWidth = easingFunc(e["sOutlineWidth"], e["eOutlineWidth"], prog);

            decal.UpdateTextDecal(e["text"], e["font"], newPos, newSize, newRotation, newColor, newOutlineColor, newOutlineWidth, e["layer"], true);
            return false;
        }

        public static Color VfxColorEase(Color start, Color end, float time, Util.EasingFunction.Function func)
        {
            Color newColor = new();
            for (int i = 0; i < 4; i++)
            {
                newColor[i] = func(start[i], end[i], time);
            }

            return newColor;
        }
    }
}
