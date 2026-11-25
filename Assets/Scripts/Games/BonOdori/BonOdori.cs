using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    using Scripts_BonOdori;
    public static class AgbBonOdoriLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("bonOdori", "The☆Bon Odori \n<color=#adadad>(Za☆Bon Odori)", "312B9F", false, false, new List<GameAction>()
            {                   
                new GameAction("bop", "Bop")
                {   
                    function = delegate { var e = eventCaller.currentEntity; BonOdori.instance.Bop(e.beat, e.length, (BonOdori.WhoBops)e["bop"], (BonOdori.WhoBops)e["auto"]);},
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("bop", BonOdori.WhoBops.Both, "Bop"),
                        new Param("auto", BonOdori.WhoBops.None, "Bop (Auto)")
                    },
                },

                new GameAction("don", "Don") //TODO: create a ClapCue class which consolidates PresetVoiceline & PresetInput for simplicity and organization for all cues
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        string[] dVariations = { "don", "do", "do_n" };
                        //converter code for old blocks; manually assigns old-named collapseParam nulls to appropriate corresponding ints
                        if (e["variationDon"] != null) e["donVariation"] = e["variationDon"]; //0
                        if (e["variationDo"] != null) e["doVariation"] = e["variationDo"]; //1
                        if (e["variationDo_n"] != null) e["do_nVariation"] = e["variationDo_n"];//2

                        int type = e["type"];
                        string variation = dVariations[type] + "Variation";
                        BonOdori.instance.StandaloneDonSfx(
                            e.beat,
                            new BonOdori.PresetVoiceline(type, (int)e[variation], e["semitone"], false) //annoying to figure out but RiqEntity makes e[variation] an object instead of int after conversion code; so recast
                        );
                    },
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("type", BonOdori.DonType.Don, "Type", "Set the type of voiceline", new List<Param.CollapseParam>(){
                            new Param.CollapseParam((x, _) => (int)x == 0, new string[] { "donVariation"}),
                            new Param.CollapseParam((x, _) => (int)x == 1, new string[] { "doVariation"}),
                            new Param.CollapseParam((x, _) => (int)x == 2, new string[] { "do_nVariation"}),
                        }),
                        new Param("donVariation", new EntityTypes.NoteSampleDropdown(BonOdori.DonVariation.DonA, BonOdori.GetSample, "semitone"), "Don Type", "Set the variation of the voice line."),
                        new Param("doVariation", new EntityTypes.NoteSampleDropdown(BonOdori.DoVariation.DoC, BonOdori.GetSample, "semitone"), "Do Type", "Set the variation of the voice line."),
                        new Param("do_nVariation", new EntityTypes.NoteSampleDropdown(BonOdori.Do_nVariation.Do_nA, BonOdori.GetSample, "semitone"), "Do-n Type", "Set the variation of the voice line."),
                        new Param("semitone", new EntityTypes.Note(offsetToC: false), "Semitone", "Set the number of semitones up or down this note should be pitched."),
                    },
                },

                new GameAction("pan", "Pan")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        string[] pVariations = { "pan", "pa", "pa_n" };

                        if (e["variationPan"] != null) e["panVariation"] = e["variationPan"]; //0
                        if (e["variationPa"] != null) e["paVariation"] = e["variationPa"]; //1
                        if (e["variationPa_n"] != null) e["pa_nVariation"] = e["variationPa_n"];//2

                        int type = e["type"];
                        string variation = pVariations[type] + "Variation"; //passes a NoteSampleDropdown Param value
                        BonOdori.instance.StandalonePanSfx(
                            e.beat, 
                            e["mute"], 
                            new BonOdori.PresetVoiceline(e["type"], (int)e[variation], e["semitone"], true)
                        );
                        BonOdori.instance.CueStandalonePan(
                            e.beat,
                            new BonOdori.PresetInput(e["clapType"])
                        );
                    },
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("mute", false, "Mute Sound", "Mute the voice line.", new List<Param.CollapseParam>(){
                            new Param.CollapseParam((x, _) => (bool)x == false, new string[] { "type", "semitone" })
                        }),
                        new Param("clapType", BonOdori.ClapType.SideClap, "Clap Type", "Set the type of clap."),
                        new Param("type",BonOdori.PanType.Pan, "Type", "Set the type of voice line.", new List<Param.CollapseParam>(){
                            new Param.CollapseParam((x, _) => (int)x == 0, new string[] { "panVariation"}),
                            new Param.CollapseParam((x, _) => (int)x == 1, new string[] { "paVariation"}),
                            new Param.CollapseParam((x, _) => (int)x == 2, new string[] { "pa_nVariation"}),
                        }),
                        new Param("panVariation", new EntityTypes.NoteSampleDropdown(BonOdori.PanVariation.PanC, BonOdori.GetSample, "semitone"), "Pan Type", "Set the variation of the voice line."),
                        new Param("paVariation", new EntityTypes.NoteSampleDropdown(BonOdori.PaVariation.PaG, BonOdori.GetSample, "semitone"), "Pa Type", "Set the variation of the voice line."),
                        new Param("pa_nVariation", new EntityTypes.NoteSampleDropdown(BonOdori.Pa_nVariation.Pa_nA, BonOdori.GetSample, "semitone") , "Pa-n Type", "Set the variation of the voice line."),
                        new Param("semitone", new EntityTypes.Note(offsetToC: false), "Semitone", "Set the number of semitones up or down this note should be pitched."),
                    },
                },

                new GameAction("donDonPanPan", "Do-n Do-n Pa-n Pa-n")
                {
                    inactiveFunction = delegate { 
                        var e = eventCaller.currentEntity;
                        BonOdori.DonDonPanPanSfx(
                            e.beat, 
                            e["mute"],
                            e["clapType"],
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Do_n, e["do_n1"], e["do_n1Semitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Do_n, e["do_n2"], e["do_n2Semitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pan, e["pan1"], e["pan1Semitone"], true),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pan, e["pan2"], e["pan2Semitone"], true),
                            isInactiveFunc: true
                        ); 
                    },
                    function = delegate { 
                        var e = eventCaller.currentEntity;
                        BonOdori.DonDonPanPanSfx(
                            e.beat, 
                            e["mute"],
                            e["clapType"],
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Do_n, e["do_n1"], e["do_n1Semitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Do_n, e["do_n2"], e["do_n2Semitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pan, e["pan1"], e["pan1Semitone"], true),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pan, e["pan2"], e["pan2Semitone"], true)
                        );
                        BonOdori.instance.CueDonDonPanPan(
                            e.beat,
                            new BonOdori.PresetInput(e["clapType"]),
                            new BonOdori.PresetInput(e["clapType"])
                        ); 
                    },
                    defaultLength = 4f,
                    parameters = ParamGenerator.ProduceParamList(
                        new List<ParamGenerator.SampleParams> {
                            new ParamGenerator.SampleParams("do_n1", BonOdori.Do_nVariation.Do_nG, "Do-n 1"), //EventParameterManager needs an Enum
                            new ParamGenerator.SampleParams("do_n2", BonOdori.Do_nVariation.Do_nA, "Do-n 2"),
                            new ParamGenerator.SampleParams("pan1", BonOdori.PanVariation.PanC, "Pan 1"),
                            new ParamGenerator.SampleParams("pan2", BonOdori.PanVariation.PanA, "Pan 2"),
                        },
                        BonOdori.ClapType.SideClap
                    ),
                },

                new GameAction("dondoPanPan", "Dondo Pa-n Pan")
                {
                    inactiveFunction = delegate { 
                        var e = eventCaller.currentEntity;
                        BonOdori.DondoPanPanSfx(
                            e.beat, 
                            e["mute"],
                            e["clapType"],
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Don, e["don"], e["donSemitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Do, e["do"], e["doSemitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa_n, e["pa_n1"], e["pa_n1Semitone"], true),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa_n, e["pa_n2"], e["pa_n2Semitone"], true),
                            isInactiveFunc: true
                        ); 
                    },
                    function = delegate { 
                        var e = eventCaller.currentEntity;
                        BonOdori.DondoPanPanSfx(
                            e.beat, 
                            e["mute"],
                            e["clapType"],
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Don, e["don"], e["donSemitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Do, e["do"], e["doSemitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa_n, e["pa_n1"], e["pa_n1Semitone"], true),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa_n, e["pa_n2"], e["pa_n2Semitone"], true)
                        ); 
                        BonOdori.instance.CueDondoPanPan(
                            e.beat, 
                            new BonOdori.PresetInput(e["clapType"]),
                            new BonOdori.PresetInput(e["clapType"])
                        ); 
                    },
                    defaultLength = 4f,
                    parameters = ParamGenerator.ProduceParamList(
                        new List<ParamGenerator.SampleParams> {
                            new ParamGenerator.SampleParams("don", BonOdori.DonVariation.DonG, "Don"),
                            new ParamGenerator.SampleParams("do", BonOdori.DoVariation.DoG, "Do"),
                            new ParamGenerator.SampleParams("pa_n1", BonOdori.Pa_nVariation.Pa_nC, "Pa_n 1"),
                            new ParamGenerator.SampleParams("pa_n2", BonOdori.Pa_nVariation.Pa_nC, "Pa_n 2"),
                        },
                        BonOdori.ClapType.FrontClap
                    ),
                },

                new GameAction("dondoPanpa", "Dondo Panpa")
                {
                    inactiveFunction = delegate { 
                        var e = eventCaller.currentEntity;
                        BonOdori.DondoPanpaSfx(
                            e.beat, 
                            e["mute"],
                            e["clapType"],
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Don, e["don"], e["donSemitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Do, e["do"], e["doSemitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa_n, e["pa_n"], e["pa_nSemitone"], true),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa, e["pa"], e["paSemitone"], true),
                            isInactiveFunc: true
                        ); 
                    },
                    function = delegate { 
                        var e = eventCaller.currentEntity;
                        BonOdori.DondoPanpaSfx(
                            e.beat, 
                            e["mute"],
                            e["clapType"],
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Don, e["don"], e["donSemitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Do, e["do"], e["doSemitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa_n, e["pa_n"], e["pa_nSemitone"], true),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa, e["pa"], e["paSemitone"], true)
                        ); 
                        BonOdori.instance.CueDondoPanpa(
                            e.beat,
                            new BonOdori.PresetInput(e["clapType"]),
                            new BonOdori.PresetInput(e["clapType"])
                        ); 
                    },
                    defaultLength = 2f,
                    parameters = ParamGenerator.ProduceParamList(
                        new List<ParamGenerator.SampleParams> {
                            new ParamGenerator.SampleParams("don", BonOdori.DonVariation.DonC, "Don"),
                            new ParamGenerator.SampleParams("do", BonOdori.DoVariation.DoC, "Do"),
                            new ParamGenerator.SampleParams("pa_n", BonOdori.Pa_nVariation.Pa_nA, "Pa_n"),
                            new ParamGenerator.SampleParams("pa", BonOdori.PaVariation.PaG, "Pa"),
                        },
                        BonOdori.ClapType.SideClap
                    ),
                },

                new GameAction("donPanPan", "Do-n Pa-n Pa-n")
                {
                    inactiveFunction = delegate { 
                        var e = eventCaller.currentEntity;
                        BonOdori.DonPanPanSfx(
                            e.beat, 
                            e["mute"],
                            e["clapType"],
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Do_n, e["do_n"], e["do_nSemitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa_n, e["pa_n1"], e["pa_n1Semitone"], true),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa_n, e["pa_n2"], e["pa_n2Semitone"], true),
                            isInactiveFunc: true
                        ); 
                    },
                    function = delegate { 
                        var e = eventCaller.currentEntity;
                        BonOdori.DonPanPanSfx(
                            e.beat, 
                            e["mute"],
                            e["clapType"],
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Do_n, e["do_n"], e["do_nSemitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa_n, e["pa_n1"], e["pa_n1Semitone"], true),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa_n, e["pa_n2"], e["pa_n2Semitone"], true)
                        ); 
                        BonOdori.instance.CueDonPanPan(
                            e.beat,
                            new BonOdori.PresetInput(e["clapType"]),
                            new BonOdori.PresetInput(e["clapType"])
                        ); 
                    },
                    defaultLength = 4f,
                    parameters = ParamGenerator.ProduceParamList(
                        new List<ParamGenerator.SampleParams> {
                            new ParamGenerator.SampleParams("do_n", BonOdori.Do_nVariation.Do_nA, "Do_n"),
                            new ParamGenerator.SampleParams("pa_n1", BonOdori.Pa_nVariation.Pa_nC, "Pa_n 1"),
                            new ParamGenerator.SampleParams("pa_n2", BonOdori.Pa_nVariation.Pa_nC, "Pa_n 2"),
                        },
                        BonOdori.ClapType.FrontClap
                    ),
                },

                new GameAction("donDonPan", "Do-n Do-n Pan")
                {
                    inactiveFunction = delegate { 
                        var e = eventCaller.currentEntity;
                        BonOdori.DonDonPanSfx(
                            e.beat, 
                            e["mute"],
                            e["clapType"],
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Don, e["don1"], e["don1Semitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Don, e["don2"], e["don2Semitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pan, e["pan"], e["panSemitone"], true),
                            isInactiveFunc: true
                        ); 
                    },
                    function = delegate { 
                        var e = eventCaller.currentEntity;
                        BonOdori.DonDonPanSfx(
                            e.beat, 
                            e["mute"],
                            e["clapType"],
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Don, e["don1"], e["don1Semitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Don, e["don2"], e["don2Semitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pan, e["pan"], e["panSemitone"], true)
                        ); 
                        BonOdori.instance.CueDonDonPan(
                            e.beat,
                            new BonOdori.PresetInput(e["clapType"])
                        ); 
                    },
                    defaultLength = 4f,
                    parameters = ParamGenerator.ProduceParamList(
                        new List<ParamGenerator.SampleParams> {
                            new ParamGenerator.SampleParams("don1", BonOdori.DonVariation.DonD, "Don 1"),
                            new ParamGenerator.SampleParams("don2", BonOdori.DonVariation.DonD, "Don 2"),
                            new ParamGenerator.SampleParams("pan", BonOdori.PanVariation.PanE, "Pan"),
                        },
                        BonOdori.ClapType.FrontClap
                    ),
                },

                new GameAction("panPaDondoDo", "Pan Pa Dondo Do")
                {
                    preFunction = delegate { 
                        var e = eventCaller.currentEntity;
                        BonOdori.PanPaDondoDoSfx(
                            e.beat, 
                            e["mute"],
                            e["clapType"],
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa_n, e["pa_n1"], e["pa_n1Semitone"], true),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa_n, e["pa_n2"], e["pa_n2Semitone"], true),
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Don, e["don"], e["donSemitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Do, e["do"], e["doSemitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Do_n, e["do_n"], e["do_nSemitone"], false)
                        );
                        BonOdori.instance.CuePanPaDondoDo(
                            e.beat,
                            new BonOdori.PresetInput(e["clapType"]),
                            new BonOdori.PresetInput(e["clapType"])
                        ); 
                    },
                    defaultLength = 4f,
                    parameters = ParamGenerator.ProduceParamList(
                        new List<ParamGenerator.SampleParams> {
                            new ParamGenerator.SampleParams("pa_n1", BonOdori.Pa_nVariation.BonDancePa_nG, "Pa_n 1"),
                            new ParamGenerator.SampleParams("pa_n2", BonOdori.Pa_nVariation.BonDancePa_nA, "Pa_n 2"),
                            new ParamGenerator.SampleParams("don", BonOdori.DonVariation.BonDanceDonC, "Don"),
                            new ParamGenerator.SampleParams("do", BonOdori.DoVariation.BonDanceDoC, "Do"),
                            new ParamGenerator.SampleParams("do_n", BonOdori.Do_nVariation.BonDanceDo_nA, "Do_n"),
                        },
                        BonOdori.ClapType.SideClap
                    ),
                },

                new GameAction("panpaDoPa", "Panpa Do Pa")
                {
                    preFunction = delegate { 
                        var e = eventCaller.currentEntity;
                        BonOdori.PanpaDoPaSfx(
                            e.beat, 
                            e["mute"],
                            e["clapType"],
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pan, e["pan"], e["panSemitone"], true),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa, e["pa"], e["paSemitone"], true),
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Do_n, e["do_n"], e["do_nSemitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa_n, e["pa_n"], e["pa_nSemitone"], true)
                        );
                        BonOdori.instance.CuePanpaDoPa(
                            e.beat,
                            new BonOdori.PresetInput(e["clapType"]),
                            new BonOdori.PresetInput(e["clapType"]),
                            new BonOdori.PresetInput(e["clapType"])
                        ); 
                    },
                    defaultLength = 4f,
                    parameters = ParamGenerator.ProduceParamList(
                        new List<ParamGenerator.SampleParams> {
                            new ParamGenerator.SampleParams("pan", BonOdori.PanVariation.BonDancePanG, "Pan"),
                            new ParamGenerator.SampleParams("pa", BonOdori.PaVariation.BonDancePaG, "Pa"),
                            new ParamGenerator.SampleParams("do_n", BonOdori.Do_nVariation.BonDanceDo_nA, "Do_n"),
                            new ParamGenerator.SampleParams("pa_n", BonOdori.Pa_nVariation.BonDancePa_nC, "Pa_n"),
                        },
                        BonOdori.ClapType.FrontClap
                    ),
                },

                new GameAction("dondoPanpaPa", "Dondo Panpa Pa")
                {
                    inactiveFunction = delegate { 
                        var e = eventCaller.currentEntity;
                        BonOdori.DondoPanpaPaSfx(
                            e.beat, 
                            e["mute"],
                            e["clapType"],
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Don, e["don"], e["donSemitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Do, e["do"], e["doSemitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pan, e["pan"], e["panSemitone"], true),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa, e["pa"], e["paSemitone"], true),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa_n, e["pa_n"], e["pa_nSemitone"], true),
                            isInactiveFunc: true
                        ); 
                    },
                    function = delegate { 
                        var e = eventCaller.currentEntity;
                        BonOdori.DondoPanpaPaSfx(
                            e.beat, 
                            e["mute"],
                            e["clapType"],
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Don, e["don"], e["donSemitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Do, e["do"], e["doSemitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pan, e["pan"], e["panSemitone"], true),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa, e["pa"], e["paSemitone"], true),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa_n, e["pa_n"], e["pa_nSemitone"], true)
                        ); 
                        BonOdori.instance.CueDondoPanpaPa(
                            e.beat,
                            new BonOdori.PresetInput(e["clapType"]),
                            new BonOdori.PresetInput(e["clapType"]),
                            new BonOdori.PresetInput(e["clapType"])
                        ); 
                    },
                    defaultLength = 4f,
                    parameters = ParamGenerator.ProduceParamList(
                        new List<ParamGenerator.SampleParams> {
                            new ParamGenerator.SampleParams("don", BonOdori.DonVariation.BonDanceDonD, "Don"),
                            new ParamGenerator.SampleParams("do", BonOdori.DoVariation.BonDanceDoD, "Do"),
                            new ParamGenerator.SampleParams("pan", BonOdori.PanVariation.BonDancePanD, "Pan"),
                            new ParamGenerator.SampleParams("pa", BonOdori.PaVariation.BonDancePaD, "Pa"),
                            new ParamGenerator.SampleParams("pa_n", BonOdori.Pa_nVariation.BonDancePa_nE, "Pa_n"),
                        },
                        BonOdori.ClapType.FrontClap
                    ),
                },

                new GameAction("panpaDondo", "Panpa Dondo")
                {
                    preFunction = delegate { 
                        var e = eventCaller.currentEntity;
                        BonOdori.PanpaDondoSfx(
                            e.beat, 
                            e["mute"],
                            e["clapType"],
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pan, e["pan"], e["panSemitone"], true),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa, e["pa"], e["paSemitone"], true),
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Don, e["don"], e["donSemitone"], false),
                            new BonOdori.PresetVoiceline((int)BonOdori.DonType.Do, e["do"], e["doSemitone"], false)
                        );
                        BonOdori.instance.CuePanpaDondo(
                            e.beat,
                            new BonOdori.PresetInput(e["clapType"]),
                            new BonOdori.PresetInput(e["clapType"])
                        ); 
                    },
                    defaultLength = 2f,
                    parameters = ParamGenerator.ProduceParamList(
                        new List<ParamGenerator.SampleParams> {
                            new ParamGenerator.SampleParams("pan", BonOdori.PanVariation.BonDancePanC, "Pan"),
                            new ParamGenerator.SampleParams("pa", BonOdori.PaVariation.BonDancePaC, "Pa"),
                            new ParamGenerator.SampleParams("don", BonOdori.DonVariation.BonDanceDonA, "Don"),
                            new ParamGenerator.SampleParams("do", BonOdori.DoVariation.BonDanceDoG, "Do"),
                        },
                        BonOdori.ClapType.SideClap
                    ),
                },

                new GameAction("panPanpaPa", "Pan Panpa Pa")
                {
                    preFunction = delegate { 
                        var e = eventCaller.currentEntity;
                        BonOdori.PanPanpaPaSfx(
                            e.beat, 
                            e["mute"],
                            e["clapType"],
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa_n, e["pa_n1"], e["pa_n1Semitone"], true),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pan, e["pan"], e["panSemitone"], true),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa, e["pa"], e["paSemitone"], true),
                            new BonOdori.PresetVoiceline((int)BonOdori.PanType.Pa_n, e["pa_n2"], e["pa_n2Semitone"], true)
                        );
                        BonOdori.instance.CuePanPanpaPa(
                            e.beat,
                            new BonOdori.PresetInput(e["clapType"]),
                            new BonOdori.PresetInput(e["clapType"]),
                            new BonOdori.PresetInput(e["clapType"]),
                            new BonOdori.PresetInput(e["clapType"])
                        ); 
                    },
                    defaultLength = 4f,
                    parameters = ParamGenerator.ProduceParamList(
                        new List<ParamGenerator.SampleParams> {
                            new ParamGenerator.SampleParams("pa_n1", BonOdori.Pa_nVariation.BonDancePa_nA, "Pan 1"),
                            new ParamGenerator.SampleParams("pan", BonOdori.PanVariation.BonDancePanC, "Pan"),
                            new ParamGenerator.SampleParams("pa", BonOdori.PaVariation.BonDancePaC, "Pa"),
                            new ParamGenerator.SampleParams("pa_n2", BonOdori.Pa_nVariation.BonDancePa_nC, "Pan 2"),
                        },
                        BonOdori.ClapType.FrontClap
                    ),
                },

                new GameAction("successReaction", "Success Reaction")
                {   
                    function = delegate { var e = eventCaller.currentEntity; BonOdori.instance.SuccessReaction(e.beat, e.length, (BonOdori.ReactionType)e["reactionType"], e["yaguraSuccess"]);},
                    defaultLength = 2f,
                    resizable = true,
                    parameters = new List<Param>()
                    {  
                        new Param("reactionType", BonOdori.ReactionType.FaceOnly, "Reaction Type", "Set the reaction during a success"),
                        new Param("yaguraSuccess", false, "Yagura Success Smile", "Does Yagura-Chan react during a success")
                    }
                },

                new GameAction("displayText", "Display Text")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity;
                        float[] textPositions = {e["horizontalPos1"], e["horizontalPos2"], e["horizontalPos3"], e["horizontalPos4"], e["horizontalPos5"]};
                        BonOdori.instance.DisplayText(e.beat, e.length, e["userText"], (BonOdori.TextPreset)e["textPreset"], e["presetLyricColor"], e["presetDonColor"], e["presetPanColor"], e["fontType"], e["whichLine"], textPositions, e["isGradient"], e["outlineColor"]);},
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {   
                        new Param("textPreset", BonOdori.TextPreset.CustomText, "Text Preset", "Select the text preset.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x == (int)BonOdori.TextPreset.CustomText, new string[] { "userText", "presetLyricColor" }),
                        }),
                        new Param("userText", BonOdori.DEFAULT_DISPLAY_TEXT, "Text", "Set the text for the specified line."),
                        new Param("presetLyricColor", "#F01300".Hex2RGB(), "Lyric Color", "Set the preset lyric text color. This replaces {lyricColor}."),
                        new Param("presetDonColor", "#46FC00".Hex2RGB(), "Don Color", "Set the preset Don text color. This replaces {donColor}."),
                        new Param("presetPanColor", "#FBFF07".Hex2RGB(), "Pan Color", "Set the preset Pan text color. This replaces {panColor}."),
                        new Param("fontType", BonOdori.FontType.BonOdoriRodin, "Font", "Set the font of this text line"),
                        new Param("whichLine", BonOdori.WhichLine.Line1, "Line #", "Set which line this text will appear on", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x == (int)BonOdori.WhichLine.Line1, new string[] { "horizontalPos1" }),
                            new Param.CollapseParam((x, _) => (int)x == (int)BonOdori.WhichLine.Line2, new string[] { "horizontalPos2" }),
                            new Param.CollapseParam((x, _) => (int)x == (int)BonOdori.WhichLine.Line3, new string[] { "horizontalPos3" }),
                            new Param.CollapseParam((x, _) => (int)x == (int)BonOdori.WhichLine.Line4, new string[] { "horizontalPos4" }),
                            new Param.CollapseParam((x, _) => (int)x == (int)BonOdori.WhichLine.Line5, new string[] { "horizontalPos5" }),
                        }),
                        new Param("horizontalPos1", new EntityTypes.Float(0f, 1f, Mathf.InverseLerp(60f, 78f, BonOdori.DEFAULT_TEXT_POS[BonOdori.WhichLine.Line1])), "Line 1 Position", "Adjust the horizontal position of this line"),
                        new Param("horizontalPos2", new EntityTypes.Float(0f, 1f, Mathf.InverseLerp(60f, 78f, BonOdori.DEFAULT_TEXT_POS[BonOdori.WhichLine.Line2])), "Line 2 Position", "Adjust the horizontal position of this line"),
                        new Param("horizontalPos3", new EntityTypes.Float(0f, 1f, Mathf.InverseLerp(60f, 78f, BonOdori.DEFAULT_TEXT_POS[BonOdori.WhichLine.Line3])), "Line 3 Position", "Adjust the horizontal position of this line"),
                        new Param("horizontalPos4", new EntityTypes.Float(0f, 1f, Mathf.InverseLerp(60f, 78f, BonOdori.DEFAULT_TEXT_POS[BonOdori.WhichLine.Line4])), "Line 4 Position", "Adjust the horizontal position of this line"),
                        new Param("horizontalPos5", new EntityTypes.Float(0f, 1f, Mathf.InverseLerp(60f, 78f, BonOdori.DEFAULT_TEXT_POS[BonOdori.WhichLine.Line5])), "Line 5 Position", "Adjust the horizontal position of this line"),
                        new Param("isGradient", true, "Text Gradient", "Will there be a gradient for this text"),
                        new Param("outlineColor", new Color(0f, 0f, 255f), "Scroll Color", "Set the scrolled text color"),
                    },
                    priority = 1
                },
                
                new GameAction("scrollText", "Scroll Text")
                {
                    function = delegate { var e = eventCaller.currentEntity; BonOdori.instance.ValidateScroll(e.beat, e.length, e["whichLine"], (Util.EasingFunction.Ease)e["ease"], e["scrollAmt"]);},
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("whichLine", BonOdori.WhichLine.Line1, "Line #", "Set which line will be scrolled"),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease"),
                        new Param("scrollAmt", new EntityTypes.Float(0.01f, 1f, 1f), "Scroll Amount", "What % of the text will be scrolled"),
                    },
                },
                
                new GameAction("bow", "Bow")
                {
                    function = delegate { var e = eventCaller.currentEntity; BonOdori.instance.Bow(e.beat, e.length);},
                    defaultLength = 2f,
                    resizable = true,
                },

                new GameAction("spin", "Spin")
                {
                    function = delegate { var e = eventCaller.currentEntity; BonOdori.instance.Spin(e.beat);},
                    defaultLength = 0.5f,
                },

                new GameAction("overlay", "Darken Screen")
                {
                    function = delegate { var e = eventCaller.currentEntity; BonOdori.instance.DarkenScreen(e.beat, e.length, e["colorStart"], e["colorEnd"], e["ease"]);},
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("colorStart", Color.clear, "Start Color", "Set the overlay color at the start of the event."),
                        new Param("colorEnd", new Color(0f, 0f, 0f, 0.4666f), "End Color", "Set the overlay color at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    }
                },

                new GameAction("donpanColors", "Donpan Colors")
                {
                    function = delegate 
                    { 
                        var e = eventCaller.currentEntity;
                        BonOdori.instance.SetDonpansColors(
                            new BonOdori.DonpanColors(e["dp1c1"], e["dp1c2"], e["dp1c3"]),
                            new BonOdori.DonpanColors(e["dp2c1"], e["dp2c2"], e["dp2c3"]),
                            new BonOdori.DonpanColors(e["dp3c1"], e["dp3c2"], e["dp3c3"]),
                            new BonOdori.DonpanColors(e["dp4c1"], e["dp4c2"], e["dp4c3"])
                        );
                    },
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {   
                        new Param("whichDonpan", BonOdori.WhichDonpan.Donpan1, "Donpan", "Which Donpan to set the color of.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x == 0, new string[] { "dp1c1", "dp1c2", "dp1c3" }),
                            new Param.CollapseParam((x, _) => (int)x == 1, new string[] { "dp2c1", "dp2c2", "dp2c3" }),
                            new Param.CollapseParam((x, _) => (int)x == 2, new string[] { "dp3c1", "dp3c2", "dp3c3" }),
                            new Param.CollapseParam((x, _) => (int)x == 3, new string[] { "dp4c1", "dp4c2", "dp4c3" }),
                        }),
                        new Param("dp1c1", BonOdori.dp1ColorLookup[0].Hex2RGB(), "Kimono", "Set the color of this attribute"),
                        new Param("dp1c2", BonOdori.dp1ColorLookup[1].Hex2RGB(), "Flower Petals", "Set the color of this attribute"),
                        new Param("dp1c3", BonOdori.dp1ColorLookup[2].Hex2RGB(), "Flower Pistil", "Set the color of this attribute"),
                        new Param("dp2c1", BonOdori.dp2ColorLookup[0].Hex2RGB(), "Kimono", "Set the color of this attribute"),
                        new Param("dp2c2", BonOdori.dp2ColorLookup[1].Hex2RGB(), "Flower Petals", "Set the color of this attribute"),
                        new Param("dp2c3", BonOdori.dp2ColorLookup[2].Hex2RGB(), "Flower Pistil", "Set the color of this attribute"),
                        new Param("dp3c1", BonOdori.dp3ColorLookup[0].Hex2RGB(), "Kimono", "Set the color of this attribute"),
                        new Param("dp3c2", BonOdori.dp3ColorLookup[1].Hex2RGB(), "Flower Petals", "Set the color of this attribute"),
                        new Param("dp3c3", BonOdori.dp3ColorLookup[2].Hex2RGB(), "Flower Pistil", "Set the color of this attribute"),
                        new Param("dp4c1", BonOdori.dp4ColorLookup[0].Hex2RGB(), "Kimono", "Set the color of this attribute"),
                        new Param("dp4c2", BonOdori.dp4ColorLookup[1].Hex2RGB(), "Flower Petals", "Set the color of this attribute"),
                        new Param("dp4c3", BonOdori.dp4ColorLookup[2].Hex2RGB(), "Flower Pistil", "Set the color of this attribute"),
                    },
                },
            },
            new List<string>() { "agb", "normal" }, "agbbonodori", "en", new List<string>() { },
            chronologicalSortKey: 10,
            font : "KanTeiRyu"
            );
        }
    };
};

