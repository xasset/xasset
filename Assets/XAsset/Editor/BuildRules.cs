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

namespace libx
{
    /// <summary>
    /// 资源打包的分组方式
    /// </summary>
    public enum GroupBy
    {
        None, 
        Explicit,
        Filename,
        Directory,
    }

    [Serializable]
    public class AssetBuild
    {
        public string path;
        public PatchId patch;
        public string bundle;
        public GroupBy groupBy = GroupBy.Filename;
    }

    [Serializable]
    public class BundleBuild
    {
        public string assetBundleName;
        public string[] assetNames;
        public AssetBundleBuild ToBuild()
        {
            return new AssetBundleBuild()
            {
                assetBundleName = assetBundleName,
                assetNames = assetNames
            };
        }
    }

    public class BuildRules : ScriptableObject
    {
        private readonly List<string> _duplicated = new List<string>();
        private readonly Dictionary<string, string[]> _conflicted = new Dictionary<string, string[]>();
        private readonly Dictionary<string, HashSet<string>> _tracker = new Dictionary<string, HashSet<string>>();
        private readonly Dictionary<string, string> _asset2Bundles = new Dictionary<string, string>(); 
        
        [Tooltip("构建的版本号")]  
        public int version;
        [Tooltip("是否把资源名字哈希处理")]
        public bool nameByHash = true;
        [Tooltip("打包选项")]
        public BuildAssetBundleOptions buildBundleOptions = BuildAssetBundleOptions.ChunkBasedCompression;
        [Tooltip("BuildPlayer 的时候被打包的场景")] 
        public SceneAsset[] scenesInBuild = new SceneAsset[0];
        [Tooltip("所有要打包的资源")] 
        public AssetBuild[] assets = new AssetBuild[0]; 
        [Tooltip("所有打包的资源")]
        public BundleBuild[] bundles = new BundleBuild[0];
        
        #region API

        public void GroupAsset(string path, GroupBy groupBy = GroupBy.Filename)
        {
            bool Match(AssetBuild bundleAsset)
            {
                return bundleAsset.path.Equals(path);
            } 
            var asset = ArrayUtility.Find(assets, Match);
            if (asset != null)
            {
                asset.groupBy = groupBy; 
                return;
            }
            ArrayUtility.Add(ref assets, new AssetBuild()
            {
                path = path,
                groupBy = groupBy, 
            }); 
        } 
        
        public void PatchAsset(string path, PatchId patch)
        {
            bool Match(AssetBuild bundleAsset)
            {
                return bundleAsset.path.Equals(path);
            } 
            var asset = ArrayUtility.Find(assets, Match);
            if (asset != null)
            {
                asset.patch = patch; 
                return;
            }
            ArrayUtility.Add(ref assets, new AssetBuild()
            {
                path = path,
                patch = patch, 
            }); 
        } 

        public int AddVersion()
        {
            version = version + 1; 
            return version;
        }

        public void Build()
        {
            Clear();
            CollectAssets();
            AnalysisAssets();
            OptimizeAssets();
            Save();
        }

        public AssetBundleBuild[] GetBuilds()
        {
            return Array.ConvertAll(bundles, input => input.ToBuild());
        }

        #endregion

        #region Private

        private string GetBundle(AssetBuild assetBuild)
        {
            if (assetBuild.path.EndsWith(".shader"))
            {
                return RuledAssetBundleName("shaders");
            }
            switch (assetBuild.groupBy)
            {
                case GroupBy.Explicit: return RuledAssetBundleName(assetBuild.bundle);
                case GroupBy.Filename: return RuledAssetBundleName(Path.Combine(Path.GetDirectoryName(assetBuild.path), Path.GetFileNameWithoutExtension(assetBuild.path)));
                case GroupBy.Directory: return RuledAssetBundleName(Path.GetDirectoryName(assetBuild.path));
                default: return string.Empty;
            }
        }

        internal bool ValidateAsset(string asset)
        {
            if (!asset.StartsWith("Assets/")) return false;

            var ext = Path.GetExtension(asset).ToLower();
            return ext != ".dll" && ext != ".cs" && ext != ".meta" && ext != ".js" && ext != ".boo";
        }

        private bool IsScene(string asset)
        {
            return asset.EndsWith(".unity");
        }

        private string RuledAssetBundleName(string assetName)
        {
            if (nameByHash)
            {
                return Utility.GetMD5Hash(assetName) + Assets.Extension;
            }

            return assetName.Replace("\\", "/").ToLower() + Assets.Extension;
        }

        private void Track(string asset, string bundle)
        {
            if (! _asset2Bundles.ContainsKey(asset))
            {
                _asset2Bundles[asset] = Path.GetFileNameWithoutExtension(bundle) + "_children" + Assets.Extension;
            }
            
            HashSet<string> hashSet;
            if (!_tracker.TryGetValue(asset, out hashSet))
            {
                hashSet = new HashSet<string>();
                _tracker.Add(asset, hashSet);
            }
            
            hashSet.Add(bundle);
            
            if (hashSet.Count > 1)
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
            var dictionary = new Dictionary<string, List<string>>();
            foreach (var item in _asset2Bundles)
            {
                var bundle = item.Value;
                List<string> list;
                if (!dictionary.TryGetValue(bundle, out list))
                {
                    list = new List<string>();
                    dictionary[bundle] = list;
                }

                if (!list.Contains(item.Key)) list.Add(item.Key);
            }

            return dictionary;
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

            bundles = new BundleBuild[getBundles.Count];
            var i = 0;
            foreach (var item in getBundles)
            {
                bundles[i] = new BundleBuild
                {
                    assetBundleName = item.Key,
                    assetNames = item.Value.ToArray()
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
            var list = new List<AssetBuild>();
            for (var index = 0; index < this.assets.Length; index++)
            {
                var asset = this.assets[index];
                if (File.Exists(asset.path) && ValidateAsset(asset.path))
                {
                    list.Add(asset);
                }
            }

            foreach (var asset in list)
            {
                asset.bundle = GetBundle(asset);
                _asset2Bundles[asset.path] = asset.bundle;
            }

            assets = list.ToArray();
        }

        private void OptimizeAsset(string asset)
        {
            if (asset.EndsWith(".shader"))
                _asset2Bundles[asset] = RuledAssetBundleName("shaders");
            else
                _asset2Bundles[asset] = RuledAssetBundleName(asset);
        }

        #endregion
    }
}