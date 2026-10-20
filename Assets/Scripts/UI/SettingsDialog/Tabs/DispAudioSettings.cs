using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

using HeavenStudio.Common;
using SFB;
using System.IO;
namespace HeavenStudio.Editor
{
    public class DispAudioSettings : TabsContent
    {
        [SerializeField] Toggle splashScreenToggle;
        [SerializeField] Toggle loadingHintsToggle;
        [SerializeField] Toggle filePickerToggle;

        [Space]
        [SerializeField] Toggle playHintsToggle;
        [SerializeField] Toggle editorHintsToggle;
        [SerializeField] Toggle sillyHintsToggle;
        [SerializeField] Toggle factHintsToggle;

        public TMP_InputField libraryPathField;


        public Slider volSlider;
        public TMP_InputField volLabel;
        public Slider musVolSlider;
        public TMP_InputField musVolLabel;
        public Slider sfxVolSlider;
        public TMP_InputField sfxVolLabel;
        public TMP_Dropdown dspSizeDropdown;
        public TMP_Dropdown sampleRateDropdown;
        public TMP_Text resetSettingsText;
        private bool confirmErase = false;

        int wantSampleRate, wantDspSize;

        private void Start()
        {
            volSlider.value = GlobalGameManager.MasterVolume;
            volLabel.text = System.Math.Round(volSlider.value * 100, 2).ToString();
            musVolSlider.value = GlobalGameManager.MusicVolume;
            musVolLabel.text = System.Math.Round(musVolSlider.value * 100, 2).ToString();
            sfxVolSlider.value = GlobalGameManager.SfxVolume;
            sfxVolLabel.text = System.Math.Round(sfxVolSlider.value * 100, 2).ToString();

            dspSizeDropdown.onValueChanged.AddListener(delegate
            {
                GlobalGameManager.currentDspSize = GlobalGameManager.DSP_BUFFER_SIZES[dspSizeDropdown.value];
                GlobalGameManager.ChangeAudioSettings(GlobalGameManager.currentDspSize, GlobalGameManager.currentSampleRate);
            });

            sampleRateDropdown.onValueChanged.AddListener(delegate
            {
                GlobalGameManager.currentSampleRate = GlobalGameManager.SAMPLE_RATES[sampleRateDropdown.value];
                GlobalGameManager.ChangeAudioSettings(GlobalGameManager.currentDspSize, GlobalGameManager.currentSampleRate);
            });
        }

        public void OnVolSliderChanged()
        {
            GlobalGameManager.ChangeMasterVolume(volSlider.value);
            volLabel.text = System.Math.Round(volSlider.value * 100, 2).ToString();
            PersistentDataManager.gameSettings.masterVolume = volSlider.value;
        }

        public void OnVolLabelChanged()
        {
            volSlider.value = (float)System.Math.Round(System.Convert.ToSingle(volLabel.text) / 100f, 2);
            GlobalGameManager.ChangeMasterVolume(volSlider.value);
            PersistentDataManager.gameSettings.masterVolume = volSlider.value;
        }

        public void OnMixerVolSliderChanged()
        {
            GlobalGameManager.ChangeMixerVolumes(musVolSlider.value, sfxVolSlider.value);
            musVolLabel.text = System.Math.Round(musVolSlider.value * 100, 2).ToString();
            sfxVolLabel.text = System.Math.Round(sfxVolSlider.value * 100, 2).ToString();
            PersistentDataManager.gameSettings.musicVolume = musVolSlider.value;
            PersistentDataManager.gameSettings.sfxVolume = sfxVolSlider.value;
        }

        public void OnMixerVolLabelChanged()
        {
            musVolSlider.value = (float)System.Math.Round(System.Convert.ToSingle(musVolLabel.text) / 100f, 2);
            sfxVolSlider.value = (float)System.Math.Round(System.Convert.ToSingle(sfxVolLabel.text) / 100f, 2);
            GlobalGameManager.ChangeMixerVolumes(musVolSlider.value, sfxVolSlider.value);
            PersistentDataManager.gameSettings.musicVolume = musVolSlider.value;
            PersistentDataManager.gameSettings.sfxVolume = sfxVolSlider.value;
        }

        public void OnSplashChanged()
        {
            PersistentDataManager.gameSettings.showSplash = splashScreenToggle.isOn;
        }

        public void OnLoadingHintsChanged()
        {
            PersistentDataManager.gameSettings.showLoadingTips = loadingHintsToggle.isOn;
        }

        public void OnFilePickerChanged()
        {
            PersistentDataManager.gameSettings.forceFallbackFileBrowser = filePickerToggle.isOn;
        }

        public void OnPlayHintsChanged()
        {
            if (playHintsToggle.isOn)
            {
                if (!PersistentDataManager.gameSettings.enabledTips.Contains(HintGiver.HintTypes.Play))
                {
                    PersistentDataManager.gameSettings.enabledTips.Add(HintGiver.HintTypes.Play);
                }
            }
            else
            {
                if (PersistentDataManager.gameSettings.enabledTips.Contains(HintGiver.HintTypes.Play))
                {
                    PersistentDataManager.gameSettings.enabledTips.Remove(HintGiver.HintTypes.Play);
                }
            }
        }

