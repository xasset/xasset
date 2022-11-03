using System;
using System.Collections.Generic;
using System.IO;

namespace xasset
{
    [Serializable]
    public class DownloadContent
    {
        public enum Status
        {
            Default,
            Downloaded,
            Downloading
        }

        private static readonly Dictionary<string, DownloadContent> Contents = new Dictionary<string, DownloadContent>();
        public string hash;
        public string savePath;
        public ulong size;
        public string url;
        public ulong downloadedBytes;
        public ulong downloadSize => size - downloadedBytes;
        public int assets { get; set; } = 1;

        public Status status { get; set; } = Status.Default;

        public ulong GetDownloadedBytes()
        {
            var file = new FileInfo(savePath);
            downloadedBytes = (ulong) (file.Exists ? file.Length : 0);
            return downloadedBytes;
        }

        public static DownloadContent Get(string url, string savePath, string hash = null, ulong size = 0)
        {
            if (Contents.TryGetValue(url, out var value)) return value;
            value = new DownloadContent
            {
                url = url,
                savePath = savePath,
                hash = hash,
                size = size
            };
            Contents[url] = value;
            return value;
        }

        public void Clear()
        {
            if (File.Exists(savePath)) File.Delete(savePath);
            downloadedBytes = 0;
            status = Status.Default;
        }
    }
}