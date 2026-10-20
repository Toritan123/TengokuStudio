using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_GardenDance
{
    public class Flower : MonoBehaviour
    {
        [SerializeField] Animator anim;
        private bool canBlink = true;

        public bool danceRight;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (UnityEngine.Random.Range(1, 600) == 1 && canBlink && !anim.IsPlayingAnimationNames("Blink"))
            {
                anim.DoScaledAnimationAsync("Blink", 0.5f, animLayer : 1);
            }
        }

        public void Glare()
        {
            anim.DoScaledAnimationAsync("Glare", 0.5f, animLayer: 1);
        }

        public void Bop()
        {
            if (!anim.IsPlayingAnimationNames("DanceR", "DanceL"))
            {
                anim.DoScaledAnimationAsync("Bop", 0.5f, animLayer: 0);
                anim.DoScaledAnimationAsync("IdleFace", 0.5f, animLayer: 1);
            }
            canBlink = true;
        }

        public void Dance(bool hit, bool barely = false, bool pose = false, bool glare = false)
        {
            if (pose)
            {
                anim.DoScaledAnimationAsync(danceRight ? "PDanceR" : "PDanceL", 0.5f, animLayer: 0);
            }
            else
            {
                anim.DoScaledAnimationAsync(danceRight ? "DanceR" : "DanceL", 0.5f, animLayer: 0);
            }

            if (hit)
            {
                if (!glare) anim.DoScaledAnimationAsync(barely ? "Barely" : "IdleFace", 0.5f, animLayer: 1);
            }
            else
            {
                anim.DoScaledAnimationAsync("MissFace", 0.5f, animLayer: 1);
            }

            SoundByte.PlayOneShotGame(barely ? "gardenDance/nearMiss" : "gardenDance/dance");

            danceRight = !danceRight;
        }

        public void Pose(bool player, bool barely = false, bool glare = false)
        {
            anim.DoScaledAnimationAsync(danceRight ? "PoseR" : "PoseL", 0.5f, animLayer: 0);
            if (!glare) anim.DoScaledAnimationAsync(barely ? "Barely" : "PoseFace", 0.5f, animLayer: 1);
            SoundByte.PlayOneShotGame(barely ? "gardenDance/nearMiss" : "gardenDance/dance");
            if (player && !barely) SoundByte.PlayOneShotGame("gardenDance/sparkle");

            danceRight = !danceRight;
        }

        public void Triplet(bool player, bool barely = false, bool glare = false)
        {
            anim.DoScaledAnimationAsync(danceRight ? "TripletR" : "TripletL", 0.5f, animLayer: 0);
            if (!glare) anim.DoScaledAnimationAsync(barely ? "Barely" : "TripletFace", 0.5f, animLayer: 1);
            SoundByte.PlayOneShotGame(barely ? "gardenDance/nearMiss" : "gardenDance/sway");

            danceRight = !danceRight;
        }

        public void Miss()
        {
            anim.DoScaledAnimationAsync("MissFace", 0.5f, animLayer: 1);
        }
    }
}
