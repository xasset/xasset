using System;
using System.Collections.Generic;
using UnityEngine;

namespace xasset
{
    [Serializable]
    public sealed class BundleRequest : LoadRequest
    {
        internal BundleRequestHandler handler;
        internal AssetBundle assetBundle { get; set; }
        public ManifestBundle info { get; set; }

        protected override void OnStart()
        {
            handler.OnStart();
        }

        protected override void OnUpdated()
        {
            handler.Update();
        }

        protected override void OnWaitForCompletion()
        {
            handler.WaitForCompletion();
        }

        public void LoadAssetBundle(string filename, ulong offset = 0)
        {
            Logger.D($"Load {info.nameWithAppendHash} from {filename} with offset {offset}");
            ReloadAssetBundle(info.name);
            assetBundle = AssetBundle.LoadFromFile(filename, 0, offset);
            progress = 1;
            if (assetBundle == null)
            {
                SetResult(Result.Failed, $"assetBundle == null, {info.nameWithAppendHash}");
                return;
            }

            SetResult(Result.Success);
            AddAssetBundle(info.name, assetBundle);
        }

        protected override void OnDispose()
        {
            Remove(this);
            handler.Dispose();
            if (assetBundle != null)
            {
                assetBundle.Unload(true);
                RemoveAssetBundle(info.name);
                assetBundle = null;
            }

            Reset();
        }

        #region Hotreload

        private static readonly Dictionary<string, AssetBundle> AssetBundles = new Dictionary<string, AssetBundle>();

        private static void AddAssetBundle(string name, AssetBundle assetBundle)
        {
            AssetBundles[name] = assetBundle;
        }

        private static void ReloadAssetBundle(string name)
        {
            if (!AssetBundles.TryGetValue(name, out var assetBundle)) return;
            if (assetBundle != null) assetBundle.Unload(false);
            AssetBundles.Remove(name);
        }

        private static void RemoveAssetBundle(string name)
        {
            AssetBundles.Remove(name);
        }

        #endregion

        #region Internal

        private static readonly Queue<BundleRequest> Unused = new Queue<BundleRequest>();
        public static readonly Dictionary<string, BundleRequest> Loaded = new Dictionary<string, BundleRequest>();

        private static BundleRequestHandler GetHandler(BundleRequest request)
        {
            if (Assets.IsWebGLPlatform)
                throw new NotImplementedException("开源版不支持 WebGL");

            var bundle = request.info;
            if (Assets.IsPlayerAsset(bundle.hash))
                return new BundleRequestHandlerFile {path = Assets.GetPlayerDataPath(bundle.nameWithAppendHash), request = request};

            if (Assets.IsDownloaded(bundle))
                return new BundleRequestHandlerFile {path = Assets.GetDownloadDataPath(bundle.nameWithAppendHash), request = request};

            throw new NotImplementedException("开源版不支持直接从服务器下载");
        }

        private static void Remove(BundleRequest request)
        {
            Loaded.Remove(request.info.nameWithAppendHash);
            Unused.Enqueue(request);
        }

        internal static BundleRequest Load(ManifestBundle bundle)
        {
            if (!Loaded.TryGetValue(bundle.nameWithAppendHash, out var request))
            {
                request = Unused.Count > 0 ? Unused.Dequeue() : new BundleRequest();
                request.Reset();
                request.info = bundle;
                request.path = bundle.nameWithAppendHash;
                request.handler = GetHandler(request);
                Loaded[bundle.nameWithAppendHash] = request;
            }

            request.LoadAsync();
            return request;
        }

        #endregion
    }
}