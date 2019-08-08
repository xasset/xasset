//
// BuildScript.cs
//
// Author:
//       fjy <jiyuan.feng@live.com>
//
// Copyright (c) 2019 fjy
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEditor;
using UnityEngine;

namespace Plugins.XAsset.Editor
{
    public static class BuildScript
    {
        public static string overloadedDevelopmentServerURL = "";

        public static void CopyAssetBundlesTo(string outputPath)
        {
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);
            var outputFolder = GetPlatformName();
            var source = Path.Combine(Path.Combine(Environment.CurrentDirectory, Utility.AssetBundles), outputFolder);
            if (!Directory.Exists(source))
                Debug.Log("No assetBundle output folder, try to build the assetBundles first.");
            var destination = Path.Combine(outputPath, outputFolder);
            if (Directory.Exists(destination))
                FileUtil.DeleteFileOrDirectory(destination);
            FileUtil.CopyFileOrDirectory(source, destination);
        }

        public static string GetPlatformName()
        {
            return GetPlatformForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
        }

        private static string GetPlatformForAssetBundles(BuildTarget target)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (target)
            {
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.WebGL:
                    return "WebGL";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
#if UNITY_2017_3_OR_NEWER
                case BuildTarget.StandaloneOSX:
                    return "OSX";
#else
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                case BuildTarget.StandaloneOSXUniversal:
                    return "OSX";
#endif
                default:
                    return null;
            }
        }

        private static string[] GetLevelsFromBuildSettings()
        {
            return EditorBuildSettings.scenes.Select(scene => scene.path).ToArray();
        }

        private static string GetAssetBundleManifestFilePath()
        {
            var relativeAssetBundlesOutputPathForPlatform = Path.Combine(Utility.AssetBundles, GetPlatformName());
            return Path.Combine(relativeAssetBundlesOutputPathForPlatform, GetPlatformName()) + ".manifest";
        }

        public static void BuildStandalonePlayer()
        {
            var outputPath = EditorUtility.SaveFolderPanel("Choose Location of the Built Game", "", "");
            if (outputPath.Length == 0)
                return;

            var levels = GetLevelsFromBuildSettings();
            if (levels.Length == 0)
            {
                Debug.Log("Nothing to build.");
                return;
            }

            var targetName = GetBuildTargetName(EditorUserBuildSettings.activeBuildTarget);
            if (targetName == null)
                return;
#if UNITY_5_4 || UNITY_5_3 || UNITY_5_2 || UNITY_5_1 || UNITY_5_0
			BuildOptions option = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
			BuildPipeline.BuildPlayer(levels, outputPath + targetName, EditorUserBuildSettings.activeBuildTarget, option);
#else
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = levels,
                locationPathName = outputPath + targetName,
                assetBundleManifestPath = GetAssetBundleManifestFilePath(),
                target = EditorUserBuildSettings.activeBuildTarget,
                options = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None
            };
            BuildPipeline.BuildPlayer(buildPlayerOptions);
