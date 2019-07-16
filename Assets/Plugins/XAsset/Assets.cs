//
// Assets.cs
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
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Plugins.XAsset
{
    public class Assets : MonoBehaviour
    {
        private static string[] _bundles = new string[0];
        private static Dictionary<string, int> _bundleAssets = new Dictionary<string, int>();

        // ReSharper disable once InconsistentNaming
        private static readonly List<Asset> _assets = new List<Asset>();

        // ReSharper disable once InconsistentNaming
        private static readonly List<Asset> _unusedAssets = new List<Asset>();

        public static Dictionary<string, int> bundleAssets
        {
            get { return _bundleAssets; }
        }

        private static string updatePath { get; set; }

        public static void Initialize(Action onSuccess, Action<string> onError)
        {
            var instance = FindObjectOfType<Assets>();
            if (instance == null)
            {
                instance = new GameObject("Assets").AddComponent<Assets>();
                DontDestroyOnLoad(instance.gameObject);
            }

            if (string.IsNullOrEmpty(Utility.dataPath)) Utility.dataPath = Application.streamingAssetsPath;

            Log(string.Format("Init->assetBundleMode {0} | dataPath {1}", Utility.assetBundleMode, Utility.dataPath));

            if (Utility.assetBundleMode)
            {
                updatePath = Utility.updatePath;
                var platform = Utility.GetPlatform();
                var path = Path.Combine(Utility.dataPath, Path.Combine(Utility.AssetBundles, platform)) +
                           Path.DirectorySeparatorChar;
                Bundles.OverrideBaseDownloadingUrl += Bundles_overrideBaseDownloadingURL;
                Bundles.Initialize(path, platform, () =>
                {
                    var asset = LoadAsync(Utility.AssetsManifestAsset, typeof(AssetsManifest));
                    asset.completed += obj =>
                    {
                        var manifest = obj.asset as AssetsManifest;
                        if (manifest == null)
                        {
                            if (onError != null) onError("manifest == null");
                            return;
                        }

                        if (string.IsNullOrEmpty(Utility.downloadURL))
                            Utility.downloadURL = manifest.downloadURL;
                        Bundles.activeVariants = manifest.activeVariants;
                        _bundles = manifest.bundles;
                        var dirs = manifest.dirs;
                        _bundleAssets = new Dictionary<string, int>(manifest.assets.Length);
                        for (int i = 0, max = manifest.assets.Length; i < max; i++)
                        {
                            var item = manifest.assets[i];
                            _bundleAssets[string.Format("{0}/{1}", dirs[item.dir], item.name)] = item.bundle;
                        }

                        if (onSuccess != null)
                            onSuccess();
                        obj.Release();
                    };
                }, onError);
            }
            else
            {
                if (onSuccess != null)
                    onSuccess();
            }
        }

        public static string[] GetAllDependencies(string path)
        {
            string assetBundleName;
            return GetAssetBundleName(path, out assetBundleName) ? Bundles.GetAllDependencies(assetBundleName) : null;
        }

        public static SceneAsset LoadScene(string path, bool async, bool addictive)
        {
            var asset = async ? new SceneAssetAsync(path, addictive) : new SceneAsset(path, addictive);
            GetAssetBundleName(path, out asset.assetBundleName);
            asset.Load();
            asset.Retain();
            _assets.Add(asset);
            return asset;
        }

        public static void UnloadScene(string path)
        {
            for (int i = 0, max = _assets.Count; i < max; i++)
            {
                var item = _assets[i];
                if (!item.name.Equals(path))
                    continue;
                Unload(item);
                break;
            }
        }

        public static Asset Load(string path, Type type)
        {
            return Load(path, type, false);
        }

        public static Asset LoadAsync(string path, Type type)
        {
            return Load(path, type, true);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static void Unload(Asset asset)
        {
            asset.Release();
            for (var i = 0; i < _unusedAssets.Count; i++)
            {
                var item = _unusedAssets[i];
                if (!item.name.Equals(asset.name))
                    continue;
                item.Unload();
                _unusedAssets.RemoveAt(i);
                return;
            }
        }

        private void Update()
        {
            for (var i = 0; i < _assets.Count; i++)
            {
                var item = _assets[i];
                if (item.Update() || !item.IsUnused())
                    continue;
                _unusedAssets.Add(item);
                _assets.RemoveAt(i);
                i--;
            }

            for (var i = 0; i < _unusedAssets.Count; i++)
            {
                var item = _unusedAssets[i];
                item.Unload();
                Log("Unload->" + item.name);
            }

            _unusedAssets.Clear();

            Bundles.Update();
        }

        [Conditional("LOG_ENABLE")]
        private static void Log(string s)
        {
            Debug.Log(string.Format("[Assets]{0}", s));
        }

        private static Asset Load(string path, Type type, bool async)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("invalid path");
                return null;
            }

            for (int i = 0, max = _assets.Count; i < max; i++)
            {
                var item = _assets[i];
                if (!item.name.Equals(path))
                    continue;
                item.Retain();
                return item;
            }

            string assetBundleName;
            Asset asset;
            if (GetAssetBundleName(path, out assetBundleName))
            {
                asset = async ? new BundleAssetAsync(assetBundleName) : new BundleAsset(assetBundleName);
            }
            else
            {
                if (path.StartsWith("http://") || path.StartsWith("https://") || path.StartsWith("file://") ||
                    path.StartsWith("ftp://") || path.StartsWith("jar:file://"))
                    asset = new WebAsset();
                else
                    asset = new Asset();
            }

            asset.name = path;
            asset.assetType = type;
            _assets.Add(asset);
            asset.Load();
            asset.Retain();

            Log(string.Format("Load->{0}|{1}", path, assetBundleName));
            return asset;
        }

        private static bool GetAssetBundleName(string path, out string assetBundleName)
        {
            if (path.Equals(Utility.AssetsManifestAsset))
            {
                assetBundleName = Path.GetFileNameWithoutExtension(path).ToLower();
                return true;
            }

            assetBundleName = null;
            int bundle;
            if (!_bundleAssets.TryGetValue(path, out bundle))
                return false;
            assetBundleName = _bundles[bundle];
            return true;
        }

        private static string Bundles_overrideBaseDownloadingURL(string bundleName)
        {
            return !File.Exists(Path.Combine(updatePath, bundleName)) ? null : updatePath;
        }
    }
}