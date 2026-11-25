using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using HeavenStudio.Editor;
using System.Collections.Generic;
using NUnit.Framework;

namespace HeavenStudio.Common 
{
    public class SettingsDialog : Dialog
    {
        [SerializeField] private TMP_Text BuildDateDisplay;
        [SerializeField] public TMP_Text DescriptionDisplay;
        [SerializeField] public string defaultDescription;
        [SerializeField] private TabsManager tabsManager;
        [SerializeField] private TabsManager.TabsEntry[] tabs;
        [SerializeField] private GameObject descriptionGO;
        public static SettingsDialog instance;
        public bool active = false;

        private void Awake()
        {
            instance = this;
            active = dialog.activeSelf;
        }

        public void SwitchSettingsDialog()
        {
            if(dialog.activeSelf) {
                dialog.SetActive(false);
                active = false;

                PersistentDataManager.SaveSettings();
                tabsManager.CleanTabs();

                if (GameManager.instance != null && GameManager.instance.CircleCursor != null && GameManager.instance.playMode)
                {
                    GameManager.instance.CircleCursor.LockCursor(true);
                }

                if (Editor.Editor.instance == null) return;
                Editor.Editor.instance.canSelect = true;
                Editor.Editor.instance.inAuthorativeMenu = false;
            } else {
                ResetAllDialogs();
                dialog.SetActive(true);
                active = true;

                tabsManager.GenerateTabs(tabs);

                BuildDateDisplay.text = GlobalGameManager.buildTime;
                DescriptionDisplay.text = Localization.T(defaultDescription);


                if (GameManager.instance != null && GameManager.instance.CircleCursor != null && GameManager.instance.playMode)
                {
                    GameManager.instance.CircleCursor.LockCursor(false);
                }

                if (Editor.Editor.instance == null) return;
                Editor.Editor.instance.canSelect = false;
                Editor.Editor.instance.inAuthorativeMenu = true;
            }
        }

        public void ToggleDescriptions(bool enabled)
        {
            descriptionGO.SetActive(enabled);
        }

        private void Update() {}
    }
}