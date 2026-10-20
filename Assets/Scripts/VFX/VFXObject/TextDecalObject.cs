using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using HeavenStudio.Util;
using HeavenStudio.Common;
using Jukebox;

namespace HeavenStudio.VFX
{
    public class TextDecalObject : VFXObject
    {
        [Header("Objects")]
        [SerializeField] TMP_Text textContent;
        [SerializeField] MeshRenderer textMesh;
        //    [SerializeField] StickyCanvas StickyLayer;

        public Transform trans;
        public List<TMP_FontAsset> fontAssets;
        private int lastFont;

        private void Awake()
        {
            trans = transform;
        }

        public void UpdateTextDecal(string newText, int newFont, Vector2 newPos, Vector2 newSize,
        float newRotation, Color newColor, Color newOutlineColor, float newOutlineWidth,
        int layer, bool active)
        {
            base.UpdateObject(null);
            textContent.text = newText;
            textContent.font = fontAssets[newFont];
            if (newFont != lastFont)
            {
                textContent.outlineColor = Color.black;
                textContent.outlineWidth = -1;
                textContent.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0);
                lastFont = newFont;
                gameObject.SetActive(false);
                
                return;
            }
            trans.localPosition = newPos;
            trans.localScale = newSize;
            trans.localRotation = Quaternion.Euler(0f, 0f, newRotation);
            textContent.color = newColor;
            textContent.outlineColor = newOutlineColor;
            textContent.outlineWidth = newOutlineWidth;
            textContent.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, newOutlineWidth);
            textMesh.sortingOrder = layer;
            lastFont = newFont;
    //        StickyLayer.Sticky = sticky;
            gameObject.SetActive(active);
        }
    }
}