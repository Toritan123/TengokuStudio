using System.Collections;
using System.Collections.Generic;
using HeavenStudio.Editor.Track;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HeavenStudio.Editor
{
    public class DisableSelectOnHover : MonoBehaviour
    {
        public EventParameterManager eventParameterManager;

        private void Update()
        {
            if (Editor.instance == null) return;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (Editor.MouseInRectTransform(transform.GetChild(i).GetComponent<RectTransform>()))
                {
                    if (Timeline.instance != null)
                        Timeline.instance.canMovePlayback = false;
                }
            }
        }

        private void LateUpdate()
        {
            if (Editor.instance == null) return;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (Editor.MouseInRectTransform(transform.GetChild(i).GetComponent<RectTransform>()))
                {
                    if (eventParameterManager != null)
                        eventParameterManager.canDisable = false;
                }
            }
        }
    }
}