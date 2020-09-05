//
// BuildRules.cs
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
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace libx
{
    public enum NameBy
    {
        Explicit,
        Path,
        Directory,
        TopDirectory
    }

    [Serializable]
    public class RuleAsset
    {
        public string bundle;
        public string path;
    }

    [Serializable]
    public class RuleBundle
    {
        public string name;
        public string[] assets;
    }

    [Serializable]
    public class BuildRule
    {
        [Tooltip("搜索路径")] public string searchPath;

        [Tooltip("搜索通配符，多个之间请用,(逗号)隔开")] public string searchPattern;

        [Tooltip("命名规则")] public NameBy nameBy = NameBy.Path;

        [Tooltip("Explicit的名称")] public string assetBundleName;

        public string[] GetAssets()
        {
            var patterns = searchPattern.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            if (!Directory.Exists(searchPath))
            {
                Debug.LogWarning("Rule searchPath not exist:" + searchPath);
                return new string[0];
            }

            var getFiles = new List<string>();
            foreach (var item in patterns)
            {
                var files = Directory.GetFiles(searchPath, item, SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    if (Directory.Exists(file)) continue;
                    var ext = Path.GetExtension(file).ToLower();
                    if ((ext == ".fbx" || ext == ".anim") && !item.Contains(ext)) continue;
                    if (!BuildRules.ValidateAsset(file)) continue;
                    var asset = file.Replace("\\", "/");
                    getFiles.Add(asset);
                }
            }

            return getFiles.ToArray();
        }
    }

    public class BuildRules : ScriptableObject
    {
        private readonly Dictionary<string, string> _asset2Bundles = new Dictionary<string, string>();
        private readonly Dictionary<string, string[]> _conflicted = new Dictionary<string, string[]>();
        private readonly List<string> _duplicated = new List<string>();
        private readonly Dictionary<string, HashSet<string>> _tracker = new Dictionary<string, HashSet<string>>();
		[Header("Patterns")]
		public string searchPatternAsset = "*.asset";
		public string searchPatternController = "*.controller";
		public string searchPatternDir = "*";
		public string searchPatternMaterial = "*.mat";
		public string searchPatternPng = "*.png";
		public string searchPatternPrefab = "*.prefab";
		public string searchPatternScene = "*.unity";
		public string searchPatternText = "*.txt,*.bytes,*.json,*.csv,*.xml,*htm,*.html,*.yaml,*.fnt";
        public static bool nameByHash = true;
        
		[Tooltip("构建的版本号")]
		[Header("Builds")] 
        public int version;
        [Tooltip("BuildPlayer 的时候被打包的场景")] public SceneAsset[] scenesInBuild = new SceneAsset[0]; 
        public BuildRule[] rules = new BuildRule[0]; 
		[Header("Assets")]
		[HideInInspector]public RuleAsset[] ruleAssets = new RuleAsset[0];
        [HideInInspector]public RuleBundle[] ruleBundles = new RuleBundle[0];
        #region API

        public int AddVersion()
        {
            version = version + 1;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            return version;
        }

        public void Apply()
        {
            Clear();
            CollectAssets();
            AnalysisAssets();
            OptimizeAssets();
            Save();
        }

        public AssetBundleBuild[] GetBuilds()
        {
            var builds = new List<AssetBundleBuild>();
            foreach (var bundle in ruleBundles)
            {
                builds.Add(new AssetBundleBuild
                {
                    assetNames = bundle.assets,
                    assetBundleName = bundle.name
                });
            }

            return builds.ToArray();
        }

        #endregion

        #region Private

        internal static bool ValidateAsset(string asset)
        {
            if (!asset.StartsWith("Assets/")) return false;

            var ext = Path.GetExtension(asset).ToLower();
            return ext != ".dll" && ext != ".cs" && ext != ".meta" && ext != ".js" && ext != ".boo";
        }

        private static bool IsScene(string asset)
        {
            return asset.EndsWith(".unity");
        }

        private static string RuledAssetBundleName(string name)
        {
            if (nameByHash)
            {
                return Utility.GetMD5Hash(name) + Assets.Extension; 
            } 
            return name.Replace("\\", "/").ToLower() + Assets.Extension;
        }

        private void Track(string asset, string bundle)
        {
            HashSet<string> assets;
            if (!_tracker.TryGetValue(asset, out assets))
            {
                assets = new HashSet<string>();
                _tracker.Add(asset, assets);
            }

            assets.Add(bundle);
            if (assets.Count > 1)
            {
                string bundleName;
                _asset2Bundles.TryGetValue(asset, out bundleName);
                if (string.IsNullOrEmpty(bundleName))
                {
                    _duplicated.Add(asset);
                }
            }
        }

        private Dictionary<string, List<string>> GetBundles()
        {
            var bundles = new Dictionary<string, List<string>>();
            foreach (var item in _asset2Bundles)
            {
                var bundle = item.Value;
                List<string> list;
                if (!bundles.TryGetValue(bundle, out list))
                {
                    list = new List<string>();
                    bundles[bundle] = list;
                }

                if (!list.Contains(item.Key)) list.Add(item.Key);
            }

            return bundles;
        }

        private void Clear()
        {
            _tracker.Clear();
            _duplicated.Clear();
            _conflicted.Clear();
            _asset2Bundles.Clear();
        }

        private void Save()
        {
            var getBundles = GetBundles();
            ruleBundles = new RuleBundle[getBundles.Count];
            var i = 0;
            foreach (var item in getBundles)
            {
                ruleBundles[i] = new RuleBundle
                {
                    name = item.Key,
                    assets = item.Value.ToArray()
                };
                i++;
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        private void OptimizeAssets()
        {
            int i = 0, max = _conflicted.Count;
            foreach (var item in _conflicted)
            {
                if (EditorUtility.DisplayCancelableProgressBar(string.Format("优化冲突{0}/{1}", i, max), item.Key,
                    i / (float) max)) break;
                var list = item.Value;
                foreach (var asset in list)
                    if (!IsScene(asset))
                        _duplicated.Add(asset);
                i++;
            }

            for (i = 0, max = _duplicated.Count; i < max; i++)
            {
                var item = _duplicated[i];
                if (EditorUtility.DisplayCancelableProgressBar(string.Format("优化冗余{0}/{1}", i, max), item,
                    i / (float) max)) break;
                OptimizeAsset(item);
            }
        }

        private void AnalysisAssets()
        {
            var getBundles = GetBundles();
            int i = 0, max = getBundles.Count;
            foreach (var item in getBundles)
            {
                var bundle = item.Key;
                if (EditorUtility.DisplayCancelableProgressBar(string.Format("分析依赖{0}/{1}", i, max), bundle,
                    i / (float) max)) break;
                var assetPaths = getBundles[bundle];
                if (assetPaths.Exists(IsScene) && !assetPaths.TrueForAll(IsScene))
                    _conflicted.Add(bundle, assetPaths.ToArray());
                var dependencies = AssetDatabase.GetDependencies(assetPaths.ToArray(), true);
                if (dependencies.Length > 0)
                    foreach (var asset in dependencies)
                        if (ValidateAsset(asset))
                            Track(asset, bundle);
                i++;
            }
        }

        private void CollectAssets()
        {
            for (int i = 0, max = rules.Length; i < max; i++)
            {
                var rule = rules[i];
                if (EditorUtility.DisplayCancelableProgressBar(string.Format("收集资源{0}/{1}", i, max), rule.searchPath,
                    i / (float) max))
                    break;
                ApplyRule(rule);
            }

            var list = new List<RuleAsset>();
            foreach (var item in _asset2Bundles)
                list.Add(new RuleAsset
                {
                    path = item.Key,
                    bundle = item.Value
                });
            list.Sort((a, b) => string.Compare(a.path, b.path, StringComparison.Ordinal));
            ruleAssets = list.ToArray();
        }

        private void OptimizeAsset(string asset)
        {
            if (asset.EndsWith(".shader"))
                _asset2Bundles[asset] = RuledAssetBundleName("shaders");
            else
                _asset2Bundles[asset] = RuledAssetBundleName(asset);
        }

        private void ApplyRule(BuildRule rule)
        {
            var assets = rule.GetAssets();
            switch (rule.nameBy)
            {
                case NameBy.Explicit:
                {
                    foreach (var asset in assets) _asset2Bundles[asset] = RuledAssetBundleName(rule.assetBundleName);

                    break;
                }
                case NameBy.Path:
                {
                    foreach (var asset in assets) _asset2Bundles[asset] = RuledAssetBundleName(asset);

                    break;
                }
                case NameBy.Directory:
                {
                    foreach (var asset in assets)
                        _asset2Bundles[asset] = RuledAssetBundleName(Path.GetDirectoryName(asset));

                    break;
                }
                case NameBy.TopDirectory:
                {
                    var startIndex = rule.searchPath.Length;
                    foreach (var asset in assets)
                    {
                        var dir = Path.GetDirectoryName(asset);
                        if (!string.IsNullOrEmpty(dir))
                            if (!dir.Equals(rule.searchPath))
                            {
                                var pos = dir.IndexOf("/", startIndex + 1, StringComparison.Ordinal);
                                if (pos != -1) dir = dir.Substring(0, pos);
                            }

                        _asset2Bundles[asset] = RuledAssetBundleName(dir);
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}