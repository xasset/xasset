using UnityEngine;

namespace VEngine
{
    internal class DownloadBundle : Bundle
    {
        private Download download;
        private AssetBundleCreateRequest request;

        protected override void OnLoad()
        {
            download = Download.DownloadAsync(pathOrURL, Versions.GetDownloadDataPath(info.nameWithAppendHash), null,
                info.size,
                info.crc);
            download.completed += OnDownloaded;
        }

        private void OnDownloaded(Download obj)
        {
            if (download.status == DownloadStatus.Failed)
            {
                Finish(download.error);
                return;
            }

            if (assetBundle != null) return;

            request = AssetBundle.LoadFromFileAsync(obj.info.savePath);
            Versions.SetBundlePathOrURl(info.nameWithAppendHash, obj.info.savePath);
        }

        protected override void OnUpdate()
        {
            if (status != LoadableStatus.Loading) return;

            if (!download.isDone)
            {
                progress = download.downloadedBytes * 1f / download.info.size * 0.5f;
                return;
            }

            if (request == null) return;

            progress = 0.5f + request.progress;
            if (!request.isDone) return;

            assetBundle = request.assetBundle;
            Finish(assetBundle == null ? "assetBundle == null" : null);
            request = null;
        }
    }
}