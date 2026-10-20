using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Editor.Track;
using HeavenStudio.Util;

using TMPro;

namespace HeavenStudio.Editor 
{
    public class Keybinds : TabsContent
    {
        [SerializeField] private TMP_Text KeybindText;

        private void Start()
        {
            // OnOpenTab();
        }

        public override void OnOpenTab()
        {
            if (Application.platform == RuntimePlatform.OSXPlayer)
            {
                KeybindText.text = KeybindText.text.Replace("Ctrl", "Cmd");
                KeybindText.text = KeybindText.text.Replace("Alt", "Option");
            }
        }

        public override void OnCloseTab()
        {
        }
    }
}