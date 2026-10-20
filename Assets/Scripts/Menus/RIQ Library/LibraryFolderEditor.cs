using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using HeavenStudio.Util;
using HeavenStudio.Common;

namespace HeavenStudio
{
    public class LibraryFolderEditor : MonoBehaviour
    {
        [SerializeField] private LibraryInfoPanel infoPanel;

        [Header("Data")]
        [SerializeField] private LibraryFolderIcon editingIcon;
        [SerializeField] private bool changedName => nameField.text != editingIcon.folderName;
        [SerializeField] private bool changedCreator => creatorField.text != editingIcon.folderData.creator;
        [SerializeField] private bool changedDesc => descField.text != editingIcon.folderData.description;
        [SerializeField] private bool changedColor => ("#" + ColorUtility.ToHtmlStringRGB(colorPicker.currentColor)) != editingIcon.folderData.color;
        [SerializeField] private bool changedShowName => nameToggle.isOn != editingIcon.folderData.showName;
        [SerializeField] private bool changedShowStats => statsToggle.isOn != editingIcon.folderData.showStats;
        [SerializeField] private bool changedSorting => CheckIfSortingChanged();
        [SerializeField] private bool changedIcon = false;
        [SerializeField] private Sprite customIconSprite;
        [SerializeField] private bool startedWithIcon = false;
        [SerializeField] private List<LibraryCustomSortEntry> sortEntries = new();
        [SerializeField] private bool deletePossible = false;


        [Header("Settings")]
        [SerializeField] private TMP_InputField nameField;
        [SerializeField] private TMP_InputField creatorField;
        [SerializeField] private TMP_InputField descField;
        [SerializeField] private LibraryColorPreview colorPicker;
        [SerializeField] private Toggle nameToggle;
        [SerializeField] private Toggle statsToggle;
        [SerializeField] private GameObject invalidNameWarning;
        [SerializeField] private GameObject noNameWarning;
        [SerializeField] private GameObject subfolderExportWarning;
        [SerializeField] private GameObject saveButton;
        [SerializeField] private GameObject exportButton;
        [SerializeField] private TMP_Text deleteText;
        [SerializeField] private Transform customSortParent;
        [SerializeField] private LibraryCustomSortEntry customSortPrefab;

        [Header("Icon Preview")]
        [SerializeField] private Image[] recolorables;
        [SerializeField] private Image customIcon;
        [SerializeField] private TMP_Text folderNameLabel;

        void Start()
        {
            colorPicker.onColorUpdated += color => SetIconColors(color);
        }

        public void StartEditing(LibraryFolderIcon folderToEdit)
        {
            editingIcon = folderToEdit;

            customIconSprite = null;

            changedIcon = false;

            nameField.SetTextWithoutNotify(editingIcon.folderName);
            folderNameLabel.text = editingIcon.folderName;
            folderNameLabel.gameObject.SetActive(editingIcon.folderData.showName);

            creatorField.SetTextWithoutNotify(editingIcon.folderData.creator);
            descField.SetTextWithoutNotify(editingIcon.folderData.description);
            nameToggle.SetIsOnWithoutNotify(editingIcon.folderData.showName);
            statsToggle.SetIsOnWithoutNotify(editingIcon.folderData.showStats);

            colorPicker.ChangeColor(editingIcon.GetFolderColor());
            SetIconColors(editingIcon.GetFolderColor());

            if (editingIcon.customIcon.transform.parent.gameObject.activeSelf)
            {
                customIcon.sprite = editingIcon.customIcon.sprite;
                customIcon.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                customIcon.transform.parent.gameObject.SetActive(false);
            }

            deleteText.text = Localization.T("Delete folder");
            deletePossible = false;

            InitCustomSort();

            SetSaveValidity();
        }

