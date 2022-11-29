using System;
using System.Collections.Generic;
using UnityEditor;

namespace xasset.editor
{
    /// <summary>
    ///     打包安装包
    /// </summary>
    public static class BuildPlayer
    {
        private static string GetTimeForNow()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        private static string GetBuildTargetName(BuildTarget target)
        {
            var targetName =
                $"{PlayerSettings.productName}-v{PlayerSettings.bundleVersion}-{GetTimeForNow()}";
            switch (target)
            {
                case BuildTarget.Android:
                    return targetName + (EditorUserBuildSettings.buildAppBundle ? ".aab" : ".apk");
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return targetName + ".exe";
                case BuildTarget.StandaloneOSX:
                    return targetName + ".app";
                default:
                    return targetName;
            }
        }

        public static void Build(string path = null)
        {
            if (string.IsNullOrEmpty(path)) path = $"Build/{Settings.Platform}";

            var levels = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
                if (scene.enabled)
                    levels.Add(scene.path);

            if (levels.Count == 0)
            {
                Logger.I("Nothing to build.");
                return;
            }

            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var targetName = GetBuildTargetName(buildTarget);
            if (targetName == null) return;

            var options = new BuildPlayerOptions
            {
                scenes = levels.ToArray(),
                locationPathName = $"{path}/{targetName}",
                target = buildTarget,
                options = EditorUserBuildSettings.development
                    ? BuildOptions.Development
                    : BuildOptions.None
            };
            BuildPipeline.BuildPlayer(options);
            EditorUtility.OpenWithDefaultApp(path);
        }
    }
}