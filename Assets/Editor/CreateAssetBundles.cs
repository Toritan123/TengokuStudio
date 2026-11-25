using System.IO;
using UnityEditor;
using UnityEngine;

using SatorImaging.UnitySourceGenerator.Editor;
using HeavenStudio;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetBundles/Current Platform")]
    static void BuildAllAssetBundlesCurrPlatform()
    {
        string assetBundleDirectory = "Assets/StreamingAssets";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        AssetDatabase.Refresh();
        USGUtility.ForceGenerateByType(typeof(Minigames));
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
    }

    [MenuItem("Assets/Build AssetBundles/Windows")]
    static void BuildAllAssetBundlesWindows()
    {
        string assetBundleDirectory = "Assets/StreamingAssets";
        if (Application.platform != RuntimePlatform.WindowsEditor)
        {
            assetBundleDirectory = "Assets/StreamingAssets_Windows";
        }
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        AssetDatabase.Refresh();
        USGUtility.ForceGenerateByType(typeof(Minigames));
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);
    }

    [MenuItem("Assets/Build AssetBundles/Linux")]
    static void BuildAllAssetBundlesLinux()
    {
        string assetBundleDirectory = "Assets/StreamingAssets";
        if (Application.platform != RuntimePlatform.LinuxEditor)
        {
            assetBundleDirectory = "Assets/StreamingAssets_Linux";
        }
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        AssetDatabase.Refresh();
        USGUtility.ForceGenerateByType(typeof(Minigames));
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneLinux64);
    }

    [MenuItem("Assets/Build AssetBundles/Mac")]
    static void BuildAllAssetBundlesMacOS()
    {
        string assetBundleDirectory = "Assets/StreamingAssets";
        if (Application.platform != RuntimePlatform.OSXEditor)
        {
            assetBundleDirectory = "Assets/StreamingAssets_Mac";
        }
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        AssetDatabase.Refresh();
        USGUtility.ForceGenerateByType(typeof(Minigames));
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneOSX);
    }
}