using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Editor.Track;

using TMPro;

namespace HeavenStudio.Editor 
{
    public class TabButton : MonoBehaviour
    {
        public GameObject Content;
        public Animator Anim;

        public void OnClick()
        {
            var tabsManager = transform.parent.GetComponent<TabsManager>();
            tabsManager.SetActiveContent(Content, gameObject);
        }

        public void Raise()
        {
            Anim.Play("Raise");
        }

        public void Lower()
        {
            Anim.Play("Lower");
        }
    }
}