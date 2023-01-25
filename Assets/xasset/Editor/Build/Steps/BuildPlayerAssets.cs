using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace xasset.editor
{
    public class BuildPlayerAssets : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public void OnPostprocessBuild(BuildReport report)
        {
            var dataPath = $"{Application.streamingAssetsPath}/{Assets.Bundles}";
            if (!Directory.Exists(dataPath)) return;
            FileUtil.DeleteFileOrDirectory(dataPath);
            FileUtil.DeleteFileOrDirectory(dataPath + ".meta");
            if (Directory.GetFiles(Application.streamingAssetsPath).Length != 0) return;
            FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath);
            FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath + ".meta");
        }

        public int callbackOrder { get; }

        public void OnPreprocessBuild(BuildReport report)
        {
            StartNew();
        }

        public static Action<BuildPlayerAssets> CustomBuilder { get; set; }
        private Versions versions { get; set; }

        public static void StartNew(Versions versions = null)
        {
            new BuildPlayerAssets {versions = versions}.Start();
        }

        private void Start()
        {
            if (versions == null) versions = Settings.GetDefaultVersions();
            if (CustomBuilder != null)
            {
                CustomBuilder.Invoke(this);
                return;
            }

            var dataPath = $"{Application.streamingAssetsPath}/{Assets.Bundles}";
            var settings = Settings.GetDefaultSettings();
            var playerAssets = settings.GetPlayerAssets();
            if (Directory.Exists(dataPath))
            {
                FileUtil.DeleteFileOrDirectory(dataPath);
                FileUtil.DeleteFileOrDirectory($"{dataPath}.meta");
            }

            var bundles = Builder.GetBundlesInBuild(settings, versions);
            if (bundles.Length > 0)
                CopyBundles(bundles, playerAssets);

            // 保存版本文件
            foreach (var version in versions.data)
            {
                var from = Settings.GetDataPath(version.file);
                var to = Assets.GetPlayerDataPath(version.file);
                Utility.CreateDirectoryIfNecessary(to);
                File.Copy(from, to, true);
            }

            // WebGL 不需要搞 PlayerAssets。
            if (Settings.Platform == Platform.WebGL)
                playerAssets.data.Clear();

            var json = JsonUtility.ToJson(playerAssets);
            // settings.json
            var path = Assets.GetPlayerDataPath(PlayerAssets.Filename);
            Utility.CreateDirectoryIfNecessary(path);
            File.WriteAllText(path, json);
            // versions.json
            path = Assets.GetPlayerDataPath(Versions.Filename);
            Utility.CreateDirectoryIfNecessary(path);
            versions.Save(path);
        }

        private static void CopyBundles(IEnumerable<ManifestBundle> bundles, PlayerAssets playerAssets)
        {
            foreach (var bundle in bundles)
            {
                var from = Settings.GetDataPath(bundle.file);
                var to = Assets.GetPlayerDataPath(bundle.file);
                var file = new FileInfo(from);
                if (file.Exists)
                {
                    Utility.CreateDirectoryIfNecessary(to);
                    file.CopyTo(to, true);
                    playerAssets.data.Add(bundle.hash);
                }
                else
                {
                    Logger.E($"File not found: {from}");
                }
            }
        }
    }
}