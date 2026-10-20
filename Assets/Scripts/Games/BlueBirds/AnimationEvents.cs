using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_BlueBirds
{
    public class AnimationEvents : MonoBehaviour
    {
        private BlueBirds game;
        private void Awake()
        {
            game = BlueBirds.instance;
        }
        void AllowScretch()
        {
            game.canStretch = true;
        }
        void DisallowStretch()
        {
            game.canStretch = false;
        }
        void StopBoppingPlayer()
        {
            game.canBop = false;
        }
        void ResumeBoppingPlayer()
        {
            game.canBop = true;
        }
    }
}
