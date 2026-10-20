using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HeavenStudio.Util;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace HeavenStudio
{
    public class LibraryLevelIcon : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private GameObject iconHolder;
        [SerializeField] public Image border;
        [SerializeField] public Image customIcon;
        [SerializeField] public Image icon1;
        [SerializeField] public Image icon2;
        [SerializeField] public Image icon3;
        [SerializeField] public Image icon4;
        [SerializeField] private Image miniSkillStar;
        [SerializeField] private Image medal;
        [SerializeField] private GameObject heart;
        [SerializeField] private TMP_Text levelName;
        [SerializeField] public string levelPath;
        [SerializeField] public string fileName;
        [SerializeField] public bool loadedIcon;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float heartInAnimationTime;
        [SerializeField] private float heartOutAnimationTime;
        [SerializeField] public bool needsIcon;

        [Space]
        [SerializeField] private Sprite unplayedBorder;
        [SerializeField] private Sprite tryAgainBorder;
        [SerializeField] private Sprite okBorder;
        [SerializeField] private Sprite superbBorder;
        [SerializeField] private Sprite perfectBorder;
        [SerializeField] private Sprite skillStarInactive;
        [SerializeField] private Sprite skillStarActive;
        [SerializeField] private Sprite superbMedal;
        [SerializeField] private Sprite perfectMedal;
        [SerializeField] private AudioClip favorite;
        [SerializeField] private AudioClip unfavorite;
        [SerializeField] private AudioClip hover;

        [Space]
        [SerializeField] private Transform resizeTarget;
        [SerializeField] private float resizeMax;
        [SerializeField] private float resizeAnimationTime;
        [SerializeField] Coroutine hoverCoroutine;
        [SerializeField] Coroutine unhoverCoroutine;
        [SerializeField] LibrarySelectable selectable;

        /// <summary>
        /// Set the file path and file name of the level.
        /// </summary>
        /// <param name="path">Full path to the .riq file</param>
        public void SetLevelPath(string path)
        {
            levelPath = path;
            fileName = Path.GetFileName(levelPath);
            gameObject.name = fileName;
        }

        /// <summary>
        /// <para>Set the display name of the level.</para>
        /// <para>If the provided string is empty or "New Remix", it will use the file name.</para>
        /// <para>For this reason, this should always be run after the file name is set with SetLevelPath().</para>
        /// </summary>
        /// <param name="name">The name to set. This should be "Level Name" in the chart properties.</param>
        public void SetLevelName(string name, bool useName)
        {
            if (name == "New Remix" || string.IsNullOrEmpty(name))
            {
                levelName.text = fileName;
            }
            else
            {
                levelName.text = name;
            }

            levelName.gameObject.SetActive(useName);
        }

        /// <summary>
        /// <para>Set the icons of the game based on an array of game names.</para>
        /// <para>"noGame" will use a blank square.</para>
        /// </summary>
        /// <param name="icons">4-long string array of game names</param>
        public void SetLevelIcons(string[] icons)
        {
            if (icons[1] == "noGame")
            {
                icon1.sprite = GetIcon(icons[0], false);
                icon1.rectTransform.localScale = new Vector3(2, 2, 2);
                icon2.gameObject.SetActive(false);
                icon3.gameObject.SetActive(false);
                icon4.gameObject.SetActive(false);
            }
            else
            {
                icon1.sprite = GetIcon(icons[0]);
            }
            icon2.sprite = GetIcon(icons[1]);
            icon3.sprite = GetIcon(icons[2]);
            icon4.sprite = GetIcon(icons[3]);

        }

        public static Sprite GetIcon(string name, bool simple = true)
        {
            if ((!simple) && (name == "rhythmTweezers"))
            {
                float rand = UnityEngine.Random.Range(0f, 1000f);
                if (rand <= 1f)
                {
                    name = "rhythmTweezers_alt";
                }
            }

            if (simple) return Resources.Load<Sprite>($"Sprites/Editor/SimpleIcons/{name}") ?? Resources.Load<Sprite>($"Sprites/Editor/SimpleIcons/noGame");
            else {return Resources.Load<Sprite>($"Sprites/Editor/GameIcons/{name}") ?? Resources.Load<Sprite>($"Sprites/Editor/GameIcons/noGame");}
        }

        public void SetLevelStatus(string hash, bool hasSkillStar)
        {
            bool hasSavedData = LibrarySaveManager.savedata.levelInfo.ContainsKey(hash);

            if (hasSavedData)
            {
                int score = LibrarySaveManager.savedata[hash].score;
                if (LibrarySaveManager.savedata[hash].perfect)
                {
                    border.sprite = perfectBorder;
                    medal.sprite = perfectMedal;
                    medal.gameObject.SetActive(true);
                }
                else if (score < Games.Minigame.rankOkThreshold * 100)
                {
                    border.sprite = tryAgainBorder;
                    medal.gameObject.SetActive(false);
                }
                else if (score < Games.Minigame.rankHiThreshold * 100)
                {
                    border.sprite = okBorder;
                    medal.gameObject.SetActive(false);
                }
                else
                {
                    border.sprite = superbBorder;
                    medal.sprite = superbMedal;
                    medal.gameObject.SetActive(true);
                }
            }
            else
            {
                border.sprite = unplayedBorder;
                medal.gameObject.SetActive(false);
            }

            if (hasSkillStar)
            {
                miniSkillStar.gameObject.SetActive(true);
                if (hasSavedData)
                {
                    miniSkillStar.sprite = LibrarySaveManager.savedata[hash].skillStar ? skillStarActive : skillStarInactive;
                }
                else
                {
                    miniSkillStar.sprite = skillStarInactive;
                }
            }
            else
            {
                miniSkillStar.gameObject.SetActive(false);
            }

            if (LibraryCacheManager.cache.favorites.Contains(levelPath))
            {
                heart.SetActive(true);
                heart.transform.localScale = Vector3.one;
            }
            else
            {
                heart.SetActive(false);
                heart.transform.localScale = Vector3.zero;
            }
        }

        private IEnumerator SetLevelCustomIcon(string levelHash, bool useCustomIcon)
        {
            if (loadedIcon)
            {
                yield break;
            }

            if (!useCustomIcon)
            {
                customIcon.sprite = null;
                customIcon.gameObject.SetActive(false);
                loadedIcon = true;
            }
            else
            {
                iconHolder.gameObject.SetActive(false);

                UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + Path.Combine(LibraryCacheManager.cachePath, "LevelIcons", levelHash, "icon.png"));
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                    customIcon.sprite = null;
                    customIcon.gameObject.SetActive(false);
                    Debug.LogError($"Custom level icon download failed");
                }
                else
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);
                    Sprite customIconSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    customIcon.sprite = customIconSprite;
                    customIcon.gameObject.SetActive(true);

                }
                iconHolder.gameObject.SetActive(true);
                loadedIcon = true;
            }
        }

        /// <summary>
        /// Set all settings of a level icon
        /// </summary>
        /// <param name="path">Full path to the .riq file</param>
        /// <param name="info">CachedLevelInfo containing the level name, icons, hash, and more</param>
        public void SetIconInfo(string path, string hash)
        {
            CachedLevelInfo info = LibraryCacheManager.cache.cachedInfo[hash];

            SetLevelPath(path);
            SetLevelIcons(info.icons);
            SetLevelName(info.levelName, info.useNameInIcon);
            SetLevelStatus(hash, info.hasSkillStar);

            if (info.customIcon)
            {
                needsIcon = true;
            }
        }

        public void LoadCustomIcon()
        {
            string hash = LibraryManager.instance.allIconInfo[levelPath].hash;

            StartCoroutine(SetLevelCustomIcon(hash, LibraryCacheManager.cache.cachedInfo[hash].customIcon));
        }

        public string GetLevelDisplayName()
        {
            return levelName.text;
        }

        public void SelectLevel()
        {
            LibraryManager.instance.SelectLevel(levelPath, false, false);
        }

        public void CursorSelectLevel()
        {
            LibraryManager.instance.SelectLevel(levelPath, true);
        }

        public void Back()
        {
            LibraryManager.instance.ReturnToPreviousFolder();
        }

        public void ToggleFavorite()
        {
            if (LibraryManager.instance.allIconInfo["Favorites"].pathsInFolder.Contains(levelPath))
            {
                LibraryManager.instance.RemoveFromFavorites(levelPath);
                audioSource.PlayOneShot(unfavorite);
                StopAllCoroutines();
                StartCoroutine(AnimateHeartOut());
            }
            else
            {
                LibraryManager.instance.AddToFavorites(levelPath);
                audioSource.PlayOneShot(favorite);
                StopAllCoroutines();
                StartCoroutine(AnimateHeartIn());
            }
        }

        private IEnumerator AnimateHeartIn()
        {
            float time = 0;
            float normalizedTime = 0;

            Util.EasingFunction.Function easeFunction = Util.EasingFunction.GetEasingFunction(Util.EasingFunction.Ease.EaseOutBack);
            float scaleStart = heart.transform.localScale.x;
            float scaleEnd = 1f;

            heart.SetActive(true);

            while (normalizedTime < 1f)
            {
                normalizedTime = Mathf.InverseLerp(0f, heartInAnimationTime, time);

                float newScale = easeFunction(scaleStart, scaleEnd, normalizedTime);

                heart.transform.localScale = Vector3.one * newScale;

                time += Time.deltaTime;
                yield return null;
            }

            heart.transform.localScale = Vector3.one * scaleEnd;

            yield return null;
        }
        private IEnumerator AnimateHeartOut()
        {
            float time = 0;
            float normalizedTime = 0;

            Util.EasingFunction.Function easeFunction = Util.EasingFunction.GetEasingFunction(Util.EasingFunction.Ease.Linear);
            float scaleStart = heart.transform.localScale.x;
            float scaleEnd = 0f;

            while (normalizedTime < 1f)
            {
                normalizedTime = Mathf.InverseLerp(0f, heartOutAnimationTime, time);

                float newScale = easeFunction(scaleStart, scaleEnd, normalizedTime);

                heart.transform.localScale = Vector3.one * newScale;

                time += Time.deltaTime;
                yield return null;
            }

            heart.transform.localScale = Vector3.one * scaleEnd;
            heart.SetActive(false);

            yield return null;
        }

        public void OnPointerClick(PointerEventData pointerEventData)
        {
            if (pointerEventData.button == PointerEventData.InputButton.Right)
            {
                ToggleFavorite();
            }

            if (pointerEventData.button == PointerEventData.InputButton.Left)
            {
                SelectLevel();
            }
        }

        public void OnNavigateTo(bool showInfo = true)
        {
            OnHover();
            LibraryManager.instance.selectedLevelPath = levelPath;
            if (showInfo) LibraryManager.instance.infoPanel.ShowLevelInfo(LibraryManager.instance.allIconInfo[levelPath].hash);

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
            if (unhoverCoroutine != null)
            {
                StopCoroutine(unhoverCoroutine);
            }
            hoverCoroutine = StartCoroutine(AnimateHover());

            audioSource.PlayOneShot(hover);
        }

        public void OnUnHover()
        {
            if (hoverCoroutine != null)
            {
                StopCoroutine(hoverCoroutine);
            }
            unhoverCoroutine = StartCoroutine(AnimateUnhover());
        }

        public void HoverLeft()
        {
            GameObject leftGameObject = LibraryManager.instance.GetGridSelectableLeft(levelPath);

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
            GameObject rightGameObject = LibraryManager.instance.GetGridSelectableRight(levelPath);

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
            GameObject upGameObject = LibraryManager.instance.GetGridSelectableUp(levelPath);

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
            GameObject downGameObject = LibraryManager.instance.GetGridSelectableDown(levelPath);

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

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (unhoverCoroutine != null)
            {
                StopCoroutine(unhoverCoroutine);
            }
            hoverCoroutine = StartCoroutine(AnimateHover());

            audioSource.PlayOneShot(hover);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (hoverCoroutine != null)
            {
                StopCoroutine(hoverCoroutine);
            }
            unhoverCoroutine = StartCoroutine(AnimateUnhover());
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


    }
}