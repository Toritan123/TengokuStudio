using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_ValiantVolley
{
    public class Ant : MonoBehaviour
    {
        [SerializeField] SpriteRenderer[] sr;
        [SerializeField] SpriteRenderer[] srAll;
        [SerializeField] bool player;
        [SerializeField] int antNum;
        [SerializeField] VolleyObject[] currentlyVolleying;
        public Animator anim;

        public bool cantBop;
        public bool justHit;
        public bool isPreparing;
        public double queuePrepare;
        double lastVolleyBeat;
        bool isActive = true;

        private ValiantVolley game;

        void Awake()
        {
            game = ValiantVolley.instance;
            queuePrepare = double.MaxValue;
        }

        void Update()
        {
            if (queuePrepare <= Conductor.instance.songPositionInBeatsAsDouble && Conductor.instance.NotStopped())
            {
                if (!(isPreparing || anim.IsPlayingAnimationNames("Volley")))
                {
                    anim.DoScaledAnimationFromBeatAsync("AntPrepare", 0.5f, queuePrepare);
                    isPreparing = true;
                    queuePrepare = double.MaxValue;
                }
            }

            if (isActive && player)
            {
                if (PlayerInput.GetIsAction(ValiantVolley.InputAction_BasicPress) && !game.IsExpectingInputNow(ValiantVolley.InputAction_BasicPress) ||
                    PlayerInput.GetIsAction(ValiantVolley.InputAction_AltPress) && !game.IsExpectingInputNow(ValiantVolley.InputAction_AltPress))
                {
                    AntAction("dirtHit");
                    SoundByte.PlayOneShotGame("nearMiss");
                    game.hasMissed = true;
                }
            }
        }

        public void RequestBop()
        {
            if (!anim.IsPlayingAnimationNames("AntBop", "AntHappy", "AntAngry", "AntOops") && !isPreparing && !justHit && !cantBop)
            {
                //if (game.bopStatus == 2 && player) print("gummy bear :)");
                PlayAnimation(game.bopStatus);
            }
            else if (game.bopStatus == 2)
            {
                game.ants[2].cantBop = false;
                game.ants[2].isPreparing = false;
                game.ants[2].justHit = false;
            }
        }

        public void PlayAnimation(int whichBop)
        {
            if (whichBop == 2 && player) whichBop = 3;
            anim.DoScaledAnimationAsync(whichBop switch
            {
                0 => "AntBop",
                1 => "AntHappy",
                2 => "AntAngry",
                3 => "AntOops",
                _ => "AntBop"
            }, 0.5f);
            //isPreparing = false;
            justHit = false;

            //Debug.Log("Bop: " + whichBop);

            if ((whichBop == 1 || whichBop == 3) && player) game.bopStatus = 0;
            //isSqueezed = false;
        }

        public void AntAction(string action, float pitch = 1)
        {
            //Debug.Log(action);
            if ((Conductor.instance.songPositionInBeatsAsDouble - lastVolleyBeat) > 0.15f)
                if  (antNum != 2 || ((action == "dirtHit") && game.IsExpectingInputNow(ValiantVolley.InputAction_BasicPress))
                    || ((action == "fruitHit") && game.IsExpectingInputNow(ValiantVolley.InputAction_AltPress)))
                        //SoundByte.PlayOneShotGame($"valiantVolley/{action.ToLower()}", -1, pitch);
                        justHit = true;
            if (action == "dirtHit" || action == "fruitHit") lastVolleyBeat = Conductor.instance.songPositionInBeatsAsDouble;

            SoundByte.PlayOneShotGame("valiantVolley/woosh");

            anim.DoScaledAnimationAsync("Volley", 0.5f);
            isPreparing = false;
            queuePrepare = double.MaxValue;
        }
    }
}