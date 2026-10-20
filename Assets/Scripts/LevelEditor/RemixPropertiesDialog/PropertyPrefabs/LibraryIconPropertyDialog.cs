using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

using Jukebox;
using TMPro;
using SFB;
using System.Linq;
using System;
using HeavenStudio.Common;

namespace HeavenStudio.Editor
{
    public class LibraryIconPropertyDialog : RemixPropertyPrefab
    {
        [SerializeField] Image customIconPreview;
        [SerializeField] Toggle useText;

        [SerializeField] Image game1;
        [SerializeField] Image game2;
        [SerializeField] Image game3;
        [SerializeField] Image game4;
        [SerializeField] TMP_Text iconText;

        Sprite customIcon;

        RemixPropertiesDialog diag;

        new public void InitProperties(RemixPropertiesDialog diag, string propertyName, string caption)
        {
            this.diag = diag;
            diag.StartCoroutine(LoadCustomImage());

            UpdateInfo();
        }

        void UpdateInfo()
        {
            if ((string)diag.chart["remixtitle"] == "New Remix" || string.IsNullOrEmpty((string)diag.chart["remixtitle"]))
            {
                string filename = Path.GetFileName(Editor.instance.currentRemixPath);

                if (string.IsNullOrEmpty(filename))
                {
                    iconText.text = Localization.T("New Remix");
                }
            }
            else
            {
                iconText.text = diag.chart["remixtitle"];
            }

            #region Icons
            string[] icons = new string[4] { "noGame", "noGame", "noGame", "noGame" };

            List<RiqEntity> allGameSwitches = EventCaller.GetAllInBeatmap(diag.chart, "gameManager", new string[] { "switchGame" });

            List<string> uniqueGames = new();
            uniqueGames.AddRange(allGameSwitches.Select(entity => entity.datamodel.Split('/')[2]));
            uniqueGames = uniqueGames.Distinct().ToList(); 

            int maxGames = Math.Min(uniqueGames.Count, 4);
            for (int i = 0; i < maxGames; i++)
            {
                icons[i] = uniqueGames[i];
            }

            SetLevelIcons(icons);
            #endregion
        }

        private void SetLevelIcons(string[] icons)
        {
            game2.sprite = LibraryLevelIcon.GetIcon(icons[1]);
            game3.sprite = LibraryLevelIcon.GetIcon(icons[2]);
            game4.sprite = LibraryLevelIcon.GetIcon(icons[3]);

            if (icons[1] == "noGame")
            {
                game1.sprite = LibraryLevelIcon.GetIcon(icons[0], false);

                game1.rectTransform.localScale = new Vector3(2, 2, 2);
                game2.gameObject.SetActive(false);
                game3.gameObject.SetActive(false);
                game4.gameObject.SetActive(false);
            }
            else
            {
                game1.sprite = LibraryLevelIcon.GetIcon(icons[0]);

                game1.rectTransform.localScale = new Vector3(1, 1, 1);
                game2.gameObject.SetActive(true);
                game3.gameObject.SetActive(true);
                game4.gameObject.SetActive(true);
            }
        }


        public void UploadImage()
        {
            var extensions = new[]
            {
                new ExtensionFilter("Image Files", "png", "jpg", "jpeg" ),
            };

            FileBrowserOpener.OpenFilePanelAsync("Open Image", "", extensions, false, (string[] paths) =>
            {
                var path = Path.Combine(paths);
                if (path == string.Empty)
                {
                    return;
                }

                try
                {
                    StartCoroutine(UploadImage(path));
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error uploading image: {e.Message}");
                    GlobalGameManager.ShowErrorMessage("Error Uploading Image", e.Message + "\n\n" + e.StackTrace);
                    return;
                }
            });
        }

        public void RemoveImage()
        {

            try
            {
                RiqFileHandler.GetResourcePath("LibraryLevelIcon", "Images/LibraryIcon/");
            }
            catch (System.Exception)
            {
                Debug.LogError($"Error removing image: No image to remove");
                return;
            }

            RiqFileHandler.RemoveResource("LibraryLevelIcon", "Images/LibraryIcon");
            customIcon = null;
            customIconPreview.sprite = null;
            customIconPreview.gameObject.SetActive(false);

            SetTextStatus();
        }

        public void SetTextStatus()
        {
            if (customIcon == null)
            {
                diag.chart["libraryicontitle"] = true;
                useText.interactable = false;
            }
            else
            {
                diag.chart["libraryicontitle"] = useText.isOn;
                useText.interactable = true;
            }

            iconText.gameObject.SetActive((bool)diag.chart["libraryicontitle"]);
            useText.SetIsOnWithoutNotify((bool)diag.chart["libraryicontitle"]);
        }

        IEnumerator LoadCustomImage()
        {
            bool getPathFailed = false;
            string path = "";
            try
            {
                path = RiqFileHandler.GetResourcePath("LibraryLevelIcon", "Images/LibraryIcon/");
            }
            catch (System.Exception e)
            {
                Debug.Log($"Error loading library icon image: {e.Message}");
                customIcon = null;
                customIconPreview.sprite = null;
                customIconPreview.gameObject.SetActive(false);
                getPathFailed = true;
            }

            if (!getPathFailed)
            {
                UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + path);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                    customIcon = null;
                    customIconPreview.sprite = null;
                    customIconPreview.gameObject.SetActive(false);
                }
                else
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);
                    customIcon = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    customIconPreview.sprite = customIcon;
                    customIconPreview.gameObject.SetActive(true);

                    useText.SetIsOnWithoutNotify(false);

                    Debug.Log($"Downloaded custom level icon successfully!");
                }
            }

            SetTextStatus();
        }

        IEnumerator UploadImage(string path)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + path);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                customIcon = null;
                customIconPreview.sprite = null;
                customIconPreview.gameObject.SetActive(false);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                customIcon = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                customIconPreview.sprite = customIcon;
                customIconPreview.gameObject.SetActive(true);

                useText.SetIsOnWithoutNotify(false);

                RiqFileHandler.AddResource(path, "LibraryLevelIcon", "Images/LibraryIcon/");
                Debug.Log("Uploaded image successfully!");
            }

            SetTextStatus();
        }
    }
}