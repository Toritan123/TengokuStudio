using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using SatorImaging.UnitySourceGenerator.Editor;
using HeavenStudio;

namespace UnityBuilderAction
{
    public static class BuildScript
    {
        private static readonly string Eol = Environment.NewLine;

        private static readonly string[] Secrets =
            {"androidKeystorePass", "androidKeyaliasName", "androidKeyaliasPass"};
        
        [MenuItem("File/Build Windows")]
        public static void StartWindows()
        {
            string appName = PlayerSettings.productName;
            // Get filename.
            string path = EditorUtility.SaveFilePanel("Build out WINDOWS to...", "", appName, "exe");
            if (string.IsNullOrEmpty(path)) return;
            Build( BuildTarget.StandaloneWindows64, 0, path);
        }

        [MenuItem("File/Build Linux")]
        public static void StartLinux()
        {
            string appName = PlayerSettings.productName;
            // Get filename.
            string path = EditorUtility.SaveFilePanel("Build out LINUX to...", "", appName, "");
            if (string.IsNullOrEmpty(path)) return;
            Build( BuildTarget.StandaloneLinux64, 0, path);
        }

        [MenuItem("File/Build Mac")]
        public static void StartMac()
        {
            string appName = PlayerSettings.productName;
            // Get filename.
            string path = EditorUtility.SaveFilePanel("Build out MAC to...", "", appName, "app");
            if (string.IsNullOrEmpty(path)) return;
            Build( BuildTarget.StandaloneOSX, 0, path);
        }

        public static void Build()
        {
            // Gather values from args
            Dictionary<string, string> options = GetValidatedOptions();

            // Set version for this build
            PlayerSettings.bundleVersion = options["buildVersion"];
            PlayerSettings.macOS.buildNumber = options["buildVersion"];
            PlayerSettings.Android.bundleVersionCode = int.Parse(options["androidVersionCode"]);

            // Apply build target
            var buildTarget = (BuildTarget) Enum.Parse(typeof(BuildTarget), options["buildTarget"]);
            switch (buildTarget)
            {
                case BuildTarget.Android:
                {
                    EditorUserBuildSettings.buildAppBundle = options["customBuildPath"].EndsWith(".aab");
                    if (options.TryGetValue("androidKeystoreName", out string keystoreName) &&
                        !string.IsNullOrEmpty(keystoreName))
                    {
                      PlayerSettings.Android.useCustomKeystore = true;
                      PlayerSettings.Android.keystoreName = keystoreName;
                    }
                    if (options.TryGetValue("androidKeystorePass", out string keystorePass) &&
                        !string.IsNullOrEmpty(keystorePass))
                        PlayerSettings.Android.keystorePass = keystorePass;
                    if (options.TryGetValue("androidKeyaliasName", out string keyaliasName) &&
                        !string.IsNullOrEmpty(keyaliasName))
                        PlayerSettings.Android.keyaliasName = keyaliasName;
                    if (options.TryGetValue("androidKeyaliasPass", out string keyaliasPass) &&
                        !string.IsNullOrEmpty(keyaliasPass))
                        PlayerSettings.Android.keyaliasPass = keyaliasPass;
                    if (options.TryGetValue("androidTargetSdkVersion", out string androidTargetSdkVersion) &&
                        !string.IsNullOrEmpty(androidTargetSdkVersion))
                    {
                        var targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
                        try
                        {
                            targetSdkVersion =
                                (AndroidSdkVersions) Enum.Parse(typeof(AndroidSdkVersions), androidTargetSdkVersion);
                        }
                        catch
                        {
                            UnityEngine.Debug.Log("Failed to parse androidTargetSdkVersion! Fallback to AndroidApiLevelAuto");
                        }

                        PlayerSettings.Android.targetSdkVersion = targetSdkVersion;
                    }

                    break;
                }
                case BuildTarget.StandaloneOSX:
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
                    break;
            }

            // Determine subtarget
            int buildSubtarget = 0;
#if UNITY_2021_2_OR_NEWER
            if (!options.TryGetValue("standaloneBuildSubtarget", out var subtargetValue) || !Enum.TryParse(subtargetValue, out StandaloneBuildSubtarget buildSubtargetValue)) {
                buildSubtargetValue = default;
            }
            buildSubtarget = (int) buildSubtargetValue;
#endif

            // Custom build
            Build(buildTarget, buildSubtarget, options["customBuildPath"]);
        }

