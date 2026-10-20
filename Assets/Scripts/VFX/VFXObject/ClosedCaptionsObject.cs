using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;

using Jukebox;

namespace HeavenStudio.VFX
{
    public class ClosedCaptionsObject : VFXObject
    {
        [Header("Objects")]
        public TMP_Text Label;
        public RectTransform LabelRect;
        public RectTransform BGRect;

        public override void UpdateObject(RiqEntity e, float prog)
        {
            base.UpdateObject(e);

            Label.text = e["text1"];

            LabelRect.sizeDelta = new Vector2(18, 2 * e["valA"]);
            BGRect.sizeDelta = new Vector2(18, 2 * e["valA"]);

            transform.localPosition = e["type"] switch
            {
                (int)ClosedCaptionsAnchor.Bottom => new Vector3(0, -5f + e["valA"]),
                _ => new Vector3(0, 5f - e["valA"]),
            };
        }
    }
}