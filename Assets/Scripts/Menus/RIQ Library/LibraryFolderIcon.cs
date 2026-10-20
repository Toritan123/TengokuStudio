using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HeavenStudio.Util;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace HeavenStudio
{
    public class LibraryFolderIcon : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
    {
        [SerializeField] private Image[] recolorables;
        [SerializeField] public Image customIcon;
        [SerializeField] private TMP_Text folderNameLabel;
        [SerializeField] public string folderPath;
        [SerializeField] public string folderName;
        [SerializeField] public LibraryFolderData folderData = new();
        [SerializeField] public bool loadedIcon;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip hover;

        [Space]
        [SerializeField] private GameObject toast;
        [SerializeField] private Image toastBorder;
        [SerializeField] private Image toastCustomIcon;
        [SerializeField] private Image toastIcon1;
        [SerializeField] private Image toastIcon2;
        [SerializeField] private Image toastIcon3;
        [SerializeField] private Image toastIcon4;
        [SerializeField] private bool toastVisible;
        [SerializeField] private float toastAnimationTime;
        [SerializeField] Coroutine toastUpCoroutine;
        [SerializeField] Coroutine toastDownCoroutine;
        [SerializeField] Coroutine toastInfoCoroutine;
        [SerializeField] public List<string> levelsInFolder;

        [Space]
        [SerializeField] private Transform resizeTarget;
        [SerializeField] private float resizeMax;
        [SerializeField] private float resizeAnimationTime;
        [SerializeField] Coroutine hoverCoroutine;
        [SerializeField] Coroutine unhoverCoroutine;
        [SerializeField] LibrarySelectable selectable;


        /// <summary>
        /// Set the path and name of the directory.
        /// </summary>
        /// <param name="path">Full path to the directory</param>
        public void SetFolderPath(string path)
        {
            folderPath = path;
            folderName = Path.GetFileName(folderPath);
            gameObject.name = folderName;
        }

        public void GetFolderData()
        {
            string metadataPath = Path.Combine(folderPath, "folderData.json");

            if (File.Exists(metadataPath))
            {
                string json = File.ReadAllText(metadataPath);
                if (string.IsNullOrEmpty(json))
                {
                    folderData = new();
                    SaveFolderData(metadataPath);
                    return;
                }

                try
                {
                    folderData = JsonConvert.DeserializeObject<LibraryFolderData>(json);
                }
                catch
                {
                    folderData = new();
                    SaveFolderData(metadataPath);
                    return;
                }
            }
            else
            {
                folderData = new();
                SaveFolderData(metadataPath);
                print($"no folder data for {folderPath}");
            }
        }

        public void SaveFolderData(string metadataPath)
        {
            string json = JsonConvert.SerializeObject(folderData);
            File.WriteAllText(metadataPath, json, System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// <para>Set the appearance of the folder.</para>
        /// <para>This will get appearance data from folderData.</para>
        /// <para>For this reason, this should always be run after the info is set with GetFolderData().</para>
        /// </summary>
        public void SetFolderAppearance()
        {
            folderNameLabel.text = folderName;
            folderNameLabel.gameObject.SetActive(folderData.showName);

            bool colorIsSet = false;
            while (!colorIsSet)
            {
                if (ColorUtility.TryParseHtmlString(folderData.color, out Color folderColor))
                {
                    folderColor.a = 1;

                    foreach (Image graphic in recolorables)
                    {
                        graphic.color = folderColor;
                    }

                    colorIsSet = true;
                }
                else
                {
                    folderData.color = "#FFF7B2";
                    foreach (Image graphic in recolorables)
                    {
                        graphic.color = new(1f, 0.9686275f, 0.6980392f, 1f);
                    }
                }
            }
        }

        public Color GetFolderColor()
        {
            return recolorables[0].color;
        }

        private IEnumerator SetLevelCustomIcon()
        {
            if (loadedIcon)
            {
                // print($"already loaded icon for {folderName}");
                yield break;
            }

            string iconPath = Path.Combine(folderPath, "folderIcon.png");

            if (!File.Exists(iconPath))
            {
                customIcon.sprite = null;
                customIcon.transform.parent.gameObject.SetActive(false);
                loadedIcon = true;

                print($"no icon for {folderName}");
            }
            else
            {
                UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + iconPath);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                    customIcon.sprite = null;
                    customIcon.transform.parent.gameObject.SetActive(false);
                    Debug.LogError($"Custom folder icon download failed");
                }
                else
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);
                    Sprite customIconSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    customIcon.sprite = customIconSprite;
                    customIcon.transform.parent.gameObject.SetActive(true);

                    Debug.Log($"Downloaded custom folder icon {folderName} successfully!");
                }
                loadedIcon = true;
            }
        }

        /// <summary>
        /// Set all settings of a folder
        /// </summary>
        /// <param name="path">Full path to the directory</param>
        public void SetFolderInfo(string path)
        {
            loadedIcon = false;

            SetFolderPath(path);
            GetFolderData();
            SetFolderAppearance();
        }

        public string GetFolderDisplayName()
        {
            return folderNameLabel.text;
        }

        public void SelectFolder()
        {
            LibraryManager.instance.SelectFolder(folderPath);
            toast.transform.localPosition = new(toast.transform.localPosition.x, 20f, toast.transform.localPosition.z);

            SoundByte.PlayOneShot("ui/Library/folder");
        }

        public void Back()
        {
            LibraryManager.instance.ReturnToPreviousFolder();
        }

        public void LoadCustomIcon()
        {
            StartCoroutine(SetLevelCustomIcon());
        }

        public bool TryGetCustomSort(out List<string> customSortList)
        {
            customSortList = new();
            List<string> pathsToRemove = folderData.customPathOrder.Where(path => !File.Exists(Path.Combine(folderPath, path + ".riq"))).ToList();

            if (pathsToRemove.Count > 0)
            {
                foreach (string path in pathsToRemove)
                {
                    folderData.customPathOrder.Remove(path);
                }

                SaveFolderData(Path.Combine(folderPath, "folderData.json"));
            }

            if (folderData.customPathOrder.Count == 0)
            {
                return false;
            }

            customSortList.AddRange(folderData.customPathOrder);
            return true;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            GetLevelsInFolder();

            if (levelsInFolder.Count > 0)
            {
                if (toastInfoCoroutine != null)
                {
                    StopCoroutine(toastInfoCoroutine);
                }

                toastInfoCoroutine = StartCoroutine(SetToastInfo(levelsInFolder));
            }

            if (unhoverCoroutine != null)
            {
                StopCoroutine(unhoverCoroutine);
            }
            hoverCoroutine = StartCoroutine(AnimateHover());

            audioSource.PlayOneShot(hover);
        }

        public void GetLevelsInFolder()
        {
            if ((levelsInFolder == null) || (levelsInFolder.Count == 0))
            {
                levelsInFolder = LibraryManager.instance.allIconInfo[folderPath].pathsInFolder.Where(path => !LibraryManager.instance.allIconInfo[path].isFolder).ToList();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (toastUpCoroutine != null)
            {
                StopCoroutine(toastUpCoroutine);
            }
            toastDownCoroutine = StartCoroutine(AnimateToastDown());

            if (hoverCoroutine != null)
            {
                StopCoroutine(hoverCoroutine);
            }
            unhoverCoroutine = StartCoroutine(AnimateUnhover());
        }

        public void OnNavigateTo(bool showInfo = true)
        {
            OnHover();
            if (showInfo) LibraryManager.instance.infoPanel.ShowFolderInfo(folderPath);

            if (!LibraryManager.instance.IsIconFullyOnscreen(gameObject.GetComponent<RectTransform>()))
            {
                LibraryManager.instance.ScrollToTarget(GetComponent<RectTransform>());
            }

            LibraryMenuCursor.instance.SetSelectable(selectable);
            LibraryMenuCursor.instance.gridSelectable = selectable;
            LibraryMenuCursor.instance.DisableMouse();
        }

        public void OnHover()
        {
            GetLevelsInFolder();
            if (levelsInFolder.Count > 0)
            {
                if (toastInfoCoroutine != null)
                {
                    StopCoroutine(toastInfoCoroutine);
                }

                toastInfoCoroutine = StartCoroutine(SetToastInfo(levelsInFolder));
            }

            if (unhoverCoroutine != null)
            {
                StopCoroutine(unhoverCoroutine);
            }
            hoverCoroutine = StartCoroutine(AnimateHover());

            audioSource.PlayOneShot(hover);
        }

        public void OnUnHover()
        {
            if (toastUpCoroutine != null)
            {
                StopCoroutine(toastUpCoroutine);
            }
            toastDownCoroutine = StartCoroutine(AnimateToastDown());

            if (hoverCoroutine != null)
            {
                StopCoroutine(hoverCoroutine);
            }
            unhoverCoroutine = StartCoroutine(AnimateUnhover());
        }

        public void HoverLeft()
        {
            GameObject leftGameObject = LibraryManager.instance.GetGridSelectableLeft(folderPath);

            if (leftGameObject == null) return;

            if (leftGameObject.TryGetComponent(out LibraryLevelIcon levelIcon))
            {
                levelIcon.OnNavigateTo();
            }
            if (leftGameObject.TryGetComponent(out LibraryFolderIcon folderIcon))
            {
                folderIcon.OnNavigateTo();
            }

            OnUnHover();
        }

        public void HoverRight()
        {
            GameObject rightGameObject = LibraryManager.instance.GetGridSelectableRight(folderPath);

            if (rightGameObject == null) return;

            if (rightGameObject.TryGetComponent(out LibraryLevelIcon levelIcon))
            {
                levelIcon.OnNavigateTo();
            }
            if (rightGameObject.TryGetComponent(out LibraryFolderIcon folderIcon))
            {
                folderIcon.OnNavigateTo();
            }

            OnUnHover();
        }

        public void HoverUp()
        {
            GameObject upGameObject = LibraryManager.instance.GetGridSelectableUp(folderPath);

            if (upGameObject == null) return;

            if (upGameObject.TryGetComponent(out LibraryLevelIcon levelIcon))
            {
                levelIcon.OnNavigateTo();
            }
            if (upGameObject.TryGetComponent(out LibraryFolderIcon folderIcon))
            {
                folderIcon.OnNavigateTo();
            }

            OnUnHover();
        }

        public void HoverDown()
        {
            GameObject downGameObject = LibraryManager.instance.GetGridSelectableDown(folderPath);

            if (downGameObject == null) return;

            if (downGameObject.TryGetComponent(out LibraryLevelIcon levelIcon))
            {
                levelIcon.OnNavigateTo();
            }
            if (downGameObject.TryGetComponent(out LibraryFolderIcon folderIcon))
            {
                folderIcon.OnNavigateTo();
            }

            OnUnHover();
        }

        private IEnumerator AnimateToastUp()
        {
            float time = 0;
            float normalizedTime = 0;

            Util.EasingFunction.Function easeFunction = Util.EasingFunction.GetEasingFunction(Util.EasingFunction.Ease.EaseOutQuint);
            float yStart = toast.transform.localPosition.y;
            float yEnd = 45f;

            toastVisible = true;

            while (normalizedTime < 1f)
            {
                normalizedTime = Mathf.InverseLerp(0f, toastAnimationTime, time);

                float newY = easeFunction(yStart, yEnd, normalizedTime);

                toast.transform.localPosition = new(toast.transform.localPosition.x, newY, toast.transform.localPosition.z);

                time += Time.deltaTime;
                yield return null;
            }

            toast.transform.localPosition = new(toast.transform.localPosition.x, yEnd, toast.transform.localPosition.z);
            yield return null;
        }

        private IEnumerator AnimateToastDown()
        {
            float time = 0;
            float normalizedTime = 0;

            Util.EasingFunction.Function easeFunction = Util.EasingFunction.GetEasingFunction(Util.EasingFunction.Ease.EaseOutQuint);
            float yStart = toast.transform.localPosition.y;
            float yEnd = 20f;

            while (normalizedTime < 1f)
            {
                normalizedTime = Mathf.InverseLerp(0f, toastAnimationTime, time);

                float newY = easeFunction(yStart, yEnd, normalizedTime);

                toast.transform.localPosition = new(toast.transform.localPosition.x, newY, toast.transform.localPosition.z);

                time += Time.deltaTime;
                yield return null;
            }

            toast.transform.localPosition = new(toast.transform.localPosition.x, yEnd, toast.transform.localPosition.z);
            toastVisible = false;

            yield return null;
        }

        private IEnumerator SetToastInfo(List<string> levelsInFolder)
        {
            if (!toastVisible)
            {
                System.Random rand = new System.Random();

                string randomLevelPath = levelsInFolder[rand.Next(levelsInFolder.Count)];
                LibraryLevelIcon levelIcon = LibraryManager.instance.allIconInfo[randomLevelPath].levelIcon;

                toastBorder.sprite = levelIcon.border.sprite;

                if (LibraryCacheManager.cache.cachedInfo[LibraryManager.instance.allIconInfo[randomLevelPath].hash].customIcon)
                {
                    toastCustomIcon.gameObject.SetActive(true);
                    if (levelIcon.loadedIcon)
                    {
                        toastCustomIcon.sprite = levelIcon.customIcon.sprite;
                    }
                    else
                    {
                        UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + Path.Combine(LibraryCacheManager.cachePath, "LevelIcons", LibraryManager.instance.allIconInfo[randomLevelPath].hash, "icon.png"));
                        yield return www.SendWebRequest();

                        if (www.result != UnityWebRequest.Result.Success)
                        {
                            Debug.Log(www.error);
                            toastCustomIcon.sprite = null;
                            toastCustomIcon.gameObject.SetActive(false);
                            Debug.LogError($"Custom level icon download failed");
                        }
                        else
                        {
                            Texture2D texture = DownloadHandlerTexture.GetContent(www);
                            Sprite customIconSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                            toastCustomIcon.sprite = customIconSprite;
                            toastCustomIcon.gameObject.SetActive(true);
                        }
                    }
                }
                else
                {
                    toastCustomIcon.gameObject.SetActive(false);
                    toastIcon1.sprite = levelIcon.icon1.sprite;
                    toastIcon2.sprite = levelIcon.icon2.sprite;
                    toastIcon3.sprite = levelIcon.icon3.sprite;
                    toastIcon4.sprite = levelIcon.icon4.sprite;
                    if (toastIcon2.sprite.name == "noGame")
                    {
                        toastIcon1.rectTransform.localScale = new Vector3(2, 2, 2);
                        toastIcon2.gameObject.SetActive(false);
                        toastIcon3.gameObject.SetActive(false);
                        toastIcon4.gameObject.SetActive(false);
                    }
                    else
                    {
                        toastIcon1.rectTransform.localScale = new Vector3(1, 1, 1);
                        toastIcon2.gameObject.SetActive(true);
                        toastIcon3.gameObject.SetActive(true);
                        toastIcon4.gameObject.SetActive(true);
                    }
                }
            }

            if (toastDownCoroutine != null)
            {
                StopCoroutine(toastDownCoroutine);
            }
            toastUpCoroutine = StartCoroutine(AnimateToastUp());
        }

        private IEnumerator AnimateHover()
        {
            float time = 0;
            float normalizedTime = 0;

            Util.EasingFunction.Function easeFunction = Util.EasingFunction.GetEasingFunction(Util.EasingFunction.Ease.EaseOutQuint);
            float scaleStart = resizeTarget.localScale.y;
            float scaleEnd = resizeMax;

            while (normalizedTime < 1f)
            {
                normalizedTime = Mathf.InverseLerp(0f, resizeAnimationTime, time);

                float newScale = easeFunction(scaleStart, scaleEnd, normalizedTime);

                resizeTarget.localScale = Vector3.one * newScale;

                time += Time.deltaTime;
                yield return null;
            }

            resizeTarget.localScale = Vector3.one * scaleEnd;
            yield return null;
        }

        private IEnumerator AnimateUnhover()
        {
            float time = 0;
            float normalizedTime = 0;

            Util.EasingFunction.Function easeFunction = Util.EasingFunction.GetEasingFunction(Util.EasingFunction.Ease.EaseOutQuint);
            float scaleStart = resizeTarget.localScale.y;
            float scaleEnd = 1f;

            while (normalizedTime < 1f)
            {
                normalizedTime = Mathf.InverseLerp(0f, resizeAnimationTime, time);

                float newScale = easeFunction(scaleStart, scaleEnd, normalizedTime);

                resizeTarget.localScale = Vector3.one * newScale;

                time += Time.deltaTime;
                yield return null;
            }

            resizeTarget.localScale = Vector3.one * scaleEnd;
            yield return null;
        }

        [Serializable]
        public class LibraryFolderData
        {
            public bool showName = true;
            public bool showStats = false;
            public string color = "#FFF7B2";
            public string creator = "";
            public string description = "";
            public LibraryManager.LibrarySortType sortType = LibraryManager.LibrarySortType.Filename;
            public bool sortAscending = true;
            public List<string> customPathOrder = new();

        }
    }
}