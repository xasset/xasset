using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace xasset.samples
{
    public class CheckForUpdates : MonoBehaviour
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
            version.text = $"{Constants.Text.Version}{Assets.Versions}";
        }
        
        void Quit()
        { 
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
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
                var updateVersion = System.Version.Parse(getUpdateInfoAsync.info.version);
                var playerVersion = System.Version.Parse(Assets.PlayerAssets.version);
                if (updateVersion.Minor != playerVersion.Minor) // 需要强更下载安装包。
                {
                    var request = MessageBox.Show(Constants.Text.Tips, string.Format(Constants.Text.TipsNewContent, updateVersion));
                    yield return request;
                    if (request.result == Request.Result.Success)
                        Application.OpenURL(getUpdateInfoAsync.info.playerDownloadURL);  
                    Quit(); 
                    yield break;
                } 
                
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
                    var request = Assets.GetDownloadSizeAsync(versions);
                    LoadingScreen.Instance.SetVisible(true);
                    while (!request.isDone)
                    {
                        LoadingScreen.Instance.SetProgress($"{Constants.Text.Checking}({request.progress*100}%)", request.progress);
                        yield return null;
                    }
                    if (request.downloadSize > 0)
                    {
                        var downloadSize = Utility.FormatBytes(request.downloadSize);
                        var message = string.Format(Constants.Text.TipsNewContent, downloadSize);
                        var update = MessageBox.Show(Constants.Text.Tips, message);
                        yield return update;
                        if (update.result == Request.Result.Success)
                        {
                            _downloadAsync = request.DownloadAsync();
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