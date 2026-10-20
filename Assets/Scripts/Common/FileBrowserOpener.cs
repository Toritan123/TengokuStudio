using System;
using System.Collections;
using System.IO;
using UnityEngine;
using SFB;
using SimpleFileBrowser;
using HeavenStudio.Common;

public static class FileBrowserOpener
{
    private static MonoBehaviour coroutineHost;

    // coroutines have to have this idk i copypaste my code
    public static void Initialize(MonoBehaviour host)
    {
        coroutineHost = host;
    }

    public static void OpenFilePanelAsync(
        string title,
        string directory,
        ExtensionFilter[] extensions,
        bool multiselect,
        Action<string[]> cb)
    {
        if (!PersistentDataManager.gameSettings.forceFallbackFileBrowser)
        {
            try
            {
                StandaloneFileBrowser.OpenFilePanelAsync(title, directory, extensions, multiselect, (string[] paths) =>
                {
                    cb?.Invoke(paths);
                });
                return;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"StandaloneFileBrowser failed, using SimpleFileBrowser\n{e.Message}");
            }
        }
        if (coroutineHost == null)
            {
                Debug.LogError("Somehow, the file picker wasn't initialized.");
                cb?.Invoke(Array.Empty<string>());
                return;
            }

        coroutineHost.StartCoroutine(OpenWithSimpleFileBrowser(title, extensions, multiselect, cb));
    }

    private static IEnumerator OpenWithSimpleFileBrowser(string title, ExtensionFilter[] extensions, bool multiselect, Action<string[]> cb)
    {
        if (extensions != null && extensions.Length > 0)
        {
            FileBrowser.SetFilters(true, ConvertFilters(extensions));
            FileBrowser.SetDefaultFilter(extensions[0].Extensions[0].StartsWith(".") 
                ? extensions[0].Extensions[0] 
                : "." + extensions[0].Extensions[0]);
        }

        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, multiselect, null, null, title, "Select");

        if (FileBrowser.Success)
            cb?.Invoke(FileBrowser.Result);
        else
            cb?.Invoke(Array.Empty<string>());
    }

    public static void SaveFilePanelAsync(
        string title,
        string directory,
        string defaultName,
        ExtensionFilter[] extensions,
        Action<string> cb)
    {
        if (!PersistentDataManager.gameSettings.forceFallbackFileBrowser)
        {
            try
            {
                StandaloneFileBrowser.SaveFilePanelAsync(title, directory, defaultName, extensions, (string path) =>
                {
                    cb?.Invoke(path);
                });
                return;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"StandaloneFileBrowser failed, using SimpleFileBrowser\n{e.Message}");
            }
        }

        if (coroutineHost == null)
        {
            Debug.LogError("Somehow, the file picker wasn't initialized.");
            cb?.Invoke(string.Empty);
            return;
        }

        coroutineHost.StartCoroutine(SaveWithSimpleFileBrowser(title, defaultName, extensions, cb));
    }

    private static IEnumerator SaveWithSimpleFileBrowser(
        string title,
        string defaultName,
        ExtensionFilter[] extensions,
        Action<string> cb)
    {
        if (extensions != null && extensions.Length > 0)
        {
            FileBrowser.SetFilters(true, ConvertFilters(extensions));
            FileBrowser.SetDefaultFilter(
                extensions[0].Extensions[0].StartsWith(".")
                    ? extensions[0].Extensions[0]
                    : "." + extensions[0].Extensions[0]);
        }

        yield return FileBrowser.WaitForSaveDialog(
            FileBrowser.PickMode.Files,
            allowMultiSelection: false,
            initialPath: null,
            initialFilename: defaultName,
            title: title,
            saveButtonText: "Save"
        );

        if (FileBrowser.Success)
            cb?.Invoke(FileBrowser.Result[0]); 
        else
            cb?.Invoke(string.Empty);
    }

    public static void OpenFolderPanelAsync(
        string title,
        string directory,
        bool multiselect,
        Action<string[]> cb)
    {
        if (!PersistentDataManager.gameSettings.forceFallbackFileBrowser) 
        {
            try
            {
                StandaloneFileBrowser.OpenFolderPanelAsync(title, directory, multiselect, (string[] paths) =>
                {
                    cb?.Invoke(paths);
                });
                return;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"StandaloneFileBrowser failed, using SimpleFileBrowser\n{e.Message}");
            }
        }

        if (coroutineHost == null)
        {
            Debug.LogError("Somehow, the file picker wasn't initialized.");
            cb?.Invoke(Array.Empty<string>());
            return;
        }

        coroutineHost.StartCoroutine(OpenFolderWithSimpleFileBrowser(title, directory, multiselect, cb));
    }

    private static IEnumerator OpenFolderWithSimpleFileBrowser(string title, string initialPath, bool multiselect, Action<string[]> cb)
    {
        yield return FileBrowser.WaitForLoadDialog(
            pickMode: FileBrowser.PickMode.Folders,
            allowMultiSelection: multiselect,
            initialPath: initialPath,
            initialFilename: null,
            title: title,
            loadButtonText: "Select"
        );

        if (FileBrowser.Success)
            cb?.Invoke(FileBrowser.Result);
        else
            cb?.Invoke(Array.Empty<string>());
    }


    private static FileBrowser.Filter[] ConvertFilters(ExtensionFilter[] extFilters)
    {
        FileBrowser.Filter[] filters = new FileBrowser.Filter[extFilters.Length];
        for (int i = 0; i < extFilters.Length; i++)
        {
            for (int j = 0; j < extFilters[i].Extensions.Length; j++)
            {
                if (!extFilters[i].Extensions[j].StartsWith("."))
                    extFilters[i].Extensions[j] = "." + extFilters[i].Extensions[j];
            }
            filters[i] = new FileBrowser.Filter(extFilters[i].Name, extFilters[i].Extensions);
        }
        return filters;
    }
}
