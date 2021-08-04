using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VEngine.Editor.Builds
{
    public static class BuildScript
    {
        public static Action<BuildTask> postprocessBuildBundles;
        public static Action<BuildTask> preprocessBuildBundles;

        public static void BuildBundles(BuildTask task)
        {
            if (preprocessBuildBundles != null) preprocessBuildBundles(task);

            task.BuildBundles();
            if (postprocessBuildBundles != null) postprocessBuildBundles(task);
        }

        public static void BuildBundles()
        {
            BuildBundles(new BuildTask());
        }

        private static string GetTimeForNow()
        {
            return DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }

        public static string GetBuildTargetName(BuildTarget target)
        {
            var productName = "xc" + "-v" + UnityEditor.PlayerSettings.bundleVersion + ".";
            var targetName = $"/{productName}-{GetTimeForNow()}";
            switch (target)
            {
                case BuildTarget.Android:
                    return targetName + ".apk";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return targetName + ".exe";
                case BuildTarget.StandaloneOSX:
                    return targetName + ".app";
                default:
                    return targetName;
            }
        }

        public static void BuildPlayer()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "Build");
            if (path.Length == 0) return;

            BuildPlayer(path);
        }

        public static void BuildPlayer(string path)
        {
            var levels = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
                if (scene.enabled)
                    levels.Add(scene.path);

            if (levels.Count == 0)
            {
                Debug.Log("Nothing to build.");
                return;
            }

            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var buildTargetName = GetBuildTargetName(buildTarget);
            if (buildTargetName == null) return;

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = levels.ToArray(),
                locationPathName = path + buildTargetName,
                target = buildTarget,
                options = EditorUserBuildSettings.development
                    ? BuildOptions.Development
                    : BuildOptions.None
            };
            BuildPipeline.BuildPlayer(buildPlayerOptions);
        }

        public static void CopyToStreamingAssets()
        {
            var settings = Settings.GetDefaultSettings();
            var destinationDir = Settings.BuildPlayerDataPath;
            if (Directory.Exists(destinationDir)) Directory.Delete(destinationDir, true);

            Directory.CreateDirectory(destinationDir);
            var bundles = Settings.GetBundlesInBuild(true);
            foreach (var bundle in bundles)
            {
                var destFile = Path.Combine(Settings.BuildPlayerDataPath, bundle.nameWithAppendHash);
                var srcFile = Settings.GetBuildPath(bundle.nameWithAppendHash);
                if (!File.Exists(srcFile))
                {
                    Debug.LogWarningFormat("Bundle not found: {0}", bundle.name);
                    continue;
                }

                var dir = Path.GetDirectoryName(destFile);
                if (!Directory.Exists(dir) && !string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                File.Copy(srcFile, destFile, true);
            }

            var config = Settings.GetPlayerSettings();
            config.assets = bundles.ConvertAll(o => o.nameWithAppendHash);
            config.offlineMode = settings.offlineMode;
            Settings.SaveAsset(config);
        }

        public static void ClearHistory()
        {
            var usedFiles = new List<string>
            {
                Settings.GetPlatformName(),
                Settings.GetPlatformName() + ".manifest"
            };
            var manifest = Settings.GetManifest();
            usedFiles.Add($"{manifest.name}");
            usedFiles.Add($"{manifest.name}.version");
            var version = ManifestVersion.Load(Settings.GetBuildPath($"{manifest.name}.version"));
            usedFiles.Add($"{manifest.name}_v{version.version}_{version.crc}");
            usedFiles.Add($"{manifest.name}_v{version.version}_{version.crc}.version");
            foreach (var bundle in manifest.bundles)
            {
                usedFiles.Add(bundle.nameWithAppendHash);
                usedFiles.Add($"{bundle.name}.manifest");
            }

            var files = Directory.GetFiles(Settings.PlatformBuildPath);
            foreach (var file in files)
            {
                var name = Path.GetFileName(file);
                if (usedFiles.Contains(name)) continue;

                File.Delete(file);
                Debug.LogFormat("Delete {0}", file);
            }
        }
    }
}