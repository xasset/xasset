using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XAsset.Editor
{
    public static class BuildScript
    {
        [InitializeOnLoadMethod]
        public static void Clear()
        {
            EditorUtility.ClearProgressBar();
        }

        static public string CreateAssetBundleDirectory()
        {
            // Choose the output path according to the build target.
            string outputPath = Path.Combine(Utility.AssetBundlesOutputPath, Utility.GetPlatformName());
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            return outputPath;
        }

        public static void BuildAssetBundles(List<AssetBundleBuild> builds)
        {
            // Choose the output path according to the build target.
            string outputPath = CreateAssetBundleDirectory();

            var options = BuildAssetBundleOptions.None;

            bool shouldCheckODR = EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS;
#if UNITY_TVOS
			shouldCheckODR |= EditorUserBuildSettings.activeBuildTarget == BuildTarget.tvOS;
#endif
            if (shouldCheckODR)
            {
#if ENABLE_IOS_ON_DEMAND_RESOURCES
				if (PlayerSettings.iOS.useOnDemandResources)
				options |= BuildAssetBundleOptions.UncompressedAssetBundle;
#endif
#if ENABLE_IOS_APP_SLICING
				options |= BuildAssetBundleOptions.UncompressedAssetBundle;
#endif
            }

            if (builds == null || builds.Count == 0)
            {
                //@TODO: use append hash... (Make sure pipeline works correctly with it.)
                BuildPipeline.BuildAssetBundles(outputPath, options, EditorUserBuildSettings.activeBuildTarget);
            }
            else
            {
                BuildPipeline.BuildAssetBundles(outputPath, builds.ToArray(), options, EditorUserBuildSettings.activeBuildTarget);
            }
        }

        public static void BuildPlayerWithoutAssetBundles()
        {
            var outputPath = EditorUtility.SaveFolderPanel("Choose Location of the Built Game", "", "");
            if (outputPath.Length == 0)
                return;

            string[] levels = GetLevelsFromBuildSettings();
            if (levels.Length == 0)
            {
                Debug.Log("Nothing to build.");
                return;
            }

            string targetName = GetBuildTargetName(EditorUserBuildSettings.activeBuildTarget);
            if (targetName == null)
                return;

#if UNITY_5_4 || UNITY_5_3 || UNITY_5_2 || UNITY_5_1 || UNITY_5_0
			BuildOptions option = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
			BuildPipeline.BuildPlayer(levels, outputPath + targetName, EditorUserBuildSettings.activeBuildTarget, option);
#else
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = levels;
            buildPlayerOptions.locationPathName = outputPath + targetName;
            buildPlayerOptions.assetBundleManifestPath = GetAssetBundleManifestFilePath();
            buildPlayerOptions.target = EditorUserBuildSettings.activeBuildTarget;
            buildPlayerOptions.options = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
            BuildPipeline.BuildPlayer(buildPlayerOptions);
#endif
        }

        public static void BuildStandalonePlayer()
        {
            var outputPath = EditorUtility.SaveFolderPanel("Choose Location of the Built Game", "", "");
            if (outputPath.Length == 0)
                return;

            string[] levels = GetLevelsFromBuildSettings();
            if (levels.Length == 0)
            {
                Debug.Log("Nothing to build.");
                return;
            }

            string targetName = GetBuildTargetName(EditorUserBuildSettings.activeBuildTarget);
            if (targetName == null)
                return;

            CopyAssetBundlesTo(Path.Combine(Application.streamingAssetsPath, Utility.AssetBundlesOutputPath));
            AssetDatabase.Refresh();

#if UNITY_5_4 || UNITY_5_3 || UNITY_5_2 || UNITY_5_1 || UNITY_5_0
			BuildOptions option = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
			BuildPipeline.BuildPlayer(levels, outputPath + targetName, EditorUserBuildSettings.activeBuildTarget, option);
#else
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = levels;
            buildPlayerOptions.locationPathName = outputPath + targetName;
            buildPlayerOptions.assetBundleManifestPath = GetAssetBundleManifestFilePath();
            buildPlayerOptions.target = EditorUserBuildSettings.activeBuildTarget;
            buildPlayerOptions.options = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
            BuildPipeline.BuildPlayer(buildPlayerOptions);
#endif
        }

        public static string GetBuildTargetName(BuildTarget target)
        {
            string name = PlayerSettings.productName + "_" + PlayerSettings.bundleVersion;
            if (target == BuildTarget.Android)
            {
                return "/" + name + PlayerSettings.Android.bundleVersionCode + ".apk";
            }
            if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
            {
                return "/" + name + PlayerSettings.Android.bundleVersionCode + ".exe";
            }
            if (target == BuildTarget.StandaloneOSXIntel || target == BuildTarget.StandaloneOSXIntel64 || target == BuildTarget.StandaloneOSXUniversal)
            {
                return "/" + name + ".app";
            }
            if (target == BuildTarget.WebGL)
            {
                return "/web";
            }
            if (target == BuildTarget.iOS)
            {
                return "/iOS";
            }
            Debug.Log("Target not implemented.");
            return null;
        }

        static public void CopyAssetBundlesTo(string outputPath)
        {
            // Clear streaming assets folder.
            //            FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath);
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            string outputFolder = Utility.GetPlatformName();

            // Setup the source folder for assetbundles.
            var source = Path.Combine(Path.Combine(System.Environment.CurrentDirectory, Utility.AssetBundlesOutputPath), outputFolder);
            if (!System.IO.Directory.Exists(source))
                Debug.Log("No assetBundle output folder, try to build the assetBundles first.");

            // Setup the destination folder for assetbundles.
            var destination = System.IO.Path.Combine(outputPath, outputFolder);
            if (System.IO.Directory.Exists(destination))
                FileUtil.DeleteFileOrDirectory(destination);

            FileUtil.CopyFileOrDirectory(source, destination);
        }

        static string[] GetLevelsFromBuildSettings()
        {
            List<string> levels = new List<string>();
            for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
            {
                if (EditorBuildSettings.scenes[i].enabled)
                    levels.Add(EditorBuildSettings.scenes[i].path);
            }

            return levels.ToArray();
        }

        static string GetAssetBundleManifestFilePath()
        {
            var relativeAssetBundlesOutputPathForPlatform = Path.Combine(Utility.AssetBundlesOutputPath, Utility.GetPlatformName());
            return Path.Combine(relativeAssetBundlesOutputPathForPlatform, Utility.GetPlatformName()) + ".manifest";
        }

    }
}