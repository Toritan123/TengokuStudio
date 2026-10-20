using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using HeavenStudio.Util;
using HeavenStudio.Common;
using Jukebox;

namespace HeavenStudio.VFX
{
    public class DecalObject : VFXObject
    {
        [Header("Objects")]
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] GameObject GO;
        [SerializeField] StickyCanvas StickyLayer;
        public Transform trans;

        private void Awake()
        {
        }

        public void UpdateDecal(Sprite sprite, FilterMode filter, Vector3 newPos, Vector2 newSize, float newRotation, Color newColor,
        int layer, int displayLayer, bool sticky, bool active)
        {
            base.UpdateObject(null);
            spriteRenderer.sprite = sprite;
            spriteRenderer.sprite.texture.filterMode = filter;
            trans.localPosition = newPos;
            trans.localScale = newSize;
            trans.localRotation = Quaternion.Euler(0f, 0f, newRotation);
            spriteRenderer.color = newColor;
            spriteRenderer.sortingOrder = layer;
            GO.layer = displayLayer;
            StickyLayer.Sticky = sticky;
            gameObject.SetActive(active);
        }
    }
}