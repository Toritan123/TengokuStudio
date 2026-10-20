using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

using Jukebox;
using TMPro;
using SFB;
using HeavenStudio.Common;

namespace HeavenStudio.Editor
{
    public class RatingScreenPropertyDialog : RemixPropertyPrefab
    {
        enum Ranks
        {
            Ng,
            Ok,
            Hi
        }

        [SerializeField] TMP_InputField headerInput;
        [SerializeField] TMP_InputField messageInput;
        [SerializeField] TMP_InputField epilogueInput;

        [SerializeField] Image imagePreview;
        [SerializeField] Image rankPreview;

        [SerializeField] Sprite catOff;
        [SerializeField] Button[] catButtons;
        [SerializeField] Sprite[] catSprites;
        [SerializeField] TMP_Text ratingLabel;

        // [SerializeField] Sprite[] rankSprites;
        [SerializeField] Image ngIcon;
        [SerializeField] Image okIcon;
        [SerializeField] Image hiIcon;

        [SerializeField] Sprite frameNg, frameOk, frameHi;
        [SerializeField] Image frame;

        [SerializeField] Sprite epilogueNg, epilogueOk, epilogueHi;

        Sprite[] rankImages;

        bool initHooks;
        int currentEditingCat = -1;
        List<int> usedCategories;
        RemixPropertiesDialog diag;
        Ranks currentEditingRank;

        new public void InitProperties(RemixPropertiesDialog diag, string propertyName, string caption)
        {
            this.diag = diag;
            currentEditingRank = Ranks.Ok;
            rankImages = new Sprite[3];
            diag.StartCoroutine(LoadRankImages());
            GetUsedCategories();

            if (!initHooks)
            {
                initHooks = true;

                for (int i = 0; i < catButtons.Length; i++)
                {
                    int cat = i;
                    catButtons[i].onClick.AddListener(() =>
                    {
                        SetCatEditing(cat);
                    });
                }

                headerInput.onValueChanged.AddListener(
                    _ =>
                    {
                        diag.chart["resultcaption"] = headerInput.text;
                    }
                );

                epilogueInput.onValueChanged.AddListener(
                    _ =>
                    {
                        string propSuffix = currentEditingRank switch
                        {
                            Ranks.Ng => "ng",
                            Ranks.Hi => "hi",
                            _ => "ok",
                        };
                        diag.chart["epilogue_" + propSuffix] = epilogueInput.text;
                    }
                );

                messageInput.onValueChanged.AddListener(DoMessageInput);
            }

            UpdateInfo();
            SetCatEditing(usedCategories[0]);
        }

        void UpdateInfo()
        {
            headerInput.text = (string)diag.chart["resultcaption"];
            if (rankImages[(int)currentEditingRank] != null)
            {
                imagePreview.sprite = rankImages[(int)currentEditingRank];
            }
            else
            {
                imagePreview.sprite = currentEditingRank switch
                {
                    Ranks.Ng => epilogueNg,
                    Ranks.Hi => epilogueHi,
                    _ => epilogueOk,
                };
            }
            imagePreview.preserveAspect = true;

            // rankPreview.sprite = rankSprites[(int)currentEditingRank];
            SetIconAndFrame();

            string propSuffix = currentEditingRank switch
            {
                Ranks.Ng => "ng",
                Ranks.Hi => "hi",
                _ => "ok",
            };
            epilogueInput.text = (string)diag.chart["epilogue_" + propSuffix];
            SetCatEditing(currentEditingCat);
        }

        void DoMessageInput(string _)
        {
            string propSuffix = currentEditingRank switch
            {
                Ranks.Ng => "ng",
                Ranks.Hi => "hi",
                _ => "ok",
            };

            if (usedCategories.Count == 1 || currentEditingRank == Ranks.Ok)
            {
                diag.chart["resultcommon_" + propSuffix] = messageInput.text;
            }
            else
            {
                diag.chart["resultcat" + currentEditingCat + "_" + propSuffix] = messageInput.text;
            }
        }

        void SetCatEditing(int cat)
        {
            string propSuffix = currentEditingRank switch
            {
                Ranks.Ng => "ng",
                Ranks.Hi => "hi",
                _ => "ok",
            };

            cat = Mathf.Clamp(cat, 0, catButtons.Length - 1);

            if (usedCategories.Count == 1 || currentEditingRank == Ranks.Ok)
            {
                currentEditingCat = usedCategories[0];
                for (int i = 0; i < catButtons.Length; i++)
                {
                    catButtons[i].gameObject.SetActive(false);
                }

                ratingLabel.text = Localization.T("Rating Message");

                messageInput.text = (string)diag.chart["resultcommon_" + propSuffix];
            }
            else
            {
                currentEditingCat = cat;
                for (int i = 0; i < catButtons.Length; i++)
                {
                    catButtons[i].gameObject.SetActive(usedCategories.Contains(i));
                    if (i == currentEditingCat)
                        catButtons[i].GetComponent<Image>().sprite = catSprites[i];
                    else
                        catButtons[i].GetComponent<Image>().sprite = catOff;
                }

                ratingLabel.text = Localization.F("Rating Message <sprite=\"categoryMarker\" index={0}>", currentEditingCat + 1);

                messageInput.text = (string)diag.chart["resultcat" + currentEditingCat + "_" + propSuffix];
            }
        }

