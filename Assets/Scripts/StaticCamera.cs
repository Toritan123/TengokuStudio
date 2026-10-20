using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using HeavenStudio.Util;
using HeavenStudio.Common;
using HeavenStudio.Editor;
using Jukebox;
using Jukebox.Legacy;
using System;

namespace HeavenStudio
{
    public class StaticCamera : MonoBehaviour
    {
        [SerializeField] RectTransform canvas;
        [SerializeField] GameObject overlayView;

        [SerializeField] Image ambientBg;
        [SerializeField] GameObject ambientBgGO;
        [SerializeField] GameObject letterboxBgGO;
        [SerializeField] Image letterboxMaskImage;

        [SerializeField] RectTransform overlayCanvas;
        [SerializeField] RectTransform letterboxMask;
        [SerializeField] RectTransform parentView;

        public static StaticCamera instance { get; private set; }
        public new Camera camera;

        public enum ViewAxis
        {
            All,
            X,
            Y,
        }

        const float AspectRatioWidth = 1;
        const float AspectRatioHeight = 1;

        private List<RiqEntity> panEvents = new();
        private List<RiqEntity> scaleEvents = new();
        private List<RiqEntity> rotationEvents = new();
        private List<RiqEntity> colorEvents = new();

        static Vector3 defaultPan = new Vector3(0, 0, 0);
        static Vector3 defaultScale = new Vector3(1, 1, 1);
        static float defaultRotation = 0;
        static Color defaultColor = Color.black;

        private static Vector3 pan;
        private static Vector3 scale;
        private static float rotation;
        public static Color color;

        private static Vector3 panLast;
        private static Vector3 scaleLast;
        private static float rotationLast;

        private void Awake()
        {
            instance = this;
            camera = this.GetComponent<Camera>();
        }

        // Start is called before the first frame update
        void Start()
        {
            GameManager.instance.onBeatChanged += OnBeatChanged;

            Reset();

            panLast = defaultPan;
            scaleLast = defaultScale;
            rotationLast = defaultRotation;

            ToggleLetterboxBg(PersistentDataManager.gameSettings.letterboxBgEnable);
            ToggleLetterboxGlow(PersistentDataManager.gameSettings.letterboxFxEnable);
        }

        public void OnBeatChanged(double beat)
        { 
            Reset();

            panEvents.Clear();
            scaleEvents.Clear();
            rotationEvents.Clear();
            foreach (var entity in GameManager.instance.Beatmap.Entities)
            {
                switch (entity.datamodel)
                {
                    case "vfx/pan view":
                        panEvents.Add(entity);
                        break;
                    case "vfx/scale view":
                        scaleEvents.Add(entity);
                        break;
                    case "vfx/rotate view":
                        rotationEvents.Add(entity);
                        break;
                    case "vfx/view background color":
                        colorEvents.Add(entity);
                        break;
                    default: break;
                }
            }

            panLast = defaultPan;
            scaleLast = defaultScale;
            rotationLast = defaultRotation;

            UpdatePan();
            UpdateRotation();
            UpdateScale();
            UpdateColor();

            canvas.localPosition = pan;
            canvas.eulerAngles = new Vector3(0, 0, rotation);
            canvas.localScale = scale;
            letterboxMaskImage.color = color;

        }

        // Update is called once per frame
        void Update()
        {
            UpdatePan();
            UpdateRotation();
            UpdateScale();
            UpdateColor();

            canvas.localPosition = pan;
            canvas.eulerAngles = new Vector3(0, 0, rotation);
            canvas.localScale = scale;
            letterboxMaskImage.color = color;
        }