namespace HeavenStudio.Games
{
    using Scripts_BonOdori;
    public class BonOdori : Minigame
    {
        #region Class Properties

        public SpriteRenderer darkPlane;
        public const string DEFAULT_DISPLAY_TEXT = "Lyrics: Type {lyricColor}\nDon: Type {donColor}\nPan: Type {panColor}";

        public struct DonpanColors
        {
            public Color kimono;
            public Color petals;
            public Color pistil;

            public DonpanColors(Color kimono, Color petals, Color pistil)
            {
                this.kimono = kimono;
                this.petals = petals;
                this.pistil  = pistil;
            }
        }

        public struct PresetVoiceline
        {
            public int voicelineType;
            public int voicelineTypeVariation;
            public int semitone;
            public bool isPan;
            public double? beatToPlayAt;

            public PresetVoiceline(int voicelineType, int voicelineTypeVariation, int semitone, bool isPan)
            {
                this.voicelineType = voicelineType;
                this.voicelineTypeVariation = voicelineTypeVariation;
                this.semitone = semitone;
                this.isPan = isPan;
                this.beatToPlayAt = null;
            }
        }

        public struct PresetInput
        {
            public int clapType;
            public double? beatToPlayAt;

            public PresetInput(int clapType)
            {
                this.clapType = clapType;
                this.beatToPlayAt = null;
            }
        }