        void GetUsedCategories()
        {
            RiqBeatmap chart = diag.chart;
            usedCategories = new();
            if (chart.data.beatmapSections.Count == 0)
            {
                usedCategories.Add(0);
                return;
            }
            foreach (var section in chart.data.beatmapSections)
            {
                int cat = section["category"];
                if (!usedCategories.Contains(cat))
                {
                    usedCategories.Add(cat);
                }
            }
            usedCategories.Sort();
        }

        public void GoPrevRank()
        {
            currentEditingRank--;
            if (currentEditingRank < 0)
                currentEditingRank = Ranks.Hi;
            UpdateInfo();
        }

        public void GoNextRank()
        {
            currentEditingRank++;
            if (currentEditingRank > Ranks.Hi)
                currentEditingRank = Ranks.Ng;
            UpdateInfo();
        }

        public void GoNgRank()
        {
            currentEditingRank = Ranks.Ng;
            UpdateInfo();
        }

        public void GoOkRank()
        {
            currentEditingRank = Ranks.Ok;
            UpdateInfo();
        }

        public void GoHiRank()
        {
            currentEditingRank = Ranks.Hi;
            UpdateInfo();
        }

        void SetIconAndFrame()
        {
            switch (currentEditingRank)
            {
                case Ranks.Ng:
                    ngIcon.color = new Color(1f, 1f, 1f);
                    okIcon.color = new Color(0.5f, 0.5f, 0.5f);
                    hiIcon.color = new Color(0.5f, 0.5f, 0.5f);
                    frame.sprite = frameNg;
                    break;
                case Ranks.Ok:
                    ngIcon.color = new Color(0.5f, 0.5f, 0.5f);
                    okIcon.color = new Color(1f, 1f, 1f);
                    hiIcon.color = new Color(0.5f, 0.5f, 0.5f);
                    frame.sprite = frameOk;
                    break;
                case Ranks.Hi:
                    ngIcon.color = new Color(0.5f, 0.5f, 0.5f);
                    okIcon.color = new Color(0.5f, 0.5f, 0.5f);
                    hiIcon.color = new Color(1f, 1f, 1f);
                    frame.sprite = frameHi;
                    break;
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
                    // fetch the image using UnityWebRequest
                    string resource = currentEditingRank switch
                    {
                        Ranks.Ng => "Ng",
                        Ranks.Hi => "Hi",
                        _ => "Ok",
                    };
                    StartCoroutine(UploadImage(path, currentEditingRank));
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error uploading image: {e.Message}");
                    GlobalGameManager.ShowErrorMessage("Error Uploading Image", e.Message + "\n\n" + e.StackTrace);
                    return;
                }
            });
        }

        IEnumerator LoadRankImages()
        {
            for (Ranks i = 0; i <= Ranks.Hi; i++)
            {
                string resource = i switch
                {
                    Ranks.Ng => "Ng",
                    Ranks.Hi => "Hi",
                    _ => "Ok",
                };
                string path;
                try
                {
                    path = RiqFileHandler.GetResourcePath(resource, "Images/Epilogue/");
                }
                catch (System.Exception e)
                {
                    Debug.Log($"Error loading image: {e.Message}, using fallback");
                    rankImages[(int)i] = null;
                    continue;
                }

                UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + path);
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log(www.error);
                    rankImages[(int)i] = null;
                }
                else
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);
                    rankImages[(int)i] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    Debug.Log($"Downloaded image {resource} successfully!");
                }
            }

            if (rankImages[(int)currentEditingRank] != null)
            {
                imagePreview.sprite = rankImages[(int)currentEditingRank];
            }
            else
            {
                imagePreview.sprite = currentEditingRank switch
                {
                    Ranks.Ng => epilogueNg,
                    Ranks.Hi => epilogueHi,
                    _ => epilogueOk,
                };
            }
            imagePreview.preserveAspect = true;
        }

        IEnumerator UploadImage(string path, Ranks rank)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + path);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
                rankImages[(int)rank] = null;
                imagePreview.sprite = rank switch
                {
                    Ranks.Ng => epilogueNg,
                    Ranks.Hi => epilogueHi,
                    _ => epilogueOk,
                };
                imagePreview.preserveAspect = true;
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                rankImages[(int)rank] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                imagePreview.sprite = rankImages[(int)rank];
                imagePreview.preserveAspect = true;

                string resource = rank switch
                {
                    Ranks.Ng => "Ng",
                    Ranks.Hi => "Hi",
                    _ => "Ok",
                };

                RiqFileHandler.AddResource(path, resource, "Images/Epilogue/");
                Debug.Log("Uploaded image successfully!");
            }
        }
    }
}