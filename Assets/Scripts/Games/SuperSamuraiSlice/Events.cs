using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_SuperSamuraiSlice
{
    public class Events : MonoBehaviour
    {
        private SuperSamuraiSlice game;
        private void Awake()
        {
            game = SuperSamuraiSlice.instance;
        }
        public void StopBopping()
        {
            game.samuraiBop = false;
        }
        public void StartBopping()
        {
            game.samuraiBop = true;
        }
    }
}
