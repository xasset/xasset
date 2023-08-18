using UnityEngine;

namespace xasset
{
    public interface IFileHandler
    {
        void OnStart(FileRequest request);
        void Update(FileRequest request);
        void Dispose(FileRequest request);
        void WaitForCompletion(FileRequest request);
        void OnReload(FileRequest request);
    }

    public class RuntimeFileHandler : IFileHandler
    {
        private AssetBundleRequest _loadAssetAsync;

        private BundleRequest _bundleRequest;

        public static readonly string dirPath = "Assets/Addressable/SourceRes";

        public void OnStart(FileRequest request)
        {
            var bundles = request.info.manifest.bundles;
            var bundleManifest = bundles[request.info.bundle];
            _bundleRequest = BundleRequest.Load(bundleManifest);
        }

        public void Update(FileRequest request)
        {
            if (request.isDone) return;
            request.progress = 0.5f * _loadAssetAsync.progress * 0.5f;
            if (!_loadAssetAsync.isDone) return;
            SetResult(request);
        }

        private void LoadAsset(FileRequest request)
        {
            TextAsset textAsset = _bundleRequest.assetBundle.LoadAsset<TextAsset>(request.path);
            request.asset = Files.GetFileAsset(request, textAsset);
            if (request.asset == null)
            {
                request.SetResult(Request.Result.Failed, $"{request.path} get asset failed");
                return;
            }

            request.SetResult(Request.Result.Success);
        }

        private void LoadAssetAsync(FileRequest request)
        {
            _bundleRequest.assetBundle.LoadAssetAsync(request.path, typeof(TextAsset));
        }

        private void SetResult(FileRequest request)
        {
            TextAsset textAsset = (TextAsset)_loadAssetAsync.asset;
            if (textAsset != null) request.asset = Files.GetFileAsset(request, textAsset);
            if (request.asset == null)
            {
                request.SetResult(Request.Result.Failed, "asset == null");
                return;
            }

            request.SetResult(Request.Result.Success);
        }

        public void Dispose(FileRequest request)
        {
            _loadAssetAsync = null;
        }

        public void WaitForCompletion(FileRequest request)
        {
            _bundleRequest.WaitForCompletion();
            if (request.result == Request.Result.Failed) return;
            if (_loadAssetAsync == null)
                LoadAsset(request);
            else
                SetResult(request);
        }

        public void OnReload(FileRequest request)
        {
            LoadAssetAsync(request);
        }

        public static IFileHandler CreateInstance()
        {
            return new RuntimeFileHandler();
        }
    }
}