        public static readonly Dictionary<TextPreset, string> PRESET_TEXTS = new Dictionary<TextPreset, string> 
        { 
            { TextPreset.CustomText, DEFAULT_DISPLAY_TEXT},
            { TextPreset.Do_nDo_nPa_nPa_n, "{donColor}Do-n do-n {panColor}pa-n pa-n" },
            { TextPreset.DondoPa_nPan, "{donColor}Dondo {panColor}pa-n pan" },
            { TextPreset.DondoPanpaDondoPanpa, "{donColor}Dondo {panColor}panpa {donColor}dondo {panColor}panpa" },
            { TextPreset.DondoPanpa, "{donColor}Dondo {panColor}panpa" },
            { TextPreset.Do_nPa_nPan, "{donColor}Do-n {panColor}pa-n pan" },
            { TextPreset.Do_nDo_nPan, "{donColor}Do-n do-n {panColor}pan" },
            { TextPreset.PanPan, "{panColor}Pan pan" },
            { TextPreset.DonDoPanPa, "{donColor}Don do {panColor}pan pa" },
            { TextPreset.DondoPanPa, "{donColor}Dondo {panColor}pan pa" },
            { TextPreset.PanPaDondoDo, "{panColor}Pan pa {donColor}dondo do" },
            { TextPreset.PanpaDoPa, "{panColor}Panpa {donColor}do {panColor}pa" },
            { TextPreset.DondoPanpaPa, "{donColor}Dondo {panColor}panpa pa" },
            { TextPreset.PanpaDondo, "{panColor}Panpa {donColor}dondo" },
            { TextPreset.PanPanpaPa, "{panColor}Pan panpa pa" },
            { TextPreset.Pan, "{panColor}Pan" },
        };

