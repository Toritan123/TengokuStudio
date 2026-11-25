using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_BonOdori
{
    public class Donpan : MonoBehaviour
    {
        [SerializeField] private Animator _donpan;
        [SerializeField] private Animator _donpanFace;
        private BonOdori _game;
        private bool _isPlayer = false;
        public bool isPlayer { set => _isPlayer = value; }

        private void Awake()
        {
            _game = BonOdori.instance;
        }

        //add spin anim to list
        public void Bop()
        {
            if (!_donpan.IsPlayingAnimationNames("ClapSide", "ClapFront", "Bow", "BopHappy", "Bop", "BopHappy")) 
            {
                _donpan.DoScaledAnimationAsync("Bop", 0.5f);
            }
        }

        public void PlayClapSfx(float clapVol, double? beat = null, bool isBarely = false, bool isScheduled = false)
        {
            if (!isBarely)
            {
                if (_isPlayer) SoundByte.PlayOneShotGame("bonOdori/clap");
                else 
                {
                   if (isScheduled) SoundByte.PlayOneShotGame("bonOdori/clap2", beat: beat.Value);
                   else SoundByte.PlayOneShotGame("bonOdori/clap2");
                }
            }
            else SoundByte.PlayOneShot("nearMiss");
        }

        public void Clap(string clapAnim)
        {
            _donpan.DoScaledAnimationAsync(clapAnim, 0.5f);
        }

        public void SuccessReaction(double beat, float length, int donpan, bool hasMissed, bool simultaneousNoBopEventExists, BonOdori.ReactionType reaction)
        {
            if (hasMissed) 
            {
                if (!_isPlayer) _donpanFace.DoScaledAnimationAsync("Annoyed", 0.5f);
            }
            else
            {
                switch (donpan)
                {
                    case 0:
                        _donpanFace.DoScaledAnimationAsync("CPU1Happy", 0.5f);
                        break;
                    case 1:
                        _donpanFace.DoScaledAnimationAsync("CPU2Happy", 0.5f);
                        break;
                    case 2:
                        _donpanFace.DoScaledAnimationAsync("CPU3Happy", 0.5f);
                        break;
                    default:
                        break;
                }

                List<BeatAction.Action> bopActions = new();
                bool doFaceAndArmsBop = (reaction == BonOdori.ReactionType.FaceAndArms && !_isPlayer);
                string bopAnim = (doFaceAndArmsBop) ? "BopHappy" : "Bop";

                if (simultaneousNoBopEventExists)
                {
                    bopActions.Add(new BeatAction.Action(beat, delegate {
                        _donpan.DoScaledAnimationAsync(bopAnim, 0.5f);
                    }));
                }
                else
                {
                    for (int i = 0; i < (int)Math.Floor(length); i++)
                    {
                        bopActions.Add(new BeatAction.Action(beat + i, delegate {
                            _donpan.DoScaledAnimationAsync(bopAnim, 0.5f);
                        })); 
                    }
                }

                if (bopActions.Count > 0) BeatAction.New(this, bopActions);
            }
        }

        //add spin anim to list
        public void Bow()
        {
            if (!_donpan.IsPlayingAnimationNames("Spin", "SpinCPU", "SpinCPU2", "SpinCPU3")) 
            {
                _donpan.DoScaledAnimationAsync("Bow", 0.5f);
            }
        }

        public void Spin(int donpan)
        {
            if (!_donpan.IsPlayingAnimationNames("ClapSide", "ClapFront", "Bow")) 
            {
                switch (donpan)
                {
                    case 0:
                        _donpan.DoScaledAnimationAsync("SpinCPU", 0.5f);
                        break;
                    case 1:
                        _donpan.DoScaledAnimationAsync("SpinCPU2", 0.5f);
                        break;
                    case 2:
                        _donpan.DoScaledAnimationAsync("SpinCPU3", 0.5f);
                        break;
                    case 3:
                        _donpan.DoScaledAnimationAsync("Spin", 0.5f);
                        break;
                }
            }
        }

        public void ResetFace(int whichDonpan)
        {
            if (whichDonpan == 0) _donpanFace.DoScaledAnimationAsync("CPU1Neut", 0.5f);
            else if (whichDonpan == 1) _donpanFace.DoScaledAnimationAsync("CPU2Neut", 0.5f);
            else if (whichDonpan == 2) _donpanFace.DoScaledAnimationAsync("CPU3Neut", 0.5f);
        }

        public void ResetToNeutral()
        {
            _donpan.DoScaledAnimationAsync("NeutralBopped", 0.5f);
        }
    }
}


