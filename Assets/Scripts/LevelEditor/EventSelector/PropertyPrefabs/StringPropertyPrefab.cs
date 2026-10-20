using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;
using HeavenStudio.Util;
using HeavenStudio.Editor;
using HeavenStudio.Editor.Commands;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;

namespace HeavenStudio.Editor
{
    public class StringPropertyPrefab : EventPropertyPrefab
    {
        [Header("String")]  //why wasn't this a thing before
        [Space(10)]
        public TMP_InputField inputFieldString;
        public DisableSelectOnHover disableSelectOnHover;
        public TMP_Dropdown fontDropdown;

        [Header("Font Size")]
        [Space(10)]
        public TMP_InputField fontSizeInput;
        public RectTransform fontSizeMenu;
        public Button sizeButton;
        public GameObject notifMessage;
        public bool isFontSizeMenuActive = false;

        [Header("Color")]
        [Space(10)]
        public TMP_InputField hex;
        public RectTransform ColorTable;
        public Button colorButton;
        public bool colorTableActive = false;
        public ColorPreview colorPreview;
        public GameObject notifMessage2;
        
        private float currFontSize = 10.0f;
        private float realFontSize = 10.0f * 10; //x10 multiplyer to feel traditional to Word fonts
        private string _defaultValue;
        private InputFieldManager inputFieldManager;
        private static readonly Regex regexFloatPattern = new Regex(@"^(\d{1,3}(\.\d{0,2})?)?$");
        private int storedSelectionStart;
        private int storedSelectionEnd;
        private Color _defaultColor;

        public override void SetProperties(string propertyName, object type, string caption)
        {
            base.SetProperties(propertyName, type, caption);

            _defaultValue = (string)type;
            _defaultColor = Color.black;
            inputFieldString.text = (string)entity[propertyName];
            inputFieldManager = new InputFieldManager(inputFieldString, this);
            fontSizeInput.text = FormatFontSize(currFontSize);

            fontSizeInput.onValidateInput += ValidateFontSizeInput; //add method to event
            fontSizeInput.characterLimit = 6;

            disableSelectOnHover.eventParameterManager = EventParameterManager.instance;

            inputFieldString.onSelect.AddListener(_ =>
                Editor.instance.editingInputField = true
            );

            inputFieldString.onValueChanged.AddListener(_ =>
                {
                    entity[propertyName] = inputFieldString.text;
                    this.caption.text = (inputFieldString.text != _defaultValue) ? (_captionText + "*") : _captionText;
                }
            );

            inputFieldString.onEndEdit.AddListener(_ =>
                {
                    Editor.instance.editingInputField = false;
                }
            );

            fontSizeInput.onSelect.AddListener(_ =>
                {
                    storedSelectionStart = inputFieldString.selectionAnchorPosition;
                    storedSelectionEnd = inputFieldString.selectionFocusPosition;
                    Editor.instance.editingInputField = true;
                    notifMessage.SetActive(true);
                }
            );
            
            fontSizeInput.onEndEdit.AddListener(_ => 
            {
                Editor.instance.editingInputField = false;
                notifMessage.SetActive(false);
                if (float.TryParse(fontSizeInput.text, out float newSize))
                {
                    currFontSize = Mathf.Clamp(newSize, 1, 999f);
                    fontSizeInput.text = FormatFontSize(currFontSize);
                }
                StartCoroutine(RestoreTextSelection());
            });

            hex.onSelect.AddListener(_ =>
                {
                    storedSelectionStart = inputFieldString.selectionAnchorPosition;
                    storedSelectionEnd = inputFieldString.selectionFocusPosition;
                    Editor.instance.editingInputField = true;
                    notifMessage2.SetActive(true);
                }
            );

            hex.onEndEdit.AddListener(_ =>
                {
                    Editor.instance.editingInputField = false;
                    notifMessage.SetActive(false);
                    StartCoroutine(RestoreTextSelection());
                }
            );

            colorPreview.onColorUpdated += newColor =>
            {
                _defaultColor = newColor;
            };

            colorPreview.ChangeColor(_defaultColor);
            ColorTable.gameObject.SetActive(false);
            fontSizeMenu.gameObject.SetActive(false);
        }

        public void ResetValue()
        {
            inputFieldString.text = _defaultValue;
            colorPreview.ChangeColor(_defaultColor);
        }