        public void OnEditorHintsChanged()
        {
            if (editorHintsToggle.isOn)
            {
                if (!PersistentDataManager.gameSettings.enabledTips.Contains(HintGiver.HintTypes.Editor))
                {
                    PersistentDataManager.gameSettings.enabledTips.Add(HintGiver.HintTypes.Editor);
                }
            }
            else
            {
                if (PersistentDataManager.gameSettings.enabledTips.Contains(HintGiver.HintTypes.Editor))
                {
                    PersistentDataManager.gameSettings.enabledTips.Remove(HintGiver.HintTypes.Editor);
                }
            }
        }

        public void OnFactHintsChanged()
        {
            if (factHintsToggle.isOn)
            {
                if (!PersistentDataManager.gameSettings.enabledTips.Contains(HintGiver.HintTypes.Fact))
                {
                    PersistentDataManager.gameSettings.enabledTips.Add(HintGiver.HintTypes.Fact);
                }
            }
            else
            {
                if (PersistentDataManager.gameSettings.enabledTips.Contains(HintGiver.HintTypes.Fact))
                {
                    PersistentDataManager.gameSettings.enabledTips.Remove(HintGiver.HintTypes.Fact);
                }
            }
        }

        public void OnSillyHintsChanged()
        {
            if (sillyHintsToggle.isOn)
            {
                if (!PersistentDataManager.gameSettings.enabledTips.Contains(HintGiver.HintTypes.Silly))
                {
                    PersistentDataManager.gameSettings.enabledTips.Add(HintGiver.HintTypes.Silly);
                }
            }
            else
            {
                if (PersistentDataManager.gameSettings.enabledTips.Contains(HintGiver.HintTypes.Silly))
                {
                    PersistentDataManager.gameSettings.enabledTips.Remove(HintGiver.HintTypes.Silly);
                }
            }
        }

        public void SetLibraryPath()
        {
            FileBrowserOpener.OpenFolderPanelAsync("Select library directory", "", false, (string[] paths) =>
            {
                var path = Path.Combine(paths);
                if (path == string.Empty) return;

                PersistentDataManager.gameSettings.libLevelsDirectory = path;
                libraryPathField.text = path;
            });
        }

        public void ResetSettings()
        {
            if (!confirmErase)
            {
                resetSettingsText.text = Localization.T("Are you sure? You can't undo this!");
                confirmErase = true;
                return;
            }
            PersistentDataManager.CreateDefaultSettings();
            OnOpenTab();
            resetSettingsText.text = Localization.T("Reset!");
        }

        public override void OnOpenTab()
        {
            var settings = PersistentDataManager.gameSettings;

            splashScreenToggle.isOn = settings.showSplash;
            loadingHintsToggle.isOn = settings.showLoadingTips;
            filePickerToggle.isOn = settings.forceFallbackFileBrowser;

            settings.enabledTips ??= new() { HintGiver.HintTypes.General };

            playHintsToggle.isOn = settings.enabledTips.Contains(HintGiver.HintTypes.Play);
            editorHintsToggle.isOn = settings.enabledTips.Contains(HintGiver.HintTypes.Editor);
            sillyHintsToggle.isOn = settings.enabledTips.Contains(HintGiver.HintTypes.Silly);
            factHintsToggle.isOn = settings.enabledTips.Contains(HintGiver.HintTypes.Fact);

            GlobalGameManager.ChangeMasterVolume(settings.masterVolume);
            GlobalGameManager.ChangeMixerVolumes(settings.musicVolume, settings.sfxVolume);

            volSlider.value = GlobalGameManager.MasterVolume;
            volLabel.text = System.Math.Round(volSlider.value * 100, 2).ToString();
            musVolSlider.value = GlobalGameManager.MusicVolume;
            musVolLabel.text = System.Math.Round(musVolSlider.value * 100, 2).ToString();
            sfxVolSlider.value = GlobalGameManager.SfxVolume;
            sfxVolLabel.text = System.Math.Round(sfxVolSlider.value * 100, 2).ToString();

            dspSizeDropdown.ClearOptions();
            sampleRateDropdown.ClearOptions();
            dspSizeDropdown.AddOptions(GlobalGameManager.DSP_BUFFER_SIZES.Select(x => x.ToString()).ToList());
            sampleRateDropdown.AddOptions(GlobalGameManager.SAMPLE_RATES.Select(x => x.ToString()).ToList());
            dspSizeDropdown.value = GlobalGameManager.DSP_BUFFER_SIZES.ToList().IndexOf(GlobalGameManager.currentDspSize);
            sampleRateDropdown.value = GlobalGameManager.SAMPLE_RATES.ToList().IndexOf(GlobalGameManager.currentSampleRate);

            wantSampleRate = GlobalGameManager.currentSampleRate;
            wantDspSize = GlobalGameManager.currentDspSize;

            libraryPathField.text = settings.libLevelsDirectory;

            resetSettingsText.text = "";
            confirmErase = false;
        }

        public override void OnCloseTab() { }
    }
}