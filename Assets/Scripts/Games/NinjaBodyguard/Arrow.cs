using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using NaughtyBezierCurves;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_NinjaBodyguard
{
    public class Arrow : MonoBehaviour
    {
        [SerializeField] Animator anim;
        [SerializeField] SortingGroup sort;
        [SerializeField] Vector3 divertPosition;
        //[SerializeField] Vector3 hitAreaPointL;
        //[SerializeField] Vector3 hitAreaPointR;
        //[SerializeField] float hitAreaHeight;
        [SerializeField] BezierCurve3D hitCurve;

        public ArrowState currentState;

        public enum ArrowState
        {
            Destroy,
            DivertL,
            DivertR,
            Hit,
        }

        // Start is called before the first frame update
        void Start()
        {
            switch (currentState)
            {
                case ArrowState.Destroy:
                    transform.position = divertPosition;
                    anim.DoScaledAnimationAsync("Destroy", 0.5f);
                    break;
                case ArrowState.DivertL:
                    transform.position = divertPosition;
                    anim.DoScaledAnimationAsync("DivertL", 0.5f);
                    break;
                case ArrowState.DivertR:
                    transform.position = divertPosition;
                    anim.DoScaledAnimationAsync("DivertR", 0.5f);
                    break;
                case ArrowState.Hit:
                    float rand = UnityEngine.Random.Range(0f, 1f);
                    transform.position = hitCurve.GetPoint(rand);
                    sort.sortingOrder = rand <= 0.5f ? 5 : 15;
                    anim.DoScaledAnimationAsync("Hit", 0.5f);
                    break;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!anim.IsPlayingAnimationNames("Destroy", "DivertL", "DivertR") && currentState != ArrowState.Hit) Destroy(gameObject);
        }
    }
}
