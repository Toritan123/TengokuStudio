using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_BonOdori
{
    public class YaguraChan : MonoBehaviour
    {
        [SerializeField] private Animator _yaguraChan;
        [SerializeField] private Animator _yaguraFace;

        public void Bop()
        {
            _yaguraChan.DoScaledAnimationAsync("Bop", 0.5f);
        }

        public void SetYaguraFace(string faceAnim)
        {
            _yaguraFace.DoScaledAnimationAsync(faceAnim, 0.5f);
        }
    }
}


