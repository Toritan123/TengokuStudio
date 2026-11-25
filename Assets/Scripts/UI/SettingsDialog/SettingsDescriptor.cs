using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

namespace HeavenStudio.Common 
{
    public class SettingsDescriptor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private string description;
        [SerializeField] private bool recursive = true;
        private SettingsDialog settings;

        private void Start()
        {
            settings = SettingsDialog.instance;
            if (recursive)
            {
                foreach (Transform child in GetComponentsInChildren<Transform>(true))
                {
                    if (child.gameObject == gameObject)
                        continue;

                    var descriptor = child.GetComponent<SettingsDescriptor>();

                    if (descriptor == null)
                        descriptor = child.gameObject.AddComponent<SettingsDescriptor>();

                    descriptor.description = description;
                }
            } 
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowDescription();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideDescription();
        }

        public void ShowDescription()
        {
            if (settings.DescriptionDisplay != null)
                settings.DescriptionDisplay.text = description;
        }

        public void HideDescription()
        {
            if (settings.DescriptionDisplay != null)
                settings.DescriptionDisplay.text = settings.defaultDescription;
        }
    }
}