using System.Collections.Generic;
using System.IO;

namespace VEngine
{
    public sealed class GetDownloadSize : Operation
    {
        public readonly List<ManifestBundle> bundles = new List<ManifestBundle>();
        public readonly List<DownloadInfo> result = new List<DownloadInfo>();
        public long totalSize { get; private set; }

        public override void Start()
        {
            base.Start();

            if (Versions.OfflineMode)
            {
                Finish();
                return;
            }

            totalSize = 0;
            if (bundles.Count == 0) Finish();
        }

        protected override void Update()
        {
            if (status == OperationStatus.Processing)
            {
                while (bundles.Count > 0)
                {
                    var bundle = bundles[0];
                    var savePath = Versions.GetDownloadDataPath(bundle.nameWithAppendHash);
                    if (!Versions.IsDownloaded(bundle) && !result.Exists(info => info.savePath == savePath))
                    {
                        var file = new FileInfo(savePath);
                        if (file.Exists)
                            totalSize += bundle.size - file.Length;
                        else
                            totalSize += bundle.size;

                        result.Add(new DownloadInfo
                        {
                            crc = bundle.crc,
                            url = Versions.GetDownloadURL(bundle.nameWithAppendHash),
                            size = bundle.size,
                            savePath = savePath
                        });
                    }

                    bundles.RemoveAt(0);
                    if (Updater.Instance.busy) return;
                }

                Finish();
            }
        }
    }
}