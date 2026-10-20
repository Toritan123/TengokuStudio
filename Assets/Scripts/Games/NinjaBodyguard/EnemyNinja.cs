using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_NinjaBodyguard
{
    public class EnemyNinja : MonoBehaviour
    {
        [SerializeField] Animator anim;
        [SerializeField] SortingGroup sort;

        public NinjaBodyguard game;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetOrder(int order)
        {
            sort.sortingOrder = order;
        }

        public void PrepareArrow()
        {
            anim.DoScaledAnimationAsync("ArrowReady", 0.5f);
        }

        public void ShootArrow()
        {
            anim.DoScaledAnimationAsync("ArrowShot", 0.5f);
        }
    }
}
