using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using NaughtyBezierCurves;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_PackingPests
{
    public class ThrowObject2 : SuperCurveObject
    {   
        private PackingPests game;
        BezierCurve3D currentCurve;
        private void Awake()
        {
            game = PackingPests.instance;
        }
        public void Init(double beat)
        {
            
        }
        public void Init2(double beat)
        {

        }
    }
}