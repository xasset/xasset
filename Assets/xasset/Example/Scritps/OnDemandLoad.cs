using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace xasset.example
{
    /// <summary>
    ///     按需加载功能示例
    /// </summary>
    public class OnDemandLoad : MonoBehaviour
    {
        public ExampleScene scene;
        public Text pauseUnPause;
        public LoadingBar loadingBar;
        private DownloadRequest _downloadAsync;

        private void Start()
        {
            pauseUnPause.text = Constants.Text.Pause;
            pauseUnPause.enabled = false;
        }

        public void Run()
        {
            StartCoroutine(CheckForUpdates());
        }

        private IEnumerator CheckForUpdates()
        {
            var request = Assets.GetDownloadSizeAsync(Assets.Versions, scene.ToString());
            yield return request;
            if (request.downloadSize > 0)
            {
                var downloadSize = Utility.FormatBytes(request.downloadSize);
                var message = string.Format(Constants.Text.TipsNewContent, downloadSize);
                var downloadNow = MessageBox.Show(Constants.Text.Tips, message);
                yield return downloadNow;
                if (downloadNow.result == Request.Result.Success)
                    yield return Downloading(request);
                else
                    Scene.LoadAsync(scene.ToString());
            }
            else
            {
                Scene.LoadAsync(scene.ToString());
            }
        }

        private IEnumerator Downloading(GetDownloadSizeRequest request)
        {
            _downloadAsync = request.DownloadAsync();
            pauseUnPause.enabled = true;
            loadingBar.SetVisible(true);
            while (_downloadAsync.result != DownloadRequest.Result.Success)
            {
                var downloadedBytes = Utility.FormatBytes(_downloadAsync.downloadedBytes);
                var downloadSize = Utility.FormatBytes(_downloadAsync.downloadSize);
                var bandwidth = Utility.FormatBytes(_downloadAsync.bandwidth);
                var msg = $"{Constants.Text.Loading}{downloadedBytes}/{downloadSize}, {bandwidth}/s";
                loadingBar.SetProgress(msg, _downloadAsync.progress);
                yield return null;
                if (!_downloadAsync.isDone || string.IsNullOrEmpty(_downloadAsync.error)) continue;
                var retry = MessageBox.Show(Constants.Text.Tips, Constants.Text.TipsDownloadFailed);
                yield return retry;
                if (retry.result == Request.Result.Success) _downloadAsync.Retry();
                else break;
            }

            loadingBar.SetProgress($"{Constants.Text.LoadComplete}{Utility.FormatBytes(request.downloadSize)}", 1);
            pauseUnPause.enabled = false;
        }

        public void ClearAssets()
        {
            StartCoroutine(Clearing());
        }

        private IEnumerator Clearing()
        {
            loadingBar.SetVisible(true);
            var request = Assets.RemoveAsync(scene.ToString());
            while (!request.isDone)
            {
                var msg = $"{Constants.Text.Clearing}{request.max - request.files.Count}/{request.max}";
                loadingBar.SetProgress(msg, request.progress);
                yield return null;
            }

            loadingBar.SetVisible(false);
        }

        public void SwitchPause()
        {
            if (_downloadAsync == null) return;

            if (_downloadAsync.status == DownloadRequest.Status.Paused)
            {
                pauseUnPause.text = Constants.Text.Pause;
                _downloadAsync.UnPause();
            }
            else
            {
                pauseUnPause.text = Constants.Text.UnPause;
                _downloadAsync.Pause();
            }
        }
    }
}