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
        public string[] assets { get; set; }

        public DownloadRequestBase DownloadAsync()
        {
            var request = DownloadRequestBatch.Create();
            foreach (var content in _contents)
                request.AddContent(content);
            request.SendRequest();
            return request;
        }

        protected override void OnStart()
        {
            if (!Assets.Updatable)
            {
                SetResult(Result.Success);
                return;
            } 
            var set = new HashSet<ManifestBundle>();
            if (assets.Length == 0)
                foreach (var version in versions.data)
                {
                    var bundles = version.manifest.bundles;
                    foreach (var bundle in bundles)
                        set.Add(bundle);
                }
            else
                foreach (var path in assets)
                    if (versions.TryGetGroups(path, out var groups))
                    {
                        foreach (var group in groups)
                        foreach (var asset in group.assets)
                        {
                            var bundles = group.manifest.bundles;
                            var bundle = bundles[asset];
                            set.Add(bundle); 
                            foreach (var dep in bundle.deps)
                                set.Add(bundles[dep]);
                        }
                    }
                    else
                    {
                        if (!versions.TryGetAssets(path, out var value)) continue;
                        foreach (var asset in value)
                        {
                            var bundles = asset.manifest.bundles;
                            var bundle = bundles[asset.bundle];
                            set.Add(bundle);
                            foreach (var dep in bundle.deps)
                                set.Add(bundles[dep]);
                        }
                    }

            _bundles.AddRange(set); 
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

        private void AddContent(Downloadable file)
        {
            if (Assets.IsDownloaded(file)) return;
            var url = Assets.GetDownloadURL(file.file);
            var savePath = Assets.GetDownloadDataPath(file.file);
            var content = DownloadContent.Get(url, savePath, file.hash, file.size);
            _contents.Add(content);
            downloadSize += content.downloadSize;
        } 
    }
}