using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Util
{
    public static class UiUtils
    {
        public static bool MouseInRectTransform(RectTransform rectTransform, Camera cam)
        {
            return rectTransform.gameObject.activeSelf && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, cam);
        }

        public static bool MouseInAnyRectTransform(RectTransform[] rectTransforms, Camera cam)
        {
            for (int i = 0; i < rectTransforms.Length; i++)
            {
                RectTransform rect = rectTransforms[i];
                if (MouseInRectTransform(rect, cam))
                {
                    return true;
                }
            }

            return false;
        }
    }
}