using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

namespace HeavenStudio
{
    public class LibraryCustomSortEntry : MonoBehaviour
    {
        public TMP_Text filePathText;
        public TMP_Text levelNameText;
        public string filePath;

        public void SetInfo(string name, string path)
        {
            gameObject.name = name;
            filePath = path;
            levelNameText.text = name;
            filePathText.text = Path.GetFileName(filePath);
        }

        public void Up()
        {
            if (transform.GetSiblingIndex() > 1)
            transform.SetSiblingIndex(transform.GetSiblingIndex() - 1);
        }

        public void Down()
        {
            transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);
        }

        public void Back()
        {
            LibraryManager.instance.infoPanel.ShowFolderEditLayout();
        }
    }
}
