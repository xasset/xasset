using System;
using System.Collections.Generic;

namespace Versions
{
    public sealed class DownloadVersions : Operation
    {
        public readonly List<DownloadInfo> items = new List<DownloadInfo>();
        private Download download;
        private long lastDownloadedBytes;
        public Action<DownloadVersions> updated;

        public long totalSize { get; set; }
        public long downloadedBytes { get; private set; }

        public override void Start()
        {
            base.Start();
            downloadedBytes = 0;
            lastDownloadedBytes = 0;
            foreach (var info in items) totalSize += info.size;

            if (items.Count > 0)
                download = Download.DownloadAsync(items[0]);
            else
                Finish();
        }

        protected override void Update()
        {
            if (status == OperationStatus.Processing)
            {
                if (download.isDone)
                {
                    if (download.status == DownloadStatus.Success)
                    {
                        lastDownloadedBytes += download.downloadedBytes;
                        items.RemoveAt(0);
                        if (items.Count > 0)
                            download = Download.DownloadAsync(items[0]);
                        else
                            Finish();
                    }
                    else
                    {
                        Finish(download.error);
                    }
                }
                else
                {
                    downloadedBytes = lastDownloadedBytes + download.downloadedBytes;
                    progress = downloadedBytes * 1f / totalSize;
                }

                if (updated != null) updated(this);
            }
        }
    }
}