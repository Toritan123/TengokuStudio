using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Jukebox;

namespace HeavenStudio.Editor 
{
    public class SpriteMenuElement : MonoBehaviour
    {
        [SerializeField] RectTransform rectTransform;
        [SerializeField] Image SpriteImage;
        [SerializeField] TMP_InputField SpriteName;
        private string spriteName;

        public void Init(Sprite sprite, string name)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 150);
            SpriteImage.color = Color.white;
            SpriteImage.sprite = sprite;
            SpriteName.text = name;
            spriteName = name;
        }

        public void OnSpriteNameValueChanged()
        {
            GameManager.instance.ImportedSprites.Remove(spriteName);
            spriteName = SpriteName.text;
            int i = 1;
            while (GameManager.instance.ImportedSprites.ContainsKey(spriteName))
            {
                spriteName += "_" + i;
            }
            SpriteName.text = spriteName;
            GameManager.instance.ImportedSprites.Add(spriteName, SpriteImage.sprite);
        }

        // called when pressing button
        public void RemoveSprite()
        {
            GameManager.instance.ImportedSprites.Remove(spriteName);
            RiqFileHandler.RemoveResource(spriteName, "Sprites");
            Destroy(gameObject);
        }
    }
}