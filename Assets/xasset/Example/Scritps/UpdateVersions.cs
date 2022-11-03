using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace xasset.example
{
    public class UpdateVersions : MonoBehaviour
    {
        public Text version;
        public UnityEvent completed;
        private DownloadRequest _downloadRequest;
        private Versions versions;

        private void Start()
        {
            UpdateVersion();
            StartCoroutine(Updating());
        }

        public void ClearAsync()
        {
            var dir = Assets.DownloadDataPath;
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }

        private void UpdateVersion()
        {
            if (version == null) return;
            version.text = string.Format(Constants.Text.Version, Assets.Versions);
        }

        private IEnumerator Updating()
        {
            LoadingScreen.Instance.SetVisible(true);
            var getUpdateInfoAsync = Assets.GetUpdateInfoAsync();
            yield return getUpdateInfoAsync;
            if (getUpdateInfoAsync.result == Request.Result.Success)
            {
                var getVersionsAsync = Assets.GetVersionsAsync(getUpdateInfoAsync.info);
                while (!getVersionsAsync.isDone)
                {
                    var msg = $"{Constants.Text.UpdateVersions} {getVersionsAsync.progress * 100:f2}%";
                    LoadingScreen.Instance.SetProgress(msg, getVersionsAsync.progress);
                    yield return null;
                }

                versions = getVersionsAsync.versions;
                if (versions != null)
                {
                    var getDownloadSizeAsync = Assets.GetDownloadSizeAsync(versions);
                    yield return getDownloadSizeAsync;
                    if (getDownloadSizeAsync.downloadSize > 0)
                    {
                        var downloadSize = Utility.FormatBytes(getDownloadSizeAsync.downloadSize);
                        var message = string.Format(Constants.Text.TipsNewContent, downloadSize);
                        var update = MessageBox.Show(Constants.Text.Tips, message);
                        yield return update;
                        if (update.result == Request.Result.Success)
                        {
                            _downloadRequest = getDownloadSizeAsync.DownloadAsync();
                            yield return Downloading();
                            if (_downloadRequest.result == DownloadRequest.Result.Success)
                            {
                                yield return Clearing();
                                Assets.Versions = versions;
                                versions.Save(Assets.GetDownloadDataPath(Versions.Filename));
                                UpdateVersion();
                            }
                        }
                    }
                }
            }

            LoadingScreen.Instance.SetVisible(false);
            completed?.Invoke();
        }

        private IEnumerator Downloading()
        {
            while (_downloadRequest.result != DownloadRequest.Result.Success)
            {
                var downloadedBytes = Utility.FormatBytes(_downloadRequest.downloadedBytes);
                var downloadSize = Utility.FormatBytes(_downloadRequest.downloadSize);
                var bandwidth = Utility.FormatBytes(_downloadRequest.bandwidth);
                var msg = $"{Constants.Text.Loading}{downloadedBytes}/{downloadSize}, {bandwidth}/s";
                LoadingScreen.Instance.SetProgress(msg, _downloadRequest.progress);
                yield return null;
                if (!_downloadRequest.isDone || string.IsNullOrEmpty(_downloadRequest.error)) continue;
                var retry = MessageBox.Show(Constants.Text.Tips, Constants.Text.TipsDownloadFailed);
                yield return retry;
                if (retry.result == Request.Result.Success) _downloadRequest.Retry();
                else break;
            }
        }

        private IEnumerator Clearing()
        {
            var bundles = new HashSet<string>();
            foreach (var item in versions.data)
            {
                bundles.Add(item.file);
                foreach (var bundle in item.manifest.bundles)
                    bundles.Add(bundle.nameWithAppendHash);
            }

            var files = new List<string>();
            foreach (var item in Assets.Versions.data)
            {
                if (!bundles.Contains(item.file))
                    files.Add(item.file);
                foreach (var bundle in item.manifest.bundles)
                    if (!bundles.Contains(bundle.nameWithAppendHash))
                        files.Add(item.file);
            }

            var removeAsync = new RemoveRequest();
            foreach (var file in files)
            {
                var path = Assets.GetDownloadDataPath(file);
                removeAsync.files.Add(path);
            }

            removeAsync.SendRequest();
            while (!removeAsync.isDone)
            {
                var msg = $"清理历史文件 {removeAsync.current}/{removeAsync.max}";
                LoadingScreen.Instance.SetProgress(msg, removeAsync.progress);
                yield return null;
            }
        }
    }
}