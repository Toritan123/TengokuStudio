using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

using HeavenStudio.Common;

using System.IO;
namespace HeavenStudio.Editor
{
    public class GraphicsSettings : TabsContent
    {
        public TMP_Dropdown resolutionsDropdown;
        public GameObject customSetter;
        public GameObject maxFps;
        public GameObject vSyncGO;
        public TMP_InputField widthInputField, heightInputField;
        [SerializeField] Toggle limitFramerateToggle;
        [SerializeField] Toggle vSyncToggle;


        public Slider fpsSlider;
        public TMP_InputField fpsLabel;
        public Slider resScaleSlider;
        public TMP_InputField resScaleLabel;

        private void Start()
        {
            List<TMP_Dropdown.OptionData> dropDownData = new List<TMP_Dropdown.OptionData>();
            var vals = GlobalGameManager.DEFAULT_SCREEN_SIZES_STRING;
            for (int i = 0; i < vals.Length; i++)
            {
                TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                optionData.text = vals[i];
                dropDownData.Add(optionData);
            }
            resolutionsDropdown.AddOptions(dropDownData);

            resolutionsDropdown.onValueChanged.AddListener(delegate
            {
                GlobalGameManager.ScreenSizeIndex = resolutionsDropdown.value;

                customSetter.SetActive(resolutionsDropdown.value == GlobalGameManager.DEFAULT_SCREEN_SIZES_STRING.Length - 1);
            });

            widthInputField.onEndEdit.AddListener(delegate
            {
                GlobalGameManager.CustomScreenWidth = System.Math.Max(int.Parse(widthInputField.text), 64);
                widthInputField.text = GlobalGameManager.CustomScreenWidth.ToString();
            });
            heightInputField.onEndEdit.AddListener(delegate
            {
                GlobalGameManager.CustomScreenHeight = System.Math.Max(int.Parse(heightInputField.text), 64);
                heightInputField.text = GlobalGameManager.CustomScreenHeight.ToString();
            });

            resolutionsDropdown.value = GlobalGameManager.ScreenSizeIndex;

            widthInputField.text = GlobalGameManager.CustomScreenWidth.ToString();
            heightInputField.text = GlobalGameManager.CustomScreenHeight.ToString();
        }

        public void WindowFullScreen()
        {
            GlobalGameManager.WindowFullScreen();
            GlobalGameManager.ResetGameRenderTexture();
        }

        public void WindowConfirmSize()
        {
            GlobalGameManager.ChangeScreenSize();
            GlobalGameManager.ResetGameRenderTexture();
        }

        public void OnLimitFramerateChanged()
        {
            PersistentDataManager.gameSettings.limitFramerate = limitFramerateToggle.isOn;
            GlobalGameManager.ChangeMaxFramerate((int)fpsSlider.value);
            maxFps.SetActive(limitFramerateToggle.isOn);
            vSyncGO.SetActive(limitFramerateToggle.isOn);
        }

        public void OnVSyncChanged()
        {
            PersistentDataManager.gameSettings.vSync = vSyncToggle.isOn;
            GlobalGameManager.ChangeMaxFramerate((int)fpsSlider.value);
        }

        public void OnFpsSliderChanged()
        {
            fpsLabel.text = fpsSlider.value.ToString();
            PersistentDataManager.gameSettings.maxFps = (int)fpsSlider.value;
            GlobalGameManager.ChangeMaxFramerate((int)fpsSlider.value);
        }

        public void OnFpsLabelChanged()
        {
            fpsSlider.value = int.Parse(fpsLabel.text);
            PersistentDataManager.gameSettings.maxFps = (int)fpsSlider.value;
            GlobalGameManager.ChangeMaxFramerate((int)fpsSlider.value);
            fpsLabel.text = fpsSlider.value.ToString();
        }

        public void OnResScaleSliderChanged()
        {
            int truncate = (int)(resScaleSlider.value * 1000);
            float value = truncate / 1000f;
            resScaleLabel.text = value.ToString();
            PersistentDataManager.gameSettings.resScale = resScaleSlider.value;
        }

        public void OnResScaleLabelChanged()
        {
            resScaleSlider.value = float.Parse(resScaleLabel.text);
            PersistentDataManager.gameSettings.resScale = resScaleSlider.value;
            resScaleLabel.text = resScaleSlider.value.ToString();
        }

        public override void OnOpenTab()
        {
            resolutionsDropdown.value = GlobalGameManager.ScreenSizeIndex;

            widthInputField.text = GlobalGameManager.CustomScreenWidth.ToString();
            heightInputField.text = GlobalGameManager.CustomScreenHeight.ToString();

            limitFramerateToggle.isOn = PersistentDataManager.gameSettings.limitFramerate;
            vSyncToggle.isOn = PersistentDataManager.gameSettings.vSync;

            maxFps.SetActive(limitFramerateToggle.isOn);
            vSyncGO.SetActive(limitFramerateToggle.isOn);
            fpsSlider.value = PersistentDataManager.gameSettings.maxFps;
            fpsLabel.text = fpsSlider.value.ToString();
            resScaleSlider.value = PersistentDataManager.gameSettings.resScale;
            resScaleLabel.text = resScaleSlider.value.ToString();
        }

        public override void OnCloseTab()
        {
            GlobalGameManager.ResetGameRenderTexture();
        }
    }
}