        //gets normalized between 60f-78f...
        public static readonly Dictionary<WhichLine, float> DEFAULT_TEXT_POS = new Dictionary<WhichLine, float>
        {
            { WhichLine.Line1, 62.84f },
            { WhichLine.Line2, 64.65f },
            { WhichLine.Line3, 66.21f },
            { WhichLine.Line4, 67.75f },
            { WhichLine.Line5, 69.19f }
        };

        public static BonOdori instance;
        public static Conductor cond;

        public static double WantDonDonPanPan = double.MinValue;
        public static double WantDondoPanPan = double.MinValue;
        public static double WantDondoPanpa = double.MinValue;
        public static double WantDonPanPan = double.MinValue;
        public static double WantDonDonPan = double.MinValue;
        public static double WantDondoPanpaPa = double.MinValue;
        public static int CurrClapAnim = 0; //fallback val | aka sideclap
        public static bool IsCpuDonpansBop { get; private set; } = true;

        private static readonly HashSet<string> _INVISIBLE_TEXTS = new HashSet<string> 
        { 
            "",
            DEFAULT_DISPLAY_TEXT
        };

        private const float _DEFAULT_OUTLINE_WIDTH = 0.4f;
        private const float _DEFAULT_UNDERLAY_WIDTH = 0.85f;
        private const float _DEFAULT_TINT_OPACITY = 0.2f;

        private bool _isPlayerBop = true;
        private bool _isYaguraBop = true;
        private bool _hasMissed = false;
        private float?[] _lastMaskPositions = new float?[5];
        private Color _skinColor = "#FADDDE".Hex2RGB();
        private Color _defaultScrollOutlineColor = "#0000FF".Hex2RGB();
        private Coroutine[] _scrollCoroutines = new Coroutine[5];
        private ColorEase _overlayColorEase = new(Color.clear);
        private Material _scrolledFontMaterial;
        private Donpan _playerDonpan;

        [SerializeField] private TMP_Text[] _regTexts;
        [SerializeField] private TMP_Text[] _scrolledTexts;
        [SerializeField] private List<TMP_FontAsset> _availableFonts; 
        [SerializeField] private List<TMP_FontAsset> _underlayFontsList; //Fonts that use underlays as their outline
        [SerializeField] private Texture2D _gradientTexture;
        [SerializeField] private Shader _customScrollShader;
        [SerializeField] private YaguraChan _yaguraChan;
        [SerializeField] private List<Donpan> _allDonpans = new List<Donpan>();
        [SerializeField] private List<Material> _donpanRecolorMats = new List<Material>();
        [SerializeField] private List<Material> _donpanRecolorFootMats = new List<Material>();

        public enum WhoBops
        {
            Both = 0,
            Donpans = 1,
            YaguraChan = 2,
            None = 3
        }

        public enum WhichLine
        {
            Line1 = 0,
            Line2 = 1,
            Line3 = 2,
            Line4 = 3,
            Line5 = 4
        }

        public enum TextPreset
        {
            CustomText = 0,
            Do_nDo_nPa_nPa_n = 1,
            DondoPa_nPan = 2,
            DondoPanpaDondoPanpa = 3,
            DondoPanpa = 4,
            Do_nPa_nPan = 5,
            Do_nDo_nPan = 6,
            PanPan = 7,
            DonDoPanPa = 8,
            DondoPanPa = 9,
            PanPaDondoDo = 10,
            PanpaDoPa = 11,
            DondoPanpaPa = 12,
            PanpaDondo = 13,
            PanPanpaPa = 14,
            Pan = 15,
        }

        public enum FontType
        {
            BonOdoriRodin = 0,
            WarioWare = 1,
            Slump = 2,
            Kurokane = 3,
            Comet = 4,
            Kanteiryu = 5,
            ComicSans = 6
        }

        public enum ReactionType
        {
            FaceOnly = 0,
            FaceAndArms = 1
        }

        public enum WhichDonpan
        {
            Donpan1 = 0,
            Donpan2 = 1,
            Donpan3 = 2,
            PlayerDonpan = 3
        }

        public enum ClapType
        {
            SideClap = 0,
            FrontClap = 1
        }

        public enum PanType
        {
            Pan = 0,
            Pa = 1,
            Pa_n = 2
        }

        public enum DonType
        {
            Don = 0,
            Do = 1,
            Do_n = 2
        }

        public enum PanVariation
        {
            PanC = 0,
            PanE = 1,
            PanA = 2,
            BonDancePanG = 3,
            BonDancePanD = 4,
            BonDancePanC = 5,
            BonDancePanA = 6
        }
        public enum Pa_nVariation
        {
            Pa_nA = 0,
            Pa_nC = 1,
            BonDancePa_nG = 2,
            BonDancePa_nA = 3,
            BonDancePa_nC = 4,
            BonDancePa_nE = 5
        }
        public enum PaVariation
        {
            PaG = 0,
            BonDancePaG = 1,
            BonDancePaD = 2,
            BonDancePaC = 3
        }
        public enum DonVariation
        {
            DonA = 0,
            DonD = 1,
            DonC = 2,
            DonG = 3,
            BonDanceDonC = 4,
            BonDanceDonD = 5,
            BonDanceDonA = 6
        }
        public enum Do_nVariation
        {
            Do_nA = 0,
            Do_nG = 1,
            BonDanceDo_nA = 2
        }
        public enum DoVariation
        {
            DoC = 0,
            DoG = 1,
            BonDanceDoC = 2,
            BonDanceDoD = 3,
            BonDanceDoG = 4
        }

        public static readonly Dictionary<object, NoteSample> NoteSamples = new() 
        {
            { PanVariation.PanC, new("bonOdori/pan1", 3, 4) }, // (file path, key, volume?)
            { PanVariation.PanE, new("bonOdori/pan2", 7, 4) },
            { PanVariation.PanA, new("bonOdori/pan3", 0, 4) },
            { PanVariation.BonDancePanG, new("bonOdori/pan4", 7, 4) },
            { PanVariation.BonDancePanD, new("bonOdori/pan5", 5, 4) },
            { PanVariation.BonDancePanC, new("bonOdori/pan6", 3, 4) },
            { PanVariation.BonDancePanA, new("bonOdori/pan7", 0, 4) },
            { Pa_nVariation.Pa_nA, new("bonOdori/pa_n1", 0, 4) },
            { Pa_nVariation.Pa_nC, new("bonOdori/pa_n2", 3, 4) },
            { Pa_nVariation.BonDancePa_nG, new("bonOdori/pa_n3", 7, 4) },
            { Pa_nVariation.BonDancePa_nA, new("bonOdori/pa_n4", 0, 4) },
            { Pa_nVariation.BonDancePa_nC, new("bonOdori/pa_n5", 3, 4) },
            { Pa_nVariation.BonDancePa_nE, new("bonOdori/pa_n6", 7, 4) },
            { PaVariation.PaG, new("bonOdori/pa1", 7, 4) },
            { PaVariation.BonDancePaG, new("bonOdori/pa2", 7, 4) },
            { PaVariation.BonDancePaD, new("bonOdori/pa3", 5, 4) },
            { PaVariation.BonDancePaC, new("bonOdori/pa4", 3, 4) },
            { DonVariation.DonA, new("bonOdori/don1", 0, 4) },
            { DonVariation.DonD, new("bonOdori/don2", 5, 4) },
            { DonVariation.DonC, new("bonOdori/don3", 3, 4) },
            { DonVariation.DonG, new("bonOdori/don4", 10, 4) },
            { DonVariation.BonDanceDonC, new("bonOdori/don5", 3, 4) },
            { DonVariation.BonDanceDonD, new("bonOdori/don6", 5, 4) },
            { DonVariation.BonDanceDonA, new("bonOdori/don7", 0, 4) },
            { Do_nVariation.Do_nA, new("bonOdori/do_n1", 0, 4) },
            { Do_nVariation.Do_nG, new("bonOdori/do_n2", 7, 4) },
            { Do_nVariation.BonDanceDo_nA, new("bonOdori/do_n3", 0, 4) },
            { DoVariation.DoC, new("bonOdori/do1", 3, 4) },
            { DoVariation.DoG, new("bonOdori/do2", 7, 4) },
            { DoVariation.BonDanceDoC, new("bonOdori/do3", 3, 4) },
            { DoVariation.BonDanceDoD, new("bonOdori/do4", 5, 4) },
            { DoVariation.BonDanceDoG, new("bonOdori/do5", 7, 4) }
        };

        public static readonly Dictionary<int, string> dp1ColorLookup = new() {
            { 0, "#00C5FF" }, //Kimono :R
            { 1, "#FA81B9" }, //Petals :G
            { 2, "#F8D8D4" }, //Pistil :B
        };

        public static readonly Dictionary<int, string> dp2ColorLookup = new() {
            { 0, "#FFAD00" }, //Kimono :R
            { 1, "#6F57F7" }, //Petals :G
            { 2, "#D2C0A9" }, //Pistil :B
        };

        public static readonly Dictionary<int, string> dp3ColorLookup = new() {
            { 0, "#FFD200" }, //Kimono :R
            { 1, "#66ACDB" }, //Petals :G
            { 2, "#DCECFA" }, //Pistil :B
        };

        public static readonly Dictionary<int, string> dp4ColorLookup = new() {
            { 0, "#E60013" }, //Kimono :R
            { 1, "#FF316B" }, //Petals :G
            { 2, "#FADCDE" }, //Pistil :B
        };

