using UnityEngine;
using UnityEngine.UI;
using TMPro;

using HeavenStudio.Common;
using System.Runtime.InteropServices;
using SFB;
using System.IO;
using HeavenStudio.Util;
using Jukebox;

namespace HeavenStudio.Editor 
{
    public class SpritesMenu : TabsContent
    {
        [SerializeField] SpriteMenuElement defaultSprite;
        [SerializeField] RectTransform viewport;
        [SerializeField] RectTransform spritesList;

        private void Start()
        {
            // OnOpenTab();
        }

        public override void OnOpenTab()
        {
            GenerateAllSprites();
        }

        public override void OnCloseTab()
        {
        }

        public void OnImportClicked()
        {
            // SoundByte.PlayOneShot("ui/UIPromptDown");

            var extensions = new[]
            {
                new ExtensionFilter("Image Files", new string[] { "png", "jpg", "jpeg" }),
            };

            FileBrowserOpener.OpenFilePanelAsync("Import Sprite", "", extensions, true, (string[] paths) =>
            {
                if (paths.Length == 0)
                {
                    // SoundByte.PlayOneShot("ui/UICancel");
                    return;
                }

                // SoundByte.PlayOneShot("ui/UIPromptUp");

                foreach (var path in paths)
                {
                    if (string.IsNullOrEmpty(path)) continue;

                    try
                    {
                        byte[] imageBytes = File.ReadAllBytes(path);

                        Texture2D texture = new Texture2D(64, 64, TextureFormat.ARGB32, false);
                        texture.LoadImage(imageBytes);
                        texture.name = Path.GetFileNameWithoutExtension(path);

                        // make sure there are no duplicate keys, while allowing the user to add duplicates of the same name
                        int i = 1;
                        while (GameManager.instance.ImportedSprites.ContainsKey(texture.name))
                        {
                            texture.name += "_" + i;
                        }

                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                        GameManager.instance.ImportedSprites.Add(texture.name, sprite);
                        RiqFileHandler.AddResource(path, texture.name, "Sprites");
                        GenerateSingleSprite(sprite, texture.name);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning("Couldn't import sprite!\n" + e);
                    }
                }
            });

        }

    

        private void GenerateAllSprites()
        {
            foreach ((string name, Sprite sprite) in GameManager.instance.ImportedSprites) {
                GenerateSingleSprite(sprite, name);
            }
        }

        private void GenerateSingleSprite(Sprite sprite, string name)
        {
            var spriteMenuElement = Instantiate(defaultSprite, spritesList);
            spriteMenuElement.Init(sprite, name);
        }
    }
}