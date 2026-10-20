using UnityEngine;
using UnityEngine.UI;
using TMPro;

using HeavenStudio.Common;
using System.Runtime.InteropServices;
using SFB;
using System.IO;
using HeavenStudio.Util;
using System.Collections;
using UnityEngine.Networking;
using Jukebox;

namespace HeavenStudio.Editor 
{
    public class SfxMenu : TabsContent
    {
        [SerializeField] SfxMenuElement defaultSfx;
        [SerializeField] RectTransform viewport;
        [SerializeField] RectTransform sfxList;

        private bool generatedClips = false;

        private void Start()
        {
            // OnOpenTab();
        }

        public override void OnOpenTab()
        {
            if (!generatedClips) GenerateAllAudioClips();
        }

        public override void OnCloseTab()
        {
        }

        public void OnImportClicked()
        {
            // SoundByte.PlayOneShot("ui/UIPromptDown");

            var extensions = new[]
            {
                new ExtensionFilter("Audio Files", new string[] { "wav", "mp3", "ogg" }),
            };

            FileBrowserOpener.OpenFilePanelAsync("Import SFX", "", extensions, true, (string[] paths) =>
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

                    GameManager.instance.StartCoroutine(LoadAudioAsync(path));
                }
            });

        }

        IEnumerator LoadAudioAsync(string path)
        {
            string url = "file://" + path;
            var request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load audio: " + request.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                clip.name = Path.GetFileNameWithoutExtension(path);

                int i = 1;
                while (GameManager.instance.ImportedSfx.ContainsKey(clip.name))
                {
                    clip.name = Path.GetFileNameWithoutExtension(path) + "_" + i;
                    i++;
                }

                GameManager.instance.ImportedSfx[clip.name] = clip;
                RiqFileHandler.AddResource(path, clip.name, "Sounds");
                GenerateSingleAudioClip(clip, clip.name);
            }
        }



        private void GenerateAllAudioClips()
        {
            generatedClips = true;
            foreach ((string name, AudioClip clip) in GameManager.instance.ImportedSfx) {
                GenerateSingleAudioClip(clip, name);
            }
        }

        private void GenerateSingleAudioClip(AudioClip clip, string name)
        {
            var sfxMenuElement = Instantiate(defaultSfx, sfxList);
            sfxMenuElement.Init(clip, name);
        }
    }
}