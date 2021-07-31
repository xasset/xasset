using System;
using System.IO;
using UnityEngine;

namespace Versions.Example
{
    public class DownloadAndUnpack : MonoBehaviour
    {
        public long[] speeds =
        {
            0, 512 * 1024, 1024 * 1024
        };

        private readonly string filename = "Arts.bin";
        private string[] displayedSpeed;
        private int index;
        private string url;

        private void Start()
        {
            displayedSpeed = Array.ConvertAll(speeds, Utility.FormatBytes);
            url = Versions.GetDownloadURL(filename);
        }

        private void OnGUI()
        {
            var rect = new Rect(Screen.width * 0.25f, Screen.height * 0.25f, Screen.width * 0.5f, Screen.height * 0.5f);
            using (new GUILayout.AreaScope(rect))
            {
                using (new GUILayout.HorizontalScope())
                {
                    url = GUILayout.TextField(url);
                    if (GUILayout.Button("下载", GUILayout.Width(128)))
                    {
                        StartDownload();
                    }
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("限速", GUILayout.Width(96));
                    var new_index = GUILayout.Toolbar(index, displayedSpeed);
                    if (new_index != index)
                    {
                        index = new_index;
                        OnSpeedChange(index);
                    }
                }
            }
        }

        public void OnSpeedChange(int value)
        {
            if (value >= 0 && value < speeds.Length)
            {
                var speed = speeds[value];
                Download.MaxBandwidth = speed;
                UnpackBinary.MaxBandwidth = speed;
            }
        }

        public void StartDownload()
        {
            var savePath = Versions.GetDownloadDataPath(Path.GetFileName(url));
            PreloadManager.Instance.ShowProgress(Download.DownloadAsync(url, savePath,
                download =>
                {
                    if (download.status == DownloadStatus.Success)
                    {
                        if (url.EndsWith(".bin"))
                        {
                            var unpackAsync = Versions.UnpackAsync(savePath);
                            unpackAsync.updated += (message, progress) =>
                            {
                                PreloadManager.Instance.SetMessage($"解压中...{message}", progress);
                            };
                            unpackAsync.completed += operation =>
                            {
                                PreloadManager.Instance.SetMessage("解压完成", 1);
                                PreloadManager.Instance.SetVisible(false);
                            };
                        }
                    }
                    else
                    {
                        MessageBox.Show("提示", "下载失败，请检测网络链接后重试", isOk =>
                        {
                            if (isOk)
                            {
                                StartDownload();
                            }
                        });
                    }
                }));
        }
    }
}