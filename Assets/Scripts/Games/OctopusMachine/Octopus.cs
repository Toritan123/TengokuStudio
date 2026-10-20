using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_OctopusMachine
{
    public class Octopus : MonoBehaviour
    {
        [SerializeField] SpriteRenderer[] sr;
        [SerializeField] SpriteRenderer[] srAll;
        [SerializeField] bool player;
        [SerializeField] int octoNum;
        public Animator anim;

        public bool cantBop;
        public bool isSqueezed;
        public bool isPreparing;
        public double queuePrepare;
        double lastSqueezeBeat;
        bool isActive = true;

        private OctopusMachine game;

        void Awake()
        {
            game = OctopusMachine.instance;
            queuePrepare = double.MaxValue;
        }

        void Update()
        {
            if (queuePrepare <= Conductor.instance.songPositionInBeatsAsDouble && Conductor.instance.NotStopped()) {
                if (!(isPreparing || isSqueezed || anim.IsPlayingAnimationNames("Release", "Pop"))) 
                {
                    anim.DoScaledAnimationFromBeatAsync("Prepare", 0.5f, queuePrepare);
                    isPreparing = true;
                    queuePrepare = double.MaxValue;
                }
            }

            if (isActive && player)
            {
                if (PlayerInput.GetIsAction(OctopusMachine.InputAction_BasicPress) && !game.IsExpectingInputNow(OctopusMachine.InputAction_BasicPress))
                {
                    OctoAction("Squeeze", Conductor.instance.songPositionInBeatsAsDouble);
                    SoundByte.PlayOneShotGame("nearMiss");
                    game.hasMissed = true;
                }

                if (PlayerInput.GetIsAction(OctopusMachine.InputAction_BasicRelease) && !game.IsExpectingInputNow(OctopusMachine.InputAction_BasicRelease))
                {
                    OctoAction("Release", Conductor.instance.songPositionInBeatsAsDouble);
                    SoundByte.PlayOneShotGame("nearMiss");
                    game.hasMissed = true;
                }
                else if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch
                    && PlayerInput.GetIsAction(OctopusMachine.InputAction_FlickRelease) && !game.IsExpectingInputNow(OctopusMachine.InputAction_FlickRelease))
                {
                    OctoAction("Pop", Conductor.instance.songPositionInBeatsAsDouble);
                    SoundByte.PlayOneShotGame("nearMiss");
                    game.hasMissed = true;
                }
            }
        }

        public void RequestBop()
        {
            if (!anim.IsPlayingAnimationNames("Bop", "Happy", "Angry", "Oops", "Release", "Pop") && !isPreparing && !isSqueezed && !cantBop)
            {
                PlayAnimation(game.bopStatus);
            }
        }

        public void PlayAnimation(int whichBop)
        {
            if (whichBop == 2 && player) whichBop = 3;
            anim.DoScaledAnimationAsync(whichBop switch
            {
                0 => "Bop",
                1 => "Happy",
                2 => "Angry",
                3 => "Oops",
                _ => "Bop"
            }, 0.5f);
            isPreparing =
            isSqueezed = false;
        }

        public void ForceSqueeze()
        {
            anim.DoScaledAnimationAsync("ForceSqueeze", 0.5f);
            isSqueezed = true;
        }

        public void OctopusModifiers(float x, float y, bool enable)
        {
            gameObject.transform.position = new Vector3(x, y, 0);
            foreach (var sprite in srAll) sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, enable ? 1 : 0);
            isActive = enable;
        }

        public void OctoAction(string action, double beat)
        {
            if (action != "Release" || (beat - lastSqueezeBeat) > 0.15f)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound($"octopusMachine/{action.ToLower()}", beat),
                });
            }

            BeatAction.New(game, new List<BeatAction.Action>() {
                new BeatAction.Action(beat, delegate {
                    if (action == "Squeeze") lastSqueezeBeat = beat;

                    anim.DoScaledAnimationAsync(action, 0.5f);
                    isSqueezed = action == "Squeeze";
                    isPreparing = false;
                    queuePrepare = double.MaxValue;
                }),
            });
        }

        public void AnimationColor(int poppingColor)
        {
            Color[] octopodesColors = { OctopusMachine.octopodesColor1, OctopusMachine.octopodesColor2, OctopusMachine.octopodesColor3 };
            Color[] octopodesSqueezedColors = { OctopusMachine.octopodesSqueezedColor1, OctopusMachine.octopodesSqueezedColor2, OctopusMachine.octopodesSqueezedColor3 };
            foreach (var sprite in sr)
            {
                sprite.material.SetColor("_ColorAlpha", (poppingColor == 0 ? octopodesColors[octoNum] : octopodesSqueezedColors[octoNum]));
            }

            if (poppingColor == 1) isSqueezed = true;
        }
    }
}