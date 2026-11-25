using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HeavenStudio
{
    public class LibraryTag : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Text label;
        [SerializeField] public Button tagButton;
        [SerializeField] private Image tagButtonImage;

        [Header("Settings")]
        [SerializeField] private Sprite addTagSprite;
        [SerializeField] private Sprite removeTagSprite;

        public delegate void OnTagButtonPressed(LibraryTag tag);

        public OnTagButtonPressed onTagButtonPressed = delegate { };

        public void SetTagButton(TagButtonType buttonType)
        {
            switch (buttonType)
            {
                case TagButtonType.None:
                    tagButtonImage.gameObject.SetActive(false);
                    break;
                case TagButtonType.Add:
                    tagButtonImage.sprite = addTagSprite;
                    tagButtonImage.gameObject.SetActive(true);
                    break;
                case TagButtonType.Remove:
                    tagButtonImage.sprite = removeTagSprite;
                    tagButtonImage.gameObject.SetActive(true);
                    break;
            }
        }

        public void SetTagText(string text)
        {
            label.text = text;
            gameObject.name = text;
        }

        public string GetTagText()
        {
            return label.text;
        }

        public static SortedSet<string> ParseTagString(string tagString)
        {
            string[] splitTags = tagString.Split(',');
            SortedSet<string> tagList = new();

            foreach (string tag in splitTags)
            {
                string newTag = RewriteTagIfInvalid(tag);

                if (!string.IsNullOrEmpty(newTag))
                {
                    tagList.Add(newTag);
                }
            }

            return tagList;
        }

        public static string RewriteTagIfInvalid(string tag)
        {
            if (Regex.IsMatch(tag, @"^[a-z](?:[a-z -]*[a-z])?$"))
            {
                return tag;
            }
            else
            {
                tag = tag.ToLower();
                tag = Regex.Replace(tag, @"[^a-zA-Z0-9 -]", ""); // https://stackoverflow.com/a/3210410
                tag = tag.Trim(' ', '-');
                return tag;
            }
        }

        public void OnButtonPressed()
        {
            onTagButtonPressed.Invoke(this);
        }

        public enum TagButtonType
        {
            None,
            Add,
            Remove
        }
    }
}
