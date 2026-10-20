using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using HeavenStudio.Util;
using HeavenStudio.VFX;

using System.Linq;
using Jukebox;
using Jukebox.Legacy;

namespace HeavenStudio.Games.Global
{
    public class Flash : MonoBehaviour
    {
        [NonSerialized] public double startBeat;
        [NonSerialized] public float length;

        [NonSerialized] public Color startColor;
        [NonSerialized] public Color endColor;

        [NonSerialized] public Util.EasingFunction.Ease ease;
        [NonSerialized] private Util.EasingFunction.Function func;

        [NonSerialized] private Image spriteRenderer;

        [SerializeField] private Color currentCol;

        [NonSerialized] private List<RiqEntity> allFadeEvents = new List<RiqEntity>();

        private void Awake()
        {
            spriteRenderer = GetComponent<Image>();
            spriteRenderer.color = currentCol;
            func = Util.EasingFunction.GetEasingFunction(Util.EasingFunction.Ease.Linear);
            GameManager.instance.onBeatChanged += OnBeatChanged;
        }

        public void OnBeatChanged(double beat)
        {
            allFadeEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "flash" });

            // fixes a bug where deleted flashes on beat 0 will persist until placing a new one
            SetFade(0, 0, new Color(), new Color(), Util.EasingFunction.Ease.Instant);
            FindFadeFromBeat(beat);
        }

        private void FindFadeFromBeat(double beat)
        {
            Color startCol = Color.white;
            Color endCol = Color.white;

            bool override_ = false;

            if (allFadeEvents.Count > 0)
            {
                RiqEntity startEntity = default;

                for (int i = 0; i < allFadeEvents.Count; i++)
                {
                    if (allFadeEvents[i].beat <= beat)
                    {
                        startEntity = allFadeEvents[i];
                    }
                    else if (i == 0 && allFadeEvents[i].beat > beat)
                    {
                        startEntity = allFadeEvents[i];
                        override_ = true;
                        startCol = new Color(1, 1, 1, 0);
                        endCol = new Color(1, 1, 1, 0);
                    }
                }

                if (!string.IsNullOrEmpty(startEntity.datamodel))
                {
                    if (!override_)
                    {
                        Color colA = startEntity["colorA"];
                        Color colB = startEntity["colorB"];

                        startCol = new Color(colA.r, colA.g, colA.b, colA.a);
                        endCol = new Color(colB.r, colB.g, colB.b, colB.a);
                    }

                    SetFade(startEntity.beat, startEntity.length, startCol, endCol, (Util.EasingFunction.Ease) startEntity["ease"]);
                }
            }
        }

        public void SetFade(double beat, float length, Color startCol, Color endCol, Util.EasingFunction.Ease ease)
        {
            this.startBeat = beat;
            this.length = length;
            this.startColor = startCol;
            this.endColor = endCol;
            this.ease = ease;
            func = Util.EasingFunction.GetEasingFunction(ease);
        }

        private void Update()
        {
            FindFadeFromBeat(Math.Max(Conductor.instance.songPositionInBeatsAsDouble, 0));
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat, length);
            if (normalizedBeat > 1) normalizedBeat = 1;

            currentCol = VFXManager.VfxColorEase(startColor, endColor, normalizedBeat, func);
            spriteRenderer.color = currentCol;
        }
    }
}