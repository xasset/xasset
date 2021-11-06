using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace VEngine.Example
{
    public class WelcomeScreen : MonoBehaviour
    {
        public Button buttonStart;
        public Text version;
        private UpdateVersions updateAsync;

        private void Start()
        {
            buttonStart.gameObject.SetActive(true);
            version.text = Versions.ManifestsVersion;
        }

        private void OnDestroy()
        {
            ClearUpdate();
            MessageBox.CloseAll();
        }

        private void ClearUpdate()
        {
            if (updateAsync != null)
            {
                updateAsync.Dispose();
                updateAsync = null;
            }
        }

        public void Clear()
        {
            MessageBox.Show("提示", "保留历史版本数据可以获得更快的更新体验，请确认是否清理？", ok =>
            {
                if (ok) Versions.ClearAsync();
            }, "清理", "退出");
        }

        public void StartUpdate()
        {
            StartCoroutine(Checking());
        }

        private void GetDownloadSizeAsync()
        {
            StartCoroutine(GetDownloadSize());
        }

        private IEnumerator GetDownloadSize()
        {
            var getDownloadSize = Versions.GetDownloadSizeAsync(updateAsync);
            yield return getDownloadSize;
            if (getDownloadSize.totalSize > 0 || updateAsync.changed)
            {
                var messageBox = MessageBox.Show("提示",
                    $"发现更新({Utility.FormatBytes(getDownloadSize.totalSize)})：服务器版本号 {updateAsync.version}，本地版本号 {Versions.ManifestsVersion}，是否更新？",
                    null, "更新", "跳过");
                yield return messageBox;
                if (messageBox.ok)
                {
                    updateAsync.Override();
                    StartDownload(getDownloadSize);
                    yield break;
                }
            }

            OnComplete();
        }

        private void StartDownload(GetDownloadSize getDownloadSize)
        {
            StartCoroutine(Downloading(getDownloadSize));
        }

        private IEnumerator Downloading(GetDownloadSize getDownloadSize)
        {
            var downloadAsync = Versions.DownloadAsync(getDownloadSize.result.ToArray());
            yield return downloadAsync;
            if (downloadAsync.status == OperationStatus.Failed)
            {
                var messageBox2 = MessageBox.Show("提示！", "下载失败！请检查网络状态后重试。", null);
                yield return messageBox2;
                if (messageBox2.ok)
                    StartDownload(getDownloadSize);
                else
                    OnComplete();

                yield break;
            }

            OnComplete();
        }

        public IEnumerator Checking()
        {
            buttonStart.gameObject.SetActive(false);
            PreloadManager.Instance.SetVisible(true);
            PreloadManager.Instance.SetMessage("获取版本信息...", 0);
            ClearUpdate();
            // TODO：生产环境这里的清单名字应该使用带 hash 的版本
            updateAsync = Versions.UpdateAsync(nameof(Manifest));
            yield return updateAsync;
            if (updateAsync.status == OperationStatus.Failed)
            {
                yield return MessageBox.Show("提示", "更新版本信息失败，请检测网络链接后重试。", ok =>
                {
                    if (ok)
                        StartUpdate();
                    else
                        OnComplete();
                }, "重试", "跳过");
                yield break;
            }

            PreloadManager.Instance.SetMessage("获取版本信息...", 1);
            GetDownloadSizeAsync();
        }

        private void OnComplete()
        {
            version.text = Versions.ManifestsVersion;
            PreloadManager.Instance.SetMessage("更新完成", 1);
            PreloadManager.Instance.StartCoroutine(LoadScene());
        }

        private static IEnumerator LoadScene()
        {
            var scene = Scene.LoadAsync(Res.GetScene("Menu"));
            PreloadManager.Instance.ShowProgress(scene);
            yield return scene;
        }
    }
}