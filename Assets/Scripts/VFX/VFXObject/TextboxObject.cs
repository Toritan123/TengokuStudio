using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Jukebox;

namespace HeavenStudio.VFX
{
    public class TextboxObject : VFXObject
    {
        [Header("Objects")]
        public TMP_Text TextboxLabel;
        public RectTransform TextboxLabelRect;
        public SpriteRenderer UL;
        public SpriteRenderer UR;
        public SpriteRenderer DL;
        public SpriteRenderer DR;

        private static Vector2 textboxSize = new Vector2(3f, 0.75f);

        public override void UpdateObject(RiqEntity e, float prog)
        {
            base.UpdateObject(e);

            TextboxLabel.text = e["text1"];

            Vector2 tScale = Vector2.Scale(textboxSize, new Vector2(e["valA"], e["valB"]));
            UL.size = tScale;
            UR.size = tScale;
            DL.size = tScale;
            DR.size = tScale;
            TextboxLabelRect.sizeDelta = new Vector2(11.2f * e["valA"], 2.2f * e["valB"]);

            transform.localPosition = GetAnchorPosFromEntity(e);
        }
    }
}