        #endregion

        public static DonpanColors ConvertColorDictToStruct(Dictionary<int, string> colorDict)
        {
            return new DonpanColors(
                colorDict[0].Hex2RGB(),
                colorDict[1].Hex2RGB(),
                colorDict[2].Hex2RGB()
            );
        }

        public static NoteSample GetSample(object sampleEnum)
        {
            return NoteSamples[sampleEnum];
        }

        public void Awake()
        {
            instance = this;
            cond = Conductor.instance;
            _playerDonpan = _allDonpans[3];
            _playerDonpan.isPlayer = true;
            _initDonpansColors();
            _initScrollColors();
            SetupBopRegion("bonOdori", "bop", "auto", false);
        }

        public override void OnGameSwitch(double beat)
        {
            _hasMissed = false; 
            _restoreBopStates(beat);
            _donpanNeutral();
            _resetAllFaces();
            _resetScrolls();
            _resetTexts();
            _resetDarkScreen();
            _persistDonpanColors(beat);

            if (WantDonDonPanPan != double.MinValue)
            {
                CueDonDonPanPan(WantDonDonPanPan, new PresetInput(CurrClapAnim), new PresetInput(CurrClapAnim));
                WantDonDonPanPan = double.MinValue;
            }
            if (WantDondoPanPan != double.MinValue)
            {
                CueDondoPanPan(WantDondoPanPan, new PresetInput(CurrClapAnim), new PresetInput(CurrClapAnim));
                WantDondoPanPan = double.MinValue;
            }
            if (WantDondoPanpa != double.MinValue)
            {
                CueDondoPanpa(WantDondoPanpa, new PresetInput(CurrClapAnim), new PresetInput(CurrClapAnim));
                WantDondoPanpa = double.MinValue;
            }
            if (WantDonPanPan != double.MinValue)
            {
                CueDonPanPan(WantDonPanPan, new PresetInput(CurrClapAnim), new PresetInput(CurrClapAnim));
                WantDonPanPan = double.MinValue;
            }
            if (WantDonDonPan != double.MinValue)
            {
                CueDonDonPan(WantDonDonPan, new PresetInput(CurrClapAnim));
                WantDonDonPan = double.MinValue;
            }
            if (WantDondoPanpaPa != double.MinValue)
            {
                CueDondoPanpaPa(WantDondoPanpaPa, new PresetInput(CurrClapAnim), new PresetInput(CurrClapAnim), new PresetInput(CurrClapAnim));
                WantDondoPanpaPa = double.MinValue;
            }
        }

        //TODO: reduce redundancy between _resetScrolls, _resetTexts, and _persistTextKill
        public override void OnPlay(double beat)
        {
            _hasMissed = false;
            _restoreBopStates(beat);
            _donpanNeutral();
            _resetAllFaces();
            _resetScrolls();
            _persistTextStates(beat);
            _persistScrollStates(beat);
            _persistTextKill(beat);
            _persistDarkScreenState(beat);
            _persistDonpanColors(beat);
        }

        public override void OnLateBeatPulse(double beat)
        {
            if (IsCpuDonpansBop) _cpuDonpansBop();
            if (_isPlayerBop) _playerDonpan.Bop();
            if (_isYaguraBop) _yaguraChan.Bop();
        }

        public void Update()
        {
            if (!cond.isPlaying || cond.isPaused) return;

            _colorUpdate();

            if (!cond.NotStopped())
            {
                for (int i = 0; i < _scrollCoroutines.Length; i++)
                {
                    if (_scrollCoroutines[i] != null) _killScrollCoroutine(i);
                    _regTexts[i].text = "";
                    _scrolledTexts[i].text = "";
                    _scrolledTexts[i].GetComponent<TextMeshPro>().SetMask(0, new Vector4(-10f, -10f, -10f, 10));
                }
            }

            if (PlayerInput.GetIsAction(BonOdori.InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            {
                List<BeatAction.Action> bopActions = new();
                var currBeat = Conductor.instance.songPositionInBeatsAsDouble;
                _playerDonpan.Clap("ClapFront");
                _playerDonpan.PlayClapSfx(1f);
                ScoreMiss(0f); //kill the perfect on unwanted input but no score punish
                _isPlayerBop = false;

                bopActions.Add(new BeatAction.Action(currBeat + 2, delegate {
                    _isPlayerBop = true;
                }));    

                BeatAction.New(this, bopActions);
            }
        }

        #region Start/Cleanup Methods

        private void _initScrollColors()
        {
            for (int i = 0; i < _scrolledTexts.Length; i++)
            {
                _setScrollOutlineColor(i, _defaultScrollOutlineColor);
            }
        }

        public void _initDonpansColors()
        {
            SetDonpansColors(
                ConvertColorDictToStruct(dp1ColorLookup),
                ConvertColorDictToStruct(dp2ColorLookup),
                ConvertColorDictToStruct(dp3ColorLookup),
                ConvertColorDictToStruct(dp4ColorLookup)
            );
        }

        private void _setScrollOutlineColor(int lineNum, Color outlineColor)
        {
            _scrolledTexts[lineNum].fontMaterial.SetColor("_OutlineColor", outlineColor);
            _scrolledTexts[lineNum].fontMaterial.SetColor("_UnderlayColor", outlineColor);//just for Chicken Rodin afaik
        }

        private void _donpanNeutral()
        {
            foreach (var donpan in _allDonpans)
            {
                donpan.ResetToNeutral();
            }
        }

        private void _restoreBopStates(double beat)
        {
            var lastBopEvent = gameManager.Beatmap.Entities.FindLast(e => e.datamodel == "bonOdori/bop" && e.beat <= beat);
            if (lastBopEvent != null)
            {
                IsCpuDonpansBop = ((WhoBops)lastBopEvent["auto"] == WhoBops.Donpans) || ((WhoBops)lastBopEvent["auto"] == WhoBops.Both);
                _isPlayerBop = ((WhoBops)lastBopEvent["auto"] == WhoBops.Donpans) || ((WhoBops)lastBopEvent["auto"] == WhoBops.Both);
                _isYaguraBop = ((WhoBops)lastBopEvent["auto"] == WhoBops.YaguraChan) || ((WhoBops)lastBopEvent["auto"] == WhoBops.Both);
            }
            else
            {
                IsCpuDonpansBop = true;
                _isPlayerBop = true;
                _isYaguraBop = true;
            }
        }

        private void _resetAllFaces()
        {
            _yaguraChan.SetYaguraFace("Neutral");
            for (int i = 0; i < _allDonpans.Count - 1; i++)
            {
                _allDonpans[i].ResetFace(i);
            }
        }

        private void _resetTexts()
        {
            for (int i = 0; i < _regTexts.Length; i++)
            {
                _regTexts[i].text = "";
                _scrolledTexts[i].text = "";

                if (_scrollCoroutines[i] != null) _killScrollCoroutine(i);

                _scrolledTexts[i].GetComponent<TextMeshPro>().SetMask(0, new Vector4(-10f, -10f, -10f, 10f));
            }
        }

        private void _resetScrolls()
        {
            for (int i = 0; i < _lastMaskPositions.Length; i ++)
            {
                _lastMaskPositions[i] = null;
            }
        }

        private void _persistTextStates(double beat)
        {
            _resetTexts();
            var activeTextEvents = gameManager.Beatmap.Entities.FindAll(e => e.datamodel == "bonOdori/displayText" && e.beat <= beat && e.beat + e.length > beat);

            foreach (var textEvent in activeTextEvents)
            {
                int lineNum = textEvent["whichLine"];
                TextPreset textPreset = (TextPreset)textEvent["textPreset"];
                string userText = textEvent["userText"];

                var regTextTMP = _regTexts[lineNum];
                var scrollTMP = _scrolledTexts[lineNum];
                float[] textPositions = { textEvent["horizontalPos1"], textEvent["horizontalPos2"], textEvent["horizontalPos3"], textEvent["horizontalPos4"], textEvent["horizontalPos5"] };

                regTextTMP.font = _availableFonts[textEvent["fontType"]];
                scrollTMP.font = _availableFonts[textEvent["fontType"]];

                if (textPreset != TextPreset.CustomText && PRESET_TEXTS.ContainsKey(textPreset))
                {
                    userText = PRESET_TEXTS[textPreset];
                    userText = _setPresetColors(userText, textEvent["presetLyricColor"], textEvent["presetDonColor"], textEvent["presetPanColor"]);
                }

                _moveTextPosition(regTextTMP, scrollTMP, textPositions[lineNum]);
                _setOutline(regTextTMP.fontMaterial, regTextTMP.font);

                if (!_INVISIBLE_TEXTS.Contains(userText))
                {   
                    _replaceFontMaterials(scrollTMP, textEvent["outlineColor"]);
                    _setGradientTexture(regTextTMP, textEvent["isGradient"]);
                    _setGradientTexture(scrollTMP, textEvent["isGradient"]);
                    _setScrollOutlineColor(lineNum, textEvent["outlineColor"]);
                    regTextTMP.text = userText;
                    scrollTMP.text = userText;
                }
            }
        }

        private void _persistTextKill(double beat)//kill all text-related events if starting midway
        {
            var activeTextEvents = gameManager.Beatmap.Entities.FindAll(e => 
                e.datamodel == "bonOdori/displayText" && 
                e.beat <= beat && 
                e.beat + e.length > beat);

            foreach (var textEvent in activeTextEvents)
            {
                WhichLine lineNum = (WhichLine)textEvent["whichLine"];
                int lineIndex = (int)lineNum;
                double killBeat = textEvent.beat + textEvent.length;
                
                BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(killBeat, delegate
                    {
                        _regTexts[lineIndex].text = "";
                        _scrolledTexts[lineIndex].text = "";
                        _setGradientTexture(_regTexts[lineIndex], false);
                        _setGradientTexture(_scrolledTexts[lineIndex], false);
                        
                        if (_scrollCoroutines[lineIndex] != null) _killScrollCoroutine(lineIndex);

                        _scrolledTexts[lineIndex].GetComponent<TextMeshPro>().SetMask(0, new Vector4(-10f, -10f, -10f, 10f));
                    })
                });
            }
        }

