using UnityEngine;

namespace VEngine.Example
{
    public interface IProgressBar
    {
        void SetVisible(bool visible);
        void SetProgress(float progress);
        void SetMessage(string message);
    }

    [DisallowMultipleComponent]
    public class PreloadManager : MonoBehaviour
    {
        public readonly AssetManager assetManager = new AssetManager();

        public IProgressBar progressBar;

        public static PreloadManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            MessageBox.Dispose();
            assetManager.Clear();
        }

        public void SetVisible(bool visible)
        {
            if (progressBar != null) progressBar.SetVisible(visible);
        }

        public void SetMessage(string message, float progress)
        {
            SetVisible(true);
            if (progressBar != null)
            {
                progressBar.SetMessage(message);
                progressBar.SetProgress(progress);
            }
        }

        private static string FormatBytes(long bytes)
        {
            return Utility.FormatBytes(bytes);
        }

        public void ShowProgress(Scene loading)
        {
            SetVisible(true);
            loading.completed += scene => { SetVisible(false); };
            loading.updated += scene =>
            {
                if (Download.Working)
                {
                    var current = Download.TotalDownloadedBytes;
                    var max = Download.TotalSize;
                    var speed = Download.TotalBandwidth;
                    SetMessage($"资源下载中...{FormatBytes(current)}/{FormatBytes(max)}(速度 {FormatBytes(speed)}/s)",
                        current * 1f / max);
                }
                else
                {
                    SetMessage($"加载游戏场景... {scene.progress * 100:F2}%", scene.progress);
                }
            };
        }

        public void ShowProgress(Download downloading)
        {
            SetVisible(true);
            downloading.completed += scene => { SetVisible(false); };
            downloading.updated += scene =>
            {
                var current = Download.TotalDownloadedBytes;
                var max = Download.TotalSize;
                var speed = Download.TotalBandwidth;
                SetMessage(
                    $"资源下载中...{FormatBytes(current)}/{FormatBytes(max)}(速度 {FormatBytes(speed)}/s)",
                    current * 1f / max);
            };
        }
    }
}