using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using UnityEngine;

namespace Versions
{
    public enum DownloadStatus
    {
        Wait,
        Progressing,
        Success,
        Failed
    }

    public class DownloadInfo
    {
        public uint crc;
        public string savePath;
        public long size;
        public string url;
    }

    public class Download : CustomYieldInstruction
    {
        public static uint MaxDownloads = 10;
        public static long MaxBandwidth = 0;
        public static int MaxRetryTimes = 3;
        public static uint ReadBufferSize = 1042 * 4;
        public static readonly List<Download> Prepared = new List<Download>();
        public static readonly List<Download> Progressing = new List<Download>();
        public static readonly Dictionary<string, Download> Cache = new Dictionary<string, Download>();
        private static float lastSampleTime;
        private static long lastTotalDownloadedBytes;


        private readonly byte[] _readBuffer = new byte[ReadBufferSize];
        private long _bandWidth;

        private Thread _thread;

        private int retryTimes;
        private FileStream writer;


        private Download()
        {
            status = DownloadStatus.Wait;
            downloadedBytes = 0;
        }

        public DownloadInfo info { get; private set; }


        public DownloadStatus status { get; private set; }

        public string error { get; private set; }
        public Action<Download> completed { get; set; }
        public Action<Download> updated { get; set; }

        public bool isDone => status == DownloadStatus.Failed || status == DownloadStatus.Success;

        public float progress => downloadedBytes * 1f / info.size;

        public long downloadedBytes { get; private set; }

        public override bool keepWaiting => !isDone;

        public static bool Working => Progressing.Count > 0;

        public static long TotalDownloadedBytes
        {
            get
            {
                var value = 0L;
                foreach (var item in Cache)
                {
                    value += item.Value.downloadedBytes;
                }

                return value;
            }
        }

        public static long TotalSize
        {
            get
            {
                var value = 0L;
                foreach (var item in Cache)
                {
                    value += item.Value.info.size;
                }

                return value;
            }
        }


        public static long TotalBandwidth { get; private set; }

        public static void ClearAllDownloads()
        {
            foreach (var download in Progressing)
            {
                download.Cancel();
            }

            Prepared.Clear();
            Progressing.Clear();
            Cache.Clear();
        }

        public static Download DownloadAsync(string url, string savePath, Action<Download> completed = null,
            long size = 0, uint crc = 0)
        {
            return DownloadAsync(new DownloadInfo
            {
                url = url,
                savePath = savePath,
                crc = crc,
                size = size
            }, completed);
        }

        public static Download DownloadAsync(DownloadInfo info, Action<Download> completed = null)
        {
            Download download;
            if (!Cache.TryGetValue(info.url, out download))
            {
                download = new Download
                {
                    info = info
                };
                Prepared.Add(download);
                Cache.Add(info.url, download);
            }
            else
            {
                Logger.W("Download url {0} already exist.", info.url);
            }

            if (completed != null)
            {
                download.completed += completed;
            }

            return download;
        }

        public static void UpdateAll()
        {
            if (Prepared.Count > 0)
            {
                for (var index = 0; index < Mathf.Min(Prepared.Count, MaxDownloads - Progressing.Count); index++)
                {
                    var download = Prepared[index];
                    Prepared.RemoveAt(index);
                    index--;
                    Progressing.Add(download);
                    download.Start();
                }
            }

            if (Progressing.Count > 0)
            {
                for (var index = 0; index < Progressing.Count; index++)
                {
                    var download = Progressing[index];
                    if (download.updated != null)
                    {
                        download.updated(download);
                    }

                    if (download.isDone)
                    {
                        if (download.status == DownloadStatus.Failed)
                        {
                            Logger.E("Unable to download {0} with error {1}", download.info.url, download.error);
                        }
                        else
                        {
                            Logger.I("Success to download {0}", download.info.url);
                        }

                        Progressing.RemoveAt(index);
                        index--;
                        download.Complete();
                    }
                }

                if (Time.realtimeSinceStartup - lastSampleTime >= 1)
                {
                    TotalBandwidth = TotalDownloadedBytes - lastTotalDownloadedBytes;
                    lastTotalDownloadedBytes = TotalDownloadedBytes;
                    lastSampleTime = Time.realtimeSinceStartup;
                }
            }
            else
            {
                if (Cache.Count <= 0)
                {
                    return;
                }

                Cache.Clear();
                lastTotalDownloadedBytes = 0;
                lastSampleTime = Time.realtimeSinceStartup;
            }
        }

        public void Retry()
        {
            status = DownloadStatus.Wait;
            Start();
        }

        public void UnPause()
        {
            Retry();
        }

        public void Pause()
        {
            status = DownloadStatus.Wait;
        }

        public void Cancel()
        {
            error = "User Cancel.";
            status = DownloadStatus.Failed;
        }