#endif
        }

        public static string CreateAssetBundleDirectory()
        {
            // Choose the output path according to the build target.
            var outputPath = Path.Combine(Utility.AssetBundles, GetPlatformName());
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            return outputPath;
        }

        private static Dictionary<string, string> GetVersions(AssetBundleManifest manifest)
        {
            var items = manifest.GetAllAssetBundles();
            return items.ToDictionary(item => item, item => manifest.GetAssetBundleHash(item).ToString());
        }

        private static void LoadVersions(string versionsTxt, IDictionary<string, string> versions)
        {
            if (versions == null)
                throw new ArgumentNullException("versions");
            if (!File.Exists(versionsTxt))
                return;
            using (var s = new StreamReader(versionsTxt))
            {
                string line;
                while ((line = s.ReadLine()) != null)
                {
                    if (line == string.Empty)
                        continue;
                    var fields = line.Split(':');
                    if (fields.Length > 1)
                        versions.Add(fields[0], fields[1]);
                }
            }
        }

        private static void SaveVersions(string versionsTxt, Dictionary<string, string> versions)
        {
            if (File.Exists(versionsTxt))
                File.Delete(versionsTxt);
            using (var s = new StreamWriter(versionsTxt))
            {
                foreach (var item in versions)
                    s.WriteLine(item.Key + ':' + item.Value);
                s.Flush();
                s.Close();
            }
        }

        public static void RemoveUnusedAssetBundleNames()
        {
            var manifest = GetManifest();
            var assetBundleNames = manifest.bundles;
            var variantNames = manifest.activeVariants;
            var dirs = manifest.dirs;

            List<string> usedBundles = new List<string>();
            List<string> usedDirs = new List<string>();
            List<string> usedVariants = new List<string>();

            for (int i = 0; i < manifest.assets.Length; i++)
            {
                var item = manifest.assets[i];
                var assetPath = dirs[item.dir] + "/" + item.name;
                if (System.IO.File.Exists(assetPath) && !string.IsNullOrEmpty(manifest.bundles[item.bundle]))
                {
                    var bundleIndex = usedBundles.FindIndex((string obj) =>
                    {
                        return obj.Equals(assetBundleNames[item.bundle]);
                    });
                    if (bundleIndex == -1)
                    {
                        usedBundles.Add(assetBundleNames[item.bundle]);
                        bundleIndex = usedBundles.Count - 1;
                    }

                    var dir = System.IO.Path.GetDirectoryName(assetPath).Replace("\\", "/");
                    var dirIndex = usedDirs.FindIndex(delegate (string obj) { return obj == dir; });
                    if (dirIndex == -1)
                    {
                        usedDirs.Add(dir);
                        dirIndex = usedDirs.Count - 1;
                    }

                    if (item.variant != -1)
                    {
                        var variantIndex = usedVariants.FindIndex(delegate (string obj) { return obj == dir; });
                        if (variantIndex == -1)
                        {
                            usedVariants.Add(variantNames[item.variant]);
                            variantIndex = usedVariants.Count - 1;
                        }

                        item.variant = variantIndex;
                    }

                    item.bundle = bundleIndex;
                    item.dir = dirIndex;
                }
                else
                {
                    ArrayUtility.RemoveAt(ref manifest.assets, i);
                    i--;
                }
            }

            manifest.dirs = usedDirs.ToArray();
            manifest.bundles = usedBundles.ToArray();
            EditorUtility.SetDirty(manifest);
        }

        /// 传入manifestCache表示外部负责文件保存,避免 loop 中多次调用AssetDatabase.SaveAssets影响速度
        public static void SetAssetBundleNameAndVariant(string assetPath, string bundleName, string variant, AssetsManifest manifestCache = null)
        {
            var manifest = manifestCache == null ? GetManifest() : manifestCache;
            var dir = Path.GetDirectoryName(assetPath).Replace("\\", "/");
            var dirs = manifest.dirs;
            var dirIndex = ArrayUtility.FindIndex(dirs, (string obj) => { return obj == dir; });

            if (dirIndex == -1)
            {
                ArrayUtility.Add(ref manifest.dirs, dir);
                dirIndex = manifest.dirs.Length - 1;
                dirs = manifest.dirs;
            }

            var assetBundleNames = manifest.bundles;
            var bundleIndex = ArrayUtility.FindIndex(assetBundleNames, (string obj) => { return obj == bundleName; });

            if (bundleIndex == -1)
            {
                ArrayUtility.Add(ref manifest.bundles, bundleName);
                assetBundleNames = manifest.bundles;
                bundleIndex = assetBundleNames.Length - 1;
            }

            var variantNames = manifest.activeVariants;
            var variantIndex = ArrayUtility.FindIndex(variantNames, (string obj) => { return obj == variant; });

            if (variantIndex == -1 && !string.IsNullOrEmpty(variant))
            {
                ArrayUtility.Add(ref manifest.activeVariants, variant);
                variantNames = manifest.activeVariants;
                variantIndex = variantNames.Length - 1;
            }

            var assets = manifest.assets;
            var assetIndex = ArrayUtility.FindIndex(assets, (AssetData obj) =>
            {
                var path = dirs[obj.dir] + "/" + obj.name;
                return path == assetPath;
            });

            if (assetIndex == -1)
            {
                var info = new AssetData();
                ArrayUtility.Add(ref manifest.assets, info);
                assetIndex = manifest.assets.Length - 1;
                assets = manifest.assets;
            }

            var asset = assets[assetIndex];
            asset.name = Path.GetFileName(assetPath);
            asset.bundle = bundleIndex;
            asset.variant = variantIndex;
            asset.dir = dirIndex;
            if (manifestCache == null)
            {
                EditorUtility.SetDirty(manifest);
                AssetDatabase.SaveAssets();
            }
        }

        public static void BuildManifest()
        {
            var manifest = GetManifest();
            var assetPath = AssetDatabase.GetAssetPath(manifest);
            var bundleName = Path.GetFileNameWithoutExtension(assetPath).ToLower();
            SetAssetBundleNameAndVariant(assetPath, bundleName, null);
        }

        public static void BuildAssetBundles()
        {
            var assetManifest = GetManifest();
            BuildScript.RemoveUnusedAssetBundleNames();

            var assets = assetManifest.assets;
            var assetBundleNames = assetManifest.bundles;
            var dirs = assetManifest.dirs;

            var map = new Dictionary<string, List<string>>();

            foreach (var item in assetManifest.bundles)
            {
                map[item] = new List<string>();
            }

            foreach (var item in assets)
            {
                var assetPath = dirs[item.dir] + "/" + item.name;
                map[assetBundleNames[item.bundle]].Add(assetPath);
            }

            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
            foreach (var item in map)
            {
                builds.Add(new AssetBundleBuild()
                {
                    assetBundleName = item.Key,
                    assetNames = item.Value.ToArray()
                });
            }

            // Choose the output path according to the build target.
            var outputPath = CreateAssetBundleDirectory();

            const BuildAssetBundleOptions options = BuildAssetBundleOptions.ChunkBasedCompression;

            var manifest =
                BuildPipeline.BuildAssetBundles(outputPath, builds.ToArray(), options,
                    EditorUserBuildSettings.activeBuildTarget);
            var versionsTxt = outputPath + "/versions.txt";
            var versions = new Dictionary<string, string>();
            LoadVersions(versionsTxt, versions);

            var buildVersions = GetVersions(manifest);

            var updates = new List<string>();

            foreach (var item in buildVersions)
            {
                string hash;
                var isNew = true;
                if (versions.TryGetValue(item.Key, out hash))
                    if (hash.Equals(item.Value))
                        isNew = false;
                if (isNew)
                    updates.Add(item.Key);
            }

            if (updates.Count > 0)
            {
                using (var s = new StreamWriter(File.Open(outputPath + "/updates.txt", FileMode.Append)))
                {
                    s.WriteLine(DateTime.Now.ToFileTime() + ":");
                    foreach (var item in updates)
                        s.WriteLine(item);
                    s.Flush();
                    s.Close();
                }

                SaveVersions(versionsTxt, buildVersions);
            }
            else
            {
                Debug.Log("nothing to update.");
            }

            string[] ignoredFiles = { GetPlatformName(), "versions.txt", "updates.txt", "manifest" };

            var files = Directory.GetFiles(outputPath, "*", SearchOption.AllDirectories);

            var deletes = (from t in files
                           let file = t.Replace('\\', '/').Replace(outputPath.Replace('\\', '/') + '/', "")
                           where !file.EndsWith(".manifest", StringComparison.Ordinal) && !Array.Exists(ignoredFiles, s => s.Equals(file))
                           where !buildVersions.ContainsKey(file)
                           select t).ToList();

            foreach (var delete in deletes)
            {
                if (!File.Exists(delete))
                    continue;
                File.Delete(delete);
                File.Delete(delete + ".manifest");
            }

            deletes.Clear();
        }

        private static string GetBuildTargetName(BuildTarget target)
        {
            var name = PlayerSettings.productName + "_" + PlayerSettings.bundleVersion;
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (target)
            {
                case BuildTarget.Android:
                    return "/" + name + PlayerSettings.Android.bundleVersionCode + ".apk";

                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "/" + name + PlayerSettings.Android.bundleVersionCode + ".exe";

#if UNITY_2017_3_OR_NEWER
                case BuildTarget.StandaloneOSX:
                    return "/" + name + ".app";

#else
                    case BuildTarget.StandaloneOSXIntel:
                    case BuildTarget.StandaloneOSXIntel64:
                    case BuildTarget.StandaloneOSXUniversal:
                                        return "/" + name + ".app";

#endif

                case BuildTarget.WebGL:
                case BuildTarget.iOS:
                    return "";
                // Add more build targets for your own.
                default:
                    Debug.Log("Target not implemented.");
                    return null;
            }
        }

        private static T GetAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
            }

            return asset;
        }

        public static Settings GetSettings()
        {
            const string path = "Assets/Settings.asset";
            return GetAsset<Settings>(path);
        }

        public static AssetsManifest GetManifest()
        {
            return GetAsset<AssetsManifest>(Utility.AssetsManifestAsset);
        }

        public static string GetServerURL()
        {
            string downloadURL;
            if (string.IsNullOrEmpty(overloadedDevelopmentServerURL) == false)
            {
                downloadURL = overloadedDevelopmentServerURL;
            }
            else
            {
                IPHostEntry host;
                string localIP = "";
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }

                downloadURL = "http://" + localIP + ":7888/";
            }

            return downloadURL;
        }
    }
}