using UnityEngine;
using UnityEngine.UI;
using TMPro;

using HeavenStudio.Common;
using System.Runtime.InteropServices;
using System;

namespace HeavenStudio.Editor 
{
    public class EditorSettings : TabsContent
    {
        [SerializeField] Toggle cursorCheckbox;
        [SerializeField] Toggle discordRPCCheckbox;
        [SerializeField] Button editorScaleDecre, editorScaleIncre;
        [SerializeField] Toggle scaleWSS;
        [SerializeField] TMP_Dropdown paramTooltipsDropdown;
        [SerializeField] Toggle previewNoteSoundsToggle;
        [SerializeField] Toggle simpleIconsToggle;
        [SerializeField] Toggle showFanGamesToggle;
        [SerializeField] Toggle advancedFeaturesToggle;
        [SerializeField] Toggle minigameColorToggle;
        [SerializeField] TMP_Dropdown fontDropdown;

        // [SerializeField] Toggle cornerTooltipsToggle;

        private void Start()
        {
            cursorCheckbox.isOn = PersistentDataManager.gameSettings.editorCursorEnable;
            discordRPCCheckbox.isOn = PersistentDataManager.gameSettings.discordRPCEnable;
            scaleWSS.isOn = PersistentDataManager.gameSettings.scaleWScreenSize;
            paramTooltipsDropdown.value = PersistentDataManager.gameSettings.showParamTooltips;
            previewNoteSoundsToggle.isOn = PersistentDataManager.gameSettings.previewNoteSounds;
            simpleIconsToggle.isOn = PersistentDataManager.gameSettings.simpleIcons;
            showFanGamesToggle.isOn = PersistentDataManager.gameSettings.showFanGames;
            minigameColorToggle.isOn = PersistentDataManager.gameSettings.editorMinigameColorEvents;
            fontDropdown.value = PersistentDataManager.gameSettings.fontType;
            advancedFeaturesToggle.isOn = PersistentDataManager.gameSettings.advancedFeatures;

            SetDecreIncreInteractable();
        }

        public void OnCursorCheckboxChanged()
        {
            Editor.instance.isCursorEnabled = cursorCheckbox.isOn;
            PersistentDataManager.gameSettings.editorCursorEnable = cursorCheckbox.isOn;
            if (Editor.instance != null && !Editor.instance.fullscreen)
            {
                GameManager.instance.CursorCam.enabled = Editor.instance.isCursorEnabled;
            }
        }

        public void OnRPCCheckboxChanged()
        {
            PersistentDataManager.gameSettings.discordRPCEnable = discordRPCCheckbox.isOn;
            Editor.instance.isDiscordEnabled = discordRPCCheckbox.isOn;
        }

        public void OnShowFangamesChanged()
        {
            PersistentDataManager.gameSettings.showFanGames = showFanGamesToggle.isOn;
            GridGameSelector.instance.showFanGames = showFanGamesToggle.isOn;
            GridGameSelector.instance.RefreshGamesList();
        }

        public void OnAdvancedFeaturesChanged()
        {
            PersistentDataManager.gameSettings.advancedFeatures = advancedFeaturesToggle.isOn;
            GridGameSelector.instance.showAdvanced = advancedFeaturesToggle.isOn;
            GridGameSelector.instance.RefreshGamesList();
        }

        public void OnMinigameColorEventsChanged()
        {
            PersistentDataManager.gameSettings.editorMinigameColorEvents = minigameColorToggle.isOn;
            Track.TimelineBlockManager.Instance.UpdateMarkerColors();
        }

        public override void OnOpenTab()
        {
            Start();
        }

        public override void OnCloseTab()
        {
        }

        public void OnSWSSChanged()
        {
            PersistentDataManager.gameSettings.scaleWScreenSize = scaleWSS.isOn;
            scaleWSS.isOn = PersistentDataManager.gameSettings.scaleWScreenSize;
        }

        public void OnParamTooltipsChanged()
        {
            // tooltip types: 0 = only corner, 1 = delayed on mouse, 2 = instant on mouse
            PersistentDataManager.gameSettings.showParamTooltips = paramTooltipsDropdown.value;
        }

        public void OnFontTypeChanged()
        {
            //font types: 0 = Megamix fonts, 1 = Rodin DB
            PersistentDataManager.gameSettings.fontType = fontDropdown.value;
            GridGameSelector.instance.RefreshTitle();
        }

        public void OnIconTypeChanged()
        {
            PersistentDataManager.gameSettings.simpleIcons = simpleIconsToggle.isOn;
            GridGameSelector.instance.RefreshIcons();
        }

        public void OnPreviewNoteSoundsChanged()
        {
            PersistentDataManager.gameSettings.previewNoteSounds = previewNoteSoundsToggle.isOn;
        }

        // public void OnCornerTooltipsChanged()
        // {
        //     PersistentDataManager.gameSettings.showParamTooltips = cornerTooltipsToggle.isOn;
        // }

        public void OnEditorScaleDecre()
        {
            PersistentDataManager.gameSettings.editorScale = Math.Max(-3, PersistentDataManager.gameSettings.editorScale - 1);

            SetDecreIncreInteractable();
        }

        public void OnEditorScaleIncre()
        {
            PersistentDataManager.gameSettings.editorScale = Math.Min(5, PersistentDataManager.gameSettings.editorScale + 1);

            SetDecreIncreInteractable();
        }

        public void OnEditorScaleReset()
        {
            PersistentDataManager.gameSettings.editorScale = 0;
            SetDecreIncreInteractable();
        }

        private void SetDecreIncreInteractable()
        {
            editorScaleDecre.interactable = PersistentDataManager.gameSettings.editorScale > -3; // hardcoded? We might not change.
            editorScaleIncre.interactable = PersistentDataManager.gameSettings.editorScale < 5;
        }


    }
}