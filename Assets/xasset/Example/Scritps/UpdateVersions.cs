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
        private DownloadRequestBase _downloadAsync;
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
            // 预加载
            yield return Asset.LoadAsync(MessageBox.Filename, typeof(GameObject));
            yield return Asset.InstantiateAsync(LoadingScreen.Filename);
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
                            _downloadAsync = getDownloadSizeAsync.DownloadAsync();
                            yield return Downloading();
                            if (_downloadAsync.result == DownloadRequestBase.Result.Success)
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
            while (_downloadAsync.result != DownloadRequestBase.Result.Success)
            {
                var downloadedBytes = Utility.FormatBytes(_downloadAsync.downloadedBytes);
                var downloadSize = Utility.FormatBytes(_downloadAsync.downloadSize);
                var bandwidth = Utility.FormatBytes(_downloadAsync.bandwidth);
                var msg = $"{Constants.Text.Loading}{downloadedBytes}/{downloadSize}, {bandwidth}/s";
                LoadingScreen.Instance.SetProgress(msg, _downloadAsync.progress);
                yield return null;
                if (!_downloadAsync.isDone || string.IsNullOrEmpty(_downloadAsync.error)) continue;
                var retry = MessageBox.Show(Constants.Text.Tips, Constants.Text.TipsDownloadFailed);
                yield return retry;
                if (retry.result == Request.Result.Success) _downloadAsync.Retry();
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
                    bundles.Add(bundle.file);
            }

            var files = new List<string>();
            foreach (var item in Assets.Versions.data)
            {
                if (!bundles.Contains(item.file))
                    files.Add(item.file);
                foreach (var bundle in item.manifest.bundles)
                    if (!bundles.Contains(bundle.file))
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