using HeavenStudio.Games;
using HeavenStudio.Util;
using UnityEngine;
using UnityEngine.UI;

namespace HeavenStudio.Editor.Track
{
    public class BlockDeleteFX : MonoBehaviour
    {
        private RectTransform rectTransform;

        [SerializeField]
        private Image mainImage;

        private bool started = false;
        private float deleteTime = 0.0f;


        private Color color;
        private double eBeat;
        private float eLength;
        private int eLayer;
        private bool wasESelected = false;

        public void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void Create(double beat, float length, int layer, bool selected)
        {
            started = true;
            deleteTime = Time.time;

            eBeat = beat;
            eLength = length;
            eLayer = layer;
            wasESelected = selected;

            rectTransform.anchoredPosition = new Vector2((float)eBeat * Timeline.instance.PixelsPerBeat, -eLayer * Timeline.instance.LayerHeight());
            rectTransform.sizeDelta = new Vector2(eLength * Timeline.instance.PixelsPerBeat, Timeline.instance.LayerHeight());
            color = EditorTheme.theme.LayerGradientIndex(eLayer);

            Destroy(this.gameObject, 0.714f);

            Update();
        }

        public void Create(double beat, Color color)
        {
            started = true;
            deleteTime = Time.time;

            eBeat = beat;

            rectTransform.anchoredPosition = new Vector2((float)eBeat * Timeline.instance.PixelsPerBeat, 0);
            rectTransform.sizeDelta = new Vector2(0.25f * Timeline.instance.PixelsPerBeat, Timeline.instance.LayerCount * Timeline.instance.LayerHeight());
            this.color = color;

            Destroy(this.gameObject, 0.714f);

            Update();
        }

        private void Update()
        {
            if (!started) return;

            // I added this because I was going to use the effect when you undo a place as well, but I didn't like it.
            // var color = (wasESelected) ? Color.cyan : EditorTheme.theme.LayerGradientIndex(eLayer);

            var norm = (Time.time - deleteTime) * 1.4f;
            mainImage.color = Color.Lerp(color, new Color(color.r, color.g, color.b, 0.0f), EasingFunction.EaseOutCirc(0, 1, norm));

            var extrudeAnim = EasingFunction.EaseOutCirc(0.0f, 8.0f, norm);
            mainImage.rectTransform.sizeDelta = new Vector2(extrudeAnim, extrudeAnim);
        }
    }
}