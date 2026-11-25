using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_BonOdori
{
    using static Minigames;

    //Easily generates a framework for block options for preset input blocks
    //Avoids repetitious/cluttering code in the game's Loader and makes implementing preset input blocks waaaaay less tedious
    public class ParamGenerator
    {
        //Audio Sample Params
        public struct SampleParams
        {
            public string internalParamName;
            public object defaultSample;
            public string editorName;

            public SampleParams(string internalParamName, object defaultSample, string editorName)
            {
                this.internalParamName = internalParamName;
                this.defaultSample = defaultSample;
                this.editorName  = editorName;
            }
        }

        private static Param _produceParam(string internalParamName, object paramType, string editorName, string tooltipText, List<Param.CollapseParam> conditionalParams = null)
        {
            return new Param(internalParamName, paramType, editorName, tooltipText, conditionalParams);
        }

        private static Param _produceClapParam(BonOdori.ClapType clapType)
        {
            return _produceParam("clapType", clapType, "Clap Type", "Set the type of clap.");
        }

        private static Param _produceVoiceSampleParam(string internalParamName, object defaultSample, string editorName)
        {
            return _produceParam(internalParamName, new EntityTypes.NoteSampleDropdown(defaultSample, BonOdori.GetSample, $"{internalParamName}Semitone"), editorName, "Set the variation of the voice line.");
        }

        private static Param _produceSemitoneParam(string internalParamName, string editorName)
        {
            return _produceParam(internalParamName, new EntityTypes.Note(offsetToC: false), editorName, "Set the number of semitones up or down this note should be pitched.");
        }

        private static string[] _produceHideableParams(List<SampleParams> sampleParams)
        {
            List<string> hideableList = new List<string>();
            foreach (var sampleParam in sampleParams)
            {
                hideableList.Add(sampleParam.internalParamName);
                hideableList.Add($"{sampleParam.internalParamName}Semitone");
            }
            return hideableList.ToArray();
        }

        public static List<Param> ProduceParamList(List<SampleParams> sampleParams, BonOdori.ClapType clapType)
        {
            List<Param> paramList = new List<Param>();
            string[] hideableList = _produceHideableParams(sampleParams);

            paramList.Add(new Param("mute", false, "Mute Voicelines", "Mute all voicelines", new List<Param.CollapseParam>()
            {
                new Param.CollapseParam((x, _) => (bool)x == false, hideableList)
            }));

            paramList.Add(_produceClapParam(clapType));

            foreach (var sampleParam in sampleParams)
            {
                paramList.Add(_produceVoiceSampleParam(sampleParam.internalParamName, sampleParam.defaultSample, sampleParam.editorName));
                paramList.Add(_produceSemitoneParam($"{sampleParam.internalParamName}Semitone", $"{sampleParam.editorName} Semitone"));
            }

            return paramList;
        }

    }
}


