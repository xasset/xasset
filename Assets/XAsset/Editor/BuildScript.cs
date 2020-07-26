//
// BuildScript.cs
//
// Author:
//       fjy <jiyuan.feng@live.com>
//
// Copyright (c) 2020 fjy
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
using UnityEditor;
using UnityEngine;

namespace libx
{
    public static class BuildScript
    {
        public static string outputPath = "DLC/" + GetPlatformName();

        public static void ClearAssetBundles()
        {
            var allAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
            for (var i = 0; i < allAssetBundleNames.Length; i++)
            {
                var text = allAssetBundleNames[i];
                if (EditorUtility.DisplayCancelableProgressBar(
                    string.Format("Clear AssetBundles {0}/{1}", i, allAssetBundleNames.Length), text,
                    i * 1f / allAssetBundleNames.Length))
                    break;

                AssetDatabase.RemoveAssetBundleName(text, true);
            }

            EditorUtility.ClearProgressBar();
        }

        internal static void BuildRules()
        {
            var rules = GetBuildRules();
            rules.Build();
        }

        internal static BuildRules GetBuildRules()
        {
            return GetAsset<BuildRules>("Assets/Rules.asset");
        }

        public static void CopyAssetBundlesTo(string path)
        {
            var files = new[]
            { 
                Versions.Filename,
            };
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            foreach (var item in files)
            {
                var src = outputPath + "/" + item;
                var dest = Application.streamingAssetsPath + "/" + item;
                if (File.Exists(src))
                {
                    File.Copy(src, dest, true);
                }
            }
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
            List<string> scenes = new List<string>();
            foreach (var item in GetBuildRules().scenesInBuild)
            {
                var path = AssetDatabase.GetAssetPath(item);
                if (!string.IsNullOrEmpty(path))
                {
                    scenes.Add(path);
                }
            }

            return scenes.ToArray();
        }

        private static string GetAssetBundleManifestFilePath()
        {
            var relativeAssetBundlesOutputPathForPlatform = Path.Combine("Asset", GetPlatformName());
            return Path.Combine(relativeAssetBundlesOutputPathForPlatform, GetPlatformName()) + ".manifest";
        }

