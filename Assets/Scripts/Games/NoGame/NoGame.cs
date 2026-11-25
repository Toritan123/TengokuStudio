using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using HeavenStudio.Util;
using HeavenStudio.Common;
using HeavenStudio.InputSystem;

using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    /// Minigame loaders handle the setup of your minigame.
    /// Here, you designate the game prefab, define entities, and mark what AssetBundle to load

    /// Names of minigame loaders follow a specific naming convention of `PlatformcodeNameLoader`, where:
    /// `Platformcode` is a three-leter platform code with the minigame's origin
    /// `Name` is a short internal name
    /// `Loader` is the string "Loader"

    /// Platform codes are as follows:
    /// Agb: Gameboy Advance    ("Advance Gameboy")
    /// Ntr: Nintendo DS        ("Nitro")
    /// Rvl: Nintendo Wii       ("Revolution")
    /// Ctr: Nintendo 3DS       ("Centrair")
    /// Mob: Mobile
    /// Pco: PC / Other

    /// Fill in the loader class label, "*prefab name*", and "*Display Name*" with the relevant information
    /// For help, feel free to reach out to us on our discord, in the #development channel.
    public static class NoGameLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("noGame", "No Game", "272727", true, false, new List<GameAction>()
            {
                new GameAction("setText", "Set Text")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        NoGame.instance.ChangeText(e["text"]);
                    },
                    parameters = new List<Param>() {
                        new Param("text", "No Game", "Text", "Set the text on the screen."),
                    },
                },
            },
            font : "Roboto"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
    public class NoGame : Minigame
    {
        [SerializeField] TMP_Text Text;
        [SerializeField] TMP_Text ErrorText;
        public static NoGame instance;

        public enum Error
        {
            None = 0,
            GameNotInBuild = 1,
            FailedToLoadGame = 2,
            Unknown = 99
        }

        public static Error currentError = 0;
        public static string gameName = "";

        void Awake()
        {
            instance = this;
            SetError();
        }

        public void ChangeText(string text)
        {
            Text.text = text;
        }

        private void SetError()
        {
            switch (currentError)
            {
                case Error.None:
                    ErrorText.text = "";
                    break;
                case Error.GameNotInBuild:
                    ErrorText.text = Localization.F("The minigame \"{0}\" is not present in this build.\nPlease make sure your game is up-to-date.", gameName);
                    break;
                case Error.FailedToLoadGame:
                    ErrorText.text = Localization.F("Failed to load the minigame \"{0}\".", gameName);
                    break;
                default:
                    ErrorText.text = "An unknown error occurred.";
                    break;
            }

            currentError = Error.None;
            gameName = "";
        }
    }
}