        private static Dictionary<string, string> GetValidatedOptions()
        {
            ParseCommandLineArguments(out Dictionary<string, string> validatedOptions);

            if (!validatedOptions.TryGetValue("projectPath", out string _))
            {
                Console.WriteLine("Missing argument -projectPath");
                EditorApplication.Exit(110);
            }

            if (!validatedOptions.TryGetValue("buildTarget", out string buildTarget))
            {
                Console.WriteLine("Missing argument -buildTarget");
                EditorApplication.Exit(120);
            }

            if (!Enum.IsDefined(typeof(BuildTarget), buildTarget ?? string.Empty))
            {
                Console.WriteLine($"{buildTarget} is not a defined {nameof(BuildTarget)}");
                EditorApplication.Exit(121);
            }

            if (!validatedOptions.TryGetValue("customBuildPath", out string _))
            {
                Console.WriteLine("Missing argument -customBuildPath");
                EditorApplication.Exit(130);
            }

            const string defaultCustomBuildName = "TestBuild";
            if (!validatedOptions.TryGetValue("customBuildName", out string customBuildName))
            {
                Console.WriteLine($"Missing argument -customBuildName, defaulting to {defaultCustomBuildName}.");
                validatedOptions.Add("customBuildName", defaultCustomBuildName);
            }
            else if (customBuildName == "")
            {
                Console.WriteLine($"Invalid argument -customBuildName, defaulting to {defaultCustomBuildName}.");
                validatedOptions.Add("customBuildName", defaultCustomBuildName);
            }

            return validatedOptions;
        }

        private static void ParseCommandLineArguments(out Dictionary<string, string> providedArguments)
        {
            providedArguments = new Dictionary<string, string>();
            string[] args = Environment.GetCommandLineArgs();

            Console.WriteLine(
                $"{Eol}" +
                $"###########################{Eol}" +
                $"#    Parsing settings     #{Eol}" +
                $"###########################{Eol}" +
                $"{Eol}"
            );

            // Extract flags with optional values
            for (int current = 0, next = 1; current < args.Length; current++, next++)
            {
                // Parse flag
                bool isFlag = args[current].StartsWith("-");
                if (!isFlag) continue;
                string flag = args[current].TrimStart('-');

                // Parse optional value
                bool flagHasValue = next < args.Length && !args[next].StartsWith("-");
                string value = flagHasValue ? args[next].TrimStart('-') : "";
                bool secret = Secrets.Contains(flag);
                string displayValue = secret ? "*HIDDEN*" : "\"" + value + "\"";

                // Assign
                Console.WriteLine($"Found flag \"{flag}\" with value {displayValue}.");
                providedArguments.Add(flag, value);
            }
        }

        private static void Build(BuildTarget buildTarget, int buildSubtarget, string filePath)
        {
            string appName = PlayerSettings.productName;
            string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(s => s.path).ToArray();

            string dataPath = "";
            switch ( buildTarget ) {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    dataPath = $"_Data/";
                    break;
                case BuildTarget.StandaloneOSX:
                    dataPath = $".app/Contents/Resources/Data/";
                    break;
                case BuildTarget.StandaloneLinux64:
                    dataPath = $"_Data/";
                    break;
            }

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                target = buildTarget,
//                targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget),
                locationPathName = filePath,
                options = BuildOptions.CompressWithLz4HC,
#if UNITY_2021_2_OR_NEWER
                subtarget = buildSubtarget
#endif
            };

            string streamingAssets = "Assets/StreamingAssets";
            string assetBundleDirectory = GetAssetBundleDirectory(buildTarget);
            string tempDirectory = "Assets/StreamingAssets_temp";
            bool swappedStreamingAssets = false;

            var targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            var currentBT = EditorUserBuildSettings.activeBuildTarget;
            var currentTG = BuildPipeline.GetBuildTargetGroup(currentBT);

            try
            {
                if (currentBT != buildTarget)
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, buildTarget);
                }

