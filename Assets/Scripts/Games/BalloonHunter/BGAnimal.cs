using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_BalloonHunter
{
    public class BGAnimal : MonoBehaviour
    {
        [SerializeField] GameObject bgObject;
        [SerializeField] Animator anim;
        [SerializeField] Animator rabbitAnim;
        [SerializeField] Animator boarAnim;
        [SerializeField] Animator mooseAnim;

        public int type;
        public double startBeat;
        public float flyLength;
        public bool right;
        public float startY;
        public float endY;

        float startX;
        float endX;

        Animator animalAnim;

        public BalloonHunter game;
        // Start is called before the first frame update
        void Start()
        {
            transform.SetParent(bgObject.transform);

            startX = right ? -10.45f : 10.45f;
            endX = right ? 10.45f : -10.45f;

            transform.localScale = new Vector3(right ? 0.3f : -0.3f, 0.3f, 1f);

            switch (type) 
            {
                case 0:
                    anim.DoScaledAnimationAsync("Rabbit");
                    animalAnim = rabbitAnim;
                    break;
                case 1:
                    anim.DoScaledAnimationAsync("Boar");
                    animalAnim = boarAnim;
                    break;
                case 2:
                    anim.DoScaledAnimationAsync("Moose");
                    animalAnim = mooseAnim;
                    break;
            }
        }

        // Update is called once per frame
        void Update()
        {
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat,flyLength);
            if (normalizedBeat > 1) Destroy(gameObject);
            Vector3 pos = transform.position;
            pos.x = startX + (endX - startX) * normalizedBeat;
            pos.y = startY + (endY - startY) * normalizedBeat;
            transform.position = pos;
        }

        public void DoBop()
        {
            animalAnim.DoScaledAnimationAsync("Bop", 0.5f);
        }
    }
}
