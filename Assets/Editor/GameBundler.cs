using UnityEngine;
using UnityEditor;
using System.IO;

public class GameBundler : EditorWindow
{
    string gameName = "exampleGameName";
    string bundleName = "pcoexamplebundle";

    [MenuItem("Tools/Game Bundler")]
    public static void ShowWindow()
    {
        GetWindow<GameBundler>("Game Bundler");
    }

    void OnGUI()
    {
        GUILayout.Label("Game Bundler", EditorStyles.boldLabel);

        gameName = EditorGUILayout.TextField("Game Name", gameName);
        bundleName = EditorGUILayout.TextField("AssetBundle Name", bundleName);

        if (GUILayout.Button("Bundle up!"))
        {
            MoveAndAssign();
        }

        if (GUILayout.Button("Unbundle/Revert!"))
        {
            RevertAndUnassign();
        }
    }

    // thanks stackoverflow ill never forget you
    static string FirstUpper(string str)
    {
        if (!string.IsNullOrEmpty(str) && char.IsLower(str[0]))
            return str.Length == 1 ? char.ToUpper(str[0]).ToString() : char.ToUpper(str[0]) + str[1..];

        return str;
    }

    static string FirstLower(string str)
    {
        if (!string.IsNullOrEmpty(str) && char.IsUpper(str[0]))
            return str.Length == 1 ? char.ToLower(str[0]).ToString() : char.ToLower(str[0]) + str[1..];

        return str;
    }

    void MoveAndAssign()
    {
        string lowerName = FirstLower(gameName);
        string upperName = FirstUpper(gameName);
        string bundledRoot = Path.Combine("Assets/Bundled/Games", upperName);

        (string src, string dst)[] paths =
        {
            ($"Assets/Resources/Sprites/Games/{upperName}", Path.Combine(bundledRoot, "Sprites")), // WHY ARE THE NAMING CONVENTIONS LIKE THIS????
            ($"Assets/Resources/Games/{lowerName}.prefab", Path.Combine(bundledRoot, $"{lowerName}.prefab")),
            ($"Assets/Resources/Sfx/Games/{lowerName}", Path.Combine(bundledRoot, "Sounds")),
            ($"Assets/Resources/Prefabs/Games/{upperName}", Path.Combine(bundledRoot, "Prefabs")),
            ($"Assets/Resources/Models/Games/{upperName}", Path.Combine(bundledRoot, "Models")),
        };

        MoveAssets(paths);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        AssignBundle(bundledRoot, $"{bundleName}/common");

        Debug.Log($"Finished moving assets and assigning bundle '{bundleName}'");
        Debug.Log("You aren't done yet! Remember to mark in game script that it uses AssetBundles.");
    }

    void RevertAndUnassign()
    {
        string lowerName = FirstLower(gameName);
        string upperName = FirstUpper(gameName);
        string bundledRoot = Path.Combine("Assets/Bundled/Games", upperName);

        (string src, string dst)[] paths =
        {
            (Path.Combine(bundledRoot, "Sprites"), $"Assets/Resources/Sprites/Games/{upperName}"), // AGAIN WHY ARE THE NAMING CONVENTIONS LIKE THIS????
            (Path.Combine(bundledRoot, $"{lowerName}.prefab"), $"Assets/Resources/Games/{lowerName}.prefab"),
            (Path.Combine(bundledRoot, "Sounds"), $"Assets/Resources/Sfx/Games/{lowerName}"),
            (Path.Combine(bundledRoot, "Prefabs"), $"Assets/Resources/Prefabs/Games/{upperName}"),
            (Path.Combine(bundledRoot, "Models"), $"Assets/Resources/Models/Games/{upperName}"),
        };

        MoveAssets(paths);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        foreach (var (src, dst) in paths)
        {
            AssignBundle(dst, string.Empty);
        }

        Debug.Log($"Reverted {upperName}, game no longer uses bundles.");
    }

    void MoveAssets((string src, string dst)[] paths)
    {
        foreach (var (src, dst) in paths)
        {
            if (AssetDatabase.IsValidFolder(src) || File.Exists(src))
            {
                string dstDir = Path.GetDirectoryName(dst);
                if (!AssetDatabase.IsValidFolder(dstDir))
                {
                    CreateFolderRecursive(dstDir);
                }

                string error = AssetDatabase.MoveAsset(src, dst);
                if (!string.IsNullOrEmpty(error))
                    Debug.LogError($"Failed to move {src} → {dst}: {error}");
                else
                    Debug.Log($"Moved {src} → {dst}");
            }
            else
            {
                Debug.Log($"Nothing to move, skipped {src}");
            }
        }
    }

    void AssignBundle(string rootPath, string bundleName)
    {
        foreach (string guid in AssetDatabase.FindAssets("", new[] { rootPath }))
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer != null)
            {
                importer.assetBundleName = bundleName;
                Debug.Log($"Assigned {assetPath} → {bundleName}");
            }
        }
    }

    void CreateFolderRecursive(string target)
    {
        string parent = "Assets";
        foreach (string part in target.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
        {
            if (string.IsNullOrEmpty(part) || part == "Assets") continue;

            string current = Path.Combine(parent, part).Replace("\\", "/");
            if (!AssetDatabase.IsValidFolder(current))
            {
                AssetDatabase.CreateFolder(parent, part);
            }
            parent = current;
        }
    }
}