        private void _persistScrollStates(double beat)
        {
            for (int lineIndex = 0; lineIndex < _scrolledTexts.Length; lineIndex++)
            {
                WhichLine lineNum = (WhichLine)lineIndex;
                
                var activeTextEvent = gameManager.Beatmap.Entities.Find(e => 
                    e.datamodel == "bonOdori/displayText" && 
                    (WhichLine)e["whichLine"] == lineNum &&
                    e.beat <= beat && 
                    e.beat + e.length > beat);
                    
                if (activeTextEvent == null) continue;
                
                var scrollEvents = gameManager.Beatmap.Entities.FindAll(e =>
                    e.datamodel == "bonOdori/scrollText" &&
                    (WhichLine)e["whichLine"] == lineNum &&
                    e.beat >= activeTextEvent.beat &&
                    e.beat < beat && //only events that have already started
                    e.beat + e.length > activeTextEvent.beat && //must overlap with text event
                    e.beat + e.length <= activeTextEvent.beat + activeTextEvent.length); //must be within displaty text bounds
                    
                if (scrollEvents.Count == 0) continue;
                
                var scrollTextTMP = _scrolledTexts[lineIndex].GetComponent<TextMeshPro>();
                float scrollWidth = scrollTextTMP.preferredWidth;
                float absoluteStartPos = -scrollTextTMP.rectTransform.sizeDelta.x / 2f;
                float absoluteEndPos = absoluteStartPos + scrollWidth;
                float currScrollPos = absoluteStartPos;
                
                foreach (var scrollEvent in scrollEvents.OrderBy(e => e.beat))//calculate the last valid scroll position
                {
                    float scrollAmt = scrollEvent["scrollAmt"];
                    float targetScrollPos = absoluteStartPos + (scrollWidth * scrollAmt);
                    currScrollPos = Mathf.Clamp(targetScrollPos, currScrollPos, absoluteEndPos);//calculate valid values; no going backwards
                }
                
                if (currScrollPos > absoluteStartPos)
                {
                    scrollTextTMP.SetMask(0, new Vector4(-10f, -10f, currScrollPos, 10f));

                    if (currScrollPos >= absoluteEndPos) _lastMaskPositions[lineIndex] = null;
                    else _lastMaskPositions[lineIndex] = currScrollPos;
                }
            }
        }

        private void _persistDarkScreenState(double beat)//i rewrote this one to be more efficient; take that copy pasters!
        {
            var lastEvent = gameManager.Beatmap.Entities.FindLast(e => e.datamodel == "bonOdori/overlay" && e.beat < beat);
            if (lastEvent != null)
            {
                DarkenScreen(lastEvent.beat, lastEvent.length, lastEvent["colorStart"], lastEvent["colorEnd"], lastEvent["ease"]);
            }
        }

        private void _persistDonpanColors(double beat)
        {
            var lastColorEvent = gameManager.Beatmap.Entities.FindLast(e => e.datamodel == "bonOdori/donpanColors" && e.beat <= beat);
            if (lastColorEvent != null)
            {
                SetDonpansColors(
                    new DonpanColors(lastColorEvent["dp1c1"], lastColorEvent["dp1c2"], lastColorEvent["dp1c3"]),
                    new DonpanColors(lastColorEvent["dp2c1"], lastColorEvent["dp2c2"], lastColorEvent["dp2c3"]),
                    new DonpanColors(lastColorEvent["dp3c1"], lastColorEvent["dp3c2"], lastColorEvent["dp3c3"]),
                    new DonpanColors(lastColorEvent["dp4c1"], lastColorEvent["dp4c2"], lastColorEvent["dp4c3"])
                );
            }
            else _initDonpansColors();
        }

        private void _resetDarkScreen()
        {
            darkPlane.color = Color.clear;
        }

        #endregion

        #region Input State Methods

        public void Just(PlayerActionEvent caller, float state, string clapAnim)
        {
            if (state >= 1f || state <= -1f)
            {
                _playerDonpan.Clap(clapAnim);
                _playerDonpan.PlayClapSfx(1f, isBarely: true);
            }
            else 
            {
                _playerDonpan.Clap(clapAnim);
                _playerDonpan.PlayClapSfx(1f);
                for (int i = 0; i < _allDonpans.Count - 1; i++)
                {
                    _allDonpans[i].PlayClapSfx(0.75f);
                }
            }
        }

        public void Miss(PlayerActionEvent caller)
        {
            _hasMissed = true;
        }

        public void Empty(PlayerActionEvent caller) { }

        #endregion

        #region Bop

        public void Bop(double beat, float length, WhoBops who, WhoBops whoAuto)
        {
            IsCpuDonpansBop = (whoAuto == WhoBops.Donpans) || (whoAuto == WhoBops.Both);
            _isPlayerBop = (whoAuto == WhoBops.Donpans) || (whoAuto == WhoBops.Both);
            _isYaguraBop = (whoAuto == WhoBops.YaguraChan) || (whoAuto == WhoBops.Both);

            if (who == WhoBops.None) return;

            List<BeatAction.Action> bopActions = new List<BeatAction.Action>();

            for (int i = 0; i < length; i++)
            {
                bopActions.Add(new BeatAction.Action(beat + i, delegate
                {
                    switch (who)
                    {
                        case WhoBops.Both:
                            _cpuDonpansBop();
                            _playerDonpan.Bop();
                            _yaguraChan.Bop();
                            break;
                        case WhoBops.Donpans:
                            _cpuDonpansBop();
                            _playerDonpan.Bop();
                            break;
                        case WhoBops.YaguraChan:
                            _yaguraChan.Bop();
                            break;
                        default:
                            break;
                    }
                }));
            }

            if (bopActions.Count > 0) BeatAction.New(this, bopActions);
        }

        private void _cpuDonpansBop()
        {
            for (int i = 0; i < _allDonpans.Count - 1; i++)
            {
                _allDonpans[i].Bop();
            }
        }

        #endregion

        #region StandaloneDon

        public void StandaloneDonSfx(double beat, PresetVoiceline donVariation)
        {
            donVariation.beatToPlayAt = beat;
            _playSfx(false, new List<PresetVoiceline> {donVariation});
        }

        #endregion

        #region StandalonePan

        public void StandalonePanSfx(double beat, bool isMuted, PresetVoiceline panVariation)
        {
            panVariation.beatToPlayAt = beat;
            _playSfx(isMuted, new List<PresetVoiceline> {panVariation});
        }

        public void CueStandalonePan(double beat, PresetInput panVariation)
        {
            panVariation.beatToPlayAt = beat;
            _scheduleClaps(beat, 1, new List<PresetInput> {panVariation});
        }

        #endregion

        // the architecture for inactive vs active curing is a lil scuffed
        // todo: refactor to be cleaner/clearer

        #region DonDonPanPan

        public static void DonDonPanPanSfx(double beat, bool isMuted, int clapType, PresetVoiceline do_n1, PresetVoiceline do_n2, PresetVoiceline pan1, PresetVoiceline pan2, bool isInactiveFunc = false)
        {
            do_n1.beatToPlayAt = beat;
            do_n2.beatToPlayAt = beat + 1;
            pan1.beatToPlayAt = beat + 2;
            pan2.beatToPlayAt = beat + 3;
            _playSfx(isMuted, new List<PresetVoiceline> {do_n1, do_n2, pan1, pan2});
            if (isInactiveFunc)
            {
                CurrClapAnim = clapType;
                WantDonDonPanPan = beat;
            }
        }

        public void CueDonDonPanPan(double beat, PresetInput pan1, PresetInput pan2)
        {
            pan1.beatToPlayAt = beat + 2;
            pan2.beatToPlayAt = beat + 3;
            _scheduleClaps(beat, 4, new List<PresetInput> {pan1, pan2});
        }

        #endregion

        #region DondoPanPan

        public static void DondoPanPanSfx(double beat, bool isMuted, int clapType, PresetVoiceline don, PresetVoiceline do_, PresetVoiceline pa_n1, PresetVoiceline pa_n2, bool isInactiveFunc = false)
        {
            don.beatToPlayAt = beat;
            do_.beatToPlayAt = beat + 0.5;
            pa_n1.beatToPlayAt = beat + 1;
            pa_n2.beatToPlayAt = beat + 2;
            _playSfx(isMuted, new List<PresetVoiceline> {don, do_, pa_n1, pa_n2});
            if (isInactiveFunc)
            {
                CurrClapAnim = clapType;
                WantDondoPanPan = beat;
            }
        }

        public void CueDondoPanPan(double beat, PresetInput pa_n1, PresetInput pa_n2)
        {
            pa_n1.beatToPlayAt = beat + 1;
            pa_n2.beatToPlayAt = beat + 2;
            _scheduleClaps(beat, 4, new List<PresetInput> {pa_n1, pa_n2});
        }

        #endregion

        #region DondoPanpa

        public static void DondoPanpaSfx(double beat, bool isMuted, int clapType, PresetVoiceline don, PresetVoiceline do_, PresetVoiceline pa_n, PresetVoiceline pa, bool isInactiveFunc = false)
        {
            don.beatToPlayAt = beat;
            do_.beatToPlayAt = beat + 0.5;
            pa_n.beatToPlayAt = beat + 1;
            pa.beatToPlayAt = beat + 1.5;
            _playSfx(isMuted, new List<PresetVoiceline> {don, do_, pa_n, pa});
            if (isInactiveFunc)
            {
                CurrClapAnim = clapType;
                WantDondoPanpa = beat;
            }
        }

        public void CueDondoPanpa(double beat, PresetInput pa_n, PresetInput pa)
        {
            pa_n.beatToPlayAt = beat + 1;
            pa.beatToPlayAt = beat + 1.5;
            _scheduleClaps(beat, 2, new List<PresetInput> {pa_n, pa});
        }

        #endregion

        #region DonPanPan

        public static void DonPanPanSfx(double beat, bool isMuted, int clapType, PresetVoiceline do_n, PresetVoiceline pa_n1, PresetVoiceline pa_n2, bool isInactiveFunc = false)
        {
            do_n.beatToPlayAt = beat;
            pa_n1.beatToPlayAt = beat + 1;
            pa_n2.beatToPlayAt = beat + 2;
            _playSfx(isMuted, new List<PresetVoiceline> {do_n, pa_n1, pa_n2});
            if (isInactiveFunc)
            {
                CurrClapAnim = clapType;
                WantDonPanPan = beat;
            }
        }

        public void CueDonPanPan(double beat, PresetInput pa_n1, PresetInput pa_n2)
        {
            pa_n1.beatToPlayAt = beat + 1;
            pa_n2.beatToPlayAt = beat + 2;
            _scheduleClaps(beat, 4, new List<PresetInput> {pa_n1, pa_n2});
        }

        #endregion

        #region DonDonPan

        public static void DonDonPanSfx(double beat, bool isMuted, int clapType, PresetVoiceline don1, PresetVoiceline don2, PresetVoiceline pan, bool isInactiveFunc = false)
        {
            don1.beatToPlayAt = beat;
            don2.beatToPlayAt = beat + 1;
            pan.beatToPlayAt = beat + 2;
            _playSfx(isMuted, new List<PresetVoiceline> {don1, don2, pan});
            if (isInactiveFunc)
            {
                CurrClapAnim = clapType;
                WantDonDonPan = beat;
            }
        }

        public void CueDonDonPan(double beat, PresetInput pan)
        {
            pan.beatToPlayAt = beat + 2;
            _scheduleClaps(beat, 4, new List<PresetInput> {pan});
        }

        #endregion

        #region PanPaDondoDo

        public static void PanPaDondoDoSfx(double beat, bool isMuted, int clapType, PresetVoiceline pa_n1, PresetVoiceline pa_n2, PresetVoiceline don, PresetVoiceline do_, PresetVoiceline do_n, bool isInactiveFunc = false)
        {
            pa_n1.beatToPlayAt = beat;
            pa_n2.beatToPlayAt = beat + 1;
            don.beatToPlayAt = beat + 2;
            do_.beatToPlayAt = beat + 2.5;
            do_n.beatToPlayAt = beat + 3;
            _playSfx(isMuted, new List<PresetVoiceline> {pa_n1, pa_n2, don, do_, do_n});
            if (isInactiveFunc)
            {
                CurrClapAnim = clapType;
            }
        }

