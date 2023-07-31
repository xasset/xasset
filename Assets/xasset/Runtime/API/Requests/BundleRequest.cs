using System;
using System.Collections.Generic;
using UnityEngine;

namespace xasset
{
    public sealed class BundleRequest : LoadRequest, IReloadable
    {
        private IBundleHandler handler;
        internal AssetBundle assetBundle { get; private set; }
        public ManifestBundle info { get; private set; }

        public override int priority => 0;

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public static Func<BundleRequest, IBundleHandler> CreateHandler { get; set; }

        public void ReloadAsync()
        {
            status = Status.Processing;
            handler = GetHandler(this);
            handler.OnStart(this);
        }

        public void OnReloaded()
        {
        }

        public bool IsReloaded()
        {
            OnUpdated();
            return isDone;
        }

        protected override void OnStart()
        {
            handler.OnStart(this);
        }

        protected override void OnUpdated()
        {
            handler.Update(this);
        }

        protected override void OnWaitForCompletion()
        {
            handler.WaitForCompletion(this);
        } 

        public void LoadAssetBundle(string filename)
        {
            ReloadAssetBundle(info.name);
            assetBundle = AssetBundle.LoadFromFile(filename);
            progress = 1;
            if (assetBundle == null)
            {
                SetResult(Result.Failed, $"assetBundle == null, {info.file}");
                return;
            }

            SetResult(Result.Success);
            AddAssetBundle(info.name, assetBundle);
        }

        protected override void OnDispose()
        {
            Remove(this);
            handler.Dispose(this);
            if (assetBundle == null) return;
            assetBundle.Unload(true);
            RemoveAssetBundle(info.name);
            assetBundle = null;
        }

        public void Reload(ManifestBundle bundle)
        {
            Reset();
            info = bundle;
            path = bundle.file;
            handler = GetHandler(this);
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

        private static IBundleHandler GetHandler(BundleRequest request)
        {
            var bundle = request.info;
            var handler = CreateHandler?.Invoke(request);
            if (handler != null) return handler;

            if (Assets.IsDownloaded(bundle, false))
                return new RuntimeLocalBundleHandler { path = Assets.GetDownloadDataPath(bundle.file) }; 
            if (Assets.IsPlayerAsset(bundle.hash))
                return new RuntimeLocalBundleHandler { path = Assets.GetPlayerDataPath( bundle.file) };
            return new RuntimeDownloadBundleHandler();
        }

        private static void Remove(BundleRequest request)
        {
            Loaded.Remove(request.info.name);
            Unused.Enqueue(request);
        }

        internal static BundleRequest Load(ManifestBundle bundle)
        {
            if (!Loaded.TryGetValue(bundle.name, out var request))
            {
                request = Unused.Count > 0 ? Unused.Dequeue() : new BundleRequest();
                request.Reload(bundle);
                Loaded[bundle.name] = request;
            }

            request.LoadAsync();
            return request;
        }

        #endregion
    }
}