using System.Collections.Generic;
using UnityEngine;

namespace xasset
{
    internal class Dependencies : IReloadable
    {
        internal static readonly Dictionary<string, Dependencies> Loaded = new Dictionary<string, Dependencies>();
        private static readonly Queue<Dependencies> Unused = new Queue<Dependencies>();
        private readonly List<BundleRequest> _bundles = new List<BundleRequest>();
        private readonly List<BundleRequest> _loading = new List<BundleRequest>();
        public ManifestAsset asset { get; set; }
        private BundleRequest _bundleRequest;
        private int _refCount;
        public bool isDone => _loading.Count == 0;
        public float progress => (_bundles.Count - _loading.Count) * 1f / _bundles.Count;

        private BundleRequest Load(ManifestBundle bundle)
        {
            var request = BundleRequest.Load(bundle);
            _bundles.Add(request);
            _loading.Add(request);
            return request;
        }

        private void LoadAsync()
        {
            if (_refCount == 0)
            {
                LoadAll();
            }

            _refCount++;
        }

        private void LoadAll()
        {
            var bundles = asset.manifest.bundles;
            var bundle = bundles[asset.bundle];
            _bundleRequest = Load(bundle);
            foreach (var dep in bundle.deps)
                Load(bundles[dep]);
        }

        public bool CheckResult(LoadRequest request, out AssetBundle assetBundle)
        {
            assetBundle = null;
            foreach (var bundle in _bundles)
            {
                if (bundle.result != Request.Result.Failed) continue;
                request.SetResult(Request.Result.Failed, bundle.error);
                return false;
            }

            assetBundle = _bundleRequest.assetBundle;
            if (assetBundle != null) return true;
            request.SetResult(Request.Result.Failed, "assetBundle == null");
            return false;
        }

        public void WaitForCompletion()
        {
            for (var index = 0; index < _loading.Count; index++)
            {
                var request = _loading[index];
                request.WaitForCompletion();
                _loading.RemoveAt(index);
                index--;
            }
        }

        public void Release()
        {
            _refCount--;
            if (_refCount != 0) return;

            ClearAll();

            Loaded.Remove(asset.path);
            Unused.Enqueue(this);
            _bundleRequest = null;
        }

        private void ClearAll()
        {
            foreach (var request in _bundles) request.Release();

            _bundles.Clear();
        }

        public static Dependencies LoadAsync(ManifestAsset asset)
        {
            if (!Loaded.TryGetValue(asset.path, out var value))
            {
                value = Unused.Count > 0 ? Unused.Dequeue() : new Dependencies();
                value.asset = asset;
                Loaded[asset.path] = value;
            }

            value.LoadAsync();
            return value;
        }

        public void Update()
        {
            if (isDone) return;
            for (var index = 0; index < _loading.Count; index++)
            {
                var request = _loading[index];
                if (!request.isDone) continue;
                _loading.RemoveAt(index);
                index--;
            }
        }

        public void ReloadAsync()
        {
            ClearAll();
            LoadAll();
        }

        public void OnReloaded()
        {
        }

        public bool IsReloaded()
        {
            Update();
            return isDone;
        }
    }
}