        public void CuePanPaDondoDo(double beat, PresetInput pa_n1, PresetInput pa_n2)
        {
            pa_n1.beatToPlayAt = beat;
            pa_n2.beatToPlayAt = beat + 1;
            _scheduleClaps(beat, 4, new List<PresetInput> {pa_n1, pa_n2});
        }

        #endregion

        #region PanpaDoPa

        public static void PanpaDoPaSfx(double beat, bool isMuted, int clapType, PresetVoiceline pan, PresetVoiceline pa, PresetVoiceline do_n, PresetVoiceline pa_n, bool isInactiveFunc = false)
        {
            pan.beatToPlayAt = beat;
            pa.beatToPlayAt = beat + 0.5;
            do_n.beatToPlayAt = beat + 1;
            pa_n.beatToPlayAt = beat + 2;
            _playSfx(isMuted, new List<PresetVoiceline> {pan, pa, do_n, pa_n});
            if (isInactiveFunc)
            {
                CurrClapAnim = clapType;
            }
        }

        public void CuePanpaDoPa(double beat, PresetInput pan, PresetInput pa, PresetInput pa_n)
        {
            pan.beatToPlayAt = beat;
            pa.beatToPlayAt = beat + 0.5;
            pa_n.beatToPlayAt = beat + 2;
            _scheduleClaps(beat, 4, new List<PresetInput> {pan, pa, pa_n});
        }

        #endregion

        #region DondoPanpaPa

        public static void DondoPanpaPaSfx(double beat, bool isMuted, int clapType, PresetVoiceline don, PresetVoiceline do_, PresetVoiceline pan, PresetVoiceline pa, PresetVoiceline pa_n, bool isInactiveFunc = false)
        {
            don.beatToPlayAt = beat;
            do_.beatToPlayAt = beat + 0.5;
            pan.beatToPlayAt = beat + 1;
            pa.beatToPlayAt = beat + 1.5;
            pa_n.beatToPlayAt = beat + 2;
            _playSfx(isMuted, new List<PresetVoiceline> {don, do_, pan, pa, pa_n});
            if (isInactiveFunc)
            {
                CurrClapAnim = clapType;
                WantDondoPanpaPa = beat;
            }
        }

        public void CueDondoPanpaPa(double beat, PresetInput pan, PresetInput pa, PresetInput pa_n)
        {
            pan.beatToPlayAt = beat + 1;
            pa.beatToPlayAt = beat + 1.5;
            pa_n.beatToPlayAt = beat + 2;
            _scheduleClaps(beat, 4, new List<PresetInput> {pan, pa, pa_n});
        }

        #endregion

        #region PanpaDondo

        public static void PanpaDondoSfx(double beat, bool isMuted, int clapType, PresetVoiceline pan, PresetVoiceline pa, PresetVoiceline don, PresetVoiceline do_, bool isInactiveFunc = false)
        {
            pan.beatToPlayAt = beat;
            pa.beatToPlayAt = beat + 0.5;
            don.beatToPlayAt = beat + 1;
            do_.beatToPlayAt = beat + 1.5;
            _playSfx(isMuted, new List<PresetVoiceline> {pan, pa, don, do_});
            if (isInactiveFunc)
            {
                CurrClapAnim = clapType;
            }
        }

        public void CuePanpaDondo(double beat, PresetInput pan, PresetInput pa)
        {
            pan.beatToPlayAt = beat;
            pa.beatToPlayAt = beat + 0.5;
            _scheduleClaps(beat, 2, new List<PresetInput> {pan, pa});
        }

        #endregion

        #region PanPanpaPa

        public static void PanPanpaPaSfx(double beat, bool isMuted, int clapType, PresetVoiceline pa_n1, PresetVoiceline pan, PresetVoiceline pa, PresetVoiceline pa_n2, bool isInactiveFunc = false)
        {
            pa_n1.beatToPlayAt = beat;
            pan.beatToPlayAt = beat + 1;
            pa.beatToPlayAt = beat + 1.5;
            pa_n2.beatToPlayAt = beat + 2;
            _playSfx(isMuted, new List<PresetVoiceline> {pa_n1, pan, pa, pa_n2});
            if (isInactiveFunc)
            {
                CurrClapAnim = clapType;
            }
        }

        public void CuePanPanpaPa(double beat, PresetInput pa_n1, PresetInput pan, PresetInput pa, PresetInput pa_n2)
        {
            pa_n1.beatToPlayAt = beat;
            pan.beatToPlayAt = beat + 1;
            pa.beatToPlayAt = beat + 1.5;
            pa_n2.beatToPlayAt = beat + 2;
            _scheduleClaps(beat, 4, new List<PresetInput> {pa_n1, pan, pa, pa_n2});
        }

        #endregion

        #region SuccessReaction