        private void InitCustomSort()
        {
            foreach (LibraryCustomSortEntry entry in sortEntries)
            {
                Destroy(entry.gameObject);
            }

            sortEntries.Clear();

            editingIcon.TryGetCustomSort(out List<string> customSortList);

            List<LibraryLevelIcon> allLevelIcons = LibraryManager.instance.allIconInfo.Values.Where(iconInfo => !iconInfo.isFolder).Select(iconInfo => iconInfo.levelIcon).ToList();

            List<LibraryLevelIcon> customSortedIcons = allLevelIcons.Where(level => LibraryManager.instance.allIconInfo[editingIcon.folderPath].pathsInFolder.Contains(level.levelPath))
                                                                        .Where(level => customSortList.Contains(Path.GetFileNameWithoutExtension(level.fileName)))
                                                                        .ToList();

            List<LibraryLevelIcon> notSortedIcons = allLevelIcons.Where(level => LibraryManager.instance.allIconInfo[editingIcon.folderPath].pathsInFolder.Contains(level.levelPath) && !customSortList.Contains(Path.GetFileNameWithoutExtension(level.fileName)))
                                                                 .ToList();

            customSortedIcons = customSortedIcons.OrderBy(level => customSortList.IndexOf(Path.GetFileNameWithoutExtension(level.fileName))).ToList();
            notSortedIcons = notSortedIcons.OrderBy(level => level.GetLevelDisplayName()).ThenBy(level => level.fileName).ToList();

            Dictionary<string, string> orderedPathsAndNames = new();

            foreach (LibraryLevelIcon level in customSortedIcons)
            {
                orderedPathsAndNames.Add(level.levelPath, level.GetLevelDisplayName());
            }

            foreach (LibraryLevelIcon level in notSortedIcons)
            {
                orderedPathsAndNames.Add(level.levelPath, level.GetLevelDisplayName());
            }

            foreach (KeyValuePair<string, string> kvp in orderedPathsAndNames)
            {
                LibraryCustomSortEntry newSortEntry = Instantiate(customSortPrefab, customSortParent);
                newSortEntry.gameObject.SetActive(true);
                newSortEntry.SetInfo(kvp.Value, kvp.Key);

                sortEntries.Add(newSortEntry);
            }
        }

        private void SetIconColors(Color color)
        {
            foreach (Image image in recolorables)
            {
                image.color = color;
            }

            SetSaveValidity();
        }

        private void SetSaveValidity()
        {
            if (editingIcon == null) return;

            invalidNameWarning.SetActive(false);
            noNameWarning.SetActive(false);
            subfolderExportWarning.SetActive(false);

            if (!(changedName || changedCreator || changedDesc || changedColor || changedShowName || changedShowStats || changedSorting || changedIcon))
            {
                saveButton.SetActive(false);
                exportButton.SetActive(true);

                if (Directory.GetDirectories(editingIcon.folderPath).Length > 0)
                {
                    subfolderExportWarning.SetActive(true);
                }

            }
            else
            {
                saveButton.SetActive(true);
                exportButton.SetActive(false);

                if (string.IsNullOrEmpty(nameField.text))
                {
                    noNameWarning.SetActive(true);
                    saveButton.SetActive(false);
                    exportButton.SetActive(false);
                }
                else if (changedName && Directory.Exists(Path.Combine(Path.GetDirectoryName(editingIcon.folderPath), nameField.text)))
                {
                    invalidNameWarning.SetActive(true);
                    saveButton.SetActive(false);
                    exportButton.SetActive(false);
                }

            }
        }

        #region Buttons

        public void SelectImage()
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
            customIcon.transform.parent.gameObject.SetActive(false);

            if (File.Exists(Path.Combine(editingIcon.folderPath, "folderIcon.png")))
            {
                changedIcon = true;
            }
            else
            {
                changedIcon = false;
            }

            SetSaveValidity();
        }

        public void EditCustomSort()
        {
            LibraryManager.instance.infoPanel.ShowCustomSortEditLayout();
        }

        public void EndEditCustomSort()
        {
            LibraryManager.instance.infoPanel.ShowFolderEditLayout();
            SetSaveValidity();
        }

        public bool CheckIfSortingChanged()
        {
            List<string> currentSortPaths = sortEntries.OrderBy(sortEntry => sortEntry.transform.GetSiblingIndex())
                                                       .Select(sortEntry => Path.GetFileNameWithoutExtension(sortEntry.filePath))
                                                       .ToList();


            return !currentSortPaths.SequenceEqual(editingIcon.folderData.customPathOrder);
        }