        public static void BuildStandalonePlayer()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "Build/" + GetPlatformName());
            //EditorUtility.SaveFolderPanel("Choose Location of the Built Game", "", "");
            if (path.Length == 0)
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
			BuildPipeline.BuildPlayer(levels, path + targetName, EditorUserBuildSettings.activeBuildTarget, option);
#else
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = levels,
                locationPathName = path + targetName,
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
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            return outputPath;
        }

        public static void BuildAssetBundles()
        {
            // Choose the output path according to the build target.
            var bundleDir = CreateAssetBundleDirectory(); 
            var targetPlatform = EditorUserBuildSettings.activeBuildTarget;
            var rules = GetBuildRules();
            var builds = rules.GetBuilds();
            var manifest = BuildPipeline.BuildAssetBundles(bundleDir, builds, rules.buildBundleOptions, targetPlatform);
            if (manifest == null)
            {
                return;
            } 
            BuildManifest(manifest, bundleDir, rules); 
        }

        private static void BuildManifest(AssetBundleManifest assetBundleManifest, string bundleDir, BuildRules rules)
        {
            var manifest = GetManifest();
            var allAssetBundles = assetBundleManifest.GetAllAssetBundles();
            var bundle2Ids = GetBundle2Ids(allAssetBundles);
            var bundles = GetBundles(assetBundleManifest, bundleDir, allAssetBundles, bundle2Ids);
            var dirs = new List<string>();
            var assets = new List<AssetRef>();
            var patches = new List<VPatch>(); 
            
            for (var i = 0; i < rules.assets.Length; i++)
            {
                var item = rules.assets[i];
                var path = item.path;
                var dir = Path.GetDirectoryName(path).Replace("\\", "/");
                var index = dirs.FindIndex(o => o.Equals(dir));
                if (index == -1)
                {
                    index = dirs.Count;
                    dirs.Add(dir);
                }

                var asset = new AssetRef();
                if (!bundle2Ids.TryGetValue(item.bundle, out asset.bundle))
                {
                    // 第三方资源
                    var bundle = new BundleRef();
                    bundle.id = bundles.Count;
                    bundle.name = Path.GetFileName(path);
                    using (var stream = File.OpenRead(path))
                    {
                        bundle.len = stream.Length;
                        bundle.crc = Utility.GetCRC32Hash(stream);
                    }

                    bundles.Add(bundle);
                    asset.bundle = bundle.id;
                } 
                
                asset.dir = index;
                asset.name = Path.GetFileName(path);
                assets.Add(asset);
                var patch = patches.Find(pr => pr.id == item.patch);
                if (patch == null)
                {
                    patch = new VPatch() { id = item.patch };
                    patches.Add(patch);
                } 
                if (asset.bundle != -1)
                {
                    if (! patch.files.Contains(asset.bundle))
                    {
                        patch.files.Add(asset.bundle);
                    }
                    var bundle = bundles[asset.bundle];
                    foreach (var child in bundle.children)
                    {
                        if (! patch.files.Contains(child))
                        {
                            patch.files.Add(child);
                        }
                    }
                }
            }

            manifest.dirs = dirs.ToArray();
            manifest.assets = assets.ToArray();
            manifest.bundles = bundles.ToArray();

            EditorUtility.SetDirty(manifest);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            var manifestBundleName = "manifest.unity3d";
            var builds = new[]
            {
                new AssetBundleBuild
                {
                    assetNames = new[] {AssetDatabase.GetAssetPath(manifest),},
                    assetBundleName = manifestBundleName
                }
            }; 
            
            var targetPlatform = EditorUserBuildSettings.activeBuildTarget; 
            BuildPipeline.BuildAssetBundles(bundleDir, builds, rules.buildBundleOptions, targetPlatform);
            {
                var path = bundleDir + "/" + manifestBundleName;
                var bundle = new BundleRef();
                bundle.id = bundles.Count;
                bundle.name = Path.GetFileName(path);
                using (var stream = File.OpenRead(path))
                {
                    bundle.len = stream.Length;
                    bundle.crc = Utility.GetCRC32Hash(stream);
                } 
                var patch = patches.Find(pr => pr.id == PatchId.Level1);
                if (patch == null)
                {
                    patch = new VPatch() { id = PatchId.Level1 };
                    patches.Add(patch);
                } 
                bundles.Add(bundle);
            }
            Versions.BuildVersion(bundleDir, bundles, patches, GetBuildRules().AddVersion()); 
        }

        private static List<BundleRef> GetBundles(AssetBundleManifest assetBundleManifest, string bundleDir,
            string[] allAssetBundles,
            Dictionary<string, int> bundle2Ids)
        {
            var bundles = new List<BundleRef>();
            for (var index = 0; index < allAssetBundles.Length; index++)
            {
                var bundle = allAssetBundles[index];
                var deps = assetBundleManifest.GetAllDependencies(bundle);
                var path = string.Format("{0}/{1}", bundleDir, bundle);
                if (File.Exists(path))
                {
                    using (var stream = File.OpenRead(path))
                    {
                        bundles.Add(new BundleRef
                        {
                            name = bundle,
                            id = index,
                            children = Array.ConvertAll(deps, input => bundle2Ids[input]),
                            len = stream.Length,
                            hash = assetBundleManifest.GetAssetBundleHash(bundle).ToString(), 
                            crc = Utility.GetCRC32Hash(stream)
                        });
                    }
                }
                else
                {
                    Debug.LogError(path + " file not exist.");
                }
            }

            return bundles;
        }

        private static Dictionary<string, int> GetBundle2Ids(string[] allAssetBundles)
        {
            var bundle2Ids = new Dictionary<string, int>();
            for (var index = 0; index < allAssetBundles.Length; index++)
            {
                var bundle = allAssetBundles[index];
                bundle2Ids[bundle] = index;
            }

            return bundle2Ids;
        }

        private static string GetBuildTargetName(BuildTarget target)
        {
            var time = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var name = PlayerSettings.productName + "-v" + PlayerSettings.bundleVersion + ".";
            switch (target)
            {
                case BuildTarget.Android:
                    return string.Format("/{0}{1}-{2}.apk", name, GetBuildRules().version, time);

                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return string.Format("/{0}{1}-{2}.exe", name, GetBuildRules().version, time);

#if UNITY_2017_3_OR_NEWER
                case BuildTarget.StandaloneOSX:
                    return "/" + name + ".app";

#else
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                case BuildTarget.StandaloneOSXUniversal:
                    return "/" + path + ".app";

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

        public static Manifest GetManifest()
        {
            return GetAsset<Manifest>(Assets.ManifestAsset);
        }
    }

    
}