        public void SuccessReaction(double beat, float length, ReactionType reaction, bool yaguraSuccess)
        {
            //edge case if there's a no-bopping block starting at the same time as a success block
            var simultaneousNoBopEvent = gameManager.Beatmap.Entities.Find(
                e => e.datamodel == "bonOdori/bop" && 
                e.beat == beat && 
                ((WhoBops)e["auto"] == WhoBops.None || (WhoBops)e["bop"] == WhoBops.None)
            );
            
            for (int i = 0; i <= _allDonpans.Count - 1; i++)
            {
                _allDonpans[i].SuccessReaction(beat, length, i, _hasMissed, (simultaneousNoBopEvent != null), reaction);
            }

            if (_hasMissed) _yaguraChan.SetYaguraFace("Sad");
            else
            {
               if (yaguraSuccess) _yaguraChan.SetYaguraFace("Happy");
            }

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length, delegate
                {
                    _resetAllFaces();
                    _restoreBopStates(beat + length);
                })
            });

            _hasMissed = false;
        }

        #endregion

        #region _playSfx
        
        private static void _playSfx(bool isMuted, List<PresetVoiceline> voicelinesToPlay)
        {
            if (!isMuted)
            {
                List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>();
                foreach (PresetVoiceline voiceline in voicelinesToPlay)
                {
                    if (voiceline.isPan)
                    {
                        soundsToPlay.Add(_getPanSound(voiceline.beatToPlayAt.Value, voiceline.voicelineType, voiceline.voicelineTypeVariation, voiceline.semitone));
                    }
                    else
                    {
                        soundsToPlay.Add(_getDonSound(voiceline.beatToPlayAt.Value, voiceline.voicelineType, voiceline.voicelineTypeVariation, voiceline.semitone));
                    }
                }
                MultiSound.Play(soundsToPlay.ToArray(), forcePlay: true);
            }
        }

        //NOTE: variation + 1 because sound clip names don't start with 0
        private static MultiSound.Sound _getDonSound(double beat, int donType, int variation, int semitone)
        {
            string clip = donType switch
            {
                0 => "don",
                1 => "do",
                2 => "do_n",
                _ => "do_n"
            };
            var pitch = SoundByte.GetPitchFromSemiTones(semitone, true);
            return new MultiSound.Sound(($"bonOdori/" + clip + (variation + 1)), beat, pitch);
        }

        private static MultiSound.Sound _getPanSound(double beat, int panType, int variation, int semitone)
        {
            string clip = panType switch
            {
                0 => "pan",
                1 => "pa",
                2 or _ => "pa_n",
            };
            var pitch = SoundByte.GetPitchFromSemiTones(semitone, true);
            return new MultiSound.Sound(($"bonOdori/" + clip + (variation + 1)), beat, pitch);
        }

        #endregion

        #region _scheduleClaps

        private void _scheduleClaps(double beat, int blockLength, List<PresetInput> inputsToSpawn)
        {
            //structure in place in case we want to make each individual clap customizable
            //for now last clap type is the claptype for the entire input
            CurrClapAnim = inputsToSpawn[inputsToSpawn.Count - 1].clapType;

            List<BeatAction.Action> actions = new();
            string clapAnimStr = _getClapType(CurrClapAnim);
            double restoreBopBeat = beat + blockLength - 1; //-1 bc we cant restore bop beat on same exact beat

            foreach (PresetInput input in inputsToSpawn)
            {
                double clapBeat = input.beatToPlayAt.Value;

                _clap(clapBeat, clapAnimStr);

                actions.Add(new BeatAction.Action(clapBeat, delegate {
                    _isPlayerBop = false;
                    IsCpuDonpansBop = false;
                    for (int i = 0; i < _allDonpans.Count - 1; i++) //except player donpan
                    {
                        _allDonpans[i].Clap(clapAnimStr);
                    }
                }));
            }
            
            actions.Add(new BeatAction.Action(restoreBopBeat, delegate {
                _restoreBopStates(restoreBopBeat);
            }));

            BeatAction.New(this, actions);
        }

        private string _getClapType(int clapType)
        {
            return clapType switch 
            {
                0 => "ClapSide",
                1 or _ => "ClapFront",
            };
        }

        public void _clap(double beat, string clapAnim)
        {
            ScheduleInput(beat, 0f, InputAction_BasicPress, (c, s) => Just(c, s, clapAnim), Miss, Empty);

            for (int i = 0; i < _allDonpans.Count - 1; i++)
            {
                _allDonpans[i].PlayClapSfx(0.2f, beat: beat, isScheduled: true);
            }
        }

        #endregion

        #region DisplayText

        public void DisplayText(double beat, float length, string userText, TextPreset textPreset, Color presetLyricColor, Color presetDonColor, Color presetPanColor, int fontType, int lineNum, float[] textPositions, bool isGradient, Color scrollColor)
        {
            var regTextTMP = _regTexts[lineNum];
            var scrollTMP = _scrolledTexts[lineNum];

            if (textPreset != TextPreset.CustomText && PRESET_TEXTS.ContainsKey(textPreset))
            {
                userText = PRESET_TEXTS[textPreset];
            }

            if (_scrollCoroutines[lineNum] != null) _killScrollCoroutine(lineNum);
            
            regTextTMP.font = _availableFonts[fontType];
            scrollTMP.font = _availableFonts[fontType];
            _moveTextPosition(regTextTMP, scrollTMP, textPositions[lineNum]);
            _setOutline(regTextTMP.fontMaterial, regTextTMP.font);

            if (!_INVISIBLE_TEXTS.Contains(userText))
            {
                userText = _setPresetColors(userText, presetLyricColor, presetDonColor, presetPanColor);
                
                regTextTMP.text = userText;
                scrollTMP.text = userText;
                regTextTMP.ForceMeshUpdate(); //Forces to acknowledge the rich texts
                scrollTMP.ForceMeshUpdate();

                scrollTMP.GetComponent<TextMeshPro>().SetMask(0, new Vector4(-10f, -10f, -10f, 10));
                _replaceFontMaterials(scrollTMP, scrollColor);
                _setGradientTexture(regTextTMP, isGradient);
                _setGradientTexture(scrollTMP, isGradient);
                _setScrollOutlineColor(lineNum, scrollColor);
            }

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length, delegate
                {
                    regTextTMP.text = "";
                    scrollTMP.text = "";
                    _setGradientTexture(regTextTMP, false);
                    _setGradientTexture(scrollTMP, false);
                })
            });
        }

        //replace all font materials in the current textline

        //KNOWN ISSUE: This was supposed to apply the masking shader to all fonts and their respective materials (rich text tag fonts for mixing)
        //It applies it to the base font in the inspector just fine, but not any rich text tag fonts,
        //Even though debug proves that the material and modifications are being set and applied (base font being applied to is proof)
        private void _replaceFontMaterials(TMP_Text textline, Color scrollColor)
        {
            var currMaterials = textline.fontMaterials;
            var baseFont = textline.font;

            for (int i = 0; i < currMaterials.Length; i++)
            {
                Material newMaskMat = new Material(_customScrollShader);
                Color scrollTintColor = Color.Lerp(Color.white, scrollColor, _DEFAULT_TINT_OPACITY);

                newMaskMat.CopyPropertiesFromMaterial(currMaterials[i]);
                newMaskMat.SetColor("_FaceColor", scrollTintColor);
                _setOutline(newMaskMat, baseFont);
                currMaterials[i] = newMaskMat;
            }
            textline.fontMaterials = currMaterials;
        }

        private void _setOutline(Material fntMaterial, TMP_FontAsset fntAsset)
        {
            if (!_underlayFontsList.Contains(fntAsset)) //hardcoding idc anymore bon odori rodin can only be assigned in unity inspector anyways
            {
                fntMaterial.EnableKeyword("OUTLINE_ON");
                fntMaterial.SetFloat("_OutlineWidth",_DEFAULT_OUTLINE_WIDTH);
            }
            else
            {
                fntMaterial.EnableKeyword("UNDERLAY_ON");
                fntMaterial.SetFloat("_UnderlayDilate", _DEFAULT_UNDERLAY_WIDTH);
                fntMaterial.SetColor("_UnderlayColor", Color.black);
            }
        }

        private void _setGradientTexture(TMP_Text textComponent, bool isGradient)
        {
            if (isGradient) textComponent.fontMaterial.SetTexture("_FaceTex", _gradientTexture);
            else textComponent.fontMaterial.SetTexture("_FaceTex", null);
        }

        private string  _setPresetColors(string userText, Color presetLyricColor, Color presetDonColor, Color presetPanColor)
        {
            string lyrHex = $"<color=#{ColorUtility.ToHtmlStringRGB(presetLyricColor)}>";
            string donHex = $"<color=#{ColorUtility.ToHtmlStringRGB(presetDonColor)}>";
            string panHex = $"<color=#{ColorUtility.ToHtmlStringRGB(presetPanColor)}>";

            return userText.Replace("{lyricColor}", lyrHex)
                           .Replace("{donColor}", donHex)
                           .Replace("{panColor}", panHex);
        }

        private void _moveTextPosition(TMP_Text regTextTMP, TMP_Text scrollTextTMP, float normalizedPos)
        {
            float revertedPos = Mathf.Lerp(60f, 78f, normalizedPos);

            Vector3 regTextPos = regTextTMP.transform.localPosition;
            Vector3 scrollTextPos = scrollTextTMP.transform.localPosition;
            regTextPos.x = revertedPos;
            scrollTextPos.x = revertedPos;
            regTextTMP.transform.localPosition = regTextPos;
            scrollTextTMP.transform.localPosition = scrollTextPos;
        }

        #endregion

        #region ScrollText

        public void ValidateScroll(double beat, float length, int lineNum, Util.EasingFunction.Ease ease, float scrollSpeed)
        {  
            if (!_INVISIBLE_TEXTS.Contains(_regTexts[lineNum].text))
            {
                var scrollTextTMP = _scrolledTexts[lineNum].GetComponent<TextMeshPro>();
                if (_scrollCoroutines[lineNum] != null)
                {
                    StartCoroutine(_delayedScrollStart(beat, length, scrollTextTMP, ease, scrollSpeed, lineNum));//idk how else to beat the race condition lol; ITS JUST A FRAME
                }
                else
                {
                    _scrollCoroutines[lineNum] = StartCoroutine(_scrollText(beat, length, scrollTextTMP, ease, scrollSpeed, lineNum));
                }
            }
        }

        private IEnumerator _delayedScrollStart(double beat, float length, TextMeshPro scrollTextTMP, Util.EasingFunction.Ease ease, float scrollSpeed, int lineNum)
        {
            yield return null; //wait a frame so the data properly solidifies for the next scroll; scuffed duct tape solution but idk what else to do
            _scrollCoroutines[lineNum] = StartCoroutine(_scrollText(beat, length, scrollTextTMP, ease, scrollSpeed, lineNum));
        }

        private IEnumerator _scrollText(double scrollStartBeat, float length, TextMeshPro scrollTextTMP, Util.EasingFunction.Ease ease, float scrollAmt, int lineNum)
        {
            double scrollEndBeat = scrollStartBeat + length;
            var relevantDisplayTextEvents = gameManager.Beatmap.Entities.FindAll(e => e.datamodel == "bonOdori/displayText" && (WhichLine)e["whichLine"] == (WhichLine)lineNum);
            var relevantTextEvent = relevantDisplayTextEvents.Find(e => (scrollStartBeat < e.beat + e.length) && (scrollEndBeat > e.beat));
            
            if (relevantTextEvent == null) yield break; //quit func if there's no Display Text with it
            
            scrollStartBeat = Math.Clamp(scrollStartBeat, relevantTextEvent.beat, scrollEndBeat); //scrollig should start when Display Text starts
            
            while (cond.unswungSongPositionInBeatsAsDouble < scrollStartBeat) yield return null; //wait until scroll start beat
            
            float scrollWidth = scrollTextTMP.preferredWidth;
            float absoluteStartPos = -scrollTextTMP.rectTransform.sizeDelta.x / 2f;
            float absoluteEndPos = absoluteStartPos + scrollWidth;

            float scrollStart = _lastMaskPositions[lineNum] ?? absoluteStartPos; //use last pos if not null; reset scroll pos otherwise
            float targetScrollPos = absoluteStartPos + (scrollWidth * scrollAmt);
            float scrollEnd = Mathf.Clamp(targetScrollPos, scrollStart, absoluteEndPos);//dont scroll backwards

            float scrollDuration = (float)(scrollEndBeat - scrollStartBeat);
            float scrollDistance = scrollEnd - scrollStart;

            var easingFunc = Util.EasingFunction.GetEasingFunction(ease);
            
            scrollTextTMP.SetMask(0, new Vector4(-10f, -10f, scrollStart, 10f));
            
            double currBeat = cond.unswungSongPositionInBeatsAsDouble;
            while (currBeat <= scrollEndBeat) //actual scrolling
            {
                float scrollProgress = Mathf.Clamp01((float)((currBeat - scrollStartBeat) / scrollDuration));
                float easedProgress = easingFunc(0f, 1f, scrollProgress);
                float maskPos = Mathf.Lerp(scrollStart, scrollStart + scrollDistance, easedProgress);
                maskPos = Mathf.Clamp(maskPos, scrollStart, scrollEnd);
                
                scrollTextTMP.SetMask(0, new Vector4(-10f, -10f, maskPos, 10f));
                
                yield return null;
                currBeat = cond.unswungSongPositionInBeatsAsDouble;
            }
            
            currBeat = cond.unswungSongPositionInBeatsAsDouble;
            if (currBeat >= scrollEndBeat && scrollEnd >= absoluteEndPos) //scroll met time and distance requirements
            {
                scrollTextTMP.SetMask(0, new Vector4(-10f, -10f, absoluteEndPos, 10f));
                _lastMaskPositions[lineNum] = null;
            }
            else //scroll incomplete; reset and kil
            {
                scrollTextTMP.SetMask(0, new Vector4(-10f, -10f, scrollEnd, 10f));
                _lastMaskPositions[lineNum] = scrollEnd;
            }
            _scrollCoroutines[lineNum] = null;
        }

        private void _killScrollCoroutine(int lineNum)
        {
            StopCoroutine(_scrollCoroutines[lineNum]);
            _scrollCoroutines[lineNum] = null;
            _lastMaskPositions[lineNum] = null;
        }

        #endregion

        #region Bow

        public void Bow(double beat, float length)
        {
            bool playerBopCache = _isPlayerBop;
            bool cpuBopCache = IsCpuDonpansBop;
            _isPlayerBop = false;
            IsCpuDonpansBop = false;

            foreach (var donpan in _allDonpans)
            {
                donpan.Bow();
            }
            
            //i think onlatbeatpulse executes first before this so -1
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                //also hella scuffed solution but idk how to fix it cuz 2 overlapping beatActions is just trouble
                new BeatAction.Action(beat + length - 0.05, delegate
                {
                    foreach (var donpan in _allDonpans)
                    {
                        donpan.Bop();
                    }
                    IsCpuDonpansBop = playerBopCache;
                    _isPlayerBop = cpuBopCache;
                })
            });
        }

        #endregion

        #region Spin

        public void Spin(double beat)
        {   
            bool playerBopCache = _isPlayerBop;
            bool cpuBopCache = IsCpuDonpansBop;
            _isPlayerBop = false;
            IsCpuDonpansBop = false;

            for (int i = 0; i < _allDonpans.Count; i++)
            {
                _allDonpans[i].Spin(i);
            }
            
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1 - 0.05, delegate
                {
                    foreach (var donpan in _allDonpans)
                    {
                        donpan.Bop();
                    }
                    IsCpuDonpansBop = playerBopCache;
                    _isPlayerBop = cpuBopCache;
                })
            });
        }

        #endregion

        #region DarkenScreen

        public void DarkenScreen(double beat, float length, Color start, Color end, int ease)
        {
            _overlayColorEase = new ColorEase(beat, length, start, end, ease);
        }
        
        private void _colorUpdate()
        {
            darkPlane.color = _overlayColorEase.GetColor();
        }

        #endregion

        #region SetDonpanColor

        public void SetDonpansColors(DonpanColors dp1, DonpanColors dp2, DonpanColors dp3, DonpanColors dp4)
        {
            DonpanColors[] allColors = {dp1, dp2, dp3, dp4};
            for (int i = 0; i < _donpanRecolorMats.Count; i++)
            {
                _donpanRecolorMats[i].SetColor("_ColorAlpha", allColors[i].kimono);
                _donpanRecolorMats[i].SetColor("_ColorBravo", allColors[i].petals);
                _donpanRecolorMats[i].SetColor("_ColorDelta", allColors[i].pistil);

                _donpanRecolorFootMats[i].SetColor("_ColorAlpha", allColors[i].petals); //sandal bindings :R
                _donpanRecolorFootMats[i].SetColor("_ColorBravo", _skinColor); //foot :G
                _donpanRecolorFootMats[i].SetColor("_ColorDelta", allColors[i].kimono); //sandal: B
            }
        }

        #endregion
    }
}