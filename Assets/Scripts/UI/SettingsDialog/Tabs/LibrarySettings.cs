using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using TMPro;

using HeavenStudio.Common;
using HeavenStudio.Util;
using SFB;
using System.IO;
namespace HeavenStudio.Editor
{
    public class LibrarySettings : TabsContent
    {
        public TMP_InputField libraryPathField;
        public GameObject confirmMove;
        public GameObject note;
        public TMP_Text noteText;
        public TMP_Text cacheConfirmText;
        public TMP_Text saveConfirmText;
        private bool confirmErase = false;
        private bool settingsModified = false;
        private string oldPath;

        private void Start()
        {
        }

        public void SetLibraryPath()
        {
            FileBrowserOpener.OpenFolderPanelAsync("Select library directory", "", false, (string[] paths) =>
            {
                var path = Path.Combine(paths);
                if (path == string.Empty || path == PersistentDataManager.gameSettings.libLevelsDirectory) return;

                oldPath = PersistentDataManager.gameSettings.libLevelsDirectory;
                PersistentDataManager.gameSettings.libLevelsDirectory = path;
                libraryPathField.text = path;
                confirmMove.SetActive(true);
                settingsModified = true;
            });
        }

        public void YesMoveLevels()
        {   
            confirmMove.SetActive(false);
            StartCoroutine(MoveLevels());
        }
        
        private IEnumerator MoveLevels()
        {
            note.SetActive(true);
            noteText.text = Localization.T("Moving levels...\nThis may take a while.\n(Creating path...)");
            yield return null;
            Directory.CreateDirectory(PersistentDataManager.gameSettings.libLevelsDirectory);
            noteText.text = Localization.T("Moving levels...\nThis may take a while.\n(Copying files...)");
            yield return null;
            CopyDirectory(oldPath, PersistentDataManager.gameSettings.libLevelsDirectory, true);
            noteText.text = Localization.T("Moving levels...\nThis may take a while.\n(Deleting old files...)");
            yield return null;
            Directory.Delete(oldPath, true);
            note.SetActive(false);
        }

        public void NoMoveLevels()
        {
            confirmMove.SetActive(false);
        }
        
        private void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }


        public void ViewLibraryPath()
        {
            if (libraryPathField.contentType == TMP_InputField.ContentType.Password)
            {
                libraryPathField.contentType = TMP_InputField.ContentType.Standard;
            }
            else
            {
                libraryPathField.contentType = TMP_InputField.ContentType.Password;
            }
            libraryPathField.ForceLabelUpdate();
        }

        public void ReImportPackIns()
        {
            settingsModified = true;
            StartCoroutine(CopyPackIns());
        }

        public void ClearCache()
        {
            settingsModified = true;
            string cachePath = Path.Combine(Application.persistentDataPath, "LibraryCache");
            if (Directory.Exists(cachePath))
            {
                Directory.Delete(cachePath, true);
            }
            LibraryCacheManager.LoadCache();
            cacheConfirmText.text = Localization.T("Erased!");
        }

        public void DeleteSaveData()
        {
            if (!confirmErase)
            {
                saveConfirmText.text = Localization.T("Are you sure? You can't undo this!");
                confirmErase = true;
                return;
            }
            settingsModified = true;
            LibraryManager.instance.DeleteAllSaveData();
            saveConfirmText.text = Localization.T("Erased!");
        }

        public override void OnOpenTab()
        {
            libraryPathField.text = PersistentDataManager.gameSettings.libLevelsDirectory;
            settingsModified = false;
            cacheConfirmText.text = "";
            saveConfirmText.text = "";
            confirmErase = false;
        }

        public override void OnCloseTab()
        {
            if (settingsModified)
            {
                settingsModified = false;
                if (SettingsDialog.instance.active)
                {
                    SettingsDialog.instance.SwitchSettingsDialog();
                }

                _ = LibraryManager.instance.Refresh();
            }
        }

        private IEnumerator CopyPackIns()
        {
            note.SetActive(true);
            noteText.text = Localization.T("Copying levels...\nThis may take a while.");
            yield return null;
            LibraryManager.instance.SetupLibrary();
            note.SetActive(false);
        }
    }
}