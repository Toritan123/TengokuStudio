using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;

namespace HeavenStudio.Editor
{
    public class ColorPropertyPrefab : EventPropertyPrefab
    {
        [Header("Color")]
        [Space(10)]
        public Button ColorBTN;
        public RectTransform ColorTable;
        public bool colorTableActive;
        public ColorPreview colorPreview;
        public TMP_InputField hex;
        public DisableSelectOnHover disableSelectOnHover;

        private Color _defaultColor;

        public override void SetProperties(string propertyName, object type, string caption)
        {
            base.SetProperties(propertyName, type, caption);

            disableSelectOnHover.eventParameterManager = EventParameterManager.instance;

            hex.onSelect.AddListener(
                _ =>
                    Editor.instance.editingInputField = true
            );
            hex.onEndEdit.AddListener(
                _ =>
                {;
                    Editor.instance.editingInputField = false;
                }
            );

            colorPreview.onColorUpdated += newColor =>
            {
                parameterManager.entity[propertyName] = newColor;
                if (newColor != _defaultColor)
                {
                    this.caption.text = _captionText + "*";
                }
                else
                {
                    this.caption.text = _captionText;
                }
            };

            _defaultColor = (Color)type;

            ColorBTN.onClick.AddListener(
                () => {
                    ColorTable.gameObject.SetActive(true);
                    colorTableActive = true;
                    colorPreview.ChangeColor(parameterManager.entity[propertyName]);
                }
            );

            colorPreview.ChangeColor(parameterManager.entity[propertyName]);
            ColorTable.gameObject.SetActive(false);
        }

        public void ResetValue()
        {
            colorPreview.ChangeColor(_defaultColor);
        }

        public override void SetCollapses(object type)
        {
            colorPreview.onColorUpdated += c => UpdateCollapse(c);
            UpdateCollapse(parameterManager.entity[propertyName]);
        }

        private void Update()
        {
            if (colorTableActive)
            {
                if (!Editor.MouseInRectTransform(ColorTable))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        ColorTable.gameObject.SetActive(false);
                        colorTableActive = false;
                        Editor.instance.editingInputField = false;
                    }
                }
            }
        }
    }
}