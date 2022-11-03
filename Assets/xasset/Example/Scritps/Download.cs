using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace xasset.example
{
    public class Download : MonoBehaviour
    {
        public InputField inputField;
        public LoadingBar loadingBar;
        private DownloadContentRequest _request;


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

            Downloader.SimulationMode = false;

            Logger.D($"Download {url}");
            var content = DownloadContent.Get(url, Assets.GetDownloadDataPath(Path.GetFileName(url)));
            _request = Downloader.DownloadAsync(content);
            loadingBar.SetVisible(true);

            var startTime = Time.realtimeSinceStartup - 1;
            while (!_request.isDone)
            {
                if (Time.realtimeSinceStartup - startTime >= 1)
                {
                    var downloadedBytes = Utility.FormatBytes(_request.downloadedBytes);
                    var downloadSize = Utility.FormatBytes(_request.downloadSize);
                    var bandwidth = Utility.FormatBytes(_request.bandwidth);
                    var remainBytes = _request.downloadSize - _request.downloadedBytes;
                    var time = remainBytes * 1f / _request.bandwidth;
                    var msg = $"{Constants.Text.Loading}{downloadedBytes}/{downloadSize}, {bandwidth}/s - 剩余：{time:F1}s";
                    loadingBar.SetProgress(msg, _request.progress);
                    startTime = Time.realtimeSinceStartup;
                }
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