using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using System;

namespace HeavenStudio.Games
{
    public class SoundEffects : MonoBehaviour
    {
        public enum CountNumbers { One, Two, Three, Four }
        public readonly static CountNumbers[] countNames = {  CountNumbers.One,  CountNumbers.Two,  CountNumbers.One,  CountNumbers.Two,  CountNumbers.Three,  CountNumbers.Four };
        public readonly static float[] timings           = {                0f,                2f,                4f,                5f,                  6f,                 7f };

        public enum CountInType 
        { 
            Normal, 
            Alt, 
            Cowbell,
            GBA,
            DSMale,
            DSFemale,
            Plus
        }

        public static string GetCountInSound(int type)
        {
            return (CountInType)type switch {
                CountInType.Normal  => "1",
                CountInType.Alt     => "2",
                CountInType.Cowbell => "cowbell",
                _ => "",
            };
        }

        public static string GetCountInFolder(int type)
        {
            return (CountInType)type switch {
                CountInType.GBA         => "gba/",
                CountInType.DSMale      => "dsmale/",
                CountInType.DSFemale    => "dsfemale/",
                CountInType.Plus        => "plus/",
                _ => "",
            };
        }

        public static double GetCountInOffset(int count, int type)
        {
            return (count + 4 * type) switch {
                (int)CountNumbers.Four  + 4 * (int)CountInType.GBA       => 0.050,
                (int)CountNumbers.One   + 4 * (int)CountInType.DSMale    => 0.028,
                (int)CountNumbers.Three + 4 * (int)CountInType.DSMale    => 0.033,
                (int)CountNumbers.Four  + 4 * (int)CountInType.DSMale    => 0.034,
                (int)CountNumbers.One   + 4 * (int)CountInType.Plus      => 0.014,
                (int)CountNumbers.Two   + 4 * (int)CountInType.Plus      => 0.014,
                (int)CountNumbers.Three + 4 * (int)CountInType.Plus      => 0.025,
                (int)CountNumbers.Four  + 4 * (int)CountInType.Plus      => 0.036,
                _ => 0,
            };
        }

        public static void PreloadCounts()
        {
            foreach (string load in new string[]
            { "one1", "two1", "three1", "four1",
              "one2", "two2", "three2", "four2",
              "cowbell", "ready1", "ready2",
              "go1", "go2", "and" })
            {
                SoundByte.PreloadAudioClipAsync(load);
            }
        }

        public static void CountIn(double beat, float length, int type, bool go, bool and)
        {
            string countType = GetCountInSound(type);
            string countFolder = GetCountInFolder(type);
            bool cowbell = countType == "cowbell";

            double startBeat = beat + length - 8;

            List<MultiSound.Sound> sfx = new();
            if (and && !cowbell) {
                sfx.Add(new MultiSound.Sound("count-ins/" + countFolder + "and", Mathf.Max((float)beat - 0.5f, (float)startBeat + 3.5f)));
            }
            for (int i = 0; i < countNames.Length; i++) {
                if (startBeat + timings[i] >= beat) {
                    string countName = cowbell ? countType : countNames[i] + countType;
                    sfx.Add(new MultiSound.Sound("count-ins/" + countFolder + countName, startBeat + timings[i], offset: GetCountInOffset((int)countNames[i], type)));
                }
            }
            if (go && !cowbell) {
                sfx[^1].name = "count-ins/" + countFolder + "go" + countType;
                sfx[^1].offset = 0f;
            }
            MultiSound.Play(sfx, false);
        }

        public static void TwoBeatCountIn(double beat, float length, int type, bool go, bool and)
        {
            string countType = GetCountInSound(type);
            string countFolder = GetCountInFolder(type);
            bool cowbell = countType == "cowbell";

            List<MultiSound.Sound> sfx = new();
            if (and && !cowbell) {
                sfx.Add(new MultiSound.Sound("count-ins/" + countFolder + "and", beat - 0.5f));
            }
            for (int i = 0; i < 2; i++)
            {
                string countName = cowbell ? countType : countNames[i + 4] + countType;
                sfx.Add(new MultiSound.Sound("count-ins/" + countFolder + countName, beat + (i * length), offset: GetCountInOffset((int)countNames[i + 4], type)));
            }
            if (go && !cowbell) {
                sfx[^1].name = "count-ins/" + countFolder + "go" + countType;
                sfx[^1].offset = 0f;
            }
            MultiSound.Play(sfx, false);
        }

        public static void FourBeatCountIn(double beat, float length, int type, bool go, bool and)
        {
            string countType = GetCountInSound(type);
            string countFolder = GetCountInFolder(type);
            bool cowbell = countType == "cowbell";
            
            List<MultiSound.Sound> sfx = new();
            if (and && !cowbell) {
                sfx.Add(new MultiSound.Sound("count-ins/" + countFolder + "and", beat - 0.5f));
            }
            for (int i = 0; i < 4; i++) {
                string countName = cowbell ? countType : countNames[i + 2] + countType;
                sfx.Add(new MultiSound.Sound("count-ins/" + countFolder + countName, beat + (i * length), offset: GetCountInOffset((int)countNames[i + 2], type)));
            }
            if (go && !cowbell) {
                sfx[^1].name = "count-ins/" + countFolder + "go" + countType;
                sfx[^1].offset = 0f;
            }
            MultiSound.Play(sfx, false);
        }

        public static void EightBeatCountIn(double beat, float length, int type, bool go, bool and)
        {
            string countType = GetCountInSound(type);
            string countFolder = GetCountInFolder(type);
            bool cowbell = countType == "cowbell";
            
            List<MultiSound.Sound> sfx = new();
            if (and && !cowbell) {
                sfx.Add(new MultiSound.Sound("count-ins/" + countFolder + "and", beat - 0.5f));
            }
            for (int i = 0; i < countNames.Length; i++) {
                string countName = cowbell ? countType : countNames[i] + countType;
                sfx.Add(new MultiSound.Sound("count-ins/" + countFolder + countName, beat + (timings[i] * length), offset: GetCountInOffset((int)countNames[i], type)));
            }
            if (go && !cowbell) {
                sfx[^1].name = "count-ins/" + countFolder + "go" + countType;
                sfx[^1].offset = 0f;
            }
            MultiSound.Play(sfx, false);
        }

        public static void Count(double beat, int count, int type)
        {
            string countType = GetCountInSound(type);
            string countFolder = GetCountInFolder(type);

            string countName = countType == "cowbell" ? countType : (CountNumbers)count + countType;
            SoundByte.PlayOneShot("count-ins/" + countFolder + countName, beat, offset: GetCountInOffset(count, type));
        }

        public static void Cowbell()
        {
            SoundByte.PlayOneShot("count-ins/cowbell");
        }

        public static void Ready(double beat, float length)
        {
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("count-ins/ready1", beat),
                new MultiSound.Sound("count-ins/ready2", beat + (1f * length)),
            }, false);
        }

        public static void And()
        {
            SoundByte.PlayOneShot("count-ins/and");
        }

        public static void Go(bool alt)
        {
            SoundByte.PlayOneShot("count-ins/go" + (!alt ? "1" : "2"));
        }
    }

}