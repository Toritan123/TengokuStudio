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
using HeavenStudio;
using HeavenStudio.Util;

namespace HeavenStudio.Editor
{
    public class TagPropertyEditorPrefab : RemixPropertyPrefab
    {
        [SerializeField] GameObject levelTagsHolder;
        [SerializeField] GameObject suggestedTagsHolder;
        [SerializeField] LibraryTag tagPrefab;
        [SerializeField] TMP_InputField tagInput;

        RemixPropertiesDialog diag;

        new public void InitProperties(RemixPropertiesDialog diag, string propertyName, string caption)
        {
            this.diag = diag;

            UpdateInfo();
        }

        void UpdateInfo()
        {
            foreach (Transform child in suggestedTagsHolder.transform)
            {
                Destroy(child.gameObject);
            }
            
            SortedSet<string> currentTags = LibraryTag.ParseTagString(diag.chart["remixtags"]);

            foreach (string tag in currentTags)
            {
                InitTagObject(tag, LibraryTag.TagButtonType.Remove, levelTagsHolder.transform);
            }

            SortChildren(levelTagsHolder.transform);
        }

        private void InitTagObject(string tag, LibraryTag.TagButtonType type, Transform parent)
        {
            LibraryTag newTag = Instantiate(tagPrefab, parent);
            newTag.SetTagText(tag);
            newTag.SetTagButton(type);

            switch (type)
            {
                case LibraryTag.TagButtonType.Remove:
                    newTag.onTagButtonPressed = RemoveTag;
                    break;
                case LibraryTag.TagButtonType.Add:
                    newTag.onTagButtonPressed = AddTag;
                    break;
            }
        }

        private void SortChildren(Transform transform)
        {
            List<Transform> children = new();

            foreach (Transform child in transform)
            {
                children.Add(child);
            }

            children = children.OrderBy(t => t.gameObject.name).ToList();

            for (int i = 0; i < children.Count; i++)
            {
                children[i].SetSiblingIndex(i);
            }
        }

        public void AddTagFromTextbox()
        {
            AddTag(tagInput.text);
        }

        public void OnTextboxUpdated(string content)
        {
            string textboxTag = LibraryTag.RewriteTagIfInvalid(content);
            List<string> matched = LibraryCacheManager.tagbank.Where(tag => tag.StartsWith(textboxTag)).ToList();

            foreach (Transform child in suggestedTagsHolder.transform)
            {
                Destroy(child.gameObject);
            }

            if (!string.IsNullOrEmpty(textboxTag))
            {
                foreach (string matchedTag in matched)
                {
                    InitTagObject(matchedTag, LibraryTag.TagButtonType.Add, suggestedTagsHolder.transform);
                }
            }

            SortChildren(suggestedTagsHolder.transform);
        }

        public void AddTag(string tag)
        {
            SortedSet<string> currentTags = LibraryTag.ParseTagString(diag.chart["remixtags"]);

            string newTagText = LibraryTag.RewriteTagIfInvalid(tag);

            if (!string.IsNullOrEmpty(newTagText))
            {
                if (currentTags.Add(newTagText))
                {
                    SetTags(currentTags);

                    InitTagObject(newTagText, LibraryTag.TagButtonType.Remove, levelTagsHolder.transform);
                    SortChildren(levelTagsHolder.transform);
                    tagInput.text = "";
                }
            }
        }

        public void AddTag(LibraryTag tag)
        {
            AddTag(tag.GetTagText());
        }

        public void RemoveTag(LibraryTag tag)
        {
            SortedSet<string> currentTags = LibraryTag.ParseTagString(diag.chart["remixtags"]);

            currentTags.Remove(tag.GetTagText());
            SetTags(currentTags);

            Destroy(tag.gameObject);
        }

        public void SetTags(SortedSet<string> tagSet)
        {
            string tagString = string.Join(',', tagSet);
            diag.chart["remixtags"] = tagString;
        }

    }
}