using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace xasset.samples
{
    public class Download : MonoBehaviour
    {
        public InputField inputField;
        public LoadingBar loadingBar;
        private DownloadRequest _request;


        private void OnDestroy()
        {
            _request?.Cancel();
        }

        public void Run()
        {
            StartCoroutine(Loading());
        }

        private IEnumerator Loading()
        {
            var url = inputField.text;

            Logger.D($"Download {url}");
            var content = DownloadContent.Get(url, Assets.GetDownloadDataPath(Path.GetFileName(url)));
            _request = Downloader.DownloadAsync(content);
            loadingBar.SetVisible(true);
            while (!_request.isDone)
            {
                var downloadedBytes = Utility.FormatBytes(_request.downloadedBytes);
                var downloadSize = Utility.FormatBytes(_request.downloadSize);
                var bandwidth = Utility.FormatBytes(_request.bandwidth);

                var time = (_request.downloadSize - _request.downloadedBytes) * 1f / _request.bandwidth;
                var msg = $"{Constants.Text.Loading}{downloadedBytes}/{downloadSize}, {bandwidth}/s - 剩余：{time}s";
                loadingBar.SetProgress(msg, _request.progress);
                yield return null;
            }

            loadingBar.SetVisible(false);
        }

        public void Retry()
        {
            _request?.Retry();
        }

        public void Pause()
        {
            _request?.Pause();
        }

        public void UnPause()
        {
            _request?.UnPause();
        }

        public void Cancel()
        {
            _request?.Cancel();
        }

        public void Clear()
        {
            _request?.Clear();
        }
    }
}