        public void SaveChanges()
        {
            if (changedCreator) editingIcon.folderData.creator = creatorField.text;
            if (changedDesc) editingIcon.folderData.description = descField.text;
            if (changedColor) editingIcon.folderData.color = "#" + ColorUtility.ToHtmlStringRGB(colorPicker.currentColor);
            if (changedShowName) editingIcon.folderData.showName = nameToggle.isOn;
            if (changedShowStats) editingIcon.folderData.showStats = statsToggle.isOn;
            if (changedSorting) editingIcon.folderData.customPathOrder = sortEntries.OrderBy(sortEntry => sortEntry.transform.GetSiblingIndex()).Select(sortEntry => Path.GetFileNameWithoutExtension(sortEntry.filePath)).ToList();
            if (changedIcon) SaveCustomIcon(Path.Combine(editingIcon.folderPath, "folderIcon.png"));

            editingIcon.SaveFolderData(Path.Combine(editingIcon.folderPath, "folderData.json"));
            editingIcon.SetFolderInfo(editingIcon.folderPath);

            if (changedName)
            {
                LibraryManager.instance.MovePath(editingIcon.folderPath, Path.Combine(Path.GetDirectoryName(editingIcon.folderPath), nameField.text));
            }

            if (editingIcon.folderData.sortType == LibraryManager.LibrarySortType.Custom)
            {
                LibraryManager.instance.SortByCustomButton();
            }

            LibraryManager.instance.customButton.gameObject.SetActive(true);
            SetSaveValidity();
        }

        public void ExportBriq()
        {
            var extensions = new[]
            {
                new ExtensionFilter("Heaven Studio Level Pack", "briq")
            };

            FileBrowserOpener.SaveFilePanelAsync("Save Level Pack As", "", editingIcon.folderName, extensions, (string path) =>
            {
                if (path != string.Empty)
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    using (ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Create))
                    {
                        foreach (string levelPath in Directory.GetFiles(editingIcon.folderPath, "*", new EnumerationOptions() { RecurseSubdirectories = true }))
                        {
                            archive.CreateEntryFromFile(levelPath, Path.Combine(editingIcon.folderName, Path.GetFileName(levelPath)));
                        }
                    }
                    SoundByte.PlayOneShot("save");
                }
            });
        }

        public void DeleteFolder()
        {
            if (!deletePossible)
            {
                deleteText.text = Localization.T("Are you sure?");
                deletePossible = true;
                return;
            }
            else
            {
                Directory.Delete(editingIcon.folderPath, true);
                SoundByte.PlayOneShot("ui/Library/delete");
                _ = LibraryManager.instance.Refresh();
            }
        }

        public void Exit()
        {
            LibraryManager.instance.FolderInfoButton();

            editingIcon = null;

            changedIcon = false;

            SoundByte.PlayOneShot("ui/ExitSmall");
        }

        #endregion

        #region Input Updaters

        public void OnNameChanged()
        {
            folderNameLabel.text = nameField.text;

            SetSaveValidity();
        }

        public void OnShowNameChanged()
        {
            folderNameLabel.gameObject.SetActive(nameToggle.isOn);

            SetSaveValidity();
        }

        public void OnColorChanged()
        {
            folderNameLabel.text = nameField.text;

            SetSaveValidity();
        }

        public void OnFieldChanged()
        {
            SetSaveValidity();
        }

        #endregion

        #region Other

        IEnumerator UploadImage(string path)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + path);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                customIconSprite = null;
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                customIconSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                customIcon.sprite = customIconSprite;
                customIcon.transform.parent.gameObject.SetActive(true);

                changedIcon = true;

                SetSaveValidity();

                Debug.Log("Uploaded new image successfully!");
            }
        }

        private void SaveCustomIcon(string path)
        {
            if (customIconSprite == null)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            else
            {
                byte[] spriteBytes = customIconSprite.texture.EncodeToPNG();
                File.WriteAllBytes(path, spriteBytes);
            }

            changedIcon = false;
            customIconSprite = null;
        }

        #endregion

    }
}