        private void UpdatePan()
        {
            foreach (var e in panEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f)
                {
                    Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease) e["ease"]);
                    switch (e["axis"])
                    {
                        case (int) ViewAxis.X:
                            pan.x = func(panLast.x, e["valA"], Mathf.Min(prog, 1f));
                            break;
                        case (int) ViewAxis.Y:
                            pan.y = func(panLast.y, e["valB"], Mathf.Min(prog, 1f));
                            break;
                        default:
                            float dx = func(panLast.x, e["valA"], Mathf.Min(prog, 1f));
                            float dy = func(panLast.y, e["valB"], Mathf.Min(prog, 1f));
                            pan = new Vector3(dx, dy, 0);
                            break;
                    }
                }
                if (prog > 1f)
                {
                    switch (e["axis"])
                    {
                        case (int) ViewAxis.X:
                            panLast.x = e["valA"];
                            break;
                        case (int) ViewAxis.Y:
                            panLast.y = e["valB"];
                            break;
                        default:
                            panLast = new Vector3(e["valA"], e["valB"], 0);
                            break;
                    }
                }
            }
        }

        private void UpdateRotation()
        {
            foreach (var e in rotationEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f)
                {
                    Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease) e["ease"]);
                    rotation = func(rotationLast, -e["valA"], Mathf.Min(prog, 1f));
                }
                if (prog > 1f)
                {
                    rotationLast = -e["valA"];
                }
            }
        }

        private void UpdateScale()
        {
            foreach (var e in scaleEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f)
                {
                    Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease) e["ease"]);
                    switch (e["axis"])
                    {
                        case (int) ViewAxis.X:
                            scale.x = func(scaleLast.x, e["valA"], Mathf.Min(prog, 1f)) * AspectRatioWidth;
                            break;
                        case (int) ViewAxis.Y:
                            scale.y = func(scaleLast.y, e["valB"], Mathf.Min(prog, 1f)) * AspectRatioHeight;
                            break;
                        default:
                            float dx = func(scaleLast.x, e["valA"], Mathf.Min(prog, 1f)) * AspectRatioWidth;
                            float dy = func(scaleLast.y, e["valB"], Mathf.Min(prog, 1f)) * AspectRatioHeight;
                            scale = new Vector3(dx, dy, 1);
                            break;
                    }
                }
                if (prog > 1f)
                {
                    switch (e["axis"])
                    {
                        case (int) ViewAxis.X:
                            scaleLast.x = e["valA"] * AspectRatioWidth;
                            break;
                        case (int) ViewAxis.Y:
                            scaleLast.y = e["valB"] * AspectRatioHeight;
                            break;
                        default:
                            scaleLast = new Vector3(e["valA"] * AspectRatioWidth, e["valB"] * AspectRatioHeight, 1);
                            break;
                    }
                }
            }
        }

        private void UpdateColor()
        {
            foreach (var e in colorEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f)
                {
                    Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);
                    float newColorR = func(e["color"].r, e["color2"].r, prog);
                    float newColorG = func(e["color"].g, e["color2"].g, prog);
                    float newColorB = func(e["color"].b, e["color2"].b, prog);
                    float newColorA = func(e["color"].a, e["color2"].a, prog);

                    color = new Color(newColorR, newColorG, newColorB, newColorA);
                }
                if (prog > 1f)
                {
                    color = e["color2"];
                }
            }
        }


        public static void Reset()
        {
            pan = defaultPan;
            scale = defaultScale;
            rotation = defaultRotation;
            color = defaultColor;
        }

        public void ToggleOverlayView(bool toggle)
        {
            overlayView.SetActive(toggle);
        }

        [NonSerialized] public bool usingMinigameAmbientColor;

        public void SetAmbientGlowColour(Color colour, bool minigameColor, bool overrideMinigameColor = true)
        {
            if (overrideMinigameColor) usingMinigameAmbientColor = minigameColor;
            ambientBg.color = colour;
            GameSettings.UpdatePreviewAmbient(colour);
        }

        public void ToggleLetterboxBg(bool toggle)
        {
            letterboxBgGO.SetActive(toggle);
        }

        public void ToggleLetterboxGlow(bool toggle)
        {
            ambientBgGO.SetActive(toggle);
        }

        public void ToggleCanvasVisibility(bool toggle)
        {
            canvas.gameObject.SetActive(toggle);
        }
    }
}