                if (!Directory.Exists(assetBundleDirectory))
                {
                    Directory.CreateDirectory(assetBundleDirectory);
                }
                BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.ChunkBasedCompression, buildTarget);

                if (assetBundleDirectory != streamingAssets)
                {
                    if (Directory.Exists(streamingAssets))
                    {
                        Directory.Move(streamingAssets, tempDirectory);
                    }

                    Directory.Move(assetBundleDirectory, streamingAssets);

                    swappedStreamingAssets = true;

                    // the pack-in levels and other non-bundle files only live in Assets/StreamingAssets:
                    // bring them along for the swapped-in bundles, or a build made on another platform ships without them
                    foreach (string name in SharedStreamingAssets)
                    {
                        string src = Path.Combine(tempDirectory, name), dst = Path.Combine(streamingAssets, name);
                        if (Directory.Exists(src)) CopyDirectory(src, dst);
                        else if (File.Exists(src)) File.Copy(src, dst, true);
                    }
                }

                AssetDatabase.Refresh();
                USGUtility.ForceGenerateByType(typeof(Minigames));

                BuildSummary buildSummary = BuildPipeline.BuildPlayer(buildPlayerOptions).summary;

                ReportSummary(buildSummary);

                if (!Application.isEditor)
                    ExitWithResult(buildSummary.result);
            }
            finally
            {
                if (swappedStreamingAssets)
                {
                    foreach (string name in SharedStreamingAssets)
                    {
                        string copied = Path.Combine(streamingAssets, name);
                        if (Directory.Exists(copied)) Directory.Delete(copied, true);
                        else if (File.Exists(copied)) File.Delete(copied);
                        if (File.Exists(copied + ".meta")) File.Delete(copied + ".meta");
                    }

                    if (Directory.Exists(assetBundleDirectory))
                    {
                        Directory.Delete(assetBundleDirectory, true);
                    }

                    if (Directory.Exists(streamingAssets))
                    {
                        Directory.Move(streamingAssets, assetBundleDirectory);
                    }

                    if (Directory.Exists(tempDirectory))
                    {
                        Directory.Move(tempDirectory, streamingAssets);
                    }

                    if (File.Exists(tempDirectory + ".meta"))
                    {
                        File.Delete(tempDirectory + ".meta");
                    }

                    AssetDatabase.Refresh();
                }

                if (currentBT != buildTarget)
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(currentTG, currentBT);
                }
            }
        }

        // files in Assets/StreamingAssets that are not asset bundles
        static readonly string[] SharedStreamingAssets = { "Library Pack-In", "samuraislice.en" };

        static void CopyDirectory(string src, string dst)
        {
            Directory.CreateDirectory(dst);
            foreach (string f in Directory.GetFiles(src))
            {
                if (f.EndsWith(".meta")) continue;  // unity makes its own; copying them would duplicate guids
                File.Copy(f, Path.Combine(dst, Path.GetFileName(f)), true);
            }
            foreach (string d in Directory.GetDirectories(src)) CopyDirectory(d, Path.Combine(dst, Path.GetFileName(d)));
        }

        private static string GetAssetBundleDirectory(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    if (Application.platform == RuntimePlatform.WindowsEditor)
                    {
                        return "Assets/StreamingAssets";
                    }
                    return "Assets/StreamingAssets_Windows";

                case BuildTarget.StandaloneLinux64:
                    if (Application.platform == RuntimePlatform.LinuxEditor)
                    {
                        return "Assets/StreamingAssets";
                    }
                    return "Assets/StreamingAssets_Linux";

                case BuildTarget.StandaloneOSX:
                    if (Application.platform == RuntimePlatform.OSXEditor)
                    {
                        return "Assets/StreamingAssets";
                    }
                    return "Assets/StreamingAssets_Mac";

                default:
                    throw new ArgumentException($"Invalid build target {buildTarget}");
            }
        }

        private static void ReportSummary(BuildSummary summary)
        {
            Console.WriteLine(
                $"{Eol}" +
                $"###########################{Eol}" +
                $"#      Build results      #{Eol}" +
                $"###########################{Eol}" +
                $"{Eol}" +
                $"Duration: {summary.totalTime.ToString()}{Eol}" +
                $"Warnings: {summary.totalWarnings.ToString()}{Eol}" +
                $"Errors: {summary.totalErrors.ToString()}{Eol}" +
                $"Size: {summary.totalSize.ToString()} bytes{Eol}" +
                $"{Eol}"
            );
        }

        private static void ExitWithResult(BuildResult result)
        {
            switch (result)
            {
                case BuildResult.Succeeded:
                    Console.WriteLine("Build succeeded!");
                    EditorApplication.Exit(0);
                    break;
                case BuildResult.Failed:
                    Console.WriteLine("Build failed!");
                    EditorApplication.Exit(101);
                    break;
                case BuildResult.Cancelled:
                    Console.WriteLine("Build cancelled!");
                    EditorApplication.Exit(102);
                    break;
                case BuildResult.Unknown:
                default:
                    Console.WriteLine("Build result is unknown!");
                    EditorApplication.Exit(103);
                    break;
            }
        }
    }
}