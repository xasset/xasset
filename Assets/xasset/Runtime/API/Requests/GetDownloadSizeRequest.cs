using System.Collections.Generic;

namespace xasset
{
    public class GetDownloadSizeRequest : Request
    {
        private readonly List<ManifestBundle> _bundles = new List<ManifestBundle>();
        private readonly List<DownloadContent> _contents = new List<DownloadContent>();
        private int _max;

        public Versions versions { get; set; }
        public ulong downloadSize { get; private set; }

        public DownloadRequestBase DownloadAsync()
        {
            var request = DownloadRequestBatch.Create();
            foreach (var content in _contents)
                if (content.status != DownloadContent.Status.Downloaded)
                    request.AddContent(content);
            request.SendRequest();
            return request;
        }

        protected override void OnStart()
        {
            if (Assets.SimulationMode || Assets.OfflineMode)
            {
                SetResult(Result.Success);
                return;
            }

            foreach (var version in versions.data)
            {
                var bundles = version.manifest.bundles;
                _bundles.AddRange(bundles);
            }

            _max = _bundles.Count;
        }

        protected override void OnUpdated()
        {
            progress = (_max - _bundles.Count) * 1f / _max;
            while (_bundles.Count > 0)
            {
                AddContent(_bundles[0]);
                _bundles.RemoveAt(0);
                if (Scheduler.Busy) return;
            }

            SetResult(Result.Success);
        }

        private void AddContent(ManifestBundle bundle)
        {
            var url = Assets.GetDownloadURL(bundle.file);
            var savePath = Assets.GetDownloadDataPath(bundle.file);
            var content = DownloadContent.Get(url, savePath, bundle.hash, bundle.size);
            content.status = DownloadContent.Status.Default;
            _contents.Add(content);
            if (!Assets.IsPlayerAsset(bundle.hash) && !Assets.IsDownloaded(bundle))
                downloadSize += content.downloadSize;
            else
                content.status = DownloadContent.Status.Downloaded;
        }
    }
}