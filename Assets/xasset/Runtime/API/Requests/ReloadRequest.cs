using System;
using System.Collections.Generic;

namespace xasset
{
    public interface IReloadable
    {
        void ReloadAsync();
        void OnReloaded();
        bool IsReloaded();
    }

    public class ReloadRequest : Request
    {
        public Versions versions;

        private readonly List<IReloadable> queue = new List<IReloadable>();
        public int max { get; private set; }
        public int pos { get; private set; }

        protected override void OnStart()
        {
            Assets.Versions = versions;
            versions.Save(Assets.GetDownloadDataPath(Versions.Filename));

            var changed = new HashSet<string>();
            foreach (var pair in BundleRequest.Loaded)
            {
                var last = pair.Value.info;
                var current = versions.GetBundle(pair.Value.info.name);
                if (current == null || current.hash == last.hash) continue;
                pair.Value.Reload(current);
                Reload(pair.Value);
                changed.Add(pair.Key);
            }

            foreach (var pair in Dependencies.Loaded)
            {
                var last = pair.Value.asset;
                var bundle = last.manifest.bundles[last.bundle];
                if (!changed.Contains(bundle.name) &&
                    !Array.Exists(bundle.deps, i => changed.Contains(last.manifest.bundles[i].name))) continue;
                if (!versions.TryGetAsset(last.path, out var value)) continue;
                pair.Value.asset = value;
                Reload(pair.Value);
            }

            foreach (var pair in AssetRequest.Loaded)
            {
                var last = pair.Value.info;
                var bundle = last.manifest.bundles[last.bundle];
                if (!changed.Contains(bundle.name) &&
                    !Array.Exists(bundle.deps, i => changed.Contains(last.manifest.bundles[i].name))) continue;
                if (!versions.TryGetAsset(last.path, out var value)) continue;
                pair.Value.info = value;
                Reload(pair.Value);
            }

            max = queue.Count;
            pos = 0;
        }

        private void Reload(IReloadable reloadable)
        {
            queue.Add(reloadable);
            reloadable.ReloadAsync();
        }

        protected override void OnUpdated()
        {
            for (var index = 0; index < queue.Count; index++)
            {
                var request = queue[index];
                if (!request.IsReloaded())
                    continue;
                request.OnReloaded();
                queue.RemoveAt(index);
                index--;
                if (Scheduler.Busy)
                    return;
            }

            pos = max - queue.Count;
            progress = pos * 1f / max;

            if (queue.Count > 0)
            {
                return;
            }

            SetResult(Result.Success);
        }
    }
}