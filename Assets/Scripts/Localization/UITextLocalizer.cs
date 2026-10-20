using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

using TMPro;

namespace HeavenStudio.Common
{
    /// <summary>
    /// Translates the static text baked into scenes and prefabs (labels, buttons, dropdown options).
    /// Text set from code should go through Localization.T instead.
    /// </summary>
    public static class UITextLocalizer
    {
        // original text, and the text we last wrote, so the language can be changed at runtime
        private static readonly Dictionary<int, (string source, string applied)> texts = new();
        private static readonly Dictionary<int, List<string>> dropdownOptions = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Hook()
        {
            SceneManager.sceneLoaded += (scene, _) => LocalizeScene(scene);
            SceneManager.sceneUnloaded += _ => Prune();
        }

        public static void LocalizeAllLoadedScenes()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                LocalizeScene(SceneManager.GetSceneAt(i));
            }
        }

        public static void LocalizeScene(Scene scene)
        {
            if (!scene.isLoaded) return;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                LocalizeHierarchy(root);
            }
        }

        public static void LocalizeHierarchy(GameObject root)
        {
            if (root == null) return;

            // text components that display what the user types shouldn't be touched
            HashSet<TMP_Text> inputTexts = new();
            foreach (var input in root.GetComponentsInChildren<TMP_InputField>(true))
            {
                if (input.textComponent != null) inputTexts.Add(input.textComponent);
            }

            foreach (var dropdown in root.GetComponentsInChildren<TMP_Dropdown>(true))
            {
                LocalizeDropdown(dropdown);
                if (dropdown.captionText != null) inputTexts.Add(dropdown.captionText);
            }

            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (inputTexts.Contains(text)) continue;
                LocalizeText(text);
            }
        }

        public static void LocalizeText(TMP_Text text)
        {
            if (text == null) return;
            int id = text.GetInstanceID();
            string source = text.text;
            if (texts.TryGetValue(id, out var entry) && entry.applied == source)
            {
                source = entry.source;
            }

            string translated = Localization.T(source);
            texts[id] = (source, translated);
            if (translated != text.text)
            {
                if (translated != source) FitSingleLine(text, source, translated);
                text.text = translated;
            }
        }

        // labels were laid out for the english text; if that fit on one line, keep the translation on one line
        // by shrinking it instead of letting it wrap or spill over the controls next to it
        private static void FitSingleLine(TMP_Text text, string source, string translated)
        {
            if (text.enableAutoSizing || source.Contains('\n') || translated.Contains('\n')) return;
            if (text.TryGetComponent(out UnityEngine.UI.ContentSizeFitter fitter) && fitter.horizontalFit != UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained) return;

            float width = text.rectTransform.rect.width;
            if (width <= 0) return;
            if (text.GetPreferredValues(source, float.PositiveInfinity, float.PositiveInfinity).x > width + 1) return;
            if (text.GetPreferredValues(translated, float.PositiveInfinity, float.PositiveInfinity).x <= width) return;

            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.fontSizeMax = text.fontSize;
            text.fontSizeMin = text.fontSize * 0.35f;
            text.enableAutoSizing = true;
        }

        public static void LocalizeDropdown(TMP_Dropdown dropdown)
        {
            if (dropdown == null || dropdown.options.Count == 0) return;
            int id = dropdown.GetInstanceID();
            if (!dropdownOptions.TryGetValue(id, out var sources) || sources.Count != dropdown.options.Count)
            {
                sources = dropdown.options.Select(o => o.text).ToList();
                dropdownOptions[id] = sources;
            }

            for (int i = 0; i < dropdown.options.Count; i++)
            {
                dropdown.options[i].text = Localization.T(sources[i]);
            }
            dropdown.RefreshShownValue();
        }

        private static void Prune()
        {
            // instance ids of destroyed objects are never reused, so this only keeps the tables small
            if (texts.Count > 20000) texts.Clear();
            if (dropdownOptions.Count > 2000) dropdownOptions.Clear();
        }
    }
}