        public override void SetCollapses(object type)
        {
            inputFieldString.onValueChanged.AddListener(newVal => UpdateCollapse(newVal));
            UpdateCollapse(inputFieldString.text);
        }

        private void LateUpdate()
        {
            //makin sure we only compare strings
            var entityValue = entity[propertyName];
            if (entityValue is string stringValue && stringValue != inputFieldString.text) 
            {
                inputFieldString.text = stringValue;
            }
        }

        public void OnClickRichText()
        {
            inputFieldManager.ToggleRichText();
        }

        public void OnClickBold()
        {
            ExecuteWithoutFocusLoss(() => 
            {
                inputFieldManager.DetermineTextOperation("b", "<b>", "</b>");
            });
        }

        public void OnClickItalics()
        {
            ExecuteWithoutFocusLoss(() => 
            {
                inputFieldManager.DetermineTextOperation("i", "<i>", "</i>");
            });
        }

        public void OnClickUnderline()
        {
            ExecuteWithoutFocusLoss(() => 
            {
                inputFieldManager.DetermineTextOperation("u", "<u>", "</u>");
            });
        }

        public void OnClickSize()
        {
            ExecuteWithoutFocusLoss(() => 
            {   
                if (isFontSizeMenuActive) { CloseFontSizeMenu(); }
                else { OpenFontSizeMenu(); }
            });
        }

        public void SubmitFontSize()
        {
            ExecuteWithoutFocusLoss(() => 
            {
                if (float.TryParse(fontSizeInput.text, out float finalSize))
                {
                    currFontSize = Mathf.Clamp(finalSize, 1f, 999f);
                    realFontSize = currFontSize * 10;
                }
            });
            inputFieldManager.DetermineTextOperation("size", $"<size={realFontSize}%>", "</size>");
        }

        public void OnClickFontIncrement()
        {
            ExecuteWithoutFocusLoss(() => 
            {
                currFontSize = (Mathf.Clamp(currFontSize + 1f, 1f, 999f));
                fontSizeInput.text = FormatFontSize(currFontSize);
                realFontSize = currFontSize * 10;
            });
            inputFieldManager.DetermineTextOperation("size", $"<size={realFontSize}%>", "</size>");
        }

        public void OnClickFontDecrement()
        {
            ExecuteWithoutFocusLoss(() => 
            {
                currFontSize = Mathf.Clamp(currFontSize - 1f, 1f, 999f);
                fontSizeInput.text = FormatFontSize(currFontSize);
                realFontSize = currFontSize * 10;
            });
            inputFieldManager.DetermineTextOperation("size", $"<size={realFontSize}%>", "</size>");
        }

        public void OnClickColor()
        {
            //dont trigger click on child elements (ColorTable is child of ColorButton)
            GameObject clickedObject = EventSystem.current.currentSelectedGameObject;
            if (clickedObject != colorButton.gameObject)
            {
                return;
            }

            ExecuteWithoutFocusLoss(() => 
            {   
                if (colorTableActive) { CloseColorMenu(); }
                else { OpenColorMenu(); }
            });
        }

        public void SubmitColor()
        {
            string hexColor = "";
            ExecuteWithoutFocusLoss(() => 
            {
                Color currentColor = colorPreview.currentColor;
                hexColor = ColorUtility.ToHtmlStringRGBA(currentColor);
            });
            inputFieldManager.DetermineTextOperation("color", $"<color=#{hexColor}>", "</color>");
        }

        public void OnClickFont()
        {
            ExecuteWithoutFocusLoss(() => 
            {
                string fontDropValue = fontDropdown.options[fontDropdown.value].text;
                string[] fontSplit = fontDropValue.Split('"');
                inputFieldManager.DetermineTextOperation("font", $"<font=\"{fontSplit[1]}\">", "</font>");
            });
        }

        public void OnClickOutlineFont()
        {
            ExecuteWithoutFocusLoss(() => 
            {
                string fontDropValue = fontDropdown.options[fontDropdown.value].text;
                string[] fontSplit = fontDropValue.Split('"');
                inputFieldManager.DetermineTextOperation("outlineFont", $"<font=\"{fontSplit[1]}\" material=\"{fontSplit[1]} Outline\">", "</font>");
            });
        }

        private void OpenFontSizeMenu()
        {
            fontSizeMenu.gameObject.SetActive(true);
            isFontSizeMenuActive = true;
            fontSizeInput.text = FormatFontSize(currFontSize);
        }

