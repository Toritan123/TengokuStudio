using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Util
{
    public class MenuAudioLooper : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip musicStart;
        [SerializeField] private AudioClip musicLoop;
        private bool hasStarted;


        // Start is called before the first frame update
        void Update()
        {
            if (!hasStarted)
            {
                audioSource.clip = musicStart;
                audioSource.Play();
                audioSource.clip = musicLoop;
                audioSource.loop = true;
                audioSource.PlayScheduled(AudioSettings.dspTime + musicStart.length);
                hasStarted = true;
            }
        }
    }
}
