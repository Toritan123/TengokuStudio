using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using HeavenStudio.Common;
using Jukebox;

namespace HeavenStudio.Editor
{
    public class AssetPropertyPrefab : EventPropertyPrefab
    {
        [Header("Dropdown")]
        [Space(10)]
        public LeftClickTMP_Dropdown dropdown;
        public Scrollbar scrollbar;
        public DisableSelectOnHover disableSelectOnHover;

        private bool openedDropdown = false;

        public override void SetProperties(string propertyName, object parameter, string caption)
        {
            base.SetProperties(propertyName, parameter, caption);

            disableSelectOnHover.eventParameterManager = EventParameterManager.instance;

            if (parameter is not EntityTypes.Asset asset) throw new Exception();

            IEnumerable<string> dropdownValues = asset.type switch
            {
                EntityTypes.Asset.AssetType.Sprite => GameManager.instance.ImportedSprites.Keys,
                EntityTypes.Asset.AssetType.Sfx => GameManager.instance.ImportedSfx.Keys,
                EntityTypes.Asset.AssetType.AssetBundle => GameManager.instance.ImportedAssBuns.Keys,
                _ => throw new NotImplementedException(),
            };
            dropdown.options = dropdownValues.Select(n => new TMP_Dropdown.OptionData(n)).ToList();
            if (string.IsNullOrEmpty(entity[propertyName]) && dropdown.options.Count > 0) {
                entity[propertyName] = dropdown.options[0].text;
            } else {
                dropdown.value = dropdown.options.FindIndex(x => x.text == entity[propertyName]);
            }

            dropdown.enabled = dropdown.options.Count > 0;

            dropdown.onValueChanged.AddListener(newValue =>
            {
                entity[propertyName] = dropdown.options[newValue].text;
                // Debug.Log("dropdown.options[newValue].text : " + dropdown.options[newValue].text);
            });
        }

        public void ResetValue()
        {
            dropdown.value = 0;
        }

        public override void SetCollapses(object type)
        {
            dropdown.onValueChanged.AddListener(_ => UpdateCollapse(dropdown.value));
            UpdateCollapse(dropdown.value);
        }

        private void Update()
        {
            if (scrollbar != null)
            {
                if (!openedDropdown)
                {
                    openedDropdown = true;

                    var valuePos = (float)dropdown.value / (dropdown.options.Count - 1);
                    var scrollVal = scrollbar.direction == Scrollbar.Direction.TopToBottom ? valuePos : 1.0f - valuePos;
                    scrollbar.value = Mathf.Max(0.001f, scrollVal);
                }
            }
            else
            {
                openedDropdown = false;
            }
        }
    }
}