        private void CloseFontSizeMenu()
        {
            fontSizeMenu.gameObject.SetActive(false);
            notifMessage.SetActive(false);
            isFontSizeMenuActive = false;
            Editor.instance.editingInputField = false;
        }

        private void OpenColorMenu()
        {
            ColorTable.gameObject.SetActive(true);
            colorTableActive = true;
            colorPreview.ChangeColor(_defaultColor);
        }

        private void CloseColorMenu()
        {
            ColorTable.gameObject.SetActive(false);
            notifMessage2.SetActive(false);
            colorTableActive = false;
            Editor.instance.editingInputField = false;
        }

        private string FormatFontSize(float fontSize)
        {
            if (fontSize == Mathf.Floor(fontSize)) return fontSize.ToString("0");
            else return fontSize.ToString("0.##");
        }

        private char ValidateFontSizeInput(string text, int charIndex, char addedChar)
        {
            string checkText = text.Insert(charIndex, addedChar.ToString());

            if (regexFloatPattern.IsMatch(checkText)) return addedChar; //3 digits if whole num or "." with 2 more num
            
            return '\0';
        }

        private void ExecuteWithoutFocusLoss(System.Action action)
        {
            storedSelectionStart = inputFieldString.selectionAnchorPosition;
            storedSelectionEnd = inputFieldString.selectionFocusPosition;
            
            action?.Invoke();
            
            StartCoroutine(RestoreTextSelection());
        }

        private IEnumerator RestoreTextSelection()
        {
            yield return null; //wait one frame
            
            inputFieldString.Select();
            inputFieldString.selectionAnchorPosition = storedSelectionStart;
            inputFieldString.selectionFocusPosition = storedSelectionEnd;

            Editor.instance.editingInputField = true;
        }

        private bool IsMouseOverDropdown()
        {
            //over main dropdown
            if (Editor.MouseInRectTransform(fontDropdown.GetComponent<RectTransform>()))
            {
                return true;
            }

            if (fontDropdown.IsExpanded)
            {
                //over any dropdown items
                Transform dropdownList = fontDropdown.transform.Find("Dropdown List");
                if (dropdownList != null && Editor.MouseInRectTransform(dropdownList.GetComponent<RectTransform>()))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsShortCutInitiatePressed()
        {
            bool isWindLinux = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
            bool isMac = (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand));
            return (isWindLinux || isMac);
        }

        private void Update()
        {
            if (isFontSizeMenuActive && Input.GetMouseButtonDown(0))
            {
                if (!Editor.MouseInRectTransform(fontSizeMenu) && !Editor.MouseInRectTransform(sizeButton.GetComponent<RectTransform>()))
                {
                    CloseFontSizeMenu();
                }
            }

            if (colorTableActive && Input.GetMouseButtonDown(0))
            {
                if (!Editor.MouseInRectTransform(ColorTable) && !Editor.MouseInRectTransform(colorButton.GetComponent<RectTransform>()))
                {
                    CloseColorMenu();
                }
                else if (Editor.MouseInRectTransform(ColorTable))
                {
                    if (!Editor.MouseInRectTransform(hex.GetComponent<RectTransform>()))
                    {
                        notifMessage2.SetActive(false);
                    }
                    StartCoroutine(RestoreTextSelection());
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (IsMouseOverDropdown())
                {
                    storedSelectionStart = inputFieldString.selectionAnchorPosition;
                    storedSelectionEnd = inputFieldString.selectionFocusPosition;
                    StartCoroutine(RestoreTextSelection());
                }
            }

            //ctrl+i is part of TMP_InputField rich text and im not sure how to override it
            if (inputFieldString.isFocused && Editor.instance.editingInputField)
            {
                if (Input.GetKeyDown(KeyCode.B) && IsShortCutInitiatePressed()) 
                {
                    OnClickBold();
                    StartCoroutine(RestoreTextSelection());
                }
                if (Input.GetKeyDown(KeyCode.I) && IsShortCutInitiatePressed()) 
                { 
                    OnClickItalics();
                    StartCoroutine(RestoreTextSelection());
                }
                if (Input.GetKeyDown(KeyCode.U) && IsShortCutInitiatePressed()) 
                { 
                    OnClickUnderline();
                    StartCoroutine(RestoreTextSelection());
                }
            }
        }
    }
}