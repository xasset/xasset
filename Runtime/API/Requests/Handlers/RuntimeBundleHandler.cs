using UnityEngine;

namespace xasset
{
    internal struct RuntimeLocalBundleHandler : IBundleHandler
    {
        public string path;

        public void OnStart(BundleRequest request)
        {
            request.LoadAssetBundle(path);
        }

        public void Update(BundleRequest request)
        {
        }

        public void Dispose(BundleRequest request)
        {
        }

        public void WaitForCompletion(BundleRequest request)
        {
        }
    } 
    internal struct RuntimeDownloadBundleHandler : IBundleHandler
    {
        private DownloadRequest _downloadAsync;
        private string _savePath;
        private int _retryTimes;

        public void OnStart(BundleRequest request)
        {
            _retryTimes = 0;
            var bundle = request.info;
            var url = Assets.GetDownloadURL(bundle.file);
            _savePath = Assets.GetDownloadDataPath(bundle.file);
            _downloadAsync = Downloader.DownloadAsync(DownloadContent.Get(url, _savePath, bundle.hash, bundle.size));
        }

        public void Update(BundleRequest request)
        {
            request.progress = _downloadAsync.progress;
            if (!_downloadAsync.isDone)
                return;

            if (_downloadAsync.result == DownloadRequestBase.Result.Success)
            {
                request.LoadAssetBundle(_savePath);
                return;
            }

            // 网络可达才自动 Retry
            if (Application.internetReachability != NetworkReachability.NotReachable
                && _retryTimes < Assets.MaxRetryTimes)
            {
                _downloadAsync.Retry();
                _retryTimes++;
                return;
            }

            // 网络不可达的时候，如果是异步加载，可以提示用户检查网络链接。

            request.SetResult(Request.Result.Failed, _downloadAsync.error);
        }


        public void Dispose(BundleRequest request)
        {
        }

        public void WaitForCompletion(BundleRequest request)
        {
            _downloadAsync.WaitForCompletion();
            while (!request.isDone) Update(request);
        }
    }

    public interface IBundleHandler
    {
        void OnStart(BundleRequest request);
        void Update(BundleRequest request);
        void Dispose(BundleRequest request);
        void WaitForCompletion(BundleRequest request);
    }
}