        private void Complete()
        {
            if (completed != null)
            {
                completed.Invoke(this);
                completed = null;
            }
        }

        private void Run()
        {
            try
            {
                Downloading();
                CloseWrite();
                if (status == DownloadStatus.Failed)
                {
                    return;
                }

                if (downloadedBytes != info.size)
                {
                    error = $"Download lenght {downloadedBytes} mismatch to {info.size}";
                    if (CanRetry())
                    {
                        return;
                    }

                    status = DownloadStatus.Failed;
                    return;
                }

                if (info.crc != 0)
                {
                    var crc = Utility.ComputeCRC32(info.savePath);
                    if (info.crc != crc)
                    {
                        File.Delete(info.savePath);
                        error = $"Download crc {crc} mismatch to {info.crc}";
                        if (CanRetry())
                        {
                            return;
                        }

                        status = DownloadStatus.Failed;
                        return;
                    }
                }

                status = DownloadStatus.Success;
            }
            catch (Exception e)
            {
                CloseWrite();
                error = e.Message;
                if (CanRetry())
                {
                    return;
                }

                status = DownloadStatus.Failed;
            }
        }

        private void CloseWrite()
        {
            if (writer != null)
            {
                writer.Flush();
                writer.Close();
            }
        }

        private bool CanRetry()
        {
            if (retryTimes < MaxRetryTimes)
            {
                Logger.W("Failed to download {0} with error {1}, auto retry {2}", info.url, error, retryTimes);
                Thread.Sleep(1000);
                Retry();
                retryTimes++;
                return true;
            }

            return false;
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors spe)
        {
            return true;
        }

        private void Downloading()
        {
            var request = CreateWebRequest();
            using (var response = request.GetResponse())
            {
                if (response.ContentLength > 0)
                {
                    if (info.size == 0)
                    {
                        info.size = response.ContentLength + downloadedBytes;
                    }

                    using (var reader = response.GetResponseStream())
                    {
                        if (downloadedBytes < info.size)
                        {
                            var startTime = DateTime.Now;
                            while (status == DownloadStatus.Progressing)
                            {
                                if (ReadToEnd(reader))
                                {
                                    break;
                                }

                                UpdateBandwidth(ref startTime);
                            }
                        }
                    }
                }
                else
                {
                    status = DownloadStatus.Success;
                }
            }
        }

        private void UpdateBandwidth(ref DateTime startTime)
        {
            var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
            while (MaxBandwidth > 0 &&
                   status == DownloadStatus.Progressing &&
                   _bandWidth >= MaxBandwidth / Progressing.Count &&
                   elapsed < 1000)
            {
                var wait = Mathf.Clamp((int) (1000 - elapsed), 1, 33);
                Thread.Sleep(wait);
                elapsed = (DateTime.Now - startTime).TotalMilliseconds;
            }

            if (!(elapsed >= 1000))
            {
                return;
            }

            startTime = DateTime.Now;
            TotalBandwidth = _bandWidth;
            _bandWidth = 0L;
        }

        private WebRequest CreateWebRequest()
        {
            WebRequest request;
            if (info.url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;
                request = GetHttpWebRequest();
            }
            else
            {
                request = GetHttpWebRequest();
            }

            return request;
        }

        private WebRequest GetHttpWebRequest()
        {
            var httpWebRequest = (HttpWebRequest) WebRequest.Create(info.url);
            httpWebRequest.ProtocolVersion = HttpVersion.Version10;
            if (downloadedBytes > 0)
            {
                httpWebRequest.AddRange(downloadedBytes);
            }

            return httpWebRequest;
        }

        private bool ReadToEnd(Stream reader)
        {
            var len = reader.Read(_readBuffer, 0, _readBuffer.Length);
            if (len > 0)
            {
                writer.Write(_readBuffer, 0, len);
                downloadedBytes += len;
                _bandWidth += len;
                return false;
            }

            return true;
        }

        private void Start()
        {
            if (status != DownloadStatus.Wait)
            {
                return;
            }

            Logger.I("Start download {0}", info.url);
            status = DownloadStatus.Progressing;
            var file = new FileInfo(info.savePath);
            if (file.Exists && file.Length > 0)
            {
                if (info.size > 0 && file.Length == info.size)
                {
                    status = DownloadStatus.Success;
                    return;
                }

                writer = File.OpenWrite(info.savePath);
                downloadedBytes = writer.Length - 1;
                if (downloadedBytes > 0)
                {
                    writer.Seek(-1, SeekOrigin.End);
                }
            }
            else
            {
                var dir = Path.GetDirectoryName(info.savePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                writer = File.Create(info.savePath);
                downloadedBytes = 0;
            }

            _thread = new Thread(Run)
            {
                IsBackground = true
            };
            _thread.Start();
        }
    }
}