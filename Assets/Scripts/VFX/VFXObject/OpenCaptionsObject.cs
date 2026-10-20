using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Jukebox;

namespace HeavenStudio.VFX
{
    public class OpenCaptionsObject : VFXObject
    {
        [Header("Objects")]
        public TMP_Text Label;
        public RectTransform LabelRect;

        public override void UpdateObject(RiqEntity e, float prog)
        {
            base.UpdateObject(e);

            Label.text = e["text1"];

            LabelRect.sizeDelta = new Vector2(18f * e["valA"], 2.5f * e["valB"]);

            if ((TextboxAnchor)e["type"] == TextboxAnchor.Custom)
            {
                var easingFunc = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);
                var newPos = new Vector2(
                    easingFunc(e["xStart"], e["x"], prog),
                    easingFunc(e["yStart"], e["y"], prog)
                );
                transform.localPosition = newPos;
            }
            else transform.localPosition = GetAnchorPosFromEntity(e);
        }
    }
}