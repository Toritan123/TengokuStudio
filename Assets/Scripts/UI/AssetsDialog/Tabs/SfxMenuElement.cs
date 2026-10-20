using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Jukebox;

namespace HeavenStudio.Editor 
{
    public class SfxMenuElement : MonoBehaviour
    {
        [SerializeField] RectTransform rectTransform;
        [SerializeField] AudioSource SoundSource;
        [SerializeField] TMP_InputField SfxName;
        private string sfxName;

        public void Init(AudioClip clip, string name)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 150);
            SoundSource.clip = clip;
            SfxName.text = name;
            sfxName = name;
        }

        public void OnSfxNameValueChanged()
        {
            GameManager.instance.ImportedSfx.Remove(sfxName);
            sfxName = SfxName.text;
            int i = 1;
            while (GameManager.instance.ImportedSfx.ContainsKey(sfxName))
            {
                sfxName += "_" + i;
            }
            SfxName.text = sfxName;
            GameManager.instance.ImportedSfx.Add(sfxName, SoundSource.clip);
        }

        public void PlayPause()
        {
            if (SoundSource.isPlaying)
            {
                SoundSource.Stop();
            }
            else
            {
                SoundSource.Play();
            }
        }

        // called when pressing button
        public void RemoveSfx()
        {
            GameManager.instance.ImportedSfx.Remove(sfxName);
            RiqFileHandler.RemoveResource(sfxName, "Sounds");
            Destroy(gameObject);
        }
    }
}