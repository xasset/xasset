//
// Assets.cs
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
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace libx
{
    public delegate string OverrideDataPathDelegate(string bundleName);

    public delegate Object LoadDelegate(string path, Type type);

    public delegate string GetPlatformDelegate();

    public class Assets : MonoBehaviour
    {
        public static readonly string AssetBundles = "AssetBundles";

        public static readonly string AssetsManifestAsset = "Assets/Manifest.asset";

        public static OverrideDataPathDelegate OverrideBaseDownloadingUrl;

        public static bool assetBundleMode = true;

        public static LoadDelegate loadDelegate = null;

        public static GetPlatformDelegate getPlatformDelegate = null;

        [Conditional("LOG_ENABLE")]
        private static void Log(string s)
        {
            Debug.Log(string.Format("[Assets]{0}", s));
        }

        #region API

        public static string[] GetAllBundleAssetPaths()
        {
            List<string> assets = new List<string>();
            assets.AddRange(_bundleAssets.Keys);
            return assets.ToArray();
        }

        public static AssetsInitRequest Initialize()
        {
            var instance = FindObjectOfType<Assets>();
            if (instance == null)
            {
                instance = new GameObject("Assets").AddComponent<Assets>();
                DontDestroyOnLoad(instance.gameObject);
            }

            Log(string.Format("Initialize->assetBundleMode {0}", Assets.assetBundleMode));

            var protocal = string.Empty;
            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                protocal = "file://";
            }
            else if (Application.platform == RuntimePlatform.OSXPlayer ||
                     Application.platform == RuntimePlatform.WindowsPlayer)
            {
                protocal = "file:///";
            }

            if (string.IsNullOrEmpty(dataPath))
                dataPath = Application.streamingAssetsPath;

            platform = getPlatformDelegate != null ? getPlatformDelegate() : GetPlatformForAssetBundles(Application.platform);
            var assetBundlePlatform = Path.Combine(AssetBundles, platform);
            assetBundleDataPath = Path.Combine(dataPath, assetBundlePlatform) + Path.DirectorySeparatorChar;
            assetBundleDataPathURL = protocal + assetBundleDataPath;
            updatePath = Path.Combine(Application.persistentDataPath, assetBundlePlatform) + Path.DirectorySeparatorChar;
            OverrideBaseDownloadingUrl += Bundles_overrideBaseDownloadingURL;

            Versions.Load();

            var request = new AssetsInitRequest();
            request.url = "AssetsInitRequest";
            AddAssetRequest(request);
            return request;
        }

        public static SceneAssetRequest LoadSceneAsync(string path, bool additive)
        {
            var asset = new SceneAssetAsyncRequest(path, additive);
            asset.Load();
            asset.Retain();
            _scenes.Add(asset);
            Log(string.Format("LoadScene:{0}", path));
            return asset;
        }

        public static void UnloadScene(SceneAssetRequest scene)
        {
            scene.Release();
        }

        public static AssetRequest LoadAssetAsync(string path, Type type)
        {
            return LoadAsset(path, type, true);
        }

        public static AssetRequest LoadAsset(string path, Type type)
        {
            return LoadAsset(path, type, false);
        }

        public static void UnloadAsset(AssetRequest asset)
        {
            asset.Release();
        }
        
        #endregion

        #region Private

        internal static void Init(Manifest manifest)
        {
            _bundleAssets.Clear(); 
            
            activeVariants = manifest.activeVariants;

            _bundleNames = manifest.bundles;

            var dirs = manifest.dirs;

            for (int i = 0, max = manifest.assets.Length; i < max; i++)
            {
                var item = manifest.assets[i];
                _bundleAssets[string.Format("{0}/{1}", dirs[item.dir], item.name)] = item.bundle;
            }
        }

        internal static Dictionary<string, int> _bundleAssets = new Dictionary<string, int>();

        internal static string[] _bundleNames = new string[0];

        private static readonly Dictionary<string, AssetRequest> _assets = new Dictionary<string, AssetRequest>();

        private static readonly List<AssetRequest> _assetRequests = new List<AssetRequest>();

        private static List<SceneAssetRequest> _scenes = new List<SceneAssetRequest>();

        private static readonly List<AssetRequest> _unusedAssets = new List<AssetRequest>();

        private void Update()
        {
            UpdateAssets();
            UpdateBundles();
        }

        private static void UpdateAssets()
        {
            for (int i = 0; i < _assetRequests.Count; ++i)
            {
                var request = _assetRequests[i];
                if (request.Update() || !request.IsUnused())
                    continue;
                _unusedAssets.Add(request);
                _assetRequests.RemoveAt(i);
                --i;
            }

            for (var i = 0; i < _unusedAssets.Count; ++i)
            {
                var request = _unusedAssets[i];
                _assets.Remove(request.url);
                Log(string.Format("UnloadAsset:{0}", request.url));
                request.Unload();
            }

            _unusedAssets.Clear();

            for (int i = 0; i < _scenes.Count; ++i)
            {
                var request = _scenes[i];
                if (request.Update() || !request.IsUnused())
                {
                    continue;
                }
                _scenes.RemoveAt(i);
                Log(string.Format("UnloadScene:{0}", request.url));
                request.Unload();
                --i;
            }
        }

        private static void AddAssetRequest(AssetRequest request)
        {
            _assets.Add(request.url, request);
            _assetRequests.Add(request);
            request.Load();
        }

        internal static AssetRequest LoadAsset(string path, Type type, bool async)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("invalid path");
                return null;
            }

            AssetRequest request;
            if (_assets.TryGetValue(path, out request))
            {
                request.Retain();
                return request;
            }

            string assetBundleName;
            if (GetAssetBundleName(path, out assetBundleName))
            {
                request = async ? new BundleAssetAsyncRequest(assetBundleName) : new BundleAssetRequest(assetBundleName);
            }
            else
            {
                if (path.StartsWith("http://", StringComparison.Ordinal) ||
                    path.StartsWith("https://", StringComparison.Ordinal) ||
                    path.StartsWith("file://", StringComparison.Ordinal) ||
                    path.StartsWith("ftp://", StringComparison.Ordinal) ||
                    path.StartsWith("jar:file://", StringComparison.Ordinal))
                    request = new WebAssetRequest();
                else
                    request = new AssetRequest();
            }

            request.url = path;
            request.assetType = type;
            AddAssetRequest(request);
            request.Retain();
            Log(string.Format("LoadAsset:{0}", path));
            return request;
        }
        #endregion

        #region Paths

        public static string dataPath { get; set; }

        public static string platform { get; private set; }

        public static string assetBundleDataPath { get; private set; }
        
        public static string assetBundleDataPathURL { get; private set; }

        public static string updatePath { get; private set; }

        private static string GetPlatformForAssetBundles(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return "Windows";
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    return "OSX";
                default:
                    return null;
            }
        }

        public static string GetRelativeUpdatePath(string path)
        {
            return updatePath + path;
        } 
        
        public static string GetAssetBundleDataPathURL(string filename)
        {
            return assetBundleDataPathURL + filename;
        }
        #endregion

        #region Bundles

        private static readonly int MAX_BUNDLE_LOAD_SIZE_PERFREME = 0;

        private static readonly Dictionary<string, BundleRequest> _bundles = new Dictionary<string, BundleRequest>();

        private static readonly List<BundleRequest> _bundleRequests = new List<BundleRequest>();

        private static readonly List<BundleRequest> _unusedBundles = new List<BundleRequest>();

        private static readonly List<BundleRequest> _bundles2Load = new List<BundleRequest>();

        private static readonly List<BundleRequest> _bundlesByLoading = new List<BundleRequest>();

        internal static string[] activeVariants { get; set; }

        internal static AssetBundleManifest bundleManifest { get; set; }

        internal static bool GetAssetBundleName(string path, out string assetBundleName)
        {
            if (path.Equals(Assets.AssetsManifestAsset))
            {
                assetBundleName = Path.GetFileNameWithoutExtension(path).ToLower();
                return true;
            }

            assetBundleName = null;
            int bundle;
            if (!_bundleAssets.TryGetValue(path, out bundle))
                return false;
            assetBundleName = _bundleNames[bundle];
            return true;
        }

        internal static string Bundles_overrideBaseDownloadingURL(string bundleName)
        { 
            if(File.Exists(Assets.GetRelativeUpdatePath(bundleName)))
            {
                return Assets.updatePath;
            }
            return null;
        }

        internal static string[] GetAllDependencies(string bundle)
        {
            if (bundleManifest == null)
            {
                return new string[0];
            }
            return bundleManifest.GetAllDependencies(bundle);
        }

        internal static BundleRequest LoadBundle(string assetBundleName)
        {
            return LoadBundle(assetBundleName, false, false);
        }

        internal static BundleRequest LoadBundleAsync(string assetBundleName)
        {
            return LoadBundle(assetBundleName, false, true);
        }

        internal static void UnloadBundle(BundleRequest bundle)
        {
            bundle.Release();
        }

        internal static void UnloadDependencies(BundleRequest bundle)
        {
            for (var i = 0; i < bundle.dependencies.Count; i++)
            {
                var item = bundle.dependencies[i];
                item.Release();
            }

            bundle.dependencies.Clear();
        }

        internal static void LoadDependencies(BundleRequest bundle, string assetBundleName, bool asyncRequest)
        {
            var dependencies = bundleManifest.GetAllDependencies(assetBundleName);
            if (dependencies.Length <= 0)
                return;
            for (var i = 0; i < dependencies.Length; i++)
            {
                var item = dependencies[i];
                bundle.dependencies.Add(LoadBundle(item, false, asyncRequest));
            }
        }

        internal static BundleRequest LoadBundle(string assetBundleName, bool isLoadingAssetBundleManifest, bool asyncMode)
        {
            if (string.IsNullOrEmpty(assetBundleName))
            {
                Debug.LogError("assetBundleName == null");
                return null;
            }

            if (!isLoadingAssetBundleManifest)
            {
                if (bundleManifest == null)
                {
                    Debug.LogError("Please initialize AssetBundleManifest by calling Bundles.Initialize()");
                    return null;
                }

                assetBundleName = RemapVariantName(assetBundleName);
            }

            var url = GetDataPath(assetBundleName) + assetBundleName;

            BundleRequest bundle;

            if (_bundles.TryGetValue(url, out bundle))
            {
                bundle.Retain();
                return bundle;
            }

            if (url.StartsWith("http://", StringComparison.Ordinal) ||
                url.StartsWith("https://", StringComparison.Ordinal) ||
                url.StartsWith("file://", StringComparison.Ordinal) ||
                url.StartsWith("ftp://", StringComparison.Ordinal))
                bundle = new WebBundleRequest
                {
                    hash = bundleManifest != null ? bundleManifest.GetAssetBundleHash(assetBundleName) : new Hash128(),
                    cache = !isLoadingAssetBundleManifest
                };
            else
                bundle = asyncMode ? new BundleAsyncRequest() : new BundleRequest();

            bundle.url = url;

            _bundles.Add(url, bundle);

            _bundleRequests.Add(bundle);

            if (MAX_BUNDLE_LOAD_SIZE_PERFREME > 0 && (bundle is BundleAsyncRequest || bundle is WebBundleRequest))
            {
                _bundles2Load.Add(bundle);
            }
            else
            {
                bundle.Load();
                Log("LoadBundle: " + url);
            }

            if (!isLoadingAssetBundleManifest)
                LoadDependencies(bundle, assetBundleName, asyncMode);

            bundle.Retain();
            return bundle;
        }

        internal static string GetDataPath(string bundleName)
        {
            if (OverrideBaseDownloadingUrl == null)
                return Assets.assetBundleDataPath;
            foreach (var @delegate in OverrideBaseDownloadingUrl.GetInvocationList())
            {
                var method = (OverrideDataPathDelegate)@delegate;
                var res = method(bundleName);
                if (res != null)
                    return res;
            }

            return Assets.assetBundleDataPath;
        }

        internal static void UpdateBundles()
        {
            if (MAX_BUNDLE_LOAD_SIZE_PERFREME > 0)
            {
                if (_bundles2Load.Count > 0 && _bundlesByLoading.Count < MAX_BUNDLE_LOAD_SIZE_PERFREME)
                {
                    for (int i = 0; i < Math.Min(MAX_BUNDLE_LOAD_SIZE_PERFREME - _bundlesByLoading.Count, _bundles2Load.Count); ++i)
                    {
                        var item = _bundles2Load[i];
                        if (item.loadState == LoadState.Init)
                        {
                            item.Load();
                            _bundlesByLoading.Add(item);
                            _bundles2Load.RemoveAt(i);
                            --i;
                        }
                    }
                }

                for (int i = 0; i < _bundlesByLoading.Count; ++i)
                {
                    var item = _bundlesByLoading[i];
                    if (item.loadState == LoadState.Loaded || item.loadState == LoadState.Unload)
                    {
                        _bundlesByLoading.RemoveAt(i);
                        --i;
                    }
                }
            }

            for (int i = 0; i < _bundleRequests.Count; i++)
            {
                BundleRequest item = _bundleRequests[i];
                if (item.Update() || !item.IsUnused())
                    continue;
                _unusedBundles.Add(item);
                _bundleRequests.RemoveAt(i);
                --i;
            }

            for (var i = 0; i < _unusedBundles.Count; i++)
            {
                var item = _unusedBundles[i];
                _bundles.Remove(item.url);
                UnloadDependencies(item);
                item.Unload();
                Log("UnloadBundle: " + item.url);
            }

            _unusedBundles.Clear();
        }

        internal static string RemapVariantName(string assetBundleName)
        {
            var bundlesWithVariant = bundleManifest.GetAllAssetBundlesWithVariant();

            // Get base bundle name
            var baseName = assetBundleName.Split('.')[0];

            var bestFit = int.MaxValue;
            var bestFitIndex = -1;
            // Loop all the assetBundles with variant to find the best fit variant assetBundle.
            for (var i = 0; i < bundlesWithVariant.Length; i++)
            {
                var curSplit = bundlesWithVariant[i].Split('.');
                var curBaseName = curSplit[0];
                var curVariant = curSplit[1];

                if (curBaseName != baseName)
                    continue;

                var found = Array.IndexOf(activeVariants, curVariant);

                // If there is no active variant found. We still want to use the first
                if (found == -1)
                    found = int.MaxValue - 1;

                if (found >= bestFit)
                    continue;
                bestFit = found;
                bestFitIndex = i;
            }

            if (bestFit == int.MaxValue - 1)
                Debug.LogWarning(
                    "Ambiguous asset bundle variant chosen because there was no matching active variant: " +
                    bundlesWithVariant[bestFitIndex]);

            return bestFitIndex != -1 ? bundlesWithVariant[bestFitIndex] : assetBundleName;
        }
        #endregion
    }
}