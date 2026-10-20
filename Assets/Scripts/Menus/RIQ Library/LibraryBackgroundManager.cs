using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio
{
    public class LibraryBackgroundManager : MonoBehaviour
    {
        [Header("Elements")]
        [SerializeField] private Material gradientMat;
        [SerializeField] private Material wavesMat;
        [SerializeField] private RectTransform puffsHolder;

        [Header("Params")]
        [SerializeField] private Color gradientTopColorStart;
        [SerializeField] private Color gradientBottomColorStart;
        [SerializeField] private float gradientHueShiftSpeed;
        [SerializeField] private float puffsSpeed;


        [Header("Widths")]
        [SerializeField] private float puffsWidth;

        [Header("Vars")]
        [SerializeField] private float time = 0f;
        [SerializeField] private Vector3 puffsStartPos = new();

        void Start()
        {
            puffsStartPos = puffsHolder.localPosition;

            gradientMat.SetColor("_ColorAlpha", gradientTopColorStart);
            gradientMat.SetColor("_ColorBravo", gradientBottomColorStart);
        }

        // Update is called once per frame
        void Update()
        {
            time += Time.deltaTime;

            // MoveTopWave();
            // MoveBottomWave();
            MoveWaves();
            MovePuffs();

            ShiftGradientHue();
        }

        // private void MoveTopWave()
        // {
        //     float topWaveX = topWaveHolder.localPosition.x + (topWaveSpeed * Time.deltaTime);
        //     float topWaveY = topWaveStartPos.y + (Mathf.Sin((topWaveWobbleSpeed * time) + topWaveSinOffset) * topWaveWobbleAmp);

        //     while ((topWaveWidth != 0) && (topWaveX > topWaveWidth))
        //     {
        //         topWaveX -= topWaveWidth;
        //     }

        //     topWaveHolder.localPosition = new(topWaveX, topWaveY);
        // }

        // private void MoveBottomWave()
        // {
        //     float bottomWaveX = bottomWaveHolder.localPosition.x + (bottomWaveSpeed * Time.deltaTime);
        //     float bottomWaveY = bottomWaveStartPos.y + (Mathf.Sin((bottomWaveWobbleSpeed * time) + bottomWaveSinOffset) * bottomWaveWobbleAmp);

        //     while ((bottomWaveWidth != 0) && (bottomWaveX > bottomWaveWidth))
        //     {
        //         bottomWaveX -= bottomWaveWidth;
        //     }

        //     bottomWaveHolder.localPosition = new(bottomWaveX, bottomWaveY);
        // }

        private void MoveWaves()
        {
            wavesMat.SetFloat("_AnimFrame", time);
        }

        private void MovePuffs()
        {
            float puffsX = puffsHolder.localPosition.x + (puffsSpeed * Time.deltaTime);
            float puffsY = puffsStartPos.y;

            while ((puffsWidth != 0) && (puffsX > puffsWidth))
            {
                puffsX -= puffsWidth;
            }

            puffsHolder.localPosition = new(puffsX, puffsY);
        }

        private void ShiftGradientHue()
        {
            Color topColor = gradientMat.GetColor("_ColorAlpha");
            Color bottomColor = gradientMat.GetColor("_ColorBravo");

            Color.RGBToHSV(topColor, out float topH, out float topS, out float topV);
            Color.RGBToHSV(bottomColor, out float bottomH, out float bottomS, out float bottomV);

            float newTopH = topH + ((gradientHueShiftSpeed / 360f) * Time.deltaTime);
            while (newTopH > 1)
            {
                newTopH--;
            }

            float newBottomH = bottomH + ((gradientHueShiftSpeed / 360f) * Time.deltaTime);
            while (newBottomH > 1)
            {
                newBottomH--;
            }

            gradientMat.SetColor("_ColorAlpha", Color.HSVToRGB(newTopH, topS, topV));
            gradientMat.SetColor("_ColorBravo", Color.HSVToRGB(newBottomH, bottomS, bottomV));
        }
    }
}